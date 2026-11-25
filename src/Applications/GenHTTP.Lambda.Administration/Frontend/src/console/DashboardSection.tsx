import { useEffect, useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';

import { api, refusedToken, type AdminOverview, type AdminStorage, type Telemetry } from '@genhttp-lambda/kit/api';
import { Chart } from '@genhttp-lambda/kit/components/Chart';
import { Ago, Section, Sparkline, StatusBadge, TierBadge } from '@genhttp-lambda/kit/ui/ui';
import { BlockedBadge } from './LambdaDetail';
import { BLUE, Fill, GREEN, GREY, PURPLE } from './charts';
import { diskState, lambdaPage, lambdasWhere, type Access } from './context';
import { AllClear, Card, Fact, Facts, Notice, Reading, Tile, Tiles, bytes, count, duration, plural, share, since } from './parts';
import { UseMarks } from './uses';

/**
 * Where the operator starts: what waits for them, then the installation at a
 * glance - its lambdas, the machine, the room left - each leading on to the
 * section that has the rest. Nothing is decided here.
 */
export function DashboardSection({ access }: { access: Access }) {
  const { token, deny, dark, overview } = access;

  const [telemetry, setTelemetry] = useState<Telemetry | null>(null);
  const [storage, setStorage] = useState<AdminStorage | null>(null);

  useEffect(() => {
    const failed = (error: unknown) => refusedToken(error) && deny();

    api.telemetry(60, token, 14).then(setTelemetry).catch(failed);
    api.admin.storage(token).then(setStorage).catch(failed);
  }, [token, deny]);

  if (overview === null) {
    return (
      <Section title="Dashboard">
        <Reading what="the installation" />
      </Section>
    );
  }

  const { lambdas, disk } = overview;
  const notices = needs(overview, storage);

  const requests = telemetry?.samples.reduce((sum, s) => sum + s.requests, 0) ?? null;
  const failures = telemetry?.samples.reduce((sum, s) => sum + s.failed, 0) ?? 0;
  const pick = (kind: string) => telemetry?.events?.series.find((s) => s.kind === kind)?.counts ?? [];
  const state = diskState(disk);

  return (
    <Section title="Dashboard" hint="What waits for you, and how the installation is doing. Read again whenever you come back to it.">
      <section aria-label="Needs attention" className="space-y-2">
        <h2 className="mb-2 text-sm font-medium">Needs attention</h2>
        {notices.length === 0 ? <AllClear>Nothing waits for you.</AllClear> : notices}
      </section>

      <div className="mt-8">
        <Tiles>
          <Tile label="Lambdas" value={count(lambdas.total)} to="/admin/lambdas"
                detail={`${count(lambdas.online)} online${lambdas.blocked > 0 ? ` · ${count(lambdas.blocked)} blocked` : ''}`} />
          <Tile label="New this week" value={count(lambdas.createdWeek)} to={lambdasWhere({ created: '7' })}
                detail={`${count(lambdas.createdDay)} today`} />
          <Tile label="Requests, last hour" value={requests === null ? '…' : count(requests)} to="/admin/server"
                detail={failures > 0 ? <span className="text-red-600 dark:text-red-400">{count(failures)} server errors</span> : 'no server errors'}>
            {telemetry && <Sparkline values={telemetry.samples.map((s) => s.requests)} label="Requests per interval over the last hour" />}
          </Tile>
          <Tile label="Free storage" value={disk ? bytes(disk.free) : 'unknown'} to="/admin/storage"
                tone={state === 'bad' ? 'bad' : state === 'warn' ? 'warn' : undefined}
                detail={disk ? `of ${bytes(disk.size)}, ${share(disk.free, disk.size)} free` : 'the system does not say'}>
            {disk && <Fill used={disk.size - disk.free} of={disk.size} label="Used of the volume" />}
          </Tile>
        </Tiles>
      </div>

      <div className="mt-6 grid gap-6 xl:grid-cols-2">
        <Card title="Newest lambdas" more={{ to: '/admin/lambdas', label: 'All lambdas' }} flush>
          {overview.newest.length === 0 ? (
            <p className="px-4 py-6 text-[13px] text-slate-500">There are no lambdas yet.</p>
          ) : (
            <ul className="divide-y divide-grey-300 dark:divide-ink-800">
              {overview.newest.map((lambda) => (
                <li key={lambda.publicKey} className="flex flex-wrap items-center gap-x-3 gap-y-1 px-4 py-2.5 text-[13px]">
                  <Link to={lambdaPage(lambda.publicKey)} className="min-w-0 truncate font-mono text-accent-600 hover:underline dark:text-accent-400">
                    {lambda.publicKey}
                  </Link>
                  {lambda.blocked ? <BlockedBadge /> : <StatusBadge version={lambda.activeVersion} />}
                  <TierBadge tier={lambda.tier} label={lambda.tierLabel} />
                  <UseMarks uses={lambda.uses} connectivity={lambda.connectivity} />
                  <span className="ml-auto text-slate-500"><Ago at={lambda.created} /></span>
                </li>
              ))}
            </ul>
          )}
        </Card>

        {telemetry?.events && telemetry.events.days.length > 1 ? (
          <Chart
            title="What people did, the last two weeks"
            hint="Per day. More under Statistics."
            labels={telemetry.events.days.map((d) => d.slice(5))}
            dark={dark}
            format={(v) => count(v)}
            shape="step"
            height={170}
            series={[
              { label: 'Created', color: BLUE, values: pick('created') },
              { label: 'Saved', color: PURPLE, values: pick('saved') },
              { label: 'Deployed', color: GREEN, values: pick('deployed') },
              { label: 'Deleted', color: GREY, values: pick('deleted') },
            ]}
          />
        ) : (
          <Card title="What people did, the last two weeks">
            <Reading what="what people did" />
          </Card>
        )}

        <Card title="Server" more={{ to: '/admin/server', label: 'Server' }}>
          {telemetry ? (
            <Facts>
              <Fact label="Running for">{duration(telemetry.server.uptimeSeconds)}</Fact>
              {/* of a process shared with other installations, nothing is read */}
              {!telemetry.server.shared && (
                <>
                  <Fact label="CPU">{telemetry.latest.cpuPercentage.toFixed(1)}%</Fact>
                  <Fact label="Working set">{bytes(telemetry.latest.workingSetBytes)}</Fact>
                  <Fact label="Managed heap">{bytes(telemetry.latest.managedBytes)}</Fact>
                  <Fact label="Open sockets">{count(telemetry.traffic.openSockets)}</Fact>
                </>
              )}
              <Fact label="Engine">{telemetry.server.engine}, GenHTTP {telemetry.server.version}</Fact>
            </Facts>
          ) : (
            <Reading what="the server" />
          )}
        </Card>

        <Card title="Storage" more={{ to: '/admin/storage', label: 'Storage' }}>
          <StorageGlance storage={storage} disk={disk} />
        </Card>
      </div>
    </Section>
  );
}

/** The room on the volume, and how much of it the installation takes. */
function StorageGlance({ storage, disk }: { storage: AdminStorage | null; disk: AdminOverview['disk'] }) {
  if (!disk) {
    return <p className="text-[13px] text-slate-500">The system does not say how large the volume is.</p>;
  }

  const ours = storage?.parts.reduce((sum, p) => sum + p.bytes, 0) ?? null;

  return (
    <Facts columns={2}>
      <Fact label="Free">{bytes(disk.free)} of {bytes(disk.size)}</Fact>
      <Fact label="Taken by this installation">
        {ours === null ? '…' : <>{bytes(ours)}{storage?.measured && <span className="text-slate-500"> · measured {since(storage.measured)} ago</span>}</>}
      </Fact>
      {storage && storage.largest[0] && (
        <Fact label="The largest lambda">
          <Link to={lambdaPage(storage.largest[0].publicKey)} className="font-mono text-accent-600 hover:underline dark:text-accent-400">
            {storage.largest[0].publicKey}
          </Link>{' '}
          <span className="text-slate-500">{bytes(storage.largest[0].totalBytes)}</span>
        </Fact>
      )}
      {storage && (
        <Fact label="Beyond their allowance">{storage.over.length === 0 ? 'none' : plural(storage.over.length, 'lambda')}</Fact>
      )}
    </Facts>
  );
}

/**
 * What waits for the operator, the most urgent first: what is wrong, then
 * what wants a decision, then what is worth knowing.
 */
function needs(overview: AdminOverview, storage: AdminStorage | null): ReactNode[] {
  const { attention, lambdas, failing, disk } = overview;

  const problems: ReactNode[] = [];
  const wanted: ReactNode[] = [];
  const info: ReactNode[] = [];

  if (attention.youAreBlocked) {
    problems.push(
      <Notice key="you" tone="problem" title="Your own address is blocked" to="/admin/blocks" action="Blocked addresses">
        This panel still answers you; nothing else on this server does until you lift the block.
      </Notice>,
    );
  }

  const state = diskState(disk);

  if (disk && state) {
    (state === 'bad' ? problems : wanted).push(
      <Notice key="disk" tone={state === 'bad' ? 'problem' : 'wanted'} title={state === 'bad' ? 'The volume has almost run out of room' : 'The volume is running out of room'} to="/admin/storage" action="Storage">
        {bytes(disk.free)} of {bytes(disk.size)} are free. Lambdas that cannot write their data fail, and so do saves.
      </Notice>,
    );
  }

  if (failing.length > 0) {
    problems.push(
      <Notice key="failing" tone="problem" title={`${plural(failing.length, 'lambda')} answered with server errors in the last hour`} to="/admin/log?level=error" action="Log">
        {failing.slice(0, 5).map((lambda, i) => (
          <span key={lambda.publicKey}>
            {i > 0 && ', '}
            <Link to={lambdaPage(lambda.publicKey)} className="font-mono text-accent-600 hover:underline dark:text-accent-400">{lambda.publicKey}</Link>
            {' '}({count(lambda.hourFailed)} of {count(lambda.hourRequests)})
          </span>
        ))}
        {failing.length > 5 && <> and {count(failing.length - 5)} more</>}
      </Notice>,
    );
  }

  if (attention.connectivityWaiting > 0) {
    wanted.push(
      <Notice key="connectivity" tone="wanted" title={`${plural(attention.connectivityWaiting, 'lambda asks', 'lambdas ask')} to connect to other servers`} to="/admin/connectivity" action="Decide">
        {attention.connectivityWaitingSince && <>The longest has waited for {since(attention.connectivityWaitingSince)}. </>}
        Their code that connects does not compile until you allow it.
      </Notice>,
    );
  }

  if (storage && storage.over.length > 0) {
    wanted.push(
      <Notice key="over" tone="wanted" title={`${plural(storage.over.length, 'lambda holds', 'lambdas hold')} more data than their tier allows`} to="/admin/storage" action="Storage">
        It is kept, and nothing more is written until it is within the allowance - from a move to a smaller tier, or a lower limit.
      </Notice>,
    );
  }

  if (attention.usageReportFailure) {
    wanted.push(
      <Notice key="report" tone="wanted" title="The last usage report did not arrive" to="/admin/usage" action="Usage report">
        {attention.usageReportFailure}
      </Notice>,
    );
  }

  if (lambdas.expiring > 0) {
    info.push(
      <Notice key="expiring" tone="info" title={`${plural(lambdas.expiring, 'unused lambda expires', 'unused lambdas expire')} this week`} to={lambdasWhere({ state: 'expiring' })} action="Show them">
        {[
          lambdas.goingOffline > 0 && `${plural(lambdas.goingOffline, 'goes', 'go')} offline`,
          lambdas.removed > 0 && `${plural(lambdas.removed, 'is', 'are')} removed`,
        ].filter(Boolean).join(', ')} within seven days unless used.
      </Notice>,
    );
  }

  if (lambdas.createdDay > 0) {
    info.push(
      <Notice key="new" tone="info" title={`${plural(lambdas.createdDay, 'lambda was', 'lambdas were')} created today`} to={lambdasWhere({ created: '1' })} action="Show them" />,
    );
  }

  return [...problems, ...wanted, ...info];
}
