import type { LambdaFile } from '../api';
import { DEV, isAsset } from './written';

/**
 * What the development space of a version holds, read off its files the way
 * somebody reviewing it would read them: which projects there are, what each
 * is built with, what it installs, and where its build goes.
 *
 * Nothing here runs anything or claims more than the files say. A project is
 * a folder with a file a toolchain is known by - a package.json, a
 * Cargo.toml - and what is said about it comes from that file and the ones
 * beside it, so a project that keeps its build somewhere these do not look
 * is simply not said to.
 */

/** The toolchains a project is recognised by, and the file each is known by. */
export type ProjectKind = 'npm' | 'deno' | 'cargo' | 'go' | 'python' | 'dotnet' | 'php' | 'ruby' | 'maven' | 'gradle' | 'make';

/** The package managers of npm projects, by their lock files. */
export type Manager = 'npm' | 'pnpm' | 'yarn' | 'bun';

export interface Project {
  /** Its folder within the development space - '' for the space itself, 'web/' for one in web. */
  folder: string;
  kind: ProjectKind;
  /** What it calls itself, where its file says. */
  name?: string;
  /** The frameworks and tools it is built with, by name, the ones that say most first. */
  stack: string[];
  manager?: Manager;
  /** The command it is built with, as somebody would type it. */
  command?: string;
  /** What that command runs, as its file says. */
  script?: string;
  /** What it installs to run, by name and version range. */
  dependencies: [string, string][];
  /** What it installs to be built and checked. */
  tooling: [string, string][];
  /** Whether a lock file says exactly which versions to install - unknown for a toolchain that has none. */
  locked?: boolean;
  /** Whether a .gitignore keeps what it installs and builds out of a version. */
  ignored: boolean;
  /** The folder of the assets its build writes into, where its configuration says so. */
  into?: string;
  /**
   * What the page its build wrote refers to and the version does not have -
   * a build saved in part, its new files left behind.
   */
  missing: string[];
}

/** The file each toolchain is known by, the ones a front end is likeliest to be first. */
const MARKERS: [ProjectKind, (name: string) => boolean][] = [
  ['npm', (name) => name === 'package.json'],
  ['deno', (name) => name === 'deno.json' || name === 'deno.jsonc'],
  ['cargo', (name) => name === 'Cargo.toml'],
  ['go', (name) => name === 'go.mod'],
  ['python', (name) => name === 'pyproject.toml' || name === 'requirements.txt'],
  ['dotnet', (name) => /\.(cs|fs)proj$/.test(name)],
  ['php', (name) => name === 'composer.json'],
  ['ruby', (name) => name === 'Gemfile'],
  ['maven', (name) => name === 'pom.xml'],
  ['gradle', (name) => name === 'build.gradle' || name === 'build.gradle.kts'],
  ['make', (name) => name === 'Makefile'],
];

/** What says which versions to install, for the toolchains that have one. */
const LOCKS: Partial<Record<ProjectKind, string[]>> = {
  npm: ['package-lock.json', 'npm-shrinkwrap.json', 'pnpm-lock.yaml', 'yarn.lock', 'bun.lockb', 'bun.lock'],
  deno: ['deno.lock'],
  cargo: ['Cargo.lock'],
  go: ['go.sum'],
  php: ['composer.lock'],
  ruby: ['Gemfile.lock'],
};

/** The packages that say what a front end is, in the words their projects use, the framework first. */
const STACK: [string, string][] = [
  ['next', 'Next.js'],
  ['nuxt', 'Nuxt'],
  ['@sveltejs/kit', 'SvelteKit'],
  ['@remix-run/react', 'Remix'],
  ['astro', 'Astro'],
  ['@angular/core', 'Angular'],
  ['react', 'React'],
  ['preact', 'Preact'],
  ['vue', 'Vue'],
  ['svelte', 'Svelte'],
  ['solid-js', 'Solid'],
  ['lit', 'Lit'],
  ['alpinejs', 'Alpine.js'],
  ['htmx.org', 'htmx'],
  ['vite', 'Vite'],
  ['webpack', 'webpack'],
  ['esbuild', 'esbuild'],
  ['parcel', 'Parcel'],
  ['rollup', 'Rollup'],
  ['@rsbuild/core', 'Rsbuild'],
  ['typescript', 'TypeScript'],
  ['tailwindcss', 'Tailwind CSS'],
  ['sass', 'Sass'],
  ['less', 'Less'],
];

/** The most projects said, should a space hold what a build installed after all. */
const MOST = 12;

