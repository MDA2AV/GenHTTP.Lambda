/*
 * The runner as the lambda servers see it: started as it runs in its
 * container, with a stand-in for the docker CLI that plays a build instead of
 * starting one - so what is checked is where a job goes, who may see it and
 * how many run at once, not what a model does.
 *
 * The stand-in is a script called docker, first on the runner's PATH, so it
 * runs where a shell script runs: not on Windows, where these are skipped.
 * CI runs it with node --test 'docker/agent/*.test.mjs'.
 */

import { test, before, after } from 'node:test';
import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { mkdtempSync, writeFileSync, readFileSync, chmodSync, renameSync, rmSync } from 'node:fs';
import { createServer } from 'node:net';
import { tmpdir } from 'node:os';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { setTimeout as delay } from 'node:timers/promises';

const skip = process.platform === 'win32' ? 'the stand-in for docker is a shell script' : false;

const HERE = dirname(fileURLToPath(import.meta.url));

const A = { name: 'team-a', token: 'a'.repeat(32), mcp: 'http://lambda-a:8080/mcp', origin: 'https://a.example.com' };
const B = { name: 'team-b', token: 'b'.repeat(32), mcp: 'http://lambda-b:8080/mcp', origin: 'https://b.example.com' };

/*
 * What the stand-in does with "docker run": writes down what the build was
 * given, waits while the brief says HOLD and no release file is there, and
 * answers as the CLI would for a build that saved and deployed a version - on
 * a lambda whose keys say which MCP endpoint the build was pointed at.
 */
const STAND_IN = `#!/usr/bin/env node
const { writeFileSync, existsSync } = require('node:fs');
const args = process.argv.slice(2);
if (args[0] !== 'run') process.exit(0);
const box = args[args.indexOf('--name') + 1];
const config = JSON.parse(process.env.AGENT_MCP_CONFIG);
const mcp = new URL(config.mcpServers.genhttp.url);
writeFileSync(process.env.STAND_IN_DIR + '/' + box + '.json', JSON.stringify({ config, direct: process.env.NO_PROXY }));
const say = event => process.stdout.write(JSON.stringify(event) + '\\n');
const at = mcp.hostname;
const answer = () => {
  say({ type: 'assistant', message: { content: [{ type: 'tool_use', id: 't1', name: 'mcp__genhttp__write_code', input: { files: [] } }] } });
  say({ type: 'user', message: { content: [{ type: 'tool_result', tool_use_id: 't1', content: [{ type: 'text', text: JSON.stringify({
    ok: true, version: 1, online: true, publicKey: 'made-at-' + at, privateKey: 'key-of-' + at, publicUrl: 'https://' + at + '.example/'
  }) }] }] } });
  say({ type: 'result', subtype: 'success', is_error: false, result: 'Built at ' + at + '.' });
};
const held = process.env.AGENT_BRIEF.includes('HOLD');
const wait = () => (!held || existsSync(process.env.STAND_IN_DIR + '/release') ? answer() : setTimeout(wait, 20));
wait();
`;

let folder;

before(() => {
  if (skip) return;

  folder = mkdtempSync(join(tmpdir(), 'runner-test-'));
  writeFileSync(join(folder, 'docker'), STAND_IN);
  chmodSync(join(folder, 'docker'), 0o755);
});

after(() => {
  if (folder) rmSync(folder, { recursive: true, force: true });
});

const freePort = () => new Promise(resolve => {
  const server = createServer().listen(0, () => {
    const { port } = server.address();
    server.close(() => resolve(port));
  });
});

/**
 * Starts the runner with the given environment, and resolves once it listens
 * - or with how it ended, where it would not start.
 */
