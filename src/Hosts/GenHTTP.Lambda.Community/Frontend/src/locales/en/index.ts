import { kit } from '@genhttp-lambda/kit/locales/en';

import { build } from './build';
import { enterpriseLanding } from './enterpriseLanding';
import { landing } from './landing';
import { ship } from './ship';

/**
 * The words of the site in English, which every other language translates:
 * the kit's - the shell, the showcase, the workspace - and its pages' own,
 * the front page as an organization's colleagues see it among them.
 */
export const messages = {
  ...kit,
  landing,
  enterpriseLanding,
  build,
  ship,
};

export type Messages = typeof messages;
