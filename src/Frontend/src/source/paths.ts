/**
 * Where things are on the page of a published source, without the language
 * (the links localize them):
 *
 *   /source/quiz                          the code, Project.cs open
 *   /source/quiz/files/assets/app.js      the code, that file open
 *   /source/quiz/docs[/decisions.md]      the documentation, a page of it
 *   /source/quiz/tests                    how it is tested
 *   /source/quiz/changes                  every version
 *
 * with ?version=7 for any version but the newest - so every view of every
 * version has an address of its own, to share and to come back to.
 */

export type SourceView = 'code' | 'docs' | 'tests' | 'changes';

/** The file the code opens on: the lambda itself. */
export const ENTRY = 'Project.cs';

/** The pages of the documentation every version is meant to have, in the order they are read. */
export const PRODUCT = 'docs/product.md';
export const DECISIONS = 'docs/decisions.md';
export const TESTING = 'tests/README.md';

interface Where {
  /** A file of the project, for the code. */
  file?: string | null;
  /** A page below docs/, for the documentation. */
  page?: string | null;
  /** The version, or nothing for the newest. */
  version?: number | null;
  /** A line, or lines, of the file. */
  lines?: string | null;
}

const segments = (path: string) => path.split('/').map(encodeURIComponent).join('/');

export function sourcePath(publicKey: string, view: SourceView, where: Where = {}): string {
  let path = `/source/${publicKey}`;

  if (view === 'code' && where.file && where.file !== ENTRY) {
    path += `/files/${segments(where.file)}`;
  } else if (view === 'docs') {
    path += where.page && where.page !== PRODUCT ? `/docs/${segments(where.page.replace(/^docs\//, ''))}` : '/docs';
  } else if (view !== 'code') {
    path += `/${view}`;
  }

  return path + (where.version != null ? `?version=${where.version}` : '') + (where.lines ? `#${where.lines}` : '');
}

/** What an address below a source asks for: its view, and the file or page in it. */
export function parseSourcePath(rest: string): { view: SourceView; file: string | null; page: string | null } {
  const parts = rest.split('/').filter((part) => part.length > 0);

  switch (parts[0]) {
    case 'files':
      return { view: 'code', file: parts.slice(1).join('/') || ENTRY, page: null };
    case 'docs':
      return { view: 'docs', file: null, page: parts.length > 1 ? `docs/${parts.slice(1).join('/')}` : null };
    case 'tests':
      return { view: 'tests', file: null, page: null };
    case 'changes':
      return { view: 'changes', file: null, page: null };
    default:
      return { view: 'code', file: ENTRY, page: null };
  }
}
