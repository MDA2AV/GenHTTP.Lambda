import { useEffect, useRef, useState } from 'react';
import { useLocation } from 'react-router-dom';

import { Footer } from './Footer';
import { IconClose, IconLock, IconLogo, IconMenu, IconMoon, IconSun } from './Icons';
import { LanguageMenu, LanguageSwitch } from './LanguageSwitch';
import { useT } from '../i18n';
import { Link, NavLink } from '../i18n/links';
import type { Theme } from '../theme';

/**
 * From which width of the screen a link has room in the bar; below it, it is
 * in the menu. The classes are written out in full for Tailwind to find.
 */
export type Room = 'always' | 'xs' | 'sm' | 'md' | 'lg';

const IN_BAR: Record<Room, string> = {
  always: '',
  xs: 'hidden min-[460px]:inline-flex',
  sm: 'hidden sm:inline-flex',
  md: 'hidden md:inline-flex',
  lg: 'hidden items-center gap-1.5 lg:inline-flex',
};

const IN_MENU: Record<Room, string> = {
  always: 'hidden',
  xs: 'min-[460px]:hidden',
  sm: 'sm:hidden',
  md: 'md:hidden',
  lg: '',
};

/** A page the bar links to. */
export interface ShellLink {
  to: string;
  label: string;
  room: Room;
  /** A lock in front of it, for the operator's panel. */
  locked?: boolean;
}

interface Props {
  theme: Theme;
  onToggleTheme: () => void;
  actions?: React.ReactNode;
  children: React.ReactNode;
  /** Fills the viewport instead of scrolling, used by the editor. */
  fixed?: boolean;
  /**
   * The pages the bar links to, which differ between the sites: the
   * cloud's marketing pages, the guide and the showcase everywhere, and the
   * operator's panel. Without, the bar links to none.
   */
  links?: ShellLink[];
}

/** A link in the bar, tinted while it is the page being looked at. */
const tab = ({ isActive }: { isActive: boolean }) =>
  `btn-ghost whitespace-nowrap !px-3.5 ${isActive ? 'bg-accent-500/10 dark:bg-accent-400/10' : ''}`;

export function Shell({ theme, onToggleTheme, actions, children, fixed, links = [] }: Props) {
  // nothing behind the bar until the page has moved under it: at rest the
  // background belongs to the whole screen, and a tinted strip across the top
  // is exactly the seam this is meant not to have
  const [moved, setMoved] = useState(false);

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

          {/* each link moves into the menu where the bar has no room for it */}
          {links.map((link) => (
            <NavLink key={link.to} to={link.to} className={(state) => `${tab(state)} ${IN_BAR[link.room]}`}>
              {link.locked && <IconLock className="h-4 w-4" />}
              {link.label}
            </NavLink>
          ))}

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

          <Menu theme={theme} themeLabel={themeLabel} onToggleTheme={onToggleTheme} links={links} />
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
  links: ShellLink[];
}

/**
 * Everything the bar has no room for, behind one button. It shows up as soon
 * as the first link has to leave the bar, and holds all of the ones that did -
 * so the pages are all one tap away on a phone, just not all at once.
 */
function Menu({ theme, themeLabel, onToggleTheme, links }: MenuProps) {
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
          {links
            .filter((link) => link.room !== 'always')
            .map((link) => (
              <NavLink key={link.to} to={link.to} className={(state) => `${item(state)} ${IN_MENU[link.room]}`}>
                {link.locked && <IconLock className="h-4 w-4" />}
                {link.label}
              </NavLink>
            ))}

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
