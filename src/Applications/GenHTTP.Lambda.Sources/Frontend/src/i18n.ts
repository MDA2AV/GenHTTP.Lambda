import { defineCatalog, defineSite, useCatalog, type Language, type Loader } from '@genhttp-lambda/kit/i18n';
import type { KitMessages } from '@genhttp-lambda/kit/locales/en';

import type { SourceMessages } from './locales/en/source';

/*
 * The catalogs of the published sources: the kit's words, which the shell
 * reads, and the pages' own.
 */

const SITE: Record<Language, Loader<KitMessages>> = {
  id: () => import('@genhttp-lambda/kit/locales/id').then((module) => module.kit),
  de: () => import('@genhttp-lambda/kit/locales/de').then((module) => module.kit),
  en: () => import('@genhttp-lambda/kit/locales/en').then((module) => module.kit),
  es: () => import('@genhttp-lambda/kit/locales/es').then((module) => module.kit),
  fr: () => import('@genhttp-lambda/kit/locales/fr').then((module) => module.kit),
  it: () => import('@genhttp-lambda/kit/locales/it').then((module) => module.kit),
  nl: () => import('@genhttp-lambda/kit/locales/nl').then((module) => module.kit),
  pl: () => import('@genhttp-lambda/kit/locales/pl').then((module) => module.kit),
  pt: () => import('@genhttp-lambda/kit/locales/pt').then((module) => module.kit),
  'pt-pt': () => import('@genhttp-lambda/kit/locales/pt-pt').then((module) => module.kit),
  tr: () => import('@genhttp-lambda/kit/locales/tr').then((module) => module.kit),
  ja: () => import('@genhttp-lambda/kit/locales/ja').then((module) => module.kit),
  ko: () => import('@genhttp-lambda/kit/locales/ko').then((module) => module.kit),
};

const SOURCE: Record<Language, Loader<SourceMessages>> = {
  id: () => import('./locales/id/source').then((module) => module.source),
  de: () => import('./locales/de/source').then((module) => module.source),
  en: () => import('./locales/en/source').then((module) => module.source),
  es: () => import('./locales/es/source').then((module) => module.source),
  fr: () => import('./locales/fr/source').then((module) => module.source),
  it: () => import('./locales/it/source').then((module) => module.source),
  nl: () => import('./locales/nl/source').then((module) => module.source),
  pl: () => import('./locales/pl/source').then((module) => module.source),
  pt: () => import('./locales/pt/source').then((module) => module.source),
  'pt-pt': () => import('./locales/pt-pt/source').then((module) => module.source),
  tr: () => import('./locales/tr/source').then((module) => module.source),
  ja: () => import('./locales/ja/source').then((module) => module.source),
  ko: () => import('./locales/ko/source').then((module) => module.source),
};

defineSite(SITE);

const source = defineCatalog(SOURCE);

/** The words of the published sources' pages, in the language of the page. */
export function useSourceT(): SourceMessages {
  return useCatalog(source);
}

/** Fetches the words of the published sources' pages in a language, together with those pages. */
export function loadSource(language: Language): Promise<SourceMessages> {
  return source.load(language);
}
