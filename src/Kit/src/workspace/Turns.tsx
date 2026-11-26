import { useState, type ReactNode } from 'react';

import { absoluteAddress } from '../address';
import type { AgentStep, BuildResult, ChangeResult, Feature, RunBlocker, RunFile } from '../api';
import { FileList, RunBlockers, RunFiles, RunLog, RunOutcome, RunProgress, type ProgressWords, type Run, type Tone } from '../components/AgentRun';
import { IconCheck, IconExternal, IconSpark } from '../components/Icons';
import { tagOf, useLanguage, useT } from '../i18n';
import { Link } from '../i18n/links';
import { useLifetimes } from '../site';
import { Reply } from './Reply';
import { buildDoing, changeDoing } from './steps';
import type { Turn } from './store';

/*
 * A conversation, turn by turn: what was asked, on the right, and what the
 * agent made of it underneath - drawn with the same parts the editor's Change
 * section draws a change with (components/AgentRun): where it has got to and
 * for how long, what it said as it worked, and how it ended. The difference
 * is only that the ending leads with what the agent said, since in a
 * conversation that is the answer.
 */

/** What somebody asked, as they wrote it, and the files it went with. */
export function Asked({ children, files }: { children: ReactNode; files?: RunFile[] }) {
  const said = useT().workspace;

  return (
    <div className="flex flex-col items-end gap-2">
      <p className="sr-only">{said.you}</p>
      {children && (
        <div className="max-w-[85%] whitespace-pre-line break-words rounded-2xl rounded-br-md bg-slate-100 px-4 py-2.5 text-[15px] leading-relaxed text-slate-900 dark:bg-ink-850 dark:text-slate-100">
          {children}
        </div>
      )}
      {files && files.length > 0 && <FileList files={files} download={said.files.save} className="max-w-[85%] justify-end" />}
    </div>
  );
}

/** How a run in progress is said on this page: its turn, what it waits for and until when. */
export function useProgressWords(): ProgressWords {
  const t = useT();
  const language = useLanguage();
  const said = t.workspace;

  return {
    queued: said.ahead,
    starting: t.buildBox.starting,
    leaveOpen: said.leaveOpen,
    waits: said.waits,
    until: (at) => said.until(at.toLocaleTimeString(tagOf(language), { hour: '2-digit', minute: '2-digit' })),
  };
}

/** What the app waits for after a run, in this page's words, and going on once it is there. */
function Waits({ blockers, onGoOn }: { blockers?: RunBlocker[] | null; onGoOn?: () => void }) {
  const said = useT().workspace.results;

  if (!blockers || blockers.length === 0) {
    return null;
  }

  return (
    <RunBlockers blockers={blockers} words={said.waitsFor}>
      {onGoOn && (
        <button type="button" onClick={onGoOn} title={said.goOnTitle} className="btn-ghost !px-3 !py-1.5 text-[13px]">
          {said.goOn}
        </button>
      )}
    </RunBlockers>
  );
}

/** What the agent made of it: its mark, and a card with the run in it. */
export function Answered({ children, busy }: { children: ReactNode; busy?: boolean }) {
  const said = useT().workspace;

  return (
    <div className="flex gap-3">
      <span
        aria-hidden="true"
        className="mt-1 flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-accent-500/10 text-accent-600 dark:bg-accent-400/10 dark:text-accent-400"
      >
        <IconSpark className="h-3.5 w-3.5" />
      </span>
      <div className="min-w-0 flex-1">
        <p className="sr-only">{said.agent}</p>
        <article className="surface overflow-hidden" aria-busy={busy}>
          {children}
        </article>
      </div>
    </div>
  );
}

/** A job the page follows as it runs: how far it is, and what it has done so far. */
export interface Following {
  run: Run;
  steps: AgentStep[];
  /** When the numbers arrived, to count the seconds on from. */
  received: number;
  /** The files the agent handed back so far. */
  files?: RunFile[] | null;
}

/** What the agent said as it worked, its own lines only - the tools it called are nothing to the owner. */
function Said({ steps, lines, running }: { steps?: AgentStep[]; lines?: string[]; running: boolean }) {
  const said = useT().workspace;
  const spoken: AgentStep[] = steps
    ? steps.filter((step) => step.kind === 'say')
    : (lines ?? []).map((text) => ({ at: 0, kind: 'say', text }) as AgentStep);

  if (spoken.length === 0) {
    return null;
  }

  return <RunLog steps={spoken} running={running} label={said.showWork} count={spoken.length} describe={(step) => step.text} />;
}

