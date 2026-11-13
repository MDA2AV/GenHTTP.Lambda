import { kit } from '@genhttp-lambda/kit/locales/es';

import type { Messages } from '../en';
import { build } from './build';
import { imprint } from './imprint';
import { landing } from './landing';
import { ship } from './ship';
import { privacy } from './privacy';
import { terms } from './terms';

/** Los textos del sitio en español. */
export const messages: Messages = {
  ...kit,
  landing,
  build,
  ship,
  terms,
  privacy,
  imprint,
};
