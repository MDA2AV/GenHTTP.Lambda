/*
 * The build agent's front door.
 *
 * One endpoint takes a sentence somebody typed, runs Claude Code against the
 * lambda server's own MCP, and reports back what came of it. There are two
 * kinds of job behind it:
 *
 *   a build    a visitor on /build wants something new. It reports the two
 *              links that matter: where the thing is, and the editor link to
 *              take it further with.
 *   a change   the owner of a lambda asked for something different from the
 *              Change section of its control center. It arrives with the
 *              editor key, and reports the version it made and whether that
 *              version is online.
 *
 * Both share the queue, the clock and the container a job runs in. The
 * change used to be a second brief tacked onto the build box, which had to be
 * handed a pasted editor link and never did it well; it lives in the control
 * center now, where the key is already known and the result can be shown as
 * what it is - a new version of something that already works.
 *
 * It is deliberately boring. No streaming to the browser, no websockets: a
 * job goes on a queue, one runs at a time, and the caller polls. A job is a
 * few minutes of work, so the difference between polling and streaming is not
 * worth the failure modes of holding a connection open across two hops.
 */

import { createServer } from 'node:http';
import { spawn } from 'node:child_process';
import { readFile } from 'node:fs/promises';
import { randomUUID } from 'node:crypto';

const PORT = Number(process.env.AGENT_PORT ?? 8401);

/*
 * What a job runs in.
 *
 * One container per request, thrown away afterwards. Nothing a prompt does
 * survives it, nothing from the last job is in it, and the timeout is a
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

// read once; written into each job's working directory so the CLI takes it
// as project context
const GUIDE = await readFile('/app/AGENTS.md', 'utf8').catch(() => '');

/*
 * Written into the container rather than passed as arguments, so that neither
 * the brief nor the credential shows up in the host's process list - and a
 * change carries an editor key in its brief, which is a credential too. "-e
 * NAME" with no value tells docker to take it from our own environment.
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
  'mcp__genhttp__platform_guide', 'mcp__genhttp__list_demos',
  'mcp__genhttp__create_lambda', 'mcp__genhttp__write_code', 'mcp__genhttp__change_code',
  'mcp__genhttp__copy_version', 'mcp__genhttp__check_code',
  'mcp__genhttp__deploy', 'mcp__genhttp__read_lambda', 'mcp__genhttp__read_logs',
  'mcp__genhttp__upload_file', 'mcp__genhttp__list_files', 'mcp__genhttp__delete_file'
];

/*
 * A change works on the lambda it was given and on no other, so it cannot
 * make one: a lambda created there would have an editor key that only the
 * model ever saw, which is a lambda nobody can find again.
 */
const CHANGE_ALLOW = ALLOW.filter(tool => tool !== 'mcp__genhttp__create_lambda');

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

/*
 * The languages the control center is written in, by the code the page sends.
 * Only used to tell the agent which language to fall back on: whatever the
 * request itself is written in comes first.
 */
const LANGUAGES = {
  id: 'Indonesian', de: 'German', en: 'English', es: 'Spanish', fr: 'French',
  it: 'Italian', nl: 'Dutch', pl: 'Polish', pt: 'Brazilian Portuguese',
  'pt-pt': 'European Portuguese', tr: 'Turkish', ja: 'Japanese', ko: 'Korean'
};

/* ---------------------------------------------------------------- briefs */

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
   not. If it wants a front end, ship its pages, scripts and styles with the
   code as assets and make it look deliberate rather than default. Pass what
   they asked for, word for word, as specification, and one line on what the
   version does as change - they read both in the version history.
4. Call check_code and fix whatever it complains about. Do not deploy code
   that does not compile.
5. Call deploy. Nothing is online until you do. If there is time, call
   read_logs to see that it answers without errors.
6. Everything after the first write_code goes into that same version: pass
   its number as version to change_code (or write_code) for every fix and
   improvement, with deploy: true. They asked for one thing, so the history
   should show one version for it, not one per fix.

