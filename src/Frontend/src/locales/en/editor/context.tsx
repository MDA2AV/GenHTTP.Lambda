import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * What a version says about itself beside its program: its documentation
 * and its tests, in .lambda/. The simple view has the documentation only,
 * and of it only what the app is for - called "About" there.
 */
export const context = {
  docs: {
    title: 'Documentation',
    titleSimple: 'About your app',
    hint: 'What this app is, who it is for and why - and why it is built the way it is. Agents write it with every change and it is kept with each version, so an older version comes back with the documentation that was true of it.',
    hintSimple: 'What your app is for and why, as the agent understood it from what you asked. It keeps this up to date with every change.',
    inDraft: "This draft's documentation. It becomes your app's when the draft goes online.",
    pages: { product: 'Product', decisions: 'Decisions' },
    emptyTitle: 'Nothing written yet',
    emptyText: (code: (text: string) => Node) => (
      <>
        Agents write the documentation with their changes: what the app is, who it is for and why in{' '}
        {code('.lambda/docs/product.md')}, and why it is built the way it is in {code('decisions.md')}. It is part of
        the version, beside the code.
      </>
    ),
    emptySimpleTitle: 'Nothing written about your app yet',
    emptySimple: 'The agent can describe what your app is for and why, from what you asked for - it keeps the description up to date from then on.',
    ask: 'Ask the agent to write it',
    describe: 'Ask the agent to describe it',
    writePrompt: 'Write the documentation of this app: what it is, who it is for and why, and the technical decisions behind it.',
    describePrompt: 'Describe what this app is for and why, for me to read under About.',
    decisionsPrompt: 'Write down the technical decisions behind this app, and why they were made.',
    missingProduct: 'No product page yet',
    missingProductText: 'What the app is, who it is for, what people do with it and why - in the words of whoever asked for it.',
    missingDecisions: 'No decisions written down yet',
    missingDecisionsText: 'How the app is built and why: how it keeps its data, what it depends on, what was left out. What whoever changes it next needs to know.',
    correctText: 'The agent writes this from what you asked for, and keeps it up to date with every change. Something wrong or missing? Tell it.',
    correct: 'Tell the agent',
    correctPrompt: 'Correct the description of the app: ',
    placeholder: 'Explains why entries are kept for a year',
  },
  tests: {
    title: 'Tests',
    hint: 'How this app is tested automatically, and the scripts and data the tests use. Agents keep it up to date and run it before they call a change done. It is kept with each version.',
    inDraft: "This draft's tests. They become your app's when the draft goes online - run them against its preview first.",
    pages: { testing: 'How it is tested' },
    emptyTitle: 'No tests yet',
    emptyText: (code: (text: string) => Node) => (
      <>
        How the app is tested - what has to keep working, how to check it, and how to run the scripts for it - is
        written by agents in {code('.lambda/tests/README.md')}, with the scripts and the test data beside it.
      </>
    ),
    ask: 'Ask the agent to write tests',
    writePrompt: 'Write the tests of this app: what has to keep working and how to check it automatically, with a script to run against its preview.',
    missing: 'Not said yet how it is tested',
    missingText: 'What has to keep working, how each of it is checked, and how to run the scripts beside it.',
    placeholder: 'Checks that a full list refuses new entries',
  },
  files: 'Files',
  noFiles: 'No files beside the pages.',
  none: 'none',
  missingPill: 'Not written yet',
  changedIn: (version: number) => `Changed in version ${version}`,
  changedInDraft: 'Changed in this draft',
  showChanges: 'Show what changed',
  hideChanges: 'Hide what changed',
  noChanges: 'Nothing changed.',
  edit: 'Edit',
  olderVersion: 'A version never changes: a page is edited on the newest version, or in a draft.',
  writeIt: 'Write it yourself',
  askPage: 'Ask the agent to write it',
  editInCode: 'Open in the code',
  cancel: 'Cancel',
  save: 'Save',
  write: 'Write',
  preview: 'Preview',
  writeOrPreview: 'Write or preview',
  discard: 'Your changes to this page will be lost. Discard them?',
  reading: 'Reading…',
  readFailed: 'This could not be read.',
  saveFailed: 'That could not be saved.',
  savedDraft: 'Saved in the draft.',
  savedVersion: (version: number) => `Saved as version ${version}.`,
  savedOnline: (version: number) => `Saved as version ${version}, and online.`,
  savedNotOnline: (version: number) => `Saved as version ${version}, but it did not go online.`,
  saveTitle: 'Save as a new version',
  saveText: (newest: number) =>
    `A version never changes, so this page is saved as the next one - on top of version ${newest}, with everything else as it is.`,
  clash: (version: number) => `Version ${version} was saved since you began, and it changed this page too. Saving replaces that.`,
  alsoOnline: 'Put it online too',
  alsoOnlineNote: 'Only the documentation changes, so visitors see nothing new - but what is online stays the newest version.',
  skeleton: {
    product: '# Name of the app\n\nWhat it is, in a sentence or two.\n\n## Who it is for\n\n## What people do with it\n\n## Features, and why they are there\n\n## What it does not do\n',
    decisions: '# Decisions\n\n## A decision\n\nWhat was decided, why, and what a change has to keep in mind.\n',
    testing: '# How it is tested\n\nHow to run the tests, and against which address.\n\n## What has to keep working\n\n| Behaviour | Request | Expected |\n|---|---|---|\n| | | |\n',
  },
};
