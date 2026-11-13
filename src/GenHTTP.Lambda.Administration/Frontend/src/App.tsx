import { Route, Routes } from 'react-router-dom';

import { useScrollToTop } from '@genhttp-lambda/kit/app';
import { Shell } from '@genhttp-lambda/kit/components/Shell';
import { ToastHost } from '@genhttp-lambda/kit/components/Toast';
import { useTheme } from '@genhttp-lambda/kit/theme';

import { Admin } from './pages/Admin';

/** The operator's panel, behind the admin token - in English, like the rest of it. */
export function App() {
  const [theme, toggleTheme] = useTheme();

  useScrollToTop();

  return (
    <ToastHost>
      <Routes>
        <Route
          path="/admin/*"
          element={
            <Shell theme={theme} onToggleTheme={toggleTheme} fixed>
              <Admin theme={theme} />
            </Shell>
          }
        />
      </Routes>
    </ToastHost>
  );
}
