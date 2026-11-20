import { useEffect, useRef, useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';

import { IconAlert, IconCheck, IconChevronDown, IconChevronRight, IconExternal, IconInfo, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { Switch } from '@genhttp-lambda/kit/ui/ui';

/*
 * The parts every section of the panel is built from, and the one rule they
 * keep: what is shown and what is done are never mixed.
 *
 * - Information is in cards: figures, facts, charts, lists. It may link -
 *   to another section, a lambda, its editor - and a link looks like a link.
 * - Actions are buttons, and only in three places: the section's header (what
 *   applies to all of it), a card titled as such - the Manage column of a
 *   lambda - and the last cell of a row in a list. An action that changes
 *   something says what it will do in a dialog before it does it. A setting
 *   is the one exception: its switch or field is what it is.
 * - What waits for the operator is a notice at the top, which leads to where
 *   it is decided rather than deciding it there.
 */

/** A card of information: a title, what it is about, and what it holds. */
export function Card({ title, hint, more, children, className = '', flush = false }: {
  title?: ReactNode;
  /** A line under the title, for what the reader needs before reading on. */
  hint?: ReactNode;
  /** A link to where there is more, at the right of the title. */
  more?: { to: string; label: string };
  children: ReactNode;
  className?: string;
  /** Content that runs to the edges, a table say. */
  flush?: boolean;
}) {
  return (
    <section className={`surface min-w-0 ${className}`}>
      {(title || more) && (
        <header className={`flex items-start gap-3 ${flush ? 'border-b border-grey-300 px-4 py-3 dark:border-ink-800' : 'px-4 pt-4'}`}>
          <div className="min-w-0 flex-1">
            {title && <h2 className="text-sm font-medium">{title}</h2>}
            {hint && <p className="mt-0.5 text-[13px] text-slate-500">{hint}</p>}
          </div>
          {more && <More to={more.to}>{more.label}</More>}
        </header>
      )}
      <div className={flush ? '' : 'p-4'}>{children}</div>
    </section>
  );
}

/** A link on to another section, or further into this one. */
export function More({ to, children }: { to: string; children: ReactNode }) {
  return (
    <Link to={to} className="inline-flex shrink-0 items-center gap-0.5 text-[13px] text-accent-600 hover:underline dark:text-accent-400">
      {children}
      <IconChevronRight className="h-3.5 w-3.5" />
    </Link>
  );
}

/**
 * A row of headline figures, each a number with its name - and, where it
 * helps, what it is out of and how it ran over the last hours.
 */
export function Tiles({ children, columns = 4 }: { children: ReactNode; columns?: 2 | 3 | 4 | 5 }) {
  const grid = { 2: 'sm:grid-cols-2', 3: 'sm:grid-cols-3', 4: 'sm:grid-cols-2 lg:grid-cols-4', 5: 'sm:grid-cols-3 lg:grid-cols-5' }[columns];

  return (
    <div className={`grid grid-cols-2 gap-px border border-grey-300 bg-grey-300 dark:border-ink-800 dark:bg-ink-800 ${grid}`}>
      {children}
    </div>
  );
}

export function Tile({ label, value, detail, tone, to, title, children }: {
  label: string;
  value: ReactNode;
  /** What the value is out of, or a word on it, under it. */
  detail?: ReactNode;
  tone?: 'good' | 'warn' | 'bad';
  /** Where it leads, for a figure with more behind it. */
  to?: string;
  title?: string;
  /** A sparkline or a meter, under the figure. */
  children?: ReactNode;
}) {
  const colour = tone === 'bad' ? 'text-red-600 dark:text-red-400' : tone === 'warn' ? 'text-amber-600 dark:text-amber-400' : tone === 'good' ? 'text-emerald-600 dark:text-emerald-400' : '';

  const body = (
    <>
      <div className="text-xs text-slate-500">{label}</div>
      <div className={`mt-1 truncate text-xl font-semibold tracking-tight ${colour}`}>{value}</div>
      {detail && <div className="mt-0.5 text-xs text-slate-500">{detail}</div>}
      {children && <div className="mt-2">{children}</div>}
    </>
  );

  const box = 'block min-w-0 bg-white px-4 py-3.5 dark:bg-ink-900';

  return to ? (
    <Link to={to} title={title} className={`${box} hover:bg-slate-50 dark:hover:bg-ink-850`}>{body}</Link>
  ) : (
    <div title={title} className={box}>{body}</div>
  );
}

/** Facts about something, a name over each value. */
export function Facts({ children, columns = 3 }: { children: ReactNode; columns?: 2 | 3 }) {
  return (
    <dl className={`grid grid-cols-2 gap-x-8 gap-y-3 text-[13px] ${columns === 3 ? 'lg:grid-cols-3' : ''}`}>
      {children}
    </dl>
  );
}

export function Fact({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className="mt-0.5 break-words">{children}</dd>
    </div>
  );
}

export type Tone = 'problem' | 'wanted' | 'info';

/**
 * Something that waits for the operator, or that they should know: what it
 * is, a line on it, and where it is dealt with.
 */
export function Notice({ tone, title, children, to, action }: {
  tone: Tone;
  title: ReactNode;
  children?: ReactNode;
  to?: string;
  /** What the link says. */
  action?: string;
}) {
  const colour = {
    problem: 'border-red-500/40 bg-red-500/5',
    wanted: 'border-amber-500/40 bg-amber-500/5',
    info: 'border-grey-300 bg-white dark:border-ink-800 dark:bg-ink-900',
  }[tone];

  const icon = {
    problem: <IconAlert className="h-4 w-4 text-red-600 dark:text-red-400" />,
    wanted: <IconAlert className="h-4 w-4 text-amber-600 dark:text-amber-400" />,
    info: <IconInfo className="h-4 w-4 text-slate-400" />,
  }[tone];

  return (
    <div className={`flex items-start gap-3 border px-4 py-3 text-[13px] ${colour}`}>
      <span className="mt-0.5 shrink-0">{icon}</span>
      <div className="min-w-0 flex-1">
        <p className="font-medium text-ink-900 dark:text-slate-100">{title}</p>
        {children && <div className="mt-0.5 text-slate-600 dark:text-slate-400">{children}</div>}
      </div>
      {to && <More to={to}>{action ?? 'Review'}</More>}
    </div>
  );
}

/** That nothing waits: said, rather than left as an empty space. */
export function AllClear({ children }: { children: ReactNode }) {
  return (
    <div className="flex items-center gap-3 border border-grey-300 bg-white px-4 py-3 text-[13px] text-slate-600 dark:border-ink-800 dark:bg-ink-900 dark:text-slate-400">
      <span className="flex h-5 w-5 items-center justify-center rounded-full bg-emerald-500/15 text-emerald-600 dark:text-emerald-400">
        <IconCheck className="h-3.5 w-3.5" />
      </span>
      {children}
    </div>
  );
}

/**
 * The card actions are in, apart from what is shown: a title, and a row per
 * action - what it changes, where it stands, and the button that starts it.
 */
export function Actions({ title = 'Manage', hint, children }: { title?: string; hint?: ReactNode; children: ReactNode }) {
  return (
    <section className="surface" aria-label={title}>
      <header className="border-b border-grey-300 px-4 py-3 dark:border-ink-800">
        <h2 className="text-sm font-medium">{title}</h2>
        {hint && <p className="mt-0.5 text-xs text-slate-500">{hint}</p>}
      </header>
      <div className="divide-y divide-grey-300 dark:divide-ink-800">{children}</div>
    </section>
  );
}

export function Action({ label, state, children, note }: {
  label: string;
  /** Where it stands now, which the action changes. */
  state?: ReactNode;
  /** The button, or the buttons. */
  children?: ReactNode;
  /** Why it cannot be done, or what doing it means. */
  note?: ReactNode;
}) {
  return (
    <div className="px-4 py-3">
      <div className="flex flex-wrap items-center gap-x-3 gap-y-2">
        <div className="min-w-0 flex-1">
          <p className="text-xs text-slate-500">{label}</p>
          {state && <div className="mt-0.5 truncate text-[13px]">{state}</div>}
        </div>
        {children && <div className="flex shrink-0 flex-wrap justify-end gap-1.5">{children}</div>}
      </div>
      {note && <p className="mt-1.5 text-xs text-slate-500">{note}</p>}
    </div>
  );
}

/** The size every action button in a card or a row has. */
export const actionButton = '!px-3 !py-1.5 text-[13px]';

/** While a section reads what it shows, or why it could not. */
export function Reading({ what, failure }: { what: string; failure?: string | null }) {
  return failure ? (
    <p className="text-sm text-red-500">{failure}</p>
  ) : (
    <div className="flex items-center gap-2 text-sm text-slate-500">
      <IconSpinner /> Reading {what}…
    </div>
  );
}

/**
 * A choice of several, for narrowing a list: a button that says what is
 * chosen and opens the options to tick.
 */
export function Choices({ label, options, chosen, onChange }: {
  label: string;
  options: { value: string; label: string; hint?: string }[];
  chosen: string[];
  onChange: (chosen: string[]) => void;
}) {
  const [open, setOpen] = useState(false);
  const holder = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    const outside = (event: PointerEvent) => !holder.current?.contains(event.target as Node) && setOpen(false);
    const escape = (event: KeyboardEvent) => event.key === 'Escape' && setOpen(false);

    document.addEventListener('pointerdown', outside);
    document.addEventListener('keydown', escape);

    return () => {
      document.removeEventListener('pointerdown', outside);
      document.removeEventListener('keydown', escape);
    };
  }, [open]);

  const names = options.filter((o) => chosen.includes(o.value)).map((o) => o.label);

  return (
    <div ref={holder} className="relative">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        aria-expanded={open}
        aria-haspopup="true"
        className={`field flex items-center gap-2 py-1.5 text-left ${chosen.length > 0 ? '!border-accent-500 dark:!border-accent-400' : ''}`}
      >
        <span className="min-w-0 flex-1 truncate">
          {names.length === 0 ? label : `${label}: ${names.join(', ')}`}
        </span>
        <IconChevronDown className="h-4 w-4 shrink-0 text-slate-400" />
      </button>

      {open && (
        <div className="absolute left-0 z-30 mt-1 w-64 border border-grey-300 bg-white py-1 shadow-lg dark:border-ink-800 dark:bg-ink-900">
          {options.map((option) => (
            <label key={option.value} className="flex cursor-pointer items-start gap-2.5 px-3 py-1.5 text-sm hover:bg-slate-50 dark:hover:bg-ink-850">
              <input
                type="checkbox"
                className="mt-0.5 h-4 w-4 accent-accent-500"
                checked={chosen.includes(option.value)}
                onChange={() =>
                  onChange(chosen.includes(option.value) ? chosen.filter((c) => c !== option.value) : [...chosen, option.value])}
              />
              <span className="min-w-0">
                <span className="block">{option.label}</span>
                {option.hint && <span className="block text-xs text-slate-500">{option.hint}</span>}
              </span>
            </label>
          ))}
          {chosen.length > 0 && (
            <button type="button" onClick={() => onChange([])} className="mt-1 w-full border-t border-grey-300 px-3 py-2 text-left text-xs text-accent-600 hover:bg-slate-50 dark:border-ink-800 dark:text-accent-400 dark:hover:bg-ink-850">
              Clear
            </button>
          )}
        </div>
      )}
    </div>
  );
}

