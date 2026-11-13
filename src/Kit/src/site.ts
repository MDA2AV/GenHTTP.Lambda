import { useEffect, useState, useSyncExternalStore } from 'react';

import { aroundKey } from './address';
import { api, type Platform, type ShowcaseListing } from './api';

/**
 * What only the server knows about a page: where it is answered from, where
 * the lambdas answer, how long this installation keeps a lambda and, on the
 * showcase, what is on it.
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
  /**
   * What a lambda's address is shown with before its key, and after it:
   * "" and ".genhttp.run" where the lambdas answer at subdomains,
   * "localhost:8080/lambda/" and "" where they answer below a path.
   */
  before: string;
  after: string;
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
  before: '__LAMBDA_BEFORE__',
  after: '__LAMBDA_AFTER__',
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

/** What the installation says about itself, asked for once by whichever page needs it first. */
let asking: Promise<Platform> | null = null;

/**
 * What the installation says about itself - the terms, the starters, where a
 * lambda answers - asked for once however many parts of a page want it.
 */
export const askPlatform = () => (asking ??= api.platform().catch((error) => {
  asking = null;
  throw error;
}));

/**
 * Where a lambda with the given key would answer, as an address is shown:
 * "q7x2k9.genhttp.run", or "localhost:8080/lambda/q7x2k9" where the lambdas
 * answer below a path. Sent with a rendered page, and asked for otherwise -
 * nothing until it is known, since any other address would be a wrong example.
 */
export function useExampleAddress(key: string): string | null {
  const facts = sentFacts();
  const [asked, setAsked] = useState<{ before: string; after: string } | null>(null);

  useEffect(() => {
    if (facts !== null) {
      return;
    }

    askPlatform()
      .then((platform) => {
        const { before, after } = aroundKey(platform.lambdaUrl);

        setAsked({ before: before.replace(/^https?:\/\//, ''), after: after.replace(/\/$/, '') });
      })
      .catch(() => undefined);
  }, [facts]);

  const around = facts ?? asked;

  return around === null ? null : around.before + key + around.after;
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

    askPlatform()
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
