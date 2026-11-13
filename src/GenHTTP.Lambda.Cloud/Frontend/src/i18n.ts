import { defineSite, useCatalog, type Language, type Loader } from '@genhttp-lambda/kit/i18n';

import type { Messages } from './locales/en';

/* The catalog of the site: the kit's words and the pages' own, in one. */

const SITE: Record<Language, Loader<Messages>> = {
  id: () => import('./locales/id').then((module) => module.messages),
  de: () => import('./locales/de').then((module) => module.messages),
  en: () => import('./locales/en').then((module) => module.messages),
  es: () => import('./locales/es').then((module) => module.messages),
  fr: () => import('./locales/fr').then((module) => module.messages),
  it: () => import('./locales/it').then((module) => module.messages),
  nl: () => import('./locales/nl').then((module) => module.messages),
  pl: () => import('./locales/pl').then((module) => module.messages),
  pt: () => import('./locales/pt').then((module) => module.messages),
  'pt-pt': () => import('./locales/pt-pt').then((module) => module.messages),
  tr: () => import('./locales/tr').then((module) => module.messages),
  ja: () => import('./locales/ja').then((module) => module.messages),
  ko: () => import('./locales/ko').then((module) => module.messages),
};

const site = defineSite(SITE);

/** The words of the site, in the language of the page. */
export function useT(): Messages {
  return useCatalog(site);
}
