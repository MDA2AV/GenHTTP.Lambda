import type { ReactNode } from 'react';

/**
 * /self-hosting: genhttp.dev and the community edition side by side. Says
 * that the platform can be run elsewhere and what differs, without
 * preferring either - whoever reads it picks what suits them. Production is
 * the tier an app people use is moved to, as the editor's tier badge calls it:
 * on genhttp.dev for non-profits on request, for companies by agreement.
 */
export const selfHosting = {
  /** The page's name in the menu of every page. */
  menu: 'Self-hosting',

  eyebrow: 'Open source · cloud or self-hosted',
  title: 'Use it here,',
  titleAccent: 'or self-host it.',
  intro:
    'GenHTTP Lambda is open source under the MIT license. genhttp.dev runs it for anybody to use, and the community edition is the same platform as a Docker image, for anybody who would rather run it themselves. Both build and host the same apps; what differs is where they live and who looks after them.',

  /** The two editions, side by side, in the same words and the same order. */
  cloud: {
    kicker: 'Cloud',
    name: 'genhttp.dev',
    summary: 'Open this site and start. The server, its updates and its limits are ours to look after.',
    points: ['Nothing to install or run', 'Apps answer at an address on genhttp.run', 'The build agent included'],
    action: 'Build something',
  },
  community: {
    kicker: 'Self-hosted',
    name: 'Community edition',
    summary: 'One Docker container on a machine of yours. The server, its updates and its limits are yours to look after.',
    points: ['One container, one volume, no database server', 'Apps answer at your address', 'The build agent as an extra container'],
    action: 'Run it yourself',
  },

  sameTitle: 'The same in both',
  sameText: 'The community edition is not a smaller version. Every app gets the same things wherever it runs:',
  /** What every app gets in both editions, a few words each. */
  same: [
    'The build agent',
    'Your own agent over MCP',
    'The REST API',
    'The editor, simple and full',
    'Drafts with test data',
    'Versions, and a way back',
    'Records, files, keys and passwords',
    'Analytics without cookies',
    'Domains of their own',
    'Git and downloads',
    'The showcase and published code',
  ],

  differTitle: 'What differs',
  /** The heading of the column of rows. */
  aspect: 'Question',
  /** Each row: what it is about, then genhttp.dev, then the community edition. */
  rows: (offline: number) => [
    ['Where the apps run', 'On our server', 'On yours, in one container with one volume'],
    ['Getting started', 'Open the build page - no account', 'Start the image, then open its address'],
    ['Who looks after the server', 'We do', 'You do'],
    ['What it costs', 'Nothing to try; production for non-profits on request, for companies by agreement', 'Nothing; the server is yours to pay for'],
    ['Where an app answers', 'At a subdomain of genhttp.run', 'Below your address, or at subdomains of a domain you give it'],
    ['A domain of its own', 'For apps in production, registered by you', 'For any app you move to production'],
    ['Apps nobody uses', `Offline after ${offline} days without a visit or a change, unless in production`, 'As you set it'],
    ['Limits and rules', 'Ours, and our terms of service', 'Yours, set in the admin panel'],
    ['The build agent', 'Included, a number of builds a day', 'Optional, with your own Claude subscription or API key'],
    ['Usage reports', 'None', 'Once a day: the version, the machine, the options and counts over the apps - never an app or a person. You can switch it off.'],
  ],

  moveTitle: 'Moving between them',
  move:
    'The same apps run on both, and your agent moves one with git: it creates the app on the other side, copies the files of the old one over and pushes them. A download is a .NET project that runs anywhere, with its records in a SQLite file.',

  pickTitle: 'Which one suits you',
  pickCloud: {
    title: 'genhttp.dev, if',
    points: [
      'you want an app online without running anything',
      'the people using it are anywhere',
      'you would rather not look after a server',
    ],
  },
  pickCommunity: {
    title: 'The community edition, if',
    points: [
      'your apps and their records should stay on a machine of yours',
      'they are tools for your team, inside your own network',
      'you want to set the limits and the rules yourself',
    ],
  },

  startTitle: 'Getting started',
  startCloud: {
    title: 'On genhttp.dev',
    text: 'Describe what you need on the build page, or connect the agent you already use. There is no account to make.',
  },
  startCommunity: {
    title: 'On your server',
    text: 'One command starts it. Open its address, connect your agent and build - the README says the rest.',
  },
  connect: 'Connect your agent',
  readme: 'Read the README',
  dockerHub: 'The image on Docker Hub',

  sourceTitle: 'Open source',
  /** Under the comparison: the license, and where the code is. */
  source: (github: (text: string) => ReactNode) => (
    <>
      GenHTTP Lambda is released under the MIT license. Its code, its issues and its releases are on {github('GitHub')}.
    </>
  ),
};
