import { kit } from '@genhttp-lambda/kit/locales/ko';

import type { Messages } from '../en';
import { build } from './build';
import { imprint } from './imprint';
import { landing } from './landing';
import { ship } from './ship';
import { privacy } from './privacy';
import { terms } from './terms';

/** 사이트의 한국어 문구. */
export const messages: Messages = {
  ...kit,
  landing,
  build,
  ship,
  terms,
  privacy,
  imprint,
};
