import type { ReactNode } from 'react';

export const landing = {
  eyebrow: 'An agentic coding platform',
  headline: 'Describe an app.',
  headlineAccent: 'Your agent puts it online.',
  intro:
    'Polls, guestbooks, leaderboards, small shops. Describe what you need to our agent or to the one you already use, and receive a working app with a shareable link. Your app stays editable, so you can keep refining it long after the first version.',
  build: 'Build something',
  ownAgent: 'Use your own agent',
  free: 'Free to use. No account, nothing to install.',
  seeIt: 'See it in action',

  videoTitle: 'From a sentence to a live app',
  videoText:
    'A private browser window, no account, and a single request on the build page - followed by the finished app, opened from its link just as any visitor would.',
  videoNote: 'The build is shown sped up. Everything else is in real time.',
  tryIt: 'Try it yourself',

  oneShotTitle: 'Not a one-shot',
  oneShotText:
    'Most generators produce a result and leave you with it. Here the app keeps running where it was built, so you and your agent can continue working on it.',
  /** How one app goes, from the sentence to the third change. */
  steps: [
    {
      title: 'Say what you want',
      body: 'Describe it in plain language, either to the agent on this site or to the one you already use. No code, no setup and no account required.',
      alt: 'The build page with a request for a lunch poll typed in',
    },
    {
      title: 'Get a working app and a link',
      body: 'The app is built, deployed and returned as a public address you can share. It keeps its data - votes, scores, messages - so everyone who opens it sees the same state.',
      alt: 'The finished lunch poll, open in a browser',
    },
    {
      title: 'Keep improving it',
      body: 'Every app comes with a private editor link. Hand it to your agent along with the next change, or open it yourself. Each change becomes a new version, and the address stays the same.',
      alt: "The poll's control center: its versions, each with what was asked for, what it changed and the difference to the one before",
    },
  ],
  weekLater: 'A week later',
  weekAsk:
    'Here is the editor link for my lunch poll. Please close voting at 11 on Fridays and show the winner at the top.',
  weekAnswer:
    'Done. Version 4 is live at the same address, and version 3 is still available if you want to roll back.',

  agentsTitle: 'Bring your favourite agent',
  agentsText:
    'Already working with Claude or another assistant? Connect it to this address and it can build, deploy and update apps here - directly from the conversation you already have open.',
  thenAsk: (em: (text: string) => ReactNode) => (
    <>Then simply ask: {em('build a sign-up sheet for our team event and put it online')}.</>
  ),

  contactTitle: 'Talk to us',
  contactText:
    'Need help, planning something larger, or looking for a solution built for you? We would be glad to hear from you.',
  mailTitle: 'Email us',
  mailText: 'For projects, enquiries and anything you would prefer to discuss privately.',
  discordTitle: 'Join the Discord',
  discordText: 'Share what you have built, get help with the next step, and talk directly with the team.',
  discordLink: 'The GenHTTP Discord',
};
