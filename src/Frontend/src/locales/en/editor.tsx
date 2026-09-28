import type { ReactNode } from 'react';

import { SHARED } from '../../control/words';

type Node = ReactNode;

/**
 * The words of the editor in English, fetched with the editor. Every other
 * language's catalog is typed as this one.
 */
export const editor = {
  /** What the parts shared with the administration console say. */
  shared: SHARED,

  /** The frame around the sections: the sidebar, its menu and its dialogs. */
  frame: {
    title: 'Editor',
    sections: {
      overview: 'Overview',
      change: 'Change',
      features: 'Features',
      showcase: 'Showcase',
      domain: 'Domain',
      files: 'Files',
      data: 'Data',
      versions: 'Versions',
      deployments: 'Deployments',
      stats: 'Stats',
      logs: 'Logs',
      code: 'Code',
    },
    sectionsLabel: 'Sections',
    loadFailed: 'This lambda could not be loaded.',
    online: (version: number | string) => `Version ${version} is online.`,
    deployFailed: 'The lambda could not be deployed.',
    offline: 'Taken offline. The code is still here.',
    offlineFailed: 'The lambda could not be taken offline.',
    leave: 'Your unsaved changes in the code will be lost. Leave anyway?',
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
  },

  /** The agent of the installation, changing the lambda for its owner. */
  change: {
    hint: 'Say what should be different, and the agent running on this server does it. It works in a feature - a copy of the lambda with an address of its own - so your visitors see nothing until it is done. Then it becomes the next version and goes online, or waits in its feature for you to try it first.',
    reading: 'Asking the agent…',
    readFailed: 'The agent could not be asked.',
    label: 'What should be different?',
    placeholder: 'Keep the ten best scores instead of five, add a dark mode, …',
    placeholderNext: 'What else should change?',
    send: 'Make the change',
    sending: 'Asking…',
    goOnline: 'Put it online when it is done',
    goOnlineOn: 'Once it works in its feature, the agent merges it into the next version and puts that online. Until then, what is online now stays.',
    goOnlineOff: 'The agent leaves it in its feature. Try it at the feature\'s own address, and merge it when you are happy with it.',
    where: 'Work in',
    whereTitle: 'A new feature, or one that is open to go on with',
    newFeature: 'A new feature',
    full: (limit: number) =>
      `This lambda has ${limit} features open, which is all it may have. Pick one to go on with, or merge or delete one first.`,
    password: 'Password',
    fable: 'Fable takes its time: no clock and no step limit, in a lane of its own.',
    left: (left: number, perDay: number) => `${left} of ${perDay} left today`,
    leftTitle: 'The build page and this section share one allowance a day.',
    noneLeft: (time: string) => `That was the last one for today. There are more from ${time}.`,
    ideas: ['Make it work well on a phone', 'Add a dark mode', 'Make it look more polished'],
    fixLog: 'Fix the errors in the log',
    how: [
      { title: 'It reads what is there', text: 'The code, and what earlier versions were asked for - so what works keeps working.' },
      { title: 'It works in a feature', text: 'A copy with its own address and its own data: it makes the change there and tries it, while you watch every step.' },
      { title: 'It puts it online', text: 'As the next version - or leaves it for you to try first. The version before stays a click away.' },
    ],
    asked: 'You asked',
    goesOnline: 'goes online when done',
    review: 'left in its feature for you to try',
    inFeature: (name: string) => `in the feature “${name}”`,
    queued: (ahead: number) =>
      ahead === 1 ? 'Waiting - one job is ahead of this one.' : `Waiting - ${ahead} jobs are ahead of this one.`,
    starting: 'Starting…',
    working: 'Working on it',
    leaveOpen: 'It carries on if you open another section or close this page.',
    stop: 'Stop',
    stopping: 'Stopping…',
    stopTitle: 'Stop this change?',
    stopText:
      'What it has saved so far stays - in its feature, or as a version. What is online stays online, unless it has already merged the change and put it there.',
    keepGoing: 'Keep going',
    stopIt: 'Stop it',
    log: 'What it did',
    took: (time: string) => `took ${time}`,
    steps: {
      guide: 'Reading the platform guide',
      demos: 'Looking at the demos',
      read: 'Reading the code',
      readFile: (file: Node) => <>Reading {file}</>,
      logs: 'Reading the log',
      logsPreview: 'Reading what the preview logged',
      readFeature: 'Reading the feature',
      create: 'Creating a lambda',
      feature: 'Starting a feature',
      featureStarted: (name: Node) => <>Started the feature {name}</>,
      update: 'Updating the feature\'s notes',
      rebase: (version: number) => `Basing the feature on version ${version}`,
      merge: 'Merging the feature',
      discard: 'Deleting a feature',
      write: (files: Node) => <>Changing {files}</>,
      writeAll: (files: Node) => <>Writing {files}</>,
      removing: (files: Node) => <>, removing {files}</>,
      more: (count: number) => `+${count} more`,
      check: 'Compiling',
      deploy: 'Putting it online',
      deployPreview: 'Putting the preview online',
      deployVersion: (version: number) => `Putting version ${version} online`,
      upload: (path: Node) => <>Storing {path}</>,
      delete: (path: Node) => <>Removing {path}</>,
      list: 'Looking at the stored files',
      other: (tool: string) => `Using ${tool}`,
    },
    marks: {
      version: (version: number) => `v${version}`,
      from: (version: number) => `from v${version}`,
      online: 'online',
      previewOnline: 'preview online',
      compiles: 'compiles',
      errors: (count: number) => (count === 1 ? '1 error' : `${count} errors`),
      problems: (count: number) => (count === 1 ? '1 error' : `${count} errors`),
      clean: 'no errors',
      refused: 'refused',
    },
    results: {
      online: (version: number) => `Version ${version} is online`,
      ready: (version: number) => `Version ${version} is ready`,
      readyNote: 'It compiles. Look at what changed, then deploy it when you are happy with it.',
      saved: (version: number) => `Saved as version ${version}`,
      notOnline: 'It did not go online.',
      broken: (version: number) => `Version ${version} is saved, but it does not compile`,
      stillOnline: (version: number) => `Version ${version} is still online.`,
      nothingOnline: 'Nothing new went online.',
      unchanged: 'Nothing was changed',
      stopped: 'Stopped',
      stoppedSaved: (version: number) => `Version ${version} was saved before that.`,
      stoppedFeature: (name: string) => `What it did until then is in the feature “${name}”.`,
      feature: (name: string) => `Ready to try in the feature “${name}”`,
      featureBroken: (name: string) => `The feature “${name}” does not compile yet`,
      tryIt: 'Try it at the feature\'s own address. When you are happy with it, merge it: it becomes the next version.',
      previewOffline: 'Its preview is not online. Deploy it from the feature to try it.',
      previewStill: 'Its preview still shows what last compiled.',
      notMerged: 'It was not merged, so nothing new went online.',
      failed: 'The change did not go through',
      timeout:
        'It ran out of time before it changed anything. Ask for something smaller, or ask again - it gets further some runs than others.',
      turns:
        'It used up the steps it may take before it changed anything. Ask for something smaller, or split it into two changes.',
      timedOut: 'It ran out of time, so this is as far as it got.',
      usedUp: 'It used up the steps it may take, so this is as far as it got.',
      unauthorised:
        "The agent could not sign in, so nothing ran. That is this server's credentials, not anything about what you asked for.",
      nothing: 'It ended without changing anything or saying why.',
    },
    seeChanges: 'See what changed',
    open: 'Open it',
    openPreview: 'Open the preview',
    openFeature: 'Open the feature',
    deploy: (version: number) => `Deploy version ${version}`,
    undo: (version: number) => `Put version ${version} back`,
    undoTitle: 'Undo the change: the version that was online before goes back online. The new one stays in the history.',
    again: 'Ask again',
    toast: {
      online: (version: number) => `The change is online as version ${version}.`,
      saved: (version: number) => `The change is saved as version ${version}.`,
      feature: (name: string) => `The change is ready to try in the feature “${name}”.`,
      unchanged: 'The agent did not change anything.',
      failed: 'The change did not go through.',
      stopped: 'The change was stopped.',
    },
    offTitle: 'There is no agent on this installation',
    off: 'This server has no agent of its own to make changes with. An agent of yours can: connect it to the address below, give it the editor link, and tell it what should be different.',
    own: 'Use your own agent instead',
    ownText:
      'Any agent that speaks MCP can change this lambda: connect it to this address, give it the editor link, and tell it what should be different. It has no daily allowance and no clock.',
    mcp: 'MCP address',
    editorLink: 'Editor link - keep it private',
  },

  summary: {
    reading: 'Reading how it is doing…',
    hint: (since: string, kept: boolean, retention: number, tier: string) =>
      `Traffic is counted since the server last started (${since}). ` +
      (kept
        ? `A lambda stays online while people use it, and is removed after ${retention} days with no visits and no changes.`
        : `This lambda is in the ${tier} tier, which keeps it online and stored however quiet it gets.`),
    onlineFor: (duration: (text: string) => Node, version: number) => (
      <>
        Online for {duration('a while')}, serving version {version}.
      </>
    ),
    offline: 'Offline. Nothing is being served until a version is deployed.',
    nothing: 'Nothing has been written yet.',
    requestsToday: 'requests today',
    lastHour: (count: number) => `${count} in the last hour`,
    hourly: 'Requests per hour over the last day',
    failed: 'failed',
    failedTitle: (failed: number, rejected: number) =>
      `${failed} server errors, ${rejected} not found or refused, over the last day`,
    average: 'to answer, on average',
    noneYet: 'none yet',
    lastVisit: 'last visit',
    problems: 'Something went wrong recently',
    openLog: 'Open the log',
    latest: 'Latest change',
    allVersions: 'All versions',
    noDescription: 'No description',
    version: (version: number) => `Version ${version}`,
    notOnline: 'not online yet',
    wanted: 'What was wanted',
    noVersions: 'No versions yet.',
    inProgress: 'Being worked on',
    allFeatures: 'All features',
    previewOnline: 'Its preview is online',
    previewOffline: 'Its preview is offline',
    behind: 'behind',
    featureTip: (start: (text: string) => Node) => (
      <>
        Changing something people use? {start('Start a feature')}: it is tried at an address of its own, on a copy of the
        data, and becomes the next version once it is right.
      </>
    ),
    storage: 'Storage',
    inVersion: (version: number) => `In version ${version}`,
    noVersion: 'In the version',
    inData: 'In the data',
    sharedByAll: 'Shared by every version',
    browse: 'Browse',
    code: 'Code',
    codeWhy: 'C# is compiled, never served.',
    characters: 'characters',
    assets: 'Assets',
    assetsPublic: 'Public: the code serves them.',
    assetsPrivate: 'Not served by the code.',
    data: 'Data',
    workspace: 'Workspace',
    workspaceOff: 'switched off',
    dataPublic: 'Public: the code serves the workspace.',
    dataPrivate: 'Private to the lambda.',
  },

  files: {
    hint: (b: (text: string) => Node) => (
      <>
        The files of one version - the program. {b('Code')} is compiled and never served. {b('Assets')} - pages,
        scripts, styles, images - are saved with the code, deployed and rolled back with it, and are public if the code
        serves them. What the lambda keeps while it runs is not here: that is its {b('Data')}.
      </>
    ),
    scope: (version: number, data: (text: string) => Node) => (
      <>
        These belong to version {version} and change with it. What the lambda keeps while it runs is the same for every
        version, and is under {data('Data')}.
      </>
    ),
    edit: 'Edit this version',
    version: 'Version',
    shown: (version: number, online: boolean, newest: boolean) =>
      `Version ${version}${online ? ', online' : newest ? ', newest' : ''}`,
    optionOnline: ' (online)',
    readFailed: 'That version could not be read.',
    noVersion: 'There is no version to show yet.',
    label: 'Files',
    code: 'Code',
    codeWhy: 'Compiled into the lambda, never served.',
    count: (files: number) => (files === 1 ? '1 file' : `${files} files`),
    codeUsage: (files: string, used: string, of: string) => `${files}, ${used} of ${of} characters`,
    usage: (files: string, used: string, of: string) => `${files}, ${used} of ${of}`,
    noCode: 'No code in this version.',
    assets: 'Assets',
    assetsPublic: 'Public: this version serves them with Assets.',
    assetsPrivate: 'Saved with the code, but this version does not serve them.',
    noAssets: 'None in this version.',
    data: 'Data',
    dataPublic: 'Public: the code online serves it with Workspace.',
    dataPrivate: 'Private to the lambda. Not part of any version.',
    uploadFailed: (path: string) => `${path} could not be uploaded.`,
    deleteFolder: (path: string, held: number) =>
      held > 0
        ? `Delete ${path} and the ${held === 1 ? '1 file' : `${held} files`} in it?`
        : `Delete the folder ${path}?`,
    deleteFile: (path: string) => `Delete ${path}? The lambda will not find it any more.`,
    deleteFailed: 'It could not be deleted.',
    full: 'The data is full',
    uploadInto: (folder: string) => `Upload into ${folder}`,
    upload: 'Upload',
    reading: 'Reading…',
    noData: 'Nothing yet. What the lambda saves while it runs appears here.',
    delete: (path: string) => `Delete ${path}`,
    deleteShort: 'Delete',
    fileFailed: 'The file could not be read.',
    pick: 'Pick a file to see what is in it.',
    tooLarge: (name: Node, size: string) => (
      <>
        {name} is {size}, too large to show here.
      </>
    ),
    download: 'Download',
    readingFile: (name: string) => `Reading ${name}…`,
    missing: (name: string) => `This version has no file called ${name}.`,
    saved: 'saved',
    notText: 'Not text. Download it to look inside.',
  },

  /** The data of a lambda: what it keeps rather than what it is. */
  data: {
    hint:
      'Data is what the lambda keeps while it runs. It belongs to the lambda, not to a version: every version reads and writes the same data, and nothing you do with versions changes it. A feature tries itself out on a copy of it. It goes when the lambda is deleted, or when you switch that kind of data off.',
    facts: [
      ['Shared by every version', 'Whichever version is online reads and writes the same data.'],
      ['Kept when you deploy', 'Deploying, rolling back or merging a feature never touches it.'],
      ['Yours to switch', 'Each kind is on only while you want it. Switching one off deletes what it holds.'],
    ] as [string, string][],
    featureHint:
      'The data this feature works on: a copy of the lambda\'s, taken when the feature began. Its preview reads and writes the copy, so trying things here never touches what the lambda keeps. Merging the feature throws the copy away and leaves the lambda\'s data as it is.',
    featureFacts: [
      ['A copy', 'Taken from the lambda when the feature began, or when you last copied it again.'],
      ['Only the preview sees it', 'What the preview writes stays here. The lambda and its visitors never see it.'],
      ['Gone with the feature', 'Merging or deleting the feature deletes the copy. The lambda\'s data stays as it is.'],
    ] as [string, string][],
    recopy: 'Copy the lambda\'s data again',
    recopyTitle: 'Replace this copy with what the lambda holds now',
    recopyConfirm: 'Copy the lambda\'s data again?',
    recopyText:
      'Everything in this copy - whatever the preview wrote into it - is replaced by what the lambda holds now. The lambda\'s own data is not touched.',
    keepCopy: 'Keep this copy',
    recopied: 'The copy is fresh. The preview reads it from its next request.',
    recopyFailed: 'The data could not be copied again.',
    copyContents: 'What the copy of the workspace holds',
    kinds: {
      workspace: {
        name: 'Workspace',
        what: 'Files the lambda reads and writes while it runs: uploads, records, anything it keeps.',
      },
    } as Record<string, { name: string; what: string }>,
    on: 'On',
    off: 'Off',
    byDefault: 'On by default',
    usage: (items: string, used: string, of: string) => `${items} · ${used} of ${of}`,
    offText: 'Switched off. It holds nothing, and code that uses it fails until it is switched on again.',
    switchLabel: (name: string) => `${name} on or off`,
    confirmOff: (name: string) => `Switch the ${name.toLowerCase()} off?`,
    confirmText: (items: string, size: string) =>
      `Everything in it - ${items}, ${size} - is deleted for good. This cannot be undone.`,
    confirmEmpty: 'It is empty, so nothing is lost.',
    inUse: 'The version online uses it, so it will fail where it does until you switch it on again.',
    deleteAndOff: 'Switch off and delete',
    keep: 'Keep it',
    switchedOn: (name: string) => `The ${name.toLowerCase()} is on. The lambda can use it from its next request.`,
    switchedOff: (name: string) => `The ${name.toLowerCase()} is off, and what it held is deleted.`,
    switchFailed: 'That could not be switched.',
    readFailed: 'The data could not be read.',
    demo: 'A demo: its data is there to be read, not changed.',
    contents: 'What the workspace holds',
    browse: 'Files',
    offBrowse: 'The workspace is off, so there are no files to show.',
  },

  /** Features: changes worked on beside the lambda, merged into the next version once they are right. */
  features: {
    hint:
      'A version never changes once it is saved. A feature is where a change is made instead: it starts as a copy of a version and of the lambda\'s data, can be tried at an address of its own while visitors keep getting what is online, and becomes the next version when you merge it.',
    newFeature: 'New feature',
    full: (limit: number) => `This lambda has ${limit} features open, which is all it may have. Merge or delete one first.`,
    emptyTitle: 'Nothing is being worked on',
    emptyText:
      'Start a feature to change the lambda without touching what is online. You, or the agent, can change it as often as it takes and try it at its own address.',
    steps: [
      ['Start it', 'A copy of the newest version, and of the lambda\'s data.'],
      ['Change and try it', 'At an address of its own, on its own copy of the data.'],
      ['Merge it', 'It becomes the next version, and goes online when you say so.'],
    ] as [string, string][],
    start: 'Start a feature',
    askAgentNew: 'Ask the agent for a change',
    noChange: 'Nothing said about what it changes yet',
    from: (version: number) => `from version ${version}`,
    mergeable: 'ready to merge',
    behindTitle: 'A newer version was saved after it began',
    behind: (newest: number) => `version ${newest} is newer`,
    previewOnline: 'preview online',
    previewOutdated: 'preview shows an earlier save',
    previewOffline: 'preview offline',
    changed: 'changed',
    openPreview: 'Preview',
    openPreviewTitle: 'Open the preview in a new tab',
    count: (open: number, limit: number) => `${open} of ${limit} features open`,
    loading: 'Loading the feature…',
    readFailed: 'The feature could not be read.',

    newTitle: 'Start a feature',
    newText:
      'A feature starts as a copy of a version - its code and its assets - and of the lambda\'s data. Change it and try it at an address of its own while visitors keep getting what is online; merge it once it is right.',
    newTextFiles:
      'What you typed in the code goes into it, instead of becoming a version. Try it at the feature\'s own address, and merge it once it is right.',
    name: 'Name',
    namePlaceholder: 'Leaderboard',
    wanted: 'What should it do?',
    wantedPlaceholder: 'Optional. Keep the ten best scores and show them after every game.',
    startFrom: 'Start from',
    version: (version: number, newest: boolean, online: boolean) =>
      `Version ${version}${newest && online ? ' (newest, online)' : newest ? ' (newest)' : online ? ' (online)' : ''}`,
    olderBase: (newest: number) =>
      `Not the newest: before it can be merged, it has to take in what versions up to ${newest} changed.`,
    create: 'Start it',
    createFailed: 'The feature could not be started.',
    retry: 'Try again',
    madeNotSaved: (name: string) =>
      `The feature “${name}” is started, but what you typed could not be put into it yet. Try again, or close this and find the feature under Features.`,
    created: (name: string) => `The feature “${name}” is started.`,
    cancel: 'Cancel',

    featureHint:
      'A change being worked on beside the lambda. Its preview runs its code against its own copy of the data, so visitors of the lambda see none of it. Merging makes it the next version.',
    askAgent: 'Ask the agent',
    editCode: 'Edit the code',
    preview: 'Preview',
    state: {
      online: 'Online',
      outdated: 'Online, with an earlier save',
      offline: 'Offline',
    },
    deployed: 'deployed',
    deployPreview: 'Deploy preview',
    updatePreview: 'Update the preview',
    previewDeployed: 'The preview is online.',
    previewFailed: 'The preview could not be put online.',
    previewStopped: 'The preview is offline.',
    previewRejected: 'The preview did not change',
    previewNotCompiling: 'It does not compile. The preview still shows what it showed before.',
    previewAddress: 'Preview address',
    previewNote: 'Anyone with this address can open the preview. Search engines are told to leave it alone.',
    basedOn: 'Based on',
    mergeableLong: 'the newest, so it can be merged',
    started: 'Started',
    changes: (version: number) => `What it changes against version ${version}`,
    noChanges: (version: number) => `Nothing yet: it holds exactly what version ${version} holds.`,
    notes: 'Notes',
    editNotes: 'Name and notes',
    what: 'What does it change?',
    whatPlaceholder: 'Adds a leaderboard that keeps the ten best scores',
    noWanted: 'Nothing said yet',
    mergeNote: (version: number) =>
      `Merging makes it version ${version}, with these notes. The feature goes with it - its preview and its copy of the data.`,
    missed: (from: number, to: number) =>
      to - from === 1 ? `What version ${to} changed` : `What versions ${from + 1} to ${to} changed`,
    missedNothing: 'Nothing in the files.',

    behindText: (base: number, newest: number) =>
      `It started from version ${base}, and version ${newest} was saved since. Merging it now would undo what that changed. Bring those changes into the feature - or ask the agent to - and then say it is based on version ${newest}.`,
    moveBase: 'Base it on another version',
    close: 'Close',
    mergeTitle: (name: string) => `Merge “${name}”`,
    mergeTitleShort: 'Make it the next version',
    leaks: (path: string, files: string) =>
      `${files} link to ${path} by its full path. From the preview, that is the live lambda with its real data, not this feature's copy. Relative paths ("api/items") stay in the preview.`,
    mergeButton: 'Merge',
    saveFirst: 'Save the code first: the preview and merging use what is saved.',
    merge: 'Merge it',
    mergeAndDeploy: (version: number) => `Merge and put version ${version} online`,
    mergeText: (version: number) =>
      `It becomes version ${version}. The feature goes with it: its preview, and its copy of the data. The lambda's own data stays as it is.`,
    deployToo: (version: number) => `Put version ${version} online right away`,
    deployTooNote: (active: number) => `Version ${active} stays a click away in the versions.`,
    deployTooOffline: 'The lambda is offline now; this puts it online.',
    notCompiling: 'It does not compile, so it was not merged. Fix it in the feature first.',
    mergeFailed: 'The feature could not be merged.',
    merged: (version: number | string) => `Merged as version ${version}.`,
    mergedOnline: (version: number | string) => `Merged as version ${version}, and online.`,

    notesTitle: 'Name and notes',
    save: 'Save',
    saveFailed: 'That could not be saved.',

    baseTitle: 'Base it on another version',
    baseText: (base: number) =>
      `It is based on version ${base}. Only a feature based on the newest version can be merged, so that merging never undoes what was saved after it began. Once it holds everything a newer version changed, say so here.`,
    moveTo: (version: number) => `Base it on version ${version}`,
    baseWarning: 'Nothing checks that those changes really are in the feature. Merging without them undoes them.',

    deleteTitle: (name: string) => `Delete “${name}”?`,
    deleteText:
      'Its code, its preview and its copy of the data are deleted for good. The lambda and its versions are not touched.',
    keep: 'Keep it',
    deleteForGood: 'Delete for good',
    deleteFailed: 'The feature could not be deleted.',
    deleted: (name: string) => `The feature “${name}” is deleted.`,

    all: 'All features',
    actions: 'More for this feature',
    download: 'Download as a zip',
    stopPreview: 'Take the preview offline',
    delete: 'Delete this feature',
    viewsLabel: 'The feature',
    views: {
      overview: 'Feature',
      code: 'Code',
      data: 'Data',
      logs: 'Logs',
    },
    missingTitle: 'This feature is not there any more',
    missingText: 'It was merged into a version, or deleted. The versions show what became of it.',
  },

  versions: {
    hint: (limit: number) =>
      `A version is the program - its code and its assets - and never changes once it is saved, so any of them can be compared with and put back online exactly as it was. Each keeps what was asked for and what it changed. To change the lambda, start a feature: it becomes the next version once it is right. The oldest are removed once there are more than ${limit}; the one online never is.`,
    none: 'No versions yet.',
    noDescription: 'No description',
    online: 'online',
    putOnline: 'Put this version online',
    rollBackTitle: 'Put this older version back online',
    deploy: 'Deploy',
    rollBack: 'Roll back',
    readFailed: 'This version could not be read.',
    comparing: 'Comparing…',
    unchanged: 'Nothing changed from the version before.',
    first: 'The first version.',
    status: { added: 'added', removed: 'removed', changed: 'changed', same: 'same' } as Record<string, string>,
    browse: 'Browse its files',
    edit: 'Edit from here',
    feature: 'Start a feature from here',
    featureTitle: 'Work on a change of this version beside the lambda, and merge it into the next version once it is right',
    binary: 'Not text, so there are no lines to compare.',
    tooLarge: 'Too large to compare line by line.',
  },

  deployments: {
    hint: (until: string | null) =>
      `A deployment stays online while people use it${until ? ` - if nobody does, until ${until}` : ''}. Deploying again, or any visit, restarts that clock.`,
    takeOffline: 'Take offline',
    readFailed: 'The history could not be read.',
    reading: 'Reading the history…',
    none: 'Nothing has been deployed yet.',
    noDescription: 'No description',
    deployed: (when: string, by: string) => `Deployed ${when} by ${by}`,
    duration: 'How long it was online',
    online: 'online',
    short: {
      replaced: 'replaced',
      stopped: 'taken offline',
      expired: 'expired',
      admin: 'by the operator',
      ended: 'ended',
    } as Record<string, string>,
    putBack: (version: number) => `Put version ${version} back online`,
    timeline: 'What was online over the last seven days',
    block: (version: number, from: string, to: string | null) => `Version ${version}, ${from} to ${to ?? 'now'}`,
    weekAgo: 'a week ago',
    now: 'now',
  },

  stats: {
    readFailed: 'The figures could not be read.',
    range: 'Time range',
    lastHour: 'Last hour',
    lastDay: 'Last day',
    hint: (since: string) =>
      `Counted in memory since the server last started, ${since}. A restart begins these figures again.`,
    reading: 'Reading the figures…',
    requests: 'requests',
    websockets: (count: number) => `and ${count} websocket connections`,
    failed: 'failed',
    serverErrors: (count: number) => `${count} server errors`,
    rejected: 'not found or refused',
    average: 'to answer, on average',
    sent: (amount: string) => `${amount} sent`,
    nobody: (hour: boolean): string => (hour ? 'Nobody has called it in the last hour.' : 'Nobody has called it in the last day.'),
    requestsTitle: 'Requests',
    per: (hour: boolean): string => (hour ? 'Per minute.' : 'Per 15 minutes.'),
    answered: 'Answered',
    rejectedSeries: 'Not found or refused',
    failedSeries: 'Failed',
    timeTitle: 'Time to answer',
    averagePer: (hour: boolean): string => (hour ? 'The average per minute.' : 'The average per 15 minutes.'),
    averageSeries: 'Average',
    mostAsked: 'Most asked for',
    path: 'Path',
    requestsColumn: 'Requests',
    failedColumn: 'Failed',
    averageColumn: 'Average',
    since: 'Since the server started.',
  },

  logs: {
    readFailed: 'The log could not be read.',
    hint: (capturing: boolean) =>
      'Requests, what the lambda printed and what went wrong, as it happens.' +
      (capturing ? '' : ' This installation does not keep what lambdas print, so only requests and errors appear.') +
      " Held in memory and shared with every lambda here, so it reaches back minutes to hours, and is empty after a restart. Visitors' addresses are not shown.",
    featureHint: (capturing: boolean) =>
      'What the preview of this feature answered, printed and threw, as it happens.' +
      (capturing ? '' : ' This installation does not keep what lambdas print, so only requests and errors appear.') +
      " Kept apart from the lambda's own log, which never shows the preview. Held in memory, so it reaches back minutes to hours.",
    nothingPreview: "Nothing yet. Open the feature's preview and its requests appear here.",
    search: 'Search',
    searchLabel: 'Search the log',
    resume: 'Show new lines as they come',
    pause: 'Stop adding new lines while you read',
    paused: 'Paused',
    live: 'Live',
    show: 'Show',
    all: 'Everything',
    requests: 'Requests',
    output: 'What it printed',
    problems: 'Problems',
    reading: 'Reading the log…',
    noProblems: 'Nothing has gone wrong that the log still remembers.',
    nothing: "Nothing yet. Open the lambda's address and its requests appear here.",
    noMatch: 'Nothing matches.',
    identical: (count: number) => `${count} identical lines`,
    at: (domain: string) => `, at ${domain}`,
    from: (country: string) => `, from ${country}`,
  },

  showcase: {
    loadFailed: 'The showcase could not be loaded.',
    loading: 'Loading…',
    title: 'a title',
    description: 'a description',
    picture: 'a picture',
    updated: 'The showcase entry is updated.',
    listed: 'It is on the showcase page now.',
    waiting: 'Saved. It appears on the showcase page once the lambda is online.',
    saveFailed: 'The showcase entry could not be saved.',
    removed: 'Taken off the showcase page.',
    removeFailed: 'The showcase entry could not be removed.',
    wrongType: 'That is not a PNG, JPEG, GIF or WebP image.',
    tooLarge: (size: string, limit: string) => `That is ${size}; a picture can be ${limit} at most.`,
    unreadable: 'That file could not be read.',
    hint: (tool: Node) => (
      <>
        The showcase page lists lambdas their owners chose to show, the ones in use lately first. Only whoever holds the
        editor key can put a lambda there or take it down, and it is only listed while it is online. An agent can do the
        same with its {tool} tool.
      </>
    ),
    open: 'Open the showcase',
    switch: 'Show this lambda on the showcase page',
    listedNow: 'Listed now. Anyone browsing the showcase can open it.',
    notListed: 'Saved, but not listed: the lambda is offline. It reappears once it is deployed again.',
    off: 'Off. Nothing about this lambda is shown anywhere until you switch this on and save.',
    offline: 'The lambda is offline, so the entry will wait until it is deployed. Only lambdas that answer are listed.',
    titleLabel: 'Title',
    titlePlaceholder: 'Pub quiz scoreboard',
    descriptionLabel: 'Description',
    descriptionPlaceholder:
      'Teams enter their answers on their phones, the host marks them, and the scoreboard updates for everyone in the room.',
    save: 'Save changes',
    add: 'Add to the showcase',
    takeOff: 'Take it off',
    needs: (missing: string[]) =>
      `Still needs ${missing.length > 1 ? `${missing.slice(0, -1).join(', ')} and ${missing[missing.length - 1]}` : missing[0]}.`,
    tooLong: 'Some of it is too long.',
    allSaved: 'Everything is saved.',
    preview: 'Preview',
    card: (address: Node) => <>This is the card visitors see. It opens {address}.</>,
    confirm: 'Take it off the showcase?',
    keep: 'Keep it',
    confirmText: 'The title, description and picture are deleted. The lambda itself stays exactly as it is.',
    pictureLabel: 'Picture',
    formats: (limit: string) => `PNG, JPEG, GIF or WebP, up to ${limit}`,
    notSaved: 'not saved yet',
    replace: 'Drop a new one here to replace it.',
    drop: 'Drop a picture here.',
    advice: 'A screenshot, or a short GIF of it in use, works best at 16:10.',
    another: 'Choose another',
    choose: 'Choose a file',
    keepSaved: 'Keep the saved one',
    clear: 'Clear',
  },

  domain: {
    readFailed: 'The domain could not be read.',
    reaching: (domain: string) => `Requests to ${domain} now reach this lambda.`,
    saveFailed: 'The domain could not be saved.',
    removed: 'The domain is removed. The lambda still answers at its address here.',
    removeFailed: 'The domain could not be removed.',
    hint:
      'A premium lambda can answer at a domain of its own - the whole of it, from the root down - as well as at its address here. Point the domain at this server, enter it here, and requests to it reach the lambda.',
    loading: 'Loading…',
    example: 'your-domain.com',
    open: (domain: string) => `Open ${domain}`,
    label: 'The domain it answers at',
    serving: (domain: Node) => <>Serving {domain} now, besides its address here.</>,
    none: 'None yet. A subdomain such as shop.example.com, or a whole domain such as example.com.',
    change: 'Change',
    use: 'Use this domain',
    remove: 'Remove',
    confirm: 'Remove the domain?',
    keep: 'Keep it',
    confirmText: (domain: Node) => (
      <>
        Requests to {domain} stop reaching this lambda at once. Its address here stays as it is, and so does whatever the
        domain's DNS says.
      </>
    ),
    point: 'Point the domain at this server',
    check: 'Check again',
    records:
      'At whoever manages the DNS of the domain, add these two records. Leave out the AAAA record if you would rather not be reachable over IPv6.',
    type: 'Type',
    name: 'Name',
    value: 'Value',
    pointsHere: (domain: Node) => <>{domain} points here.</>,
    alsoElsewhere: (addresses: string) =>
      ` It also resolves to ${addresses}, which is not this server - visitors sent there will not reach the lambda.`,
    elsewhere: (addresses: string) => `It resolves to ${addresses}, which is not this server yet.`,
    wait: 'A change can take a while to be seen everywhere - up to the time to live of the old record.',
    cname: 'Using a CNAME record instead',
    cnameText: (target: Node) => (
      <>
        A subdomain can point at {target} with a CNAME record instead, and then follows this server if its addresses ever
        change. It has drawbacks:
      </>
    ),
    cnameRoot: (example: Node) => (
      <>
        It cannot be used for a whole domain ({example} itself): the standard does not allow a CNAME next to the records
        every domain has at its root. Some providers offer an ALIAS, ANAME or "flattened" record that works there
        instead.
      </>
    ),
    cnameAlone: 'Nothing else can sit on the same name - no MX record for mail, no TXT record for verifications.',
    cnameLookup: "Visitors' resolvers make one more lookup before they arrive.",
    copy: 'Copy',
    copyValue: (value: string) => `Copy ${value}`,
  },

  code: {
    title: 'Code',
    version: (version: number) => `version ${version}`,
    edited: ', edited',
    online: ', online',
    loadFailed: 'That version could not be loaded.',
    compiles: 'It compiles.',
    notYet: 'It does not compile yet.',
    checkFailed: 'The code could not be checked.',
    saved: (version: number | undefined) => `Saved as version ${version}.`,
    featureSaved: 'Saved into the feature. Deploy its preview to try it.',
    featureLoadFailed: 'The feature could not be loaded.',
    previewOnline: 'The preview is online.',
    previewRefused: 'The preview did not change. See what the compiler said below.',
    isOnline: (version: number | undefined) => `Version ${version} is online.`,
    notOnline: 'It did not go online. See what the compiler said below.',
    failed: 'That did not work.',
    unchanged: 'Nothing has changed since the last save.',
    demo: 'A demo, so everything here is read only. Create a lambda of your own from it to change it. ',
    edit: 'Edit the code by hand. Saving makes a new version and leaves what is online alone; deploying puts it online. To try a change first, start a feature. ',
    editFeature:
      'The code of this feature. Saving keeps it in the feature - nothing the lambda\'s visitors get changes. Deploying puts it online at the feature\'s own address, to try it; merging the feature makes it the next version. ',
    inFeature: (name: string) => `in “${name}”`,
    previewed: ', in the preview',
    changedElsewhere: 'The feature was saved elsewhere since you opened it - by the agent, perhaps. Load what is saved before saving here; your changes would not be saved over it.',
    readAgain: 'Load what is saved',
    files: (entry: Node, cs: Node) => (
      <>
        {entry} returns what gets served, other {cs} files hold types, and any other file is served as it is. Ctrl-S
        saves, F12 goes to a declaration.
      </>
    ),
    newer: (version: number) => ` Version ${version} is newer than the one open here.`,
    check: 'Check',
    save: 'Save',
    deploy: 'Deploy',
    deployPreview: 'Deploy preview',
    deployPreviewTitle: 'Save, and put the feature online at its own address to try it',
    binary: (size: number) => `Not text, so there is nothing to edit. It is served as it is and weighs ${size} kB.`,
    saveAndDeploy: 'Save and deploy',
    saveVersion: 'Save a new version',
    fromOlder: (version: number, newest: number) =>
      `This starts from version ${version}, and version ${newest} is newer. Saving makes it the newest version, without what came after version ${version}.`,
    featureInstead: (start: (text: string) => Node) => (
      <>
        Trying something out? {start('Put it into a new feature instead')}: it gets an address of its own, and no version is
        saved until it is right.
      </>
    ),
    cancel: 'Cancel',
    what: 'What does it change? Optional - it is shown in the history.',
    placeholder: 'Adds a contact form',
    goToDefinition: 'Go to definition',
  },

  /** The strip of files above the code. */
  tabs: {
    codeName: 'Letters, digits, dashes and underscores, ending in .cs',
    slashes: 'No leading or trailing slash, and under 120 characters.',
    deep: 'At most six folders deep.',
    characters: 'Letters, digits, dashes, underscores and dots, separated by slashes.',
    extension: 'It needs an extension, so it can be served as the right thing.',
    exists: 'There is already a file with that name.',
    remove: (name: string) => `Remove ${name}? Its contents go with it.`,
    there: (name: string) => `${name} is already there.`,
    entry: 'The snippet: what it returns is what gets served',
    errors: 'has errors',
    removeFile: (name: string) => `Remove ${name}`,
    removeTitle: 'Remove this file',
    placeholder: 'Types.cs or site/index.html',
    newFile: 'New file',
    uploadTitle: 'Upload a file - an image, a font, a page',
    upload: 'Upload a file',
  },
};

export type EditorMessages = typeof editor;
