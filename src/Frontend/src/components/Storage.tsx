import { useCallback, useEffect, useRef, useState } from 'react';

import { ApiError, api, type WorkspaceEntry, type WorkspaceListing } from '../api';
import { IconSpinner, IconTrash } from './Icons';
import { useToast } from './Toast';

/**
 * What a lambda has on disk, which is two different things.
 *
 * What it ships was written in the editor and is served as it is; what it has
 * written it did itself, at runtime, through Workspace. They were easy to
 * confuse when only one of them was shown here and the button that opened it
 * said "Files" - somebody who had just put a page in a folder came looking
 * for it and found somebody else's directory.
 *
 * The shipped half is listed rather than managed: it is edited in the tabs
 * above, and offering a second place to change it would only raise the
 * question of which one wins.
 *
 * The private directory of a lambda, as a list you can add to and take from.
 *
 * Content travels base64 encoded, which is what lets the same panel carry an
 * image, an archive or a text file without knowing which it has.
 */
export function Storage({
  privateKey,
  shipped,
  onClose,
}: {
  privateKey: string;
  shipped: { name: string; code: string }[];
  onClose: () => void;
}) {
  const toast = useToast();

  const [listing, setListing] = useState<WorkspaceListing | null>(null);
  const [busy, setBusy] = useState<string | null>(null);

  /** Which folder is open. Empty is the top of the workspace. */
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

  async function upload(files: FileList | null) {
    if (files === null || files.length === 0) {
      return;
    }

    for (const file of Array.from(files)) {
      setBusy(file.name);

      try {
        await api.writeFile(privateKey, where ? `${where}/${file.name}` : file.name, await encode(file));
      } catch (error) {
        toast(error instanceof ApiError ? error.message : `${file.name} could not be uploaded.`, 'error');
      }
    }

    setBusy(null);

    if (picker.current) {
      picker.current.value = '';
    }

    await load();
  }

  async function makeFolder(event: React.FormEvent) {
    event.preventDefault();

    const wanted = folder.trim().replace(/^\/+|\/+$/g, '');

    if (!wanted) {
      return;
    }

    setBusy('folder');

    try {
      setListing(await api.createFolder(privateKey, where ? `${where}/${wanted}` : wanted));
      setWhere(where ? `${where}/${wanted}` : wanted);
      setFolder('');
      setNaming(false);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The folder could not be made.', 'error');
    } finally {
      setBusy(null);
    }
  }

  async function download(entry: WorkspaceEntry) {
    setBusy(entry.path);

    try {
      const file = await api.readFile(privateKey, entry.path);

      // the browser is handed the bytes rather than a link to them: the file
      // belongs to whoever wrote it and has no business being rendered here
      const blob = new Blob([decode(file.content)]);
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');

      link.href = url;
      link.download = entry.path.split('/').pop() ?? entry.path;
      link.click();

      URL.revokeObjectURL(url);
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The file could not be read.', 'error');
    } finally {
      setBusy(null);
    }
  }

  async function remove(entry: WorkspaceEntry) {
    const isFolder = (listing?.folders ?? []).includes(entry.path);

    if (isFolder) {
      const held = (listing?.files ?? []).filter((file) => file.path.startsWith(`${entry.path}/`)).length;

      const warning = held === 0
        ? `Remove the folder ${entry.path}?`
        : `Remove ${entry.path} and the ${held} file${held === 1 ? '' : 's'} in it?`;

      if (!window.confirm(warning)) {
        return;
      }
    }

    setBusy(entry.path);

    try {
      await api.deleteFile(privateKey, entry.path);

      if (where === entry.path || where.startsWith(`${entry.path}/`)) {
        setWhere(entry.path.includes('/') ? entry.path.slice(0, entry.path.lastIndexOf('/')) : '');
      }

      await load();
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The file could not be removed.', 'error');
    } finally {
      setBusy(null);
    }
  }

  const full = listing !== null && listing.files.length >= listing.maxFiles;

  /*
   * What is in the folder that is open, rather than everything at once. A
   * workspace with a few hundred files in it was one flat list of paths, which
   * is readable right up until somebody puts things in folders and then is
   * not.
   */
  const inside = (path: string) => {
    const rest = where ? (path.startsWith(`${where}/`) ? path.slice(where.length + 1) : null) : path;

    return rest === null || rest.includes('/') ? null : rest;
  };

  const here = (listing?.files ?? [])
    .map((entry) => ({ entry, name: inside(entry.path) }))
    .filter((row): row is { entry: WorkspaceEntry; name: string } => row.name !== null);

  const folders = (listing?.folders ?? [])
    .map((path) => ({ path, name: inside(path) }))
    .filter((row): row is { path: string; name: string } => row.name !== null);

  const crumbs = where ? where.split('/') : [];

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
        aria-label="Workspace"
      >
        <div className="flex items-start justify-between gap-4 border-b border-slate-200 px-5 py-4 dark:border-ink-800">
          <div>
            <h2 className="text-base font-semibold">Workspace</h2>
            <p className="mt-0.5 text-xs text-slate-500">
              The private directory your lambda reads and writes through <code className="font-mono">Workspace</code>.
            </p>
          </div>

          <button type="button" onClick={onClose} className="btn-ghost !px-2 !py-1 text-xs">
            Close
          </button>
        </div>

        {shipped.length > 0 && (
          <div className="border-b border-slate-200 px-5 py-3 dark:border-ink-800">
            <div className="text-xs font-medium text-slate-600 dark:text-slate-300">
              Shipped with the code
            </div>
            <p className="mt-0.5 text-xs text-slate-500">
              Served exactly as written, never compiled. Edit these in the tabs above the editor; a
              name with a slash in it puts one in a folder, and{' '}
              <code className="font-mono">Assets.App("site")</code> serves that folder.
            </p>

            <ul className="mt-2 space-y-0.5">
              {shipped.map((file) => (
                <li key={file.name} className="flex items-baseline justify-between gap-3 font-mono text-xs">
                  <span className="truncate text-slate-700 dark:text-slate-300">{file.name}</span>
                  <span className="shrink-0 text-slate-400">{bytes(file.code.length)}</span>
                </li>
              ))}
            </ul>
          </div>
        )}

        {listing === null ? (
          <div className="flex items-center gap-2 px-5 py-10 text-sm text-slate-500">
            <IconSpinner /> Reading the workspace…
          </div>
        ) : (
          <>
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

            <div className="flex flex-wrap items-center gap-1 border-b border-slate-200 px-5 py-2 text-xs dark:border-ink-800">
              <button
                type="button"
                onClick={() => setWhere('')}
                className={where === '' ? 'font-medium' : 'text-accent-500 hover:underline'}
              >
                workspace
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
              {here.length === 0 && folders.length === 0 ? (
                <p className="px-5 py-10 text-center text-sm text-slate-500">
                  {where === '' ? (
                    <>
                      Nothing here yet. Your lambda can write files with{' '}
                      <code className="font-mono">Workspace.WriteText(…)</code>, or you can upload some.
                    </>
                  ) : (
                    <>
                      This folder is empty. Upload into it, or write to{' '}
                      <code className="font-mono">{where}/…</code> from your lambda.
                    </>
                  )}
                </p>
              ) : (
                <ul className="divide-y divide-slate-200 dark:divide-ink-800">
                  {folders.map((row) => (
                    <li key={row.path} className="flex items-center gap-3 px-5 py-2.5">
                      <button
                        type="button"
                        onClick={() => setWhere(row.path)}
                        className="min-w-0 flex-1 text-left"
                      >
                        <span className="block truncate font-mono text-sm text-accent-500">{row.name}/</span>
                        <span className="text-xs text-slate-500">folder</span>
                      </button>

                      <button
                        type="button"
                        onClick={() => remove({ path: row.path, size: 0, modified: new Date().toISOString() })}
                        disabled={busy !== null}
                        className="btn-danger !px-2 !py-1"
                        aria-label={`Delete ${row.path}`}
                      >
                        <IconTrash className="h-4 w-4" />
                      </button>
                    </li>
                  ))}

                  {here.map(({ entry }) => (
                    <li key={entry.path} className="flex items-center gap-3 px-5 py-2.5">
                      <button
                        type="button"
                        onClick={() => download(entry)}
                        className="min-w-0 flex-1 text-left"
                        title="Download"
                      >
                        <span className="block truncate font-mono text-sm">{inside(entry.path)}</span>
                        <span className="text-xs text-slate-500">
                          {bytes(entry.size)} · {new Date(entry.modified).toLocaleString()}
                        </span>
                      </button>

                      {busy === entry.path && <IconSpinner className="h-4 w-4 text-slate-400" />}

                      <button
                        type="button"
                        onClick={() => remove(entry)}
                        disabled={busy !== null}
                        className="btn-danger !px-2 !py-1"
                        aria-label={`Delete ${entry.path}`}
                      >
                        <IconTrash className="h-4 w-4" />
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </div>

            <div className="flex items-center gap-3 border-t border-slate-200 px-5 py-3 dark:border-ink-800">
              <input
                ref={picker}
                type="file"
                multiple
                className="hidden"
                onChange={(event) => upload(event.target.files)}
              />

              <button
                type="button"
                onClick={() => picker.current?.click()}
                disabled={busy !== null || full}
                className="btn-ghost"
              >
                {busy !== null ? <IconSpinner /> : null}
                {where === '' ? 'Upload files' : `Upload into ${where}`}
              </button>

              {naming ? (
                <form onSubmit={makeFolder} className="flex items-center gap-2">
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
                <button
                  type="button"
                  onClick={() => setNaming(true)}
                  disabled={busy !== null}
                  className="btn-ghost"
                >
                  New folder
                </button>
              )}

              {full && <span className="text-xs text-amber-600 dark:text-amber-400">The workspace is full.</span>}
            </div>
          </>
        )}
      </div>
    </div>
  );
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

const units = ['B', 'kB', 'MB'];

function bytes(value: number): string {
  let size = value;
  let unit = 0;

  while (size >= 1024 && unit < units.length - 1) {
    size /= 1024;
    unit++;
  }

  return `${size.toFixed(unit === 0 || size >= 100 ? 0 : 1)} ${units[unit]}`;
}
