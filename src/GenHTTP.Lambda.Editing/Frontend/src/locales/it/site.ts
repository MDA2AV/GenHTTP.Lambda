import { kit } from '@genhttp-lambda/kit/locales/it';

import type { Messages } from '../en/site';
import { create } from './create';

export const messages: Messages = {
  ...kit,
  create,
};
