import { kit } from '@genhttp-lambda/kit/locales/es';

import type { Messages } from '../en';
import { landing } from './landing';

export const messages: Messages = {
  ...kit,
  landing,
};
