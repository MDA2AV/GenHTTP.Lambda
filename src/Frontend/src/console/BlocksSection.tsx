import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError, api, refusedToken, type AddressBlock, type AddressBlockListing } from '../api';
import { Dialog } from '../components/Dialog';
import { IconSpinner, IconTrash } from '../components/Icons';
import { useToast } from '../components/Toast';
import { Ago, Empty, Section } from '../control/ui';
import type { Access } from './context';

/**
 * The addresses the server refuses, why, and the way to lift each block.
 *
 * A blocked address is refused before anything else is done for it, on the
 * platform and at every lambda - apart from this panel, so an operator who
 * blocked their own address can lift it here. Which one is theirs is marked,
 * and said at the top when it is blocked.
 */
export function BlocksSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [listing, setListing] = useState<AddressBlockListing | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [busy, setBusy] = useState<number | null>(null);
  const [lifting, setLifting] = useState<AddressBlock | null>(null);

  const load = useCallback(async () => {
    try {
      setListing(await api.admin.blocks(token));
      setFailure(null);
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        setFailure('The blocks could not be read.');
      }
    }
  }, [token, deny]);

  useEffect(() => {
    load();

    // what each block refused goes on being counted while the page is open
    const timer = window.setInterval(() => document.visibilityState === 'visible' && load(), 15_000);

    return () => window.clearInterval(timer);
  }, [load]);

  async function lift(block: AddressBlock) {
    setLifting(null);
    setBusy(block.id);

    try {
      await api.admin.removeBlock(token, block.id);
      toast(`${block.address} is no longer blocked.`, 'success');
      await load();
    } catch (error) {
      if (refusedToken(error) && !(error as ApiError).kind) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : 'The block could not be lifted.', 'error');
        await load();
      }
    } finally {
      setBusy(null);
    }
  }

  const blocks = listing?.blocks ?? [];
  const you = listing?.you ?? null;
  const yours = blocks.find((block) => block.address === you);

  return (
    <Section
      title="Blocked addresses"
      hint="Refused before anything else, on the platform and at every lambda, for two weeks each. This panel stays reachable from a blocked address. The counts are kept in memory since the server came up."
    >
      {yours && (
        <div className="surface mb-5 flex flex-wrap items-center gap-3 border-amber-500/40 bg-amber-500/10 p-4 text-sm">
          <p className="min-w-0 flex-1">
            <span className="font-medium">Your own address is blocked.</span>{' '}
            <span className="text-slate-600 dark:text-slate-400">
              This panel still answers you, nothing else on this server does until you lift the block.
            </span>
          </p>
          <button type="button" onClick={() => lift(yours)} disabled={busy !== null} className="btn-primary !px-3 !py-1.5 text-[13px]">
            {busy === yours.id && <IconSpinner />}
            Lift it
          </button>
        </div>
      )}

      {listing === null ? (
        failure ? (
          <p className="text-sm text-slate-500">{failure}</p>
        ) : (
          <div className="flex items-center gap-2 text-sm text-slate-500"><IconSpinner /> Reading the blocks…</div>
        )
      ) : blocks.length === 0 ? (
        <Empty>No address is blocked. Addresses are blocked with a lambda, from its page.</Empty>
      ) : (
        <ul className="surface divide-y divide-slate-100 dark:divide-ink-850">
          {blocks.map((block) => (
            <li key={block.id} className="flex flex-wrap items-start gap-x-6 gap-y-2 px-4 py-3 text-[13px]">
              <div className="min-w-[12rem] flex-1">
                <p className="flex flex-wrap items-center gap-1.5">
                  <span className="break-all font-mono text-sm">{block.address}</span>
                  {block.address === you && (
                    <span className="rounded-full bg-amber-500/15 px-2 py-0.5 text-[11px] font-medium text-amber-700 dark:text-amber-400">
                      your address
                    </span>
                  )}
                </p>
                <p className="mt-0.5 text-slate-600 dark:text-slate-400">
                  {block.reason ?? <span className="text-slate-400">No reason given</span>}
                  {block.lambda && (
                    <>
                      {' · '}
                      <Link to={`/admin/lambdas/${encodeURIComponent(block.lambda)}`} className="font-mono text-accent-500 hover:underline dark:text-accent-400">
                        {block.lambda}
                      </Link>
                    </>
                  )}
                </p>
              </div>

              <dl className="flex flex-wrap gap-x-6 gap-y-1 text-slate-500">
                <div>
                  <dt className="text-[11px] uppercase tracking-wide text-slate-400">Blocked</dt>
                  <dd><Ago at={block.created} /></dd>
                </div>
                <div>
                  <dt className="text-[11px] uppercase tracking-wide text-slate-400">Lifted</dt>
                  <dd><Ago at={block.expires} /></dd>
                </div>
                <div>
                  <dt className="text-[11px] uppercase tracking-wide text-slate-400">Refused</dt>
                  <dd className="tabular-nums" title={block.lastRefused ? `last ${new Date(block.lastRefused).toLocaleString()}` : undefined}>
                    {block.refused.toLocaleString()}
                    {block.lastRefused && <span className="text-slate-400"> · last <Ago at={block.lastRefused} /></span>}
                  </dd>
                </div>
              </dl>

              <button
                type="button"
                onClick={() => setLifting(block)}
                disabled={busy !== null}
                className="btn-ghost ml-auto !px-2 !py-1 text-xs"
                title="Lift this block"
              >
                {busy === block.id ? <IconSpinner className="h-4 w-4" /> : <IconTrash className="h-4 w-4" />}
                Lift
              </button>
            </li>
          ))}
        </ul>
      )}

      <Dialog
        title={`Lift the block of ${lifting?.address}?`}
        open={lifting !== null}
        onClose={() => setLifting(null)}
        footer={
          <>
            <button type="button" onClick={() => setLifting(null)} className="btn-ghost">Keep it</button>
            <button type="button" onClick={() => lifting && lift(lifting)} className="btn-primary">Lift it</button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          Requests from it are answered again from the next one on.
          {lifting?.lambda && <> The lambda it was blocked for, <span className="font-mono">{lifting.lambda}</span>, stays as it is.</>}
        </p>
      </Dialog>
    </Section>
  );
}
