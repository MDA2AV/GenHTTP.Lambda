import { useEffect, useLayoutEffect, useRef, useState, type ReactNode } from 'react';
import { Link, useSearchParams } from 'react-router-dom';

import { absoluteAddress } from '../address';
import { isActive, type AgentState, type AgentStep, type ChangeJob, type Feature } from '../api';
import { CopyField } from '../components/CopyField';
import { Dialog } from '../components/Dialog';
import {
  IconAlert,
  IconBook,
  IconCheck,
  IconChevronDown,
  IconDots,
  IconDraft,
  IconExternal,
  IconEye,
  IconFolder,
  IconHistory,
  IconInfo,
  IconLayers,
  IconList,
  IconPencil,
  IconPlay,
  IconPlus,
  IconSpark,
  IconSpinner,
  IconStop,
  IconTrash,
  IconUpload,
  IconWrench,
} from '../components/Icons';
import { tagOf, useEditorT, useLanguage } from '../i18n';
import { useOrigin } from '../site';
import type { Control } from './context';
import { span } from './format';
import { Section, Switch } from './ui';
import { useShared } from './words';

type Words = ReturnType<typeof useEditorT>['change'];

/**
 * Asking the agent of this installation for a change.
 *
 * Built like a conversation with somebody who does the work while you watch:
 * what you asked for at the top, what it is doing underneath as it does it,
 * and how it ended with what to do next - look at the difference, open it,
 * or put the version before back. Then the box again, for the next thing.
 *
 * The change itself is followed by the frame (control/agent.ts), so this only
 * draws it: leaving the section does not stop anything, and coming back finds
 * it where it got to.
 *
 * The agent works in a feature - a draft, to the owner: a new one, or one
 * the owner picks to go on with - so what it does is tried at the feature's
 * own address before any visitor sees it. Told to put it online, it merges
 * the feature into the next version once it works and deploys that; told
 * not to, it leaves the draft for the owner to try and put online from here.
 */
export function ChangeTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.change;
  const heading = t.frame.sections.change;

  const { agent } = control;
  const state = agent.state;
  const job = state?.job ?? null;
  const running = isActive(job);

  const [prompt, setPrompt] = useDraft(control.lambda.publicKey);
  const composer = useRef<HTMLTextAreaElement>(null);

  // a feature asked for by the link - "ask the agent" on a feature - or the
  // one the last change left open, which is what a next request builds on
  const [params, setParams] = useSearchParams();
  const open = (key?: string | null) => (key && control.features.some((f) => f.key === key) ? key : null);
  const target = open(params.get('feature')) ?? open(job?.result?.feature) ?? null;

  // a request the link suggests - bringing a draft up to date - goes into the
  // box once, to be sent or changed; a reload does not put it back
  const suggested = params.get('ask');

  useEffect(() => {
    if (!suggested) {
      return;
    }

    setPrompt(suggested);
    setParams((was) => {
      const next = new URLSearchParams(was);
      next.delete('ask');
      return next;
    }, { replace: true });

    window.requestAnimationFrame(() => composer.current?.focus());
    // the box takes what is typed into it; only a new suggestion replaces that
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [suggested]);

  // looking at the section is looking at how the last change ended
  const { seen } = agent;

  useEffect(() => {
    seen();
  }, [seen, job?.state]);

  if (!state) {
    return (
      <Section title={heading} hint={said.hint}>
        {agent.failure ? (
          <p className="flex items-center gap-2 py-10 text-sm text-red-500">
            <IconAlert className="h-4 w-4" /> {agent.failure}
          </p>
        ) : (
          <div className="flex items-center gap-2 py-10 text-sm text-slate-500">
            <IconSpinner /> {said.reading}
          </div>
        )}
      </Section>
    );
  }

  if (!state.available) {
    return (
      <Section title={heading}>
        <div className="max-w-2xl">
          <h2 className="text-[15px] font-medium">{said.offTitle}</h2>
          <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{said.off}</p>
          <OwnAgent control={control} open />
        </div>
      </Section>
    );
  }

  /** Puts words into the box and the cursor at their end, to ask again or build on them. */
  const prefill = (text: string) => {
    setPrompt(text);

    window.requestAnimationFrame(() => {
      const field = composer.current;

      if (field) {
        field.focus();
        field.setSelectionRange(text.length, text.length);
        field.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
      }
    });
  };

  return (
    <Section title={heading} hint={said.hint}>
      <div className="max-w-3xl space-y-6">
        {job && <JobCard control={control} job={job} onAgain={() => prefill(job.prompt)} />}

        {!running && (
          <Composer
            key={target ?? 'new'}
            control={control}
            state={state}
            prompt={prompt}
            onPrompt={setPrompt}
            field={composer}
            next={job != null}
            target={target}
          />
        )}

        {!job && <HowItWorks />}

        <OwnAgent control={control} />
      </div>
    </Section>
  );
}

