import { Suspense, lazy } from 'react';
import { Route, Routes } from 'react-router-dom';

import { ChunkBoundary, Loading, recovering, useScrollToTop } from '@genhttp-lambda/kit/app';
import { Shell, type ShellLink } from '@genhttp-lambda/kit/components/Shell';
import { ToastHost } from '@genhttp-lambda/kit/components/Toast';
import { preferredLanguage } from '@genhttp-lambda/kit/i18n/languages';
import { NotFound } from '@genhttp-lambda/kit/pages/NotFound';
import { useTheme } from '@genhttp-lambda/kit/theme';

import { loadEditor, useT } from './i18n';
import { Create } from './pages/Create';

// Monaco is most of the bundle, so the page creating a lambda never downloads it
const Editor = lazy(() =>
  recovering(() => Promise.all([import('./pages/Editor'), loadEditor(preferredLanguage())])).then(([module]) => ({ default: module.Editor })),
);

/**
 * The editor of a lambda, behind its key, and the page that creates one.
 *
 * The same in every installation, so the bar links only to what every site
 * has: its guide and its showcase.
 */
export function App() {
  const [theme, toggleTheme] = useTheme();
  const t = useT();

  useScrollToTop();

  const links: ShellLink[] = [
    { to: '/showcase', label: t.shell.showcase, room: 'sm' },
    { to: '/docs', label: t.shell.docs, room: 'md' },
  ];

  return (
    <ToastHost>
      <Routes>
        <Route
          path="/editor/create"
          element={
            <Shell theme={theme} onToggleTheme={toggleTheme} links={links}>
              <Create />
            </Shell>
          }
        />
        <Route
          path="/editor/:privateKey/*"
          element={
            <Shell theme={theme} onToggleTheme={toggleTheme} links={links} fixed>
              <ChunkBoundary>
                <Suspense fallback={<Loading />}>
                  <Editor theme={theme} />
                </Suspense>
              </ChunkBoundary>
            </Shell>
          }
        />
        <Route
          path="*"
          element={
            <Shell theme={theme} onToggleTheme={toggleTheme} links={links}>
              <NotFound />
            </Shell>
          }
        />
      </Routes>
    </ToastHost>
  );
}
