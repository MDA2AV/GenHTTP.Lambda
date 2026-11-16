import { useCallback, useEffect, useState, type ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { ApiError, api, isDemo, type AdminLambdaDetail, type LambdaFile } from '@genhttp-lambda/kit/api';
import { Chart } from '@genhttp-lambda/kit/components/Chart';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconChevronLeft, IconExternal, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { LambdaLink } from '@genhttp-lambda/kit/components/LambdaLink';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Entrances } from '@genhttp-lambda/kit/ui/Entrances';
import { AgentMark, Ago, Empty, LevelMark, Meter, Sparkline, StatusBadge, TierBadge } from '@genhttp-lambda/kit/ui/ui';
import { clock, ending, millis, origin, percent, span } from '@genhttp-lambda/kit/ui/format';
import { BlockAction } from './BlockControl';
import { ConnectivityAction } from './ConnectivityControl';
import type { Access } from './context';
import { Action, Actions, Card, Fact, Facts, Notice, Reading, Tile, Tiles, actionButton, bytes, count, plural } from './parts';
import { FEATURES, connectivityWord } from './uses';

type Busy = 'deploy' | 'undeploy' | 'tier' | 'domain' | 'remove' | null;

/** What the operator is asked to confirm, or nothing. */
type Asking = { what: 'deploy'; version: number } | { what: 'undeploy' } | { what: 'remove' } | { what: 'tier' } | { what: 'domain' } | null;

/**
 * One lambda, for whoever runs the installation: everything about it in one
 * column, and everything the operator can do to it in another, apart.
 *
 * The column is what the owner's editor would show, summed up: what the app
 * is, how it is doing, which of the platform's features it uses and what it
 * keeps, its traffic, problems and history - so the editor is opened to
 * read the code or change it, not to find out how it is set up. The Manage
 * card is only what the operator decides: whether it is online, its tier and
 * domain, its connectivity, a block, and removing it.
 */
