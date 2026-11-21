import './i18n';

import { boot } from '@genhttp-lambda/kit/app';
import { claim, isSourcePath } from '@genhttp-lambda/kit/i18n/links';
import { definePages } from '@genhttp-lambda/kit/meta';

import '@genhttp-lambda/kit/index.css';

import { App } from './App';
import pages from './pages.json';

definePages(pages);

// the pages of the published sources; a link anywhere else on the host loads that page
claim(isSourcePath);

boot(<App />);
