import { useEffect, useState } from 'react';

import { api, refusedToken, type Integration } from '@genhttp-lambda/kit/api';
import { Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { Card, Fact, Reading } from './parts';

const KINDS: Record<Integration['kind'], string> = {
  database: 'Database',
  files: 'Files',
  secrets: 'Secrets',
};

/**
 * What the installation keeps its data in - read only, since the host chooses
 * it as it starts. A card per kind: the integration, the assembly that names
 * it, and where it keeps what it keeps.
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
    <Section title="Integrations" hint="What the installation keeps its data in, chosen as it starts.">
      {integrations === null ? (
        <Reading what="the integrations" failure={error} />
      ) : (
        <div className="space-y-3">
          {integrations.map((integration) => (
            <Card
              key={integration.kind}
              title={`${KINDS[integration.kind] ?? integration.kind} · ${integration.name}`}
              hint={<span className="font-mono text-[12px]">{integration.assembly}</span>}
            >
              {/* one column on a phone: the values are paths and addresses,
                  which read badly broken into two narrow ones */}
              <dl className="grid grid-cols-1 gap-x-8 gap-y-3 text-[13px] sm:grid-cols-2">
                {integration.details.map((detail) => (
                  <Fact key={detail.label} label={detail.label}>
                    <span className="break-all font-mono text-[12px]">{detail.value}</span>
                  </Fact>
                ))}
              </dl>
            </Card>
          ))}
        </div>
      )}
    </Section>
  );
}
