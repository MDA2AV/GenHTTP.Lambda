import type { ReactNode } from 'react';

/**
 * /platform, the features of genhttp.dev: what every app gets beyond being
 * built and changed - for both groups, in the words of the simple view (a
 * draft, test data, putting online, records, keys and passwords), with the
 * developer's words only where a feature is about code. Every screenshot
 * shows one example app, the court booking of a tennis club that does not
 * exist.
 */
export const platform = {
  /** The page's name in the menu of every page. */
  menu: 'Features',

  eyebrow: 'Features',
  title: 'Building it takes a minute.',
  titleAccent: 'Everything else is built in.',
  intro:
    'An app made here is more than the code an AI wrote. What keeps it useful comes after: its records, its versions and what it is for, the people who visit it, a domain of its own. Every app has all of it, in one place opened with the link you get when it is built.',
  build: 'Build something',
  ownAgent: 'Use your own agent',
  free: 'Free. No sign-up, no credit card.',
  heroAlt: 'The editor of a court booking app: online at the club’s own domain, how many people used it today, and its latest change',
  /** Under the first picture: every picture on the page shows the same app, an example made up for it. */
  example: 'Every picture on this page shows the same example app: the court booking of a tennis club.',

  /** Above the list of every feature, which leads to their sections. */
  contents: 'On this page',

  makeTitle: 'Build it, change it',
  makeText: 'Say what you need, and keep saying it. Nobody has to write or read code for either.',

  behindTitle: 'Behind every app',
  behindText:
    'What keeps an app useful is everything around its code. Every app has all of it from the first minute - for whoever made it, not only for whoever can read code.',

  yoursTitle: 'Yours to take further',
  yoursText: 'For developers, and for anybody who wants their app to outlive this site.',

  /**
   * Each feature: its name in the list at the top (one or two words), its
   * section, and what its screenshot shows (alt) for whoever cannot see it.
   */
  features: {
    build: {
      name: 'Build',
      title: 'Describe it, and it is online',
      text: 'Tell our agent what you need, in your own words - or ask Claude Code, Codex or Cursor. A few minutes later the app runs at an address of its own, ready to share. There is no server, database or account to set up.',
      points: ['Our agent on the build page, or your own over MCP', 'A link to share in minutes', 'Free, without signing up'],
      alt: 'The court booking page: four courts, hour by hour, with the booked hours named',
    },
    change: {
      name: 'Change',
      title: 'Change it by asking',
      text: 'Every app has an editor, opened with the private link you got when it was built. Say what should be different, and the agent changes the app and tells you when to look - or hand the link to your own agent with the next change.',
      points: ['A sentence is enough', 'Online by itself once it works, or kept for you to try first', 'The code stays out of sight unless you want it'],
      alt: 'The Change section: the club asked for the coach’s lessons, and the agent says the change is ready to try',
    },
    drafts: {
      name: 'Drafts',
      title: 'Try a change before anyone sees it',
      text: 'A bigger change is made in a draft: the app as it will be, at an address of its own, with a copy of the records to try it on. Nothing done there touches the real ones. When it is right, one click puts it online.',
      points: ['An address of its own, to try it and to show others', 'Test data: a copy of the records, not the records', 'Put online when it is right, or discard it'],
      alt: 'A draft of the booking page, where the coach’s lessons are shown in purple - in a copy of the bookings',
    },
    history: {
      name: 'History',
      title: 'Every change a version, with a way back',
      text: 'Each change becomes a new version, described by what it changed. If one goes wrong, the version before it is one click away - at the same address, with the same records.',
      points: ['What was asked for, and what it changed', 'Back to an earlier version in one click', 'Records are shared by every version, so going back loses none'],
      alt: 'The history of the app: its changes, one under the other, the newest online',
    },
    about: {
      name: 'About',
      title: 'It knows what it is',
      text: 'With every change the agent writes down what the app is for, who uses it and why it works the way it does. You read it in plain words, and the next agent reads it before it changes anything - so a change builds on what is there instead of guessing.',
      points: ['What it does and for whom, in plain words', 'The decisions behind it, for whoever changes it next', 'How it is tested, kept with each version'],
      alt: 'About your app: the court booking described in plain words, who uses it and what it does',
    },
    records: {
      name: 'Records',
      title: 'Records in a database of its own',
      text: 'An app that keeps things - bookings, sign-ups, scores - keeps them in a database of its own, and they stay when the app changes. Read them table by table in the editor, next to the files it saved, such as the weekly list for the noticeboard.',
      points: ['A database for every app that needs one', 'Every table, readable in the editor', 'The files the app saves, listed beside them'],
      alt: 'The records of the app: its bookings and its members, read table by table',
    },
    keys: {
      name: 'Keys and passwords',
      title: 'Keys and passwords stay secret',
      text: 'When an app needs a password or the key to another service, the agent only asks for it by name. You type the value into the editor, and from then on nobody sees it - not you, not the agent. Only the app reads it.',
      points: ['The agent never sees the value', 'Kept encrypted, never shown again', 'Replaced whenever you like'],
      alt: 'Keys and passwords: the front desk’s password is set, and its value is hidden',
    },
    analytics: {
      name: 'Analytics',
      title: 'See who visits, without cookies',
      text: 'Switch on analytics and see how many people opened the app, where they came from, and on which devices - counted on our server, without cookies and without a script in your pages.',
      points: ['Visitors, page views and where they came from', 'No cookies, and nothing that names a visitor', 'Off until you switch it on'],
      alt: 'Analytics: the visitors of the last thirty days, day by day',
    },
    domain: {
      name: 'Domain',
      title: 'A domain of its own',
      text: 'Every app starts at an address of ours. When it catches on, it can move to a domain you own: the same app and the same editor, at an address people remember. Links to the old address keep working - they lead to the new one.',
      /** Premium is what genhttp.dev calls the tier above the free one: a name, written as it is in every language. */
      points: ['For Premium apps: write to us', 'The same app, editor and records', 'The old address sends visitors on'],
      alt: 'The address of the app moving from its address on genhttp.run to the club’s own domain',
    },
    code: {
      name: 'Code and git',
      title: 'The code, whenever you want it',
      text: 'Switch the editor to its full view, and the code of every version is there, with its logs and its figures. Clone the app with git and push a change, or download it as a .NET project that runs anywhere. It is your code.',
      points: ['Every file, compared from version to version', 'Git: a commit to main is a version, a branch is a draft', 'A download that runs on its own, records included'],
      alt: 'The full view of the editor: the files of the app and its code',
    },
    publish: {
      name: 'Showcase and source',
      title: 'Show it, or publish its code',
      text: 'Put the app in the showcase for others to find, or publish its code under an open source license for anybody to read, star and download. Both are your choice, and neither shares its records.',
      points: ['In the showcase, with a picture and a description', 'Its code under MIT or another license', 'Never its records, keys or visitors'],
      alt: 'The published code of the court booking: its files, its documentation and its versions, under the MIT license',
    },
  },

  /** On the address bar above the picture of a draft. */
  draftBadge: 'Draft',
  /** On the address bar above the domain the app moved to: the name of the tier, written as it is in every language. */
  domainOwn: 'Premium',
  domainSubject: 'A domain for my app',
  domainAsk: 'Ask about a domain',
  zoom: 'Show larger',
  close: 'Close',

  closeTitle: 'Free to try.',
  closeAccent: 'No sign-up.',
  closeBuild: 'Build something',
  closeShip: 'Publish what you built',
  /** At the end of the page: the platform is open source, and can be run elsewhere. */
  openSource: (selfHosting: (text: string) => ReactNode, github: (text: string) => ReactNode) => (
    <>
      GenHTTP Lambda is open source. Use it here, or {selfHosting('run it on your own server')} - the code is on{' '}
      {github('GitHub')}.
    </>
  ),
};
