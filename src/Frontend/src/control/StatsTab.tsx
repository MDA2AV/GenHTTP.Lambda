import { useEffect, useState } from 'react';

import { platformPath } from '../address';
import { ApiError, api, type LambdaTraffic, type TrafficPoint } from '../api';
import { Chart } from '../components/Chart';
import { IconSpinner } from '../components/Icons';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { ago, bytes, clock, count, millis, percent, stamp } from './format';
import { Empty, Figure, Pills, Section } from './ui';
import { useShared } from './words';

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
  const t = useEditorT();
  const said = t.stats;
  const title = t.frame.sections.stats;

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
          setFailure(error instanceof ApiError ? error.message : said.readFailed);
        }
      }
    }

    load();

    const timer = window.setInterval(() => document.visibilityState === 'visible' && load(), 15_000);

    return () => {
      alive = false;
      window.clearInterval(timer);
    };
  }, [control.privateKey, said]);

  const pills = (
    <Pills
      label={said.range}
      value={range}
      onChange={setRange}
      options={[
        { value: 'hour', label: said.lastHour },
        { value: 'day', label: said.lastDay },
      ]}
    />
  );

  const hint = traffic && said.hint(ago(traffic.since, t.shared));

  if (!traffic) {
    return (
      <Section title={title} pills={pills}>
        {failure ? (
          <p className="text-sm text-red-500">{failure}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500"><IconSpinner /> {said.reading}</div>
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
  const hourly = range === 'hour';

  return (
    <Section title={title} hint={hint} pills={pills}>
      <div className="surface grid grid-cols-2 gap-6 p-5 lg:grid-cols-4">
        <Figure value={count(total)} label={said.requests} title={upgrades > 0 ? said.websockets(upgrades) : undefined} />
        <Figure
          value={total > 0 ? percent(failed, total) : '-'}
          label={said.failed}
          tone={failed === 0 ? 'default' : 'bad'}
          title={said.serverErrors(failed)}
        />
        <Figure value={count(rejected)} label={said.rejected} tone={rejected > 0 ? 'warn' : 'default'} />
        <Figure value={total > 0 ? millis(average) : '-'} label={said.average} title={said.sent(bytes(sent))} />
      </div>

      {total === 0 ? (
        <Empty>{said.nobody(hourly)}</Empty>
      ) : (
        <div className="mt-6 space-y-6">
          <Chart
            title={said.requestsTitle}
            hint={said.per(hourly)}
            labels={labels}
            dark={dark}
            shape="stacked"
            format={(v) => count(Math.round(v))}
            series={[
              { label: said.answered, color: BLUE, values: points.map((p) => Math.max(0, p.requests - p.failed - p.rejected)) },
              { label: said.rejectedSeries, color: AMBER, values: points.map((p) => p.rejected) },
              { label: said.failedSeries, color: RED, values: points.map((p) => p.failed) },
            ]}
          />

          <Chart
            title={said.timeTitle}
            hint={said.averagePer(hourly)}
            labels={labels}
            dark={dark}
            shape="step"
            height={150}
            format={(v) => millis(v)}
            series={[{ label: said.averageSeries, color: PURPLE, values: points.map((p) => p.averageMillis) }]}
          />
        </div>
      )}

      <Entrances traffic={traffic} publicKey={control.lambda.publicKey} />

      {traffic.paths.length > 0 && (
        <section className="mt-8">
          <h2 className="text-sm font-medium">{said.mostAsked}</h2>
          <table className="mt-2 w-full text-left text-[13px]">
            <thead className="text-slate-500">
              <tr className="border-b border-slate-200 dark:border-ink-800">
                <th className="py-2 pr-3 font-normal">{said.path}</th>
                <th className="py-2 pr-3 text-right font-normal">{said.requestsColumn}</th>
                <th className="py-2 pr-3 text-right font-normal">{said.failedColumn}</th>
                <th className="py-2 text-right font-normal">{said.averageColumn}</th>
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
          <p className="mt-2 text-xs text-slate-400">{said.since}</p>
        </section>
      )}
    </Section>
  );
}

/**
 * Which way visitors came in - the lambda's own domain or its path here -
 * for a lambda that has been reached at a domain at all. For every other
 * lambda there is only the one way, and nothing to say about it.
 */
export function Entrances({ traffic, publicKey }: { traffic: LambdaTraffic; publicKey: string }) {
  const said = useShared().entrances;
  const entrances = traffic.entrances ?? [];

  if (!entrances.some((entrance) => entrance.domain)) {
    return null;
  }

  const all = entrances.reduce((total, entrance) => total + entrance.requests, 0);

  return (
    <section className="mt-8">
      <h2 className="text-sm font-medium">{said.title}</h2>
      <ul className="mt-2 divide-y divide-slate-100 text-[13px] dark:divide-ink-850">
        {entrances.map((entrance) => (
          <li key={entrance.domain ?? ''} className="flex items-center gap-3 py-2">
            <span className="min-w-0 flex-1 truncate font-mono">
              {entrance.domain ?? platformPath(publicKey)}
            </span>
            <span className="w-24 shrink-0">
              <span className="block h-1 bg-slate-200 dark:bg-ink-800">
                <span className="block h-full bg-accent-500 dark:bg-accent-400" style={{ width: `${(entrance.requests / Math.max(1, all)) * 100}%` }} />
              </span>
            </span>
            <span className="w-16 shrink-0 text-right tabular-nums">{count(entrance.requests)}</span>
          </li>
        ))}
      </ul>
      <p className="mt-2 text-xs text-slate-400">{said.note}</p>
    </section>
  );
}

const sum = (points: TrafficPoint[], pick: (p: TrafficPoint) => number) => points.reduce((total, p) => total + pick(p), 0);
