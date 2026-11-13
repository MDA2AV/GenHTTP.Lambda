import { kit } from '@genhttp-lambda/kit/locales/de';

import type { Messages } from '../en/site';
import { create } from './create';

export const messages: Messages = {
  ...kit,
  create,
};
