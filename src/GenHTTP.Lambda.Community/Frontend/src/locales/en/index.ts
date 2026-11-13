import { kit } from '@genhttp-lambda/kit/locales/en';

import { landing } from './landing';

/**
 * The words of the site in English, which every other language translates:
 * the kit's - the shell, the guide, the showcase - and the front page's.
 */
export const messages = {
  ...kit,
  landing,
};

export type Messages = typeof messages;
