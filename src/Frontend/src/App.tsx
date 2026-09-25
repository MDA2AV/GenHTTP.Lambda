import { Component, Suspense, lazy, useEffect, type ReactNode } from 'react';
import { Navigate, Route, Routes, useLocation } from 'react-router-dom';

import { Shell } from './components/Shell';
import { IconSpinner } from './components/Icons';
import { ToastHost } from './components/Toast';
import { Create } from './pages/Create';
import { LambdaMissing } from './pages/LambdaMissing';
import { Landing } from './pages/Landing';
import { NotFound } from './pages/NotFound';
import { Admin } from './pages/Admin';
import { Guide } from './pages/Guide';
import { Build } from './pages/Build';
import { Showcase } from './pages/Showcase';
import { Terms } from './pages/Terms';
import { useTheme } from './theme';

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
  import('./pages/Editor')
    .then((module) => {
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
                <Route path="/" element={<Landing />} />
                <Route path="/docs" element={<Guide />} />
                <Route path="/build" element={<Build />} />
                <Route path="/showcase" element={<Showcase />} />
                {/* the page this replaced, which is linked from elsewhere */}
                <Route path="/agentic-coding" element={<Navigate to="/showcase" replace />} />
                <Route path="/terms" element={<Terms />} />
                <Route path="/lambda/:publicKey/*" element={<LambdaMissing />} />
                <Route path="*" element={<NotFound />} />
              </Routes>
            </Shell>
          }
        />
      </Routes>
    </ToastHost>
  );
}

function Loading() {
  return (
    <div className="flex flex-1 items-center justify-center gap-2 text-sm text-slate-500">
      <IconSpinner />
      Loading the editor…
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

    return (
      <div className="mx-auto max-w-md px-5 py-20 text-center">
        <h1 className="text-lg font-semibold">The editor could not be loaded</h1>
        <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
          This usually means the site was updated while this tab was open.
        </p>
        <button
          type="button"
          onClick={() => {
            sessionStorage.removeItem(RELOADED);
            window.location.reload();
          }}
          className="btn-primary mt-6"
        >
          Reload the page
        </button>
      </div>
    );
  }
}
