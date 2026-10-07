import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * The files of a version, in the full view: its code and its resources,
 * browsed in a tree and changed by hand.
 */
export const code = {
  title: 'Code',
  version: (version: number) => `version ${version}`,
  edited: ', edited',
  loadFailed: 'That version could not be loaded.',
  compiles: 'It compiles.',
  notYet: 'It does not compile yet.',
  checkFailed: 'The code could not be checked.',
  saved: (version: number | undefined) => `Saved as version ${version}.`,
  featureSaved: 'Saved in the draft.',
  featureLoadFailed: 'The draft could not be loaded.',
  previewOnline: 'Saved. Its preview shows it now.',
  previewRefused: 'Saved, but it does not compile yet, so the preview still shows the last version that did. See what the compiler said below.',
  isOnline: (version: number | undefined) => `Version ${version} is online.`,
  notOnline: 'It did not go online. See what the compiler said below.',
  failed: 'That did not work.',
  unchanged: 'Nothing has changed since the last save.',
  demo: 'A demo, so everything here is read only. Create a lambda of your own from it to change it.',
  /** The hint beside the title, on a version of the lambda. */
  hint: (b: (text: string) => Node) => (
    <>
      The files of one version. Its {b('code')} is the program and everything kept with it: the .cs files at the top
      are compiled, and every other file - its documentation, its tests, what a front end is built from - is kept with
      the version and never compiled or served. Its {b('resources')} - pages, scripts, styles, pictures, the database's
      migrations - are read and served while it runs, and public where the code serves them. Saving makes a new version
      and leaves what is online alone; to try a change first, start a draft. Ctrl-S saves, F12 goes to a declaration.
    </>
  ),
  /** The hint beside the title, in a draft. */
  hintFeature: (b: (text: string) => Node) => (
    <>
      The files of this draft: its {b('code')} - the .cs files at the top compiled, the rest kept with it - and its{' '}
      {b('resources')}, read and served while it runs. Saving keeps them in the draft and shows them at the draft's own
      address; your visitors see nothing of it until you put the draft online.
    </>
  ),
  inFeature: (name: string) => `in “${name}”`,
  changedElsewhere: 'This draft was saved elsewhere since you opened it - by the agent, perhaps. Load what is saved before saving here; your changes would not be saved over it.',
  readAgain: 'Load what is saved',
  newer: (version: number) => `Version ${version} is newer than the one open here.`,
  check: 'Check',
  save: 'Save',
  deploy: 'Deploy',
  deployPreviewTitle: 'Save, and show it at the draft\'s own address',
  binary: (size: string) => `Not text, so there is nothing to edit here. It weighs ${size}.`,
  saveAndDeploy: 'Save and deploy',
  saveVersion: 'Save a new version',
  fromOlder: (version: number, newest: number) =>
    `This starts from version ${version}, and version ${newest} is newer. Saving makes it the newest version, without what came after version ${version}.`,
  featureInstead: (start: (text: string) => Node) => (
    <>
      Want to try it first? {start('Save it as a draft instead')}: it gets an address of its own, and nothing goes online
      until you say so.
    </>
  ),
  cancel: 'Cancel',
  what: 'What does it change? Optional - it is shown in the history.',
  placeholder: 'Adds a contact form',
  goToDefinition: 'Go to definition',

  /** The version shown, picked from the pill under the title. */
  versionLabel: 'Version',
  shown: (version: number, online: boolean, newest: boolean) =>
    `Version ${version}${online ? ', online' : newest ? ', newest' : ''}`,
  optionOnline: ' (online)',
  /** Asked before another version is opened over what was typed and not saved. */
  switchUnsaved: 'What you changed here is not saved. Open the other version anyway?',
  noVersion: 'There is no version to show yet.',

  /** The tree of the files, beside the editor. */
  label: 'Files',
  /** The group of the files that are the program and what is kept with it. */
  codeGroup: 'Code',
  codeWhy: 'Never served. The .cs files at the top are compiled; the rest is kept with the version.',
  /** The group of the files read and served while the lambda runs. */
  resources: 'Resources',
  resourcesPublic: 'Public: this version serves them with Resources.',
  resourcesPrivate: 'Shipped with the version, but this version does not serve them.',
  noResources: 'None in this version.',
  count: (files: number) => (files === 1 ? '1 file' : `${files} files`),
  groupUsage: (files: string, size: string) => `${files}, ${size}`,
  /** Under the tree: what the version comes to, of what a version may. */
  usage: (used: string, of: string) => `This version comes to ${used} of the ${of} a version may have, its code and its resources together.`,
  scope: (data: (text: string) => Node) => (
    <>What the lambda keeps while it runs is the same for every version, and is under {data('Data')}.</>
  ),
  download: 'Download',
  /** The buttons in the header of each group of files. */
  newIn: (group: string) => `New file in ${group}`,
  uploadIn: (group: string) => `Upload into ${group}`,
  pick: 'Pick a file to see what is in it.',
};
