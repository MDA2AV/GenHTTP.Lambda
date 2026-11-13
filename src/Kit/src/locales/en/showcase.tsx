import type { ReactNode } from 'react';

export const showcase = {
  eyebrow: 'Showcase',
  title: 'Built here, running now',
  intro:
    'Lambdas their owners chose to show. Every one of them is online, so each card opens the real thing. The ones in use lately come first.',
  /** Written by the server as well, for a page it renders - in the form for one and in the form for more. */
  counted: (total: number) => (total === 1 ? '1 lambda' : `${total} lambdas`),
  failed: 'The showcase could not be loaded.',
  loadingMore: 'Loading more…',
  showMore: 'Show more',
  nothingTitle: 'Nothing on show yet',
  nothing: (tab: (text: string) => ReactNode) => (
    <>
      Built something that works? Open its control center, choose {tab('Showcase')}, and add a title, a few words
      and a picture. It appears here while it is online.
    </>
  ),
  buildOne: 'Build one',
  yoursTitle: 'Want yours here?',
  yours: (tab: (text: string) => ReactNode) => (
    <>
      Open the control center of your lambda and choose {tab('Showcase')}, or ask the agent that built it to
      showcase it. Only whoever holds the editor key can, and it can be taken down again at any time.
    </>
  ),
  buildSomething: 'Build something',
};

/** One card of the showcase, which the editor draws as a preview as well. */
export const card = {
  noPicture: 'No picture yet',
  title: 'Title',
  description: 'What a visitor can do with it.',
  opens: (title: string, address: string) => `${title}, opens ${address} in a new tab`,
};
