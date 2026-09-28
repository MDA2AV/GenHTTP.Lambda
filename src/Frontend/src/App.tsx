import { Component, Suspense, lazy, useEffect, type ReactNode } from 'react';
import { Navigate, Route, Routes, useLocation, useParams } from 'react-router-dom';

import { Shell } from './components/Shell';
import { IconSpinner } from './components/Icons';
import { ToastHost } from './components/Toast';
import { Create } from './pages/Create';
import { Enterprise } from './pages/Enterprise';
import { LambdaMissing } from './pages/LambdaMissing';
import { Landing } from './pages/Landing';
import { NotFound } from './pages/NotFound';
import { Admin } from './pages/Admin';
import { Guide } from './pages/Guide';
import { Build } from './pages/Build';
import { Showcase } from './pages/Showcase';
import { Ship } from './pages/Ship';
import { Terms } from './pages/Terms';
import { useTheme } from './theme';
import { loadEditor, useLanguage, useT } from './i18n';
import { inLanguage, isLanguage, preferredLanguage } from './i18n/languages';
import pages from './pages.json';

// Monaco is most of the bundle, so the landing page never downloads it
const RELOADED = 'lambda-reloaded-for-chunk';

/**
 * Loads the editor, and recovers from the one way that reliably fails: the
 * application was deployed again while this tab was open, so the index it was
 * built from names a chunk the server no longer has. Fetching the page again
 * is all it takes, and the flag keeps a genuinely broken build from turning
 * that into a loop.
 */
const Editor = lazy(() =>
  Promise.all([import('./pages/Editor'), loadEditor(preferredLanguage())])
    .then(([module]) => {
      sessionStorage.removeItem(RELOADED);
      return { default: module.Editor };
    })
    .catch((error: unknown) => {
      if (sessionStorage.getItem(RELOADED) === null) {
        sessionStorage.setItem(RELOADED, '1');
        window.location.reload();
      }

      throw error;
    }),
);

export function App() {
  const [theme, toggleTheme] = useTheme();

  useScrollToTop();

  return (
    <ToastHost>
      {/* catalogs are fetched before a page is drawn, so this is a net rather than a wait */}
      <Suspense fallback={null}>
        <Routes>
          <Route
            path="/editor/create"
            element={
              <Shell theme={theme} onToggleTheme={toggleTheme}>
                <Create />
              </Shell>
            }
          />
          <Route
            path="/editor/:privateKey/*"
            element={
              <Shell theme={theme} onToggleTheme={toggleTheme} fixed>
                <ChunkBoundary>
                  <Suspense fallback={<Loading />}>
                    <Editor theme={theme} />
                  </Suspense>
                </ChunkBoundary>
              </Shell>
            }
          />
          <Route
            path="/admin/*"
            element={
              <Shell theme={theme} onToggleTheme={toggleTheme} fixed>
                <Admin theme={theme} />
              </Shell>
            }
          />
          <Route
            path="*"
            element={
              <Shell theme={theme} onToggleTheme={toggleTheme}>
                <Routes>
                  <Route path="/lambda/:publicKey/*" element={<LambdaMissing />} />
                  <Route path="/:language/*" element={<Localized />} />
                  <Route path="*" element={<Unlocalized />} />
                </Routes>
              </Shell>
            }
          />
        </Routes>
      </Suspense>
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
      <Route path="docs" element={<Guide />} />
      <Route path="build" element={<Build />} />
      <Route path="ship" element={<Ship />} />
      <Route path="showcase" element={<Showcase />} />
      <Route path="enterprise" element={<Enterprise />} />
      <Route path="terms" element={<Terms />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

/** Paths that are a public page without a language. */
const UNLOCALIZED = new Set(Object.keys(pages));

/**
 * A public page asked for without a language. The server answers those with
 * a redirect to the language the visitor prefers; this is the same for a link
 * followed inside the application, and for the development server.
 */
function Unlocalized() {
  const { pathname, search, hash } = useLocation();
  const language = useLanguage();

  const page = pathname.length > 1 ? pathname.replace(/\/+$/, '') : pathname;

  if (!UNLOCALIZED.has(page)) {
    return <NotFound />;
  }

  return <Navigate to={inLanguage(language, page) + search + hash} replace />;
}

function Loading() {
  const t = useT();

  return (
    <div className="flex flex-1 items-center justify-center gap-2 text-sm text-slate-500">
      <IconSpinner />
      {t.common.loadingEditor}
    </div>
  );
}

/** Every route starts at the top, the way arriving at a page does. */
function useScrollToTop(): void {
  const { pathname } = useLocation();

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);
}

/**
 * Catches an editor that refuses to load. Without one the failed import leaves
 * the fallback on screen for good, which reads as a spinner that never stops.
 */
class ChunkBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false };

  static getDerivedStateFromError() {
    return { failed: true };
  }

  render() {
    if (!this.state.failed) {
      return this.props.children;
    }

    return <ChunkFailure />;
  }
}

function ChunkFailure() {
  const t = useT();

  return (
    <div className="mx-auto max-w-md px-5 py-20 text-center">
      <h1 className="text-lg font-semibold">{t.common.editorFailed}</h1>
      <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{t.common.editorFailedWhy}</p>
      <button
        type="button"
        onClick={() => {
          sessionStorage.removeItem(RELOADED);
          window.location.reload();
        }}
        className="btn-primary mt-6"
      >
        {t.common.reload}
      </button>
    </div>
  );
}
