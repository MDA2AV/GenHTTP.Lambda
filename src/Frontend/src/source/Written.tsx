import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { ApiError, api, type SourceTree } from '../api';
import { IconBeaker, IconBook, IconSpinner } from '../components/Icons';
import { MarkdownPage } from '../components/MarkdownPage';
import { useLanguage, useSourceT } from '../i18n';
import { Link, localize } from '../i18n/links';
import { pictureType, renderMarkdown } from '../markdown';
import { KIND_DOT } from './CodeView';
import { useFormat } from './format';
import { highlightBlocks } from './highlight';
import { DECISIONS, PRODUCT, TESTING, sourcePath } from './paths';

/** What was read of each page of each version, so going back to one does not ask again. */
const pages = new Map<string, string>();

/**
 * What is written about a version: its documentation, page by page.
 *
 * The pages are the version's own, as its agent wrote them: what the app is,
 * the decisions behind it, and whatever else it wrote down - read here as
 * pages rather than as files, with the pictures beside them shown in place.
 */
export function DocsView({ publicKey, tree, version, page }: {
  publicKey: string;
  tree: SourceTree;
  /** The version, or nothing for the newest. */
  version: number | null;
  page: string | null;
}) {
  const said = useSourceT().docs;

  const written = useMemo(() => {
    const known = [PRODUCT, DECISIONS];

    return tree.files
      .filter((f) => f.kind === 'docs' && f.path.toLowerCase().endsWith('.md'))
      .map((f) => f.path)
      .sort((a, b) => {
        const at = (path: string) => (known.includes(path) ? known.indexOf(path) : known.length);

        return at(a) - at(b) || a.localeCompare(b);
      });
  }, [tree]);

  if (written.length === 0) {
    return <Unwritten title={said.noneTitle} text={said.none} />;
  }

  const open = page && written.includes(page) ? page : written[0];

  const name = (path: string) =>
    path === PRODUCT ? said.product : path === DECISIONS ? said.decisions : path.replace(/^docs\//, '').replace(/\.md$/i, '');

  return (
    <div className="grid gap-6 lg:grid-cols-[14rem_minmax(0,1fr)]">
      <nav aria-label={said.pages} className="lg:sticky lg:top-20 lg:self-start">
        <p className="hidden px-3 pb-2 text-[11px] font-medium uppercase tracking-wide text-slate-400 lg:block">{said.pages}</p>
        <ul className="flex flex-wrap gap-1.5 lg:flex-col lg:gap-0.5">
          {written.map((path) => {
            const current = path === open;

            return (
              <li key={path}>
                <Link
                  to={sourcePath(publicKey, 'docs', { page: path, version })}
                  aria-current={current ? 'page' : undefined}
                  className={`flex items-center gap-2 rounded-full border px-3 py-1 text-[13px] lg:rounded-none lg:border-0 lg:border-l-2 lg:py-1.5 ${
                    current
                      ? 'border-accent-500 bg-accent-500/10 font-medium text-accent-700 dark:border-accent-400 dark:text-accent-400 lg:bg-slate-100 lg:text-ink-900 lg:dark:bg-ink-850 lg:dark:text-slate-100'
                      : 'border-slate-200 text-slate-600 hover:text-slate-900 dark:border-ink-800 dark:text-slate-400 dark:hover:text-slate-200 lg:border-transparent'
                  }`}
                >
                  {name(path)}
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>

      <Page publicKey={publicKey} tree={tree} version={version} path={open} />
    </div>
  );
}

/**
 * How a version is tested: what its tests say about themselves, and the
 * scripts and data they run with - which are read in the code.
 */
export function TestsView({ publicKey, tree, version }: {
  publicKey: string;
  tree: SourceTree;
  version: number | null;
}) {
  const said = useSourceT().tests;
  const format = useFormat();

  const readme = tree.files.some((f) => f.path === TESTING);
  const others = tree.files.filter((f) => f.kind === 'tests' && f.path !== TESTING);

  if (!readme && others.length === 0) {
    return <Unwritten title={said.noneTitle} text={said.none} tests />;
  }

  return (
    <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_16rem]">
      {readme ? <Page publicKey={publicKey} tree={tree} version={version} path={TESTING} /> : <Unwritten title={said.noneTitle} text={said.none} tests />}

      {others.length > 0 && (
        <aside className="lg:sticky lg:top-20 lg:self-start">
          <p className="px-1 pb-2 text-[11px] font-medium uppercase tracking-wide text-slate-400">{said.files}</p>
          <ul className="surface py-1.5 text-[13px]">
            {others.map((file) => (
              <li key={file.path}>
                <Link
                  to={sourcePath(publicKey, 'code', { file: file.path, version })}
                  className="flex items-center gap-2 px-3 py-1 text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-ink-850"
                  title={format.size(file.size)}
                >
                  <span className={`h-1.5 w-1.5 shrink-0 rounded-full ${KIND_DOT[file.kind]}`} aria-hidden="true" />
                  <span className="truncate font-mono text-[12.5px]">{file.path.replace(/^tests\//, '')}</span>
                </Link>
              </li>
            ))}
          </ul>
        </aside>
      )}
    </div>
  );
}

/** One page of markdown of a version, read when it is opened. */
function Page({ publicKey, tree, version, path }: {
  publicKey: string;
  tree: SourceTree;
  version: number | null;
  path: string;
}) {
  const said = useSourceT().docs;
  const language = useLanguage();
  const navigate = useNavigate();
  const holder = useRef<HTMLDivElement>(null);

  const known = `${publicKey}/${tree.version}/${path}`;

  const [text, setText] = useState<string | null>(pages.get(known) ?? null);
  const [failure, setFailure] = useState<string | null>(null);

  useEffect(() => {
    setText(pages.get(known) ?? null);
    setFailure(null);

    if (pages.has(known)) {
      return;
    }

    let alive = true;

    api.sources
      .file(publicKey, tree.version, path)
      .then((file) => {
        pages.set(known, file.content ?? '');

        if (alive) {
          setText(file.content ?? '');
        }
      })
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.failed));

    return () => {
      alive = false;
    };
  }, [publicKey, tree.version, path, known, said]);

  const html = useMemo(() => {
    if (text === null) {
      return '';
    }

    const has = (name: string) => tree.files.some((f) => f.path === name);

    return renderMarkdown(text, {
      name: path,
      page: has,
      picture: (name) => (has(name) && pictureType(name) ? api.sources.rawUrl(publicKey, tree.version, name) : null),
    });
  }, [text, path, tree, publicKey]);

  useEffect(() => {
    if (holder.current) {
      highlightBlocks(holder.current);
    }
  }, [html]);

  /** A page linked from this one: another page where it is shown, anything else in the code. */
  function open(name: string) {
    const to = name.startsWith('docs/')
      ? sourcePath(publicKey, 'docs', { page: name, version })
      : name === TESTING
        ? sourcePath(publicKey, 'tests', { version })
        : sourcePath(publicKey, 'code', { file: name, version });

    navigate(localize(language, to));
  }

  if (failure) {
    return <p className="py-10 text-sm text-slate-500">{failure}</p>;
  }

  if (text === null) {
    return (
      <p className="flex items-center gap-2 py-10 text-sm text-slate-500">
        <IconSpinner /> {said.loading}
      </p>
    );
  }

  return (
    <article className="min-w-0">
      <MarkdownPage html={html} holder={holder} onOpen={open} />
    </article>
  );
}

function Unwritten({ title, text, tests = false }: { title: string; text: string; tests?: boolean }) {
  const Icon = tests ? IconBeaker : IconBook;

  return (
    <div className="surface flex flex-col items-center px-6 py-14 text-center">
      <Icon className="h-6 w-6 text-slate-400" />
      <h2 className="mt-3 font-semibold tracking-tight">{title}</h2>
      <p className="mt-1.5 max-w-md text-sm text-slate-600 dark:text-slate-400">{text}</p>
    </div>
  );
}
