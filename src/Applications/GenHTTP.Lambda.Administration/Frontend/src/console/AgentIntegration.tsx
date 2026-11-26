import { useEffect, useState } from 'react';

import { api, refusedToken, type AdminAgentSettings, type Integration } from '@genhttp-lambda/kit/api';
import type { Access } from './context';
import { Card, Fact, Facts, plural, Reading } from './parts';

const WEB: Record<string, string> = {
  none: 'None',
  named: 'Only the sites a request names',
  any: 'Any public site',
};

/**
 * The agent among the integrations: what it is, how it is doing and what it
 * can do, and where a run sends what - its technicalities, beside the other
 * integrations'. What the operator decides about it and how it was used are
 * the Build agent section's.
 */
export function AgentIntegration({ integration, access }: { integration: Integration; access: Access }) {
  const { token, deny } = access;

  const [agent, setAgent] = useState<AdminAgentSettings | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.admin.agent(token)
      .then(setAgent)
      .catch((problem) => {
        if (refusedToken(problem)) {
          deny();
        } else {
          setError('What the agent says of itself could not be read.');
        }
      });
  }, [token, deny]);

  const runner = agent?.runner ?? null;
  const said = agent?.agent ?? null;

  return (
    <>
      <Card
        title={`Agent · ${integration.name}`}
        hint={<span className="font-mono text-[12px]">{integration.assembly}</span>}
        more={{ to: '/admin/agent', label: 'Build agent' }}
      >
        {agent === null ? (
          <Reading what="what the agent says of itself" failure={error} />
        ) : (
          <Facts>
            {said && (
              <Fact label="Is">
                {said.agent}
                {said.vendor && <span className="text-slate-500"> · {said.vendor}</span>}
              </Fact>
            )}

            <Fact label="State">
              {runner?.reachable ? 'Answering' : <span className="text-red-600 dark:text-red-400">Not answering</span>}
            </Fact>

            {integration.details.map((detail) => (
              <Fact key={detail.label} label={detail.label}>
                <span className="break-all font-mono text-[12px]">{detail.value}</span>
              </Fact>
            ))}

            {runner?.reachable && runner.concurrency != null && <Fact label="At once">{plural(runner.concurrency, 'run')}</Fact>}

            {runner?.reachable &&
              (runner.facts ?? []).map((fact) => (
                <Fact key={fact.label} label={fact.label}>
                  {fact.value}
                </Fact>
              ))}
          </Facts>
        )}
      </Card>

      {said && <Destinations said={said} />}
    </>
  );
}

/**
 * Where a run sends what - for the privacy statement of the installation -
 * and what the agent does: the web, files both ways, asking back, stopping.
 */
function Destinations({ said }: { said: NonNullable<AdminAgentSettings['agent']> }) {
  return (
    <Card title="Where a run sends what" hint="What a run is given - the request, the conversation, the files sent with it, the app’s code - goes to these. Say so in the privacy statement of the installation." flush>
      <table className="w-full text-[13px]">
        <thead className="text-left text-xs text-slate-500">
          <tr className="border-b border-grey-300 dark:border-ink-800">
            <th className="px-4 py-2 font-medium">To</th>
            <th className="hidden px-4 py-2 font-medium sm:table-cell">Where</th>
            <th className="px-4 py-2 font-medium">What</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-grey-300 dark:divide-ink-800">
          {said.destinations.map((destination) => (
            <tr key={`${destination.name}-${destination.address}`}>
              <td className="px-4 py-2.5 font-medium">{destination.name}</td>
              <td className="hidden px-4 py-2.5 font-mono text-[12px] text-slate-500 sm:table-cell sm:whitespace-nowrap">{destination.address}</td>
              <td className="px-4 py-2.5 text-slate-600 dark:text-slate-400">{destination.what}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="border-t border-grey-300 p-4 dark:border-ink-800">
        <Facts>
          <Fact label="Web pages">
            {WEB[said.web] ?? said.web}
            {said.webPolicy && <span className="block text-xs text-slate-500">{said.webPolicy}</span>}
          </Fact>
          <Fact label="Searches the web">{said.searches ? 'Yes' : 'No'}</Fact>
          <Fact label="Files sent with a message">{said.attachments?.length ? said.attachments.join(', ') : 'None'}</Fact>
          <Fact label="Hands files back">{said.returnsFiles ? 'Yes' : 'No'}</Fact>
          <Fact label="Asks back">{said.asks ? 'Yes - the next message answers' : 'No'}</Fact>
          <Fact label="Can be stopped">{said.cancels ? 'Yes' : 'No'}</Fact>
        </Facts>
      </div>
    </Card>
  );
}
