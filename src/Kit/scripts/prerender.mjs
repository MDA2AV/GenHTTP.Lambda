import { readFileSync, rmSync, writeFileSync } from 'node:fs';
import path from 'node:path';
import { pathToFileURL } from 'node:url';

/*
 * Renders the public pages of a site to markup, after its bundle has been
 * built: `vite build --ssr` bundles the site's src/prerender.tsx for node into
 * .prerender, and this runs it and puts the result next to the index page,
 * where the server reads it (Web/SitePrerender.cs).
 *
 * Run from the site's frontend, with the folder its bundle was built into:
 *   node ../../Kit/scripts/prerender.mjs ../bundle
 */

const site = process.cwd();

const output = path.resolve(site, process.argv[2] ?? '../bundle');

const pages = JSON.parse(readFileSync(path.join(site, 'src', 'pages.json'), 'utf-8'));

const bundle = pathToFileURL(path.join(site, '.prerender', 'prerender.js'));

const { prerender } = await import(bundle.href);

// not the templates for pages the build cannot know (/source/:key), and not the
// pages whose content is all fetched, which are drawn by the browser (/source)
const rendered = Object.entries(pages)
  .filter(([path, page]) => !path.includes(':') && page.prerender !== false)
  .map(([path]) => path);

const result = await prerender(rendered);

for (const [path, markup] of Object.entries(result.pages)) {
  if (markup.length === 0) {
    throw new Error(`The page at ${path} rendered as nothing`);
  }
}

// the showcase is found by the entry the server replaces, so it has to be there
for (const [language, shapes] of Object.entries(result.showcase)) {
  for (const [shape, markup] of Object.entries(shapes)) {
    if (shape !== 'empty' && !markup.includes(result.entry[language])) {
      throw new Error(`The showcase (${language}, ${shape}) does not contain its entry as it was rendered on its own`);
    }
  }
}

writeFileSync(path.join(output, 'prerender.json'), JSON.stringify(result));

rmSync(path.join(site, '.prerender'), { recursive: true, force: true });

console.log(
  `prerendered ${Object.keys(result.pages).length} pages and the showcase in ${Object.keys(result.showcase).length} languages`,
);
