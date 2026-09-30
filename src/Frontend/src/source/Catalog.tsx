import { useCallback, useEffect, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { shownAddress } from '../address';
import { ApiError, api, type SourceEntry, type SourceOrder } from '../api';
import { IconSearch, IconSpinner, IconStar } from '../components/Icons';
import { useSourceT, useT } from '../i18n';
import { Link } from '../i18n/links';
import { usePublicPage } from '../meta';
import { useFormat } from './format';
import { LambdaNotice, LicenseChip } from './parts';

const PAGE = 24;

const ORDERS: SourceOrder[] = ['stars', 'updated', 'published'];

/**
 * Every published source, to be browsed and searched.
 *
 * A list rather than a wall of pictures, since what is being chosen here is
 * code to read: what each is, in the words its documentation opens with, and
 * the few facts that tell one project from another - its license, how many
 * starred it, how recently it changed, whether it runs. The search and the
 * order are in the address, so a search can be shared and the way back
 * returns to it.
 */
export function Catalog() {
  usePublicPage('/source');

  const t = useT();
  const said = useSourceT().catalog;
  const [params, setParams] = useSearchParams();

  const search = params.get('q') ?? '';
  const order = (ORDERS.find((o) => o === params.get('order')) ?? 'stars') as SourceOrder;

  const [typed, setTyped] = useState(search);
  const [entries, setEntries] = useState<SourceEntry[]>([]);
  const [total, setTotal] = useState<number | null>(null);
  const [next, setNext] = useState<number | null>(0);
  const [loading, setLoading] = useState(false);
  const [failure, setFailure] = useState<string | null>(null);

  const sentinel = useRef<HTMLDivElement>(null);
  const asked = useRef(0);

  useEffect(() => setTyped(search), [search]);

  // what is typed becomes the search a moment after typing stops
  useEffect(() => {
    if (typed.trim() === search) {
      return;
    }

    const timer = window.setTimeout(() => {
      setParams((was) => {
        const query = new URLSearchParams(was);

        if (typed.trim()) {
          query.set('q', typed.trim());
        } else {
          query.delete('q');
        }

        return query;
      }, { replace: true });
    }, 300);

    return () => window.clearTimeout(timer);
  }, [typed, search, setParams]);

  const load = useCallback(async (from: number) => {
    const mine = ++asked.current;

    setLoading(true);
    setFailure(null);

    try {
      const page = await api.sources.list({ search: search || undefined, order, skip: from, take: PAGE });

      // a newer search began meanwhile: this answer is for one nobody wants now
      if (mine !== asked.current) {
        return;
      }

      setEntries((known) => {
        const kept = from === 0 ? [] : known;
        const seen = new Set(kept.map((e) => e.publicKey));

        return [...kept, ...page.entries.filter((e) => !seen.has(e.publicKey))];
      });

      setTotal(page.total);
      setNext(page.next ?? null);
    } catch (error) {
      if (mine === asked.current) {
        setFailure(error instanceof ApiError ? error.message : said.failed);
      }
    } finally {
      if (mine === asked.current) {
        setLoading(false);
      }
    }
  }, [search, order, said]);

  // a new search or order starts over
  useEffect(() => {
    setTotal(null);
    setNext(0);
    load(0);
  }, [load]);

  // the next page, once the end of this one comes into view
  useEffect(() => {
    const element = sentinel.current;

    if (element === null || next === null || next === 0 || failure !== null || loading) {
      return;
    }

    const observer = new IntersectionObserver((seen) => seen.some((e) => e.isIntersecting) && load(next), { rootMargin: '600px 0px' });

    observer.observe(element);

    return () => observer.disconnect();
  }, [load, next, failure, loading]);

  const setOrder = (value: SourceOrder) =>
    setParams((was) => {
      const query = new URLSearchParams(was);

      if (value === 'stars') {
        query.delete('order');
      } else {
        query.set('order', value);
      }

      return query;
    }, { replace: true });

  const first = total === null;

  return (
    <div className="mx-auto w-full max-w-5xl px-5 pb-24 pt-10 sm:px-6 sm:pt-14">
      <header>
        <p className="text-xs font-medium uppercase tracking-[0.2em] text-accent-500 dark:text-accent-400">{said.eyebrow}</p>
        <h1 className="mt-3 text-3xl font-bold tracking-tight sm:text-4xl">{said.title}</h1>
        <p className="mt-3 max-w-2xl text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">{said.intro}</p>
      </header>

      <LambdaNotice className="mt-8" />

      <div className="mt-10 flex flex-wrap items-center gap-3">
        <label className="relative min-w-0 flex-1 basis-72">
          <span className="sr-only">{said.searchLabel}</span>
          <IconSearch className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            value={typed}
            onChange={(event) => setTyped(event.target.value)}
            placeholder={said.searchPlaceholder}
            className="field !py-2.5 pl-9"
            autoComplete="off"
            spellCheck={false}
          />
        </label>

        <div role="radiogroup" aria-label={said.orderLabel} className="flex flex-wrap gap-1.5">
          {ORDERS.map((value) => (
            <button
              key={value}
              type="button"
              role="radio"
              aria-checked={order === value}
              onClick={() => setOrder(value)}
              className={`rounded-full border px-3 py-1.5 text-[13px] transition-colors ${
                order === value
                  ? 'border-accent-500 bg-accent-500/10 text-accent-700 dark:border-accent-400 dark:text-accent-400'
                  : 'border-slate-200 text-slate-600 hover:border-slate-300 hover:text-slate-900 dark:border-ink-800 dark:text-slate-400 dark:hover:border-ink-700 dark:hover:text-slate-200'
              }`}
            >
              {said.orders[value]}
            </button>
          ))}
        </div>
      </div>

      <p className="mt-4 h-5 text-sm tabular-nums text-slate-500" aria-live="polite">
        {total !== null && total > 0 && said.counted(total)}
      </p>

      {first && !failure ? (
        <List>
          {Array.from({ length: 4 }, (_, i) => <Placeholder key={i} />)}
        </List>
      ) : total === 0 && !failure ? (
        search ? <NoMatch query={search} onClear={() => setTyped('')} /> : <Nothing />
      ) : (
        <List>
          {entries.map((entry, i) => (
            <Row key={entry.publicKey} entry={entry} index={i} />
          ))}
        </List>
      )}

      <div ref={sentinel} aria-hidden="true" />

      {failure && (
        <div className="mt-10 flex flex-col items-center gap-3 text-center">
          <p className="text-sm text-slate-600 dark:text-slate-400">{failure}</p>
          <button type="button" className="btn-ghost" onClick={() => load(entries.length === 0 ? 0 : (next ?? 0))}>
            {t.common.tryAgain}
          </button>
        </div>
      )}

      {!first && !failure && next !== null && next > 0 && (
        <div className="mt-10 flex justify-center">
          <button type="button" className="btn-ghost" onClick={() => load(next)} disabled={loading}>
            {loading && <IconSpinner />}
            {loading ? said.loadingMore : said.showMore}
          </button>
        </div>
      )}

      {total !== null && total > 0 && <Yours />}
    </div>
  );
}

function List({ children }: { children: React.ReactNode }) {
  return <ul className="mt-2 grid gap-3 md:grid-cols-2">{children}</ul>;
}

/**
 * One published source: its name and what it is, and the facts that tell it
 * from the next - arriving a little after the one before it.
 */
function Row({ entry, index }: { entry: SourceEntry; index: number }) {
  const said = useSourceT().catalog;
  const format = useFormat();

  const about = entry.about ?? entry.description;

  return (
    <li className="rise" style={{ animationDelay: `${(index % PAGE) * 30}ms` }}>
      <Link
        to={entry.path}
        className="surface group flex h-full gap-4 p-4 transition-[border-color,box-shadow] duration-200 hover:border-accent-500/60 hover:shadow-md focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent-500 dark:hover:border-accent-400/50"
      >
        <div className="min-w-0 flex-1">
          <div className="flex items-baseline gap-2">
            <h2 className="truncate font-mono text-[15px] font-semibold text-accent-600 group-hover:underline dark:text-accent-400">
              {entry.publicKey}
            </h2>
            {entry.title && <span className="truncate text-[13px] text-slate-500">{entry.title}</span>}
          </div>

          <p className="mt-1.5 line-clamp-2 min-h-[2.5rem] text-sm leading-5 text-slate-600 dark:text-slate-400">{about}</p>

          <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-1.5 text-[13px] text-slate-500">
            <LicenseChip license={entry.license} plain />
            <span className="inline-flex items-center gap-1 tabular-nums" title={said.stars(entry.stars)}>
              <IconStar className="h-3.5 w-3.5" />
              {format.count(entry.stars)}
            </span>
            {entry.updated && <span title={format.moment(entry.updated)}>{said.changed(format.ago(entry.updated))}</span>}
            <span className="inline-flex items-center gap-1.5" title={entry.online ? shownAddress(entry.address) : undefined}>
              <span className={`h-1.5 w-1.5 rounded-full ${entry.online ? 'bg-emerald-500' : 'bg-slate-400'}`} aria-hidden="true" />
              {entry.online ? said.online : said.offline}
            </span>
          </div>
        </div>

        {entry.imagePath && (
          <div className="hidden aspect-[16/10] w-28 shrink-0 self-start overflow-hidden bg-grey-100 sm:block dark:bg-ink-850">
            <img src={entry.imagePath} alt="" loading="lazy" decoding="async" className="h-full w-full object-cover" />
          </div>
        )}
      </Link>
    </li>
  );
}

function Placeholder() {
  return (
    <li className="surface space-y-3 p-4" aria-hidden="true">
      <div className="h-4 w-1/3 animate-pulse bg-grey-100 dark:bg-ink-850" />
      <div className="h-3 w-full animate-pulse bg-grey-100 dark:bg-ink-850" />
      <div className="h-3 w-4/5 animate-pulse bg-grey-100 dark:bg-ink-850" />
      <div className="h-3 w-1/2 animate-pulse bg-grey-100 dark:bg-ink-850" />
    </li>
  );
}

/** Nothing published at all: an invitation rather than an error. */
function Nothing() {
  const said = useSourceT().catalog;

  return (
    <div className="surface mt-2 px-6 py-14 text-center">
      <h2 className="text-lg font-semibold tracking-tight">{said.nothingTitle}</h2>
      <p className="mx-auto mt-2 max-w-md text-sm leading-relaxed text-slate-600 dark:text-slate-400">
        {said.nothing((text) => <b>{text}</b>)}
      </p>
      <Link to="/build" className="btn-primary mt-6">
        {said.build}
      </Link>
    </div>
  );
}

function NoMatch({ query, onClear }: { query: string; onClear: () => void }) {
  const said = useSourceT().catalog;

  return (
    <div className="surface mt-2 px-6 py-14 text-center">
      <h2 className="text-lg font-semibold tracking-tight">{said.noMatchTitle}</h2>
      <p className="mx-auto mt-2 max-w-md text-sm text-slate-600 dark:text-slate-400">{said.noMatch(query)}</p>
      <button type="button" className="btn-ghost mt-5" onClick={onClear}>
        {said.clear}
      </button>
    </div>
  );
}

/** How to be on this page, for whoever has just scrolled through it. */
function Yours() {
  const said = useSourceT().catalog;

  return (
    <aside className="mt-20 grid gap-6 border-t border-slate-200 pt-10 dark:border-ink-800 md:grid-cols-[1fr_auto] md:items-center">
      <div>
        <h2 className="text-lg font-semibold tracking-tight">{said.yoursTitle}</h2>
        <p className="mt-1.5 max-w-2xl text-sm leading-relaxed text-slate-600 dark:text-slate-400">
          {said.yours((text) => <b>{text}</b>)}
        </p>
      </div>
      <Link to="/build" className="btn-primary justify-self-start">
        {said.build}
      </Link>
    </aside>
  );
}
