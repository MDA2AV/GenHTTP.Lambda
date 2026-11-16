import './setup';

import { prerendering } from '@genhttp-lambda/kit/prerender';

import { App } from './App';

/*
 * The public pages of the site as markup, for the server (see the kit's
 * prerender) - and once more for a brand, since whoever runs the community
 * edition may give it a name and a logo of their own.
 */
export const prerender = prerendering(() => <App />, { branded: true });
