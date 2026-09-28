import { useCallback, useEffect, useRef, useState } from 'react';

import { ApiError, api, isDemo, type DataStore, type LambdaFile, type WorkspaceListing } from '../api';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconFolder, IconHistory, IconLayers, IconSpinner, IconTrash, IconUpload } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { GroupList, Tree, Viewer, workspaceOf, type Selection } from './FileBrowser';
import { bytes } from './format';
import { Exposure } from './SummaryTab';
import { Meter, Section, Switch } from './ui';

/** No files of a version are shown here; the viewer is handed none. */
const NO_FILES: LambdaFile[] = [];

/**
 * What the lambda keeps, as opposed to what it is.
 *
 * A version is the program and is replaced by the next one; data belongs to
 * the lambda, is shared by every version and outlives all of them. The page
 * says that first, in three lines, because it is what decides where anything
 * goes. Then the kinds of data there are, each switched on or off by the
 * owner - only the workspace so far, listed the way every kind will be - and
 * then what the workspace holds.
 *
 * Opened on a feature, it shows the feature's copy instead - the test data
 * of a draft, to the owner: what the preview reads and writes, taken from
 * the lambda when the feature began and thrown away when it goes online.
 * That is said in a line rather than three, since it is all there is to know
 * about it. Which kinds there are is still the lambda's to switch, so there
 * are no switches here - only resetting the copy, for a preview that has
 * made a mess of it.
 */
