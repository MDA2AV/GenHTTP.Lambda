import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * The words of the pages of the published sources, /source and every project
 * below it, in English. Fetched with those pages, since most visitors never
 * open one; every other language's catalog is typed as this one.
 *
 * Read by developers and by anybody else who followed a link, so the words
 * are plain: a project is a lambda somebody built here, a version is what it
 * was at one save, and a change is the line that version says about itself.
 */
export const source = {
  /** The frame of these pages. */
  shell: {
    section: 'Open source',
    home: 'GenHTTP Lambda, the start page',
  },

  /** What a lambda is, for somebody who arrived at the code of one. */
  lambda: {
    label: 'What is a lambda?',
    text: 'A web app on GenHTTP Lambda: somebody says what they want, an AI agent writes it in C#, and it is online at an address of its own within minutes - every version kept, with what it changed.',
    build: 'Build your own',
  },

  catalog: {
    eyebrow: 'Open source',
    title: 'See how the apps here are made',
    intro:
      'Lambdas whose owners published their code: every version, what it changed, its documentation and its tests. Read it here, or download a project that runs wherever .NET does.',
    searchLabel: 'Search the projects',
    searchPlaceholder: 'Search by name or what it does',
    orderLabel: 'Order',
    orders: {
      stars: 'Most stars',
      updated: 'Recently changed',
      published: 'Newly published',
    },
    /** Written for one, and for more. */
    counted: (total: number) => (total === 1 ? '1 project' : `${total} projects`),
    failed: 'The projects could not be loaded.',
    loadingMore: 'Loading more…',
    showMore: 'Show more',
    nothingTitle: 'Nothing published yet',
    nothing: (tab: (text: string) => Node) => (
      <>
        Built something others could learn from? Open its control center, choose {tab('Open source')}, pick a
        license, and its code appears here.
      </>
    ),
    noMatchTitle: 'Nothing matches that',
    noMatch: (query: string) => `No published project mentions “${query}”.`,
    clear: 'Show every project',
    yoursTitle: 'Publish yours',
    yours: (tab: (text: string) => Node) => (
      <>
        Open the control center of your lambda and choose {tab('Open source')}, or ask the agent that built it to
        publish it. Only whoever holds the editor key can, under the license they pick - and what the app keeps,
        its records, files and keys, is never part of it.
      </>
    ),
    build: 'Build something',
    online: 'Online',
    offline: 'Offline',
    changed: (ago: string) => `changed ${ago}`,
    stars: (count: number) => (count === 1 ? '1 star' : `${count} stars`),
  },

  project: {
    loading: 'Loading the source…',
    failed: 'The source could not be loaded.',
    missingTitle: 'There is no published source here',
    missing: 'Its owner may have taken it down, or there has never been a lambda at this address.',
    all: 'All projects',
    by: (name: string) => `by ${name}`,
    versions: (count: number) => (count === 1 ? '1 version' : `${count} versions`),
    onlineAt: (address: Node) => <>Online at {address}</>,
    offline: 'Offline right now',
    openApp: 'Open the app',
    opens: (address: string) => `Opens ${address} in a new tab`,
    published: (ago: string) => `Published ${ago}`,
    changed: (ago: string) => `Changed ${ago}`,
    picture: (name: string) => `${name}, as it looks`,
    tabsLabel: 'What to read',
    tabs: {
      code: 'Code',
      docs: 'Documentation',
      tests: 'Tests',
      changes: 'Changes',
    },
  },

  /** Which version is being read. */
  versions: {
    label: 'Version',
    choose: 'Read another version',
    newest: 'newest',
    online: 'online',
    older: (version: number, ago: string, newest: number) =>
      `You are reading version ${version}, saved ${ago}. The newest is version ${newest}.`,
    toNewest: 'Read the newest',
    noChange: 'No note about what it changed',
  },

  star: {
    star: 'Star',
    add: 'Star this project',
    remove: 'Take your star back',
    count: (count: number) => (count === 1 ? '1 star' : `${count} stars`),
    failed: 'The star could not be saved.',
  },

  /** The menu beside the star, as a repository page has it: cloning with git first, then downloading. */
  clone: {
    button: 'Code',
    title: 'Clone with git',
    /** Says which versions a clone holds, as tags, and which is main. */
    what: (oldest: number, newest: number) =>
      oldest === newest
        ? `Its version is the commit of main, tagged v${newest}.`
        : `Every version comes along as a commit of main, tagged v${oldest} to v${newest} - main is the newest.`,
    readOnly:
      'Read only. To build on it, start a lambda of your own and bring these files over - AGENTS.md in the clone says how, and its license what you may do.',
  },

  download: {
    title: (version: number) => `Version ${version} as a project`,
    what:
      'A .NET 10 project with a Dockerfile, its documentation, its tests and its license. What the app keeps - its records, the files it saved, its keys - is not part of it.',
    zip: 'Download ZIP',
    preparing: 'Preparing the project…',
    slow: 'The first download of a version is packed while you wait.',
    failed: 'The project could not be prepared. Try again in a moment.',
    run: 'Run it',
    local: 'With the .NET 10 SDK:',
    container: 'Or in a container:',
    agent: 'Or hand the folder to your coding agent and build on it - keeping to its license.',
    copy: 'Copy',
    copied: 'Copied',
  },

  tree: {
    label: 'Files',
    files: (count: number) => (count === 1 ? '1 file' : `${count} files`),
    packing: 'Packing this version…',
    packingSlow: 'A version is packed the first time anybody reads it, which takes a moment for a large one.',
    failed: 'The files of this version could not be loaded.',
    legend: 'What is what',
    /** What each kind of file is, by where the project keeps it. */
    kinds: {
      code: 'The lambda’s own code',
      asset: 'What it serves: pages, scripts, styles, pictures - and its database migrations',
      docs: 'What it is, and why it is built this way',
      tests: 'How it is tested',
      dev: 'What its assets are built from: the project of its front end',
      platform: 'What stands in for the platform',
      project: 'The host, the build, the container and the license',
    },
    /** The same, in a word, beside the legend's dots. */
    short: {
      code: 'Code',
      asset: 'Served',
      docs: 'Docs',
      tests: 'Tests',
      dev: 'Dev',
      platform: 'Platform',
      project: 'Project',
    },
  },

  file: {
    loading: 'Loading…',
    failed: 'This file could not be loaded.',
    missing: (path: string) => `There is no ${path} in this version.`,
    binary: 'This file is not text.',
    tooLarge: 'This file is too long to show here.',
    download: 'Download',
    raw: 'Raw',
    rawTitle: 'Open the file as it is',
    copy: 'Copy',
    copied: 'Copied',
    lines: (count: number) => (count === 1 ? '1 line' : `${count} lines`),
    plain: 'Shown without colours: it is long.',
    line: (line: number) => `Line ${line}`,
  },

  docs: {
    pages: 'Pages',
    product: 'What it is',
    decisions: 'Decisions',
    loading: 'Loading…',
    failed: 'This page could not be loaded.',
    noneTitle: 'Nothing is written about this version',
    none: 'Its documentation would be in docs/: what the app is, who it is for, and why it is built the way it is.',
  },

  tests: {
    files: 'Scripts and data',
    noneTitle: 'This version says nothing about its tests',
    none: 'How it is tested would be in tests/README.md, with the scripts it runs beside it.',
  },

  changes: {
    title: 'Every version, newest first',
    intro: 'A version never changes once it is saved. Each one says in a line what it changed.',
    agent: 'Written by an agent',
    online: 'online',
    browse: 'Read the code',
    noChange: 'No note',
  },

  /** What each license lets others do, in a sentence, by its SPDX identifier. */
  licenses: {
    MIT: 'Anybody may use, change and pass it on, in anything, as long as the license and the copyright notice stay with it.',
    'Apache-2.0': 'Like MIT, with a patent license from everybody who contributed, and changes marked as changes.',
    'BSD-3-Clause': 'Like MIT, and nobody may use the authors’ names to promote what they made of it.',
    'MPL-2.0': 'Changes to these files stay under the same license; they may be combined with code under any other.',
    'GPL-3.0-or-later': 'Whoever passes it on, changed or not, passes on its source under the same license.',
    'AGPL-3.0-or-later': 'Like the GPL, and offering a changed copy to people over the network counts as passing it on.',
    Unlicense: 'Given to the public domain: anybody may do anything with it, without conditions.',
  },

  kinds: {
    Permissive: 'Permissive',
    Copyleft: 'Copyleft',
    PublicDomain: 'Public domain',
  },
};

export type SourceMessages = typeof source;
