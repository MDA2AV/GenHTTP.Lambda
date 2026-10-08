import { build } from './build';
import { abuse, common, notFound, shell } from './common';
import { connect } from './connect';
import { create } from './create';
import { enterprise } from './enterprise';
import { guide } from './guide';
import { imprint } from './imprint';
import { landing } from './landing';
import { ship } from './ship';
import { privacy } from './privacy';
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
  connect,
  notFound,
  abuse,
  landing,
  build,
  ship,
  showcase,
  card,
  enterprise,
  guide,
  terms,
  privacy,
  imprint,
  create,
};

export type Messages = typeof messages;
