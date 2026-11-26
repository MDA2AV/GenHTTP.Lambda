import { useSyncExternalStore } from 'react';

import type { AgentStep, BuildResult, ChangeResult, ChangeState, RunFile } from '../api';

/*
 * The apps this browser knows, and what was said about each - the workspace
 * on /build, which has no accounts behind it.
 *
 * An app is known by its editor key, which is the only thing that opens it:
 * remembered when it is built here, or when somebody adds it with its editor
 * link. Kept in the browser's local storage on every installation - decided
 * by the owner - although a script that runs on the platform's own origin can
 * read it there: a draft's preview at /features/…, and every lambda of an
 * installation that answers by path. The README says so for path mode.
 *
 * The conversation is kept here as well, since the agent keeps nothing
 * between messages and the runner forgets a job after an hour: what was
 * asked, what the agent answered and what came of it, turn by turn. The next
 * message carries the newest turns along, so the agent understands "and last
 * month?".
 */

/** One message to the agent, and what came of it. */
export interface Turn {
  /** The job's id at the agent, by which a running one is followed again after a reload. */
  id: string;
  /** Whether it built the app, or was a message about it afterwards. */
  kind: 'build' | 'change';
  /** When it was asked, as milliseconds since the epoch. */
  at: number;
  prompt: string;
  state: ChangeState;
  /** What the agent said as it worked, its own lines only, kept once it is done. */
  said?: string[];
  /** How a message about the app ended. */
  result?: ChangeResult | null;
  /** How a build ended. */
  built?: BuildResult | null;
  /** How long it ran, in seconds. */
  seconds?: number;
  /** The files it was asked with. */
  attachments?: RunFile[];
  /** The files the agent handed back. */
  files?: RunFile[];
  /** The earlier turn it carries on from: the one whose question it answers, or whose wait is over. */
  continues?: string;
}

/** An app this browser knows. */
export interface KnownApp {
  privateKey: string;
  publicKey: string;
  /** When it became known here, and when it was last opened - for the order of the list. */
  added: number;
  opened: number;
  /** What its documentation says it is, as last read, to name it in the list before it is read again. */
  about?: string | null;
  /** Whether it was online when it was last read. */
  online?: boolean;
  turns: Turn[];
}

/** A build asked for here that has not ended yet, to be followed again after a reload. */
export interface PendingBuild {
  id: string;
  prompt: string;
  at: number;
  model?: string;
}

interface Kept {
  apps: KnownApp[];
  pending?: PendingBuild | null;
}

/** The key everything is kept under. */
const KEY = 'lambda-workspace';

/** How many turns of a conversation are kept per app, the newest ones. */
const KEPT_TURNS = 60;

/** How many apps are remembered at most, the most recently opened ones. */
const KEPT_APPS = 200;

/** How long a build is followed again after a reload: the agent forgets one after an hour. */
const PENDING_FOR = 60 * 60 * 1000;

const EMPTY: Kept = { apps: [] };

let cached: { raw: string | null; value: Kept } | null = null;

const listeners = new Set<() => void>();

function read(): Kept {
  let raw: string | null = null;

  try {
    raw = localStorage.getItem(KEY);
  } catch {
    // a browser that keeps nothing knows no apps - and loses none it was not told about
  }

  if (cached && cached.raw === raw) {
    return cached.value;
  }

  let value = EMPTY;

  try {
    const parsed = raw ? (JSON.parse(raw) as Partial<Kept>) : null;

    if (parsed && Array.isArray(parsed.apps)) {
      value = {
        apps: parsed.apps.filter((app) => typeof app?.privateKey === 'string' && typeof app?.publicKey === 'string')
          .map((app) => ({ ...app, turns: Array.isArray(app.turns) ? app.turns : [] })),
        pending: parsed.pending ?? null,
      };
    }
  } catch {
    // something else wrote there; it is replaced with the next change
  }

  cached = { raw, value };

  return value;
}

function write(change: (kept: Kept) => Kept) {
  const next = change(read());

  try {
    localStorage.setItem(KEY, JSON.stringify(next));
  } catch {
    // full, or not allowed: the page still works for as long as it is open
    cached = { raw: cached?.raw ?? null, value: next };
  }

  listeners.forEach((listener) => listener());
}

function subscribe(listener: () => void) {
  listeners.add(listener);

  // another tab added an app, or the conversation went on there
  const onStorage = (event: StorageEvent) => event.key === KEY && listener();
  window.addEventListener('storage', onStorage);

  return () => {
    listeners.delete(listener);
    window.removeEventListener('storage', onStorage);
  };
}

