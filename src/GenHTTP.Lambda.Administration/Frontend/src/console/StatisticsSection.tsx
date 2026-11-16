import { useEffect, useState } from 'react';

import { api, refusedToken, type Activity, type AdminStorage, type EventHistory } from '@genhttp-lambda/kit/api';
import { Chart } from '@genhttp-lambda/kit/components/Chart';
import { Pills, Section } from '@genhttp-lambda/kit/ui/ui';
import { BLUE, GREEN, GREY, ORANGE, PURPLE, RED, Ranked, Segments } from './charts';
import { lambdaPage, lambdasWhere, type Access } from './context';
import { Card, Reading, Tile, Tiles, bytes, count, plural, share } from './parts';
import { FEATURES } from './uses';

/** What each kind of event is called, and what it counts. */
const EVENTS: { kind: string; label: string; note: string }[] = [
  { kind: 'created', label: 'Created', note: 'Lambdas people have made, demos excluded. Counted from the beginning: the older half of this was recovered from the timestamps already stored rather than started from zero when the counting began.' },
  { kind: 'saved', label: 'Saved', note: 'Versions written. One per save, so a lambda worked on all afternoon contributes many.' },
  { kind: 'deployed', label: 'Deployed', note: 'Times something was put online, including redeployments of the same lambda. Only this one and the two after it begin from the day they started being recorded - a redeploy used to overwrite the timestamp of the one before it.' },
  { kind: 'undeployed', label: 'Undeployed', note: 'Times something was taken offline by hand, rather than by expiring.' },
  { kind: 'deleted', label: 'Deleted', note: 'Lambdas removed. The lambda is gone; the fact that it existed is not.' },
];

const RANGES = [
  { days: 30, label: '30 days' },
  { days: 90, label: '90 days' },
  { days: 365, label: 'A year' },
];

/** Where the versions came from, in the order the bar shows them; the rest is other. */
const ORIGINS = [
  { origin: 'agent', label: 'An agent, over MCP', color: BLUE },
  { origin: 'api', label: 'The REST API', color: ORANGE },
  { origin: 'git', label: 'Pushed with git', color: PURPLE },
  { origin: 'template', label: 'A new lambda’s first', color: GREEN },
];

/**
 * The lambdas of the installation, counted and drawn: how many and in which
 * state, what they are used for, what of the platform they use, how they
 * are made, and which of them are busiest and largest. Each part leads to
 * the lambdas it counts.
 */
