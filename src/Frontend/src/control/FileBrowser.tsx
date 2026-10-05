import { useEffect, useMemo, useState, type ReactNode } from 'react';

import { ApiError, api, type LambdaFile, type WorkspaceEntry, type WorkspaceListing } from '../api';
import { decode, download, encodeBytes, readable } from '../bytes';
import { CodeEditor } from '../components/CodeEditor';
import { IconChevronDown, IconDownload, IconSpinner } from '../components/Icons';
import { useEditorT } from '../i18n';
import { languageFor } from '../monaco';
import type { Control } from './context';
import { bytes } from './format';
import { Ago, pill } from './ui';

/**
 * The parts the two places files live are browsed with: the files of a
 * version under Files, and the workspace under Data. Built once so that the
 * two look and behave alike, which is what makes the difference between them
 * - one belongs to a version, the other to the lambda - the only thing to
 * notice.
 */

/**
 * Where the files of a workspace are read and written: the lambda's own, or
 * the copy a feature works on. The same calls either way, so the pages that
 * browse one browse the other.
 */
export interface WorkspaceAccess {
  list: () => Promise<WorkspaceListing>;
  read: (path: string) => Promise<{ path: string; content: string; size: number }>;
  /** Where a file is streamed from, for a download link. */
  url: (path: string) => string;
  upload: (path: string, content: Blob) => Promise<WorkspaceEntry>;
  remove: (path: string) => Promise<void>;
}

/** The workspace a control center shows: the feature's copy while it is opened on one, the lambda's otherwise. */
export function workspaceOf(control: Control): WorkspaceAccess {
  const { privateKey } = control;
  const feature = control.feature?.info.key;

  if (feature) {
    return {
      list: () => api.feature.workspace.list(privateKey, feature),
      read: (path) => api.feature.workspace.read(privateKey, feature, path),
      url: (path) => api.feature.workspace.url(privateKey, feature, path),
      upload: (path, content) => api.feature.workspace.upload(privateKey, feature, path, content),
      remove: (path) => api.feature.workspace.remove(privateKey, feature, path),
    };
  }

  return {
    list: () => api.files(privateKey),
    read: (path) => api.readFile(privateKey, path),
    url: (path) => api.fileUrl(privateKey, path),
    upload: (path, content) => api.uploadFile(privateKey, path, content),
    remove: (path) => api.deleteFile(privateKey, path),
  };
}

/** Where a selected file lives: in the version, as code, an asset, its documentation and tests or its development space, or in the data. */
export type Group = 'code' | 'assets' | 'context' | 'development' | 'data';

export interface Entry {
  path: string;
  size: number;
  modified?: string;
}

export interface Selection {
  group: Group;
  path: string;
}

/** A group of files with its name, whether the public can reach it, and an action. */
export function GroupList({ title, exposure, usage, action, children }: {
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

export function Tree({ entries, folders, selected, onSelect, empty, action }: {
  entries: Entry[];
  folders?: string[];
  selected: string | null;
  onSelect: (path: string) => void;
  empty: string;
  action?: (node: { path: string; folder: boolean }) => ReactNode;
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

/** Past this a data file is not read into the page to be shown, only offered to download. */
const PREVIEW_BYTES = 2 * 1024 * 1024;

/** What is selected, shown as what it is: code as code, pictures as pictures. */
export function Viewer({ control, selection, files, listing }: {
  control: Control;
  selection: Selection | null;
  files: LambdaFile[];
  listing: WorkspaceListing | null;
}) {
  const said = useEditorT().files;
  const [loaded, setLoaded] = useState<{ path: string; bytes: Uint8Array<ArrayBuffer> } | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  const entry = selection?.group === 'data' ? listing?.files.find((f) => f.path === selection.path) : undefined;

  const workspace = workspaceOf(control);

  // a data file is read when it is opened, never before: it can be a
  // megabyte, and the tree only needs its name
  useEffect(() => {
    setLoaded(null);
    setFailure(null);

    if (!entry || entry.size > PREVIEW_BYTES) {
      return;
    }

    let alive = true;

    workspace
      .read(entry.path)
      .then((file) => alive && setLoaded({ path: file.path, bytes: new Uint8Array(decode(file.content)) }))
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.fileFailed));

    return () => {
      alive = false;
    };
    // the accessor is made again with every render; the feature it reads from is what matters
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [control.privateKey, control.feature?.info.key, entry?.path, entry?.modified, said]);

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
          <a href={workspace.url(entry.path)} download={name.split('/').pop()} className={pill(false)}>
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
            href={workspace.url(entry.path)}
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

export function sizeOf(file: LambdaFile): number {
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
