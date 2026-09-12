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
  return (
    <div className={fixed ? 'flex h-screen flex-col overflow-hidden' : 'flex min-h-screen flex-col'}>
      <header className="flex shrink-0 items-center gap-4 border-b border-slate-200 px-4 py-3 dark:border-ink-800 sm:px-6">
        <Link to="/" className="flex items-center gap-2.5 font-semibold tracking-tight">
          <IconLogo />
          <span>
            GenHTTP <span className="text-accent-500">Lambda</span>
          </span>
        </Link>

        <div className="ml-auto flex items-center gap-2">
          {actions}
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
