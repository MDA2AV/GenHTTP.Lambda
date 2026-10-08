import type { ReactNode } from 'react';

export const enterprise = {
  eyebrow: 'Enterprise',
  title: 'Free to try, yours to run',
  intro:
    'Everything here is free, with no account. When your team needs apps that stay online for good, behind its own sign-in, get an installation of your own, in the cloud or on premises.',

  free: 'Free',
  freeTagline: 'For trying things out',
  forever: 'forever',
  buildOne: 'Build one',
  freeFeatures: (offline: number, removed: number) => [
    'Unlimited lambdas, no account',
    'The built-in agent, or your own over MCP',
    'Online for as long as it is used',
    `Offline after ${offline} days without visits, removed after ${removed}`,
    'Served at a subdomain of the shared domain',
  ],
  freeNote: 'No card, no sign-up. Create a lambda and it is yours.',

  name: 'Enterprise',
  tagline: 'For teams that want an instance of their own',
  perUser: 'per user / month',
  contact: 'Contact us',
  features: [
    'Your own instance, cloud or on premises',
    'A single service runs all apps',
    'Sign in with your own SSO',
    'Your governance and compliance rules built in',
    'Apps stay online for good, nothing is ever removed',
    'Bring your own agent over MCP',
    'Priority support',
  ],
  users: (count: ReactNode) => <>{count} users</>,
  perMonth: ' / month',
  price: (amount: number) => `$${amount}`,
  perUserPrice: (amount: number) => `$${amount} / user / month`,

  compareTitle: 'Compare the tiers',
  compareText: 'Both run the same platform. What changes is how long it keeps your app, and where.',
  included: 'Included',
  notIncluded: 'Not included',
  /** Groups of rows, each row with what the free tier and what Enterprise has; true and false are ticks and dashes. */
  groups: (offline: number, removed: number): { title: string; rows: [string, boolean | string, boolean | string][] }[] => [
    {
      title: 'Building',
      rows: [
        ['Lambdas', 'Unlimited', 'Unlimited'],
        ['Built-in agent', true, false],
        ['Your own agent over MCP', true, true],
        ['Editor, versions and logs', true, true],
        ['Showcase', true, 'Your own'],
      ],
    },
    {
      title: 'Hosting',
      rows: [
        ['Taken offline when unused', `After ${offline} days`, 'Never'],
        ['Removed when unused', `After ${removed} days`, 'Never'],
        ['Instance', 'Shared', 'Your own'],
        ['Runs', 'In our cloud', 'Cloud or on premises'],
        ['What you operate', 'Nothing', 'A single service'],
        ['Custom domains', false, true],
      ],
    },
    {
      title: 'Control',
      rows: [
        ['Sign-in', 'None needed', 'Your own SSO'],
        ['Your governance and compliance rules for agents', false, true],
        ['Administration console', false, true],
        ['Data kept apart from other customers', false, true],
        ['Support', 'Community', 'Priority'],
      ],
    },
  ],

  questionsTitle: 'Questions',
  questions: [
    ['Do I need an account to start?', 'No. A free lambda needs nothing but the editor link you get when you create one.'],
    [
      'Who counts as a user in Enterprise?',
      'Everybody who signs in through your SSO - whether to build in the editor or to use an app deployed on your installation. Anybody reaching an app without signing in is not counted.',
    ],
    [
      'Is the built-in agent included in Enterprise?',
      'No. Your team brings its own agent - Claude, Claude Code or anything else that speaks MCP - and connects it to your installation, on whatever plan you already have with its vendor.',
    ],
    [
      'How do agents learn our compliance rules?',
      'We build your governance and compliance rules into what the platform tells agents over MCP. Every agent your team connects gets them while it writes code, so the apps come out following your rules without everybody having to know them by heart.',
    ],
    [
      'Do we need Kubernetes or a cluster?',
      'No. Every app runs inside one service, so there are no pods to spread out and nothing to orchestrate per app. Running the installation means running that one service.',
    ],
    [
      'Where does an Enterprise installation run?',
      'Where you choose. We can host it for you in our cloud, or it runs in a cloud account of yours or on your own servers - anywhere that runs containers. Either way we help you set it up and keep it updated.',
    ],
  ] as [string, string][],
  anythingElse: (mail: ReactNode) => <>Anything else? Write to {mail}.</>,
};
