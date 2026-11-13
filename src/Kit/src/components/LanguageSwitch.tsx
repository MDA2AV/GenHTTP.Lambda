import { useEffect, useRef, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import { choose, useLanguage, useT } from '../i18n';
import { LANGUAGES, LANGUAGE_NAMES, inLanguage, languageOf, tagOf, type Language } from '../i18n/languages';
import { IconCheck, IconGlobe } from './Icons';

/**
 * Where the same page is in another language, or nothing for a page that has
 * no language in its address - the editor, where a choice applies in place.
 */
function useTargets(): ((language: Language) => string) | null {
  const { pathname, search, hash } = useLocation();

  if (languageOf(pathname) === null) {
    return null;
  }

  return (language) => inLanguage(language, pathname) + search + hash;
}

/**
 * Switches to the same page in another language.
 *
 * On a public page each language is a real link to its own address, which a
 * crawler follows as well; the choice is remembered, so an address without a
 * language takes the visitor there next time. Its words are fetched before
 * going, so the page is never drawn without them.
 */
function useSwitch(onDone?: () => void) {
  const navigate = useNavigate();
  const targets = useTargets();

  return (event: React.MouseEvent, language: Language) => {
    // a new tab, or a download: that is the browser's business
    if (event.metaKey || event.ctrlKey || event.shiftKey || event.button !== 0) {
      return;
    }

    event.preventDefault();

    void choose(language).then(() => {
      if (targets !== null) {
        navigate(targets(language));
      }

      onDone?.();
    });
  };
}

/** The switcher in the bar: the language being read, and the others behind it. */
export function LanguageSwitch() {
  const t = useT();
  const current = useLanguage();
  const [open, setOpen] = useState(false);
  const host = useRef<HTMLDivElement>(null);
  const { pathname } = useLocation();

  const pick = useSwitch(() => setOpen(false));

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
    <div ref={host} className="relative hidden sm:block">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        className="btn-ghost !px-2.5 uppercase"
        aria-expanded={open}
        aria-controls="site-languages"
        aria-label={t.shell.language}
        title={t.shell.language}
      >
        <IconGlobe />
        {/* the tag rather than the code, so the two Portuguese read PT-BR and PT-PT */}
        <span className="text-xs font-semibold tracking-wide">{tagOf(current)}</span>
      </button>

      {/* always in the page, so a crawler finds every language of it */}
      <div
        id="site-languages"
        hidden={!open}
        className="absolute right-0 top-full mt-2 w-52 border border-grey-200 bg-white py-1.5 shadow-lg dark:border-ink-800 dark:bg-ink-900"
      >
        <Choices onPick={pick} className="flex items-center justify-between gap-2 px-4 py-2 text-sm" />
      </div>
    </div>
  );
}

/** The same choices, as a section of the menu on a phone. */
export function LanguageMenu() {
  const t = useT();
  const pick = useSwitch();

  return (
    <div role="group" aria-label={t.shell.language} className="grid grid-cols-2 px-3">
      <Choices onPick={pick} className="flex items-center gap-2 px-2 py-2.5 text-[15px]" />
    </div>
  );
}

interface ChoicesProps {
  onPick: (event: React.MouseEvent, language: Language) => void;
  className: string;
}

function Choices({ onPick, className }: ChoicesProps) {
  const current = useLanguage();
  const targets = useTargets();

  return (
    <>
      {LANGUAGES.map((language) => {
        const active = language === current;

        const look = `${className} font-medium transition-colors ${
          active
            ? 'text-accent-600 dark:text-accent-400'
            : 'text-grey-800 hover:bg-grey-100 dark:text-grey-200 dark:hover:bg-ink-850'
        }`;

        const content = (
          <>
            {LANGUAGE_NAMES[language]}
            {active && <IconCheck className="h-4 w-4 shrink-0" />}
          </>
        );

        return targets === null ? (
          <button
            key={language}
            type="button"
            lang={tagOf(language)}
            onClick={(event) => onPick(event, language)}
            className={`${look} w-full text-left`}
            aria-current={active ? 'true' : undefined}
          >
            {content}
          </button>
        ) : (
          <a
            key={language}
            href={targets(language)}
            hrefLang={tagOf(language)}
            lang={tagOf(language)}
            onClick={(event) => onPick(event, language)}
            className={look}
            aria-current={active ? 'page' : undefined}
          >
            {content}
          </a>
        );
      })}
    </>
  );
}
