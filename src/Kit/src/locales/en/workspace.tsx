/**
 * The workspace on /build: the apps somebody had built, kept in their
 * browser, and a conversation with the agent about each - what it is, how
 * it is doing, what should change. For people who do not write code, in the
 * words of the editor's simple view: a draft, putting it online, records,
 * keys and passwords. Never merge, branch, commit, version numbers or files.
 */
export const workspace = {
  /** The button at the top of the list of apps, which starts on a new one. */
  newApp: 'New app',
  /** The heading of the list of apps this browser remembers. */
  yourApps: 'Your apps',
  /** Under the list: where it is kept. */
  keptHere: 'Remembered in this browser only. To open an app somewhere else, use its editor link.',
  /** On a phone, the button that shows the list of apps. */
  showApps: 'Your apps',
  closeApps: 'Close the list',
  /** In the list, beside an app the agent is working on. */
  working: 'The agent is working on it',
  /** In the list, beside an app that is not online. */
  offline: 'Offline',

  add: 'Add an app',
  addLabel: 'Its editor link or editor key',
  addButton: 'Add',
  addNotKey: 'That is not an editor link or an editor key.',
  addUnknown: 'No app has this editor key.',
  addDemo: 'This is a demo, which nobody can change. Open it in the editor to start a copy of your own.',
  addFailed: 'It could not be added just now. Try again in a moment.',

  /** The state of an app, at the top of its conversation. */
  online: 'Online',
  blocked: 'Blocked',
  openApp: 'Open app',
  more: 'More',
  openEditor: 'Open the editor',
  copyEditor: 'Copy the editor link',
  forget: 'Remove from this list',
  forgetTitle: 'Remove it from this list?',
  forgetText:
    'The app stays online, and nothing about it changes. Only this browser forgets its editor link - to add it again, you need that link.',
  cancel: 'Cancel',
  gone: 'This app is not there any more',
  goneText: 'It was removed, or its editor key is no longer the one this browser knows. Remove it from this list.',
  unreadable: 'Your app could not be read just now.',

  /** What waits for the owner, above the conversation. */
  attention: {
    offlineTitle: 'Your app is offline',
    offlineText: 'Nobody can open it until it is back online.',
    putBack: 'Put it online',
    keysTitle: (count: number) => (count === 1 ? 'Your app needs a key or password' : `Your app needs ${count} keys or passwords`),
    keysText: 'Enter the value here. Only your app reads it - nobody sees it again, not the agent either.',
    value: 'Value',
    save: 'Save',
    /** A draft the agent left for the owner to try: its name follows. */
    draftTitle: (name: string) => `Ready to try: ${name}`,
    draftText: 'A draft, with test data at an address of its own. Nobody else sees it until you put it online.',
    tryIt: 'Try it',
    putOnline: 'Put online',
    putOnlineTitle: 'Put this draft online?',
    putOnlineText: 'Everybody who opens your app sees the change from now on. What was online before stays in its history, to go back to.',
    putOnlineFailed: 'The draft does not work yet, so it was not put online. Ask the agent to fix it.',
    /** A draft begun before the app last changed, which has to take that change in before it can go online. */
    outOfDate: 'Out of date: your app changed after this draft began.',
    bringUp: 'Bring it up to date',
    /** What the button above puts into the box, for the agent to do in that draft. */
    bringUpAsk: 'Bring this draft up to date with what is online now, and keep its changes.',
    /** Above the box, while the next message goes to a draft rather than a new one. */
    inDraft: (name: string) => `In the draft “${name}”`,
    leaveDraft: 'Start a new draft instead',
    blockedTitle: 'This app was blocked by whoever runs this site',
    blockedText: 'It is offline, and nothing about it can be changed. Open the editor to read why.',
    failed: 'That did not work. Try again in a moment.',
  },

  /** For an app with no conversation yet. */
  firstTitle: (name: string) => `What would you like to know about ${name}, or change?`,
  firstText: 'Ask a question - who visits, what was entered, whether something is wrong - or say what should be different.',
  /** What the owner might ask about an app, offered before they type. */
  suggestions: [
    'How many people visited this week?',
    'What did people enter lately?',
    'Is anything going wrong?',
    'Make it look better on phones',
    'Undo the last change',
  ],

  /** The box at the bottom: for a new app, and for one that exists. */
  placeholderApp: (name: string) => `Ask about ${name}, or say what should change…`,
  /** The box while the agent waits for an answer to the question it asked. */
  placeholderAnswer: 'Your answer…',
  /** Above the box while the next message answers the agent's question. */
  answering: 'Your next message answers its question.',
  /** Beside that, for whoever wants to ask about something else rather than answer. */
  askSomethingElse: 'Ask something else instead',

  /** Files that go with a message, and files the agent hands back. */
  files: {
    /** The paper clip beside the box. */
    attach: 'Attach a file',
    /** What the paper clip says when pointed at: how many files and how large each may be. */
    attachHint: (count: number, size: string) => (count === 1 ? `Attach a file of up to ${size}` : `Attach up to ${count} files of up to ${size} each`),
    uploading: 'Uploading…',
    remove: 'Remove',
    save: 'Save',
    /** Above the files someone sent with a message. */
    attached: 'Sent with it',
    /** Above the files the agent wrote for the person, such as a text or a table. */
    forYou: 'Files for you',
    tooMany: (most: number) => (most === 1 ? 'Only one file can go with a message.' : `Only ${most} files can go with a message.`),
    tooLarge: (name: string, most: string) => `${name} is too large: a file may be up to ${most}.`,
    /** A file of a kind the agent of this site takes none of, such as a video. */
    kind: (name: string) => `${name} cannot go with a message: the agent of this site takes no files of this kind.`,
    failed: (name: string) => `${name} could not be uploaded. Try again in a moment.`,
  },

  /** What a request waits for besides its turn, while nobody works on it. */
  waits: {
    /** The AI service behind the agent has too much to do right now. */
    overloaded: 'The AI service is busy right now. It goes on by itself.',
    /** This site used up what its account with the AI service allows for now. */
    rateLimited: 'This site has reached the AI service’s limit for now. It goes on by itself later.',
    preparing: 'Getting ready…',
    /** The agent of this site does not answer; it is asked again. */
    unreachable: 'The agent does not answer right now. It is asked again in a moment.',
    other: 'Waiting…',
  },
  /** After one of the waits above, where it is known: the time it goes on. */
  until: (time: string) => `(until about ${time})`,
  send: 'Send',
  stop: 'Stop',
  stopping: 'Stopping…',
  goOnline: 'Put changes online right away',
  goOnlineOn: 'A change goes online as soon as it works.',
  goOnlineOff: 'A change is left in a draft, for you to try first.',
  /** A value in the lists under the box - the agent's model, say, named as the agent names it - that only runs with the site's password. */
  withPassword: (value: string) => `${value} (password)`,
  left: (left: number, perDay: number) => `${left} of ${perDay} left today`,
  noneLeft: (time: string) => `That is all for today. You can ask again from ${time}.`,
  /** Where the operator of the site switched building off, or there is no agent. */
  noBuilds: 'New apps are not built on this site. Connect your own AI assistant to build one.',
  /** Where the operator switched asking the agent about an app off. */
  noChanges: 'Asking the agent is switched off on this site. Change your app in its editor, or with your own AI assistant.',
  connectOwn: 'Connect your own assistant',

  /** The two sides of the conversation, for whoever cannot see who said what. */
  you: 'You',
  agent: 'Agent',
  ahead: (waiting: number) => (waiting === 1 ? 'One request ahead of yours - you are next.' : `${waiting} requests ahead of yours.`),
  leaveOpen: 'You can close this page: the agent keeps working, and its answer is here when you come back.',
  showWork: 'What it did',

  /** What came of a message, under the agent's answer. */
  results: {
    online: 'The change is online',
    undo: 'Undo',
    draft: 'Ready to try in a draft',
    draftBroken: 'The draft does not work yet',
    saved: 'The change is saved, but not online',
    putOnline: 'Put it online',
    earlier: 'Your app is back to how it was before',
    stopped: 'Stopped. Whatever it had saved is kept.',
    failed: 'That did not work.',
    /** The agent said it was done, but the app is as it was. */
    unchanged: 'Nothing was changed.',
    /** It stopped with an error after it had done part of the work. */
    brokeOff: 'It stopped with an error before it was done. Whatever it saved is kept.',
    /** It stopped with an error before it saved anything. */
    nothingSaved: 'It stopped with an error before it changed anything. Try again in a moment.',
    /** Opens the technical words of what went wrong, for whoever wants them. */
    whatWentWrong: 'What went wrong',
    timedOut: 'It ran out of time before it was done.',
    usedUp: 'It used up its steps before it was done.',
    unauthorised: 'The agent could not sign in, so nothing ran. That is this site’s fault, not your request’s.',
    /** A message whose answer never came back: the agent forgot it, or another took its place. */
    lost: 'No answer came back. Whatever the agent did is in the history of your app, in its editor.',
    /** The agent asks something before it goes on; its question follows. */
    asked: 'A question before it goes on',
    /** Under its question. */
    answerBelow: 'Answer in the box below, and it carries on from there.',
    /** What the app waits for after a message, and whom. */
    waitsFor: {
      /** The agent asked for the app to use other services on the internet; whoever runs this site decides. */
      connectivity: 'Waiting for permission to use other services - whoever runs this site decides.',
      /** The names are those of the keys and passwords the app's code reads, such as WEATHER_API_KEY. */
      keys: (names: string[]) => `Waiting for you to enter ${names.join(', ')}.`,
    },
    /** Carries on where a message stopped: it waited for something, or ran out of time. */
    goOn: 'Go on',
    goOnTitle: 'Carry on where it stopped, without asking again',
    /** Undoes what a message did, where it can. */
    undoTitle: 'Undo what this message did',
    undoDialog: 'Undo this message?',
    undoes: 'This is undone:',
    keeps: 'This stays as it is:',
    /** What undoing a message undoes. */
    undone: {
      online: 'What was online before goes back online.',
      draft: 'The draft it started is discarded.',
      connectivity: 'Its request to use other services is withdrawn.',
      source: 'The published code is taken down.',
      showcase: 'Your app leaves the showcase.',
      view: 'The editor opens as it did before.',
    } as Record<string, string>,
    /** What undoing a message leaves as it is, and why. */
    kept: {
      versions: 'What it changed stays in the history of your app, to go back to.',
      lambda: 'A new app it built stays.',
      data: 'Records, keys and passwords or saving files stay switched on - switching them off deletes them.',
      analytics: 'Counting visitors stays on - switching it off deletes what was counted.',
      workspace: 'Files it saved for your app stay.',
      draft: 'What it changed in a draft it did not start stays.',
      undone: 'It was undone already.',
    } as Record<string, string>,
    /** After undoing: what changed since the message stayed, as it was not as the message left it. */
    changedSince: 'Some of it changed since, so that stayed.',
    /** After undoing: what was online before could not go back online - it is gone, or does not run any more. */
    notUndone: 'What was online before could not be put back, so what is online now stays.',
    undoneDone: 'Undone.',
    /** A new app the agent built beside this one, by its address name; its editor link is in the answer. */
    created: (name: string) => `Built a new app: ${name}`,
    /** The settings it changed, each a small mark under the answer. */
    settings: {
      analytics: 'Counting visitors from now on',
      database: 'Records switched on',
      secrets: 'Keys and passwords switched on',
      workspace: 'Saving files switched on',
      source: 'Code published',
      connectivity: 'Asked for permission to use other services',
      discarded: 'Draft discarded',
      showcase: 'Listed in the showcase',
      /** It changed whether the editor opens simple or with everything. */
      view: 'The editor opens differently',
    },
    seeSource: 'See the code',
    /** After the first build: the editor link is remembered here as well. */
    remembered: 'This browser remembers it too, under Your apps.',
  },

  /** What the agent is doing while it works on an app, in the words of this page. */
  steps: {
    reading: 'Looking over your app',
    changing: 'Making the change',
    draft: 'Starting a draft',
    upToDate: 'Bringing the draft up to date',
    putOnline: 'Putting it online',
    discard: 'Discarding a draft',
    visitors: 'Looking at who visits',
    publish: 'Publishing the code',
    errors: 'Checking for errors',
  },
};
