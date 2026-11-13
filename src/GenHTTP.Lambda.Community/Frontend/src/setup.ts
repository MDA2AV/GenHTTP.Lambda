import './i18n';

import { claim, isSitePath } from '@genhttp-lambda/kit/i18n/links';
import { definePages } from '@genhttp-lambda/kit/meta';

import pages from './pages.json';

/*
 * What the site is, said once for the browser (main.tsx) and for the build
 * that renders its pages (prerender.tsx) alike. Whom to write to, where to
 * report abuse and whether there are legal pages is the installation's to
 * say (LAMBDA_CONTACT, LAMBDA_ABUSE_CONTACT), so the pages ask the server.
 */

definePages(pages);

// the pages of the site, rather than the editor's, the panel's or the
// published sources', which are bundles of their own
claim(isSitePath);
