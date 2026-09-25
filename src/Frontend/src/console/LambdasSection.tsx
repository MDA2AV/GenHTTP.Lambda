import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError, TIERS, api, type AdminListing, type LambdaOverview } from '../api';
import { Dialog } from '../components/Dialog';
import { IconSpinner, IconTrash } from '../components/Icons';
import { useToast } from '../components/Toast';
import { Ago, Empty, Pills, Section, TierBadge } from '../control/ui';
import type { Access } from './context';

/**
 * Every lambda on the installation, to find the one that needs looking at.
 *
 * A row says enough to decide - whether it is up, how busy it is, whether it
 * is failing, which tier it is in and where it answers - and opens the page
 * of the lambda, where everything else about it is. The two things done in a
 * hurry, taking something offline and removing it, stay on the row.
 */
export function LambdasSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [listing, setListing] = useState<AdminListing | null>(null);
  const [search, setSearch] = useState('');
  const [typed, setTyped] = useState('');
  const [tier, setTier] = useState('');
  const [page, setPage] = useState(1);
  const [busy, setBusy] = useState<string | null>(null);
  const [removing, setRemoving] = useState<LambdaOverview | null>(null);

  const load = useCallback(async () => {
    try {
      setListing(await api.admin.list(token, search, page, tier));
    } catch (error) {
      // a wrong token and a missing panel answer the same way on purpose
      if (error instanceof ApiError && error.status === 404) {
        deny();
      } else {
        toast('The lambdas could not be read.', 'error');
      }
    }
  }, [token, search, page, tier, toast, deny]);

  useEffect(() => {
    load();
  }, [load]);

  // a search that asked the server on every keystroke would ask it five times
  // for a key somebody pasted in one go
  useEffect(() => {
    const timer = setTimeout(() => {
      setSearch(typed.trim());
      setPage(1);
    }, 250);

    return () => clearTimeout(timer);
  }, [typed]);

  async function undeploy(lambda: LambdaOverview) {
    setBusy(lambda.publicKey);

    try {
      await api.admin.undeploy(token, lambda.publicKey);
      toast(`"${lambda.publicKey}" is offline.`, 'success');
      await load();
    } catch {
      toast(`"${lambda.publicKey}" could not be taken offline.`, 'error');
    } finally {
      setBusy(null);
    }
  }

  async function remove(lambda: LambdaOverview) {
    setBusy(lambda.publicKey);
    setRemoving(null);

    try {
      await api.admin.remove(token, lambda.publicKey);
      toast(`"${lambda.publicKey}" was removed.`, 'success');
      await load();
    } catch {
      toast(`"${lambda.publicKey}" could not be removed.`, 'error');
    } finally {
      setBusy(null);
    }
  }

  return (
    <Section
      title="Lambdas"
      hint="Every lambda on this server, whoever made it. Open one to see all of it and to change its tier or domain. The request figures count since the server came up."
      actions={
        <input
          type="search"
          value={typed}
          onChange={(event) => setTyped(event.target.value)}
          placeholder="Search by key or domain"
          aria-label="Search by key or domain"
          className="field w-64 py-1.5 font-mono text-sm"
        />
      }
      pills={
        <Pills
          label="Tier"
          value={tier}
          onChange={(value) => {
            setTier(value);
            setPage(1);
          }}
          options={[{ value: '', label: 'All tiers' }, ...TIERS.map((t) => ({ value: t as string, label: t }))]}
        />
      }
    >
      {listing !== null && (
        <p className="mb-3 text-[13px] text-slate-500">
          {listing.total.toLocaleString()} on this server, {listing.deployed.toLocaleString()} online.
        </p>
      )}

      {listing === null ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <IconSpinner /> Reading the server…
        </div>
      ) : listing.lambdas.length === 0 ? (
        <Empty>
          {search === '' && tier === '' ? 'There are no lambdas on this server.' : 'Nothing here matches.'}
        </Empty>
      ) : (
        <div className="surface overflow-x-auto">
          <table className="w-full whitespace-nowrap text-left text-sm">
            <thead className="text-xs text-slate-500">
              <tr className="border-b border-slate-200 dark:border-ink-800">
                <th className="px-4 py-2 font-medium">Key</th>
                <th className="px-4 py-2 font-medium">State</th>
                <th className="px-4 py-2 font-medium">Tier</th>
                <th className="px-4 py-2 font-medium">Domain</th>
                <th className="px-4 py-2 text-right font-medium" title="Since the server came up">Requests</th>
                <th className="px-4 py-2 text-right font-medium" title="Since the server came up">Errors</th>
                <th className="px-4 py-2 font-medium">Last seen</th>
                <th className="px-4 py-2 font-medium">Created</th>
                <th className="px-4 py-2 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {listing.lambdas.map((lambda) => {
                const live = lambda.activeVersion != null;

                return (
                  <tr key={lambda.publicKey} className="border-b border-slate-200 last:border-0 hover:bg-slate-50 dark:border-ink-800 dark:hover:bg-ink-850/50">
                    <td className="px-4 py-2">
                      <Link
                        to={`/admin/lambdas/${encodeURIComponent(lambda.publicKey)}`}
                        className="font-mono text-accent-500 hover:underline dark:text-accent-400"
                        title="Open this lambda"
                      >
                        {lambda.publicKey}
                      </Link>
                    </td>
                    <td className="px-4 py-2">
                      <span
                        className={`chip ${
                          live
                            ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
                            : 'bg-slate-400/10 text-slate-500'
                        }`}
                      >
                        <span className={`h-1.5 w-1.5 ${live ? 'bg-emerald-500' : 'bg-slate-400'}`} />
                        {live ? `live · v${lambda.activeVersion}` : 'offline'}
                      </span>
                    </td>
                    <td className="px-4 py-2"><TierBadge tier={lambda.tier} /></td>
                    <td className="max-w-[14rem] truncate px-4 py-2 font-mono text-[13px]">
                      {lambda.domain ? (
                        <span
                          className={lambda.domainServed ? '' : 'text-slate-400 line-through'}
                          title={lambda.domainServed ? `Answers at ${lambda.domain}` : 'Configured, but not served outside the premium tier'}
                        >
                          {lambda.domain}
                        </span>
                      ) : (
                        <span className="text-slate-400">—</span>
                      )}
                    </td>
                    <td className="px-4 py-2 text-right tabular-nums text-slate-500">
                      {lambda.requests.toLocaleString()}
                    </td>
                    <td className={`px-4 py-2 text-right tabular-nums ${lambda.failed > 0 ? 'text-red-500' : 'text-slate-500'}`}>
                      {lambda.failed.toLocaleString()}
                    </td>
                    <td className="px-4 py-2 text-slate-500">
                      {lambda.lastSeen ? <Ago at={lambda.lastSeen} /> : '—'}
                    </td>
                    <td className="px-4 py-2 text-slate-500"><Ago at={lambda.created} /></td>
                    <td className="px-4 py-2">
                      <div className="flex items-center justify-end gap-1.5">
                        {busy === lambda.publicKey && <IconSpinner className="h-4 w-4 text-slate-400" />}

                        <a
                          href={`/editor/${lambda.privateKey}`}
                          target="_blank"
                          rel="noreferrer"
                          className="btn-ghost !px-2 !py-1 text-xs"
                          title="Open the editor, where the code can be changed as well as read"
                        >
                          Editor
                        </a>

                        <Link
                          to={`/admin/log?lambda=${encodeURIComponent(lambda.publicKey)}`}
                          className="btn-ghost !px-2 !py-1 text-xs"
                          title="What this lambda has printed, and what the server said about it"
                        >
                          Log
                        </Link>

                        {live && (
                          <button type="button" onClick={() => undeploy(lambda)} disabled={busy !== null} className="btn-ghost !px-2 !py-1 text-xs">
                            Undeploy
                          </button>
                        )}

                        <button
                          type="button"
                          onClick={() => setRemoving(lambda)}
                          disabled={busy !== null}
                          className="btn-danger !px-2 !py-1"
                          aria-label={`Delete ${lambda.publicKey}`}
                        >
                          <IconTrash className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {listing !== null && listing.pages > 1 && (
        <div className="mt-4 flex items-center justify-between text-sm">
          <button
            type="button"
            onClick={() => setPage((at) => Math.max(1, at - 1))}
            disabled={listing.page <= 1}
            className="btn-ghost"
          >
            Previous
          </button>

          <span className="text-slate-500">
            Page {listing.page} of {listing.pages} · {listing.matched.toLocaleString()} lambda
            {listing.matched === 1 ? '' : 's'}
          </span>

          <button
            type="button"
            onClick={() => setPage((at) => Math.min(listing.pages, at + 1))}
            disabled={listing.page >= listing.pages}
            className="btn-ghost"
          >
            Next
          </button>
        </div>
      )}

      <Dialog
        open={removing !== null}
        title={`Remove "${removing?.publicKey}"?`}
        onClose={() => setRemoving(null)}
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          Its code, its versions, its files{removing?.domain ? ' and its domain' : ''} go with it. Whoever holds the
          editor link will find nothing there, and there is no undo.
        </p>
        <div className="mt-5 flex justify-end gap-2">
          <button type="button" onClick={() => setRemoving(null)} className="btn-ghost">
            Keep it
          </button>
          <button type="button" onClick={() => removing && remove(removing)} className="btn-danger">
            Remove
          </button>
        </div>
      </Dialog>
    </Section>
  );
}
