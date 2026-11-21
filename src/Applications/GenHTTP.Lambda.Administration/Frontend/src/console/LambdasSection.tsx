import { useCallback, useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';

import { ApiError, api, refusedToken, type AdminListing, type AdminQuery, type LambdaOverview } from '@genhttp-lambda/kit/api';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconClose, IconExternal, IconSearch, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Ago, Empty, Menu, Section, StatusBadge, TierBadge, menuItem, menuRule } from '@genhttp-lambda/kit/ui/ui';
import { lambdaPage, type Access } from './context';
import { BlockedBadge } from './LambdaDetail';
import { Choices, Reading, Select, count } from './parts';
import { FEATURES, UseMarks, feature } from './uses';

const TIERS = [
  { value: '', label: 'Every tier' },
  { value: 'Sandbox', label: 'Sandbox' },
  { value: 'Production', label: 'Production' },
  { value: 'System', label: 'System' },
  { value: 'Demo', label: 'Demos' },
];

const STATES = [
  { value: '', label: 'Any state' },
  { value: 'online', label: 'Online' },
  { value: 'offline', label: 'Offline' },
  { value: 'blocked', label: 'Blocked' },
  { value: 'expiring', label: 'Expiring this week' },
];

const VIEWS = [
  { value: '', label: 'Any view' },
  { value: 'simple', label: 'Simple view' },
  { value: 'full', label: 'Full view' },
];

const CREATED = [
  { value: '', label: 'Created any time' },
  { value: '1', label: 'Created today' },
  { value: '7', label: 'Created this week' },
  { value: '30', label: 'Created this month' },
];

const SORTS = [
  { value: '', label: 'Newest first' },
  { value: 'oldest', label: 'Oldest first' },
  { value: 'changed', label: 'Changed lately' },
  { value: 'visited', label: 'Visited lately' },
  { value: 'key', label: 'By key' },
];

/** The filters there are, as the address carries them. */
const FILTERS = ['search', 'tier', 'state', 'uses', 'view', 'created', 'sort'] as const;

/**
 * Every lambda on the installation, to find the one that needs looking at.
 *
 * The filters are in the address, so the dashboards link to the lambdas they
 * count and a narrowed list survives a reload. A row says enough to decide -
 * whether it is up, its tier, what it uses, how busy and how broken it is -
 * and opens the page of the lambda. What can be done to one is in the menu
 * at the end of its row, apart from what it shows.
 */
