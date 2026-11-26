import { useEffect, useLayoutEffect, useRef, useState, type ReactNode } from 'react';

import { api, type AgentStep, type RunBlocker, type RunFile, type RunWait } from '../api';
import { bytes } from '../ui/format';
import {
  IconAlert,
  IconBook,
  IconCheck,
  IconChevronDown,
  IconDatabase,
  IconDots,
  IconDraft,
  IconDownload,
  IconExternal,
  IconEye,
  IconFile,
  IconFolder,
  IconGlobe,
  IconHistory,
  IconInfo,
  IconKey,
  IconLayers,
  IconList,
  IconPaperclip,
  IconPencil,
  IconPlay,
  IconPlus,
  IconSearch,
  IconSpark,
  IconSpinner,
  IconStar,
  IconTable,
  IconTrash,
  IconUpload,
  IconWrench,
} from './Icons';

/*
 * A run of the agent of this installation, drawn the same wherever it works:
 * a change in the Change section of the editor, and a new app on /build.
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
  /** What it waits for while nobody works on it, where the platform knows. */
  wait?: RunWait | null;
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
  /** What a run waits for besides its turn: the service behind the agent, its account's limit, room being made. */
  waits?: { overloaded: string; rateLimited: string; preparing: string; unreachable: string; other: string };
  /** Until when it waits, where the agent says: "until 14:05". */
  until?: (at: Date) => string;
}

