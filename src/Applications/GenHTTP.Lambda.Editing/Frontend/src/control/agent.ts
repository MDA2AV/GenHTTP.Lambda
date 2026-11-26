import { useCallback, useEffect, useRef, useState } from 'react';

import { ApiError, api, followRun, isActive, type AgentState, type ChangeJob, type ChangeRequest, type UndoOutcome } from '@genhttp-lambda/kit/api';

/**
 * The agent changing this lambda, as the whole control center knows it.
 *
 * Held by the frame rather than by the Change section, because a change takes
 * minutes and nobody should have to sit on one section to see it through: the
 * sidebar marks it while it runs, the other sections say so, and when it ends
 * the lambda is read again wherever the owner happens to be - so the version
 * it made is in the history and, where it went online, in the badge.
 *
 * Nothing about the change is kept in the browser. The platform keeps the
 * runs, so a reload, a second tab or another device all find the same one -
 * read once as the page opens, then pushed to it as it goes on, wherever it
 * was asked.
 */
export interface AgentControl {
  /** Null until the first answer, and for a demo, which nobody changes. */
  state: AgentState | null;
  /** When that answer arrived, to count the seconds of a running change from. */
  received: number;
  failure: string | null;
  /** Whether a change ended while the owner was somewhere else in the control center. */
  unseen: boolean;
  /** Asks for a change; answers with why it was refused, or null. */
  start: (change: ChangeRequest) => Promise<string | null>;
  /** Stops the change under way; answers with why that did not work, or null. */
  stop: () => Promise<string | null>;
  /** Undoes what a change did, where it can; answers with what was done, or why not. */
  undo: (run: string) => Promise<UndoOutcome | string>;
  /** The owner has looked at how the last change ended. */
  seen: () => void;
}

/**
 * How many things a change has done so far that the page shows: versions
 * saved or put online, and features started, saved, tried, merged or deleted.
 */
const milestones = (job?: ChangeJob | null) =>
  (job?.steps ?? []).filter((step) =>
    step.done && (
      ((step.kind === 'write' || step.kind === 'deploy') && (step.version != null || step.preview))
      || step.kind === 'feature' || step.kind === 'update' || step.kind === 'merge' || step.kind === 'discard'
    )).length;

export function useAgent({
  privateKey,
  enabled,
  watching,
  refresh,
  onFinished,
  failed,
}: {
  privateKey: string;
  /** Off for a demo, and until the lambda is known. */
  enabled: boolean;
  /** Whether the Change section is open, where an ending needs no marking. */
  watching: boolean;
  /** Reads the lambda again. */
  refresh: () => Promise<void>;
  /** Said once, when a change this page watched run comes to an end. */
  onFinished: (job: ChangeJob) => void;
  /** What to say when the agent could not be asked. */
  failed: string;
}): AgentControl {
  const [state, setState] = useState<AgentState | null>(null);
  const [received, setReceived] = useState(0);
  const [failure, setFailure] = useState<string | null>(null);
  const [unseen, setUnseen] = useState(false);

  /** Bumped to read and listen again, after a change was refused. */
  const [round, setRound] = useState(0);

  const last = useRef<ChangeJob | null>(null);

  // read through refs, so polling does not start over whenever the frame
  // hands down a new function
  const hooks = useRef({ refresh, onFinished, watching });
  hooks.current = { refresh, onFinished, watching };

  const take = useCallback((next: AgentState) => {
    const before = last.current;
    const job = next.job ?? null;

    last.current = job;

    setState(next);
    setReceived(Date.now());
    setFailure(null);

    // a change first seen here is news to nobody: it was running, or ended,
    // before this page looked
    if (!before || !job || before.id !== job.id) {
      return;
    }

    // a version saved or put online changes what the rest of the page shows,
    // so it is read now rather than on the next tick of the frame
    if (milestones(job) > milestones(before)) {
      hooks.current.refresh().catch(() => undefined);
    }

    if (isActive(before) && !isActive(job)) {
      hooks.current.refresh().catch(() => undefined);
      hooks.current.onFinished(job);

      if (!hooks.current.watching) {
        setUnseen(true);
      }
    }
  }, []);

  useEffect(() => {
    if (!enabled) {
      return;
    }

    let alive = true;
    let timer: number | undefined;
    let close: (() => void) | undefined;
    let patience = 10_000;

    const later = () => {
      if (!alive) return;

      window.clearTimeout(timer);
      timer = window.setTimeout(begin, patience);
      patience = Math.min(patience * 2, 120_000);
    };

    // read once, and again when the section opens or the tab is looked at;
    // while a change is under way, told what happens as it happens - and
    // only then, since a browser keeps six connections to a site for every tab
    const begin = async () => {
      try {
        const next = await api.agent.state(privateKey);

        if (!alive) {
          return;
        }

        take(next);
        patience = 10_000;

        if (isActive(next.job)) {
          close?.();
          close = followRun<AgentState>(api.agent.events(privateKey), 'state', (state) => alive && take(state), (state) => !isActive(state.job), later);
        }
      } catch (error) {
        if (!alive) return;

        setFailure(error instanceof ApiError ? error.message : failed);

        // not there, or not to be read: asking again changes nothing
        if (!(error instanceof ApiError && [400, 403, 404].includes(error.status))) {
          later();
        }
      }
    };

    const onLook = () => {
      if (document.visibilityState === 'visible') void begin();
    };

    void begin();
    document.addEventListener('visibilitychange', onLook);

    return () => {
      alive = false;
      window.clearTimeout(timer);
      close?.();
      document.removeEventListener('visibilitychange', onLook);
    };
    // opening the section reads again, for an allowance that is up to date
  }, [privateKey, enabled, take, round, watching, failed]);

  const start = useCallback(
    async (change: ChangeRequest) => {
      try {
        take(await api.agent.start(privateKey, change));
        setUnseen(false);

        // under way now: followed as it goes on
        setRound((n) => n + 1);
        return null;
      } catch (error) {
        // most likely one already under way, asked for elsewhere: show it
        setRound((n) => n + 1);
        return error instanceof ApiError ? error.message : failed;
      }
    },
    [privateKey, take, failed],
  );

  const stop = useCallback(async () => {
    try {
      take(await api.agent.stop(privateKey));
      return null;
    } catch (error) {
      return error instanceof ApiError ? error.message : failed;
    }
  }, [privateKey, take, failed]);

  const undo = useCallback(
    async (run: string) => {
      try {
        const outcome = await api.agent.undo(privateKey, run);
        await hooks.current.refresh().catch(() => undefined);
        take(await api.agent.state(privateKey));
        return outcome;
      } catch (error) {
        return error instanceof ApiError ? error.message : failed;
      }
    },
    [privateKey, take, failed],
  );

  const seen = useCallback(() => setUnseen(false), []);

  return { state: enabled ? state : null, received, failure, unseen, start, stop, undo, seen };
}
