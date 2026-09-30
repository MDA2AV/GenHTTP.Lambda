import { useEffect, useRef, useState } from 'react';

import { ApiError, api, type License } from '../api';
import { IconScale, IconSpinner, IconStar } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useSourceT } from '../i18n';
import { Link } from '../i18n/links';
import { useFormat } from './format';

/**
 * What a lambda is, said once where somebody meets one's code: in a line,
 * with the way to build one. Most people who arrive here came for the code,
 * not for the platform - so it is small, and it never gets in the way of the
 * code.
 */
export function LambdaNotice({ className = '', slim = false }: { className?: string; slim?: boolean }) {
  const said = useSourceT().lambda;

  // above the code of one lambda: a line, so the code stays where the eye lands
  if (slim) {
    return (
      <aside className={`flex flex-wrap items-baseline gap-x-3 gap-y-1 border-l-2 border-logo-500 bg-logo-500/[0.04] py-1.5 pl-3 pr-2 text-[13px] ${className}`}>
        <p className="min-w-0 flex-1 basis-72 leading-relaxed text-slate-600 dark:text-slate-400">
          <span aria-hidden="true" className="mr-1.5 font-semibold text-logo-500">λ</span>
          <span className="font-medium text-ink-900 dark:text-slate-100">{said.label}</span> {said.text}
        </p>
        <Link to="/build" className="shrink-0 whitespace-nowrap font-medium text-accent-600 hover:underline dark:text-accent-400">
          {said.build} →
        </Link>
      </aside>
    );
  }

  return (
    <aside className={`flex flex-wrap items-center gap-x-4 gap-y-2 border border-logo-500/25 bg-logo-500/[0.04] px-4 py-3 ${className}`}>
      <span aria-hidden="true" className="flex h-8 w-8 shrink-0 items-center justify-center bg-logo-500 text-lg font-semibold leading-none text-white">
        λ
      </span>
      <p className="min-w-0 flex-1 basis-64 text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">
        <span className="font-medium text-ink-900 dark:text-slate-100">{said.label}</span> {said.text}
      </p>
      <Link to="/build" className="btn-ghost shrink-0 !px-3 !py-1.5 text-[13px]">
        {said.build} →
      </Link>
    </aside>
  );
}

/**
 * The license a source is under, linking to its full text, with what it
 * allows on hover - or, inside something that is a link already, only saying
 * which it is.
 */
export function LicenseChip({ license, plain = false }: { license: License; plain?: boolean }) {
  const said = useSourceT();
  const summary = (said.licenses as Record<string, string>)[license.id];
  const title = summary ? `${license.name}: ${summary}` : license.name;

  if (plain) {
    return (
      <span title={title} className="inline-flex items-center gap-1.5 whitespace-nowrap">
        <IconScale className="h-3.5 w-3.5" />
        {license.id}
      </span>
    );
  }

  return (
    <a
      href={license.url}
      target="_blank"
      rel="noreferrer"
      title={title}
      className="inline-flex items-center gap-1.5 whitespace-nowrap text-[13px] text-slate-600 hover:text-accent-600 dark:text-slate-400 dark:hover:text-accent-400"
    >
      <IconScale className="h-3.5 w-3.5" />
      {license.id}
    </a>
  );
}

/** Where this browser remembers the sources it starred, so the star shows as given when it comes back. */
const STARRED = 'lambda-starred';

function remembered(): Set<string> {
  try {
    return new Set(JSON.parse(localStorage.getItem(STARRED) ?? '[]') as string[]);
  } catch {
    return new Set();
  }
}

function remember(publicKey: string, starred: boolean) {
  try {
    const keys = remembered();

    if (starred) {
      keys.add(publicKey);
    } else {
      keys.delete(publicKey);
    }

    localStorage.setItem(STARRED, JSON.stringify([...keys]));
  } catch {
    // private mode: the star is counted all the same, just not remembered here
  }
}

/** How old a ticket has to be before the server takes a star with it. */
const YOUNGEST = 1100;

/**
 * The star of a published source, and how many it has.
 *
 * A POST with the ticket the source was read with - never a link - which the
 * server counts once per visitor. The star shows at once and is corrected by
 * what the server says; a ticket too fresh to be taken is waited for, and one
 * that went stale is fetched again, so a visitor never sees why.
 */
export function StarButton({ publicKey, stars, ticket, ticketAt }: {
  publicKey: string;
  stars: number;
  ticket: string;
  /** When the ticket arrived, which it has to be a second older than. */
  ticketAt: number;
}) {
  const said = useSourceT().star;
  const format = useFormat();
  const toast = useToast();

  const [starred, setStarred] = useState(false);
  const [count, setCount] = useState(stars);
  const [busy, setBusy] = useState(false);

  const current = useRef({ ticket, ticketAt });

  useEffect(() => {
    current.current = { ticket, ticketAt };
  }, [ticket, ticketAt]);

  useEffect(() => setCount(stars), [stars]);

  // after the first render, so a page drawn without a browser looks the same
  useEffect(() => setStarred(remembered().has(publicKey)), [publicKey]);

  async function send(wanted: boolean, again = true): Promise<void> {
    const wait = YOUNGEST - (Date.now() - current.current.ticketAt);

    if (wait > 0) {
      await new Promise((resolve) => setTimeout(resolve, wait));
    }

    try {
      const result = await api.sources.star(publicKey, current.current.ticket, wanted);

      setCount(result.stars);
      remember(publicKey, wanted);
    } catch (error) {
      // a page left open for a day: read it again for a fresh ticket
      if (again && error instanceof ApiError && error.status === 400) {
        const fresh = await api.sources.get(publicKey);

        current.current = { ticket: fresh.starTicket, ticketAt: Date.now() };

        return send(wanted, false);
      }

      throw error;
    }
  }

  async function toggle() {
    if (busy) {
      return;
    }

    const wanted = !starred;

    setBusy(true);
    setStarred(wanted);
    setCount((was) => Math.max(0, was + (wanted ? 1 : -1)));

    try {
      await send(wanted);
    } catch (error) {
      setStarred(!wanted);
      setCount((was) => Math.max(0, was + (wanted ? -1 : 1)));
      toast(error instanceof ApiError ? error.message : said.failed, 'error');
    } finally {
      setBusy(false);
    }
  }

  return (
    <button
      type="button"
      onClick={toggle}
      aria-pressed={starred}
      title={starred ? said.remove : said.add}
      className={`inline-flex h-9 items-stretch overflow-hidden rounded-full border text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent-500 ${
        starred
          ? 'border-amber-500/60 bg-amber-500/10 text-amber-700 dark:text-amber-400'
          : 'border-slate-300 text-slate-700 hover:border-slate-400 hover:bg-slate-50 dark:border-ink-700 dark:text-slate-200 dark:hover:bg-ink-850'
      }`}
    >
      <span className="flex items-center gap-1.5 pl-3.5 pr-3">
        {busy ? <IconSpinner className="h-4 w-4" /> : <IconStar filled={starred} className="h-4 w-4" />}
        {said.star}
      </span>
      <span
        className={`flex items-center border-l px-3 tabular-nums ${starred ? 'border-amber-500/40' : 'border-slate-300 dark:border-ink-700'}`}
        aria-label={said.count(count)}
      >
        {format.count(count)}
      </span>
    </button>
  );
}
