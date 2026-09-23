import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { useAdminToken } from '../admin';
import { ApiError, api, type AdminListing, type LambdaOverview } from '../api';
import { Dialog } from '../components/Dialog';
import { Locked } from '../components/Locked';
import { IconSpinner, IconTrash } from '../components/Icons';
import { useToast } from '../components/Toast';

/**
 * Every lambda on the installation.
 *
 * The token is the one the header asks for, so unlocking there opens this and
 * the server figures together.
 */
export function Admin() {
  const toast = useToast();

  const [token, setToken] = useAdminToken();
  const [listing, setListing] = useState<AdminListing | null>(null);
  const [search, setSearch] = useState('');
  const [typed, setTyped] = useState('');
  const [page, setPage] = useState(1);
  const [denied, setDenied] = useState(false);
  const [busy, setBusy] = useState<string | null>(null);
  const [viewing, setViewing] = useState<{ key: string; code: string } | null>(null);
  const [removing, setRemoving] = useState<LambdaOverview | null>(null);

  const load = useCallback(async () => {
    if (token === '') {
      return;
    }

    try {
      setListing(await api.admin.list(token, search, page));
      setDenied(false);
    } catch (error) {
      // a wrong token and a missing panel answer the same way on purpose
      if (error instanceof ApiError && error.status === 404) {
        setDenied(true);
        setListing(null);
      } else {
        toast('The panel could not be read.', 'error');
      }
    }
  }, [token, search, page, toast]);

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

  function forget() {
    setToken('');
    setListing(null);
    setDenied(false);
  }

  async function view(lambda: LambdaOverview) {
    setBusy(lambda.publicKey);

    try {
      if (lambda.latestVersion === undefined) {
        toast(`"${lambda.publicKey}" has no code saved.`, 'error');
        return;
      }

      const content = await api.admin.version(token, lambda.publicKey, lambda.latestVersion);
      setViewing({ key: lambda.publicKey, code: content.files[0]?.code ?? '' });
    } catch {
      toast(`The code of "${lambda.publicKey}" could not be read.`, 'error');
    } finally {
      setBusy(null);
    }
  }

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

  if (token === '' || denied) {
    return (
      <Locked
        title="Lambdas"
        denied={denied}
        what="This panel reads and removes lambdas that belong to other people, so it asks for the token first."
      />
    );
  }

  return (
    <div className="mx-auto w-full max-w-5xl px-5 py-10">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Lambdas</h1>
          <p className="mt-1.5 text-sm text-slate-600 dark:text-slate-400">
            {listing === null
              ? 'Reading…'
              : `${listing.total} on this server, ${listing.deployed} online.`}
          </p>
        </div>

        <button type="button" onClick={forget} className="btn-ghost">
          Lock
        </button>
      </div>

      <input
        type="search"
        value={typed}
        onChange={(event) => setTyped(event.target.value)}
        placeholder="Search by key"
        aria-label="Search by key"
        className="field mt-6 font-mono"
      />

      {listing === null ? (
        <div className="mt-8 flex items-center gap-2 text-sm text-slate-500">
          <IconSpinner /> Reading the server…
        </div>
      ) : listing.lambdas.length === 0 ? (
        <p className="surface mt-6 px-5 py-10 text-center text-sm text-slate-500">
          {search === '' ? 'There are no lambdas on this server.' : `Nothing here matches "${search}".`}
        </p>
      ) : (
        <div className="surface mt-6 overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-slate-500">
              <tr className="border-b border-slate-200 dark:border-ink-800">
                <th className="px-4 py-2 font-medium">Key</th>
                <th className="px-4 py-2 font-medium">State</th>
                <th className="px-4 py-2 font-medium">Created</th>
                <th className="px-4 py-2 text-right font-medium" title="Since the server came up">Requests</th>
                <th className="px-4 py-2 text-right font-medium" title="Since the server came up">Errors</th>
                <th className="px-4 py-2 font-medium">Last seen</th>
                <th className="px-4 py-2 text-right font-medium">Versions</th>
                <th className="px-4 py-2 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {listing.lambdas.map((lambda) => {
                const live = lambda.activeVersion != null;

                return (
                  <tr key={lambda.publicKey} className="border-b border-slate-200 last:border-0 dark:border-ink-800">
                    <td className="px-4 py-2">
                      <a
                        href={`/lambda/${lambda.publicKey}/`}
                        target="_blank"
                        rel="noreferrer"
                        className="font-mono text-accent-500 hover:underline dark:text-accent-400"
                      >
                        {lambda.publicKey}
                      </a>
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
                    <td className="px-4 py-2 text-slate-500">{new Date(lambda.created).toLocaleString()}</td>
                    <td className="px-4 py-2 text-right tabular-nums text-slate-500">
                      {lambda.requests.toLocaleString()}
                    </td>
                    <td
                      className={`px-4 py-2 text-right tabular-nums ${
                        lambda.failed > 0 ? 'text-red-500' : 'text-slate-500'
                      }`}
                    >
                      {lambda.failed.toLocaleString()}
                    </td>
                    <td className="px-4 py-2 text-slate-500">
                      {lambda.lastSeen ? new Date(lambda.lastSeen).toLocaleTimeString() : '—'}
                    </td>
                    <td className="px-4 py-2 text-right tabular-nums text-slate-500">{lambda.versions}</td>
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

                        <button type="button" onClick={() => view(lambda)} disabled={busy !== null} className="btn-ghost !px-2 !py-1 text-xs">
                          Code
                        </button>

                        <Link
                          to={`/logs?lambda=${encodeURIComponent(lambda.publicKey)}`}
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

      {viewing !== null && (
        <div
          className="fixed inset-0 z-40 flex items-center justify-center bg-black/40 p-4"
          onClick={() => setViewing(null)}
          role="presentation"
        >
          <div
            className="surface flex max-h-[80vh] w-full max-w-3xl flex-col shadow-xl"
            onClick={(event) => event.stopPropagation()}
            role="dialog"
            aria-modal="true"
            aria-label={`Code of ${viewing.key}`}
          >
            <div className="flex items-center justify-between border-b border-slate-200 px-5 py-3 dark:border-ink-800">
              <h2 className="font-mono text-sm">{viewing.key}</h2>
              <button type="button" onClick={() => setViewing(null)} className="btn-ghost !px-2 !py-1 text-xs">
                Close
              </button>
            </div>
            <pre className="min-h-0 flex-1 overflow-auto px-5 py-4 font-mono text-[12.5px] leading-relaxed">
              {viewing.code}
            </pre>
          </div>
        </div>
      )}

      <Dialog
        open={removing !== null}
        title={`Remove "${removing?.publicKey}"?`}
        onClose={() => setRemoving(null)}
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          Its code, its versions and its files go with it. Whoever holds the editor link will find nothing there,
          and there is no undo.
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
    </div>
  );
}
