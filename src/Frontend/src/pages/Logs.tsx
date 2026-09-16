import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { useAdminToken } from '../admin';
import { ApiError, api, type LogEntry, type PreviousRun } from '../api';
import { IconPlay, IconStop, IconDownload, IconTrash, IconSpinner } from '../components/Icons';
import { Locked } from '../components/Locked';

/**
 * How often the tail is asked for while following.
 *
 * A cursor means each poll carries only what is new, so this is a small
 * request rather than the whole ring - and a second and a half is fast enough
 * to read as live without a socket to keep open.
 */
const EVERY = 1500;

/**
 * How many lines the page holds before it starts dropping its own oldest.
 *
 * The server's ring is bounded; a tab left open for a day is not, and a
 * hundred thousand rows in the DOM is a frozen tab.
 */
const HELD = 4000;

const LEVELS = [
  { value: 'trace', label: 'All' },
  { value: 'debug', label: 'Debug' },
  { value: 'info', label: 'Info' },
  { value: 'warn', label: 'Warnings' },
  { value: 'error', label: 'Errors' },
];

/** The tint each level is read by. Chosen against both surfaces, not by eye. */
const TINT: Record<string, string> = {
  trace: 'text-grey-500 dark:text-grey-600',
  debug: 'text-grey-500 dark:text-grey-600',
  info: 'text-sky-700 dark:text-sky-300',
  warn: 'text-amber-700 dark:text-amber-300',
  error: 'text-red-600 dark:text-red-400',
  critical: 'text-red-700 dark:text-red-300',
};

const SHORT: Record<string, string> = {
  trace: 'TRC',
  debug: 'DBG',
  info: 'INF',
  warn: 'WRN',
  error: 'ERR',
  critical: 'CRT',
};

/**
 * What the server and the lambdas on it are saying, as it happens.
 *
 * Polled rather than streamed: every line carries a sequence, so asking for
 * everything after the last one seen is a cheap request that survives a
 * reconnect, a sleeping laptop and a proxy that buffers - none of which a long
 * lived response does.
 */
