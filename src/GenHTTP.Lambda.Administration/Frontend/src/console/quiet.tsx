import { useState } from 'react';

import type { TypicalDay } from '@genhttp-lambda/kit/api';
import { BLUE, GREY } from './charts';

/*
 * A typical day by the hour, as the quiet hours are worked out from: the
 * server counts in UTC, the operator reads their own clock. So the hours are
 * drawn from the operator's midnight on, each labelled with the time it
 * starts at there - half past where their zone is half an hour off.
 */

/** One hour of the day as the browser's clock has it. */
interface Slot {
  /** The hour of the day in UTC it is. */
  utc: number;
  /** Where it starts on the operator's clock, in minutes after their midnight. */
  start: number;
  requests: number;
  quiet: boolean;
}

/** The browser's offset from UTC, in minutes. */
const offset = () => -new Date().getTimezoneOffset();

/** A time of day, given in minutes after midnight, as 24 hour clock. */
const clockOf = (minutes: number) => {
  const wrapped = ((minutes % 1440) + 1440) % 1440;

  return `${String(Math.floor(wrapped / 60)).padStart(2, '0')}:${String(wrapped % 60).padStart(2, '0')}`;
};

/** The zone the times are shown in, such as UTC+1 or UTC+5:30. */
export function zone(): string {
  const minutes = offset();

  if (minutes === 0) return 'UTC';

  const sign = minutes > 0 ? '+' : '−';
  const whole = Math.abs(minutes);

  return `UTC${sign}${Math.floor(whole / 60)}${whole % 60 ? `:${String(whole % 60).padStart(2, '0')}` : ''}`;
}

/** Whether an hour of the day, in UTC, is in the quietest stretch. */
const isQuiet = (day: TypicalDay, utc: number) => (((utc - day.quietFrom) % 24) + 24) % 24 < day.quietLength;

function slotsOf(day: TypicalDay): Slot[] {
  return day.hours
    .map((requests, utc) => ({ utc, start: (((utc * 60 + offset()) % 1440) + 1440) % 1440, requests, quiet: day.known && isQuiet(day, utc) }))
    .sort((a, b) => a.start - b.start);
}

/** The quietest stretch on the operator's clock, such as 02:00–05:00. */
export function quietStretch(day: TypicalDay): string {
  const start = day.quietFrom * 60 + offset();

  return `${clockOf(start)}–${clockOf(start + day.quietLength * 60)}`;
}

/** Whether anything was counted at all. */
export const counted = (day: TypicalDay | null | undefined) => !!day && day.hours.some((h) => h > 0);

/** A typical count, which is an average: a decimal where it is small. */
const requestsOf = (value: number) =>
  value === 0 ? '0' : value < 10 ? value.toFixed(1) : Math.round(value).toLocaleString('en-US');

const PAD = { top: 10, right: 8, bottom: 22, left: 44 };

/**
 * The requests of a typical day as a column per hour, the quietest stretch
 * in the accent and the rest in grey - and named under the columns, so the
 * colour is not what says which they are. Each column says its figure when
 * pointed at, and the figures are a table one click away.
 *
 * Until enough was counted the stretch is not singled out: what the columns
 * show is as far as the hours were counted, and nothing to plan by yet.
 */
