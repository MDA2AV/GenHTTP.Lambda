import { forwardRef } from 'react';
import {
  Link as RouterLink,
  NavLink as RouterNavLink,
  type LinkProps,
  type NavLinkProps,
  type To,
} from 'react-router-dom';

import { useLanguage } from '.';
import { inLanguage, withoutLanguage, type Language } from './languages';

/**
 * The paths of the platform that are no page of a site: the components that
 * have no language in their address, and the lambdas.
 */
const UNLOCALIZED = ['/editor', '/admin', '/api', '/mcp', '/lambda', '/features', '/assets'];

/**
 * Whether a path is a public page in every language: whatever a site serves -
 * the front page, its guide, the published sources - as opposed to the
 * editor, the panel, the API and the lambdas, which have no language in their
 * address. Said by what it is not, since the pages of a site differ between
 * installations and the components that link to them are the same in each.
 */
export const isLocalized = (path: string) =>
  path.startsWith('/') && !UNLOCALIZED.some((prefix) => path === prefix || path.startsWith(prefix + '/'));

/**
 * The paths this bundle draws itself; a link anywhere else on the host is
 * another bundle's, and is followed by loading it rather than by the router.
 */
let owned: (path: string) => boolean = () => true;

/** Whether a path is a page of the published sources, in a language or not. */
export function isSourcePath(path: string): boolean {
  const bare = withoutLanguage(path);

  return bare === '/source' || bare.startsWith('/source/');
}

/**
 * Whether a path is a page of a site - its front page, its guide, its
 * showcase - rather than one of the components every site has: the editor,
 * the panel, the published sources, the API and the lambdas.
 */
export function isSitePath(path: string): boolean {
  return isLocalized(path) && !isSourcePath(path);
}

/** Says which paths of the host this bundle draws, as it starts. */
export function claim(paths: (path: string) => boolean) {
  owned = paths;
}

/** Whether this bundle draws a path, so the router can take a link there. */
export function owns(to: string): boolean {
  const cut = to.search(/[?#]/);
  const path = cut < 0 ? to : to.slice(0, cut);

  return !path.startsWith('/') || owned(path);
}

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
export const Link = forwardRef<HTMLAnchorElement, LinkProps>(function Link({ to, reloadDocument, ...props }, ref) {
  const localize = useLocalize();
  const target = localized(to, localize);

  // a page of another bundle is loaded rather than routed to, which this
  // bundle could not draw
  return <RouterLink ref={ref} to={target} reloadDocument={reloadDocument || (typeof target === 'string' && !owns(target))} {...props} />;
});

/** The router's navigation link, in the language of the page. */
export const NavLink = forwardRef<HTMLAnchorElement, NavLinkProps>(function NavLink({ to, reloadDocument, ...props }, ref) {
  const localize = useLocalize();
  const target = localized(to, localize);

  return <RouterNavLink ref={ref} to={target} reloadDocument={reloadDocument || (typeof target === 'string' && !owns(target))} {...props} />;
});
