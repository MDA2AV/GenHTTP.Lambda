/**
 * Building a new app in the workspace on /build, where there is an agent to
 * build it: what is asked, the steps it takes, and how it ended - in the words
 * of somebody who wants a website, as the editor’s Change section says them.
 */
export const buildBox = {
  placeholder: 'I would like a website that…',
  password: 'password',
  /** Under the box, while a model that only runs with the site's password is chosen. */
  unlimited:
    'This model needs the password, and then runs with no time limit: it keeps going until your website is finished rather than until the clock runs out.',
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
    /** The agent asked for the website to use other services on the internet, which the operator of the site allows. */
    services: 'Asking to use other services',
  },

  /** Above the editor link, once the website is built. */
  further: 'Your editor link',
  keep: 'Keep that link somewhere safe. It is the only way into your website from another device, and it cannot be recovered - not by us either.',
  copyLink: 'Copy the editor link',
  lifetime: (offline: number, removed: number) =>
    `We keep it online for as long as it is used: after ${offline} days without visits or changes it is taken offline, and after ${removed} it is removed. You can put it back online here, or in its editor.`,

  failedToStart: 'That did not go through.',
  noAnswer: 'It finished without saying what happened.',
  failed: 'That did not work.',
};
