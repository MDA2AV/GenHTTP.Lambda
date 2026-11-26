import { useCallback, useEffect, useState } from 'react';

import {
  ApiError,
  refusedToken,
  api,
  type AdminAgentChange,
  type AdminAgentOption,
  type AdminAgentRun,
  type AdminAgentSettings,
  type AgentLimits,
  type AgentRunner,
  type AgentUsage,
  type ModelAccess,
} from '@genhttp-lambda/kit/api';
import { IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { bytes, Card, count, duration, Fact, Facts, More, plural, Reading, Setting, since } from './parts';

const ACCESS: { value: ModelAccess; label: string }[] = [
  { value: 'open', label: 'Everybody' },
  { value: 'password', label: 'With the password' },
  { value: 'off', label: 'Nobody' },
];

const ENDINGS: Record<string, string> = {
  completed: 'completed',
  asked: 'asked back',
  declined: 'declined',
  cancelled: 'stopped',
  'timed-out': 'out of time',
  'out-of-steps': 'out of steps',
  'sign-in-failed': 'could not sign in',
  failed: 'failed',
  lost: 'lost',
};

/**
 * What the operator decides about the agent and how it was used: whether it
 * builds and changes apps here, the values of its options people may choose -
 * and which need the password - what a run may take, its runs and what they
 * took. What it is, can do and sends where is its card under Integrations.
 *
 * The options are the agent integration's own - a model and an effort for
 * Claude Code, anything for another - so the section draws whatever it has.
 * Every switch and list is saved as it changes, as the settings are; a
 * choice the platform refuses - the default taken from everybody - says why
 * and stays as it was.
 */
export function AgentSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [agent, setAgent] = useState<AdminAgentSettings | null>(null);
  const [runner, setRunner] = useState<AgentRunner | null>(null);
  const [usage, setUsage] = useState<AgentUsage | null>(null);
  const [runs, setRuns] = useState<AdminAgentRun[] | null>(null);
  const [saving, setSaving] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const fail = useCallback((problem: unknown, fallback: string) => {
    if (refusedToken(problem)) {
      deny();
    } else {
      setError(fallback);
    }
  }, [deny]);

  useEffect(() => {
    api.admin.agent(token)
      .then((read) => {
        setAgent(read);
        setRunner(read.runner ?? null);
      })
      .catch((problem) => fail(problem, 'The settings of the agent could not be read.'));

    // the figures are their own, and a section without them still works
    api.admin.agentUsage(token).then(setUsage).catch(() => setUsage(null));
    api.admin.agentRuns(token).then(setRuns).catch(() => setRuns(null));
  }, [token, fail]);

  async function save(what: string, change: AdminAgentChange, said: string) {
    if (agent === null || saving !== null) return;

    setSaving(what);

    try {
      // the answer to a change leaves the agent system out: it was not asked again
      setAgent(await api.admin.saveAgent(token, change));
      toast(said, 'success');
    } catch (problem) {
      if (refusedToken(problem)) {
        deny();
      } else {
        toast(problem instanceof ApiError ? problem.message : 'The settings of the agent could not be saved.', 'error');
      }
    } finally {
      setSaving(null);
    }
  }

  const options = agent?.options ?? [];

  return (
    <Section
      title="Build agent"
      hint="Whether it builds and changes apps here, on which of its options, and what a run may take. Applies to the next request; a run under way goes on as it was asked for."
    >
      {agent === null ? (
        <Reading what="the settings of the agent" failure={error} />
      ) : (
        <div className="space-y-6">
          <Status agent={agent} runner={runner} />

          <div className="space-y-3">
            <Setting
              id="agent-builds"
              title="Build new apps in the workspace on /build"
              on={agent.builds}
              saving={saving === 'builds'}
              onToggle={() => save('builds', { builds: !agent.builds },
                agent.builds ? 'No new apps are built; the workspace leads to connecting an agent.' : 'New apps are built again.')}
            >
              {agent.builds
                ? 'Visitors describe what they want and the agent creates it.'
                : 'The workspace only leads to connecting an agent of one’s own over MCP.'}
            </Setting>

            <Setting
              id="agent-changes"
              title="Let owners ask the agent about their apps"
              on={agent.changes}
              saving={saving === 'changes'}
              onToggle={() => save('changes', { changes: !agent.changes },
                agent.changes ? 'Owners can no longer ask the agent.' : 'Owners can ask the agent again.')}
            >
              {agent.changes
                ? 'In the workspace on /build and the Change section of the editor: questions, settings and changes.'
                : 'The workspace and the Change section only explain how to connect an agent of one’s own.'}
            </Setting>
          </div>

          {options.map((option) => (
            <OptionCard
              key={option.name}
              option={option}
              saving={saving}
              onAccess={(value, wanted) =>
                save(`${option.name}-${value.name}`, { options: [{ name: option.name, values: [{ name: value.name, access: wanted }] }] },
                  `${value.label}: ${ACCESS.find((a) => a.value === wanted)?.label.toLowerCase()}.`)}
              onDefault={(value) =>
                save(option.name, { options: [{ name: option.name, default: value.name }] }, `${value.label} is the default ${option.label.toLowerCase()}.`)}
            />
          ))}

          {agent.limitSupport && (agent.limitSupport.time || agent.limitSupport.steps) && (
            <LimitsCard agent={agent} saving={saving === 'limits'} onSave={(limits, guarded) => save('limits', { limits, guardedLimits: guarded }, 'The limits of a run are saved.')} />
          )}

          <UsageCard usage={usage} options={options} />

          <RunsCard runs={runs} />
        </div>
      )}
    </Section>
  );
}

