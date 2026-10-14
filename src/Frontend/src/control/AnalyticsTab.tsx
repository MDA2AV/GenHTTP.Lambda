import { useEffect, useMemo, useRef, useState, type ReactNode } from 'react';

import { absoluteAddress } from '../address';
import { ApiError, api, isDemo, type AnalyticsTotals, type AnalyticsValue, type LambdaAnalytics } from '../api';
import { CONTACT_MAIL } from '../contact';
import { Dialog } from '../components/Dialog';
import { IconArrowDown, IconArrowUp, IconCheck, IconGlobe, IconLock, IconMail, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT, useLanguage } from '../i18n';
import { tagOf } from '../i18n/languages';
import type { Control } from './context';
import { count, percent } from './format';
import { Section, Sparkline, Switch, pill } from './ui';

/** The time ranges offered; those longer than the tier shows are offered locked. */
const RANGES = [7, 30, 90, 365];

type Measure = 'visitors' | 'views' | 'hits';

/*
 * The accent of the editor for the one measure the chart shows, and for the
 * devices a fixed categorical order - validated as a set against both
 * surfaces (light needs its labels, which the legend gives). "Other" is the
 * neutral grey, since it is not an identity of its own.
 */
const ACCENT: [string, string] = ['#1a73e8', '#4285f4'];

const DEVICES: Record<string, [string, string]> = {
  desktop: ['#1a73e8', '#4285f4'],
  mobile: ['#eb6834', '#d95926'],
  tablet: ['#1baf7a', '#199e70'],
  bot: ['#eda100', '#c98500'],
  other: ['#9aa0a6', '#5f6368'],
};

const DEVICE_ORDER = ['desktop', 'mobile', 'tablet', 'bot', 'other'];

/** Whether the owner counts the pages bots opened as well, remembered in their browser. */
const BOTS = 'lambda-analytics-bots';

function rememberedBots(): boolean {
  try {
    return localStorage.getItem(BOTS) === 'yes';
  } catch {
    return false;
  }
}

/**
 * Who visits the lambda: how many people come each day, which pages they
 * open, which sites send them, from which countries and on what.
 *
 * Off until the owner switches it on - then counted by the platform, in
 * front of the lambda, by day, and written down every few minutes. The same
 * in both views and in the words of the simple one: an owner who never reads
 * the code wants to know who comes as much as one who does.
 *
 * The figures at the top pick what the chart under them shows, so the page
 * answers one question at a time - how many came - before the cards below
 * answer the others: to what, from where, on what.
 */
