import { useEffect, useRef, useState, type ReactNode } from 'react';

import { IconCheck, IconChevronDown, IconCopy } from './Icons';

/** What copying says, in the words of the page it is on. */
export interface CopyWords {
  copy: string;
  copied: string;
}

/**
 * Copies a text, and says so for a moment - nothing at all where the
 * clipboard refuses, which is not worth an error.
 */
function useCopy(text: string) {
  const [copied, setCopied] = useState(false);

  async function copy() {
    try {
      await navigator.clipboard.writeText(text);
    } catch {
      return;
    }

    setCopied(true);
    window.setTimeout(() => setCopied(false), 1500);
  }

  return { copied, copy };
}

/** The address a repository is cloned from, on one line, with a way to copy it. */
export function CloneAddress({ url, words }: { url: string; words: CopyWords }) {
  const { copied, copy } = useCopy(url);

  return (
    <div className="flex items-stretch border border-slate-200 bg-slate-50 dark:border-ink-800 dark:bg-ink-950">
      <input
        readOnly
        value={url}
        onFocus={(event) => event.currentTarget.select()}
        aria-label={url}
        className="min-w-0 flex-1 bg-transparent px-3 py-2 font-mono text-[12px] text-ink-800 outline-none dark:text-slate-200"
      />
      <button
        type="button"
        onClick={copy}
        aria-label={copied ? words.copied : words.copy}
        title={copied ? words.copied : words.copy}
        className="border-l border-slate-200 px-2.5 text-slate-400 hover:bg-slate-100 hover:text-slate-700 dark:border-ink-800 dark:hover:bg-ink-850 dark:hover:text-slate-200"
      >
        {copied ? <IconCheck className="h-3.5 w-3.5 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5" />}
      </button>
    </div>
  );
}

/** A command to type, with a way to copy it. */
export function Command({ text, words }: { text: string; words: CopyWords }) {
  const { copied, copy } = useCopy(text);

  // the button in a column of its own, so a long line scrolls beside it rather than under it
  return (
    <div className="mt-1.5 flex items-stretch border border-slate-200 bg-slate-50 dark:border-ink-800 dark:bg-ink-950">
      <pre className="min-w-0 flex-1 overflow-x-auto px-3 py-2 font-mono text-[12px] leading-5">{text}</pre>
      <button
        type="button"
        onClick={copy}
        className="flex items-start border-l border-slate-200 px-2.5 pt-2.5 text-slate-400 hover:bg-slate-100 hover:text-slate-700 dark:border-ink-800 dark:hover:bg-ink-850 dark:hover:text-slate-200"
        aria-label={copied ? words.copied : words.copy}
        title={copied ? words.copied : words.copy}
      >
        {copied ? <IconCheck className="h-3.5 w-3.5 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5" />}
      </button>
    </div>
  );
}

/**
 * A button that opens a panel below it, closed again by Escape or a click
 * anywhere else - how a repository page offers its clone address.
 *
 * The panel lines up with the right edge of its button where there is room,
 * and is moved in from either edge of the window where there is not - on a
 * phone the button may stand anywhere in a row that wrapped.
 */
export function Popover({ label, title, button, width = 400, children }: {
  /** What the button says, beside its icon. */
  label: ReactNode;
  /** What the panel is, for whoever cannot see it. */
  title: string;
  /** How the button looks. */
  button: string;
  /** How wide the panel is where the window has room for it. */
  width?: number;
  children: ReactNode;
}) {
  const [place, setPlace] = useState<{ left: number; width: number } | null>(null);
  const host = useRef<HTMLDivElement>(null);

  const open = place != null;

  function toggle() {
    if (open || !host.current) {
      setPlace(null);
      return;
    }

    const box = host.current.getBoundingClientRect();
    const margin = 16;
    const wide = Math.min(width, window.innerWidth - 2 * margin);

    // its right edge under the button's, kept inside the window
    const left = Math.min(Math.max(box.right - wide, margin), window.innerWidth - margin - wide);

    setPlace({ left: left - box.left, width: wide });
  }

  useEffect(() => {
    if (!open) {
      return;
    }

    const onKey = (event: KeyboardEvent) => event.key === 'Escape' && setPlace(null);

    const onPointer = (event: PointerEvent) => {
      if (!host.current?.contains(event.target as Node)) {
        setPlace(null);
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
    <div ref={host} className="relative">
      <button type="button" onClick={toggle} aria-expanded={open} aria-haspopup="dialog" className={button}>
        {label}
        <IconChevronDown className="h-3.5 w-3.5 opacity-60" />
      </button>

      {place && (
        <div role="dialog" aria-label={title} style={{ left: place.left, width: place.width }}
             className="surface absolute top-full z-40 mt-2 p-4 text-left shadow-lg">
          {children}
        </div>
      )}
    </div>
  );
}
