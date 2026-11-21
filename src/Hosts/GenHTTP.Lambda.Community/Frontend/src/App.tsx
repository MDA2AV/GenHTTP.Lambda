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
import { Landing } from './pages/Landing';
import { Ship } from './pages/Ship';

/** The pages of this site, by their path without a language. */
const PAGES = new Set(Object.keys(pages));

/**
 * The pages of a community installation: its front page, the workspace that
 * builds apps (/build), the guide for an agent of one's own (/ship) and the
 * showcase - the same way round as genhttp.dev, with a simpler front page.
 * The editor, the panel and the published sources are components with
 * bundles of their own: a link there loads them.
 */
export function App() {
  const [theme, toggleTheme] = useTheme();
  const t = useT();
  const { pathname } = useLocation();

  useScrollToTop();

  const links: ShellLink[] = [
    // on the narrowest phones this one moves into the menu, the next stays
    { to: '/build', label: t.shell.build, room: 'xs' },
    { to: '/ship', label: t.shell.ship, room: 'always' },
    { to: '/showcase', label: t.shell.showcase, room: 'md' },
    { to: '/admin', label: t.shell.admin, room: 'lg', locked: true },
  ];

  return (
    <ToastHost>
      {/* the workspace fills the window, as a chat does: its conversation scrolls, its box stays at the foot */}
      <Shell theme={theme} onToggleTheme={toggleTheme} links={links} fixed={isWorkspace(pathname)}>
        <Routes>
          <Route path="/:language/*" element={<Localized />} />
          <Route path="*" element={<Unlocalized />} />
        </Routes>
      </Shell>
    </ToastHost>
  );
}

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
      <Route path="showcase" element={<Showcase />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

/** Whether a path is the workspace, in any language. */
const isWorkspace = (pathname: string) => /^\/[a-z]{2}(-[a-z]{2})?\/build\/?$/.test(pathname);

/** A page asked for without a language, sent on to the visitor's - as the server does. */
function Unlocalized() {
  const { pathname, search, hash } = useLocation();
  const language = useLanguage();

  const page = pathname.length > 1 ? pathname.replace(/\/+$/, '') : pathname;

  if (!PAGES.has(page)) {
    return <NotFound />;
  }

  return <Navigate to={inLanguage(language, page) + search + hash} replace />;
}