/* ------------------------------------------------------------ a new app */

/**
 * Building a new app: while it runs, and how it ended - online, or not yet,
 * asking something before it goes on, waiting for somebody, or failed.
 * onGoOn carries on where it stopped; offered on the newest turn only.
 */
export function BuildAnswer({ turn, following, onAgain, onGoOn, newest = true, live }: {
  turn: Turn;
  following?: Following | null;
  onAgain?: () => void;
  onGoOn?: () => void;
  /** Whether it is the newest turn: what the app waits for is said under that one only. */
  newest?: boolean;
  /** What still waits, as the app is now. */
  live?: (blockers?: RunBlocker[] | null) => RunBlocker[] | null;
}) {
  const t = useT();
  const said = t.buildBox;
  const words = t.workspace;
  const progress = useProgressWords();

  if (following && (turn.state === 'queued' || turn.state === 'running')) {
    return (
      <Answered busy>
        <RunProgress run={following.run} received={following.received} doing={buildDoing(following.steps, said.steps) ?? said.starting} words={{ ...progress, queued: said.ahead }} />
        <RunFiles files={following.files ?? []} label={words.files.forYou} download={words.files.save} />
        <Said steps={following.steps} running />
      </Answered>
    );
  }

  const result = turn.built;
  const files = <RunFiles files={turn.files ?? []} label={words.files.forYou} download={words.files.save} />;

  // a question back: what it asks is the answer for now, the next message the reply
  if (result?.asked && result.question) {
    return (
      <>
        <Answered>
          <RunOutcome tone="answer" headline={words.results.asked} notes={newest ? [words.results.answerBelow] : []} summary={<Reply text={result.question} />} icon={false} />
          {files}
          <Said lines={turn.said} running={false} />
        </Answered>

        {result.privateKey && <EditorLink result={result} />}
      </>
    );
  }

  // what cut it short, after it had made the app: as far as it got, and it can go on
  const cutNote =
    result?.reason === 'timeout' ? words.results.timedOut
      : result?.reason === 'turns' ? words.results.usedUp
        : result?.reason === 'failed' ? words.results.brokeOff
          : result?.reason === 'lost' ? words.results.lost
            : null;

  if (turn.state === 'done' && result?.ok) {
    return (
      <>
        <Answered>
          <RunOutcome
            tone={result.deployed === false || cutNote ? 'warn' : 'good'}
            headline={result.deployed === false ? said.notOnline : said.online}
            notes={cutNote ? [cutNote] : []}
            summary={result.summary ? <Reply text={result.summary} /> : null}
          >
            {cutNote && onGoOn && (
              <button type="button" className="btn-ghost !px-3 !py-1.5 text-[13px]" onClick={onGoOn} title={words.results.goOnTitle}>
                {words.results.goOn}
              </button>
            )}
            {result.url && result.deployed !== false && (
              <a href={result.url} target="_blank" rel="noreferrer" className="btn-primary !px-4 !py-1.5 text-[13px]">
                <IconExternal className="h-3.5 w-3.5" />
                {said.open}
              </a>
            )}
          </RunOutcome>
          {newest && <Waits blockers={live ? live(result.waitsFor) : result.waitsFor} onGoOn={onGoOn} />}
          {files}
          <Said lines={turn.said} running={false} />
        </Answered>

        <EditorLink result={result} />
      </>
    );
  }

  // a refusal is the agent's sentence, in the language of the request;
  // anything else is what went wrong - and where it was cut short, it can go
  // on from where it got to rather than start again
  const cut = result?.reason === 'timeout' || result?.reason === 'turns';

  return (
    <Answered>
      <RunOutcome
        tone={result?.declined ? 'quiet' : cut ? 'warn' : 'bad'}
        headline={result?.declined ? null : cut ? cutNote : said.failed}
        notes={result?.declined || cut ? [] : failedNotes(result, words.results)}
        summary={result?.declined ? result.error : null}
        detail={result?.detail}
        detailLabel={words.results.whatWentWrong}
      >
        {cut && onGoOn && (
          <button type="button" className="btn-primary !px-4 !py-1.5 text-[13px]" onClick={onGoOn} title={words.results.goOnTitle}>
            {words.results.goOn}
          </button>
        )}
        {onAgain && (
          <button type="button" className="btn-ghost !px-3 !py-1.5 text-[13px]" onClick={onAgain}>
            {t.common.tryAgain}
          </button>
        )}
      </RunOutcome>
      {files}
      <Said lines={turn.said} running={false} />
    </Answered>
  );
}