/**
 * A setting: what it is, what it does as it stands, how it fared lately, and
 * the switch that is what it is.
 */
export function Setting({ id, title, on, saving, onToggle, link, status, children }: {
  id: string;
  title: string;
  on: boolean;
  saving: boolean;
  onToggle: () => void;
  link?: string;
  /** What the setting came to lately, under what it does. */
  status?: ReactNode;
  children: ReactNode;
}) {
  return (
    <div className="surface flex items-start gap-4 p-4">
      <div className="min-w-0 flex-1">
        <p id={id} className="text-[15px] font-medium">{title}</p>
        <p className="mt-1 text-[13px] text-slate-500">
          {children}
          {link && (
            <>
              {' '}
              {/* a page of the site, which is a bundle of its own: loaded, not routed to */}
              <a href={link} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-accent-600 hover:underline dark:text-accent-400">
                Open it
                <IconExternal className="h-3 w-3" />
              </a>
            </>
          )}
        </p>
        {status && <p className="mt-2 text-xs text-slate-500">{status}</p>}
      </div>

      {saving && <IconSpinner />}

      <Switch on={on} onToggle={onToggle} labelledBy={id} />
    </div>
  );
}

/** A plain choice of one, as a select. */
export function Select<T extends string>({ label, value, options, onChange, className = '' }: {
  label: string;
  value: T;
  options: { value: T; label: string }[];
  onChange: (value: T) => void;
  className?: string;
}) {
  return (
    <select
      aria-label={label}
      value={value}
      onChange={(event) => onChange(event.target.value as T)}
      className={`field py-1.5 ${value !== '' && value !== options[0]?.value ? '!border-accent-500 dark:!border-accent-400' : ''} ${className}`}
    >
      {options.map((option) => (
        <option key={option.value} value={option.value}>{option.label}</option>
      ))}
    </select>
  );
}

