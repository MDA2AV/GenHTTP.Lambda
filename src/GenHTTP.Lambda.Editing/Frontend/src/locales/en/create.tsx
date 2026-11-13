import type { ReactNode } from 'react';

export const create = {
  title: 'Create a Lambda',
  whatTitle: 'What would you like to build?',
  whatText:
    'Pick the closest one and you start with a copy of something that already works - yours to change. Or start from nothing.',
  seeIt: 'See it running',
  startFrom: 'Start from this',
  /**
   * What each starter is called, by its id - the server names them in
   * English, and a starter it adds later is shown as the server names it.
   */
  starters: {
    'demo-crud': {
      title: 'Keep track of things',
      description: 'A list people can add to, change and tick off - tasks, notes, bookmarks or a small inventory.',
    },
    'demo-registration': {
      title: 'Let people sign up',
      description: 'Accounts people register and sign in with, and pages only they get to see.',
    },
    'demo-game': {
      title: 'A game to play together',
      description: 'Something several people play at the same time, live in their browsers.',
    },
    'demo-files': {
      title: 'Share files and pictures',
      description: 'People upload pictures or documents, and everybody else can see them.',
    },
    'demo-live': {
      title: 'Show things as they happen',
      description: 'A page that updates by itself the moment something changes - votes, scores, a dashboard.',
    },
    empty: {
      title: 'Something else',
      description: 'Start from an empty lambda and build whatever you have in mind.',
    },
  } as Record<string, { title: string; description: string }>,

  addressTitle: 'Give it an address',
  fromDemo: (title: ReactNode) => (
    <>{title} - your lambda starts as a copy of the demo, and everything in it is yours to change.</>
  ),
  fromNothing: 'Your lambda starts empty, ready for whatever you have in mind.',
  pickAgain: 'Pick something else',
  publicKey: 'Public key',
  free: (key: string) => `"${key}" is free.`,
  keyHint: 'Lower case letters, digits and dashes. Three characters or more. Leave it empty for a random one.',
  accept: 'I accept the terms of service',
  fullTerms: 'Read the full terms of service',
  back: 'Back',
  creating: 'Creating…',
  submit: 'Create my lambda',
  keepLink: 'The next screen shows your editor link. It is the only way back in, so keep it.',
  failed: 'The lambda could not be created.',
  /** The terms in short, beside the box that accepts them - where the installation has terms. */
  termsShort:
    'Lambdas run on shared infrastructure. By creating one you agree not to deploy malware, phishing pages, crypto miners, or anything that attacks, scans or floods other systems, and not to publish content you have no right to publish. Anyone who knows the editor link can change your lambda, so treat it as a password. Free tier lambdas stay online for as long as they are used: one that nobody visits and nobody edits for a month is taken offline, and removed if nothing happens for two months after that. Anything you deploy may be removed at any time.',
};
