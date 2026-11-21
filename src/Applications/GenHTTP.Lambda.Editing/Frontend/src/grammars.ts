// which grammar a file is coloured with, by its name - kept apart from monaco.ts
// so that what only names a grammar does not load the editor

/**
 * Which grammar a file is written in, by what it is called.
 *
 * Anything unrecognised is plain text rather than a guess: colouring markup as
 * if it were code is worse than not colouring it.
 */
const GRAMMARS: Record<string, string> = {
  cs: 'csharp',
  html: 'html',
  htm: 'html',
  css: 'css',
  js: 'javascript',
  mjs: 'javascript',
  cjs: 'javascript',
  jsx: 'javascript',
  ts: 'typescript',
  mts: 'typescript',
  cts: 'typescript',
  tsx: 'typescript',
  json: 'json',
  jsonc: 'json',
  vue: 'html',
  svelte: 'html',
  astro: 'html',
  scss: 'scss',
  less: 'less',
  yml: 'yaml',
  yaml: 'yaml',
  toml: 'ini',
  ini: 'ini',
  xml: 'xml',
  csproj: 'xml',
  svg: 'html',
  txt: 'plaintext',
  md: 'markdown',
  sh: 'shell',
};

export function languageFor(name: string): string {
  const dot = name.lastIndexOf('.');

  return (dot < 0 ? '' : GRAMMARS[name.slice(dot + 1).toLowerCase()]) ?? 'plaintext';
}
