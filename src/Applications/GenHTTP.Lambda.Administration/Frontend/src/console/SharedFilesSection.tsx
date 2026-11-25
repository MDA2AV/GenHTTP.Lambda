import { useCallback, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError, api, refusedToken, type SharedFile, type SharedFiles } from '@genhttp-lambda/kit/api';
import { Dialog } from '@genhttp-lambda/kit/components/Dialog';
import { IconCheck, IconCopy, IconExternal, IconFile, IconSpinner, IconTrash, IconUpload } from '@genhttp-lambda/kit/components/Icons';
import { useToast } from '@genhttp-lambda/kit/components/Toast';
import { Ago, Empty, Section } from '@genhttp-lambda/kit/ui/ui';
import { drawn } from './BrandFields';
import { faces } from './BrandPreview';
import type { Access } from './context';
import { Card, Reading, actionButton, bytes, plural } from './parts';
import { refusal, share, sharedName } from './shared';

/** What uses a file, and where that is decided. */
const USES: Record<'textFont' | 'headingFont' | 'guide', { label: string; to: string }> = {
  textFont: { label: 'Font', to: '/admin/branding' },
  headingFont: { label: 'Headings', to: '/admin/branding' },
  guide: { label: 'Guide', to: '/admin/guide' },
};

/**
 * The files the operator shares across the installation: for its branding,
 * for the guide, and for the apps built on it.
 *
 * A short list and what may be done with each: its address copied, the file
 * opened, replaced or deleted. Files are added from the header or dropped on
 * the list; one of a name that is there replaces it, which is asked first,
 * since whatever uses it has the new one from then on.
 */