/** Why a build failed, in this page's words where the platform said why - its own English otherwise. */
function failedNotes(result: BuildResult | null | undefined, said: ReturnType<typeof useT>['workspace']['results']): string[] {
  switch (result?.reason) {
    case 'unauthorised':
      return [said.unauthorised];
    case 'lost':
      return [said.lost];
    case 'failed':
      return [said.nothingSaved];
    case 'cancelled':
      return [said.stopped];
    default:
      return result?.error ? [result.error] : [];
  }
}

/** The editor link of what was just built: the one thing that cannot be got back, said once. */
function EditorLink({ result }: { result: BuildResult }) {
  const t = useT();
  const said = t.buildBox;
  const { offlineDays, retentionDays, expires } = useLifetimes();
  const [copied, setCopied] = useState(false);

  if (!result.editorUrl) {
    return null;
  }

  return (
    <div className="ml-10 rounded-xl border border-dashed border-slate-300 px-4 py-3 text-sm dark:border-ink-700">
      <span className="block text-xs font-medium uppercase tracking-wide text-slate-500">{said.further}</span>
      <a href={result.editorUrl} target="_blank" rel="noreferrer" className="mt-1 block truncate font-mono text-[13px] underline">
        {result.editorUrl}
      </a>
      <p className="mt-2 text-slate-600 dark:text-slate-400">
        {said.keep} {t.workspace.results.remembered}
      </p>
      {/* said only where a new lambda starts in a tier that is not kept while unused */}
      {expires && <p className="mt-2 text-slate-500">{said.lifetime(offlineDays, retentionDays)}</p>}
      <button
        type="button"
        className="btn-ghost mt-2 !px-3 !py-1 text-[13px]"
        onClick={() => {
          navigator.clipboard?.writeText(result.editorUrl ?? '');
          setCopied(true);
          window.setTimeout(() => setCopied(false), 1600);
        }}
      >
        {copied ? t.common.copied : said.copyLink}
      </button>
    </div>
  );
}

/* ------------------------------------------------------------ a message about an app */

/** What a message about an app may lead to: things done to the app from the answer. */
export interface AppHand {
  publicKey: string;
  /** Where the app answers, to open it. */
  address: string | null;
  /** The version online now, if any. */
  active?: number | null;
  /** The drafts open now. */
  features: Feature[];
  /** Whether something is being done to the app already. */
  busy: boolean;
  deploy: (version?: number) => void;
  putOnline: (feature: Feature) => void;
  /** Puts a message into the box, for the agent to do in a draft. */
  ask: (text: string, feature: Feature) => void;
  /** Carries a turn on where it stopped: it waited for somebody, or ran out of time. */
  goOn: (turn: Turn) => void;
  /** Opens an app this browser knows, by its public key - one a message built beside this one. */
  open?: (publicKey: string) => void;
  /** Whether a message can go now: one left today, nothing under way, the password where one is needed. */
  possible: boolean;
  /** What of what a turn waited for still waits, as the app is now. */
  live: (blockers?: RunBlocker[] | null) => RunBlocker[] | null;
}

/**
 * How a message ended, in a line and its notes - read off the facts the
 * agent collected from the tools rather than off what the model said, the
 * way the editor's Change section reads them. An answer to a question has
 * no line: what the agent said is the answer.
 */
