/**
 * Building a new app in the workspace on /build, where there is an agent to
 * build it: what is asked, the steps it takes, and how it ended - in the words
 * of somebody who wants an app, as the editor’s Change section says them.
 */
export const buildBox = {
  placeholder: 'I would like an app that…',
  password: 'password',
  /** Under the box, while something chosen in the lists under it only runs with the site's password. */
  unlimited: 'This choice needs the site’s password, and is then given the time and steps set for it rather than the usual ones.',
  ideas: [
    'an app for our club where members sign up for events',
    'a sign-up sheet for our potluck, so nobody brings the same dish',
    'a guestbook for our wedding',
    'a poll where people vote and see the totals',
    'a leaderboard for our weekly quiz night',
    'a birthday page where friends leave their wishes',
  ],
  ahead: (waiting: number) =>
    waiting === 1 ? 'One app ahead of yours - you are next.' : `${waiting} apps ahead of yours.`,
  starting: 'Starting…',
  online: 'Your app is online',
  notOnline: 'Your app was created, but it did not go online.',
  open: 'Open your app',
  /** What the agent is doing, in the words of this page rather than those of its tools. */
  steps: {
    guide: 'Getting ready',
    examples: 'Looking at examples',
    create: 'Choosing an address for your app',
    write: 'Writing your app',
    improve: 'Improving your app',
    check: 'Checking it for mistakes',
    online: 'Putting it online',
    trying: 'Trying it out',
    looking: 'Looking over your app',
    forRecords: 'Making room for its records',
    forKeys: 'Making room for keys and passwords',
    forFiles: 'Making room for what it saves',
    records: 'Looking at its records',
    keys: 'Checking which keys and passwords it needs',
    addFile: 'Adding a file',
    removeFile: 'Removing a file',
    files: 'Looking at what it saved',
    /** The agent asked for the app to use other services on the internet, which the operator of the site allows. */
    services: 'Asking to use other services',
    /** The agent is reading a page of a website - one the person named, such as their old homepage, or one it found; site is its address, like www.example.com. */
    site: (site: string) => `Looking at ${site}`,
    /** The agent searches the web for something the app needs. */
    search: 'Looking something up on the web',
    /** The agent looks at a file the person sent with their message, such as a logo or a list. */
    attachment: 'Looking at the file you sent',
    /** The agent lists the app in the showcase of the site, which the person asked for. */
    showcase: 'Putting it in the showcase',
    /** The agent writes a file for the person, such as a text or a table, which is shown beside its answer. */
    file: 'Writing a file for you',
  },

  /** Above the editor link, once the app is built. */
  further: 'Your editor link',
  keep: 'Keep that link somewhere safe. It is the only way into your app from another device, and it cannot be recovered - not by us either.',
  copyLink: 'Copy the editor link',
  lifetime: (offline: number, removed: number) =>
    `We keep it online for as long as it is used: after ${offline} days without visits or changes it is taken offline, and after ${removed} it is removed. You can put it back online here, or in its editor.`,

  failedToStart: 'That did not go through.',
  noAnswer: 'It finished without saying what happened.',
  failed: 'That did not work.',

  /**
   * The same box on an installation an organization runs for its people
   * (its operator chose "enterprise"): what colleagues build for their work
   * rather than for a club or a party. Only what differs from the sentences
   * above.
   */
  profiles: {
    enterprise: {
      placeholder: 'We need an app that…',
      ideas: [
        'a form to request time off, approved by the team lead',
        'a booking calendar for our meeting rooms',
        'a checklist for onboarding new colleagues',
        'an inventory of our equipment and who has it',
        'a list of open support requests and who is on each',
        'a feedback form for our training sessions',
      ],
    },
  },
};
