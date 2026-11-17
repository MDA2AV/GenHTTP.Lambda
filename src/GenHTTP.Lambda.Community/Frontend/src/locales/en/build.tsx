import type { ReactNode } from 'react';

/**
 * /build of an installation somebody runs for themselves: the workspace,
 * where its build agent - if it has one - builds an app from a sentence and
 * answers about it afterwards. Plain words, for developers and for whoever
 * they let use it.
 */
export const build = {
  title: 'Build an app by describing it',
  intro:
    'Tell the build agent of this installation what you need. It writes the app, puts it online here and keeps it in your list - to ask about and change later, in the same way.',
  /** Under the box: the two other ways to an app, each a link around its words. */
  other: (ship: (text: string) => ReactNode, code: (text: string) => ReactNode) => (
    <>
      Or {ship('connect your own agent')}, or {code('write the code yourself')}.
    </>
  ),
};
