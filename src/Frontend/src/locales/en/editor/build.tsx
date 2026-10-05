import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * What a version is built from, in the full view: the files whoever changes
 * the app runs a build tool on to make its assets or its code - its
 * development space - read here, and changed where it is built.
 */
export const build = {
  title: 'Build',
  hint: 'What the assets or the code of a version are built from: files that whoever changes the app - your agent, in a clone - runs a build tool on, kept with every version and never compiled or served. This platform builds nothing, so they are read here, not edited.',
  overview: 'Overview',
  files: 'Files',
  /** The line under the title, beside the version it is about. */
  scope: (version: number) =>
    `What version ${version} is built from - kept with it, never compiled or served, and built by whoever changes it, never here.`,
  scopeDraft: 'What this draft is built from - kept with it, never compiled or served, and built by whoever changes it, never here.',
  reading: 'Reading what it is built from…',
  readFailed: 'What it is built from could not be read.',

  emptyTitle: (version: number) => `Version ${version} keeps nothing it is built from`,
  emptyTitleDraft: 'This draft keeps nothing it is built from',
  emptyText: (code: (text: string) => Node) => (
    <>
      Where the assets or the code of a version are made by a build tool - compiled, bundled or generated - the files
      they are made from are kept here, with every version: the folder {code('dev/')} in a clone. Whoever changes the
      app runs the build where they work and saves both together; this platform builds nothing. What is written as it is
      served or compiled needs none.
    </>
  ),
  emptyHow: (code: (text: string) => Node) => (
    <>{code('AGENTS.md')} in a clone tells a coding agent how it is used.</>
  ),

  /** What the version changed here and in the program, against the one before - or a draft against what it began from. */
  inVersion: (version: number) => `In version ${version}`,
  inDraft: 'In this draft',
  comparedWith: (version: number) => `against version ${version}`,
  first: 'The first version that keeps it.',
  both: (here: number, program: number) =>
    `${here === 1 ? '1 file' : `${here} files`} changed here, and ${program === 1 ? '1 file' : `${program} files`} of the code and the assets.`,
  hereOnly: (here: number) =>
    `${here === 1 ? '1 file' : `${here} files`} changed here, and nothing of the code or the assets: if what changed is built into them, it was not built.`,
  programOnly: 'Nothing changed here.',
  unchanged: 'Nothing changed here, nor in the code or the assets.',
  showChanges: 'Show the changes',
  hideChanges: 'Hide the changes',
  noChanges: 'Nothing changed here.',

  readme: 'How it is built',
  noReadme: (code: (text: string) => Node) => (
    <>
      Nothing says how it is built. A {code('README.md')} at the top - the commands, and where the build goes - is
      what the next agent builds from.
    </>
  ),
  readOnly: 'Read only: it is changed where it is built.',
  noFiles: 'No files.',
};
