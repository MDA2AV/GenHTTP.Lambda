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
      docs: 'Documentation',
      tests: 'Tests',
      change: 'Change',
      features: 'Drafts',
      showcase: 'Showcase',
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
  },

  /**
   * The simple view: the app, how it is doing and a box to ask for a change,
   * for somebody who had it built and does not read code. Nothing here says
   * version, deployment, file or log - a version is a change, putting one
   * online is putting it online, and going back to one is going back.
   */
  simple: {
    view: 'View',
    simple: 'Simple',
    full: 'Full',
    simpleTitle: 'Your app, how it is doing, and a box to ask for changes',
    fullTitle: 'Every section: the code, the documentation, the tests, the files, the data, the versions and the logs',
    simpleNote: 'Your app and a box to ask for changes.',
    fullNote: 'Every section, the code included.',
    toFull: 'Show every section',
    toSimple: 'Switch to the simple view',
    /** The documentation, in the simple view: what the app is for. */
    about: 'About',
    aboutMore: 'More about your app',

    outsideTitle: 'This is part of the full view',
    outsideText: 'The simple view leaves out the code, the files and the history. Show every section to work with them here.',
    back: 'Back to your app',

    title: 'Your app',
    online: 'Your app is online',
    onlineFor: (span: Node) => <>Online for {span}. Anyone with the address can open it.</>,
    onlineNow: 'Anyone with the address can open it.',
    offline: 'Your app is offline',
    offlineText: 'Nobody can open it right now. Put it back online whenever you like.',
    putOnline: 'Put it online',
    openApp: 'Open your app',
    copy: 'Copy the address',
    copied: 'Copied',
    badge: 'Online',
    pending: 'A change is ready, but not online yet',
    pendingText: 'Your visitors still see your app as it was before it.',

    askOnline: 'Once it works, it goes online by itself.',
    askDraft: 'It stays a draft for you to try first.',


    problems: 'Something went wrong for visitors recently',
    problemsText: 'The agent can look into what happened and fix it.',
    fix: 'Ask the agent to fix it',

    needsKey: (count: number): string => (count === 1 ? 'Your app needs a key to work' : 'Your app needs a few keys to work'),
    needsKeyText: (names: Node) => (
      <>It is waiting for {names}. Enter it once - nobody can see it afterwards, not you and not the agent.</>
    ),
    enterKey: 'Enter it',

    activity: 'Today',
    hits: 'Hits today',
    hitsTitle: 'Every page, picture and request your app answered today',
    lastVisit: 'Last visit',
    noVisit: 'None yet',
    latest: 'Latest change',
    allChanges: 'All changes',
    askCta: 'Ask for a change',

    historyHint: 'Every change your app went through, newest first. Go back to an earlier one whenever you like - what your app has stored stays as it is.',
    noNote: 'A change without a description',
    created: 'Your app was created',
    isOnline: 'online',
    goBack: 'Go back to this',
    putThisOnline: 'Put this online',
    goBackTitle: 'Go back to this?',
    goBackText: 'Your app goes back to how it was after this change. What it has stored stays as it is, and the newer changes stay here for you to come back to.',
    putOnlineTitle: 'Put this online?',
    putOnlineText: 'Your visitors see your app as it is after this change. What it has stored stays as it is.',
    goBackConfirm: 'Go back',
    putOnlineConfirm: 'Put it online',
    cancel: 'Cancel',


    publish: 'Put the latest change online',
    deployed: 'Done - that is what your visitors see now.',
    refused: 'The change did not go online',
    refusedText: 'It has a problem that keeps it from running. Whatever was online before is still online.',
    fixRefused: 'Fix what keeps the newest change from running, and put it online.',

    results: {
      online: (_version: number) => 'Your change is online',
      ready: (_version: number) => 'Your change is ready',
      readyNote: 'Put it online when you are happy with it.',
      saved: (_version: number) => 'Your change is saved',
      broken: (_version: number) => 'The change is saved, but it does not work yet',
      stillOnline: (_version: number) => 'What was online before is still online.',
      stoppedSaved: (_version: number) => 'What it did until then is saved.',
    },
    deploy: 'Put it online',
    undo: 'Undo the change',
    undoTitle: 'What was online before goes back online.',

    behindText: 'Your app changed after this draft began. It has to be brought up to date before it can go online - the agent can do that for you.',
    mergeText: 'It replaces what is online now. What your app has stored stays as it is.',
    mergeUndo: 'You can go back to what was online before from the overview.',
  },

  /** The agent of the installation, changing the lambda for its owner. */
  change: {
    hint: 'Say what should be different, and the agent running on this server does it while you watch. It works on a draft - a copy of your app with an address of its own - so your visitors see nothing until it works. Then it goes online, or stays a draft for you to try first.',
    reading: 'Asking the agent…',
    readFailed: 'The agent could not be asked.',
    label: 'What should be different?',
    placeholder: 'Keep the ten best scores instead of five, add a dark mode, …',
    placeholderNext: 'What else should change?',
    send: 'Make the change',
    sending: 'Asking…',
    goOnline: 'Put it online when it is done',
    goOnlineOn: 'Once it works, it goes online as a new version. Until then, your visitors keep seeing what is online now.',
    goOnlineOff: 'It stays a draft: try it at its own address, and put it online when you are happy with it.',
    where: 'Work on',
    whereTitle: 'A new draft, or one to go on with',
    newFeature: 'A new draft',
    full: (limit: number) =>
      `There are ${limit} drafts already, which is all there may be. Pick one to go on with, or put one online or discard it first.`,
    password: 'Password',
    fable: 'Fable takes its time: no clock and no step limit, in a lane of its own.',
    left: (left: number, perDay: number) => `${left} of ${perDay} left today`,
    leftTitle: 'The build page and this section share one allowance a day.',
    noneLeft: (time: string) => `That was the last one for today. There are more from ${time}.`,
    ideas: ['Make it work well on a phone', 'Add a dark mode', 'Make it look more polished'],
    fixLog: 'Fix the errors in the log',
    how: [
      { title: 'It reads what is there', text: 'The code, and what earlier versions were asked for - so what works keeps working.' },
      { title: 'It tries it on a draft', text: 'A copy of your app with an address of its own, so nothing changes for your visitors while it works.' },
      { title: 'It puts it online', text: 'As a new version - or leaves the draft for you to try first. The version before stays a click away.' },
    ],
    asked: 'You asked',
    goesOnline: 'goes online when done',
    review: 'stays a draft for you to try',
    inFeature: (name: string) => `in the draft “${name}”`,
    queued: (ahead: number) =>
      ahead === 1 ? 'Waiting - one job is ahead of this one.' : `Waiting - ${ahead} jobs are ahead of this one.`,
    starting: 'Starting…',
    working: 'Working on it',
    leaveOpen: 'It carries on if you open another section or close this page.',
    stop: 'Stop',
    stopping: 'Stopping…',
    stopTitle: 'Stop this change?',
    stopText:
      'What it has done so far stays in its draft. What is online stays online, unless it has already put the change there.',
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
      logsPreview: 'Reading what the draft logged',
      readFeature: 'Reading the draft',
      create: 'Creating a lambda',
      feature: 'Starting a draft',
      featureStarted: (name: Node) => <>Started the draft {name}</>,
      update: 'Updating the draft\'s notes',
      rebase: (_version: number) => 'Marking the draft as up to date',
      merge: 'Putting the draft online',
      discard: 'Discarding a draft',
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
      stoppedFeature: (name: string) => `What it did until then is in the draft “${name}”.`,
      feature: (name: string) => `Ready to try in the draft “${name}”`,
      featureBroken: (name: string) => `The draft “${name}” does not work yet`,
      tryIt: 'Try it at its own address. When you are happy with it, put it online.',
      previewOffline: 'Its preview is not running. Start it from the draft to try it.',
      previewStill: 'It does not compile, so its preview still shows the last version that did.',
      notMerged: 'Nothing new went online.',
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
    openPreview: 'Try it',
    openFeature: 'Open the draft',
    deploy: (version: number) => `Deploy version ${version}`,
    undo: (version: number) => `Put version ${version} back`,
    undoTitle: 'Undo the change: the version that was online before goes back online. The new one stays in the history.',
    again: 'Ask again',
    toast: {
      online: (version: number) => `The change is online as version ${version}.`,
      saved: (version: number) => `The change is saved as version ${version}.`,
      feature: (name: string) => `The change is ready to try in the draft “${name}”.`,
      unchanged: 'The agent did not change anything.',
      failed: 'The change did not go through.',
      stopped: 'The change was stopped.',
    },
    offTitle: 'Change it with your own agent',
    off: 'Connect an agent of your own - Claude, for example - to the address below, give it the editor link, and tell it what should be different. It changes your app here and puts it online for you.',
    own: 'Or use your own agent',
    ownText:
      'Any agent that speaks MCP can change this app too: connect it to the address below, give it the editor link, and tell it what should be different. It has no daily allowance and no clock.',
  },

  summary: {
    reading: 'Reading how it is doing…',
    readDocs: 'Read the documentation',
    written: 'Documentation and tests',
    writtenWhy: 'Never compiled and never served. Kept with each version, and counted with the assets.',
    writtenMissing: 'Not written yet',
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
    inProgress: 'Drafts',
    allFeatures: 'All drafts',
    previewOnline: 'Its preview is running',
    previewOffline: 'Its preview is not running',
    behind: 'out of date',
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
    secrets: 'Secrets',
    secretsOff: 'switched off',
    secretsCount: (count: number) => (count === 1 ? '1 secret' : `${count} secrets`),
    secretsMissing: (count: number) => `${count} missing`,
    secretsMissingTitle: 'The code reads secrets that are not set, and fails where it does.',
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
    context: 'Documentation and tests',
    contextWhy: 'Never compiled and never served: what is written about this version, for whoever reads or changes it.',
    contextUsage: (files: string, size: string) => `${files}, ${size} - counted with the assets`,
    noContext: 'Nothing written about this version yet.',
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

  /**
   * What a version says about itself beside its program: its documentation
   * and its tests, in .lambda/. The simple view has the documentation only,
   * and of it only what the app is for - called "About" there.
   */
  context: {
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
  },

  /** The data of a lambda: what it keeps rather than what it is. */
  data: {
    hint:
      'Data is what the lambda keeps while it runs. It belongs to the lambda, not to a version: every version reads and writes the same data, and nothing you do with versions changes it. It goes when the lambda is deleted, or when you switch that kind of data off.',
    facts: [
      ['Shared by every version', 'Whichever version is online reads and writes the same data.'],
      ['Kept when you deploy', 'Deploying or rolling back never touches it.'],
      ['Yours to switch', 'Each kind is on only while you want it. Switching one off deletes what it holds.'],
    ] as [string, string][],
    featureHint:
      'A copy of your app\'s data, just for this draft: its preview reads and writes only this copy, so trying things here never touches the real data. It goes when the draft does.',
    recopy: 'Reset the test data',
    recopyTitle: 'Replace it with a fresh copy of your app\'s data',
    recopyConfirm: 'Reset the test data?',
    recopyText:
      'Everything the preview wrote into it is replaced by a fresh copy of your app\'s data. Your app\'s own data is not touched.',
    keepCopy: 'Keep it',
    recopied: 'The test data is fresh again.',
    recopyFailed: 'The test data could not be reset.',
    copyContents: 'What the test data holds',
    kindsLabel: 'Kinds of data',
    kinds: {
      workspace: {
        name: 'Workspace',
        what: 'Files the lambda reads and writes while it runs: uploads, records, anything it keeps.',
        count: (items: number) => (items === 1 ? '1 file' : `${items} files`),
        confirmOff: 'Switch the workspace off?',
        switchedOn: 'The workspace is on. The lambda can use it from its next request.',
        switchedOff: 'The workspace is off, and what it held is deleted.',
      },
      secrets: {
        name: 'Secrets',
        what: 'API keys, passwords and tokens the code reads by name. Once saved, a value is never shown again - not to you, not to an agent.',
        count: (items: number) => (items === 1 ? '1 secret' : `${items} secrets`),
        confirmOff: 'Switch secrets off?',
        switchedOn: 'Secrets are on. Add the values the code needs.',
        switchedOff: 'Secrets are off, and every value is deleted.',
      },
    } as Record<string, {
      name: string;
      what: string;
      count: (items: number) => string;
      confirmOff: string;
      switchedOn: string;
      switchedOff: string;
    }>,
    on: 'On',
    off: 'Off',
    byDefault: 'On by default',
    offText: 'Switched off. It holds nothing, and code that uses it fails until it is switched on again.',
    switchLabel: (name: string) => `${name} on or off`,
    confirmText: (what: string) => `Everything in it - ${what} - is deleted for good. This cannot be undone.`,
    confirmEmpty: 'It is empty, so nothing is lost.',
    inUse: 'The version online uses it, so it will fail where it does until you switch it on again.',
    deleteAndOff: 'Switch off and delete',
    keep: 'Keep it',
    switchFailed: 'That could not be switched.',
    readFailed: 'The data could not be read.',
    demo: 'A demo: its data is there to be read, not changed.',
    contents: 'What the workspace holds',
    browse: 'Files',
    offBrowse: 'The workspace is off, so there are no files to show.',
    missingDot: 'The code needs a secret that is not set',

    /** The secrets: written and replaced, never read back. */
    secrets: {
      add: 'Add a secret',
      addTitle: 'Add a secret',
      replaceTitle: (name: string) => `Replace ${name}`,
      setTitle: (name: string) => `Set ${name}`,
      name: 'Name',
      nameHint: 'Letters, digits and underscores, not starting with a digit. The code reads it by this name.',
      nameInvalid: 'Use letters, digits and underscores, and do not start with a digit.',
      nameTaken: (name: string) => `${name} is set already. Saving replaces its value.`,
      value: 'Value',
      valuePlaceholder: 'Paste the key, password or token',
      show: 'Show',
      hide: 'Hide',
      sealed: 'Once saved, nobody can see it again - not you, not an agent. Only the code reads it.',
      inDraft: 'Only this draft\'s test data gets it. The secrets of your app stay as they are.',
      save: 'Save',
      cancel: 'Cancel',
      saved: (name: string) => `${name} is saved. The code reads it from its next request.`,
      saveFailed: 'The secret could not be saved.',
      replace: 'Replace',
      replaceHint: 'Give it a new value',
      delete: (name: string) => `Delete ${name}`,
      deleteTitle: (name: string) => `Delete ${name}?`,
      deleteUsed: 'The code reads it, so whatever reads it fails until it is set again.',
      deleteUnused: 'Nothing in the code reads it by this name.',
      deleteDraft: 'Only this draft\'s copy is deleted.',
      deleteConfirm: 'Delete',
      deleted: (name: string) => `${name} is deleted.`,
      deleteFailed: 'The secret could not be deleted.',
      used: 'read by the code',
      usedHint: 'The code reads it with Secret.Read or Secret.Exists.',
      unused: 'not read by the code',
      unusedHint: 'No code reads it by this name. Is it spelled the way the code spells it?',
      set: (when: Node) => <>set {when}</>,
      hidden: 'Hidden for good',
      missingTitle: (count: number) => (count === 1 ? 'The code needs a secret that is not set' : `The code needs ${count} secrets that are not set`),
      missingText: (count: number): string => (count === 1 ? 'Until it is set, whatever reads it fails.' : 'Until they are set, whatever reads them fails.'),
      contents: 'Stored secrets',
      copyContents: 'Secrets in the test data',
      setIt: 'Set it',
      optionalTitle: 'Optional',
      optionalText: 'The code asks whether these are set, and works without them.',
      empty: 'No secrets yet',
      emptyText: 'Keep API keys, passwords and tokens here rather than in the code, where every version, every download and everybody reading it would have them.',
      howTo: 'In the code',
      howToText: (code: (text: string) => Node) => (
        <>
          {code('Secret.Read("NAME")')} reads a value, {code('Secret.Exists("NAME")')} says whether it is set. In a
          downloaded project, both read the environment variable of the same name.
        </>
      ),
      copy: 'Copy',
      offTitle: 'Secrets are off',
      offText: 'Switch them on to keep API keys and passwords that the code can read and nobody can see.',
      offWanted: (names: Node) => <>The code reads {names}. Switch secrets on to set them.</>,
      switchOn: 'Switch on',
      readFailed: 'The secrets could not be read.',
    },

    /**
     * The data in the simple view, shown once there is any: what the app
     * saved, and the keys it uses - nothing about files as a program has them.
     */
    simple: {
      hint: 'What your app has saved, and the keys it uses. It stays as it is whatever change is online.',
      workspace: 'Saved by your app',
      workspaceText: 'What your app saved while people used it: uploads, entries, anything it keeps.',
      workspaceSize: (files: string, size: string) => `${files}, ${size}`,
      more: (count: number) => `and ${count} more`,
      secrets: 'Keys and passwords',
      secretsText: 'What your app uses to connect to other services. Once saved, nobody can see them - not you, not the agent.',
      needed: 'Your app needs this',
      enter: 'Enter it',
      change: 'Change',
      enterTitle: (name: string) => `Enter ${name}`,
      changeTitle: (name: string) => `Change ${name}`,
      switchesOn: 'Saving it lets your app use keys and passwords from now on.',
      saved: (name: string) => `${name} is saved. Your app uses it right away.`,
      nothing: 'Your app has not saved anything yet.',
    },
  },

  /**
   * Drafts - features, to the code: a copy of the lambda a change is tried on,
   * put online once it is right. Said without merging, bases or branches,
   * since the people reading it built an app, not a repository.
   */
  features: {
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
  },

  versions: {
    hint: (limit: number) =>
      `A version is your app as it was saved - its code and its assets - and never changes afterwards, so any of them can be compared with and put back online exactly as it was. Each keeps what was asked for and what it changed. The oldest are removed once there are more than ${limit}; the one online never is.`,
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
    docs: 'Read its documentation',
    edit: 'Edit from here',
    feature: 'Start a draft from here',
    featureTitle: 'Try a change on a copy of this version, without touching what is online',
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
      'What the preview of this draft answered, printed and threw, as it happens.' +
      (capturing ? '' : ' This installation does not keep what lambdas print, so only requests and errors appear.') +
      " Kept apart from your app's own log. Held in memory, so it reaches back minutes to hours.",
    nothingPreview: "Nothing yet. Open the draft's preview and its requests appear here.",
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
        what is in {context}: the documentation and the tests, never compiled or served. Ctrl-S saves, F12 goes to a
        declaration.
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
  },

  /** The strip of files above the code. */
  tabs: {
    codeName: 'Letters, digits, dashes and underscores, ending in .cs',
    slashes: 'No leading or trailing slash, and under 120 characters.',
    deep: 'At most six folders deep.',
    characters: 'Letters, digits, dashes, underscores and dots, separated by slashes.',
    extension: 'It needs an extension, so it can be served as the right thing.',
    context: 'In .lambda/, only docs/ and tests/ - letters, digits, dashes, underscores and dots, separated by slashes.',
    contextFiles: 'Documentation and tests: part of the version, never compiled or served',
    exists: 'There is already a file with that name.',
    remove: (name: string) => `Remove ${name}? Its contents go with it.`,
    there: (name: string) => `${name} is already there.`,
    entry: 'The snippet: what it returns is what gets served',
    errors: 'has errors',
    removeFile: (name: string) => `Remove ${name}`,
    removeTitle: 'Remove this file',
    placeholder: 'Types.cs, site/index.html or .lambda/docs/api.md',
    newFile: 'New file',
    uploadTitle: 'Upload a file - an image, a font, a page',
    upload: 'Upload a file',
  },
};

export type EditorMessages = typeof editor;