export function SharedFilesSection({ access }: { access: Access }) {
  const { token, deny } = access;

  const toast = useToast();

  const [shared, setShared] = useState<SharedFiles | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [over, setOver] = useState(false);
  const [problems, setProblems] = useState<string[]>([]);
  /** What was chosen and would replace what is there, until the operator says so. */
  const [replacing, setReplacing] = useState<{ name: string; file: File }[]>([]);
  const [deleting, setDeleting] = useState<SharedFile | null>(null);

  const input = useRef<HTMLInputElement>(null);
  /** The file a chosen one is to replace, whatever it is called on this computer. */
  const target = useRef<SharedFile | null>(null);

  const load = useCallback(async () => {
    try {
      setShared(await api.admin.sharedFiles(token));
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        setFailure('The shared files could not be read.');
      }
    }
  }, [token, deny]);

  useEffect(() => void load(), [load]);

  const files = shared?.files ?? [];

  /** Shares what is new, and asks before anything there is replaced. */
  async function add(chosen: File[]) {
    if (shared === null || chosen.length === 0) {
      return;
    }

    const known = new Map(files.map((file) => [file.name.toLowerCase(), file.name]));
    const into = target.current;

    target.current = null;

    const fresh: { name: string; file: File }[] = [];
    const there: { name: string; file: File }[] = [];
    const refused: string[] = [];

    for (const file of chosen) {
      const name = into !== null ? into.name : known.get(sharedName(file.name).toLowerCase()) ?? sharedName(file.name);
      const why = refusal(name, file, shared.limits);

      if (why !== null) {
        refused.push(why);
      } else {
        (known.has(name.toLowerCase()) ? there : fresh).push({ name, file });
      }
    }

    setProblems(refused);
    setReplacing(there);

    await send(fresh, refused);
  }

  async function send(chosen: { name: string; file: File }[], refused: string[] = []) {
    if (chosen.length === 0) {
      return;
    }

    setBusy(true);

    const problems = [...refused];
    let taken = 0;

    try {
      for (const { name, file } of chosen) {
        const answer = await share(token, name, file);

        if (typeof answer === 'string') {
          problems.push(answer);
        } else {
          taken++;
        }
      }

      if (taken > 0) {
        toast(taken === 1 ? 'The file is shared.' : `${taken} files are shared.`, 'success');
      }

      await load();
    } catch (error) {
      if (refusedToken(error)) {
        deny();
      } else {
        problems.push('The file could not be sent.');
      }
    } finally {
      setProblems(problems);
      setBusy(false);
    }
  }

  async function remove(file: SharedFile) {
    setBusy(true);

    try {
      await api.admin.deleteSharedFile(token, file.name);

      toast(`${file.name} is deleted.`, 'success');
    } catch (error) {
      if (refusedToken(error) && !(error instanceof ApiError && error.kind)) {
        deny();
      } else {
        toast(error instanceof ApiError ? error.message : 'The file could not be deleted.', 'error');
      }
    } finally {
      setBusy(false);
      setDeleting(null);

      await load();
    }
  }

  const pick = (into: SharedFile | null) => {
    target.current = into;
    input.current?.click();
  };

  return (
    <Section
      title="Shared files"
      hint="Fonts, pictures, style sheets and texts of this installation's own: for its branding, for the guide agents read, and for the apps built here. Public, each at its address."
      actions={shared && (
        <button type="button" onClick={() => pick(null)} disabled={busy} className={`btn-primary ${actionButton}`}>
          {busy ? <IconSpinner className="h-4 w-4" /> : <IconUpload className="h-4 w-4" />}
          Upload
        </button>
      )}
    >
      {shared === null ? (
        <Reading what="the shared files" failure={failure} />
      ) : (
        <div className="space-y-6">
          <style>{files.filter((file) => file.kind === 'font').map((file) => faces(specimen(file), [file])).join('')}</style>

          <div
            onDragOver={(event) => {
              event.preventDefault();
              setOver(true);
            }}
            onDragLeave={() => setOver(false)}
            onDrop={(event) => {
              event.preventDefault();
              setOver(false);
              target.current = null;
              void add([...event.dataTransfer.files]);
            }}
            className={over ? 'outline-dashed outline-2 outline-offset-4 outline-accent-500' : ''}
          >
            {files.length === 0 ? (
              <Empty>Nothing is shared yet. Upload a file, or drop some here.</Empty>
            ) : (
              <ul className="surface divide-y divide-grey-300 dark:divide-ink-800">
                {files.map((file) => (
                  <Entry key={file.name} file={file} busy={busy} onReplace={() => pick(file)} onDelete={() => setDeleting(file)} />
                ))}
              </ul>
            )}

            <p className="mt-2 text-xs tabular-nums text-slate-500">
              {plural(files.length, 'file')} of {shared.limits.files} · {bytes(shared.bytes)} of {bytes(shared.limits.totalBytes)} ·{' '}
              {bytes(shared.limits.bytes)} each at most
            </p>

            {problems.map((problem) => (
              <p key={problem} className="mt-1.5 text-xs text-red-500">{problem}</p>
            ))}
          </div>

          <input
            ref={input}
            type="file"
            multiple
            accept={Object.values(shared.limits.types).flat().map((end) => `.${end}`).join(',')}
            className="sr-only"
            tabIndex={-1}
            onChange={(event) => {
              void add([...(event.target.files ?? [])].slice(0, target.current === null ? undefined : 1));
              event.target.value = '';
            }}
          />

          <Card title="What for">
            <ul className="list-disc space-y-1.5 pl-5 text-[13px] text-slate-600 dark:text-slate-400">
              <li>
                <Link to="/admin/branding" className="text-accent-600 hover:underline dark:text-accent-400">Branding</Link>: the fonts the pages are
                set in, and a picture for the logo.
              </li>
              <li>
                <Link to="/admin/guide" className="text-accent-600 hover:underline dark:text-accent-400">Guide</Link>: link to one as{' '}
                <code className="font-mono text-[12px]">shared/house-style.md</code>. Agents are told of every file, read the texts and style
                sheets with <code className="font-mono text-[12px]">platform_guide</code> - the build agent too - and link to the rest.
              </li>
              <li>Apps: any page may load one by its address. One that is replaced is new in every app within a minute; one that is deleted is gone from them.</li>
            </ul>
            <p className="mt-3 text-[13px] text-slate-500">
              {Object.values(shared.limits.types).flat().map((end) => `.${end}`).join(' ')} - no pages and no scripts, since they are served
              from this address.
            </p>
          </Card>

          <Card title="For agents">
            <p className="text-[13px] text-slate-600 dark:text-slate-400">
              An agent you give the admin token does the same: <code className="font-mono text-[12px]">PUT /api/v1/admin/shared/&#123;name&#125;</code> with
              the file as the body, or <code className="font-mono text-[12px]">write_shared_file</code> on the operator&apos;s MCP endpoint,{' '}
              <code className="font-mono text-[12px]">/admin/mcp</code>.
            </p>
          </Card>
        </div>
      )}

      <Dialog
        title={replacing.length === 1 ? `Replace ${replacing[0].name}?` : `Replace ${replacing.length} files?`}
        open={replacing.length > 0}
        onClose={() => setReplacing([])}
        footer={
          <>
            <button type="button" onClick={() => setReplacing([])} className="btn-ghost">
              Keep {replacing.length === 1 ? 'it' : 'them'}
            </button>
            <button
              type="button"
              onClick={() => {
                const chosen = replacing;

                setReplacing([]);
                void send(chosen, problems);
              }}
              disabled={busy}
              className="btn-primary"
            >
              Replace
            </button>
          </>
        }
      >
        {replacing.length > 1 && <p className="break-words font-mono text-[13px]">{replacing.map((one) => one.name).join(', ')}</p>}
        <p className="text-slate-600 dark:text-slate-400">
          Whatever uses {replacing.length === 1 ? 'it' : 'them'} - the branding, the guide, every app that links to{' '}
          {replacing.length === 1 ? 'it' : 'one'} - has the new one from now on. What is there is not kept.
        </p>
      </Dialog>

      <Dialog
        title={`Delete ${deleting?.name}?`}
        open={deleting !== null}
        onClose={() => setDeleting(null)}
        footer={
          <>
            <button type="button" onClick={() => setDeleting(null)} className="btn-ghost">Keep it</button>
            <button type="button" onClick={() => deleting && void remove(deleting)} disabled={busy} className="btn-danger">
              {busy && <IconSpinner />}
              Delete
            </button>
          </>
        }
      >
        <p className="text-slate-600 dark:text-slate-400">For good. An app that links to it goes without.</p>
      </Dialog>
    </Section>
  );
}

