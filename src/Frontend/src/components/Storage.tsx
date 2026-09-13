import { useCallback, useEffect, useRef, useState } from 'react';

import { ApiError, api, type WorkspaceEntry, type WorkspaceListing } from '../api';
import { IconSpinner, IconTrash } from './Icons';
import { useToast } from './Toast';

/**
 * The private directory of a lambda, as a list you can add to and take from.
 *
 * Content travels base64 encoded, which is what lets the same panel carry an
 * image, an archive or a text file without knowing which it has.
 */
export function Storage({ privateKey, onClose }: { privateKey: string; onClose: () => void }) {
  const toast = useToast();

  const [listing, setListing] = useState<WorkspaceListing | null>(null);
  const [busy, setBusy] = useState<string | null>(null);
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
        await api.writeFile(privateKey, file.name, await encode(file));
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
    setBusy(entry.path);

    try {
      await api.deleteFile(privateKey, entry.path);
      await load();
    } catch (error) {
      toast(error instanceof ApiError ? error.message : 'The file could not be removed.', 'error');
    } finally {
      setBusy(null);
    }
  }

  const full = listing !== null && listing.files.length >= listing.maxFiles;

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

            <div className="min-h-0 flex-1 overflow-y-auto">
              {listing.files.length === 0 ? (
                <p className="px-5 py-10 text-center text-sm text-slate-500">
                  Nothing here yet. Your lambda can write files with{' '}
                  <code className="font-mono">Workspace.WriteText(…)</code>, or you can upload some.
                </p>
              ) : (
                <ul className="divide-y divide-slate-200 dark:divide-ink-800">
                  {listing.files.map((entry) => (
                    <li key={entry.path} className="flex items-center gap-3 px-5 py-2.5">
                      <button
                        type="button"
                        onClick={() => download(entry)}
                        className="min-w-0 flex-1 text-left"
                        title="Download"
                      >
                        <span className="block truncate font-mono text-sm">{entry.path}</span>
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
                Upload files
              </button>

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