export function DayChart({ day, dark, label, height = 150 }: { day: TypicalDay; dark: boolean; label: string; height?: number }) {
  const [table, setTable] = useState(false);
  const [hover, setHover] = useState<number | null>(null);

  const slots = slotsOf(day);

  const width = 720;
  const plotW = width - PAD.left - PAD.right;
  const plotH = height - PAD.top - PAD.bottom;

  const max = niceCeiling(Math.max(...slots.map((s) => s.requests)));

  const band = plotW / slots.length;
  const thickness = Math.min(20, band - 6);

  const x = (i: number) => PAD.left + i * band + (band - thickness) / 2;
  const y = (value: number) => PAD.top + plotH - (value / max) * plotH;

  // the stretch, as runs of neighbouring columns: one, or two where it
  // wraps past the operator's midnight
  const runs: { from: number; to: number }[] = [];

  slots.forEach((slot, i) => {
    if (!slot.quiet) return;

    const last = runs[runs.length - 1];

    if (last && last.to === i - 1) {
      last.to = i;
    } else {
      runs.push({ from: i, to: i });
    }
  });

  const named = runs.reduce<{ from: number; to: number } | null>((best, run) => (!best || run.to - run.from > best.to - best.from ? run : best), null);

  const fill = (slot: Slot) => (day.known && !slot.quiet ? GREY : BLUE)[dark ? 1 : 0];

  const pointed = hover !== null ? slots[hover] : null;

  return (
    <div>
      <div className="relative">
        <svg viewBox={`0 0 ${width} ${height + (named ? 16 : 0)}`} className="block w-full" role="img"
             aria-label={`${label}, by the hour on your clock (${zone()})${day.known ? `; quietest ${quietStretch(day)}` : ''}`}
             onMouseLeave={() => setHover(null)}>
          {[0, max / 2, max].map((tick) => (
            <g key={tick}>
              <line x1={PAD.left} x2={width - PAD.right} y1={y(tick)} y2={y(tick)} className="stroke-grey-300 dark:stroke-ink-800" strokeWidth={1} />
              <text x={PAD.left - 8} y={y(tick)} dy="0.32em" textAnchor="end" className="fill-slate-500 text-[11px] tabular-nums">
                {requestsOf(tick)}
              </text>
            </g>
          ))}

          {slots.map((slot, i) => {
            const top = y(slot.requests);
            const tall = PAD.top + plotH - top;

            return (
              <g key={slot.utc}>
                {tall > 0 && <path d={column(x(i), top, thickness, tall)} fill={fill(slot)} opacity={hover === null || hover === i ? 1 : 0.55} />}

                {i % 3 === 0 && (
                  <text x={x(i) + thickness / 2} y={height - 6} textAnchor="middle" className="fill-slate-500 text-[11px] tabular-nums">
                    {clockOf(slot.start)}
                  </text>
                )}

                {/* the whole height of the hour is what is pointed at, not only its column */}
                <rect x={PAD.left + i * band} y={PAD.top} width={band} height={plotH} fill="transparent" onMouseEnter={() => setHover(i)} />
              </g>
            );
          })}

          {named && (
            <g className="fill-slate-600 dark:fill-slate-300">
              <line x1={x(named.from)} x2={x(named.to) + thickness} y1={height + 2} y2={height + 2} className="stroke-slate-400" strokeWidth={1} />
              <text x={(x(named.from) + x(named.to) + thickness) / 2} y={height + 13} textAnchor="middle" className="text-[11px]">
                Quietest
              </text>
            </g>
          )}
        </svg>

        {pointed && hover !== null && (
          <div
            className="pointer-events-none absolute top-0 z-10 -translate-x-1/2 whitespace-nowrap rounded border border-grey-300 bg-white px-2 py-1 text-xs shadow-sm dark:border-ink-800 dark:bg-ink-900"
            style={{ left: `${((x(hover) + thickness / 2) / width) * 100}%` }}
          >
            <span className="tabular-nums">{clockOf(pointed.start)}–{clockOf(pointed.start + 60)}</span>
            <span className="ml-2 font-medium tabular-nums">{requestsOf(pointed.requests)}</span>
            <span className="text-slate-500"> requests</span>
            {pointed.quiet && <span className="text-slate-500"> · quietest</span>}
          </div>
        )}
      </div>

      {table && (
        <table className="mt-3 w-full text-[13px]">
          <thead className="text-slate-500">
            <tr className="border-b border-grey-300 dark:border-ink-800">
              <th className="py-1.5 text-left font-normal">Hour ({zone()})</th>
              <th className="py-1.5 text-right font-normal">Requests on a typical day</th>
              <th className="w-24 py-1.5 text-right font-normal" />
            </tr>
          </thead>
          <tbody className="divide-y divide-grey-300 dark:divide-ink-800">
            {slots.map((slot) => (
              <tr key={slot.utc}>
                <td className="py-1.5 tabular-nums">{clockOf(slot.start)}–{clockOf(slot.start + 60)}</td>
                <td className="py-1.5 text-right tabular-nums">{requestsOf(slot.requests)}</td>
                <td className="py-1.5 text-right text-slate-500">{slot.quiet ? 'quietest' : ''}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <button type="button" onClick={() => setTable((was) => !was)} className="mt-2 text-xs text-accent-600 hover:underline dark:text-accent-400">
        {table ? 'Hide the table' : 'Show as a table'}
      </button>
    </div>
  );
}

/** A column grown from the baseline, its data end rounded. */
function column(left: number, top: number, thickness: number, tall: number): string {
  const r = Math.min(4, tall, thickness / 2);
  const bottom = top + tall;

  return `M${left},${bottom} V${top + r} Q${left},${top} ${left + r},${top} H${left + thickness - r} Q${left + thickness},${top} ${left + thickness},${top + r} V${bottom} Z`;
}

/** The top of the axis: a round figure at or above the largest. */
function niceCeiling(value: number): number {
  if (value <= 0) return 1;

  const step = 10 ** Math.floor(Math.log10(value));

  for (const multiple of [1, 2, 2.5, 5, 10]) {
    if (value <= multiple * step) return multiple * step;
  }

  return 10 * step;
}
