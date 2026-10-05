import { useEffect, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, isDemo, type LambdaFile, type VersionContent, type VersionInfo } from '../api';
import { IconChevronDown, IconSpinner } from '../components/Icons';
import { useEditorT } from '../i18n';
import { ChangeList } from './Changes';
import type { Control } from './context';
import { AgentMark, Ago, Empty, Quote, Section } from './ui';

/**
 * Every version, newest first: what it changed in a line, and - opened - what
 * was asked for and the difference to the one before.
 *
 * A link can name the version to open, with ?version= - which is how the
 * Change section shows what the agent just did.
 *
 * Versions never change once saved, so each opened one offers to start a
 * feature from it: that is where a change is worked on, and a feature
 * becomes the next version once it is merged.
 */
export function VersionsTab({ control }: { control: Control }) {
  const t = useEditorT();
  const { versions, lambda } = control;
  const [params] = useSearchParams();
  const asked = Number(params.get('version')) || null;
  const [open, setOpen] = useState<number | null>(asked);

  // a link followed while the section is already open still opens its version
  useEffect(() => {
    if (asked != null) {
      setOpen(asked);
    }
  }, [asked]);

  return (
    <Section title={t.frame.sections.versions} hint={t.versions.hint(control.summary?.limits.versions ?? 50)}>
      {versions.length === 0 ? (
        <Empty>{t.versions.none}</Empty>
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
              reveal={asked === version.version}
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
  reveal,
}: {
  control: Control;
  version: VersionInfo;
  previous?: VersionInfo;
  live: boolean;
  latest: boolean;
  open: boolean;
  onToggle: () => void;
  /** Whether the address asked for this version, so it is scrolled to. */
  reveal: boolean;
}) {
  const said = useEditorT().versions;
  const row = useRef<HTMLLIElement>(null);

  useEffect(() => {
    if (reveal) {
      row.current?.scrollIntoView({ block: 'start', behavior: 'smooth' });
    }
  }, [reveal]);

  return (
    <li ref={row} className="scroll-mt-4">
      <div className="group flex items-center gap-3 py-3">
        <button type="button" onClick={onToggle} className="flex min-w-0 flex-1 items-center gap-3 text-left" aria-expanded={open}>
          <IconChevronDown className={`h-4 w-4 shrink-0 text-slate-400 transition-transform ${open ? '' : '-rotate-90'}`} />
          <span className="w-8 shrink-0 text-[13px] tabular-nums text-slate-500">{version.version}</span>
          <span className="min-w-0 flex-1 truncate text-[15px]" title={version.change ?? undefined}>
            {version.change ?? <span className="text-slate-400">{said.noDescription}</span>}
          </span>
        </button>

        <span className="flex shrink-0 items-center gap-3 text-[13px] text-slate-500">
          {live && (
            <span className="flex items-center gap-1.5 text-emerald-600 dark:text-emerald-400">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
              {said.online}
            </span>
          )}
          <AgentMark origin={version.origin} git />
          <Ago at={version.created} className="hidden w-24 text-right sm:inline" />
        </span>

        <span className="w-20 shrink-0 text-right">
          {!live && !isDemo(control.lambda.tier) && (
            <button
              type="button"
              onClick={() => control.deploy(version.version)}
              disabled={control.busy !== null}
              className="text-[13px] font-medium text-accent-500 opacity-70 hover:underline group-hover:opacity-100 disabled:opacity-40"
              title={latest ? said.putOnline : said.rollBackTitle}
            >
              {latest ? said.deploy : said.rollBack}
            </button>
          )}
        </span>
      </div>

      {open && <Detail control={control} version={version} previous={previous} />}
    </li>
  );
}

function Detail({ control, version, previous }: { control: Control; version: VersionInfo; previous?: VersionInfo }) {
  const said = useEditorT().versions;
  const [sides, setSides] = useState<{ before: LambdaFile[]; after: LambdaFile[] } | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;

    Promise.all([
      api.version(control.privateKey, version.version),
      previous ? api.version(control.privateKey, previous.version).catch(() => null) : Promise.resolve(null as VersionContent | null),
    ])
      .then(([after, before]) => alive && setSides({ before: before?.files ?? [], after: after.files }))
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.readFailed));

    return () => {
      alive = false;
    };
  }, [control.privateKey, version.version, previous, said]);

  return (
    <div className="space-y-4 pb-5 pl-7 sm:pl-[3.75rem]">
      {version.specification && <Quote>{version.specification}</Quote>}

      {failure ? (
        <p className="text-sm text-red-500">{failure}</p>
      ) : sides === null ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <IconSpinner /> {said.comparing}
        </div>
      ) : (
        <ChangeList before={sides.before} after={sides.after} theme={control.theme} empty={previous ? said.unchanged : said.first} />
      )}

      <div className="flex flex-wrap gap-4 text-[13px]">
        <button type="button" onClick={() => control.browse(version.version)} className="text-accent-500 hover:underline">
          {said.browse}
        </button>
        <button type="button" onClick={() => control.openContext('docs', version.version)} className="text-accent-500 hover:underline">
          {said.docs}
        </button>
        <button type="button" onClick={() => control.edit(version.version)} className="text-accent-500 hover:underline">
          {said.edit}
        </button>
        {!isDemo(control.lambda.tier) && (
          <button type="button" onClick={() => control.startFeature(version.version)} className="text-accent-500 hover:underline"
                  title={said.featureTitle}>
            {said.feature}
          </button>
        )}
      </div>
    </div>
  );
}
