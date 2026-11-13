import { kit } from '@genhttp-lambda/kit/locales/de';

import type { Messages } from '../en';
import { build } from './build';
import { imprint } from './imprint';
import { landing } from './landing';
import { ship } from './ship';
import { privacy } from './privacy';
import { terms } from './terms';

/** Die Texte der Seite auf Deutsch. */
export const messages: Messages = {
  ...kit,
  landing,
  build,
  ship,
  terms,
  privacy,
  imprint,
};
