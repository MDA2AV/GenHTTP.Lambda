import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { IconLogo, IconMoon, IconSun } from './Icons';
import type { Theme } from '../theme';

interface Props {
  theme: Theme;
  onToggleTheme: () => void;
  actions?: React.ReactNode;
  children: React.ReactNode;
  /** Fills the viewport instead of scrolling, used by the editor. */
  fixed?: boolean;
}

export function Shell({ theme, onToggleTheme, actions, children, fixed }: Props) {
  // nothing behind the bar until the page has moved under it: at rest the
  // background belongs to the whole screen, and a tinted strip across the top
  // is exactly the seam this is meant not to have
  const [moved, setMoved] = useState(false);

  useEffect(() => {
    const onScroll = () => setMoved(window.scrollY > 8);

    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });

    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  return (
    <div className={fixed ? 'flex h-screen flex-col overflow-hidden' : 'flex min-h-screen flex-col'}>
      {/*
        No rule under it, and the page showing through: the background belongs
        to the whole screen rather than starting below a bar. It stays put as
        the page moves, so the blur is what keeps the words on it legible.
      */}
      <header
        className={`sticky top-0 z-30 flex shrink-0 items-center gap-4 px-4 py-3 transition-colors duration-300 sm:px-6 ${
          moved || fixed ? 'bg-white/70 backdrop-blur-md dark:bg-ink-950/70' : 'bg-transparent'
        }`}
      >
        <Link to="/" className="flex items-center gap-2.5 font-semibold tracking-tight">
          <IconLogo />
          <span>
            GenHTTP <span className="text-accent-500">Lambda</span>
          </span>
        </Link>

        <div className="ml-auto flex items-center gap-2">
          {actions}
          <Link to="/stats" className="btn-ghost hidden sm:inline-flex">
            Server
          </Link>
          <Link to="/admin" className="btn-ghost hidden sm:inline-flex">
            Lambdas
          </Link>
          <button
            type="button"
            onClick={onToggleTheme}
            className="btn-ghost !px-2"
            aria-label={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
            title={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
          >
            {theme === 'dark' ? <IconSun /> : <IconMoon />}
          </button>
        </div>
      </header>

      <main className={fixed ? 'flex min-h-0 flex-1 flex-col' : 'flex-1'}>{children}</main>
    </div>
  );
}
