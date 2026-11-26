import type { ReactNode } from 'react';

type Node = ReactNode;

/** The agent of the installation, changing the lambda for its owner. */
export const change = {
  hint: 'Say what should be different, and the agent running on this server does it while you watch. It works on a draft - a copy of your app with an address of its own - so your visitors see nothing until it works. Then it goes online, or stays a draft for you to try first.',
  reading: 'Asking the agent…',
  readFailed: 'The agent could not be asked.',
  label: 'What should be different?',
  placeholder: 'Keep the ten best scores instead of five, add a dark mode, …',
  placeholderNext: 'What else should change?',
  /** Above the box, while the next message answers the question the agent asked. */
  answering: 'Your answer to its question',
  placeholderAnswer: 'Answer it, and it carries on…',
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
  /** Under the box, while a value in the lists - a model, say - that only runs with the installation's password is chosen. */
  unlimited: 'With the password it runs on the limits the operator set for such runs - without a clock or a step limit, unless they set one.',
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
    switchOn: { database: 'Switching on the database', secrets: 'Switching on secrets', workspace: 'Switching on the workspace' },
    records: 'Reading the database',
    secrets: 'Looking at the secrets',
    /** The agent asked for the lambda to connect to other servers, which the operator of the site allows. */
    connectivity: 'Asking to connect to other servers',
    /** The agent read who visits the app, or - asked to - switched counting visits on. */
    analytics: 'Looking at the analytics',
    /** The agent published the app's source code, or looked at whether it is. */
    source: 'Publishing the source code',
    /** The agent read a page of a website the owner named in their message; site is its address, like www.example.com. */
    web: (site: Node) => <>Reading a page of {site}</>,
    search: 'Searching the web',
    /** The agent reads a file the owner sent with their message; file is its name. */
    attachment: (file: Node) => <>Looking at {file}, which you sent</>,
    showcase: 'Listing it in the showcase',
    /** The agent hands the owner a file, shown beside its answer; file is its name. */
    file: (file: Node) => <>Handing over {file}</>,
    /** The agent changed whether the editor opens in the simple or the full view. */
    view: 'Changing how the editor opens',
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
    /** It stopped with an error after it had done part of the work. */
    brokeOff: 'It stopped with an error, so this is as far as it got.',
    unauthorised:
      "The agent could not sign in, so nothing ran. That is this server's credentials, not anything about what you asked for.",
    nothing: 'It ended without changing anything or saying why.',
    /** The agent never said how the change ended: it forgot it, or the platform could not reach it for too long. */
    lost: 'No answer came back from the agent. What it saved is in the history.',
    asked: 'It asks before it goes on',
    answerBelow: 'Answer in the box below, and it carries on from there.',
  },
  /** What the lambda waits for after a change, and whom. */
  waitsFor: {
    connectivity: 'Waits for the operator to decide on connecting to other servers.',
    keys: (names: string[]) => (names.length === 1 ? `Waits for the secret ${names[0]} to be set.` : `Waits for the secrets ${names.join(', ')} to be set.`),
  },
  /** Carries a change on where it stopped: it waited for something, or ran out of time. */
  goOn: 'Go on',
  goOnTitle: 'Carry on where it stopped, without asking again',
  /** A lambda the agent created beside this one, by its public key; its editor link is in the answer. */
  created: (key: string) => `Created the lambda ${key}`,
  undoDialog: 'Undo this change?',
  undoes: 'This is undone:',
  keeps: 'This stays as it is:',
  undone: {
    online: 'The version online before goes back online.',
    draft: 'The draft it started is discarded.',
    connectivity: 'Its request to connect to other servers is withdrawn.',
    source: 'The published source is taken down.',
    showcase: 'The lambda leaves the showcase.',
    view: 'The editor opens in the view it had before.',
  } as Record<string, string>,
  kept: {
    versions: 'The versions it saved stay in the history.',
    lambda: 'A lambda it created stays.',
    data: 'A database, secrets or workspace it switched on stays on - switching it off deletes it.',
    analytics: 'Analytics stay on - switching them off deletes what was counted.',
    workspace: 'Files it put into the workspace stay.',
    draft: 'What it changed in a draft it did not start stays.',
    undone: 'It was undone already.',
  } as Record<string, string>,
  /** After undoing: what changed since the change stayed, as it was not as the change left it. */
  changedSince: 'Some of it changed since, so that stayed as it is.',
  /** After undoing: the version online before could not go back online - it is gone from the history, or does not compile any more. */
  notUndone: 'The version online before could not be put back, so what is online now stays.',
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

  /**
   * The box on an installation an organization runs for its people (its
   * operator chose "enterprise"): a change to a tool for work rather than to
   * a game.
   */
  profiles: {
    enterprise: {
      placeholder: 'Add a column for the department, sort the list by date, …',
    },
  },
};
