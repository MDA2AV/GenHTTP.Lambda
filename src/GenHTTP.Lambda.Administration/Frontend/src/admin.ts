import { useEffect, useState } from 'react';

import { askPlatform } from '@genhttp-lambda/kit/site';

const KEY = 'lambda-admin-token';

/*
 * The token is shared between the menu that asks for it and the pages it opens,
 * so it lives in one place with a notification rather than in each component's
 * own state - otherwise unlocking in the header leaves whatever is already on
 * screen still locked until it is navigated to again.
 *
 * Where the lambdas answer at hosts of their own, the token is kept in local
 * storage, so that an admin link opened in a new tab is unlocked too and the
 * token survives closing the browser. It stays until "Lock again" is used or
 * the server rejects it - which is the way to leave a shared machine.
 *
 * Where they answer below /lambda/ on the panel's own host, a lambda's page
 * could read whatever this origin keeps - so the token is kept for the tab
 * only (session storage), out of reach of a lambda opened in another one,
 * and asked for again in a new tab. Until the platform has said which it is,
 * it is kept for the tab only.
 */
const CHANGED = 'lambda-admin-token-changed';

/** Whether the lambdas answer on this origin; unknown until the platform said. */
let shared: boolean | null = null;

function storages(): Storage[] {
  try {
    return shared === false ? [sessionStorage, localStorage] : [sessionStorage];
  } catch {
    return [];
  }
}

export function readToken(): string {
  try {
    // local storage holds one only where it was written once nothing of a
    // lambda was known to run here - so it is read until the platform says
    // otherwise, and a panel that was unlocked opens unlocked
    return sessionStorage.getItem(KEY) ?? (shared !== true ? localStorage.getItem(KEY) : null) ?? '';
  } catch {
    // storage is unavailable in a locked down browser; the panel then asks
    // once per page rather than not working at all
    return '';
  }
}

export function writeToken(token: string) {
  try {
    for (const storage of storages()) {
      if (token === '') {
        storage.removeItem(KEY);
      } else {
        storage.setItem(KEY, token);
      }
    }

    if (token === '') {
      // locking leaves nothing behind wherever the token was kept before
      localStorage.removeItem(KEY);
    }
  } catch {
    // as above
  }

  window.dispatchEvent(new Event(CHANGED));
}

askPlatform()
  .then((platform) => {
    shared = new URL(platform.lambdaUrl.replace('{key}', 'key'), window.location.href).origin === window.location.origin;

    try {
      if (shared) {
        // kept by a version of the panel that did not know better
        localStorage.removeItem(KEY);
      } else {
        const kept = localStorage.getItem(KEY);
        const current = sessionStorage.getItem(KEY);

        if (current !== null && kept === null) {
          localStorage.setItem(KEY, current);
        }
      }
    } catch {
      // as above
    }

    window.dispatchEvent(new Event(CHANGED));
  })
  .catch(() => undefined);

/** The token as it stands, kept current as other components change it. */
export function useAdminToken(): [string, (token: string) => void] {
  const [token, setToken] = useState(readToken);

  useEffect(() => {
    const sync = () => setToken(readToken());

    window.addEventListener(CHANGED, sync);
    // another tab unlocking or locking counts too
    window.addEventListener('storage', sync);

    return () => {
      window.removeEventListener(CHANGED, sync);
      window.removeEventListener('storage', sync);
    };
  }, []);

  return [token, writeToken];
}
