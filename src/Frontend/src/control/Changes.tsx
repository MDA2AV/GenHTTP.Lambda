import { Fragment, useEffect, useMemo, useState } from 'react';

import type { LambdaFile } from '../api';
import { useEditorT } from '../i18n';
import { languageFor, monaco } from '../monaco';
import type { Theme } from '../theme';
import { compare, type DiffLine, type FileDiff } from './diff';
import { isCode, isContext, isDevelopment } from './written';

/**
 * What a changed file is to the version, in the order a change is read: the
 * code, then what the front end is built from, then the assets - which hold
 * what such a build wrote, minified and least worth reading first - and what
 * is written about it last.
 */
const GROUPS = ['code', 'development', 'assets', 'context'] as const;

type Kind = (typeof GROUPS)[number];

const kindOf = (name: string): Kind => (isCode(name) ? 'code' : isDevelopment(name) ? 'development' : isContext(name) ? 'context' : 'assets');

/**
 * What changed between two sets of files: one row per file that differs,
 * with its lines added and removed, and the first of them opened - which is
 * usually the one to read. The versions show the difference to the version
 * before; a feature shows the difference to the version it is based on.
 *
 * Where the files that changed are of more than one kind, they are listed
 * kind by kind under a heading each: a change that touched the sources of a
 * front end and the assets they build to reads as both, rather than as one
 * list in which the sources are found among the minified files.
 */
export function ChangeList({ before, after, theme, empty, folded = false }: {
  before: LambdaFile[];
  after: LambdaFile[];
  theme: Theme;
  /** What to say when nothing differs. */
  empty: string;
  /** Whether every file starts closed, where the lines are there for whoever wants them rather than the point of the page. */
  folded?: boolean;
}) {
  const said = useEditorT().versions;

  const changed = useMemo(
    () => compare(before, after)
      .filter((d) => d.status !== 'same')
      .sort((a, b) => GROUPS.indexOf(kindOf(a.name)) - GROUPS.indexOf(kindOf(b.name))),
    [before, after],
  );

  const grouped = new Set(changed.map((d) => kindOf(d.name))).size > 1;

  const first = folded ? null : changed[0]?.name ?? null;
  const [shown, setShown] = useState<string | null>(first);

  // a file that is no longer among the changes is not held open
  useEffect(() => {
    setShown((was) => (was != null && changed.some((d) => d.name === was) ? was : first));
  }, [changed, first]);

  if (changed.length === 0) {
    return <p className="text-sm text-slate-500">{empty}</p>;
  }

  return (
    <div className="surface overflow-hidden">
      <ul className="divide-y divide-slate-200 dark:divide-ink-800">
        {changed.map((diff, index) => (
          <Fragment key={diff.name}>
            {grouped && (index === 0 || kindOf(changed[index - 1].name) !== kindOf(diff.name)) && (
              <li className="flex items-center justify-between bg-slate-50 px-3 py-1.5 text-[11px] font-medium uppercase tracking-wide text-slate-500 dark:bg-ink-850">
                {said.groups[kindOf(diff.name)]}
                <span className="tabular-nums">{changed.filter((d) => kindOf(d.name) === kindOf(diff.name)).length}</span>
              </li>
            )}
            <li>
              <button
                type="button"
                onClick={() => setShown((was) => (was === diff.name ? null : diff.name))}
                className="flex w-full items-center gap-3 px-3 py-2 text-left text-sm hover:bg-slate-50 dark:hover:bg-ink-850"
                aria-expanded={shown === diff.name}
              >
                <span className="min-w-0 flex-1 truncate font-mono text-[13px]">
                  {diff.name}
                  {diff.status !== 'changed' && <span className="ml-2 font-sans text-xs text-slate-500">{said.status[diff.status]}</span>}
                </span>
                {!diff.binary && (
                  <span className="shrink-0 font-mono text-xs">
                    <span className="text-emerald-600 dark:text-emerald-400">+{diff.added}</span>{' '}
                    <span className="text-red-500 dark:text-red-400">−{diff.removed}</span>
                  </span>
                )}
              </button>

              {shown === diff.name && (
                <Patch
                  diff={diff}
                  before={before.find((f) => f.name === diff.name)?.code ?? ''}
                  after={after.find((f) => f.name === diff.name)?.code ?? ''}
                  theme={theme}
                />
              )}
            </li>
          </Fragment>
        ))}
      </ul>
    </div>
  );
}

/**
 * Each side coloured as a whole by the editor's own grammar and theme, then
 * cut into lines - a line coloured on its own would not know it sits inside a
 * comment or a string that began above it. Monaco is already loaded for the
 * code view, so this costs nothing more than the tokenising.
 */
function useColoured(code: string, language: string, theme: Theme): string[] | null {
  const [lines, setLines] = useState<string[] | null>(null);

  useEffect(() => {
    let alive = true;

    setLines(null);

    // the theme is global to monaco; the code view sets it the same way
    monaco.editor.setTheme(theme === 'dark' ? 'lambda-dark' : 'lambda-light');

    monaco.editor
      .colorize(code.replace(/\r\n/g, '\n'), language, { tabSize: 4 })
      .then((html) => alive && setLines(html.split('<br/>')))
      .catch(() => {
        // uncoloured is still readable
      });

    return () => {
      alive = false;
    };
  }, [code, language, theme]);

  return lines;
}

/** Prose, whose lines are paragraphs: wrapped to be read, where code keeps its lines as they are. */
const PROSE = /\.(md|markdown|txt)$/i;

function Patch({ diff, before, after, theme }: { diff: FileDiff; before: string; after: string; theme: Theme }) {
  const said = useEditorT().versions;
  const language = languageFor(diff.name);
  const wrap = PROSE.test(diff.name);
  const old = useColoured(before, language, theme);
  const now = useColoured(after, language, theme);

  if (diff.binary || !diff.hunks) {
    return (
      <p className="border-t border-slate-200 px-3 py-2 text-xs text-slate-500 dark:border-ink-800">
        {diff.binary ? said.binary : said.tooLarge}
      </p>
    );
  }

  return (
    <div className="max-h-[28rem] overflow-auto border-t border-slate-200 dark:border-ink-800">
      <table className="w-full border-collapse font-mono text-[12.5px] leading-5">
        <tbody>
          {diff.hunks.map((hunk, h) => (
            <Hunk key={h} lines={hunk.lines} separator={h > 0} old={old} now={now} wrap={wrap} />
          ))}
        </tbody>
      </table>
    </div>
  );
}

function Hunk({ lines, separator, old, now, wrap }: {
  lines: DiffLine[];
  separator: boolean;
  old: string[] | null;
  now: string[] | null;
  wrap: boolean;
}) {
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
          <Code text={line.text} html={(line.kind === 'removed' ? old : now)?.[line.number - 1]} wrap={wrap} />
        </tr>
      ))}
    </>
  );
}

function Code({ text, html, wrap }: { text: string; html?: string; wrap: boolean }) {
  const layout = wrap ? 'whitespace-pre-wrap break-words pr-4' : 'whitespace-pre pr-4';

  // an empty line has no text to hold the row open, coloured or not
  if (text === '' || html === undefined) {
    return <td className={layout}>{text || ' '}</td>;
  }

  // monaco escapes the source as it renders it - and writes every space as a
  // non-breaking one, which prose has to have back to wrap at all
  return <td className={layout} dangerouslySetInnerHTML={{ __html: wrap ? html.replace(/\u00a0|&nbsp;/g, ' ') : html }} />;
}