export function AnalyticsTab({ control }: { control: Control }) {
  const { privateKey, lambda } = control;

  const t = useEditorT();
  const said = t.analytics;
  const title = t.frame.sections.analytics;
  const tag = tagOf(useLanguage());
  const toast = useToast();

  // a month to begin with - a week where the tier shows no more, which the
  // server says - rather than all of a year, of which the last weeks are
  // what an owner opens the page for
  const [days, setDays] = useState<number | null>(30);
  const [state, setState] = useState<LambdaAnalytics | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [measure, setMeasure] = useState<Measure>('visitors');
  const [switching, setSwitching] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [bots, setBots] = useState(rememberedBots);

  const toggleBots = () => {
    setBots((were) => {
      try {
        localStorage.setItem(BOTS, were ? 'no' : 'yes');
      } catch {
        // a browser that keeps nothing starts with people only, which is fine
      }

      return !were;
    });
  };

  // the pages people opened, unless the owner asked for those of bots as
  // well: worked out here rather than asked for again, so the switch answers
  // at once
  const people = useMemo(() => state && withoutBots(state), [state]);
  const shown = bots ? state : people;

  /*
   * Read when opened and when the range changes, and again when the page is
   * looked at after a while away - not on a timer: the figures move every few
   * minutes, not every few seconds.
   */
  useEffect(() => {
    let alive = true;

    const load = () =>
      api
        .analytics(privateKey, days ?? undefined)
        .then((found) => {
          if (alive) {
            setState(found);
            setFailure(null);
          }
        })
        .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.loadFailed));

    load();

    const back = () => document.visibilityState === 'visible' && load();

    document.addEventListener('visibilitychange', back);

    return () => {
      alive = false;
      document.removeEventListener('visibilitychange', back);
    };
  }, [privateKey, days, said]);

  async function switchOn() {
    setSwitching(true);

    try {
      setState(await api.enableAnalytics(privateKey));
      toast(said.switchedOn, 'success');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.switchOnFailed, 'error');
    } finally {
      setSwitching(false);
    }
  }

  async function switchOff() {
    setSwitching(true);

    try {
      await api.disableAnalytics(privateKey);
      setConfirming(false);
      setState(await api.analytics(privateKey));
      toast(said.switchedOff, 'success');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.switchOffFailed, 'error');
    } finally {
      setSwitching(false);
    }
  }

  if (!state || !shown || !people) {
    return (
      <Section title={title}>
        {failure ? (
          <p className="text-sm text-red-500">{failure}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500"><IconSpinner /> {said.loading}</div>
        )}
      </Section>
    );
  }

  if (!state.enabled || !state.totals || !state.daily) {
    return (
      <Section title={title} hint={said.hint}>
        <Off window={state.window} demo={isDemo(lambda.tier)} busy={switching} onSwitch={switchOn} />
      </Section>
    );
  }

  const dark = control.theme === 'dark';
  const date = (day: string, options: Intl.DateTimeFormatOptions = { day: 'numeric', month: 'short' }) =>
    new Date(`${day}T00:00:00Z`).toLocaleDateString(tag, { ...options, timeZone: 'UTC' });

  // pages opened where there are any, by bots included, so the switch never
  // turns the page into another; an app that only answers calls is described
  // by what it was asked for
  const by: 'views' | 'hits' = state.totals.views > 0 ? 'views' : 'hits';
  const hidden = state.earliest != null && state.earliest < state.from;

  const ranges = (
    <div role="radiogroup" aria-label={said.range} className="flex flex-wrap gap-1.5">
      {RANGES.map((range) => {
        const locked = range > state.window;
        const active = range === state.days || (range === 365 && state.days >= 365);

        return (
          <button
            key={range}
            type="button"
            role="radio"
            aria-checked={active}
            aria-disabled={locked}
            title={locked ? said.premiumRange : undefined}
            onClick={() => !locked && setDays(range)}
            className={`${pill(active)} ${locked ? 'cursor-not-allowed opacity-60' : ''}`}
          >
            {locked && <IconLock className="h-3 w-3" />}
            {said.days(range)}
          </button>
        );
      })}
    </div>
  );

  /*
   * Beside the ranges rather than in the page views' own group, since it
   * changes the figure at the top and its chart as well. Only where pages
   * are opened: requests and visitors are what they are either way.
   */
  const toolbar = (
    <>
      {ranges}
      {by === 'views' && (
        <label title={said.botsHint} className="ml-auto inline-flex cursor-pointer items-center gap-2 text-[13px] text-slate-600 dark:text-slate-400">
          <span id="analytics-bots">{said.bots}</span>
          <Switch on={bots} onToggle={toggleBots} labelledBy="analytics-bots" />
        </label>
      )}
    </>
  );

  return (
    <Section title={title} hint={said.hint} pills={toolbar}>
      {hidden && lambda.tier !== 'Premium' && (
        <Nudge since={date(state.earliest!, { day: 'numeric', month: 'long', year: 'numeric' })} address={absoluteAddress(lambda.address)} />
      )}

      <Figures state={shown} measure={measure} onPick={setMeasure} dark={dark} bots={bots} />

      <Columns state={shown} measure={measure} dark={dark} date={date} />

      {/*
        * In two groups by what their numbers count, since the two can be far
        * apart: a person opens a page and it loads a dozen things, a crawler
        * asks for a hundred that are no page at all. An app that has no pages
        * people open is described by its requests alone.
        */}
      {by === 'views' && (
        <Group title={said.views} hint={said.viewsHint} badge={bots ? said.withBots : undefined}>
          <Card title={said.pages} hint={bots ? said.pagesHintBots : said.pagesHint}>
            <Ranked
              values={(shown.pages ?? []).filter((p) => p.views > 0)}
              by="views"
              mono
              name={(value) => (value === '(other)' ? said.other : value)}
            />
          </Card>

          {/* people's, whatever the switch says: a bot comes from nowhere, and would be counted as typed in */}
          <Card title={said.sources} hint={said.sourcesHint}>
            <Ranked
              values={(people.referrers ?? []).filter((r) => r.views > 0)}
              by="views"
              name={(value) => (value === '(other)' ? said.other : value)}
              icon={<IconGlobe className="h-3.5 w-3.5 shrink-0 text-slate-400" />}
              rest={{ label: said.direct, value: Math.max(0, people.totals!.views - sum(people.referrers, 'views')) }}
            />
          </Card>

          <Card title={said.countries} hint={said.countriesHint}>
            <Countries values={(shown.countries ?? []).filter((c) => c.views > 0)} by="views" tag={tag} />
          </Card>

          <Agents devices={shown.devices ?? []} browsers={shown.browsers ?? []} systems={shown.systems ?? []} by="views" dark={dark} />
        </Group>
      )}

      <Group title={said.hits} hint={said.hitsHint}>
        <Card title={said.requested} hint={by === 'hits' ? said.requestedHint : undefined}>
          <Ranked values={state.pages ?? []} by="hits" mono name={(value) => (value === '(other)' ? said.other : value)} />
        </Card>

        {by === 'hits' && (
          <>
            <Card title={said.sources} hint={said.sourcesHint}>
              <Ranked
                values={state.referrers ?? []}
                by="hits"
                name={(value) => (value === '(other)' ? said.other : value)}
                icon={<IconGlobe className="h-3.5 w-3.5 shrink-0 text-slate-400" />}
              />
            </Card>

            <Card title={said.countries} hint={said.countriesHint}>
              <Countries values={state.countries ?? []} by="hits" tag={tag} />
            </Card>

            <Agents devices={state.devices ?? []} browsers={state.browsers ?? []} systems={state.systems ?? []} by="hits" dark={dark} />
          </>
        )}

        <Card title={said.crawlers} hint={said.crawlersHint}>
          {(state.crawlers ?? []).length === 0 ? (
            <p className="py-6 text-center text-sm text-slate-500">{said.noCrawlers}</p>
          ) : (
            <Ranked values={state.crawlers ?? []} by="hits" name={(value) => <Crawler value={value} confirmed={state.confirmedCrawlers ?? []} />} />
          )}
        </Card>
      </Group>

      <footer className="mt-8 flex flex-wrap items-center gap-x-4 gap-y-2 border-t border-slate-200 pt-4 text-xs text-slate-500 dark:border-ink-800">
        <span>
          {said.footer(Math.max(1, Math.round(state.intervalSeconds / 60)))}{' '}
          {state.updated && said.updated(new Date(state.updated).toLocaleTimeString(tag, { hour: '2-digit', minute: '2-digit' }))}{' '}
          {state.since && said.since(new Date(state.since).toLocaleDateString(tag, { day: 'numeric', month: 'long', year: 'numeric' }))}
        </span>
        <button type="button" onClick={() => setConfirming(true)} className="ml-auto text-red-500 hover:underline dark:text-red-400">
          {said.switchOff}
        </button>
      </footer>

      <Dialog
        title={said.switchOffTitle}
        open={confirming}
        onClose={() => setConfirming(false)}
        footer={
          <>
            <button type="button" onClick={() => setConfirming(false)} className="btn-ghost">{said.keep}</button>
            <button type="button" onClick={switchOff} disabled={switching} className="btn-danger">
              {switching && <IconSpinner />}
              {said.switchOffConfirm}
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">{said.switchOffText}</p>
      </Dialog>
    </Section>
  );
}

