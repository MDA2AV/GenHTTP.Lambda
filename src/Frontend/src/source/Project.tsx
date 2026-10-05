import { useEffect, useState } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';

import { absoluteAddress, shownAddress } from '../address';
import { ApiError, api, type SourceProject, type SourceTree } from '../api';
import { IconExternal, IconSpinner } from '../components/Icons';
import { useLanguage, useSourceT } from '../i18n';
import { Link } from '../i18n/links';
import { pageMeta, usePageMeta } from '../meta';
import { CodeView } from './CodeView';
import { CodeMenu } from './Download';
import { useFormat } from './format';
import { ChangesView, OlderVersion, VersionPicker } from './History';
import { LambdaNotice, LicenseChip, StarButton } from './parts';
import { parseSourcePath, sourcePath, type SourceView } from './paths';
import { DocsView, TestsView } from './Written';

/** The versions read so far, by project and version - each is packed and listed once. */
const trees = new Map<string, SourceTree>();

/**
 * The published source of one lambda.
 *
 * What it is and where it runs at the top, with what can be done with it -
 * star it, take it away, open the app - and below that one of four views of
 * the version being read: its code, its documentation, its tests, and the
 * history of every version. The version is picked once and kept across the
 * views, in the address.
 *
 * Nothing here needs the editor key, and nothing here shows what only its
 * owner may see: no traffic, no log, no data, and not what they asked for in
 * their own words - only what each version says it changed.
 */
export function Project() {
  const { key = '', '*': rest = '' } = useParams();
  const [params] = useSearchParams();

  const said = useSourceT();
  const language = useLanguage();

  const [project, setProject] = useState<SourceProject | null>(null);
  const [ticketAt, setTicketAt] = useState(0);
  const [failure, setFailure] = useState<'missing' | string | null>(null);

  const { view, file, page } = parseSourcePath(rest);

  useEffect(() => {
    let alive = true;

    setProject(null);
    setFailure(null);

    api.sources
      .get(key)
      .then((loaded) => {
        if (alive) {
          setProject(loaded);
          setTicketAt(Date.now());
        }
      })
      .catch((error) => alive && setFailure(error instanceof ApiError && error.status === 404 ? 'missing' : error instanceof ApiError ? error.message : said.project.failed));

    return () => {
      alive = false;
    };
  }, [key, said]);

  const latest = project?.versions[0]?.version ?? project?.source.latestVersion ?? null;
  const asked = Number(params.get('version'));
  const version = project && project.versions.some((v) => v.version === asked) ? asked : latest;

  const [tree, setTree] = useState<SourceTree | null>(null);
  const [treeFailure, setTreeFailure] = useState<string | null>(null);

  useEffect(() => {
    if (version == null) {
      return;
    }

    const known = `${key}/${version}`;

    setTreeFailure(null);
    setTree(trees.get(known) ?? null);

    if (trees.has(known)) {
      return;
    }

    let alive = true;

    api.sources
      .tree(key, version)
      .then((loaded) => {
        trees.set(known, loaded);

        if (alive) {
          setTree(loaded);
        }
      })
      .catch((error) => alive && setTreeFailure(error instanceof ApiError ? error.message : said.tree.failed));

    return () => {
      alive = false;
    };
  }, [key, version, said]);

  // named after the lambda, in the words the build gave the page of a source
  const template = pageMeta('/source/:key', language);
  const name = project?.source.title ?? key;

  usePageMeta({
    title: template.title.replace('{name}', name),
    description: project?.source.about ?? project?.source.description ?? template.description?.replace('{name}', name),
    index: failure !== 'missing',
  });

  if (failure === 'missing') {
    return <Missing />;
  }

  if (failure) {
    return <p className="mx-auto max-w-xl px-5 py-24 text-center text-sm text-slate-500">{failure}</p>;
  }

  if (!project || version == null || latest == null) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center gap-2 text-sm text-slate-500">
        <IconSpinner /> {said.project.loading}
      </div>
    );
  }

  const older = version === latest ? null : version;

  return (
    <div className="mx-auto w-full max-w-[min(90rem,100%)] px-4 pb-24 pt-4 sm:px-6">
      <LambdaNotice slim className="mb-5" />

      <Header project={project} ticketAt={ticketAt} version={version} tree={tree} />

      <div className="mt-8 flex flex-wrap items-end justify-between gap-x-4 gap-y-3 border-b border-slate-200 dark:border-ink-800">
        <nav aria-label={said.project.tabsLabel} className="-mb-px flex gap-1 overflow-x-auto [scrollbar-width:none]">
          {(['code', 'docs', 'tests', 'changes'] as SourceView[]).map((tab) => (
            <Link
              key={tab}
              to={sourcePath(key, tab, { version: tab === 'changes' ? null : older })}
              aria-current={view === tab ? 'page' : undefined}
              className={`flex shrink-0 items-center gap-2 whitespace-nowrap border-b-2 px-3 py-2.5 text-sm ${
                view === tab
                  ? 'border-accent-500 font-medium text-ink-900 dark:border-accent-400 dark:text-slate-100'
                  : 'border-transparent text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-200'
              }`}
            >
              {said.project.tabs[tab]}
              {tab === 'changes' && <span className="text-xs tabular-nums text-slate-400">{project.versions.length}</span>}
            </Link>
          ))}
        </nav>

        {view !== 'changes' && (
          <div className="pb-2">
            <VersionPicker project={project} version={version} view={view} />
          </div>
        )}
      </div>

      <div className="mt-5">
        {view !== 'changes' && <OlderVersion project={project} version={version} view={view} file={file} page={page} />}

        {view === 'changes' ? (
          <ChangesView project={project} />
        ) : treeFailure ? (
          <p className="py-16 text-center text-sm text-slate-500">{treeFailure}</p>
        ) : !tree ? (
          <Packing />
        ) : view === 'docs' ? (
          <DocsView publicKey={key} tree={tree} version={older} page={page} />
        ) : view === 'tests' ? (
          <TestsView publicKey={key} tree={tree} version={older} />
        ) : (
          <CodeView publicKey={key} tree={tree} version={version} latest={latest} file={file ?? 'Project.cs'} />
        )}
      </div>
    </div>
  );
}

