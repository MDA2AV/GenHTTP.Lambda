import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, type LambdaFile } from '../api';
import { IconAlert, IconChevronDown, IconFolder, IconLayers, IconLock, IconPackage, IconSpinner } from '../components/Icons';
import { Markdown } from '../components/Markdown';
import { useEditorT } from '../i18n';
import { ChangeList } from './Changes';
import { CloneMenu } from './CloneMenu';
import type { Control } from './context';
import { Tree, Viewer, sizeOf } from './FileBrowser';
import { bytes } from './format';
import { assetsIn, changedNames, projectsOf, type Project } from './projects';
import { Section, pill } from './ui';
import { DEV, DEV_README, isAsset, isDevelopment } from './written';

/**
 * The development space of a version - or, opened on a draft, the draft's:
 * what its assets are built from, where a toolchain builds them.
 *
 * Written by an agent that builds it where it works, read here - the
 * asymmetric interface once more. Nothing here edits it: a change to its
 * sources is a change to nothing visitors get until it is built, and the
 * platform builds nothing, so a box to type into would promise what it does
 * not do. What is here is what somebody reviewing it asks: which projects
 * there are and what each is built with, what each installs and whether a
 * lock file pins it, where the build goes, how it is built - its README -
 * and what the version changed in it against what it changed in the assets,
 * since sources changed and nothing rebuilt, or a build changed by hand, is
 * the mistake worth catching before it goes online.
 *
 * In the full view only, and in its sidebar only where there is one.
 */
