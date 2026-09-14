import { useEffect, useId, useRef, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { useAdminToken } from '../admin';
import { IconChevronDown, IconLock, IconUnlock } from './Icons';

/**
 * The way in to everything an operator can see.
 *
 * Both pages behind this ask the server for the token as well, so this is a
 * door rather than a disguise - hiding the links while the API answered anyone
 * who asked would be worth nothing.
 */
export function AdminMenu() {
  const [token, setToken] = useAdminToken();
  const [open, setOpen] = useState(false);
  const [entered, setEntered] = useState('');

  const box = useRef<HTMLDivElement>(null);
  const field = useRef<HTMLInputElement>(null);
  const menu = useId();

  const unlocked = token !== '';

  // a menu that stays open after the pointer has gone elsewhere is a menu in
  // the way, and escape is what people press when something is in the way
  useEffect(() => {
    if (!open) {
      return;
    }

    const outside = (event: MouseEvent) => {
      if (!box.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    };

    const key = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setOpen(false);
      }
    };

    document.addEventListener('mousedown', outside);
    document.addEventListener('keydown', key);

    return () => {
      document.removeEventListener('mousedown', outside);
      document.removeEventListener('keydown', key);
    };
  }, [open]);

  useEffect(() => {
    if (open && !unlocked) {
      field.current?.focus();
    }
  }, [open, unlocked]);

  function unlock(event: React.FormEvent) {
    event.preventDefault();

    if (entered.trim() === '') {
      return;
    }

    setToken(entered.trim());
    setEntered('');
  }

  function lock() {
    setToken('');
    setOpen(false);
  }

  return (
    <div ref={box} className="relative hidden sm:block">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        className="btn-ghost inline-flex items-center gap-1.5"
        aria-expanded={open}
        aria-haspopup="menu"
        aria-controls={open ? menu : undefined}
        title={unlocked ? 'Administration, unlocked for this tab' : 'Administration, needs the token'}
      >
        {unlocked ? <IconUnlock className="h-4 w-4" /> : <IconLock className="h-4 w-4" />}
        Admin
        <IconChevronDown className={`h-3.5 w-3.5 transition-transform ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <div
          id={menu}
          role="menu"
          className="absolute right-0 z-40 mt-1 w-72 border border-slate-200 bg-white p-1 shadow-lg dark:border-ink-800 dark:bg-ink-900"
        >
          {unlocked ? (
            <>
              <Item to="/stats" label="Server" note="Memory, connections and what the engine is doing" onGo={() => setOpen(false)} />
              <Item to="/admin" label="Lambdas" note="Every lambda on the installation" onGo={() => setOpen(false)} />

              <button
                type="button"
                onClick={lock}
                role="menuitem"
                className="mt-1 flex w-full items-center gap-2 border-t border-slate-200 px-3 py-2 text-left text-sm text-slate-600 hover:bg-slate-50 dark:border-ink-800 dark:text-slate-400 dark:hover:bg-ink-850"
              >
                <IconLock className="h-4 w-4" />
                Lock again
              </button>
            </>
          ) : (
            <form onSubmit={unlock} className="p-2">
              <p className="text-xs text-slate-500">
                The server figures and the lambdas of other people are behind this. It is the token the host
                was started with.
              </p>

              <input
                ref={field}
                type="password"
                value={entered}
                onChange={(event) => setEntered(event.target.value)}
                placeholder="Admin token"
                autoComplete="off"
                className="field mt-2 font-mono text-sm"
              />

              <button type="submit" className="btn-primary mt-2 w-full py-2 text-sm">
                Unlock
              </button>
            </form>
          )}
        </div>
      )}
    </div>
  );
}

function Item({ to, label, note, onGo }: { to: string; label: string; note: string; onGo: () => void }) {
  const navigate = useNavigate();

  return (
    <Link
      to={to}
      role="menuitem"
      onClick={(event) => {
        event.preventDefault();
        onGo();
        navigate(to);
      }}
      className="block px-3 py-2 hover:bg-slate-50 dark:hover:bg-ink-850"
    >
      <div className="text-sm">{label}</div>
      <div className="text-xs text-slate-500">{note}</div>
    </Link>
  );
}
