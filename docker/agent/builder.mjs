/*
 * The build agent's front door.
 *
 * One endpoint takes a sentence a visitor typed, runs Claude Code against the
 * lambda server's own MCP, and reports back the two links that matter: where
 * the thing is, and the editor link to take it further with.
 *
 * It only ever creates. Changing something that already exists is left to
 * the person's own agent over MCP, or to the editor: a second brief for
 * changes lived here once and never worked well enough to keep.
 *
 * It is deliberately boring. No streaming to the browser, no websockets: a
 * job goes on a queue, one runs at a time, and the caller polls. A build is
 * a minute of work, so the difference between polling and streaming is not
 * worth the failure modes of holding a connection open across two hops.
 */

import { createServer } from 'node:http';
import { spawn } from 'node:child_process';
import { readFile } from 'node:fs/promises';
import { randomUUID } from 'node:crypto';

const PORT = Number(process.env.AGENT_PORT ?? 8401);

/*
 * What a build runs in.
 *
 * One container per request, thrown away afterwards. Nothing a prompt does
 * survives it, nothing from the last build is in it, and the timeout is a
 * docker kill rather than a signal to a process that may or may not take it.
 *
 * It is deliberately not this container. This one holds the docker socket,
 * which is root on the machine to anything that can use it, so the rule that
 * keeps that honest is that nothing a model wrote ever runs in here.
 */
const BUILD_IMAGE = process.env.AGENT_BUILD_IMAGE ?? 'genhttp-agent:latest';
const BUILD_NETWORK = process.env.AGENT_BUILD_NETWORK ?? 'genhttp-build-net';
const BUILD_MEMORY = process.env.AGENT_BUILD_MEMORY ?? '2g';
const BUILD_CPUS = process.env.AGENT_BUILD_CPUS ?? '1.5';
const BUILD_PIDS = process.env.AGENT_BUILD_PIDS ?? '256';

// only needed while there is no token of its own: the credential the builds
// share until CLAUDE_CODE_OAUTH_TOKEN is set
const CONFIG_VOLUME = process.env.AGENT_CONFIG_VOLUME ?? 'genhttplambda_agent-config';

// read once; written into each build's working directory so the CLI takes it
// as project context
const GUIDE = await readFile('/app/AGENTS.md', 'utf8').catch(() => '');

/*
 * Written into the container rather than passed as arguments, so that neither
 * the brief nor the credential shows up in the host's process list. "-e NAME"
 * with no value tells docker to take it from our own environment.
 */
const INSIDE = `set -e
printf '%s' "$AGENT_GUIDE" > /work/AGENTS.md
printf '{"mcpServers":{"genhttp":{"type":"http","url":"%s"}}}' "$AGENT_MCP" > /work/mcp.json
exec claude -p "$AGENT_BRIEF" --mcp-config /work/mcp.json "$@"`;
const MCP_URL = process.env.AGENT_MCP_URL ?? "https://genhttp.dev/mcp";
const MODEL = process.env.AGENT_MODEL ?? 'claude-opus-5-5';

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
const MAX_TURNS = Number(process.env.AGENT_MAX_TURNS ?? 60);
const TIMEOUT = Number(process.env.AGENT_TIMEOUT_SECONDS ?? 600) * 1000;
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
  'mcp__genhttp__deploy', 'mcp__genhttp__read_lambda', 'mcp__genhttp__read_logs',
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
   than default. Pass what they asked for, word for word, as prompt, and one
   line on what the version does as change - they read both in the version
   history, and every later write_code gets its own.
4. Call check_code and fix whatever it complains about. Do not deploy code
   that does not compile.
5. Call deploy. Nothing is online until you do. If there is time, call
   read_logs to see that it answers without errors.

You have about ten minutes and then you are stopped, wherever you have got
to. Aim at something that works end to end within that rather than something
ambitious and half finished: a small game that plays beats a large one that
does not load, and nothing at all is worse than either. Get something
deployed and working first; make it better with whatever time is left.

