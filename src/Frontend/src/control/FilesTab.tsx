import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, isDemo, type LambdaFile, type VersionContent, type WorkspaceListing } from '../api';
import { decode, download, encodeBytes, readable } from '../bytes';
import { CodeEditor } from '../components/CodeEditor';
import { IconChevronDown, IconDownload, IconSpinner, IconTrash, IconUpload } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useEditorT } from '../i18n';
import { languageFor } from '../monaco';
import type { Control } from './context';
import { bytes, servesAssets, servesWorkspace } from './format';
import { Exposure } from './SummaryTab';
import { Ago, Section, pill } from './ui';

type Group = 'code' | 'assets' | 'data';

interface Entry {
  path: string;
  size: number;
  modified?: string;
}

interface Selection {
  group: Group;
  path: string;
}

/**
 * Everything a lambda keeps, in the three places it keeps it, and which of
 * them anybody on the internet can reach.
 *
 * Code and assets belong to a version, so the version is this section's view.
 * Data belongs to the lambda: whatever it has written, the same whichever
 * version is picked.
 */
export function FilesTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.files;
  const toast = useToast();
  const [params, setParams] = useSearchParams();

  const { lambda, versions, summary } = control;

  const wanted = Number(params.get('version')) || lambda.activeVersion || lambda.latestVersion || versions[0]?.version;

  const [content, setContent] = useState<VersionContent | null>(null);
  const [listing, setListing] = useState<WorkspaceListing | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [selected, setSelected] = useState<Selection | null>(null);

  useEffect(() => {
    if (wanted == null) {
      return;
    }

    let alive = true;

    setContent(null);

    api
      .version(control.privateKey, wanted)
      .then((found) => {
        if (!alive) {
          return;
        }

        setContent(found);

        // the snippet is where reading starts, unless something else is open
        setSelected((was) =>
          was && (was.group === 'data' || found.files.some((f) => f.name === was.path))
            ? was
            : { group: 'code', path: found.files[0]?.name ?? 'lambda.cs' });
      })
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.readFailed));

    return () => {
      alive = false;
    };
  }, [control.privateKey, wanted, said]);

  const reload = useCallback(async () => {
    try {
      setListing(await api.files(control.privateKey));
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.dataFailed, 'error');
    }
  }, [control.privateKey, toast, said]);

  useEffect(() => {
    reload();
  }, [reload]);

  const files = content?.files ?? [];
  const code = files.filter((f) => f.name.endsWith('.cs'));
  const assets = files.filter((f) => !f.name.endsWith('.cs'));

  const source = code.map((f) => f.code).join('\n');
  const limits = summary?.limits;

  const version = versions.find((v) => v.version === wanted);

  return (
    <Section
      title={t.frame.sections.files}
      hint={said.hint((text) => <b>{text}</b>)}
      actions={
        wanted != null && (
          <button type="button" onClick={() => control.edit(wanted)} className="btn-ghost !px-3 !py-1.5 text-[13px]">
            {said.edit}
          </button>
        )
      }
      pills={
        versions.length > 0 && (
          <label className={`${pill(true)} relative cursor-pointer pr-7`}>
            <span className="sr-only">{said.version}</span>
            <span>{said.shown(wanted, wanted === lambda.activeVersion, wanted === lambda.latestVersion)}</span>
            <IconChevronDown className="pointer-events-none absolute right-2.5 h-3.5 w-3.5" />
            <select
              value={wanted}
              onChange={(event) => setParams({ version: event.target.value }, { replace: true })}
              className="absolute inset-0 cursor-pointer opacity-0"
            >
              {versions.map((v) => (
                <option key={v.version} value={v.version}>
                  {v.version}
                  {v.version === lambda.activeVersion ? said.optionOnline : ''}
                  {v.change ? ` - ${v.change.slice(0, 60)}` : ''}
                </option>
              ))}
            </select>
          </label>
        )
      }
    >
      {wanted == null ? (
        <p className="text-sm text-slate-500">{said.noVersion}</p>
      ) : (
        <>
          {version?.change && <p className="-mt-1 mb-4 text-[13px] text-slate-500">{version.change}</p>}
          {failure && <p className="mb-4 text-sm text-red-500">{failure}</p>}

          <div className="grid gap-5 lg:grid-cols-[17rem,1fr]">
            <nav aria-label={said.label} className="space-y-5 lg:max-h-[40rem] lg:overflow-y-auto">
              <GroupList
                title={said.code}
                exposure={<Exposure open={false} why={said.codeWhy} />}
                usage={limits && said.codeUsage(
                  said.count(code.length),
                  code.reduce((total, f) => total + f.code.length, 0).toLocaleString(),
                  limits.codeCharacters.toLocaleString(),
                )}
              >
                <Tree
                  entries={code.map((f) => ({ path: f.name, size: sizeOf(f) }))}
                  selected={selected?.group === 'code' ? selected.path : null}
                  onSelect={(path) => setSelected({ group: 'code', path })}
                  empty={said.noCode}
                />
              </GroupList>

              <GroupList
                title={said.assets}
                exposure={<Exposure open={servesAssets(source)} why={servesAssets(source) ? said.assetsPublic : said.assetsPrivate} />}
                usage={limits && said.usage(
                  said.count(assets.length),
                  bytes(assets.reduce((total, f) => total + sizeOf(f), 0)),
                  bytes(limits.assetBytes),
                )}
              >
                <Tree
                  entries={assets.map((f) => ({ path: f.name, size: sizeOf(f) }))}
                  selected={selected?.group === 'assets' ? selected.path : null}
                  onSelect={(path) => setSelected({ group: 'assets', path })}
                  empty={said.noAssets}
                />
              </GroupList>

              <Data
                control={control}
                listing={listing}
                publicly={servesWorkspace(source)}
                selected={selected?.group === 'data' ? selected.path : null}
                onSelect={(path) => setSelected(path ? { group: 'data', path } : null)}
                onChanged={reload}
              />
            </nav>

            <Viewer control={control} selection={selected} files={files} listing={listing} />
          </div>
        </>
      )}
    </Section>
  );
}

