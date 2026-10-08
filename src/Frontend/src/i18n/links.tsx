import { forwardRef } from 'react';
import {
  Link as RouterLink,
  NavLink as RouterNavLink,
  type LinkProps,
  type NavLinkProps,
  type To,
} from 'react-router-dom';

import pages from '../pages.json';
import { useLanguage } from '.';
import { inLanguage, type Language } from './languages';

/**
 * The pages that exist once per language, by their path without one - not the
 * templates the table holds the words of other pages in, whose keys have a
 * colon in them.
 */
const LOCALIZED = new Set(Object.keys(pages).filter((path) => !path.includes(':')));

/**
 * Whether a path is a public page in every language: one of the build's, or
 * the page of a published source - "/source/quiz" and whatever is below it,
 * which the build cannot list.
 */
export const isLocalized = (path: string) => LOCALIZED.has(path) || path.startsWith('/source/');

/**
 * A link to a public page, in the given language: "/build" is "/de/build"
 * in German, and "/#agents" is "/de#agents". Anything else - the editor, the
 * lambdas, an address elsewhere - has no language, and stays as it is.
 */
export function localize(language: Language, to: string): string {
  if (!to.startsWith('/')) {
    return to;
  }

  const cut = to.search(/[?#]/);
  const path = cut < 0 ? to : to.slice(0, cut);
  const rest = cut < 0 ? '' : to.slice(cut);

  return isLocalized(path) ? inLanguage(language, path) + rest : to;
}

/** Localizes links for the language of the page being looked at. */
export function useLocalize(): (to: string) => string {
  const language = useLanguage();

  return (to) => localize(language, to);
}

function localized(to: To, localize: (to: string) => string): To {
  return typeof to === 'string' ? localize(to) : to;
}

/**
 * The router's link, staying in the language of the page: a page links to
 * "/build" and a visitor reading German is taken to "/de/build".
 */
export const Link = forwardRef<HTMLAnchorElement, LinkProps>(function Link({ to, ...props }, ref) {
  const localize = useLocalize();

  return <RouterLink ref={ref} to={localized(to, localize)} {...props} />;
});

/** The router's navigation link, in the language of the page. */
export const NavLink = forwardRef<HTMLAnchorElement, NavLinkProps>(function NavLink({ to, ...props }, ref) {
  const localize = useLocalize();

  return <RouterNavLink ref={ref} to={localized(to, localize)} {...props} />;
});
