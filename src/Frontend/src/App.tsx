import { Suspense, lazy } from 'react';
import { Route, Routes } from 'react-router-dom';

import { Shell } from './components/Shell';
import { IconSpinner } from './components/Icons';
import { ToastHost } from './components/Toast';
import { Create } from './pages/Create';
import { LambdaMissing } from './pages/LambdaMissing';
import { Landing } from './pages/Landing';
import { NotFound } from './pages/NotFound';
import { useTheme } from './theme';

// Monaco is most of the bundle, so the landing page never downloads it
const Editor = lazy(() => import('./pages/Editor').then((module) => ({ default: module.Editor })));

export function App() {
  const [theme, toggleTheme] = useTheme();

  return (
    <ToastHost>
      <Routes>
        <Route
          path="/editor/:privateKey"
          element={
            <Shell theme={theme} onToggleTheme={toggleTheme} fixed>
              <Suspense fallback={<Loading />}>
                <Editor theme={theme} />
              </Suspense>
            </Shell>
          }
        />
        <Route
          path="*"
          element={
            <Shell theme={theme} onToggleTheme={toggleTheme}>
              <Routes>
                <Route path="/" element={<Landing />} />
                <Route path="/editor/create" element={<Create />} />
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
