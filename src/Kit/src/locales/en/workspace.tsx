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
  send: 'Send',
  stop: 'Stop',
  stopping: 'Stopping…',
  enterHint: 'Enter to send · Shift + Enter for a new line',
  goOnline: 'Put changes online right away',
  goOnlineOn: 'A change goes online as soon as it works.',
  goOnlineOff: 'A change is left in a draft, for you to try first.',
  model: 'Model',
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
    undoTitle: 'Put what was online before back online',
    draft: 'Ready to try in a draft',
    draftBroken: 'The draft does not work yet',
    saved: 'The change is saved, but not online',
    putOnline: 'Put it online',
    earlier: 'Your app is back to how it was before',
    stopped: 'Stopped. Whatever it had saved is kept.',
    failed: 'That did not work.',
    timedOut: 'It ran out of time before it was done.',
    usedUp: 'It used up its steps before it was done.',
    unauthorised: 'The agent could not sign in, so nothing ran. That is this site’s fault, not your request’s.',
    /** A message whose answer never came back: the agent forgot it, or another took its place. */
    lost: 'No answer came back. Whatever the agent did is in the history of your app, in its editor.',
    /** The settings it changed, each a small mark under the answer. */
    settings: {
      analytics: 'Counting visitors from now on',
      database: 'Records switched on',
      secrets: 'Keys and passwords switched on',
      workspace: 'Saving files switched on',
      source: 'Code published',
      connectivity: 'Asked for permission to use other services',
      discarded: 'Draft discarded',
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
