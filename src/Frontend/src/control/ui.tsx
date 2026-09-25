import { useEffect, useRef, useState, type ReactNode } from 'react';

import { IconDots, IconInfo, IconSpark } from '../components/Icons';
import { ago } from './format';

/**
 * The parts every section of the control center is built from.
 *
 * Three kinds of navigation live on one screen, so each has one form and no
 * other: the site's links along the top, the lambda's sections down the
 * sidebar, and a section's own views as a row of pills under its title.
 * Buttons are actions and never navigate.
 */

/**
 * One section: its title, what it is for on hover rather than in a paragraph,
 * its actions on the right and its views underneath.
 */
export function Section({
  title,
  hint,
  actions,
  pills,
  children,
  flush = false,
}: {
  title: ReactNode;
  hint?: ReactNode;
  actions?: ReactNode;
  pills?: ReactNode;
  children: ReactNode;
  /** Content that fills the rest of the height rather than scrolling in a column, like the code view. */
  flush?: boolean;
}) {
  // the page scrolls, not the section; only the code, which fills the height,
  // is a column of its own
  return (
    <div className={flush ? 'flex min-h-0 flex-1 flex-col' : 'min-w-0'}>
      <header className="px-4 pt-6 md:px-0">
        <div className="flex min-h-9 flex-wrap items-center gap-x-3 gap-y-2">
          <h1 className="text-lg font-semibold tracking-tight">{title}</h1>
          {hint && <Hint>{hint}</Hint>}
          {actions && <div className="ml-auto flex flex-wrap items-center gap-1.5">{actions}</div>}
        </div>
        {pills && <div className="mt-3 flex flex-wrap items-center gap-1.5">{pills}</div>}
      </header>

      {flush ? (
        <div className="mt-4 flex min-h-0 flex-1 flex-col pb-6">{children}</div>
      ) : (
        <div className="w-full px-4 pb-16 pt-5 md:px-0">{children}</div>
      )}
    </div>
  );
}

/** How a pill looks, for the rows that are more than a plain choice - the files of the code view. */
export const pill = (active: boolean) =>
  `inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-[13px] transition-colors ${
    active
      ? 'border-accent-500 bg-accent-500/10 text-accent-700 dark:border-accent-400 dark:text-accent-400'
      : 'border-slate-200 text-slate-600 hover:border-slate-300 hover:text-slate-900 dark:border-ink-800 dark:text-slate-400 dark:hover:border-ink-700 dark:hover:text-slate-200'
  }`;

/** A section's views, of which one is showing. */
export function Pills<T extends string | number>({
  options,
  value,
  onChange,
  label,
}: {
  options: { value: T; label: ReactNode; title?: string }[];
  value: T;
  onChange: (value: T) => void;
  label: string;
}) {
  return (
    <div role="radiogroup" aria-label={label} className="flex flex-wrap gap-1.5">
      {options.map((option) => (
        <button
          key={String(option.value)}
          type="button"
          role="radio"
          aria-checked={option.value === value}
          title={option.title}
          onClick={() => onChange(option.value)}
          className={pill(option.value === value)}
        >
          {option.label}
        </button>
      ))}
    </div>
  );
}

/**
 * An explanation that is there when it is wanted: on hover, and on focus for
 * anybody who is not using a pointer.
 */
export function Hint({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <span className={`group relative inline-flex ${className}`}>
      <button type="button" className="rounded-full text-slate-400 hover:text-slate-600 dark:hover:text-slate-200" aria-label="What this is">
        <IconInfo className="h-4 w-4" />
      </button>
      <span
        role="tooltip"
        className="pointer-events-none invisible absolute left-1/2 top-full z-40 mt-2 w-72 -translate-x-1/2 border border-slate-200 bg-white p-3 text-xs font-normal leading-relaxed text-slate-600 opacity-0 shadow-lg transition-opacity group-focus-within:visible group-focus-within:opacity-100 group-hover:visible group-hover:opacity-100 dark:border-ink-800 dark:bg-ink-900 dark:text-slate-300"
      >
        {children}
      </span>
    </span>
  );
}

/** A moment as people say it - "3 min ago" - with the exact time on hover. */
export function Ago({ at, className = '' }: { at?: string | null; className?: string }) {
  if (!at) {
    return <span className={className}>never</span>;
  }

  return (
    <time dateTime={at} title={new Date(at).toLocaleString()} className={className}>
      {ago(at)}
    </time>
  );
}

/** A figure with its name underneath, for the row at the top of a section. */
export function Figure({ value, label, tone = 'default', title }: {
  value: ReactNode;
  label: ReactNode;
  tone?: 'default' | 'good' | 'warn' | 'bad';
  title?: string;
}) {
  const colour = {
    default: '',
    good: 'text-emerald-600 dark:text-emerald-400',
    warn: 'text-amber-600 dark:text-amber-400',
    bad: 'text-red-500 dark:text-red-400',
  }[tone];

  return (
    <div className="min-w-0" title={title}>
      <div className={`text-2xl font-semibold tabular-nums tracking-tight ${colour}`}>{value}</div>
      <div className="mt-0.5 text-[13px] text-slate-500">{label}</div>
    </div>
  );
}

/**
 * How much of an allowance is spent. Amber, then red, as it fills.
 *
 * A unit that is the same on both sides is named once, after the pair, rather
 * than abbreviated onto each number: "30.4k / 262k characters" says what it
 * counts, where "30.4k ch / 262k ch" leaves the reader guessing.
 */
