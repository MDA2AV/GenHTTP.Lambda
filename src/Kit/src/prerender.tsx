import React from 'react';
import { renderToString } from 'react-dom/server';
import { StaticRouter } from 'react-router-dom';

import type { ShowcaseEntry, ShowcaseListing } from './api';
import { BRAND_PLACEHOLDERS, PLATFORM, prerenderBrand, type Brand } from './brand';
import { loadEverything } from './i18n';
import { LANGUAGES, inLanguage } from './i18n/languages';
import { Entry } from './pages/Showcase';
import { prerenderProfile, type Profile } from './profile';
import { PLACEHOLDERS, prerenderWith } from './site';

/*
 * The public pages of a site as markup, rendered once when its frontend is
 * built (see scripts/prerender.mjs) and put into the index page by the server - so a crawler
 * that runs no script reads the page it asked for, and a visitor sees it
 * before the script has arrived. Each page once for every language, under the
 * address it has in that language.
 *
 * What only the server knows is rendered as a placeholder (see site.ts). The
 * showcase is the one page whose content changes by the minute, so it is
 * rendered with a single entry made of placeholders, which the server fills
 * in once for each entry it has - and in one variant for each shape the page
 * can take, since those are more than a value in a sentence.
 *
 * A site whose installation may be branded has all of it rendered once more
 * with a brand made of placeholders (see brand.ts): its operator's name and
 * picture are other markup than the platform's mark, so the server takes
 * this rendering for a branded installation and fills them in.
 *
 * And a site whose installation may be for one profile or another has all of
 * it rendered in the words of each profile it names (see profile.ts), as the
 * platform and as a brand - keeping only the pages that differ from those
 * rendered in the words every profile shares, since most say nothing of
 * their own. The server takes the rendering of the installation's profile,
 * and tells the page which it was.
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

function render(path: string, content: React.ReactNode): string {
  return renderToString(
    <React.StrictMode>
      <StaticRouter location={path}>{content}</StaticRouter>
    </React.StrictMode>,
  );
}

/** The public pages of a site as one brand shows them. */
export interface PrerenderedPages {
  /** Every public page in every language, by the path it has in that language. */
  pages: Record<string, string>;
  /** The showcase by language, and in there by the shape of its first page. */
  showcase: Record<string, Record<string, string>>;
}

export interface Prerendered extends PrerenderedPages {
  /** One entry of the showcase by language, as it is found in the pages above. */
  entry: Record<string, string>;
  /** The same pages as a branded installation shows them, where the site may be branded. */
  branded?: PrerenderedPages;
  /**
   * The pages that differ in the words of each profile the site names, as the
   * platform and as a brand - empty where a profile differs nowhere.
   */
  profiles?: Partial<Record<Profile, PrerenderedPages & { branded?: PrerenderedPages }>>;
}

export interface PrerenderOptions {
  /** Whether the installation of the site may be branded, so its pages are rendered for a brand as well. */
  branded?: boolean;
  /** The profiles the installation of the site may have, so its pages are rendered in the words of each as well. */
  profiles?: Profile[];
}

/**
 * What renders the pages of a site, at the paths it is given - which have no
 * language yet. A site's src/prerender.tsx exports it as <c>prerender</c>,
 * for its application.
 */
export const prerendering = (app: () => React.ReactNode, options: PrerenderOptions = {}) => async function prerender(paths: string[]): Promise<Prerendered> {
  // every word of every language is there before anything is drawn - a page
  // rendered while its words are on their way would be rendered empty
  await loadEverything();

  const entry: Record<string, string> = {};

  for (const language of LANGUAGES) {
    entry[language] = render(inLanguage(language, '/showcase'), <Entry entry={ENTRY} index={0} />);
  }

  // in the words every profile shares, then in each profile's own
  prerenderProfile(null);

  const plain = renderAll(app, paths, PLATFORM);
  const branded = options.branded ? renderAll(app, paths, BRAND_PLACEHOLDERS) : undefined;

  const result: Prerendered = branded ? { ...plain, entry, branded } : { ...plain, entry };

  if (options.profiles?.length) {
    result.profiles = {};

    for (const profile of options.profiles) {
      prerenderProfile(profile);

      const own = differing(renderAll(app, paths, PLATFORM), plain);

      result.profiles[profile] = branded ? { ...own, branded: differing(renderAll(app, paths, BRAND_PLACEHOLDERS), branded) } : own;
    }

    prerenderProfile(null);
  }

  return result;
};

/** What a rendering of the pages has that differs from another: the pages, and the shapes of the showcase. */
function differing(pages: PrerenderedPages, shared: PrerenderedPages): PrerenderedPages {
  const own: PrerenderedPages = { pages: {}, showcase: {} };

  for (const [path, page] of Object.entries(pages.pages)) {
    if (shared.pages[path] !== page) {
      own.pages[path] = page;
    }
  }

  for (const [language, shapes] of Object.entries(pages.showcase)) {
    for (const [shape, page] of Object.entries(shapes)) {
      if (shared.showcase[language]?.[shape] !== page) {
        (own.showcase[language] ??= {})[shape] = page;
      }
    }
  }

  return own;
}

/** Every page and every shape of the showcase in every language, as the given brand shows them. */
function renderAll(app: () => React.ReactNode, paths: string[], brand: Brand): PrerenderedPages {
  const pages: Record<string, string> = {};
  const showcase: Record<string, Record<string, string>> = {};

  prerenderBrand(brand);

  for (const language of LANGUAGES) {
    for (const path of paths) {
      const localized = inLanguage(language, path);

      prerenderWith({ ...PLACEHOLDERS });
      pages[localized] = render(localized, app());
    }

    const address = inLanguage(language, '/showcase');

    showcase[language] = {};

    for (const [shape, listing] of Object.entries(SHOWCASE)) {
      prerenderWith({ ...PLACEHOLDERS, showcase: listing });
      showcase[language][shape] = render(address, app());
    }
  }

  prerenderBrand(PLATFORM);

  return { pages, showcase };
}
