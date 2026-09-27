import { useEffect, useState, useSyncExternalStore } from 'react';

import { api, type ShowcaseListing } from './api';

/**
 * What only the server knows about a page: where it is answered from, how
 * long this installation keeps a lambda and, on the showcase, what is on it.
 *
 * The public pages are rendered to markup when the frontend is built, so a
 * crawler reads them without running a script. The build cannot know any of
 * this, so it renders a placeholder for each value instead. The server swaps
 * them for the real ones before sending the page, and hands the same values
 * over in `#site-facts` - so the first render in the browser draws exactly
 * the markup it was sent, and React takes it over rather than redrawing it.
 */
export interface SiteFacts {
  /** Scheme and host, without a trailing slash. */
  origin: string;
  host: string;
  /** How long a deployment stays up without being used. */
  lifetimeHours: number;
  /** The same, in the days it is written as on most pages. */
  offlineDays: number;
  /** How long an unused lambda is kept before it is removed. */
  retentionDays: number;
  /** The first page of the showcase, on the showcase only. */
  showcase?: ShowcaseListing | null;
}

/**
 * The placeholders the build renders, which the server replaces. Typed as the
 * values they stand for, because to the components that is what they are.
 */
export const PLACEHOLDERS = {
  origin: '__LAMBDA_ORIGIN__',
  host: '__LAMBDA_HOST__',
  lifetimeHours: '__LAMBDA_LIFETIME_HOURS__' as unknown as number,
  offlineDays: '__LAMBDA_OFFLINE_DAYS__' as unknown as number,
  retentionDays: '__LAMBDA_RETENTION_DAYS__' as unknown as number,
} satisfies SiteFacts;

/** Until the installation has said otherwise: its defaults. */
const DEFAULT_LIFETIME_HOURS = 30 * 24;
const DEFAULT_RETENTION_DAYS = 90;

let prerendered: SiteFacts | null = null;

/** Called by the build before rendering, never in a browser. */
export function prerenderWith(facts: SiteFacts) {
  prerendered = facts;
}

let sent: SiteFacts | null | undefined;

/** The facts the server sent along with the page, if it rendered one. */
function sentFacts(): SiteFacts | null {
  if (prerendered !== null) {
    return prerendered;
  }

  if (sent === undefined) {
    try {
      const element = document.getElementById('site-facts');
      sent = element?.textContent ? (JSON.parse(element.textContent) as SiteFacts) : null;
    } catch {
      sent = null;
    }
  }

  return sent;
}

/** Where the page is answered from, as `https://host`. */
export function useOrigin(): { origin: string; host: string } {
  const facts = sentFacts();

  return facts ?? { origin: window.location.origin, host: window.location.host };
}

export interface Lifetimes {
  lifetimeHours: number;
  offlineDays: number;
  retentionDays: number;
}

/**
 * How long this installation keeps a lambda. Sent with a rendered page, and
 * asked for otherwise - after navigating here from a page that was not.
 */
export function useLifetimes(): Lifetimes {
  const facts = sentFacts();

  const [asked, setAsked] = useState<Lifetimes | null>(null);

  useEffect(() => {
    if (facts !== null) {
      return;
    }

    api
      .platform()
      .then((platform) =>
        setAsked({
          lifetimeHours: platform.deploymentLifetimeHours,
          offlineDays: Math.round(platform.deploymentLifetimeHours / 24),
          retentionDays: platform.retentionDays,
        }),
      )
      .catch(() => undefined);
  }, [facts]);

  return (
    facts ??
    asked ?? {
      lifetimeHours: DEFAULT_LIFETIME_HOURS,
      offlineDays: Math.round(DEFAULT_LIFETIME_HOURS / 24),
      retentionDays: DEFAULT_RETENTION_DAYS,
    }
  );
}

/** The first page of the showcase as the server sent it, if it did. */
export function sentShowcase(): ShowcaseListing | null {
  return sentFacts()?.showcase ?? null;
}

/**
 * Drops the showcase the server sent, once it is on screen: a visitor who
 * comes back to the showcase later wants it as it is by then.
 */
export function forgetShowcase() {
  if (sent) {
    sent.showcase = null;
  }
}

const never = () => () => undefined;

/**
 * A value the browser knows and a prerendered page cannot, such as a setting
 * remembered in this browser. While React takes over the markup it was sent,
 * the value the markup was rendered with is used, and the real one right
 * after - anything else would not match what is already on screen.
 */
export function useBrowserValue<T>(read: () => T, rendered: T, subscribe: (changed: () => void) => () => void = never): T {
  return useSyncExternalStore(subscribe, read, () => rendered);
}
