import { useSyncExternalStore } from 'react';
import { useLocation } from 'react-router-dom';

import type { KitMessages } from '../locales/en';
import { DEFAULT_LANGUAGE, LANGUAGES, languageOf, preferredLanguage, rememberLanguage, type Language } from './languages';

export * from './languages';

/*
 * The words of a bundle, one catalog per language, fetched on their own so a
 * visitor downloads only the language they read. Every bundle has a catalog of
 * its site - the kit's words with its own (see the kit's catalog in
 * locales/en/index.ts) - which every page of it reads, and may have more that
 * only some of its pages need: the editor's, fetched with the editor.
 *
 * A catalog that is not there yet suspends whoever asked for it. The page a
 * visitor lands on has its catalog fetched before it is drawn (each bundle's
 * main.tsx), and the switcher fetches the next one before it goes there, so in
 * practice this only ever waits where a Suspense boundary is waiting anyway.
 */

export type Loader<T> = () => Promise<T>;

/** Catalogs by language, with the fetch that is bringing one while it is on its way. */
export class Catalogs<T> {
  private readonly loaded = new Map<Language, T>();

  private readonly pending = new Map<Language, Promise<T>>();

  constructor(private readonly loaders: Record<Language, Loader<T>>) {}

  get(language: Language): T | undefined {
    return this.loaded.get(language);
  }

  /** Whether any language of this catalog was ever asked for - the editor's, once it is open. */
  get used(): boolean {
    return this.pending.size > 0;
  }

  load(language: Language): Promise<T> {
    let promise = this.pending.get(language);

    if (promise === undefined) {
      promise = this.loaders[language]().then((catalog) => {
        this.loaded.set(language, catalog);
        return catalog;
      });

      // a failed fetch may be tried again, rather than failing for good
      promise.catch(() => this.pending.delete(language));

      this.pending.set(language, promise);
    }

    return promise;
  }

  /** The catalog, or a suspension until it is there. */
  read(language: Language): T {
    const catalog = this.loaded.get(language);

    if (catalog !== undefined) {
      return catalog;
    }

    throw this.load(language);
  }
}

/** The catalog every page of this bundle reads, which holds the kit's words as well. */
let site: Catalogs<KitMessages> | null = null;

/** The others, fetched with the pages that read them. */
const others = new Set<Catalogs<unknown>>();

function requireSite(): Catalogs<KitMessages> {
  if (site === null) {
    throw new Error('No catalog of the site was defined: call defineSite in the bundle before drawing anything.');
  }

  return site;
}

/**
 * Names the catalog of this bundle's site, which every page of it reads - the
 * kit's words, and the bundle's own beside them. Called once, as the bundle
 * starts.
 */
export function defineSite<T extends KitMessages>(loaders: Record<Language, Loader<T>>): Catalogs<T> {
  const catalogs = new Catalogs(loaders);

  site = catalogs;

  return catalogs;
}

/**
 * Another catalog of this bundle, which only some of its pages read - fetched
 * with them, and in every language the visitor switches to once it was.
 */
export function defineCatalog<T>(loaders: Record<Language, Loader<T>>): Catalogs<T> {
  const catalogs = new Catalogs(loaders);

  others.add(catalogs as Catalogs<unknown>);

  return catalogs;
}

/** Fetches the words of the site in a language, before a page in it is drawn. */
export function loadSite(language: Language): Promise<KitMessages> {
  return requireSite().load(language);
}

/** Everything a page in the given language needs, fetched before going there. */
export async function prepare(language: Language): Promise<void> {
  await Promise.all([requireSite().load(language), ...[...others].filter((catalog) => catalog.used).map((catalog) => catalog.load(language))]);
}

/* ------------------------------------------------ what the visitor prefers */

let preferred: Language | null = null;

const listeners = new Set<() => void>();

const readPreferred = () => (preferred ??= preferredLanguage());

function subscribe(changed: () => void) {
  listeners.add(changed);
  return () => listeners.delete(changed);
}

/**
 * Takes a language the visitor chose: remembered for the addresses without a
 * prefix, here and on the server, and applied at once to a page that has no
 * language of its own, like the editor.
 */
export async function choose(language: Language): Promise<void> {
  await prepare(language);

  rememberLanguage(language);
  preferred = language;

  listeners.forEach((changed) => changed());
}

/**
 * The language of the page being looked at: the one in its address, or the
 * one the visitor prefers where the address has none.
 */
export function useLanguage(): Language {
  const { pathname } = useLocation();

  const fallback = useSyncExternalStore(subscribe, readPreferred, () => DEFAULT_LANGUAGE);

  return languageOf(pathname) ?? fallback;
}

/** The words the kit's components read, in the language of the page. */
export function useT(): KitMessages {
  return requireSite().read(useLanguage());
}

/** The words of a catalog of this bundle, in the language of the page. */
export function useCatalog<T>(catalogs: Catalogs<T>): T {
  return catalogs.read(useLanguage());
}

/** Every language of the site at once, for rendering the pages when the frontend is built. */
export async function loadEverything(): Promise<void> {
  const catalogs = requireSite();

  await Promise.all(LANGUAGES.map((language) => catalogs.load(language)));
}
