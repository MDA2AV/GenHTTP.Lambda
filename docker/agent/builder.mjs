/*
 * The build agent's front door.
 *
 * One endpoint takes a sentence a visitor typed, runs Claude Code against the
 * lambda server's own MCP, and reports back the two links that matter: where
 * the thing is, and where to go to change it.
 *
 * It is deliberately boring. No streaming to the browser, no websockets: a
 * job goes on a queue, one runs at a time, and the caller polls. A build is
 * a minute of work, so the difference between polling and streaming is not
 * worth the failure modes of holding a connection open across two hops.
 */

import { createServer } from 'node:http';
import { spawn } from 'node:child_process';
import { mkdtemp, rm, writeFile } from 'node:fs/promises';
import { randomUUID } from 'node:crypto';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const PORT = Number(process.env.AGENT_PORT ?? 8401);
const MCP_URL = process.env.AGENT_MCP_URL ?? "https://genhttp.dev/mcp";
const MODEL = process.env.AGENT_MODEL ?? 'claude-opus-5';

/*
 * Which models a caller may ask for, by short name.
 *
 * An allowlist rather than a passthrough, because the name arrives from a text
 * box on the public internet: handing whatever it says to --model would let a
 * visitor pick anything the account can reach, including something far more
 * expensive than what is on offer. The short names are all the outside world
 * ever sees, and the identifiers stay in here.
 */
const MODELS = {
  opus: MODEL,
  fable: process.env.AGENT_FABLE_MODEL ?? 'claude-fable-5-1'
};
const MAX_TURNS = Number(process.env.AGENT_MAX_TURNS ?? 40);
const TIMEOUT = Number(process.env.AGENT_TIMEOUT_SECONDS ?? 300) * 1000;
const ORIGIN = process.env.AGENT_PUBLIC_ORIGIN ?? 'https://genhttp.dev';
const TOKEN = process.env.AGENT_TOKEN ?? '';

/*
 * The tools the agent may use, and the tools it may not.
 *
 * The allowlist is the capability: everything the platform's MCP offers and
 * nothing else. The denylist exists because an allowlist alone was measured
 * not to be enough - with only the MCP tools allowed, SendMessage and
 * TaskCreate still ran, and a test agent used SendMessage to talk to another
 * Claude session on the same machine. These are the ones that got through on
 * the pinned version; verify-tools.sh is how you find out whether a new
 * version has more.
 */
const ALLOW = [
  'mcp__genhttp__platform_guide', 'mcp__genhttp__list_examples', 'mcp__genhttp__read_example',
  'mcp__genhttp__create_lambda', 'mcp__genhttp__write_code', 'mcp__genhttp__check_code',
  'mcp__genhttp__deploy', 'mcp__genhttp__read_lambda',
  'mcp__genhttp__upload_file', 'mcp__genhttp__list_files', 'mcp__genhttp__delete_file'
];

const DENY = [
  'Bash', 'Read', 'Write', 'Edit', 'NotebookEdit', 'Glob', 'Grep',
  'WebFetch', 'WebSearch', 'Agent', 'Artifact', 'Monitor',
  'SendMessage', 'RemoteTrigger', 'PushNotification', 'ListAgents',
  'TaskCreate', 'TaskGet', 'TaskList', 'TaskOutput', 'TaskStop', 'TaskUpdate',
  'CronCreate', 'CronList', 'CronDelete', 'DesignSync',
  'EnterWorktree', 'ExitWorktree', 'EnterPlanMode', 'ExitPlanMode',
  'ScheduleWakeup', 'SendUserFile', 'AskUserQuestion', 'EndConversation',
  'Skill', 'ToolSearch', 'ReportFindings', 'Workflow'
];

