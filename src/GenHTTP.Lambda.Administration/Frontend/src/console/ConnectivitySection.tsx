import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { api, refusedToken, type ConnectivityEntry, type ConnectivityListing } from '@genhttp-lambda/kit/api';
import { IconGlobe, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Ago, Empty, Section, StatusBadge, TierBadge } from '@genhttp-lambda/kit/ui/ui';
import type { Access } from './context';
import { ConnectivityDecisionDialog } from './ConnectivityControl';
import { BlockedBadge } from './LambdaDetail';

/**
 * The lambdas that ask to connect to other servers, and the ones allowed to.
 *
 * Lambdas were used as proxies - an API of their own that fetched whatever
 * address it was handed - so only a premium lambda connects without asking.
 * The rest ask, saying what for, and wait here: the longest waiting first,
 * each with what its owner said, whether its code connects at all, and the
 * two answers. What was allowed stays listed below, to be looked at again
 * and taken back.
 */
export function ConnectivitySection({ access }: { access: Access }) {
  const { token, deny, changed } = access;

  const toast = useToast();

  const [listing, setListing] = useState<ConnectivityListing | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [deciding, setDeciding] = useState<{ entry: ConnectivityEntry; allow: boolean } | null>(null);

  const load = useCallback(async () => {
    try {
      setListing(await api.admin.connectivity(token));
      setFailure(null);
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        setFailure('The requests could not be read.');
      }
    }
  }, [token, deny]);

  useEffect(() => {
    load();
  }, [load]);

  const requested = listing?.requested ?? [];
  const allowed = listing?.allowed ?? [];

  return (
    <Section
      title="Connectivity"
      hint="Whether a lambda's code may connect to other servers - call an API, open a socket, send mail. Premium lambdas may without asking; any other asks here, saying what for. A decision applies from the lambda's next request, without a deploy."
    >
      {listing === null ? (
        failure ? (
          <p className="text-sm text-slate-500">{failure}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500"><IconSpinner /> Reading the requests…</div>
        )
      ) : (
        <>
          <h2 className="text-sm font-medium">
            Waiting for you {requested.length > 0 && <span className="ml-1 text-slate-500">{requested.length}</span>}
          </h2>

          {requested.length === 0 ? (
            <Empty>Nothing waits for a decision. An owner asks in their editor, or their agent does with request_connectivity.</Empty>
          ) : (
            <ul className="mt-3 space-y-3">
              {requested.map((entry) => (
                <li key={entry.publicKey} className="surface border-amber-500/30 p-4">
                  <div className="flex flex-wrap items-start gap-x-4 gap-y-3">
                    <div className="min-w-0 flex-1">
                      <p className="flex flex-wrap items-center gap-2">
                        <Link to={`/admin/lambdas/${encodeURIComponent(entry.publicKey)}`} className="font-mono text-sm text-accent-500 hover:underline dark:text-accent-400">
                          {entry.publicKey}
                        </Link>
                        {entry.blocked ? <BlockedBadge /> : <StatusBadge version={entry.activeVersion} />}
                        <TierBadge tier={entry.tier} />
                        <Connects connects={entry.connects} />
                      </p>

                      <blockquote className="mt-2 whitespace-pre-line break-words border-l-2 border-amber-500/40 pl-3 text-sm text-slate-700 dark:text-slate-300">
                        {entry.connectivity.purpose}
                      </blockquote>

                      <p className="mt-2 text-[12px] text-slate-500">
                        Asked <Ago at={entry.connectivity.changed!} />
                      </p>
                    </div>

                    {!entry.blocked && (
                      <div className="flex shrink-0 gap-1.5">
                        <button type="button" onClick={() => setDeciding({ entry, allow: true })} className="btn-primary !px-3 !py-1.5 text-[13px]">
                          Allow…
                        </button>
                        <button type="button" onClick={() => setDeciding({ entry, allow: false })} className="btn-ghost !px-3 !py-1.5 text-[13px]">
                          Decline…
                        </button>
                      </div>
                    )}
                  </div>
                </li>
              ))}
            </ul>
          )}

          <h2 className="mt-10 text-sm font-medium">
            Allowed {allowed.length > 0 && <span className="ml-1 text-slate-500">{allowed.length}</span>}
          </h2>
          <p className="mt-1 text-[13px] text-slate-500">
            By you, the most recently allowed first. Premium lambdas are not listed: their tier includes it.
          </p>

          {allowed.length === 0 ? (
            <Empty>No lambda is allowed by itself.</Empty>
          ) : (
            <div className="surface mt-3 overflow-x-auto">
              <table className="w-full text-left text-[13px]">
                <thead className="text-xs text-slate-500">
                  <tr className="border-b border-slate-200 dark:border-ink-800">
                    <th className="px-4 py-2 font-medium">Key</th>
                    <th className="px-4 py-2 font-medium">What it is for</th>
                    <th className="px-4 py-2 font-medium">Allowed</th>
                    <th className="px-4 py-2 text-right font-medium" />
                  </tr>
                </thead>
                <tbody>
                  {allowed.map((entry) => (
                    <tr key={entry.publicKey} className="border-b border-slate-200 align-top last:border-0 dark:border-ink-800">
                      <td className="whitespace-nowrap px-4 py-2.5">
                        <Link to={`/admin/lambdas/${encodeURIComponent(entry.publicKey)}`} className="font-mono text-accent-500 hover:underline dark:text-accent-400">
                          {entry.publicKey}
                        </Link>
                        <span className="ml-2 inline-flex gap-1.5 align-middle">
                          <TierBadge tier={entry.tier} />
                          <Connects connects={entry.connects} />
                        </span>
                      </td>
                      <td className="max-w-md px-4 py-2.5 text-slate-600 dark:text-slate-400">
                        <span className="line-clamp-2 break-words" title={entry.connectivity.purpose ?? undefined}>
                          {entry.connectivity.purpose ?? <span className="text-slate-400">Allowed without being asked</span>}
                        </span>
                        {entry.connectivity.note && <span className="mt-0.5 block text-[12px] text-slate-500">You: “{entry.connectivity.note}”</span>}
                      </td>
                      <td className="whitespace-nowrap px-4 py-2.5 text-slate-500"><Ago at={entry.connectivity.changed!} /></td>
                      <td className="whitespace-nowrap px-4 py-2.5 text-right">
                        {!entry.blocked && (
                          <button type="button" onClick={() => setDeciding({ entry, allow: false })} className="btn-ghost !px-2 !py-1 text-xs">
                            Take back…
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      <ConnectivityDecisionDialog
        token={token}
        publicKey={deciding?.entry.publicKey ?? ''}
        purpose={deciding?.entry.connectivity.purpose}
        current={deciding?.entry.connectivity.status}
        allow={deciding?.allow ?? null}
        connects={deciding?.entry.connects ?? false}
        onClose={() => setDeciding(null)}
        onDecided={(_, allow) => {
          toast(allow
            ? `"${deciding?.entry.publicKey}" may connect to other servers from its next request.`
            : `"${deciding?.entry.publicKey}" may no longer connect to other servers.`, 'success');
          setDeciding(null);
          changed?.();
          load();
        }}
        deny={deny}
        failed={(message) => toast(message, 'error')}
      />
    </Section>
  );
}

/** Whether the code of a lambda connects to other servers at all. */
function Connects({ connects }: { connects: boolean }) {
  return connects ? (
    <span
      className="inline-flex items-center gap-1 whitespace-nowrap rounded-full bg-slate-400/10 px-2 py-0.5 text-[11px] font-medium text-slate-600 dark:text-slate-300"
      title="The code online, or the newest, connects to other servers"
    >
      <IconGlobe className="h-3 w-3" />
      code connects
    </span>
  ) : (
    <span className="whitespace-nowrap text-[11px] text-slate-400" title="Nothing in the code online, or the newest, connects to other servers yet">
      code does not connect yet
    </span>
  );
}
