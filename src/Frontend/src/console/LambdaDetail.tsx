import { useCallback, useEffect, useState, type ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { ApiError, allowsDomain, api, type AdminLambdaDetail, type LambdaFile } from '../api';
import { Chart } from '../components/Chart';
import { Dialog } from '../components/Dialog';
import { IconExternal, IconPlay, IconSpinner, IconTrash } from '../components/Icons';
import { useToast } from '../components/Toast';
import { Entrances } from '../control/StatsTab';
import { AgentMark, Ago, Empty, Figure, LiveDot, Section, TierBadge } from '../control/ui';
import { clock, count, ending, millis, origin, percent, span } from '../control/format';
import type { Access } from './context';

type Busy = 'deploy' | 'undeploy' | 'tier' | 'domain' | 'remove' | null;

/**
 * One lambda, for whoever runs the installation: what it is, how it is doing,
 * and everything the operator can change about it.
 *
 * The owner's control center answers the same questions for one lambda; this
 * answers them for any lambda, and adds the two things only the operator
 * decides - the tier, and with it whether the lambda may have a domain.
 */
export function LambdaDetail({ access, publicKey }: { access: Access; publicKey: string }) {
  const { token, deny, dark } = access;

  const toast = useToast();
  const navigate = useNavigate();

  const [detail, setDetail] = useState<AdminLambdaDetail | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [busy, setBusy] = useState<Busy>(null);
  const [domain, setDomain] = useState('');
  const [domainProblem, setDomainProblem] = useState<string | null>(null);
  const [viewing, setViewing] = useState<{ version: number; files: LambdaFile[] } | null>(null);
  const [removing, setRemoving] = useState(false);

  const adopt = useCallback((next: AdminLambdaDetail) => {
    setDetail(next);
    setDomain(next.lambda.domain ?? '');
  }, []);

  const load = useCallback(async () => {
    try {
      adopt(await api.admin.lambda(token, publicKey));
      setFailure(null);
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        // a lambda that is not there and a token that is not accepted answer
        // alike; the listing can tell which, so that is where this goes
        setFailure(`There is no lambda at "${publicKey}", or the token is no longer accepted.`);
      } else {
        setFailure('The lambda could not be read.');
      }
    }
  }, [token, publicKey, adopt]);

  useEffect(() => {
    load();

    const timer = window.setInterval(() => document.visibilityState === 'visible' && load(), 15_000);

    return () => window.clearInterval(timer);
  }, [load]);

  async function act(kind: Busy, action: () => Promise<void>, failed: string) {
    setBusy(kind);

    try {
      await action();
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : failed, 'error');
      }
    } finally {
      setBusy(null);
    }
  }

  const back = (
    <Link to="/admin/lambdas" className="text-[13px] text-accent-500 hover:underline">
      ← All lambdas
    </Link>
  );

  if (!detail) {
    return (
      <Section title={<span className="font-mono">{publicKey}</span>} actions={back}>
        {failure ? (
          <p className="text-sm text-slate-500">{failure}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500"><IconSpinner /> Reading the lambda…</div>
        )}
      </Section>
    );
  }

  const { lambda, traffic, versions, activations, tiers } = detail;

  const live = lambda.activeVersion != null;
  const latest = lambda.latestVersion;
  const totals = traffic.totals;
  const premium = allowsDomain(lambda.tier);

  const deploy = (version?: number) =>
    act('deploy', async () => {
      const result = await api.admin.deploy(token, lambda.publicKey, version);

      if (!result.success) {
        toast(`It does not compile: ${result.diagnostics[0]?.message ?? 'see the code'}`, 'error');
      } else {
        toast(`Version ${result.lambda?.activeVersion} of "${lambda.publicKey}" is online.`, 'success');
      }

      await load();
    }, 'The lambda could not be deployed.');

  const undeploy = () =>
    act('undeploy', async () => {
      await api.admin.undeploy(token, lambda.publicKey);
      toast(`"${lambda.publicKey}" is offline.`, 'success');
      await load();
    }, 'The lambda could not be taken offline.');

  const changeTier = (tier: string) =>
    act('tier', async () => {
      adopt(await api.admin.tier(token, lambda.publicKey, tier));
      toast(`"${lambda.publicKey}" is in the ${tier} tier now.`, 'success');
    }, 'The tier could not be changed.');

  const changeDomain = (value: string | null) => {
    setDomainProblem(null);

    return act('domain', async () => {
      try {
        adopt(await api.admin.domain(token, lambda.publicKey, value));
        toast(value ? `"${lambda.publicKey}" answers at ${value.trim()} now.` : 'The domain is removed.', 'success');
      } catch (error) {
        if (error instanceof ApiError && error.status !== 404) {
          setDomainProblem(error.message);
          return;
        }

        throw error;
      }
    }, 'The domain could not be changed.');
  };

  const remove = () =>
    act('remove', async () => {
      setRemoving(false);
      await api.admin.remove(token, lambda.publicKey);
      toast(`"${lambda.publicKey}" was removed.`, 'success');
      navigate('/admin/lambdas');
    }, 'The lambda could not be removed.');

  const view = (version: number) =>
    act(null, async () => {
      const content = await api.admin.version(token, lambda.publicKey, version);
      setViewing({ version, files: content.files });
    }, 'The code could not be read.');

  const points = traffic.minutes;

  return (
    <Section
      title={
        <span className="flex items-center gap-2">
          <LiveDot live={live} />
          <span className="font-mono">{lambda.publicKey}</span>
          <TierBadge tier={lambda.tier} />
        </span>
      }
      hint="Everything about one lambda. The figures are counted in memory since the server came up."
      actions={
        <>
          {back}
          <a href={lambda.editorPath} target="_blank" rel="noreferrer" className="btn-ghost !px-3 !py-1.5 text-[13px]"
             title="The owner's control center, where the code can be changed as well as read">
            Editor <IconExternal className="h-3.5 w-3.5" />
          </a>
          <Link to={`/admin/log?lambda=${encodeURIComponent(lambda.publicKey)}`} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            Log
          </Link>
          {latest != null && latest !== lambda.activeVersion && (
            <button type="button" onClick={() => deploy(latest)} disabled={busy !== null} className="btn-primary !px-3 !py-1.5 text-[13px]">
              {busy === 'deploy' ? <IconSpinner /> : <IconPlay className="h-3.5 w-3.5" />}
              Deploy version {latest}
            </button>
          )}
          {live && (
            <button type="button" onClick={undeploy} disabled={busy !== null} className="btn-ghost !px-3 !py-1.5 text-[13px]">
              {busy === 'undeploy' && <IconSpinner />}
              Take offline
            </button>
          )}
          <button type="button" onClick={() => setRemoving(true)} disabled={busy !== null} className="btn-danger !px-2 !py-1.5"
                  aria-label={`Delete ${lambda.publicKey}`} title="Delete">
            <IconTrash className="h-4 w-4" />
          </button>
        </>
      }
    >
      {/* ------------------------------------------------------------ facts */}

      <dl className="surface grid gap-x-8 gap-y-3 p-5 text-[13px] sm:grid-cols-2 lg:grid-cols-3">
        <Fact label="State">
          {live ? <>Online, version {lambda.activeVersion}{lambda.deployedAt && <> since <Ago at={lambda.deployedAt} /></>}</> : 'Offline'}
        </Fact>
        <Fact label="Address">
          <a href={lambda.publicPath} target="_blank" rel="noreferrer" className="font-mono text-accent-500 hover:underline dark:text-accent-400">
            {lambda.publicPath}
          </a>
        </Fact>
        <Fact label="Domain">
          {lambda.domain ? (
            <span className="font-mono">
              {lambda.domainServed ? (
                <a href={`https://${lambda.domain}/`} target="_blank" rel="noreferrer" className="text-accent-500 hover:underline dark:text-accent-400">
                  {lambda.domain}
                </a>
              ) : (
                <span title="Configured, but not served outside the premium tier" className="text-slate-400 line-through">{lambda.domain}</span>
              )}
            </span>
          ) : (
            <span className="text-slate-400">none</span>
          )}
        </Fact>
        <Fact label="Created"><Ago at={lambda.created} /></Fact>
        <Fact label="Last changed"><Ago at={lambda.modified} /></Fact>
        <Fact label="Kept">
          {lambda.keptUntil ? (
            <>
              {live && lambda.deployedUntil && <>online until <Ago at={lambda.deployedUntil} />, </>}
              removed <Ago at={lambda.keptUntil} /> unless used
            </>
          ) : (
            <>by its tier, however quiet</>
          )}
        </Fact>
      </dl>

      {/* ------------------------------------------------------- administration */}

      <div className="mt-6 grid gap-4 lg:grid-cols-2">
        <div className="surface p-4">
          <h2 className="text-sm font-medium">Tier</h2>
          <p className="mt-1 text-[13px] text-slate-500">
            Premium lambdas may answer at a domain of their own, and are never taken offline or removed for going unused.
          </p>
          <div role="radiogroup" aria-label="Tier" className="mt-3 flex flex-wrap gap-1.5">
            {tiers.map((tier) => (
              <button
                key={tier}
                type="button"
                role="radio"
                aria-checked={tier === lambda.tier}
                disabled={busy !== null || tier === lambda.tier}
                onClick={() => changeTier(tier)}
                className={`inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-[13px] ${
                  tier === lambda.tier
                    ? 'border-accent-500 bg-accent-500/10 text-accent-700 dark:border-accent-400 dark:text-accent-400'
                    : 'border-slate-200 text-slate-600 hover:border-slate-300 dark:border-ink-800 dark:text-slate-400'
                }`}
              >
                {busy === 'tier' && tier !== lambda.tier && <IconSpinner className="h-3 w-3" />}
                {tier}
              </button>
            ))}
          </div>
          {lambda.domain && !premium && (
            <p className="mt-3 text-[13px] text-amber-600 dark:text-amber-400">
              {lambda.domain} is configured but not served while the lambda is outside the premium tier.
            </p>
          )}
        </div>

        <form
          className="surface p-4"
          onSubmit={(event) => {
            event.preventDefault();
            changeDomain(domain.trim() || null);
          }}
        >
          <h2 className="text-sm font-medium">Domain</h2>
          <p className="mt-1 text-[13px] text-slate-500">
            {premium
              ? 'The owner sets this in their editor; it can be corrected or removed here.'
              : 'Only a premium lambda can be given one. Removing one is always possible.'}
          </p>
          <div className="mt-3 flex flex-wrap gap-2">
            <input
              value={domain}
              onChange={(event) => setDomain(event.target.value)}
              placeholder={premium ? 'shop.example.com' : 'premium only'}
              disabled={!premium && !lambda.domain}
              spellCheck={false}
              autoComplete="off"
              className="field min-w-0 flex-1 py-1.5 font-mono text-sm"
              aria-label="Domain"
            />
            <button
              type="submit"
              disabled={busy !== null || domain.trim() === (lambda.domain ?? '') || (!premium && domain.trim() !== '')}
              className="btn-primary !px-3 !py-1.5 text-[13px]"
            >
              {busy === 'domain' && <IconSpinner />}
              {domain.trim() === '' && lambda.domain ? 'Remove' : 'Save'}
            </button>
          </div>
          {domainProblem && <p className="mt-2 text-xs text-red-500">{domainProblem}</p>}
        </form>
      </div>

      {/* ---------------------------------------------------------- traffic */}

      <section className="mt-8">
        <h2 className="text-sm font-medium">Traffic</h2>

        <div className="surface mt-3 grid grid-cols-2 gap-6 p-5 lg:grid-cols-4">
          <Figure value={count(totals?.requests ?? 0)} label="requests"
                  title={totals?.upgrades ? `and ${totals.upgrades} websocket connections` : undefined} />
          <Figure
            value={totals && totals.requests > 0 ? percent(totals.failed, totals.requests) : '-'}
            label="failed"
            tone={!totals || totals.failed === 0 ? 'default' : 'bad'}
          />
          <Figure value={totals && totals.requests > 0 ? millis(totals.averageMillis) : '-'} label="to answer, on average"
                  title={totals ? `the slowest took ${millis(totals.slowestMillis)}` : undefined} />
          <Figure value={totals?.lastSeen ? <Ago at={totals.lastSeen} /> : 'none yet'} label="last visit" />
        </div>

        {points.some((p) => p.requests > 0 || p.upgrades > 0) && (
          <div className="mt-4">
            <Chart
              title="Requests"
              hint="Per minute, over the last hour."
              labels={points.map((p) => clock(p.at))}
              dark={dark}
              shape="stacked"
              height={150}
              format={(v) => count(Math.round(v))}
              series={[
                { label: 'Answered', color: ['#1a73e8', '#4285f4'], values: points.map((p) => Math.max(0, p.requests - p.failed - p.rejected)) },
                { label: 'Not found or refused', color: ['#e8710a', '#d56e0c'], values: points.map((p) => p.rejected) },
                { label: 'Failed', color: ['#c5221f', '#ea4335'], values: points.map((p) => p.failed) },
              ]}
            />
          </div>
        )}

        <Entrances traffic={traffic} publicKey={lambda.publicKey} />

        {traffic.paths.length > 0 && (
          <table className="mt-6 w-full text-left text-[13px]">
            <thead className="text-slate-500">
              <tr className="border-b border-slate-200 dark:border-ink-800">
                <th className="py-2 pr-3 font-normal">Most asked for</th>
                <th className="py-2 pr-3 text-right font-normal">Requests</th>
                <th className="py-2 pr-3 text-right font-normal">Failed</th>
                <th className="py-2 text-right font-normal">Average</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-ink-850">
              {traffic.paths.slice(0, 8).map((path) => (
                <tr key={path.path}>
                  <td className="max-w-xs truncate py-2 pr-3 font-mono" title={path.path}>{path.path}</td>
                  <td className="py-2 pr-3 text-right tabular-nums">{count(path.requests)}</td>
                  <td className={`py-2 pr-3 text-right tabular-nums ${path.failed > 0 ? 'text-red-500' : 'text-slate-400'}`}>{count(path.failed)}</td>
                  <td className="py-2 text-right tabular-nums text-slate-500">{millis(path.averageMillis)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>

      {/* --------------------------------------------------------- versions */}

      <div className="mt-8 grid gap-8 xl:grid-cols-2">
        <section>
          <h2 className="text-sm font-medium">Versions</h2>

          {versions.length === 0 ? (
            <Empty>Nothing saved.</Empty>
          ) : (
            <ul className="mt-2 divide-y divide-slate-100 text-[13px] dark:divide-ink-850">
              {versions.map((version) => (
                <li key={version.version} className="flex items-start gap-3 py-2">
                  <span className="w-8 shrink-0 tabular-nums text-slate-500">v{version.version}</span>
                  <span className="min-w-0 flex-1">
                    <span className="flex items-center gap-1.5">
                      <span className="truncate" title={version.change ?? undefined}>
                        {version.change ?? <span className="text-slate-400">No description</span>}
                      </span>
                      <AgentMark origin={version.origin} />
                    </span>
                    <span className="text-slate-500">
                      <Ago at={version.created} /> · {origin(version.origin)}
                      {version.version === lambda.activeVersion && <span className="text-emerald-600 dark:text-emerald-400"> · online</span>}
                    </span>
                  </span>
                  <span className="flex shrink-0 gap-1">
                    <button type="button" onClick={() => view(version.version)} disabled={busy !== null} className="btn-ghost !px-2 !py-0.5 text-xs">
                      Code
                    </button>
                    {version.version !== lambda.activeVersion && (
                      <button type="button" onClick={() => deploy(version.version)} disabled={busy !== null} className="btn-ghost !px-2 !py-0.5 text-xs">
                        Deploy
                      </button>
                    )}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section>
          <h2 className="text-sm font-medium">Deployments</h2>

          {activations.length === 0 ? (
            <Empty>Never online.</Empty>
          ) : (
            <ul className="mt-2 divide-y divide-slate-100 text-[13px] dark:divide-ink-850">
              {activations.slice(0, 15).map((activation) => (
                <li key={`${activation.started}-${activation.version}`} className="flex items-start gap-3 py-2">
                  <span className="w-8 shrink-0 tabular-nums text-slate-500">v{activation.version}</span>
                  <span className="min-w-0 flex-1">
                    <span>
                      By {origin(activation.origin)}, <Ago at={activation.started} />
                    </span>
                    <span className="block text-slate-500">
                      {activation.ended ? `${span(activation.seconds)}, ${ending(activation.endedBy)}` : `online for ${span(activation.seconds)}`}
                    </span>
                  </span>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>

      {/* ---------------------------------------------------------- dialogs */}

      <Dialog title={`Version ${viewing?.version} of ${lambda.publicKey}`} open={viewing !== null} onClose={() => setViewing(null)}>
        <div className="max-h-[60vh] space-y-4 overflow-auto">
          {viewing?.files.map((file) => (
            <div key={file.name}>
              <p className="mb-1 font-mono text-xs text-slate-500">{file.name}</p>
              <pre className="overflow-auto border border-slate-200 p-3 font-mono text-[12.5px] leading-relaxed dark:border-ink-800">
                {file.encoding === 'base64' ? '(not text)' : file.code}
              </pre>
            </div>
          ))}
        </div>
      </Dialog>

      <Dialog
        title={`Remove "${lambda.publicKey}"?`}
        open={removing}
        onClose={() => setRemoving(false)}
        footer={
          <>
            <button type="button" onClick={() => setRemoving(false)} className="btn-ghost">Keep it</button>
            <button type="button" onClick={remove} className="btn-danger">Remove</button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          Its code, its versions, its files{lambda.domain ? ' and its domain' : ''} go with it. Whoever holds the editor
          link will find nothing there, and there is no undo.
        </p>
      </Dialog>
    </Section>
  );
}

function Fact({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className="mt-0.5 truncate">{children}</dd>
    </div>
  );
}