const CHANGE = `You are changing a web application that already exists, for somebody who asked in
a sentence and is not a programmer. They cannot answer questions: there is no
one to ask, so make reasonable choices and change something rather than
stopping to clarify.

How to work:

1. Call read_lambda with the editor key you were given. Read what is there
   before you change any of it - you are editing somebody's working
   application, not starting again.
2. Make the change they asked for and nothing else. Keep what already works,
   keep the parts they did not mention, and keep anything the application has
   stored: rewriting a file that reads saved data into one that reads it
   differently throws away what people have already put in.
3. Call write_code with the full set of files. Then check_code, and fix
   whatever it complains about.
4. Call deploy. The change is not live until you do.

If what they asked for does not make sense for this application, do the
closest reasonable thing and say so at the end.

Finish by writing two or three sentences for the person who asked: what you
changed. Do not list the links, they are picked up automatically. Do not
describe your process.`;

const BRIEF = `You are building one small web application for somebody who asked for it in a
sentence and is not a programmer. They cannot answer questions: there is no
one to ask, so make reasonable choices and build something rather than
stopping to clarify.

How to work:

1. Call platform_guide first. It tells you what this platform is and what the
   rules are.
2. Call create_lambda with acceptTerms true. Pick a short, readable public
   key that suits what they asked for.
3. Write the code with write_code. One page that works beats four that do
   not. If it wants a front end, serve it and make it look deliberate rather
   than default.
4. Call check_code and fix whatever it complains about. Do not deploy code
   that does not compile.
5. Call deploy. Nothing is online until you do.

What to build: something that works end to end and is worth opening. If they
asked for something with state - scores, entries, a list - keep it on the
server so it is still there tomorrow and everybody sees the same thing. That
is the thing this platform can do that a static page cannot, so lean on it.

Finish by writing two or three sentences for the person who asked: what you
built and what they can do with it. Do not list the links, they are picked up
automatically. Do not describe your process.`;

/* ------------------------------------------------------------------ jobs */

const jobs = new Map();
const queue = [];
let running = false;

const clip = (s, n) => (s.length > n ? s.slice(0, n) + '…' : s);

function enqueue(prompt, key, model) {
  const id = randomUUID();

  jobs.set(id, { id, state: 'queued', prompt, key, model, events: [], created: Date.now(), result: null });

  queue.push(id);
  setImmediate(pump);

  // an hour is long enough for somebody to come back to a tab
  setTimeout(() => jobs.delete(id), 60 * 60 * 1000).unref?.();

  return id;
}

async function pump() {
  if (running || queue.length === 0) return;

  running = true;

  const id = queue.shift();
  const job = jobs.get(id);

  if (job) {
    try {
      await run(job);
    } catch (e) {
      job.state = 'failed';
      job.result = { ok: false, error: String(e?.message ?? e) };
    }
  }

  running = false;

  setImmediate(pump);
}

function say(job, text) {
  job.events.push({ at: Date.now(), text });

  if (job.events.length > 120) job.events.shift();
}

/* --------------------------------------------------------------- the run */