/**
 * What this browser knows - nothing while a prerendered page is taken over,
 * since the page was rendered without it, and what is kept right after.
 */
export function useWorkspace(): Kept {
  return useSyncExternalStore(subscribe, read, () => EMPTY);
}

/** The apps, the most recently opened first. */
export function sorted(apps: KnownApp[]): KnownApp[] {
  return [...apps].sort((a, b) => b.opened - a.opened);
}

/** Remembers an app, or - known already - brings it to the top of the list. */
export function remember(app: { privateKey: string; publicKey: string; about?: string | null; online?: boolean }, turn?: Turn) {
  const now = Date.now();

  write((kept) => {
    const known = kept.apps.find((candidate) => candidate.privateKey === app.privateKey);

    const updated: KnownApp = known
      ? { ...known, ...definedOf(app), opened: now, turns: turn ? [...known.turns.filter((was) => was.id !== turn.id), turn] : known.turns }
      : { ...app, added: now, opened: now, turns: turn ? [turn] : [] };

    const others = kept.apps.filter((candidate) => candidate.privateKey !== app.privateKey);

    return { ...kept, apps: sorted([updated, ...others]).slice(0, KEPT_APPS) };
  });
}

/**
 * Brings an app to the top of the list: something was asked of it. Opening
 * one does not, or the list would rearrange itself under every click.
 */
export function touch(privateKey: string) {
  write((kept) => ({
    ...kept,
    apps: sorted(kept.apps.map((app) => (app.privateKey === privateKey ? { ...app, opened: Date.now() } : app))),
  }));
}

/** What is known about an app, as it was read again: its public key may have changed, its description too. */
export function describe(privateKey: string, facts: { publicKey?: string; about?: string | null; online?: boolean }) {
  write((kept) => ({
    ...kept,
    apps: kept.apps.map((app) => (app.privateKey === privateKey ? { ...app, ...definedOf(facts) } : app)),
  }));
}

/** Forgets an app here. It stays online, and its editor link still opens it. */
export function forget(privateKey: string) {
  write((kept) => ({ ...kept, apps: kept.apps.filter((app) => app.privateKey !== privateKey) }));
}

/** Adds a turn to the conversation of an app, or replaces the one with its id. */
export function keepTurn(privateKey: string, turn: Turn) {
  write((kept) => ({
    ...kept,
    apps: kept.apps.map((app) => {
      if (app.privateKey !== privateKey) {
        return app;
      }

      const at = app.turns.findIndex((candidate) => candidate.id === turn.id);
      const turns = at >= 0 ? app.turns.map((candidate, index) => (index === at ? turn : candidate)) : [...app.turns, turn];

      return { ...app, turns: turns.slice(-KEPT_TURNS) };
    }),
  }));
}

/** Notes a build that was asked for, until it ends. */
export function pend(build: PendingBuild | null) {
  write((kept) => ({ ...kept, pending: build }));
}

/** The build asked for here that may still be running, if the agent can still say how it went. */
export function pendingOf(kept: Kept): PendingBuild | null {
  const pending = kept.pending;

  return pending && Date.now() - pending.at < PENDING_FOR ? pending : null;
}

/** Whether a turn is still waiting for the agent, or being worked on. */
export const activeState = (state: string) => state === 'queued' || state === 'running';

/** The lines the agent wrote as it worked, to keep with a turn once it is done. */
export function saidIn(steps: AgentStep[]): string[] {
  return steps.filter((step) => step.kind === 'say' && step.text).map((step) => step.text!);
}

function definedOf<T extends object>(facts: T): Partial<T> {
  return Object.fromEntries(Object.entries(facts).filter(([, value]) => value !== undefined)) as Partial<T>;
}

/**
 * The editor key in what somebody pasted: an editor link of any installation,
 * or the key alone. Keys are lower case letters and digits, as the platform
 * makes them; nothing is asked of the server here.
 */
export function keyIn(pasted: string): string | null {
  const text = pasted.trim();

  const linked = /\/editor\/([a-z0-9-]{8,64})(?:[/?#]|$)/i.exec(text);
  const bare = /^[a-z0-9-]{8,64}$/i.exec(text);

  const key = linked?.[1] ?? bare?.[0];

  return key ? key.toLowerCase() : null;
}