function GroupList({ title, exposure, usage, action, children }: {
  title: string;
  exposure: ReactNode;
  usage?: string | false;
  action?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section>
      <header className="flex items-center gap-2 px-1 pb-1">
        <h2 className="text-[13px] font-medium" title={usage || undefined}>{title}</h2>
        {exposure}
        <span className="ml-auto">{action}</span>
      </header>
      {children}
    </section>
  );
}

interface Node {
  name: string;
  path: string;
  size: number;
  folder: boolean;
  children: Node[];
}

/** Paths into a tree, folders first, each folder weighing what it holds. */
function grow(entries: Entry[], folders: string[] = []): Node[] {
  const root: Node = { name: '', path: '', size: 0, folder: true, children: [] };

  const place = (path: string, size: number, folder: boolean) => {
    const parts = path.split('/');
    let at = root;

    parts.forEach((part, depth) => {
      const here = parts.slice(0, depth + 1).join('/');
      const last = depth === parts.length - 1;

      let next = at.children.find((child) => child.name === part);

      if (!next) {
        next = { name: part, path: here, size: 0, folder: !last || folder, children: [] };
        at.children.push(next);
      }

      next.size += last ? size : 0;
      at = next;
    });
  };

  folders.forEach((folder) => place(folder, 0, true));
  entries.forEach((entry) => place(entry.path, entry.size, false));

  const settle = (node: Node): number => {
    if (node.folder) {
      node.size = node.children.reduce((total, child) => total + settle(child), 0);

      // folders first, then the snippet - it is where the program starts -
      // then everything else by name
      node.children.sort((a, b) =>
        a.folder !== b.folder
          ? a.folder ? -1 : 1
          : a.path === 'lambda.cs' ? -1 : b.path === 'lambda.cs' ? 1 : a.name.localeCompare(b.name));
    }

    return node.size;
  };

  settle(root);

  return root.children;
}

