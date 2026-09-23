import { useEffect, useState } from 'react';

import { ApiError, api, type LambdaTraffic, type TrafficPoint } from '../api';
import { Chart } from '../components/Chart';
import { IconSpinner } from '../components/Icons';
import type { Control } from './context';
import { ago, bytes, clock, count, millis, percent, stamp } from './format';
import { Empty, Figure, Pills, Section } from './ui';

// the admin panel's palette, so a request is the same colour on both pages
const BLUE: [string, string] = ['#1a73e8', '#4285f4'];
const AMBER: [string, string] = ['#e8710a', '#d56e0c'];
const RED: [string, string] = ['#c5221f', '#ea4335'];
const PURPLE: [string, string] = ['#9334e6', '#a142f4'];

type Window = 'hour' | 'day';

/**
 * How much it is asked, how it answers, how fast, and what for.
 */
export function StatsTab({ control }: { control: Control }) {
  const [traffic, setTraffic] = useState<LambdaTraffic | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [range, setRange] = useState<Window>('hour');

  useEffect(() => {
    let alive = true;

    async function load() {
      try {
        const found = await api.traffic(control.privateKey);

        if (alive) {
          setTraffic(found);
          setFailure(null);
        }
      } catch (error) {
        if (alive) {
          setFailure(error instanceof ApiError ? error.message : 'The figures could not be read.');
        }
      }
    }

    load();

    const timer = window.setInterval(() => document.visibilityState === 'visible' && load(), 15_000);

    return () => {
      alive = false;
      window.clearInterval(timer);
    };
  }, [control.privateKey]);

  const pills = (
    <Pills
      label="Time range"
      value={range}
      onChange={setRange}
      options={[
        { value: 'hour', label: 'Last hour' },
        { value: 'day', label: 'Last day' },
      ]}
    />
  );

  const hint = traffic && (
    <>Counted in memory since the server last started, {ago(traffic.since)}. A restart begins these figures again.</>
  );

  if (!traffic) {
    return (
      <Section title="Stats" pills={pills}>
        {failure ? (
          <p className="text-sm text-red-500">{failure}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500"><IconSpinner /> Reading the figures…</div>
        )}
      </Section>
    );
  }

  const points = range === 'hour' ? traffic.minutes : traffic.quarters;
  const labels = points.map((p) => (range === 'hour' ? clock(p.at) : stamp(p.at)));

  const total = sum(points, (p) => p.requests);
  const failed = sum(points, (p) => p.failed);
  const rejected = sum(points, (p) => p.rejected);
  const upgrades = sum(points, (p) => p.upgrades);
  const sent = sum(points, (p) => p.bytes);
  const average = total > 0 ? sum(points, (p) => p.averageMillis * p.requests) / total : 0;

  const dark = control.theme === 'dark';
  const unit = range === 'hour' ? 'minute' : '15 minutes';

  return (
    <Section title="Stats" hint={hint} pills={pills}>
      <div className="surface grid grid-cols-2 gap-6 p-5 lg:grid-cols-4">
        <Figure value={count(total)} label="requests" title={upgrades > 0 ? `and ${upgrades} websocket connections` : undefined} />
        <Figure
          value={total > 0 ? percent(failed, total) : '-'}
          label="failed"
          tone={failed === 0 ? 'default' : 'bad'}
          title={`${failed} server errors`}
        />
        <Figure value={count(rejected)} label="not found or refused" tone={rejected > 0 ? 'warn' : 'default'} />
        <Figure value={total > 0 ? millis(average) : '-'} label="to answer, on average" title={`${bytes(sent)} sent`} />
      </div>

      {total === 0 ? (
        <Empty>Nobody has called it in the last {range}.</Empty>
      ) : (
        <div className="mt-6 space-y-6">
          <Chart
            title="Requests"
            hint={`Per ${unit}.`}
            labels={labels}
            dark={dark}
            shape="stacked"
            format={(v) => count(Math.round(v))}
            series={[
              { label: 'Answered', color: BLUE, values: points.map((p) => Math.max(0, p.requests - p.failed - p.rejected)) },
              { label: 'Not found or refused', color: AMBER, values: points.map((p) => p.rejected) },
              { label: 'Failed', color: RED, values: points.map((p) => p.failed) },
            ]}
          />

          <Chart
            title="Time to answer"
            hint={`The average per ${unit}.`}
            labels={labels}
            dark={dark}
            shape="step"
            height={150}
            format={(v) => millis(v)}
            series={[{ label: 'Average', color: PURPLE, values: points.map((p) => p.averageMillis) }]}
          />
        </div>
      )}

      {traffic.paths.length > 0 && (
        <section className="mt-8">
          <h2 className="text-sm font-medium">Most asked for</h2>
          <table className="mt-2 w-full text-left text-[13px]">
            <thead className="text-slate-500">
              <tr className="border-b border-slate-200 dark:border-ink-800">
                <th className="py-2 pr-3 font-normal">Path</th>
                <th className="py-2 pr-3 text-right font-normal">Requests</th>
                <th className="py-2 pr-3 text-right font-normal">Failed</th>
                <th className="py-2 text-right font-normal">Average</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-ink-850">
              {traffic.paths.slice(0, 10).map((path) => (
                <tr key={path.path}>
                  <td className="max-w-xs truncate py-2 pr-3 font-mono" title={path.path}>{path.path}</td>
                  <td className="py-2 pr-3 text-right tabular-nums">{count(path.requests)}</td>
                  <td className={`py-2 pr-3 text-right tabular-nums ${path.failed > 0 ? 'text-red-500' : 'text-slate-400'}`}>{count(path.failed)}</td>
                  <td className="py-2 text-right tabular-nums text-slate-500">{millis(path.averageMillis)}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <p className="mt-2 text-xs text-slate-400">Since the server started.</p>
        </section>
      )}
    </Section>
  );
}

const sum = (points: TrafficPoint[], pick: (p: TrafficPoint) => number) => points.reduce((total, p) => total + pick(p), 0);