/**
 * What analytics are, before they are on: what is counted, and what is not -
 * the second as important as the first, since it is about the owner's
 * visitors.
 */
function Off({ window, demo, busy, onSwitch }: { window: number; demo: boolean; busy: boolean; onSwitch: () => void }) {
  const said = useEditorT().analytics;

  return (
    <div className="surface relative overflow-hidden p-6 sm:p-8">
      <Preview />

      <div className="relative max-w-xl">
        <h2 className="text-xl font-semibold tracking-tight">{said.offTitle}</h2>
        <p className="mt-2 text-sm leading-relaxed text-slate-600 dark:text-slate-400">{said.offText}</p>

        <ul className="mt-5 space-y-2 text-sm">
          {[said.offPrivate, said.offAddresses, said.offKept(window)].map((line) => (
            <li key={line} className="flex items-start gap-2">
              <IconCheck className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
              <span>{line}</span>
            </li>
          ))}
        </ul>

        {demo ? (
          <p className="mt-6 text-sm text-slate-500">{said.demo}</p>
        ) : (
          <button type="button" onClick={onSwitch} disabled={busy} className="btn-primary mt-6">
            {busy && <IconSpinner />}
            {said.switchOn}
          </button>
        )}
      </div>
    </div>
  );
}

/** A quiet picture of what is to come, behind the words: a week of columns. */
function Preview() {
  const heights = [34, 52, 41, 68, 59, 83, 74];

  return (
    <svg viewBox="0 0 140 90" aria-hidden="true" className="pointer-events-none absolute -right-4 bottom-0 hidden h-40 w-64 opacity-60 sm:block">
      {heights.map((h, i) => (
        <rect key={i} x={6 + i * 19} y={88 - h} width={13} height={h} rx={2} className="fill-accent-500/15 dark:fill-accent-400/15" />
      ))}
    </svg>
  );
}

