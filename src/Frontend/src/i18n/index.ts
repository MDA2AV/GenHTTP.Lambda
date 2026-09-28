import { useSyncExternalStore } from 'react';
import { useLocation } from 'react-router-dom';

import type { Messages } from '../locales/en';
import type { EditorMessages } from '../locales/en/editor';
import { DEFAULT_LANGUAGE, languageOf, preferredLanguage, rememberLanguage, type Language } from './languages';

export * from './languages';

/*
 * The words of the site, one catalog per language, fetched on their own so a
 * visitor downloads only the language they read. The editor has a catalog of
 * its own, fetched with the editor, since most visitors never open one.
 *
 * A catalog that is not there yet suspends whoever asked for it. The page a
 * visitor lands on has its catalog fetched before it is drawn (main.tsx), and
 * the switcher fetches the next one before it goes there, so in practice this
 * only ever waits where a Suspense boundary is waiting anyway.
 */

type Loader<T> = () => Promise<T>;

const SITE: Record<Language, Loader<Messages>> = {
  id: () => import('../locales/id').then((module) => module.messages),
  de: () => import('../locales/de').then((module) => module.messages),
  en: () => import('../locales/en').then((module) => module.messages),
  es: () => import('../locales/es').then((module) => module.messages),
  fr: () => import('../locales/fr').then((module) => module.messages),
  it: () => import('../locales/it').then((module) => module.messages),
  nl: () => import('../locales/nl').then((module) => module.messages),
  pl: () => import('../locales/pl').then((module) => module.messages),
  pt: () => import('../locales/pt').then((module) => module.messages),
  'pt-pt': () => import('../locales/pt-pt').then((module) => module.messages),
  tr: () => import('../locales/tr').then((module) => module.messages),
  ja: () => import('../locales/ja').then((module) => module.messages),
  ko: () => import('../locales/ko').then((module) => module.messages),
};

const EDITOR: Record<Language, Loader<EditorMessages>> = {
  id: () => import('../locales/id/editor').then((module) => module.editor),
  de: () => import('../locales/de/editor').then((module) => module.editor),
  en: () => import('../locales/en/editor').then((module) => module.editor),
  es: () => import('../locales/es/editor').then((module) => module.editor),
  fr: () => import('../locales/fr/editor').then((module) => module.editor),
  it: () => import('../locales/it/editor').then((module) => module.editor),
  nl: () => import('../locales/nl/editor').then((module) => module.editor),
  pl: () => import('../locales/pl/editor').then((module) => module.editor),
  pt: () => import('../locales/pt/editor').then((module) => module.editor),
  'pt-pt': () => import('../locales/pt-pt/editor').then((module) => module.editor),
  tr: () => import('../locales/tr/editor').then((module) => module.editor),
  ja: () => import('../locales/ja/editor').then((module) => module.editor),
  ko: () => import('../locales/ko/editor').then((module) => module.editor),
};

/** Catalogs by language, with the fetch that is bringing one while it is on its way. */
class Catalogs<T> {
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

const site = new Catalogs(SITE);

const editor = new Catalogs(EDITOR);

/** Fetches the words of the site in a language, before a page in it is drawn. */
export function loadSite(language: Language): Promise<Messages> {
  return site.load(language);
}

/** Fetches the words of the editor in a language, together with the editor itself. */
export function loadEditor(language: Language): Promise<EditorMessages> {
  return editor.load(language);
}

/** Everything a page in the given language needs, fetched before going there. */
export async function prepare(language: Language): Promise<void> {
  await Promise.all([site.load(language), editor.used ? editor.load(language) : undefined]);
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

/** The words of the site, in the language of the page. */
export function useT(): Messages {
  return site.read(useLanguage());
}

/** The words of the editor, in the language of the page. */
export function useEditorT(): EditorMessages {
  return editor.read(useLanguage());
}

/** Every catalog at once, for rendering the pages when the frontend is built. */
export async function loadEverything(): Promise<void> {
  await Promise.all(Object.keys(SITE).map((language) => site.load(language as Language)));
}
