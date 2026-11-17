import type { ReactNode } from 'react';

/**
 * /ship, the guide for developers who built an app with their own agent and
 * want it online: connect the agent, ask it to publish, share the link - in
 * the words they search for (hosting a vibe-coded app, from localhost, with a
 * database and multiplayer built in).
 */
export const ship = {
  eyebrow: 'Free hosting for vibe-coded apps',
  title: 'From localhost to everyone’s screen.',
  intro:
    'You built an app with Claude Code, Codex or Cursor, and it only runs on your machine. Ask your agent to publish it here. A few minutes later it has a public link for anyone, its own database and a live line to everyone who has it open, so people can play, chat and post in it together.',
  facts: ['Free', 'No sign-up', 'No credit card', 'Nothing to install'],

  stepsTitle: 'Three steps from localhost to a public link',
  step: (n: number) => `Step ${n}`,
  steps: [
    {
      title: 'Connect once',
      body: 'Add one address, a remote MCP server, to Claude Code, Codex, Cursor or whichever agent you work with. It takes under a minute and you only ever do it once.',
    },
    {
      title: 'Ask it to publish',
      body: 'Tell it to put the app online here. It packs up your app, deploys it and checks that it answers - no GitHub repo, no deploy pipeline, no Docker.',
    },
    {
      title: 'Share the link',
      body: 'You get a public address and a private editor link. Send the first to anyone. Keep the second, it is how you change the app later.',
    },
  ],

  togetherTitle: 'Not just hosting. A database and multiplayer, built in.',
  together:
    'Most hosts hand out a copy of your app to each visitor, and everybody plays alone: what one browser keeps in localStorage, the next one never sees. Here every app has its own database and a live line to everyone who has it open. A move one person makes shows up for all the others straight away, and what they post is still there tomorrow.',

  sayLike: 'Then, in your project, say something like',
  asks: [
    'Publish this app on GenHTTP Lambda and send me the link.',
    'Make the high scores shared, so everyone sees the same leaderboard.',
  ],

  questionsTitle: 'Before you publish',
  questions: (
    offline: number,
    removed: number,
    showcase: (text: string) => ReactNode,
    terms: (text: string) => ReactNode,
    selfHosting: (text: string) => ReactNode,
  ): [string, ReactNode][] => [
    [
      'Is it really free?',
      <>
        Yes. No sign-up, no credit card and no trial. Your app stays online for as long as people use it. After{' '}
        {offline} days without a single visit or change it is taken offline, and after {removed} it is removed.
      </>,
    ],
    [
      'Can Claude Code, Codex or Cursor deploy my app here?',
      'Yes, and any other agent that can add a remote MCP server. Connect it once with the address above, then ask it to publish: it deploys the app, checks that it answers and sends you the link.',
    ],
    [
      'Why can’t my friends open my localhost link?',
      'Because localhost is your own computer: the address only works there, and only while the app is running. A tunnel lends it a public address for as long as your laptop stays on. Published here, the app runs on our servers with a link that keeps working when your laptop is closed.',
    ],
    [
      'Do I need a server, a backend or Supabase?',
      'No. Every app gets its own database, file storage and a live line to everyone who has it open. There is no server to rent and no second service to set up - and nothing to keep running on your side either.',
    ],
    [
      'Can I make my game multiplayer without running a server?',
      'Yes. What one browser keeps in localStorage the next one never sees, so the shared part has to live on a server - here it is ours. Ask your agent to make the game multiplayer, and every move reaches everyone who has it open.',
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
      'Can I push to it with git?',
      'Yes. Every app is a git repository as well: clone it from the address under Clone in the editor, change it with your own tools or agent, and push. Each commit pushed to main becomes the next version, and a branch you push becomes a draft with a preview of its own.',
    ],
    [
      'Where do my API keys go?',
      'Not into the code. Your agent asks for a key by name, and you type the value into the editor. Nobody reads it back - not the editor, not the agent.',
    ],
    [
      'Can I take my code with me?',
      'Yes, it is yours. Download it from the editor whenever you like, as a project that runs on its own, database included - or clone it with git, every version included.',
    ],
    [
      'Can I host it on my own server instead?',
      <>
        Yes. GenHTTP Lambda is open source, and its community edition is the same platform as one Docker container, for
        your own server and your own agent. {selfHosting('Cloud or self-hosted')} says what differs.
      </>,
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

  /** The way to the workspace on /build, for whoever has no agent of their own. */
  noAgent: 'No agent? Build it here',
};
