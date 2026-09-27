import { useEffect, useRef, useState, type DragEvent } from 'react';

import { shownAddress } from '../address';
import { ApiError, api, type OwnShowcase } from '../api';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconExternal, IconSpinner, IconUpload } from '../components/Icons';
import { ShowcaseCard } from '../components/ShowcaseCard';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import { Link } from '../i18n/links';
import type { Control } from './context';
import { bytes } from './format';
import { Section, Switch } from './ui';

/** A picture chosen here and not saved yet. */
interface Picked {
  /** For the preview. */
  url: string;
  /** For the server, without the data prefix. */
  base64: string;
  name: string;
  size: number;
}

/**
 * Putting the lambda on the showcase page.
 *
 * Nothing here is part of making the lambda work, so it has a section of its
 * own and nothing else in the control center depends on it. It is off until
 * the owner switches it on; the form sits beside the card it produces, so
 * what is written is read the way a visitor will read it.
 */
export function ShowcaseTab({ control }: { control: Control }) {
  const { privateKey, lambda } = control;

  const t = useEditorT();
  const said = t.showcase;
  const heading = t.frame.sections.showcase;

  const toast = useToast();

  const [state, setState] = useState<OwnShowcase | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  const [enabled, setEnabled] = useState(false);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [picked, setPicked] = useState<Picked | null>(null);
  const [pictureError, setPictureError] = useState<string | null>(null);

  const [saving, setSaving] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [removing, setRemoving] = useState(false);

  useEffect(() => {
    let alive = true;

    api
      .showcase(privateKey)
      .then((own) => {
        if (!alive) {
          return;
        }

        setState(own);
        setEnabled(own.showcase != null);
        setTitle(own.showcase?.title ?? '');
        setDescription(own.showcase?.description ?? '');
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

  const { limits } = state;
  const entry = state.showcase ?? null;
  const live = lambda.activeVersion != null;

  const trimmedTitle = title.trim();
  const trimmedDescription = description.trim();

  const changed = entry === null
    || picked !== null
    || trimmedTitle !== entry.title
    || trimmedDescription !== entry.description;

  const missing = [
    trimmedTitle === '' && said.title,
    trimmedDescription === '' && said.description,
    entry === null && picked === null && said.picture,
  ].filter(Boolean) as string[];

  const tooLong = title.length > limits.title || description.length > limits.description;
  const ready = missing.length === 0 && !tooLong && changed;

  async function save() {
    setSaving(true);

    try {
      const saved = await api.saveShowcase(privateKey, {
        title: trimmedTitle,
        description: trimmedDescription,
        image: picked?.base64,
      });

      setState((was) => (was ? { ...was, showcase: saved } : was));
      setTitle(saved.title);
      setDescription(saved.description);
      setPicked(null);

      toast(entry ? said.updated : live ? said.listed : said.waiting, 'success');
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.saveFailed, 'error');
    } finally {
      setSaving(false);
    }
  }

  async function remove() {
    setRemoving(true);

    try {
      await api.removeShowcase(privateKey);

      setState((was) => (was ? { ...was, showcase: null } : was));
      setEnabled(false);
      setPicked(null);
      setConfirming(false);

      toast(said.removed);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.removeFailed, 'error');
    } finally {
      setRemoving(false);
    }
  }

  function toggle() {
    if (!enabled) {
      setEnabled(true);
    } else if (entry) {
      // switching off something that is listed takes it down, which is worth a question
      setConfirming(true);
    } else {
      setEnabled(false);
      setPicked(null);
    }
  }

  function choose(file: File | undefined) {
    setPictureError(null);

    if (!file) {
      return;
    }

    if (!limits.imageTypes.includes(file.type)) {
      setPictureError(said.wrongType);
      return;
    }

    if (file.size > limits.imageBytes) {
      setPictureError(said.tooLarge(bytes(file.size), bytes(limits.imageBytes)));
      return;
    }

    const reader = new FileReader();

    reader.onload = () => {
      const url = String(reader.result);
      setPicked({ url, base64: url.slice(url.indexOf(',') + 1), name: file.name, size: file.size });
    };

    reader.onerror = () => setPictureError(said.unreadable);

    reader.readAsDataURL(file);
  }

  const preview = picked?.url ?? entry?.imagePath ?? null;

  return (
    <Section
      title={heading}
      hint={said.hint(<code className="font-mono">showcase</code>)}
      actions={
        entry && live ? (
          <Link to="/showcase" target="_blank" className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {said.open}
            <IconExternal className="h-3.5 w-3.5" />
          </Link>
        ) : undefined
      }
    >
      {/* ---------------------------------------------------------- the switch */}

      <div className="surface flex items-start gap-4 p-4">
        <div className="min-w-0 flex-1">
          <p id="showcase-switch" className="text-[15px] font-medium">{said.switch}</p>
          <p className="mt-1 text-[13px] text-slate-500">{entry ? (live ? said.listedNow : said.notListed) : said.off}</p>
        </div>

        <Switch on={enabled} onToggle={toggle} labelledBy="showcase-switch" />
      </div>

      {enabled && !live && (
        <p className="mt-4 flex items-start gap-2 border-l-2 border-amber-500 pl-3 text-[13px] text-slate-600 dark:text-slate-400">
          <IconAlert className="mt-0.5 h-4 w-4 shrink-0 text-amber-500" />
          {said.offline}
        </p>
      )}

      {/* ------------------------------------------------------------ the form */}

      {enabled && (
        <div className="mt-8 grid gap-10 lg:grid-cols-[minmax(0,1fr)_20rem]">
          <form
            className="space-y-6"
            onSubmit={(event) => {
              event.preventDefault();
              if (ready && !saving) {
                save();
              }
            }}
          >
            <Field label={said.titleLabel} used={title.length} of={limits.title} htmlFor="showcase-title">
              <input
                id="showcase-title"
                value={title}
                onChange={(event) => setTitle(event.target.value.replace(/\n/g, ' '))}
                placeholder={said.titlePlaceholder}
                className="field"
                autoComplete="off"
              />
            </Field>

            <Field label={said.descriptionLabel} used={description.length} of={limits.description} htmlFor="showcase-description">
              <textarea
                id="showcase-description"
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                rows={4}
                placeholder={said.descriptionPlaceholder}
                className="field resize-y leading-relaxed"
              />
            </Field>

            <Picture
              current={entry?.imagePath ?? null}
              picked={picked}
              error={pictureError}
              limit={limits.imageBytes}
              onChoose={choose}
              onReset={() => {
                setPicked(null);
                setPictureError(null);
              }}
            />

            <div className="flex flex-wrap items-center gap-3 border-t border-slate-200 pt-5 dark:border-ink-800">
              <button type="submit" className="btn-primary" disabled={!ready || saving}>
                {saving && <IconSpinner />}
                {entry ? said.save : said.add}
              </button>

              {entry && (
                <button type="button" className="btn-danger" onClick={() => setConfirming(true)}>
                  {said.takeOff}
                </button>
              )}

              <span className="text-[13px] text-slate-500">
                {missing.length > 0 ? said.needs(missing) : tooLong ? said.tooLong : !changed ? said.allSaved : ''}
              </span>
            </div>
          </form>

          <aside className="lg:sticky lg:top-20 lg:self-start">
            <p className="mb-3 text-xs font-medium uppercase tracking-[0.2em] text-slate-400">{said.preview}</p>
            <ShowcaseCard
              title={trimmedTitle}
              description={trimmedDescription}
              address={lambda.address}
              image={preview}
            />
            <p className="mt-3 text-[13px] text-slate-500">
              {said.card(<span className="font-mono">{shownAddress(lambda.address)}</span>)}
            </p>
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
            <button type="button" onClick={remove} disabled={removing} className="btn-danger">
              {removing && <IconSpinner />}
              {said.takeOff}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">{said.confirmText}</p>
      </Dialog>
    </Section>
  );
}

function Field({ label, used, of, htmlFor, children }: {
  label: string;
  used: number;
  of: number;
  htmlFor: string;
  children: React.ReactNode;
}) {
  const over = used > of;

  return (
    <div>
      <div className="mb-1.5 flex items-baseline justify-between gap-3 text-sm">
        <label htmlFor={htmlFor} className="font-medium">{label}</label>
        <span className={`text-xs tabular-nums ${over ? 'text-red-500' : used > of * 0.9 ? 'text-amber-600 dark:text-amber-400' : 'text-slate-400'}`}>
          {used} / {of}
        </span>
      </div>
      {children}
    </div>
  );
}

/**
 * Where the picture goes: dropped, or chosen from a dialog. A short recording
 * as a GIF shows a lambda best, so that is what the hint suggests.
 */
function Picture({ current, picked, error, limit, onChoose, onReset }: {
  current: string | null;
  picked: Picked | null;
  error: string | null;
  limit: number;
  onChoose: (file: File | undefined) => void;
  onReset: () => void;
}) {
  const said = useEditorT().showcase;
  const input = useRef<HTMLInputElement>(null);
  const [over, setOver] = useState(false);

  const drop = (event: DragEvent) => {
    event.preventDefault();
    setOver(false);
    onChoose(event.dataTransfer.files[0]);
  };

  const shown = picked?.url ?? current;

  return (
    <div>
      <div className="mb-1.5 flex items-baseline justify-between gap-3 text-sm">
        <span className="font-medium">{said.pictureLabel}</span>
        <span className="text-xs text-slate-400">{said.formats(bytes(limit))}</span>
      </div>

      <div
        onDragOver={(event) => {
          event.preventDefault();
          setOver(true);
        }}
        onDragLeave={() => setOver(false)}
        onDrop={drop}
        className={`flex items-center gap-4 border border-dashed p-3 transition-colors ${
          over ? 'border-accent-500 bg-accent-500/5' : 'border-grey-300 dark:border-ink-700'
        }`}
      >
        <div className="aspect-[16/10] w-32 shrink-0 overflow-hidden bg-grey-100 dark:bg-ink-850 sm:w-40">
          {shown && <img src={shown} alt="" className="h-full w-full object-cover" />}
        </div>

        <div className="min-w-0 flex-1 text-[13px]">
          {picked ? (
            <p className="truncate" title={picked.name}>
              {picked.name} <span className="text-slate-500">· {bytes(picked.size)} · {said.notSaved}</span>
            </p>
          ) : (
            <p className="text-slate-600 dark:text-slate-400">
              {current ? said.replace : said.drop} {said.advice}
            </p>
          )}

          <div className="mt-2 flex flex-wrap gap-1.5">
            <button type="button" className="btn-ghost !px-3 !py-1 text-[13px]" onClick={() => input.current?.click()}>
              <IconUpload className="h-3.5 w-3.5" />
              {shown ? said.another : said.choose}
            </button>
            {picked && (
              <button type="button" className="btn-ghost !px-3 !py-1 text-[13px]" onClick={onReset}>
                {current ? said.keepSaved : said.clear}
              </button>
            )}
          </div>
        </div>

        <input
          ref={input}
          type="file"
          accept="image/png,image/jpeg,image/gif,image/webp"
          className="hidden"
          onChange={(event) => {
            onChoose(event.target.files?.[0]);
            event.target.value = '';
          }}
        />
      </div>

      {error && <p className="mt-2 text-xs text-red-500">{error}</p>}
    </div>
  );
}
