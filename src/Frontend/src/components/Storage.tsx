import { useCallback, useEffect, useRef, useState } from 'react';

import { ApiError, api, type LambdaFile, type WorkspaceListing } from '../api';
import { IconFolder, IconPlus, IconSpinner, IconTrash } from './Icons';
import { useToast } from './Toast';
import { ENTRY } from './FileTabs';

/**
 * Everything a lambda has on disk, which is two directories and not one.
 *
 * One is **part of the code**: saved when the code is saved, deployed when it
 * is deployed, rolled back when a version is, and copied when the lambda is
 * cloned. The other is a **folder on the server**: it changes the moment
 * something is uploaded or the lambda writes to it, and no deploy touches it.
 *
 * The first half means every file of the version, the C# included. It used to
 * leave the .cs files out, from when it was called "shipped" and meant the
 * things served rather than compiled - which made a tab called "saved with
 * your code" show everything except the code. They are all saved together;
 * that is the whole of what the tab is saying.
 *
 * "Shipped" was the word used here for the first of those, and somebody had
 * to ask what it meant, which is the answer to whether it was a good word.
 *
 * That difference is why they cannot simply be one directory. Merge them and a
 * deploy either wipes whatever the lambda has written since, or nothing can
 * ever be removed from what it ships. Both are real front ends to serve from -
 * Assets.App() for the first, Workspace.App() for the second - so the choice
 * is which one a particular file belongs in, not which one is correct.
 *
 * They behave identically here on purpose. One trail back to the top, one
 * upload that lands where you are, one button that makes a folder. Learning
 * the panel once is the whole point of it looking like this.
 */
