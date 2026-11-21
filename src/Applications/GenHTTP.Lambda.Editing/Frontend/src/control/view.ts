import { useCallback, useEffect, useState } from 'react';

/**
 * Which of the two views of the control center is showing.
 *
 * The simple one is for somebody who had an app built from a sentence: the
 * app, how it is doing and where to ask for a change, and nothing about code,
 * files, versions or deployments. The full one is every section.
 *
 * A lambda says which it opens in - simple when it was built on /build, full
 * otherwise - but that is only for somebody who has not chosen. Whoever
 * switches has chosen for themselves, for that lambda: kept in their browser
 * and never sent anywhere. That is the point of it being here and not on the
 * server - an operator who opens somebody's lambda in the full view leaves it
 * simple for its owner.
 *
 * Kept by the public key, like the draft of a change: moving the lambda to
 * another key forgets the choice, and it opens as the lambda says again.
 */
export type View = 'simple' | 'full';

const PREFIX = 'lambda-editor-view:';

/** What was chosen on this page, for a browser that keeps nothing: the switch still switches until it is left. */
const kept = new Map<string, View>();

function read(key: string): View | null {
  try {
    const stored = localStorage.getItem(key);

    if (stored === 'simple' || stored === 'full') {
      return stored;
    }
  } catch {
    // nothing stored, so nothing to read
  }

  return kept.get(key) ?? null;
}

/**
 * @param lambda the public key of the lambda, while it is known
 * @param fallback the view it says it opens in
 */
export function useView(lambda: string | undefined, fallback: string | undefined): { view: View; choose: (view: View) => void } {
  const key = lambda ? `${PREFIX}${lambda}` : null;

  // read on every render rather than held, so the choice for a lambda is
  // there the moment its key is, without a frame of the other view first
  const [, changed] = useState(0);

  // switched in another tab: this one follows
  useEffect(() => {
    const follow = (event: StorageEvent) => {
      if (event.key === key) {
        changed((count) => count + 1);
      }
    };

    window.addEventListener('storage', follow);

    return () => window.removeEventListener('storage', follow);
  }, [key]);

  const choose = useCallback((view: View) => {
    if (!key) {
      return;
    }

    kept.set(key, view);

    try {
      localStorage.setItem(key, view);
    } catch {
      // kept for this page only, then
    }

    changed((count) => count + 1);
  }, [key]);

  const chosen = key ? read(key) : null;

  return { view: chosen ?? (fallback === 'Simple' ? 'simple' : 'full'), choose };
}
