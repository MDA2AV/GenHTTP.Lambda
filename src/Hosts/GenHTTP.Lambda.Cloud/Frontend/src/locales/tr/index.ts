import { kit } from '@genhttp-lambda/kit/locales/tr';

import type { Messages } from '../en';
import { build } from './build';
import { imprint } from './imprint';
import { platform } from './platform';
import { selfHosting } from './selfHosting';
import { ship } from './ship';
import { privacy } from './privacy';
import { terms } from './terms';

/** Sitenin Türkçe metinleri. */
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
