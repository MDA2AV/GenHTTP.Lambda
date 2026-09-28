import { useEffect, useRef, useState } from 'react';
import { useLocation } from 'react-router-dom';

import { Footer } from './Footer';
import { IconClose, IconLock, IconLogo, IconMenu, IconMoon, IconSun } from './Icons';
import { LanguageMenu, LanguageSwitch } from './LanguageSwitch';
import { useFeatures } from '../features';
import { useT } from '../i18n';
import { Link, NavLink } from '../i18n/links';
import type { Theme } from '../theme';

interface Props {
  theme: Theme;
  onToggleTheme: () => void;
  actions?: React.ReactNode;
  children: React.ReactNode;
  /** Fills the viewport instead of scrolling, used by the editor. */
  fixed?: boolean;
}

/** A link in the bar, tinted while it is the page being looked at. */
const tab = ({ isActive }: { isActive: boolean }) =>
  `btn-ghost whitespace-nowrap !px-3.5 ${isActive ? 'bg-accent-500/10 dark:bg-accent-400/10' : ''}`;

export function Shell({ theme, onToggleTheme, actions, children, fixed }: Props) {
  // nothing behind the bar until the page has moved under it: at rest the
  // background belongs to the whole screen, and a tinted strip across the top
  // is exactly the seam this is meant not to have
  const [moved, setMoved] = useState(false);

  const features = useFeatures();
  const t = useT();

  useEffect(() => {
    const onScroll = () => setMoved(window.scrollY > 8);

    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });

    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  const themeLabel = theme === 'dark' ? t.shell.lightMode : t.shell.darkMode;

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
        <Link to="/" className="flex shrink-0 items-center gap-2.5 whitespace-nowrap font-semibold tracking-tight">
          <IconLogo />
          {/* the name is the mark plus the letter the thing is named after */}
          <span>
            GenHTTP{' '}
            <span className="text-accent-500" title="Lambda">
              <span aria-hidden="true" className="text-[1.15em] leading-none">λ</span>
              <span className="sr-only">Lambda</span>
            </span>
          </span>
        </Link>

        <nav className="ml-auto flex items-center gap-1" aria-label={t.shell.main}>
          {actions}

          {/* on the narrowest phones this one moves into the menu, the other stays */}
          <NavLink to="/build" className={(state) => `${tab(state)} hidden min-[460px]:inline-flex`}>
            {t.shell.build}
          </NavLink>

          <NavLink to="/ship" className={tab}>
            {t.shell.ship}
          </NavLink>

          <NavLink to="/showcase" className={(state) => `${tab(state)} hidden sm:inline-flex`}>
            {t.shell.showcase}
          </NavLink>

          {/*
            Switched off in the panel, the page is still there - just not
            linked. It and the docs join the bar a step later than their
            width in English would allow: the same words in Polish or
            Japanese take half as much room again, and the bar would push its
            own menu off the screen.
          */}
          {features.enterprise && (
            <NavLink to="/enterprise" className={(state) => `${tab(state)} hidden lg:inline-flex`}>
              {t.shell.enterprise}
            </NavLink>
          )}

          <NavLink to="/docs" className={(state) => `${tab(state)} hidden md:inline-flex`}>
            {t.shell.docs}
          </NavLink>

          <NavLink to="/admin" className={(state) => `${tab(state)} hidden items-center gap-1.5 lg:inline-flex`}>
            <IconLock className="h-4 w-4" />
            {t.shell.admin}
          </NavLink>

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

          <Menu
            theme={theme}
            themeLabel={themeLabel}
            onToggleTheme={onToggleTheme}
            enterprise={features.enterprise}
          />
        </nav>
      </header>

      <main className={fixed ? 'flex min-h-0 flex-1 flex-col' : 'flex-1'}>{children}</main>

      {!fixed && <Footer />}
    </div>
  );
}

interface MenuProps {
  theme: Theme;
  themeLabel: string;
  onToggleTheme: () => void;
  enterprise: boolean;
}

/**
 * Everything the bar has no room for, behind one button. It shows up as soon
 * as the first link has to leave the bar, and holds all of the ones that did -
 * so the pages are all one tap away on a phone, just not all at once.
 */
function Menu({ theme, themeLabel, onToggleTheme, enterprise }: MenuProps) {
  const [open, setOpen] = useState(false);
  const host = useRef<HTMLDivElement>(null);
  const { pathname } = useLocation();
  const t = useT();

  // going somewhere is the end of choosing where to go
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

  const item = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-2 px-5 py-3 text-[15px] font-medium transition-colors ${
      isActive
        ? 'bg-accent-500/10 text-accent-600 dark:bg-accent-400/10 dark:text-accent-400'
        : 'text-grey-800 hover:bg-grey-100 dark:text-grey-200 dark:hover:bg-ink-850'
    }`;

  return (
    <div ref={host} className="lg:hidden">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        className="btn-ghost !px-2"
        aria-expanded={open}
        aria-controls="site-menu"
        aria-label={open ? t.shell.closeMenu : t.shell.openMenu}
      >
        {open ? <IconClose className="h-5 w-5" /> : <IconMenu className="h-5 w-5" />}
      </button>

      {open && (
        <div
          id="site-menu"
          className="absolute inset-x-0 top-full border-y border-grey-200 bg-white py-2 shadow-lg sm:left-auto sm:right-4 sm:w-64 sm:border-x dark:border-ink-800 dark:bg-ink-900"
        >
          <NavLink to="/build" className={(state) => `${item(state)} min-[460px]:hidden`}>
            {t.shell.build}
          </NavLink>
          <NavLink to="/showcase" className={(state) => `${item(state)} sm:hidden`}>
            {t.shell.showcase}
          </NavLink>
          {enterprise && (
            <NavLink to="/enterprise" className={(state) => `${item(state)} lg:hidden`}>
              {t.shell.enterprise}
            </NavLink>
          )}
          <NavLink to="/docs" className={(state) => `${item(state)} md:hidden`}>
            {t.shell.docs}
          </NavLink>
          <NavLink to="/admin" className={item}>
            <IconLock className="h-4 w-4" />
            {t.shell.admin}
          </NavLink>

          <div className="mx-5 my-2 border-t border-grey-200 sm:hidden dark:border-ink-800" />

          <button
            type="button"
            onClick={onToggleTheme}
            className="flex w-full items-center gap-2 px-5 py-3 text-left text-[15px] font-medium text-grey-800 hover:bg-grey-100 sm:hidden dark:text-grey-200 dark:hover:bg-ink-850"
          >
            {theme === 'dark' ? <IconSun /> : <IconMoon />}
            {themeLabel}
          </button>

          {/* the bar has the switcher from here up */}
          <div className="sm:hidden">
            <div className="mx-5 my-2 border-t border-grey-200 dark:border-ink-800" />
            <LanguageMenu />
          </div>
        </div>
      )}
    </div>
  );
}