async function start(env) {
  const port = await freePort();

  const child = spawn(process.execPath, [join(HERE, 'builder.mjs')], {
    env: {
      PATH: `${folder}:${process.env.PATH}`,
      AGENT_PORT: String(port),
      AGENT_FETCH: 'false',
      ANTHROPIC_API_KEY: 'not-a-real-key',
      STAND_IN_DIR: folder,
      ...env
    },
    stdio: ['ignore', 'pipe', 'pipe']
  });

  let output = '';

  child.stdout.on('data', d => { output += d; });
  child.stderr.on('data', d => { output += d; });

  const ended = new Promise(resolve => child.on('exit', code => resolve({ code, output })));

  while (!output.includes('listening on')) {
    const done = await Promise.race([ended, delay(20).then(() => null)]);

    if (done) return { ended: done };
  }

  const call = async (token, method, path, body) => {
    const response = await fetch(`http://127.0.0.1:${port}${path}`, {
      method,
      headers: { 'content-type': 'application/json', ...(token ? { 'x-agent-token': token } : {}) },
      body: body ? JSON.stringify(body) : undefined
    });

    return { status: response.status, body: await response.json() };
  };

  return { call, port, output: () => output, stop: () => child.kill() };
}

/** Asks after a job until it has ended. */
async function settled(call, token, id) {
  for (;;) {
    const { body } = await call(token, 'GET', `/build/${id}`);

    if (body.state !== 'queued' && body.state !== 'running') return body;

    await delay(20);
  }
}

/** What the stand-in was given for a job. */
const given = id => JSON.parse(readFileSync(join(folder, `build-${id.replace(/[^a-z0-9]/gi, '').slice(0, 24)}.json`), 'utf8'));

const shared = extra => start({ AGENT_HOSTS: JSON.stringify([A, B]), ...extra });

const ticket = '0123456789abcdef0123456789abcdef';

test('each installation\'s build is made at its own MCP and handed back with its own links', { skip }, async () => {
  const { call, stop } = await shared();

  try {
    const fromA = await call(A.token, 'POST', '/build', { prompt: 'a guest book', ticket });
    const fromB = await call(B.token, 'POST', '/build', { prompt: 'a quiz' });

    assert.equal(fromA.status, 202);
    assert.equal(fromB.status, 202);

    const builtA = await settled(call, A.token, fromA.body.id);
    const builtB = await settled(call, B.token, fromB.body.id);

    assert.equal(builtA.state, 'done');
    assert.equal(builtA.result.publicKey, 'made-at-lambda-a');
    assert.equal(builtA.result.editorUrl, 'https://a.example.com/editor/key-of-lambda-a');

    assert.equal(builtB.result.publicKey, 'made-at-lambda-b');
    assert.equal(builtB.result.editorUrl, 'https://b.example.com/editor/key-of-lambda-b');

    // the ticket goes to the installation that issued it, and only with its job
    const configA = given(fromA.body.id).config.mcpServers.genhttp;

    assert.equal(configA.url, A.mcp);
    assert.equal(configA.headers['X-Lambda-Agent'], ticket);
    assert.equal(given(fromB.body.id).config.mcpServers.genhttp.headers, undefined);

    // reached on the build network, never asked for through the proxy
    assert.match(given(fromA.body.id).direct, /(^|,)lambda-a(,|$)/);
    assert.match(given(fromA.body.id).direct, /(^|,)lambda-b(,|$)/);
  } finally {
    stop();
  }
});

test('an installation sees, waits for and stops only its own jobs', { skip }, async () => {
  const { call, stop } = await shared();

  try {
    // lambda 12 of each installation is a different lambda
    const changeA = await call(A.token, 'POST', '/build', { prompt: 'HOLD make it blue', key: 'editorkeyofa', lambda: '12' });
    const changeB = await call(B.token, 'POST', '/build', { prompt: 'HOLD make it red', key: 'editorkeyofb', lambda: '12' });

    assert.equal(changeA.status, 202);
    assert.equal(changeB.status, 202, 'a change of another installation\'s lambda 12 is no change under way');

    assert.equal((await call(A.token, 'GET', '/lambda/12')).body.prompt, 'HOLD make it blue');
    assert.equal((await call(B.token, 'GET', '/lambda/12')).body.prompt, 'HOLD make it red');

    // the same installation's lambda is busy, as it always was
    assert.equal((await call(A.token, 'POST', '/build', { prompt: 'and green', key: 'editorkeyofa', lambda: '12' })).status, 409);

    // another installation does not find the job, by id or to stop it
    assert.equal((await call(B.token, 'GET', `/build/${changeA.body.id}`)).status, 404);
    assert.equal((await call(B.token, 'DELETE', `/build/${changeA.body.id}?lambda=12`)).status, 404);

    const stopped = await call(A.token, 'DELETE', `/build/${changeA.body.id}?lambda=12`);

    assert.equal(stopped.status, 200);

    writeFileSync(join(folder, 'release'), '');

    assert.equal((await settled(call, B.token, changeB.body.id)).state, 'done');
    assert.equal((await settled(call, A.token, changeA.body.id)).state, 'cancelled');
  } finally {
    rmSync(join(folder, 'release'), { force: true });
    stop();
  }
});