export function DataTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.data;
  const toast = useToast();

  const [stores, setStores] = useState<DataStore[] | null>(null);
  const [listing, setListing] = useState<WorkspaceListing | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [selected, setSelected] = useState<Selection | null>(null);
  const [switching, setSwitching] = useState<string | null>(null);
  const [confirming, setConfirming] = useState<DataStore | null>(null);
  const [recopying, setRecopying] = useState<'asking' | 'busy' | null>(null);

  // what a demo keeps is there to be read, not replaced
  const demo = isDemo(control.lambda.tier);

  const feature = control.feature?.info ?? null;
  const workspace = workspaceOf(control);

  const reload = useCallback(async () => {
    try {
      const [found, files] = await Promise.all([
        feature ? api.feature.data(control.privateKey, feature.key) : api.data(control.privateKey),
        workspace.list(),
      ]);

      setStores(found);
      setListing(files);
      setFailure(null);
    } catch (error) {
      setFailure(error instanceof ApiError ? error.message : said.readFailed);
    }
    // the accessor is made again with every render; the feature it reads from is what matters
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [control.privateKey, feature?.key, said]);

  useEffect(() => {
    reload();
  }, [reload]);

  const name = (kind: string) => said.kinds[kind]?.name ?? kind;

  async function toggle(store: DataStore, on: boolean) {
    setConfirming(null);
    setSwitching(store.kind);

    try {
      if (on) {
        await api.enableData(control.privateKey, store.kind);
        toast(said.switchedOn(name(store.kind)), 'success');
      } else {
        await api.disableData(control.privateKey, store.kind);
        setSelected(null);
        toast(said.switchedOff(name(store.kind)));
      }

      await Promise.all([reload(), control.refresh()]);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.switchFailed, 'error');
    } finally {
      setSwitching(null);
    }
  }

  /** Throws the feature's copy away and copies the lambda's data again. */
  async function recopy() {
    if (!feature) {
      return;
    }

    setRecopying('busy');

    try {
      await api.feature.refresh(control.privateKey, feature.key);
      setSelected(null);
      toast(said.recopied, 'success');

      await Promise.all([reload(), control.feature?.refresh()]);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.recopyFailed, 'error');
    } finally {
      setRecopying(null);
    }
  }

  const storage = control.summary?.storage;
  const kept = stores?.find((s) => s.kind === 'workspace');

  return (
    <Section
      title={feature ? t.features.views.data : t.frame.sections.data}
      hint={feature ? undefined : said.hint}
      actions={feature && (
        <button
          type="button"
          onClick={() => setRecopying('asking')}
          disabled={recopying !== null}
          className="btn-ghost !px-3 !py-1.5 text-[13px]"
          title={said.recopyTitle}
        >
          {recopying === 'busy' ? <IconSpinner /> : <IconHistory className="h-3.5 w-3.5" />}
          {said.recopy}
        </button>
      )}
    >
      {demo && <p className="-mt-1 mb-4 text-[13px] text-slate-500">{said.demo}</p>}

      {feature ? (
        <p className="max-w-2xl text-[13px] text-slate-600 dark:text-slate-400">{said.featureHint}</p>
      ) : (
        <dl className="grid gap-4 sm:grid-cols-3">
          {said.facts.map(([title, text]) => (
            <div key={title} className="border-l-2 border-accent-500/40 pl-3 dark:border-accent-400/40">
              <dt className="text-[13px] font-medium">{title}</dt>
              <dd className="mt-0.5 text-[13px] text-slate-500">{text}</dd>
            </div>
          ))}
        </dl>
      )}

      {failure && <p className="mt-6 text-sm text-red-500">{failure}</p>}

      {stores === null && !failure ? (
        <div className="mt-6 flex items-center gap-2 text-sm text-slate-500">
          <IconSpinner /> {t.files.reading}
        </div>
      ) : !feature && (
        // which kinds of data there are is the lambda's to switch, so a draft does not list them
        <ul className="mt-6 space-y-3">
          {(stores ?? []).map((store) => (
            <li key={store.kind}>
              <Store
                store={store}
                name={name(store.kind)}
                what={said.kinds[store.kind]?.what}
                publicly={store.kind === 'workspace' && !!storage?.servesWorkspace}
                busy={switching === store.kind}
                readOnly={demo}
                onToggle={() => (store.enabled ? setConfirming(store) : toggle(store, true))}
              />
            </li>
          ))}
        </ul>
      )}

      {kept && (
        <div className="mt-8">
          <h2 className="mb-3 flex items-center gap-2 text-sm font-medium">
            <IconFolder className="h-4 w-4 text-slate-400" />
            {feature ? said.copyContents : said.contents}
          </h2>

          {kept.enabled ? (
            <div className="grid gap-5 lg:grid-cols-[17rem,1fr]">
              <nav aria-label={said.browse} className="lg:max-h-[40rem] lg:overflow-y-auto">
                <WorkspaceFiles
                  control={control}
                  listing={listing}
                  publicly={feature ? null : !!storage?.servesWorkspace}
                  readOnly={demo}
                  selected={selected?.group === 'data' ? selected.path : null}
                  onSelect={(path) => setSelected(path ? { group: 'data', path } : null)}
                  onChanged={reload}
                />
              </nav>

              <Viewer control={control} selection={selected} files={NO_FILES} listing={listing} />
            </div>
          ) : (
            <p className="surface p-6 text-center text-sm text-slate-500">{said.offBrowse}</p>
          )}
        </div>
      )}

      <Dialog
        title={said.recopyConfirm}
        open={recopying === 'asking'}
        onClose={() => setRecopying(null)}
        footer={
          <>
            <button type="button" onClick={() => setRecopying(null)} className="btn-ghost">
              {said.keepCopy}
            </button>
            <button type="button" className="btn-primary" onClick={recopy}>
              {said.recopy}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">{said.recopyText}</p>
      </Dialog>

      <Dialog
        title={confirming ? said.confirmOff(name(confirming.kind)) : ''}
        open={confirming !== null}
        onClose={() => setConfirming(null)}
        footer={
          <>
            <button type="button" onClick={() => setConfirming(null)} className="btn-ghost">
              {said.keep}
            </button>
            <button type="button" className="btn-danger" onClick={() => confirming && toggle(confirming, false)}>
              {said.deleteAndOff}
            </button>
          </>
        }
      >
        {confirming && (
          <>
            <p className="text-slate-600 dark:text-slate-400">
              {confirming.items > 0 || confirming.usedBytes > 0
                ? said.confirmText(t.files.count(confirming.items), bytes(confirming.usedBytes))
                : said.confirmEmpty}
            </p>
            {confirming.kind === 'workspace' && storage?.usesWorkspace && control.lambda.activeVersion != null && (
              <p className="flex gap-2 text-amber-700 dark:text-amber-400">
                <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
                {said.inUse}
              </p>
            )}
          </>
        )}
      </Dialog>
    </Section>
  );
}