function Tree({ entries, folders, selected, onSelect, empty, action }: {
  entries: Entry[];
  folders?: string[];
  selected: string | null;
  onSelect: (path: string) => void;
  empty: string;
  action?: (node: Node) => ReactNode;
}) {
  const nodes = useMemo(() => grow(entries, folders), [entries, folders]);
  const [closed, setClosed] = useState<Set<string>>(new Set());

  if (nodes.length === 0) {
    return <p className="px-1 text-[13px] text-slate-400">{empty}</p>;
  }

  const render = (node: Node, depth: number): ReactNode => {
    const shut = closed.has(node.path);

    return (
      <li key={node.path}>
        <div
          className={`group flex items-center gap-1.5 pr-1 text-[13px] ${
            selected === node.path ? 'bg-accent-500/10 text-accent-700 dark:text-accent-400' : 'hover:bg-slate-100 dark:hover:bg-ink-850'
          }`}
          style={{ paddingLeft: `${0.25 + depth * 0.9}rem` }}
        >
          <button
            type="button"
            onClick={() =>
              node.folder
                ? setClosed((was) => {
                    const next = new Set(was);
                    if (next.has(node.path)) next.delete(node.path);
                    else next.add(node.path);
                    return next;
                  })
                : onSelect(node.path)
            }
            className="flex min-w-0 flex-1 items-center gap-1.5 py-1 text-left"
            aria-expanded={node.folder ? !shut : undefined}
          >
            {node.folder ? (
              <IconChevronDown className={`h-3 w-3 shrink-0 text-slate-400 transition-transform ${shut ? '-rotate-90' : ''}`} />
            ) : (
              <span className="w-3 shrink-0" />
            )}
            <span className={`truncate ${node.folder ? '' : 'font-mono'}`}>{node.name}</span>
          </button>
          <span className="shrink-0 text-[11px] tabular-nums text-slate-400">{bytes(node.size)}</span>
          {action && <span className="inline-flex shrink-0 opacity-0 focus-within:opacity-100 group-hover:opacity-100">{action(node)}</span>}
        </div>

        {node.folder && !shut && node.children.length > 0 && <ul>{node.children.map((child) => render(child, depth + 1))}</ul>}
      </li>
    );
  };

  return <ul>{nodes.map((node) => render(node, 0))}</ul>;
}