/** What a font is called on this page, to show it in itself. */
const specimen = (file: SharedFile) => `Lambda Shared ${file.name}`;

/** A shared file: what it looks like, what it is, what uses it, and what may be done with it. */
function Entry({ file, busy, onReplace, onDelete }: { file: SharedFile; busy: boolean; onReplace: () => void; onDelete: () => void }) {
  const [copied, setCopied] = useState(false);

  const address = `${window.location.origin}${file.path}`;
  const uses = file.uses ?? [];

  async function copy() {
    try {
      await navigator.clipboard.writeText(address);
    } catch {
      return;
    }

    setCopied(true);
    window.setTimeout(() => setCopied(false), 1600);
  }

  return (
    <li className="flex flex-wrap items-center gap-x-4 gap-y-2 px-4 py-3 text-[13px]">
      <span className="flex h-12 w-12 shrink-0 items-center justify-center border border-grey-300 bg-grey-50 dark:border-ink-800 dark:bg-ink-850">
        {file.kind === 'picture' ? (
          <img src={file.versionedPath} alt="" className="max-h-10 max-w-10 object-contain" />
        ) : file.kind === 'font' ? (
          <span
            aria-hidden="true"
            className="text-xl leading-none"
            style={{ fontFamily: `"${specimen(file)}"`, fontWeight: file.font ? Math.min(Math.max(400, file.font.from), file.font.to) : 400, fontStyle: file.font?.italic ? 'italic' : 'normal' }}
          >
            Ag
          </span>
        ) : (
          <IconFile className="h-5 w-5 text-slate-400" />
        )}
      </span>

      <div className="min-w-[10rem] flex-1">
        <p className="break-all font-mono text-sm">{file.name}</p>
        <p className="mt-0.5 text-slate-500">
          {file.kind === 'font' ? drawn(file) : file.type.split(';')[0]} · {bytes(file.bytes)} · <Ago at={file.updated} />
        </p>
      </div>

      {uses.length > 0 && (
        <p className="flex flex-wrap gap-1.5">
          {uses.map((use) => (
            <Link key={use} to={USES[use].to} className="chip rounded-full bg-accent-500/10 text-accent-700 hover:bg-accent-500/20 dark:bg-accent-400/10 dark:text-accent-400">
              {USES[use].label}
            </Link>
          ))}
        </p>
      )}

      <div className="ml-auto flex shrink-0 items-center gap-0.5">
        <button type="button" onClick={() => void copy()} className="btn-ghost !px-2 !py-1 text-xs" title={address}>
          {copied ? <IconCheck className="h-4 w-4" /> : <IconCopy className="h-4 w-4" />}
          <span className="hidden sm:inline">{copied ? 'Copied' : 'Address'}</span>
          <span className="sr-only sm:hidden">Copy the address of {file.name}</span>
        </button>

        <a href={file.versionedPath} target="_blank" rel="noreferrer" className="btn-ghost !px-2 !py-1 text-xs" title="Open it">
          <IconExternal className="h-4 w-4" />
          <span className="sr-only">Open {file.name}</span>
        </a>

        <button type="button" onClick={onReplace} disabled={busy} className="btn-ghost !px-2 !py-1 text-xs" title="Replace it with another file">
          <IconUpload className="h-4 w-4" />
          <span className="sr-only">Replace {file.name}</span>
        </button>

        <button
          type="button"
          onClick={onDelete}
          disabled={busy || uses.length > 0}
          className="btn-ghost !px-2 !py-1 text-xs"
          title={uses.length > 0 ? `In use: ${uses.map((use) => USES[use].label).join(', ')}` : 'Delete it'}
        >
          <IconTrash className="h-4 w-4" />
          <span className="sr-only">Delete {file.name}</span>
        </button>
      </div>
    </li>
  );
}
