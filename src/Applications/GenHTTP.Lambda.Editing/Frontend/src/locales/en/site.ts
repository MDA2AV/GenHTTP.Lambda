import { kit } from '@genhttp-lambda/kit/locales/en';

import { create } from './create';

/**
 * The words every page of the editor reads: the kit's - the shell, what is
 * said in many places - and the page that creates a lambda. The editor's own
 * are a catalog of their own (index.ts), fetched with the editor.
 */
export const messages = {
  ...kit,
  create,
};

export type Messages = typeof messages;
