import './i18n';

import { claim, isSitePath } from '@genhttp-lambda/kit/i18n/links';
import { defineInstallation } from '@genhttp-lambda/kit/installation';
import { definePages } from '@genhttp-lambda/kit/meta';

import { ABUSE_MAILBOX } from './abuse';
import { CONTACT_MAIL } from './contact';
import pages from './pages.json';

/*
 * What the site is, said once for the browser (main.tsx) and for the build
 * that renders its pages (prerender.tsx) alike.
 */

definePages(pages);

// the pages of the site, rather than the editor's, the panel's or the
// published sources', which are bundles of their own
claim(isSitePath);

// known when the site is built, so its rendered pages say it as well
defineInstallation({ contact: CONTACT_MAIL, abuse: ABUSE_MAILBOX, legal: true });
