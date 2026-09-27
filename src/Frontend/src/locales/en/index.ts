import { build } from './build';
import { abuse, common, missing, notFound, shell } from './common';
import { create } from './create';
import { enterprise } from './enterprise';
import { guide } from './guide';
import { landing } from './landing';
import { ship } from './ship';
import { card, showcase } from './showcase';
import { terms } from './terms';

/**
 * The words of the site in English, which every other language translates:
 * its catalog is typed as this one, so a sentence missing there is an error
 * when the frontend is built rather than a gap on the page.
 */
export const messages = {
  shell,
  common,
  notFound,
  missing,
  abuse,
  landing,
  build,
  ship,
  showcase,
  card,
  enterprise,
  guide,
  terms,
  create,
};

export type Messages = typeof messages;
