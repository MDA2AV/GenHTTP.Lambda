import type { ReactNode } from 'react';

/**
 * /ship of an installation somebody runs for themselves: how to connect an
 * agent of one's own over MCP and have it build and publish apps here - a
 * guide in three steps, for developers, selling nothing.
 */
export const ship = {
  eyebrow: 'Connect your agent',
  title: 'Publish from the agent you already use.',
  intro:
    'Claude Code, Codex, Cursor or any agent that speaks MCP gets this installation as tools: it builds an app, tries a change in a draft, puts it online and reads its logs. Every app answers at an address of its own, with its own database.',
  stepsTitle: 'Three steps to an app online',
  step: (n: number) => `Step ${n}`,
  steps: [
    {
      title: 'Connect once',
      body: 'Add the address of this installation’s MCP server to your agent. It needs no key: whoever holds an app’s editor key can change that app, and nothing else.',
    },
    {
      title: 'Ask it to build or publish',
      body: 'Describe a new app, or ask it to publish the one in your project. It writes the code, checks that it compiles and puts it online.',
    },
    {
      title: 'Keep the editor link',
      body: 'You get the app’s address and its editor link. The address is for everybody; the editor link is how you - or your agent - change the app later.',
    },
  ],
  sayLike: 'Then say something like',
  asks: ['Build a guestbook, with a page where I approve the entries.', 'Publish this app here and send me the link.'],
  /** The way to the workspace on /build, for whoever has no agent of their own. */
  noAgent: 'No agent? Use the build agent',

  questionsTitle: 'Good to know',
  questions: (create: (text: string) => ReactNode): [string, ReactNode][] => [
    [
      'Which agents can connect?',
      'Any agent that can add a remote MCP server over HTTP: Claude Code, Claude, Codex, Cursor, VS Code and others, at the address above.',
    ],
    [
      'How do I change an app later?',
      'Give your agent the app’s editor link with the next change, or open the link yourself: the editor shows its versions, code, records and logs. A change is tried in a draft, with a copy of the data, before it goes online.',
    ],
    [
      'Can I work on an app with git?',
      'Yes. Every app is a git repository: clone it from its editor, push a commit to main to save a version, or push a branch to start a draft with a preview of its own.',
    ],
    [
      'Where do API keys and passwords go?',
      'Not into the code. The agent reads them by name, and you enter the values in the editor, where nobody reads them back - not the agent either.',
    ],
    [
      'Can I write an app without an agent?',
      <>Yes: {create('write the code yourself')}, starting from one of the demos. An app is a C# GenHTTP handler, compiled on this server.</>,
    ],
  ],
};
