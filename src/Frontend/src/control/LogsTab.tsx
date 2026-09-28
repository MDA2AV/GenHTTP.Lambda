import { useEffect, useRef, useState } from 'react';

import { ApiError, api, type OwnerLogEntry } from '../api';
import { IconChevronDown, IconSpinner } from '../components/Icons';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { clock, local, preview } from './format';
import { Empty, LevelMark, Pills, Section } from './ui';

/** How many lines are held on the page before the oldest are let go. */
const HELD = 2000;

type View = 'all' | 'requests' | 'output' | 'problems';

/**
 * What this lambda has been saying, as it happens: the requests it answered,
 * what it printed, and what went wrong - a handler that threw comes with its
 * stack trace. Newest first, so what just happened is where the eye is.
 *
 * Opened on a feature, what its preview has been saying instead - kept apart
 * from the lambda's own log, so trying a feature leaves no trace among the
 * lambda's visitors, and the lambda's visitors none in the feature's.
 */
export function LogsTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.logs;
  const feature = control.feature?.info.key ?? null;

  const [lines, setLines] = useState<OwnerLogEntry[]>([]);
  const [loaded, setLoaded] = useState(false);
  const [failure, setFailure] = useState<string | null>(null);
  const [capturing, setCapturing] = useState(true);

  const [view, setView] = useState<View>('all');
  const [search, setSearch] = useState('');
  const [paused, setPaused] = useState(false);
  const [open, setOpen] = useState<number | null>(null);

  const cursor = useRef<number | undefined>(undefined);

  // problems are asked of the server, which has more of them than a page of
  // everything would; the other views are the same lines, filtered here
  const level = view === 'problems' ? 'warn' : 'info';

  useEffect(() => {
    cursor.current = undefined;
    setLines([]);
    setLoaded(false);
  }, [level, control.privateKey, feature]);

  useEffect(() => {
    if (paused) {
      return;
    }

    let alive = true;

    async function poll() {
      try {
        const page = feature
          ? await api.feature.logs(control.privateKey, feature, { since: cursor.current, level })
          : await api.lambdaLogs(control.privateKey, { since: cursor.current, level });

        if (!alive) {
          return;
        }

        cursor.current = page.cursor;

        setCapturing(page.capturing);
        setFailure(null);
        setLoaded(true);

        if (page.lines.length > 0) {
          setLines((was) => [...page.lines.slice().reverse(), ...was].slice(0, HELD));
        }
      } catch (error) {
        if (alive) {
          setFailure(error instanceof ApiError ? error.message : said.readFailed);
          setLoaded(true);
        }
      }
    }

    poll();

    const timer = window.setInterval(() => document.visibilityState === 'visible' && poll(), 3000);

    return () => {
      alive = false;
      window.clearInterval(timer);
    };
  }, [control.privateKey, feature, level, paused, said]);

  const needle = search.trim().toLowerCase();

  const shown = lines.filter((line) => {
    if (view === 'requests' && line.source !== 'Requests') return false;
    if (view === 'output' && line.source !== 'stdout' && line.source !== 'stderr') return false;
    if (view === 'problems' && line.source === 'Requests' && line.level === 'warn') return false;

    return needle === '' || line.text.toLowerCase().includes(needle) || (line.detail?.toLowerCase().includes(needle) ?? false);
  });

  return (
    <Section
      title={t.frame.sections.logs}
      hint={feature ? said.featureHint(capturing) : said.hint(capturing)}
      actions={
        <>
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder={said.search}
            className="field !w-40 !py-1 text-[13px] sm:!w-52"
            aria-label={said.searchLabel}
          />
          <button
            type="button"
            onClick={() => setPaused((was) => !was)}
            className="btn-ghost !px-3 !py-1 text-[13px]"
            title={paused ? said.resume : said.pause}
          >
            <span className={`h-1.5 w-1.5 rounded-full ${paused ? 'bg-slate-400' : 'animate-pulse bg-emerald-500'}`} />
            {paused ? said.paused : said.live}
          </button>
        </>
      }
      pills={
        <Pills
          label={said.show}
          value={view}
          onChange={setView}
          options={[
            { value: 'all', label: said.all },
            { value: 'requests', label: said.requests },
            { value: 'output', label: said.output },
            { value: 'problems', label: said.problems },
          ]}
        />
      }
    >
      {failure ? (
        <p className="text-sm text-red-500">{failure}</p>
      ) : !loaded ? (
        <div className="flex items-center gap-2 text-sm text-slate-500"><IconSpinner /> {said.reading}</div>
      ) : shown.length === 0 ? (
        <Empty>{lines.length === 0 ? (view === 'problems' ? said.noProblems : feature ? said.nothingPreview : said.nothing) : said.noMatch}</Empty>
      ) : (
        <ul className="divide-y divide-slate-100 border-y border-slate-200 font-mono text-[12.5px] dark:divide-ink-850 dark:border-ink-800">
          {shown.map((line) => {
            const expandable = !!line.detail || !!line.agent || !!line.country || !!line.domain;
            const expanded = open === line.seq;

            return (
              <li key={line.seq}>
                <button
                  type="button"
                  onClick={() => expandable && setOpen(expanded ? null : line.seq)}
                  className={`flex w-full items-start gap-3 px-1 py-1.5 text-left ${expandable ? 'hover:bg-slate-50 dark:hover:bg-ink-850' : 'cursor-default'}`}
                  aria-expanded={expandable ? expanded : undefined}
                  title={line.source}
                >
                  <span className="w-12 shrink-0 text-slate-400" title={new Date(line.at).toLocaleString()}>
                    {clock(line.at)}
                  </span>
                  <LevelMark level={line.level} />
                  <span className="min-w-0 flex-1 whitespace-pre-wrap break-words">
                    {feature ? preview(line.text, feature) : local(line.text, control.lambda.publicKey)}
                    {line.repeats > 1 && <span className="ml-2 text-slate-400" title={said.identical(line.repeats)}>×{line.repeats}</span>}
                  </span>
                  {expandable && (
                    <IconChevronDown className={`mt-0.5 h-3.5 w-3.5 shrink-0 text-slate-300 transition-transform dark:text-ink-700 ${expanded ? '' : '-rotate-90'}`} />
                  )}
                </button>

                {expanded && (
                  <div className="space-y-2 bg-slate-50 px-3 py-2 pl-[4.75rem] dark:bg-ink-950/50">
                    <p className="font-sans text-xs text-slate-500">
                      {line.source}
                      {line.domain && said.at(line.domain)}
                      {line.country && said.from(line.country)}
                      {line.agent && `, ${line.agent}`}
                    </p>
                    {line.detail && <pre className="max-h-80 overflow-auto whitespace-pre text-[12px] text-slate-700 dark:text-slate-300">{line.detail}</pre>}
                  </div>
                )}
              </li>
            );
          })}
        </ul>
      )}
    </Section>
  );
}
