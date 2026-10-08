import React from 'react';
import { renderToString } from 'react-dom/server';
import { StaticRouter } from 'react-router-dom/server';

import { App } from './App';
import type { ShowcaseEntry, ShowcaseListing } from './api';
import { loadEverything } from './i18n';
import { LANGUAGES, inLanguage } from './i18n/languages';
import { Entry } from './pages/Showcase';
import { PLACEHOLDERS, prerenderWith } from './site';

/*
 * The public pages as markup, rendered once when the frontend is built (see
 * prerender.mjs) and put into the index page by the server - so a crawler
 * that runs no script reads the page it asked for, and a visitor sees it
 * before the script has arrived. Each page once for every language, under the
 * address it has in that language.
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
  address: '__LAMBDA_ENTRY_ADDRESS__',
  imagePath: '__LAMBDA_ENTRY_IMAGE__',
  imageType: 'image/png',
  imageBytes: 0,
  online: true,
  created: '',
  updated: '',
} satisfies ShowcaseEntry;

/** Written in by the server; the words around it are the language's own. */
const TOTAL = '__LAMBDA_SHOWCASE_TOTAL__' as unknown as number;

/**
 * A page of the showcase, in each shape it can take. A single lambda is a
 * shape of its own, because it is counted in other words than several.
 */
const SHOWCASE: Record<string, ShowcaseListing> = {
  // nothing on show: nothing to fill in either
  empty: { entries: [], total: 0, next: null },
  // one lambda, and nothing else
  one: { entries: [ENTRY], total: 1, next: null },
  // everything fits on the first page
  complete: { entries: [ENTRY], total: TOTAL, next: null },
  // there is more to load
  partial: { entries: [ENTRY], total: TOTAL, next: 1 },
};

function render(path: string, content: React.ReactNode = <App />): string {
  return renderToString(
    <React.StrictMode>
      <StaticRouter location={path}>{content}</StaticRouter>
    </React.StrictMode>,
  );
}

export interface Prerendered {
  /** Every public page in every language, by the path it has in that language. */
  pages: Record<string, string>;
  /** The showcase by language, and in there by the shape of its first page. */
  showcase: Record<string, Record<string, string>>;
  /** One entry of the showcase by language, as it is found in the pages above. */
  entry: Record<string, string>;
}

/** Renders the pages at the given paths, which have no language yet. */
export async function prerender(paths: string[]): Promise<Prerendered> {
  // every word of every language is there before anything is drawn - a page
  // rendered while its words are on their way would be rendered empty
  await loadEverything();

  const pages: Record<string, string> = {};
  const showcase: Record<string, Record<string, string>> = {};
  const entry: Record<string, string> = {};

  for (const language of LANGUAGES) {
    for (const path of paths) {
      const localized = inLanguage(language, path);

      prerenderWith({ ...PLACEHOLDERS });
      pages[localized] = render(localized);
    }

    const address = inLanguage(language, '/showcase');

    showcase[language] = {};

    for (const [shape, listing] of Object.entries(SHOWCASE)) {
      prerenderWith({ ...PLACEHOLDERS, showcase: listing });
      showcase[language][shape] = render(address);
    }

    entry[language] = render(address, <Entry entry={ENTRY} index={0} />);
  }

  return { pages, showcase, entry };
}
