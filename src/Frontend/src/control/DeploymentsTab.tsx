import { useEffect, useState } from 'react';

import { ApiError, api, type Activation } from '../api';
import { IconPlay, IconSpinner, IconStop } from '../components/Icons';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { ending, origin, span, stamp } from './format';
import { AgentMark, Ago, Empty, Section } from './ui';

/**
 * When each version was online, and what took it down.
 */
export function DeploymentsTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.deployments;

  const { lambda, versions, busy } = control;

  const [history, setHistory] = useState<Activation[] | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  // read again whenever the lambda changes underneath - which is what a
  // deployment from the sidebar, or by an agent, looks like from here
  useEffect(() => {
    let alive = true;

    api
      .deployments(control.privateKey)
      .then((found) => alive && setHistory(found))
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.readFailed));

    return () => {
      alive = false;
    };
  }, [control.privateKey, lambda.activeVersion, lambda.deployedAt, said]);

  const live = lambda.activeVersion != null;
  const change = (version: number) => versions.find((v) => v.version === version)?.change;
  const kept = (version: number) => versions.some((v) => v.version === version);

  return (
    <Section
      title={t.frame.sections.deployments}
      hint={said.hint(lambda.deployedUntil ? stamp(lambda.deployedUntil) : null)}
      actions={
        live && (
          <button type="button" onClick={control.undeploy} disabled={busy !== null} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {busy === 'undeploy' ? <IconSpinner /> : <IconStop className="h-3.5 w-3.5" />}
            {said.takeOffline}
          </button>
        )
      }
    >
      {failure ? (
        <p className="text-sm text-red-500">{failure}</p>
      ) : history === null ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <IconSpinner /> {said.reading}
        </div>
      ) : history.length === 0 ? (
        <Empty>{said.none}</Empty>
      ) : (
        <>
          <Timeline history={history} />

          <ol className="mt-6 divide-y divide-slate-200 border-y border-slate-200 dark:divide-ink-800 dark:border-ink-800">
            {history.map((entry, index) => {
              const now = !entry.ended;

              return (
                <li key={`${entry.started}-${index}`} className="flex items-center gap-3 py-3 text-[13px]">
                  <span className="w-8 shrink-0 tabular-nums text-slate-500">{entry.version}</span>

                  <span className="min-w-0 flex-1">
                    <span className="block truncate text-[15px]" title={change(entry.version) ?? undefined}>
                      {change(entry.version) ?? <span className="text-slate-400">{said.noDescription}</span>}
                    </span>
                  </span>

                  <AgentMark origin={entry.origin} />

                  <span className="hidden w-24 shrink-0 text-right text-slate-500 sm:block" title={said.deployed(stamp(entry.started), origin(entry.origin, t.shared))}>
                    <Ago at={entry.started} />
                  </span>

                  <span className="w-16 shrink-0 text-right tabular-nums text-slate-500" title={said.duration}>
                    {span(entry.seconds, t.shared)}
                  </span>

                  <span className="w-28 shrink-0 text-right">
                    {now ? (
                      <span className="inline-flex items-center gap-1.5 text-emerald-600 dark:text-emerald-400">
                        <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
                        {said.online}
                      </span>
                    ) : (
                      <span
                        className={entry.endedBy === 'expired' || entry.endedBy === 'admin' ? 'text-amber-600 dark:text-amber-400' : 'text-slate-400'}
                        title={`${ending(entry.endedBy, t.shared)}${entry.ended ? `, ${stamp(entry.ended)}` : ''}`}
                      >
                        {(entry.endedBy && said.short[entry.endedBy]) || said.short.ended}
                      </span>
                    )}
                  </span>

                  <span className="w-7 shrink-0 text-right">
                    {!now && entry.version !== lambda.activeVersion && kept(entry.version) && (
                      <button
                        type="button"
                        onClick={() => control.deploy(entry.version)}
                        disabled={busy !== null}
                        className="rounded-full p-1 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500 disabled:opacity-40"
                        title={said.putBack(entry.version)}
                        aria-label={said.putBack(entry.version)}
                      >
                        <IconPlay className="h-3.5 w-3.5" />
                      </button>
                    )}
                  </span>
                </li>
              );
            })}
          </ol>
        </>
      )}
    </Section>
  );
}

/**
 * The last week as a strip: a block for every stretch a version was online,
 * a gap wherever nothing was. What a list of dates takes a minute to add up,
 * this shows at once.
 */
function Timeline({ history }: { history: Activation[] }) {
  const said = useEditorT().deployments;
  const now = Date.now();
  const from = now - 7 * 24 * 3600_000;

  const blocks = history
    .map((entry) => ({
      entry,
      start: Math.max(from, new Date(entry.started).getTime()),
      end: entry.ended ? new Date(entry.ended).getTime() : now,
    }))
    .filter((block) => block.end > from);

  const x = (at: number) => ((at - from) / (now - from)) * 100;

  // alternating shades, so two versions back to back read as two
  const order = [...new Set(blocks.map((b) => b.entry.version))].sort((a, b) => a - b);

  return (
    <div>
      <div className="relative h-5 w-full bg-slate-100 dark:bg-ink-850" role="img" aria-label={said.timeline}>
        {blocks.map((block, index) => (
          <div
            key={index}
            title={said.block(block.entry.version, stamp(block.entry.started), block.entry.ended ? stamp(block.entry.ended) : null)}
            className={`absolute top-0 h-full border-r border-white dark:border-ink-950 ${
              order.indexOf(block.entry.version) % 2 === 0 ? 'bg-emerald-500/70' : 'bg-emerald-600/45 dark:bg-emerald-400/45'
            }`}
            style={{ left: `${x(block.start)}%`, width: `${Math.max(0.4, x(block.end) - x(block.start))}%` }}
          />
        ))}
      </div>
      <div className="mt-1 flex justify-between text-xs text-slate-400">
        <span>{said.weekAgo}</span>
        <span>{said.now}</span>
      </div>
    </div>
  );
}