test('as many jobs run at once as AGENT_CONCURRENCY allows, and the health says whose', { skip }, async () => {
  const { call, stop } = await shared({ AGENT_CONCURRENCY: '2' });

  try {
    const first = await call(A.token, 'POST', '/build', { prompt: 'HOLD one' });
    const second = await call(B.token, 'POST', '/build', { prompt: 'HOLD two' });
    const third = await call(A.token, 'POST', '/build', { prompt: 'HOLD three' });

    // both places taken - one each - and the third waiting for one
    for (;;) {
      const states = await Promise.all([[A, first], [B, second]].map(async ([host, job]) => (await call(host.token, 'GET', `/build/${job.body.id}`)).body.state));

      if (states.every(state => state === 'running')) break;

      await delay(20);
    }

    assert.equal((await call(A.token, 'GET', `/build/${third.body.id}`)).body.state, 'queued');

    const healthA = (await call(A.token, 'GET', '/health')).body;
    const healthB = (await call(B.token, 'GET', '/health')).body;

    assert.deepEqual(healthA.quick, { running: 1, queued: 1 });
    assert.deepEqual(healthB.quick, { running: 1, queued: 0 });
    assert.equal(healthA.concurrency, 2);
    assert.equal(healthA.installations, 2);

    writeFileSync(join(folder, 'release'), '');

    assert.equal((await settled(call, A.token, third.body.id)).state, 'done');
  } finally {
    rmSync(join(folder, 'release'), { force: true });
    stop();
  }
});

test('a request without a known token is turned away', { skip }, async () => {
  const { call, stop } = await shared();

  try {
    assert.equal((await call(undefined, 'GET', '/health')).status, 403);
    assert.equal((await call('c'.repeat(32), 'POST', '/build', { prompt: 'a guest book' })).status, 403);
  } finally {
    stop();
  }
});

test('without AGENT_HOSTS the runner builds for the one installation as it always did', { skip }, async () => {
  const { call, stop } = await start({
    AGENT_TOKEN: 'secret',
    AGENT_MCP_URL: 'http://lambda:8080/mcp',
    AGENT_PUBLIC_ORIGIN: 'http://localhost:8080',
    NO_PROXY: 'lambda,localhost'
  });

  try {
    assert.equal((await call('wrong', 'GET', '/health')).status, 403);

    const health = (await call('secret', 'GET', '/health')).body;

    assert.equal(health.installations, undefined);
    assert.equal(health.concurrency, 1);

    const started = await call('secret', 'POST', '/build', { prompt: 'a guest book' });
    const built = await settled(call, 'secret', started.body.id);

    assert.equal(built.result.editorUrl, 'http://localhost:8080/editor/key-of-lambda');
    assert.equal(given(started.body.id).config.mcpServers.genhttp.url, 'http://lambda:8080/mcp');
    assert.equal(given(started.body.id).direct, 'lambda,localhost');
  } finally {
    stop();
  }
});

test('a runner whose installations are named wrongly does not start, and says why', { skip }, async () => {
  const { ended } = await start({ AGENT_HOSTS: JSON.stringify([A, { ...B, token: A.token }]) });

  assert.equal(ended?.code, 1);
  assert.match(ended.output, /same token/);

  const { ended: many } = await start({ AGENT_CONCURRENCY: 'lots' });

  assert.equal(many?.code, 1);
  assert.match(many.output, /AGENT_CONCURRENCY/);
});

