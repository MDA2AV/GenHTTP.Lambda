#!/usr/bin/env node
/*
 * A load generator for the arena, to be run from somewhere that is not the
 * server.
 *
 * Every connection here is a whole client as far as the server is concerned:
 * its own websocket, its own handshake, its own player on the books, its own
 * frame built and serialised and written to it twenty times a second. That is
 * the half of the server the in-process bots do not touch at all - they are
 * objects in a dictionary and never go near a socket.
 *
 * No dependencies. Node 22 has WebSocket built in.
 *
 *   node arena-load.mjs --count 1000 --ramp 50 --seconds 120
 *   node arena-load.mjs --url ws://10.0.0.5:8080/lambda/mine/play --count 250
 *
 * Run it on several machines at once and add the numbers up; each one reports
 * only what it saw itself.
 *
 * A thousand sockets needs a thousand file descriptors. If connections start
 * failing at a round number near 1024, that is the limit and not the server:
 *
 *   ulimit -n 65535
 */

const arg = (name, fallback) => {
  const i = process.argv.indexOf(`--${name}`);
  return i > 0 ? process.argv[i + 1] : fallback;
};

if (process.argv.includes('--help')) {
  console.log(`
  arena-load  - real websockets against an arena

    --url      where to connect          (default wss://genhttp.dev/lambda/example-arena/play)
    --count    how many sockets          (default 200)
    --ramp     new sockets a second      (default 25)
    --seconds  how long to hold them     (default 60)
    --hold     press space on all of them (default off)
    --silent   only the summary
`);
  process.exit(0);
}

const URL = arg('url', 'wss://genhttp.dev/lambda/example-arena/play');
const COUNT = +arg('count', 200);
const RAMP = Math.max(1, +arg('ramp', 25));
const SECONDS = +arg('seconds', 60);
const HOLD = process.argv.includes('--hold');
const SILENT = process.argv.includes('--silent');

const wait = ms => new Promise(r => setTimeout(r, ms));

const live = new Set();
const seenBy = new Map();

let opened = 0, refused = 0, closed = 0, errored = 0;
let frames = 0, bytes = 0, welcomed = 0;
const handshakes = [];
const gaps = [];

function join(n) {
  const began = Date.now();
  let ws;

  try {
    ws = new WebSocket(URL);
  } catch (e) {
    refused++;
    return;
  }

  ws.binaryType = 'arraybuffer';

  const seen = { frames: 0, bytes: 0, world: 0, last: 0 };

  seenBy.set(ws, seen);

  ws.addEventListener('open', () => {
    opened++;
    live.add(ws);
    handshakes.push(Date.now() - began);
    ws.send(JSON.stringify({ kind: 'join', name: `Load${n}` }));
  });

  ws.addEventListener('message', (e) => {
    const size = typeof e.data === 'string' ? e.data.length : e.data.byteLength;

    frames++;
    seen.frames++;
    bytes += size;
    seen.bytes += size;

    const now = Date.now();

    // one socket in fifty times the gap between its frames, which is what
    // the server's real tick rate looks like from the outside
    if (n % 50 === 0 && seen.last) gaps.push(now - seen.last);
    seen.last = now;

    /*
     * Only the welcome is parsed. Parsing twenty frames a second on a
     * thousand sockets measures this program rather than the server, and the
     * whole point of running it elsewhere is to stop doing that.
     */
    if (!seen.world && typeof e.data === 'string' && e.data.startsWith('{"kind":"welcome"')) {
      welcomed++;
      try { seen.world = JSON.parse(e.data).world || 9000; } catch { seen.world = 9000; }
    }
  });

  ws.addEventListener('error', () => { errored++; });
  ws.addEventListener('close', () => { closed++; live.delete(ws); seenBy.delete(ws); });
}

// everybody steers at the rate a browser does, so the server is doing the
// work it would really be doing rather than idling on quiet sockets
let beat = 0;

const steering = setInterval(() => {
  beat++;

  for (const ws of live) {
    if (ws.readyState !== 1) continue;

    const seen = seenBy.get(ws);
    const w = seen?.world || 9000;
    const a = beat / 20 + (seen?.frames ?? 0) * 0.001;

    ws.send(`{"kind":"steer","x":${(w / 2 + Math.cos(a) * w * 0.3).toFixed(0)},"y":${(w / 2 + Math.sin(a) * w * 0.3).toFixed(0)},"hold":${HOLD}}`);
  }
}, 50);

const started = Date.now();
let wasFrames = 0, wasBytes = 0, wasAt = Date.now();

const ticker = setInterval(() => {
  const now = Date.now();
  const dt = (now - wasAt) / 1000;
  const fps = (frames - wasFrames) / dt;
  const mbs = (bytes - wasBytes) / dt / 1048576;

  wasFrames = frames; wasBytes = bytes; wasAt = now;

  if (!SILENT) {
    console.log(`  ${String(Math.round((now - started) / 1000)).padStart(4)}s`
      + `  live ${String(live.size).padStart(5)}`
      + `  joined ${String(welcomed).padStart(5)}`
      + `  frames/s ${String(Math.round(fps)).padStart(7)}`
      + `  ${mbs.toFixed(1).padStart(6)} MB/s`
      + `  each ${(fps / Math.max(live.size, 1)).toFixed(1).padStart(5)}/s`
      + `  closed ${closed}  errors ${errored}`);
  }
}, 3000);

const middle = xs => xs.length ? [...xs].sort((a, b) => a - b)[xs.length >> 1] : 0;

(async () => {
  for (let i = 0; i < COUNT; i++) {
    join(i);

    if ((i + 1) % RAMP === 0) await wait(1000);
  }

  await wait(SECONDS * 1000);

  clearInterval(steering);
  clearInterval(ticker);

  const span = (Date.now() - started) / 1000;
  const each = [...seenBy.values()].map(s => s.frames / span);

  console.log(`\n  --- ${COUNT} asked for, ${span.toFixed(0)}s, ${URL} ---`);
  console.log(`  connected      ${opened}, still live ${live.size}, closed early ${closed}, errors ${errored}, refused ${refused}`);
  console.log(`  welcomed       ${welcomed}`);
  console.log(`  handshake      median ${middle(handshakes)} ms, worst ${Math.max(0, ...handshakes)} ms`);
  console.log(`  frames         ${frames.toLocaleString()}, ${(frames / span).toFixed(0)} a second across all of them`);
  console.log(`  a socket got   ${middle(each).toFixed(1)} frames a second (median), worst ${Math.min(99, ...each).toFixed(1)}`);
  console.log(`  frame gaps     median ${middle(gaps)} ms, worst ${Math.max(0, ...gaps)} ms`);
  console.log(`  traffic        ${(bytes / span / 1048576).toFixed(2)} MB/s leaving the server`);

  for (const ws of live) ws.close();
  await wait(500);
  process.exit(0);
})();
