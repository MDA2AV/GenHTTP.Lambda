import { useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError, api, isDemo, refusedToken, type AdminLambdaDetail, type EditorAddress } from '../api';
import { Dialog } from '../components/Dialog';
import { IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { Ago } from '../control/ui';

/**
 * Blocking a lambda and lifting the block, for the operator - on the lambda's
 * page in the panel and in the Admin section of its editor alike, so it is
 * one piece rather than two that drift apart.
 *
 * Blocking may block the addresses the lambda was changed from as well. All
 * of them are offered ticked but the operator's own, which is marked: an
 * operator who blocks themselves still reaches the panel, but nothing else.
 *
 * In English, like the panel it is a part of.
 */
export function BlockControl({ token, detail, onChanged, deny }: {
  token: string;
  detail: AdminLambdaDetail;
  onChanged: (detail: AdminLambdaDetail) => void;
  /** The server turned the token away. */
  deny: () => void;
}) {
  const toast = useToast();

  const [asking, setAsking] = useState(false);
  const [reason, setReason] = useState('');
  const [chosen, setChosen] = useState<string[]>([]);
  const [busy, setBusy] = useState(false);

  const { lambda, block, editorAddresses, you } = detail;

  const mine = (editor: EditorAddress) => you != null && editor.range === you;

  async function act(action: () => Promise<AdminLambdaDetail>, done: string, failed: string) {
    setBusy(true);

    try {
      onChanged(await action());
      toast(done, 'success');
      setAsking(false);
    } catch (error) {
      // the platform's own refusals say what kind they are; the token's do not
      if (refusedToken(error) && !(error as ApiError).kind) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : failed, 'error');
      }
    } finally {
      setBusy(false);
    }
  }

  const ask = () => {
    setReason('');
    // every address it was changed from, but whoever is asking and what is blocked already
    setChosen(editorAddresses.filter((e) => !e.blocked && !mine(e)).map((e) => e.address));
    setAsking(true);
  };

  const toggle = (address: string) =>
    setChosen((now) => (now.includes(address) ? now.filter((a) => a !== address) : [...now, address]));

  const blockIt = () =>
    act(
      () => api.admin.block(token, lambda.publicKey, reason.trim(), chosen),
      chosen.length > 0
        ? `"${lambda.publicKey}" is blocked, and ${chosen.length === 1 ? 'one address' : `${chosen.length} addresses`} with it.`
        : `"${lambda.publicKey}" is blocked.`,
      'The lambda could not be blocked.',
    );

  const liftIt = () =>
    act(() => api.admin.unblock(token, lambda.publicKey), `The block of "${lambda.publicKey}" is lifted. It stays offline.`,
        'The block could not be lifted.');

  if (block) {
    return (
      <div className="surface border-red-500/30 bg-red-500/5 p-4">
        <div className="flex flex-wrap items-start gap-3">
          <div className="min-w-0 flex-1">
            <h2 className="text-sm font-medium text-red-700 dark:text-red-400">Blocked <Ago at={block.since} /></h2>
            <p className="mt-1 text-[13px] text-slate-600 dark:text-slate-400">
              {block.reason ? <>“{block.reason}”</> : <span className="text-slate-400">No reason given.</span>}
            </p>
            <p className="mt-2 text-[13px] text-slate-500">
              Offline, and every change is refused - for its owner through every door, for you too. Its owner can still read and
              download it, and is told it was blocked, not why. You can still delete it.
            </p>
          </div>
          <button type="button" onClick={liftIt} disabled={busy} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {busy && <IconSpinner />}
            Lift the block
          </button>
        </div>
        <p className="mt-3 text-[12px] text-slate-500">
          Lifting it leaves the lambda offline until it is put online again. Blocked addresses stay blocked - they are lifted
          under <Link to="/admin/blocks" className="text-accent-500 hover:underline">Blocked addresses</Link>, or by themselves
          after two weeks.
        </p>
      </div>
    );
  }

  const demo = isDemo(lambda.tier);

  return (
    <div className="surface p-4">
      <div className="flex flex-wrap items-start gap-3">
        <div className="min-w-0 flex-1">
          <h2 className="text-sm font-medium">Block</h2>
          <p className="mt-1 text-[13px] text-slate-500">
            {demo
              ? 'A demo is the installation’s own: there is nothing to block.'
              : 'Takes it offline with the previews of its drafts, and refuses every change to it - through the editor, the API, MCP and git - until the block is lifted. The addresses it was changed from can be blocked with it.'}
          </p>
        </div>
        {!demo && (
          <button type="button" onClick={ask} disabled={busy} className="btn-danger !px-3 !py-1.5 text-[13px]">
            Block…
          </button>
        )}
      </div>

      <Dialog
        title={`Block "${lambda.publicKey}"?`}
        open={asking}
        onClose={() => setAsking(false)}
        footer={
          <>
            <button type="button" onClick={() => setAsking(false)} className="btn-ghost">Keep it</button>
            <button type="button" onClick={blockIt} disabled={busy} className="btn-danger">
              {busy && <IconSpinner />}
              {chosen.length > 0 ? `Block it and ${chosen.length === 1 ? 'one address' : `${chosen.length} addresses`}` : 'Block it'}
            </button>
          </>
        }
      >
        <p className="text-sm text-slate-600 dark:text-slate-400">
          It goes offline, and nobody can change it until you lift the block. Its owner is told it was blocked, not why.
        </p>

        <label className="mt-4 block text-[13px] font-medium" htmlFor="block-reason">Why</label>
        <textarea
          id="block-reason"
          value={reason}
          onChange={(event) => setReason(event.target.value)}
          maxLength={500}
          rows={2}
          placeholder="Phishing for bank logins, reported by …"
          className="field mt-1 w-full text-sm"
        />

        <p className="mt-4 text-[13px] font-medium">Block these addresses as well, for two weeks</p>

        {editorAddresses.length === 0 ? (
          <p className="mt-1 text-[13px] text-slate-500">
            None known: it was not changed from outside the installation’s own networks in the last 30 days, or this server
            keeps no addresses.
          </p>
        ) : (
          <ul className="mt-1 divide-y divide-slate-100 text-[13px] dark:divide-ink-850">
            {editorAddresses.map((editor) => (
              <li key={editor.address}>
                <label className={`flex items-start gap-2.5 py-2 ${editor.blocked ? 'opacity-60' : 'cursor-pointer'}`}>
                  <input
                    type="checkbox"
                    className="mt-0.5"
                    checked={editor.blocked || chosen.includes(editor.address)}
                    disabled={editor.blocked}
                    onChange={() => toggle(editor.address)}
                  />
                  <span className="min-w-0 flex-1">
                    <span className="flex flex-wrap items-center gap-1.5">
                      <span className="break-all font-mono">{editor.range}</span>
                      {mine(editor) && (
                        <span className="rounded-full bg-amber-500/15 px-2 py-0.5 text-[11px] font-medium text-amber-700 dark:text-amber-400">
                          your address
                        </span>
                      )}
                      {editor.blocked && (
                        <span className="rounded-full bg-red-500/10 px-2 py-0.5 text-[11px] font-medium text-red-600 dark:text-red-400">
                          blocked already
                        </span>
                      )}
                    </span>
                    <span className="block text-slate-500">
                      {editor.range !== editor.address && <><span className="font-mono">{editor.address}</span> · </>}
                      first <Ago at={editor.firstSeen} />, last <Ago at={editor.lastSeen} />
                    </span>
                  </span>
                </label>
              </li>
            ))}
          </ul>
        )}

        {chosen.some((address) => editorAddresses.some((e) => e.address === address && mine(e))) && (
          <p className="mt-2 text-[13px] text-amber-600 dark:text-amber-400">
            That includes your own address: nothing but this panel will answer you until you lift its block.
          </p>
        )}
      </Dialog>
    </div>
  );
}
