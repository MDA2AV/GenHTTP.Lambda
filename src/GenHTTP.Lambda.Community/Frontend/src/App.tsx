import { Navigate, Route, Routes, useLocation, useParams } from 'react-router-dom';

import { useScrollToTop } from '@genhttp-lambda/kit/app';
import { Shell, type ShellLink } from '@genhttp-lambda/kit/components/Shell';
import { ToastHost } from '@genhttp-lambda/kit/components/Toast';
import { useLanguage } from '@genhttp-lambda/kit/i18n';
import { inLanguage, isLanguage } from '@genhttp-lambda/kit/i18n/languages';
import { Guide } from '@genhttp-lambda/kit/pages/Guide';
import { NotFound } from '@genhttp-lambda/kit/pages/NotFound';
import { Showcase } from '@genhttp-lambda/kit/pages/Showcase';
import { useTheme } from '@genhttp-lambda/kit/theme';

import { useT } from './i18n';
import pages from './pages.json';
import { Landing } from './pages/Landing';

/** The pages of this site, by their path without a language. */
const PAGES = new Set(Object.keys(pages));

/**
 * The pages of a community installation: its front page, the guide and the
 * showcase. The editor, the panel and the published sources are components
 * with bundles of their own: a link there loads them.
 */
export function App() {
  const [theme, toggleTheme] = useTheme();
  const t = useT();

  useScrollToTop();

  const links: ShellLink[] = [
    { to: '/showcase', label: t.shell.showcase, room: 'xs' },
    { to: '/docs', label: t.shell.docs, room: 'always' },
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

function Localized() {
  const { language } = useParams();

  if (!isLanguage(language)) {
    return <Unlocalized />;
  }

  return (
    <Routes>
      <Route index element={<Landing />} />
      <Route path="docs" element={<Guide />} />
      <Route path="showcase" element={<Showcase />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

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
