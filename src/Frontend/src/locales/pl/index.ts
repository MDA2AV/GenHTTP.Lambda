import type { Messages } from '../en';
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

/** Teksty strony po polsku. */
export const messages: Messages = {
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