export function StatisticsSection({ access }: { access: Access }) {
  const { token, deny, dark, overview } = access;

  const [days, setDays] = useState(30);
  const [events, setEvents] = useState<EventHistory | null>(null);
  const [activity, setActivity] = useState<Activity | null>(null);
  const [storage, setStorage] = useState<AdminStorage | null>(null);

  useEffect(() => {
    let current = true;

    api.telemetry(1, token, days)
      .then((read) => current && setEvents(read.events))
      .catch((error) => refusedToken(error) && deny());

    return () => {
      current = false;
    };
  }, [token, deny, days]);

  useEffect(() => {
    const failed = (error: unknown) => refusedToken(error) && deny();

    api.activity(token).then(setActivity).catch(failed);
    api.admin.storage(token).then(setStorage).catch(failed);
  }, [token, deny]);

  if (overview === null) {
    return (
      <Section title="Statistics">
        <Reading what="the lambdas" />
      </Section>
    );
  }

  const { lambdas } = overview;
  const pick = (kind: string) => events?.series.find((s) => s.kind === kind)?.counts ?? [];
  const other = Object.entries(lambdas.saved).filter(([origin]) => !ORIGINS.some((o) => o.origin === origin)).reduce((sum, [, n]) => sum + n, 0);

  return (
    <Section
      title="Statistics"
      hint="The lambdas of this installation, counted as of now. The demos are counted apart and in nothing else: every installation has the same ones. The requests are counted in memory, since the server came up."
    >
      <Tiles columns={5}>
        <Tile label="Lambdas" value={count(lambdas.total)} to="/admin/lambdas" detail={`and ${plural(lambdas.demos, 'demo')}`} />
        <Tile label="Online" value={count(lambdas.online)} to={lambdasWhere({ state: 'online' })} detail={`${share(lambdas.online, lambdas.total)} of them`} />
        <Tile label="Created, 30 days" value={count(lambdas.createdMonth)} to={lambdasWhere({ created: '30' })} detail={`${count(lambdas.createdWeek)} in the last week`} />
        <Tile label="Active, 30 days" value={count(lambdas.activeMonth)} detail="changed or visited" title="Changed, or visited as the platform last wrote it down" />
        <Tile label="Versions kept" value={count(lambdas.versions)} detail={`${(lambdas.versions / Math.max(1, lambdas.total)).toFixed(1)} a lambda`} />
      </Tiles>

      <div className="mt-6">
        {events === null ? (
          <Card title="What people did">
            <Reading what="what people did" />
          </Card>
        ) : (
          <Chart
            title="What people did"
            hint="Per day, drawn as steps: each value counts a whole day. Deployed counts every time something went online, so a lambda redeployed five times in an afternoon appears five times."
            toolbar={<Pills label="Range" value={days} onChange={setDays} options={RANGES.map((r) => ({ value: r.days, label: r.label }))} />}
            labels={events.days.map((d) => d.slice(5))}
            dark={dark}
            format={(v) => count(v)}
            shape="step"
            series={[
              { label: 'Created', color: BLUE, values: pick('created') },
              { label: 'Saved', color: PURPLE, values: pick('saved') },
              { label: 'Deployed', color: GREEN, values: pick('deployed') },
              { label: 'Deleted', color: GREY, values: pick('deleted') },
            ]}
          />
        )}
      </div>

      {/* the whole life of the installation: the tables hold the present, and
          only the events remember last week */}
      {events !== null && (
        <div className="mt-6">
          <h2 className="mb-2 text-sm font-medium">Since the installation began</h2>
          <Tiles columns={5}>
            {EVENTS.map((event) => (
              <Tile key={event.kind} label={event.label} title={event.note} value={count(events.series.find((s) => s.kind === event.kind)?.total ?? 0)} />
            ))}
          </Tiles>
        </div>
      )}

      <div className="mt-6 grid gap-6 lg:grid-cols-2">
        <Card title="State" hint="Whether they are online. A sandbox lambda goes offline by itself once unused.">
          <Segments
            label="State"
            dark={dark}
            parts={[
              { label: 'Online', value: lambdas.online, color: BLUE, to: lambdasWhere({ state: 'online' }) },
              { label: 'Offline', value: lambdas.offline, color: GREY, to: lambdasWhere({ state: 'offline' }) },
              { label: 'Blocked', value: lambdas.blocked, color: RED, to: lambdasWhere({ state: 'blocked' }) },
            ]}
          />
        </Card>

        <Card title="Tiers" hint={`What they are for. The ${plural(lambdas.demos, 'demo')} are left out.`}>
          <Segments
            label="Tiers"
            dark={dark}
            parts={[
              { label: 'Sandbox', value: lambdas.tiers.Sandbox ?? 0, color: GREY, to: lambdasWhere({ tier: 'Sandbox' }), hint: 'trying something' },
              { label: 'Production', value: lambdas.tiers.Production ?? 0, color: BLUE, to: lambdasWhere({ tier: 'Production' }), hint: 'an app people use' },
              { label: 'System', value: lambdas.tiers.System ?? 0, color: PURPLE, to: lambdasWhere({ tier: 'System' }), hint: 'what the installation depends on' },
            ]}
          />
        </Card>

        <Card title="What they use" hint="Of the platform's features, by how many lambdas use each. The workspace comes switched on.">
          <Ranked
            limit={FEATURES.length}
            of={lambdas.total}
            empty="There are no lambdas yet."
            rows={FEATURES.map((f) => ({
              key: f.id,
              label: f.label,
              value: lambdas.uses[f.id],
              aside: share(lambdas.uses[f.id], lambdas.total),
              to: lambdasWhere({ uses: f.id }),
            })).sort((a, b) => b.value - a.value)}
          />
        </Card>

        <div className="space-y-6">
          <Card title="Who they are for" hint="The view their editor opens in: the simple one for an owner who does not write code - what the build agent makes - the full one for developers.">
            <Segments
              label="Views"
              dark={dark}
              parts={[
                { label: 'Simple view', value: lambdas.simple, color: ORANGE, to: lambdasWhere({ view: 'simple' }) },
                { label: 'Full view', value: lambdas.full, color: BLUE, to: lambdasWhere({ view: 'full' }) },
              ]}
            />
          </Card>

          <Card title="How their versions are made" hint="The versions saved in the last thirty days, by where they came from.">
            <Segments
              label="Origins"
              dark={dark}
              parts={[
                ...ORIGINS.map((o) => ({ label: o.label, value: lambdas.saved[o.origin] ?? 0, color: o.color })),
                { label: 'Other', value: other, color: GREY, hint: 'the operator, the platform' },
              ]}
            />
          </Card>
        </div>

        <Card title="Busiest since the server came up" hint="Requests answered, counted in memory." more={{ to: '/admin/server', label: 'Server' }}>
          {activity === null ? (
            <Reading what="the traffic" />
          ) : (
            <Ranked
              empty="Nothing was called yet."
              rows={activity.lambdas.filter((a) => a.requests > 0).map((a) => ({ key: a.publicKey, label: a.publicKey, value: a.requests, to: lambdaPage(a.publicKey), mono: true }))}
            />
          )}
        </Card>

        <Card title="Most server errors since the server came up" hint="Requests answered with a server error, counted in memory." more={{ to: '/admin/log?level=error', label: 'Log' }}>
          {activity === null ? (
            <Reading what="the traffic" />
          ) : (
            <Ranked
              empty="No lambda answered with a server error."
              rows={activity.lambdas
                .filter((a) => a.failed > 0)
                .sort((a, b) => b.failed - a.failed)
                .map((a) => ({ key: a.publicKey, label: a.publicKey, value: a.failed, aside: `of ${count(a.requests)}`, to: lambdaPage(a.publicKey), mono: true, bad: true }))}
            />
          )}
        </Card>

        <Card title="Largest" hint="What each takes on the volume: its data, versions, drafts and what is kept for it." more={{ to: '/admin/storage', label: 'Storage' }}>
          {storage === null ? (
            <Reading what="the room" />
          ) : (
            <Ranked
              empty="Nothing was measured yet."
              rows={storage.largest.map((l) => ({ key: l.publicKey, label: l.publicKey, value: l.totalBytes, said: bytes(l.totalBytes), to: lambdaPage(l.publicKey), mono: true }))}
            />
          )}
        </Card>

        <Card title="Expiring" hint="Lambdas of a tier that is not kept while unused - the sandbox - that go within seven days unless they are visited or changed.">
          {lambdas.goingOffline === 0 && lambdas.removed === 0 ? (
            <p className="py-3 text-[13px] text-slate-500">Nothing expires this week.</p>
          ) : (
            <Ranked
              empty=""
              of={Math.max(lambdas.goingOffline, lambdas.removed, 1)}
              rows={[
                { key: 'offline', label: 'Go offline', value: lambdas.goingOffline, to: lambdasWhere({ state: 'expiring' }) },
                { key: 'removed', label: 'Are removed', value: lambdas.removed, to: lambdasWhere({ state: 'expiring' }) },
              ]}
            />
          )}
        </Card>
      </div>
    </Section>
  );
}
