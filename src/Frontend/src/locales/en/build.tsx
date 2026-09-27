import type { ReactNode } from 'react';

export const build = {
  offTitle: 'Not switched on here',
  off: (write: (text: string) => ReactNode, mcp: ReactNode) => (
    <>
      This installation has no build agent. You can still {write('write it yourself')}, or point your own Claude at{' '}
      {mcp}.
    </>
  ),

  title: 'Say what you want.',
  intro:
    'It gets built, put online, and you get a link you can send to anyone. No account, no install, and it can remember things - scores, messages, entries - so everybody who opens it sees the same thing.',
  placeholder: 'build a…',
  working: 'working…',
  shortcut: 'ctrl + enter',
  building: 'Building',
  buildIt: 'Build it',
  builtBy: 'Built by',
  password: 'password',
  fable:
    'Fable is behind a password while it is being tried out. It runs with no time limit, so it will keep going until the thing is finished rather than until the clock runs out.',
  onlyNew:
    'This only builds new ones. To take something you have already made further, give its editor link to your own coding agent - see below.',
  ideas: [
    'a wall where anyone can leave a one line message',
    'a highscore board for a dice game',
    'a poll where people vote and see the totals',
    'a guestbook for my wedding',
    'a countdown to a date everyone can see',
  ],
  ahead: (waiting: number) =>
    waiting === 1 ? 'One build ahead of yours - you are next.' : `${waiting} builds ahead of yours.`,
  starting: 'Starting…',

  yourApp: 'Your app',
  further: 'To take it further',
  keep: 'Keep that one. It is the only way back in and it cannot be recovered - not by us either. Bookmark it before you close this tab.',
  change:
    'This page only builds new things. To change this one, connect your own coding agent as described below, hand it the editor link and tell it what you want different.',
  copyLink: 'Copy the editor link',
  lifetime: (offline: number, removed: number) =>
    `It stays online while it is used: after ${offline} days without visits or changes it goes offline, and after ${removed} it is removed. Open the editor and press deploy to put it back up.`,
  openEditor: 'Open the editor',
  another: 'Build something else',

  keepGoing: 'Keep going with your own agent',
  orOwn: 'Or use your own agent',
  ownText:
    'The box above is a Claude running on this machine. If you already have one of your own, point it here instead and it can do the same things - make a lambda, write the code, put it online - without a daily limit and without going through this page.',
  thenAsk: 'Then ask it for what you want, the same way you would here.',
  claudeWeb: 'Claude on the web',
  claudeWebHow:
    'Settings, then Connectors, then Add custom connector. Paste the address above as the remote MCP server URL. There is no key and no sign in step.',
  howToChange: 'That is also how to change something once it is built: give your agent the editor link and tell it what to do.',
  more: 'More about using an agent here',

  failedToStart: 'That did not go through.',
  noAnswer: 'It finished without saying what happened.',
  failed: 'That did not work.',
};
