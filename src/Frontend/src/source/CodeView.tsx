import { useEffect, useMemo, useRef, useState, type MouseEvent } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import { ApiError, api, type SourceFile, type SourceFileContent, type SourceKind, type SourceTree } from '../api';
import { IconCheck, IconChevronDown, IconChevronRight, IconCopy, IconDownload, IconExternal, IconFolder, IconSpinner } from '../components/Icons';
import { useSourceT } from '../i18n';
import { Link } from '../i18n/links';
import { pictureType } from '../markdown';
import { useFormat } from './format';
import { colourable, highlight, languageOf } from './highlight';
import { ENTRY, sourcePath } from './paths';

/** The colour each kind of file is marked with, in the tree and the legend. */
export const KIND_DOT: Record<SourceKind, string> = {
  code: 'bg-accent-500 dark:bg-accent-400',
  asset: 'bg-emerald-500 dark:bg-emerald-400',
  docs: 'bg-logo-500 dark:bg-logo-400',
  tests: 'bg-amber-500 dark:bg-amber-400',
  platform: 'bg-slate-400 dark:bg-ink-600',
  project: 'bg-slate-300 dark:bg-ink-700',
};

const KINDS: SourceKind[] = ['code', 'asset', 'docs', 'tests', 'platform', 'project'];

/**
 * The code of a version: its files down the side, the one open beside them.
 *
 * Each file is marked with what it is to the lambda - its own code, what it
 * serves, what is written about it, what stands in for the platform - since
 * the layout of a project is not something a visitor should have to know to
 * find the part that matters. The lambda itself, Project.cs, is where it
 * opens.
 */