export function Logs() {
  const [token] = useAdminToken();
  const [params, setParams] = useSearchParams();

  const [lines, setLines] = useState<LogEntry[]>([]);
  const [denied, setDenied] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [ready, setReady] = useState(false);
  const [following, setFollowing] = useState(true);
  const [missed, setMissed] = useState(0);
  const [capturing, setCapturing] = useState(true);
  const [capacity, setCapacity] = useState(0);
  const [previous, setPrevious] = useState<PreviousRun | null>(null);
  const [opened, setOpened] = useState<number | null>(null);
  const [find, setFind] = useState('');

  // the lambda and the level live in the address, so a link from the lambda
  // listing opens this already narrowed and the narrowing survives a reload
  const lambda = params.get('lambda') ?? '';
  const level = params.get('level') ?? 'info';

  const [typed, setTyped] = useState(lambda);

  /*
   * The cursor is a ref rather than state on purpose: the poll reads it and
   * writes it every time round, and doing that through state would make the
   * timer's callback stale, restart the timer on every tick, or both.
   */
  const cursor = useRef<number | null>(null);
  const view = useRef<HTMLDivElement>(null);
  const pinned = useRef(true);

  const load = useCallback(async () => {
    if (token === '') {
      return;
    }

    try {
      const page = await api.logs(token, {
        since: cursor.current ?? undefined,
        lambda: lambda === '' ? undefined : lambda,
        level,
      });

      cursor.current = page.cursor;

      setCapturing(page.capturing);
      setCapacity(page.capacity);
      setPrevious(page.previous ?? null);
      setDenied(false);
      setError(null);
      setReady(true);

      if (page.missed > 0) {
        setMissed((was) => was + page.missed);
      }

      if (page.lines.length > 0) {
        setLines((was) => {
          const all = was.concat(page.lines);

          return all.length > HELD ? all.slice(all.length - HELD) : all;
        });
      }
    } catch (problem) {
      if (problem instanceof ApiError && problem.status === 404) {
        setDenied(true);
        setLines([]);
      } else {
        setError('The log could not be read.');
      }
    }
  }, [token, lambda, level]);

  // a change of filter is a different question, so the answer starts over
  // rather than appending lines from one query onto lines from another
  useEffect(() => {
    cursor.current = null;
    pinned.current = true;
    setLines([]);
    setMissed(0);
    setReady(false);
    setOpened(null);
  }, [lambda, level]);

  useEffect(() => {
    load();

    if (!following) {
      return;
    }

    const timer = window.setInterval(load, EVERY);

    return () => window.clearInterval(timer);
  }, [load, following]);

  // following means the newest line is the one on screen; it stops as soon as
  // somebody scrolls up to read something, and resumes when they come back
  useEffect(() => {
    const box = view.current;

    if (box == null || !pinned.current) {
      return;
    }

    box.scrollTop = box.scrollHeight;
  }, [lines]);

  const shown = useMemo(() => {
    const needle = find.trim().toLowerCase();

    if (needle === '') {
      return lines;
    }

    return lines.filter(
      (l) =>
        l.text.toLowerCase().includes(needle) ||
        l.source.toLowerCase().includes(needle) ||
        (l.lambda ?? '').toLowerCase().includes(needle),
    );
  }, [lines, find]);

  function narrow(to: string) {
    const next = new URLSearchParams(params);

    if (to === '') {
      next.delete('lambda');
    } else {
      next.set('lambda', to);
    }

    setParams(next, { replace: true });
  }

  function choose(to: string) {
    const next = new URLSearchParams(params);

    next.set('level', to);
    setParams(next, { replace: true });
  }

  function save() {
    const text = shown.map((l) => `${l.at} ${l.level.toUpperCase()} ${l.source}${l.lambda ? ` [${l.lambda}]` : ''} ${l.text}`).join('\n');

    const url = URL.createObjectURL(new Blob([text], { type: 'text/plain' }));

    const link = document.createElement('a');

    link.href = url;
    link.download = `lambda-log-${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '-')}.txt`;
    link.click();

    URL.revokeObjectURL(url);
  }

  if (token === '' || denied) {
    return (
      <Locked
        title="Log"
        denied={denied}
        what="Whatever this server and the code running on it decided to print, which can be anything either of them saw."
      />
    );
  }

  return (
    <div className="mx-auto w-full max-w-6xl px-5 py-10">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Log</h1>
          <p className="mt-1.5 text-sm text-slate-600 dark:text-slate-400">
            The last {capacity.toLocaleString()} lines of this run, held in memory and lost on a restart.
            {capturing
              ? ' What a lambda prints while it is serving a request is filed under it.'
              : ' What lambdas print is not being kept on this installation.'}
          </p>
        </div>

        <div className="flex items-center gap-1">
          <button
            type="button"
            onClick={() => setFollowing((was) => !was)}
            className="btn-ghost"
            title={following ? 'Stop following the tail' : 'Follow the tail again'}
          >
            {following ? <IconStop className="h-4 w-4" /> : <IconPlay className="h-4 w-4" />}
            {following ? 'Following' : 'Paused'}
          </button>

          <button type="button" onClick={save} className="btn-ghost" disabled={shown.length === 0} title="Save what is on screen">
            <IconDownload className="h-4 w-4" />
            Save
          </button>

          <button
            type="button"
            onClick={() => {
              setLines([]);
              setMissed(0);
            }}
            className="btn-ghost"
            title="Empty the view. The server keeps its own ring either way."
          >
            <IconTrash className="h-4 w-4" />
            Clear
          </button>
        </div>
      </div>

      <div className="mt-6 flex flex-wrap items-center gap-2">
        <div className="flex border border-grey-300 dark:border-ink-800" role="group" aria-label="Level">
          {LEVELS.map((choice) => (
            <button
              key={choice.value}
              type="button"
              onClick={() => choose(choice.value)}
              className={`px-3 py-1.5 text-sm ${
                choice.value === level
                  ? 'bg-accent-500 text-white dark:bg-accent-400 dark:text-ink-950'
                  : 'text-slate-600 hover:bg-slate-50 dark:text-slate-400 dark:hover:bg-ink-850'
              }`}
            >
              {choice.label}
            </button>
          ))}
        </div>

        <form
          onSubmit={(event) => {
            event.preventDefault();
            narrow(typed.trim());
          }}
          className="flex items-center gap-2"
        >
          <input
            value={typed}
            onChange={(event) => setTyped(event.target.value)}
            placeholder="A lambda's public key"
            aria-label="Only this lambda"
            className="field w-56 font-mono text-sm"
          />
          <button type="submit" className="btn-ghost px-3">
            Only this
          </button>
        </form>

        {lambda !== '' && (
          <button
            type="button"
            onClick={() => {
              setTyped('');
              narrow('');
            }}
            className="chip border border-accent-500/40 text-accent-500 dark:border-accent-400/40 dark:text-accent-400"
            title="Show everything again"
          >
            {lambda} ✕
          </button>
        )}

        <input
          value={find}
          onChange={(event) => setFind(event.target.value)}
          placeholder="Find in what is loaded"
          aria-label="Find"
          className="field ml-auto w-56 text-sm"
        />
      </div>

      {previous !== null && (
        <div className="mt-4 border border-amber-500/40 bg-amber-500/5 px-4 py-3 text-sm">
          <p className="font-medium text-amber-700 dark:text-amber-300">The run before this one did not stop, it ended.</p>
          <p className="mt-1 text-slate-600 dark:text-slate-400">
            It had been up {Math.round(previous.minutes).toLocaleString()} minutes and was last seen at{' '}
            {new Date(previous.lastSeen).toLocaleString()} holding{' '}
            {Math.round(previous.workingSet / 1024 / 1024).toLocaleString()} MB with{' '}
            {previous.sockets.toLocaleString()} socket{previous.sockets === 1 ? '' : 's'} open, after{' '}
            {previous.requests.toLocaleString()} request{previous.requests === 1 ? '' : 's'}.{' '}
            {previous.signalled
              ? 'It had been asked to stop and did not finish doing so.'
              : 'Nothing asked it to stop.'}
          </p>
          {previous.fault != null && (
            <p className="mt-1 font-mono text-xs text-red-600 dark:text-red-400">{previous.fault}</p>
          )}
          <p className="mt-1 text-xs text-slate-500">
            Nothing above this point is from that run — the log is held in memory. The container’s stdout has it.
          </p>
        </div>
      )}

      {missed > 0 && (
        <p className="mt-3 text-xs text-amber-700 dark:text-amber-300">
          {missed.toLocaleString()} {missed === 1 ? 'line' : 'lines'} went past before this page read them — the server
          holds {capacity.toLocaleString()} and no more.
        </p>
      )}

      {error !== null && <p className="mt-3 text-sm text-red-500">{error}</p>}

      <div
        ref={view}
        onScroll={(event) => {
          const box = event.currentTarget;

          // a few pixels of slack: a list that is one subpixel short of the
          // bottom is, to the person reading it, at the bottom
          pinned.current = box.scrollHeight - box.scrollTop - box.clientHeight < 24;
        }}
        className="surface mt-3 h-[60vh] overflow-auto font-mono text-xs leading-relaxed"
      >
        {!ready ? (
          <div className="flex items-center gap-2 p-4 text-sm text-slate-500">
            <IconSpinner /> Reading the log…
          </div>
        ) : shown.length === 0 ? (
          <div className="p-4 text-sm text-slate-500">
            {lines.length === 0
              ? 'Nothing has been said at this level yet.'
              : 'Nothing loaded matches that.'}
          </div>
        ) : (
          shown.map((line) => (
            <div
              key={line.seq}
              className="border-b border-grey-300/40 px-3 py-1 last:border-0 hover:bg-slate-50 dark:border-ink-800/60 dark:hover:bg-ink-850"
            >
              <button
                type="button"
                onClick={() => setOpened(opened === line.seq ? null : line.seq)}
                disabled={line.detail == null}
                className="flex w-full items-baseline gap-2 text-left disabled:cursor-default"
              >
                <span className="shrink-0 text-grey-500 dark:text-grey-600">{clock(line.at)}</span>

                <span className={`w-8 shrink-0 font-semibold ${TINT[line.level] ?? ''}`}>
                  {SHORT[line.level] ?? line.level}
                </span>

                {line.lambda != null && (
                  <span
                    onClick={(event) => {
                      event.stopPropagation();
                      setTyped(line.lambda!);
                      narrow(line.lambda!);
                    }}
                    role="link"
                    tabIndex={0}
                    onKeyDown={(event) => {
                      if (event.key === 'Enter') {
                        setTyped(line.lambda!);
                        narrow(line.lambda!);
                      }
                    }}
                    className="shrink-0 cursor-pointer text-accent-500 hover:underline dark:text-accent-400"
                    title="Only this lambda"
                  >
                    {line.lambda}
                  </span>
                )}

                <span className="shrink-0 text-grey-500 dark:text-grey-600">{line.source}</span>

                <span className="whitespace-pre-wrap break-all text-slate-800 dark:text-grey-200">{line.text}</span>

                {line.detail != null && (
                  <span className="ml-auto shrink-0 text-grey-500 dark:text-grey-600">
                    {opened === line.seq ? '▾' : '▸'}
                  </span>
                )}
              </button>

              {opened === line.seq && line.detail != null && (
                <pre className="mt-1 overflow-x-auto whitespace-pre bg-slate-50 p-2 text-[11px] text-slate-700 dark:bg-ink-950 dark:text-grey-400">
                  {line.detail}
                </pre>
              )}
            </div>
          ))
        )}
      </div>

      <p className="mt-2 text-xs text-slate-500">
        {shown.length.toLocaleString()} of {lines.length.toLocaleString()} loaded
        {following ? ' · following' : ' · paused'}
        {' · every line is also on the container’s stdout, which is what survives a restart'}
      </p>
    </div>
  );
}

/** The time of day, which is what a log is read by. */
function clock(at: string): string {
  const when = new Date(at);

  if (Number.isNaN(when.getTime())) {
    return '--:--:--';
  }

  return when.toLocaleTimeString([], { hour12: false });
}
