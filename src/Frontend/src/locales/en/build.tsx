/*
 * The words of /build, for somebody who wants a website and does not write
 * code. They say what happens in the words such a person would search for -
 * creating a website, hosting, putting it online - and leave out the words
 * of the people who make such things.
 */
export const build = {
  title: 'From idea to website.',
  intro:
    'Describe the website or app you have in mind. AI creates it for you, we host it on our servers, and it is online right away - with a link you can send to anyone. No coding, no hosting to set up, no account.',
  placeholder: 'I would like a website that…',
  working: 'working…',
  shortcut: 'ctrl + enter',
  building: 'Creating',
  buildIt: 'Create my website',
  builtBy: 'Created by',
  password: 'password',
  fable:
    'Fable is behind a password while it is being tried out. It runs with no time limit, so it will keep going until your website is finished rather than until the clock runs out.',
  onlyNew:
    'This creates new websites. To change one you already have, open its editor link and describe what should be different under Change.',
  ideas: [
    'a website for our club where members sign up for events',
    'a guestbook for our wedding',
    'a poll where people vote and see the totals',
    'a scoreboard for our weekly quiz night',
    'a countdown to our opening day that everyone can see',
  ],
  ahead: (waiting: number) =>
    waiting === 1 ? 'One website ahead of yours - you are next.' : `${waiting} websites ahead of yours.`,
  starting: 'Starting…',

  points: [
    {
      title: 'Described, not programmed',
      text: 'Say in your own words what your website should do. No coding and no technical knowledge needed.',
    },
    {
      title: 'Hosting included',
      text: 'Your website runs on our servers. Hosting, security and updates are taken care of - there is nothing for you to set up or look after.',
    },
    {
      title: 'Online in minutes',
      text: 'You get a link to share straight away. It can remember things too - entries, votes, scores - so everybody sees the same.',
    },
  ],

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