/* ------------------------------------------------------------ the agent system */

/**
 * Which agent it is and whether it answers, this installation's runs now, and
 * what the operator gives people: the password, the builds a day. What else it
 * says of itself is under Integrations.
 */
function Status({ agent, runner }: { agent: AdminAgentSettings; runner: AgentRunner | null }) {
  const bad = 'text-red-600 dark:text-red-400';

  return (
    <Card title="Agent" more={agent.agent ? { to: '/admin/integrations', label: 'What it is and sends where' } : undefined}>
      <Facts>
        <Fact label="Is">
          {agent.agent ? (
            <>
              {agent.agent.agent}
              {agent.agent.vendor && <span className="text-slate-500"> · {agent.agent.vendor}</span>}
            </>
          ) : (
            <span className={bad}>None - LAMBDA_AGENT_URL is not set</span>
          )}
        </Fact>

        {agent.agent && (
          <Fact label="State">
            {runner?.reachable ? 'Answering' : <span className={bad}>Not answering</span>}
          </Fact>
        )}

        {runner?.reachable && (
          <Fact label="Runs of this installation">
            {plural(runner.running, 'working', 'working')} · {plural(runner.queued, 'waiting', 'waiting')}
          </Fact>
        )}

        <Fact label="Password">{agent.passwordSet ? 'Set - LAMBDA_AGENT_PASSWORD' : 'Not set - LAMBDA_AGENT_PASSWORD'}</Fact>

        <Fact label="Builds and changes a day">
          <span className="inline-flex flex-wrap items-center gap-x-2">
            {plural(agent.perDay, 'per address', 'per address')}
            <More to="/admin/limits">Limits</More>
          </span>
        </Fact>
      </Facts>
    </Card>
  );
}

/* ------------------------------------------------------------ options */

