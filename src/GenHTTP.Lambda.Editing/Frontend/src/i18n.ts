import { defineCatalog, defineSite, useCatalog, type Language, type Loader } from '@genhttp-lambda/kit/i18n';

import type { EditorMessages } from './locales/en';
import type { Messages } from './locales/en/site';

/*
 * The catalogs of the editor: the words every page of it reads - the shell,
 * and the page that creates a lambda - and the editor's own, fetched with the
 * editor, since a visitor who only creates a lambda never needs them.
 */

const SITE: Record<Language, Loader<Messages>> = {
  id: () => import('./locales/id/site').then((module) => module.messages),
  de: () => import('./locales/de/site').then((module) => module.messages),
  en: () => import('./locales/en/site').then((module) => module.messages),
  es: () => import('./locales/es/site').then((module) => module.messages),
  fr: () => import('./locales/fr/site').then((module) => module.messages),
  it: () => import('./locales/it/site').then((module) => module.messages),
  nl: () => import('./locales/nl/site').then((module) => module.messages),
  pl: () => import('./locales/pl/site').then((module) => module.messages),
  pt: () => import('./locales/pt/site').then((module) => module.messages),
  'pt-pt': () => import('./locales/pt-pt/site').then((module) => module.messages),
  tr: () => import('./locales/tr/site').then((module) => module.messages),
  ja: () => import('./locales/ja/site').then((module) => module.messages),
  ko: () => import('./locales/ko/site').then((module) => module.messages),
};

const EDITOR: Record<Language, Loader<EditorMessages>> = {
  id: () => import('./locales/id').then((module) => module.editor),
  de: () => import('./locales/de').then((module) => module.editor),
  en: () => import('./locales/en').then((module) => module.editor),
  es: () => import('./locales/es').then((module) => module.editor),
  fr: () => import('./locales/fr').then((module) => module.editor),
  it: () => import('./locales/it').then((module) => module.editor),
  nl: () => import('./locales/nl').then((module) => module.editor),
  pl: () => import('./locales/pl').then((module) => module.editor),
  pt: () => import('./locales/pt').then((module) => module.editor),
  'pt-pt': () => import('./locales/pt-pt').then((module) => module.editor),
  tr: () => import('./locales/tr').then((module) => module.editor),
  ja: () => import('./locales/ja').then((module) => module.editor),
  ko: () => import('./locales/ko').then((module) => module.editor),
};

const site = defineSite(SITE);

const editor = defineCatalog(EDITOR);

/** The words of the pages of the editor, in the language of the page. */
export function useT(): Messages {
  return useCatalog(site);
}

/** The words of the editor, in the language of the page. */
export function useEditorT(): EditorMessages {
  return useCatalog(editor);
}

/** Fetches the words of the editor in a language, together with the editor itself. */
export function loadEditor(language: Language): Promise<EditorMessages> {
  return editor.load(language);
}
