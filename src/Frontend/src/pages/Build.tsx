import { useEffect, useRef, useState } from 'react';

import { api, ApiError } from '../api';
import { ConnectAgent } from '../components/ConnectAgent';
import { useT } from '../i18n';
import { usePublicPage } from '../meta';
import { useLifetimes, useOrigin } from '../site';

/**
 * One text box.
 *
 * This is the whole page on purpose. Everything else this site offers - the
 * editor, the files, the versions - is for somebody who wants to write the
 * code. This is for somebody who wants the thing to exist, so there is one
 * field, one button, and afterwards two links: where it is, and the editor
 * link to take it further with.
 *
 * Where the operator switched the box off, or there is no agent to run it,
 * the page is what it says about itself and how to get the same from an
 * AI assistant of one's own.
 *
 * It only ever makes new things. Changing one afterwards happens in the
 * Change section of the editor, which already holds the key and can show the
 * change as what it is - a new version of something that works - or with the
 * person's own agent over MCP. The box used to take a pasted editor link and
 * do both, and it never did the second well.
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

export function Build() {
  usePublicPage('/build');

  const t = useT();
  const said = t.build;
  const { offlineDays, retentionDays } = useLifetimes();

  const [prompt, setPrompt] = useState('');
  const { origin } = useOrigin();
  const [state, setState] = useState<'idle' | 'working' | 'done' | 'failed'>('idle');
  const [events, setEvents] = useState<string[]>([]);
  const [waiting, setWaiting] = useState(0);
  const [result, setResult] = useState<Result | null>(null);
  const [available, setAvailable] = useState<boolean | null>(null);
  const [secondModel, setSecondModel] = useState(false);
  const [model, setModel] = useState('opus');
  const [password, setPassword] = useState('');
  const [copied, setCopied] = useState(false);

  const polling = useRef<number | null>(null);

  // offered until the server says otherwise, so the box is in the page as
  // it is prerendered for crawlers and does not pop in for everybody else
  const offered = available !== false;

  useEffect(() => {
    api
      .platform()
      .then(({ build }) => {
        setAvailable(build.available);
        setSecondModel(build.secondModel);
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
    setWaiting(0);
    setResult(null);

    let id: string;

    try {
      id = (
        await api.builds.start(
          prompt.trim(),
          model === 'opus' ? undefined : model,
          model === 'opus' ? undefined : password,
        )
      ).id;
    } catch (e) {
      setState('failed');
      setResult({ ok: false, error: e instanceof ApiError ? e.message : said.failedToStart });
      return;
    }

    const poll = async () => {
      try {
        const job = await api.builds.progress(id);

        setEvents(job.events ?? []);
        setWaiting(job.waiting ?? 0);

        // cancelled is never asked for from here, but the agent knows the
        // state, and a page that polled for ever on it would be worse
        if (job.state === 'done' || job.state === 'failed' || job.state === 'cancelled') {
          if (polling.current) window.clearInterval(polling.current);
          setResult(job.result ?? { ok: false, error: said.noAnswer });
          setState(job.state === 'done' && job.result?.ok ? 'done' : 'failed');
        }
      } catch {
        /* a poll that fails is a poll; the next one will do */
      }
    };

    // asked once straight away, so a queue shows up without a wait of its own
    void poll();
    polling.current = window.setInterval(poll, 2500);
  }

  return (
    <main className="mx-auto w-full max-w-2xl px-6 py-16 sm:py-24">
      <h1 className="text-center text-4xl font-light tracking-tight sm:text-5xl">
        {said.title}
      </h1>

      <p className="mx-auto mt-4 max-w-lg text-center text-slate-500">{said.intro}</p>

      {offered && (
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
            placeholder={said.placeholder}
            className="w-full resize-none bg-transparent px-4 py-3 text-lg outline-none placeholder:text-slate-400 disabled:opacity-60"
          />

          <div className="flex items-center justify-between gap-3 px-2 pb-1">
            <span className="text-xs text-slate-400">
              {state === 'working' ? said.working : said.shortcut}
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
              {state === 'working' ? said.building : said.buildIt}
            </button>
          </div>
        </div>
      )}

      {offered && state === 'idle' && secondModel && (
        <div className="mt-4 flex flex-wrap items-center justify-center gap-2 text-sm">
          <span className="text-slate-400">{said.builtBy}</span>

          {[
            ['opus', 'Opus 5.5'],
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
              placeholder={said.password}
              autoComplete="off"
              className="field !h-auto !w-36 !py-1 text-sm"
            />
          )}
        </div>
      )}

      {offered && state === 'idle' && model === 'fable' && (
        <p className="mt-2 text-center text-xs text-slate-400">{said.fable}</p>
      )}

      {offered && state === 'idle' && (
        <p className="mt-4 text-center text-sm text-slate-500">{said.onlyNew}</p>
      )}

      {offered && state === 'idle' && (
        <div className="mt-6 flex flex-wrap justify-center gap-2">
          {said.ideas.map((idea) => (
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

      {state === 'idle' && (
        <ul className="mt-12 grid gap-4 sm:grid-cols-3">
          {said.points.map((point) => (
            <li key={point.title} className="surface p-4">
              <h2 className="text-sm font-medium">{point.title}</h2>
              <p className="mt-1.5 text-sm leading-relaxed text-slate-500">{point.text}</p>
            </li>
          ))}
        </ul>
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
          {events.length === 0 && waiting > 0 && (
            <li className="flex items-center gap-3 px-5 py-2.5 text-slate-500">
              <span className="h-1.5 w-1.5 animate-pulse rounded-full bg-amber-500" />
              {/* one build runs at a time, so everyone else waits their turn */}
              {said.ahead(waiting)}
            </li>
          )}
          {events.length === 0 && waiting === 0 && (
            <li className="px-5 py-2.5 text-slate-500">{said.starting}</li>
          )}
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
              <span className="block text-xs uppercase tracking-widest text-slate-400">{said.yourApp}</span>
              <span className="block truncate font-medium">{result.url}</span>
            </span>
            <span aria-hidden>→</span>
          </a>

          <div className="surface px-5 py-4">
            <span className="block text-xs uppercase tracking-widest text-slate-400">{said.further}</span>

            <a
              href={result.editorUrl}
              target="_blank"
              rel="noreferrer"
              className="mt-1 block truncate font-medium underline"
            >
              {result.editorUrl}
            </a>

            <p className="mt-3 text-sm text-slate-500">{said.keep}</p>

            <p className="mt-3 text-sm text-slate-500">{said.change}</p>

            <button
              type="button"
              className="btn btn-ghost mt-3"
              onClick={() => {
                navigator.clipboard?.writeText(result.editorUrl ?? '');
                setCopied(true);
                window.setTimeout(() => setCopied(false), 1600);
              }}
            >
              {copied ? t.common.copied : said.copyLink}
            </button>
          </div>

          <p className="text-sm text-slate-500">{said.lifetime(offlineDays, retentionDays)}</p>

          <div className="flex flex-wrap gap-3">
            <a
              href={result.editorUrl}
              target="_blank"
              rel="noreferrer"
              className="btn btn-primary"
            >
              {said.openEditor}
            </a>

            <button
              type="button"
              className="btn btn-ghost"
              onClick={() => { setState('idle'); setPrompt(''); setResult(null); }}
            >
              {said.another}
            </button>
          </div>
        </section>
      )}

      {/* folded away, so the box stays the page - but there for whoever wants to know first */}
      {state === 'idle' && (
        <section className="mt-16" aria-labelledby="questions">
          <h2 id="questions" className="text-xl font-light tracking-tight">
            {said.questionsTitle}
          </h2>

          <div className="mt-4 divide-y divide-slate-200 border-t border-slate-200 dark:divide-slate-800 dark:border-slate-800">
            {said.questions(offlineDays, retentionDays).map(([question, answer]) => (
              <details key={question} className="group">
                <summary className="flex min-h-[3rem] cursor-pointer list-none items-center justify-between gap-4 py-3 text-sm font-medium [&::-webkit-details-marker]:hidden">
                  {question}
                  <span aria-hidden="true" className="text-lg leading-none text-slate-400 transition-transform group-open:rotate-45">+</span>
                </summary>
                <p className="pb-4 pr-8 text-sm leading-relaxed text-slate-500">{answer}</p>
              </details>
            ))}
          </div>
        </section>
      )}

      {(state === 'idle' || (state === 'done' && result?.ok)) && (
        <section className="mt-16 border-t border-slate-200 pt-10 dark:border-slate-800">
          <h2 className="text-xl font-light tracking-tight">
            {!offered ? said.ownTitle : state === 'done' ? said.keepGoing : said.orOwn}
          </h2>

          <p className="mb-5 mt-3 max-w-xl text-sm text-slate-500">{offered ? said.ownText : said.ownOnly}</p>

          <ConnectAgent origin={origin} />

          <p className="mt-5 text-sm text-slate-500">{said.thenAsk}</p>

          <p className="mt-2 text-sm text-slate-500">{said.howToChange}</p>
        </section>
      )}

      {state === 'failed' && (
        <section className="surface mt-8 px-5 py-4">
          <p className="font-medium">{result?.error ?? said.failed}</p>
          {result?.detail && <p className="mt-2 text-sm text-slate-500">{result.detail}</p>}
          <button
            type="button"
            className="btn btn-ghost mt-4"
            onClick={() => setState('idle')}
          >
            {t.common.tryAgain}
          </button>
        </section>
      )}
    </main>
  );
}
