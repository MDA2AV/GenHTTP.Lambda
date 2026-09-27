import { useCallback, useEffect } from 'react';

import { useBrowserValue } from './site';

export type Theme = 'dark' | 'light';

const key = 'lambda-theme';

const CHANGED = 'lambda-theme-changed';

function read(): Theme {
  try {
    return localStorage.getItem(key) === 'light' ? 'light' : 'dark';
  } catch {
    return 'dark';
  }
}

let current: Theme | null = null;

const theme = () => (current ??= read());

function subscribe(changed: () => void) {
  window.addEventListener(CHANGED, changed);
  return () => window.removeEventListener(CHANGED, changed);
}

/**
 * Dark is the default; the choice is remembered per browser. A prerendered
 * page is drawn dark, and switched to the remembered theme right after.
 */
export function useTheme(): [Theme, () => void] {
  const shown = useBrowserValue(theme, 'dark', subscribe);

  useEffect(() => {
    // the remembered theme rather than the one drawn: while a prerendered page
    // is taken over it is drawn dark for a moment, and the document, which the
    // script in the index page already set right, must not follow that
    const applied = theme();

    document.documentElement.classList.toggle('dark', applied === 'dark');

    // the bar a phone draws above the page, so it does not stay dark over a light one
    document.querySelector<HTMLMetaElement>('meta[name="theme-color"]')?.setAttribute(
      'content',
      applied === 'dark' ? '#202124' : '#ffffff',
    );
  }, [shown]);

  const toggle = useCallback(() => {
    current = theme() === 'dark' ? 'light' : 'dark';

    try {
      localStorage.setItem(key, current);
    } catch {
      /* private mode */
    }

    window.dispatchEvent(new Event(CHANGED));
  }, []);

  return [shown, toggle];
}
