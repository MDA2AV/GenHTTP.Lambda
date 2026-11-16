import { kit } from '@genhttp-lambda/kit/locales/pt';

import type { Messages } from '../en';
import { build } from './build';
import { imprint } from './imprint';
import { landing } from './landing';
import { platform } from './platform';
import { selfHosting } from './selfHosting';
import { ship } from './ship';
import { privacy } from './privacy';
import { terms } from './terms';

/** Os textos do site em português. */
export const messages: Messages = {
  ...kit,
  landing,
  build,
  ship,
  platform,
  selfHosting,
  terms,
  privacy,
  imprint,
};
