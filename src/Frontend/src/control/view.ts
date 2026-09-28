import { useCallback, useEffect, useState } from 'react';

/**
 * Which of the two views of the control center is showing.
 *
 * The simple one is for somebody who had an app built from a sentence: the
 * app, how it is doing and a box to ask for a change, and nothing about
 * code, files, versions or deployments. The full one is every section.
 *
 * A lambda says which it opens in - simple when it was built on /build, full
 * otherwise - but that is only for somebody who has not chosen. Whoever
 * switches has chosen for themselves: kept in their browser, for every
 * lambda they open, and never sent anywhere. That is the point of it being
 * here and not on the server - an operator who opens somebody's lambda in the
 * full view leaves it simple for its owner - and why it is one choice rather
 * than one per lambda: which view suits somebody is about them, not about
 * whichever lambda they happen to have open.
 */
export type View = 'simple' | 'full';

const KEY = 'lambda-editor-view';

function read(): View | null {
  try {
    const stored = localStorage.getItem(KEY);

    return stored === 'simple' || stored === 'full' ? stored : null;
  } catch {
    // a browser that keeps nothing gets what the lambda says
    return null;
  }
}

export function useView(fallback: string | undefined): { view: View; choose: (view: View) => void } {
  const [chosen, setChosen] = useState<View | null>(read);

  // switched in another tab: this one follows
  useEffect(() => {
    const changed = (event: StorageEvent) => {
      if (event.key === KEY) {
        setChosen(read());
      }
    };

    window.addEventListener('storage', changed);

    return () => window.removeEventListener('storage', changed);
  }, []);

  const choose = useCallback((view: View) => {
    setChosen(view);

    try {
      localStorage.setItem(KEY, view);
    } catch {
      // kept for this page, then; the next one asks the lambda again
    }
  }, []);

  return { view: chosen ?? (fallback === 'Simple' ? 'simple' : 'full'), choose };
}