function verdict(turn: Turn, said: ReturnType<typeof useT>['workspace']['results']): { tone: Tone; headline: string | null; notes: string[] } {
  const result = turn.result;

  if (turn.state === 'cancelled') {
    return { tone: 'quiet', headline: said.stopped, notes: [] };
  }

  if (!result) {
    return { tone: 'bad', headline: said.failed, notes: [] };
  }

  if (result.reason === 'unauthorised') {
    return { tone: 'bad', headline: said.failed, notes: [said.unauthorised] };
  }

  if (result.reason === 'lost') {
    return { tone: 'bad', headline: said.failed, notes: [said.lost] };
  }

  // cut short, or broken off with an error, after it had done something:
  // what is there is as far as it got
  const cut =
    result.reason === 'timeout' ? [said.timedOut] : result.reason === 'turns' ? [said.usedUp] : result.reason === 'failed' ? [said.brokeOff] : [];
  const version = result.version ?? null;

  if (result.ok && version == null && result.feature) {
    return result.compiles === false
      ? { tone: 'warn', headline: said.draftBroken, notes: cut }
      : { tone: 'ready', headline: said.draft, notes: cut };
  }

  if (result.ok && version == null && result.online != null) {
    return { tone: 'good', headline: said.earlier, notes: cut };
  }

  if (result.ok && version != null) {
    if (result.online != null) {
      return { tone: 'good', headline: said.online, notes: cut };
    }

    return { tone: 'warn', headline: said.saved, notes: cut };
  }

  // a question answered, or a setting changed: what the agent said is the
  // answer - but only where the platform's records say so, never on the
  // agent's word that something was done
  if (result.answered || result.declined || (result.ok && cut.length === 0)) {
    return { tone: result.declined ? 'quiet' : 'answer', headline: null, notes: [] };
  }

  if (result.ok) {
    return { tone: 'warn', headline: cut[0], notes: [] };
  }

  if (cut.length > 0) {
    return { tone: 'warn', headline: result.reason === 'failed' ? said.failed : cut[0], notes: result.reason === 'failed' ? [said.nothingSaved] : [] };
  }

  if (result.unchanged) {
    return { tone: 'quiet', headline: said.unchanged, notes: [] };
  }

  return { tone: 'bad', headline: said.failed, notes: result.error ? [result.error] : [] };
}

/**
 * A message about an app: while the agent works on it, and what came of it.
 * newest says it is the last turn, the only one that can be carried on.
 */
