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
      showcase: 'Showcase',
      domain: 'Domain',
      files: 'Files',
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
      <>Every version, its files, its history and the address {key} go with it. This cannot be undone.</>
    ),
    openInTab: 'Open in a new tab',
    open: (address: string) => `Open ${address} in a new tab`,
    copyAddress: 'Copy the address',
    renameFailed: 'The address could not be changed.',
    moveIt: 'Move it',
    renameText: 'The old address stops working right away, so update anything that links to it.',
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
    storage: 'Storage',
    browse: 'Browse',
    code: 'Code',
    codeWhy: 'C# is compiled, never served.',
    characters: 'characters',
    assets: 'Assets',
    assetsPublic: 'Public: the code serves them.',
    assetsPrivate: 'Not served by the code.',
    data: 'Data',
    dataPublic: 'Public: the code serves the workspace.',
    dataPrivate: 'Private to the lambda.',
  },

  files: {
    hint: (b: (text: string) => Node) => (
      <>
        {b('Code')} is compiled and never served. {b('Assets')} - pages, styles, images - are saved with each version and
        are public if the code serves them. {b('Data')} is what the lambda writes while it runs; it is not part of any
        version, and is public only if the code serves it.
      </>
    ),
    edit: 'Edit this version',
    version: 'Version',
    shown: (version: number, online: boolean, newest: boolean) =>
      `Version ${version}${online ? ', online' : newest ? ', newest' : ''}`,
    optionOnline: ' (online)',
    readFailed: 'That version could not be read.',
    dataFailed: 'The data could not be read.',
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
    dataPublic: 'Public: this version serves it with Workspace.',
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

  versions: {
    hint: (limit: number) =>
      `Each version keeps what was asked for and what it changed, where whoever wrote it said so. The oldest are removed once there are more than ${limit}; the one online never is.`,
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
    isOnline: (version: number | undefined) => `Version ${version} is online.`,
    notOnline: 'It did not go online. See what the compiler said below.',
    failed: 'That did not work.',
    unchanged: 'Nothing has changed since the last save.',
    demo: 'A demo, so everything here is read only. Create a lambda of your own from it to change it. ',
    edit: 'Edit the code by hand. Saving makes a new version and leaves what is online alone; deploying puts it online. ',
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
    binary: (size: number) => `Not text, so there is nothing to edit. It is served as it is and weighs ${size} kB.`,
    saveAndDeploy: 'Save and deploy',
    saveVersion: 'Save a new version',
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