export function Meter({ label, used, of, format, unit, extra }: {
  label: ReactNode;
  used: number;
  of: number;
  format: (n: number) => string;
  unit?: string;
  extra?: ReactNode;
}) {
  const share = of > 0 ? Math.min(1, used / of) : 0;

  const bar = share > 0.9 ? 'bg-red-500' : share > 0.7 ? 'bg-amber-500' : 'bg-accent-500 dark:bg-accent-400';

  return (
    <div>
      <div className="flex items-center justify-between gap-3 text-[13px]">
        <span className="flex items-center gap-1.5 text-slate-700 dark:text-slate-300">
          {label}
          {extra}
        </span>
        <span className="shrink-0 tabular-nums text-slate-500" title={`${format(used)} of ${format(of)}${unit ? ` ${unit}` : ''}`}>
          {format(used)} <span className="text-slate-400">/ {format(of)}{unit ? ` ${unit}` : ''}</span>
        </span>
      </div>
      <div className="mt-1.5 h-1 w-full bg-slate-200 dark:bg-ink-800" role="meter" aria-valuemin={0} aria-valuemax={of} aria-valuenow={used}>
        <div className={`h-full ${bar}`} style={{ width: `${Math.max(share > 0 ? 1 : 0, share * 100)}%` }} />
      </div>
    </div>
  );
}

/**
 * A row of bars, one per interval. Bars rather than a line: each counts what
 * happened in its interval rather than a reading at an instant.
 */
export function Sparkline({ values, label }: { values: number[]; label: string }) {
  const max = Math.max(1, ...values);
  const width = 100;
  const height = 24;
  const step = width / Math.max(1, values.length);

  return (
    <svg viewBox={`0 0 ${width} ${height}`} preserveAspectRatio="none" className="h-6 w-full" role="img" aria-label={label}>
      {values.map((value, i) => {
        const h = value > 0 ? Math.max(1.5, (value / max) * height) : 0;

        return (
          <rect key={i} x={i * step + step * 0.15} y={height - h} width={step * 0.7} height={h}
                className="fill-accent-500/60 dark:fill-accent-400/60" />
        );
      })}
    </svg>
  );
}

/**
 * That an agent wrote something. The only origin worth marking: the rest -
 * the template, the owner, the platform - are what anybody would assume.
 */
export function AgentMark({ origin }: { origin?: string | null }) {
  if (origin !== 'agent') {
    return null;
  }

  return (
    <span title="Written by an agent" className="inline-flex text-accent-500 dark:text-accent-400">
      <IconSpark className="h-3.5 w-3.5" />
      <span className="sr-only">by an agent</span>
    </span>
  );
}

/**
 * The tier a lambda is in. Quiet for the tier everybody starts in, marked for
 * the one somebody was given.
 */
export function TierBadge({ tier, className = '' }: { tier: string; className?: string }) {
  const premium = tier === 'Premium';

  return (
    <span
      title={premium ? 'Premium: may answer at a domain of its own, and is kept online however quiet it gets' : `${tier} tier`}
      className={`chip !px-1.5 !py-0 text-[11px] uppercase tracking-wide ${
        premium
          ? 'bg-amber-500/15 text-amber-700 dark:text-amber-400'
          : 'bg-slate-400/10 text-slate-500'
      } ${className}`}
    >
      {tier}
    </span>
  );
}

export function LiveDot({ live }: { live: boolean }) {
  return (
    <span
      className={`inline-block h-2 w-2 shrink-0 rounded-full ${live ? 'bg-emerald-500' : 'bg-slate-400'}`}
      aria-hidden="true"
    />
  );
}

export function LevelMark({ level }: { level: string }) {
  const tone =
    level === 'error' || level === 'critical'
      ? 'bg-red-500'
      : level === 'warn'
        ? 'bg-amber-500'
        : 'bg-transparent';

  return <span className={`mt-1.5 h-2 w-2 shrink-0 rounded-full ${tone}`} title={level} aria-label={level} />;
}

export function Empty({ children }: { children: ReactNode }) {
  return <p className="py-10 text-center text-sm text-slate-500">{children}</p>;
}

/** A specification, set apart as somebody else's words. */
export function Quote({ children }: { children: ReactNode }) {
  return (
    <blockquote className="whitespace-pre-line border-l-2 border-slate-300 pl-3 text-sm text-slate-600 dark:border-ink-700 dark:text-slate-400">
      {children}
    </blockquote>
  );
}

/** A quiet button that opens a list of the things done rarely enough not to need a button each. */
export function Menu({ label = 'More', children, align = 'right' }: {
  label?: string;
  children: (close: () => void) => ReactNode;
  align?: 'left' | 'right';
}) {
  const [open, setOpen] = useState(false);
  const holder = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    const outside = (event: MouseEvent) => {
      if (!holder.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    };

    const escape = (event: KeyboardEvent) => event.key === 'Escape' && setOpen(false);

    document.addEventListener('mousedown', outside);
    document.addEventListener('keydown', escape);

    return () => {
      document.removeEventListener('mousedown', outside);
      document.removeEventListener('keydown', escape);
    };
  }, [open]);

  return (
    <div ref={holder} className="relative">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        className="flex h-8 w-8 items-center justify-center rounded-full text-slate-500 hover:bg-slate-100 hover:text-slate-900 dark:hover:bg-ink-850 dark:hover:text-slate-100"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-label={label}
        title={label}
      >
        <IconDots className="h-5 w-5" />
      </button>

      {open && (
        <div role="menu" className={`surface absolute top-full z-40 mt-1 w-64 py-1 shadow-lg ${align === 'right' ? 'right-0' : 'left-0'}`}>
          {children(() => setOpen(false))}
        </div>
      )}
    </div>
  );
}

export const menuItem = 'flex w-full items-center gap-2.5 px-3 py-2 text-left text-sm hover:bg-slate-100 disabled:opacity-50 dark:hover:bg-ink-850';

export const menuRule = <div className="my-1 border-t border-slate-200 dark:border-ink-800" />;