export function LambdasSection({ access }: { access: Access }) {
  const { token, deny, changed } = access;

  const toast = useToast();
  const [params, setParams] = useSearchParams();

  const [listing, setListing] = useState<AdminListing | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [typed, setTyped] = useState(params.get('search') ?? '');
  const [busy, setBusy] = useState<string | null>(null);
  const [asking, setAsking] = useState<{ lambda: LambdaOverview; what: 'offline' | 'remove' } | null>(null);

  const read = (name: (typeof FILTERS)[number]) => params.get(name) ?? '';
  const uses = read('uses') === '' ? [] : read('uses').split(',');
  const page = Math.max(1, Number(params.get('page')) || 1);

  const query: AdminQuery = {
    search: read('search'),
    tier: read('tier'),
    state: read('state'),
    uses,
    view: read('view'),
    created: Number(read('created')) || undefined,
    sort: read('sort'),
    page,
  };

  const key = params.toString();

  const load = useCallback(async () => {
    try {
      setListing(await api.admin.list(token, query));
      setFailure(null);
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        setFailure(error instanceof ApiError ? error.message : 'The lambdas could not be read.');
      }
    }
    // the query is the address, which the key says in full
  }, [token, deny, key]);

  useEffect(() => {
    load();
  }, [load]);

  /** Narrows the list, from its first page. */
  const narrow = useCallback((name: string, value: string) => {
    setParams((current) => {
      const next = new URLSearchParams(current);

      if (value === '') {
        next.delete(name);
      } else {
        next.set(name, value);
      }

      next.delete('page');

      return next;
    }, { replace: true });
  }, [setParams]);

  // the address changed by other means - the sidebar, a link - says what is
  // searched for, and the box follows it rather than putting back its own
  const searched = params.get('search') ?? '';

  useEffect(() => {
    setTyped((now) => (now.trim() === searched ? now : searched));
  }, [searched]);

  // a search that asked the server on every keystroke would ask it five times
  // for a key somebody pasted in one go
  useEffect(() => {
    if (typed.trim() === (params.get('search') ?? '')) {
      return;
    }

    const timer = setTimeout(() => narrow('search', typed.trim()), 250);

    return () => clearTimeout(timer);
  }, [typed, params, narrow]);

  const narrowed = FILTERS.some((name) => name !== 'sort' && read(name) !== '');

  const turn = (to: number) =>
    setParams((current) => {
      const next = new URLSearchParams(current);

      if (to <= 1) {
        next.delete('page');
      } else {
        next.set('page', String(to));
      }

      return next;
    });

  async function act(lambda: LambdaOverview, what: 'offline' | 'remove') {
    setAsking(null);
    setBusy(lambda.publicKey);

    try {
      if (what === 'offline') {
        await api.admin.undeploy(token, lambda.publicKey);
        toast(`"${lambda.publicKey}" is offline.`, 'success');
      } else {
        await api.admin.remove(token, lambda.publicKey);
        toast(`"${lambda.publicKey}" was removed.`, 'success');
      }

      changed();
      await load();
    } catch (error) {
      if (refusedToken(error) && !(error as ApiError).kind) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : `"${lambda.publicKey}" could not be ${what === 'offline' ? 'taken offline' : 'removed'}.`, 'error');
      }
    } finally {
      setBusy(null);
    }
  }

  return (
    <Section
      title="All lambdas"
      hint="Every lambda on this server, whoever made it. Open one to see all of it and to change it. The requests count since the server came up."
    >
      {/* the search and the order, then what narrows the list: each a
          control of its own, so what is chosen is in sight */}
      <div className="flex flex-wrap items-center gap-2">
        <label className="relative min-w-[12rem] flex-1">
          <IconSearch className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            value={typed}
            onChange={(event) => setTyped(event.target.value)}
            placeholder="Search by key or domain"
            aria-label="Search by key or domain"
            className="field py-1.5 pl-9"
          />
        </label>
        <Select label="Order" value={read('sort')} options={SORTS} onChange={(v) => narrow('sort', v)} className="!w-auto" />
      </div>

      <div className="mt-2 flex flex-wrap items-center gap-2" role="group" aria-label="Filters">
        <Select label="Tier" value={read('tier')} options={TIERS} onChange={(v) => narrow('tier', v)} className="!w-auto" />
        <Select label="State" value={read('state')} options={STATES} onChange={(v) => narrow('state', v)} className="!w-auto" />
        <div className="w-44">
          <Choices
            label="Uses"
            options={FEATURES.filter((f) => f.id !== 'workspace').map((f) => ({ value: f.id, label: f.label, hint: f.hint }))}
            chosen={uses}
            onChange={(chosen) => narrow('uses', chosen.join(','))}
          />
        </div>
        <Select label="View" value={read('view')} options={VIEWS} onChange={(v) => narrow('view', v)} className="!w-auto" />
        <Select label="Created" value={read('created')} options={CREATED} onChange={(v) => narrow('created', v)} className="!w-auto" />
      </div>

      {/* what it is narrowed to, each to take away again */}
      <div className="mt-3 flex min-h-7 flex-wrap items-center gap-2 text-[13px] text-slate-500">
        {listing && (
          <span>
            {narrowed ? `${count(listing.matched)} of ${count(listing.total)}` : count(listing.total)} lambda{listing.total === 1 ? '' : 's'}
            {!narrowed && <> · {count(listing.deployed)} online</>}
            {/* the demos are counted apart everywhere in the panel, and listed apart too */}
            {read('tier') !== 'Demo' && (access.overview?.lambdas.demos ?? 0) > 0 && (
              <>
                {' · '}
                <button type="button" onClick={() => narrow('tier', 'Demo')} className="text-accent-600 hover:underline dark:text-accent-400">
                  {count(access.overview!.lambdas.demos)} demos apart
                </button>
              </>
            )}
          </span>
        )}

        {FILTERS.filter((name) => name !== 'search' && name !== 'sort' && read(name) !== '').flatMap((name) =>
          name === 'uses'
            ? uses.map((use) => (
                <Chip key={`uses-${use}`} onClear={() => narrow('uses', uses.filter((u) => u !== use).join(','))}>
                  {`Uses ${feature(use)?.label ?? use}`}
                </Chip>
              ))
            : [
                <Chip key={name} onClear={() => narrow(name, '')}>
                  {[...TIERS, ...STATES, ...VIEWS, ...CREATED].find((o) => o.value === read(name) && o.value !== '')?.label ?? read(name)}
                </Chip>,
              ])}

        {narrowed && (
          <button
            type="button"
            onClick={() => {
              setTyped('');
              setParams(read('sort') ? { sort: read('sort') } : {}, { replace: true });
            }}
            className="text-accent-600 hover:underline dark:text-accent-400"
          >
            Clear all
          </button>
        )}
      </div>

      <div className="mt-3">
        {listing === null ? (
          <Reading what="the lambdas" failure={failure} />
        ) : listing.lambdas.length === 0 ? (
          <Empty>{narrowed ? 'Nothing here matches.' : 'There are no lambdas on this server.'}</Empty>
        ) : (
          <div className="surface">
            {/* a table where there is room for its columns, a list of rows on a phone */}
            <table className="hidden w-full whitespace-nowrap text-left text-sm md:table">
              <thead className="text-xs text-slate-500">
                <tr className="border-b border-grey-300 dark:border-ink-800">
                  <th className="px-4 py-2 font-medium">Lambda</th>
                  <th className="px-3 py-2 font-medium">State</th>
                  <th className="px-3 py-2 font-medium">Tier</th>
                  <th className="px-3 py-2 font-medium">Uses</th>
                  <th className="px-3 py-2 text-right font-medium" title="Since the server came up">Requests</th>
                  <th className="px-3 py-2 text-right font-medium" title="Server errors since the server came up">Errors</th>
                  <th className="px-3 py-2 font-medium" title="As the platform writes it down - at most hourly">Visited</th>
                  <th className="px-3 py-2 font-medium">Created</th>
                  <th className="px-2 py-2"><span className="sr-only">Actions</span></th>
                </tr>
              </thead>
              <tbody>
                {listing.lambdas.map((lambda) => (
                  <tr key={lambda.publicKey} className="border-b border-grey-300 last:border-0 hover:bg-slate-50 dark:border-ink-800 dark:hover:bg-ink-850/50">
                    <td className="max-w-[18rem] px-4 py-2">
                      <Link to={lambdaPage(lambda.publicKey)} className="block truncate font-mono text-accent-600 hover:underline dark:text-accent-400">
                        {lambda.publicKey}
                      </Link>
                      {lambda.domain && (
                        <span
                          className={`block truncate font-mono text-xs ${lambda.domainServed ? 'text-slate-500' : 'text-slate-400 line-through'}`}
                          title={lambda.domainServed ? `Answers at ${lambda.domain}` : 'Configured, but not served in a tier without domains'}
                        >
                          {lambda.domain}
                        </span>
                      )}
                    </td>
                    <td className="px-3 py-2">{lambda.blocked ? <BlockedBadge /> : <StatusBadge version={lambda.activeVersion} />}</td>
                    <td className="px-3 py-2"><TierBadge tier={lambda.tier} label={lambda.tierLabel} /></td>
                    <td className="px-3 py-2"><UseMarks uses={lambda.uses} connectivity={lambda.connectivity} /></td>
                    <td className="px-3 py-2 text-right tabular-nums text-slate-500">{count(lambda.requests)}</td>
                    <td className={`px-3 py-2 text-right tabular-nums ${lambda.failed > 0 ? 'text-red-500' : 'text-slate-500'}`}>{count(lambda.failed)}</td>
                    <td className="px-3 py-2 text-slate-500">{lambda.visited ? <Ago at={lambda.visited} /> : '—'}</td>
                    <td className="px-3 py-2 text-slate-500"><Ago at={lambda.created} /></td>
                    <td className="px-2 py-2 text-right">
                      <RowActions lambda={lambda} busy={busy} onAsk={(what) => setAsking({ lambda, what })} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <ul className="divide-y divide-grey-300 dark:divide-ink-800 md:hidden">
              {listing.lambdas.map((lambda) => (
                <li key={lambda.publicKey} className="flex items-start gap-3 px-4 py-3 text-[13px]">
                  <div className="min-w-0 flex-1">
                    <Link to={lambdaPage(lambda.publicKey)} className="block truncate font-mono text-sm text-accent-600 hover:underline dark:text-accent-400">
                      {lambda.publicKey}
                    </Link>
                    <div className="-ml-2 mt-1.5 flex flex-wrap items-center gap-1.5">
                      {lambda.blocked ? <BlockedBadge /> : <StatusBadge version={lambda.activeVersion} />}
                      <TierBadge tier={lambda.tier} label={lambda.tierLabel} />
                      <span className="ml-1"><UseMarks uses={lambda.uses} connectivity={lambda.connectivity} /></span>
                    </div>
                    <p className="mt-1.5 text-slate-500">
                      {count(lambda.requests)} requests{lambda.failed > 0 && <span className="text-red-500"> · {count(lambda.failed)} errors</span>} · created <Ago at={lambda.created} />
                    </p>
                  </div>
                  <RowActions lambda={lambda} busy={busy} onAsk={(what) => setAsking({ lambda, what })} />
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {listing !== null && listing.pages > 1 && (
        <div className="mt-4 flex items-center justify-between text-sm">
          <button type="button" onClick={() => turn(listing.page - 1)} disabled={listing.page <= 1} className="btn-ghost">
            Previous
          </button>
          <span className="text-slate-500">
            Page {listing.page} of {listing.pages}
          </span>
          <button type="button" onClick={() => turn(listing.page + 1)} disabled={listing.page >= listing.pages} className="btn-ghost">
            Next
          </button>
        </div>
      )}

      <Dialog
        open={asking !== null}
        title={asking?.what === 'remove' ? `Remove "${asking.lambda.publicKey}"?` : `Take "${asking?.lambda.publicKey}" offline?`}
        onClose={() => setAsking(null)}
        footer={
          <>
            <button type="button" onClick={() => setAsking(null)} className="btn-ghost">Keep it</button>
            <button type="button" onClick={() => asking && act(asking.lambda, asking.what)} className="btn-danger">
              {asking?.what === 'remove' ? 'Remove' : 'Take it offline'}
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          {asking?.what === 'remove'
            ? <>Its code, its versions, its data{asking.lambda.domain ? ' and its domain' : ''} go with it. Whoever holds the editor link will find nothing there, and there is no undo.</>
            : 'Its address stops answering. Nothing of it is removed, and its owner can put it online again.'}
        </p>
      </Dialog>
    </Section>
  );
}

/** What can be done to a lambda from the list: in a menu at the end of its row. */
function RowActions({ lambda, busy, onAsk }: { lambda: LambdaOverview; busy: string | null; onAsk: (what: 'offline' | 'remove') => void }) {
  if (busy === lambda.publicKey) {
    return <IconSpinner className="ml-auto h-4 w-4 text-slate-400" />;
  }

  return (
    <Menu label={`Actions for ${lambda.publicKey}`}>
      {(close) => (
        <>
          <Link role="menuitem" to={lambdaPage(lambda.publicKey)} className={menuItem} onClick={close}>Open its page</Link>
          <a role="menuitem" href={`/editor/${lambda.privateKey}`} target="_blank" rel="noreferrer" className={menuItem} onClick={close}>
            Open its editor <IconExternal className="h-3.5 w-3.5 text-slate-400" />
          </a>
          <Link role="menuitem" to={`/admin/log?lambda=${encodeURIComponent(lambda.publicKey)}`} className={menuItem} onClick={close}>Read its log</Link>
          {menuRule}
          {lambda.activeVersion != null && (
            <button type="button" role="menuitem" className={menuItem} disabled={busy !== null} onClick={() => { close(); onAsk('offline'); }}>
              Take offline…
            </button>
          )}
          <button type="button" role="menuitem" className={`${menuItem} text-red-500`} disabled={busy !== null} onClick={() => { close(); onAsk('remove'); }}>
            Remove…
          </button>
        </>
      )}
    </Menu>
  );
}

function Chip({ children, onClear }: { children: string; onClear: () => void }) {
  return (
    <span className="inline-flex items-center gap-1 rounded-full bg-accent-500/10 py-0.5 pl-2.5 pr-1 text-xs font-medium text-accent-700 dark:bg-accent-400/15 dark:text-accent-400">
      {children}
      <button type="button" onClick={onClear} aria-label={`Not “${children}”`} className="rounded-full p-0.5 hover:bg-accent-500/20">
        <IconClose className="h-3 w-3" />
      </button>
    </span>
  );
}
