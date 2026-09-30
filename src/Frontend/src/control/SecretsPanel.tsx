import { useEffect, useState, type ReactNode } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, type SecretListing } from '../api';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconCheck, IconCopy, IconEye, IconKey, IconLock, IconPlus, IconSpinner, IconTrash } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { Ago } from './ui';

/** What a secret may be called: the name of an environment variable, which is where an exported lambda reads it. */
const NAME = /^[A-Za-z_][A-Za-z0-9_]{0,127}$/;

/** What the dialog is doing: adding a new secret, or giving one a value by the name it has. */
type Editing = { name?: string; replacing: boolean } | null;

/**
 * The secrets of a lambda - or, opened on a draft, of its test data.
 *
 * A value goes in once and never comes out, so the list is of names: when
 * each was set, and whether the code reads it. What the code reads that has
 * no value yet comes first, with the button that sets it, because until then
 * something is failing. The value field is hidden while it is typed, since
 * the reason to be here is often a screen somebody else can see.
 *
 * The simple view gets the same list in its own words, without the code: the
 * keys the app uses, and the ones it is waiting for. Setting one there
 * switches secrets on if the agent left them off, since the owner asked for
 * exactly that by typing one in.
 */
export function SecretsPanel({ control, listing, readOnly, onChanged }: {
  control: Control;
  listing: SecretListing | null;
  readOnly: boolean;
  onChanged: () => Promise<void>;
}) {
  const t = useEditorT();
  const said = t.data.secrets;
  const plain = t.data.simple;
  const toast = useToast();
  const [params, setParams] = useSearchParams();

  const [editing, setEditing] = useState<Editing>(null);
  const [removing, setRemoving] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const feature = control.feature?.info.key;
  const simple = control.simple;

  // arriving from the overview with a name to set opens the dialog for it
  const asked = params.get('set');

  useEffect(() => {
    if (asked && listing && !readOnly) {
      setEditing({ name: asked, replacing: listing.secrets.some((s) => s.name === asked) });

      const rest = new URLSearchParams(params);
      rest.delete('set');
      setParams(rest, { replace: true });
    }
  }, [asked, listing, readOnly, params, setParams]);

  if (!listing) {
    return (
      <div className="flex items-center gap-2 text-sm text-slate-500">
        <IconSpinner /> {t.files.reading}
      </div>
    );
  }

  async function save(name: string, value: string) {
    // what the owner types in the simple view is a request to have secrets
    if (!listing?.enabled && !feature) {
      await api.enableData(control.privateKey, 'secrets');
    }

    await api.secrets.set(control.privateKey, name, value, feature);

    toast(simple ? plain.saved(name) : said.saved(name), 'success');

    setEditing(null);

    await Promise.all([onChanged(), control.refresh()]);
  }

  async function remove(name: string) {
    setBusy(true);

    try {
      await api.secrets.remove(control.privateKey, name, feature);

      toast(said.deleted(name));
      setRemoving(null);

      await Promise.all([onChanged(), control.refresh()]);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.deleteFailed, 'error');
    } finally {
      setBusy(false);
    }
  }

  const stored = listing.secrets;
  const full = stored.length >= listing.limit;
  const deleting = stored.find((s) => s.name === removing) ?? null;

  // off in the full view is said by the switch above, and a draft's copy is
  // never off on its own; what is left to say is what the code would read
  if (!listing.enabled && !simple) {
    // the switch above says they are off; nothing more to say unless the code wants some
    if (listing.used.length === 0 && !feature) {
      return null;
    }

    return (
      <div className="surface flex flex-wrap items-center gap-x-4 gap-y-3 p-5">
        <IconLock className="h-5 w-5 shrink-0 text-slate-400" />
        <div className="min-w-0 flex-1">
          {feature && <p className="font-medium">{said.offTitle}</p>}
          <p className="mt-0.5 text-[13px] text-slate-600 dark:text-slate-400">
            {listing.used.length > 0 ? said.offWanted(<Names names={listing.used} />) : said.offText}
          </p>
        </div>
        {!readOnly && !feature && (
          <button
            type="button"
            className="btn-primary !px-4 !py-1.5 text-[13px]"
            onClick={async () => {
              try {
                await api.enableData(control.privateKey, 'secrets');
                toast(t.data.kinds.secrets.switchedOn, 'success');
                await Promise.all([onChanged(), control.refresh()]);
              } catch (error) {
                toast(error instanceof ApiError ? error.message : t.data.switchFailed, 'error');
              }
            }}
          >
            {said.switchOn}
          </button>
        )}
      </div>
    );
  }

  return (
    <div className="space-y-5">
      {simple ? (
        <div className="flex flex-wrap items-start gap-x-4 gap-y-2">
          <div className="min-w-0 flex-1">
            <h2 className="flex items-center gap-2 text-[15px] font-medium">
              <IconKey className="h-4 w-4 text-accent-500 dark:text-accent-400" />
              {plain.secrets}
            </h2>
            <p className="mt-1 max-w-2xl text-[13px] text-slate-600 dark:text-slate-400">{plain.secretsText}</p>
          </div>
        </div>
      ) : (
        (stored.length > 0 || listing.missing.length > 0) && (
          <div className="flex min-h-8 flex-wrap items-center justify-between gap-3">
            <h2 className="flex items-center gap-2 text-sm font-medium">
              <IconKey className="h-4 w-4 text-slate-400" />
              {feature ? said.copyContents : said.contents}
            </h2>
            {!readOnly && (
              <button type="button" onClick={() => setEditing({ replacing: false })} disabled={full} className="btn-primary !px-4 !py-1.5 text-[13px]">
                <IconPlus className="h-3.5 w-3.5" />
                {said.add}
              </button>
            )}
          </div>
        )
      )}

      {listing.missing.length > 0 && (
        <section className="border border-amber-500/40 bg-amber-500/5">
          <div className="flex items-start gap-3 px-4 py-3">
            <IconAlert className="mt-0.5 h-4 w-4 shrink-0 text-amber-600 dark:text-amber-400" />
            <div className="min-w-0">
              <h3 className="text-sm font-medium">{simple ? t.simple.needsKey(listing.missing.length) : said.missingTitle(listing.missing.length)}</h3>
              {!simple && <p className="mt-0.5 text-[13px] text-slate-600 dark:text-slate-400">{said.missingText(listing.missing.length)}</p>}
            </div>
          </div>
          <ul className="divide-y divide-amber-500/20 border-t border-amber-500/20">
            {listing.missing.map((name) => (
              <li key={name} className="flex items-center gap-3 px-4 py-2.5">
                <IconKey className="h-4 w-4 shrink-0 text-amber-600 dark:text-amber-400" />
                <code className="min-w-0 flex-1 truncate font-mono text-[13px] font-medium">{name}</code>
                {simple && <span className="hidden text-xs text-amber-700 dark:text-amber-400 sm:inline">{plain.needed}</span>}
                {!readOnly && (
                  <button type="button" onClick={() => setEditing({ name, replacing: false })} className="btn-primary !px-3.5 !py-1 text-[13px]">
                    {simple ? plain.enter : said.setIt}
                  </button>
                )}
              </li>
            ))}
          </ul>
        </section>
      )}

      {stored.length > 0 ? (
        <ul className="surface divide-y divide-slate-200 dark:divide-ink-800">
          {stored.map((secret) => (
            <li key={secret.name} className="flex flex-wrap items-center gap-x-4 gap-y-2 px-4 py-3">
              <span className="flex h-8 w-8 shrink-0 items-center justify-center bg-accent-500/10 text-accent-600 dark:text-accent-400">
                <IconKey className="h-4 w-4" />
              </span>

              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center gap-2">
                  <code className="break-all font-mono text-[13px] font-medium">{secret.name}</code>
                  {!simple && (
                    <span
                      title={secret.used ? said.usedHint : said.unusedHint}
                      className={`rounded-full px-2 py-px text-[11px] font-medium ${
                        secret.used
                          ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400'
                          : 'bg-slate-400/10 text-slate-500'
                      }`}
                    >
                      {secret.used ? said.used : said.unused}
                    </span>
                  )}
                </div>
                <p className="mt-1 flex flex-wrap items-center gap-x-2 text-xs text-slate-500">
                  <span className="inline-flex items-center gap-1.5" title={said.hidden}>
                    <IconLock className="h-3 w-3" />
                    <span aria-hidden="true" className="font-mono tracking-[0.2em]">••••••••••</span>
                    <span className="sr-only">{said.hidden}</span>
                  </span>
                  <span aria-hidden="true" className="hidden sm:inline">·</span>
                  <span>{said.set(<Ago at={secret.changed} />)}</span>
                </p>
              </div>

              {!readOnly && (
                <div className="flex shrink-0 items-center gap-1">
                  <button
                    type="button"
                    onClick={() => setEditing({ name: secret.name, replacing: true })}
                    className="btn-ghost !px-3 !py-1 text-[13px]"
                    title={said.replaceHint}
                  >
                    {simple ? plain.change : said.replace}
                  </button>
                  <button
                    type="button"
                    onClick={() => setRemoving(secret.name)}
                    className="rounded-full p-1.5 text-slate-400 hover:bg-red-500/10 hover:text-red-500"
                    aria-label={said.delete(secret.name)}
                    title={said.delete(secret.name)}
                  >
                    <IconTrash className="h-4 w-4" />
                  </button>
                </div>
              )}
            </li>
          ))}
        </ul>
      ) : (
        listing.missing.length === 0 && (
          <div className="surface flex flex-col items-center px-6 py-10 text-center">
            <span className="flex h-12 w-12 items-center justify-center rounded-full bg-accent-500/10 text-accent-500 dark:text-accent-400">
              <IconKey className="h-6 w-6" />
            </span>
            <h3 className="mt-4 font-medium">{said.empty}</h3>
            <p className="mt-1 max-w-md text-[13px] text-slate-500">{said.emptyText}</p>
            {!readOnly && (
              <button type="button" onClick={() => setEditing({ replacing: false })} className="btn-primary mt-5 !px-4 !py-1.5 text-[13px]">
                <IconPlus className="h-3.5 w-3.5" />
                {said.add}
              </button>
            )}
          </div>
        )
      )}

      {listing.optional.length > 0 && !simple && (
        <section>
          <h3 className="text-xs font-medium uppercase tracking-wide text-slate-500">{said.optionalTitle}</h3>
          <p className="mt-0.5 text-[13px] text-slate-500">{said.optionalText}</p>
          <ul className="mt-2 flex flex-wrap gap-2">
            {listing.optional.map((name) => (
              <li key={name}>
                <button
                  type="button"
                  disabled={readOnly}
                  onClick={() => setEditing({ name, replacing: false })}
                  className="inline-flex items-center gap-1.5 border border-dashed border-slate-300 px-2.5 py-1 font-mono text-[12px] text-slate-600 hover:border-accent-500 hover:text-accent-600 disabled:pointer-events-none dark:border-ink-700 dark:text-slate-400 dark:hover:text-accent-400"
                >
                  <IconPlus className="h-3 w-3" />
                  {name}
                </button>
              </li>
            ))}
          </ul>
        </section>
      )}

      {!simple && <HowTo />}

      <SecretDialog
        open={editing !== null}
        editing={editing}
        existing={stored.map((s) => s.name)}
        draft={!!feature}
        simple={simple}
        switchesOn={simple && !listing.enabled}
        onClose={() => setEditing(null)}
        onSave={save}
      />

      <Dialog
        title={deleting ? said.deleteTitle(deleting.name) : ''}
        open={deleting !== null}
        onClose={() => setRemoving(null)}
        footer={
          <>
            <button type="button" onClick={() => setRemoving(null)} className="btn-ghost">
              {said.cancel}
            </button>
            <button type="button" className="btn-danger" disabled={busy} onClick={() => deleting && remove(deleting.name)}>
              {busy && <IconSpinner />}
              {said.deleteConfirm}
            </button>
          </>
        }
      >
        {deleting && (
          <>
            <p className={deleting.used ? 'flex gap-2 text-amber-700 dark:text-amber-400' : 'text-slate-600 dark:text-slate-400'}>
              {deleting.used && <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />}
              {deleting.used ? said.deleteUsed : said.deleteUnused}
            </p>
            {feature && <p className="text-slate-600 dark:text-slate-400">{said.deleteDraft}</p>}
          </>
        )}
      </Dialog>
    </div>
  );
}

