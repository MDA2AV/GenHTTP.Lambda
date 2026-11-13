import { kit } from '@genhttp-lambda/kit/locales/de';

import type { Messages } from '../en';
import { landing } from './landing';

export const messages: Messages = {
  ...kit,
  landing,
};
