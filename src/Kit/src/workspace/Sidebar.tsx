import { useState } from 'react';

import { api, ApiError } from '../api';
import { IconPlus, IconSpinner } from '../components/Icons';
import { useT } from '../i18n';
import { keyIn, remember, type KnownApp } from './store';

/**
 * The apps this browser knows, the most recently opened first, under the
 * button that starts on a new one - and at the foot, a way to add one that
 * was built elsewhere: with its editor link, or the key alone.
 *
 * Nothing about an app is asked of the server to list it: what it is and
 * whether it was online are what was read the last time it was opened, so
 * the list costs nothing however long it grows.
 */
export function Sidebar({
  apps,
  selected,
  working,
  onSelect,
  onNew,
}: {
  apps: KnownApp[];
  /** The editor key of the app open now; none on a new one. */
  selected: string | null;
  /** The editor keys of the apps the agent is working on. */
  working: Set<string>;
  onSelect: (app: KnownApp) => void;
  onNew: () => void;
}) {
  const said = useT().workspace;

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="shrink-0 p-3">
        <button
          type="button"
          onClick={onNew}
          aria-current={selected === null ? 'page' : undefined}
          className={`flex w-full items-center gap-2 rounded-lg border px-3 py-2 text-sm font-medium transition-colors ${
            selected === null
              ? 'border-accent-500/50 bg-accent-500/10 text-accent-700 dark:border-accent-400/40 dark:text-accent-400'
              : 'border-slate-200 hover:bg-slate-100 dark:border-ink-800 dark:hover:bg-ink-850'
          }`}
        >
          <IconPlus className="h-4 w-4" />
          {said.newApp}
        </button>
      </div>

      <nav aria-label={said.yourApps} className="min-h-0 flex-1 overflow-y-auto px-3 pb-3">
        <h2 className="px-2 pb-1.5 pt-2 text-[11px] font-semibold uppercase tracking-wider text-slate-500">{said.yourApps}</h2>

        <ul className="space-y-0.5">
          {apps.map((app) => {
            const active = app.privateKey === selected;

            return (
              <li key={app.privateKey}>
                <button
                  type="button"
                  onClick={() => onSelect(app)}
                  aria-current={active ? 'page' : undefined}
                  className={`group flex w-full items-start gap-2.5 rounded-lg px-2 py-2 text-left transition-colors ${
                    active ? 'bg-slate-200/70 dark:bg-ink-800' : 'hover:bg-slate-100 dark:hover:bg-ink-850'
                  }`}
                >
                  <span className="mt-[5px] flex h-2.5 w-2.5 shrink-0 items-center justify-center">
                    {working.has(app.privateKey) ? (
                      <IconSpinner className="h-3 w-3 text-accent-500" aria-label={said.working} />
                    ) : (
                      <span
                        className={`h-2 w-2 rounded-full ${app.online === false ? 'bg-slate-300 dark:bg-ink-700' : 'bg-emerald-500'}`}
                        title={app.online === false ? said.offline : said.online}
                      />
                    )}
                  </span>
                  <span className="min-w-0 flex-1">
                    <span className="block truncate text-sm font-medium">{app.publicKey}</span>
                    {app.about && <span className="mt-0.5 line-clamp-2 text-xs leading-snug text-slate-500">{app.about}</span>}
                  </span>
                </button>
              </li>
            );
          })}
        </ul>
      </nav>

      <div className="shrink-0 border-t border-slate-200 p-3 dark:border-ink-800">
        <AddApp onAdded={onSelect} />
        <p className="mt-3 px-1 text-[11px] leading-snug text-slate-500">{said.keptHere}</p>
      </div>
    </div>
  );
}

/**
 * Adds an app built elsewhere - on another device, or before this browser
 * kept a list - by what opens it: its editor link, or the key alone. Read
 * once to know it is there and what it is; a demo, which nobody changes, is
 * not added.
 */
function AddApp({ onAdded }: { onAdded: (app: KnownApp) => void }) {
  const said = useT().workspace;
  const [open, setOpen] = useState(false);
  const [text, setText] = useState('');
  const [problem, setProblem] = useState<string | null>(null);
  const [adding, setAdding] = useState(false);

  async function add() {
    const key = keyIn(text);

    if (!key) {
      setProblem(said.addNotKey);
      return;
    }

    setAdding(true);
    setProblem(null);

    try {
      const summary = await api.summary(key);
      const lambda = summary.lambda;

      if (lambda.tier === 'Demo') {
        setProblem(said.addDemo);
        return;
      }

      const app = {
        privateKey: lambda.privateKey,
        publicKey: lambda.publicKey,
        about: summary.documentation.about ?? null,
        online: lambda.deployedAt != null && !lambda.blocked,
      };

      remember(app);
      setText('');
      setOpen(false);

      onAdded({ ...app, added: Date.now(), opened: Date.now(), turns: [] });
    } catch (error) {
      setProblem(error instanceof ApiError && (error.status === 404 || error.status === 400) ? said.addUnknown : said.addFailed);
    } finally {
      setAdding(false);
    }
  }

  if (!open) {
    return (
      <button
        type="button"
        onClick={() => setOpen(true)}
        className="flex w-full items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-slate-600 hover:bg-slate-100 hover:text-slate-900 dark:text-slate-400 dark:hover:bg-ink-850 dark:hover:text-slate-100"
      >
        <IconPlus className="h-3.5 w-3.5" />
        {said.add}
      </button>
    );
  }

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        void add();
      }}
      className="space-y-2"
    >
      <label htmlFor="add-app" className="block px-1 text-xs font-medium text-slate-600 dark:text-slate-400">
        {said.addLabel}
      </label>
      <input
        id="add-app"
        type="text"
        value={text}
        onChange={(event) => setText(event.target.value)}
        autoComplete="off"
        spellCheck={false}
        autoFocus
        className="field w-full font-mono text-xs"
      />
      {problem && <p className="px-1 text-xs text-red-600 dark:text-red-400">{problem}</p>}
      <div className="flex gap-2">
        <button type="submit" disabled={adding || text.trim() === ''} className="btn-primary !px-3 !py-1 text-[13px]">
          {adding && <IconSpinner className="h-3.5 w-3.5" />}
          {said.addButton}
        </button>
        <button
          type="button"
          onClick={() => {
            setOpen(false);
            setProblem(null);
          }}
          className="btn-ghost !px-3 !py-1 text-[13px]"
        >
          {said.cancel}
        </button>
      </div>
    </form>
  );
}
