import { useCallback, useEffect, useRef, useState } from 'react';

import { ApiError, api, isActive, type AgentState, type ChangeJob, type ChangeRequest } from '../api';

/**
 * The agent changing this lambda, as the whole control center knows it.
 *
 * Held by the frame rather than by the Change section, because a change takes
 * minutes and nobody should have to sit on one section to see it through: the
 * sidebar marks it while it runs, the other sections say so, and when it ends
 * the lambda is read again wherever the owner happens to be - so the version
 * it made is in the history and, where it went online, in the badge.
 *
 * Nothing about the change is kept in the browser. The server asks the agent
 * for the change of this lambda, so a reload, a second tab or another device
 * all find the same one.
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

  /** Bumped to start polling again, after a change is asked for. */
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

    const tick = async () => {
      try {
        const next = await api.agent.state(privateKey);

        if (!alive) {
          return;
        }

        take(next);
      } catch (error) {
        if (alive) {
          setFailure(error instanceof ApiError ? error.message : failed);
        }
      }

      if (!alive) {
        return;
      }

      // only a change that is still waiting or working is worth asking after;
      // a hidden tab asks less often, but still notices it ending
      if (isActive(last.current)) {
        timer = window.setTimeout(tick, document.visibilityState === 'visible' ? 2000 : 10_000);
      }
    };

    void tick();

    return () => {
      alive = false;
      window.clearTimeout(timer);
    };
    // opening the section asks again, for an allowance that is up to date
  }, [privateKey, enabled, take, round, watching, failed]);

  const start = useCallback(
    async (change: ChangeRequest) => {
      try {
        take(await api.agent.start(privateKey, change));
        setUnseen(false);
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

  const seen = useCallback(() => setUnseen(false), []);

  return { state: enabled ? state : null, received, failure, unseen, start, stop, seen };
}
