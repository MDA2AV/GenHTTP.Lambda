import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react';

import { ApiError, api, isDemo, type DataStore, type LambdaFile, type SecretListing, type WorkspaceListing } from '@genhttp-lambda/kit/api';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconAlert, IconDatabase, IconDownload, IconFolder, IconHistory, IconKey, IconLayers, IconSpinner, IconTrash, IconUpload } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { useEditorT } from '../i18n';
import type { Control, DataKind } from './context';
import { DatabasePanel } from './DatabasePanel';
import { GroupList, Tree, Viewer, workspaceOf, type Selection } from './FileBrowser';
import { bytes } from '@genhttp-lambda/kit/ui/format';
import { SecretsPanel } from './SecretsPanel';
import { Exposure } from './SummaryTab';
import { Ago, Meter, Section, Switch } from '@genhttp-lambda/kit/ui/ui';

/** No files of a version are shown here; the viewer is handed none. */
const NO_FILES: LambdaFile[] = [];

/** How a kind of data is drawn wherever it is named. */
const ICONS: Record<string, (props: { className?: string }) => ReactNode> = {
  database: IconDatabase,
  workspace: IconFolder,
  secrets: IconKey,
};

/**
 * What a kind of data is called in a view: its own name in the full view,
 * and in the simple view what it is to somebody who does not know what a
 * database or a workspace is - records, what the app saved, keys and
 * passwords. The sidebar names the sections the same way.
 */
export function useKindName() {
  const said = useEditorT().data;

  return (kind: DataKind, simple: boolean) => (simple ? said.simple[kind] : said.kinds[kind]?.name ?? kind);
}

/**
 * One kind of what the lambda keeps, as opposed to what it is: its database,
 * its workspace or its secrets, each a section of its own.
 *
 * A version is the program and is replaced by the next one; data belongs to
 * the lambda, is shared by every version and outlives all of them. The ⓘ
 * beside the title says so, and the group the sections are in - the program
 * and its data - puts them beside the versions that share them.
 *
 * Every kind is drawn the same way - what it is, whether it is on and how
 * full it is, with its switch - and only what it holds is its own: its
 * tables for the database, a tree of files for the workspace, a list of
 * names for the secrets. So a kind added later looks like the others.
 *
 * Opened on a feature, it shows the feature's copy instead - the test data
 * of a draft, to the owner: what the preview reads and writes, taken from
 * the lambda when the feature began and thrown away when it goes online.
 * Which kinds there are is still the lambda's to switch, so there is no
 * switch here - only resetting the copy, for a preview that made a mess,
 * which resets every kind of it at once, as it was copied.
 *
 * The simple view has the section only once the kind holds something, in
 * words that do not assume anybody knows what a database or a workspace is.
 */
