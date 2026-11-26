/*
 * The rules of the page fetch, checked without the internet: which sites a
 * message names, which addresses may be read, and what a page comes back as.
 * CI runs it with node --test 'docker/agent/*.test.mjs'.
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';

import { sitesIn, belongs, sitesFrom } from './sites.mjs';
import { serveFetch, forTests } from './fetch.mjs';

const { refusalOf, readable, isPublic, vetted } = forTests;

test('the sites a message names are found, whatever is written around them', () => {
  assert.deepEqual(sitesIn(['Look at myoldhomepage.com and create a new version for it']), ['myoldhomepage.com']);
  assert.deepEqual(sitesIn(['See https://www.Example.org/about?x=1.']), ['www.example.org']);
  assert.deepEqual(sitesIn(['(club.de), "shop.example.co.uk/path" and http://foo.lambda:8080/x']), ['club.de', 'shop.example.co.uk', 'foo.lambda']);
  assert.deepEqual(sitesIn(['Die Seite der Müller-Bäckerei.de, bitte']), ['xn--mller-bckerei-hfb76a.de']);
  assert.deepEqual(sitesIn(['first a.com', 'then b.org and a.com']), ['a.com', 'b.org']);
});

test('files, abbreviations and mail addresses are not sites', () => {
  assert.deepEqual(sitesIn(['Put it in index.html, app.js and styles.css, like Node.js does']), []);
  assert.deepEqual(sitesIn(['e.g. this, z.B. das, i.e. U.S.A.']), []);
  assert.deepEqual(sitesIn(['write to info@club.de or a@b.com']), []);
  assert.deepEqual(sitesIn(['a GenHTTP.Lambda app']), []);
  assert.deepEqual(sitesIn(['999.1.1.1 is no address']), []);
});

test('no more than ten sites are taken from a message', () => {
  const many = Array.from({ length: 15 }, (_, i) => `site${i}.com`).join(' ');
  assert.equal(sitesIn([many]).length, 10);
});

test('a host belongs to a site with or without www, and nowhere else', () => {
  assert.ok(belongs('www.example.com', ['example.com']));
  assert.ok(belongs('example.com', ['www.example.com']));
  assert.ok(belongs('EXAMPLE.com.', ['example.com']));
  assert.ok(!belongs('blog.example.com', ['example.com']), 'a name below it is somebody else\'s as often as not');
  assert.ok(!belongs('evil.github.io', ['github.io']));
  assert.ok(!belongs('evilexample.com', ['example.com']));
  assert.ok(!belongs('example.com.evil.net', ['example.com']));
  assert.ok(belongs('93.184.216.34', ['93.184.216.34']));
  assert.ok(!belongs('1.93.184.216.34', ['93.184.216.34']));
});

test('the header takes host names only', () => {
  assert.deepEqual(sitesFrom('a.com, B.org,localhost,[::1],x y.com,'), ['a.com', 'b.org']);
});

test('only public addresses are public', () => {
  for (const [address, family] of [['8.8.8.8', 4], ['2606:4700::1111', 6], ['93.184.216.34', 4], ['::ffff:8.8.8.8', 6]]) {
    assert.ok(isPublic(address, family), address);
  }

  for (const [address, family] of [
    ['127.0.0.1', 4], ['10.1.2.3', 4], ['172.17.0.2', 4], ['192.168.1.1', 4], ['169.254.169.254', 4],
    ['100.64.0.1', 4], ['0.0.0.0', 4], ['224.0.0.1', 4], ['::1', 6], ['::', 6], ['fd00:c0de:1a::2', 6],
    ['fe80::1', 6], ['::ffff:10.0.0.1', 6], ['::ffff:127.0.0.1', 6], ['2002:a00:1::', 6], ['64:ff9b::a00:1', 6]
  ]) {
    assert.ok(!isPublic(address, family), address);
  }
});

test('an address is read only where it is a named site on a standard port', () => {
  const sites = ['example.com', '127.0.0.1'];
  const refused = url => refusalOf(new URL(url), sites);

  assert.equal(refused('https://example.com/'), null);
  assert.equal(refused('http://www.example.com/old/index.html'), null);
  assert.equal(refused('https://example.com:443/'), null);

  assert.match(refused('https://example.com:8080/'), /standard ports/);
  assert.match(refused('ftp://example.com/'), /http and https/);
  assert.match(refused('https://user:secret@example.com/'), /user name or password/);
  assert.match(refused('https://evil.example/?key=abc'), /not one of the sites/);
  assert.match(refused('http://127.0.0.1/'), /not a public address/);
  assert.match(refused('http://[::1]/'), /not one of the sites/);
  assert.match(refusalOf(new URL('https://example.com/'), []), /names no site/);
});

test('a name that resolves into the machine is refused as it is connected to', async () => {
  const error = await new Promise(resolve => vetted('localhost', { all: true }, e => resolve(e)));
  assert.match(String(error?.message), /not at a public address/);
});

test('a page comes back as text with its links and pictures', () => {
  const page = `<!doctype html><html lang="de"><head><title>Our  club</title>
    <meta name="description" content="Tennis since 1920">
    <link rel="stylesheet" href="/css/site.css"><style>body{color:#123456}</style>
    <script>alert("no")</script></head>
    <body><h1>Welcome &amp; hello</h1><p>Courts&nbsp;open <b>daily</b>.<br>Call <a href="tel:+491234">us</a></p>
    <ul><li><a href="/members">Members</a></li><li><a href="javascript:void(0)">Nothing</a></li></ul>
    <img src="img/logo.png" alt="Logo"><script>document.write("hidden")</script>
    <p>K&ouml;ln &#8211; &#x2764;</p></body></html>`;

  const text = readable(page, 'https://club.example/start/');

  assert.match(text, /^Title: Our club$/m);
  assert.match(text, /^Language: de$/m);
  assert.match(text, /^description: Tennis since 1920$/m);
  assert.match(text, /^Stylesheets: https:\/\/club\.example\/css\/site\.css$/m);
  assert.match(text, /^Inline styles: /m);
  assert.match(text, /^# Welcome & hello$/m);
  assert.match(text, /Courts open daily\.\nCall \[us\]\(tel:\+491234\)/);
  assert.match(text, /^- \[Members\]\(https:\/\/club\.example\/members\)$/m);
  assert.match(text, /^- Nothing$/m);
  assert.match(text, /!\[Logo\]\(https:\/\/club\.example\/start\/img\/logo\.png\)/);
  assert.match(text, /Köln – ❤/);
  assert.doesNotMatch(text, /alert|hidden|color:#123456/);
});

test('the server speaks MCP and refuses what is not a named public site', async () => {
  const server = serveFetch(0);
  await new Promise(resolve => server.once('listening', resolve));

  const { port } = server.address();

  const call = async (body, sites = '127.0.0.1') => {
    const res = await fetch(`http://127.0.0.1:${port}/mcp`, {
      method: 'POST',
      headers: { 'content-type': 'application/json', accept: 'application/json, text/event-stream', 'x-fetch-sites': sites },
      body: JSON.stringify(body)
    });

    return { status: res.status, body: res.status === 202 ? null : await res.json() };
  };

  try {
    const init = await call({ jsonrpc: '2.0', id: 1, method: 'initialize', params: { protocolVersion: '2025-06-18', capabilities: {}, clientInfo: { name: 't', version: '1' } } });
    assert.equal(init.body.result.protocolVersion, '2025-06-18');
    assert.deepEqual(init.body.result.capabilities, { tools: {} });

    assert.equal((await call({ jsonrpc: '2.0', method: 'notifications/initialized' })).status, 202);

    const list = await call({ jsonrpc: '2.0', id: 2, method: 'tools/list' });
    assert.deepEqual(list.body.result.tools.map(t => t.name), ['fetch', 'hand_over']);
    assert.equal(list.body.result.tools[0].annotations.readOnlyHint, true);

    // a build that may read no site has nothing to fetch with, but may hand a file over
    const none = await call({ jsonrpc: '2.0', id: 9, method: 'tools/list' }, '');
    assert.deepEqual(none.body.result.tools.map(t => t.name), ['hand_over']);

    const handed = await call({ jsonrpc: '2.0', id: 10, method: 'tools/call', params: { name: 'hand_over', arguments: { name: 'notes.md', mediaType: 'text/markdown', content: '# Hi' } } });
    assert.match(handed.body.result.content[0].text, /Handed notes.md over/, 'kept nowhere here: the runner reads it off the call');

    // where the runner reads any public site, any name is one - and its address is checked all the same
    const any = await call({ jsonrpc: '2.0', id: 11, method: 'tools/call', params: { name: 'fetch', arguments: { url: 'http://127.0.0.1/' } } }, '*');
    assert.match(any.body.result.content[0].text, /not a public address/);
    assert.match((await call({ jsonrpc: '2.0', id: 12, method: 'tools/list' }, '*')).body.result.tools[0].description, /any public site/);

    const inside = await call({ jsonrpc: '2.0', id: 3, method: 'tools/call', params: { name: 'fetch', arguments: { url: `http://127.0.0.1:${port}/mcp` } } });
    assert.equal(inside.body.result.isError, true);
    assert.match(inside.body.result.content[0].text, /standard ports/);

    const local = await call({ jsonrpc: '2.0', id: 4, method: 'tools/call', params: { name: 'fetch', arguments: { url: 'http://127.0.0.1/' } } });
    assert.equal(local.body.result.isError, true);
    assert.match(local.body.result.content[0].text, /not a public address/);

    const elsewhere = await call({ jsonrpc: '2.0', id: 5, method: 'tools/call', params: { name: 'fetch', arguments: { url: 'https://evil.example/?k=1' } } });
    assert.match(elsewhere.body.result.content[0].text, /not one of the sites/);

    const browser = await fetch(`http://127.0.0.1:${port}/mcp`, { method: 'POST', headers: { origin: 'https://evil.example' }, body: '{}' });
    assert.equal(browser.status, 403);

    assert.equal((await fetch(`http://127.0.0.1:${port}/mcp`)).status, 405);
  } finally {
    server.close();
  }
});
