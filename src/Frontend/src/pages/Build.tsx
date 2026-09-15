import { useEffect, useRef, useState } from 'react';

import { api, ApiError } from '../api';

/**
 * One text box.
 *
 * This is the whole page on purpose. Everything else this site offers - the
 * editor, the files, the versions - is for somebody who wants to write the
 * code. This is for somebody who wants the thing to exist, so there is one
 * field, one button, and afterwards two links: where it is, and where to go
 * to change it.
 */

type Result = {
  ok: boolean;
  url?: string;
  editorUrl?: string;
  publicKey?: string;
  privateKey?: string;
  summary?: string;
  error?: string;
  detail?: string;
  deployed?: boolean;
};

const IDEAS = [
  'a wall where anyone can leave a one line message',
  'a highscore board for a dice game',
  'a poll where people vote and see the totals',
  'a guestbook for my wedding',
  'a countdown to a date everyone can see',
];

export function Build() {
  const [prompt, setPrompt] = useState('');
  const [state, setState] = useState<'idle' | 'working' | 'done' | 'failed'>('idle');
  const [events, setEvents] = useState<string[]>([]);
  const [result, setResult] = useState<Result | null>(null);
  const [available, setAvailable] = useState<boolean | null>(null);
  const [copied, setCopied] = useState(false);

  const polling = useRef<number | null>(null);

  useEffect(() => {
    api.build
      .available()
      .then((r) => setAvailable(r.available))
      .catch(() => setAvailable(false));

    return () => {
      if (polling.current) window.clearInterval(polling.current);
    };
  }, []);

  async function start() {
    if (prompt.trim().length < 3 || state === 'working') return;

    setState('working');
    setEvents([]);
    setResult(null);

    let id: string;

    try {
      id = (await api.build.start(prompt.trim())).id;
    } catch (e) {
      setState('failed');
      setResult({ ok: false, error: e instanceof ApiError ? e.message : 'That did not go through.' });
      return;
    }

    polling.current = window.setInterval(async () => {
      try {
        const job = await api.build.progress(id);

        setEvents(job.events ?? []);

        if (job.state === 'done' || job.state === 'failed') {
          if (polling.current) window.clearInterval(polling.current);
          setResult(job.result ?? { ok: false, error: 'It finished without saying what happened.' });
          setState(job.state === 'done' && job.result?.ok ? 'done' : 'failed');
        }
      } catch {
        /* a poll that fails is a poll; the next one will do */
      }
    }, 2500);
  }

  if (available === false) {
    return (
      <main className="mx-auto max-w-2xl px-6 py-24 text-center">
        <h1 className="text-3xl font-light tracking-tight">Not switched on here</h1>
        <p className="mt-4 text-slate-500">
          This installation has no build agent. You can still{' '}
          <a className="underline" href="/editor/create">write it yourself</a>, or point your own
          Claude at <code>/mcp</code>.
        </p>
      </main>
    );
  }

  return (
    <main className="mx-auto w-full max-w-2xl px-6 py-16 sm:py-24">
      <h1 className="text-center text-4xl font-light tracking-tight sm:text-5xl">
        Say what you want.
      </h1>

      <p className="mx-auto mt-4 max-w-lg text-center text-slate-500">
        It gets built, put online, and you get a link you can send to anyone. No account, no
        install, and it can remember things - scores, messages, entries - so everybody who opens it
        sees the same thing.
      </p>

      <div className="surface mt-10 p-2">
        <textarea
          value={prompt}
          onChange={(e) => setPrompt(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter' && (e.metaKey || e.ctrlKey)) start();
          }}
          rows={3}
          maxLength={2000}
          disabled={state === 'working'}
          placeholder="build a…"
          className="w-full resize-none bg-transparent px-4 py-3 text-lg outline-none placeholder:text-slate-400 disabled:opacity-60"
        />

        <div className="flex items-center justify-between gap-3 px-2 pb-1">
          <span className="text-xs text-slate-400">
            {state === 'working' ? 'building…' : 'ctrl + enter'}
          </span>

          <button
            type="button"
            onClick={start}
            disabled={state === 'working' || prompt.trim().length < 3}
            className="btn btn-primary"
          >
            {state === 'working' ? 'Building' : 'Build it'}
          </button>
        </div>
      </div>

      {state === 'idle' && (
        <div className="mt-6 flex flex-wrap justify-center gap-2">
          {IDEAS.map((idea) => (
            <button
              key={idea}
              type="button"
              onClick={() => setPrompt(idea)}
              className="chip hover:opacity-80"
            >
              {idea}
            </button>
          ))}
        </div>
      )}

      {state === 'working' && (
        <ol className="surface mt-6 divide-y divide-slate-200/60 text-sm dark:divide-slate-700/60">
          {events.map((event, i) => (
            <li key={i} className="flex items-center gap-3 px-5 py-2.5">
              <span
                className={
                  i === events.length - 1
                    ? 'h-1.5 w-1.5 animate-pulse rounded-full bg-sky-500'
                    : 'h-1.5 w-1.5 rounded-full bg-slate-300 dark:bg-slate-600'
                }
              />
              {event}
            </li>
          ))}
          {events.length === 0 && <li className="px-5 py-2.5 text-slate-500">Starting…</li>}
        </ol>
      )}

      {state === 'done' && result?.ok && (
        <section className="mt-8 space-y-4">
          {result.summary && (
            <p className="text-slate-600 dark:text-slate-300">{result.summary.split('\n')[0]}</p>
          )}

          <a
            href={result.url}
            target="_blank"
            rel="noreferrer"
            className="surface flex items-center justify-between gap-4 px-5 py-4 transition hover:opacity-80"
          >
            <span>
              <span className="block text-xs uppercase tracking-widest text-slate-400">
                Your app
              </span>
              <span className="block truncate font-medium">{result.url}</span>
            </span>
            <span aria-hidden>→</span>
          </a>

          <div className="surface px-5 py-4">
            <span className="block text-xs uppercase tracking-widest text-slate-400">
              To change it later
            </span>

            <a
              href={result.editorUrl}
              target="_blank"
              rel="noreferrer"
              className="mt-1 block truncate font-medium underline"
            >
              {result.editorUrl}
            </a>

            <p className="mt-3 text-sm text-slate-500">
              Keep that one. It is the only way back in and it cannot be recovered - not by us
              either. Bookmark it before you close this tab.
            </p>

            <button
              type="button"
              className="btn btn-ghost mt-3"
              onClick={() => {
                navigator.clipboard?.writeText(result.editorUrl ?? '');
                setCopied(true);
                window.setTimeout(() => setCopied(false), 1600);
              }}
            >
              {copied ? 'Copied' : 'Copy the editor link'}
            </button>
          </div>

          <p className="text-sm text-slate-500">
            It stays online for a day and is kept for thirty. Open the editor and press deploy to
            put it back up, or ask for something else below.
          </p>

          <button type="button" className="btn btn-ghost" onClick={() => { setState('idle'); setPrompt(''); }}>
            Build something else
          </button>
        </section>
      )}

      {state === 'failed' && (
        <section className="surface mt-8 px-5 py-4">
          <p className="font-medium">{result?.error ?? 'That did not work.'}</p>
          {result?.detail && <p className="mt-2 text-sm text-slate-500">{result.detail}</p>}
          <button
            type="button"
            className="btn btn-ghost mt-4"
            onClick={() => setState('idle')}
          >
            Try again
          </button>
        </section>
      )}
    </main>
  );
}
