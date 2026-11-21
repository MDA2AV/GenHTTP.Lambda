import { kit } from '@genhttp-lambda/kit/locales/pt-pt';

import type { Messages } from '../en';
import { build } from './build';
import { imprint } from './imprint';
import { platform } from './platform';
import { selfHosting } from './selfHosting';
import { ship } from './ship';
import { privacy } from './privacy';
import { terms } from './terms';

/** Os textos do site em português europeu. */
export const messages: Messages = {
  ...kit,
  build,
  ship,
  platform,
  selfHosting,
  terms,
  privacy,
  imprint,
};
