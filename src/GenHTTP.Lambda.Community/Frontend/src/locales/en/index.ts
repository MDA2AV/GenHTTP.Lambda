import { kit } from '@genhttp-lambda/kit/locales/en';

import { build } from './build';
import { landing } from './landing';
import { ship } from './ship';

/**
 * The words of the site in English, which every other language translates:
 * the kit's - the shell, the showcase, the workspace - and its pages' own.
 */
export const messages = {
  ...kit,
  landing,
  build,
  ship,
};

export type Messages = typeof messages;
