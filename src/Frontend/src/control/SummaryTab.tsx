import { Link } from 'react-router-dom';

import { IconAlert, IconGlobe, IconLock, IconSpinner } from '../components/Icons';
import type { Control } from './context';
import { ago, bytes, count, local, millis, percent, span } from './format';
import { AgentMark, Ago, Figure, Meter, Quote, Section, Sparkline } from './ui';

/**
 * Whether it is working, in the order somebody asks: is it up, is anybody
 * using it, is it failing, what changed last, and is there room left.
 */
export function SummaryTab({ control }: { control: Control }) {
  const { lambda, summary } = control;
  const base = `/editor/${control.privateKey}`;

  if (!summary) {
    return (
      <Section title="Overview">
        <div className="flex items-center gap-2 py-10 text-sm text-slate-500">
          <IconSpinner /> Reading how it is doing…
        </div>
      </Section>
    );
  }

  const { traffic, storage, limits, activation, latest } = summary;

  const live = lambda.activeVersion != null;
  const errorTone = traffic.dayFailed === 0 ? (traffic.dayRequests > 0 ? 'good' : 'default') : traffic.dayFailed / traffic.dayRequests > 0.05 ? 'bad' : 'warn';

  return (
    <Section
      title="Overview"
      hint={
        <>
          Traffic is counted since the server last started ({ago(traffic.since)}). A lambda stays online while
          people use it, and is removed after {limits.retentionDays} days with no visits and no changes.
        </>
      }
    >
      <p className="text-[15px]">
        {live ? (
          <>
            Online for <strong className="font-semibold">{activation ? span(activation.seconds) : 'a while'}</strong>,
            serving version {lambda.activeVersion}.
          </>
        ) : latest ? (
          'Offline. Nothing is being served until a version is deployed.'
        ) : (
          'Nothing has been written yet.'
        )}
      </p>

      <div className="surface mt-5 grid grid-cols-2 gap-6 p-5 lg:grid-cols-4">
        <div>
          <Figure value={count(traffic.dayRequests)} label="requests today" title={`${traffic.hourRequests} in the last hour`} />
          <div className="mt-2"><Sparkline values={traffic.hourly} label="Requests per hour over the last day" /></div>
        </div>
        <Figure
          value={traffic.dayRequests > 0 ? percent(traffic.dayFailed, traffic.dayRequests) : '-'}
          label="failed"
          tone={errorTone}
          title={`${traffic.dayFailed} server errors, ${traffic.dayRejected} not found or refused, over the last day`}
        />
        <Figure value={traffic.dayRequests > 0 ? millis(traffic.averageMillis) : '-'} label="to answer, on average" />
        <Figure value={traffic.lastSeen ? ago(traffic.lastSeen) : 'none yet'} label="last visit" />
      </div>

      {summary.recentProblems.length > 0 && (
        <div className="mt-6 border-l-2 border-red-500 pl-4">
          <div className="flex items-center justify-between gap-3">
            <h2 className="flex items-center gap-2 text-sm font-medium">
              <IconAlert className="h-4 w-4 text-red-500" />
              Something went wrong recently
            </h2>
            <Link to={`${base}/logs`} className="text-[13px] text-accent-500 hover:underline">Open the log</Link>
          </div>
          <ul className="mt-2 space-y-1.5">
            {distinct(summary.recentProblems).slice(0, 3).map((problem) => (
              <li key={problem.seq} className="flex gap-3 text-[13px]">
                <span className="min-w-0 flex-1 truncate" title={problem.text}>{local(problem.text, lambda.publicKey)}</span>
                <Ago at={problem.at} className="shrink-0 text-slate-500" />
              </li>
            ))}
          </ul>
        </div>
      )}

      <div className="mt-8 grid gap-8 lg:grid-cols-5">
        <section className="lg:col-span-3">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-medium">Latest change</h2>
            <Link to={`${base}/versions`} className="text-[13px] text-accent-500 hover:underline">All versions</Link>
          </div>

          {latest ? (
            <div className="mt-3 space-y-2">
              <p className="text-[15px]">
                {latest.change ?? <span className="text-slate-500">No description</span>}
              </p>
              <p className="flex items-center gap-2 text-[13px] text-slate-500">
                <span>Version {latest.version}</span>
                <AgentMark origin={latest.origin} />
                <Ago at={latest.created} />
                {latest.version !== lambda.activeVersion && <span className="text-amber-600 dark:text-amber-400">not online yet</span>}
              </p>
              {latest.specification && (
                <details className="text-[13px]">
                  <summary className="cursor-pointer select-none text-slate-500 hover:text-slate-700 dark:hover:text-slate-300">
                    What was wanted
                  </summary>
                  <div className="mt-2"><Quote>{latest.specification}</Quote></div>
                </details>
              )}
            </div>
          ) : (
            <p className="mt-3 text-sm text-slate-500">No versions yet.</p>
          )}
        </section>

        <section className="lg:col-span-2">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-medium">Storage</h2>
            <Link to={`${base}/files`} className="text-[13px] text-accent-500 hover:underline">Browse</Link>
          </div>

          <div className="mt-3 space-y-4">
            <Meter
              label="Code"
              extra={<Exposure open={false} why="C# is compiled, never served." />}
              used={storage.codeCharacters}
              of={limits.codeCharacters}
              format={count}
              unit="characters"
            />
            <Meter
              label="Assets"
              extra={<Exposure open={storage.servesAssets} why={storage.servesAssets ? 'Public: the code serves them.' : 'Not served by the code.'} />}
              used={storage.assetBytes}
              of={limits.assetBytes}
              format={bytes}
            />
            <Meter
              label="Data"
              extra={<Exposure open={storage.servesWorkspace} why={storage.servesWorkspace ? 'Public: the code serves the workspace.' : 'Private to the lambda.'} />}
              used={storage.workspaceBytes}
              of={limits.workspaceBytes}
              format={bytes}
            />
          </div>
        </section>
      </div>
    </Section>
  );
}

/**
 * One line per problem rather than per occurrence: the same request failing
 * three times is one thing that is wrong. The timing on a request line is a
 * measurement rather than part of what went wrong, so it is left out of the
 * comparison.
 */
function distinct<T extends { text: string }>(lines: T[]): T[] {
  const seen = new Set<string>();

  return lines.filter((line) => {
    const key = line.text.replace(/ — \d{3} ·.*$/, '');

    if (seen.has(key)) {
      return false;
    }

    seen.add(key);
    return true;
  });
}

/** Whether the public can reach it, as an icon, with the reason on hover. */
export function Exposure({ open, why }: { open: boolean; why: string }) {
  return open ? (
    <span title={why} className="inline-flex text-accent-500"><IconGlobe className="h-3.5 w-3.5" /><span className="sr-only">{why}</span></span>
  ) : (
    <span title={why} className="inline-flex text-slate-400"><IconLock className="h-3.5 w-3.5" /><span className="sr-only">{why}</span></span>
  );
}