async function run(job) {
  job.state = 'running';
  job.started = Date.now();

  say(job, job.key ? 'Reading what is already there' : 'Reading the platform guide');

  const cwd = await mkdtemp(join(tmpdir(), 'build-'));

  // the MCP server it is allowed to talk to, and the only one: --strict-mcp-config
  // means nothing from a settings file can add another
  const config = join(cwd, 'mcp.json');

  await writeFile(config, JSON.stringify({
    mcpServers: { genhttp: { type: 'http', url: MCP_URL } }
  }));

  const brief = job.key
    ? `${CHANGE}\n\nThe editor key of the application to change: ${job.key}\n\nWhat they asked for:\n\n${job.prompt}`
    : `${BRIEF}\n\nWhat they asked for:\n\n${job.prompt}`;

  const args = [
    '-p', brief,
    '--model', MODELS[job.model] ?? MODEL,
    '--mcp-config', config,
    '--strict-mcp-config',
    '--permission-mode', 'dontAsk',
    '--max-turns', String(MAX_TURNS),
    '--output-format', 'stream-json',
    '--verbose',
    '--allowedTools', ...ALLOW,
    '--disallowedTools', ...DENY
  ];

  const child = spawn('claude', args, {
    cwd,
    env: { ...process.env, XDG_RUNTIME_DIR: join(process.env.HOME, 'run') },
    stdio: ['ignore', 'pipe', 'pipe']
  });

  const killer = setTimeout(() => child.kill('SIGKILL'), TIMEOUT);

  // a change already knows its own key: nothing in the run will announce one,
  // because create_lambda is not called. The public half is picked up from
  // read_lambda the same way as everything else.
  let created = job.key ? { privateKey: job.key } : null;
  let deployed = false;
  let summary = '';
  let stderr = '';
  let buffer = '';

  child.stderr.on('data', d => { stderr = clip(stderr + d, 4000); });

  child.stdout.on('data', chunk => {
    buffer += chunk;

    let cut;

    while ((cut = buffer.indexOf('\n')) >= 0) {
      const line = buffer.slice(0, cut).trim();
      buffer = buffer.slice(cut + 1);

      if (line === '') continue;

      let event;
      try { event = JSON.parse(line); } catch { continue; }

      // what the agent is doing, in words a visitor can follow
      if (event.type === 'assistant') {
        for (const part of event.message?.content ?? []) {
          if (part.type === 'tool_use') say(job, describe(part.name, part.input));
          if (part.type === 'text' && part.text.trim()) summary = part.text.trim();
        }
      }

      // the keys come out of the tool result rather than out of the prose,
      // because the prose is a language model and the result is a fact
      if (event.type === 'user') {
        for (const part of event.message?.content ?? []) {
          if (part.type !== 'tool_result') continue;

          const found = harvest(part.content);

          // merged field by field rather than spread: a later tool answers
          // with the public key and no private one, and spreading that over
          // the first answer replaces the editor key with undefined - which
          // is the one thing the visitor cannot get back afterwards
          if (found?.publicKey) {
            created ??= {};
            if (found.publicKey) created.publicKey = found.publicKey;
            if (found.privateKey) created.privateKey = found.privateKey;
          }

          if (found?.deployed) deployed = true;
        }
      }

      if (event.type === 'result' && event.subtype !== 'success' && !summary) {
        summary = String(event.result ?? '').trim();
      }
    }
  });

  const code = await new Promise(resolve => child.on('close', resolve));

  clearTimeout(killer);

  await rm(cwd, { recursive: true, force: true }).catch(() => {});

  if (!created?.publicKey) {
    job.state = 'failed';
    job.result = {
      ok: false,
      error: code === null || child.killed
        ? 'The build ran out of time.'
        : job.key
          ? 'Nothing was changed. The editor key may be wrong, or the change could not be made.'
          : 'The build did not produce anything that could be put online.',
      detail: clip(summary || stderr, 600)
    };
    return;
  }

  job.state = 'done';
  job.result = {
    ok: true,
    changed: !!job.key,
    // said plainly because it cannot be recovered: this key is the only way
    // back into what was just built
    keep: 'The editor link is the only way back in. There is no way to recover it.',
    deployed,
    publicKey: created.publicKey,
    privateKey: created.privateKey,
    url: `${ORIGIN}/lambda/${created.publicKey}/`,
    editorUrl: `${ORIGIN}/editor/${created.privateKey}`,
    summary: clip(summary, 1200)
  };

  say(job, deployed ? 'Deployed' : 'Built, but it never went online');
}

