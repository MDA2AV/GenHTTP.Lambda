import './setup';

import { prerendering } from '@genhttp-lambda/kit/prerender';

import { App } from './App';

/* The public pages of the site as markup, for the server (see the kit's prerender). */
export const prerender = prerendering(() => <App />);
