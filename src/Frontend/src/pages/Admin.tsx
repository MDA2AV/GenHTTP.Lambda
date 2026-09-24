import { useCallback, useEffect, useRef, useState } from 'react';
import { Link, useParams } from 'react-router-dom';

import { useAdminToken } from '../admin';
import { ApiError, api } from '../api';
import { IconLock, IconSpinner } from '../components/Icons';
import type { Access } from '../console/context';
import { LambdasSection } from '../console/LambdasSection';
import { LogSection } from '../console/LogSection';
import { ServerSection } from '../console/ServerSection';
import { usePageMeta } from '../meta';
import type { Theme } from '../theme';

type SectionId = 'server' | 'lambdas' | 'log';

const SECTIONS: { id: SectionId; title: string; note: string }[] = [
  { id: 'server', title: 'Server', note: 'Memory, connections and what the engine is doing' },
  { id: 'lambdas', title: 'Lambdas', note: 'Every lambda on the installation' },
  { id: 'log', title: 'Log', note: 'What the server and the lambdas are printing, live' },
];

/**
 * Everything an operator can see, laid out the way the editor is: the
 * sections down a sidebar, one of them beside it.
 *
 * Nothing is shown before the token is. Every section asks the server for it
 * as well, so this is a door rather than a disguise - hiding the sections
 * while the API answered anyone who asked would be worth nothing.
 */
export function Admin({ theme }: { theme: Theme }) {
  usePageMeta({ title: 'Admin', index: false });

  const { '*': rest = '' } = useParams();

  const segment = rest.split('/')[0];
  const section: SectionId = SECTIONS.find((s) => s.id === segment)?.id ?? 'server';

  const [token, setToken] = useAdminToken();
  const [refused, setRefused] = useState(false);

  // a token that stops working - the host restarted with another one - locks
  // the whole panel again rather than leaving each section to say so
  const deny = useCallback(() => {
    setRefused(true);
    setToken('');
  }, [setToken]);

  if (token === '') {
    return (
      <Unlock
        refused={refused}
        onUnlocked={(entered) => {
          setRefused(false);
          setToken(entered);
        }}
      />
    );
  }

  const access: Access = { token, deny, dark: theme === 'dark' };

  return (
    <div className="min-h-0 flex-1 overflow-y-auto">
      <div className="mx-auto flex w-full max-w-7xl flex-col md:flex-row md:gap-6 md:px-6">
        <aside className="shrink-0 border-b border-slate-200 dark:border-ink-800 md:sticky md:top-0 md:flex md:w-56 md:flex-col md:self-start md:border-b-0">
          <div className="px-4 pb-3 pt-4 md:px-3 md:pt-6">
            <div className="flex items-start gap-2">
              <div className="min-w-0 flex-1">
                <span className="text-[15px] font-semibold">Administration</span>
                <p className="mt-1 text-[13px] text-slate-500">Unlocked for this tab</p>
              </div>

              <button
                type="button"
                onClick={() => setToken('')}
                className="flex h-8 w-8 items-center justify-center rounded-full text-slate-500 hover:bg-slate-100 hover:text-slate-900 dark:hover:bg-ink-850 dark:hover:text-slate-100"
                aria-label="Lock again"
                title="Lock again"
              >
                <IconLock className="h-4 w-4" />
              </button>
            </div>
          </div>

          <nav aria-label="Sections" className="flex gap-1 overflow-x-auto [scrollbar-width:none] px-3 pb-2 md:mt-2 md:flex-col md:gap-0.5 md:overflow-visible md:px-0">
            {SECTIONS.map((item) => {
              const current = section === item.id;

              return (
                <Link
                  key={item.id}
                  to={item.id === 'server' ? '/admin' : `/admin/${item.id}`}
                  title={item.note}
                  aria-current={current ? 'page' : undefined}
                  className={`flex shrink-0 items-center gap-2 whitespace-nowrap border-b-2 px-3 py-2 text-sm md:border-b-0 md:border-l-2 ${
                    current
                      ? 'border-accent-500 font-medium text-ink-900 dark:border-accent-400 dark:text-slate-100 md:bg-slate-100 md:dark:bg-ink-850'
                      : 'border-transparent text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-200'
                  }`}
                >
                  {item.title}
                </Link>
              );
            })}
          </nav>
        </aside>

        <main className="flex min-w-0 flex-1 flex-col">
          {section === 'lambdas' ? (
            <LambdasSection access={access} />
          ) : section === 'log' ? (
            <LogSection access={access} />
          ) : (
            <ServerSection access={access} />
          )}
        </main>
      </div>
    </div>
  );
}

/**
 * The door. The token is tried against the server before it is kept, so a
 * typo is answered here rather than by every section failing at once.
 */
function Unlock({ refused, onUnlocked }: { refused: boolean; onUnlocked: (token: string) => void }) {
  const [entered, setEntered] = useState('');
  const [working, setWorking] = useState(false);
  const [problem, setProblem] = useState<string | null>(
    refused ? 'That token is no longer accepted. Enter it again.' : null,
  );

  const field = useRef<HTMLInputElement>(null);

  useEffect(() => {
    field.current?.focus();
  }, []);

  async function submit(event: React.FormEvent) {
    event.preventDefault();

    const token = entered.trim();

    if (token === '') {
      return;
    }

    setWorking(true);
    setProblem(null);

    try {
      await api.admin.list(token, '', 1);
      onUnlocked(token);
    } catch (error) {
      // a wrong token and a missing panel answer the same way on purpose
      setProblem(
        error instanceof ApiError && error.status === 404
          ? 'That token was not accepted, or this installation has no administration.'
          : 'The server could not be asked. Try again in a moment.',
      );
    } finally {
      setWorking(false);
    }
  }

  return (
    <div className="flex min-h-0 flex-1 overflow-y-auto">
      <form onSubmit={submit} className="m-auto w-full max-w-sm px-5 py-16">
        <div className="flex h-11 w-11 items-center justify-center bg-slate-100 text-slate-500 dark:bg-ink-850">
          <IconLock className="h-5 w-5" />
        </div>

        <h1 className="mt-5 text-2xl font-bold tracking-tight">Administration</h1>

        <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
          The server figures, the log and the lambdas of other people are behind this. It is the token the host
          was started with, and it is kept until this tab is closed.
        </p>

        <input
          ref={field}
          type="password"
          value={entered}
          onChange={(event) => setEntered(event.target.value)}
          placeholder="Admin token"
          aria-label="Admin token"
          autoComplete="off"
          className="field mt-6 font-mono text-sm"
        />

        {problem && <p className="mt-2 text-xs text-red-500">{problem}</p>}

        <button type="submit" disabled={working || entered.trim() === ''} className="btn-primary mt-4 w-full py-2">
          {working && <IconSpinner />}
          Unlock
        </button>
      </form>
    </div>
  );
}