/** Pulls the facts out of a tool result, whatever shape it arrived in. */
function harvest(content) {
  const texts = Array.isArray(content)
    ? content.filter(c => c.type === 'text').map(c => c.text)
    : [String(content ?? '')];

  const found = {};

  for (const text of texts) {
    try {
      const body = JSON.parse(text);

      if (!body || typeof body !== 'object') continue;

      if (body.publicKey) found.publicKey = body.publicKey;
      if (body.privateKey) found.privateKey = body.privateKey;

      /*
       * onlineUntil is only in the answer deploy gives, which is what makes it
       * usable as the signal. Reading it as "anything mentioning deployed"
       * also matched read_lambda, and testing publicKey first - as this did -
       * meant the deploy was never examined at all, because its answer carries
       * a public key too. The change went online and the page said it had not.
       */
      if (body.ok && body.onlineUntil !== undefined) found.deployed = true;
    } catch { /* not every tool answers in json */ }
  }

  return Object.keys(found).length > 0 ? found : null;
}

const WORDS = {
  platform_guide: 'Reading the platform guide',
  list_examples: 'Looking at the examples',
  read_example: 'Reading an example',
  create_lambda: 'Claiming an address',
  write_code: 'Writing the code',
  check_code: 'Compiling it',
  deploy: 'Putting it online',
  read_lambda: 'Checking what is there',
  upload_file: 'Uploading a file',
  list_files: 'Listing the files',
  delete_file: 'Removing a file'
};

function describe(name, input) {
  const short = String(name ?? '').replace('mcp__genhttp__', '');

  if (short === 'write_code' && input?.files?.length) {
    return `Writing ${input.files.length} file${input.files.length === 1 ? '' : 's'}`;
  }

  return WORDS[short] ?? `Working (${short})`;
}

/* ------------------------------------------------------------ the server */

const send = (res, status, body) => {
  const payload = JSON.stringify(body);

  res.writeHead(status, { 'content-type': 'application/json', 'content-length': Buffer.byteLength(payload) });
  res.end(payload);
};

createServer((req, res) => {
  // only the lambda server can reach this network, but a shared secret costs
  // nothing and means a mistake in the network definition is not an open door
  if (TOKEN && req.headers['x-agent-token'] !== TOKEN) {
    return send(res, 403, { error: 'no' });
  }

  if (req.method === 'GET' && req.url === '/health') {
    return send(res, 200, { ok: true, running, queued: queue.length, models: Object.keys(MODELS) });
  }

  if (req.method === 'POST' && req.url === '/build') {
    let body = '';

    req.on('data', d => {
      body += d;
      if (body.length > 8000) req.destroy();
    });

    req.on('end', () => {
      let prompt;

      try { prompt = String(JSON.parse(body).prompt ?? '').trim(); } catch { prompt = ''; }

      if (prompt.length < 3) return send(res, 400, { error: 'Say what you want built.' });

      if (queue.length > 12) return send(res, 503, { error: 'Too many builds waiting. Try again shortly.' });

      let key = '';

      try { key = String(JSON.parse(body).key ?? '').trim(); } catch { key = ''; }

      if (key && !/^[a-z0-9]{8,64}$/.test(key)) {
        return send(res, 400, { error: 'That does not look like an editor key.' });
      }

      let model = '';

      try { model = String(JSON.parse(body).model ?? '').trim(); } catch { model = ''; }

      if (model && !Object.hasOwn(MODELS, model)) {
        return send(res, 400, { error: 'There is no such model here.' });
      }

      return send(res, 202, { id: enqueue(clip(prompt, 2000), key, model), queued: queue.length });
    });

    return;
  }

  const match = req.method === 'GET' && /^\/build\/([0-9a-f-]{36})$/.exec(req.url ?? '');

  if (match) {
    const job = jobs.get(match[1]);

    if (!job) return send(res, 404, { error: 'No such build.' });

    return send(res, 200, {
      state: job.state,
      events: job.events.map(e => e.text),
      result: job.result,
      waiting: queue.indexOf(job.id) + 1
    });
  }

  return send(res, 404, { error: 'No such thing.' });
}).listen(PORT, () => console.log(`build agent listening on ${PORT}, model ${MODEL}, mcp ${MCP_URL}`));
