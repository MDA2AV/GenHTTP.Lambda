import { useEffect, useState, type ReactNode } from 'react';
import { useLocation, useSearchParams } from 'react-router-dom';

import type { Platform } from '../api';
import { Footer } from '../components/Footer';
import { IconClose, IconMenu, IconPlus } from '../components/Icons';
import { useT } from '../i18n';
import { askPlatform } from '../site';
import { AppChat } from './AppChat';
import { NewApp } from './NewApp';
import { Sidebar } from './Sidebar';
import { sorted, useWorkspace, type KnownApp } from './store';

/**
 * /build: a workspace for having apps built and talking to the agent about
 * them, for people who do not write code - in the shape every chat has: the
 * conversations on the left, the one open in the middle, the box at its foot.
 *
 * A conversation is about one app, which this browser knows by its editor
 * key (see store). A new one starts with the box in the middle of the page,
 * under what the site says about building a website; once built, the app
 * joins the list and its conversation goes on - a question, a setting, a
 * change, each a message.
 *
 * Which app is open is in the address by its public key (?app=…), never its
 * editor key: the address is what a browser shares, keeps in its history and
 * sends along as a referrer.
 *
 * The page is prerendered without any of it - nothing is known of a browser
 * then - so a search engine reads what the site says about building, and the
 * browser draws its own apps right after.
 */
export function Workspace({ intro, after }: { intro: ReactNode; after?: ReactNode }) {
  const said = useT().workspace;
  const kept = useWorkspace();
  const apps = sorted(kept.apps);

  const [params, setParams] = useSearchParams();
  const wanted = params.get('app');
  const selected = (wanted && apps.find((app) => app.publicKey === wanted)) || null;

  const [drawer, setDrawer] = useState(false);
  const [working, setWorking] = useState<Set<string>>(new Set());
  const [left, setLeft] = useState<{ left: number; perDay: number } | null>(null);

  // whether the box has an agent behind it; until the server says, it is
  // offered, so the page is rendered with it for whoever reads it unscripted
  const [build, setBuild] = useState<Platform['build'] | null>(null);

  useEffect(() => {
    askPlatform()
      .then((platform) => setBuild(platform.build))
      .catch(() => setBuild({ available: false, perDay: 0, offer: { models: [], model: '', efforts: [], effort: '' } }));
  }, []);

  // choosing where to go is the end of the list on a phone
  const { search } = useLocation();
  useEffect(() => setDrawer(false), [search]);

  const open = (app: KnownApp) => setParams({ app: app.publicKey });
  const start = () => setParams({});

  const markWorking = (privateKey: string, busy: boolean) =>
    setWorking((was) => {
      if (was.has(privateKey) === busy) return was;

      const next = new Set(was);
      if (busy) next.add(privateKey);
      else next.delete(privateKey);

      return next;
    });

  const list = (
    <Sidebar apps={apps} selected={selected?.privateKey ?? null} working={working} onSelect={open} onNew={start} />
  );

  return (
    <div className="flex min-h-0 flex-1">
      {/* the list is there once there is something in it: a first visit is the page about building, nothing else */}
      {apps.length > 0 && (
        <aside className="hidden w-72 shrink-0 border-r border-slate-200 bg-slate-50/70 dark:border-ink-800 dark:bg-ink-900/40 lg:block">
          {list}
        </aside>
      )}

      {drawer && (
        <div className="fixed inset-0 z-40 lg:hidden" role="dialog" aria-modal="true" aria-label={said.yourApps}>
          <div aria-hidden="true" onClick={() => setDrawer(false)} className="absolute inset-0 bg-slate-900/30 dark:bg-black/60" />
          <div className="absolute inset-y-0 left-0 flex w-[min(20rem,85vw)] flex-col bg-white shadow-xl dark:bg-ink-950">
            <div className="flex shrink-0 justify-end px-2 pt-2">
              <button type="button" onClick={() => setDrawer(false)} aria-label={said.closeApps} className="btn-ghost !px-2 !py-1.5">
                <IconClose className="h-4 w-4" />
              </button>
            </div>
            <div className="min-h-0 flex-1">{list}</div>
          </div>
        </div>
      )}

      <section className="flex min-w-0 flex-1 flex-col">
        {apps.length > 0 && (
          <div className="flex shrink-0 items-center justify-between gap-2 border-b border-slate-200 px-3 py-1.5 dark:border-ink-800 lg:hidden">
            <button type="button" onClick={() => setDrawer(true)} className="btn-ghost !px-2.5 !py-1.5 text-[13px]">
              <IconMenu className="h-4 w-4" />
              {said.showApps}
              <span className="tabular-nums text-slate-400">{apps.length}</span>
            </button>
            {selected && (
              <button type="button" onClick={start} className="btn-ghost !px-2.5 !py-1.5 text-[13px]">
                <IconPlus className="h-4 w-4" />
                {said.newApp}
              </button>
            )}
          </div>
        )}

        {selected ? (
          <AppChat
            key={selected.privateKey}
            app={selected}
            onLeft={(left, perDay) => setLeft({ left, perDay })}
            onWorking={(busy) => markWorking(selected.privateKey, busy)}
            onForgotten={start}
          />
        ) : (
          <NewApp
            intro={intro}
            after={
              <>
                {after}
                {/* the page is fixed to the window, so the foot of every page is part of what scrolls */}
                <div className="mt-16">
                  <Footer />
                </div>
              </>
            }
            offered={build?.available ?? true}
            offer={build?.offer ?? null}
            onBuilt={open}
            allowance={left}
            onLeft={(left) => setLeft((was) => ({ left, perDay: was?.perDay ?? build?.perDay ?? left }))}
          />
        )}
      </section>
    </div>
  );
}
