import { defineSite, type Language, type Loader } from '@genhttp-lambda/kit/i18n';
import type { KitMessages } from '@genhttp-lambda/kit/locales/en';

/*
 * The panel is in English (see CLAUDE.md, Translations); only the shell
 * around it reads the kit's words, in the language the operator chose.
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

defineSite(SITE);
