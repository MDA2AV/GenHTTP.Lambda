import { useEffect, useRef, useState } from 'react';

import { api, ApiError, type AgentStep, type BuildResult } from '../api';
import { RunHeader, RunLog, RunOutcome, RunProgress, type Run } from '../components/AgentRun';
import { ConnectAgent } from '../components/ConnectAgent';
import { IconExternal } from '../components/Icons';
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
 * While it is built, the box becomes what the Change section of the editor
 * shows of a change (components/AgentRun): what was asked, where it has got
 * to and for how long, what the agent says as it works, and how it ended.
 * Somebody who builds a website here and changes it later in the editor
 * watches the same thing twice.
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

type Words = ReturnType<typeof useT>['build']['steps'];

/** A build as this page follows it: the run, and when its numbers arrived. */
type Followed = Run & { steps: AgentStep[]; received: number };

const WAITING: Followed = { state: 'queued', waiting: 0, seconds: 0, limit: null, steps: [], received: 0 };

/**
 * What the agent is doing, in the words of this page rather than those of its
 * tools: the newest thing it did that somebody who wants a website would put
 * into words - getting ready, choosing an address, checking it for mistakes.
 * Only the newest, since what it said about it is in the log underneath.
 */
function doingOf(steps: AgentStep[], words: Words): string | null {
  for (let index = steps.length - 1; index >= 0; index--) {
    const step = steps[index];
    const before = steps.slice(0, index);

    const text = (() => {
      switch (step.kind) {
        case 'guide':
          return words.guide;
        case 'demos':
          return words.examples;
        case 'read':
          // before it has an address of its own, what it reads is an example
          return before.some((s) => s.kind === 'create') ? words.looking : words.examples;
        case 'create':
          return words.create;
        case 'write':
          return before.some((s) => s.kind === 'write') ? words.improve : words.write;
        case 'check':
          return words.check;
        case 'deploy':
          return words.online;
        case 'logs':
          return words.trying;
        case 'data':
          return step.data === 'database'
            ? words.forRecords
            : step.data === 'secrets'
              ? words.forKeys
              : step.data === 'workspace'
                ? words.forFiles
                : null;
        case 'records':
          return words.records;
        case 'secrets':
          return words.keys;
        case 'upload':
          return words.addFile;
        case 'delete':
          return words.removeFile;
        case 'list':
          return words.files;
        default:
          return null;
      }
    })();

    if (text != null) {
      return text;
    }
  }

  return null;
}

export function Build() {
  usePublicPage('/build');

  const t = useT();
  const said = t.build;
  const { offlineDays, retentionDays } = useLifetimes();

  const [prompt, setPrompt] = useState('');
  const { origin } = useOrigin();
  const [state, setState] = useState<'idle' | 'working' | 'done' | 'failed'>('idle');
  const [run, setRun] = useState<Followed>(WAITING);
  const [result, setResult] = useState<BuildResult | null>(null);
  const [available, setAvailable] = useState<boolean | null>(null);
  const [secondModel, setSecondModel] = useState(false);
  const [model, setModel] = useState('opus');
  const [password, setPassword] = useState('');
  const [copied, setCopied] = useState(false);

  const polling = useRef<number | null>(null);

  // what the agent said as it worked; the tools it called are nothing to
  // somebody who wants a website, and the line above the log names the step
  const spoken = run.steps.filter((step) => step.kind === 'say');

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
    setRun(WAITING);
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

        setRun({
          state: job.state,
          waiting: job.waiting ?? 0,
          seconds: job.seconds ?? 0,
          limit: job.limit ?? null,
          steps: job.steps ?? [],
          received: Date.now(),
        });

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

      {offered && state === 'idle' && (
        <div className="surface mt-10 p-2">
          <textarea
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && (e.metaKey || e.ctrlKey)) start();
            }}
            rows={3}
            maxLength={2000}
            placeholder={said.placeholder}
            className="w-full resize-none bg-transparent px-4 py-3 text-lg outline-none placeholder:text-slate-400"
          />

          <div className="flex items-center justify-between gap-3 px-2 pb-1">
            <span className="text-xs text-slate-400">{said.shortcut}</span>

            <button
              type="button"
              onClick={start}
              disabled={prompt.trim().length < 3 || (model === 'fable' && password.length < 1)}
              className="btn btn-primary"
            >
              {said.buildIt}
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

      {state !== 'idle' && (
        <article className="surface mt-10 text-left" aria-busy={state === 'working'}>
          <RunHeader label={said.asked} prompt={prompt.trim()} meta={model === 'fable' ? 'Fable 5.1' : undefined} />

          {state === 'working' ? (
            <RunProgress
              run={run}
              received={run.received}
              doing={doingOf(run.steps, said.steps) ?? said.starting}
              words={{ queued: said.ahead, starting: said.starting, leaveOpen: said.leaveOpen }}
            />
          ) : state === 'done' && result?.ok ? (
            <RunOutcome
              tone={result.deployed === false ? 'warn' : 'good'}
              headline={result.deployed === false ? said.notOnline : said.online}
              summary={result.summary}
            >
              {result.url && result.deployed !== false && (
                <a href={result.url} target="_blank" rel="noreferrer" className="btn-primary !px-4 !py-1.5 text-[13px]">
                  <IconExternal className="h-3.5 w-3.5" />
                  {said.open}
                </a>
              )}
            </RunOutcome>
          ) : (
            // a refusal is the agent's sentence, in the language of the
            // request; anything else is what went wrong
            <RunOutcome
              tone={result?.declined ? 'quiet' : 'bad'}
              headline={result?.error ?? said.failed}
              detail={result?.detail}
              detailLabel={said.log}
            >
              <button type="button" className="btn-ghost !px-3 !py-1.5 text-[13px]" onClick={() => setState('idle')}>
                {t.common.tryAgain}
              </button>
            </RunOutcome>
          )}

          {spoken.length > 0 && (
            <RunLog steps={spoken} running={state === 'working'} label={said.log} count={spoken.length} describe={(step) => step.text} />
          )}
        </article>
      )}

      {state === 'done' && result?.ok && (
        <section className="mt-8 space-y-4">
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
    </main>
  );
}
