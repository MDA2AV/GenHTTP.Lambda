import { useCallback, useEffect, useState } from 'react';

import { useAdminToken } from '../admin';
import { ApiError, api, type Activity, type Telemetry, type TelemetrySample } from '../api';
import { Chart, type Series } from '../components/Chart';
import { IconSpinner } from '../components/Icons';
import { Locked } from '../components/Locked';

const WINDOWS = [
  { minutes: 15, label: '15m' },
  { minutes: 60, label: '1h' },
  { minutes: 360, label: '6h' },
  { minutes: 1440, label: '24h' },
];

// validated against each surface with the palette checker, not picked by eye
const BLUE: [string, string] = ['#1a73e8', '#4285f4'];
const ORANGE: [string, string] = ['#e8710a', '#d56e0c'];
const PURPLE: [string, string] = ['#9334e6', '#a142f4'];
const RED: [string, string] = ['#c5221f', '#ea4335'];
// the four things a resident set is made of; checked as a set against both
// surfaces, including for colour blindness, rather than picked to look nice
const ANON: [string, string] = ['#1a73e8', '#4285f4'];
const CODE: [string, string] = ['#e8710a', '#d56e0c'];
const META: [string, string] = ['#9334e6', '#a142f4'];
const FILES: [string, string] = ['#188038', '#34a853'];

// gen 0 to gen 2 is an order, so it gets one hue in three steps
const GEN0: [string, string] = ['#8ab4f8', '#aecbfa'];
const GEN1: [string, string] = ['#4285f4', '#669df6'];
const GEN2: [string, string] = ['#174ea6', '#1a73e8'];

