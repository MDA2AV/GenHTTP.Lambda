/**
 * The languages the site is written in.
 *
 * Every public page lives once per language, under its own prefix - /de/build
 * is the German build page, for everybody, whatever their browser says. That
 * is what lets a search engine find and list each of them: a page that
 * changed its language by a header or a cookie would be one page to a crawler,
 * in whatever language the crawler happened to ask for.
 *
 * Only the addresses without a prefix pick a language, and the server does it
 * (Web/SiteLanguages.cs, which lists the same codes): a choice made here,
 * remembered in a cookie, and otherwise the browser's Accept-Language.
 */
export const LANGUAGES = ['en', 'de', 'es', 'pt', 'fr', 'it'] as const;

export type Language = (typeof LANGUAGES)[number];

export const DEFAULT_LANGUAGE: Language = 'en';

/** Each language in its own words, the way a switcher should offer it. */
export const LANGUAGE_NAMES: Record<Language, string> = {
  en: 'English',
  de: 'Deutsch',
  es: 'Español',
  pt: 'Português',
  fr: 'Français',
  it: 'Italiano',
};

/** Where a choice is remembered, and read by the server as well. */
export const COOKIE = 'lang';

export function isLanguage(value: string | undefined | null): value is Language {
  return (LANGUAGES as readonly string[]).includes(value ?? '');
}

/**
 * The language a path is in, if it has a prefix - "/de/build" is German, and
 * "/editor/x" is in none, which leaves it to whatever the visitor prefers.
 */
export function languageOf(pathname: string): Language | null {
  const first = pathname.split('/')[1];

  return isLanguage(first) ? first : null;
}

/** The path with its language taken off: "/de/build" is "/build", "/de" is "/". */
export function withoutLanguage(pathname: string): string {
  if (languageOf(pathname) === null) {
    return pathname;
  }

  const rest = pathname.slice(3);

  return rest === '' ? '/' : rest;
}

/** The same path in the given language: "/build" in German is "/de/build", "/" is "/de". */
export function inLanguage(language: Language, path: string): string {
  const bare = withoutLanguage(path);

  return bare === '/' ? `/${language}` : `/${language}${bare}`;
}

/**
 * What the visitor prefers, for the pages that have no language of their own:
 * the language they chose here, or else the first one their browser names
 * that the site is written in.
 */
export function preferredLanguage(): Language {
  if (typeof document === 'undefined') {
    return DEFAULT_LANGUAGE;
  }

  const chosen = readCookie();

  if (chosen !== null) {
    return chosen;
  }

  for (const tag of navigator.languages ?? [navigator.language]) {
    const primary = tag?.split('-')[0]?.toLowerCase();

    if (isLanguage(primary)) {
      return primary;
    }
  }

  return DEFAULT_LANGUAGE;
}

function readCookie(): Language | null {
  const match = document.cookie.match(/(?:^|;\s*)lang=([a-z]+)/);

  return match && isLanguage(match[1]) ? match[1] : null;
}

/** Remembers a choice for a year, for this page and for the server. */
export function rememberLanguage(language: Language) {
  document.cookie = `${COOKIE}=${language}; path=/; max-age=31536000; SameSite=Lax`;
}
