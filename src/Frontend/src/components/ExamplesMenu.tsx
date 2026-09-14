import { useEffect, useId, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { api, type ExampleListing } from '../api';
import { IconChevronDown } from './Icons';

/**
 * The way into the lambdas the installation keeps running.
 *
 * Two levels rather than one list: the six of them split into the same two
 * kinds the editor offers, and a flat list of six would make a visitor read
 * all of them to find out there are only two decisions to make.
 *
 * The listing is fetched when the menu is first opened rather than with the
 * page, because most visits never open it.
 */
export function ExamplesMenu() {
  const navigate = useNavigate();

  const [open, setOpen] = useState(false);
  const [listing, setListing] = useState<ExampleListing | null>(null);
  const [expanded, setExpanded] = useState<string | null>(null);

  const box = useRef<HTMLDivElement>(null);
  const menu = useId();

  useEffect(() => {
    if (!open || listing !== null) {
      return;
    }

    api.examples()
       .then((result) => {
         setListing(result);
         // one kind open to start with, so the menu shows what it is for
         setExpanded(result.groups[0]?.id ?? null);
       })
       .catch(() => undefined);
  }, [open, listing]);

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

  function go(id: string) {
    setOpen(false);
    navigate(`/examples/${id}`);
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
      >
        Examples
        <IconChevronDown className={`h-3.5 w-3.5 transition-transform ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <div
          id={menu}
          role="menu"
          className="absolute right-0 z-40 mt-1 w-80 border border-slate-200 bg-white p-1 shadow-lg dark:border-ink-800 dark:bg-ink-900"
        >
          {listing === null ? (
            <p className="px-3 py-3 text-sm text-slate-500">Reading the examples…</p>
          ) : (
            listing.groups.map((group) => {
              const isOpen = expanded === group.id;

              return (
                <div key={group.id}>
                  <button
                    type="button"
                    onClick={() => setExpanded(isOpen ? null : group.id)}
                    aria-expanded={isOpen}
                    className="flex w-full items-center justify-between px-3 py-2 text-left text-sm font-medium hover:bg-slate-50 dark:hover:bg-ink-850"
                  >
                    {group.name}
                    <IconChevronDown
                      className={`h-3.5 w-3.5 text-slate-400 transition-transform ${isOpen ? 'rotate-180' : ''}`}
                    />
                  </button>

                  {isOpen && (
                    <ul className="border-l-2 border-slate-200 pl-1 dark:border-ink-800">
                      {group.examples.map((example) => (
                        <li key={example.id}>
                          <button
                            type="button"
                            role="menuitem"
                            onClick={() => go(example.id)}
                            className="block w-full px-3 py-2 text-left hover:bg-slate-50 dark:hover:bg-ink-850"
                          >
                            <span className="flex items-center gap-2 text-sm">
                              {example.name}
                              {!example.live && (
                                <span className="text-[11px] text-slate-400">starting…</span>
                              )}
                            </span>
                            <span className="mt-0.5 block text-xs leading-snug text-slate-500">
                              {example.description}
                            </span>
                          </button>
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
              );
            })
          )}
        </div>
      )}
    </div>
  );
}
