import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * Publishing the source of a lambda. Seen in both views, so worded for
 * somebody who had the app built: no versions, no files - its code, what it
 * shows, what is written about it, and the changes it went through.
 */
export const openSource = {
  loading: 'Loading…',
  loadFailed: 'Whether the code is published could not be read.',
  hint: (tool: Node) => (
    <>
      A published lambda can be read, starred and downloaded by anybody on its page under Open source - every
      version of it, under the license you pick, and never the data it keeps. Only whoever holds the editor key can
      publish it or take it down. An agent can do the same with its {tool} tool.
    </>
  ),
  hintSimple:
    'Anybody can read how your app is made on its own page, and build on it under the license you pick - never with what it keeps. Only you can publish it or take it down.',
  open: 'Open the source page',
  switch: 'Publish the code of this app',
  publishedNow: (license: string) => `Published under ${license}. Anybody can read and download it.`,
  off: 'Off. Nobody sees the code until you publish it.',
  keptStars: (stars: number) => (stars === 1 ? 'Its star is kept for when you publish it again.' : `Its ${stars} stars are kept for when you publish it again.`),
  published: 'Published. Anybody can read the code now.',
  saved: 'Saved.',
  saveFailed: 'The code could not be published.',
  withdrawn: 'Taken down. Its page is gone.',
  withdrawFailed: 'It could not be taken down.',
  whatTitle: 'What is published',
  what: [
    'Its code - as it is now, and every earlier state of it',
    'Everything it shows: its pages, styles and pictures',
    'What is written about it: what it is for, and how it is tested',
    'Every change it went through, in a line each',
  ],
  neverTitle: 'What is never published',
  never: [
    'What it keeps: its records, what it saved, its keys and passwords',
    'What you asked for, in your own words',
    'Who uses it: its visitors and what they did',
    'The editor link',
  ],
  careful:
    'Everything in the code becomes public, the earlier states of it too. A password or key never belongs in the code - it belongs with the keys and passwords under Data, which are never published.',
  licenseLabel: 'License',
  licenseHint: 'What others may do with the code. MIT, the most common, lets anybody do nearly anything with it as long as your name stays on it.',
  readLicense: 'Read the license',
  authorLabel: 'Name in the license',
  optional: 'optional',
  authorPlaceholder: (key: string) => `The authors of ${key}`,
  authorHint: 'Your name or your organization, shown on the source page and in the license. Left empty, the license names the authors of this app.',
  publish: 'Publish',
  save: 'Save changes',
  allSaved: 'Everything is saved.',
  takeDown: 'Take it down',
  confirm: 'Take the code down?',
  confirmText:
    'Its page and its downloads go away at once. Whoever downloaded it already keeps it under the license it came with. Its stars are kept for when you publish it again.',
  keep: 'Keep it published',
  stars: (count: number) => (count === 1 ? '1 star' : `${count} stars`),
  /** Under the addresses in the sidebar, once it is published. */
  sidebar: 'Source code',
};
