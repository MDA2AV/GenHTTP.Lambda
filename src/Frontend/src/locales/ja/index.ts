import type { Messages } from '../en';
import { build } from './build';
import { abuse, common, missing, notFound, shell } from './common';
import { create } from './create';
import { enterprise } from './enterprise';
import { guide } from './guide';
import { landing } from './landing';
import { ship } from './ship';
import { card, showcase } from './showcase';
import { terms } from './terms';

/** サイトの日本語テキスト。 */
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
  create,
};