export function DataTab({ control, kind }: {
  control: Control;
  kind: DataKind;
}) {
  const t = useEditorT();
  const said = t.data;
  const toast = useToast();
  const nameOf = useKindName();

  const [stores, setStores] = useState<DataStore[] | null>(null);
  const [secrets, setSecrets] = useState<SecretListing | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [switching, setSwitching] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [recopying, setRecopying] = useState<'asking' | 'busy' | null>(null);

  /** Counts every change to what is held, so the views below read again. */
  const [generation, setGeneration] = useState(0);

  // what a demo keeps is there to be read, not replaced
  const demo = isDemo(control.lambda.tier);
  const simple = control.simple;

  const feature = control.feature?.info ?? null;

  const reload = useCallback(async () => {
    try {
      const [found, kept] = await Promise.all([
        feature ? api.feature.data(control.privateKey, feature.key) : api.data(control.privateKey),
        // only the secrets have a list of their own to read beside what the kind is
        kind === 'secrets' ? api.secrets.list(control.privateKey, feature?.key) : Promise.resolve(null),
      ]);

      setStores(found);
      setSecrets(kept);
      setFailure(null);
    } catch (error) {
      setFailure(error instanceof ApiError ? error.message : said.readFailed);
    }
  }, [control.privateKey, feature?.key, kind, said]);

  useEffect(() => {
    reload();
  }, [reload]);

  const changed = useCallback(async () => {
    setGeneration((was) => was + 1);
    await reload();
  }, [reload]);

  const words = said.kinds[kind];
  const current = stores?.find((store) => store.kind === kind) ?? null;

  async function toggle(on: boolean) {
    setConfirming(false);
    setSwitching(true);

    try {
      if (on) {
        await api.enableData(control.privateKey, kind);
        toast(words?.switchedOn ?? said.on, 'success');
      } else {
        await api.disableData(control.privateKey, kind);
        toast(words?.switchedOff ?? said.off);
      }

      await Promise.all([changed(), control.refresh()]);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.switchFailed, 'error');
    } finally {
      setSwitching(false);
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
      toast(said.recopied, 'success');

      await Promise.all([changed(), control.feature?.refresh()]);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.recopyFailed, 'error');
    } finally {
      setRecopying(null);
    }
  }

  const storage = control.summary?.storage;

  /**
   * The simple view never shows a kind that is off - it has no switches -
   * but for the secrets, which it switches on when the owner enters one the
   * app asks for. Come to by a link, it says there is nothing.
   */
  const nothing = simple && current !== null && !current.enabled && kind !== 'secrets';

  return (
    <Section
      title={nameOf(kind, simple)}
      hint={feature ? undefined : simple ? said.simple.hint : said.hint}
      actions={feature && !simple && (
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

      {feature && <p className="mb-5 max-w-2xl text-[13px] text-slate-600 dark:text-slate-400">{said.featureHint}</p>}

      {failure && <p className="mb-6 text-sm text-red-500">{failure}</p>}

      {stores === null && !failure ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <IconSpinner /> {t.files.reading}
        </div>
      ) : current === null ? null : nothing ? (
        <p className="surface p-6 text-center text-sm text-slate-500">{kind === 'database' ? said.simple.noRecords : said.simple.nothing}</p>
      ) : (
        <>
          {/* which kinds there are is the lambda's to switch, so a draft has no switch */}
          {!feature && !simple && (
            <Store
              store={current}
              publicly={kind === 'workspace' ? !!storage?.servesWorkspace : null}
              busy={switching}
              readOnly={demo}
              onToggle={() => (current.enabled ? setConfirming(true) : toggle(true))}
            />
          )}

          <div className={feature || simple ? '' : 'mt-8'}>
            {kind === 'secrets' ? (
              <SecretsPanel control={control} listing={secrets} readOnly={demo} onChanged={changed} />
            ) : kind === 'database' ? (
              <DatabasePanel control={control} enabled={current.enabled} readOnly={demo} generation={generation} onChanged={changed} />
            ) : simple ? (
              <SavedFiles key={generation} control={control} />
            ) : (
              <WorkspaceView key={generation} control={control} store={current} readOnly={demo} onChanged={changed} />
            )}
          </div>
        </>
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
        title={words?.confirmOff ?? ''}
        open={confirming && current !== null}
        onClose={() => setConfirming(false)}
        footer={
          <>
            <button type="button" onClick={() => setConfirming(false)} className="btn-ghost">
              {said.keep}
            </button>
            <button type="button" className="btn-danger" onClick={() => toggle(false)}>
              {said.deleteAndOff}
            </button>
          </>
        }
      >
        {current && (
          <>
            <p className="text-slate-600 dark:text-slate-400">
              {current.items > 0 || current.usedBytes > 0
                ? said.confirmText(
                    [words?.count(current.items), current.usedBytes > 0 ? bytes(current.usedBytes) : null]
                      .filter(Boolean)
                      .join(', '),
                  )
                : said.confirmEmpty}
            </p>
            {((kind === 'workspace' && storage?.usesWorkspace)
              || (kind === 'database' && storage?.usesDatabase)
              || (kind === 'secrets' && (secrets?.used.length ?? 0) > 0))
              && control.lambda.activeVersion != null && (
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
function Store({ store, publicly, busy, readOnly, onToggle }: {
  store: DataStore;
  /** Whether the code online serves it; null where that says nothing. */
  publicly: boolean | null;
  busy: boolean;
  readOnly: boolean;
  onToggle: () => void;
}) {
  const t = useEditorT();
  const said = t.data;
  const words = said.kinds[store.kind];
  const name = words?.name ?? store.kind;
  const id = `data-${store.kind}`;
  const Icon = ICONS[store.kind] ?? IconLayers;

  // a kind counts what it holds either in room or in things, and says so
  const counted = store.maxItems != null
    ? <Meter label={words?.count(store.items) ?? store.items} used={store.items} of={store.maxItems} format={String} />
    : <Meter label={words?.count(store.items) ?? store.items} used={store.usedBytes} of={store.quotaBytes} format={bytes} />;

  return (
    <div className={`surface flex items-start gap-4 p-4 ${store.enabled ? '' : 'bg-slate-50 dark:bg-ink-900'}`}>
      <span
        className={`mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center ${
          store.enabled ? 'bg-accent-500/10 text-accent-600 dark:text-accent-400' : 'bg-slate-400/10 text-slate-400'
        }`}
      >
        <Icon className="h-[18px] w-[18px]" />
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

        {words?.what && <p className="mt-1 max-w-2xl text-[13px] text-slate-500">{words.what}</p>}

        {store.enabled ? (
          <div className="mt-3 max-w-lg">{counted}</div>
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

/** What the workspace holds, browsed as a tree beside what the selected file holds. */
function WorkspaceView({ control, store, readOnly, onChanged }: {
  control: Control;
  store: DataStore;
  readOnly: boolean;
  onChanged: () => Promise<void>;
}) {
  const t = useEditorT();
  const said = t.data;
  const feature = control.feature?.info ?? null;
  const storage = control.summary?.storage;

  const [listing, setListing] = useState<WorkspaceListing | null>(null);
  const [selected, setSelected] = useState<Selection | null>(null);

  const workspace = workspaceOf(control);

  const read = useCallback(async () => {
    setListing(await workspace.list());
    // the accessor is made again with every render; the feature it reads from is what matters
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [control.privateKey, feature?.key]);

  useEffect(() => {
    read().catch(() => undefined);
  }, [read]);

  if (!store.enabled) {
    return <p className="surface p-6 text-center text-sm text-slate-500">{said.offBrowse}</p>;
  }

  return (
    <>
      <h2 className="mb-3 flex items-center gap-2 text-sm font-medium">
        <IconFolder className="h-4 w-4 text-slate-400" />
        {feature ? said.copyContents : said.contents}
      </h2>

      <div className="grid gap-5 lg:grid-cols-[17rem,1fr]">
        <nav aria-label={said.browse} className="lg:max-h-[40rem] lg:overflow-y-auto">
          <WorkspaceFiles
            control={control}
            listing={listing}
            publicly={feature ? null : !!storage?.servesWorkspace}
            readOnly={readOnly}
            selected={selected?.group === 'data' ? selected.path : null}
            onSelect={(path) => setSelected(path ? { group: 'data', path } : null)}
            onChanged={async () => {
              await read();
              await onChanged();
            }}
          />
        </nav>

        <Viewer control={control} selection={selected} files={NO_FILES} listing={listing} />
      </div>
    </>
  );
}

/**
 * What the app saved, for the simple view: a plain list with a way to take a
 * copy, newest first - without folders to open, uploads or deleting, which
 * are what the full view is for.
 */
function SavedFiles({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.data.simple;
  const [listing, setListing] = useState<WorkspaceListing | null>(null);
  const [all, setAll] = useState(false);

  const workspace = workspaceOf(control);

  useEffect(() => {
    workspace.list().then(setListing).catch(() => undefined);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [control.privateKey]);

  if (!listing) {
    return (
      <div className="flex items-center gap-2 text-sm text-slate-500">
        <IconSpinner /> {t.files.reading}
      </div>
    );
  }

  const files = [...listing.files].sort((a, b) => b.modified.localeCompare(a.modified));
  const visible = all ? files : files.slice(0, 12);

  // the title of the section names it; what is left to say is what it is
  return (
    <div className="space-y-4">
      <div>
        <p className="max-w-2xl text-[13px] text-slate-600 dark:text-slate-400">{said.workspaceText}</p>
        {files.length > 0 && (
          <p className="mt-2 text-[13px] text-slate-500">{said.workspaceSize(t.data.kinds.workspace.count(files.length), bytes(listing.usedBytes))}</p>
        )}
      </div>

      {files.length === 0 ? (
        <p className="surface p-6 text-center text-sm text-slate-500">{said.nothing}</p>
      ) : (
        <ul className="surface divide-y divide-slate-200 dark:divide-ink-800">
          {visible.map((file) => (
            <li key={file.path} className="flex items-center gap-3 px-4 py-2.5 text-[13px]">
              <span className="min-w-0 flex-1 truncate" title={file.path}>{file.path}</span>
              <span className="hidden shrink-0 tabular-nums text-slate-500 sm:inline">{bytes(file.size)}</span>
              <Ago at={file.modified} className="hidden shrink-0 text-slate-500 md:inline" />
              <a
                href={workspace.url(file.path)}
                download
                className="shrink-0 p-1 text-slate-400 hover:text-accent-500"
                aria-label={t.files.download}
                title={t.files.download}
              >
                <IconDownload className="h-4 w-4" />
              </a>
            </li>
          ))}
        </ul>
      )}

      {files.length > visible.length && (
        <button type="button" onClick={() => setAll(true)} className="text-[13px] text-accent-500 hover:underline">
          {said.more(files.length - visible.length)}
        </button>
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
