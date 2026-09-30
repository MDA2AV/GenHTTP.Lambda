import { useEffect, useRef, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import type { SourceProject, SourceVersion } from '../api';
import { IconCheck, IconChevronDown, IconHistory, IconSpark } from '../components/Icons';
import { useSourceT } from '../i18n';
import { Link } from '../i18n/links';
import { DownloadLink } from './Download';
import { useFormat } from './format';
import { sourcePath, type SourceView } from './paths';

/**
 * Every version the lambda went through, newest first, each with the line it
 * says about what it changed - the history a visitor reads to see how the
 * app came to be, and where each version's code is read or downloaded.
 *
 * What was asked for in the owner's words is not here: that stays with the
 * owner, and what a version changed is what it says about itself.
 */
export function ChangesView({ project }: { project: SourceProject }) {
  const said = useSourceT().changes;
  const format = useFormat();

  const latest = project.versions[0]?.version;

  return (
    <div className="max-w-3xl">
      <h2 className="text-lg font-semibold tracking-tight">{said.title}</h2>
      <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{said.intro}</p>

      <ol className="relative mt-6 border-l border-slate-200 dark:border-ink-800">
        {project.versions.map((version) => (
          <li key={version.version} className="relative pb-6 pl-6 last:pb-0">
            <span
              aria-hidden="true"
              className={`absolute -left-[5px] top-1.5 h-[9px] w-[9px] rounded-full border-2 ${
                version.online
                  ? 'border-emerald-500 bg-emerald-500'
                  : 'border-slate-300 bg-white dark:border-ink-700 dark:bg-ink-950'
              }`}
            />

            <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
              <span className="font-mono text-[13px] font-semibold">v{version.version}</span>
              <span className={`text-[15px] ${version.change ? '' : 'italic text-slate-400'}`}>{version.change ?? said.noChange}</span>
            </div>

            <div className="mt-1.5 flex flex-wrap items-center gap-x-4 gap-y-1 text-[13px] text-slate-500">
              <time dateTime={version.created} title={format.moment(version.created)}>
                {format.ago(version.created)}
              </time>
              {version.origin === 'agent' && (
                <span className="inline-flex items-center gap-1 text-accent-600 dark:text-accent-400">
                  <IconSpark className="h-3.5 w-3.5" />
                  {said.agent}
                </span>
              )}
              {version.online && (
                <span className="inline-flex items-center gap-1.5 text-emerald-600 dark:text-emerald-400">
                  <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" aria-hidden="true" />
                  {said.online}
                </span>
              )}
              <span className="flex gap-3">
                <Link
                  to={sourcePath(project.source.publicKey, 'code', { version: version.version === latest ? null : version.version })}
                  className="text-accent-600 hover:underline dark:text-accent-400"
                >
                  {said.browse}
                </Link>
                <DownloadLink publicKey={project.source.publicKey} version={version.version} />
              </span>
            </div>
          </li>
        ))}
      </ol>
    </div>
  );
}

/**
 * Which version is being read, and every other one behind it - chosen here,
 * kept in the address, and carried along from one view to the next.
 */
export function VersionPicker({ project, version, view }: { project: SourceProject; version: number; view: SourceView }) {
  const said = useSourceT().versions;
  const format = useFormat();
  const navigate = useNavigate();
  const { pathname, hash } = useLocation();

  const [open, setOpen] = useState(false);
  const host = useRef<HTMLDivElement>(null);

  const latest = project.versions[0]?.version;

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

  function pick(picked: SourceVersion) {
    setOpen(false);

    // the same view of another version; a marked line means nothing in another one
    const search = picked.version === latest ? '' : `?version=${picked.version}`;

    navigate({ pathname, search, hash: view === 'code' ? '' : hash });
  }

  return (
    <div ref={host} className="relative">
      <button
        type="button"
        onClick={() => setOpen((was) => !was)}
        aria-haspopup="listbox"
        aria-expanded={open}
        title={said.choose}
        className="inline-flex items-center gap-2 rounded-full border border-slate-300 px-3 py-1.5 text-[13px] hover:border-slate-400 dark:border-ink-700 dark:hover:border-ink-600"
      >
        <IconHistory className="h-3.5 w-3.5 text-slate-400" />
        <span className="text-slate-500">{said.label}</span>
        <span className="font-mono font-semibold">{version}</span>
        {version === latest && (
          <span className="rounded-full bg-accent-500/10 px-1.5 text-[11px] text-accent-700 dark:text-accent-400">{said.newest}</span>
        )}
        <IconChevronDown className="h-3.5 w-3.5 text-slate-400" />
      </button>

      {open && (
        <div className="surface absolute right-0 top-full z-40 mt-1.5 max-h-96 w-[min(24rem,calc(100vw-2.5rem))] overflow-y-auto py-1 shadow-lg" role="listbox" aria-label={said.choose}>
          {project.versions.map((item) => (
            <button
              key={item.version}
              type="button"
              role="option"
              aria-selected={item.version === version}
              onClick={() => pick(item)}
              className="flex w-full items-start gap-3 px-3 py-2 text-left hover:bg-slate-100 dark:hover:bg-ink-850"
            >
              <span className="w-4 pt-0.5">{item.version === version && <IconCheck className="h-3.5 w-3.5 text-accent-500" />}</span>
              <span className="w-9 shrink-0 font-mono text-[13px] font-semibold">v{item.version}</span>
              <span className="min-w-0 flex-1">
                <span className={`block truncate text-[13px] ${item.change ? '' : 'italic text-slate-400'}`}>{item.change ?? said.noChange}</span>
                <span className="mt-0.5 flex items-center gap-2 text-xs text-slate-500">
                  {format.ago(item.created)}
                  {item.version === latest && <span className="text-accent-600 dark:text-accent-400">{said.newest}</span>}
                  {item.online && (
                    <span className="inline-flex items-center gap-1 text-emerald-600 dark:text-emerald-400">
                      <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" aria-hidden="true" />
                      {said.online}
                    </span>
                  )}
                </span>
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

/** That an older version is being read, and the way to the newest. */
export function OlderVersion({ project, version, view, file, page }: {
  project: SourceProject;
  version: number;
  view: SourceView;
  file: string | null;
  page: string | null;
}) {
  const said = useSourceT().versions;
  const format = useFormat();

  const newest = project.versions[0];
  const read = project.versions.find((v) => v.version === version);

  if (!newest || !read || version === newest.version) {
    return null;
  }

  return (
    <div className="mb-5 flex flex-wrap items-center gap-x-4 gap-y-2 border border-amber-500/30 bg-amber-500/[0.06] px-4 py-2.5 text-sm">
      <IconHistory className="h-4 w-4 shrink-0 text-amber-600 dark:text-amber-400" />
      <p className="min-w-0 flex-1">{said.older(version, format.ago(read.created), newest.version)}</p>
      <Link to={sourcePath(project.source.publicKey, view, { file, page })} className="btn-ghost !px-3 !py-1 text-[13px]">
        {said.toNewest}
      </Link>
    </div>
  );
}