export function CodeView({ publicKey, tree, version, latest, file }: {
  publicKey: string;
  tree: SourceTree;
  version: number;
  latest: number;
  file: string;
}) {
  const words = useSourceT();
  const said = words.tree;
  const [shown, setShown] = useState(false);

  const known = tree.files.find((f) => f.path === file) ?? null;

  // on a phone the files are behind a button, and opening one puts them away
  useEffect(() => setShown(false), [file]);

  return (
    <div className="grid gap-4 lg:grid-cols-[17rem_minmax(0,1fr)] lg:gap-6">
      <aside className="lg:sticky lg:top-20 lg:max-h-[calc(100vh-6rem)] lg:self-start lg:overflow-y-auto">
        <button
          type="button"
          onClick={() => setShown((was) => !was)}
          aria-expanded={shown}
          className="surface flex w-full items-center justify-between px-3 py-2 text-sm font-medium lg:hidden"
        >
          <span>
            {said.label} <span className="font-normal text-slate-500">· {said.files(tree.files.length)}</span>
          </span>
          <IconChevronDown className={`h-4 w-4 transition-transform ${shown ? 'rotate-180' : ''}`} />
        </button>

        <div className={shown ? 'mt-2 lg:mt-0' : 'hidden lg:block'}>
          <FileTree publicKey={publicKey} files={tree.files} version={version === latest ? null : version} open={file} />
          <Legend />
        </div>
      </aside>

      <div className="min-w-0">
        {known ? (
          <FileView key={`${version}/${file}`} publicKey={publicKey} version={version} file={known} />
        ) : (
          <div className="surface px-6 py-12 text-center text-sm text-slate-600 dark:text-slate-400">
            <p>{words.file.missing(file)}</p>
            <Link to={sourcePath(publicKey, 'code', { file: ENTRY, version: version === latest ? null : version })} className="btn-ghost mt-4">
              {ENTRY}
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}

/* ------------------------------------------------------------------ tree */

interface Folder {
  name: string;
  path: string;
  folders: Folder[];
  files: SourceFile[];
}

/**
 * Where a folder or a file of the project's root is listed, before the rest
 * by name: what the lambda is - its code, what it serves, what is written
 * about it - before what makes it a project, and what stands in for the
 * platform last, being the part nobody came here to read.
 */
const FOLDERS = ['assets', 'docs', 'tests'];

const rankFolder = (name: string) => (FOLDERS.includes(name) ? FOLDERS.indexOf(name) : name === 'Platform' ? FOLDERS.length + 1 : FOLDERS.length);

const rankFile = (file: SourceFile) => (file.path === ENTRY ? 0 : file.kind === 'code' ? 1 : 2);

/** The flat list of files as the folders they are in, folders first, the root in the order above and the rest by name. */
function nest(files: SourceFile[]): Folder {
  const root: Folder = { name: '', path: '', folders: [], files: [] };

  for (const file of files) {
    const parts = file.path.split('/');
    let folder = root;

    for (const part of parts.slice(0, -1)) {
      const path = folder.path ? `${folder.path}/${part}` : part;
      let next = folder.folders.find((f) => f.name === part);

      if (!next) {
        next = { name: part, path, folders: [], files: [] };
        folder.folders.push(next);
      }

      folder = next;
    }

    folder.files.push(file);
  }

  const byName = (a: string, b: string) => a.localeCompare(b, undefined, { sensitivity: 'base' });

  const sort = (folder: Folder) => {
    const top = folder === root;

    folder.folders.sort((a, b) => (top ? rankFolder(a.name) - rankFolder(b.name) : 0) || byName(a.name, b.name));
    folder.files.sort((a, b) => (top ? rankFile(a) - rankFile(b) : 0) || byName(a.path, b.path));
    folder.folders.forEach(sort);
  };

  sort(root);

  return root;
}

/** How many files a folder holds, all the way down. */
const sizeOf = (folder: Folder): number => folder.files.length + folder.folders.reduce((sum, f) => sum + sizeOf(f), 0);

function FileTree({ publicKey, files, version, open }: {
  publicKey: string;
  files: SourceFile[];
  version: number | null;
  open: string;
}) {
  const said = useSourceT().tree;
  const root = useMemo(() => nest(files), [files]);

  return (
    <nav aria-label={said.label} className="surface py-1.5 text-[13px]">
      <FolderItems publicKey={publicKey} folder={root} version={version} open={open} depth={0} />
    </nav>
  );
}

function FolderItems({ publicKey, folder, version, open, depth }: {
  publicKey: string;
  folder: Folder;
  version: number | null;
  open: string;
  depth: number;
}) {
  const said = useSourceT().tree;
  const format = useFormat();

  return (
    <ul>
      {folder.folders.map((child) => (
        <FolderItem key={child.path} publicKey={publicKey} folder={child} version={version} open={open} depth={depth} />
      ))}
      {folder.files.map((file) => {
        const current = file.path === open;
        const name = file.path.split('/').pop();

        return (
          <li key={file.path}>
            <Link
              to={sourcePath(publicKey, 'code', { file: file.path, version })}
              aria-current={current ? 'page' : undefined}
              title={`${file.path} · ${format.size(file.size)} · ${said.kinds[file.kind]}`}
              className={`flex items-center gap-2 py-1 pr-3 ${
                current
                  ? 'bg-accent-500/10 font-medium text-accent-700 dark:bg-accent-400/10 dark:text-accent-400'
                  : 'text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-ink-850'
              }`}
              style={{ paddingLeft: `${0.75 + depth * 0.9}rem` }}
            >
              <span className={`h-1.5 w-1.5 shrink-0 rounded-full ${KIND_DOT[file.kind]}`} aria-hidden="true" />
              <span className="truncate font-mono text-[12.5px]">{name}</span>
            </Link>
          </li>
        );
      })}
    </ul>
  );
}

/**
 * A folder, open when what is open is in it and closed otherwise - except
 * the ones worth seeing at a glance: what is written about the lambda, and
 * a small front end.
 */
function FolderItem({ publicKey, folder, version, open, depth }: {
  publicKey: string;
  folder: Folder;
  version: number | null;
  open: string;
  depth: number;
}) {
  const inside = open.startsWith(`${folder.path}/`);
  const worth = depth === 0 && (folder.name === 'docs' || folder.name === 'tests' || (folder.name === 'assets' && sizeOf(folder) <= 12));

  const [expanded, setExpanded] = useState(inside || worth);

  useEffect(() => {
    if (inside) {
      setExpanded(true);
    }
  }, [inside]);

  return (
    <li>
      <button
        type="button"
        onClick={() => setExpanded((was) => !was)}
        aria-expanded={expanded}
        className="flex w-full items-center gap-1.5 py-1 pr-3 text-left text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-ink-850"
        style={{ paddingLeft: `${0.35 + depth * 0.9}rem` }}
      >
        <IconChevronRight className={`h-3.5 w-3.5 shrink-0 text-slate-400 transition-transform ${expanded ? 'rotate-90' : ''}`} />
        <IconFolder className="h-3.5 w-3.5 shrink-0 text-slate-400" />
        <span className="truncate font-mono text-[12.5px]">{folder.name}</span>
        {!expanded && <span className="ml-auto text-[11px] tabular-nums text-slate-400">{sizeOf(folder)}</span>}
      </button>

      {expanded && <FolderItems publicKey={publicKey} folder={folder} version={version} open={open} depth={depth + 1} />}
    </li>
  );
}

/** What the colours in the tree mean. */
function Legend() {
  const said = useSourceT().tree;

  return (
    <div className="mt-3 px-1">
      <p className="text-[11px] font-medium uppercase tracking-wide text-slate-400">{said.legend}</p>
      <ul className="mt-1.5 space-y-1 text-xs text-slate-500">
        {KINDS.map((kind) => (
          <li key={kind} className="flex items-start gap-2" title={said.kinds[kind]}>
            <span className={`mt-[5px] h-1.5 w-1.5 shrink-0 rounded-full ${KIND_DOT[kind]}`} aria-hidden="true" />
            <span>{said.kinds[kind]}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}

/* ------------------------------------------------------------------ file */

/** What was read of each file of each version, so going back to one does not ask again. */
const read = new Map<string, SourceFileContent>();

/** One file: what it is, and its lines - or the picture, or where to get it when it is neither. */
function FileView({ publicKey, version, file }: {
  publicKey: string;
  version: number;
  file: SourceFile;
}) {
  const said = useSourceT();
  const words = said.file;
  const format = useFormat();

  const known = `${publicKey}/${version}/${file.path}`;
  const picture = pictureType(file.path);

  const [content, setContent] = useState<SourceFileContent | null>(read.get(known) ?? null);
  const [failure, setFailure] = useState<string | null>(null);

  useEffect(() => {
    if (picture || read.has(known)) {
      return;
    }

    let alive = true;

    api.sources
      .file(publicKey, version, file.path)
      .then((loaded) => {
        read.set(known, loaded);

        if (alive) {
          setContent(loaded);
        }
      })
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : words.failed));

    return () => {
      alive = false;
    };
  }, [publicKey, version, file.path, known, picture, words]);

  const raw = api.sources.rawUrl(publicKey, version, file.path);
  const lines = content?.content != null ? countLines(content.content) : null;

  return (
    <section className="surface min-w-0" aria-label={file.path}>
      <header className="flex flex-wrap items-center gap-x-3 gap-y-2 border-b border-slate-200 px-3 py-2 dark:border-ink-800">
        <span className={`h-2 w-2 shrink-0 rounded-full ${KIND_DOT[file.kind]}`} title={said.tree.kinds[file.kind]} aria-hidden="true" />
        <h2 className="min-w-0 break-all font-mono text-[13px] font-medium">{file.path}</h2>
        <span className="text-xs text-slate-500">
          {said.tree.short[file.kind]} · {format.size(file.size)}
          {lines != null && ` · ${words.lines(lines)}`}
        </span>

        <div className="ml-auto flex items-center gap-1">
          {content?.content != null && <CopyText text={content.content} />}
          <a href={raw} target="_blank" rel="noreferrer" className="btn-ghost !px-2.5 !py-1 text-[13px]" title={words.rawTitle}>
            <IconExternal className="h-3.5 w-3.5" />
            {words.raw}
          </a>
          <a href={api.sources.rawUrl(publicKey, version, file.path, true)} className="btn-ghost !px-2.5 !py-1 text-[13px]" download>
            <IconDownload className="h-3.5 w-3.5" />
            <span className="sr-only sm:not-sr-only">{words.download}</span>
          </a>
        </div>
      </header>

      {picture ? (
        <div className="flex justify-center bg-[repeating-conic-gradient(theme(colors.grey.100)_0_25%,transparent_0_50%)] bg-[length:16px_16px] p-6 dark:bg-[repeating-conic-gradient(theme(colors.ink.850)_0_25%,transparent_0_50%)]">
          <img src={raw} alt={file.path} className="max-h-[32rem] max-w-full" />
        </div>
      ) : failure ? (
        <p className="px-4 py-10 text-center text-sm text-slate-500">{failure}</p>
      ) : !content ? (
        <p className="flex items-center justify-center gap-2 px-4 py-10 text-sm text-slate-500">
          <IconSpinner /> {words.loading}
        </p>
      ) : content.content == null ? (
        <div className="px-4 py-10 text-center text-sm text-slate-600 dark:text-slate-400">
          <p>{content.text ? words.tooLarge : words.binary}</p>
          <a href={api.sources.rawUrl(publicKey, version, file.path, true)} className="btn-ghost mt-3" download>
            <IconDownload className="h-4 w-4" />
            {words.download} · {format.size(file.size)}
          </a>
        </div>
      ) : (
        <CodeLines text={content.content} language={languageOf(file.path)} />
      )}
    </section>
  );
}

const countLines = (text: string) => (text.length === 0 ? 0 : text.split('\n').length - (text.endsWith('\n') ? 1 : 0));

/** The height of a line, which the numbers, the code and the marked lines agree on. */
const LINE = 20;

/**
 * The lines of a file, numbered, coloured where its grammar is known. A
 * number is a link to its line - #L12, or #L12-L20 with shift held for the
 * lines between - which marks it and scrolls it into view for whoever the
 * link is sent to.
 */
function CodeLines({ text, language }: { text: string; language: string | null }) {
  const words = useSourceT().file;
  const { hash, pathname, search } = useLocation();
  const navigate = useNavigate();
  const holder = useRef<HTMLDivElement>(null);

  const html = useMemo(() => highlight(text, language), [text, language]);
  const count = Math.max(1, countLines(text));

  const marked = /^#L(\d+)(?:-L(\d+))?$/.exec(hash);
  const first = marked ? Math.min(Number(marked[1]), Number(marked[2] ?? marked[1])) : null;
  const last = marked ? Math.max(Number(marked[1]), Number(marked[2] ?? marked[1])) : null;

  useEffect(() => {
    if (first == null) {
      return;
    }

    // after the page has settled where the router put it
    const frame = requestAnimationFrame(() =>
      holder.current?.querySelector(`#L${first}`)?.scrollIntoView({ block: 'center' }));

    return () => cancelAnimationFrame(frame);
    // only when the lines arrive or another is asked for
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [first, text]);

  function mark(event: MouseEvent<HTMLAnchorElement>, line: number) {
    event.preventDefault();

    const next = event.shiftKey && first != null ? `#L${Math.min(first, line)}-L${Math.max(first, line)}` : `#L${line}`;

    navigate({ pathname, search, hash: next }, { replace: true });
  }

  return (
    <>
    {!colourable(text) && language && <p className="border-b border-slate-200 px-4 py-1.5 text-xs text-slate-500 dark:border-ink-800">{words.plain}</p>}
    <div ref={holder} className="flex font-mono text-[12.5px]" style={{ lineHeight: `${LINE}px` }}>
      <div className="select-none border-r border-slate-200 py-3 text-right text-slate-400 dark:border-ink-800">
        {Array.from({ length: count }, (_, i) => (
          <a
            key={i}
            id={`L${i + 1}`}
            href={`#L${i + 1}`}
            onClick={(event) => mark(event, i + 1)}
            aria-label={words.line(i + 1)}
            className={`block px-3 hover:text-slate-700 dark:hover:text-slate-200 ${
              first != null && i + 1 >= first && i + 1 <= last! ? 'text-amber-700 dark:text-amber-400' : ''
            }`}
            style={{ height: LINE }}
          >
            {i + 1}
          </a>
        ))}
      </div>

      <div className="min-w-0 flex-1 overflow-x-auto">
        <div className="relative w-max min-w-full py-3">
          {first != null && (
            <div
              aria-hidden="true"
              className="absolute inset-x-0 bg-amber-400/15 dark:bg-amber-400/10"
              style={{ top: 12 + (first - 1) * LINE, height: (last! - first + 1) * LINE }}
            />
          )}
          <pre className="relative px-4">
            <code className="code-view hljs" dangerouslySetInnerHTML={{ __html: html }} />
          </pre>
        </div>
      </div>

    </div>
    </>
  );
}

function CopyText({ text }: { text: string }) {
  const words = useSourceT().file;
  const [copied, setCopied] = useState(false);

  return (
    <button
      type="button"
      className="btn-ghost !px-2.5 !py-1 text-[13px]"
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(text);
          setCopied(true);
          window.setTimeout(() => setCopied(false), 1500);
        } catch {
          // a clipboard that refuses is not worth an error
        }
      }}
    >
      {copied ? <IconCheck className="h-3.5 w-3.5 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5" />}
      <span className="sr-only sm:not-sr-only">{copied ? words.copied : words.copy}</span>
    </button>
  );
}
