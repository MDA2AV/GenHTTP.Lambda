import { useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';

import { count, share } from './parts';

/*
 * The charts of the dashboards that are not a series over time - the kit's
 * Chart draws those. Each answers one question: how a whole splits (a
 * segmented bar), or which parts are the largest (ranked bars).
 */

/** A colour as a step for each surface, validated against both. */
export type Pair = [light: string, dark: string];

// the categorical order, checked with the palette validator against both
// surfaces, for colour blindness too - the server section's, in its order
export const BLUE: Pair = ['#1a73e8', '#4285f4'];
export const ORANGE: Pair = ['#e8710a', '#d56e0c'];
export const PURPLE: Pair = ['#9334e6', '#a142f4'];
export const GREEN: Pair = ['#188038', '#34a853'];
/** What is left over, or nothing in particular. */
export const GREY: Pair = ['#5f6368', '#9aa0a6'];
/** Reserved for what is wrong: a blocked lambda, a server error. */
export const RED: Pair = ['#c5221f', '#ea4335'];

export interface Part {
  label: string;
  value: number;
  color: Pair;
  /** Where the part leads, the lambdas it counts say. */
  to?: string;
  /** A word on what the part is, under its name in the legend. */
  hint?: string;
}

/**
 * One whole, split into a handful of parts: a bar of blocks, each its share
 * printed where it fits, and a legend that names them with their figures.
 * The figures are a table one click away, for whoever cannot tell the
 * colours apart.
 */
export function Segments({ parts, dark, label, unit = (n) => count(n) }: {
  parts: Part[];
  dark: boolean;
  /** What the bar is, for whoever cannot see it. */
  label: string;
  /** How a value is said. */
  unit?: (value: number) => string;
}) {
  const [table, setTable] = useState(false);

  const whole = parts.reduce((sum, p) => sum + p.value, 0);
  const shown = parts.filter((p) => p.value > 0);

  if (whole === 0) {
    return <p className="py-3 text-[13px] text-slate-500">Nothing to count yet.</p>;
  }

  return (
    <div>
      <div
        role="img"
        aria-label={`${label}: ${shown.map((p) => `${p.label} ${unit(p.value)}, ${share(p.value, whole)}`).join('; ')}`}
        className="flex h-7 w-full gap-[2px]"
      >
        {shown.map((part) => {
          const width = (part.value / whole) * 100;

          return (
            <div
              key={part.label}
              title={`${part.label}: ${unit(part.value)} (${share(part.value, whole)})`}
              className="flex min-w-[3px] items-center overflow-hidden rounded px-2 text-[11px] font-medium"
              style={{ width: `${width}%`, backgroundColor: part.color[dark ? 1 : 0], color: ink(part.color[dark ? 1 : 0]) }}
            >
              {/* a share printed only where it fits, never cut */}
              {width >= 9 && <span className="whitespace-nowrap">{share(part.value, whole)}</span>}
            </div>
          );
        })}
      </div>

      {table ? (
        <table className="mt-3 w-full text-[13px]">
          <tbody className="divide-y divide-grey-300 dark:divide-ink-800">
            {parts.map((part) => (
              <tr key={part.label}>
                <td className="py-1.5">{part.label}</td>
                <td className="py-1.5 text-right tabular-nums">{unit(part.value)}</td>
                <td className="w-14 py-1.5 text-right tabular-nums text-slate-500">{share(part.value, whole)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <ul className="mt-3 flex flex-wrap gap-x-5 gap-y-2 text-[13px]">
          {/* the parts that are there; the table lists the rest at nothing */}
          {shown.map((part) => (
            <li key={part.label} className="flex min-w-0 items-start gap-2">
              <span aria-hidden="true" className="mt-1.5 h-2 w-2 shrink-0 rounded-full" style={{ backgroundColor: part.color[dark ? 1 : 0] }} />
              <span className="min-w-0">
                <Named to={part.to}>{part.label}</Named>
                <span className="ml-1.5 tabular-nums text-slate-500">{unit(part.value)}</span>
                {part.hint && <span className="block text-xs text-slate-500">{part.hint}</span>}
              </span>
            </li>
          ))}
        </ul>
      )}

      <button type="button" onClick={() => setTable((was) => !was)} className="mt-2 text-xs text-accent-600 hover:underline dark:text-accent-400">
        {table ? 'Show the legend' : 'Show as a table'}
      </button>
    </div>
  );
}

export interface Rank {
  key: string;
  label: ReactNode;
  value: number;
  /** Where the row leads. */
  to?: string;
  /** What the value is said as, where a count is not it. */
  said?: string;
  /** A word beside the figure - its share of something. */
  aside?: string;
  /** Drawn in red, for what is wrong. */
  bad?: boolean;
  mono?: boolean;
}

/**
 * The largest parts first, one bar each on one baseline - which compares
 * them more exactly than blocks laid end to end. One hue: the bars are told
 * apart by their names, not their colours.
 */
export function Ranked({ rows, empty, limit = 8, of }: {
  rows: Rank[];
  empty: string;
  /** How many are shown before the rest is asked for. */
  limit?: number;
  /** The whole the bars are measured against, where it is not the largest - every lambda, say. */
  of?: number;
}) {
  const [all, setAll] = useState(false);

  if (rows.length === 0) {
    return <p className="py-3 text-[13px] text-slate-500">{empty}</p>;
  }

  const largest = Math.max(1, of ?? 0, ...rows.map((r) => r.value));
  const shown = all ? rows : rows.slice(0, limit);

  return (
    <>
      <ul className="space-y-1">
        {shown.map((row) => (
          <li key={row.key} className="relative flex items-center gap-3 text-[13px]">
            <span
              aria-hidden="true"
              className={`absolute inset-y-0 left-0 rounded-r ${row.bad ? 'bg-red-500/15' : 'bg-accent-500/10 dark:bg-accent-400/15'}`}
              style={{ width: `${(row.value / largest) * 100}%` }}
            />
            <span className={`relative min-w-0 flex-1 truncate px-2 py-1.5 ${row.mono ? 'font-mono text-[12px]' : ''}`}>
              <Named to={row.to}>{row.label}</Named>
            </span>
            {row.aside && <span className="relative shrink-0 text-xs tabular-nums text-slate-500">{row.aside}</span>}
            <span className="relative w-16 shrink-0 pr-1 text-right tabular-nums">{row.said ?? count(row.value)}</span>
          </li>
        ))}
      </ul>

      {rows.length > limit && (
        <button type="button" onClick={() => setAll((was) => !was)} className="mt-2 text-xs text-accent-600 hover:underline dark:text-accent-400">
          {all ? 'Show fewer' : `Show all ${rows.length}`}
        </button>
      )}
    </>
  );
}

/**
 * The ink for a label inside a fill: white on a dark one, near black on a
 * light one, by the fill's luminance - the same two in both themes.
 */
function ink(fill: string): string {
  const [r, g, b] = [1, 3, 5].map((at) => {
    const channel = parseInt(fill.slice(at, at + 2), 16) / 255;

    return channel <= 0.03928 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4;
  });

  return 0.2126 * r + 0.7152 * g + 0.0722 * b > 0.2 ? '#202124' : '#ffffff';
}

/** A name that leads somewhere where it has somewhere to lead. */
function Named({ to, children }: { to?: string; children: ReactNode }) {
  return to ? (
    <Link to={to} className="text-ink-900 hover:text-accent-600 hover:underline dark:text-slate-200 dark:hover:text-accent-400">{children}</Link>
  ) : (
    <>{children}</>
  );
}

/**
 * How much of a whole is used, as a bar that turns amber and then red as it
 * fills - for what runs out.
 */
export function Fill({ used, of, warn = 0.8, bad = 0.9, label }: { used: number; of: number; warn?: number; bad?: number; label: string }) {
  const part = of > 0 ? Math.min(1, used / of) : 0;
  const colour = part >= bad ? 'bg-red-500' : part >= warn ? 'bg-amber-500' : 'bg-accent-500 dark:bg-accent-400';

  return (
    <div className="h-1.5 w-full rounded-full bg-accent-500/15 dark:bg-accent-400/15" role="meter" aria-label={label} aria-valuemin={0} aria-valuemax={of} aria-valuenow={used}>
      <div className={`h-full rounded-full ${colour}`} style={{ width: `${Math.max(part > 0 ? 1 : 0, part * 100)}%` }} />
    </div>
  );
}
