import type { LambdaFile } from '../api';

/**
 * What a version keeps about itself beside its program: its documentation
 * and its tests, under .lambda/.
 *
 * The same rule the server applies (LambdaSource): a file under .lambda/ is
 * never code and never an asset, whatever it is called - a test written in
 * C# is not compiled, and a page of documentation is not served. It belongs
 * to the version like any other file, so it is saved, compared, rolled back
 * and merged with the program it describes.
 */

export const CONTEXT = '.lambda/';

export const DOCS = '.lambda/docs/';

export const TESTS = '.lambda/tests/';

/** What the app is, who it is for and why - the one page the simple view shows. */
export const PRODUCT = '.lambda/docs/product.md';

/** The technical decisions, and why they were made. */
export const DECISIONS = '.lambda/docs/decisions.md';

/** How the app is tested automatically. */
export const TESTING = '.lambda/tests/README.md';

export const isContext = (name: string) => name.startsWith(CONTEXT);

export const isCode = (name: string) => name.toLowerCase().endsWith('.cs') && !isContext(name);

export const isAsset = (name: string) => !isCode(name) && !isContext(name);

/** The two parts of the context, each a section of the editor. */
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

/** A file's name within its area: '.lambda/tests/data/x.json' is 'data/x.json'. */
export function within(name: string, area: Area): string {
  const { folder } = AREAS[area];

  return name.startsWith(folder) ? name.slice(folder.length) : name;
}

/** The first heading of a page, which names it better than its file does. */
export function titleOf(page: LambdaFile): string | null {
  const match = /^#\s+(.+?)\s*#*\s*$/m.exec(page.code);

  return match ? match[1] : null;
}