/*
 * Figures as an operator reads them. English like the rest of the panel, so
 * they do not follow the language of the shell.
 */

export const count = (value: number) => value.toLocaleString('en-US');

const UNITS = ['B', 'kB', 'MB', 'GB', 'TB'];

export function bytes(value: number): string {
  let size = value;
  let unit = 0;

  while (size >= 1024 && unit < UNITS.length - 1) {
    size /= 1024;
    unit++;
  }

  return `${size.toFixed(size >= 100 || unit === 0 ? 0 : 1)} ${UNITS[unit]}`;
}

/** A part of a whole as a percentage - never a zero for a part that is there. */
export function share(part: number, whole: number): string {
  if (whole <= 0 || part <= 0) {
    return '0%';
  }

  const percent = (part / whole) * 100;

  return percent < 1 ? '<1%' : `${Math.round(percent)}%`;
}

export const plural = (n: number, one: string, many = `${one}s`) => `${count(n)} ${n === 1 ? one : many}`;

/** How long ago, in the coarsest unit that still says something. */
export function since(iso: string, now = Date.now()): string {
  const seconds = Math.max(0, (now - new Date(iso).getTime()) / 1000);

  if (seconds < 60) return 'less than a minute';
  if (seconds < 3600) return plural(Math.floor(seconds / 60), 'minute');
  if (seconds < 86400) return plural(Math.floor(seconds / 3600), 'hour');

  return plural(Math.floor(seconds / 86400), 'day');
}

export function duration(seconds: number): string {
  const days = Math.floor(seconds / 86400);
  const hours = Math.floor((seconds % 86400) / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);

  if (days > 0) return `${days}d ${hours}h`;
  if (hours > 0) return `${hours}h ${minutes}m`;

  return `${minutes}m`;
}