Whatever the application keeps - entries, scores, accounts - is data: write
it to the workspace from the code, never into the files of a version.

{clock}

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

const BUILD_CLOCK = `You have about {minutes} minutes and then you are stopped, wherever you have got
to. Aim at something that works end to end within that rather than something
ambitious and half finished: a small game that plays beats a large one that
does not load, and nothing at all is worse than either. Get something
deployed and working first; make it better with whatever time is left.`;

/*
 * The brief for changing what somebody already has.
 *
 * Written against the ways the first version of it went wrong: it rewrote
 * files wholesale and dropped the parts nobody had mentioned, it changed how
 * data was stored and lost what people had entered, and it said nothing for
 * minutes while the owner looked at a spinner. So it is told to read first,
 * to send only what changes, to keep the data, and to say what it is doing
 * as it goes - every line of prose it writes between tools is shown to the
 * owner as it happens.
 */
const CHANGE = `You are changing a web application that already exists, for its owner. They
typed what they want different into the control center of the application
and are watching you work: every short line you write between tool calls is
shown to them as it happens. They cannot answer questions, so make
reasonable choices rather than stopping to ask.

The editor key of the application is {key}. Every tool takes it as
privateKey. It is the only application you may touch, and you cannot create
another one.

How to work:

1. Call read_lambda with the key. It answers with the files of the newest
   version, what the recent versions changed, and what the newest one was
   asked for. When there are too many files to send at once it lists them
   instead - read the ones this change is about, one at a time, with file.
   Read before you change anything: this is somebody's working application,
   not a blank page.
2. If they say something is broken or does not work, call read_logs before
   you touch anything. The errors it threw are there, with stack traces.
3. Make the change they asked for, and only that. Keep everything they did
   not mention working the way it did. Keep what the application has stored:
   its data lives in the workspace and outlasts every version, so code that
   reads it differently loses what people already put in. If a format has to
   change, keep reading the old one.
4. Save with change_code. Name only the files that change - whole, in files -
   or replace a passage within one with edits; everything you leave out stays
   as it is. {deploy} Pass what they asked for, in their words, as
   specification, and one line on what this version does as change. They read
   both in the version history. This first save makes a new version: the
   answer says its number.
5. Everything after that goes into that same version: pass its number as
   version to change_code, which saves over it, for every fix and
   improvement. They asked for one change, so the history should show one
   version for it, not one per fix. If an answer comes with diagnostics, the
   code does not compile and nothing new went online: fix what they say that
   way. What is online stays online until something compiles.

Before each step, write one short sentence for the owner saying what you are
about to do - "Looking at how the scores are stored", not "Calling
read_lambda". Under fifteen words, no lists, no code.

{clock}

If what they asked for does not make sense for this application, or cannot
be done on this platform, change nothing and say why in your closing note.

Finish with two or three sentences for the owner: what is different now, and
anything they should know - something to try, something you left out. Do not
list links or describe your process.

{language}`;

const CHANGE_CLOCK = `You have about {minutes} minutes and then you are stopped, wherever you have got
to. A small change that works beats a large one that is half written: get the
change saved and compiling first, then improve it with whatever time is left.`;

const UNBOUNDED_CLOCK = `Take the time you need. There is no clock on this one and no limit on how many
steps you take, so do the thing properly rather than the smallest version of
it: get it working first, then keep going until it is actually good.`;

const DEPLOY = {
  yes: 'Pass deploy: true, so it goes online as soon as it compiles.',
  no: 'Do not deploy: the owner wants to look at it before it goes online.\n'
    + '   Pass check: true instead, which compiles it and answers with what is\n'
    + '   wrong without putting anything online.'
};

