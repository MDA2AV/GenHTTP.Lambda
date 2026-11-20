import { useEffect, useState } from 'react';

import type { AgentOffer } from '../api';
import { useT } from '../i18n';

/** Where the model and the effort last chosen are remembered, in this browser - never the password. */
const REMEMBERED = 'lambda-agent-choice';

/**
 * Which model the agent runs on and at which effort, as the operator offers
 * them, with the password a model may need - what the box on /build, the
 * workspace and the editor's Change section send with a message.
 *
 * Nothing is chosen until somebody chooses: the offer's defaults hold, and a
 * choice the offer no longer has falls back to them. What was chosen is
 * remembered in the browser, read once the page is there - a page rendered
 * on the server knows only the defaults.
 */
export function useAgentChoice(offer: AgentOffer | null | undefined) {
  const [model, setModel] = useState<string | null>(null);
  const [effort, setEffort] = useState<string | null>(null);
  const [password, setPassword] = useState('');

  useEffect(() => {
    try {
      const kept = JSON.parse(localStorage.getItem(REMEMBERED) ?? 'null') as { model?: string; effort?: string } | null;

      if (kept) {
        setModel(kept.model ?? null);
        setEffort(kept.effort ?? null);
      }
    } catch {
      // nothing remembered, or a browser that keeps nothing: the defaults
    }
  }, []);

  const remember = (next: { model?: string | null; effort?: string | null }) => {
    try {
      localStorage.setItem(REMEMBERED, JSON.stringify({ model: next.model ?? model, effort: next.effort ?? effort }));
    } catch {
      // a browser that keeps nothing asks again next time, which is fine
    }
  };

  const models = offer?.models ?? [];
  const chosen = models.find((m) => m.name === model) ?? models.find((m) => m.name === offer?.model) ?? models[0];
  const efforts = offer?.efforts ?? [];
  const at = effort !== null && efforts.includes(effort) ? effort : offer?.effort;
  const guarded = chosen?.password === true;

  return {
    offer,
    model: chosen?.name,
    effort: at,
    password,
    /** Whether the model chosen needs the password - and runs without a clock with it. */
    guarded,
    /** Whether a message can go with this choice: a model behind the password needs it typed. */
    ready: !guarded || password.length > 0,
    setModel: (name: string) => {
      setModel(name);
      remember({ model: name });
    },
    setEffort: (name: string) => {
      setEffort(name);
      remember({ effort: name });
    },
    setPassword,
    /** What a request carries: the choice, and the password only for a model that needs it. */
    request: { model: chosen?.name, effort: at, password: guarded ? password : undefined },
  };
}

export type AgentChoiceState = ReturnType<typeof useAgentChoice>;

/**
 * What a job runs on, as a page says it beside the job: the model by the
 * label the offer gives it - by its name where the offer no longer has it -
 * and the effort in words.
 */
export function useChoiceLabel() {
  const said = useT().workspace;

  return (offer: AgentOffer | null | undefined, model: string, effort?: string | null) => {
    const label = offer?.models.find((m) => m.name === model)?.label ?? model;

    return effort ? said.ranOn(label, said.efforts[effort as keyof typeof said.efforts] ?? effort) : label;
  };
}

/**
 * The two lists - model and effort - and the password where the model chosen
 * needs one. A list with nothing to choose from is left out, and so is all
 * of it where the operator offers one model at one effort.
 */
export function AgentChoice({ choice, className = '' }: { choice: AgentChoiceState; className?: string }) {
  const t = useT();
  const said = t.workspace;

  const models = choice.offer?.models ?? [];
  const efforts = choice.offer?.efforts ?? [];

  if (models.length <= 1 && efforts.length <= 1) {
    return null;
  }

  const list =
    'max-w-[11rem] truncate rounded border border-slate-200 bg-transparent px-1.5 py-0.5 text-xs text-slate-700 dark:border-ink-800 dark:bg-ink-950 dark:text-slate-300';

  return (
    <span className={`flex flex-wrap items-center gap-x-3 gap-y-1.5 text-xs ${className}`}>
      {models.length > 1 && (
        <label className="flex items-center gap-1.5">
          <span className="text-slate-400">{said.model}</span>
          <select value={choice.model ?? ''} onChange={(event) => choice.setModel(event.target.value)} className={list}>
            {models.map((m) => (
              <option key={m.name} value={m.name}>
                {m.password ? said.withPassword(m.label) : m.label}
              </option>
            ))}
          </select>
        </label>
      )}

      {efforts.length > 1 && (
        <label className="flex items-center gap-1.5">
          <span className="text-slate-400">{said.effort}</span>
          <select value={choice.effort ?? ''} onChange={(event) => choice.setEffort(event.target.value)} className={list}>
            {efforts.map((e) => (
              <option key={e} value={e}>
                {said.efforts[e as keyof typeof said.efforts] ?? e}
              </option>
            ))}
          </select>
        </label>
      )}

      {choice.guarded && (
        <input
          type="password"
          value={choice.password}
          onChange={(event) => choice.setPassword(event.target.value)}
          placeholder={t.buildBox.password}
          aria-label={t.buildBox.password}
          autoComplete="off"
          className="field !h-auto !w-28 !py-0.5 text-xs"
        />
      )}
    </span>
  );
}