/** One kind of data: what it is for, whether it is on, and how full it is. */
function Store({ store, name, what, publicly, busy, readOnly, onToggle }: {
  store: DataStore;
  name: string;
  what?: string;
  /** Whether the code online serves it; null where that says nothing, as for the copy of a feature. */
  publicly: boolean | null;
  busy: boolean;
  readOnly: boolean;
  onToggle: () => void;
}) {
  const t = useEditorT();
  const said = t.data;
  const id = `data-${store.kind}`;

  return (
    <div className={`surface flex items-start gap-4 p-4 ${store.enabled ? '' : 'bg-slate-50 dark:bg-ink-900'}`}>
      <span
        className={`mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center ${
          store.enabled ? 'bg-accent-500/10 text-accent-600 dark:text-accent-400' : 'bg-slate-400/10 text-slate-400'
        }`}
      >
        {store.kind === 'workspace' ? <IconFolder className="h-[18px] w-[18px]" /> : <IconLayers className="h-[18px] w-[18px]" />}
      </span>

      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-center gap-2">
          <h2 id={id} className="text-[15px] font-medium">{name}</h2>
          {store.enabled && publicly !== null && (
            <Exposure open={publicly} why={publicly ? t.files.dataPublic : t.files.dataPrivate} />
          )}
          <span
            className={`rounded-full px-2 py-0.5 text-[11px] font-medium ${
              store.enabled ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400' : 'bg-slate-400/10 text-slate-500'
            }`}
          >
            {store.enabled ? said.on : said.off}
          </span>
          {store.default && !store.changed && store.enabled && (
            <span className="text-xs text-slate-400">{said.byDefault}</span>
          )}
        </div>

        {what && <p className="mt-1 text-[13px] text-slate-500">{what}</p>}

        {store.enabled ? (
          <div className="mt-3 max-w-lg">
            <Meter label={t.files.count(store.items)} used={store.usedBytes} of={store.quotaBytes} format={bytes} />
          </div>
        ) : (
          <p className="mt-2 text-[13px] text-slate-600 dark:text-slate-400">{said.offText}</p>
        )}
      </div>

      {!readOnly && (
        <div className="flex shrink-0 items-center gap-2">
          {busy && <IconSpinner className="h-4 w-4 text-slate-400" />}
          <span className="sr-only">{said.switchLabel(name)}</span>
          <Switch on={store.enabled} onToggle={() => !busy && onToggle()} labelledBy={id} />
        </div>
      )}
    </div>
  );
}

/** The files of the workspace, with uploading into it and deleting from it. */
function WorkspaceFiles({ control, listing, publicly, readOnly, selected, onSelect, onChanged }: {
  control: Control;
  listing: WorkspaceListing | null;
  publicly: boolean | null;
  readOnly: boolean;
  selected: string | null;
  onSelect: (path: string | null) => void;
  onChanged: () => Promise<void>;
}) {
  const t = useEditorT();
  const said = t.files;
  const toast = useToast();
  const picker = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);

  const workspace = workspaceOf(control);

  // uploads land in the folder of whatever is selected, or at the top
  const into = selected
    ? listing?.folders.includes(selected)
      ? selected
      : selected.includes('/') ? selected.slice(0, selected.lastIndexOf('/')) : ''
    : '';

  async function upload(chosen: FileList | null) {
    if (!chosen || chosen.length === 0) {
      return;
    }

    setBusy(true);

    for (const file of Array.from(chosen)) {
      const path = into ? `${into}/${file.name}` : file.name;

      try {
        await workspace.upload(path, file);
      } catch (error) {
        toast(error instanceof ApiError ? error.message : said.uploadFailed(path), 'error');
      }
    }

    if (picker.current) {
      picker.current.value = '';
    }

    await onChanged();
    setBusy(false);
  }

  async function remove(path: string, folder: boolean) {
    const held = listing?.files.filter((f) => f.path.startsWith(`${path}/`)).length ?? 0;

    const question = folder ? said.deleteFolder(path, held) : said.deleteFile(path);

    if (!window.confirm(question)) {
      return;
    }

    try {
      await workspace.remove(path);

      if (selected === path || selected?.startsWith(`${path}/`)) {
        onSelect(null);
      }

      await onChanged();
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.deleteFailed, 'error');
    }
  }

  const full = listing !== null && listing.usedBytes >= listing.quotaBytes;

  return (
    <GroupList
      title={t.data.browse}
      exposure={publicly !== null && <Exposure open={publicly} why={publicly ? said.dataPublic : said.dataPrivate} />}
      usage={listing ? said.usage(said.count(listing.files.length), bytes(listing.usedBytes), bytes(listing.quotaBytes)) : undefined}
      action={readOnly ? undefined : (
        <>
          <input ref={picker} type="file" multiple className="hidden" onChange={(event) => upload(event.target.files)} />
          <button
            type="button"
            onClick={() => picker.current?.click()}
            disabled={busy || listing === null || full}
            className="rounded-full p-1 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500 disabled:opacity-40"
            title={full ? said.full : into ? said.uploadInto(into) : said.upload}
            aria-label={said.upload}
          >
            {busy ? <IconSpinner className="h-3.5 w-3.5" /> : <IconUpload className="h-3.5 w-3.5" />}
          </button>
        </>
      )}
    >
      {listing === null ? (
        <div className="flex items-center gap-2 px-1 text-[13px] text-slate-500"><IconSpinner className="h-3.5 w-3.5" /> {said.reading}</div>
      ) : (
        <Tree
          entries={listing.files}
          folders={listing.folders}
          selected={selected}
          onSelect={onSelect}
          empty={said.noData}
          action={readOnly ? undefined : (node) => (
            <button
              type="button"
              onClick={() => remove(node.path, node.folder)}
              className="p-0.5 text-slate-400 hover:text-red-500"
              aria-label={said.delete(node.path)}
              title={said.deleteShort}
            >
              <IconTrash className="h-3.5 w-3.5" />
            </button>
          )}
        />
      )}
    </GroupList>
  );
}
