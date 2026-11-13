
/**
 * Drafts - features, to the code: a copy of the lambda a change is tried on,
 * put online once it is right. Said without merging, bases or branches,
 * since the people reading it built an app, not a repository.
 */
export const features = {
  hint:
    'A draft is a copy of your app to try a change on before anybody sees it, with an address and test data of its own. Put it online once it is right; until then, your visitors keep getting what is online now.',
  newFeature: 'New draft',
  full: (limit: number) => `There are ${limit} drafts already, which is all there may be. Put one online or discard it first.`,
  emptyTitle: 'No drafts',
  emptyText:
    'A draft is a copy of your app to try a change on before it goes online. When the agent leaves a change for you to try, you find it here.',
  start: 'New draft',
  askAgentNew: 'Ask the agent for a change',
  noChange: 'Nothing said about what it changes yet',
  behindTitle: 'Your app changed since this draft began',
  behind: (_newest: number) => 'out of date',
  /** Beside the branch a draft is in the lambda's git repository, in the full view. */
  branchTitle: 'The branch this draft is in the git repository of the app',
  previewOnline: 'preview running',
  previewOutdated: 'preview shows an earlier save',
  previewOffline: 'preview not running',
  changed: 'changed',
  openPreview: 'Try it',
  openPreviewTitle: 'Open its preview in a new tab',
  count: (open: number, limit: number) => `${open} of ${limit} drafts`,
  loading: 'Loading the draft…',
  readFailed: 'The draft could not be read.',

  newTitle: 'New draft',
  newText:
    'A copy of your app and its data, with an address of its own. Change it and try it there - your visitors see nothing of it until you put it online.',
  newTextFiles:
    'What you typed goes into the draft instead of becoming a version, so you can try it at its own address before it goes online.',
  name: 'Name',
  namePlaceholder: 'Leaderboard',
  wanted: 'What should it do?',
  wantedPlaceholder: 'Optional. Keep the ten best scores and show them after every game.',
  olderBase: (newest: number) =>
    `This starts from an older version, so it is out of date from the start: before it can go online, what changed up to version ${newest} has to be brought in.`,
  create: 'Start the draft',
  createFailed: 'The draft could not be started.',
  retry: 'Try again',
  madeNotSaved: (name: string) =>
    `The draft “${name}” is started, but what you typed could not be put into it yet. Try again, or close this and find it under Drafts.`,
  created: (name: string) => `The draft “${name}” is started.`,
  cancel: 'Cancel',

  featureHint:
    'A copy of your app to try this change on. Its preview has an address and test data of its own, so your visitors see none of it until you put it online.',
  askAgent: 'Ask the agent',
  askCatchUp: 'Ask the agent to bring it up to date',
  catchUp: 'Bring this draft up to date with the newest version of the app, and keep what it changes.',
  editCode: 'Edit the code',
  deployPreview: 'Start the preview',
  updatePreview: 'Update the preview',
  previewDeployed: 'The preview is running.',
  previewFailed: 'The preview could not be started.',
  previewStopped: 'The preview is stopped.',
  previewRejected: 'The preview did not change',
  previewNotCompiling: 'It does not compile, so the preview still shows the last version that did.',
  started: 'Started',
  changes: (_version: number) => 'Changed files',
  noChanges: (_version: number) => 'Nothing is changed yet.',
  editNotes: 'Name and notes',
  what: 'What does it change?',
  whatPlaceholder: 'Adds a leaderboard that keeps the ten best scores',
  missed: (_from: number, _to: number) => 'What changed in your app since it began',
  missedNothing: 'Nothing in the files.',

  behindText: (_base: number, newest: number) =>
    `Version ${newest} of your app was saved after this draft began. Putting the draft online now would undo what that changed, so it has to be brought up to date first - the agent can do that for you.`,
  moveBase: 'Mark as up to date',
  close: 'Close',
  mergeTitle: (name: string) => `Put “${name}” online`,
  mergeTitleShort: 'Make it the new version of your app, and put it online',
  leaks: (path: string, files: string) =>
    `${files} link to ${path}, which is your live app. From the preview, those links read and change its real data instead of the test data. Ask the agent to link without that part ("api/items").`,
  mergeButton: 'Put online',
  saveFirst: 'Save your changes first: the preview and putting it online use what is saved.',
  mergeAndDeploy: (_version: number) => 'Put online',
  mergeText: (version: number) =>
    `It becomes version ${version} of your app and goes online. Your app's data stays as it is.`,
  deployTooNote: (active: number) => `Version ${active} stays a click away in the versions.`,
  deployTooOffline: 'Your app is offline now; this puts it online.',
  notCompiling: 'It does not compile, so it did not go online. Fix it in the draft first.',
  mergeFailed: 'The draft could not be put online.',
  merged: (version: number | string) => `Saved as version ${version}.`,
  mergedOnline: (version: number | string) => `Version ${version} is online.`,

  notesTitle: 'Name and notes',
  save: 'Save',
  saveFailed: 'That could not be saved.',

  baseTitle: 'Mark it as up to date?',
  baseText: (_base: number) =>
    'Only a draft that holds what the newest version changed can go online without undoing it. If those changes are in this draft now - brought in by you or by the agent - mark it as up to date.',
  moveTo: (_version: number) => 'Mark as up to date',
  baseWarning: 'Nothing checks this. If the changes are not in the draft, putting it online undoes them.',

  deleteTitle: (name: string) => `Discard “${name}”?`,
  deleteText:
    'Its code, its preview and its test data are deleted for good. Your app and its versions are not touched.',
  keep: 'Keep it',
  deleteForGood: 'Discard',
  deleteFailed: 'The draft could not be discarded.',
  deleted: (name: string) => `The draft “${name}” is discarded.`,

  all: 'All drafts',
  actions: 'More for this draft',
  download: 'Download as a zip',
  stopPreview: 'Stop the preview',
  delete: 'Discard this draft',
  viewsLabel: 'The draft',
  views: {
    overview: 'Draft',
    docs: 'Documentation',
    code: 'Code',
    tests: 'Tests',
    data: 'Test data',
    logs: 'Logs',
  },
  missingTitle: 'This draft is not there any more',
  missingText: 'It was put online or discarded. The versions show what became of it.',
};
