import type { LambdaFile } from '../api';

/**
 * What a version keeps beside its program, under .lambda/: what is written
 * about it - its documentation and its tests - and what its assets or code
 * are built from, its development space.
 *
 * The same rule the server applies (LambdaSource): a file under .lambda/ is
 * never code and never an asset, whatever it is called - a test written in
 * C# is not compiled, and a page of documentation is not served. It belongs
 * to the version like any other file, so it is saved, compared, rolled back
 * and merged with the program it describes.
 */

/** The folder of everything kept beside the program. */
export const BESIDE = '.lambda/';

export const DOCS = '.lambda/docs/';

export const TESTS = '.lambda/tests/';

/** What the app is, who it is for and why - the one page the simple view shows. */
export const PRODUCT = '.lambda/docs/product.md';

/** The technical decisions, and why they were made. */
export const DECISIONS = '.lambda/docs/decisions.md';

/** How the app is tested automatically. */
export const TESTING = '.lambda/tests/README.md';

/**
 * The development space: the files the assets or the code are built from
 * with a build tool, whatever that tool works from - built by whoever
 * changes them, never by the platform. The editor calls it Build.
 */
export const DEV = '.lambda/dev/';

/** How the development space is built, and where the build goes. */
export const DEV_README = '.lambda/dev/README.md';

/** Whether a file is kept beside the program rather than being part of it. */
export const isBeside = (name: string) => name.startsWith(BESIDE);

export const isDevelopment = (name: string) => name.startsWith(DEV);

/** Whether a file is documentation or a test. */
export const isContext = (name: string) => isBeside(name) && !isDevelopment(name);

export const isCode = (name: string) => name.toLowerCase().endsWith('.cs') && !isBeside(name);

export const isAsset = (name: string) => !isCode(name) && !isBeside(name);

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
