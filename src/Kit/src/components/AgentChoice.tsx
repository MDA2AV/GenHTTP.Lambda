import { useEffect, useState } from 'react';

import type { AgentOffer, OfferedOption } from '../api';
import { useT } from '../i18n';

/** Where the values last chosen are remembered, in this browser - never the password. */
const REMEMBERED = 'lambda-agent-choice';

/**
 * The options of the agent as the offer lists them. An offer from before the
 * options were the integration's has only models and efforts, which are the
 * options model and effort.
 */
export function optionsOf(offer: AgentOffer | null | undefined): OfferedOption[] {
  if (!offer) {
    return [];
  }

  if (offer.options) {
    return offer.options;
  }

  const options: OfferedOption[] = [];

  if (offer.models.length > 0) {
    options.push({ name: 'model', label: 'Model', values: offer.models, default: offer.model ?? offer.models[0].name });
  }

  if (offer.efforts.length > 0) {
    options.push({
      name: 'effort',
      label: 'Effort',
      values: offer.efforts.map((name) => ({ name, label: name, password: false })),
      default: offer.effort ?? offer.efforts[0],
    });
  }

  return options;
}

/** What was remembered: the values by option - or, kept before there were options, a model and an effort. */
function remembered(): Record<string, string> {
  try {
    const kept = JSON.parse(localStorage.getItem(REMEMBERED) ?? 'null') as { options?: Record<string, string>; model?: string; effort?: string } | null;

    if (!kept) {
      return {};
    }

    const values: Record<string, string> = { ...(kept.options ?? {}) };

    if (kept.model && !values.model) values.model = kept.model;
    if (kept.effort && !values.effort) values.effort = kept.effort;

    return values;
  } catch {
    // nothing remembered, or a browser that keeps nothing: the defaults
    return {};
  }
}

/**
 * The values the agent runs on - one per option its integration has, as the
 * operator offers them - with the password a value may need: what the box on
 * /build, the workspace and the editor's Change section send with a message.
 *
 * Nothing is chosen until somebody chooses: the offer's defaults hold, and a
 * choice the offer no longer has falls back to them. What was chosen is
 * remembered in the browser, read once the page is there - a page rendered
 * on the server knows only the defaults.
 */
export function useAgentChoice(offer: AgentOffer | null | undefined) {
  const [picked, setPicked] = useState<Record<string, string>>({});
  const [password, setPassword] = useState('');

  useEffect(() => setPicked(remembered()), []);

  const options = optionsOf(offer);

  // the value of each option: the one picked, where the offer still has it, or its default
  const values = Object.fromEntries(
    options.map((option) => {
      const kept = picked[option.name];

      return [option.name, option.values.some((v) => v.name === kept) ? kept : option.default];
    }),
  );

  const guarded = options.some((option) => option.values.find((v) => v.name === values[option.name])?.password === true);

  return {
    offer,
    options,
    values,
    password,
    /** Whether a value chosen needs the password - and runs on the limits the operator set for such runs. */
    guarded,
    /** Whether a message can go with this choice: a value behind the password needs it typed. */
    ready: !guarded || password.length > 0,
    choose: (option: string, value: string) => {
      setPicked((was) => {
        const next = { ...was, [option]: value };

        try {
          localStorage.setItem(REMEMBERED, JSON.stringify({ options: next, model: next.model, effort: next.effort }));
        } catch {
          // a browser that keeps nothing asks again next time, which is fine
        }

        return next;
      });
    },
    setPassword,
    /** What a request carries: the values, and the password only where one needs it. */
    request: { options: values, password: guarded ? password : undefined },
  };
}

export type AgentChoiceState = ReturnType<typeof useAgentChoice>;

/**
 * What a run ran on, as a page says it beside it: each value by the label the
 * offer gives it - by its name where the offer no longer has it - in the
 * order of the offer's options. The labels are the integration's own, the
 * same in every language.
 */
export function useChoiceLabel() {
  return (offer: AgentOffer | null | undefined, values: Record<string, string> | null | undefined) => {
    if (!values) {
      return '';
    }

    const options = optionsOf(offer);
    const order = [...options.map((o) => o.name), ...Object.keys(values).filter((name) => !options.some((o) => o.name === name))];

    return order
      .filter((name) => values[name])
      .map((name) => options.find((o) => o.name === name)?.values.find((v) => v.name === values[name])?.label ?? values[name])
      .join(' · ');
  };
}

/**
 * A list per option that has something to choose from, and the password
 * where a value chosen needs one. Nothing at all where the operator offers
 * one value of each.
 */
export function AgentChoice({ choice, className = '' }: { choice: AgentChoiceState; className?: string }) {
  const t = useT();
  const said = t.workspace;

  const choosable = choice.options.filter((option) => option.values.length > 1);

  if (choosable.length === 0) {
    return null;
  }

  const list =
    'max-w-[11rem] truncate rounded border border-slate-200 bg-transparent px-1.5 py-0.5 text-xs text-slate-700 dark:border-ink-800 dark:bg-ink-950 dark:text-slate-300';

  return (
    <span className={`flex flex-wrap items-center gap-x-3 gap-y-1.5 text-xs ${className}`}>
      {choosable.map((option) => (
        <label key={option.name} className="flex items-center gap-1.5">
          <span className="text-slate-400">{option.label}</span>
          <select value={choice.values[option.name] ?? ''} onChange={(event) => choice.choose(option.name, event.target.value)} className={list}>
            {option.values.map((value) => (
              <option key={value.name} value={value.name}>
                {value.password ? said.withPassword(value.label) : value.label}
              </option>
            ))}
          </select>
        </label>
      ))}

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
