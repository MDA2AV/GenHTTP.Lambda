/**
 * The box on the page that creates a website, where there is an agent to
 * build it: what is asked, the steps it takes, and how it ended - in the words
 * of somebody who wants a website, as the editor’s Change section says them.
 */
export const buildBox = {
  placeholder: 'I would like a website that…',
  shortcut: 'ctrl + enter',
  buildIt: 'Create my website',
  builtBy: 'Created by',
  password: 'password',
  fable:
    'Fable is behind a password while it is being tried out. It runs with no time limit, so it will keep going until your website is finished rather than until the clock runs out.',
  onlyNew:
    'This creates new websites. To change one you already have, open its editor link and describe what should be different under Change.',
  ideas: [
    'a website for our club where members sign up for events',
    'a sign-up sheet for our potluck, so nobody brings the same dish',
    'a guestbook for our wedding',
    'a poll where people vote and see the totals',
    'a leaderboard for our weekly quiz night',
    'a birthday page where friends leave their wishes',
  ],
  ahead: (waiting: number) =>
    waiting === 1 ? 'One website ahead of yours - you are next.' : `${waiting} websites ahead of yours.`,
  starting: 'Starting…',
  asked: 'You asked for',
  leaveOpen: 'Keep this page open: the link to change your website later is shown here, once it is done.',
  log: 'What it did',
  online: 'Your website is online',
  notOnline: 'Your website was created, but it did not go online.',
  open: 'Open your website',
  /** What the agent is doing, in the words of this page rather than those of its tools. */
  steps: {
    guide: 'Getting ready',
    examples: 'Looking at examples',
    create: 'Choosing an address for your website',
    write: 'Writing your website',
    improve: 'Improving your website',
    check: 'Checking it for mistakes',
    online: 'Putting it online',
    trying: 'Trying it out',
    looking: 'Looking over your website',
    forRecords: 'Making room for its records',
    forKeys: 'Making room for keys and passwords',
    forFiles: 'Making room for what it saves',
    records: 'Looking at its records',
    keys: 'Checking which keys and passwords it needs',
    addFile: 'Adding a file',
    removeFile: 'Removing a file',
    files: 'Looking at what it saved',
  },

  yourApp: 'Your website',
  further: 'To change it later',
  keep: 'Keep that link. It is the only way back in and it cannot be recovered - not by us either. Bookmark it before you close this tab.',
  change:
    'To change your website, open the editor link and describe what should be different under Change, the same way as here. Your own AI assistant can do it too, as described below.',
  copyLink: 'Copy the editor link',
  lifetime: (offline: number, removed: number) =>
    `We keep it online for as long as it is used: after ${offline} days without visits or changes it is taken offline, and after ${removed} it is removed. Open the editor to put it back online.`,
  openEditor: 'Open the editor',
  another: 'Create another website',

  keepGoing: 'Keep going with your own AI assistant',
  orOwn: 'Or use your own AI assistant',
  ownText:
    'Already use Claude or another AI assistant? Connect it here and it creates and changes websites for you in the same way - we host them, so there is still nothing to set up. There is no daily limit.',
  ownTitle: 'Create your website with your AI assistant',
  ownOnly:
    'Connect Claude or another AI assistant to the address below, then describe the website you would like. It creates it, we host it on our servers, and it is online right away with a link to share.',
  thenAsk: 'Then tell it what you would like, for example: “Create a website for our choir with a calendar of our concerts.”',
  howToChange: 'That is also how to change a website later: give your assistant the editor link and tell it what should be different.',

  failedToStart: 'That did not go through.',
  noAnswer: 'It finished without saying what happened.',
  failed: 'That did not work.',
};