/**
 * The projects of a development space.
 *
 * @param files every file of the version or the draft, so where a build goes can be found among its assets
 */
export function projectsOf(files: LambdaFile[]): Project[] {
  const space = files.filter((file) => file.name.startsWith(DEV)).map((file) => ({ ...file, path: file.name.slice(DEV.length) }));

  const found: Project[] = [];
  const seen = new Set<string>();

  // the shallow ones first, so a space's own project leads
  const ordered = [...space].sort((a, b) => depth(a.path) - depth(b.path) || a.path.localeCompare(b.path));

  for (const file of ordered) {
    const folder = folderOf(file.path);
    const name = file.path.slice(folder.length);

    // what a build installed, should it be there, is not a project of the space
    if (seen.has(folder) || /(^|\/)node_modules\//.test(file.path)) {
      continue;
    }

    const kind = MARKERS.find(([, matches]) => matches(name))?.[0];

    // a Makefile beside a project's own file is that project's, not one of its own
    if (!kind || (kind === 'make' && space.some((other) => folderOf(other.path) === folder && MARKERS.some(([k, m]) => k !== 'make' && m(other.path.slice(folder.length)))))) {
      continue;
    }

    seen.add(folder);

    const above =(wanted: string) => ancestors(folder).some((at) => space.some((other) => other.path === `${at}${wanted}`));

    const project: Project = {
      folder,
      kind,
      stack: [],
      dependencies: [],
      tooling: [],
      locked: LOCKS[kind] ? LOCKS[kind]!.some(above) : undefined,
      ignored: above('.gitignore'),
      missing: [],
    };

    if (kind === 'npm') {
      describePackage(project, file.code, space.filter((other) => folderOf(other.path) === folder).map((other) => other.path.slice(folder.length)), above);
      project.into = buildsInto(folder, space, files);
      project.missing = project.into ? unresolved(files, project.into) : [];
    }

    if (kind === 'cargo' || kind === 'go' || kind === 'dotnet') {
      // what a project is called is in its file, which is enough to tell two apart
      project.name = kind === 'dotnet' ? name.replace(/\.(cs|fs)proj$/, '') : /^\s*(?:name\s*=\s*"([^"]+)"|module\s+(\S+))/m.exec(file.code)?.slice(1).find(Boolean);
    }

    found.push(project);

    if (found.length >= MOST) {
      break;
    }
  }

  return found;
}

/** Fills in what a package.json says: its name, what it is built with and how, and what it installs. */
function describePackage(project: Project, text: string, siblings: string[], above: (name: string) => boolean) {
  let manifest: {
    name?: unknown;
    scripts?: Record<string, unknown>;
    dependencies?: Record<string, unknown>;
    devDependencies?: Record<string, unknown>;
    packageManager?: unknown;
  };

  try {
    manifest = JSON.parse(text);
  } catch {
    return;
  }

  const entries = (record: unknown): [string, string][] =>
    record && typeof record === 'object'
      ? Object.entries(record as Record<string, unknown>).filter(([, version]) => typeof version === 'string').map(([name, version]) => [name, version as string])
      : [];

  project.name = typeof manifest.name === 'string' ? manifest.name : undefined;
  project.dependencies = entries(manifest.dependencies).sort(([a], [b]) => a.localeCompare(b));
  project.tooling = entries(manifest.devDependencies).sort(([a], [b]) => a.localeCompare(b));

  const all = new Set([...project.dependencies, ...project.tooling].map(([name]) => name));

  project.stack = STACK.filter(([pkg]) => all.has(pkg)).map(([, label]) => label);

  // a framework built on another says the other too: SvelteKit is Svelte, Next.js React
  if (project.stack.includes('SvelteKit')) project.stack = project.stack.filter((label) => label !== 'Svelte');
  if (project.stack.includes('Next.js') || project.stack.includes('Remix')) project.stack = project.stack.filter((label) => label !== 'React');
  if (project.stack.includes('Nuxt')) project.stack = project.stack.filter((label) => label !== 'Vue');

  const declared = typeof manifest.packageManager === 'string' ? manifest.packageManager.split('@')[0] : null;

  project.manager = (['pnpm', 'yarn', 'bun', 'npm'] as Manager[]).find((manager) => manager === declared)
    ?? (above('pnpm-lock.yaml') ? 'pnpm' : above('yarn.lock') ? 'yarn' : above('bun.lockb') || above('bun.lock') ? 'bun' : 'npm');

  const build = manifest.scripts?.build;

  if (typeof build === 'string') {
    project.script = build;
    project.command = { npm: 'npm run build', pnpm: 'pnpm build', yarn: 'yarn build', bun: 'bun run build' }[project.manager];
  }

  // a project without a lock file of its own may share one a workspace keeps above it
  if (!project.locked && siblings.some((name) => LOCKS.npm!.includes(name))) {
    project.locked = true;
  }
}