test('an installation added to the file while the runner runs has its builds made at once', { skip }, async () => {
  const file = join(folder, 'hosts.json');

  writeFileSync(file, JSON.stringify([A]));

  const runner = await start({ AGENT_HOSTS_FILE: file, AGENT_CONCURRENCY: '2' });

  try {
    assert.equal((await runner.call(B.token, 'GET', '/health')).status, 403);

    // a build under way, which a restart would have ended
    const { body: held } = await runner.call(A.token, 'POST', '/build', { prompt: 'HOLD it', ticket });

    writeFileSync(`${file}.new`, JSON.stringify([A, B]));
    renameSync(`${file}.new`, file);

    const { status, body: started } = await runner.call(B.token, 'POST', '/build', { prompt: 'an app', ticket });

    assert.equal(status, 202);

    const built = await settled(runner.call, B.token, started.id);

    assert.equal(built.result.editorUrl, 'https://b.example.com/editor/key-of-lambda-b');
    assert.deepEqual(given(started.id).direct.split(',').sort(), ['lambda-a', 'lambda-b']);

    assert.match(runner.output(), /building for 2 installations now - added team-b/);

    // the file made wrong: those that are known stay known
    writeFileSync(file, 'nonsense');

    assert.equal((await runner.call(B.token, 'GET', '/health')).status, 200);
    assert.match(runner.output(), /The installations stay as they were: AGENT_HOSTS_FILE is not JSON/);

    writeFileSync(join(folder, 'release'), '');

    assert.equal((await settled(runner.call, A.token, held.id)).result.editorUrl, 'https://a.example.com/editor/key-of-lambda-a');
  } finally {
    rmSync(join(folder, 'release'), { force: true });
    runner.stop();
  }
});

/* ------------------------------------------------------------ protocol 2 */

/** The events of a job as a platform of protocol 2 follows them, until it ended. */
async function followed(port, token, id, after = 0) {
  const response = await fetch(`http://127.0.0.1:${port}/build/${id}/events?after=${after}`, { headers: { 'x-agent-token': token } });

  assert.equal(response.status, 200);

  const text = await response.text();

  return text.split('\n').filter(line => line.trim()).map(line => JSON.parse(line));
}

test('a platform of protocol 2 names the run and follows what happens in it as events', { skip }, async () => {
  const runner = await start({ AGENT_TOKEN: A.token, AGENT_MCP_URL: A.mcp, AGENT_PUBLIC_ORIGIN: A.origin });
  const { port } = runner;

  try {
    const run = '0f0e0d0c-0b0a-4908-8706-050403020100';

    const taken = await runner.call(A.token, 'POST', '/build', { prompt: 'a guest book', ticket, protocol: 2, run, view: 'simple', timeout: 120, maxTurns: 7,
      attachments: [{ id: 'a'.repeat(32), name: 'logo.png', type: 'image/png', size: 2048 }] });

    assert.equal(taken.status, 202);
    assert.equal(taken.body.id, run, 'filed under the platform\'s own id');

    const events = await followed(port, A.token, run);

    assert.equal(events[0].type, 'queued');
    assert.ok(events.some(e => e.type === 'started'));

    const ended = events.at(-1);

    assert.equal(ended.type, 'ended');
    assert.equal(ended.ending, 'completed');
    assert.equal(ended.message, 'Built at lambda-a.');
    assert.ok(events.every((e, i) => i === 0 || e.seq > events[i - 1].seq), 'numbered in order');

    // asked again from a position, only what came after it
    assert.deepEqual((await followed(port, A.token, run, ended.seq - 1)).map(e => e.type), ['ended']);

    // its MCP has the hand-over beside the platform's, and no fetch: the runner reads no page here
    assert.ok(given(run).config.mcpServers.genhttp);
  } finally {
    runner.stop();
  }
});

test('an older platform is answered as it always was', { skip }, async () => {
  const runner = await start({ AGENT_TOKEN: A.token, AGENT_MCP_URL: A.mcp, AGENT_PUBLIC_ORIGIN: A.origin });

  try {
    const taken = await runner.call(A.token, 'POST', '/build', { prompt: 'a guest book', ticket, run: '0f0e0d0c-0b0a-4908-8706-050403020101' });

    const built = await settled(runner.call, A.token, taken.body.id);

    assert.equal(built.state, 'done');
    assert.equal(built.result.publicKey, 'made-at-lambda-a', 'with the result it reads');

    const health = await runner.call(A.token, 'GET', '/health');

    assert.equal(health.body.protocol, 2, 'and the protocol it may speak instead');
    assert.equal(health.body.fetch, false, 'still said the way it understands');
  } finally {
    runner.stop();
  }
});