function briefOf(job) {
  // the brief says how long there is, so it must not promise ten minutes to a
  // job that has no clock on it at all
  const clock = unbounded(job)
    ? UNBOUNDED_CLOCK
    : (job.kind === 'change' ? CHANGE_CLOCK : BUILD_CLOCK)
      .replace('{minutes}', String(Math.max(1, Math.round(TIMEOUT / 60000))));

  if (job.kind !== 'change') {
    return `${BRIEF.replace('{clock}', clock)}\n\nWhat they asked for:\n\n${job.prompt}`;
  }

  const name = LANGUAGES[job.language];

  const language = 'Write everything the owner reads - the short lines, the closing note and\n'
    + 'change - in the language their request is written in.'
    + (name ? ` Their control center is set to ${name}; use that where you cannot tell.` : '');

  return `${CHANGE
    .replace('{key}', job.key)
    .replace('{deploy}', job.deploy ? DEPLOY.yes : DEPLOY.no)
    .replace('{clock}', clock)
    .replace('{language}', language)}\n\nWhat they asked for:\n\n${job.prompt}`;
}

/* ------------------------------------------------------------------ jobs */

const jobs = new Map();

/*
 * The newest job of each lambda, by the tag the server files it under.
 *
 * The server keeps nothing about a change itself: it asks here, by lambda,
 * whenever the control center asks it. So a change survives the server being
 * redeployed under it, which happens more often than a change takes.
 */
const latest = new Map();

/*
 * Two lanes, because one of them has no end.
 *
 * A job asked for with the password runs without a turn limit and without a
 * clock, which is the point of it - but the ordinary queue runs one at a time,
 * so an unbounded job sharing it would hold the public text box shut for as
 * long as it felt like running. They get a lane of their own instead: at most
 * one of each kind at once, and neither waits on the other. Builds and
 * changes share both, first come first served.
 */
const lanes = {
  quick: { queue: [], running: false },
  long: { queue: [], running: false }
};

/** Whether a job runs without a clock or a turn limit. */
const unbounded = job => job.model === 'fable';

const laneOf = job => (unbounded(job) ? lanes.long : lanes.quick);

const active = job => job.state === 'queued' || job.state === 'running';

const clip = (s, n) => (s.length > n ? s.slice(0, n) + '…' : s);

function enqueue(order) {
  const id = randomUUID();

  const job = {
    id,
    kind: order.key ? 'change' : 'build',
    state: 'queued',
    prompt: order.prompt,
    model: order.model,
    key: order.key,
    lambda: order.lambda,
    deploy: order.deploy,
    language: order.language,
    before: order.before,
    events: [],
    steps: [],
    created: Date.now(),
    result: null
  };

  jobs.set(id, job);

  if (job.lambda) latest.set(job.lambda, id);

  const lane = laneOf(job);

  lane.queue.push(id);
  setImmediate(() => pump(lane));

  // an hour is long enough for somebody to come back to a tab
  setTimeout(() => {
    jobs.delete(id);
    if (job.lambda && latest.get(job.lambda) === id) latest.delete(job.lambda);
  }, 60 * 60 * 1000).unref?.();

  return job;
}

async function pump(lane) {
  if (lane.running || lane.queue.length === 0) return;

  lane.running = true;

  const id = lane.queue.shift();
  const job = jobs.get(id);

  if (job && job.state === 'queued') {
    try {
      await run(job);
    } catch (e) {
      job.state = 'failed';
      job.result = { ok: false, error: String(e?.message ?? e) };
    }

    job.finished = Date.now();
    record(job);
  }

  lane.running = false;

  setImmediate(() => pump(lane));
}

/**
 * Stops a job: takes it off the queue if it has not started, kills its
 * container if it has. What it saved before that stays saved - versions are
 * only ever added - so stopping is always safe.
 */
