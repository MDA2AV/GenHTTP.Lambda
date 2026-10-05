import { useState } from 'react';

import { ApiError, api } from '../api';
import { CloneAddress, Command, Popover } from '../components/Clone';
import { IconBranch, IconCode, IconDownload, IconSpinner } from '../components/Icons';
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
 * Taking the code away, the way a repository page offers it: cloned with
 * git - every version, as the commits of main - or the version being read
 * as a project, with how to run it. The way out every lambda has, offered to
 * everybody once its owner published it.
 */
export function CodeMenu({ publicKey, version, root, bytes, gitUrl, oldest, newest }: {
  publicKey: string;
  version: number;
  /** The folder the project unpacks into, once the version is known. */
  root: string | null;
  /** How large the zip is, once it is known. */
  bytes: number | null;
  /** Where it is cloned from. */
  gitUrl: string;
  /** The oldest version kept, and the newest - what a clone has as tags. */
  oldest: number;
  newest: number;
}) {
  const t = useSourceT();
  const said = t.download;
  const cloning = t.clone;
  const format = useFormat();
  const { state, start } = useDownload(publicKey, version);

  const folder = root ?? publicKey;
  const tag = folder.toLowerCase();

  const words = { copy: said.copy, copied: said.copied };

  return (
    <Popover
      label={<><IconCode className="h-4 w-4" />{cloning.button}</>}
      title={cloning.title}
      button="inline-flex h-9 items-center gap-1.5 rounded-full border border-slate-300 px-3.5 text-sm font-medium text-slate-700 hover:border-slate-400 hover:bg-slate-50 dark:border-ink-700 dark:text-slate-200 dark:hover:bg-ink-850"
    >
      <p className="flex items-center gap-2 text-sm font-semibold"><IconBranch className="h-4 w-4 text-slate-400" />{cloning.title}</p>
      <p className="mt-1.5 text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">{cloning.what(oldest, newest)}</p>

      <div className="mt-3"><CloneAddress url={gitUrl} words={words} /></div>

      <Command text={`git clone ${gitUrl}\ncd ${publicKey}\ndotnet run`} words={words} />

      <p className="mt-2 text-[12px] leading-relaxed text-slate-500">{cloning.readOnly}</p>

      <div className="mt-5 border-t border-slate-200 pt-4 dark:border-ink-800">
        <p className="text-sm font-semibold">{said.title(version)}</p>
        <p className="mt-1.5 text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">{said.what}</p>

        <button type="button" onClick={start} disabled={state !== 'idle'} className="btn-primary mt-4 w-full">
          {state === 'idle' ? <IconDownload className="h-4 w-4" /> : <IconSpinner />}
          {state === 'idle' ? said.zip : said.preparing}
          {state === 'idle' && bytes != null && <span className="font-normal opacity-80">· {format.size(bytes)}</span>}
        </button>

        {state === 'slow' && <p className="mt-2 text-xs text-slate-500">{said.slow}</p>}
      </div>

      <div className="mt-5 border-t border-slate-200 pt-4 dark:border-ink-800">
        <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">{said.run}</p>

        <p className="mt-2 text-[13px] text-slate-600 dark:text-slate-400">{said.local}</p>
        <Command text={`cd ${folder}\ndotnet run`} words={words} />

        <p className="mt-3 text-[13px] text-slate-600 dark:text-slate-400">{said.container}</p>
        <Command text={`docker build -t ${tag} .\ndocker run -p 8080:8080 ${tag}`} words={words} />

        <p className="mt-3 text-[13px] leading-relaxed text-slate-600 dark:text-slate-400">{said.agent}</p>
      </div>
    </Popover>
  );
}
