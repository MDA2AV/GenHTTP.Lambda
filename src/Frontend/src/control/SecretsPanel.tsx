import { useEffect, useRef, useState, type FormEvent } from 'react';

import { ApiError, api, type SecretEntry, type SecretListing } from '../api';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconCheck, IconCopy, IconEye, IconEyeOff, IconLock, IconPencil, IconPlus, IconSpinner, IconTrash } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { Ago } from './ui';

/** The shape of a name: what the code asks for a secret by, and what an environment variable is called when the lambda is exported. */
const NAME = /^[A-Za-z_][A-Za-z0-9_]{0,63}$/;

/**
 * The secrets of a lambda - or, opened on a draft, of its test copy.
 *
 * Everything here is about one asymmetry: a secret can be written and can not
 * be read. So the list has names and moments, never a value; the only thing
 * that can be done to one that is there is to replace it or delete it; and the
 * page says so before anybody wonders. What the code reads it with is next to
 * each name, since that is the one thing whoever writes the code has to know.
 *
 * In the simple view the same list is for somebody who had an app built: keys
 * rather than secrets, and nothing about how the code reads them.
 */
export function SecretsPanel({ control, listing, readOnly, onChanged }: {
  control: Control;
  /** Null until it has been read. */
  listing: SecretListing | null;
  readOnly: boolean;
  onChanged: () => Promise<void>;
}) {
  const t = useEditorT();
  const said = t.data.secrets;
  const simple = control.simple;
  const words = t.data.simple.secrets;
  const toast = useToast();

  const feature = control.feature?.info.key;

  /** The name being given a new value, or true for a secret that is not there yet. */
  const [editing, setEditing] = useState<string | true | null>(null);
  const [removing, setRemoving] = useState<SecretEntry | null>(null);
  const [deleting, setDeleting] = useState(false);

  const secrets = listing?.secrets ?? [];
  const full = listing !== null && secrets.length >= listing.limit;

  // what the version online reads is what would fail; a draft's copy breaks nothing that is online
  const inUse = !feature && !!control.summary?.storage.usesSecrets && control.lambda.activeVersion != null;

  async function remove(secret: SecretEntry) {
    setDeleting(true);

    try {
      await api.deleteSecret(control.privateKey, secret.name, feature);
      toast(simple ? words.removed(secret.name) : said.deleted(secret.name));
      setRemoving(null);

      await onChanged();
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.deleteFailed, 'error');
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-start gap-3 border-l-2 border-accent-500/40 pl-3 dark:border-accent-400/40">
        <IconLock className="mt-0.5 h-4 w-4 shrink-0 text-accent-500 dark:text-accent-400" />
        <p className="max-w-2xl text-[13px] text-slate-600 dark:text-slate-400">
          {simple ? words.privacy : feature ? said.copyPrivacy : said.privacy}
        </p>
      </div>

      {!readOnly && (
        <div className="flex flex-wrap items-center gap-3">
          <button
            type="button"
            className="btn-primary !px-4 !py-1.5 text-[13px]"
            onClick={() => setEditing(true)}
            disabled={editing !== null || listing === null || full}
            title={full ? said.full(listing?.limit ?? 0) : undefined}
          >
            <IconPlus className="h-3.5 w-3.5" />
            {simple ? words.add : said.add}
          </button>
          {listing && !simple && (
            <span className="text-[13px] tabular-nums text-slate-500">{said.limit(secrets.length, listing.limit)}</span>
          )}
          {full && <span className="text-[13px] text-amber-700 dark:text-amber-400">{said.full(listing?.limit ?? 0)}</span>}
        </div>
      )}
      {readOnly && <p className="text-[13px] text-slate-500">{said.demo}</p>}

      {editing === true && (
        <Composer
          control={control}
          existing={secrets.map((s) => s.name)}
          onDone={async (saved) => {
            setEditing(null);

            if (saved) {
              await onChanged();
            }
          }}
        />
      )}

      {listing === null ? (
        <div className="flex items-center gap-2 text-[13px] text-slate-500">
          <IconSpinner className="h-3.5 w-3.5" /> {t.files.reading}
        </div>
      ) : secrets.length === 0 && editing !== true ? (
        <div className="surface flex flex-col items-center px-6 py-10 text-center">
          <span className="flex h-11 w-11 items-center justify-center bg-accent-500/10 text-accent-600 dark:text-accent-400">
            <IconLock className="h-5 w-5" />
          </span>
          <h3 className="mt-3 text-sm font-medium">{simple ? words.empty : feature ? said.emptyCopy : said.empty}</h3>
          {!feature && <p className="mt-1 max-w-md text-[13px] text-slate-500">{simple ? words.emptyText : said.emptyText}</p>}
        </div>
      ) : secrets.length > 0 && (
        <ul className="surface divide-y divide-slate-200 dark:divide-ink-800">
          {secrets.map((secret) => (
            <li key={secret.name} className="px-4 py-3">
              {editing === secret.name ? (
                <Composer
                  control={control}
                  fixed={secret.name}
                  existing={secrets.map((s) => s.name)}
                  onDone={async (saved) => {
                    setEditing(null);

                    if (saved) {
                      await onChanged();
                    }
                  }}
                />
              ) : (
                <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
                  <span className="flex h-8 w-8 shrink-0 items-center justify-center bg-accent-500/10 text-accent-600 dark:text-accent-400">
                    <IconLock className="h-4 w-4" />
                  </span>

                  <div className="min-w-0 flex-1 basis-40">
                    <div className="break-all font-mono text-[13px] font-medium">{secret.name}</div>
                    <div className="mt-0.5 flex flex-wrap items-center gap-x-3 gap-y-0.5 text-xs text-slate-500">
                      <span className="select-none tracking-[0.2em]" title={said.hiddenTitle}>
                        <span aria-hidden="true">••••••••••</span>
                        <span className="sr-only">{said.hidden}</span>
                      </span>
                      <span>{said.set(<Ago at={secret.updated} />)}</span>
                    </div>
                  </div>

                  {!simple && <ReadCode name={secret.name} />}

                  {!readOnly && (
                    <div className="flex shrink-0 items-center gap-1">
                      <button
                        type="button"
                        onClick={() => setEditing(secret.name)}
                        disabled={editing !== null}
                        className="rounded-full p-1.5 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500 disabled:opacity-40"
                        title={said.replace(secret.name)}
                        aria-label={said.replace(secret.name)}
                      >
                        <IconPencil className="h-3.5 w-3.5" />
                      </button>
                      <button
                        type="button"
                        onClick={() => setRemoving(secret)}
                        className="rounded-full p-1.5 text-slate-400 hover:bg-red-500/10 hover:text-red-500"
                        title={said.delete(secret.name)}
                        aria-label={said.delete(secret.name)}
                      >
                        <IconTrash className="h-3.5 w-3.5" />
                      </button>
                    </div>
                  )}
                </div>
              )}
            </li>
          ))}
        </ul>
      )}

      <Dialog
        title={removing ? (simple ? words.removeTitle(removing.name) : said.deleteTitle(removing.name)) : ''}
        open={removing !== null}
        onClose={() => !deleting && setRemoving(null)}
        footer={
          <>
            <button type="button" onClick={() => setRemoving(null)} className="btn-ghost" disabled={deleting}>
              {said.keep}
            </button>
            <button type="button" className="btn-danger" onClick={() => removing && remove(removing)} disabled={deleting}>
              {deleting && <IconSpinner />}
              {simple ? words.removeConfirm : said.deleteConfirm}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">{simple ? words.removeText : said.deleteText}</p>
        {inUse && !simple && (
          <p className="mt-3 flex gap-2 text-amber-700 dark:text-amber-400">
            <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
            {said.deleteInUse}
          </p>
        )}
      </Dialog>
    </div>
  );
}

/**
 * How the code reads a secret, as the line to write - copied with one click,
 * since typing the name of a secret wrong is how one fails to be found.
 */
function ReadCode({ name }: { name: string }) {
  const said = useEditorT().data.secrets;
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (!copied) {
      return;
    }

    const timer = window.setTimeout(() => setCopied(false), 1600);

    return () => window.clearTimeout(timer);
  }, [copied]);

  return (
    <button
      type="button"
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(said.read(name));
          setCopied(true);
        } catch {
          // no clipboard here: the line is on the button to be copied by hand
        }
      }}
      className="group inline-flex max-w-full items-center gap-2 border border-slate-200 bg-slate-50 px-2 py-1 font-mono text-xs text-slate-600 hover:border-accent-500 hover:text-accent-600 dark:border-ink-800 dark:bg-ink-900 dark:text-slate-400 dark:hover:border-accent-400 dark:hover:text-accent-400"
      title={said.copyRead(name)}
      aria-label={said.copyRead(name)}
    >
      <span className="truncate">{said.read(name)}</span>
      {copied ? <IconCheck className="h-3.5 w-3.5 shrink-0 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5 shrink-0 opacity-60 group-hover:opacity-100" />}
      <span className="sr-only" role="status">{copied ? said.copied : ''}</span>
    </button>
  );
}

