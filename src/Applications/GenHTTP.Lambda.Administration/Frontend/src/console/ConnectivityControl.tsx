import { useState, type ReactNode } from 'react';

import { ApiError, api, isDemo, refusedToken, type AdminLambdaDetail, type Connectivity, type ConnectivityStatus } from '@genhttp-lambda/kit/api';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Action, actionButton } from './parts';

/**
 * Whether a lambda may connect to other servers, as a row of the Manage card
 * on its page: where it stands, and the decisions the operator can make. What
 * its owner asked for and what the operator said back are shown with the
 * lambda, apart from the row.
 *
 * A lambda whose tier connects by default - production, usually - may
 * without asking; what the operator decides about one comes before that
 * default, either way, and holds whatever tier it moves to.
 *
 * In English, like the panel it is a part of.
 */
export function ConnectivityAction({ token, detail, onChanged, deny }: {
  token: string;
  detail: AdminLambdaDetail;
  onChanged: (detail: AdminLambdaDetail) => void;
  /** The server turned the token away. */
  deny: () => void;
}) {
  const toast = useToast();

  const [deciding, setDeciding] = useState<boolean | null>(null);

  const { lambda, connects } = detail;
  const connectivity = lambda.connectivity;

  const demo = isDemo(lambda.tier);
  const blocked = detail.block != null;

  const decided = (next: AdminLambdaDetail, allow: boolean) => {
    onChanged(next);
    setDeciding(null);
    toast(allow ? `"${lambda.publicKey}" may connect to other servers from its next request.` : `"${lambda.publicKey}" may no longer connect to other servers.`, 'success');
  };

  return (
    <Action
      label="Connectivity"
      state={<span className="inline-flex items-center gap-1.5"><ConnectivityChip connectivity={connectivity} /></span>}
      note={demo ? 'A demo does not connect to other servers.' : blocked ? 'Nothing about it changes while it is blocked.' : undefined}
    >
      {!demo && !blocked && !connectivity.allowed && (
        <button type="button" onClick={() => setDeciding(true)} className={`btn-primary ${actionButton}`}>
          Allow…
        </button>
      )}
      {!demo && !blocked && (connectivity.status === 'Requested' || connectivity.allowed) && (
        <button type="button" onClick={() => setDeciding(false)} className={`btn-ghost ${actionButton}`}>
          {connectivity.status === 'Allowed' ? 'Take back…' : 'Decline…'}
        </button>
      )}

      <ConnectivityDecisionDialog
        token={token}
        publicKey={lambda.publicKey}
        purpose={connectivity.purpose}
        current={connectivity.status}
        allow={deciding}
        connects={connects}
        onClose={() => setDeciding(null)}
        onDecided={decided}
        deny={deny}
        failed={(message) => toast(message, 'error')}
      />
    </Action>
  );
}

/**
 * The operator's decision, with a note for the owner: allowing, or declining
 * - which takes back what was allowed as well. Shared by the lambda's page
 * and the list of requests.
 */
export function ConnectivityDecisionDialog({ token, publicKey, purpose, current, allow, connects, onClose, onDecided, deny, failed }: {
  token: string;
  publicKey: string;
  purpose?: string | null;
  /** Where it stands now, which says whether declining takes back what was allowed. */
  current?: ConnectivityStatus | null;
  /** What is being decided, or nothing while the dialog is closed. */
  allow: boolean | null;
  connects: boolean;
  onClose: () => void;
  onDecided: (detail: AdminLambdaDetail, allow: boolean) => void;
  deny: () => void;
  failed: (message: string) => void;
}) {
  const [note, setNote] = useState('');
  const [busy, setBusy] = useState(false);

  async function decide() {
    if (allow === null) {
      return;
    }

    setBusy(true);

    try {
      onDecided(await api.admin.decideConnectivity(token, publicKey, allow, note), allow);
      setNote('');
    } catch (error) {
      // the platform's own refusals say what kind they are; the token's do not
      if (refusedToken(error) && !(error as ApiError).kind) {
        deny();
      } else {
        failed(error instanceof ApiError ? error.message : 'The decision could not be saved.');
      }
    } finally {
      setBusy(false);
    }
  }

  return (
    <Dialog
      title={allow
        ? `Let "${publicKey}" connect to other servers?`
        : current === 'Allowed' ? `Take back what "${publicKey}" was allowed?` : `Decline what "${publicKey}" asked for?`}
      open={allow !== null}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">Cancel</button>
          <button type="button" onClick={decide} disabled={busy} className={allow ? 'btn-primary' : 'btn-danger'}>
            {busy && <IconSpinner />}
            {allow ? 'Allow it' : current === 'Allowed' ? 'Take it back' : 'Decline it'}
          </button>
        </>
      }
    >
      {purpose && (
        <div className="mb-4">
          <p className="text-[13px] font-medium">What its owner says it is for</p>
          <blockquote className="mt-1 whitespace-pre-line break-words border-l-2 border-slate-300 pl-3 text-sm text-slate-700 dark:border-ink-700 dark:text-slate-300">
            {purpose}
          </blockquote>
        </div>
      )}

      <p className="text-sm text-slate-600 dark:text-slate-400">
        {allow
          ? 'From its next request on, its code may call any server - not only the ones named. The lambda and the previews of its drafts are built again with it; nothing is put online.'
          : connects
            ? 'From its next request on, its code may not connect to other servers. Its code does, and no longer compiles until it changes - where that is the version online, it stops working. Its owner may ask again.'
            : 'From its next request on, its code may not connect to other servers. Its owner may ask again.'}
      </p>

      <label className="mt-4 block text-[13px] font-medium" htmlFor="connectivity-note">
        Note <span className="font-normal text-slate-500">- its owner reads this, optional</span>
      </label>
      <textarea
        id="connectivity-note"
        value={note}
        onChange={(event) => setNote(event.target.value)}
        maxLength={500}
        rows={2}
        placeholder={allow ? 'Fine for the forecast' : 'It fetches whatever address it is given'}
        className="field mt-1 w-full text-sm"
      />
    </Dialog>
  );
}

/** Where a lambda's connectivity stands, as a chip beside its name. */
export function ConnectivityChip({ connectivity }: { connectivity: Connectivity }) {
  if (connectivity.included && connectivity.status == null) {
    return <Chip tone="bg-logo-500/10 text-logo-700 dark:text-logo-400" title="Its tier connects by default">Included</Chip>;
  }

  switch (connectivity.status) {
    case 'Requested':
      return <Chip tone="bg-amber-500/15 text-amber-700 dark:text-amber-400" title="Its owner asked, and waits for a decision">Requested</Chip>;
    case 'Allowed':
      return <Chip tone="bg-emerald-500/10 text-emerald-700 dark:text-emerald-400" title="You allowed it">Allowed</Chip>;
    case 'Declined':
      return <Chip tone="bg-red-500/10 text-red-600 dark:text-red-400" title="Declined, or taken back">Declined</Chip>;
    default:
      return <Chip tone="bg-slate-400/10 text-slate-500" title="Nothing was asked">Not allowed</Chip>;
  }
}

function Chip({ tone, title, children }: { tone: string; title: string; children: ReactNode }) {
  return (
    <span title={title} className={`inline-flex items-center whitespace-nowrap rounded-full px-2 py-0.5 text-[11px] font-medium ${tone}`}>
      {children}
    </span>
  );
}
