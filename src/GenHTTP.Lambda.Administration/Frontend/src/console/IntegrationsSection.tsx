import { useEffect, useState } from 'react';

import { api, refusedToken, type Integration } from '@genhttp-lambda/kit/api';
import { Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { Card, Fact, Reading } from './parts';

/** What each kind of integration keeps, as the operator reads it. */
const KEEPS: Record<Integration['kind'], { title: string; keeps: string }> = {
  database: { title: 'Database', keeps: 'The platform’s records, its analytics, and the database of every lambda and draft.' },
  files: { title: 'Files', keeps: 'The workspace of every lambda and draft: what the lambdas save and what is uploaded to them.' },
  secrets: { title: 'Secrets', keeps: 'The API keys and passwords of the lambdas, which only the lambda they belong to reads.' },
};

/**
 * What the installation keeps its data in - read only, since the host chooses
 * it as it starts. A card per kind: what it keeps, which integration keeps it
 * and what it says about where, and whether that is the platform's own.
 */
export function IntegrationsSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const [integrations, setIntegrations] = useState<Integration[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.admin.integrations(token)
      .then(setIntegrations)
      .catch((problem) => {
        if (refusedToken(problem)) {
          deny();
        } else {
          setError('The integrations could not be read.');
        }
      });
  }, [token, deny]);

  return (
    <Section
      title="Integrations"
      hint="What the installation keeps its data in. Chosen as it starts - LAMBDA_DATABASE on the community edition - and moving data from one to another is not done for you."
    >
      {integrations === null ? (
        <Reading what="the integrations" failure={error} />
      ) : (
        <div className="space-y-3">
          {integrations.map((integration) => (
            <Card
              key={integration.kind}
              title={(
                <span className="flex flex-wrap items-center gap-2">
                  {KEEPS[integration.kind]?.title ?? integration.kind}
                  <span className="text-slate-400">·</span>
                  <span>{integration.name}</span>
                  <span className="rounded bg-grey-200 px-1.5 py-0.5 text-[11px] font-normal text-slate-600 dark:bg-ink-800 dark:text-slate-400">
                    {integration.default ? 'The platform’s own' : 'Chosen by the host'}
                  </span>
                </span>
              )}
              hint={KEEPS[integration.kind]?.keeps}
            >
              <p className="text-[13px] text-slate-600 dark:text-slate-400">{integration.summary}</p>

              {integration.details.length > 0 && (
                // one column on a phone: the values are paths and addresses,
                // which read badly broken into two narrow ones
                <dl className="mt-3 grid grid-cols-1 gap-x-8 gap-y-3 text-[13px] sm:grid-cols-2">
                  {integration.details.map((detail) => (
                    <Fact key={detail.label} label={detail.label}>
                      <span className="break-all font-mono text-[12px]">{detail.value}</span>
                    </Fact>
                  ))}
                </dl>
              )}
            </Card>
          ))}
        </div>
      )}
    </Section>
  );
}