If what they asked for is genuinely too big - a multiplayer strategy game, an
operating system - build the smallest honest version of it and say in your
closing note what you left out.

What to build: something that works end to end and is worth opening. If they
asked for something with state - scores, entries, a list - keep it on the
server so it is still there tomorrow and everybody sees the same thing. That
is the thing this platform can do that a static page cannot, so lean on it.

Finish by writing two or three sentences for the person who asked: what you
built and what they can do with it. Do not list the links, they are picked up
automatically. Do not describe your process.`;

/* ------------------------------------------------------------------ jobs */

const jobs = new Map();

/*
 * Two lanes, because one of them has no end.
 *
 * A build asked for with the password runs without a turn limit and without a
 * clock, which is the point of it - but the ordinary queue runs one at a time,
 * so an unbounded build sharing it would hold the public text box shut for as
 * long as it felt like running. They get a lane of their own instead: at most
 * one of each kind at once, and neither waits on the other.
 */
const lanes = {
  quick: { queue: [], running: false },
  long: { queue: [], running: false }
};

/** Whether a job runs without a clock or a turn limit. */
const unbounded = job => job.model === 'fable';

const clip = (s, n) => (s.length > n ? s.slice(0, n) + '…' : s);

function enqueue(prompt, model) {
  const id = randomUUID();

  jobs.set(id, { id, state: 'queued', prompt, model, events: [], created: Date.now(), result: null });

  const lane = model === 'fable' ? lanes.long : lanes.quick;

  lane.queue.push(id);
  setImmediate(() => pump(lane));

  // an hour is long enough for somebody to come back to a tab
  setTimeout(() => jobs.delete(id), 60 * 60 * 1000).unref?.();

  return id;
}

async function pump(lane) {
  if (lane.running || lane.queue.length === 0) return;

  lane.running = true;

  const id = lane.queue.shift();
  const job = jobs.get(id);

  if (job) {
    try {
      await run(job);
    } catch (e) {
      job.state = 'failed';
      job.result = { ok: false, error: String(e?.message ?? e) };
    }

    record(job);
  }

  lane.running = false;

  setImmediate(() => pump(lane));
}

function say(job, text) {
  job.events.push({ at: Date.now(), text });

  if (job.events.length > 120) job.events.shift();
}

/* --------------------------------------------------------------- the run */

async function run(job) {
  job.state = 'running';
  job.started = Date.now();

  say(job, 'Reading the platform guide');

  let brief = `${BRIEF}\n\nWhat they asked for:\n\n${job.prompt}`;

  // the brief tells it how long it has, so it must not keep saying ten
  // minutes to a build that has no clock on it at all
  if (unbounded(job)) {
    brief = brief.replace(
      /You have about ten minutes[\s\S]*?time is left\./,
      'Take the time you need. There is no clock on this one and no limit on how many\n'
      + 'steps you take, so build the thing properly rather than the smallest version of\n'
      + 'it: get it working first, then keep going until it is actually good.'
    );
  }

  const free = unbounded(job);

  // the MCP server it is allowed to talk to, and the only one:
  // --strict-mcp-config means nothing from a settings file can add another
  const args = [
    '--model', MODELS[job.model] ?? MODEL,
    '--strict-mcp-config',
    '--permission-mode', 'dontAsk',
    // left off entirely rather than set high: there is no value that means
    // "no limit", and a large one is still a limit somebody eventually hits
    ...(free ? [] : ['--max-turns', String(MAX_TURNS)]),
    '--output-format', 'stream-json',
    '--verbose',
    '--allowedTools', ...ALLOW,
    '--disallowedTools', ...DENY
  ];

  const box = `build-${job.id.replace(/[^a-z0-9]/gi, '').slice(0, 24)}`;

  /*
   * Everything it is allowed, spelled out. What is not here it does not have:
   * no capabilities, no privilege it can gain, a filesystem that is a tmpfs
   * and a network that reaches the MCP and the proxy and nothing else.
   */
  const shared = process.env.CLAUDE_CODE_OAUTH_TOKEN
    ? []
    // no token of its own yet, so it falls back to the copied credential -
    // which is the one thing builds still share, and the reason to set a token
    : ['-v', `${CONFIG_VOLUME}:/home/builder/.claude`];

  const run = [
    'run', '--rm', '--name', box,
    '--network', BUILD_NETWORK,
    '--memory', BUILD_MEMORY, '--cpus', BUILD_CPUS, '--pids-limit', BUILD_PIDS,
    '--security-opt', 'no-new-privileges:true',
    '--cap-drop', 'ALL',
    '--tmpfs', '/work:rw,size=64m,mode=0700,uid=1002,gid=1002',
    '--workdir', '/work',
    '-e', 'AGENT_BRIEF', '-e', 'AGENT_GUIDE', '-e', 'AGENT_MCP',
    '-e', 'CLAUDE_CODE_OAUTH_TOKEN',
    /*
     * Emptied rather than passed through.
     *
     * Builds run on the subscription and only on the subscription. An API key
     * reaching a build container would be spent per run against a different
     * account without anything saying so, and the one place it could come
     * from is an environment nobody meant to export it into - so the variable
     * is set empty here, which is stronger than not forwarding it.
     */
    '-e', 'ANTHROPIC_API_KEY=',
    '-e', 'HTTPS_PROXY', '-e', 'HTTP_PROXY', '-e', 'NO_PROXY',
    ...shared,
    '--entrypoint', 'sh',
    BUILD_IMAGE,
    '-c', INSIDE, 'build', ...args
  ];

  const child = spawn('docker', run, {
    env: {
      ...process.env,
      AGENT_BRIEF: brief,
      AGENT_GUIDE: GUIDE,
      AGENT_MCP: MCP_URL
    },
    stdio: ['ignore', 'pipe', 'pipe']
  });

  // killing the client leaves the container running, so the timeout kills the
  // container and lets --rm take it away
  const killer = free ? null : setTimeout(() => {
    spawn('docker', ['kill', box], { stdio: 'ignore' }).on('error', () => {});
  }, TIMEOUT);

  let created = null;
  let deployed = false;
  let wrote = false;

  // which call each result belongs to. Without it a result is just an object,
  // and two tools that answer with the same field are indistinguishable
  const calls = new Map();
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
          if (part.type === 'tool_use') {
            calls.set(part.id, String(part.name ?? '').replace('mcp__genhttp__', ''));
            say(job, describe(part.name, part.input));
          }
          if (part.type === 'text' && part.text.trim()) summary = part.text.trim();
        }
      }

      // the keys come out of the tool result rather than out of the prose,
      // because the prose is a language model and the result is a fact
      if (event.type === 'user') {
        for (const part of event.message?.content ?? []) {
          if (part.type !== 'tool_result') continue;

          const from = calls.get(part.tool_use_id);

          // code having been written is the difference between an application
          // and an empty address. Nothing else in the run proves it: creating
          // a lambda seeds a starter template, and deploying that answers
          // exactly like deploying something real.
          if (from === 'write_code' && looksOk(part.content)) wrote = true;

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

  if (killer) clearTimeout(killer);

  // nothing to clean up: the container took its filesystem with it

  /*
   * A fresh build that never wrote code is a failure however cheerfully it
   * ended. create_lambda seeds the starter template, so the address answers,
   * the deploy succeeds and everything looks right - and what is online is
   * "My Lambda / It works". That was reported as a success, with a link,
   * to somebody who had asked for a game.
   */
  /*
   * Why it wrote nothing matters. A build that cannot sign in dies in a
   * second or two having done nothing at all, and telling that person to
   * "try asking for something smaller" blames their prompt for this server's
   * expired credentials - which is what it did, to everybody who used the
   * page, for the twelve hours the token was stale.
   */
  const unauthorised = /authenticat|oauth|401|revoked|invalid api key|credit balance/i
    .test(`${summary} ${stderr}`);

  if (!wrote) {
    job.state = 'failed';
    job.result = {
      ok: false,
      error: unauthorised
        ? 'The builder could not sign in, so nothing ran. That is this server\u2019s credentials '
        + 'rather than anything about what you asked for.'
        : 'It ran out of time before it wrote anything. Try asking for something smaller, or '
        + 'ask again - it gets further some runs than others.',
      detail: clip(summary || stderr, 400),
      publicKey: created?.publicKey,
      privateKey: created?.privateKey
    };
    return;
  }

  if (!created?.publicKey) {
    job.state = 'failed';
    job.result = {
      ok: false,
      error: !free && (code === null || child.killed)
        ? 'The build ran out of time.'
        : 'The build did not produce anything that could be put online.',
      detail: clip(summary || stderr, 600)
    };
    return;
  }

  job.state = 'done';
  job.result = {
    ok: true,
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

/**
 * One line per finished job.
 *
 * There was none of this, and the first time somebody asked why their build
 * came out wrong the only evidence left was the shape of the lambda table. A
 * prompt, what it did and how long it took is the difference between
 * answering that and guessing at it.
 */
function record(job) {
  const seconds = Math.round((Date.now() - (job.started ?? job.created)) / 1000);
  const r = job.result ?? {};

  console.log(JSON.stringify({
    at: new Date().toISOString(),
    id: job.id.slice(0, 8),
    model: job.model || 'opus',
    unbounded: unbounded(job) || undefined,
    seconds,
    state: job.state,
    wrote: r.ok === true || undefined,
    deployed: r.deployed,
    key: r.publicKey,
    error: r.error,
    prompt: clip(job.prompt, 300)
  }));
}

/** Whether a tool answered with something that was not a refusal. */
function looksOk(content) {
  const texts = Array.isArray(content)
    ? content.filter(c => c.type === 'text').map(c => c.text)
    : [String(content ?? '')];

  for (const text of texts) {
    try {
      const body = JSON.parse(text);
      if (body && typeof body === 'object' && body.ok === true) return true;
    } catch { /* a refusal is prose */ }
  }

  return false;
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
  read_logs: 'Checking how it answers',
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
    return send(res, 200, {
      ok: true,
      models: Object.keys(MODELS),
      quick: { running: lanes.quick.running, queued: lanes.quick.queue.length },
      long: { running: lanes.long.running, queued: lanes.long.queue.length }
    });
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

      if (lanes.quick.queue.length + lanes.long.queue.length > 12) {
        return send(res, 503, { error: 'Too many builds waiting. Try again shortly.' });
      }

      let model = '';

      try { model = String(JSON.parse(body).model ?? '').trim(); } catch { model = ''; }

      if (model && !Object.hasOwn(MODELS, model)) {
        return send(res, 400, { error: 'There is no such model here.' });
      }

      const id = enqueue(clip(prompt, 2000), model);

      const lane = model === 'fable' ? lanes.long : lanes.quick;

      return send(res, 202, { id, queued: lane.queue.length });
    });

    return;
  }

  const match = req.method === 'GET' && /^\/build\/([0-9a-f-]{36})$/.exec(req.url ?? '');

  if (match) {
    const job = jobs.get(match[1]);

    if (!job) return send(res, 404, { error: 'No such build.' });

    const lane = unbounded(job) ? lanes.long : lanes.quick;

    return send(res, 200, {
      state: job.state,
      events: job.events.map(e => e.text),
      result: job.result,
      waiting: lane.queue.indexOf(job.id) + 1
    });
  }

  return send(res, 404, { error: 'No such thing.' });
}).listen(PORT, () => console.log(`build agent listening on ${PORT}, model ${MODEL}, mcp ${MCP_URL}`));
