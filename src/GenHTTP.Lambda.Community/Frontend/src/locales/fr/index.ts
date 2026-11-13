import { kit } from '@genhttp-lambda/kit/locales/fr';

import type { Messages } from '../en';
import { landing } from './landing';

export const messages: Messages = {
  ...kit,
  landing,
};
