import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

import pages from './pages.json';

const SITE = 'GenHTTP Lambda';

export interface PageMeta {
  /** The page's own name, without the site's - that is appended here. */
  title: string;
  description?: string;
  /** False for pages that are nobody's business but their owner's, or not there at all. */
  index?: boolean;
}

/**
 * The pages a search engine is meant to find, by path.
 *
 * The server reads the same file from the build, writes these into the index
 * page before sending it and lists them in the sitemap - so a crawler that
 * runs no script and a link preview in a chat both see the page they asked
 * for rather than the front page.
 */
export const PAGES: Record<keyof typeof pages, PageMeta> = pages;

/**
 * Names the page the visitor is on.
 *
 * Every route is answered with the same index page, so without this the whole
 * site is one title and one description to a tab bar and to a crawler that
 * renders. Pages that should never turn up in a search - an editor behind its
 * key, the admin views, a path that does not exist - say so here as well.
 */
export function usePageMeta({ title, description, index = true }: PageMeta): void {
  const { pathname } = useLocation();

  useEffect(() => {
    const full = `${title} - ${SITE}`;

    document.title = full;

    set('name', 'description', description);
    set('property', 'og:title', full);
    set('property', 'og:description', description);
    set('name', 'twitter:title', full);
    set('name', 'twitter:description', description);
    set('name', 'robots', index ? undefined : 'noindex');

    // the server only writes these for the page that was loaded, so they
    // follow the visitor from there - and a page not to be indexed has none
    const canonical = document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');

    if (canonical !== null) {
      const address = `${new URL(canonical.href).origin}${pathname}`;

      canonical.href = address;
      set('property', 'og:url', index ? address : undefined);
    }
  }, [title, description, index, pathname]);
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
