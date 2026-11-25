/*
 * Which installations a runner builds for, and which of them asks: read from
 * the environment, without starting anything.
 * CI runs it with node --test 'docker/agent/*.test.mjs'.
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';

import { hostsFrom, hostFor, directOf, HostsError, Installations } from './hosts.mjs';

const A = { name: 'team-a', token: 'a'.repeat(32), mcp: 'http://lambda-a:8080/mcp', origin: 'https://a.example.com' };
const B = { name: 'team-b', token: 'b'.repeat(32), mcp: 'https://b.example.com/mcp', origin: 'https://b.example.com/' };

const shared = (...hosts) => hostsFrom({ AGENT_HOSTS: JSON.stringify(hosts) });

const refused = (env, message) => assert.throws(() => hostsFrom(env), e => e instanceof HostsError && message.test(e.message));

test('without AGENT_HOSTS the runner builds for the one installation it always did', () => {
  const { hosts, shared: several, ignored } = hostsFrom({
    AGENT_TOKEN: 'secret', AGENT_MCP_URL: 'http://lambda:8080/mcp', AGENT_PUBLIC_ORIGIN: 'http://localhost:8080/'
  });

  assert.equal(several, false);
  assert.deepEqual(ignored, []);
  assert.deepEqual(hosts, [{ name: 'default', token: 'secret', mcp: 'http://lambda:8080/mcp', origin: 'http://localhost:8080' }]);
});

test('the one installation keeps the defaults it had: genhttp.dev, and no token', () => {
  const { hosts } = hostsFrom({});

  assert.deepEqual(hosts, [{ name: 'default', token: '', mcp: 'https://genhttp.dev/mcp', origin: 'https://genhttp.dev' }]);
});

test('AGENT_HOSTS names several installations, each with its own MCP and origin', () => {
  const { hosts, shared: several } = shared(A, B);

  assert.equal(several, true);
  assert.deepEqual(hosts.map(h => [h.name, h.mcp, h.origin]), [
    ['team-a', 'http://lambda-a:8080/mcp', 'https://a.example.com'],
    ['team-b', 'https://b.example.com/mcp', 'https://b.example.com']
  ]);
});

test('AGENT_HOSTS_FILE is read like AGENT_HOSTS', () => {
  const { hosts } = hostsFrom({ AGENT_HOSTS_FILE: '/run/secrets/hosts.json' }, path => {
    assert.equal(path, '/run/secrets/hosts.json');
    return JSON.stringify([A]);
  });

  assert.deepEqual(hosts.map(h => h.name), ['team-a']);
});

test('the single installation\'s variables are said to be ignored beside AGENT_HOSTS', () => {
  const { ignored } = hostsFrom({ AGENT_HOSTS: JSON.stringify([A]), AGENT_TOKEN: 'x', AGENT_MCP_URL: '' });

  assert.deepEqual(ignored, ['AGENT_TOKEN']);
});

test('a configuration that would send a build to the wrong installation is refused', () => {
  refused({ AGENT_HOSTS: '[{' }, /not JSON/);
  refused({ AGENT_HOSTS: '{}' }, /JSON array/);
  refused({ AGENT_HOSTS: '[]' }, /JSON array/);
  refused({ AGENT_HOSTS: '[1]' }, /not an object/);
  refused({ AGENT_HOSTS: JSON.stringify([A]), AGENT_HOSTS_FILE: '/x' }, /both set/);
  refused({ AGENT_HOSTS_FILE: '/nowhere' }, /cannot be read/);

  refused({ AGENT_HOSTS: JSON.stringify([{ ...A, mcpUrl: A.mcp }]) }, /mcpUrl, which is not one of/);
  refused({ AGENT_HOSTS: JSON.stringify([{ ...A, name: 'Team A' }]) }, /needs a name/);
  refused({ AGENT_HOSTS: JSON.stringify([{ ...A, token: 'short' }]) }, /at least 16/);
  refused({ AGENT_HOSTS: JSON.stringify([{ ...A, mcp: undefined }]) }, /needs mcp/);
  refused({ AGENT_HOSTS: JSON.stringify([{ ...A, mcp: 'lambda-a:8080' }]) }, /neither http nor https|not an address/);
  refused({ AGENT_HOSTS: JSON.stringify([{ ...A, origin: undefined }]) }, /needs origin/);

  refused({ AGENT_HOSTS: JSON.stringify([A, { ...B, name: A.name }]) }, /called team-a/);
  refused({ AGENT_HOSTS: JSON.stringify([A, { ...B, token: A.token }]) }, /same token/);
});

test('the token a request carries says which installation asks', () => {
  const { hosts } = shared(A, B);

  assert.equal(hostFor(hosts, A.token)?.name, 'team-a');
  assert.equal(hostFor(hosts, B.token)?.name, 'team-b');
  assert.equal(hostFor(hosts, [B.token, A.token])?.name, 'team-b');

  assert.equal(hostFor(hosts, undefined), null);
  assert.equal(hostFor(hosts, ''), null);
  assert.equal(hostFor(hosts, A.token.slice(1)), null);
});

test('the one installation answers to its token, or to anybody where it has none', () => {
  const secret = hostsFrom({ AGENT_TOKEN: 'secret' }).hosts;

  assert.equal(hostFor(secret, 'secret')?.name, 'default');
  assert.equal(hostFor(secret, 'wrong'), null);
  assert.equal(hostFor(secret, undefined), null);

  const open = hostsFrom({}).hosts;

  assert.equal(hostFor(open, undefined)?.name, 'default');
  assert.equal(hostFor(open, 'anything')?.name, 'default');
});

test('builds reach the MCP of every installation sharing the runner without the proxy', () => {
  const { hosts } = shared(A, B);

  assert.equal(directOf(hosts, true, ['localhost,127.0.0.1', 'egress-proxy', null]),
    'localhost,127.0.0.1,egress-proxy,lambda-a,b.example.com');
});

test('the one installation keeps NO_PROXY as it was given', () => {
  const { hosts } = hostsFrom({ AGENT_MCP_URL: 'https://genhttp.dev/mcp' });

  assert.equal(directOf(hosts, false, ['lambda,localhost', 'egress-proxy']), 'lambda,localhost,egress-proxy');
  assert.equal(directOf(hosts, false, [undefined, null]), '');
});

/** A file of installations that can be written again. */
function file(...hosts) {
  let text = JSON.stringify(hosts);
  let written = 0;

  return {
    write: value => { text = typeof value === 'string' ? value : JSON.stringify(value); written++; },
    installations: () => new Installations({ AGENT_HOSTS_FILE: '/agent/hosts.json' }, () => text, () => String(written))
  };
}