export function Storage({
  privateKey,
  shipped,
  onShip,
  onUnship,
  onOpen,
  onClose,
}: {
  privateKey: string;
  shipped: LambdaFile[];
  onShip: (added: LambdaFile[]) => void;
  onUnship: (name: string) => void;
  onOpen: (name: string) => void;
  onClose: () => void;
}) {
  const toast = useToast();

  const [listing, setListing] = useState<WorkspaceListing | null>(null);
  const [busy, setBusy] = useState<string | null>(null);

  /** Which half is showing, and which of its folders is open. */
  const [side, setSide] = useState<'shipped' | 'workspace'>('shipped');
  const [where, setWhere] = useState('');

  const [naming, setNaming] = useState(false);
  const [folder, setFolder] = useState('');

  const picker = useRef<HTMLInputElement>(null);

  const load = useCallback(async () => {
    try {
      setListing(await api.files(privateKey));
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The workspace could not be read.', 'error');
    }
  }, [privateKey, toast]);

  useEffect(() => {
    load();
  }, [load]);

  function show(half: 'shipped' | 'workspace') {
    setSide(half);
    setWhere('');
    setNaming(false);
    setFolder('');
  }

  /*
   * What is in the folder that is open. Names are held whole - "site/app.css"
   * - and cut down to what is inside the current one, so a file deeper than
   * here shows as the folder that holds it rather than as itself.
   */
  const inside = (path: string) => {
    const rest = where ? (path.startsWith(`${where}/`) ? path.slice(where.length + 1) : null) : path;

    return rest === null || rest.includes('/') ? null : rest;
  };

  const names = side === 'shipped' ? shipped.map((file) => file.name) : (listing?.files ?? []).map((f) => f.path);

  /*
   * Shipped folders are worked out from the names; workspace folders are
   * listed by the server. The difference is not an inconsistency: nothing but
   * a file can be shipped, so a shipped folder with nothing in it cannot
   * exist, while an empty workspace folder is a thing somebody can make.
   */
  const folders =
    side === 'shipped'
      ? [...new Set(names.flatMap((name) => {
          const parts = name.split('/');
          return parts.slice(0, -1).map((_, depth) => parts.slice(0, depth + 1).join('/'));
        }))]
      : (listing?.folders ?? []);

  const hereFolders = folders.map((path) => ({ path, name: inside(path) }))
                             .filter((row): row is { path: string; name: string } => row.name !== null);

  const hereFiles = names.map((path) => ({ path, name: inside(path) }))
                         .filter((row): row is { path: string; name: string } => row.name !== null);

  const crumbs = where ? where.split('/') : [];

  const sizeOf = (path: string) => {
    if (side === 'shipped') {
      const file = shipped.find((one) => one.name === path);

      return file ? (file.encoding === 'base64' ? (file.code.length * 3) / 4 : file.code.length) : 0;
    }

    return listing?.files.find((one) => one.path === path)?.size ?? 0;
  };

  const binary = (path: string) =>
    side === 'shipped' && shipped.find((one) => one.name === path)?.encoding === 'base64';

  async function upload(files: FileList | null) {
    if (files === null || files.length === 0) {
      return;
    }

    const added: LambdaFile[] = [];

    for (const file of Array.from(files)) {
      const name = where ? `${where}/${file.name}` : file.name;

      setBusy(name);

      try {
        if (side === 'shipped') {
          if (shipped.some((one) => one.name.toLowerCase() === name.toLowerCase())) {
            toast(`${name} is already there. Remove it first.`, 'error');
            continue;
          }

          // anything that is not text goes as base64, which is the only way an
          // image or a font gets in at all - the tabs are a text editor
          const bytes = new Uint8Array(await file.arrayBuffer());

          added.push(readable(bytes)
            ? { name, code: new TextDecoder().decode(bytes) }
            : { name, code: encodeBytes(bytes), encoding: 'base64' });
        } else {
          await api.writeFile(privateKey, name, await encode(file));
        }
      } catch (error) {
        toast(error instanceof ApiError ? error.message : `${name} could not be uploaded.`, 'error');
      }
    }

    setBusy(null);

    if (picker.current) {
      picker.current.value = '';
    }

    if (added.length > 0) {
      onShip(added);
      toast(`${added.length} file${added.length === 1 ? '' : 's'} added. Save to keep them.`, 'success');
    } else if (side === 'workspace') {
      await load();
    }
  }

  async function makeFolder(event: React.FormEvent) {
    event.preventDefault();

    const wanted = folder.trim().replace(/^\/+|\/+$/g, '');

    if (!wanted) {
      return;
    }

    const path = where ? `${where}/${wanted}` : wanted;

    setFolder('');
    setNaming(false);

    /*
     * A shipped folder is not made, it is gone to: there is nothing to create
     * until something is put in it, so the panel simply opens it and the next
     * upload lands there. In the workspace it is a real directory and the
     * server is asked for one.
     */
    if (side === 'shipped') {
      setWhere(path);
      return;
    }

    setBusy('folder');

    try {
      setListing(await api.createFolder(privateKey, path));
      setWhere(path);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The folder could not be made.', 'error');
    } finally {
      setBusy(null);
    }
  }

  async function download(path: string) {
    if (side === 'shipped') {
      // a file of the version opens where it is edited, unless it is bytes
      if (!binary(path)) {
        onOpen(path);
      }

      return;
    }

    setBusy(path);

    try {
      const file = await api.readFile(privateKey, path);

      // the browser is handed the bytes rather than a link to them: the file
      // belongs to whoever wrote it and has no business being rendered here
      const url = URL.createObjectURL(new Blob([decode(file.content)]));
      const link = document.createElement('a');

      link.href = url;
      link.download = path.split('/').pop() ?? path;
      link.click();

      URL.revokeObjectURL(url);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The file could not be read.', 'error');
    } finally {
      setBusy(null);
    }
  }

  async function remove(path: string, isFolder: boolean) {
    if (isFolder) {
      const held = names.filter((name) => name.startsWith(`${path}/`)).length;

      const warning = held === 0
        ? `Remove the folder ${path}?`
        : `Remove ${path} and the ${held} file${held === 1 ? '' : 's'} in it?`;

      if (!window.confirm(warning)) {
        return;
      }
    }

    if (side === 'shipped') {
      const going = isFolder ? names.filter((one) => one.startsWith(`${path}/`)) : [path];

      if (going.includes(ENTRY)) {
        toast(`${ENTRY} is the snippet that runs. It cannot be removed.`, 'error');
        return;
      }

      for (const name of going) {
        onUnship(name);
      }

      if (isFolder && (where === path || where.startsWith(`${path}/`))) {
        setWhere('');
      }

      return;
    }

    setBusy(path);

    try {
      await api.deleteFile(privateKey, path);

      if (where === path || where.startsWith(`${path}/`)) {
        setWhere(path.includes('/') ? path.slice(0, path.lastIndexOf('/')) : '');
      }

      await load();
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'It could not be removed.', 'error');
    } finally {
      setBusy(null);
    }
  }

  const full = side === 'workspace' && listing !== null && listing.files.length >= listing.maxFiles;

  const empty = hereFiles.length === 0 && hereFolders.length === 0;

  return (
    <div
      className="fixed inset-0 z-40 flex items-center justify-center bg-black/40 p-4"
      onClick={onClose}
      role="presentation"
    >
      <div
        className="surface flex max-h-[80vh] w-full max-w-2xl flex-col shadow-xl"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-label="Storage"
      >
        <div className="flex items-start justify-between gap-4 border-b border-slate-200 px-5 py-4 dark:border-ink-800">
          <div>
            <h2 className="text-base font-semibold">Storage</h2>
            <p className="mt-0.5 text-xs text-slate-500">
              {side === 'shipped' ? (
                <>
                  Every file of your lambda, the C# included. They go out when you press Deploy and
                  come back if you roll a version back. The ones that are not C# are served as they
                  are — <code className="font-mono">Assets.App()</code> serves them.
                </>
              ) : (
                <>
                  A folder on the server. It changes the moment you upload, and a deploy never
                  touches it. Your lambda reads and writes it through{' '}
                  <code className="font-mono">Workspace</code>, and can serve it with{' '}
                  <code className="font-mono">Workspace.App()</code>.
                </>
              )}
            </p>
          </div>

          <button type="button" onClick={onClose} className="btn-ghost !px-2 !py-1 text-xs">
            Close
          </button>
        </div>

        <div className="flex items-stretch border-b border-slate-200 dark:border-ink-800">
          {(['shipped', 'workspace'] as const).map((half) => (
            <button
              key={half}
              type="button"
              title={half === 'shipped'
                ? 'Every file of your lambda: saved and deployed together'
                : 'A folder on the server: changes as soon as you upload, and a deploy never touches it'}
              onClick={() => show(half)}
              className={`px-5 py-2 text-xs ${
                side === half
                  ? 'border-b-2 border-accent-500 font-medium dark:border-accent-400'
                  : 'text-slate-500 hover:text-slate-700 dark:hover:text-slate-300'
              }`}
            >
              {half === 'shipped' ? 'Saved with your code' : 'Workspace'}
              <span className="ml-1.5 text-slate-400">
                {half === 'shipped' ? shipped.length : (listing?.files.length ?? 0)}
              </span>
            </button>
          ))}
        </div>

        {side === 'workspace' && listing !== null && (
          <div className="border-b border-slate-200 px-5 py-3 dark:border-ink-800">
            <div className="flex items-center justify-between text-xs text-slate-500">
              <span>
                {listing.files.length} of {listing.maxFiles} files · {bytes(listing.usedBytes)} of{' '}
                {bytes(listing.quotaBytes)}
              </span>
              <span>at most {bytes(listing.maxFileSize)} per file</span>
            </div>

            <div className="mt-1.5 h-1.5 w-full bg-slate-200 dark:bg-ink-800">
              <div
                className="h-full bg-accent-500 dark:bg-accent-400"
                style={{ width: `${Math.min(100, (listing.usedBytes / listing.quotaBytes) * 100)}%` }}
              />
            </div>
          </div>
        )}

        <div className="flex flex-wrap items-center gap-1 border-b border-slate-200 px-5 py-2 text-xs dark:border-ink-800">
          <button
            type="button"
            onClick={() => setWhere('')}
            className={where === '' ? 'font-medium' : 'text-accent-500 hover:underline'}
          >
            {side === 'shipped' ? 'shipped' : 'workspace'}
          </button>

          {crumbs.map((crumb, depth) => (
            <span key={crumb + depth} className="flex items-center gap-1">
              <span className="text-slate-400">/</span>
              <button
                type="button"
                onClick={() => setWhere(crumbs.slice(0, depth + 1).join('/'))}
                className={
                  depth === crumbs.length - 1 ? 'font-mono font-medium' : 'font-mono text-accent-500 hover:underline'
                }
              >
                {crumb}
              </button>
            </span>
          ))}
        </div>

        <div className="min-h-0 flex-1 overflow-y-auto">
          {side === 'workspace' && listing === null ? (
            <div className="flex items-center gap-2 px-5 py-10 text-sm text-slate-500">
              <IconSpinner /> Reading the workspace…
            </div>
          ) : empty ? (
            <p className="px-5 py-10 text-center text-sm text-slate-500">
              {where === '' ? 'Nothing here yet. Upload something.' : `${where} is empty. Upload into it.`}
            </p>
          ) : (
            <ul className="divide-y divide-slate-200 dark:divide-ink-800">
              {hereFolders.map((row) => (
                <li key={row.path} className="flex items-center gap-3 px-5 py-2.5">
                  <button type="button" onClick={() => setWhere(row.path)} className="flex min-w-0 flex-1 items-center gap-2 text-left">
                    <IconFolder className="h-4 w-4 shrink-0 text-slate-400" />
                    <span className="block truncate font-mono text-sm text-accent-500">{row.name}</span>
                  </button>

                  <button
                    type="button"
                    onClick={() => remove(row.path, true)}
                    disabled={busy !== null}
                    className="btn-danger !px-2 !py-1"
                    aria-label={`Delete ${row.path}`}
                  >
                    <IconTrash className="h-4 w-4" />
                  </button>
                </li>
              ))}

              {hereFiles.map((row) => (
                <li key={row.path} className="flex items-center gap-3 px-5 py-2.5">
                  <button
                    type="button"
                    onClick={() => download(row.path)}
                    className="min-w-0 flex-1 text-left"
                    title={side === 'workspace' ? 'Download' : binary(row.path) ? row.path : `Open ${row.path}`}
                  >
                    <span className="block truncate font-mono text-sm">{row.name}</span>
                    <span className="text-xs text-slate-500">
                      {bytes(sizeOf(row.path))}
                      {binary(row.path) && ' · binary'}
                    </span>
                  </button>

                  {busy === row.path && <IconSpinner className="h-4 w-4 text-slate-400" />}

                  {!(side === 'shipped' && row.path === ENTRY) && (
                    <button
                      type="button"
                      onClick={() => remove(row.path, false)}
                      disabled={busy !== null}
                      className="btn-danger !px-2 !py-1"
                      aria-label={`Delete ${row.path}`}
                    >
                      <IconTrash className="h-4 w-4" />
                    </button>
                  )}
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-3 border-t border-slate-200 px-5 py-3 dark:border-ink-800">
          <input ref={picker} type="file" multiple className="hidden" onChange={(event) => upload(event.target.files)} />

          <button
            type="button"
            onClick={() => picker.current?.click()}
            disabled={busy !== null || full}
            className="btn-ghost"
          >
            {busy !== null ? <IconSpinner /> : <IconPlus />}
            {where === '' ? 'Upload here' : `Upload into ${where}`}
          </button>

          {naming ? (
            <form onSubmit={makeFolder}>
              <input
                autoFocus
                value={folder}
                onChange={(event) => setFolder(event.target.value)}
                onBlur={() => { setNaming(false); setFolder(''); }}
                placeholder="folder name"
                className="w-40 border border-slate-300 bg-white px-2 py-1 font-mono text-xs dark:border-ink-700 dark:bg-ink-900"
              />
            </form>
          ) : (
            <button type="button" onClick={() => setNaming(true)} disabled={busy !== null} className="btn-ghost">
              New folder
            </button>
          )}

          {full && <span className="text-xs text-amber-600 dark:text-amber-400">The workspace is full.</span>}

          {side === 'shipped' && (
            <span className="ml-auto text-xs text-slate-500">Press Save to keep these.</span>
          )}
        </div>
      </div>
    </div>
  );
}

/**
 * Whether bytes are text somebody could reasonably edit in the tabs.
 *
 * A NUL settles it - no text file has one - and so does anything that is not
 * valid UTF-8. Guessing from the extension would be wrong for exactly the
 * files it matters for: a .txt full of bytes and a .dat full of JSON.
 */
function readable(bytes: Uint8Array): boolean {
  if (bytes.includes(0)) {
    return false;
  }

  try {
    new TextDecoder('utf-8', { fatal: true }).decode(bytes);
    return true;
  } catch {
    return false;
  }
}

/** Base64 of bytes in hand, in chunks so a large file does not blow the stack. */
function encodeBytes(bytes: Uint8Array): string {
  let binary = '';

  for (let i = 0; i < bytes.length; i += 0x8000) {
    binary += String.fromCharCode(...bytes.subarray(i, i + 0x8000));
  }

  return btoa(binary);
}

/** Reads a file as base64, without the data URL prefix the reader adds. */
function encode(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = () => resolve(String(reader.result).split(',')[1] ?? '');
    reader.onerror = () => reject(reader.error);
    reader.readAsDataURL(file);
  });
}

function decode(content: string): ArrayBuffer {
  const binary = atob(content);
  const buffer = new ArrayBuffer(binary.length);
  const bytes = new Uint8Array(buffer);

  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }

  return buffer;
}

function bytes(value: number): string {
  if (value < 1024) {
    return `${Math.round(value)} B`;
  }

  if (value < 1024 * 1024) {
    return `${(value / 1024).toFixed(1)} kB`;
  }

  return `${(value / 1024 / 1024).toFixed(1)} MB`;
}
