import { useEffect, useState } from 'react';

import { ApiError, api, type DomainState } from '../api';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconCheck, IconCopy, IconExternal, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import type { Control } from './context';
import { Section } from './ui';

/**
 * Where a domain has to point to reach this server.
 *
 * Written down here rather than asked for: they are the addresses genhttp.dev
 * resolves to, and an installation cannot reliably tell its own public
 * addresses from inside a container.
 */
const ADDRESSES = {
  v4: '152.53.120.139',
  v6: '2a0a:4cc0:c0:4fc0:54d0:b1ff:fe46:c896',
};

/** What a CNAME points at, for the owners who prefer one. */
const CANONICAL = 'genhttp.dev';

/**
 * A domain of the lambda's own.
 *
 * Part of the premium tier, which only the operator assigns - the editor
 * only offers this section to a lambda in it. Once a domain is set the page
 * is mostly about DNS - what to set, and whether it has been set - since that
 * is the part only the owner can do.
 */
export function DomainTab({ control }: { control: Control }) {
  const { privateKey, lambda } = control;

  const toast = useToast();

  const [state, setState] = useState<DomainState | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [typed, setTyped] = useState('');
  const [problem, setProblem] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [checking, setChecking] = useState(false);
  const [removing, setRemoving] = useState(false);

  async function load(quiet = false) {
    if (!quiet) {
      setChecking(true);
    }

    try {
      const found = await api.domain(privateKey);

      setState(found);
      setFailure(null);
      setTyped((was) => (was === '' ? found.domain ?? '' : was));
    } catch (error) {
      setFailure(error instanceof ApiError ? error.message : 'The domain could not be read.');
    } finally {
      setChecking(false);
    }
  }

  useEffect(() => {
    load(true);
    // the tier is changed by somebody else, so the frame's refresh is the cue
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [privateKey, lambda.tier]);

  async function save(event: React.FormEvent) {
    event.preventDefault();

    setSaving(true);
    setProblem(null);

    try {
      const saved = await api.setDomain(privateKey, typed.trim());

      setState(saved);
      setTyped(saved.domain ?? '');

      await control.refresh();

      toast(`Requests to ${saved.domain} now reach this lambda.`, 'success');
    } catch (error) {
      setProblem(error instanceof ApiError ? error.message : 'The domain could not be saved.');
    } finally {
      setSaving(false);
    }
  }

  async function remove() {
    setRemoving(false);

    try {
      const removed = await api.removeDomain(privateKey);

      setState(removed);
      setTyped('');

      await control.refresh();

      toast('The domain is removed. The lambda still answers at its address here.');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The domain could not be removed.', 'error');
    }
  }

  const hint = (
    <>
      A premium lambda can answer at a domain of its own - the whole of it, from the root down - as well as at its
      address here. Point the domain at this server, enter it here, and requests to it reach the lambda.
    </>
  );

  if (!state) {
    return (
      <Section title="Domain" hint={hint}>
        {failure ? (
          <p className="py-10 text-sm text-red-500">{failure}</p>
        ) : (
          <div className="flex items-center gap-2 py-10 text-sm text-slate-500"><IconSpinner /> Loading…</div>
        )}
      </Section>
    );
  }

  const changed = typed.trim() !== (state.domain ?? '') && typed.trim() !== '';
  const visible = state.domain ?? (typed.trim() || 'your-domain.com');

  return (
    <Section
      title="Domain"
      hint={hint}
      actions={
        state.served && state.domain ? (
          <a href={`https://${state.domain}/`} target="_blank" rel="noreferrer" className="btn-ghost !px-3 !py-1.5 text-[13px]">
            Open {state.domain}
            <IconExternal className="h-3.5 w-3.5" />
          </a>
        ) : undefined
      }
    >
      <>
        <form onSubmit={save} className="surface p-4">
          <label htmlFor="domain" className="text-[15px] font-medium">The domain it answers at</label>
          <p className="mt-1 text-[13px] text-slate-500">
            {state.served && state.domain
              ? <>Serving <span className="font-mono">{state.domain}</span> now, besides its address here.</>
              : 'None yet. A subdomain such as shop.example.com, or a whole domain such as example.com.'}
          </p>

          <div className="mt-3 flex flex-wrap gap-2">
            <input
              id="domain"
              value={typed}
              onChange={(event) => setTyped(event.target.value)}
              placeholder="shop.example.com"
              spellCheck={false}
              autoComplete="off"
              className="field min-w-0 flex-1 font-mono sm:max-w-md"
            />
            <button type="submit" disabled={!changed || saving} className="btn-primary">
              {saving && <IconSpinner />}
              {state.domain ? 'Change' : 'Use this domain'}
            </button>
            {state.domain && (
              <button type="button" onClick={() => setRemoving(true)} className="btn-ghost">
                Remove
              </button>
            )}
          </div>

          {problem && <p className="mt-2 text-xs text-red-500">{problem}</p>}
        </form>

        <Records domain={visible} configured={state.domain != null} state={state} checking={checking} onCheck={() => load()} />
      </>

      <Dialog
        title="Remove the domain?"
        open={removing}
        onClose={() => setRemoving(false)}
        footer={
          <>
            <button type="button" onClick={() => setRemoving(false)} className="btn-ghost">Keep it</button>
            <button type="button" onClick={remove} className="btn-danger">Remove</button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">
          Requests to <span className="font-mono">{state.domain}</span> stop reaching this lambda at once. Its address
          here stays as it is, and so does whatever the domain's DNS says.
        </p>
      </Dialog>
    </Section>
  );
}

/**
 * The records to set at the domain's DNS provider, and whether the server
 * sees them yet.
 */
function Records({ domain, configured, state, checking, onCheck }: {
  domain: string;
  configured: boolean;
  state: DomainState;
  checking: boolean;
  onCheck: () => void;
}) {
  const resolved = state.dns?.addresses ?? [];
  const ours = new Set([ADDRESSES.v4, ADDRESSES.v6]);
  const pointsHere = resolved.some((address) => ours.has(address));
  const elsewhere = resolved.filter((address) => !ours.has(address));

  return (
    <section className="mt-8">
      <div className="flex items-center justify-between gap-3">
        <h2 className="text-sm font-medium">Point the domain at this server</h2>
        {configured && (
          <button type="button" onClick={onCheck} disabled={checking} className="btn-ghost !px-3 !py-1 text-[13px]">
            {checking && <IconSpinner />}
            Check again
          </button>
        )}
      </div>

      <p className="mt-1 text-[13px] text-slate-600 dark:text-slate-400">
        At whoever manages the DNS of the domain, add these two records. Leave out the AAAA record if you would rather
        not be reachable over IPv6.
      </p>

      <div className="surface mt-3 overflow-x-auto">
        <table className="w-full text-left text-[13px]">
          <thead className="text-slate-500">
            <tr className="border-b border-slate-200 dark:border-ink-800">
              <th className="px-4 py-2 font-normal">Type</th>
              <th className="px-4 py-2 font-normal">Name</th>
              <th className="px-4 py-2 font-normal">Value</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 dark:divide-ink-850">
            <Record type="A" name={domain} value={ADDRESSES.v4} />
            <Record type="AAAA" name={domain} value={ADDRESSES.v6} />
          </tbody>
        </table>
      </div>

      {configured && state.dns && (
        <p className="mt-3 flex items-start gap-2 text-[13px]">
          {pointsHere ? (
            <>
              <IconCheck className="mt-0.5 h-4 w-4 shrink-0 text-emerald-500" />
              <span>
                <span className="font-mono">{domain}</span> points here.
                {elsewhere.length > 0 && <> It also resolves to {elsewhere.join(', ')}, which is not this server - visitors sent there will not reach the lambda.</>}
              </span>
            </>
          ) : (
            <>
              <IconAlert className="mt-0.5 h-4 w-4 shrink-0 text-amber-500" />
              <span className="text-slate-600 dark:text-slate-400">
                {resolved.length > 0
                  ? <>It resolves to {resolved.join(', ')}, which is not this server yet.</>
                  : state.dns.problem}{' '}
                A change can take a while to be seen everywhere - up to the time to live of the old record.
              </span>
            </>
          )}
        </p>
      )}

      <details className="mt-5 text-[13px]">
        <summary className="cursor-pointer select-none text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-200">
          Using a CNAME record instead
        </summary>
        <div className="mt-2 space-y-2 text-slate-600 dark:text-slate-400">
          <p>
            A subdomain can point at <span className="font-mono">{CANONICAL}</span> with a CNAME record instead, and then
            follows this server if its addresses ever change. It has drawbacks:
          </p>
          <ul className="list-disc space-y-1 pl-5">
            <li>
              It cannot be used for a whole domain (<span className="font-mono">example.com</span> itself): the
              standard does not allow a CNAME next to the records every domain has at its root. Some providers offer
              an ALIAS, ANAME or "flattened" record that works there instead.
            </li>
            <li>Nothing else can sit on the same name - no MX record for mail, no TXT record for verifications.</li>
            <li>Visitors' resolvers make one more lookup before they arrive.</li>
          </ul>
        </div>
      </details>
    </section>
  );
}

function Record({ type, name, value }: { type: string; name: string; value: string }) {
  return (
    <tr>
      <td className="px-4 py-2 font-mono">{type}</td>
      <td className="px-4 py-2 font-mono">{name}</td>
      <td className="px-4 py-2">
        <span className="flex items-center gap-1 font-mono">
          <span className="min-w-0 truncate">{value}</span>
          <Copy value={value} />
        </span>
      </td>
    </tr>
  );
}

function Copy({ value }: { value: string }) {
  const [copied, setCopied] = useState(false);

  return (
    <button
      type="button"
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(value);
          setCopied(true);
          window.setTimeout(() => setCopied(false), 1500);
        } catch {
          // a clipboard that refuses is not worth an error
        }
      }}
      className="p-1 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
      title="Copy"
      aria-label={`Copy ${value}`}
    >
      {copied ? <IconCheck className="h-3.5 w-3.5 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5" />}
    </button>
  );
}
