import { useState, type ReactNode } from 'react';

import { absoluteAddress } from '../address';
import type { AgentStep, BuildResult, ChangeResult, Feature } from '../api';
import { RunLog, RunOutcome, RunProgress, type Run, type Tone } from '../components/AgentRun';
import { IconCheck, IconExternal, IconSpark } from '../components/Icons';
import { useT } from '../i18n';
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

/** What somebody asked, as they wrote it. */
export function Asked({ children }: { children: ReactNode }) {
  const said = useT().workspace;

  return (
    <div className="flex justify-end">
      <p className="sr-only">{said.you}</p>
      <div className="max-w-[85%] whitespace-pre-line break-words rounded-2xl rounded-br-md bg-slate-100 px-4 py-2.5 text-[15px] leading-relaxed text-slate-900 dark:bg-ink-850 dark:text-slate-100">
        {children}
      </div>
    </div>
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

/** Building a new app: while it runs, and how it ended. */
export function BuildAnswer({ turn, following, onAgain }: { turn: Turn; following?: Following | null; onAgain?: () => void }) {
  const t = useT();
  const said = t.buildBox;

  if (following && (turn.state === 'queued' || turn.state === 'running')) {
    return (
      <Answered busy>
        <RunProgress
          run={following.run}
          received={following.received}
          doing={buildDoing(following.steps, said.steps) ?? said.starting}
          words={{ queued: said.ahead, starting: said.starting, leaveOpen: t.workspace.leaveOpen }}
        />
        <Said steps={following.steps} running />
      </Answered>
    );
  }

  const result = turn.built;

  if (turn.state === 'done' && result?.ok) {
    return (
      <>
        <Answered>
          <RunOutcome
            tone={result.deployed === false ? 'warn' : 'good'}
            headline={result.deployed === false ? said.notOnline : said.online}
            summary={result.summary ? <Reply text={result.summary} /> : null}
          >
            {result.url && result.deployed !== false && (
              <a href={result.url} target="_blank" rel="noreferrer" className="btn-primary !px-4 !py-1.5 text-[13px]">
                <IconExternal className="h-3.5 w-3.5" />
                {said.open}
              </a>
            )}
          </RunOutcome>
          <Said lines={turn.said} running={false} />
        </Answered>

        <EditorLink result={result} />
      </>
    );
  }

  // a refusal is the agent's sentence, in the language of the request;
  // anything else is what went wrong
  return (
    <Answered>
      <RunOutcome
        tone={result?.declined ? 'quiet' : 'bad'}
        headline={result?.declined ? null : (result?.error ?? said.failed)}
        summary={result?.declined ? result.error : null}
        detail={result?.detail}
        detailLabel={t.workspace.showWork}
      >
        {onAgain && (
          <button type="button" className="btn-ghost !px-3 !py-1.5 text-[13px]" onClick={onAgain}>
            {t.common.tryAgain}
          </button>
        )}
      </RunOutcome>
      <Said lines={turn.said} running={false} />
    </Answered>
  );
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

  const cut = result.reason === 'timeout' ? [said.timedOut] : result.reason === 'turns' ? [said.usedUp] : [];
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

  if (result.ok || result.answered || result.declined || (result.unchanged && result.summary && cut.length === 0)) {
    return { tone: result.declined ? 'quiet' : 'answer', headline: null, notes: [] };
  }

  if (cut.length > 0) {
    return { tone: 'warn', headline: cut[0], notes: [] };
  }

  return { tone: 'bad', headline: said.failed, notes: result.error ? [result.error] : [] };
}

/** A message about an app: while the agent works on it, and what came of it. */
export function ChangeAnswer({ turn, following, app, onAgain }: { turn: Turn; following?: Following | null; app: AppHand; onAgain?: () => void }) {
  const t = useT();
  const said = t.workspace;

  if (turn.state === 'queued' || turn.state === 'running') {
    const steps = following?.steps ?? [];

    return (
      <Answered busy>
        <RunProgress
          run={following?.run ?? { state: turn.state, waiting: 0, seconds: 0 }}
          received={following?.received ?? Date.now()}
          doing={changeDoing(steps, t.buildBox.steps, said.steps) ?? t.buildBox.starting}
          words={{ queued: said.ahead, starting: t.buildBox.starting, leaveOpen: said.leaveOpen }}
        />
        <Said steps={steps} running />
      </Answered>
    );
  }

  const result: ChangeResult | null | undefined = turn.result;
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
  const retry = turn.state === 'failed' || (result != null && !result.ok && !result.answered && !result.declined && !result.summary);

  const reply = result?.summary ?? null;

  return (
    <Answered>
      <RunOutcome
        tone={tone}
        headline={headline}
        notes={notes}
        summary={reply ? <Reply text={reply} /> : null}
        detail={result?.detail}
        detailLabel={said.showWork}
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

        {undoable && (
          <button type="button" onClick={() => app.deploy(before!)} disabled={app.busy} title={said.results.undoTitle} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {said.results.undo}
          </button>
        )}

        {retry && onAgain && (
          <button type="button" onClick={onAgain} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {t.common.tryAgain}
          </button>
        )}
      </RunOutcome>

      <Settings settings={result?.settings ?? []} publicKey={app.publicKey} />

      <Said lines={turn.said} running={false} />
    </Answered>
  );
}

/** The settings a message changed, each a small mark under the answer. */
function Settings({ settings, publicKey }: { settings: string[]; publicKey: string }) {
  const said = useT().workspace.results;
  const words: Record<string, string> = said.settings;
  const known = settings.filter((setting) => words[setting]);

  if (known.length === 0) {
    return null;
  }

  return (
    <ul className="flex flex-wrap gap-2 border-t border-slate-200 px-4 py-2.5 dark:border-ink-800">
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