/** What happens after the button, for somebody who has not pressed it yet. */
function HowItWorks() {
  const said = useEditorT().change;
  const icons = [IconEye, IconDraft, IconPlay];

  return (
    <ol className="grid gap-5 pt-2 sm:grid-cols-3">
      {said.how.map((step, index) => {
        const Icon = icons[index] ?? IconSpark;

        return (
          <li key={step.title} className="flex gap-3">
            <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-accent-500/10 text-accent-500 dark:text-accent-400">
              <Icon className="h-3.5 w-3.5" />
            </span>
            <div>
              <p className="text-[13px] font-medium">{step.title}</p>
              <p className="mt-0.5 text-[13px] leading-relaxed text-slate-500">{step.text}</p>
            </div>
          </li>
        );
      })}
    </ol>
  );
}

/** How many more the owner may ask for today, beside the button that spends one. */
function Allowance({ state }: { state: AgentState }) {
  const said = useEditorT().change;

  return (
    <span
      title={said.leftTitle}
      className={`text-xs tabular-nums ${state.left === 0 ? 'text-amber-600 dark:text-amber-400' : 'text-slate-500'}`}
    >
      {said.left(state.left, state.perDay)}
    </span>
  );
}

/* ------------------------------------------------------------ the box */

/**
 * What is typed in the box, kept for the tab while the owner looks at another
 * section: a request half written is not something to lose to a click.
 */
function useDraft(lambda: string): [string, (text: string) => void] {
  const key = `lambda-change-draft:${lambda}`;

  const [draft, setDraft] = useState(() => {
    try {
      return sessionStorage.getItem(key) ?? '';
    } catch {
      return '';
    }
  });

  const keep = (text: string) => {
    setDraft(text);

    try {
      if (text.trim() === '') {
        sessionStorage.removeItem(key);
      } else {
        sessionStorage.setItem(key, text);
      }
    } catch {
      // nothing kept, nothing lost but the draft
    }
  };

  return [draft, keep];
}

const REMEMBERED = 'lambda-change-online';

function remembered(): boolean {
  try {
    return localStorage.getItem(REMEMBERED) !== 'no';
  } catch {
    return true;
  }
}

