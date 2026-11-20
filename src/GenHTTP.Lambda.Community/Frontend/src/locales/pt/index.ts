import { kit } from '@genhttp-lambda/kit/locales/pt';

import type { Messages } from '../en';
import { enterpriseLanding } from './enterpriseLanding';
import { landing } from './landing';
import { build } from './build';
import { ship } from './ship';

export const messages: Messages = {
  ...kit,
  landing,
  enterpriseLanding,
  build,
  ship,
};
