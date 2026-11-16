import { Navigate, Route, Routes, useLocation, useParams } from 'react-router-dom';

import { useScrollToTop } from '@genhttp-lambda/kit/app';
import { Shell, type ShellLink } from '@genhttp-lambda/kit/components/Shell';
import { ToastHost } from '@genhttp-lambda/kit/components/Toast';
import { useLanguage } from '@genhttp-lambda/kit/i18n';
import { inLanguage, isLanguage } from '@genhttp-lambda/kit/i18n/languages';
import { NotFound } from '@genhttp-lambda/kit/pages/NotFound';
import { Showcase } from '@genhttp-lambda/kit/pages/Showcase';
import { useTheme } from '@genhttp-lambda/kit/theme';

import { useT } from './i18n';
import pages from './pages.json';
import { Build } from './pages/Build';
import { Imprint } from './pages/Imprint';
import { Landing } from './pages/Landing';
import { Platform } from './pages/Platform';
import { Privacy } from './pages/Privacy';
import { SelfHosting } from './pages/SelfHosting';
import { Ship } from './pages/Ship';
import { Terms } from './pages/Terms';

/** The pages of this site, by their path without a language. */
const PAGES = new Set(Object.keys(pages));

/**
 * The pages of genhttp.dev that are this host's own. The editor, the panel
 * and the published sources are components of their own, with bundles of
 * their own: a link there loads them (see claim in main.tsx).
 */
export function App() {
  const [theme, toggleTheme] = useTheme();
  const t = useT();

  useScrollToTop();

  const links: ShellLink[] = [
    // on the narrowest phones this one moves into the menu, the next stays
    { to: '/build', label: t.shell.build, room: 'xs' },
    { to: '/ship', label: t.shell.ship, room: 'always' },
    { to: '/platform', label: t.platform.menu, room: 'sm' },
    // these join the bar a step later than their width in English would
    // allow: the same words in Polish or Japanese take half as much room again
    { to: '/showcase', label: t.shell.showcase, room: 'md' },
    { to: '/self-hosting', label: t.selfHosting.menu, room: 'lg' },
    { to: '/admin', label: t.shell.admin, room: 'lg', locked: true },
  ];

  return (
    <ToastHost>
      <Shell theme={theme} onToggleTheme={toggleTheme} links={links}>
        <Routes>
          <Route path="/:language/*" element={<Localized />} />
          <Route path="*" element={<Unlocalized />} />
        </Routes>
      </Shell>
    </ToastHost>
  );
}

/**
 * The public pages, in the language their address names - "/de/build" is the
 * German build page. A first segment that is no language is an address
 * without one.
 */
function Localized() {
  const { language } = useParams();

  if (!isLanguage(language)) {
    return <Unlocalized />;
  }

  return (
    <Routes>
      <Route index element={<Landing />} />
      <Route path="build" element={<Build />} />
      <Route path="ship" element={<Ship />} />
      <Route path="platform" element={<Platform />} />
      <Route path="self-hosting" element={<SelfHosting />} />
      <Route path="showcase" element={<Showcase />} />
      <Route path="terms" element={<Terms />} />
      <Route path="privacy" element={<Privacy />} />
      <Route path="imprint" element={<Imprint />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

/**
 * A public page asked for without a language. The server answers those with
 * a redirect to the language the visitor prefers; this is the same for a link
 * followed inside the application, and for the development server.
 */
function Unlocalized() {
  const { pathname, search, hash } = useLocation();
  const language = useLanguage();

  const page = pathname.length > 1 ? pathname.replace(/\/+$/, '') : pathname;

  if (!PAGES.has(page)) {
    return <NotFound />;
  }

  return <Navigate to={inLanguage(language, page) + search + hash} replace />;
}
