import { useEffect, useState } from 'react';

const KEY = 'lambda-admin-token';

/*
 * The token is shared between the menu that asks for it and the pages it opens,
 * so it lives in one place with a notification rather than in each component's
 * own state - otherwise unlocking in the header leaves whatever is already on
 * screen still locked until it is navigated to again.
 *
 * Local storage, so that an admin link opened in a new tab is unlocked too and
 * the token survives closing the browser. It stays until "Lock again" is used
 * or the server rejects it - which is the way to leave a shared machine.
 */
const CHANGED = 'lambda-admin-token-changed';

export function readToken(): string {
  try {
    return localStorage.getItem(KEY) ?? '';
  } catch {
    // storage is unavailable in a locked down browser; the panel then asks
    // once per page rather than not working at all
    return '';
  }
}

export function writeToken(token: string) {
  try {
    if (token === '') {
      localStorage.removeItem(KEY);
    } else {
      localStorage.setItem(KEY, token);
    }
  } catch {
    // as above
  }

  window.dispatchEvent(new Event(CHANGED));
}

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
