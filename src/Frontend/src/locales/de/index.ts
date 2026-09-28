import type { Messages } from '../en';
import { build } from './build';
import { abuse, common, missing, notFound, shell } from './common';
import { create } from './create';
import { enterprise } from './enterprise';
import { guide } from './guide';
import { imprint } from './imprint';
import { landing } from './landing';
import { ship } from './ship';
import { privacy } from './privacy';
import { card, showcase } from './showcase';
import { terms } from './terms';

/** Die Texte der Seite auf Deutsch. */
export const messages: Messages = {
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
  privacy,
  imprint,
  create,
};