/**
 * What the lambda is and where it runs, with what anybody can do with it.
 */
function Header({ project, ticketAt, version, tree }: {
  project: SourceProject;
  ticketAt: number;
  version: number;
  tree: SourceTree | null;
}) {
  const said = useSourceT();
  const format = useFormat();

  const { source } = project;
  const about = source.about ?? source.description;
  const address = absoluteAddress(source.address);
  const shown = shownAddress(source.address);

  return (
    <header>
      <nav aria-label={said.shell.section} className="text-[13px] text-slate-500">
        <Link to="/source" className="hover:text-accent-600 hover:underline dark:hover:text-accent-400">
          {said.project.all}
        </Link>
        <span className="px-1.5" aria-hidden="true">/</span>
        <span className="font-mono">{source.publicKey}</span>
      </nav>

      <div className="mt-3 grid gap-6 lg:grid-cols-[minmax(0,1fr)_auto]">
        <div className="min-w-0">
          <h1 className="break-words font-mono text-2xl font-semibold tracking-tight sm:text-3xl">{source.publicKey}</h1>
          {source.title && <p className="mt-1 text-lg text-slate-700 dark:text-slate-300">{source.title}</p>}
          {about && <p className="mt-3 max-w-3xl text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">{about}</p>}

          <div className="mt-4 flex flex-wrap items-center gap-x-5 gap-y-2 text-[13px] text-slate-500">
            <LicenseChip license={source.license} />
            {project.author && <span>{said.project.by(project.author)}</span>}
            <Link to={sourcePath(source.publicKey, 'changes')} className="hover:text-accent-600 hover:underline dark:hover:text-accent-400">
              {said.project.versions(project.versions.length)}
            </Link>
            {source.updated && <span title={format.moment(source.updated)}>{said.project.changed(format.ago(source.updated))}</span>}
            <span title={format.moment(source.publishedAt)}>{said.project.published(format.ago(source.publishedAt))}</span>
            <span className="inline-flex items-center gap-1.5">
              <span className={`h-1.5 w-1.5 rounded-full ${source.online ? 'bg-emerald-500' : 'bg-slate-400'}`} aria-hidden="true" />
              {source.online ? (
                said.project.onlineAt(
                  <a href={address} target="_blank" rel="noreferrer" className="font-mono text-accent-600 hover:underline dark:text-accent-400">
                    {shown}
                  </a>,
                )
              ) : (
                said.project.offline
              )}
            </span>
          </div>
        </div>

        <div className="flex flex-col gap-4 lg:items-end">
          <div className="flex flex-wrap items-center gap-2 lg:justify-end">
            <StarButton publicKey={source.publicKey} stars={source.stars} ticket={project.starTicket} ticketAt={ticketAt} />
            <CodeMenu
              publicKey={source.publicKey}
              version={version}
              root={tree?.root ?? null}
              bytes={tree?.bytes ?? null}
              gitUrl={absoluteAddress(project.gitPath)}
              oldest={project.versions[project.versions.length - 1]?.version ?? version}
              newest={project.versions[0]?.version ?? version}
            />
            {source.online && (
              <a href={address} target="_blank" rel="noreferrer" className="btn-primary h-9" title={said.project.opens(shown)}>
                {said.project.openApp}
                <IconExternal className="h-3.5 w-3.5" />
              </a>
            )}
          </div>

          {source.imagePath && (
            <a
              href={source.online ? address : undefined}
              target="_blank"
              rel="noreferrer"
              className="hidden aspect-[16/10] w-72 overflow-hidden border border-slate-200 bg-grey-100 lg:block dark:border-ink-800 dark:bg-ink-850"
            >
              <img src={source.imagePath} alt={said.project.picture(source.title ?? source.publicKey)} className="h-full w-full object-cover" />
            </a>
          )}
        </div>
      </div>
    </header>
  );
}

/**
 * A version being packed, which happens the first time anybody reads it: a
 * moment for most, and a while for one that ships a lot.
 */
function Packing() {
  const said = useSourceT().tree;
  const [slow, setSlow] = useState(false);

  useEffect(() => {
    const timer = window.setTimeout(() => setSlow(true), 900);

    return () => window.clearTimeout(timer);
  }, []);

  return (
    <div className="flex flex-col items-center gap-3 py-20 text-center" role="status">
      <span className="flex items-center gap-2 text-sm text-slate-600 dark:text-slate-400">
        <IconSpinner /> {said.packing}
      </span>
      {slow && (
        <>
          <span className="block h-1 w-48 overflow-hidden bg-slate-200 dark:bg-ink-800">
            <span className="block h-full w-1/3 animate-[packing_1.2s_ease-in-out_infinite] bg-accent-500 dark:bg-accent-400" />
          </span>
          <span className="max-w-sm text-xs text-slate-500">{said.packingSlow}</span>
        </>
      )}
    </div>
  );
}

function Missing() {
  const said = useSourceT().project;

  return (
    <div className="mx-auto flex max-w-xl flex-col items-center px-5 py-24 text-center">
      <h1 className="text-2xl font-bold tracking-tight">{said.missingTitle}</h1>
      <p className="mt-3 text-[15px] text-slate-600 dark:text-slate-400">{said.missing}</p>
      <Link to="/source" className="btn-primary mt-8">
        {said.all}
      </Link>
    </div>
  );
}
