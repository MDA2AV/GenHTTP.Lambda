import { useEffect, useState } from 'react';

import { api, type Features } from './api';

const KEY = 'lambda-features';

const CHANGED = 'lambda-features-changed';

/*
 * Which pages the header links to. Asked once per page load and shared by
 * every header, since the answer only changes when the operator switches
 * something in the panel.
 *
 * The last answer is remembered in this browser, so a returning visitor does
 * not see a link appear a moment after the page. Until there is any answer,
 * nothing optional is linked: a link popping in is better than one that is
 * shown and then taken away.
 */
const NONE: Features = { enterprise: false };

let current: Features = remembered();

let pending: Promise<void> | null = null;

function remembered(): Features {
  try {
    const stored = localStorage.getItem(KEY);
    return stored ? { ...NONE, ...JSON.parse(stored) } : NONE;
  } catch {
    return NONE;
  }
}

/** Takes a new answer, from the server or from the panel that just changed it. */
export function publishFeatures(features: Features) {
  current = features;

  try {
    localStorage.setItem(KEY, JSON.stringify(features));
  } catch {
    // private mode; it is asked again on the next page load anyway
  }

  window.dispatchEvent(new Event(CHANGED));
}

export function useFeatures(): Features {
  const [features, setFeatures] = useState(current);

  useEffect(() => {
    const sync = () => setFeatures(current);

    window.addEventListener(CHANGED, sync);

    pending ??= api.features().then(publishFeatures, () => undefined);

    // an answer that came in before this header was listening
    sync();

    return () => window.removeEventListener(CHANGED, sync);
  }, []);

  return features;
}
