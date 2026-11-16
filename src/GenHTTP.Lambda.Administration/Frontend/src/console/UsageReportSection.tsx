import { useEffect, useState } from 'react';

import { api, refusedToken, type UsageReportStatus } from '@genhttp-lambda/kit/api';
import { IconExternal, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { Ago, Section } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';

/**
 * What the installation says about itself once a day, and to whom - read
 * only, since it is switched in the environment rather than here.
 */
export function UsageReportSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const [status, setStatus] = useState<UsageReportStatus | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.admin.usageReport(token)
      .then(setStatus)
      .catch((problem) => {
        if (refusedToken(problem)) {
          deny();
        } else {
          setError('The usage report could not be read.');
        }
      });
  }, [token, deny]);

  return (
    <Section
      title="Usage report"
      hint={status?.enabled ? 'Switched off with LAMBDA_USAGE_REPORTS=false in the environment, which takes a restart.' : undefined}
    >
      {status === null ? (
        error !== null ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500">
            <IconSpinner /> Reading the usage report…
          </div>
        )
      ) : (
        <div className="space-y-3">
          <div className="surface p-4 text-[13px] text-slate-600 dark:text-slate-400">
            <p className="text-[15px] font-medium text-ink-900 dark:text-slate-100">
              {status.enabled ? 'Sent once a day' : 'Off'}
            </p>

            {status.enabled && status.endpoint ? (
              <>
                <p className="mt-1">
                  To <span className="break-all font-mono">{status.endpoint}</span>, the first shortly after the server starts.
                  {' '}
                  {/* the collector's own page says what is kept of a report */}
                  <a href={new URL(status.endpoint).origin} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-accent-600 hover:underline dark:text-accent-400">
                    What happens to it
                    <IconExternal className="h-3 w-3" />
                  </a>
                </p>

                <p className="mt-3">
                  It says how the installation is set up and used: its version and address, the machine it runs on, which
                  options are on, and counts over the lambdas - how many, in which state, how much room they take and which
                  of the platform’s features they use. It never says what a lambda is or holds: no keys, names, domains,
                  code, records, secrets or visitors.
                </p>

                <dl className="mt-3 flex flex-wrap gap-x-6 gap-y-1 text-slate-500">
                  <div>
                    <dt className="text-[11px] uppercase tracking-wide text-slate-400">Last sent</dt>
                    <dd><Ago at={status.sent} /></dd>
                  </div>
                  {status.failure && (
                    <div>
                      <dt className="text-[11px] uppercase tracking-wide text-slate-400">Last attempt</dt>
                      <dd>
                        <Ago at={status.attempted} /> - did not arrive: {status.failure}
                      </dd>
                    </div>
                  )}
                </dl>
              </>
            ) : (
              <p className="mt-1">Nothing is gathered and nothing is sent.</p>
            )}
          </div>

          {status.report && (
            <details className="surface p-4 text-[13px]">
              <summary className="cursor-pointer font-medium">The last report, as it was sent</summary>
              <pre className="mt-3 overflow-x-auto whitespace-pre text-[12px] leading-relaxed text-slate-600 dark:text-slate-400">
                {pretty(status.report)}
              </pre>
            </details>
          )}
        </div>
      )}
    </Section>
  );
}

function pretty(report: string): string {
  try {
    return JSON.stringify(JSON.parse(report), null, 2);
  } catch {
    return report;
  }
}
