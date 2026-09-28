import type { ReactNode } from 'react';

export const ship = {
  title: 'From your laptop to everyone’s screen.',
  intro:
    'You built something with your coding agent and it only runs on your machine. Ask the agent to publish it here. A few minutes later it has a public link anyone can open, and it can remember things, so people can play, chat and post in it together.',
  facts: ['Free', 'No account', 'Nothing to install'],
  connect: 'Connect your agent',
  seeOthers: 'See what others shipped',

  stepsTitle: 'Three moves, and one of them is a sentence',
  step: (n: number) => `Step ${n}`,
  steps: [
    {
      title: 'Connect once',
      body: 'Add one address to Claude, Cursor or whichever agent you work with. It takes under a minute and you only ever do it once.',
    },
    {
      title: 'Ask it to publish',
      body: 'Tell it to put the app online here. It packs up your app, publishes it and checks that it answers.',
    },
    {
      title: 'Share the link',
      body: 'You get a public address and a private editor link. Send the first to anyone. Keep the second, it is how you change the app later.',
    },
  ],

  togetherTitle: 'Not just a page. A place people meet.',
  together:
    'Most hosts hand out a copy of your app to each visitor, and everybody plays alone. Here every app has its own memory and a live line to everyone who has it open. A move one person makes shows up for all the others straight away, and what they post is still there tomorrow.',
  together2:
    'No database to sign up for, no second service to wire in. Ask for it the way you would describe it to a friend.',
  /** What people build together, and how they would ask for it. */
  kinds: [
    { name: 'Multiplayer games', ask: 'Let up to eight friends join the same round and see each other’s moves live.' },
    { name: 'Chat rooms', ask: 'Add a room where everyone with the link can talk, and keep the last hundred messages.' },
    { name: 'Shared lists', ask: 'Turn the packing list into one the whole team edits at once.' },
    { name: 'Scores and records', ask: 'Keep a leaderboard with everyone’s best time and show the top ten on the start screen.' },
    { name: 'Little social networks', ask: 'Let wedding guests post photos to one wall and like each other’s.' },
  ],
  quote: (text: string) => `“${text}”`,

  connectTitle: 'Connect your agent once',
  connectText:
    'Give your agent this address. From then on it knows how to publish here, no key and no sign-in needed.',
  sayLike: 'Then, in your project, say something like',
  asks: [
    'Publish this app on GenHTTP Lambda and send me the link.',
    'Make the high scores shared, so everyone sees the same leaderboard.',
  ],

  domainChip: 'When it catches on',
  domainTitle: 'Give it a name of its own',
  domainText:
    'The same app, the same editor link, but served at an address you own. Easier to say out loud, easier to remember, and it looks the part when people start sharing it.',
  domainSubject: 'A domain for my app',
  domainAsk: 'Ask about your domain',

  questionsTitle: 'Before you ask',
  questions: (
    offline: number,
    removed: number,
    showcase: (text: string) => ReactNode,
    terms: (text: string) => ReactNode,
  ): [string, ReactNode][] => [
    [
      'Is it really free?',
      <>
        Yes. No card, no trial and no account. Your app stays online for as long as people use it. After {offline} days
        without a single visit or change it is taken offline, and after {removed} it is removed.
      </>,
    ],
    [
      'Does my app have to be built a certain way?',
      'No, your agent takes care of that. Pages, pictures and styles go up as they are, and anything that has to run on the server is adapted by the agent for this platform. You describe what the app should do and it does the translating.',
    ],
    [
      'How do I change it later?',
      'With the editor link you got when it was published. Hand it to your agent with the next change, or open it in your browser. Every change becomes a new version at the same address, and you can go back to an older one at any time.',
    ],
    [
      'Who can see my app?',
      <>Anyone you give the link to. It is not listed anywhere unless you choose to add it to the {showcase('showcase')}.</>,
    ],
    [
      'Is there anything I cannot publish?',
      <>A few things, like anything that harms or deceives people. The {terms('terms')} are short and in plain words.</>,
    ],
  ],

  closeTitle: 'It works on your machine.',
  closeAccent: 'Let it work on theirs.',
  noAgent: 'No agent? Build it here',
  closeFacts: 'Free. No account. Nothing to install.',

  /** The scene at the top: a request, the address going public, people arriving. */
  scene: {
    label: 'An agent is asked to publish an app. The address changes from localhost to a public link, and people join.',
    ask: 'Put my quiz game online so my friends can join.',
    live: 'It’s live. Here’s your link.',
    publishing: 'Publishing…',
    public: 'Public',
    onlyYou: 'Only you',
    app: 'Friday quiz night',
    /** Who is in the app, around the number: "3 playing", where Japanese writes "3人がプレイ中". */
    playing: (count: ReactNode) => <>{count} playing</>,
    you: 'You',
  },

  compareTitle: 'The shortest way from “it works” to “try it”',
  compareText:
    'Vercel, Cloudflare and Lovable are great places to run things. They also start with a sign-up form, and the moment your app needs to share anything between visitors, with a second service to set up. Here is how it looks when you start from nothing.',
  rows: [
    'Start without an account',
    'Publish from the agent you already use',
    'Live shared data: chat, multiplayer, records',
    'Cost to get your first link',
  ],
  us: ['Yes', 'Connect once, then just ask', 'Built into every app', 'Free'],
  /** Vercel, Cloudflare and Lovable, in that order, a cell for each row. */
  rivals: [
    ['Sign-up needed', 'After signing in its tools', 'Add a database service', 'Free plan'],
    ['Sign-up needed', 'After signing in its tools', 'Possible, with setup', 'Free plan'],
    ['Sign-up needed', 'Built in its own editor', 'Through a connected backend', 'Free plan, limited credits'],
  ],
  compareNote:
    'As of September 2026, for somebody without an account anywhere. Plans and features of other services change; check with them for the details.',
};
