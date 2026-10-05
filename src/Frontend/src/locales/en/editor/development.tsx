import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * The development space of a version, in the full view: what its assets are
 * built from - the project of a front end - read here, and changed where it
 * is built, by whoever builds it.
 */
export const development = {
  title: 'Development space',
  hint: 'What the assets of a version are built from, where a toolchain builds them: the project of its front end, with its sources, its configuration and its lock file. It is kept with every version and never compiled or served. Whoever changes it - your agent, in a clone - builds it where they work and saves it together with what it built: this platform builds nothing. So it is read here, not edited.',
  overview: 'Overview',
  files: 'Files',
  /** The line under the title, beside the version it is about. */
  scope: (version: number) =>
    `What the assets of version ${version} are built from - kept with it, never compiled or served, and built by whoever changes it, never here.`,
  scopeDraft: 'What the assets of this draft are built from - kept with it, never compiled or served, and built by whoever changes it, never here.',
  reading: 'Reading the development space…',
  readFailed: 'The development space could not be read.',

  emptyTitle: (version: number) => `No development space in version ${version}`,
  emptyTitleDraft: 'No development space in this draft',
  emptyText: (code: (text: string) => Node) => (
    <>
      Where a front end is built with a toolchain - React, Vue or Svelte with Vite, TypeScript, Tailwind - its project
      is kept here, with every version: what the assets are built from. Your agent builds it where it works and saves
      the sources together with what they built - in a clone it is the folder {code('dev/')}. A front end of plain HTML,
      CSS and JavaScript needs none.
    </>
  ),
  emptyHow: (code: (text: string) => Node) => (
    <>{code('AGENTS.md')} in a clone tells a coding agent how to set one up.</>
  ),

  /** The projects found in the space - a folder with a package.json, a Cargo.toml and the like. */
  projects: 'Projects',
  /** A project at the top of the space rather than in a folder of its own. */
  atTheTop: 'the development space itself',
  kinds: {
    npm: 'npm',
    deno: 'Deno',
    cargo: 'Rust',
    go: 'Go',
    python: 'Python',
    dotnet: '.NET',
    php: 'PHP',
    ruby: 'Ruby',
    maven: 'Maven',
    gradle: 'Gradle',
    make: 'Make',
  },
  builtWith: 'Built with',
  build: 'Build',
  noBuild: 'No build script in its package.json.',
  into: 'Builds into',
  intoAssets: (folder: Node, files: number, size: string) => (
    <>
      {folder} of the assets - {files === 1 ? '1 file' : `${files} files`}, {size} in this version
    </>
  ),
  intoNothing: (folder: Node) => <>{folder} of the assets - which holds nothing in this version</>,
  packages: 'Packages',
  packagesCount: (runtime: number, tooling: number) =>
    `${runtime === 1 ? '1 to run' : `${runtime} to run`}, ${tooling === 1 ? '1 to build' : `${tooling} to build`}`,
  showPackages: 'Show them',
  hidePackages: 'Hide them',
  runtime: 'To run',
  tooling: 'To build',
  /** Said where the page a build wrote refers to files the version does not have: a build saved in part. */
  missing: (page: Node, files: string[]) => (
    <>
      {page} refers to {files.length === 1 ? 'a file' : `${files.length} files`} that {files.length === 1 ? 'is' : 'are'} not
      among the assets ({files.slice(0, 3).join(', ')}{files.length > 3 ? ', …' : ''}): what the build wrote was not
      saved whole, and the page does not load.
    </>
  ),
  noLock: 'No lock file: the next build may install other versions of its packages than the last one did.',
  noIgnore: 'No .gitignore: what its toolchain installs and builds can end up in a version.',

  /** What the version changed in the space, against the one before - or a draft against what it began from. */
  inVersion: (version: number) => `In version ${version}`,
  inDraft: 'In this draft',
  comparedWith: (version: number) => `against version ${version}`,
  first: 'The first version that has it.',
  both: (here: number, assets: number) =>
    `${here === 1 ? '1 file' : `${here} files`} changed here, and ${assets === 1 ? '1 file' : `${assets} files`} of the assets.`,
  hereOnly: (here: number) =>
    `${here === 1 ? '1 file' : `${here} files`} changed here, and none of the assets: unless the change needed no build, visitors get what they got before.`,
  builtOnly: (folder: Node) => (
    <>What is built into {folder} changed, and nothing here did: a change made in what the build wrote is undone by the next build.</>
  ),
  assetsOnly: 'Nothing changed here.',
  unchanged: 'Nothing changed here or in the assets.',
  showChanges: 'Show the changes',
  hideChanges: 'Hide the changes',
  noChanges: 'Nothing changed here.',

  readme: 'How it is built',
  noReadme: (code: (text: string) => Node) => (
    <>
      Nothing says how it is built. A {code('README.md')} at the top of the development space - the commands, and
      where the build goes - is what the next agent builds from.
    </>
  ),
  readOnly: 'Read only: it is changed where it is built.',
  noFiles: 'No files.',
};
