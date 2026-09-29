import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, isDemo, type DataStore, type LambdaFile, type SecretListing, type WorkspaceListing } from '../api';
import { Dialog } from '../components/Dialog';
import { IconAlert, IconFolder, IconHistory, IconLayers, IconLock, IconSpinner, IconTrash, IconUpload } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import type { Control } from './context';
import { GroupList, Tree, Viewer, workspaceOf, type Selection } from './FileBrowser';
import { bytes } from './format';
import { SecretsPanel } from './SecretsPanel';
import { Exposure } from './SummaryTab';
import { Empty, Meter, Pills, Section, Switch } from './ui';

/** No files of a version are shown here; the viewer is handed none. */
const NO_FILES: LambdaFile[] = [];

/** What is read to show the data: the kinds there are, the files of the workspace, and the secrets by name. */
interface Kept {
  stores: DataStore[] | null;
  listing: WorkspaceListing | null;
  secrets: SecretListing | null;
  failure: string | null;
  reload: () => Promise<void>;
}

/**
 * Reads the data of the lambda - or, opened on a draft, of the draft's copy.
 * Every kind is read the same way here, so the two ways of showing them share
 * one place that asks.
 */
function useKept(control: Control): Kept {
  const said = useEditorT().data;

  const [stores, setStores] = useState<DataStore[] | null>(null);
  const [listing, setListing] = useState<WorkspaceListing | null>(null);
  const [secrets, setSecrets] = useState<SecretListing | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  const feature = control.feature?.info.key;
  const workspace = workspaceOf(control);

  const reload = useCallback(async () => {
    try {
      const [found, files, keys] = await Promise.all([
        feature ? api.feature.data(control.privateKey, feature) : api.data(control.privateKey),
        workspace.list(),
        api.secrets(control.privateKey, feature),
      ]);

      setStores(found);
      setListing(files);
      setSecrets(keys);
      setFailure(null);
    } catch (error) {
      setFailure(error instanceof ApiError ? error.message : said.readFailed);
    }
    // the accessor is made again with every render; the feature it reads from is what matters
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [control.privateKey, feature, said]);

  useEffect(() => {
    reload();
  }, [reload]);

  return { stores, listing, secrets, failure, reload };
}

/**
 * What the lambda keeps, as opposed to what it is.
 *
 * A version is the program and is replaced by the next one; data belongs to
 * the lambda, is shared by every version and outlives all of them. The page
 * says that first, in three lines, because it is what decides where anything
 * goes. Then the kinds of data there are, one row of pills - and one panel for
 * whichever is picked. Every kind has the same panel: what it is, whether it is
 * on, how full it is, and then what it holds, which is the only part that
 * differs - the files of the workspace, the names of the secrets.
 *
 * Opened on a feature, it shows the feature's copy instead - the test data
 * of a draft, to the owner: what the preview reads and writes, taken from
 * the lambda when the feature began and thrown away when it goes online.
 * That is said in a line rather than three, since it is all there is to know
 * about it. Which kinds there are is still the lambda's to switch, so there
 * are no switches here - only resetting the copy, for a preview that has
 * made a mess of it.
 *
 * In the simple view it is a page of what there is, for somebody who does not
 * think in kinds of data: see SimpleData below.
 */
export function DataTab({ control }: { control: Control }) {
  return control.simple && !control.feature ? <SimpleData control={control} /> : <FullData control={control} />;
}

/** The pill of a kind: its name, and whether it is on and what it holds. */
function KindLabel({ store, name }: { store: DataStore; name: string }) {
  const said = useEditorT().data;

  return (
    <span className="inline-flex items-center gap-2">
      {store.kind === 'secrets' ? <IconLock className="h-3.5 w-3.5" /> : <IconFolder className="h-3.5 w-3.5" />}
      {name}
      {store.enabled ? (
        store.items > 0 && <span className="text-xs tabular-nums text-slate-400">{store.items}</span>
      ) : (
        <span className="text-xs text-slate-400">{said.off}</span>
      )}
    </span>
  );
}

function FullData({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.data;
  const toast = useToast();

  const { stores, listing, secrets, failure, reload } = useKept(control);

  const [params, setParams] = useSearchParams();
  const [selected, setSelected] = useState<Selection | null>(null);
  const [switching, setSwitching] = useState<string | null>(null);
  const [confirming, setConfirming] = useState<DataStore | null>(null);
  const [recopying, setRecopying] = useState<'asking' | 'busy' | null>(null);

  // what a demo keeps is there to be read, not replaced
  const demo = isDemo(control.lambda.tier);

  const feature = control.feature?.info ?? null;

  const name = (kind: string) => said.kinds[kind]?.name ?? kind;

  const kind = params.get('kind');
  const store = stores?.find((s) => s.kind === kind) ?? stores?.[0] ?? null;

  async function toggle(target: DataStore, on: boolean) {
    setConfirming(null);
    setSwitching(target.kind);

    try {
      if (on) {
        await api.enableData(control.privateKey, target.kind);
        toast(said.switchedOn(name(target.kind)), 'success');
      } else {
        await api.disableData(control.privateKey, target.kind);
        setSelected(null);
        toast(said.switchedOff(name(target.kind)));
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
      pills={stores && stores.length > 1 && (
        <Pills
          label={said.kindsLabel}
          value={store?.kind ?? ''}
          onChange={(next) => {
            setSelected(null);
            setParams(next === stores[0].kind ? {} : { kind: next }, { replace: true });
          }}
          options={stores.map((s) => ({ value: s.kind, label: <KindLabel store={s} name={name(s.kind)} />, title: said.kinds[s.kind]?.what }))}
        />
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

      {stores === null && !failure && (
        <div className="mt-6 flex items-center gap-2 text-sm text-slate-500">
          <IconSpinner /> {t.files.reading}
        </div>
      )}

      {store && (
        <KindPanel
          key={store.kind}
          store={store}
          name={name(store.kind)}
          what={said.kinds[store.kind]?.what}
          publicly={store.kind === 'workspace' && !feature ? !!storage?.servesWorkspace : null}
          busy={switching === store.kind}
          readOnly={demo}
          draft={feature != null}
          onToggle={() => (store.enabled ? setConfirming(store) : toggle(store, true))}
        >
          {store.kind === 'secrets' ? (
            <SecretsPanel control={control} listing={secrets} readOnly={demo} onChanged={async () => { await Promise.all([reload(), control.refresh()]); }} />
          ) : (
            <WorkspaceDetails
              control={control}
              listing={listing}
              publicly={feature ? null : !!storage?.servesWorkspace}
              readOnly={demo}
              selected={selected?.group === 'data' ? selected.path : null}
              onSelect={(path) => setSelected(path ? { group: 'data', path } : null)}
              onChanged={reload}
              selection={selected}
            />
          )}
        </KindPanel>
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
                ? confirming.kind === 'secrets'
                  ? said.confirmItems(said.secretCount(confirming.items))
                  : said.confirmText(t.files.count(confirming.items), bytes(confirming.usedBytes))
                : said.confirmEmpty}
            </p>
            {((confirming.kind === 'workspace' && storage?.usesWorkspace) || (confirming.kind === 'secrets' && storage?.usesSecrets))
              && control.lambda.activeVersion != null && (
              <p className="mt-3 flex gap-2 text-amber-700 dark:text-amber-400">
                <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />
                {confirming.kind === 'secrets' ? said.inUseSecrets : said.inUse}
              </p>
            )}
          </>
        )}
      </Dialog>
    </Section>
  );
}

/**
 * One kind of data, the same for every kind: what it is for, whether it is
 * on and how full it is - and, below, what it holds, which is the part that
 * differs and is handed in.
 */
function KindPanel({ store, name, what, publicly, busy, readOnly, draft, onToggle, children }: {
  store: DataStore;
  name: string;
  what?: string;
  /** Whether the code online serves it; null where that says nothing, as for a copy or for kinds that cannot be served. */
  publicly: boolean | null;
  busy: boolean;
  readOnly: boolean;
  /** Whether this is a draft's copy, which has no switch: which kinds there are is the lambda's. */
  draft: boolean;
  onToggle: () => void;
  children: ReactNode;
}) {
  const t = useEditorT();
  const said = t.data;
  const id = `data-${store.kind}`;
  const secrets = store.kind === 'secrets';

  return (
    <section aria-labelledby={id} className="mt-6">
      <div className={`surface flex items-start gap-4 p-4 ${store.enabled ? '' : 'bg-slate-50 dark:bg-ink-900'}`}>
        <span
          className={`mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center ${
            store.enabled ? 'bg-accent-500/10 text-accent-600 dark:text-accent-400' : 'bg-slate-400/10 text-slate-400'
          }`}
        >
          {secrets ? <IconLock className="h-[18px] w-[18px]" /> : store.kind === 'workspace' ? <IconFolder className="h-[18px] w-[18px]" /> : <IconLayers className="h-[18px] w-[18px]" />}
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

          {store.enabled && (
            <div className="mt-3 max-w-lg">
              {secrets ? (
                <Meter label={said.secretCount(store.items)} used={store.items} of={store.limit} format={(n) => String(n)} />
              ) : (
                <Meter label={t.files.count(store.items)} used={store.usedBytes} of={store.quotaBytes} format={bytes} />
              )}
            </div>
          )}
        </div>

        {!readOnly && !draft && (
          <div className="flex shrink-0 items-center gap-2">
            {busy && <IconSpinner className="h-4 w-4 text-slate-400" />}
            <span className="sr-only">{said.switchLabel(name)}</span>
            <Switch on={store.enabled} onToggle={() => !busy && onToggle()} labelledBy={id} />
          </div>
        )}
      </div>

      <div className="mt-6">
        {store.enabled ? (
          children
        ) : (
          <div className="surface flex flex-col items-center px-6 py-10 text-center">
            <span className="flex h-11 w-11 items-center justify-center bg-slate-400/10 text-slate-400">
              {secrets ? <IconLock className="h-5 w-5" /> : <IconFolder className="h-5 w-5" />}
            </span>
            <p className="mt-3 max-w-md text-sm text-slate-600 dark:text-slate-400">
              {draft ? said.offCopy : secrets ? said.offSecrets : said.offText}
            </p>
            {!readOnly && !draft && (
              <button type="button" className="btn-primary mt-5 !px-4 !py-1.5 text-[13px]" onClick={onToggle} disabled={busy}>
                {busy && <IconSpinner />}
                {said.switchOn}
              </button>
            )}
          </div>
        )}
      </div>
    </section>
  );
}

/** The files of the workspace, and a viewer for the one picked. */
function WorkspaceDetails({ control, listing, publicly, readOnly, selected, selection, onSelect, onChanged }: {
  control: Control;
  listing: WorkspaceListing | null;
  publicly: boolean | null;
  readOnly: boolean;
  selected: string | null;
  selection: Selection | null;
  onSelect: (path: string | null) => void;
  onChanged: () => Promise<void>;
}) {
  const t = useEditorT();
  const said = t.data;

  return (
    <div>
      <h3 className="mb-3 flex items-center gap-2 text-sm font-medium">
        <IconFolder className="h-4 w-4 text-slate-400" />
        {control.feature ? said.copyContents : said.contents}
      </h3>

      <div className="grid gap-5 lg:grid-cols-[17rem,1fr]">
        <nav aria-label={said.browse} className="lg:max-h-[40rem] lg:overflow-y-auto">
          <WorkspaceFiles
            control={control}
            listing={listing}
            publicly={publicly}
            readOnly={readOnly}
            selected={selected}
            onSelect={onSelect}
            onChanged={onChanged}
          />
        </nav>

        <Viewer control={control} selection={selection} files={NO_FILES} listing={listing} />
      </div>
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

/**
 * The data of the app for somebody who had it built: what there is, and only
 * what there is. A kind that holds nothing is not shown - whether it is on
 * is not something they have to think about - and each kind that does is a
 * card with what it is in plain words, how much of it there is and what can
 * be done: replacing a key, removing it, or deleting what the app saved.
 * Nothing says file, workspace, secret or code.
 */
function SimpleData({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.data;
  const words = said.simple;
  const toast = useToast();

  const { stores, secrets, failure, reload } = useKept(control);

  const [clearing, setClearing] = useState<'asking' | 'busy' | null>(null);

  const demo = isDemo(control.lambda.tier);

  const held = (stores ?? []).filter((s) => s.enabled && s.items > 0);

  const saved = held.find((s) => s.kind === 'workspace');

  /** Deleting what the app saved, and leaving the app with a workspace of its own again, empty. */
  async function clear() {
    setClearing('busy');

    try {
      await api.disableData(control.privateKey, 'workspace');
      await api.enableData(control.privateKey, 'workspace');

      toast(words.deleted, 'success');
      setClearing(null);

      await Promise.all([reload(), control.refresh()]);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : words.deleteFailed, 'error');
      setClearing('asking');
    }
  }

  return (
    <Section title={t.frame.sections.data} hint={words.hint}>
      {failure && <p className="text-sm text-red-500">{failure}</p>}

      {stores === null && !failure && (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <IconSpinner /> {t.files.reading}
        </div>
      )}

      {stores !== null && held.length === 0 && <Empty>{words.none}</Empty>}

      <div className="space-y-6">
        {held.map((store) => {
          const secretsKind = store.kind === 'secrets';
          const kind = words.kinds[store.kind];

          return (
            <section key={store.kind} aria-labelledby={`simple-${store.kind}`} className="space-y-4">
              <div className="surface flex flex-wrap items-start gap-x-4 gap-y-3 p-4">
                <span className="mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center bg-accent-500/10 text-accent-600 dark:text-accent-400">
                  {secretsKind ? <IconLock className="h-[18px] w-[18px]" /> : <IconFolder className="h-[18px] w-[18px]" />}
                </span>

                <div className="min-w-0 flex-1 basis-48">
                  <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
                    <h2 id={`simple-${store.kind}`} className="text-[15px] font-medium">{kind?.name ?? store.kind}</h2>
                    <span className="text-[13px] tabular-nums text-slate-500">
                      {secretsKind ? words.keys(store.items) : `${words.items(store.items)} · ${bytes(store.usedBytes)}`}
                    </span>
                  </div>
                  {kind && <p className="mt-1 text-[13px] text-slate-500">{kind.what}</p>}
                </div>

                {!secretsKind && !demo && (
                  <button type="button" className="btn-danger !px-3 !py-1.5 text-[13px]" onClick={() => setClearing('asking')}>
                    <IconTrash className="h-3.5 w-3.5" />
                    {words.deleteAll}
                  </button>
                )}
              </div>

              {secretsKind && <SecretsPanel control={control} listing={secrets} readOnly={demo} onChanged={async () => { await Promise.all([reload(), control.refresh()]); }} />}
            </section>
          );
        })}
      </div>

      <Dialog
        title={words.deleteTitle}
        open={clearing !== null}
        onClose={() => clearing !== 'busy' && setClearing(null)}
        footer={
          <>
            <button type="button" onClick={() => setClearing(null)} className="btn-ghost" disabled={clearing === 'busy'}>
              {said.keep}
            </button>
            <button type="button" className="btn-danger" onClick={clear} disabled={clearing === 'busy'}>
              {clearing === 'busy' && <IconSpinner />}
              {words.deleteConfirm}
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">
          {words.deleteText(words.items(saved?.items ?? 0), bytes(saved?.usedBytes ?? 0))}
        </p>
      </Dialog>
    </Section>
  );
}
