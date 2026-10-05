import type { ReactNode } from 'react';

type Node = ReactNode;

export const code = {
  title: 'Code',
  version: (version: number) => `version ${version}`,
  edited: ', edited',
  online: ', online',
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
  demo: 'A demo, so everything here is read only. Create a lambda of your own from it to change it. ',
  edit: 'Edit the code by hand. Saving makes a new version and leaves what is online alone; deploying puts it online. To try a change first, start a draft. ',
  editFeature:
    'The code of this draft. Saving keeps it in the draft and shows it at the draft\'s own address; your visitors see nothing of it until you put the draft online. ',
  inFeature: (name: string) => `in “${name}”`,
  changedElsewhere: 'This draft was saved elsewhere since you opened it - by the agent, perhaps. Load what is saved before saving here; your changes would not be saved over it.',
  readAgain: 'Load what is saved',
  files: (entry: Node, cs: Node, context: Node) => (
    <>
      {entry} returns what gets served, other {cs} files hold types, and any other file is served as it is - except
      what is in {context}: the documentation, the tests and what it is built from, never compiled or served. Ctrl-S
      saves, F12 goes to a declaration.
    </>
  ),
  newer: (version: number) => ` Version ${version} is newer than the one open here.`,
  check: 'Check',
  save: 'Save',
  deploy: 'Deploy',
  deployPreviewTitle: 'Save, and show it at the draft\'s own address',
  binary: (size: number) => `Not text, so there is nothing to edit. It is served as it is and weighs ${size} kB.`,
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
};