test('an installation added to the file is known without a restart', () => {
  const on = file(A);
  const installations = on.installations();

  assert.equal(installations.find(B.token), null);
  assert.equal(installations.refresh(), null, 'nothing changed');

  on.write([A, B]);

  assert.deepEqual(installations.refresh(), { added: ['team-b'], removed: [] });
  assert.equal(installations.find(B.token)?.name, 'team-b');
  assert.equal(installations.refresh(), null, 'read once');
});

test('an installation that stays is the one its jobs hold, with what it says now', () => {
  const on = file(A, B);
  const installations = on.installations();

  const before = installations.find(A.token);

  on.write([{ ...A, token: 'c'.repeat(32), mcp: 'http://elsewhere:8080/mcp' }]);

  assert.deepEqual(installations.refresh(), { added: [], removed: ['team-b'] });

  const now = installations.find('c'.repeat(32));

  assert.equal(now, before, 'its jobs are filed under it');
  assert.equal(now.mcp, 'http://elsewhere:8080/mcp');
  assert.equal(installations.find(A.token), null, 'the token it had is none any more');
  assert.equal(installations.find(B.token), null);
});

test('a file that is wrong changes nothing until it reads again, and is said once', () => {
  const on = file(A);
  const installations = on.installations();

  on.write('[{ "name": "team-a"');

  assert.match(installations.refresh().problem, /not JSON/);
  assert.equal(installations.refresh(), null, 'said once');
  assert.equal(installations.find(A.token)?.name, 'team-a');

  on.write([]);

  assert.match(installations.refresh().problem, /JSON array of installations/);
  assert.equal(installations.find(A.token)?.name, 'team-a');

  on.write([A, B]);

  assert.deepEqual(installations.refresh(), { added: ['team-b'], removed: [] });
});

test('a file that is gone for a moment changes nothing', () => {
  let there = true;

  const gone = () => { throw Object.assign(new Error('gone'), { code: 'ENOENT' }); };

  const installations = new Installations({ AGENT_HOSTS_FILE: '/agent/hosts.json' },
    () => there ? JSON.stringify([A]) : gone(), () => there ? '1' : gone());

  there = false;

  assert.match(installations.refresh().problem, /cannot be read: ENOENT/);
  assert.equal(installations.find(A.token)?.name, 'team-a');

  there = true;

  assert.deepEqual(installations.refresh(), { added: [], removed: [] });
});

test('what the environment names does not change', () => {
  const installations = new Installations({ AGENT_HOSTS: JSON.stringify([A]) }, () => assert.fail('read'), () => assert.fail('looked at'));

  assert.equal(installations.refresh(), null);
  assert.equal(installations.shared, true);
});
