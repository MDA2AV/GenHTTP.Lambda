import { kit } from '@genhttp-lambda/kit/locales/en';

import { build } from './build';
import { imprint } from './imprint';
import { landing } from './landing';
import { ship } from './ship';
import { privacy } from './privacy';
import { terms } from './terms';

/**
 * The words of the site in English, which every other language translates:
 * its catalog is typed as this one, so a sentence missing there is an error
 * when the frontend is built rather than a gap on the page. The kit's words
 * are part of it, so the shell and the shared pages read the same catalog.
 */
export const messages = {
  ...kit,
  landing,
  build,
  ship,
  terms,
  privacy,
  imprint,
};

export type Messages = typeof messages;
