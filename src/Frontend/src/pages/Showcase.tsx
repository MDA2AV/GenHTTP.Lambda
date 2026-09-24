import { useCallback, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError, api, type ShowcaseEntry } from '../api';
import { IconSpinner } from '../components/Icons';
import { ShowcaseCard } from '../components/ShowcaseCard';
import { PAGES, usePageMeta } from '../meta';

const PAGE = 12;

/**
 * What people have built here and chosen to show.
 *
 * Every entry is a lambda that is online right now, put here by whoever holds
 * its editor key, in the order of how much is going on with it. The list
 * grows as it is scrolled rather than being cut into numbered pages: nobody
 * browsing pictures wants to aim for a page two, and the order shifts with
 * use anyway, so a page number would not name the same thing twice.
 */
export function Showcase() {
  usePageMeta(PAGES['/showcase']);

  const [entries, setEntries] = useState<ShowcaseEntry[]>([]);
  const [total, setTotal] = useState<number | null>(null);
  const [next, setNext] = useState<number | null>(0);
  const [loading, setLoading] = useState(false);
  const [failure, setFailure] = useState<string | null>(null);

  const sentinel = useRef<HTMLDivElement>(null);
  const busy = useRef(false);

  const more = useCallback(async () => {
    if (busy.current || next === null) {
      return;
    }

    busy.current = true;
    setLoading(true);
    setFailure(null);

    try {
      const page = await api.showcases(next, PAGE);

      // the order moves with use while somebody scrolls, so an entry can turn
      // up on two pages - it is shown where it was seen first
      setEntries((known) => {
        const seen = new Set(known.map((entry) => entry.publicKey));
        return [...known, ...page.entries.filter((entry) => !seen.has(entry.publicKey))];
      });

      setTotal(page.total);
      setNext(page.next ?? null);
    } catch (error) {
      setFailure(error instanceof ApiError ? error.message : 'The showcase could not be loaded.');
    } finally {
      busy.current = false;
      setLoading(false);
    }
  }, [next]);

  // the first page
  useEffect(() => {
    if (total === null && !busy.current) {
      more();
    }
  }, [more, total]);

  // the next one, once the end of this one comes into view
  useEffect(() => {
    const element = sentinel.current;

    if (element === null || next === null || failure !== null) {
      return;
    }

    const observer = new IntersectionObserver((seen) => seen.some((entry) => entry.isIntersecting) && more(), {
      rootMargin: '600px 0px',
    });

    observer.observe(element);

    return () => observer.disconnect();
  }, [more, next, failure]);

  const first = total === null;

  return (
    <div className="mx-auto w-full max-w-6xl px-5 pb-24 pt-12 sm:px-6 sm:pt-16">
      <header className="flex flex-wrap items-end justify-between gap-x-10 gap-y-4">
        <div className="max-w-2xl">
          <p className="text-xs font-medium uppercase tracking-[0.2em] text-accent-500 dark:text-accent-400">Showcase</p>
          <h1 className="mt-3 text-3xl font-bold tracking-tight sm:text-4xl">Built here, running now</h1>
          <p className="mt-3 text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
            Lambdas their owners chose to show. Every one of them is online, so each card opens the real
            thing. The ones in use lately come first.
          </p>
        </div>

        {total !== null && total > 0 && (
          <p className="text-sm tabular-nums text-slate-500">
            {total} {total === 1 ? 'lambda' : 'lambdas'}
          </p>
        )}
      </header>

      {first && !failure ? (
        <Grid>
          {Array.from({ length: 6 }, (_, i) => <Placeholder key={i} />)}
        </Grid>
      ) : total === 0 ? (
        <Nothing />
      ) : (
        <Grid>
          {entries.map((entry, i) => (
            <div key={entry.publicKey} className="rise" style={{ animationDelay: `${(i % PAGE) * 40}ms` }}>
              <ShowcaseCard
                title={entry.title}
                description={entry.description}
                publicKey={entry.publicKey}
                image={entry.imagePath}
                href={entry.path}
              />
            </div>
          ))}
        </Grid>
      )}

      <div ref={sentinel} aria-hidden="true" />

      {failure && (
        <div className="mt-10 flex flex-col items-center gap-3 text-center">
          <p className="text-sm text-slate-600 dark:text-slate-400">{failure}</p>
          <button type="button" className="btn-ghost" onClick={() => more()}>
            Try again
          </button>
        </div>
      )}

      {!first && !failure && next !== null && (
        <div className="mt-10 flex justify-center">
          {/* the observer does this on its own; the button is for anybody it does not reach */}
          <button type="button" className="btn-ghost" onClick={() => more()} disabled={loading}>
            {loading && <IconSpinner />}
            {loading ? 'Loading more…' : 'Show more'}
          </button>
        </div>
      )}

      {total !== null && total > 0 && <Yours />}
    </div>
  );
}

function Grid({ children }: { children: React.ReactNode }) {
  return <div className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">{children}</div>;
}

/** The shape of a card, while the cards are on their way. */
function Placeholder() {
  return (
    <div className="surface flex flex-col overflow-hidden" aria-hidden="true">
      <div className="aspect-[16/10] animate-pulse bg-grey-100 dark:bg-ink-850" />
      <div className="space-y-2.5 p-4">
        <div className="h-4 w-1/2 animate-pulse bg-grey-100 dark:bg-ink-850" />
        <div className="h-3 w-full animate-pulse bg-grey-100 dark:bg-ink-850" />
        <div className="h-3 w-4/5 animate-pulse bg-grey-100 dark:bg-ink-850" />
      </div>
    </div>
  );
}

/** An empty showcase, which is an invitation rather than an error. */
function Nothing() {
  return (
    <div className="surface mt-10 px-6 py-14 text-center">
      <h2 className="text-lg font-semibold tracking-tight">Nothing on show yet</h2>
      <p className="mx-auto mt-2 max-w-md text-sm leading-relaxed text-slate-600 dark:text-slate-400">
        Built something that works? Open its control center, choose <b>Showcase</b>, and add a title, a few
        words and a picture. It appears here while it is online.
      </p>
      <div className="mt-6 flex flex-wrap justify-center gap-2">
        <Link to="/build" className="btn-primary">Build one</Link>
        <Link to="/examples/game" className="btn-ghost">See an example</Link>
      </div>
    </div>
  );
}

/** How to be on this page, for whoever has just scrolled through it. */
function Yours() {
  return (
    <aside className="mt-20 grid gap-6 border-t border-slate-200 pt-10 dark:border-ink-800 md:grid-cols-[1fr_auto] md:items-center">
      <div>
        <h2 className="text-lg font-semibold tracking-tight">Want yours here?</h2>
        <p className="mt-1.5 max-w-2xl text-sm leading-relaxed text-slate-600 dark:text-slate-400">
          Open the control center of your lambda and choose <b>Showcase</b>, or ask the agent that built it to
          showcase it. Only whoever holds the editor key can, and it can be taken down again at any time.
        </p>
      </div>
      <Link to="/build" className="btn-primary justify-self-start">Build something</Link>
    </aside>
  );
}