export function DevelopmentTab({ control }: { control: Control }) {
  const t = useEditorT();
  const said = t.development;
  const [params, setParams] = useSearchParams();

  const { lambda, versions } = control;
  const feature = control.feature?.info ?? null;

  const wanted = feature ? null : Number(params.get('version')) || lambda.latestVersion || versions[0]?.version || null;

  // what it is compared with: the version before it, or what a draft began from
  const index = versions.findIndex((v) => v.version === wanted);
  const previous = feature ? feature.base : index >= 0 ? versions[index + 1]?.version : undefined;

  const [files, setFiles] = useState<LambdaFile[] | null>(null);
  const [before, setBefore] = useState<LambdaFile[] | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [comparing, setComparing] = useState(false);

  const load = useCallback(async () => {
    try {
      // every file, not the space alone: where its build goes is among the assets
      const [mine, theirs] = await Promise.all([
        feature
          ? api.feature.get(control.privateKey, feature.key).then((content) => content.files)
          : wanted != null
            ? api.version(control.privateKey, wanted).then((content) => content.files)
            : Promise.resolve([] as LambdaFile[]),
        previous != null
          ? api.version(control.privateKey, previous).then((content) => content.files).catch(() => null)
          : Promise.resolve(null),
      ]);

      setFiles(mine);
      setBefore(theirs);
      setFailure(null);
    } catch (error) {
      setFailure(error instanceof ApiError ? error.message : said.readFailed);
    }
  }, [control.privateKey, feature?.key, wanted, previous, said]);

  useEffect(() => {
    setFiles(null);
    setComparing(false);
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [control.privateKey, feature?.key, wanted, previous]);

  // a draft saved elsewhere - by its agent, most likely - is read again in place
  useEffect(() => {
    if (feature && files) {
      load();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [feature?.revision]);

  const all = useMemo(() => files ?? [], [files]);
  const space = useMemo(() => all.filter((file) => isDevelopment(file.name)), [all]);
  const projects = useMemo(() => projectsOf(all), [all]);
  const readme = space.find((file) => file.name === DEV_README && file.encoding !== 'base64') ?? null;

  /** What it changed here, against what it changed in the assets - which is what says whether it was built. */
  const changes = useMemo(() => {
    if (before == null) {
      return null;
    }

    const had = before.filter((file) => isDevelopment(file.name));
    const here = changedNames(had, space);
    const assets = changedNames(before.filter((file) => isAsset(file.name)), all.filter((file) => isAsset(file.name)));

    const built = projects.map((project) => project.into).filter((folder): folder is string => folder != null);

    return {
      had,
      here,
      assets,
      first: had.length === 0,
      rebuiltByHand: here.length === 0 ? built.find((folder) => assets.some((name) => name.startsWith(folder))) : undefined,
    };
  }, [before, space, all, projects]);

  const chosen = params.get('file');
  const browsing = space.length > 0 && (params.get('view') === 'files' || chosen != null);
  const selected = browsing ? (space.find((file) => file.name === `${DEV}${chosen}`) ?? readme ?? space[0]) : null;

  /** Somewhere else within the section, keeping the version. */
  function go(next: Record<string, string>) {
    const query = new URLSearchParams();

    if (params.get('version') && !feature) {
      query.set('version', params.get('version')!);
    }

    Object.entries(next).forEach(([key, value]) => query.set(key, value));

    setParams(query, { replace: true });
  }

  const code = (text: string) => <code className="font-mono text-[13px]">{text}</code>;

  return (
    <Section
      title={said.title}
      hint={said.hint}
      actions={<CloneMenu lambda={lambda} feature={feature} />}
      pills={
        <>
          {space.length > 0 && (
            <>
              <button type="button" aria-pressed={!browsing} onClick={() => go({})} className={pill(!browsing)}>
                {said.overview}
              </button>
              <button type="button" aria-pressed={browsing} onClick={() => go({ view: 'files' })} className={pill(browsing)}>
                {said.files}
                <span className="tabular-nums text-slate-400">{space.length}</span>
              </button>
            </>
          )}

          {!feature && versions.length > 0 && wanted != null && (
            <label className={`${pill(false)} relative cursor-pointer pr-7 sm:ml-auto`}>
              <span className="sr-only">{t.files.version}</span>
              <span>{t.files.shown(wanted, wanted === lambda.activeVersion, wanted === lambda.latestVersion)}</span>
              <IconChevronDown className="pointer-events-none absolute right-2.5 h-3.5 w-3.5" />
              <select
                value={wanted}
                onChange={(event) => setParams({ version: event.target.value }, { replace: true })}
                className="absolute inset-0 cursor-pointer opacity-0"
              >
                {versions.map((v) => (
                  <option key={v.version} value={v.version}>
                    {v.version}
                    {v.version === lambda.activeVersion ? t.files.optionOnline : ''}
                    {v.change ? ` - ${v.change.slice(0, 60)}` : ''}
                  </option>
                ))}
              </select>
            </label>
          )}
        </>
      }
    >
      {failure ? (
        <p className="text-sm text-red-500">{failure}</p>
      ) : files === null ? (
        <div className="flex items-center gap-2 py-10 text-sm text-slate-500"><IconSpinner /> {said.reading}</div>
      ) : space.length === 0 ? (
        <div className="max-w-xl py-6">
          <div className="flex h-11 w-11 items-center justify-center bg-accent-500/10 text-accent-500 dark:text-accent-400">
            <IconPackage className="h-5 w-5" />
          </div>
          <h2 className="mt-5 text-base font-semibold">{feature ? said.emptyTitleDraft : said.emptyTitle(wanted ?? 0)}</h2>
          <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{said.emptyText(code)}</p>
          <p className="mt-3 text-[13px] text-slate-500">{said.emptyHow(code)}</p>
        </div>
      ) : browsing ? (
        <div className="grid gap-5 lg:grid-cols-[17rem,1fr]">
          <nav aria-label={said.files} className="lg:max-h-[40rem] lg:overflow-y-auto">
            <Tree
              entries={space.map((file) => ({ path: file.name.slice(DEV.length), size: sizeOf(file) }))}
              selected={selected ? selected.name.slice(DEV.length) : null}
              onSelect={(path) => go({ file: path })}
              empty={said.noFiles}
            />
            <p className="mt-4 flex items-start gap-1.5 px-1 text-[12px] text-slate-500">
              <IconLock className="mt-0.5 h-3 w-3 shrink-0" />
              {said.readOnly}
            </p>
          </nav>
          <Viewer control={control} selection={selected ? { group: 'development', path: selected.name } : null} files={all} listing={null} />
        </div>
      ) : (
        <div className="max-w-5xl">
          <p className="-mt-1 mb-5 flex items-start gap-2 text-[13px] text-slate-600 dark:text-slate-400">
            <IconLayers className="mt-0.5 h-3.5 w-3.5 shrink-0 text-slate-400" />
            <span>{feature ? said.scopeDraft : said.scope(wanted ?? 0)}</span>
          </p>

          {changes && (
            <Changed
              title={feature ? said.inDraft : said.inVersion(wanted ?? 0)}
              against={previous}
              changes={changes}
              comparing={comparing}
              onCompare={() => setComparing((was) => !was)}
            >
              <ChangeList before={changes.had} after={space} theme={control.theme} empty={said.noChanges} folded />
            </Changed>
          )}

          {projects.length > 0 && (
            <section className="mt-8">
              <h2 className="text-sm font-medium">{said.projects}</h2>
              <div className="mt-3 grid gap-4 md:grid-cols-2">
                {projects.map((project) => (
                  <ProjectCard key={project.folder} project={project} files={all} onOpen={(path) => go({ file: path })} />
                ))}
              </div>
            </section>
          )}

          {/* named by its file where it is there, since it names itself in its own heading */}
          <section className="mt-10">
            {readme ? (
              <>
                <h2 className="flex items-center gap-2 border-b border-slate-200 pb-2 text-[13px] text-slate-500 dark:border-ink-800">
                  <span className="font-mono">{readme.name.slice(DEV.length)}</span>
                  <span aria-hidden="true">·</span>
                  <span>{said.readme}</span>
                </h2>
                <article className="mt-5">
                  <Markdown source={readme.code} name={readme.name} files={space} theme={control.theme}
                            onOpen={(name) => name.startsWith(DEV) && go({ file: name.slice(DEV.length) })} />
                </article>
              </>
            ) : (
              <>
                <h2 className="text-sm font-medium">{said.readme}</h2>
                <p className="mt-3 max-w-2xl border-l-2 border-slate-300 py-1 pl-4 text-[13px] text-slate-600 dark:border-ink-700 dark:text-slate-400">
                  {said.noReadme(code)}
                </p>
              </>
            )}
          </section>
        </div>
      )}
    </Section>
  );
}

/**
 * What the version changed in the space against the assets, in a sentence -
 * the one to read before putting it online - and the files on request.
 */
function Changed({ title, against, changes, comparing, onCompare, children }: {
  title: string;
  against?: number;
  changes: { here: string[]; assets: string[]; first: boolean; rebuiltByHand?: string };
  comparing: boolean;
  onCompare: () => void;
  children: ReactNode;
}) {
  const said = useEditorT().development;

  const folder = (name: string) => <code className="font-mono text-[12.5px]">{name}</code>;

  const [text, warn] = changes.first
    ? [said.first, false]
    : changes.here.length > 0 && changes.assets.length > 0
      ? [said.both(changes.here.length, changes.assets.length), false]
      : changes.here.length > 0
        ? [said.hereOnly(changes.here.length), true]
        : changes.rebuiltByHand
          ? [said.builtOnly(folder(changes.rebuiltByHand)), true]
          : changes.assets.length > 0
            ? [said.assetsOnly, false]
            : [said.unchanged, false];

  return (
    <section className={`border-l-2 pl-4 ${warn ? 'border-amber-500' : 'border-slate-300 dark:border-ink-700'}`}>
      <h2 className="flex flex-wrap items-baseline gap-x-2 text-sm font-medium">
        {title}
        {against != null && !changes.first && <span className="text-[13px] font-normal text-slate-500">{said.comparedWith(against)}</span>}
      </h2>
      <p className={`mt-1 flex items-start gap-2 text-[13px] ${warn ? 'text-amber-700 dark:text-amber-400' : 'text-slate-600 dark:text-slate-400'}`}>
        {warn && <IconAlert className="mt-0.5 h-4 w-4 shrink-0" />}
        <span>{text}</span>
      </p>
      {changes.here.length > 0 && (
        <>
          <button type="button" onClick={onCompare} aria-expanded={comparing} className="mt-2 text-[13px] text-accent-500 hover:underline">
            {comparing ? said.hideChanges : said.showChanges}
          </button>
          {comparing && <div className="mt-3">{children}</div>}
        </>
      )}
    </section>
  );
}

/** One project of the space, as somebody reviewing it reads it. */
function ProjectCard({ project, files, onOpen }: { project: Project; files: LambdaFile[]; onOpen: (path: string) => void }) {
  const said = useEditorT().development;
  const [shown, setShown] = useState(false);

  const into = project.into ? assetsIn(files, project.into, sizeOf) : null;
  const marker = { npm: 'package.json', deno: 'deno.json', cargo: 'Cargo.toml', go: 'go.mod', python: 'pyproject.toml', dotnet: '', php: 'composer.json', ruby: 'Gemfile', maven: 'pom.xml', gradle: 'build.gradle', make: 'Makefile' }[project.kind];
  const opened = marker && files.some((file) => file.name === `${DEV}${project.folder}${marker}`) ? `${project.folder}${marker}` : null;

  const row = 'grid grid-cols-[6.5rem,minmax(0,1fr)] gap-x-3 gap-y-1 text-[13px]';
  const term = 'text-slate-500';

  return (
    <article className="surface flex flex-col gap-3 p-4">
      <header className="flex items-start gap-2">
        <IconFolder className="mt-0.5 h-4 w-4 shrink-0 text-slate-400" />
        <div className="min-w-0 flex-1">
          {opened ? (
            <button type="button" onClick={() => onOpen(opened)} className="truncate font-mono text-[13.5px] hover:text-accent-600 hover:underline dark:hover:text-accent-400">
              {project.folder || said.atTheTop}
            </button>
          ) : (
            <span className="truncate font-mono text-[13.5px]">{project.folder || said.atTheTop}</span>
          )}
          {project.name && <p className="truncate text-xs text-slate-500">{project.name}</p>}
        </div>
        <span className="shrink-0 rounded-full border border-slate-200 px-2 py-px text-[11px] text-slate-500 dark:border-ink-800">
          {said.kinds[project.kind]}{project.manager && project.manager !== 'npm' ? ` · ${project.manager}` : ''}
        </span>
      </header>

      <dl className="space-y-2">
        {project.stack.length > 0 && (
          <div className={row}>
            <dt className={term}>{said.builtWith}</dt>
            <dd className="flex flex-wrap gap-1">
              {project.stack.map((part) => (
                <span key={part} className="rounded-full bg-accent-500/10 px-2 py-px text-[12px] text-accent-700 dark:text-accent-400">{part}</span>
              ))}
            </dd>
          </div>
        )}

        {project.kind === 'npm' && (
          <div className={row}>
            <dt className={term}>{said.build}</dt>
            <dd className="min-w-0">
              {project.command ? (
                <>
                  <code className="font-mono text-[12.5px]">{project.command}</code>
                  {project.script && <span className="block truncate font-mono text-[12px] text-slate-500" title={project.script}>{project.script}</span>}
                </>
              ) : (
                <span className="text-slate-500">{said.noBuild}</span>
              )}
            </dd>
          </div>
        )}

        {project.into && into && (
          <div className={row}>
            <dt className={term}>{said.into}</dt>
            <dd className="text-slate-700 dark:text-slate-300">
              {into.files > 0
                ? said.intoAssets(<code className="font-mono text-[12.5px]">{project.into}</code>, into.files, bytes(into.bytes))
                : said.intoNothing(<code className="font-mono text-[12.5px]">{project.into}</code>)}
            </dd>
          </div>
        )}

        {project.dependencies.length + project.tooling.length > 0 && (
          <div className={row}>
            <dt className={term}>{said.packages}</dt>
            <dd>
              <span className="text-slate-700 dark:text-slate-300">{said.packagesCount(project.dependencies.length, project.tooling.length)}</span>{' '}
              <button type="button" onClick={() => setShown((was) => !was)} aria-expanded={shown} className="text-accent-500 hover:underline">
                {shown ? said.hidePackages : said.showPackages}
              </button>
            </dd>
          </div>
        )}
      </dl>

      {shown && (
        <div className="grid gap-4 border-t border-slate-200 pt-3 text-[12.5px] sm:grid-cols-2 dark:border-ink-800">
          {([[said.runtime, project.dependencies], [said.tooling, project.tooling]] as const).map(([title, list]) =>
            list.length > 0 && (
              <div key={title}>
                <h3 className="text-[11px] font-medium uppercase tracking-wide text-slate-500">{title}</h3>
                <ul className="mt-1.5 space-y-0.5 font-mono">
                  {list.map(([name, version]) => (
                    <li key={name} className="flex justify-between gap-3">
                      <span className="truncate">{name}</span>
                      <span className="shrink-0 text-slate-500">{version}</span>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
        </div>
      )}

      {(project.locked === false || !project.ignored || project.missing.length > 0) && (
        <ul className="space-y-1.5 text-[12.5px] text-amber-700 dark:text-amber-400">
          {project.missing.length > 0 && project.into && (
            <li className="flex gap-1.5">
              <IconAlert className="mt-px h-3.5 w-3.5 shrink-0" />
              <span>{said.missing(<code className="font-mono">{`${project.into}index.html`}</code>, project.missing)}</span>
            </li>
          )}
          {project.locked === false && (
            <li className="flex gap-1.5"><IconAlert className="mt-px h-3.5 w-3.5 shrink-0" />{said.noLock}</li>
          )}
          {!project.ignored && (
            <li className="flex gap-1.5"><IconAlert className="mt-px h-3.5 w-3.5 shrink-0" />{said.noIgnore}</li>
          )}
        </ul>
      )}
    </article>
  );
}