function Composer({
  control,
  state,
  prompt,
  onPrompt,
  field,
  next,
  target,
}: {
  control: Control;
  state: AgentState;
  prompt: string;
  onPrompt: (text: string) => void;
  field: React.RefObject<HTMLTextAreaElement>;
  /** Whether this follows a change, which changes what the box asks. */
  next: boolean;
  /** The feature to go on with, as the page suggests it; a new one when null. */
  target: string | null;
}) {
  const t = useEditorT();
  const said = t.change;
  const plain = t.simple;
  const language = useLanguage();

  const [online, setOnline] = useState(remembered);
  const [picked, setWhere] = useState<string>(target ?? '');

  const { features } = control;

  // merged or deleted since it was picked: a new one, then
  const where = features.some((f) => f.key === picked) ? picked : '';
  const limit = control.summary?.limits.features ?? Infinity;
  const full = features.length >= limit;
  const [model, setModel] = useState<'opus' | 'fable'>('opus');
  const [password, setPassword] = useState('');
  const [sending, setSending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const spent = state.left <= 0;
  const wanted = prompt.trim();
  const room = where !== '' || !full;
  const ready = wanted.length >= 3 && !sending && !spent && room && (model === 'opus' || password.length > 0);

  // the box grows with what is in it, up to a point, and the page scrolls after that
  useLayoutEffect(() => {
    const box = field.current;

    if (box) {
      box.style.height = 'auto';
      box.style.height = `${Math.min(box.scrollHeight, 320)}px`;
    }
  }, [prompt, field]);

  const toggle = () => {
    setOnline((was) => {
      try {
        localStorage.setItem(REMEMBERED, was ? 'no' : 'yes');
      } catch {
        // a browser that keeps nothing asks every time, which is fine
      }

      return !was;
    });
  };

  async function send() {
    if (!ready) {
      return;
    }

    setSending(true);
    setError(null);

    const refused = await control.agent.start({
      prompt: wanted,
      deploy: online,
      model: model === 'opus' ? undefined : model,
      password: model === 'opus' ? undefined : password,
      language,
      feature: where || undefined,
    });

    setSending(false);

    if (refused) {
      setError(refused);
    } else {
      onPrompt('');
    }
  }

  const problems = (control.summary?.recentProblems.length ?? 0) > 0;
  const ideas = problems ? [said.fixLog, ...said.ideas] : said.ideas;

  const mac = typeof navigator !== 'undefined' && /Mac|iPhone|iPad/.test(navigator.platform);

  // the allowance starts again at midnight UTC, which is some other hour here
  const reset = new Date();
  reset.setUTCHours(24, 0, 0, 0);

  return (
    <div>
      <div className="mb-2 flex flex-wrap items-center justify-between gap-x-4 gap-y-1.5">
        <label htmlFor="change-prompt" className="block text-[15px] font-medium">
          {next ? said.placeholderNext : said.label}
        </label>

        {/* where it works: a draft of its own, or one to go on with - which
            is only a question once there is one to go on with */}
        {features.length > 0 && (
          <label className="flex items-center gap-2 text-[13px] text-slate-600 dark:text-slate-400" title={said.whereTitle}>
            <IconDraft className="h-3.5 w-3.5 shrink-0 text-slate-400" />
            {said.where}
            <select
              value={where}
              onChange={(event) => setWhere(event.target.value)}
              className="max-w-[14rem] truncate rounded border border-slate-200 bg-transparent px-1.5 py-0.5 text-[13px] dark:border-ink-800"
            >
              <option value="" disabled={full}>{said.newFeature}</option>
              {features.map((feature) => (
                <option key={feature.key} value={feature.key}>{feature.name}</option>
              ))}
            </select>
          </label>
        )}
      </div>

      <div className="surface focus-within:border-accent-500 focus-within:ring-1 focus-within:ring-accent-500 dark:focus-within:border-accent-400 dark:focus-within:ring-accent-400">
        <textarea
          id="change-prompt"
          ref={field}
          value={prompt}
          onChange={(event) => onPrompt(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' && (event.metaKey || event.ctrlKey)) {
              event.preventDefault();
              void send();
            }
          }}
          rows={3}
          maxLength={2000}
          disabled={sending}
          placeholder={said.placeholder}
          className="block min-h-[5.5rem] w-full resize-none bg-transparent px-4 py-3 text-[15px] leading-relaxed outline-none placeholder:text-slate-400 disabled:opacity-60 dark:placeholder:text-slate-600"
        />

        <div className="flex flex-wrap items-center gap-x-4 gap-y-2 border-t border-slate-200 px-3 py-2 dark:border-ink-800">
          <div className="flex items-center gap-2" title={control.simple ? (online ? plain.askOnline : plain.askDraft) : online ? said.goOnlineOn : said.goOnlineOff}>
            <Switch on={online} onToggle={toggle} labelledBy="change-online" />
            <span id="change-online" className="text-[13px] text-slate-600 dark:text-slate-400">
              {said.goOnline}
            </span>
          </div>

          {state.secondModel && (
            <div className="flex items-center gap-1.5">
              {(
                [
                  ['opus', 'Opus 5.5'],
                  ['fable', 'Fable 5.1'],
                ] as const
              ).map(([id, name]) => (
                <button
                  key={id}
                  type="button"
                  onClick={() => setModel(id)}
                  aria-pressed={model === id}
                  className={`rounded-full border px-2.5 py-0.5 text-xs ${
                    model === id
                      ? 'border-accent-500 bg-accent-500/10 text-accent-700 dark:border-accent-400 dark:text-accent-400'
                      : 'border-slate-200 text-slate-500 hover:text-slate-800 dark:border-ink-800 dark:hover:text-slate-200'
                  }`}
                >
                  {name}
                </button>
              ))}
              {model === 'fable' && (
                <input
                  type="password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  placeholder={said.password}
                  autoComplete="off"
                  className="field !w-28 !py-0.5 text-xs"
                />
              )}
            </div>
          )}

          <span className="ml-auto flex items-center gap-3">
            <Allowance state={state} />
            <span className="hidden text-xs text-slate-400 lg:inline">{mac ? '⌘ Enter' : 'Ctrl + Enter'}</span>
          </span>

          <button type="button" onClick={send} disabled={!ready} className="btn-primary !px-4 !py-1.5 text-[13px]">
            {sending ? <IconSpinner className="h-3.5 w-3.5" /> : <IconSpark className="h-3.5 w-3.5" />}
            {sending ? said.sending : said.send}
          </button>
        </div>
      </div>

      <p className="mt-2 text-xs text-slate-500">
        {control.simple ? (online ? plain.askOnline : plain.askDraft) : online ? said.goOnlineOn : said.goOnlineOff}
        {model === 'fable' && ` ${said.fable}`}
      </p>

      {!room && (
        <p className="mt-3 text-sm text-amber-600 dark:text-amber-400">{said.full(limit)}</p>
      )}

      {spent && (
        <p className="mt-3 text-sm text-amber-600 dark:text-amber-400">
          {said.noneLeft(reset.toLocaleTimeString(tagOf(language), { hour: '2-digit', minute: '2-digit' }))}
        </p>
      )}

      {error && (
        <p className="mt-3 flex items-start gap-2 text-sm text-red-500">
          <IconAlert className="mt-0.5 h-4 w-4 shrink-0" /> {error}
        </p>
      )}

      {wanted === '' && !spent && (
        <div className="mt-4 flex flex-wrap gap-1.5">
          {ideas.map((idea) => (
            <button
              key={idea}
              type="button"
              onClick={() => {
                onPrompt(idea);
                field.current?.focus();
              }}
              className="rounded-full border border-slate-200 px-3 py-1 text-[13px] text-slate-600 transition-colors hover:border-slate-300 hover:text-slate-900 dark:border-ink-800 dark:text-slate-400 dark:hover:border-ink-700 dark:hover:text-slate-200"
            >
              {idea}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

/* ------------------------------------------------------------ one change */

function JobCard({ control, job, onAgain }: { control: Control; job: ChangeJob; onAgain: () => void }) {
  const said = useEditorT().change;
  const shared = useShared();
  const running = isActive(job);
  const given = job.feature ? control.features.find((f) => f.key === job.feature) : undefined;
  const [stopping, setStopping] = useState(false);
  const [halting, setHalting] = useState(false);
  const [stopError, setStopError] = useState<string | null>(null);

  // a container is killed a moment after it is asked to be, so the button
  // says it is stopping until the change says it has stopped
  useEffect(() => {
    if (!running) {
      setHalting(false);
    }
  }, [running]);

  return (
    <article className="surface" aria-busy={running}>
      <header className="flex items-start gap-3 border-b border-slate-200 px-4 py-3 dark:border-ink-800">
        <div className="min-w-0 flex-1">
          <div className="text-xs font-medium uppercase tracking-wide text-slate-500">{said.asked}</div>
          <p className="mt-1 whitespace-pre-line break-words text-[15px] leading-relaxed">{job.prompt}</p>
          <p className="mt-1.5 text-xs text-slate-500">
            {given && `${said.inFeature(given.name)} · `}
            {job.deploy ? said.goesOnline : said.review}
            {job.model === 'fable' && ' · Fable 5.1'}
            {!running && job.seconds > 0 && ` · ${said.took(span(job.seconds, shared))}`}
          </p>
        </div>

        {running && (
          <button type="button" onClick={() => setStopping(true)} disabled={halting} className="btn-danger !px-3 !py-1.5 text-[13px]">
            {halting ? <IconSpinner className="h-3.5 w-3.5" /> : <IconStop className="h-3.5 w-3.5" />}
            {halting ? said.stopping : said.stop}
          </button>
        )}
      </header>

      {running ? <Progress control={control} job={job} /> : <Outcome control={control} job={job} onAgain={onAgain} />}

      {job.steps.some((step) => !control.simple || step.kind === 'say') && <Timeline job={job} running={running} simple={control.simple} />}

      <Dialog
        title={said.stopTitle}
        open={stopping}
        onClose={() => setStopping(false)}
        footer={
          <>
            <button type="button" onClick={() => setStopping(false)} className="btn-ghost">
              {said.keepGoing}
            </button>
            <button
              type="button"
              className="btn-danger"
              onClick={async () => {
                setStopping(false);
                setHalting(true);

                const refused = await control.agent.stop();

                setStopError(refused);

                if (refused) {
                  setHalting(false);
                }
              }}
            >
              {said.stopIt}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">{said.stopText}</p>
      </Dialog>

      {stopError && <p className="border-t border-slate-200 px-4 py-2 text-sm text-red-500 dark:border-ink-800">{stopError}</p>}
    </article>
  );
}

/** Seconds as a clock reads them: 1:42, or 1:02:03 past the hour. */
function clock(seconds: number): string {
  const s = Math.max(0, Math.floor(seconds));
  const pad = (n: number) => String(n).padStart(2, '0');

  return s >= 3600
    ? `${Math.floor(s / 3600)}:${pad(Math.floor((s % 3600) / 60))}:${pad(s % 60)}`
    : `${Math.floor(s / 60)}:${pad(s % 60)}`;
}

/** The time now, once a second while something is counting. */
function useNow(counting: boolean): number {
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    if (!counting) {
      return;
    }

    const timer = window.setInterval(() => setNow(Date.now()), 1000);

    return () => window.clearInterval(timer);
  }, [counting]);

  return now;
}

/** Where a running change has got to: in the queue, or at work, and for how long. */
function Progress({ control, job }: { control: Control; job: ChangeJob }) {
  const said = useEditorT().change;
  const now = useNow(job.state === 'running');

  // counted by the agent and carried on here, so a browser whose clock is
  // wrong still counts the right seconds
  const elapsed = job.state === 'running' ? job.seconds + Math.max(0, (now - control.agent.received) / 1000) : 0;
  const share = job.limit ? Math.min(1, elapsed / job.limit) : 0;

  // the step it is on, when it is on one; what it said last is already on
  // the screen, underneath
  const pending = [...job.steps].reverse().find((step) => step.kind !== 'say');
  const doing = pending && !pending.done && !control.simple ? <StepText step={pending} said={said} /> : said.working;

  return (
    <div className="px-4 py-3">
      <div className="flex items-center gap-2.5 text-sm">
        <IconSpinner className={`h-4 w-4 shrink-0 ${job.state === 'queued' ? 'text-amber-500' : 'text-accent-500'}`} />
        <span role="status" className="min-w-0 flex-1 truncate font-medium">
          {job.state === 'queued' ? (job.waiting > 0 ? said.queued(job.waiting) : said.starting) : doing}
        </span>
        {job.state === 'running' && (
          <span className="shrink-0 font-mono text-xs tabular-nums text-slate-500">
            {clock(elapsed)}
            {job.limit ? <span className="text-slate-400"> / {clock(job.limit)}</span> : null}
          </span>
        )}
      </div>

      {job.state === 'running' && job.limit ? (
        <div className="mt-2.5 h-0.5 w-full bg-slate-200 dark:bg-ink-800" aria-hidden="true">
          <div className="h-full bg-accent-500 transition-[width] duration-1000 ease-linear dark:bg-accent-400" style={{ width: `${share * 100}%` }} />
        </div>
      ) : null}

      <p className="mt-2 text-xs text-slate-500">{said.leaveOpen}</p>
    </div>
  );
}

type Tone = 'good' | 'ready' | 'warn' | 'bad' | 'quiet';

const TONES: Record<Tone, { box: string; icon: string }> = {
  good: { box: 'bg-emerald-500/5', icon: 'text-emerald-600 dark:text-emerald-400' },
  ready: { box: 'bg-accent-500/5', icon: 'text-accent-500 dark:text-accent-400' },
  warn: { box: 'bg-amber-500/5', icon: 'text-amber-600 dark:text-amber-400' },
  bad: { box: 'bg-red-500/5', icon: 'text-red-500 dark:text-red-400' },
  quiet: { box: '', icon: 'text-slate-400' },
};

/**
 * What a finished change comes to, in a line and a note.
 *
 * Read off the facts the agent collected from the tools - which version was
 * saved, which went online, whether it compiles - rather than off what the
 * model said, because a model says it deployed things it never deployed.
 */
function verdict(job: ChangeJob, active: number | undefined, feature: string | null, said: Words): { tone: Tone; headline: string; notes: string[] } {
  const result = job.result;
  const version = result?.version ?? undefined;

  if (job.state === 'cancelled') {
    const kept = version != null ? [said.results.stoppedSaved(version)] : feature != null ? [said.results.stoppedFeature(feature)] : [];

    return { tone: 'quiet', headline: said.results.stopped, notes: kept };
  }

  if (!result) {
    return { tone: 'bad', headline: said.results.failed, notes: [] };
  }

  if (result.reason === 'unauthorised') {
    return { tone: 'bad', headline: said.results.failed, notes: [said.results.unauthorised] };
  }

  // cut short after it had saved something: what is there is as far as it got
  const cut = result.reason === 'timeout' ? [said.results.timedOut] : result.reason === 'turns' ? [said.results.usedUp] : [];

  // left in a draft rather than put online: asked to, or not done by the end
  if (result.ok && version == null && feature != null) {
    const merged = job.deploy ? [said.results.notMerged] : [];

    if (result.compiles === false) {
      return { tone: 'warn', headline: said.results.featureBroken(feature), notes: [said.results.previewStill, ...merged, ...cut] };
    }

    return {
      tone: 'ready',
      headline: said.results.feature(feature),
      notes: [result.preview ? said.results.tryIt : said.results.previewOffline, ...merged, ...cut],
    };
  }

  if (!result.ok || version == null) {
    if (result.reason === 'timeout' || result.reason === 'turns') {
      return {
        tone: 'warn',
        headline: said.results.unchanged,
        notes: [result.reason === 'timeout' ? said.results.timeout : said.results.turns],
      };
    }

    if (result.unchanged) {
      return { tone: 'quiet', headline: said.results.unchanged, notes: result.summary ? [] : [said.results.nothing] };
    }

    return { tone: 'bad', headline: said.results.failed, notes: result.error ? [result.error] : [] };
  }

  const timedOut = cut;

  if (result.online != null) {
    return { tone: 'good', headline: said.results.online(result.online), notes: timedOut };
  }

  const still = active != null ? said.results.stillOnline(active) : said.results.nothingOnline;

  if (result.compiles === false) {
    return { tone: 'warn', headline: said.results.broken(version), notes: [still, ...timedOut] };
  }

  if (!job.deploy) {
    return {
      tone: 'ready',
      headline: said.results.ready(version),
      notes: [...(result.compiles ? [said.results.readyNote] : []), ...timedOut],
    };
  }

  return { tone: 'warn', headline: said.results.saved(version), notes: [said.results.notOnline, still, ...timedOut] };
}

function Outcome({ control, job, onAgain }: { control: Control; job: ChangeJob; onAgain: () => void }) {
  const t = useEditorT();
  const { simple } = control;

  // the simple view says what happened to the change, not which version it became
  const said: Words = simple ? { ...t.change, results: { ...t.change.results, ...t.simple.results } } : t.change;
  const { lambda, busy } = control;
  const result = job.result;

  // the feature it left the change in, as it is now: merged or deleted since,
  // there is nothing left to open
  const left: Feature | undefined = result?.feature ? control.features.find((f) => f.key === result.feature) : undefined;
  const name = left?.name ?? result?.featureName ?? null;

  // the name it was given, or - never said - the start of its key
  const judged = verdict(job, lambda.activeVersion, result?.feature ? name ?? result.feature.slice(0, 8) : null, said);

  // the draft it left is gone since - put online, or discarded - so there is
  // nothing left to try, and what the verdict says about it is history
  const gone = result?.feature != null && !left && result.version == null && (result.ok || job.state === 'cancelled');
  const { tone, headline, notes } = gone ? { ...judged, tone: 'quiet' as Tone, notes: [t.features.missingText] } : judged;
  const look = TONES[tone];

  const version = result?.version ?? undefined;
  const online = result?.online ?? undefined;
  const before = result?.before ?? job.before ?? undefined;

  // what is offered depends on how things are now, not on how the change
  // left them: once the owner has put the old version back, there is nothing
  // left to undo
  const live = online != null && lambda.activeVersion === online;
  const deployable = version != null && online == null && result?.compiles !== false && lambda.activeVersion !== version;
  const undoable = live && before != null && before !== online;
  const retry = !result?.ok || (version == null && !left && !gone);

  const Icon = tone === 'good' ? IconCheck : tone === 'ready' ? IconCheck : tone === 'quiet' ? IconInfo : IconAlert;

  return (
    <div className={`flex gap-3 px-4 py-4 ${look.box}`}>
      <Icon className={`mt-0.5 h-5 w-5 shrink-0 ${look.icon}`} />

      <div className="min-w-0 flex-1">
        <p className="text-[15px] font-semibold">{headline}</p>

        {notes.map((note) => (
          <p key={note} className="mt-1 text-sm text-slate-600 dark:text-slate-400">
            {note}
          </p>
        ))}

        {result?.summary && (
          <p className="mt-3 whitespace-pre-line break-words text-[15px] leading-relaxed text-slate-800 dark:text-slate-200">{result.summary}</p>
        )}

        {result?.detail && !result.summary && (
          <details className="mt-2 text-xs text-slate-500">
            <summary className="cursor-pointer select-none">{said.log}</summary>
            <pre className="mt-1 whitespace-pre-wrap break-words font-mono">{result.detail}</pre>
          </details>
        )}

        <div className="mt-4 flex flex-wrap items-center gap-2">
          {/* left as a draft: try it, and put it online from here once it is right */}
          {left?.online && (
            <a href={absoluteAddress(left.previewPath)} target="_blank" rel="noreferrer" className="btn-primary !px-4 !py-1.5 text-[13px]">
              <IconExternal className="h-3.5 w-3.5" />
              {said.openPreview}
            </a>
          )}

          {left && result?.compiles !== false && (
            <button type="button" onClick={() => control.putOnline(left.key)} className="btn-ghost !px-3 !py-1.5 text-[13px]" title={t.features.mergeTitleShort}>
              <IconPlay className="h-3.5 w-3.5" />
              {t.features.mergeButton}
            </button>
          )}

          {left && (
            <button type="button" onClick={() => control.openFeature(left.key)} className={`${left.online ? 'btn-ghost !px-3' : 'btn-primary !px-4'} !py-1.5 text-[13px]`}>
              <IconDraft className="h-3.5 w-3.5" />
              {said.openFeature}
            </button>
          )}

          {deployable && (
            <button type="button" onClick={() => control.deploy(version)} disabled={busy !== null} className="btn-primary !px-4 !py-1.5 text-[13px]">
              {busy === 'deploy' ? <IconSpinner className="h-3.5 w-3.5" /> : <IconPlay className="h-3.5 w-3.5" />}
              {simple ? t.simple.deploy : said.deploy(version!)}
            </button>
          )}

          {live && (
            <a href={absoluteAddress(lambda.address)} target="_blank" rel="noreferrer" className="btn-primary !px-4 !py-1.5 text-[13px]">
              <IconExternal className="h-3.5 w-3.5" />
              {said.open}
            </a>
          )}

          {version != null && !simple && (
            <Link to={`/editor/${control.privateKey}/versions?version=${version}`} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              {said.seeChanges}
            </Link>
          )}

          {undoable && (
            <button
              type="button"
              onClick={() => control.deploy(before)}
              disabled={busy !== null}
              title={simple ? t.simple.undoTitle : said.undoTitle}
              className="btn-ghost !px-3 !py-1.5 text-[13px]"
            >
              {simple ? t.simple.undo : said.undo(before!)}
            </button>
          )}

          {retry && (
            <button type="button" onClick={onAgain} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              {said.again}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

/* ------------------------------------------------------------ the steps */

/**
 * Everything the agent did, oldest first, in the order it did it. Open and
 * following along while it runs; folded away once it has ended, where the
 * outcome is what is read and this is there for anybody who wants the how.
 *
 * The simple view keeps what the agent said and leaves out the tools it
 * called: "Looking at how the scores are stored" means something to the
 * owner, "Changing lambda.cs" does not.
 */
function Timeline({ job, running, simple }: { job: ChangeJob; running: boolean; simple: boolean }) {
  const said = useEditorT().change;
  const steps = simple ? job.steps.filter((step) => step.kind === 'say') : job.steps;
  const list = useRef<HTMLOListElement>(null);
  const [open, setOpen] = useState(running);

  // it was open while it ran; once it ends, the outcome takes over
  useEffect(() => setOpen(running), [running]);

  // new steps arrive at the bottom, so the list follows them - unless the
  // owner has scrolled up to read something, which it then leaves alone
  const following = useRef(true);

  useLayoutEffect(() => {
    const box = list.current;

    if (box && following.current) {
      box.scrollTop = box.scrollHeight;
    }
  }, [steps.length, open]);

  // the last step that has not answered is the one being worked on
  const current = running ? lastIndex(steps, (step) => step.kind !== 'say' && !step.done) : -1;

  return (
    <div className="border-t border-slate-200 dark:border-ink-800">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        aria-expanded={open}
        className="flex w-full items-center gap-2 px-4 py-2.5 text-left text-[13px] text-slate-500 hover:text-slate-800 dark:hover:text-slate-200"
      >
        <IconChevronDown className={`h-4 w-4 transition-transform ${open ? '' : '-rotate-90'}`} />
        <span className="font-medium">{said.log}</span>
        {!open && <span className="tabular-nums text-slate-400">{simple ? steps.length : steps.filter((step) => step.kind !== 'say').length}</span>}
      </button>

      {open && (
        <ol
          ref={list}
          onScroll={(event) => {
            const box = event.currentTarget;
            following.current = box.scrollHeight - box.scrollTop - box.clientHeight < 48;
          }}
          className="max-h-[26rem] overflow-y-auto px-4 pb-3"
        >
          {steps.map((step, index) => (
            <Step key={index} step={step} said={said} working={index === current} />
          ))}
        </ol>
      )}
    </div>
  );
}

function lastIndex<T>(items: T[], test: (item: T) => boolean): number {
  for (let index = items.length - 1; index >= 0; index--) {
    if (test(items[index])) {
      return index;
    }
  }

  return -1;
}

const ICONS: Record<AgentStep['kind'], (props: { className?: string }) => ReactNode> = {
  say: IconSpark,
  guide: IconBook,
  demos: IconLayers,
  read: IconEye,
  logs: IconList,
  create: IconPlus,
  write: IconPencil,
  check: IconWrench,
  deploy: IconPlay,
  upload: IconUpload,
  delete: IconTrash,
  list: IconFolder,
  feature: IconDraft,
  update: IconHistory,
  merge: IconPlay,
  discard: IconTrash,
  other: IconDots,
};

function Step({ step, said, working }: { step: AgentStep; said: Words; working: boolean }) {
  const Icon = ICONS[step.kind] ?? IconDots;
  const say = step.kind === 'say';

  return (
    <li className={`flex gap-3 ${say ? 'py-2' : 'py-1'}`}>
      <span className="w-9 shrink-0 pt-[3px] text-right font-mono text-[11px] tabular-nums text-slate-400">{clock(step.at)}</span>

      <span className={`mt-0.5 shrink-0 ${say ? 'text-accent-500 dark:text-accent-400' : 'text-slate-400'}`}>
        {working ? <IconSpinner className="h-4 w-4 text-accent-500" /> : <Icon className="h-4 w-4" />}
      </span>

      <div className="min-w-0 flex-1">
        <p className={say ? 'break-words text-[14px] leading-relaxed text-slate-800 dark:text-slate-200' : 'break-words text-[13px] text-slate-600 dark:text-slate-400'}>
          <StepText step={step} said={said} />
          <Marks step={step} said={said} />
        </p>

        {step.problem && <p className="mt-0.5 break-words text-xs text-red-500 dark:text-red-400">{step.problem}</p>}
      </div>
    </li>
  );
}

/** A file name set as one. */
const File = ({ name }: { name: string }) => <code className="font-mono text-[12px] text-slate-700 dark:text-slate-300">{name}</code>;

/** A few file names, and how many more there are. */
function Files({ names, said }: { names: string[]; said: Words }) {
  const shown = names.slice(0, 3);

  return (
    <>
      {shown.map((name, index) => (
        <span key={name}>
          {index > 0 && ', '}
          <File name={name} />
        </span>
      ))}
      {names.length > shown.length && <span className="text-slate-400"> {said.steps.more(names.length - shown.length)}</span>}
    </>
  );
}

/** What a step is, in the owner's words. */
function StepText({ step, said }: { step: AgentStep; said: Words }) {
  const words = said.steps;

  switch (step.kind) {
    case 'say':
      return <>{step.text}</>;
    case 'guide':
      return <>{words.guide}</>;
    case 'demos':
      return <>{words.demos}</>;
    case 'read':
      return <>{step.files?.length ? words.readFile(<File name={step.files[0]} />) : step.preview ? words.readFeature : words.read}</>;
    case 'logs':
      return <>{step.preview ? words.logsPreview : words.logs}</>;
    case 'create':
      return <>{words.create}</>;
    case 'feature':
      return <>{step.done && step.feature ? words.featureStarted(<strong className="font-medium">{step.feature}</strong>) : words.feature}</>;
    case 'update':
      return <>{step.version != null ? words.rebase(step.version) : words.update}</>;
    case 'merge':
      return <>{words.merge}</>;
    case 'discard':
      return <>{words.discard}</>;
    case 'write': {
      const files = step.files ?? [];
      const removed = step.removed ?? [];

      return (
        <>
          {files.length > 0 && (step.whole ? words.writeAll(<Files names={files} said={said} />) : words.write(<Files names={files} said={said} />))}
          {removed.length > 0 && words.removing(<Files names={removed} said={said} />)}
          {files.length === 0 && removed.length === 0 && words.write('')}
        </>
      );
    }
    case 'check':
      return <>{words.check}</>;
    case 'deploy':
      return <>{step.preview ? words.deployPreview : step.version != null && !step.done ? words.deployVersion(step.version) : words.deploy}</>;
    case 'upload':
      return <>{words.upload(<File name={step.path ?? ''} />)}</>;
    case 'delete':
      return <>{words.delete(<File name={step.path ?? ''} />)}</>;
    case 'list':
      return <>{words.list}</>;
    default:
      return <>{words.other(step.tool ?? '')}</>;
  }
}

/** A small mark after a step, for a fact that came out of it. */
function Mark({ tone, children }: { tone: 'good' | 'bad' | 'warn' | 'plain'; children: ReactNode }) {
  const colour = {
    good: 'bg-emerald-500/10 text-emerald-700 dark:text-emerald-400',
    bad: 'bg-red-500/10 text-red-600 dark:text-red-400',
    warn: 'bg-amber-500/10 text-amber-700 dark:text-amber-400',
    plain: 'bg-slate-400/10 text-slate-600 dark:text-slate-400',
  }[tone];

  return <span className={`ml-2 inline-flex whitespace-nowrap rounded-full px-1.5 py-px align-[1px] text-[11px] font-medium ${colour}`}>{children}</span>;
}

/** How a step went, once its tool has answered. */
function Marks({ step, said }: { step: AgentStep; said: Words }) {
  const marks = said.marks;

  if (!step.done || step.kind === 'say') {
    return null;
  }

  if (step.problem) {
    return <Mark tone="bad">{marks.refused}</Mark>;
  }

  const shown: ReactNode[] = [];

  if (step.version != null && (step.kind === 'write' || step.kind === 'read')) {
    shown.push(<Mark key="version" tone="plain">{marks.version(step.version)}</Mark>);
  }

  // a merge makes a version; which one a draft began from is nothing to the owner
  if (step.version != null && step.kind === 'merge' && !step.online) {
    shown.push(<Mark key="merged" tone="good">{marks.version(step.version)}</Mark>);
  }

  if (step.errors != null && step.errors > 0) {
    shown.push(<Mark key="errors" tone="bad">{marks.errors(step.errors)}</Mark>);
  } else if (step.online) {
    shown.push(
      <Mark key="online" tone="good">
        {step.preview && step.kind !== 'merge'
          ? marks.previewOnline
          : (step.kind === 'deploy' || step.kind === 'merge') && step.version != null
            ? `${marks.version(step.version)} ${marks.online}`
            : marks.online}
      </Mark>,
    );
  } else if (step.errors === 0) {
    shown.push(<Mark key="compiles" tone="good">{marks.compiles}</Mark>);
  }

  if (step.kind === 'logs' && step.problems != null) {
    shown.push(
      step.problems > 0
        ? <Mark key="problems" tone="warn">{marks.problems(step.problems)}</Mark>
        : <Mark key="clean" tone="plain">{marks.clean}</Mark>,
    );
  }

  return <>{shown}</>;
}

/* ------------------------------------------------------------ your own */

/**
 * The other way to change a lambda: an agent of the owner's own, over MCP.
 * Folded away where this installation has one, and the whole answer where it
 * does not.
 */
function OwnAgent({ control, open = false }: { control: Control; open?: boolean }) {
  const said = useEditorT().change;
  const { origin } = useOrigin();
  const editor = `${origin}${control.lambda.editorPath}`;

  const body = (
    <div className="mt-3 space-y-3">
      <p className="text-sm text-slate-600 dark:text-slate-400">{said.ownText}</p>
      <CopyField label={said.mcp} value={`${origin}/mcp`} />
      <CopyField label={said.editorLink} value={editor} />
      <pre className="overflow-x-auto bg-slate-900 p-3 font-mono text-xs text-slate-100 dark:bg-black/40">
        {`claude mcp add --transport http genhttp ${origin}/mcp`}
      </pre>
    </div>
  );

  if (open) {
    return <div className="mt-6">{body}</div>;
  }

  return (
    <details className="group border-t border-slate-200 pt-4 dark:border-ink-800">
      <summary className="cursor-pointer select-none text-[13px] text-slate-500 hover:text-slate-800 dark:hover:text-slate-200">
        {said.own}
      </summary>
      {body}
    </details>
  );
}
