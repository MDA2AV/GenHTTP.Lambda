import type { ReactNode } from 'react';

type Node = ReactNode;

/**
 * The simple view: the app, how it is doing and a box to ask for a change,
 * for somebody who had it built and does not read code. Nothing here says
 * version, deployment, file or log - a version is a change, putting one
 * online is putting it online, and going back to one is going back.
 */
export const simple = {
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
  onlineFor: (span: Node) => <>Online for {span}.</>,
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

  /** The tile that offers hosting it professionally, while the app is not premium. */
  premiumHeading: 'When it catches on',
  premiumChip: 'Premium',
  premiumTitle: 'Host it professionally',
  premiumText:
    'Your app at an address of your own, like yourapp.com - kept online however quiet it gets, with more room for everything it stores.',
  premiumAsk: 'Write to us',
  premiumSubject: 'Premium hosting for my app',
  premiumBody: (address: string) => `Hello,\n\nI would like to host my app professionally: ${address}\n\n`,

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
};
