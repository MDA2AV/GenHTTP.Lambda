import type { ReactNode } from 'react';

/**
 * The front page of an installation an organization runs for its people (its
 * operator chose "enterprise"), for the colleagues who come to have their
 * tools built: what they can have built for their work, how it goes, and
 * what they ask first. For people who do not write code, in the words of the
 * editor's simple view - an app, a draft, putting it online, records - and
 * nothing about running the platform, which is the operators' business.
 */
export const enterpriseLanding = {
  /** Above the heading, where the installation has no name of its own to show there. */
  eyebrow: 'Apps for your work',
  headline: 'Describe the app your work needs.',
  headlineAccent: 'An AI agent builds it and puts it online here.',
  intro:
    'A form, a booking calendar, a list for your team: say in your own words what it should do. Within minutes you have a link to share with your colleagues - and you change the app later the same way, without writing any code.',
  describe: 'Describe an app',
  /** The way to /ship, for whoever already works with an AI agent of their own. */
  own: 'Use your own AI agent',

  examplesTitle: 'What people build here',
  examples: [
    { title: 'Forms and requests', text: 'Time off, purchases, feedback: people fill it in, and whoever decides approves it.' },
    { title: 'Bookings', text: 'Meeting rooms, desks, equipment: who has what, and when.' },
    { title: 'Lists and trackers', text: 'Support requests, inventory, onboarding: what is open, and who is on it.' },
    { title: 'Pages for a team', text: 'A schedule, a handbook, news: one link everybody opens.' },
  ],

  stepsTitle: 'How it works',
  steps: [
    {
      title: 'Describe it',
      text: 'Say what the app should do, the way you would explain it to a colleague. The ideas under the box are a place to start.',
    },
    {
      title: 'Share the link',
      text: 'A few minutes later it is online here. Send its link to the people who use it, and keep your editor link: it is how you change the app.',
    },
    {
      title: 'Ask for more',
      text: 'In the same box: something new, something different, or how it is doing - who used it and what was entered. Earlier versions are kept, so the one before can go back online.',
    },
  ],

  questionsTitle: 'Good to know',
  /** The last answer links to the showcase, around the words it is given. */
  questions: (showcase: (text: string) => ReactNode): [string, ReactNode][] => [
    ['Do I need to know how to code?', 'No. You describe what the app should do, and the agent writes it. You never have to look at the code.'],
    ['Who can open my app?', 'Whoever has its link and can reach this site. Send it to the people who should use it.'],
    ['Where is what people enter kept?', 'Each app keeps its own records on this installation, which your organization runs.'],
    [
      'How do I get back to my app?',
      'This browser remembers the apps you built here, beside the box where you built them. On another device, open the editor link you got with the app - keep it somewhere safe, since nobody can recover it.',
    ],
    [
      'Something does not work. What now?',
      'Tell the agent what is wrong, the same way you asked for the app. If a change made things worse, put the version before back online in the app’s editor.',
    ],
    ['What have colleagues built?', <>The apps they chose to show are on the {showcase('showcase')}.</>],
  ],
};
