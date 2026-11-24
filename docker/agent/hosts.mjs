/*
 * The installations a runner builds for.
 *
 * A runner used to belong to one lambda server: it took jobs from whoever
 * held AGENT_TOKEN and sent every build to AGENT_MCP_URL. Several servers on
 * one machine can share one runner instead - one image, one set of
 * credentials, one place to decide how many builds run at once - as long as
 * each job goes back to the server that asked for it. A build sent to another
 * server's MCP creates its lambda there, and the server that asked hands its
 * visitor the keys of an application it does not have.
 *
 * So every installation is named in AGENT_HOSTS (or the file AGENT_HOSTS_FILE
 * names) with a token of its own, and the token a request carries is what
 * says which installation asked: its MCP address and its public origin are the
 * runner's configuration, never what a request says. Without AGENT_HOSTS the
 * runner is what it always was - one installation, AGENT_TOKEN (or none),
 * AGENT_MCP_URL and AGENT_PUBLIC_ORIGIN.
 *
 * Kept apart from builder.mjs, which starts a server as it is loaded, so that
 * the rules can be checked on their own (hosts.test.mjs).
 */

import { createHash, timingSafeEqual } from 'node:crypto';
import { readFileSync } from 'node:fs';

/** What an installation is called in the runner's log and nowhere else. */
const NAME = /^[a-z0-9](?:[a-z0-9-]{0,30}[a-z0-9])?$/;

/**
 * The shortest token an installation may have where several share a runner:
 * the token is what tells them apart, so it has to be more than a guess.
 */
export const SHORTEST_TOKEN = 16;

/** What an entry of AGENT_HOSTS may say. Anything else is a typo, and refused. */
const FIELDS = new Set(['name', 'token', 'mcp', 'origin']);

/** The variables of the single installation, which AGENT_HOSTS takes the place of. */
const SINGLE = ['AGENT_TOKEN', 'AGENT_MCP_URL', 'AGENT_PUBLIC_ORIGIN'];

/** A configuration the runner cannot start with; the message says what to fix. */
export class HostsError extends Error {}

/**
 * The installations of a runner, from its environment.
 *
 * Throws HostsError where AGENT_HOSTS is there and wrong: a runner that
 * guessed would send somebody's build to the wrong installation.
 *
 * @param {Record<string, string | undefined>} env the runner's environment
 * @param {(path: string) => string} read how a file is read, for AGENT_HOSTS_FILE
 * @returns {{ hosts: Host[], shared: boolean, ignored: string[] }}
 *   the installations; whether there are several by configuration (AGENT_HOSTS)
 *   rather than the single one; and the single installation's variables that
 *   were set but are not read
 */
export function hostsFrom(env, read = path => readFileSync(path, 'utf8')) {
  const file = (env.AGENT_HOSTS_FILE ?? '').trim();
  const inline = (env.AGENT_HOSTS ?? '').trim();

  if (!file && !inline) {
    return {
      hosts: [{
        name: 'default',
        token: env.AGENT_TOKEN ?? '',
        mcp: env.AGENT_MCP_URL ?? 'https://genhttp.dev/mcp',
        origin: (env.AGENT_PUBLIC_ORIGIN ?? 'https://genhttp.dev').replace(/\/+$/, '')
      }],
      shared: false,
      ignored: []
    };
  }

  if (file && inline) {
    throw new HostsError('AGENT_HOSTS and AGENT_HOSTS_FILE are both set: name the installations in one of them');
  }

  let text = inline;

  if (file) {
    try {
      text = read(file);
    } catch (e) {
      throw new HostsError(`AGENT_HOSTS_FILE names ${file}, which cannot be read: ${e?.code ?? e?.message ?? e}`);
    }
  }

  let list;

  try {
    list = JSON.parse(text);
  } catch (e) {
    throw new HostsError(`${file ? 'AGENT_HOSTS_FILE' : 'AGENT_HOSTS'} is not JSON: ${e.message}`);
  }

  if (!Array.isArray(list) || list.length === 0) {
    throw new HostsError('AGENT_HOSTS is a JSON array of installations, each { "name", "token", "mcp", "origin" }');
  }

  const hosts = list.map((entry, at) => hostOf(entry, at));

  for (const [field, what] of [['name', 'name'], ['token', 'token']]) {
    const seen = new Set();

    for (const host of hosts) {
      if (seen.has(host[field])) {
        throw new HostsError(what === 'token'
          ? `Two installations of AGENT_HOSTS have the same token: the token is what tells them apart`
          : `Two installations of AGENT_HOSTS are called ${host.name}`);
      }

      seen.add(host[field]);
    }
  }

  return { hosts, shared: true, ignored: SINGLE.filter(name => (env[name] ?? '') !== '') };
}

/**
 * One installation of AGENT_HOSTS, checked.
 *
 * @typedef {{ name: string, token: string, mcp: string, origin: string }} Host
 */
function hostOf(entry, at) {
  const where = `installation ${at + 1} of AGENT_HOSTS`;

  if (!entry || typeof entry !== 'object' || Array.isArray(entry)) {
    throw new HostsError(`${where} is not an object`);
  }

  const unknown = Object.keys(entry).filter(field => !FIELDS.has(field));

  if (unknown.length > 0) {
    throw new HostsError(`${where} has ${unknown.join(', ')}, which is not one of name, token, mcp and origin`);
  }

  const name = typeof entry.name === 'string' ? entry.name.trim() : '';

  if (!NAME.test(name)) {
    throw new HostsError(`${where} needs a name of lower case letters, digits and dashes, at most 32`);
  }

  const token = typeof entry.token === 'string' ? entry.token.trim() : '';

  if (token.length < SHORTEST_TOKEN) {
    throw new HostsError(`${name} in AGENT_HOSTS needs a token of at least ${SHORTEST_TOKEN} characters - its LAMBDA_AGENT_TOKEN`);
  }

  return {
    name,
    token,
    mcp: addressOf(entry.mcp, `${name} in AGENT_HOSTS needs mcp, the address its builds reach its MCP endpoint at`),
    // the address people open its pages at, which the editor link of a build
    // is made of - not the one its builds reach it at, which may be a name
    // only the build network knows
    origin: new URL(addressOf(entry.origin, `${name} in AGENT_HOSTS needs origin, the address people open it at`)).origin
  };
}

function addressOf(value, missing) {
  if (typeof value !== 'string' || value.trim() === '') throw new HostsError(missing);

  let url;

  try {
    url = new URL(value.trim());
  } catch {
    throw new HostsError(`${missing}: ${value} is not an address`);
  }

  if (url.protocol !== 'http:' && url.protocol !== 'https:') {
    throw new HostsError(`${missing}: ${value} is neither http nor https`);
  }

  return url.toString().replace(/\/+$/, '');
}

/**
 * The installation a request comes from, by the token it carries - or
 * nothing, where it carries none that is known.
 *
 * The single installation of a runner without AGENT_TOKEN takes every
 * request, as a runner always did: the network it listens on is the lock.
 * Every other comparison takes the same time whichever token is near, so the
 * answers say nothing about how much of one was right.
 *
 * @param {Host[]} hosts
 * @param {string | string[] | undefined} token the X-Agent-Token header
 */
export function hostFor(hosts, token) {
  if (hosts.length === 1 && hosts[0].token === '') return hosts[0];

  const given = digest(Array.isArray(token) ? token[0] : token ?? '');

  let found = null;

  for (const host of hosts) {
    if (timingSafeEqual(given, digest(host.token)) && host.token !== '') found ??= host;
  }

  return found;
}

const digest = text => createHash('sha256').update(String(text), 'utf8').digest();

/**
 * The names a build reaches without the egress proxy, for its NO_PROXY.
 *
 * Where several installations share a runner, their MCP endpoints are on the
 * build network by definition - a build has no other way to reach them - so
 * their names go in, and an installation added to AGENT_HOSTS needs nothing
 * else. The single installation of a runner keeps to NO_PROXY as it is
 * given: its MCP may be somewhere the proxy is allowed to tunnel to.
 *
 * @param {Host[]} hosts
 * @param {boolean} shared whether the installations came from AGENT_HOSTS
 * @param {string[]} more NO_PROXY as given, and whatever else is reached directly
 */
export function directOf(hosts, shared, more) {
  const names = [...more.flatMap(entry => String(entry ?? '').split(','))];

  if (shared) names.push(...hosts.map(host => new URL(host.mcp).hostname));

  return [...new Set(names.map(name => name.trim()).filter(Boolean))].join(',');
}