export function ChangeAnswer({ turn, following, app, onAgain, newest = false }: {
  turn: Turn;
  following?: Following | null;
  app: AppHand;
  onAgain?: () => void;
  newest?: boolean;
}) {
  const t = useT();
  const said = t.workspace;
  const progress = useProgressWords();

  if (turn.state === 'queued' || turn.state === 'running') {
    const steps = following?.steps ?? [];

    return (
      <Answered busy>
        <RunProgress
          run={following?.run ?? { state: turn.state, waiting: 0, seconds: 0 }}
          received={following?.received ?? Date.now()}
          doing={changeDoing(steps, t.buildBox.steps, said.steps) ?? t.buildBox.starting}
          words={progress}
        />
        <RunFiles files={following?.files ?? []} label={said.files.forYou} download={said.files.save} />
        <Said steps={steps} running />
      </Answered>
    );
  }

  const result: ChangeResult | null | undefined = turn.result;
  const files = <RunFiles files={turn.files ?? []} label={said.files.forYou} download={said.files.save} />;

  // a question back: what it asks is the answer for now, the next message the reply
  if (result?.asked && result.question) {
    return (
      <Answered>
        <RunOutcome tone="answer" headline={said.results.asked} notes={newest ? [said.results.answerBelow] : []} summary={<Reply text={result.question} />} icon={false} />
        {files}
        <Settings settings={result.settings ?? []} created={result.created ?? []} publicKey={app.publicKey} open={app.open} />
        <Said lines={turn.said} running={false} />
      </Answered>
    );
  }

  const { tone, headline, notes } = verdict(turn, said.results);

  const version = result?.version ?? null;
  const online = result?.online ?? null;
  const before = result?.before ?? null;

  // what is offered depends on how things are now, not on how the change
  // left them: once the owner has put the old version back, there is nothing
  // left to undo, and a draft put online is no draft any more
  const draft = result?.feature ? app.features.find((feature) => feature.key === result.feature) : undefined;
  const live = online != null && app.active === online;
  const undoable = live && before != null && before !== online;
  const deployable = version != null && online == null && result?.compiles !== false && app.active !== version;
  const cut = result?.reason === 'timeout' || result?.reason === 'turns';
  const retry = !cut && (turn.state === 'failed' || (result != null && !result.ok && !result.answered && !result.declined && !result.summary));

  const reply = result?.summary ?? null;

  return (
    <Answered>
      <RunOutcome
        tone={tone}
        headline={headline}
        notes={notes}
        summary={reply ? <Reply text={reply} /> : null}
        detail={result?.detail}
        detailLabel={said.results.whatWentWrong}
        icon={tone !== 'answer' && tone !== 'quiet'}
      >
        {draft?.online && (
          <a href={absoluteAddress(draft.previewPath)} target="_blank" rel="noreferrer" className="btn-primary !px-4 !py-1.5 text-[13px]">
            <IconExternal className="h-3.5 w-3.5" />
            {said.attention.tryIt}
          </a>
        )}

        {draft && result?.compiles !== false && draft.mergeable && (
          <button type="button" onClick={() => app.putOnline(draft)} disabled={app.busy} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {said.attention.putOnline}
          </button>
        )}

        {/* begun before the app last changed: it takes that change in first, which is the agent's to do */}
        {draft && !draft.mergeable && (
          <button type="button" onClick={() => app.ask(said.attention.bringUpAsk, draft)} title={said.attention.outOfDate} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {said.attention.bringUp}
          </button>
        )}

        {deployable && (
          <button type="button" onClick={() => app.deploy(version!)} disabled={app.busy} className="btn-primary !px-4 !py-1.5 text-[13px]">
            {said.results.putOnline}
          </button>
        )}

        {live && app.address && (
          <a href={app.address} target="_blank" rel="noreferrer" className="btn-primary !px-4 !py-1.5 text-[13px]">
            <IconExternal className="h-3.5 w-3.5" />
            {said.openApp}
          </a>
        )}

        {cut && newest && (
          <button type="button" onClick={() => app.goOn(turn)} disabled={app.busy || !app.possible} title={said.results.goOnTitle} className="btn-primary !px-4 !py-1.5 text-[13px]">
            {said.results.goOn}
          </button>
        )}

        {undoable && (
          <button
            type="button"
            onClick={() => app.deploy(before!)}
            disabled={app.busy}
            title={said.results.undoTitle}
            className="btn-ghost !px-3 !py-1.5 text-[13px]"
          >
            {said.results.undo}
          </button>
        )}

        {retry && onAgain && (
          <button type="button" onClick={onAgain} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {t.common.tryAgain}
          </button>
        )}
      </RunOutcome>

      {/* what the app waits for is as of now, so it is said once, under the newest */}
      {newest && <Waits blockers={app.live(result?.waitsFor)} onGoOn={app.possible ? () => app.goOn(turn) : undefined} />}

      {files}

      <Settings settings={result?.settings ?? []} created={result?.created ?? []} publicKey={app.publicKey} open={app.open} />

      <Said lines={turn.said} running={false} />
    </Answered>
  );
}

/** The settings a message changed, and the apps it built beside this one, each a small mark under the answer. */
function Settings({ settings, created, publicKey, open }: { settings: string[]; created: string[]; publicKey: string; open?: (publicKey: string) => void }) {
  const said = useT().workspace.results;
  const words: Record<string, string> = said.settings;
  const known = settings.filter((setting) => words[setting]);

  if (known.length === 0 && created.length === 0) {
    return null;
  }

  return (
    <ul className="flex flex-wrap gap-2 border-t border-slate-200 px-4 py-2.5 dark:border-ink-800">
      {created.map((name) => (
        <li key={`created-${name}`} className="inline-flex items-center gap-1.5 rounded-full bg-accent-500/10 px-2.5 py-0.5 text-xs font-medium text-accent-700 dark:text-accent-400">
          <IconSpark className="h-3 w-3" />
          {open ? (
            <button type="button" onClick={() => open(name)} className="underline">
              {said.created(name)}
            </button>
          ) : (
            said.created(name)
          )}
        </li>
      ))}
      {known.map((setting) => (
        <li key={setting} className="inline-flex items-center gap-1.5 rounded-full bg-emerald-500/10 px-2.5 py-0.5 text-xs font-medium text-emerald-700 dark:text-emerald-400">
          <IconCheck className="h-3 w-3" />
          {words[setting]}
          {setting === 'source' && (
            <Link to={`/source/${publicKey}`} className="ml-1 underline">
              {said.seeSource}
            </Link>
          )}
        </li>
      ))}
    </ul>
  );
}