/**
 * For a free app that counted more than its tier shows: where the rest is.
 * The address of the app in the mail, never its editor link.
 */
function Nudge({ since, address }: { since: string; address: string }) {
  const said = useEditorT().analytics;

  const mail = `mailto:${CONTACT_MAIL}?subject=${encodeURIComponent(said.premiumSubject)}&body=${encodeURIComponent(said.premiumBody(address))}`;

  return (
    <div className="mb-5 flex flex-wrap items-center gap-x-4 gap-y-2 border border-logo-500/40 bg-logo-500/[0.03] px-4 py-3 text-sm dark:border-logo-400/30 dark:bg-logo-400/[0.04]">
      <span className="flex-1">{said.moreKept(since)}</span>
      <a href={mail} className="inline-flex items-center gap-1.5 text-[13px] text-logo-700 hover:underline dark:text-logo-400">
        <IconMail className="h-3.5 w-3.5" />
        {said.premiumAsk}
      </a>
    </div>
  );
}

/**
 * The three figures, each a tab for the chart below: its total over the
 * range, how it changed against the days before where those are shown, and
 * its days in small.
 */
function Figures({ state, measure, onPick, dark, bots }: {
  state: LambdaAnalytics;
  measure: Measure;
  onPick: (m: Measure) => void;
  dark: boolean;
  bots: boolean;
}) {
  const said = useEditorT().analytics;

  const figures: { id: Measure; label: string; hint: string }[] = [
    { id: 'visitors', label: said.visitors, hint: said.visitorsHint },
    { id: 'views', label: said.views, hint: said.viewsHint },
    { id: 'hits', label: said.hits, hint: said.hitsHint },
  ];

  return (
    <div role="tablist" className="surface grid grid-cols-3 divide-x divide-slate-200 dark:divide-ink-800">
      {figures.map((figure) => {
        const active = figure.id === measure;
        const value = state.totals![figure.id];
        const before = state.previous?.[figure.id];

        return (
          <button
            key={figure.id}
            type="button"
            role="tab"
            aria-selected={active}
            title={figure.hint}
            onClick={() => onPick(figure.id)}
            className={`relative min-w-0 p-3 text-left transition-colors sm:p-5 ${active ? '' : 'hover:bg-slate-50 dark:hover:bg-ink-850/60'}`}
          >
            {/* the one picked is marked by a rule as well as by colour */}
            <span
              aria-hidden="true"
              className="absolute inset-x-0 top-0 h-0.5"
              style={{ background: active ? ACCENT[dark ? 1 : 0] : 'transparent' }}
            />
            <div className={`flex flex-wrap items-center gap-x-1.5 text-[13px] ${active ? 'font-medium text-slate-900 dark:text-slate-100' : 'text-slate-500'}`}>
              {figure.label}
              {/* so a figure read on its own, or in a screenshot, says what it counts */}
              {figure.id === 'views' && bots && <Badge>{said.withBots}</Badge>}
            </div>
            <div className="mt-1 text-xl font-semibold tabular-nums tracking-tight sm:text-3xl">{count(value)}</div>
            <Change now={value} before={before} days={state.days} />
            <div className="mt-3 hidden sm:block">
              <Sparkline values={state.daily!.map((d) => d.totals[figure.id])} label={figure.label} />
            </div>
          </button>
        );
      })}
    </div>
  );
}

/**
 * How a figure changed against the same number of days before - with an
 * arrow and a word, never colour alone.
 */
function Change({ now, before, days }: { now: number; before: number | undefined; days: number }) {
  const said = useEditorT().analytics;

  if (before === undefined || before === 0) {
    return <div className="mt-1 hidden text-xs text-slate-500 sm:block">{said.inDays(days)}</div>;
  }

  const change = (now - before) / before;
  const shown = `${Math.abs(Math.round(change * 100))}%`;

  if (Math.round(change * 100) === 0) {
    return <div className="mt-1 text-xs text-slate-500">{said.noChange}<span className="hidden sm:inline"> · {said.versus(days)}</span></div>;
  }

  const up = change > 0;

  return (
    <div className="mt-1 flex items-center gap-1 text-xs text-slate-500">
      <span className={`inline-flex items-center gap-0.5 font-medium ${up ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-500 dark:text-red-400'}`}>
        {up ? <IconArrowUp className="h-3 w-3" /> : <IconArrowDown className="h-3 w-3" />}
        {shown}
      </span>
      <span className="hidden sm:inline">{said.versus(days)}</span>
    </div>
  );
}

