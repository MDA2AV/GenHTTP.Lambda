import { useState, type ReactNode } from 'react';

import { ApiError, api, isDemo, refusedToken, type AdminLambdaDetail, type Egress, type EgressStatus } from '@genhttp-lambda/kit/api';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconGlobe, IconSpinner } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Ago } from '@genhttp-lambda/kit/ui/ui';

/**
 * Whether a lambda may connect to other servers, on its page in the panel:
 * where its owner's request stands, what they said it is for, whether its
 * code connects at all - and the operator's decision.
 *
 * A premium lambda may without asking; what the operator decides for one
 * holds once it leaves the tier.
 *
 * In English, like the panel it is a part of.
 */
export function EgressControl({ token, detail, onChanged, deny }: {
  token: string;
  detail: AdminLambdaDetail;
  onChanged: (detail: AdminLambdaDetail) => void;
  /** The server turned the token away. */
  deny: () => void;
}) {
  const toast = useToast();

  const [deciding, setDeciding] = useState<boolean | null>(null);

  const { lambda, connects } = detail;
  const egress = lambda.egress;

  const demo = isDemo(lambda.tier);
  const blocked = detail.block != null;

  const decided = (next: AdminLambdaDetail, allow: boolean) => {
    onChanged(next);
    setDeciding(null);
    toast(allow ? `"${lambda.publicKey}" may connect to other servers from its next request.` : `"${lambda.publicKey}" may no longer connect to other servers.`, 'success');
  };

  const status = egress.included
    ? 'Included with premium'
    : egress.status === 'Requested'
      ? 'Waiting for you'
      : egress.status === 'Allowed'
        ? 'Allowed'
        : egress.status === 'Declined'
          ? 'Declined'
          : 'Not allowed';

  return (
    <div className={`surface p-4 ${egress.status === 'Requested' && !egress.included ? 'border-amber-500/40 bg-amber-500/5' : ''}`}>
      <div className="flex flex-wrap items-start gap-3">
        <div className="min-w-0 flex-1">
          <h2 className="flex flex-wrap items-center gap-2 text-sm font-medium">
            <IconGlobe className="h-4 w-4 text-slate-400" />
            Egress
            <EgressChip egress={egress} />
          </h2>

          <p className="mt-1 text-[13px] text-slate-500">
            {demo
              ? 'A demo does not connect to other servers.'
              : egress.included
                ? 'Its code may connect to other servers - call an API, open a socket, send mail - because premium includes it.'
                : egress.allowed
                  ? 'Its code may connect to other servers - call an API, open a socket, send mail.'
                  : 'Its code may not connect to other servers: code that calls an API, opens a socket or sends mail does not compile.'}
            {' '}
            <span className={connects && !egress.allowed ? 'text-amber-600 dark:text-amber-400' : ''}>
              {connects
                ? egress.allowed ? 'Its code connects.' : 'Its code connects, and so does not compile.'
                : 'Its code does not connect.'}
            </span>
          </p>
        </div>

        {!demo && !blocked && (
          <div className="flex shrink-0 gap-1.5">
            {egress.status !== 'Allowed' && (
              <button type="button" onClick={() => setDeciding(true)} className="btn-primary !px-3 !py-1.5 text-[13px]">
                Allow…
              </button>
            )}
            {(egress.status === 'Requested' || egress.status === 'Allowed') && (
              <button type="button" onClick={() => setDeciding(false)} className="btn-ghost !px-3 !py-1.5 text-[13px]">
                {egress.status === 'Allowed' ? 'Take back…' : 'Decline…'}
              </button>
            )}
          </div>
        )}
      </div>

      {(egress.purpose || egress.note) && (
        <dl className="mt-3 grid gap-3 text-[13px] sm:grid-cols-2">
          {egress.purpose && (
            <Said label={egress.status === 'Requested' ? <>Asked <Ago at={egress.changed!} /></> : 'What it was asked for'}>
              {egress.purpose}
            </Said>
          )}
          {egress.note && (
            <Said label={<>{status} <Ago at={egress.changed!} /> - the owner reads this</>}>{egress.note}</Said>
          )}
        </dl>
      )}

      {blocked && !demo && <p className="mt-3 text-[12px] text-slate-500">Nothing about it changes while it is blocked.</p>}

      <EgressDecisionDialog
        token={token}
        publicKey={lambda.publicKey}
        purpose={egress.purpose}
        current={egress.status}
        allow={deciding}
        connects={connects}
        onClose={() => setDeciding(null)}
        onDecided={decided}
        deny={deny}
        failed={(message) => toast(message, 'error')}
      />
    </div>
  );
}

/**
 * The operator's decision, with a note for the owner: allowing, or declining
 * - which takes back what was allowed as well. Shared by the lambda's page
 * and the list of requests.
 */
export function EgressDecisionDialog({ token, publicKey, purpose, current, allow, connects, onClose, onDecided, deny, failed }: {
  token: string;
  publicKey: string;
  purpose?: string | null;
  /** Where it stands now, which says whether declining takes back what was allowed. */
  current?: EgressStatus | null;
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
      onDecided(await api.admin.decideEgress(token, publicKey, allow, note), allow);
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

      <label className="mt-4 block text-[13px] font-medium" htmlFor="egress-note">
        Note <span className="font-normal text-slate-500">- its owner reads this, optional</span>
      </label>
      <textarea
        id="egress-note"
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

/** Where a lambda's egress stands, as a chip beside its name. */
export function EgressChip({ egress }: { egress: Egress }) {
  if (egress.included) {
    return <Chip tone="bg-logo-500/10 text-logo-700 dark:text-logo-400" title="Premium includes it">Included</Chip>;
  }

  switch (egress.status) {
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

/**
 * The same, beside the lambda in a list - only where there is something to
 * see: a request waiting, or egress allowed. Premium includes it, and its
 * badge says premium already.
 */
export function EgressBadge({ egress }: { egress?: Egress | null }) {
  if (!egress || egress.included) {
    return null;
  }

  if (egress.status === 'Requested') {
    return <Chip tone="bg-amber-500/15 text-amber-700 dark:text-amber-400" title="Asks to connect to other servers">Asks for egress</Chip>;
  }

  if (egress.status === 'Allowed') {
    return <Chip tone="bg-emerald-500/10 text-emerald-700 dark:text-emerald-400" title="May connect to other servers">Egress</Chip>;
  }

  return null;
}

function Chip({ tone, title, children }: { tone: string; title: string; children: ReactNode }) {
  return (
    <span title={title} className={`inline-flex items-center whitespace-nowrap rounded-full px-2 py-0.5 text-[11px] font-medium ${tone}`}>
      {children}
    </span>
  );
}

function Said({ label, children }: { label: ReactNode; children: ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className="mt-1 whitespace-pre-line break-words border-l-2 border-slate-200 pl-3 text-slate-700 dark:border-ink-700 dark:text-slate-300">
        {children}
      </dd>
    </div>
  );
}
