import { useEffect, useState } from 'react';

import { ApiError, api, type OwnSource } from '../api';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconCheck, IconClose, IconExternal, IconScale, IconSpinner, IconStar } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT, useSourceT } from '../i18n';
import { Link } from '../i18n/links';
import type { Control } from './context';
import { Section, Switch } from './ui';

/**
 * Publishing the code of the lambda at /source, for anybody to read, star
 * and download.
 *
 * Off until the owner switches it on, like the showcase: nothing about
 * building a lambda publishes its code. Switching it on says, before anything
 * is published, what becomes public and what never does - the second list is
 * as important as the first - and asks under which license, MIT unless
 * another is chosen. Everything is in the words of the simple view, so both
 * views share it as it is.
 */
export function SourceTab({ control, onChanged }: { control: Control; onChanged?: () => void }) {
  const { privateKey, lambda, simple } = control;

  const t = useEditorT();
  const said = t.openSource;
  const licenses = useSourceT();
  const heading = t.frame.sections.source;

  const toast = useToast();

  const [state, setState] = useState<OwnSource | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  const [enabled, setEnabled] = useState(false);
  const [license, setLicense] = useState('MIT');
  const [author, setAuthor] = useState('');

  const [saving, setSaving] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [removing, setRemoving] = useState(false);

  useEffect(() => {
    let alive = true;

    api
      .source(privateKey)
      .then((own) => {
        if (!alive) {
          return;
        }

        setState(own);
        setEnabled(own.source?.published === true);
        setLicense(own.source?.license.id ?? own.default);
        setAuthor(own.source?.author ?? '');
      })
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.loadFailed));

    return () => {
      alive = false;
    };
  }, [privateKey, said]);

  if (failure) {
    return (
      <Section title={heading}>
        <p className="py-10 text-sm text-slate-500">{failure}</p>
      </Section>
    );
  }

  if (!state) {
    return (
      <Section title={heading}>
        <div className="flex items-center gap-2 py-10 text-sm text-slate-500">
          <IconSpinner /> {said.loading}
        </div>
      </Section>
    );
  }

  const source = state.source ?? null;
  const published = source?.published === true;

  const trimmed = author.trim();
  const changed = !published || license !== source?.license.id || trimmed !== (source?.author ?? '');
  const tooLong = trimmed.length > state.maxAuthor;

  async function save() {
    setSaving(true);

    try {
      const saved = await api.publishSource(privateKey, { license, author: trimmed });

      setState((was) => (was ? { ...was, source: saved } : was));
      setAuthor(saved.author ?? '');

      toast(published ? said.saved : said.published, 'success');
      onChanged?.();
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.saveFailed, 'error');
    } finally {
      setSaving(false);
    }
  }

  async function withdraw() {
    setRemoving(true);

    try {
      await api.withdrawSource(privateKey);

      setState((was) => (was && was.source ? { ...was, source: { ...was.source, published: false } } : was));
      setEnabled(false);
      setConfirming(false);

      toast(said.withdrawn);
      onChanged?.();
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.withdrawFailed, 'error');
    } finally {
      setRemoving(false);
    }
  }

  function toggle() {
    if (!enabled) {
      setEnabled(true);
    } else if (published) {
      // switching off what anybody can read takes it down, which is worth a question
      setConfirming(true);
    } else {
      setEnabled(false);
    }
  }

  const chosen = state.licenses.find((l) => l.id === license) ?? state.licenses[0];

  return (
    <Section
      title={heading}
      hint={simple ? said.hintSimple : said.hint(<code className="font-mono">open_source</code>)}
      actions={
        published ? (
          <Link to={source!.path} target="_blank" className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {said.open}
            <IconExternal className="h-3.5 w-3.5" />
          </Link>
        ) : undefined
      }
    >
      {/* ---------------------------------------------------------- the switch */}

      <div className="surface flex items-start gap-4 p-4">
        <div className="min-w-0 flex-1">
          <p id="source-switch" className="text-[15px] font-medium">{said.switch}</p>
          <p className="mt-1 text-[13px] text-slate-500">
            {published ? said.publishedNow(source!.license.id) : said.off}
            {!published && source != null && source.stars > 0 && ` ${said.keptStars(source.stars)}`}
          </p>
        </div>

        {published && (
          <span className="mt-0.5 inline-flex items-center gap-1 whitespace-nowrap text-[13px] tabular-nums text-slate-500" title={said.stars(source!.stars)}>
            <IconStar className="h-3.5 w-3.5" />
            {source!.stars}
          </span>
        )}

        <Switch on={enabled} onToggle={toggle} labelledBy="source-switch" />
      </div>

      {enabled && (
        <div className="mt-8 grid gap-10 lg:grid-cols-[minmax(0,1fr)_20rem]">
          <form
            className="space-y-7"
            onSubmit={(event) => {
              event.preventDefault();

              if (changed && !tooLong && !saving) {
                save();
              }
            }}
          >
            {/* ------------------------------------------------------ license */}

            <fieldset>
              <legend className="text-sm font-medium">{said.licenseLabel}</legend>
              <p className="mt-1 text-[13px] text-slate-500">{said.licenseHint}</p>

              <div role="radiogroup" aria-label={said.licenseLabel} className="mt-3 grid gap-2 sm:grid-cols-2">
                {state.licenses.map((item) => {
                  const on = item.id === license;

                  return (
                    <button
                      key={item.id}
                      type="button"
                      role="radio"
                      aria-checked={on}
                      onClick={() => setLicense(item.id)}
                      className={`flex flex-col items-start gap-1 border p-3 text-left transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent-500 ${
                        on
                          ? 'border-accent-500 bg-accent-500/[0.06] dark:border-accent-400'
                          : 'border-slate-200 hover:border-slate-300 dark:border-ink-800 dark:hover:border-ink-700'
                      }`}
                    >
                      <span className="flex w-full items-center gap-2">
                        <span className={`flex h-4 w-4 shrink-0 items-center justify-center rounded-full border ${on ? 'border-accent-500 bg-accent-500 dark:border-accent-400 dark:bg-accent-400' : 'border-slate-300 dark:border-ink-700'}`}>
                          {on && <span className="h-1.5 w-1.5 rounded-full bg-white dark:bg-ink-950" />}
                        </span>
                        <span className="text-sm font-medium">{item.name}</span>
                      </span>
                      <span className="pl-6 text-[11px] font-medium uppercase tracking-wide text-slate-400">{licenses.kinds[item.kind]}</span>
                      <span className="pl-6 text-[13px] leading-snug text-slate-600 dark:text-slate-400">
                        {(licenses.licenses as Record<string, string>)[item.id]}
                      </span>
                    </button>
                  );
                })}
              </div>

              <a href={chosen.url} target="_blank" rel="noreferrer" className="mt-2 inline-flex items-center gap-1.5 text-[13px] text-accent-600 hover:underline dark:text-accent-400">
                <IconScale className="h-3.5 w-3.5" />
                {said.readLicense}: {chosen.name}
              </a>
            </fieldset>

            {/* ------------------------------------------------------- author */}

            <div>
              <div className="mb-1.5 flex items-baseline justify-between gap-3 text-sm">
                <label htmlFor="source-author" className="font-medium">
                  {said.authorLabel} <span className="font-normal text-slate-400">· {said.optional}</span>
                </label>
                <span className={`text-xs tabular-nums ${tooLong ? 'text-red-500' : 'text-slate-400'}`}>
                  {trimmed.length} / {state.maxAuthor}
                </span>
              </div>
              <input
                id="source-author"
                value={author}
                onChange={(event) => setAuthor(event.target.value.replace(/[\r\n]/g, ' '))}
                placeholder={said.authorPlaceholder(lambda.publicKey)}
                className="field"
                autoComplete="name"
              />
              <p className="mt-1.5 text-[13px] text-slate-500">{said.authorHint}</p>
            </div>

            <p className="flex items-start gap-2 border-l-2 border-amber-500 pl-3 text-[13px] text-slate-600 dark:text-slate-400">
              <IconAlert className="mt-0.5 h-4 w-4 shrink-0 text-amber-500" />
              {said.careful}
            </p>

            <div className="flex flex-wrap items-center gap-3 border-t border-slate-200 pt-5 dark:border-ink-800">
              <button type="submit" className="btn-primary" disabled={!changed || tooLong || saving}>
                {saving && <IconSpinner />}
                {published ? said.save : said.publish}
              </button>

              {published && (
                <button type="button" className="btn-danger" onClick={() => setConfirming(true)}>
                  {said.takeDown}
                </button>
              )}

              {published && !changed && <span className="text-[13px] text-slate-500">{said.allSaved}</span>}
            </div>
          </form>

          {/* ------------------------------------------ what is, and what is not */}

          <aside className="space-y-6 lg:sticky lg:top-20 lg:self-start">
            <List title={said.whatTitle} items={said.what} good />
            <List title={said.neverTitle} items={said.never} />
          </aside>
        </div>
      )}

      <Dialog
        title={said.confirm}
        open={confirming}
        onClose={() => setConfirming(false)}
        footer={
          <>
            <button type="button" onClick={() => setConfirming(false)} className="btn-ghost">
              {said.keep}
            </button>
            <button type="button" onClick={withdraw} disabled={removing} className="btn-danger">
              {removing && <IconSpinner />}
              {said.takeDown}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">{said.confirmText}</p>
      </Dialog>
    </Section>
  );
}

function List({ title, items, good = false }: { title: string; items: string[]; good?: boolean }) {
  return (
    <div>
      <p className="text-xs font-medium uppercase tracking-[0.15em] text-slate-400">{title}</p>
      <ul className="mt-2.5 space-y-2 text-[13px] text-slate-700 dark:text-slate-300">
        {items.map((item) => (
          <li key={item} className="flex items-start gap-2">
            {good ? (
              <IconCheck className="mt-0.5 h-3.5 w-3.5 shrink-0 text-emerald-500" />
            ) : (
              <IconClose className="mt-0.5 h-3.5 w-3.5 shrink-0 text-red-500" />
            )}
            <span>{item}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