/**
 * Where a Vite project writes its build, as a folder of the assets - read
 * off its configuration, in either of the layouts it may have been set up
 * in: a clone, where it is dev/... and the assets are assets/, or the
 * version itself, where it is .lambda/dev/... and the assets are at the root.
 */
function buildsInto(folder: string, space: { path: string; code: string }[], files: LambdaFile[]): string | undefined {
  const config = space.find((file) => /^vite\.config\.(ts|js|mjs|mts|cjs|cts)$/.test(file.path.slice(folder.length)) && folderOf(file.path) === folder);

  const outDir = config && /\boutDir\s*:\s*(['"`])([^'"`]+)\1/.exec(config.code)?.[2];

  if (!outDir || outDir.startsWith('/')) {
    return undefined;
  }

  const clone = resolve(`dev/${folder}${outDir}`);

  if (clone?.startsWith('assets/')) {
    return `${clone.slice('assets/'.length)}/`;
  }

  const version = resolve(`${DEV}${folder}${outDir}`);

  if (version && !version.startsWith('.lambda/') && !version.startsWith('..')) {
    return `${version}/`;
  }

  // set up for neither: said only where the assets have such a folder
  const named = outDir.replace(/^(\.\.\/)+/, '').replace(/^assets\//, '');

  return files.some((file) => isAsset(file.name) && file.name.startsWith(`${named}/`)) ? `${named}/` : undefined;
}

/**
 * The files the page a build wrote refers to - its scripts, its styles, its
 * pictures - that are not among the assets, as their names among them.
 *
 * A bundler names what it writes after what is in it, so a build writes new
 * names every time: saved without them, the page asks for files nobody has.
 * Only what the page refers to relatively is looked for, which is what a
 * build set up for this platform writes.
 */
function unresolved(files: LambdaFile[], folder: string): string[] {
  const page = files.find((file) => file.name === `${folder}index.html` && file.encoding !== 'base64');

  if (!page) {
    return [];
  }

  const assets = new Set(files.filter((file) => isAsset(file.name)).map((file) => file.name));
  const missing = new Set<string>();

  for (const [, target] of page.code.matchAll(/\b(?:src|href)\s*=\s*["']([^"'#?]+)["']/g)) {
    if (/^([a-z]+:|\/|#)/i.test(target) || !/\.[a-z0-9]+$/i.test(target)) {
      continue;
    }

    const name = resolve(`${folder}${target}`);

    if (name && !assets.has(name)) {
      missing.add(name);
    }
  }

  return [...missing];
}

/** A path with its . and .. taken out, or nothing where it leaves the root. */
function resolve(path: string): string | undefined {
  const parts: string[] = [];

  for (const part of path.split('/')) {
    if (part === '' || part === '.') {
      continue;
    }

    if (part === '..') {
      if (parts.length === 0) {
        return undefined;
      }

      parts.pop();
    } else {
      parts.push(part);
    }
  }

  return parts.join('/');
}

const depth = (path: string) => path.split('/').length;

/** The folder a path is in, with its slash: 'web/' for 'web/package.json', '' at the top. */
const folderOf = (path: string) => path.slice(0, path.lastIndexOf('/') + 1);

/** A folder and every folder it is in, itself first: 'web/app/', 'web/', ''. */
function ancestors(folder: string): string[] {
  const result = [folder];
  let at = folder;

  while (at !== '') {
    at = folderOf(at.slice(0, -1));
    result.push(at);
  }

  return result;
}

/** The files of the assets below a folder, and what they weigh. */
export function assetsIn(files: LambdaFile[], folder: string, sizeOf: (file: LambdaFile) => number): { files: number; bytes: number } {
  const inside = files.filter((file) => isAsset(file.name) && file.name.startsWith(folder));

  return { files: inside.length, bytes: inside.reduce((total, file) => total + sizeOf(file), 0) };
}

/** The names that differ between two sets of files - added, removed or changed - without comparing lines. */
export function changedNames(before: LambdaFile[], after: LambdaFile[]): string[] {
  const old = new Map(before.map((file) => [file.name, file.code]));
  const now = new Map(after.map((file) => [file.name, file.code]));

  return [...new Set([...old.keys(), ...now.keys()])].filter((name) => old.get(name) !== now.get(name));
}
