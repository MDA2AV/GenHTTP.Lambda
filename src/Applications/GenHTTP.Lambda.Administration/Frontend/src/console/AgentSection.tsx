import { useCallback, useEffect, useState } from 'react';

import { ApiError, refusedToken, api, type AdminAgentChange, type AdminAgentSettings, type AgentRunner, type ModelAccess } from '@genhttp-lambda/kit/api';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { Card, Fact, Facts, More, plural, Reading, Setting } from './parts';

const ACCESS: { value: ModelAccess; label: string }[] = [
  { value: 'open', label: 'Everybody' },
  { value: 'password', label: 'With the password' },
  { value: 'off', label: 'Nobody' },
];

const EFFORTS: Record<string, string> = {
  low: 'Low',
  medium: 'Medium',
  high: 'High',
  xhigh: 'Extra high',
  max: 'Maximum',
};

/**
 * The build agent: whether it builds and changes apps here, the models and
 * efforts people may choose - and which models need the password - and what
 * its runner says of itself.
 *
 * Every switch and list is what it is, saved as it changes, as the settings
 * are: there is nothing to confirm, and a choice the platform refuses - the
 * default model taken from everybody, the last effort switched off - says
 * why and stays as it was.
 */
export function AgentSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [agent, setAgent] = useState<AdminAgentSettings | null>(null);
  const [runner, setRunner] = useState<AgentRunner | null>(null);
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
      .catch((problem) => fail(problem, 'The settings of the build agent could not be read.'));
  }, [token, fail]);

  async function save(what: string, change: AdminAgentChange, said: string) {
    if (agent === null || saving !== null) return;

    setSaving(what);

    try {
      // the answer to a change leaves the runner out: it was not asked again
      setAgent(await api.admin.saveAgent(token, change));
      toast(said, 'success');
    } catch (problem) {
      if (refusedToken(problem)) {
        deny();
      } else {
        toast(problem instanceof ApiError ? problem.message : 'The settings of the build agent could not be saved.', 'error');
      }
    } finally {
      setSaving(null);
    }
  }

  const models = (wanted: (name: string, access: ModelAccess) => ModelAccess) =>
    agent!.models.map((m) => ({ name: m.name, access: wanted(m.name, m.access) }));

  return (
    <Section
      title="Build agent"
      hint="Whether it builds and changes apps here, and on which models and efforts. Applies to the next request; a job under way runs on as it was asked for."
    >
      {agent === null ? (
        <Reading what="the settings of the build agent" failure={error} />
      ) : (
        <div className="space-y-6">
          <Runner agent={agent} runner={runner} />

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

          <Card
            title="Models"
            hint="Who may choose each. One with the password runs without the clock and the step limit, in a lane of its own. The default is everybody’s."
            flush
          >
            <table className="w-full text-[13px]">
              <thead className="text-left text-xs text-slate-500">
                <tr className="border-b border-grey-300 dark:border-ink-800">
                  <th className="px-4 py-2 font-medium">Model</th>
                  <th className="hidden px-4 py-2 font-medium sm:table-cell">Runs as</th>
                  <th className="px-4 py-2 font-medium">Who</th>
                  <th className="px-4 py-2 text-center font-medium">Default</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-grey-300 dark:divide-ink-800">
                {agent.models.map((model) => {
                  const isDefault = agent.model === model.name;
                  const unoffered = model.access === 'password' && !model.offered;

                  return (
                    <tr key={model.name}>
                      <td className="px-4 py-2.5">
                        <span className="font-medium">{model.label}</span>
                        <span className="ml-2 font-mono text-[12px] text-slate-500">{model.name}</span>
                        {unoffered && <div className="mt-0.5 text-xs text-amber-600 dark:text-amber-400">Offered to nobody: there is no password</div>}
                      </td>
                      <td className="hidden px-4 py-2.5 font-mono text-[12px] text-slate-500 sm:table-cell">
                        {runner?.models?.[model.name] ?? '-'}
                      </td>
                      <td className="px-4 py-2.5">
                        <select
                          aria-label={`Who may choose ${model.label}`}
                          value={model.access}
                          disabled={isDefault || saving !== null}
                          title={isDefault ? 'The default model is everybody’s. Make another the default first.' : undefined}
                          onChange={(event) => {
                            const access = event.target.value as ModelAccess;

                            void save(`model-${model.name}`, { models: models((name, was) => (name === model.name ? access : was)) },
                              `${model.label}: ${ACCESS.find((a) => a.value === access)?.label.toLowerCase()}.`);
                          }}
                          className="field py-1 text-[13px] disabled:opacity-60"
                        >
                          {ACCESS.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
                        </select>
                      </td>
                      <td className="px-4 py-2.5 text-center">
                        <input
                          type="radio"
                          name="agent-default-model"
                          aria-label={`${model.label} by default`}
                          checked={isDefault}
                          disabled={model.access !== 'open' || saving !== null}
                          title={model.access !== 'open' ? 'Only a model for everybody can be the default.' : undefined}
                          onChange={() => save('model', { model: model.name }, `${model.label} is the default.`)}
                          className="h-4 w-4 accent-accent-500"
                        />
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </Card>

          <Card title="Efforts" hint="How much the model thinks, and how many steps it takes. More is slower and costs more." flush>
            <table className="w-full text-[13px]">
              <thead className="text-left text-xs text-slate-500">
                <tr className="border-b border-grey-300 dark:border-ink-800">
                  <th className="px-4 py-2 font-medium">Effort</th>
                  <th className="px-4 py-2 font-medium">Offered</th>
                  <th className="px-4 py-2 text-center font-medium">Default</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-grey-300 dark:divide-ink-800">
                {agent.efforts.map((effort) => {
                  const isDefault = agent.effort === effort.name;
                  const offered = agent.efforts.filter((e) => e.offered).map((e) => e.name);

                  return (
                    <tr key={effort.name}>
                      <td className="px-4 py-2.5">
                        <span className="font-medium">{EFFORTS[effort.name] ?? effort.name}</span>
                        <span className="ml-2 font-mono text-[12px] text-slate-500">{effort.name}</span>
                      </td>
                      <td className="px-4 py-2.5">
                        <input
                          type="checkbox"
                          aria-label={`Offer ${EFFORTS[effort.name] ?? effort.name}`}
                          checked={effort.offered}
                          disabled={isDefault || saving !== null}
                          title={isDefault ? 'The default effort is always offered. Make another the default first.' : undefined}
                          onChange={() => {
                            const next = effort.offered ? offered.filter((e) => e !== effort.name) : [...offered, effort.name];

                            void save(`effort-${effort.name}`, { efforts: next },
                              `${EFFORTS[effort.name] ?? effort.name} is ${effort.offered ? 'no longer offered' : 'offered'}.`);
                          }}
                          className="h-4 w-4 accent-accent-500 disabled:opacity-60"
                        />
                      </td>
                      <td className="px-4 py-2.5 text-center">
                        <input
                          type="radio"
                          name="agent-default-effort"
                          aria-label={`${EFFORTS[effort.name] ?? effort.name} by default`}
                          checked={isDefault}
                          disabled={!effort.offered || saving !== null}
                          onChange={() => save('effort', { effort: effort.name }, `${EFFORTS[effort.name] ?? effort.name} is the default effort.`)}
                          className="h-4 w-4 accent-accent-500"
                        />
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </Card>
        </div>
      )}
    </Section>
  );
}

/** What the runner is and says of itself, as facts. */
function Runner({ agent, runner }: { agent: AdminAgentSettings; runner: AgentRunner | null }) {
  const bad = 'text-red-600 dark:text-red-400';

  return (
    <Card title="Runner">
      <Facts>
        <Fact label="Address">
          {agent.url ? <span className="break-all font-mono text-[12px]">{agent.url}</span> : <span className={bad}>None - LAMBDA_AGENT_URL is not set</span>}
        </Fact>

        {agent.url && (
          <Fact label="State">
            {runner?.reachable ? 'Answering' : <span className={bad}>Not answering</span>}
          </Fact>
        )}

        {runner?.reachable && (
          <Fact label="Signs in with">
            {runner.signsInWith === 'subscription'
              ? 'A subscription token'
              : runner.signsInWith === 'api-key'
                ? 'An API key'
                : <span className={bad}>Nothing - CLAUDE_CODE_OAUTH_TOKEN or ANTHROPIC_API_KEY</span>}
          </Fact>
        )}

        {runner?.reachable && runner.cli && <Fact label="Claude Code">{runner.cli}</Fact>}

        {runner?.reachable && runner.readsPages != null && (
          <Fact label="Web pages">
            {runner.readsPages ? 'The sites a request names - LAMBDA_AGENT_FETCH' : 'None - LAMBDA_AGENT_FETCH is false'}
          </Fact>
        )}

        {runner?.reachable && (
          <Fact label={(runner.installations ?? 1) > 1 ? 'Jobs of this installation' : 'Jobs'}>
            {plural(runner.running, 'running', 'running')} · {plural(runner.queued, 'waiting', 'waiting')}
          </Fact>
        )}

        {runner?.reachable && runner.concurrency != null && (
          <Fact label="At once">
            {plural(runner.concurrency, 'build')} - AGENT_CONCURRENCY
          </Fact>
        )}

        {runner?.reachable && runner.installations != null && (
          <Fact label="Shared">
            {runner.installations > 1
              ? `With ${plural(runner.installations - 1, 'other installation')} - AGENT_HOSTS`
              : 'With nobody - AGENT_HOSTS names this installation alone'}
          </Fact>
        )}

        <Fact label="Password">
          {agent.passwordSet ? 'Set - LAMBDA_AGENT_PASSWORD' : 'Not set - LAMBDA_AGENT_PASSWORD'}
        </Fact>

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
