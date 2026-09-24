import { useEffect, useState } from 'react';

import { ApiError, api, type VersionContent, type VersionInfo } from '../api';
import { IconChevronDown, IconSpinner } from '../components/Icons';
import type { Control } from './context';
import { compare, type DiffLine, type FileDiff } from './diff';
import { AgentMark, Ago, Empty, Quote, Section } from './ui';

/**
 * Every version, newest first: what it changed in a line, and - opened - what
 * was asked for and the difference to the one before.
 */
export function VersionsTab({ control }: { control: Control }) {
  const { versions, lambda } = control;
  const [open, setOpen] = useState<number | null>(null);

  return (
    <Section
      title="Versions"
      hint={
        <>
          Each version keeps what was asked for and what it changed, where whoever wrote it said so. The oldest are
          removed once there are more than {control.summary?.limits.versions ?? 50}; the one online never is.
        </>
      }
    >
      {versions.length === 0 ? (
        <Empty>No versions yet.</Empty>
      ) : (
        <ol className="divide-y divide-slate-200 border-y border-slate-200 dark:divide-ink-800 dark:border-ink-800">
          {versions.map((version, index) => (
            <Row
              key={version.version}
              control={control}
              version={version}
              previous={versions[index + 1]}
              live={version.version === lambda.activeVersion}
              latest={index === 0}
              open={open === version.version}
              onToggle={() => setOpen((was) => (was === version.version ? null : version.version))}
            />
          ))}
        </ol>
      )}
    </Section>
  );
}

function Row({
  control,
  version,
  previous,
  live,
  latest,
  open,
  onToggle,
}: {
  control: Control;
  version: VersionInfo;
  previous?: VersionInfo;
  live: boolean;
  latest: boolean;
  open: boolean;
  onToggle: () => void;
}) {
  return (
    <li>
      <div className="group flex items-center gap-3 py-3">
        <button type="button" onClick={onToggle} className="flex min-w-0 flex-1 items-center gap-3 text-left" aria-expanded={open}>
          <IconChevronDown className={`h-4 w-4 shrink-0 text-slate-400 transition-transform ${open ? '' : '-rotate-90'}`} />
          <span className="w-8 shrink-0 text-[13px] tabular-nums text-slate-500">{version.version}</span>
          <span className="min-w-0 flex-1 truncate text-[15px]" title={version.change ?? undefined}>
            {version.change ?? <span className="text-slate-400">No description</span>}
          </span>
        </button>

        <span className="flex shrink-0 items-center gap-3 text-[13px] text-slate-500">
          {live && (
            <span className="flex items-center gap-1.5 text-emerald-600 dark:text-emerald-400">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
              online
            </span>
          )}
          <AgentMark origin={version.origin} />
          <Ago at={version.created} className="hidden w-24 text-right sm:inline" />
        </span>

        <span className="w-20 shrink-0 text-right">
          {!live && (
            <button
              type="button"
              onClick={() => control.deploy(version.version)}
              disabled={control.busy !== null}
              className="text-[13px] font-medium text-accent-500 opacity-70 hover:underline group-hover:opacity-100 disabled:opacity-40"
              title={latest ? 'Put this version online' : 'Put this older version back online'}
            >
              {latest ? 'Deploy' : 'Roll back'}
            </button>
          )}
        </span>
      </div>

      {open && <Detail control={control} version={version} previous={previous} />}
    </li>
  );
}

