import { readFileSync, rmSync, writeFileSync } from 'node:fs';

/*
 * Renders the public pages to markup, after the application itself has been
 * built: `vite build --ssr` bundles src/prerender.tsx for node into
 * .prerender, and this runs it and puts the result next to the index page,
 * where the server reads it (Web/SitePrerender.cs).
 */

const pages = JSON.parse(readFileSync(new URL('./src/pages.json', import.meta.url), 'utf-8'));

const bundle = new URL('./.prerender/prerender.js', import.meta.url);

const { prerender } = await import(bundle.href);

const result = await prerender(Object.keys(pages));

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

writeFileSync(new URL('../GenHTTP.Lambda/wwwroot/prerender.json', import.meta.url), JSON.stringify(result));

rmSync(new URL('./.prerender', import.meta.url), { recursive: true, force: true });

console.log(
  `prerendered ${Object.keys(result.pages).length} pages and the showcase in ${Object.keys(result.showcase).length} languages`,
);
