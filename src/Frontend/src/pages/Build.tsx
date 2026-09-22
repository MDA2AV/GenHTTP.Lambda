import { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';

import { api, ApiError } from '../api';
import { CopyField } from '../components/CopyField';

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
  changed?: boolean;
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
  const [origin, setOrigin] = useState('');
  const [state, setState] = useState<'idle' | 'working' | 'done' | 'failed'>('idle');
  const [events, setEvents] = useState<string[]>([]);
  const [result, setResult] = useState<Result | null>(null);
  const [available, setAvailable] = useState<boolean | null>(null);
  const [secondModel, setSecondModel] = useState(false);
  const [model, setModel] = useState('opus');
  const [password, setPassword] = useState('');
  const [copied, setCopied] = useState(false);

  const polling = useRef<number | null>(null);

  useEffect(() => {
    setOrigin(window.location.origin);

    api.build
      .available()
      .then((r) => {
        setAvailable(r.available);
        setSecondModel(r.secondModel);
      })
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
      id = (
        await api.build.start(
          prompt.trim(),
          // this form only ever creates. Changing something that exists is the
          // editor's job, or an agent's over MCP.
          undefined,
          model === 'opus' ? undefined : model,
          model === 'opus' ? undefined : password,
        )
      ).id;
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
        It gets built, put online, and you get a link you can send to anyone. No account, no install, and it can remember things - scores, messages, entries - so everybody who opens it sees the same thing.
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
            {state === 'working' ? 'working…' : 'ctrl + enter'}
          </span>

          <button
            type="button"
            onClick={start}
            disabled={
              state === 'working' ||
              prompt.trim().length < 3 ||
              (model === 'fable' && password.length < 1)
            }
            className="btn btn-primary"
          >
            {state === 'working' ? 'Building' : 'Build it'}
          </button>
        </div>
      </div>

      {state === 'idle' && secondModel && (
        <div className="mt-4 flex flex-wrap items-center justify-center gap-2 text-sm">
          <span className="text-slate-400">Built by</span>

          {[
            ['opus', 'Opus 5'],
            ['fable', 'Fable 5.1'],
          ].map(([id, label]) => (
            <button
              key={id}
              type="button"
              onClick={() => setModel(id)}
              className={
                model === id
                  ? 'chip !border-sky-500/60 !text-sky-600 dark:!text-sky-400'
                  : 'chip hover:opacity-80'
              }
            >
              {label}
            </button>
          ))}

          {model === 'fable' && (
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="password"
              autoComplete="off"
              className="field !h-auto !w-36 !py-1 text-sm"
            />
          )}
        </div>
      )}

      {state === 'idle' && model === 'fable' && (
        <p className="mt-2 text-center text-xs text-slate-400">
          Fable is behind a password while it is being tried out. It runs with no time limit,
          so it will keep going until the thing is finished rather than until the clock runs out.
        </p>
      )}

      {state === 'idle' && (
        <p className="mt-4 text-center text-sm text-slate-500">
          This builds a new one from what you write here. To change something you have already made,
          open its editor link.
        </p>
      )}

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
                {result.changed ? 'Changed, and back online' : 'Your app'}
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
            put it back up.
          </p>

          <div className="flex flex-wrap gap-3">
            <a
              href={result.editorUrl}
              target="_blank"
              rel="noreferrer"
              className="btn btn-primary"
            >
              Open the editor to change it
            </a>

            <button
              type="button"
              className="btn btn-ghost"
              onClick={() => { setState('idle'); setPrompt(''); setResult(null); }}
            >
              Build something else
            </button>
          </div>
        </section>
      )}

      {state === 'idle' && (
        <section className="mt-16 border-t border-slate-200 pt-10 dark:border-slate-800">
          <h2 className="text-xl font-light tracking-tight">Or use your own agent</h2>

          <p className="mt-3 max-w-xl text-sm text-slate-500">
            The box above is a Claude running on this machine. If you already have one of your own,
            point it here instead and it can do the same things - make a lambda, write the code,
            put it online - without a daily limit and without going through this page.
          </p>

          <div className="mt-5">
            <CopyField value={`${origin}/mcp`} tone="accent" />
          </div>

          <div className="mt-5 grid gap-4 sm:grid-cols-2">
            <div className="surface p-4">
              <h3 className="text-sm font-medium">Claude Code</h3>
              <pre className="mt-2 whitespace-pre-wrap break-all rounded-md bg-slate-900 p-3 text-xs text-slate-100 dark:bg-black/40">
{`claude mcp add --transport http genhttp ${origin}/mcp`}
              </pre>
              <p className="mt-2 text-xs text-slate-500">
                Then ask it for what you want, the same way you would here.
              </p>
            </div>

            <div className="surface p-4">
              <h3 className="text-sm font-medium">Claude on the web</h3>
              <p className="mt-2 text-xs leading-relaxed text-slate-500">
                Settings, then Connectors, then Add custom connector. Paste the address above as
                the remote MCP server URL. There is no key and no sign in step.
              </p>
            </div>
          </div>

          <p className="mt-4 text-sm text-slate-500">
            It works the same way for changing something: give your agent the editor link and tell
            it what to do.{' '}
            <Link to="/agentic-coding" className="underline">More about using an agent here</Link>.
          </p>
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