function Detail({ control, version, previous }: { control: Control; version: VersionInfo; previous?: VersionInfo }) {
  const [diffs, setDiffs] = useState<FileDiff[] | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [shown, setShown] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;

    Promise.all([
      api.version(control.privateKey, version.version),
      previous ? api.version(control.privateKey, previous.version).catch(() => null) : Promise.resolve(null as VersionContent | null),
    ])
      .then(([after, before]) => {
        if (!alive) {
          return;
        }

        const result = compare(before?.files ?? [], after.files);

        setDiffs(result);

        // the first file that changed is usually the one to read
        setShown(result.find((d) => d.status !== 'same')?.name ?? null);
      })
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : 'This version could not be read.'));

    return () => {
      alive = false;
    };
  }, [control.privateKey, version.version, previous]);

  const changed = diffs?.filter((d) => d.status !== 'same') ?? [];

  return (
    <div className="space-y-4 pb-5 pl-7 sm:pl-[3.75rem]">
      {version.specification && <Quote>{version.specification}</Quote>}

      {failure ? (
        <p className="text-sm text-red-500">{failure}</p>
      ) : diffs === null ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <IconSpinner /> Comparing…
        </div>
      ) : changed.length === 0 ? (
        <p className="text-sm text-slate-500">{previous ? 'Nothing changed from the version before.' : 'The first version.'}</p>
      ) : (
        <div className="surface overflow-hidden">
          <ul className="divide-y divide-slate-200 dark:divide-ink-800">
            {changed.map((diff) => (
              <li key={diff.name}>
                <button
                  type="button"
                  onClick={() => setShown((was) => (was === diff.name ? null : diff.name))}
                  className="flex w-full items-center gap-3 px-3 py-2 text-left text-sm hover:bg-slate-50 dark:hover:bg-ink-850"
                  aria-expanded={shown === diff.name}
                >
                  <span className="min-w-0 flex-1 truncate font-mono text-[13px]">
                    {diff.name}
                    {diff.status !== 'changed' && <span className="ml-2 font-sans text-xs text-slate-500">{diff.status}</span>}
                  </span>
                  {!diff.binary && (
                    <span className="shrink-0 font-mono text-xs">
                      <span className="text-emerald-600 dark:text-emerald-400">+{diff.added}</span>{' '}
                      <span className="text-red-500 dark:text-red-400">−{diff.removed}</span>
                    </span>
                  )}
                </button>

                {shown === diff.name && <Patch diff={diff} />}
              </li>
            ))}
          </ul>
        </div>
      )}

      <div className="flex gap-4 text-[13px]">
        <button type="button" onClick={() => control.browse(version.version)} className="text-accent-500 hover:underline">
          Browse its files
        </button>
        <button type="button" onClick={() => control.edit(version.version)} className="text-accent-500 hover:underline">
          Edit from here
        </button>
      </div>
    </div>
  );
}

function Patch({ diff }: { diff: FileDiff }) {
  if (diff.binary || !diff.hunks) {
    return (
      <p className="border-t border-slate-200 px-3 py-2 text-xs text-slate-500 dark:border-ink-800">
        {diff.binary ? 'Not text, so there are no lines to compare.' : 'Too large to compare line by line.'}
      </p>
    );
  }

  return (
    <div className="max-h-[28rem] overflow-auto border-t border-slate-200 dark:border-ink-800">
      <table className="w-full border-collapse font-mono text-[12.5px] leading-5">
        <tbody>
          {diff.hunks.map((hunk, h) => (
            <Hunk key={h} lines={hunk.lines} separator={h > 0} />
          ))}
        </tbody>
      </table>
    </div>
  );
}

function Hunk({ lines, separator }: { lines: DiffLine[]; separator: boolean }) {
  return (
    <>
      {separator && (
        <tr>
          <td colSpan={3} className="bg-slate-100 px-3 text-center text-slate-400 dark:bg-ink-850">⋯</td>
        </tr>
      )}
      {lines.map((line, i) => (
        <tr key={i} className={line.kind === 'added' ? 'bg-emerald-500/10' : line.kind === 'removed' ? 'bg-red-500/10' : ''}>
          <td className="w-12 select-none px-2 text-right align-top text-slate-400">{line.number}</td>
          <td className="w-4 select-none align-top text-slate-500">
            {line.kind === 'added' ? '+' : line.kind === 'removed' ? '−' : ''}
          </td>
          <td className="whitespace-pre pr-4">{line.text || ' '}</td>
        </tr>
      ))}
    </>
  );
}