export function LambdaDetail({ access, publicKey }: { access: Access; publicKey: string }) {
  const { token, deny, dark, changed } = access;

  const toast = useToast();
  const navigate = useNavigate();

  const [detail, setDetail] = useState<AdminLambdaDetail | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [busy, setBusy] = useState<Busy>(null);
  const [asking, setAsking] = useState<Asking>(null);
  const [tier, setTier] = useState('');
  const [domain, setDomain] = useState('');
  const [domainProblem, setDomainProblem] = useState<string | null>(null);
  const [viewing, setViewing] = useState<{ version: number; files: LambdaFile[] } | null>(null);

  const load = useCallback(async () => {
    try {
      setDetail(await api.admin.lambda(token, publicKey));
      setFailure(null);
    } catch (error) {
      if (error instanceof ApiError && (error.status === 401 || error.status === 403)) {
        deny();
      } else if (error instanceof ApiError && error.status === 404) {
        setFailure(`There is no lambda at "${publicKey}".`);
      } else {
        setFailure('The lambda could not be read.');
      }
    }
  }, [token, publicKey, deny]);

  useEffect(() => {
    load();

    const timer = window.setInterval(() => document.visibilityState === 'visible' && load(), 15_000);

    return () => window.clearInterval(timer);
  }, [load]);

  async function act(kind: Busy, action: () => Promise<void>, failed: string) {
    setBusy(kind);

    try {
      await action();
      changed();
    } catch (error) {
      // the platform's own refusals - a demo, a blocked lambda - say what
      // kind they are; only the token's lock the panel again
      if (error instanceof ApiError && (error.status === 401 || error.status === 403) && !error.kind) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : failed, 'error');
      }
    } finally {
      setBusy(null);
    }
  }

  const back = (
    <Link to="/admin/lambdas" className="inline-flex items-center gap-1 text-[13px] text-accent-600 hover:underline dark:text-accent-400">
      <IconChevronLeft className="h-3.5 w-3.5" />
      All lambdas
    </Link>
  );

  if (!detail) {
    return (
      <div className="px-4 pb-16 pt-6 md:px-0">
        {back}
        <h1 className="mt-3 font-mono text-lg font-semibold">{publicKey}</h1>
        <div className="mt-5">
          <Reading what="the lambda" failure={failure} />
        </div>
      </div>
    );
  }

  const { lambda, traffic, versions, activations, tiers, summary, uses } = detail;

  const live = lambda.activeVersion != null;
  const latest = lambda.latestVersion;
  const blocked = detail.block != null;
  const demo = isDemo(lambda.tier);
  const ownDomain = lambda.tierPolicy.ownDomain;
  const called = (name: string) => (name === lambda.tier && lambda.tierPolicy.label ? `${name} (“${lambda.tierPolicy.label}”)` : name);

  const deploy = (version: number) =>
    act('deploy', async () => {
      setAsking(null);

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
      setAsking(null);
      await api.admin.undeploy(token, lambda.publicKey);
      toast(`"${lambda.publicKey}" is offline.`, 'success');
      await load();
    }, 'The lambda could not be taken offline.');

  const changeTier = () =>
    act('tier', async () => {
      setDetail(await api.admin.tier(token, lambda.publicKey, tier));
      setAsking(null);
      toast(`"${lambda.publicKey}" is in the ${tier} tier now. It keeps running; only what it may have changed.`, 'success');
    }, 'The tier could not be changed.');

  const changeDomain = () => {
    setDomainProblem(null);

    const value = domain.trim() || null;

    return act('domain', async () => {
      try {
        setDetail(await api.admin.domain(token, lambda.publicKey, value));
        setAsking(null);
        toast(value ? `"${lambda.publicKey}" answers at ${value} now.` : 'The domain is removed.', 'success');
      } catch (error) {
        if (error instanceof ApiError && error.status !== 401 && error.status !== 403) {
          setDomainProblem(error.message);
          return;
        }

        throw error;
      }
    }, 'The domain could not be changed.');
  };

  const remove = () =>
    act('remove', async () => {
      setAsking(null);
      await api.admin.remove(token, lambda.publicKey);
      toast(`"${lambda.publicKey}" was removed.`, 'success');
      navigate('/admin/lambdas');
    }, 'The lambda could not be removed.');

  const view = (version: number) =>
    act(null, async () => {
      const content = await api.admin.version(token, lambda.publicKey, version);
      setViewing({ version, files: content.files });
    }, 'The code could not be read.');

  const adopt = (next: AdminLambdaDetail) => {
    setDetail(next);
    changed();
  };

  const day = summary?.traffic;
  const storage = summary?.storage;
  const limits = summary?.limits;
  const docs = summary?.documentation;
  const newest = versions[0];
  const missing = storage?.missingSecrets ?? [];
  const dataBytes = (storage?.databaseBytes ?? 0) + (storage?.workspaceBytes ?? 0);

  return (
    <div className="min-w-0 px-4 pb-16 pt-6 md:px-0">
      {back}

      {/* ------------------------------------------------------------ what it is */}

      <header className="mt-3">
        <div className="flex flex-wrap items-center gap-x-3 gap-y-2">
          <h1 className="min-w-0 break-all font-mono text-xl font-semibold tracking-tight">{lambda.publicKey}</h1>
          <div className="flex flex-wrap items-center gap-1.5">
            {blocked ? <BlockedBadge /> : <StatusBadge version={lambda.activeVersion} />}
            <TierBadge tier={lambda.tier} label={lambda.tierPolicy.label} />
          </div>
        </div>

        <nav aria-label="Where it is" className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-1 text-[13px]">
          <LambdaLink address={lambda.address} className="font-mono text-accent-600 hover:underline dark:text-accent-400" />
          <a href={lambda.editorPath} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-accent-600 hover:underline dark:text-accent-400"
             title="The owner's editor, where the code can be changed as well as read">
            Editor <IconExternal className="h-3 w-3" />
          </a>
          <Link to={`/admin/log?lambda=${encodeURIComponent(lambda.publicKey)}`} className="text-accent-600 hover:underline dark:text-accent-400">Log</Link>
          {detail.source?.published && !blocked && (
            <a href={detail.source.path} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-accent-600 hover:underline dark:text-accent-400">
              Published source <IconExternal className="h-3 w-3" />
            </a>
          )}
          <a href="#manage" className="text-accent-600 hover:underline dark:text-accent-400 lg:hidden">Manage</a>
        </nav>
      </header>

      {/* ------------------------------------------------------- what waits */}

      <Notices detail={detail} dataBytes={dataBytes} />

      <div className="mt-6 grid gap-6 lg:grid-cols-[minmax(0,1fr)_19rem]">
        <div className="min-w-0 space-y-6">
          <Card title="About">
            {docs?.about ? (
              <p className="text-sm leading-relaxed">{docs.about}</p>
            ) : (
              <p className="text-[13px] text-slate-500">Its documentation does not say what it is.</p>
            )}

            <p className="mt-3 flex flex-wrap gap-1.5 text-xs">
              <Page has={docs?.product ?? false}>docs/product.md</Page>
              <Page has={docs?.decisions ?? false}>docs/decisions.md</Page>
              <Page has={docs?.tests ?? false}>tests/README.md</Page>
            </p>

            {newest?.specification && (
              <div className="mt-4 border-t border-grey-300 pt-3 dark:border-ink-800">
                <p className="text-xs text-slate-500">What was asked for, with version {newest.version}</p>
                <blockquote className="mt-1 line-clamp-4 whitespace-pre-line break-words border-l-2 border-grey-300 pl-3 text-[13px] text-slate-600 dark:border-ink-700 dark:text-slate-400" title={newest.specification}>
                  {newest.specification}
                </blockquote>
              </div>
            )}
          </Card>

          {day && (
            <Tiles>
              <Tile label="Requests, last day" value={count(day.dayRequests)} detail={day.upgrades > 0 ? `and ${count(day.upgrades)} websockets` : `${count(day.hourRequests)} in the last hour`}>
                <Sparkline values={day.hourly} label="Requests per hour over the last day" />
              </Tile>
              <Tile label="Server errors, last day" value={day.dayRequests > 0 ? percent(day.dayFailed, day.dayRequests) : '-'}
                    detail={`${count(day.dayFailed)} of them`} tone={day.dayFailed > 0 ? 'bad' : undefined} />
              <Tile label="Answer time" value={day.dayRequests > 0 ? millis(day.averageMillis) : '-'} detail="on average, over the last day" />
              <Tile label="Last visit" value={day.lastSeen ? <Ago at={day.lastSeen} /> : 'none yet'} detail="since the server came up" />
            </Tiles>
          )}

          <Card title="What it uses" hint="Of the platform's features, as its owner and its agent set them up.">
            <ul className="grid gap-px border border-grey-300 bg-grey-300 dark:border-ink-800 dark:bg-ink-800 sm:grid-cols-2 xl:grid-cols-3">
              <Use id="database" on={storage?.databaseEnabled ?? uses?.database ?? false}>
                {storage?.databaseEnabled
                  ? <>{plural(storage.databaseTables, 'table')} · {bytes(storage.databaseBytes)}{!storage.usesDatabase && ' · its code does not use it'}</>
                  : storage?.usesDatabase ? <span className="text-amber-600 dark:text-amber-400">Off, though its code uses it</span> : 'Off'}
              </Use>
              <Use id="workspace" on={storage?.workspaceEnabled ?? uses?.workspace ?? false}>
                {storage?.workspaceEnabled
                  ? <>{plural(storage.workspaceFiles, 'file')} · {bytes(storage.workspaceBytes)}{storage.servesWorkspace && ' · served publicly'}</>
                  : 'Switched off'}
              </Use>
              <Use id="secrets" on={storage?.secretsEnabled ?? uses?.secrets ?? false}>
                {storage?.secretsEnabled
                  ? <>{plural(storage.secrets, 'secret')} kept{missing.length > 0 && <span className="text-amber-600 dark:text-amber-400"> · waits for {missing.join(', ')}</span>}</>
                  : 'Off'}
              </Use>
              <Use id="analytics" on={uses?.analyticsSince != null}>
                {uses?.analyticsSince ? <>Counted since <Ago at={uses.analyticsSince} /></> : 'Not counted'}
              </Use>
              <Use id="showcase" on={detail.showcase != null}>
                {detail.showcase ? <>“{detail.showcase.title}”{!detail.showcase.online && ' · not listed while offline'}</> : 'Not listed'}
              </Use>
              <Use id="source" on={detail.source?.published ?? false}>
                {detail.source?.published
                  ? <>{detail.source.license.id} · {plural(detail.source.stars, 'star')}</>
                  : detail.source ? 'Taken down' : 'Not published'}
              </Use>
              <Use id="domain" on={lambda.domain != null}>
                {lambda.domain
                  ? <span className={lambda.domainServed ? 'font-mono' : 'font-mono text-slate-400 line-through'} title={lambda.domainServed ? undefined : 'Configured, but not served in a tier without domains'}>{lambda.domain}</span>
                  : 'None'}
              </Use>
              <Use id="connects" on={lambda.connectivity.allowed}>
                {connectivityWord(lambda.connectivity)} · its code {detail.connects ? 'connects' : 'does not connect'}
              </Use>
              <Use id="drafts" on={(detail.features?.length ?? 0) > 0}>
                {detail.features && detail.features.length > 0
                  ? detail.features.map((f) => `${f.name}${f.online ? ' (preview online)' : ''}`).join(', ')
                  : 'None open'}
              </Use>
            </ul>
            <p className="mt-3 text-xs text-slate-500">
              Its editor opens in the {lambda.view === 'Simple' ? 'simple view, for an owner who does not write code' : 'full view, for a developer'}.
            </p>
          </Card>

          {storage && limits && (
            <Card title="What it keeps" hint="Against what its tier allows. The version is the one online, or the newest.">
              <div className="space-y-4">
                <Meter label={`Version ${storage.version ?? ''} - code and resources`} used={storage.codeBytes + storage.resourceBytes} of={limits.buildBytes} format={bytes}
                       extra={<span className="text-xs text-slate-500">{plural(storage.codeFiles + storage.resourceFiles, 'file')}</span>} />
                <Meter label="Data - database and workspace" used={dataBytes} of={limits.dataBytes} format={bytes} />
              </div>
            </Card>
          )}

          <Card title="Traffic since the server came up" hint="Counted in memory, by the minute over the last hour.">
            {traffic.minutes.some((p) => p.requests > 0 || p.upgrades > 0) ? (
              <Chart
                title="Requests"
                labels={traffic.minutes.map((p) => clock(p.at))}
                dark={dark}
                shape="stacked"
                height={150}
                format={(v) => count(Math.round(v))}
                series={[
                  { label: 'Answered', color: ['#1a73e8', '#4285f4'], values: traffic.minutes.map((p) => Math.max(0, p.requests - p.failed - p.rejected)) },
                  { label: 'Not found or refused', color: ['#e8710a', '#d56e0c'], values: traffic.minutes.map((p) => p.rejected) },
                  { label: 'Failed', color: ['#c5221f', '#ea4335'], values: traffic.minutes.map((p) => p.failed) },
                ]}
              />
            ) : (
              <p className="text-[13px] text-slate-500">Nothing in the last hour.</p>
            )}

            <Entrances traffic={traffic} publicUrl={lambda.publicUrl} />

            {traffic.paths.length > 0 && (
              <table className="mt-6 w-full text-left text-[13px]">
                <thead className="text-slate-500">
                  <tr className="border-b border-grey-300 dark:border-ink-800">
                    <th className="py-2 pr-3 font-normal">Most asked for</th>
                    <th className="py-2 pr-3 text-right font-normal">Requests</th>
                    <th className="py-2 pr-3 text-right font-normal">Failed</th>
                    <th className="py-2 text-right font-normal">Average</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-grey-300 dark:divide-ink-800">
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
          </Card>

          {summary && summary.recentProblems.length > 0 && (
            <Card title="Recent problems" hint="The last errors of its own log, and what the server said about it." flush
                  more={{ to: `/admin/log?lambda=${encodeURIComponent(lambda.publicKey)}&level=warn`, label: 'Log' }}>
              <ul className="divide-y divide-grey-300 dark:divide-ink-800">
                {summary.recentProblems.map((line) => (
                  <li key={line.seq} className="flex items-start gap-3 px-4 py-2.5 text-[13px]">
                    <LevelMark level={line.level} />
                    <div className="min-w-0 flex-1">
                      <p className="break-words font-mono text-[12px]">{line.text}</p>
                      <p className="mt-0.5 text-xs text-slate-500">{line.source} · <Ago at={line.at} /></p>
                    </div>
                  </li>
                ))}
              </ul>
            </Card>
          )}

          <div className="grid gap-6 2xl:grid-cols-2">
            <Card title="Versions" hint={`The newest first, ${plural(versions.length, 'version')} kept.`} flush>
              {versions.length === 0 ? (
                <Empty>Nothing saved.</Empty>
              ) : (
                <ul className="divide-y divide-grey-300 text-[13px] dark:divide-ink-800">
                  {versions.map((version) => (
                    <li key={version.version} className="flex items-start gap-3 px-4 py-2.5">
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
                        {version.version !== lambda.activeVersion && !blocked && !demo && (
                          <button type="button" onClick={() => setAsking({ what: 'deploy', version: version.version })} disabled={busy !== null} className="btn-ghost !px-2 !py-0.5 text-xs">
                            Put online…
                          </button>
                        )}
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </Card>

            <Card title="Deployments" hint="When it was online, the newest first." flush>
              {activations.length === 0 ? (
                <Empty>Never online.</Empty>
              ) : (
                <ul className="divide-y divide-grey-300 text-[13px] dark:divide-ink-800">
                  {activations.slice(0, 15).map((activation) => (
                    <li key={`${activation.started}-${activation.version}`} className="flex items-start gap-3 px-4 py-2.5">
                      <span className="w-8 shrink-0 tabular-nums text-slate-500">v{activation.version}</span>
                      <span className="min-w-0 flex-1">
                        <span>By {origin(activation.origin)}, <Ago at={activation.started} /></span>
                        <span className="block text-slate-500">
                          {activation.ended ? `${span(activation.seconds)}, ${ending(activation.endedBy)}` : `online for ${span(activation.seconds)}`}
                        </span>
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </Card>
          </div>

          <Card title="Its lifetime">
            <Facts>
              <Fact label="Created"><Ago at={lambda.created} /></Fact>
              <Fact label="Last changed"><Ago at={lambda.modified} /></Fact>
              <Fact label="Online">
                {live ? <>Version {lambda.activeVersion}{lambda.deployedAt && <>, since <Ago at={lambda.deployedAt} /></>}</> : 'No'}
              </Fact>
              <Fact label="Goes offline">
                {lambda.deployedUntil ? <><Ago at={lambda.deployedUntil} /> unless used</> : live ? 'Not by itself' : '-'}
              </Fact>
              <Fact label="Removed">
                {lambda.keptUntil ? <><Ago at={lambda.keptUntil} /> unless used</> : blocked ? 'When you remove it' : 'Kept by its tier, however quiet'}
              </Fact>
              {limits && <Fact label="Its tier allows">{plural(limits.versions, 'version')} kept, {plural(limits.features, 'draft')} at once</Fact>}
            </Facts>
          </Card>

          <Card title="Changed from" hint="The addresses it was changed from with its editor key in the last 30 days - what a block can block with it." flush>
            {detail.editorAddresses.length === 0 ? (
              <p className="px-4 py-4 text-[13px] text-slate-500">
                None known: it was not changed from outside the installation’s own networks lately, or this server keeps no addresses.
              </p>
            ) : (
              <ul className="divide-y divide-grey-300 text-[13px] dark:divide-ink-800">
                {detail.editorAddresses.map((editor) => (
                  <li key={editor.address} className="flex flex-wrap items-center gap-x-3 gap-y-1 px-4 py-2.5">
                    <span className="break-all font-mono">{editor.range}</span>
                    {detail.you != null && editor.range === detail.you && (
                      <span className="rounded-full bg-amber-500/15 px-2 py-0.5 text-[11px] font-medium text-amber-700 dark:text-amber-400">your address</span>
                    )}
                    {editor.blocked && (
                      <span className="rounded-full bg-red-500/10 px-2 py-0.5 text-[11px] font-medium text-red-600 dark:text-red-400">blocked</span>
                    )}
                    <span className="ml-auto text-slate-500">first <Ago at={editor.firstSeen} />, last <Ago at={editor.lastSeen} /></span>
                  </li>
                ))}
              </ul>
            )}
          </Card>
        </div>

        {/* ---------------------------------------------- what the operator does */}

        <aside id="manage" className="min-w-0 scroll-mt-4 lg:sticky lg:top-6 lg:self-start">
          <Actions hint="What you decide about it. Each asks before it acts, and is recorded as yours.">
            <Action
              label="Online"
              state={blocked ? 'Offline while blocked' : live ? `Version ${lambda.activeVersion}` : 'Offline'}
              note={demo ? 'A demo is kept online by the installation.' : undefined}
            >
              {!blocked && !demo && latest != null && latest !== lambda.activeVersion && (
                <button type="button" onClick={() => setAsking({ what: 'deploy', version: latest })} disabled={busy !== null} className={`btn-primary ${actionButton}`}>
                  {busy === 'deploy' && <IconSpinner />}
                  Put v{latest} online…
                </button>
              )}
              {!blocked && !demo && live && (
                <button type="button" onClick={() => setAsking({ what: 'undeploy' })} disabled={busy !== null} className={`btn-ghost ${actionButton}`}>
                  Take offline…
                </button>
              )}
            </Action>

            <Action label="Tier" state={called(lambda.tier)} note={demo ? 'The demos are the seeder’s to place.' : undefined}>
              {!demo && (
                <button type="button" onClick={() => { setTier(lambda.tier); setAsking({ what: 'tier' }); }} disabled={busy !== null} className={`btn-ghost ${actionButton}`}>
                  Change…
                </button>
              )}
            </Action>

            <Action
              label="Domain"
              state={lambda.domain ? <span className="font-mono">{lambda.domain}</span> : 'None'}
              note={blocked ? 'Kept as it is while the lambda is blocked.' : !ownDomain && !lambda.domain ? 'Only production and system lambdas may have one.' : undefined}
            >
              {!blocked && !demo && (ownDomain || lambda.domain) && (
                <button
                  type="button"
                  onClick={() => { setDomain(lambda.domain ?? ''); setDomainProblem(null); setAsking({ what: 'domain' }); }}
                  disabled={busy !== null}
                  className={`btn-ghost ${actionButton}`}
                >
                  Change…
                </button>
              )}
            </Action>

            <ConnectivityAction token={token} detail={detail} onChanged={adopt} deny={deny} />

            <BlockAction token={token} detail={detail} onChanged={adopt} deny={deny} />

            <Action label="Remove" note={demo ? 'A demo is removed with the installation’s demos only.' : 'Its code, versions and data, for good.'}>
              {!demo && (
                <button type="button" onClick={() => setAsking({ what: 'remove' })} disabled={busy !== null} className={`btn-danger ${actionButton}`}>
                  Remove…
                </button>
              )}
            </Action>
          </Actions>
        </aside>
      </div>

      {/* ---------------------------------------------------------- dialogs */}

      <Dialog
        title={asking?.what === 'deploy' ? `Put version ${asking.version} of "${lambda.publicKey}" online?` : `Take "${lambda.publicKey}" offline?`}
        open={asking?.what === 'deploy' || asking?.what === 'undeploy'}
        onClose={() => setAsking(null)}
        footer={
          <>
            <button type="button" onClick={() => setAsking(null)} className="btn-ghost">Cancel</button>
            {asking?.what === 'deploy' ? (
              <button type="button" onClick={() => deploy(asking.version)} disabled={busy !== null} className="btn-primary">
                {busy === 'deploy' && <IconSpinner />}
                Put it online
              </button>
            ) : (
              <button type="button" onClick={undeploy} disabled={busy !== null} className="btn-danger">
                {busy === 'undeploy' && <IconSpinner />}
                Take it offline
              </button>
            )}
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          {asking?.what === 'deploy'
            ? <>It is compiled first, and put online only where it compiles. {live ? `Version ${lambda.activeVersion} goes offline with it. ` : ''}Its data stays as it is. Its owner sees it was you.</>
            : 'Its address stops answering until it is put online again. Nothing of it is removed, and its owner can put it online again.'}
        </p>
      </Dialog>

      <Dialog
        title={`Move "${lambda.publicKey}" to another tier?`}
        open={asking?.what === 'tier'}
        onClose={() => setAsking(null)}
        footer={
          <>
            <button type="button" onClick={() => setAsking(null)} className="btn-ghost">Cancel</button>
            <button type="button" onClick={changeTier} disabled={busy !== null || tier === lambda.tier} className="btn-primary">
              {busy === 'tier' && <IconSpinner />}
              Move it
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          Moving it changes what it may have, not what it has: it keeps running, its data stays, and what it saves next is held to
          the new tier.
        </p>
        <div role="radiogroup" aria-label="Tier" className="mt-4 space-y-2">
          {tiers.map((name) => {
            // a system lambda is never blocked, so a blocked one is not moved there
            const closed = blocked && name === 'System' && name !== lambda.tier;

            return (
              <label key={name} className={`flex items-start gap-2.5 text-sm ${closed ? 'opacity-50' : 'cursor-pointer'}`}>
                <input type="radio" name="tier" className="mt-0.5 accent-accent-500" checked={tier === name} disabled={closed} onChange={() => setTier(name)} />
                <span>
                  <span className="font-medium">{called(name)}</span>
                  {name === lambda.tier && <span className="text-slate-500"> - now</span>}
                  <span className="block text-[13px] text-slate-500">
                    {closed ? 'Not while it is blocked: a system lambda is never blocked. Lift the block first.' : TIER_PURPOSE[name] ?? ''}
                  </span>
                </span>
              </label>
            );
          })}
        </div>
        {lambda.domain && tier === 'Sandbox' && (
          <p className="mt-3 text-[13px] text-amber-600 dark:text-amber-400">{lambda.domain} is kept, but not served while it is in the sandbox.</p>
        )}
      </Dialog>

      <Dialog
        title={`The domain of "${lambda.publicKey}"`}
        open={asking?.what === 'domain'}
        onClose={() => setAsking(null)}
        footer={
          <>
            <button type="button" onClick={() => setAsking(null)} className="btn-ghost">Cancel</button>
            <button
              type="button"
              onClick={changeDomain}
              disabled={busy !== null || domain.trim() === (lambda.domain ?? '') || (!ownDomain && domain.trim() !== '')}
              className={domain.trim() === '' && lambda.domain ? 'btn-danger' : 'btn-primary'}
            >
              {busy === 'domain' && <IconSpinner />}
              {domain.trim() === '' && lambda.domain ? 'Remove the domain' : 'Save'}
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          {ownDomain
            ? 'Its owner sets this in their editor; you can correct it or remove it. It has to point at this server.'
            : 'Only a lambda in production or the system tier can be given one. Removing it is always possible.'}
        </p>
        <input
          value={domain}
          onChange={(event) => setDomain(event.target.value)}
          placeholder={ownDomain ? 'shop.example.com' : 'production only'}
          disabled={!ownDomain && !lambda.domain}
          spellCheck={false}
          autoComplete="off"
          aria-label="Domain"
          className="field mt-4 font-mono"
        />
        {domainProblem && <p className="mt-2 text-xs text-red-500">{domainProblem}</p>}
      </Dialog>

      <Dialog
        title={`Remove "${lambda.publicKey}"?`}
        open={asking?.what === 'remove'}
        onClose={() => setAsking(null)}
        footer={
          <>
            <button type="button" onClick={() => setAsking(null)} className="btn-ghost">Keep it</button>
            <button type="button" onClick={remove} disabled={busy !== null} className="btn-danger">
              {busy === 'remove' && <IconSpinner />}
              Remove
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          Its code, its versions, its data{lambda.domain ? ' and its domain' : ''} go with it. Whoever holds the editor link will find
          nothing there, and there is no undo.
        </p>
      </Dialog>

      <Dialog title={`Version ${viewing?.version} of ${lambda.publicKey}`} open={viewing !== null} onClose={() => setViewing(null)} wide>
        <div className="max-h-[60vh] space-y-4 overflow-auto">
          {viewing?.files.map((file) => (
            <div key={file.name}>
              <p className="mb-1 font-mono text-xs text-slate-500">{file.name}</p>
              <pre className="overflow-auto border border-grey-300 p-3 font-mono text-[12.5px] leading-relaxed dark:border-ink-800">
                {file.encoding === 'base64' ? '(not text)' : file.code}
              </pre>
            </div>
          ))}
        </div>
      </Dialog>
    </div>
  );
}

/** What each tier is for, in the dialog that moves a lambda. */
const TIER_PURPOSE: Record<string, string> = {
  Sandbox: 'Trying something: taken offline once unused, and removed later.',
  Production: 'An app people use: kept, with a domain of its own and more room.',
  System: 'What this installation depends on: kept, never blocked, never in the showcase, and deleted only here.',
};

/**
 * What waits about this lambda, or what the operator should know before
 * reading on: a block and why, a request to connect, data beyond its room,
 * keys its code waits for, and an end coming up.
 */
function Notices({ detail, dataBytes }: { detail: AdminLambdaDetail; dataBytes: number }) {
  const { lambda, block, summary } = detail;
  const connectivity = lambda.connectivity;

  const notices: ReactNode[] = [];

  if (isDemo(lambda.tier)) {
    notices.push(
      <Notice key="demo" tone="info" title="A demo of this installation">Read only, through every door; anybody starts a copy of it.</Notice>,
    );
  }

  if (block) {
    notices.push(
      <Notice key="block" tone="problem" title={<>Blocked <Ago at={block.since} /></>}>
        {block.reason ? <>“{block.reason}” - its owner reads this.</> : 'No reason given.'} Offline, and every change is refused until you lift the block.
      </Notice>,
    );
  }

  if (connectivity.status === 'Requested' && !block) {
    notices.push(
      <Notice key="connectivity" tone="wanted" title={<>Asks to connect to other servers, <Ago at={connectivity.changed} /></>}>
        <span className="whitespace-pre-line break-words">“{connectivity.purpose}”</span>
        {' '}Its code {detail.connects ? 'connects, and does not compile until you allow it' : 'does not connect yet'}. Decide under Manage.
      </Notice>,
    );
  } else if (connectivity.note && connectivity.changed) {
    notices.push(
      <Notice key="note" tone="info" title={<>Connectivity {connectivity.status === 'Declined' ? 'declined' : 'allowed'} <Ago at={connectivity.changed} /></>}>
        {connectivity.purpose && <>Asked for: “{connectivity.purpose}”. </>}You said: “{connectivity.note}”
      </Notice>,
    );
  }

  if (summary && dataBytes > summary.limits.dataBytes) {
    notices.push(
      <Notice key="over" tone="wanted" title="Holds more data than its tier allows">
        {bytes(dataBytes)} of {bytes(summary.limits.dataBytes)}. It is kept, and nothing more is written until it is within its allowance.
      </Notice>,
    );
  }

  const missing = summary?.storage.missingSecrets ?? [];

  if (missing.length > 0) {
    notices.push(
      <Notice key="secrets" tone="info" title={`Its code waits for ${plural(missing.length, 'secret')}`}>
        {missing.join(', ')} - its owner enters {missing.length === 1 ? 'it' : 'them'} in the editor, and what reads {missing.length === 1 ? 'it' : 'them'} fails until then.
      </Notice>,
    );
  }

  const soon = Date.now() + 7 * 24 * 3600 * 1000;

  if (!block && (lambda.deployedUntil && new Date(lambda.deployedUntil).getTime() < soon || lambda.keptUntil && new Date(lambda.keptUntil).getTime() < soon)) {
    notices.push(
      <Notice key="expiring" tone="info" title="Expires this week unless it is used">
        {lambda.deployedUntil && <>Goes offline <Ago at={lambda.deployedUntil} />. </>}
        {lambda.keptUntil && <>Removed <Ago at={lambda.keptUntil} />. </>}
        Moving it to production keeps it.
      </Notice>,
    );
  }

  return notices.length === 0 ? null : <div className="mt-5 space-y-2">{notices}</div>;
}

/** A feature the lambda may use: whether it does, and what of it. */
function Use({ id, on, children }: { id: string; on: boolean; children: ReactNode }) {
  const known = FEATURES.find((f) => f.id === id)!;
  const Icon = known.icon;

  return (
    <li className="flex items-start gap-3 bg-white px-3 py-3 dark:bg-ink-900">
      <span className={`mt-0.5 shrink-0 ${on ? 'text-accent-600 dark:text-accent-400' : 'text-slate-300 dark:text-slate-600'}`}>
        <Icon className="h-4 w-4" />
      </span>
      <span className="min-w-0">
        <span className={`block text-[13px] font-medium ${on ? '' : 'text-slate-500'}`}>{known.label}</span>
        <span className="block break-words text-xs text-slate-500">{children}</span>
      </span>
    </li>
  );
}

/** A page of its documentation, there or not. */
function Page({ has, children }: { has: boolean; children: ReactNode }) {
  return (
    <span className={`rounded-full px-2 py-0.5 font-mono ${has ? 'bg-emerald-500/10 text-emerald-700 dark:text-emerald-400' : 'bg-slate-400/10 text-slate-400 line-through'}`}
          title={has ? 'Written' : 'Missing'}>
      {children}
    </span>
  );
}

/** That the operator blocked a lambda, beside its other badges. */
export function BlockedBadge() {
  return (
    <span
      title="Blocked by the operator: offline, and nothing about it can be changed"
      className="inline-flex items-center whitespace-nowrap rounded-full bg-red-500/10 px-2 py-0.5 text-[11px] font-medium text-red-600 dark:text-red-400"
    >
      Blocked
    </span>
  );
}