function cancel(job) {
  if (job.state === 'queued') {
    const lane = laneOf(job);
    const at = lane.queue.indexOf(job.id);

    if (at >= 0) lane.queue.splice(at, 1);

    job.state = 'cancelled';
    job.finished = Date.now();
    job.result = { ok: false, cancelled: true, deployed: false };

    record(job);
    return;
  }

  if (job.state === 'running') {
    job.cancelled = true;

    if (job.box) spawn('docker', ['kill', job.box], { stdio: 'ignore' }).on('error', () => {});
  }
}

/** A line for the build page, which shows these as they are. */
function say(job, text) {
  job.events.push({ at: Date.now(), text });

  if (job.events.length > 120) job.events.shift();
}

/**
 * A step for the control center, which draws these in the owner's language:
 * what kind of thing happened, the facts of it, and - once the tool has
 * answered - how it went.
 */
function step(job, entry) {
  const at = Math.max(0, Math.round((Date.now() - (job.started ?? job.created)) / 1000));
  const made = { at, ...entry };

  job.steps.push(made);

  // a long unbounded job keeps its most recent steps, which are the ones
  // anybody is still reading
  if (job.steps.length > 200) job.steps.shift();

  return made;
}

/* --------------------------------------------------------------- the run */

async function run(job) {
  job.state = 'running';
  job.started = Date.now();

  say(job, job.kind === 'change' ? 'Reading what is already there' : 'Reading the platform guide');

  const brief = briefOf(job);
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
    '--allowedTools', ...(job.kind === 'change' ? CHANGE_ALLOW : ALLOW),
    '--disallowedTools', ...DENY
  ];

  const box = `build-${job.id.replace(/[^a-z0-9]/gi, '').slice(0, 24)}`;

  job.box = box;

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
  let expired = false;

  const killer = free ? null : setTimeout(() => {
    expired = true;
    spawn('docker', ['kill', box], { stdio: 'ignore' }).on('error', () => {});
  }, TIMEOUT);

  // a job stopped while the container was still starting has nothing to kill
  // yet at the moment it is asked; asking again once it runs catches that
  if (job.cancelled) spawn('docker', ['kill', box], { stdio: 'ignore' }).on('error', () => {});

  // a change already knows its own lambda, and nothing in the run announces
  // one: create_lambda is not even on its list
  let created = null;
  let deployed = false;
  let wrote = false;

  // what a change has done, from the answers of the tools rather than from
  // what the model says it did: the newest version it saved, the version it
  // put online, and whether the last thing compiled said it compiles
  let saved = null;
  let online = null;
  let compiles;

  // which call each result belongs to. Without it a result is just an object,
  // and two tools that answer with the same field are indistinguishable
  const calls = new Map();
  let summary = '';
  let stderr = '';

  // what it said after its last tool call, which is the closing note - but
  // only once the run has concluded: the last thing said by a run the clock
  // stopped halfway is "now fixing the error", which is no summary of anything
  let closing = '';
  let concluded = false;

  // how the CLI said the run ended: error_max_turns is a run that used up its
  // steps, which is worth telling apart from one that simply failed
  let ending = '';
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
            const tool = String(part.name ?? '').replace('mcp__genhttp__', '');

            calls.set(part.id, { tool, step: step(job, begin(tool, part.input)) });
            say(job, describe(part.name, part.input));
            closing = '';
          }

          if (part.type === 'text' && part.text.trim()) {
            summary = part.text.trim();
            closing = closing ? `${closing}\n\n${summary}` : summary;

            // what it says between tools is said to the owner, so it is kept
            // as a step of its own - clipped, since a model asked for one line
            // occasionally writes ten
            step(job, { kind: 'say', text: clip(summary, 400) });
          }
        }
      }

      // the keys come out of the tool result rather than out of the prose,
      // because the prose is a language model and the result is a fact
      if (event.type === 'user') {
        for (const part of event.message?.content ?? []) {
          if (part.type !== 'tool_result') continue;

          const call = calls.get(part.tool_use_id);
          const from = call?.tool;
          const body = parse(part.content);

          if (call) finish(call.step, from, body);

          // code having been written is the difference between an application
          // and an empty address. Nothing else in the run proves it: creating
          // a lambda seeds a starter template, and deploying that answers
          // exactly like deploying something real.
          if ((from === 'write_code' || from === 'change_code') && body?.ok === true) wrote = true;

          if ((from === 'write_code' || from === 'change_code' || from === 'copy_version') && Number.isInteger(body?.version)) {
            // a failed deploy after a save still names the version it saved
            saved = Math.max(saved ?? 0, body.version);
          }

          if (from === 'write_code' || from === 'change_code' || from === 'deploy' || from === 'check_code') {
            if (Array.isArray(body?.diagnostics)) compiles = errorsIn(body.diagnostics) === 0;
            else if (body?.ok === true && body.onlineUntil !== undefined) compiles = true;
          }

          if (body?.ok === true && body.onlineUntil !== undefined && Number.isInteger(body.version)) {
            online = body.version;
          }

          const found = harvest(body);

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

      if (event.type === 'result') {
        ending = String(event.subtype ?? '');

        if (event.subtype === 'success') {
          concluded = true;
          if (typeof event.result === 'string' && event.result.trim()) closing = event.result.trim();
        } else if (!summary) {
          summary = String(event.result ?? '').trim();
        }
      }
    }
  });

  const code = await new Promise(resolve => child.on('close', resolve));

  if (killer) clearTimeout(killer);

  // nothing to clean up: the container took its filesystem with it

  // the closing note is the result's summary; shown again as the last step
  // it would be said twice
  const last = job.steps[job.steps.length - 1];

  if (!concluded) closing = '';

  if (last?.kind === 'say' && closing && closing.includes(last.text.replace(/…$/, ''))) job.steps.pop();

  /*
   * Why it wrote nothing matters. A build that cannot sign in dies in a
   * second or two having done nothing at all, and telling that person to
   * "try asking for something smaller" blames their prompt for this server's
   * expired credentials - which is what it did, to everybody who used the
   * page, for the twelve hours the token was stale.
   */
  const unauthorised = /authenticat|oauth|401|revoked|invalid api key|credit balance/i
    .test(`${summary} ${stderr}`);

  if (job.kind === 'change') {
    // cut short by the clock, or by the number of steps it may take
    const cut = expired ? 'timeout' : ending === 'error_max_turns' ? 'turns' : undefined;

    settle(job, { wrote, saved, online, compiles, summary: closing, stderr, unauthorised, cut });
    return;
  }

  /*
   * A fresh build that never wrote code is a failure however cheerfully it
   * ended. create_lambda seeds the starter template, so the address answers,
   * the deploy succeeds and everything looks right - and what is online is
   * "My Lambda / It works". That was reported as a success, with a link,
   * to somebody who had asked for a game.
   */
  if (!wrote) {
    job.state = 'failed';
    job.result = {
      ok: false,
      error: unauthorised
        ? 'The builder could not sign in, so nothing ran. That is this server’s credentials '
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
      error: !free && (code === null || child.killed || expired)
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
 * How a change ended, from what the tools said rather than what the model
 * said about them.
 *
 * The owner already has the lambda, so there are no links to hand over: what
 * matters is which version it made, whether that is the one online now, and -
 * when it is not - why. The words are left to the control center, which says
 * them in the owner's language; the error strings here are for the log and
 * for a client that has no words of its own.
 */
function settle(job, { wrote, saved, online, compiles, summary, stderr, unauthorised, cut }) {
  const facts = {
    version: saved ?? undefined,
    online: online ?? undefined,
    deployed: online != null,
    compiles,
    before: job.before ?? undefined
  };

  if (job.cancelled) {
    job.state = 'cancelled';
    job.result = { ok: false, cancelled: true, ...facts };
    say(job, 'Stopped');
    return;
  }

  if (unauthorised && !wrote) {
    job.state = 'failed';
    job.result = {
      ok: false,
      reason: 'unauthorised',
      error: 'The agent could not sign in, so nothing ran. That is this server’s credentials '
        + 'rather than anything about what you asked for.',
      detail: clip(summary || stderr, 400)
    };
    return;
  }

  if (!wrote) {
    // an agent that looked and decided the change could not or should not be
    // made has done its job, and said why; one that was stopped has not
    job.state = cut || !summary ? 'failed' : 'done';
    job.result = {
      ok: false,
      unchanged: true,
      reason: cut ?? (summary ? undefined : 'nothing'),
      error: cut === 'timeout'
        ? 'It ran out of time before it changed anything.'
        : cut === 'turns'
          ? 'It used up its steps before it changed anything.'
          : 'Nothing was changed.',
      summary: summary ? clip(summary, 1200) : undefined,
      detail: summary ? undefined : clip(stderr, 400) || undefined
    };
    say(job, 'Nothing was changed');
    return;
  }

  job.state = 'done';
  job.result = {
    ok: true,
    ...facts,
    reason: cut,
    summary: summary ? clip(summary, 1200) : undefined
  };

  say(job, online != null ? `Version ${online} is online` : `Saved as version ${saved}`);
}

/**
 * One line per finished job.
 *
 * There was none of this, and the first time somebody asked why their build
 * came out wrong the only evidence left was the shape of the lambda table. A
 * prompt, what it did and how long it took is the difference between
 * answering that and guessing at it. The editor key of a change is never in
 * it: it is the one thing standing between anybody reading this log and the
 * ability to change what the job worked on.
 */
function record(job) {
  const seconds = Math.round(((job.finished ?? Date.now()) - (job.started ?? job.created)) / 1000);
  const r = job.result ?? {};

  console.log(JSON.stringify({
    at: new Date().toISOString(),
    id: job.id.slice(0, 8),
    kind: job.kind,
    model: job.model || 'opus',
    unbounded: unbounded(job) || undefined,
    lambda: job.lambda,
    seconds,
    state: job.state,
    wrote: r.ok === true || undefined,
    deployed: r.deployed,
    version: r.version,
    key: job.kind === 'build' ? r.publicKey : undefined,
    error: r.error,
    prompt: clip(job.prompt, 300)
  }));
}

/** A tool's answer as the object it is, or nothing where it answered in prose. */
function parse(content) {
  const texts = Array.isArray(content)
    ? content.filter(c => c.type === 'text').map(c => c.text)
    : [String(content ?? '')];

  for (const text of texts) {
    try {
      const body = JSON.parse(text);
      if (body && typeof body === 'object') return body;
    } catch { /* a refusal from the CLI itself is prose */ }
  }

  return null;
}

/** Pulls the facts out of a tool result. */
function harvest(body) {
  if (!body) return null;

  const found = {};

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

  return Object.keys(found).length > 0 ? found : null;
}

const errorsIn = diagnostics => diagnostics.filter(d => /error/i.test(String(d?.severity ?? ''))).length;

const names = list => (Array.isArray(list) ? list.map(f => String(f?.name ?? f ?? '')).filter(Boolean) : []);

/** What a tool call is about to do, as a step. */
function begin(tool, input) {
  switch (tool) {
    case 'platform_guide': return { kind: 'guide' };
    case 'list_demos': return { kind: 'demos' };
    case 'read_lambda':
      return input?.file ? { kind: 'read', files: [String(input.file)] } : { kind: 'read' };
    case 'read_logs': return { kind: 'logs' };
    case 'create_lambda': return { kind: 'create' };
    case 'write_code': return { kind: 'write', files: names(input?.files), whole: true };
    case 'change_code': {
      const files = new Set([
        ...names(input?.files),
        ...(Array.isArray(input?.edits) ? input.edits.map(e => String(e?.file ?? '')).filter(Boolean) : [])
      ]);
      const removed = Array.isArray(input?.remove) ? input.remove.map(String) : [];

      return { kind: 'write', files: [...files], ...(removed.length ? { removed } : {}) };
    }
    case 'copy_version': return { kind: 'copy' };
    case 'check_code': return { kind: 'check' };
    case 'deploy':
      return Number.isInteger(input?.version) ? { kind: 'deploy', version: input.version } : { kind: 'deploy' };
    case 'upload_file': return { kind: 'upload', path: String(input?.path ?? '') };
    case 'delete_file': return { kind: 'delete', path: String(input?.path ?? '') };
    case 'list_files': return { kind: 'list' };
    default: return { kind: 'other', tool };
  }
}

/** How a tool call went, written onto its step once the tool has answered. */
function finish(entry, tool, body) {
  entry.done = true;

  if (!body) return;

  if (body.ok === false && body.problem && !Array.isArray(body.diagnostics)) {
    entry.problem = clip(String(body.problem), 300);
    return;
  }

  switch (tool) {
    case 'write_code':
    case 'change_code':
    case 'copy_version':
    case 'deploy':
      if (Number.isInteger(body.version)) entry.version = body.version;

      if (body.ok === true && body.onlineUntil !== undefined) entry.online = true;

      if (Array.isArray(body.diagnostics)) {
        entry.errors = errorsIn(body.diagnostics);
        if (body.ok === false) entry.online = false;
      }
      break;

    case 'check_code':
      entry.errors = errorsIn(body.diagnostics ?? []);
      break;

    case 'read_lambda':
      if (Number.isInteger(body.version)) entry.version = body.version;
      break;

    case 'read_logs':
      if (Array.isArray(body.lines)) {
        entry.problems = body.lines.filter(l => /error|critical/i.test(String(l?.level ?? ''))).length;
      }
      break;
  }
}

const WORDS = {
  platform_guide: 'Reading the platform guide',
  list_demos: 'Looking at the demos',
  create_lambda: 'Claiming an address',
  write_code: 'Writing the code',
  change_code: 'Changing the code',
  copy_version: 'Starting a new version',
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

  if (short === 'change_code') {
    const count = (input?.files?.length ?? 0) + (input?.edits?.length ?? 0) + (input?.remove?.length ?? 0);

    return count > 0 ? `Making ${count} change${count === 1 ? '' : 's'}` : WORDS.change_code;
  }

  return WORDS[short] ?? `Working (${short})`;
}

/** How a job is getting on, as both pages read it. */
function progress(job) {
  const lane = laneOf(job);
  const from = job.started ?? job.created;
  const until = job.finished ?? Date.now();

  return {
    id: job.id,
    kind: job.kind,
    state: job.state,
    events: job.events.map(e => e.text),
    steps: job.steps,
    result: job.result,
    waiting: lane.queue.indexOf(job.id) + 1,
    // how long it has been running, measured here, so a browser with its
    // clock wrong still counts the right number of seconds
    seconds: job.started ? Math.max(0, Math.round((until - from) / 1000)) : 0,
    limit: unbounded(job) ? null : Math.round(TIMEOUT / 1000),
    // what a change was asked for is only ever read back by whoever holds
    // the key it was started with; a build's page still has it in its box
    ...(job.kind === 'change'
      ? { prompt: job.prompt, deploy: job.deploy, model: job.model || 'opus', before: job.before ?? null }
      : {})
  };
}

/* ------------------------------------------------------------ the server */

const send = (res, status, body) => {
  const payload = JSON.stringify(body);

  res.writeHead(status, { 'content-type': 'application/json', 'content-length': Buffer.byteLength(payload) });
  res.end(payload);
};

/** The body of a request, as JSON, or nothing if it was too large or not JSON. */
const read = req => new Promise(resolve => {
  let body = '';

  req.on('data', d => {
    body += d;
    if (body.length > 8000) { req.destroy(); resolve(null); }
  });

  req.on('end', () => {
    try { resolve(JSON.parse(body)); } catch { resolve(null); }
  });
});

createServer(async (req, res) => {
  // only the lambda server can reach this network, but a shared secret costs
  // nothing and means a mistake in the network definition is not an open door
  if (TOKEN && req.headers['x-agent-token'] !== TOKEN) {
    return send(res, 403, { error: 'no' });
  }

  const url = new URL(req.url ?? '/', 'http://agent');

  if (req.method === 'GET' && url.pathname === '/health') {
    return send(res, 200, {
      ok: true,
      models: Object.keys(MODELS),
      quick: { running: lanes.quick.running, queued: lanes.quick.queue.length },
      long: { running: lanes.long.running, queued: lanes.long.queue.length }
    });
  }

  if (req.method === 'POST' && url.pathname === '/build') {
    const body = await read(req) ?? {};

    const prompt = String(body.prompt ?? '').trim();

    if (prompt.length < 3) return send(res, 400, { error: 'Say what you want built.' });

    if (lanes.quick.queue.length + lanes.long.queue.length > 12) {
      return send(res, 503, { error: 'Too many builds waiting. Try again shortly.' });
    }

    const model = String(body.model ?? '').trim();

    if (model && !Object.hasOwn(MODELS, model)) {
      return send(res, 400, { error: 'There is no such model here.' });
    }

    // a key makes it a change of the lambda it opens, filed under the tag the
    // server gave it so the server can ask after it by lambda later
    const key = String(body.key ?? '').trim();
    const lambda = String(body.lambda ?? '').trim();

    if (key && !/^[a-z0-9-]{8,64}$/.test(key)) {
      return send(res, 400, { error: 'That does not look like an editor key.' });
    }

    if (key && !/^[0-9]{1,18}$/.test(lambda)) {
      return send(res, 400, { error: 'A change needs the lambda it belongs to.' });
    }

    if (key) {
      const running = jobs.get(latest.get(lambda));

      // two agents writing the same files at once would each undo the other
      if (running && active(running)) {
        return send(res, 409, { error: 'A change of this lambda is already under way.', id: running.id });
      }
    }

    const job = enqueue({
      prompt: clip(prompt, 2000),
      model,
      key: key || undefined,
      lambda: key ? lambda : undefined,
      deploy: body.deploy !== false,
      language: Object.hasOwn(LANGUAGES, body.language) ? body.language : undefined,
      before: Number.isInteger(body.before) ? body.before : undefined
    });

    return send(res, 202, { id: job.id, queued: laneOf(job).queue.length, ...(key ? progress(job) : {}) });
  }

  // the newest job of a lambda, which is how the control center finds a
  // change it did not start in this tab
  const byLambda = /^\/lambda\/([0-9]{1,18})$/.exec(url.pathname);

  if (byLambda && req.method === 'GET') {
    const job = jobs.get(latest.get(byLambda[1]));

    return job ? send(res, 200, progress(job)) : send(res, 404, { error: 'Nothing has been asked of this lambda lately.' });
  }

  const match = /^\/build\/([0-9a-f-]{36})$/.exec(url.pathname);

  if (match && req.method === 'GET') {
    const job = jobs.get(match[1]);

    if (!job) return send(res, 404, { error: 'No such build.' });

    return send(res, 200, progress(job));
  }

  if (match && req.method === 'DELETE') {
    const job = jobs.get(match[1]);

    // the lambda is checked here as well as by the server, so a job can only
    // be stopped by whoever could have started it
    if (!job || (job.lambda ?? '') !== (url.searchParams.get('lambda') ?? '')) {
      return send(res, 404, { error: 'No such build.' });
    }

    cancel(job);

    return send(res, 200, progress(job));
  }

  return send(res, 404, { error: 'No such thing.' });
}).listen(PORT, () => console.log(`build agent listening on ${PORT}, model ${MODEL}, mcp ${MCP_URL}`));