/** What a run waits for, in the page's words, or nothing while it waits for nothing but its turn. */
function waitingFor(wait: RunWait | null | undefined, words: ProgressWords): string | null {
  if (!wait || wait.reason === 'queue' || !words.waits) {
    return null;
  }

  const what = {
    overloaded: words.waits.overloaded,
    'rate-limited': words.waits.rateLimited,
    preparing: words.waits.preparing,
    unreachable: words.waits.unreachable,
    other: words.waits.other,
  }[wait.reason] ?? words.waits.other;

  const until = wait.until ? new Date(wait.until) : null;

  return until && words.until && until.getTime() > Date.now() ? `${what} ${words.until(until)}` : what;
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

  // waiting for something other than its turn - the service behind the agent,
  // a limit of its account - is said as that, whether it started or not
  const waiting = waitingFor(run.wait, words);
  const ahead = run.wait?.position ?? run.waiting;

  return (
    <div className="px-4 py-3">
      <div className="flex items-center gap-2.5 text-sm">
        <IconSpinner className={`h-4 w-4 shrink-0 ${run.state === 'queued' || waiting ? 'text-amber-500' : 'text-accent-500'}`} />
        <span role="status" className={`min-w-0 flex-1 font-medium ${waiting ? '' : 'truncate'}`}>
          {waiting ?? (run.state === 'queued' ? (ahead > 0 ? words.queued(ahead) : words.starting) : doing)}
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

/** How a run ended; an answer is what the agent said to a question, with no verdict of its own. */
export type Tone = 'good' | 'ready' | 'warn' | 'bad' | 'quiet' | 'answer';

const TONES: Record<Tone, { box: string; icon: string }> = {
  good: { box: 'bg-emerald-500/5', icon: 'text-emerald-600 dark:text-emerald-400' },
  ready: { box: 'bg-accent-500/5', icon: 'text-accent-500 dark:text-accent-400' },
  warn: { box: 'bg-amber-500/5', icon: 'text-amber-600 dark:text-amber-400' },
  bad: { box: 'bg-red-500/5', icon: 'text-red-500 dark:text-red-400' },
  quiet: { box: '', icon: 'text-slate-400' },
  answer: { box: '', icon: 'text-accent-500 dark:text-accent-400' },
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
  icon = true,
  children,
}: {
  tone: Tone;
  /** The verdict, read off the facts; left out where what the agent said is all there is to say. */
  headline?: string | null;
  notes?: string[];
  summary?: ReactNode;
  detail?: string | null;
  detailLabel?: string;
  /** Whether the verdict has its mark: a page that marks the agent's side of a conversation already leaves it out. */
  icon?: boolean;
  children?: ReactNode;
}) {
  const look = TONES[tone];
  const Icon = tone === 'good' || tone === 'ready' ? IconCheck : tone === 'answer' ? IconSpark : tone === 'quiet' ? IconInfo : IconAlert;

  return (
    <div className={`flex gap-3 px-4 py-4 ${look.box}`}>
      {icon && <Icon className={`mt-0.5 h-5 w-5 shrink-0 ${look.icon}`} />}

      <div className="min-w-0 flex-1">
        {headline && <p className="text-[15px] font-semibold">{headline}</p>}

        {notes.map((note) => (
          <p key={note} className="mt-1 text-sm text-slate-600 dark:text-slate-400">
            {note}
          </p>
        ))}

        {summary && (
          <div className={`${headline || notes.length > 0 ? 'mt-3' : ''} whitespace-pre-line break-words text-[15px] leading-relaxed text-slate-800 dark:text-slate-200`}>
            {summary}
          </div>
        )}

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

/* ------------------------------------------------------------ files */

/**
 * The files that went with a run - those somebody attached, or those the
 * agent handed back: a picture shown small, any other file by its name and
 * size, each to open or save. They are read from the platform's API, which
 * shows a picture and has every other file saved, never run.
 */
export function RunFiles({ files, label, download }: { files: RunFile[]; label: string; download: string }) {
  if (files.length === 0) {
    return null;
  }

  return (
    <div className="border-t border-slate-200 px-4 py-3 dark:border-ink-800">
      <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{label}</p>
      <FileList files={files} download={download} className="mt-2" />
    </div>
  );
}

/** Files as a row of small cards: pictures as themselves, the rest by name. */
export function FileList({ files, download, className = '', onRemove, remove }: {
  files: RunFile[];
  download: string;
  className?: string;
  /** Takes one off a message not sent yet. */
  onRemove?: (file: RunFile) => void;
  remove?: string;
}) {
  return (
    <ul className={`flex flex-wrap gap-2 ${className}`}>
      {files.map((file) => {
        const url = api.attachments.url(file.id);
        const picture = /^image\/(png|jpeg|gif|webp)$/.test(file.type);

        return (
          <li key={file.id} className="group relative max-w-full">
            <a
              href={url}
              target="_blank"
              rel="noreferrer"
              download={picture ? undefined : file.name}
              title={file.description ?? file.name}
              className="flex max-w-[16rem] items-center gap-2 rounded-md border border-slate-200 bg-white px-2 py-1.5 text-[13px] transition-colors hover:border-slate-300 dark:border-ink-800 dark:bg-ink-950 dark:hover:border-ink-700"
            >
              {picture ? (
                <img
                  src={url}
                  alt=""
                  loading="lazy"
                  className="h-9 w-9 shrink-0 rounded object-cover"
                  // gone with its run, a while after it ended
                  onError={(event) => {
                    event.currentTarget.style.display = 'none';
                  }}
                />
              ) : (
                <IconFile className="h-4 w-4 shrink-0 text-slate-400" />
              )}
              <span className="min-w-0">
                <span className="block truncate font-medium text-slate-700 dark:text-slate-300">{file.name}</span>
                <span className="block truncate text-[11px] text-slate-500">{file.description ?? bytes(file.size)}</span>
              </span>
              {!onRemove && !picture && <IconDownload className="h-3.5 w-3.5 shrink-0 text-slate-400" aria-label={download} />}
            </a>

            {onRemove && (
              <button
                type="button"
                onClick={() => onRemove(file)}
                aria-label={remove ? `${remove}: ${file.name}` : file.name}
                title={remove}
                className="absolute -right-2 -top-2 flex h-6 w-6 items-center justify-center rounded-full bg-slate-700 text-[12px] leading-none text-white hover:bg-slate-900 dark:bg-slate-300 dark:text-ink-950"
              >
                ×
              </button>
            )}
          </li>
        );
      })}
    </ul>
  );
}

/* ------------------------------------------------------------ what it waits for */

export interface BlockerWords {
  /** Waiting for the operator to decide on connecting to other services. */
  connectivity: string;
  /** Waiting for keys and passwords to be entered, by name. */
  keys: (names: string[]) => string;
}

/**
 * What the app waits for after a run, and whom - read off the platform's
 * records rather than off what the agent said - with what the page offers to
 * go on once it has happened.
 */
export function RunBlockers({ blockers, words, children }: { blockers: RunBlocker[]; words: BlockerWords; children?: ReactNode }) {
  if (blockers.length === 0) {
    return null;
  }

  return (
    <div className="border-t border-amber-500/30 bg-amber-500/5 px-4 py-3">
      <ul className="space-y-1.5">
        {blockers.map((blocker) => (
          <li key={`${blocker.who}-${blocker.what}`} className="flex items-start gap-2 text-sm text-slate-700 dark:text-slate-300">
            {blocker.what === 'keys' ? (
              <IconKey className="mt-0.5 h-4 w-4 shrink-0 text-amber-600 dark:text-amber-400" />
            ) : (
              <IconGlobe className="mt-0.5 h-4 w-4 shrink-0 text-amber-600 dark:text-amber-400" />
            )}
            <span>{blocker.what === 'keys' ? words.keys(blocker.names ?? []) : words.connectivity}</span>
          </li>
        ))}
      </ul>

      {children && <div className="mt-3 flex flex-wrap items-center gap-2">{children}</div>}
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
  connectivity: IconGlobe,
  analytics: IconEye,
  source: IconStar,
  web: IconExternal,
  search: IconSearch,
  attachment: IconPaperclip,
  showcase: IconStar,
  file: IconFile,
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
