import { Suspense, lazy } from 'react';
import { Navigate, Route, Routes, useLocation, useParams } from 'react-router-dom';

import { ChunkBoundary, recovering, useScrollToTop } from '@genhttp-lambda/kit/app';
import { IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { ToastHost } from '@genhttp-lambda/kit/components/Toast';
import { useLanguage } from '@genhttp-lambda/kit/i18n';
import { inLanguage, isLanguage, languageOf, preferredLanguage } from '@genhttp-lambda/kit/i18n/languages';
import { useTheme } from '@genhttp-lambda/kit/theme';

import { loadSource } from './i18n';

/** The pages of the published sources, with their words and their highlighter. */
const Source = lazy(() =>
  recovering(() =>
    Promise.all([import('./SourceApp'), loadSource(languageOf(window.location.pathname) ?? preferredLanguage())]),
  ).then(([module]) => ({ default: module.SourceApp })),
);

/** The published sources, in a frame of their own (see SourceApp). */
export function App() {
  const [theme, toggleTheme] = useTheme();

  useScrollToTop();

  return (
    <ToastHost>
      <Routes>
        <Route path="/:language/source/*" element={<Localized theme={theme} onToggleTheme={toggleTheme} />} />
        <Route path="*" element={<Unlocalized />} />
      </Routes>
    </ToastHost>
  );
}

function Localized({ theme, onToggleTheme }: { theme: 'light' | 'dark'; onToggleTheme: () => void }) {
  const { language } = useParams();

  if (!isLanguage(language)) {
    return <Unlocalized />;
  }

  return (
    <ChunkBoundary page>
      <Suspense fallback={<SourceLoading />}>
        <Source theme={theme} onToggleTheme={onToggleTheme} />
      </Suspense>
    </ChunkBoundary>
  );
}

/** A page of the sources asked for without a language, which the server sends on as well. */
function Unlocalized() {
  const { pathname, search, hash } = useLocation();
  const language = useLanguage();

  return <Navigate to={inLanguage(language, pathname) + search + hash} replace />;
}

function SourceLoading() {
  return (
    <div className="flex min-h-screen items-center justify-center text-slate-500">
      <IconSpinner />
    </div>
  );
}