function Data({ control, listing, publicly, selected, onSelect, onChanged }: {
  control: Control;
  listing: WorkspaceListing | null;
  publicly: boolean;
  selected: string | null;
  onSelect: (path: string | null) => void;
  onChanged: () => Promise<void>;
}) {
  const said = useEditorT().files;
  const toast = useToast();
  const picker = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);

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
        await api.uploadFile(control.privateKey, path, file);
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
      await api.deleteFile(control.privateKey, path);

      if (selected === path || selected?.startsWith(`${path}/`)) {
        onSelect(null);
      }

      await onChanged();
    } catch (error) {
      toast(error instanceof ApiError ? error.message : said.deleteFailed, 'error');
    }
  }

  const full = listing !== null && listing.usedBytes >= listing.quotaBytes;

  // what a demo keeps is there to be read, not replaced
  const demo = isDemo(control.lambda.tier);

  return (
    <GroupList
      title={said.data}
      exposure={<Exposure open={publicly} why={publicly ? said.dataPublic : said.dataPrivate} />}
      usage={listing ? said.usage(said.count(listing.files.length), bytes(listing.usedBytes), bytes(listing.quotaBytes)) : undefined}
      action={demo ? undefined : (
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
          action={demo ? undefined : (node) => (
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

/** Past this a data file is not read into the page to be shown, only offered to download. */
const PREVIEW_BYTES = 2 * 1024 * 1024;

/** What is selected, shown as what it is: code as code, pictures as pictures. */
function Viewer({ control, selection, files, listing }: {
  control: Control;
  selection: Selection | null;
  files: LambdaFile[];
  listing: WorkspaceListing | null;
}) {
  const said = useEditorT().files;
  const [loaded, setLoaded] = useState<{ path: string; bytes: Uint8Array<ArrayBuffer> } | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  const entry = selection?.group === 'data' ? listing?.files.find((f) => f.path === selection.path) : undefined;

  // a data file is read when it is opened, never before: it can be a
  // megabyte, and the tree only needs its name
  useEffect(() => {
    setLoaded(null);
    setFailure(null);

    if (!entry || entry.size > PREVIEW_BYTES) {
      return;
    }

    let alive = true;

    api
      .readFile(control.privateKey, entry.path)
      .then((file) => alive && setLoaded({ path: file.path, bytes: new Uint8Array(decode(file.content)) }))
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.fileFailed));

    return () => {
      alive = false;
    };
  }, [control.privateKey, entry?.path, entry?.modified, said]);

  const frame = 'surface flex min-h-[20rem] flex-col';

  if (!selection || (selection.group === 'data' && !entry)) {
    return <div className={`${frame} items-center justify-center p-6 text-sm text-slate-500`}>{said.pick}</div>;
  }

  let name = selection.path;
  let text: string | null = null;
  let base64: string | null = null;
  let size = 0;

  if (selection.group === 'data') {
    size = entry?.size ?? 0;

    // a model or a dataset: the workspace does not limit how large a file
    // may be, and nothing is gained by pulling one into the page
    if (entry && entry.size > PREVIEW_BYTES) {
      return (
        <div className={`${frame} items-center justify-center gap-3 p-6 text-center text-sm text-slate-500`}>
          <p>{said.tooLarge(<span className="font-mono">{name}</span>, bytes(entry.size))}</p>
          <a href={api.fileUrl(control.privateKey, entry.path)} download={name.split('/').pop()} className={pill(false)}>
            <IconDownload className="h-3.5 w-3.5" /> {said.download}
          </a>
        </div>
      );
    }

    if (failure) {
      return <div className={`${frame} p-6 text-sm text-red-500`}>{failure}</div>;
    }

    if (!loaded || loaded.path !== selection.path) {
      return <div className={`${frame} items-center justify-center gap-2 text-sm text-slate-500`}><IconSpinner /> {said.readingFile(name)}</div>;
    }

    if (readable(loaded.bytes)) {
      text = new TextDecoder().decode(loaded.bytes);
    } else {
      base64 = encodeBytes(loaded.bytes);
    }
  } else {
    const file = files.find((f) => f.name === selection.path);

    if (!file) {
      return <div className={`${frame} p-6 text-sm text-slate-500`}>{said.missing(name)}</div>;
    }

    name = file.name;
    size = sizeOf(file);

    if (file.encoding === 'base64') {
      base64 = file.code;
    } else {
      text = file.code;
    }
  }

  const image = base64 && imageType(name);

  return (
    <div className={frame}>
      <header className="flex items-center gap-3 border-b border-slate-200 px-3 py-2 dark:border-ink-800">
        <span className="min-w-0 truncate font-mono text-[13px]" title={name}>{name}</span>
        <span className="text-xs text-slate-400">
          {bytes(size)}
          {entry?.modified && <>, {said.saved} <Ago at={entry.modified} /></>}
        </span>
        {entry ? (
          <a
            href={api.fileUrl(control.privateKey, entry.path)}
            download={name.split('/').pop()}
            className="ml-auto rounded-full p-1 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500"
            title={said.download}
            aria-label={said.download}
          >
            <IconDownload className="h-3.5 w-3.5" />
          </a>
        ) : (
          <button
            type="button"
            onClick={() => download(name, text !== null ? new TextEncoder().encode(text) : new Uint8Array(decode(base64!)))}
            className="ml-auto rounded-full p-1 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500"
            title={said.download}
            aria-label={said.download}
          >
            <IconDownload className="h-3.5 w-3.5" />
          </button>
        )}
      </header>

      {text !== null ? (
        <div className="h-[34rem] min-h-0">
          <CodeEditor path={`${selection.group}:${name}`} value={text} language={languageFor(name)} theme={control.theme} diagnostics={[]} readOnly />
        </div>
      ) : image ? (
        <div className="flex flex-1 items-center justify-center bg-[repeating-conic-gradient(#8881_0%_25%,transparent_0%_50%)] bg-[length:16px_16px] p-6">
          <img src={`data:${image};base64,${base64}`} alt={name} className="max-h-[30rem] max-w-full" />
        </div>
      ) : (
        <p className="flex flex-1 items-center justify-center p-6 text-sm text-slate-500">{said.notText}</p>
      )}
    </div>
  );
}

function sizeOf(file: LambdaFile): number {
  return file.encoding === 'base64' ? Math.floor((file.code.length * 3) / 4) : new TextEncoder().encode(file.code).length;
}

function imageType(name: string): string | null {
  const extension = name.split('.').pop()?.toLowerCase();

  return (
    {
      png: 'image/png',
      jpg: 'image/jpeg',
      jpeg: 'image/jpeg',
      gif: 'image/gif',
      webp: 'image/webp',
      svg: 'image/svg+xml',
      ico: 'image/x-icon',
      avif: 'image/avif',
    }[extension ?? ''] ?? null
  );
}