/**
 * Writing a secret: a new one, or a new value for one that is there.
 *
 * The value is masked as it is typed, and can be shown while it is typed - to
 * check a paste - which is all the reading there is. Several lines are for
 * what does not fit one, a certificate or the JSON of a service account.
 */
function Composer({ control, fixed, existing, onDone }: {
  control: Control;
  /** The secret whose value is replaced; left out for a new one. */
  fixed?: string;
  existing: string[];
  onDone: (saved: boolean) => Promise<void>;
}) {
  const said = useEditorT().data.secrets;
  const toast = useToast();

  const [name, setName] = useState(fixed ?? '');
  const [value, setValue] = useState('');
  const [reveal, setReveal] = useState(false);
  const [lines, setLines] = useState(false);
  const [saving, setSaving] = useState(false);

  const first = useRef<HTMLInputElement>(null);
  const second = useRef<HTMLInputElement & HTMLTextAreaElement>(null);

  // where the typing starts: the name for a new secret, the value for one that is there
  useEffect(() => {
    (fixed ? second : first).current?.focus();
  }, [fixed, lines]);

  const feature = control.feature?.info.key;

  const trimmed = name.trim();
  const valid = NAME.test(trimmed);
  const replaces = !fixed && existing.includes(trimmed);

  async function submit(event: FormEvent) {
    event.preventDefault();

    if (!valid || value.length === 0) {
      return;
    }

    setSaving(true);

    try {
      await api.setSecret(control.privateKey, trimmed, value, feature);
      toast(said.saved(trimmed), 'success');

      await onDone(true);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.saveFailed, 'error');
      setSaving(false);
    }
  }

  const id = `secret-${fixed ?? 'new'}`;

  return (
    <form
      onSubmit={submit}
      className="space-y-3 border border-accent-500/30 bg-accent-500/5 p-4 dark:border-accent-400/30"
      aria-label={fixed ? said.replacing(fixed) : said.add}
    >
      {fixed ? (
        <div>
          <h3 className="text-sm font-medium">{said.replacing(fixed)}</h3>
          <p className="mt-0.5 text-[13px] text-slate-500">{said.replaceNote}</p>
        </div>
      ) : (
        <div>
          <label htmlFor={`${id}-name`} className="mb-1 block text-[13px] font-medium">{said.name}</label>
          <input
            id={`${id}-name`}
            ref={first}
            className="field font-mono"
            value={name}
            onChange={(event) => setName(event.target.value)}
            placeholder={said.namePlaceholder}
            spellCheck={false}
            autoComplete="off"
            autoCapitalize="off"
            maxLength={64}
            aria-invalid={trimmed.length > 0 && !valid}
            aria-describedby={`${id}-help`}
            required
          />
          <p id={`${id}-help`} className={`mt-1 text-xs ${trimmed.length > 0 && !valid ? 'text-red-500' : 'text-slate-500'}`}>
            {trimmed.length > 0 && !valid ? said.nameInvalid : said.nameHelp}
          </p>
        </div>
      )}

      <div>
        <div className="mb-1 flex items-center justify-between gap-3">
          <label htmlFor={`${id}-value`} className="text-[13px] font-medium">{said.value}</label>
          <button
            type="button"
            onClick={() => setLines((was) => !was)}
            className="text-xs text-accent-600 hover:underline dark:text-accent-400"
          >
            {lines ? said.singleLine : said.multiline}
          </button>
        </div>

        <div className="relative">
          {lines ? (
            <textarea
              id={`${id}-value`}
              ref={second}
              className="field min-h-28 resize-y pr-10 font-mono"
              rows={5}
              value={value}
              onChange={(event) => setValue(event.target.value)}
              placeholder={said.valuePlaceholder}
              spellCheck={false}
              autoComplete="off"
              // masked with the browser's own disc, for those that have one; the eye shows it
              style={reveal ? undefined : ({ WebkitTextSecurity: 'disc' } as React.CSSProperties)}
              required
            />
          ) : (
            <input
              id={`${id}-value`}
              ref={second}
              className="field pr-10 font-mono"
              type={reveal ? 'text' : 'password'}
              value={value}
              onChange={(event) => setValue(event.target.value)}
              placeholder={said.valuePlaceholder}
              spellCheck={false}
              autoComplete="new-password"
              required
            />
          )}

          <button
            type="button"
            onClick={() => setReveal((was) => !was)}
            className="absolute right-2 top-2 rounded-full p-1 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
            title={reveal ? said.hide : said.show}
            aria-label={reveal ? said.hide : said.show}
            aria-pressed={reveal}
          >
            {reveal ? <IconEyeOff className="h-4 w-4" /> : <IconEye className="h-4 w-4" />}
          </button>
        </div>
      </div>

      {replaces && <p className="text-xs text-amber-700 dark:text-amber-400">{said.replaceNote}</p>}

      <div className="flex justify-end gap-2">
        <button type="button" className="btn-ghost !px-4 !py-1.5 text-[13px]" onClick={() => onDone(false)} disabled={saving}>
          {said.cancel}
        </button>
        <button type="submit" className="btn-primary !px-4 !py-1.5 text-[13px]" disabled={saving || !valid || value.length === 0}>
          {saving && <IconSpinner />}
          {said.save}
        </button>
      </div>
    </form>
  );
}
