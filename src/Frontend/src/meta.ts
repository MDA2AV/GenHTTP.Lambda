import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

import { useLanguage } from './i18n';
import { LANGUAGES, inLanguage, withoutLanguage, type Language } from './i18n/languages';
import pages from './pages.json';

const SITE = 'GenHTTP Lambda';

export interface PageMeta {
  /** The page's own name, without the site's - that is appended here. */
  title: string;
  description?: string;
  /** False for pages that are nobody's business but their owner's, or not there at all. */
  index?: boolean;
}

/** The path of a public page, without the language it is shown in. */
export type PagePath = keyof typeof pages;

/** How a search result and a link preview name each locale, as Open Graph spells them. */
export const OG_LOCALES: Record<Language, string> = {
  en: 'en_US',
  de: 'de_DE',
  es: 'es_ES',
  pt: 'pt_BR',
  fr: 'fr_FR',
  it: 'it_IT',
};

/**
 * The name and description of a public page, in a language.
 *
 * The table is the one the server reads from the build, writes into the index
 * page before sending it and lists in the sitemap - so a crawler that runs no
 * script and a link preview in a chat both see the page they asked for, in
 * the language they asked for it in.
 */
export function pageMeta(path: PagePath, language: Language): PageMeta {
  return pages[path].text[language];
}

/** Names a public page, in the language it is being shown in. */
export function usePublicPage(path: PagePath): void {
  usePageMeta(pageMeta(path, useLanguage()));
}

/**
 * Names the page the visitor is on.
 *
 * Every route is answered with the same index page, so without this the whole
 * site is one title and one description to a tab bar and to a crawler that
 * renders. Pages that should never turn up in a search - an editor behind its
 * key, the admin views, a path that does not exist - say so here as well.
 *
 * The server writes the canonical address and the addresses of the page in
 * every other language for the page that was loaded; they follow the visitor
 * from there.
 */
export function usePageMeta({ title, description, index = true }: PageMeta): void {
  const { pathname } = useLocation();
  const language = useLanguage();

  useEffect(() => {
    const full = `${title} - ${SITE}`;

    document.title = full;
    document.documentElement.lang = language;

    set('name', 'description', description);
    set('property', 'og:title', full);
    set('property', 'og:description', description);
    set('property', 'og:locale', OG_LOCALES[language]);
    set('name', 'twitter:title', full);
    set('name', 'twitter:description', description);
    set('name', 'robots', index ? undefined : 'noindex');

    // the server only writes these when it knows its public address, and
    // then for the page that was loaded - a page not to be indexed has none
    const canonical = document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');

    if (canonical === null) {
      return;
    }

    const origin = new URL(canonical.href).origin;
    const address = `${origin}${pathname}`;

    canonical.href = address;
    set('property', 'og:url', index ? address : undefined);

    alternates(index ? origin : null, withoutLanguage(pathname));
  }, [title, description, index, pathname, language]);
}

/**
 * The same page in every language, and the address without one for whoever
 * matches none of them - or none of that, for a page that is not indexed.
 */
function alternates(origin: string | null, path: string) {
  document.head.querySelectorAll('link[rel="alternate"][hreflang]').forEach((link) => link.remove());

  if (origin === null) {
    return;
  }

  const links: [string, string][] = [
    ...LANGUAGES.map((language): [string, string] => [language, inLanguage(language, path)]),
    ['x-default', path],
  ];

  for (const [hreflang, href] of links) {
    const link = document.createElement('link');

    link.rel = 'alternate';
    link.hreflang = hreflang;
    link.href = `${origin}${href}`;

    document.head.appendChild(link);
  }
}

/** Sets a meta tag, creating it if needed; no value removes it. */
function set(attribute: 'name' | 'property', key: string, value: string | undefined): void {
  let tag = document.head.querySelector<HTMLMetaElement>(`meta[${attribute}="${key}"]`);

  if (value === undefined) {
    tag?.remove();
    return;
  }

  if (tag === null) {
    tag = document.createElement('meta');
    tag.setAttribute(attribute, key);
    document.head.appendChild(tag);
  }

  tag.content = value;
}
