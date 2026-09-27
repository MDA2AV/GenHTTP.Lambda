import { useCallback, useEffect, useState } from 'react';

export type Theme = 'dark' | 'light';

const key = 'lambda-theme';

function read(): Theme {
  try {
    return localStorage.getItem(key) === 'light' ? 'light' : 'dark';
  } catch {
    return 'dark';
  }
}

/** Dark is the default; the choice is remembered per browser. */
export function useTheme(): [Theme, () => void] {
  const [theme, setTheme] = useState<Theme>(read);

  useEffect(() => {
    document.documentElement.classList.toggle('dark', theme === 'dark');

    // the bar a phone draws above the page, so it does not stay dark over a light one
    document.querySelector<HTMLMetaElement>('meta[name="theme-color"]')?.setAttribute(
      'content',
      theme === 'dark' ? '#202124' : '#ffffff',
    );

    try {
      localStorage.setItem(key, theme);
    } catch {
      /* private mode */
    }
  }, [theme]);

  const toggle = useCallback(() => setTheme((current) => (current === 'dark' ? 'light' : 'dark')), []);

  return [theme, toggle];
}
