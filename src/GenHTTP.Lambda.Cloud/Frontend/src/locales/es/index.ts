import { kit } from '@genhttp-lambda/kit/locales/es';

import type { Messages } from '../en';
import { build } from './build';
import { imprint } from './imprint';
import { platform } from './platform';
import { selfHosting } from './selfHosting';
import { ship } from './ship';
import { privacy } from './privacy';
import { terms } from './terms';

/** Los textos del sitio en español. */
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