export function Stats({ dark }: { dark: boolean }) {
  const [token] = useAdminToken();
  const [denied, setDenied] = useState(false);
  const [data, setData] = useState<Telemetry | null>(null);
  const [activity, setActivity] = useState<Activity | null>(null);
  const [minutes, setMinutes] = useState(60);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (token === '') {
      return;
    }

    try {
      setData(await api.telemetry(minutes, token));
      // absent when the installation keeps the per lambda figures to itself
      setActivity(await api.activity(token).catch(() => null));
      setDenied(false);
      setError(null);
    } catch (problem) {
      // a wrong token and an installation with no administration answer the
      // same way, so this page cannot tell them apart either
      if (problem instanceof ApiError && problem.status === 404) {
        setDenied(true);
        setData(null);
      } else {
        setError('The telemetry could not be read.');
      }
    }
  }, [minutes, token]);

  useEffect(() => {
    load();

    const timer = window.setInterval(load, 15000);

    return () => window.clearInterval(timer);
  }, [load]);

  if (token === '' || denied) {
    return (
      <Locked
        title="Server"
        denied={denied}
        what="What the process is holding and what the engine is carrying, which is nobody's business but the operator's."
      />
    );
  }

  if (error !== null) {
    return <div className="mx-auto max-w-5xl px-5 py-14 text-sm text-red-500">{error}</div>;
  }

  if (data === null) {
    return (
      <div className="mx-auto flex max-w-5xl items-center gap-2 px-5 py-14 text-sm text-slate-500">
        <IconSpinner /> Reading the server…
      </div>
    );
  }

  const { server, traffic, platform, latest, samples } = data;
  const labels = samples.map((s) => time(s.taken));

  const line = (label: string, color: [string, string], pick: (s: TelemetrySample) => number): Series => ({
    label,
    color,
    values: samples.map(pick),
  });

  // the collector reports totals since start; the interesting number is how
  // many ran in each interval, so the series is differenced
  const deltas = (pick: (s: TelemetrySample) => number) =>
    samples.map((s, i) => (i === 0 ? 0 : Math.max(0, pick(s) - pick(samples[i - 1]))));

  return (
    <div className="mx-auto w-full max-w-5xl px-5 py-10">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Server</h1>
          <p className="mt-1.5 text-sm text-slate-600 dark:text-slate-400">
            Running on <strong className="font-medium">{server.engine}</strong>, GenHTTP {server.version},{' '}
            {server.runtime}. Sampled every {data.intervalSeconds}s
            {latest?.taken ? <> · figures below as of {time(latest.taken)}</> : null}.
          </p>
        </div>

        <div className="flex border border-slate-200 dark:border-ink-800" role="group" aria-label="Time range">
          {WINDOWS.map((window) => (
            <button
              key={window.minutes}
              type="button"
              onClick={() => setMinutes(window.minutes)}
              className={`px-3 py-1.5 text-sm ${
                window.minutes === minutes
                  ? 'bg-accent-500 text-white dark:bg-accent-400 dark:text-ink-950'
                  : 'text-slate-600 hover:bg-slate-50 dark:text-slate-400 dark:hover:bg-ink-850'
              }`}
            >
              {window.label}
            </button>
          ))}
        </div>
      </div>

      <div className="mt-7 grid grid-cols-2 gap-px border border-slate-200 bg-slate-200 sm:grid-cols-4 dark:border-ink-800 dark:bg-ink-800">
        <Tile label="Uptime" value={duration(server.uptimeSeconds)} />
        <Tile label="Managed heap" value={bytes(latest.managedBytes)} />
        <Tile label="Working set" value={bytes(latest.workingSetBytes)} />
        <Tile label="CPU" value={`${latest.cpuPercentage.toFixed(1)}%`} />
        <Tile label="Requests" value={latest ? count(traffic.requests) : '—'} />
        <Tile label="Server errors" value={count(traffic.failed)} tone={traffic.failed > 0 ? 'bad' : undefined} />
        <Tile label="Open sockets" value={count(traffic.openSockets)} />
        <Tile label="Lambdas" value={`${platform.deployed} / ${platform.lambdas}`} />
      </div>

      {/* absent rather than zeroed where the kernel does not offer them */}
      {latest.residentBytes > 0 && (
        <div className="mt-px grid grid-cols-2 gap-px border border-slate-200 bg-slate-200 sm:grid-cols-4 dark:border-ink-800 dark:bg-ink-800">
          <Tile label="Connections open" value={count(latest.openConnections)} />
          <Tile
            label="Connections accepted"
            value={count(latest.acceptedConnections)}
            note="Every inbound connection since the container started, counted by the kernel. Unlike the graph below, this one includes the health check's own connections over loopback."
          />
          <Tile label="File descriptors" value={count(latest.fileDescriptors)} />
          <Tile
            label="io_uring rings"
            value={count(latest.ringDescriptors)}
            note="A fixed handful for the lifetime of the process. If this climbs with traffic, the engine is opening a ring per connection and not closing it."
          />
        </div>
      )}

      <div className="mt-6 space-y-5">
        <Chart
          title="Managed heap"
          hint="The leak chart. A heap that keeps climbing across gen 2 collections is holding references it should have dropped. Committed rising while the live heap stays flat is the collector keeping pages it could hand back, which is not the same thing."
          labels={labels}
          dark={dark}
          format={bytes}
          series={[
            line('Live', BLUE, (s) => s.managedBytes),
            line('Committed', ORANGE, (s) => s.heapCommittedBytes),
            line('Fragmented', PURPLE, (s) => s.heapFragmentedBytes),
          ]}
        />

        <Chart
          title="Process memory"
          hint="What the operating system thinks the process is using. Kept apart from the heap above because the two live an order of magnitude apart, and one axis holding both would flatten whichever is smaller."
          labels={labels}
          dark={dark}
          format={bytes}
          series={[
            line('Working set', BLUE, (s) => s.workingSetBytes),
            line('Private', ORANGE, (s) => s.privateBytes),
          ]}
        />

        {latest.residentBytes > 0 && (
          <Chart
            title="What the memory is"
            hint="The resident set, split by what is actually holding it. The managed heap sits inside the blue band and is usually a small share of it - the rest of that band is the engine's buffers and the runtime's own allocations. The other three are not objects at all, so no collection will hand them back."
            labels={labels}
            dark={dark}
            shape="stacked"
            format={bytes}
            series={[
              line('Anonymous (heap, stacks, buffers)', ANON, (s) => s.anonymousBytes),
              line('Compiled code', CODE, (s) => s.jitBytes),
              line('Assemblies', META, (s) => s.assemblyBytes),
              line('Other mapped files', FILES, (s) => s.otherFileBytes),
            ]}
          />
        )}

        {latest.residentBytes > 0 && (
          <Chart
            title="Connections open"
            hint="Connections, not requests. A browser holding a page open keeps one and sends many requests down it, while a visitor who reloads opens a new one each time - which is the difference that decides how much the server holds. Loopback is left out, so the health check does not put a floor under this."
            labels={labels}
            dark={dark}
            format={(v) => v.toFixed(0)}
            series={[
              line('Open', BLUE, (s) => s.openConnections),
              line('Upgraded', PURPLE, (s) => s.openSockets),
            ]}
          />
        )}

        {latest.residentBytes > 0 && (
          <Chart
            title="Connections and requests per interval"
            hint="Both counted the same way over the same interval, so the gap between them is how much reuse the clients are getting. Requests far above connections means keep alive is working; the two lines together means every request is paying for a new connection."
            labels={labels}
            dark={dark}
            shape="step"
            format={(v) => v.toFixed(0)}
            series={[
              line('Connections accepted', ORANGE, (s) => s.connections),
              line('Requests answered', BLUE, (s) => s.requests),
            ]}
          />
        )}

        <Chart
          title="Collections per interval"
          hint="Gen 2 runs should be rare and should bring the heap back down. If they run often and the heap does not fall, something is holding references."
          labels={labels}
          dark={dark}
          shape="step"
          format={(v) => v.toFixed(0)}
          series={[
            { label: 'Gen 0', color: GEN0, values: deltas((s) => s.gen0Collections) },
            { label: 'Gen 1', color: GEN1, values: deltas((s) => s.gen1Collections) },
            { label: 'Gen 2', color: GEN2, values: deltas((s) => s.gen2Collections) },
          ]}
        />

        <Chart
          title="Requests per interval"
          hint="What the engine actually carried while the memory above was measured."
          labels={labels}
          dark={dark}
          shape="step"
          format={(v) => v.toFixed(0)}
          series={[line('Answered', BLUE, (s) => s.requests), line('Server errors', RED, (s) => s.failed)]}
        />

        <Chart
          title="Response time"
          hint="The mean over each interval. Upgraded connections are left out - they leave the handler at once and then live for as long as the socket does."
          labels={labels}
          dark={dark}
          format={(v) => `${v.toFixed(0)} ms`}
          series={[line('Average', BLUE, (s) => s.averageMillis)]}
        />
      </div>

      {activity !== null && activity.lambdas.length > 0 && (
        <figure className="surface mt-6">
          <figcaption className="border-b border-slate-200 px-4 py-3 dark:border-ink-800">
            <h2 className="text-sm font-medium">Lambdas</h2>
            <p className="mt-0.5 text-xs text-slate-500">
              What each one has served since the server came up, busiest first. Counters, not a history - they
              start over with the process.
            </p>
          </figcaption>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="text-xs text-slate-500">
                <tr className="border-b border-slate-200 dark:border-ink-800">
                  <th className="px-4 py-2 font-medium">Key</th>
                  <th className="px-4 py-2 text-right font-medium">Requests</th>
                  <th className="px-4 py-2 text-right font-medium">Errors</th>
                  <th className="px-4 py-2 text-right font-medium">Sockets</th>
                  <th className="px-4 py-2 text-right font-medium">Avg</th>
                  <th className="px-4 py-2 text-right font-medium">Slowest</th>
                  <th className="px-4 py-2 text-right font-medium" title="Counting only responses that declared a length">
                    Sent
                  </th>
                  <th className="px-4 py-2 text-right font-medium">Last seen</th>
                </tr>
              </thead>
              <tbody>
                {activity.lambdas.map((entry) => (
                  <tr key={entry.publicKey} className="border-b border-slate-200 last:border-0 dark:border-ink-800">
                    <td className="px-4 py-2">
                      <a href={`/lambda/${entry.publicKey}/`} className="font-mono text-accent-500 hover:underline dark:text-accent-400">
                        {entry.publicKey}
                      </a>
                    </td>
                    <td className="px-4 py-2 text-right tabular-nums">{count(entry.requests)}</td>
                    <td className={`px-4 py-2 text-right tabular-nums ${entry.failed > 0 ? 'text-red-500' : 'text-slate-500'}`}>
                      {count(entry.failed)}
                    </td>
                    <td className="px-4 py-2 text-right tabular-nums text-slate-500">
                      {entry.openSockets > 0 ? `${entry.openSockets} open` : count(entry.upgrades)}
                    </td>
                    <td className="px-4 py-2 text-right tabular-nums">{entry.averageMillis.toFixed(1)} ms</td>
                    <td className="px-4 py-2 text-right tabular-nums text-slate-500">{entry.slowestMillis.toFixed(0)} ms</td>
                    {/* zero means the responses never declared a length, which
                        is not the same as having sent nothing */}
                    <td className="px-4 py-2 text-right tabular-nums text-slate-500">
                      {entry.bytesOut > 0 ? bytes(entry.bytesOut) : '—'}
                    </td>
                    <td className="px-4 py-2 text-right text-slate-500">{entry.lastSeen ? ago(entry.lastSeen) : '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </figure>
      )}

      <dl className="mt-6 grid grid-cols-2 gap-x-6 gap-y-2 text-sm sm:grid-cols-3">
        <Fact label="Engine" value={server.engine} />
        <Fact label="GenHTTP" value={server.version} />
        <Fact label="Runtime" value={server.runtime} />
        <Fact label="Platform" value={server.platform} />
        <Fact label="Server GC" value={server.serverGarbageCollection ? 'on' : 'off'} />
        <Fact label="Processors" value={String(server.processors)} />
        <Fact label="Threads" value={String(latest.threads)} />
        <Fact label="GC pause" value={`${latest.pausePercentage.toFixed(2)}%`} />
        <Fact label="Heap fragmented" value={bytes(latest.heapFragmentedBytes)} />
        <Fact label="Allocated total" value={bytes(latest.allocatedBytes)} />
        <Fact label="Upgrades" value={count(traffic.upgrades)} />
        <Fact label="Versions stored" value={count(platform.versions)} />
      </dl>
    </div>
  );
}

function Tile({ label, value, tone, note }: { label: string; value: string; tone?: 'bad'; note?: string }) {
  return (
    <div className="bg-white px-4 py-3.5 dark:bg-ink-900" title={note}>
      <div className="text-xs text-slate-500">{label}</div>
      <div className={`mt-1 text-xl font-semibold tabular-nums ${tone === 'bad' ? 'text-red-500' : ''}`}>{value}</div>
    </div>
  );
}

function Fact({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between gap-3 border-b border-slate-200 py-1.5 dark:border-ink-800">
      <dt className="text-slate-500">{label}</dt>
      <dd className="truncate font-mono text-xs">{value}</dd>
    </div>
  );
}

const units = ['B', 'kB', 'MB', 'GB', 'TB'];

function bytes(value: number): string {
  let size = value;
  let unit = 0;

  while (size >= 1024 && unit < units.length - 1) {
    size /= 1024;
    unit++;
  }

  return `${size.toFixed(size >= 100 || unit === 0 ? 0 : 1)} ${units[unit]}`;
}

const count = (value: number) => value.toLocaleString('en-US');

function duration(seconds: number): string {
  const days = Math.floor(seconds / 86400);
  const hours = Math.floor((seconds % 86400) / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);

  if (days > 0) return `${days}d ${hours}h`;
  if (hours > 0) return `${hours}h ${minutes}m`;

  return `${minutes}m`;
}

/** How long ago, in the coarsest unit that still says something. */
function ago(iso: string): string {
  const seconds = Math.max(0, (Date.now() - new Date(iso.endsWith('Z') ? iso : `${iso}Z`).getTime()) / 1000);

  if (seconds < 60) return 'just now';
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
  if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;

  return `${Math.floor(seconds / 86400)}d ago`;
}

const time = (iso: string) =>
  new Date(iso.endsWith('Z') ? iso : `${iso}Z`).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });
