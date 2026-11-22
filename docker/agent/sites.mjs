/*
 * Which sites a build may read: the ones the person named.
 *
 * The runner finds them in what was typed - the request, and on /build the
 * earlier messages of the same conversation, never what the agent answered -
 * and hands them to the page fetch with the job. The fetch reads a page only
 * where its host belongs to one of them. So a page that tells the agent to
 * fetch an address with something in it can only send it back to a site the
 * person named themselves.
 *
 * Shared by both, so the two halves of the rule are read in one place.
 */

import { isIP } from 'node:net';

/** At most this many sites to a job. */
export const SITES = 10;

/*
 * The last parts of names people write that are files rather than sites -
 * "index.html", "Node.js" - and are no domain anybody could register. Where a
 * real top-level domain looks like a file (.md, .py, .sh), it stays a site:
 * the worst a wrong one does is name a site nobody asked to be read.
 */
const FILES = new Set(['html', 'htm', 'js', 'mjs', 'cjs', 'ts', 'tsx', 'jsx', 'css', 'scss', 'sass', 'less', 'json',
  'txt', 'cs', 'csproj', 'sln', 'xml', 'yml', 'yaml', 'png', 'jpg', 'jpeg', 'gif', 'svg', 'webp', 'ico', 'bmp', 'pdf',
  'csv', 'sql', 'exe', 'dll', 'php', 'java', 'kt', 'toml', 'lock', 'log', 'ini', 'cfg', 'env', 'vue', 'docx', 'xlsx',
  'pptx', 'mp3', 'mp4', 'wav', 'zip', 'tar', 'gz']);

/*
 * Top-level domains a site is written with when nobody types https:// in
 * front of it. Two letters are a country's; these are the others people
 * commonly use. A name ending in anything else still counts when it is
 * written with its scheme or www.
 */
const COMMON = new Set(['com', 'net', 'org', 'info', 'biz', 'name', 'pro', 'edu', 'gov', 'int', 'app', 'dev', 'xyz',
  'online', 'site', 'website', 'shop', 'store', 'blog', 'page', 'cloud', 'tech', 'art', 'design', 'studio', 'agency',
  'email', 'club', 'life', 'live', 'world', 'today', 'news', 'space', 'fun', 'top', 'icu', 'vip', 'one', 'link',
  'digital', 'media', 'group', 'team', 'family', 'photo', 'photography', 'restaurant', 'cafe', 'bar', 'wine', 'coffee',
  'church', 'school', 'academy', 'travel', 'asia', 'mobi', 'berlin', 'hamburg', 'koeln', 'wien', 'swiss', 'paris',
  'london', 'nyc', 'tokyo', 'amsterdam', 'brussels', 'gmbh', 'ltd', 'inc', 'llc', 'run', 'build', 'works', 'social']);

const NAME = /^(?:[\p{L}\p{N}](?:[\p{L}\p{N}-]{0,61}[\p{L}\p{N}])?\.){1,10}[\p{L}][\p{L}\p{N}-]{0,61}$/u;

const ADDRESS = /^(?:\d{1,3}\.){3}\d{1,3}$/;

/**
 * The sites named in some texts, each as a host name - lower case, an
 * international name in the form DNS knows it - in the order they were
 * named, at most SITES of them.
 */
export function sitesIn(texts) {
  const found = [];

  for (const text of texts) {
    for (const token of String(text ?? '').split(/[\s<>"'()[\]{},;!?|`*]+/u)) {
      if (found.length >= SITES) return found;

      const site = siteOf(token);

      if (site && !found.includes(site)) found.push(site);
    }
  }

  return found;
}

function siteOf(token) {
  const schemed = /^https?:\/\//i.test(token);

  // the host of it: no scheme, path, query, fragment or port, and no full
  // stop or colon from the sentence it ended
  let host = token.replace(/^https?:\/\//i, '').split(/[/?#]/)[0].replace(/[.:]+$/, '').replace(/:\d{1,5}$/, '');

  // somebody@example.com is a mail address, and a site with a user in it is
  // not one to read
  if (!host || host.includes('@') || host.length > 253) return null;

  if (ADDRESS.test(host)) return isIP(host) === 4 ? host : null;

  if (!NAME.test(host)) return null;

  try {
    host = new URL(`http://${host}`).hostname;
  } catch {
    return null;
  }

  const top = host.slice(host.lastIndexOf('.') + 1);
  const written = schemed || host.startsWith('www.');

  if (FILES.has(top) && !written) return null;

  // an international top-level domain is spelt xn--… once it is a host name
  if (written || top.length === 2 || COMMON.has(top) || top.startsWith('xn--')) return host;

  return null;
}

const bare = host => host.replace(/^www\./, '');

/**
 * Whether a host belongs to one of the sites: the site itself, with or
 * without www., and nothing else. Not the names below it: somebody who wrote
 * github.io or blogspot.com would otherwise have named every site there,
 * whoever's - and one of them is where a page could send what it asked for.
 * An address is only ever itself.
 */
export function belongs(host, sites) {
  const name = String(host ?? '').toLowerCase().replace(/^\[|\]$/g, '').replace(/\.$/, '');

  return sites.some(site => isIP(site) ? name === site : bare(name) === bare(site));
}

/** The sites as a header carries them, from the runner. */
export function sitesFrom(header) {
  return String(header ?? '')
    .split(',')
    .map(s => s.trim().toLowerCase())
    .filter(s => /^[a-z0-9.-]{1,253}$/.test(s) && s.includes('.'))
    .slice(0, SITES);
}
