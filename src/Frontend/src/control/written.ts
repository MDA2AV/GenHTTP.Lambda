import type { LambdaFile } from '../api';

/**
 * How a version's files are laid out: its code - every file that is not a
 * resource, in whatever folders, of which the .cs files are compiled - and
 * its resources, below resources/, which it reads and serves while it runs.
 *
 * The same rule the server applies (LambdaSource). What is written about a
 * version - its documentation and its tests - is in docs/ and tests/ of its
 * code: folders like any other, which this editor reads to show them as
 * what they are.
 */

/** Where the resources of a version are: what it reads and serves. */
export const RESOURCES = 'resources/';

export const DOCS = 'docs/';

export const TESTS = 'tests/';

/** What the app is, who it is for and why - the one page the simple view shows. */
export const PRODUCT = 'docs/product.md';

/** The technical decisions, and why they were made. */
export const DECISIONS = 'docs/decisions.md';

/** How the app is tested automatically. */
export const TESTING = 'tests/README.md';

/** Whether a file is a resource, read and served while the lambda runs. */
export const isResource = (name: string) => name.startsWith(RESOURCES);

/** Whether a file belongs to the code: everything that is not a resource. */
export const isCode = (name: string) => !isResource(name);

/** Whether a file is C# the lambda is compiled from: a .cs file of the code, in whichever folder. */
export const isCompiled = (name: string) => isCode(name) && name.toLowerCase().endsWith('.cs');

/** The two folders of the code shown as what they are, each a section of the editor. */
export type Area = 'docs' | 'tests';

export const AREAS: Record<Area, { folder: string; pages: string[] }> = {
  docs: { folder: DOCS, pages: [PRODUCT, DECISIONS] },
  tests: { folder: TESTS, pages: [TESTING] },
};

/** Whether a file is a page to read - markdown, as text - rather than a file to look at. */
export const isPage = (file: LambdaFile) => file.encoding !== 'base64' && file.name.toLowerCase().endsWith('.md');

/**
 * The pages of an area in the order they are read: the known ones first,
 * then the rest by name.
 */
export function pagesOf(files: LambdaFile[], area: Area): LambdaFile[] {
  const { folder, pages } = AREAS[area];
  const rank = (name: string) => (pages.includes(name) ? pages.indexOf(name) : pages.length);

  return files
    .filter((file) => file.name.startsWith(folder) && isPage(file))
    .sort((a, b) => rank(a.name) - rank(b.name) || a.name.localeCompare(b.name));
}

/** The files of an area that are not pages: pictures, scripts, test data. */
export function othersOf(files: LambdaFile[], area: Area): LambdaFile[] {
  const { folder } = AREAS[area];

  return files.filter((file) => file.name.startsWith(folder) && !isPage(file)).sort((a, b) => a.name.localeCompare(b.name));
}

/** A file's name within its area: 'tests/data/x.json' is 'data/x.json'. */
export function within(name: string, area: Area): string {
  const { folder } = AREAS[area];

  return name.startsWith(folder) ? name.slice(folder.length) : name;
}

/** The first heading of a page, which names it better than its file does. */
export function titleOf(page: LambdaFile): string | null {
  const match = /^#\s+(.+?)\s*#*\s*$/m.exec(page.code);

  return match ? match[1] : null;
}
