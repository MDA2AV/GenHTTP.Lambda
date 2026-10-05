import hljs from 'highlight.js/lib/core';
import bash from 'highlight.js/lib/languages/bash';
import csharp from 'highlight.js/lib/languages/csharp';
import css from 'highlight.js/lib/languages/css';
import ini from 'highlight.js/lib/languages/ini';
import dockerfile from 'highlight.js/lib/languages/dockerfile';
import javascript from 'highlight.js/lib/languages/javascript';
import json from 'highlight.js/lib/languages/json';
import markdown from 'highlight.js/lib/languages/markdown';
import scss from 'highlight.js/lib/languages/scss';
import sql from 'highlight.js/lib/languages/sql';
import typescript from 'highlight.js/lib/languages/typescript';
import xml from 'highlight.js/lib/languages/xml';
import yaml from 'highlight.js/lib/languages/yaml';

/*
 * Colours the code of a published source.
 *
 * Not Monaco, which the editor colours with: it is three megabytes, and the
 * page of a source is somewhere people arrive at from a search, to read. The
 * core of highlight.js with the handful of grammars a lambda's project is
 * written in is a fraction of that, and the colours are the editor's (see
 * index.css), so the code reads the same in both places.
 */

hljs.registerLanguage('csharp', csharp);
hljs.registerLanguage('xml', xml);
hljs.registerLanguage('css', css);
hljs.registerLanguage('javascript', javascript);
hljs.registerLanguage('typescript', typescript);
hljs.registerLanguage('json', json);
hljs.registerLanguage('markdown', markdown);
hljs.registerLanguage('sql', sql);
hljs.registerLanguage('bash', bash);
hljs.registerLanguage('dockerfile', dockerfile);
hljs.registerLanguage('yaml', yaml);
// what a build folder holds beside its sources
hljs.registerLanguage('scss', scss);
hljs.registerLanguage('ini', ini);

/** The grammar of a file, by its extension. */
const EXTENSIONS: Record<string, string> = {
  cs: 'csharp',
  csproj: 'xml',
  xml: 'xml',
  html: 'xml',
  htm: 'xml',
  svg: 'xml',
  css: 'css',
  js: 'javascript',
  mjs: 'javascript',
  cjs: 'javascript',
  jsx: 'javascript',
  ts: 'typescript',
  mts: 'typescript',
  cts: 'typescript',
  tsx: 'typescript',
  vue: 'xml',
  svelte: 'xml',
  astro: 'xml',
  scss: 'scss',
  toml: 'ini',
  ini: 'ini',
  json: 'json',
  jsonc: 'json',
  webmanifest: 'json',
  md: 'markdown',
  sql: 'sql',
  sh: 'bash',
  yml: 'yaml',
  yaml: 'yaml',
};

/** The same, for the names the editor gives the languages of code blocks in the documentation. */
const BLOCKS: Record<string, string> = {
  csharp: 'csharp',
  javascript: 'javascript',
  typescript: 'typescript',
  html: 'xml',
  css: 'css',
  json: 'json',
  shell: 'bash',
  markdown: 'markdown',
  sql: 'sql',
  yaml: 'yaml',
  dockerfile: 'dockerfile',
};

/** How long a file may be to be coloured; longer ones are shown as they are, which stays quick. */
const MOST = 400_000;

/** The grammar of a file, or nothing for one that is shown as plain text. */
export function languageOf(path: string): string | null {
  const name = path.split('/').pop() ?? path;

  if (name === 'Dockerfile') {
    return 'dockerfile';
  }

  const extension = name.includes('.') ? name.split('.').pop()!.toLowerCase() : '';

  return EXTENSIONS[extension] ?? null;
}

/** Whether a text is short enough to be coloured. */
export const colourable = (text: string) => text.length <= MOST;

/**
 * The text as markup, coloured where its grammar is known and escaped
 * either way - highlight.js escapes what it does not colour.
 */
export function highlight(text: string, language: string | null): string {
  if (language === null || !colourable(text)) {
    return escape(text);
  }

  try {
    return hljs.highlight(text, { language, ignoreIllegals: true }).value;
  } catch {
    return escape(text);
  }
}

/** Colours the code blocks of a page of the documentation, as the editor names their languages. */
export function highlightBlocks(root: HTMLElement): void {
  root.querySelectorAll<HTMLElement>('pre > code[data-language]').forEach((block) => {
    const language = BLOCKS[block.dataset.language ?? ''];

    if (language) {
      block.innerHTML = highlight(block.textContent ?? '', language);
      block.classList.add('hljs');
    }
  });
}

function escape(text: string): string {
  return text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}
