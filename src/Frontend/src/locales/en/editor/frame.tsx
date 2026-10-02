import type { ReactNode } from 'react';

type Node = ReactNode;

/** The frame around the sections: the sidebar, its menu and its dialogs. */
export const frame = {
  title: 'Editor',
  sections: {
    overview: 'Overview',
    docs: 'Documentation',
    tests: 'Tests',
    change: 'Change',
    features: 'Drafts',
    showcase: 'Showcase',
    source: 'Open source',
    domain: 'Domain',
    files: 'Files',
    data: 'Data',
    versions: 'Versions',
    history: 'History',
    deployments: 'Deployments',
    stats: 'Stats',
    logs: 'Logs',
    code: 'Code',
  },
  sectionsLabel: 'Sections',
  /** The groups the sections of the full view are gathered in, under the overview and the documentation. */
  groups: {
    build: 'Build',
    program: 'Program and data',
    run: 'Run',
    sharing: 'Sharing',
  },
  loadFailed: 'This lambda could not be loaded.',
  online: (version: number | string) => `Version ${version} is online.`,
  deployFailed: 'The lambda could not be deployed.',
  offline: 'Taken offline. The code is still here.',
  offlineFailed: 'The lambda could not be taken offline.',
  leave: 'Your unsaved changes will be lost. Leave anyway?',
  nothingTitle: 'This link does not open anything',
  createNew: 'Create a new lambda',
  loading: 'Loading your lambda…',
  moreActions: 'More actions',
  redeploy: (version: number) => `Redeploy version ${version}`,
  takeOffline: 'Take offline',
  copyLink: 'Copy the link',
  copyPrivate: 'Copy the private link',
  privateLink: 'Anyone with this link can change the lambda. Keep it private.',
  rename: 'Change the address',
  download: 'Download as a .NET project',
  delete: 'Delete this lambda',
  deploy: (version: number) => `Deploy version ${version}`,
  problems: 'Something went wrong recently',
  demoTitle: 'A demo, kept online by this installation and read only.',
  demo: (start: (text: string) => Node) => (
    <>
      Read its code, its history, what it stores and its logs - that is what it is here for. To change it,{' '}
      {start('start a lambda of your own from it')}.
    </>
  ),
  keep: 'Keep this link. It is the only way back into this lambda.',
  gotIt: 'Got it',
  rejected: (version: number) => `Version ${version} did not go online`,
  refused: 'The deployment was refused',
  openCode: 'Open the code',
  close: 'Close',
  notCompiling: 'It does not compile. Whatever was online before is still online.',
  moved: (path: string) => `Now at ${path}.`,
  deleteTitle: 'Delete this lambda?',
  cancel: 'Cancel',
  deleteForGood: 'Delete for good',
  deleteFailed: 'The lambda could not be deleted.',
  deleteText: (key: Node) => (
    <>Every version, all of its data, its history and the address {key} go with it. This cannot be undone.</>
  ),
  openInTab: 'Open in a new tab',
  open: (address: string) => `Open ${address} in a new tab`,
  copyAddress: 'Copy the address',
  renameFailed: 'The address could not be changed.',
  moveIt: 'Move it',
  renameText: 'The old address stops working right away, so update anything that links to it.',
  changing: 'The agent is changing this lambda',
  waiting: 'A change is waiting for the agent',
  follow: 'Follow it',
  changeRunning: 'A change is under way',
  changeEnded: 'A change has ended',
};
