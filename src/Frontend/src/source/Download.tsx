import { useEffect, useRef, useState } from 'react';

import { ApiError, api } from '../api';
import { IconCheck, IconChevronDown, IconCopy, IconDownload, IconSpinner } from '../components/Icons';
import { useToast } from '../components/Toast';
import { useSourceT } from '../i18n';
import { useFormat } from './format';

type Preparing = 'idle' | 'preparing' | 'slow';

/**
 * Downloads a version as a project.
 *
 * A version is packed the first time anybody asks for it, which for a large
 * one takes a while - and a link straight to the zip would show nothing at
 * all in that time. So the version is asked for first, with the page saying
 * that the project is being prepared, and the download starts once it is
 * ready, which it then is at once.
 */
function useDownload(publicKey: string, version: number) {
  const said = useSourceT().download;
  const toast = useToast();

  const [state, setState] = useState<Preparing>('idle');

  async function start() {
    if (state !== 'idle') {
      return;
    }

    setState('preparing');

    const slow = window.setTimeout(() => setState('slow'), 700);

    try {
      await api.sources.tree(publicKey, version);

      const link = document.createElement('a');

      link.href = api.sources.zipUrl(publicKey, version);
      link.download = '';
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (error) {
      toast(error instanceof ApiError && error.status !== 404 ? error.message : said.failed, 'error');
    } finally {
      window.clearTimeout(slow);
      setState('idle');
    }
  }

  return { state, start };
}

/** A version's download as a word in a line, for the history. */
export function DownloadLink({ publicKey, version }: { publicKey: string; version: number }) {
  const said = useSourceT().download;
  const { state, start } = useDownload(publicKey, version);

  return (
    <button type="button" onClick={start} disabled={state !== 'idle'} className="inline-flex items-center gap-1.5 text-accent-600 hover:underline disabled:no-underline dark:text-accent-400">
      {state === 'idle' ? <IconDownload className="h-3.5 w-3.5" /> : <IconSpinner className="h-3.5 w-3.5" />}
      {state === 'idle' ? said.zip : said.preparing}
    </button>
  );
}

/**
 * Taking the code away: the version being read as a project, what is in it
 * and what is not, and how to run it - the way out every lambda has, offered
 * to everybody once its owner published it.
 */
export function DownloadMenu({ publicKey, version, root, bytes }: {
  publicKey: string;
  version: number;
  /** The folder the project unpacks into, once the version is known. */
  root: string | null;
  /** How large the zip is, once it is known. */
  bytes: number | null;
}) {
  const said = useSourceT().download;
  const format = useFormat();
  const { state, start } = useDownload(publicKey, version);

  const [open, setOpen] = useState(false);
  const host = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    const onKey = (event: KeyboardEvent) => event.key === 'Escape' && setOpen(false);

    const onPointer = (event: PointerEvent) => {
      if (!host.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    };

    document.addEventListener('keydown', onKey);
    document.addEventListener('pointerdown', onPointer);

    return () => {
      document.removeEventListener('keydown', onKey);
      document.removeEventListener('pointerdown', onPointer);
    };
  }, [open]);

  const folder = root ?? publicKey;
  const tag = folder.toLowerCase();

  return (
    <div ref={host} className="relative">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        aria-expanded={open}
        aria-haspopup="dialog"
        className="inline-flex h-9 items-center gap-1.5 rounded-full border border-slate-300 px-3.5 text-sm font-medium text-slate-700 hover:border-slate-400 hover:bg-slate-50 dark:border-ink-700 dark:text-slate-200 dark:hover:bg-ink-850"
      >
        <IconDownload className="h-4 w-4" />
        {said.button}
        <IconChevronDown className="h-3.5 w-3.5 text-slate-400" />
      </button>

      {open && (
        <div role="dialog" aria-label={said.title(version)} className="surface absolute right-0 top-full z-40 mt-2 w-[min(23rem,calc(100vw-2.5rem))] p-4 shadow-lg">
          <p className="text-sm font-semibold">{said.title(version)}</p>
          <p className="mt-1.5 text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">{said.what}</p>

          <button type="button" onClick={start} disabled={state !== 'idle'} className="btn-primary mt-4 w-full">
            {state === 'idle' ? <IconDownload className="h-4 w-4" /> : <IconSpinner />}
            {state === 'idle' ? said.zip : said.preparing}
            {state === 'idle' && bytes != null && <span className="font-normal opacity-80">· {format.size(bytes)}</span>}
          </button>

          {state === 'slow' && <p className="mt-2 text-xs text-slate-500">{said.slow}</p>}

          <div className="mt-5 border-t border-slate-200 pt-4 dark:border-ink-800">
            <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">{said.run}</p>

            <p className="mt-2 text-[13px] text-slate-600 dark:text-slate-400">{said.local}</p>
            <Command text={`cd ${folder}\ndotnet run`} />

            <p className="mt-3 text-[13px] text-slate-600 dark:text-slate-400">{said.container}</p>
            <Command text={`docker build -t ${tag} .\ndocker run -p 8080:8080 ${tag}`} />

            <p className="mt-3 text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">{said.agent}</p>
          </div>
        </div>
      )}
    </div>
  );
}

/** A command to type, with a way to copy it. */
function Command({ text }: { text: string }) {
  const said = useSourceT().download;
  const [copied, setCopied] = useState(false);

  return (
    <div className="group relative mt-1.5">
      <pre className="overflow-x-auto border border-slate-200 bg-slate-50 px-3 py-2 pr-10 font-mono text-[12px] leading-5 dark:border-ink-800 dark:bg-ink-950">
        {text}
      </pre>
      <button
        type="button"
        onClick={async () => {
          try {
            await navigator.clipboard.writeText(text);
            setCopied(true);
            window.setTimeout(() => setCopied(false), 1500);
          } catch {
            // a clipboard that refuses is not worth an error
          }
        }}
        className="absolute right-1.5 top-1.5 p-1 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
        aria-label={copied ? said.copied : said.copy}
        title={copied ? said.copied : said.copy}
      >
        {copied ? <IconCheck className="h-3.5 w-3.5 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5" />}
      </button>
    </div>
  );
}
