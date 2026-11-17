import { kit } from '@genhttp-lambda/kit/locales/de';

import type { Messages } from '../en';
import { landing } from './landing';
import { build } from './build';
import { ship } from './ship';

export const messages: Messages = {
  ...kit,
  landing,
  build,
  ship,
};
