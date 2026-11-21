import './i18n';

import { boot } from '@genhttp-lambda/kit/app';
import { claim } from '@genhttp-lambda/kit/i18n/links';

import '@genhttp-lambda/kit/index.css';

import { App } from './App';

// the panel's own pages; a link anywhere else on the host loads that page
claim((path) => path === '/admin' || path.startsWith('/admin/'));

boot(<App />);