/** Asks for a value - and a name, for a new secret - and saves it. */
function SecretDialog({ open, editing, existing, draft, simple, switchesOn, onClose, onSave }: {
  open: boolean;
  editing: Editing;
  existing: string[];
  draft: boolean;
  /** Whether the simple view is asking, which says it in its own words. */
  simple: boolean;
  /** Whether saving switches secrets on, which is said. */
  switchesOn: boolean;
  onClose: () => void;
  onSave: (name: string, value: string) => Promise<void>;
}) {
  const t = useEditorT();
  const said = t.data.secrets;
  const plain = t.data.simple;

  const [name, setName] = useState('');
  const [value, setValue] = useState('');
  const [shown, setShown] = useState(false);
  const [busy, setBusy] = useState(false);
  const [failure, setFailure] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setName(editing?.name ?? '');
      setValue('');
      setShown(false);
      setFailure(null);
    }
  }, [open, editing]);

  const fixed = editing?.name != null;
  const valid = NAME.test(name);
  const taken = !fixed && existing.includes(name);

  const title = !fixed
    ? said.addTitle
    : editing!.replacing
      ? (simple ? plain.changeTitle(name) : said.replaceTitle(name))
      : (simple ? plain.enterTitle(name) : said.setTitle(name));

  async function submit() {
    if (!valid || value.length === 0 || busy) {
      return;
    }

    setBusy(true);
    setFailure(null);

    try {
      await onSave(name, value);
    } catch (error) {
      setFailure(error instanceof ApiError ? error.message : said.saveFailed);
    } finally {
      setBusy(false);
    }
  }

  return (
    <Dialog
      title={title}
      open={open}
      onClose={onClose}
      footer={
        <>
          <button type="button" onClick={onClose} className="btn-ghost">
            {said.cancel}
          </button>
          <button type="button" onClick={submit} disabled={!valid || value.length === 0 || busy} className="btn-primary">
            {busy ? <IconSpinner /> : <IconLock className="h-3.5 w-3.5" />}
            {said.save}
          </button>
        </>
      }
    >
      {!fixed && (
        <label className="block">
          <span className="text-slate-600 dark:text-slate-400">{said.name}</span>
          <input
            autoFocus
            value={name}
            onChange={(event) => setName(event.target.value.replace(/\s/g, '_'))}
            placeholder="STRIPE_KEY"
            maxLength={128}
            spellCheck={false}
            autoComplete="off"
            autoCapitalize="characters"
            className="field mt-2 font-mono"
            aria-invalid={name.length > 0 && !valid}
          />
          <span className={`mt-1.5 block text-xs ${name.length > 0 && !valid ? 'text-red-500' : taken ? 'text-amber-700 dark:text-amber-400' : 'text-slate-500'}`}>
            {name.length > 0 && !valid ? said.nameInvalid : taken ? said.nameTaken(name) : said.nameHint}
          </span>
        </label>
      )}

      <label className="block">
        <span className="flex items-center justify-between">
          <span className="text-slate-600 dark:text-slate-400">{said.value}</span>
          <button
            type="button"
            onClick={() => setShown((was) => !was)}
            className="inline-flex items-center gap-1 text-xs text-slate-500 hover:text-slate-800 dark:hover:text-slate-200"
            aria-pressed={shown}
          >
            <IconEye className="h-3.5 w-3.5" />
            {shown ? said.hide : said.show}
          </button>
        </span>
        <textarea
          autoFocus={fixed}
          value={value}
          onChange={(event) => setValue(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' && (event.ctrlKey || event.metaKey || !value.includes('\n'))) {
              if (!event.shiftKey) {
                event.preventDefault();
                submit();
              }
            }
          }}
          rows={value.includes('\n') ? 6 : 2}
          placeholder={said.valuePlaceholder}
          spellCheck={false}
          autoComplete="off"
          autoCorrect="off"
          autoCapitalize="off"
          data-1p-ignore="true"
          data-lpignore="true"
          className={`field mt-2 resize-y break-all font-mono text-[13px] ${shown ? '' : '[-webkit-text-security:disc]'}`}
        />
      </label>

      <p className="flex gap-2 text-[13px] text-slate-600 dark:text-slate-400">
        <IconLock className="mt-0.5 h-3.5 w-3.5 shrink-0 text-accent-500 dark:text-accent-400" />
        <span>{simple ? plain.secretsText : said.sealed}</span>
      </p>

      {draft && <p className="text-[13px] text-slate-600 dark:text-slate-400">{said.inDraft}</p>}
      {switchesOn && <p className="text-[13px] text-slate-600 dark:text-slate-400">{plain.switchesOn}</p>}
      {failure && <p className="text-[13px] text-red-500">{failure}</p>}
    </Dialog>
  );
}