/**
 * The measure picked, a column a day - or a week, over a year, where a
 * column a day would be thinner than the gap between two. Columns rather
 * than a line: each counts what happened in its day.
 */
function Columns({ state, measure, dark, date }: {
  state: LambdaAnalytics;
  measure: Measure;
  dark: boolean;
  date: (day: string, options?: Intl.DateTimeFormatOptions) => string;
}) {
  const said = useEditorT().analytics;
  const [hover, setHover] = useState<number | null>(null);
  const [table, setTable] = useState(false);
  const [frame, width] = useWidth(720);

  const weekly = state.days > 92;

  const points = useMemo(() => {
    const days = state.daily!;

    if (!weekly) {
      return days.map((d) => ({ start: d.date, value: d.totals[measure] }));
    }

    // whole weeks back from today, so the newest column is never a stub
    const weeks: { start: string; value: number }[] = [];

    for (let end = days.length; end > 0; end -= 7) {
      const slice = days.slice(Math.max(0, end - 7), end);
      weeks.unshift({ start: slice[0].date, value: slice.reduce((total, d) => total + d.totals[measure], 0) });
    }

    return weeks;
  }, [state.daily, measure, weekly]);

  const total = points.reduce((all, p) => all + p.value, 0);
  const max = Math.max(1, ...points.map((p) => p.value));
  const nice = niceCeiling(max);

  // drawn at the width it is given, so its labels stay the size of text on
  // a phone rather than shrinking with a picture made for a desk
  const height = width < 480 ? 180 : 260;
  const pad = { top: 12, right: 8, bottom: 26, left: 48 };
  const plotW = width - pad.left - pad.right;
  const plotH = height - pad.top - pad.bottom;
  const slot = plotW / points.length;
  const gap = Math.min(4, Math.max(1, slot * 0.25));
  const barW = Math.max(1, slot - gap);
  const y = (v: number) => plotH - (v / nice) * plotH;

  const ink = dark ? '#9aa0a6' : '#5f6368';
  const grid = dark ? '#3c4043' : '#e8eaed';
  const colour = ACCENT[dark ? 1 : 0];

  const heading = (weekly ? said.perWeek : said.perDay)[measure];
  const label = (start: string) => (weekly ? `${said.week} ${date(start)}` : date(start, { weekday: 'short', day: 'numeric', month: 'short' }));

  // the first, the last, and a few between - never one under every column
  const every = Math.max(1, Math.ceil(points.length / (width < 480 ? 3 : 6)));

  return (
    <figure className="surface mt-6 p-4">
      <div className="flex items-start justify-between gap-4">
        <figcaption className="text-sm font-medium">{heading}</figcaption>
        <button type="button" onClick={() => setTable((open) => !open)} className="shrink-0 text-xs text-accent-500 hover:underline dark:text-accent-400">
          {table ? said.showChart : said.showValues}
        </button>
      </div>

      {total === 0 ? (
        <p className="py-14 text-center text-sm text-slate-500">
          {state.earliest == null ? said.nobodyYet : said.nobody}
        </p>
      ) : table ? (
        <div className="mt-3 max-h-72 overflow-auto">
          <table className="w-full text-left text-xs">
            <thead className="sticky top-0 bg-white text-slate-500 dark:bg-ink-900">
              <tr>
                <th className="py-1 pr-3 font-medium">{weekly ? said.week : said.day}</th>
                <th className="py-1 text-right font-medium">{heading}</th>
              </tr>
            </thead>
            <tbody className="tabular-nums">
              {[...points].reverse().map((p) => (
                <tr key={p.start} className="border-t border-slate-200 dark:border-ink-800">
                  <td className="py-1 pr-3 text-slate-500">{label(p.start)}</td>
                  <td className="py-1 text-right">{count(p.value)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div ref={frame} className="relative mt-3">
          {hover !== null && (
            <div
              className="pointer-events-none absolute top-0 z-10 -translate-x-1/2 whitespace-nowrap border border-slate-200 bg-white px-2.5 py-1.5 text-xs shadow-md dark:border-ink-800 dark:bg-ink-900"
              style={{ left: `${((pad.left + slot * hover + slot / 2) / width) * 100}%` }}
            >
              <div className="text-slate-500">{label(points[hover].start)}</div>
              <div className="font-semibold tabular-nums">{count(points[hover].value)}</div>
            </div>
          )}

          <svg
            viewBox={`0 0 ${width} ${height}`}
            className="w-full"
            role="img"
            aria-label={`${heading}: ${count(total)}`}
            onMouseLeave={() => setHover(null)}
            onMouseMove={(event) => {
              const box = event.currentTarget.getBoundingClientRect();
              const at = ((event.clientX - box.left) / box.width) * width - pad.left;
              setHover(Math.min(points.length - 1, Math.max(0, Math.floor(at / slot))));
            }}
          >
            <g transform={`translate(${pad.left},${pad.top})`}>
              {[0, nice / 2, nice].map((tick) => (
                <g key={tick}>
                  <line x1={0} x2={plotW} y1={y(tick)} y2={y(tick)} stroke={grid} strokeWidth={1} />
                  <text x={-8} y={y(tick) + 4} textAnchor="end" fontSize={12} fill={ink}>{count(tick)}</text>
                </g>
              ))}

              {points.map((p, i) => {
                const h = p.value > 0 ? Math.max(2, plotH - y(p.value)) : 0;
                const x = i * slot + gap / 2;

                return (
                  <g key={p.start}>
                    {/* the whole slot answers the pointer, not only the column */}
                    <rect x={i * slot} y={0} width={slot} height={plotH} fill="transparent" />
                    {h > 0 && (
                      <path
                        d={column(x, plotH - h, barW, h, Math.min(4, barW / 2))}
                        fill={colour}
                        opacity={hover === null || hover === i ? 1 : 0.45}
                      />
                    )}
                  </g>
                );
              })}

              {points.map((p, i) =>
                i % every === 0 || i === points.length - 1 ? (
                  i !== points.length - 1 && points.length - 1 - i < every / 2 ? null : (
                    <text
                      key={p.start}
                      x={i * slot + slot / 2}
                      y={plotH + 18}
                      fontSize={12}
                      fill={ink}
                      textAnchor={i === 0 ? 'start' : i === points.length - 1 ? 'end' : 'middle'}
                    >
                      {date(p.start)}
                    </text>
                  )
                ) : null)}
            </g>
          </svg>
        </div>
      )}
    </figure>
  );
}

/** The width an element is laid out at, measured as it changes. */
function useWidth(initial: number) {
  const ref = useRef<HTMLDivElement>(null);
  const [width, setWidth] = useState(initial);

  useEffect(() => {
    const element = ref.current;

    if (!element || typeof ResizeObserver === 'undefined') {
      return;
    }

    const observer = new ResizeObserver(([entry]) => setWidth(Math.max(280, Math.round(entry.contentRect.width))));

    observer.observe(element);

    return () => observer.disconnect();
  }, []);

  return [ref, width] as const;
}

/** The cards whose numbers count the same thing, under the name of the figure they count. */
function Group({ title, hint, badge, children }: { title: string; hint: string; badge?: string; children: ReactNode }) {
  return (
    <section className="mt-8">
      <h2 className="flex flex-wrap items-center gap-x-2 text-base font-semibold tracking-tight">
        {title}
        {badge && <Badge>{badge}</Badge>}
      </h2>
      <p className="mt-0.5 text-xs text-slate-500">{hint}</p>
      <div className="mt-3 grid gap-6 lg:grid-cols-2">{children}</div>
    </section>
  );
}

/** A word beside a heading that says what its numbers count besides the usual. */
function Badge({ children }: { children: ReactNode }) {
  return <span className="rounded-sm bg-slate-100 px-1 text-[11px] font-normal tracking-normal text-slate-500 dark:bg-ink-800 dark:text-slate-400">{children}</span>;
}

/** A card of a group. */
function Card({ title, hint, children }: { title: string; hint?: string; children: ReactNode }) {
  return (
    <section className="surface flex min-w-0 flex-col p-4">
      <h3 className="text-sm font-medium">{title}</h3>
      {hint && <p className="mt-0.5 text-xs text-slate-500">{hint}</p>}
      <div className="mt-3 flex-1">{children}</div>
    </section>
  );
}

/**
 * A list of values, the largest first, each over a bar as long as its share
 * of the largest - so the list reads as a chart at a glance and as numbers
 * when read. Eight at first, the rest a click away.
 */
function Ranked({ values, by, name, mono = false, icon, rest }: {
  values: AnalyticsValue[];
  by: 'views' | 'hits';
  name: (value: string) => ReactNode;
  mono?: boolean;
  icon?: ReactNode;
  /** A row for what the values leave over - the visitors no site sent. */
  rest?: { label: string; value: number };
}) {
  const said = useEditorT().analytics;
  const [all, setAll] = useState(false);

  const rows = [
    ...values.map((v) => ({ key: v.value, label: name(v.value), value: v[by], muted: false, icon })),
    ...(rest && rest.value > 0 ? [{ key: '\u0000rest', label: rest.label, value: rest.value, muted: true, icon: undefined }] : []),
  ].sort((a, b) => b.value - a.value);

  if (rows.length === 0) {
    return <p className="py-6 text-center text-sm text-slate-500">{said.none}</p>;
  }

  const largest = Math.max(1, ...rows.map((r) => r.value));
  const whole = rows.reduce((total, r) => total + r.value, 0);
  const shown = all ? rows : rows.slice(0, 8);

  return (
    <>
      <ul className="space-y-1">
        {shown.map((row) => (
          <li key={row.key} className="relative flex items-center gap-3 text-[13px]" title={said.share(percent(row.value, whole), count(whole))}>
            <span
              aria-hidden="true"
              className="absolute inset-y-0 left-0 rounded-r-sm bg-accent-500/10 dark:bg-accent-400/15"
              style={{ width: `${(row.value / largest) * 100}%` }}
            />
            <span className={`relative flex min-w-0 flex-1 items-center gap-2 px-2 py-1.5 ${row.muted ? 'italic text-slate-500' : ''}`}>
              {row.icon}
              <span className={`truncate ${mono ? 'font-mono text-[12px]' : ''}`}>{row.label}</span>
            </span>
            <span className="relative w-14 shrink-0 pr-1 text-right tabular-nums">{count(row.value)}</span>
          </li>
        ))}
      </ul>

      {rows.length > 8 && (
        <button type="button" onClick={() => setAll((open) => !open)} className="mt-2 text-xs text-accent-500 hover:underline dark:text-accent-400">
          {all ? said.showFewer : said.showAll(rows.length)}
        </button>
      )}
    </>
  );
}

/** The countries by their names in the reader's language, their codes beside them. */
function Countries({ values, by, tag }: { values: AnalyticsValue[]; by: 'views' | 'hits'; tag: string }) {
  const said = useEditorT().analytics;

  const names = useMemo(() => {
    try {
      return new Intl.DisplayNames([tag], { type: 'region' });
    } catch {
      return null;
    }
  }, [tag]);

  const named = (code: string) => {
    if (code === '') {
      return said.unknownCountry;
    }

    try {
      return names?.of(code) ?? code;
    } catch {
      return code;
    }
  };

  return (
    <Ranked
      values={values}
      by={by}
      name={(code) => (
        <>
          <span className="mr-2 inline-block w-6 font-mono text-[11px] text-slate-400">{code || '··'}</span>
          {named(code)}
        </>
      )}
    />
  );
}

/**
 * The devices as one bar of their shares, part of a whole, with a legend
 * that names each and says its share - identity never rests on colour.
 */
function Devices({ values, by, dark }: { values: AnalyticsValue[]; by: 'views' | 'hits'; dark: boolean }) {
  const said = useEditorT().analytics;

  const parts = DEVICE_ORDER.map((id) => ({ id, value: values.find((v) => v.value === id)?.[by] ?? 0 })).filter((p) => p.value > 0);
  const whole = parts.reduce((total, p) => total + p.value, 0);

  if (whole === 0) {
    return <p className="py-6 text-center text-sm text-slate-500">{said.none}</p>;
  }

  const word = (id: string) => said.device[id as keyof typeof said.device] ?? id;

  return (
    <div>
      <div className="flex h-3 w-full gap-0.5 overflow-hidden rounded-sm" role="img" aria-label={parts.map((p) => `${word(p.id)} ${percent(p.value, whole)}`).join(', ')}>
        {parts.map((p) => (
          <span key={p.id} className="h-full first:rounded-l-sm last:rounded-r-sm" style={{ width: `${(p.value / whole) * 100}%`, background: DEVICES[p.id][dark ? 1 : 0] }} />
        ))}
      </div>

      <ul className="mt-4 space-y-2">
        {parts.map((p) => (
          <li key={p.id} className="flex items-center gap-2 text-[13px]">
            <span className="h-2.5 w-2.5 shrink-0 rounded-sm" style={{ background: DEVICES[p.id][dark ? 1 : 0] }} aria-hidden="true" />
            <span className="flex-1">{word(p.id)}</span>
            <span className="tabular-nums text-slate-500">{count(p.value)}</span>
            <span className="w-12 text-right font-medium tabular-nums">{percent(p.value, whole)}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}

/**
 * A crawler by its operator's name, marked as confirmed, where it came from
 * the addresses the operator publishes - or by the kind of program it is.
 */
function Crawler({ value, confirmed }: { value: string; confirmed: string[] }) {
  const said = useEditorT().analytics;
  const kinds: Record<string, string> = said.crawlerKinds;

  if (confirmed.includes(value)) {
    return (
      <span className="inline-flex items-center gap-1.5" title={said.confirmed(value)}>
        {value}
        <IconCheck className="h-3.5 w-3.5 text-emerald-600 dark:text-emerald-400" />
        <span className="sr-only">{said.confirmed(value)}</span>
      </span>
    );
  }

  return <>{kinds[value] ?? value}</>;
}

/**
 * What visitors came with - the devices, and the browsers and systems of
 * people - one card with a pill for each, so a group's cards pair up.
 */
function Agents({ devices, browsers, systems, by, dark }: {
  devices: AnalyticsValue[];
  browsers: AnalyticsValue[];
  systems: AnalyticsValue[];
  by: 'views' | 'hits';
  dark: boolean;
}) {
  const said = useEditorT().analytics;
  const [showing, setShowing] = useState<'devices' | 'browsers' | 'systems'>('devices');

  const values = (showing === 'browsers' ? browsers : systems).filter((v) => v[by] > 0);

  return (
    <section className="surface flex min-w-0 flex-col p-4">
      <div role="tablist" className="flex flex-wrap gap-1.5">
        {(['devices', 'browsers', 'systems'] as const).map((id) => (
          <button key={id} type="button" role="tab" aria-selected={showing === id} onClick={() => setShowing(id)} className={pill(showing === id)}>
            {said[id]}
          </button>
        ))}
      </div>
      <div className="mt-3 flex-1">
        {showing === 'devices' ? (
          <Devices values={devices} by={by} dark={dark} />
        ) : (
          <Ranked values={values} by={by} name={(value) => (value === 'other' ? said.device.other : value)} />
        )}
      </div>
    </section>
  );
}

/** A column with rounded ends at the top, square on the baseline. */
function column(x: number, y: number, w: number, h: number, r: number) {
  const radius = Math.min(r, h);

  return `M ${x},${y + h} L ${x},${y + radius} Q ${x},${y} ${x + radius},${y} L ${x + w - radius},${y} Q ${x + w},${y} ${x + w},${y + radius} L ${x + w},${y + h} Z`;
}

/** A round number at or above the largest value, so the axis reads 0, 50, 100 rather than 0, 43.5, 87. */
function niceCeiling(value: number) {
  // small counts keep whole numbers at the half way line
  if (value <= 10) {
    return Math.max(2, Math.ceil(value / 2) * 2);
  }

  const power = 10 ** Math.floor(Math.log10(value));

  for (const step of [1, 2, 2.5, 5, 10]) {
    if (step * power >= value) {
      return step * power;
    }
  }

  return 10 * power;
}

/**
 * The pages people opened: what the analytics are without the views of bots,
 * everywhere a view is counted - the totals, the days and every breakdown.
 */
function withoutBots(state: LambdaAnalytics): LambdaAnalytics {
  const people = <T extends { views: number; botViews: number }>(counted: T): T => ({
    ...counted,
    views: Math.max(0, counted.views - (counted.botViews ?? 0)),
    botViews: 0,
  });

  const list = (values: AnalyticsValue[] | null) => values?.map(people) ?? null;

  return {
    ...state,
    totals: state.totals && people(state.totals),
    previous: state.previous && people(state.previous),
    daily: state.daily?.map((day) => ({ ...day, totals: people(day.totals) })) ?? null,
    pages: list(state.pages),
    referrers: list(state.referrers),
    countries: list(state.countries),
    devices: list(state.devices),
    browsers: list(state.browsers),
    systems: list(state.systems),
    crawlers: list(state.crawlers),
  };
}

const sum = (values: AnalyticsValue[] | null, by: keyof Pick<AnalyticsTotals, 'views' | 'hits'>) =>
  (values ?? []).reduce((total, v) => total + v[by], 0);
