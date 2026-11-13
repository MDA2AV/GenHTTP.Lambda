import { useEffect, useLayoutEffect, useRef, useState, type ReactNode } from 'react';

import type { AgentStep } from '../api';
import {
  IconAlert,
  IconBook,
  IconCheck,
  IconChevronDown,
  IconDatabase,
  IconDots,
  IconDraft,
  IconEye,
  IconFolder,
  IconHistory,
  IconInfo,
  IconKey,
  IconLayers,
  IconList,
  IconPencil,
  IconPlay,
  IconPlus,
  IconSpark,
  IconSpinner,
  IconTable,
  IconTrash,
  IconUpload,
  IconWrench,
} from './Icons';

/*
 * A run of the agent of this installation, drawn the same wherever it works:
 * a change in the Change section of the editor, and a new website on /build.
 *
 * What was asked at the top, where it has got to and for how long while it
 * runs, what it did - in its own words, as it did it - and how it ended. The
 * pages bring their own words and their own idea of what an outcome is; the
 * shape is shared, so somebody who has watched one knows how to read the
 * other.
 */

/** How far a run has got, as both pages hear it from the agent. */
export interface Run {
  state: string;
  /** Its place in the queue; zero once it runs. */
  waiting: number;
  /** How long it has been running, counted by the agent. */
  seconds: number;
  /** How long it may run, in seconds; absent when there is no clock on it. */
  limit?: number | null;
}

/** Seconds as a clock reads them: 1:42, or 1:02:03 past the hour. */
export function clock(seconds: number): string {
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

/* ------------------------------------------------------------ what was asked */

/** The top of a run: what was asked, a line about it, and what can be done to it. */
export function RunHeader({ label, prompt, meta, action }: { label: string; prompt: string; meta?: ReactNode; action?: ReactNode }) {
  return (
    <header className="flex items-start gap-3 border-b border-slate-200 px-4 py-3 dark:border-ink-800">
      <div className="min-w-0 flex-1">
        <div className="text-xs font-medium uppercase tracking-wide text-slate-500">{label}</div>
        <p className="mt-1 whitespace-pre-line break-words text-[15px] leading-relaxed">{prompt}</p>
        {meta && <p className="mt-1.5 text-xs text-slate-500">{meta}</p>}
      </div>

      {action}
    </header>
  );
}

/* ------------------------------------------------------------ under way */

export interface ProgressWords {
  queued: (ahead: number) => string;
  starting: string;
  leaveOpen: string;
}

/**
 * Where a running run has got to: in the queue, or at work - on what, for how
 * long, and how much of its time is gone.
 *
 * The seconds are the agent's, carried on here from the moment they arrived
 * (received), so a browser whose clock is wrong still counts the right ones.
 */
export function RunProgress({ run, received, doing, words }: { run: Run; received: number; doing: ReactNode; words: ProgressWords }) {
  const now = useNow(run.state === 'running');

  const elapsed = run.state === 'running' ? run.seconds + Math.max(0, (now - received) / 1000) : 0;
  const share = run.limit ? Math.min(1, elapsed / run.limit) : 0;

  return (
    <div className="px-4 py-3">
      <div className="flex items-center gap-2.5 text-sm">
        <IconSpinner className={`h-4 w-4 shrink-0 ${run.state === 'queued' ? 'text-amber-500' : 'text-accent-500'}`} />
        <span role="status" className="min-w-0 flex-1 truncate font-medium">
          {run.state === 'queued' ? (run.waiting > 0 ? words.queued(run.waiting) : words.starting) : doing}
        </span>
        {run.state === 'running' && (
          <span className="shrink-0 font-mono text-xs tabular-nums text-slate-500">
            {clock(elapsed)}
            {run.limit ? <span className="text-slate-400"> / {clock(run.limit)}</span> : null}
          </span>
        )}
      </div>

      {run.state === 'running' && run.limit ? (
        <div className="mt-2.5 h-0.5 w-full bg-slate-200 dark:bg-ink-800" aria-hidden="true">
          <div className="h-full bg-accent-500 transition-[width] duration-1000 ease-linear dark:bg-accent-400" style={{ width: `${share * 100}%` }} />
        </div>
      ) : null}

      <p className="mt-2 text-xs text-slate-500">{words.leaveOpen}</p>
    </div>
  );
}

/* ------------------------------------------------------------ how it ended */

export type Tone = 'good' | 'ready' | 'warn' | 'bad' | 'quiet';

const TONES: Record<Tone, { box: string; icon: string }> = {
  good: { box: 'bg-emerald-500/5', icon: 'text-emerald-600 dark:text-emerald-400' },
  ready: { box: 'bg-accent-500/5', icon: 'text-accent-500 dark:text-accent-400' },
  warn: { box: 'bg-amber-500/5', icon: 'text-amber-600 dark:text-amber-400' },
  bad: { box: 'bg-red-500/5', icon: 'text-red-500 dark:text-red-400' },
  quiet: { box: '', icon: 'text-slate-400' },
};

/**
 * How a run ended: a line and its notes, what the agent said at the end, and
 * what to do next. What the line is comes from the page, which reads it off
 * the facts the agent collected rather than off what the model said.
 */
export function RunOutcome({
  tone,
  headline,
  notes = [],
  summary,
  detail,
  detailLabel,
  children,
}: {
  tone: Tone;
  headline: string;
  notes?: string[];
  summary?: string | null;
  detail?: string | null;
  detailLabel?: string;
  children?: ReactNode;
}) {
  const look = TONES[tone];
  const Icon = tone === 'good' || tone === 'ready' ? IconCheck : tone === 'quiet' ? IconInfo : IconAlert;

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

        {summary && <p className="mt-3 whitespace-pre-line break-words text-[15px] leading-relaxed text-slate-800 dark:text-slate-200">{summary}</p>}

        {detail && !summary && (
          <details className="mt-2 text-xs text-slate-500">
            <summary className="cursor-pointer select-none">{detailLabel}</summary>
            <pre className="mt-1 whitespace-pre-wrap break-words font-mono">{detail}</pre>
          </details>
        )}

        {children && <div className="mt-4 flex flex-wrap items-center gap-2">{children}</div>}
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
 * Which steps are shown is the page's choice: a page for people who do not
 * write code passes only what the agent said ("Looking at how the scores are
 * stored" means something to them, "Changing lambda.cs" does not).
 */
export function RunLog({
  steps,
  running,
  label,
  count,
  describe,
}: {
  steps: AgentStep[];
  running: boolean;
  label: string;
  /** What the folded log says it holds. */
  count: number;
  /** A step in the page's words. */
  describe: (step: AgentStep) => ReactNode;
}) {
  const list = useRef<HTMLOListElement>(null);
  const [open, setOpen] = useState(running);

  // it was open while it ran; once it ends, the outcome takes over
  useEffect(() => setOpen(running), [running]);

  // new steps arrive at the bottom, so the list follows them - unless the
  // reader has scrolled up to read something, which it then leaves alone
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
        <span className="font-medium">{label}</span>
        {!open && <span className="tabular-nums text-slate-400">{count}</span>}
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
            <Step key={index} step={step} working={index === current} describe={describe} />
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
  data: IconDatabase,
  records: IconTable,
  secrets: IconKey,
  other: IconDots,
};

function Step({ step, working, describe }: { step: AgentStep; working: boolean; describe: (step: AgentStep) => ReactNode }) {
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
          {describe(step)}
        </p>

        {step.problem && <p className="mt-0.5 break-words text-xs text-red-500 dark:text-red-400">{step.problem}</p>}
      </div>
    </li>
  );
}
