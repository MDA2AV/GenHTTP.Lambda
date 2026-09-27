/*
 * Writes the icons and the link previews into public/, from logo.svg and
 * cards.html next to this file. Run it after changing either:
 *
 *   node brand/render.mjs
 *
 * It drives a headless Chrome over its debugging protocol, so nothing needs
 * to be installed beyond Chrome itself - set CHROME if it is somewhere other
 * than the usual places. The results are committed, so a build never needs it.
 */
import { spawn } from 'node:child_process';
import { existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const out = join(here, '..', 'public');

const CHROMES = [
  process.env.CHROME,
  'C:/Program Files/Google/Chrome/Application/chrome.exe',
  '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',
  '/usr/bin/google-chrome',
  '/usr/bin/chromium',
];

const chromePath = CHROMES.find((candidate) => candidate && existsSync(candidate));

if (!chromePath) {
  throw new Error('Chrome was not found; set CHROME to its executable');
}

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

const profile = mkdtempSync(join(tmpdir(), 'brand-'));
const port = 9400 + Math.floor(Math.random() * 400);

const chrome = spawn(chromePath, [
  '--headless=new', `--remote-debugging-port=${port}`, `--user-data-dir=${profile}`,
  '--hide-scrollbars', '--no-first-run', '--allow-file-access-from-files', 'about:blank',
]);

try {
  const page = await connect();

  mkdirSync(join(out, 'social'), { recursive: true });

  // ------------------------------------------------------------------ icons

  const logo = readFileSync(join(here, 'logo.svg'), 'utf-8');
  const mark = logo.replace(/<rect[^>]*\/>/, '');

  writeFileSync(join(out, 'favicon.svg'), logo);

  const icon = async (size, svg, padding = 0) => {
    const inner = size - padding * 2;
    const html = `<html><body style="margin:0;background:#aa55ff;width:${size}px;height:${size}px">
      <div style="padding:${padding}px">${svg.replace('<svg ', `<svg width="${inner}" height="${inner}" `)}</div></body></html>`;

    return page.shoot(`data:text/html;base64,${Buffer.from(html).toString('base64')}`, size, size);
  };

  writeFileSync(join(out, 'apple-touch-icon.png'), await icon(180, logo));
  writeFileSync(join(out, 'icon-192.png'), await icon(192, logo));
  writeFileSync(join(out, 'icon-512.png'), await icon(512, logo));
  // a launcher may cut a maskable icon down to the circle inside it, so the
  // mark keeps to the safe zone in the middle
  writeFileSync(join(out, 'icon-maskable-512.png'), await icon(512, mark, 64));

  // the old format still asked for at /favicon.ico, holding PNGs as it may
  writeFileSync(join(out, 'favicon.ico'), ico(await Promise.all([16, 32, 48].map((size) => icon(size, logo)))));

  // ------------------------------------------------------------- previews

  const cards = pathToFileURL(join(here, 'cards.html')).href;

  await page.shoot(`${cards}?page=/`, 1200, 630);

  const paths = await page.evaluate('window.CARDS');

  for (const path of paths) {
    const name = path === '/' ? 'default' : path.slice(1);
    writeFileSync(join(out, 'social', `${name}.png`), await page.shoot(`${cards}?page=${path}`, 1200, 630));
    console.log(`social/${name}.png`);
  }

  console.log('icons written');
} finally {
  chrome.kill();
  await sleep(300);
  rmSync(profile, { recursive: true, force: true });
}

/** A tab to render into, with the two things done to it here. */
async function connect() {
  let targets;

  for (let attempt = 0; attempt < 50 && !targets; attempt++) {
    try {
      targets = await (await fetch(`http://127.0.0.1:${port}/json`)).json();
    } catch {
      await sleep(200);
    }
  }

  const socket = new WebSocket(targets.find((target) => target.type === 'page').webSocketDebuggerUrl);
  await new Promise((resolve) => socket.addEventListener('open', resolve));

  let id = 0;
  const pending = new Map();

  socket.addEventListener('message', (event) => {
    const message = JSON.parse(event.data);
    pending.get(message.id)?.(message);
    pending.delete(message.id);
  });

  const send = (method, params = {}) =>
    new Promise((resolve) => {
      const next = ++id;
      pending.set(next, resolve);
      socket.send(JSON.stringify({ id: next, method, params }));
    });

  await send('Page.enable');

  const evaluate = async (expression) =>
    (await send('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true })).result?.result?.value;

  return {
    evaluate,
    async shoot(url, width, height) {
      await send('Emulation.setDeviceMetricsOverride', { width, height, deviceScaleFactor: 1, mobile: false });
      await send('Page.navigate', { url });
      await sleep(400);
      await evaluate('document.fonts.ready.then(() => true)');

      const shot = await send('Page.captureScreenshot', { format: 'png', clip: { x: 0, y: 0, width, height, scale: 1 } });
      return Buffer.from(shot.result.data, 'base64');
    },
  };
}

/** An ICO file holding the given square PNGs. */
function ico(pngs) {
  const header = Buffer.alloc(6 + pngs.length * 16);
  header.writeUInt16LE(1, 2);
  header.writeUInt16LE(pngs.length, 4);

  let offset = header.length;

  pngs.forEach((png, i) => {
    const size = png.readUInt32BE(16);
    const entry = 6 + i * 16;

    header.writeUInt8(size >= 256 ? 0 : size, entry);
    header.writeUInt8(size >= 256 ? 0 : size, entry + 1);
    header.writeUInt16LE(1, entry + 4);
    header.writeUInt16LE(32, entry + 6);
    header.writeUInt32LE(png.length, entry + 8);
    header.writeUInt32LE(offset, entry + 12);

    offset += png.length;
  });

  return Buffer.concat([header, ...pngs]);
}
