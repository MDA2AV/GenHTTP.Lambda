import { useEffect, useRef, useState, type ReactNode } from 'react';
import { useLocation } from 'react-router-dom';

import { Footer } from '../components/Footer';
import { IconClose, IconLogo, IconMenu, IconMoon, IconSun } from '../components/Icons';
import { LanguageMenu, LanguageSwitch } from '../components/LanguageSwitch';
import { useSourceT, useT } from '../i18n';
import { Link } from '../i18n/links';
import type { Theme } from '../theme';

/**
 * The frame of the published sources.
 *
 * Not the site's bar: these pages are not somewhere the landing page leads,
 * and a visitor who came to read code has no use for its menu. The brand
 * leads home, the section name leads to every project, and the language and
 * the theme are where they are on every other page - the rest of the width
 * belongs to the code. The footer is the site's, with the report link and
 * the legal pages a step away as everywhere.
 */
export function SourceShell({ theme, onToggleTheme, children }: {
  theme: Theme;
  onToggleTheme: () => void;
  children: ReactNode;
}) {
  const t = useT();
  const said = useSourceT();
  const [moved, setMoved] = useState(false);

  useEffect(() => {
    const onScroll = () => setMoved(window.scrollY > 8);

    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });

    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  const themeLabel = theme === 'dark' ? t.shell.lightMode : t.shell.darkMode;

  return (
    <div className="flex min-h-screen flex-col">
      <header
        className={`sticky top-0 z-30 flex shrink-0 items-center gap-3 px-4 py-3 transition-colors duration-300 sm:px-6 ${
          moved ? 'bg-white/80 backdrop-blur-md dark:bg-ink-950/80' : 'bg-transparent'
        }`}
      >
        <Link to="/" className="flex shrink-0 items-center gap-2.5 whitespace-nowrap font-semibold tracking-tight" aria-label={said.shell.home}>
          <IconLogo />
          <span className="hidden min-[400px]:inline">
            GenHTTP{' '}
            <span className="text-accent-500" title="Lambda">
              <span aria-hidden="true" className="text-[1.15em] leading-none">λ</span>
              <span className="sr-only">Lambda</span>
            </span>
          </span>
        </Link>

        <span aria-hidden="true" className="text-lg font-light text-slate-300 dark:text-ink-700">/</span>

        <Link
          to="/source"
          className="whitespace-nowrap text-[15px] font-medium text-slate-700 hover:text-accent-600 dark:text-slate-300 dark:hover:text-accent-400"
        >
          {said.shell.section}
        </Link>

        <nav className="ml-auto flex items-center gap-1" aria-label={t.shell.main}>
          <LanguageSwitch />

          <button
            type="button"
            onClick={onToggleTheme}
            className="btn-ghost hidden !px-2 sm:inline-flex"
            aria-label={themeLabel}
            title={themeLabel}
          >
            {theme === 'dark' ? <IconSun /> : <IconMoon />}
          </button>

          <Menu theme={theme} themeLabel={themeLabel} onToggleTheme={onToggleTheme} />
        </nav>
      </header>

      <main className="flex-1">{children}</main>

      <Footer />
    </div>
  );
}

/** The theme and the languages on a phone, where the bar has no room for them. */
function Menu({ theme, themeLabel, onToggleTheme }: { theme: Theme; themeLabel: string; onToggleTheme: () => void }) {
  const t = useT();
  const [open, setOpen] = useState(false);
  const host = useRef<HTMLDivElement>(null);
  const { pathname } = useLocation();

  useEffect(() => setOpen(false), [pathname]);

  useEffect(() => {
    if (!open) {
      return;
    }

    const onKey = (event: KeyboardEvent) => event.key === 'Escape' && setOpen(false);

    const onPointer = (event: PointerEvent) => {
      if (!host.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    };

    document.addEventListener('keydown', onKey);
    document.addEventListener('pointerdown', onPointer);

    return () => {
      document.removeEventListener('keydown', onKey);
      document.removeEventListener('pointerdown', onPointer);
    };
  }, [open]);

  return (
    <div ref={host} className="sm:hidden">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        className="btn-ghost !px-2"
        aria-expanded={open}
        aria-controls="source-menu"
        aria-label={open ? t.shell.closeMenu : t.shell.openMenu}
      >
        {open ? <IconClose className="h-5 w-5" /> : <IconMenu className="h-5 w-5" />}
      </button>

      {open && (
        <div id="source-menu" className="absolute inset-x-0 top-full border-y border-grey-200 bg-white py-2 shadow-lg dark:border-ink-800 dark:bg-ink-900">
          <button
            type="button"
            onClick={onToggleTheme}
            className="flex w-full items-center gap-2 px-5 py-3 text-left text-[15px] font-medium text-grey-800 hover:bg-grey-100 dark:text-grey-200 dark:hover:bg-ink-850"
          >
            {theme === 'dark' ? <IconSun /> : <IconMoon />}
            {themeLabel}
          </button>

          <div className="mx-5 my-2 border-t border-grey-200 dark:border-ink-800" />

          <LanguageMenu />
        </div>
      )}
    </div>
  );
}
