import React from 'react';
import { renderToString } from 'react-dom/server';
import { StaticRouter } from 'react-router-dom/server';

import { App } from './App';
import type { ShowcaseEntry, ShowcaseListing } from './api';
import { Entry } from './pages/Showcase';
import { PLACEHOLDERS, prerenderWith } from './site';

/*
 * The public pages as markup, rendered once when the frontend is built (see
 * prerender.mjs) and put into the index page by the server - so a crawler
 * that runs no script reads the page it asked for, and a visitor sees it
 * before the script has arrived.
 *
 * What only the server knows is rendered as a placeholder (see site.ts). The
 * showcase is the one page whose content changes by the minute, so it is
 * rendered with a single entry made of placeholders, which the server fills
 * in once for each entry it has - and in one variant for each shape the page
 * can take, since those are more than a value in a sentence.
 */

/** A showcase entry made of placeholders, which the server fills in. */
export const ENTRY = {
  publicKey: '__LAMBDA_ENTRY_KEY__',
  title: '__LAMBDA_ENTRY_TITLE__',
  description: '__LAMBDA_ENTRY_DESCRIPTION__',
  path: '__LAMBDA_ENTRY_PATH__',
  imagePath: '__LAMBDA_ENTRY_IMAGE__',
  imageType: 'image/png',
  imageBytes: 0,
  online: true,
  created: '',
  updated: '',
} satisfies ShowcaseEntry;

/** Written by the server as "12 lambdas", or "1 lambda". */
const TOTAL = '__LAMBDA_SHOWCASE_TOTAL__' as unknown as number;

/** A page of the showcase, in each shape it can take. */
const SHOWCASE: Record<string, ShowcaseListing> = {
  // nothing on show: nothing to fill in either
  empty: { entries: [], total: 0, next: null },
  // everything fits on the first page
  complete: { entries: [ENTRY], total: TOTAL, next: null },
  // there is more to load
  partial: { entries: [ENTRY], total: TOTAL, next: 1 },
};

function render(path: string): string {
  return renderToString(
    <React.StrictMode>
      <StaticRouter location={path}>
        <App />
      </StaticRouter>
    </React.StrictMode>,
  );
}

export interface Prerendered {
  /** Every public page, by path. */
  pages: Record<string, string>;
  /** The showcase, by the shape of its first page. */
  showcase: Record<string, string>;
  /** One entry of the showcase, as it is found in the pages above. */
  entry: string;
}

export function prerender(paths: string[]): Prerendered {
  const pages: Record<string, string> = {};

  for (const path of paths) {
    prerenderWith({ ...PLACEHOLDERS });
    pages[path] = render(path);
  }

  const showcase: Record<string, string> = {};

  for (const [shape, listing] of Object.entries(SHOWCASE)) {
    prerenderWith({ ...PLACEHOLDERS, showcase: listing });
    showcase[shape] = render('/showcase');
  }

  const entry = renderToString(<Entry entry={ENTRY} index={0} />);

  return { pages, showcase, entry };
}
