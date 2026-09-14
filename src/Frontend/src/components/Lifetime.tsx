import { useState } from 'react';

import type { Lambda } from '../api';

/**
 * The two timers a free lambda runs on, shown before they expire rather than
 * explained afterwards.
 *
 * Both are read from the lambda rather than recomputed here, so what is shown
 * is what the maintenance job will actually act on.
 */
export function Lifetime({ lambda }: { lambda: Lambda }) {
  const [open, setOpen] = useState(false);

  const days = remaining(lambda.keptUntil, DAY);
  const deployHours = lambda.deployedUntil ? remaining(lambda.deployedUntil, HOUR) : null;

  // green while there is plenty of room, amber once it is worth noticing
  const tone =
    days > 7
      ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
      : 'bg-amber-500/10 text-amber-600 dark:text-amber-400';

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen(true)}
        className={`chip ${tone} hover:underline`}
        title="How long this lambda is kept"
      >
        {days}d
      </button>

      {open && (
        <div
          className="fixed inset-0 z-40 flex items-center justify-center bg-black/40 p-4"
          onClick={() => setOpen(false)}
          role="presentation"
        >
          <div
            className="surface w-full max-w-md p-5 shadow-xl"
            onClick={(event) => event.stopPropagation()}
            role="dialog"
            aria-modal="true"
            aria-label="How long this lambda is kept"
          >
            <h2 className="text-base font-semibold">Everything here is free, so nothing here is forever</h2>

            <p className="mt-2 text-sm leading-relaxed text-slate-600 dark:text-slate-400">
              Two timers keep the shared machine tidy. Both reset when you use the lambda, so anything you
              are actually working on stays.
            </p>

            <dl className="mt-4 space-y-3 text-sm">
              <div className="border-l-2 border-accent-500 pl-3">
                <dt className="font-medium">
                  {deployHours === null
                    ? 'Nothing is online right now'
                    : deployHours >= 23
                      ? 'Online for about a day more'
                      : `Online for about ${format(deployHours, 'hour')} more`}
                </dt>
                <dd className="mt-0.5 text-slate-600 dark:text-slate-400">
                  {deployHours === null
                    ? 'Once you deploy, it stays reachable for a day.'
                    : 'A deployment runs for a day. Press Deploy again whenever you like and it starts over.'}
                </dd>
              </div>

              <div className="border-l-2 border-accent-500 pl-3">
                <dt className="font-medium">Kept for another {format(days, 'day')}</dt>
                <dd className="mt-0.5 text-slate-600 dark:text-slate-400">
                  A lambda nobody has touched for a month is removed with its versions. Saving or deploying
                  counts as touching it, so the month starts again.
                </dd>
              </div>
            </dl>

            <p className="mt-4 text-xs text-slate-500">
              Removed on {new Date(lambda.keptUntil).toLocaleDateString()} if nothing changes before then.
            </p>

            <div className="mt-5 flex justify-end">
              <button type="button" onClick={() => setOpen(false)} className="btn-ghost">
                Got it
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

const HOUR = 3_600_000;
const DAY = 24 * HOUR;

/**
 * Units left, never below zero - a lambda past its date still reads as 0.
 *
 * Rounded up rather than down, because down reads as expired while there is
 * still most of a unit to go: a deployment made a minute ago has 23 hours and
 * 59 minutes left, which is not "0 hours", and the day it is measured in is
 * not over until it is over.
 */
function remaining(until: string, unit: number): number {
  const ms = new Date(until).getTime() - Date.now();

  return Math.max(0, Math.ceil(ms / unit));
}

const format = (value: number, unit: string) => `${value} ${unit}${value === 1 ? '' : 's'}`;