/** How the code reads them, for whoever writes it - with the line to copy. */
function HowTo() {
  const said = useEditorT().data.secrets;

  return (
    <aside className="border-l-2 border-slate-300 pl-3 text-[13px] text-slate-600 dark:border-ink-700 dark:text-slate-400">
      <h3 className="font-medium text-slate-700 dark:text-slate-300">{said.howTo}</h3>
      <p className="mt-1 leading-relaxed">{said.howToText((text) => <Snippet text={text} />)}</p>
    </aside>
  );
}

function Snippet({ text }: { text: string }) {
  const said = useEditorT().data.secrets;
  const [copied, setCopied] = useState(false);

  return (
    <span className="inline-flex items-center gap-1 whitespace-nowrap bg-slate-100 px-1.5 py-0.5 align-baseline dark:bg-ink-850">
      <code className="font-mono text-[12px] text-ink-900 dark:text-slate-200">{text}</code>
      <button
        type="button"
        onClick={async () => {
          try {
            await navigator.clipboard.writeText(text);
            setCopied(true);
            window.setTimeout(() => setCopied(false), 1500);
          } catch {
            // nothing to copy to is not worth an error
          }
        }}
        className="text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
        aria-label={said.copy}
        title={said.copy}
      >
        {copied ? <IconCheck className="h-3 w-3 text-emerald-500" /> : <IconCopy className="h-3 w-3" />}
      </button>
    </span>
  );
}

/** A few names, in the code's own spelling. */
export function Names({ names }: { names: string[] }): ReactNode {
  return names.map((name, index) => (
    <span key={name}>
      {index > 0 && ', '}
      <code className="font-mono text-[12px] font-medium text-ink-900 dark:text-slate-200">{name}</code>
    </span>
  ));
}
