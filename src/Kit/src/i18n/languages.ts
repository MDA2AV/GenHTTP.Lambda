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
 *
 * A code is a language, or a language and a region where the site is written
 * in more than one variant of it: "pt" is Portuguese as Brazil writes it,
 * which most readers of Portuguese do, and "pt-pt" as Portugal does.
 */
export const LANGUAGES = ['id', 'de', 'en', 'es', 'fr', 'it', 'nl', 'pl', 'pt', 'pt-pt', 'tr', 'ja', 'ko'] as const;

export type Language = (typeof LANGUAGES)[number];

export const DEFAULT_LANGUAGE: Language = 'en';

/**
 * Each language in its own words, the way a switcher should offer it - in the
 * order above, which is alphabetical by these names, with the languages in
 * other scripts after the ones in Latin letters.
 */
export const LANGUAGE_NAMES: Record<Language, string> = {
  id: 'Bahasa Indonesia',
  de: 'Deutsch',
  en: 'English',
  es: 'Español',
  fr: 'Français',
  it: 'Italiano',
  nl: 'Nederlands',
  pl: 'Polski',
  pt: 'Português (Brasil)',
  'pt-pt': 'Português (Portugal)',
  tr: 'Türkçe',
  ja: '日本語',
  ko: '한국어',
};

/**
 * How a language is tagged in the markup, for a search engine and a screen
 * reader - its code, except where the code leaves out the region.
 */
const TAGS: Partial<Record<Language, string>> = { pt: 'pt-BR', 'pt-pt': 'pt-PT' };

/**
 * The variant that also stands for its whole language, for a reader in a
 * country with no variant of its own here: Portuguese outside of Brazil and
 * Portugal finds the Brazilian pages, which most of its readers write.
 */
const WHOLE: Partial<Record<Language, string>> = { pt: 'pt' };

/**
 * Where a language is written after the variant of another country than the
 * one its primary code stands for: Angola, Mozambique and the other countries
 * of Portuguese in Africa and Asia write it as Portugal does.
 */
const REGIONS: Record<string, Language> = Object.fromEntries(
  ['pt', 'ao', 'mz', 'cv', 'gw', 'st', 'tl', 'mo'].map((region): [string, Language] => [`pt-${region}`, 'pt-pt']),
);

/**
 * Codes some browsers still send for a language that has another one now:
 * older Android and Java call Indonesian "in". The server knows them too.
 */
const ALIASES: Record<string, Language> = { in: 'id' };

/** The language as the markup tags it: "de", or "pt-BR" for Portuguese. */
export function tagOf(language: Language): string {
  return TAGS[language] ?? language;
}

/**
 * What a page in the language answers for among its translations - its tag,
 * and for the variant standing for a whole language, that language.
 */
export function hreflangsOf(language: Language): string[] {
  const whole = WHOLE[language];

  return whole === undefined ? [tagOf(language)] : [tagOf(language), whole];
}

/**
 * The language the site has for a tag a browser sent: "pt-PT" is Portuguese
 * as Portugal writes it, "pt-AO" as well, "pt-BR" and "pt" are Brazilian,
 * "de-CH" is German - or nothing, for a language it has not.
 */
export function matchLanguage(tag: string): Language | null {
  const lower = tag.trim().toLowerCase();

  if (REGIONS[lower] !== undefined) {
    return REGIONS[lower];
  }

  const primary = lower.split('-')[0] ?? '';
  const language = ALIASES[primary] ?? primary;

  return isLanguage(language) ? language : null;
}

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

  const rest = pathname.slice(pathname.split('/')[1].length + 1);

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
    const language = tag ? matchLanguage(tag) : null;

    if (language !== null) {
      return language;
    }
  }

  return DEFAULT_LANGUAGE;
}

function readCookie(): Language | null {
  const match = document.cookie.match(/(?:^|;\s*)lang=([a-z-]+)/);

  return match && isLanguage(match[1]) ? match[1] : null;
}

/** Remembers a choice for a year, for this page and for the server. */
export function rememberLanguage(language: Language) {
  document.cookie = `${COOKIE}=${language}; path=/; max-age=31536000; SameSite=Lax`;
}