/** One option of the agent's: who may choose each value, and which a run takes where nobody chose. */
function OptionCard({ option, saving, onAccess, onDefault }: {
  option: AdminAgentOption;
  saving: string | null;
  onAccess: (value: AdminAgentOption['values'][number], access: ModelAccess) => void;
  onDefault: (value: AdminAgentOption['values'][number]) => void;
}) {
  return (
    <Card
      title={option.label}
      hint="Who may choose each value. One with the password runs on the limits for such runs below. The default is everybody’s."
      flush
    >
      <table className="w-full text-[13px]">
        <thead className="text-left text-xs text-slate-500">
          <tr className="border-b border-grey-300 dark:border-ink-800">
            <th className="px-4 py-2 font-medium">{option.label}</th>
            <th className="px-4 py-2 font-medium">Who</th>
            <th className="px-4 py-2 text-center font-medium">Default</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-grey-300 dark:divide-ink-800">
          {option.values.map((value) => {
            const isDefault = option.default === value.name;
            const unoffered = value.access === 'password' && !value.offered;

            return (
              <tr key={value.name}>
                <td className="px-4 py-2.5">
                  <span className="font-medium">{value.label}</span>
                  <span className="ml-2 font-mono text-[12px] text-slate-500">{value.name}</span>
                  {value.description && <div className="mt-0.5 text-xs text-slate-500">{value.description}</div>}
                  {unoffered && <div className="mt-0.5 text-xs text-amber-600 dark:text-amber-400">Offered to nobody: there is no password</div>}
                </td>
                <td className="px-4 py-2.5">
                  <select
                    aria-label={`Who may choose ${value.label}`}
                    value={value.access}
                    disabled={isDefault || saving !== null}
                    title={isDefault ? 'The default is everybody’s. Make another the default first.' : undefined}
                    onChange={(event) => onAccess(value, event.target.value as ModelAccess)}
                    className="field py-1 text-[13px] disabled:opacity-60"
                  >
                    {ACCESS.map((choice) => <option key={choice.value} value={choice.value}>{choice.label}</option>)}
                  </select>
                </td>
                <td className="px-4 py-2.5 text-center">
                  <input
                    type="radio"
                    name={`agent-default-${option.name}`}
                    aria-label={`${value.label} by default`}
                    checked={isDefault}
                    disabled={value.access !== 'open' || saving !== null}
                    title={value.access !== 'open' ? 'Only a value for everybody can be the default.' : undefined}
                    onChange={() => onDefault(value)}
                    className="h-4 w-4 accent-accent-500"
                  />
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </Card>
  );
}

/* ------------------------------------------------------------ limits */

/** A limit as the form holds it: minutes or steps as typed, empty for the agent's default. */
const shown = (value: number | null | undefined, unit: 'minutes' | 'steps') =>
  value == null ? '' : unit === 'minutes' ? String(Math.round(value / 60)) : String(value);

const read = (text: string, unit: 'minutes' | 'steps'): number | null | undefined => {
  if (text.trim() === '') return null;
  const number = Number(text);
  if (!Number.isInteger(number) || number < 0) return undefined;
  return unit === 'minutes' ? number * 60 : number;
};

/**
 * What a run may take - and a run on a value behind the password - where the
 * agent holds runs to limits at all. Empty is what the agent gives a run,
 * zero is none.
 */
function LimitsCard({ agent, saving, onSave }: { agent: AdminAgentSettings; saving: boolean; onSave: (limits: AgentLimits, guarded: AgentLimits) => void }) {
  const support = agent.limitSupport!;

  const initial = () => ({
    time: shown(agent.limits?.seconds, 'minutes'),
    steps: shown(agent.limits?.steps, 'steps'),
    guardedTime: shown(agent.guardedLimits?.seconds, 'minutes'),
    guardedSteps: shown(agent.guardedLimits?.steps, 'steps'),
  });

  const [draft, setDraft] = useState(initial);

  // read again where the limits changed - not where another setting was saved,
  // which would throw away what is being typed here
  const saved = JSON.stringify([agent.limits, agent.guardedLimits]);

  useEffect(() => setDraft(initial()), [saved]); // eslint-disable-line react-hooks/exhaustive-deps

  // a field left as it was keeps what was saved to the second, which minutes would round
  const kept = initial();
  const parsed = {
    time: draft.time === kept.time ? (agent.limits?.seconds ?? null) : read(draft.time, 'minutes'),
    steps: read(draft.steps, 'steps'),
    guardedTime: draft.guardedTime === kept.guardedTime ? (agent.guardedLimits?.seconds ?? null) : read(draft.guardedTime, 'minutes'),
    guardedSteps: read(draft.guardedSteps, 'steps'),
  };

  const valid = Object.values(parsed).every((value) => value !== undefined);
  const dirty = JSON.stringify(draft) !== JSON.stringify(initial());

  const defaults = `The agent gives a run ${support.defaultSeconds ? duration(support.defaultSeconds) : 'no time limit'}${
    support.steps ? ` and ${support.defaultSteps ? plural(support.defaultSteps, 'step') : 'no step limit'}` : ''
  } where nothing is set.`;

  const field = (key: keyof typeof draft, label: string, unit: string, disabled: boolean) => (
    <label className="block text-[13px]">
      <span className="text-slate-500">{label}</span>
      <span className="mt-1 flex items-center gap-2">
        <input
          type="number"
          min={0}
          inputMode="numeric"
          value={draft[key]}
          disabled={disabled}
          placeholder="Default"
          onChange={(event) => setDraft((was) => ({ ...was, [key]: event.target.value }))}
          className="field w-28 py-1 text-[13px] disabled:opacity-50"
        />
        <span className="text-xs text-slate-500">{unit}</span>
      </span>
    </label>
  );

  return (
    <Card title="What a run may take" hint={`Empty for the agent’s own limit, 0 for none. ${defaults}`}>
      <form
        onSubmit={(event) => {
          event.preventDefault();

          if (valid && dirty) {
            onSave(
              { seconds: parsed.time ?? null, steps: parsed.steps ?? null },
              { seconds: parsed.guardedTime ?? null, steps: parsed.guardedSteps ?? null },
            );
          }
        }}
        className="space-y-4"
      >
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-3">
            <p className="text-[13px] font-medium">A run</p>
            {field('time', 'Time', 'minutes', !support.time)}
            {field('steps', 'Steps', 'steps', !support.steps)}
          </div>
          <div className="space-y-3">
            <p className="text-[13px] font-medium">A run with the password</p>
            {field('guardedTime', 'Time', 'minutes', !support.time)}
            {field('guardedSteps', 'Steps', 'steps', !support.steps)}
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <button type="submit" disabled={!dirty || !valid || saving} className="btn-primary !px-4 !py-1.5 text-[13px]">
            {saving && <IconSpinner className="h-3.5 w-3.5" />}
            Save limits
          </button>
          {!valid && <span className="text-xs text-red-600 dark:text-red-400">Whole numbers, 0 or more - or empty.</span>}
        </div>
      </form>
    </Card>
  );
}

/* ------------------------------------------------------------ how it was used */

/** How long a run worked: seconds while it is under a minute, which most answers are. */
const took = (seconds: number) => (seconds < 60 ? `${Math.round(seconds)}s` : duration(seconds));

/** Runs and their time, in all and by each value of each option - never what they were about. */
function UsageCard({ usage, options }: { usage: AgentUsage | null; options: AdminAgentOption[] }) {
  if (!usage) {
    return null;
  }

  const labelOf = (option: string, value: string) =>
    options.find((o) => o.name === option)?.values.find((v) => v.name === value)?.label ?? value;

  return (
    <Card title={`Used in the last ${plural(usage.days, 'day')}`} flush>
      <div className="p-4">
        <Facts>
          <Fact label="Runs">{count(usage.runs)}</Fact>
          <Fact label="Of them new apps">{count(usage.builds)}</Fact>
          <Fact label="Worked">{took(usage.seconds)}</Fact>
        </Facts>
      </div>

      {usage.options.length > 0 && (
        <table className="w-full border-t border-grey-300 text-[13px] dark:border-ink-800">
          <thead className="text-left text-xs text-slate-500">
            <tr className="border-b border-grey-300 dark:border-ink-800">
              <th className="px-4 py-2 font-medium">On</th>
              <th className="px-4 py-2 text-right font-medium">Runs</th>
              <th className="px-4 py-2 text-right font-medium">Worked</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-grey-300 dark:divide-ink-800">
            {[...usage.options]
              .sort((a, b) => order(options, a.option) - order(options, b.option))
              .flatMap((option) =>
              option.values.map((value) => (
                <tr key={`${option.option}-${value.value}`}>
                  <td className="px-4 py-2">
                    <span className="text-slate-500">{options.find((o) => o.name === option.option)?.label ?? option.option}: </span>
                    {labelOf(option.option, value.value)}
                  </td>
                  <td className="px-4 py-2 text-right tabular-nums">{count(value.runs)}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{took(value.seconds)}</td>
                </tr>
              )),
            )}
          </tbody>
        </table>
      )}
    </Card>
  );
}

/** Where an option is in the agent's list; one it no longer has goes last. */
const order = (options: AdminAgentOption[], name: string) => {
  const at = options.findIndex((o) => o.name === name);
  return at < 0 ? options.length : at;
};

/** The newest runs, for looking into one that went wrong: what was asked is in the log as well. */
function RunsCard({ runs }: { runs: AdminAgentRun[] | null }) {
  if (!runs || runs.length === 0) {
    return null;
  }

  return (
    <Card title="Newest runs" hint="Kept for a while after they ended. Files open as they were sent or handed back." flush>
      <ul className="divide-y divide-grey-300 dark:divide-ink-800">
        {runs.map((run) => (
          <li key={`${run.ref}-${run.created}`} className="px-4 py-3 text-[13px]">
            <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
              <span className="font-medium">{run.kind === 'build' ? 'Build' : 'Change'}</span>
              <span className="font-mono text-[12px] text-slate-400" title="The start of its id, as the log names it">{run.ref}</span>
              {run.lambda && <span className="font-mono text-[12px]">{run.lambda}</span>}
              <span className={run.ending && !['completed', 'asked'].includes(run.ending) ? 'text-amber-600 dark:text-amber-400' : 'text-slate-500'}>
                {run.ending ? (ENDINGS[run.ending] ?? run.ending) : run.state}
              </span>
              <span className="text-slate-500">{since(run.created)} ago</span>
              {run.seconds != null && run.seconds > 0 && <span className="text-slate-500">{took(run.seconds)}</span>}
              {run.options && Object.keys(run.options).length > 0 && (
                <span className="font-mono text-[12px] text-slate-500">{Object.values(run.options).join(' · ')}</span>
              )}
              {(run.inputTokens != null || run.cost != null) && (
                <span className="text-slate-500">
                  {run.inputTokens != null && `${count(run.inputTokens)} in`}
                  {run.outputTokens != null && ` · ${count(run.outputTokens)} out`}
                  {run.cost != null && ` · ${run.cost.toFixed(2)} ${run.currency ?? ''}`}
                </span>
              )}
            </div>

            <p className="mt-1 line-clamp-2 text-slate-600 dark:text-slate-400" title={run.prompt}>{run.prompt}</p>

            {run.detail && <p className="mt-1 break-words font-mono text-xs text-slate-500">{run.detail}</p>}

            {[...(run.attachments ?? []), ...(run.files ?? [])].length > 0 && (
              <p className="mt-1 flex flex-wrap gap-x-3 text-xs">
                {(run.attachments ?? []).map((file) => (
                  <a key={file.id} href={api.attachments.url(file.id)} target="_blank" rel="noreferrer" className="text-accent-600 hover:underline dark:text-accent-400">
                    sent: {file.name} ({bytes(file.size)})
                  </a>
                ))}
                {(run.files ?? []).map((file) => (
                  <a key={file.id} href={api.attachments.url(file.id)} target="_blank" rel="noreferrer" className="text-accent-600 hover:underline dark:text-accent-400">
                    handed back: {file.name} ({bytes(file.size)})
                  </a>
                ))}
              </p>
            )}
          </li>
        ))}
      </ul>
    </Card>
  );
}
