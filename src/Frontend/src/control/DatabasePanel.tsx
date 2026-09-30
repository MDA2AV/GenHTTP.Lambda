import { useCallback, useEffect, useState, type ReactNode } from 'react';
import { useSearchParams } from 'react-router-dom';

import { ApiError, api, type DatabaseColumn, type DatabaseInfo, type DatabaseRows, type DatabaseTable, type DatabaseValue } from '../api';
import { Dialog } from '../components/Dialog';
import {
  IconAlert, IconArrowDown, IconArrowUp, IconCheck, IconChevronLeft, IconChevronRight, IconCopy, IconDatabase, IconHistory, IconKey,
  IconRefresh, IconSpinner, IconTable,
} from '../components/Icons';
import { useToast } from '../components/Toast';
import { tagOf, useEditorT, useLanguage } from '../i18n';
import type { Control } from './context';
import { bytes } from './format';

/** How many rows one page shows: enough to read, few enough to be quick on a phone. */
const PAGE = 50;

/** How the rows are put in order: a column and which way, or the order they were written in. */
interface Sorting {
  order: string | null;
  descending: boolean;
}

/** Newest first, which is what somebody looking at what an app keeps wants first. */
const NEWEST: Sorting = { order: null, descending: true };

/**
 * The database of a lambda - or, opened on a draft, of its test data - as its
 * tables and what is in them.
 *
 * Read only, and said so: what is in a database is written by the lambda, and
 * a change to it is a change to the app, asked of the agent or made in the
 * code. So this is a place to look - which tables there are, what they hold,
 * one record in full - and it never offers to edit a cell.
 *
 * The tables sit beside the rows of the one that is open, as the files of the
 * workspace sit beside the file that is open. The newest rows come first, a
 * page at a time, and a column heading sorts by it. A row opens whole, since a
 * row of ten columns does not fit a phone and a long text does not fit a cell.
 *
 * The simple view gets the same thing in its own words: records, table by
 * table - without the types of the columns, the views, or the history Evolve
 * keeps of its migrations, none of which is anything the app keeps.
 */
export function DatabasePanel({ control, enabled, readOnly, generation, onChanged }: {
  control: Control;
  /** Whether the lambda has its database switched on. */
  enabled: boolean;
  readOnly: boolean;
  /** Counts every change to the data, so what is shown is read again. */
  generation: number;
  onChanged: () => Promise<void>;
}) {
  const t = useEditorT();
  const said = t.data.database;
  const plain = t.data.simple;
  const toast = useToast();
  const [params, setParams] = useSearchParams();

  const [info, setInfo] = useState<DatabaseInfo | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [reading, setReading] = useState(false);

  /** Counts the reloads somebody asked for, so the table that is open reads again too. */
  const [asked, setAsked] = useState(0);

  const feature = control.feature?.info.key;
  const simple = control.simple;

  const read = useCallback(async () => {
    setReading(true);

    try {
      setInfo(await api.database.get(control.privateKey, feature));
      setFailure(null);
    } catch (error) {
      setFailure(error instanceof ApiError ? error.message : t.data.readFailed);
    } finally {
      setReading(false);
    }
  }, [control.privateKey, feature, t]);

  useEffect(() => {
    read();
  }, [read, generation, enabled]);

  if (failure) {
    return <p className="text-sm text-red-500">{failure}</p>;
  }

  if (!info) {
    return (
      <div className="flex items-center gap-2 text-sm text-slate-500">
        <IconSpinner /> {t.files.reading}
      </div>
    );
  }

  // switched off, the card above says so; what is left to say is that the
  // code would like it on
  if (!info.enabled) {
    // the simple view never shows code; it shows the kind only once it holds something
    if (simple) {
      return null;
    }

    if (!info.used && !feature) {
      return <HowTo />;
    }

    return (
      <div className="space-y-5">
        <div className={`surface flex flex-wrap items-center gap-x-4 gap-y-3 p-5 ${info.used ? 'border-amber-500/40 bg-amber-500/5' : ''}`}>
          {info.used
            ? <IconAlert className="h-5 w-5 shrink-0 text-amber-600 dark:text-amber-400" />
            : <IconDatabase className="h-5 w-5 shrink-0 text-slate-400" />}
          <div className="min-w-0 flex-1">
            <p className="font-medium">{said.offTitle}</p>
            <p className="mt-0.5 text-[13px] text-slate-600 dark:text-slate-400">{info.used ? said.offWanted : said.offText}</p>
          </div>
          {!readOnly && !feature && (
            <button
              type="button"
              className="btn-primary !px-4 !py-1.5 text-[13px]"
              onClick={async () => {
                try {
                  await api.enableData(control.privateKey, 'database');
                  toast(t.data.kinds.database.switchedOn, 'success');
                  await Promise.all([onChanged(), control.refresh()]);
                } catch (error) {
                  toast(error instanceof ApiError ? error.message : t.data.switchFailed, 'error');
                }
              }}
            >
              {said.switchOn}
            </button>
          )}
        </div>
        {!feature && <HowTo />}
      </div>
    );
  }

  // what the simple view calls records: the app's own tables, not its views
  // or the history of its migrations
  const tables = simple ? info.tables.filter((table) => table.kind === 'table' && !table.migrations) : ordered(info.tables);

  const wanted = params.get('table');
  const current = tables.find((table) => table.name === wanted) ?? tables[0] ?? null;

  const pick = (name: string) => {
    const next = new URLSearchParams(params);
    next.set('table', name);
    setParams(next, { replace: true });
  };

  const reload = async () => {
    setAsked((was) => was + 1);
    await read();
  };

  return (
    <div className="space-y-5">
      <div className="flex min-h-8 flex-wrap items-center justify-between gap-3">
        {simple ? (
          <div className="min-w-0 flex-1">
            <h2 className="flex items-center gap-2 text-[15px] font-medium">
              <IconDatabase className="h-4 w-4 text-accent-500 dark:text-accent-400" />
              {plain.databaseTitle}
            </h2>
            <p className="mt-1 max-w-2xl text-[13px] text-slate-600 dark:text-slate-400">{plain.databaseText}</p>
          </div>
        ) : (
          <div className="flex min-w-0 flex-wrap items-center gap-x-3 gap-y-1">
            <h2 className="flex items-center gap-2 text-sm font-medium">
              <IconDatabase className="h-4 w-4 text-slate-400" />
              {feature ? said.copyContents : said.contents}
            </h2>
            <span className="text-xs text-slate-500">{said.readOnly}</span>
          </div>
        )}
        <button type="button" onClick={reload} disabled={reading} className="btn-ghost !px-3 !py-1.5 text-[13px]" title={said.reload}>
          {reading ? <IconSpinner /> : <IconRefresh className="h-3.5 w-3.5" />}
          {said.reload}
        </button>
      </div>

      {tables.length === 0 ? (
        simple ? (
          <p className="surface p-6 text-center text-sm text-slate-500">{plain.noRecords}</p>
        ) : (
          <div className="surface flex flex-col items-center px-6 py-10 text-center">
            <span className="flex h-12 w-12 items-center justify-center rounded-full bg-accent-500/10 text-accent-500 dark:text-accent-400">
              <IconDatabase className="h-6 w-6" />
            </span>
            <h3 className="mt-4 font-medium">{said.empty}</h3>
            <p className="mt-1 max-w-md text-[13px] text-slate-500">{said.emptyText}</p>
          </div>
        )
      ) : (
        <div className="grid gap-5 lg:grid-cols-[15rem,minmax(0,1fr)]">
          <nav aria-label={simple ? plain.database : said.contents}>
            <TableList tables={tables} current={current?.name ?? null} simple={simple} onPick={pick} />
          </nav>

          {current ? (
            <TableView
              key={`${feature ?? ''}/${current.name}`}
              control={control}
              table={current}
              simple={simple}
              refresh={asked + generation}
            />
          ) : (
            <div className="surface flex min-h-[16rem] items-center justify-center p-6 text-sm text-slate-500">{said.pick}</div>
          )}
        </div>
      )}

      {!simple && (
        <p className="text-xs text-slate-500">
          {bytes(info.usedBytes)} / {bytes(info.quotaBytes)}
        </p>
      )}

      {!simple && !feature && <HowTo />}
    </div>
  );
}

/** Tables first and then views, each by name, and the history of the migrations last. */
function ordered(tables: DatabaseTable[]): DatabaseTable[] {
  const rank = (table: DatabaseTable) => (table.migrations ? 2 : table.kind === 'view' ? 1 : 0);

  return [...tables].sort((a, b) => rank(a) - rank(b) || a.name.localeCompare(b.name));
}

/** The tables to pick from, with how much each holds. */
function TableList({ tables, current, simple, onPick }: {
  tables: DatabaseTable[];
  current: string | null;
  simple: boolean;
  onPick: (name: string) => void;
}) {
  const t = useEditorT();
  const said = t.data.database;
  const plain = t.data.simple;

  return (
    <ul className="surface divide-y divide-slate-200 dark:divide-ink-800 lg:max-h-[40rem] lg:overflow-y-auto">
      {tables.map((table) => {
        const active = table.name === current;

        return (
          <li key={table.name}>
            <button
              type="button"
              onClick={() => onPick(table.name)}
              aria-current={active ? 'true' : undefined}
              title={table.migrations ? said.migrationsHint : table.name}
              className={`flex w-full items-center gap-2.5 border-l-2 px-3 py-2.5 text-left text-[13px] transition-colors ${
                active
                  ? 'border-accent-500 bg-accent-500/10 text-accent-700 dark:border-accent-400 dark:text-accent-400'
                  : 'border-transparent hover:bg-slate-50 dark:hover:bg-ink-850'
              } ${table.migrations ? 'text-slate-500' : ''}`}
            >
              {table.migrations
                ? <IconHistory className="h-3.5 w-3.5 shrink-0 text-slate-400" />
                : <IconTable className={`h-3.5 w-3.5 shrink-0 ${active ? '' : 'text-slate-400'}`} />}
              <span className="min-w-0 flex-1">
                <span className={`block truncate ${simple ? 'font-medium' : 'font-mono'}`}>{table.name}</span>
                {!simple && (table.kind === 'view' || table.migrations) && (
                  <span className="text-[11px] uppercase tracking-wide text-slate-400">{table.migrations ? said.migrations : said.view}</span>
                )}
              </span>
              <span className="shrink-0 text-xs tabular-nums text-slate-400">
                {table.rows == null ? '–' : simple ? plain.records(table.rows) : table.rows.toLocaleString()}
              </span>
            </button>
          </li>
        );
      })}
    </ul>
  );
}

/** The rows of one table, a page at a time, with its columns and a way to open a row whole. */
function TableView({ control, table, simple, refresh }: {
  control: Control;
  table: DatabaseTable;
  simple: boolean;
  /** Counts the reasons to read again. */
  refresh: number;
}) {
  const t = useEditorT();
  const said = t.data.database;
  const plain = t.data.simple;

  const [page, setPage] = useState<DatabaseRows | null>(null);
  const [offset, setOffset] = useState(0);
  const [sorting, setSorting] = useState<Sorting>(NEWEST);
  const [failure, setFailure] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [open, setOpen] = useState<number | null>(null);
  const [structure, setStructure] = useState(false);

  const feature = control.feature?.info.key;

  useEffect(() => {
    let alive = true;

    setBusy(true);

    api.database
      .rows(control.privateKey, table.name, { offset, limit: PAGE, order: sorting.order, descending: sorting.descending }, feature)
      .then((rows) => {
        if (alive) {
          setPage(rows);
          setFailure(null);
        }
      })
      .catch((error) => alive && setFailure(error instanceof ApiError ? error.message : said.readFailed))
      .finally(() => alive && setBusy(false));

    return () => {
      alive = false;
    };
  }, [control.privateKey, feature, table.name, offset, sorting, refresh, said]);

  // a heading sorts by its column, then the other way, then back to newest first
  const sortBy = (column: string) => {
    setOffset(0);
    setSorting((was) =>
      was.order !== column ? { order: column, descending: false } : !was.descending ? { order: column, descending: true } : NEWEST);
  };

  const columns = page?.columns ?? table.columns;
  const total = page?.total ?? table.rows ?? 0;
  const shown = page?.rows ?? [];

  return (
    <section className="min-w-0 space-y-3" aria-busy={busy}>
      <header className="flex flex-wrap items-center gap-x-3 gap-y-1.5">
        <h3 className={`min-w-0 truncate text-[15px] font-medium ${simple ? '' : 'font-mono'}`}>{table.name}</h3>
        <span className="text-[13px] tabular-nums text-slate-500">{simple ? plain.records(total) : said.rows(total)}</span>
        {busy && <IconSpinner className="h-3.5 w-3.5 text-slate-400" />}
        <span className="ml-auto flex items-center gap-2">
          {!simple && (
            <button
              type="button"
              onClick={() => setStructure((was) => !was)}
              aria-expanded={structure}
              className={`rounded-full border px-3 py-1 text-xs transition-colors ${
                structure
                  ? 'border-accent-500 bg-accent-500/10 text-accent-700 dark:border-accent-400 dark:text-accent-400'
                  : 'border-slate-200 text-slate-600 hover:border-slate-300 dark:border-ink-800 dark:text-slate-400'
              }`}
            >
              {said.columns} · {columns.length}
            </button>
          )}
          {sorting.order !== null && (
            <button type="button" onClick={() => { setOffset(0); setSorting(NEWEST); }} className="text-xs text-accent-500 hover:underline">
              {said.newestFirst}
            </button>
          )}
        </span>
      </header>

      {structure && !simple && <Structure columns={columns} />}

      {failure ? (
        <p className="surface p-4 text-sm text-red-500">{failure}</p>
      ) : page && shown.length === 0 ? (
        <p className="surface p-6 text-center text-sm text-slate-500">{said.noRows}</p>
      ) : (
        <div className="surface overflow-x-auto">
          <table className="w-full border-collapse text-[13px]">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50 text-left dark:border-ink-800 dark:bg-ink-850">
                {columns.map((column) => {
                  const active = sorting.order === column.name;

                  return (
                    <th key={column.name} scope="col" className="whitespace-nowrap p-0 font-medium">
                      <button
                        type="button"
                        onClick={() => sortBy(column.name)}
                        title={said.sortBy(column.name)}
                        className={`flex w-full items-center gap-1 px-3 py-2 text-left ${
                          active ? 'text-accent-700 dark:text-accent-400' : 'text-slate-600 hover:text-slate-900 dark:text-slate-300 dark:hover:text-white'
                        }`}
                      >
                        {column.primaryKey && !simple && <IconKey className="h-3 w-3 shrink-0 text-amber-500" />}
                        <span className={simple ? '' : 'font-mono'}>{column.name}</span>
                        {active && (sorting.descending ? <IconArrowDown className="h-3 w-3" /> : <IconArrowUp className="h-3 w-3" />)}
                      </button>
                    </th>
                  );
                })}
                <th scope="col" className="w-8 p-0"><span className="sr-only">{said.openRow}</span></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-ink-800">
              {shown.map((row, index) => (
                <tr
                  key={index}
                  onClick={() => setOpen(index)}
                  className="cursor-pointer align-top hover:bg-accent-500/5"
                >
                  {row.map((value, column) => (
                    <td key={column} className="max-w-[20rem] px-3 py-2">
                      <Cell value={value} friendly={simple} />
                    </td>
                  ))}
                  <td className="px-1 py-2 text-right">
                    <button
                      type="button"
                      onClick={(event) => { event.stopPropagation(); setOpen(index); }}
                      className="rounded-full p-0.5 text-slate-400 hover:text-accent-500"
                      aria-label={said.openRow}
                      title={said.openRow}
                    >
                      <IconChevronRight className="h-3.5 w-3.5" />
                    </button>
                  </td>
                </tr>
              ))}
              {!page && (
                <tr>
                  <td colSpan={columns.length + 1} className="px-3 py-6 text-center text-slate-500">
                    <IconSpinner className="inline h-4 w-4" />
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {page && total > PAGE && (
        <nav className="flex items-center justify-end gap-2 text-[13px] text-slate-500">
          <span className="tabular-nums">{said.page(offset + 1, Math.min(offset + shown.length, total), total)}</span>
          <button
            type="button"
            onClick={() => setOffset(Math.max(0, offset - PAGE))}
            disabled={offset === 0 || busy}
            className="rounded-full p-1 hover:bg-slate-100 disabled:opacity-40 dark:hover:bg-ink-850"
            aria-label={said.previous}
            title={said.previous}
          >
            <IconChevronLeft className="h-4 w-4" />
          </button>
          <button
            type="button"
            onClick={() => setOffset(offset + PAGE)}
            disabled={offset + PAGE >= total || busy}
            className="rounded-full p-1 hover:bg-slate-100 disabled:opacity-40 dark:hover:bg-ink-850"
            aria-label={said.next}
            title={said.next}
          >
            <IconChevronRight className="h-4 w-4" />
          </button>
        </nav>
      )}

      <Dialog
        title={open !== null ? (simple ? table.name : `${table.name} · ${said.row(offset + open + 1)}`) : ''}
        open={open !== null && shown[open] !== undefined}
        onClose={() => setOpen(null)}
        wide
        footer={
          <button type="button" onClick={() => setOpen(null)} className="btn-ghost">
            {said.close}
          </button>
        }
      >
        {open !== null && shown[open] && (
          <dl className="divide-y divide-slate-100 dark:divide-ink-800">
            {columns.map((column, index) => (
              <div key={column.name} className="grid gap-1 py-2.5 sm:grid-cols-[10rem,minmax(0,1fr)] sm:gap-4">
                <dt className="min-w-0">
                  <span className={`block truncate text-[13px] font-medium ${simple ? '' : 'font-mono'}`} title={column.name}>{column.name}</span>
                  {!simple && column.type && <span className="text-[11px] uppercase tracking-wide text-slate-400">{column.type}</span>}
                </dt>
                <dd className="min-w-0">
                  <Whole value={shown[open][index]} friendly={simple} />
                </dd>
              </div>
            ))}
          </dl>
        )}
      </Dialog>
    </section>
  );
}

/** A time as SQLite keeps it by convention: ISO 8601 text, with or without its zone. */
const MOMENT = /^\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}(:\d{2}(\.\d+)?)?(Z|[+-]\d{2}:?\d{2})?$/;

/**
 * A time as people read it, where a text is one - for the simple view, which
 * shows what the app keeps rather than how it keeps it.
 */
function readable(text: string, tag: string): string | null {
  if (!MOMENT.test(text)) {
    return null;
  }

  const moment = new Date(text.includes('T') ? text : text.replace(' ', 'T'));

  return Number.isNaN(moment.getTime()) ? null : moment.toLocaleString(tag, { dateStyle: 'medium', timeStyle: 'short' });
}

/**
 * A value in one line of a cell: nothing as NULL, numbers as numbers, text
 * cut at the edge, bytes by their length - and, where it is friendly, times
 * as people read them, with what is stored on hover.
 */
function Cell({ value, friendly = false, align = true }: { value: DatabaseValue; friendly?: boolean; align?: boolean }) {
  const said = useEditorT().data.database;
  const tag = tagOf(useLanguage());

  if (value === null) {
    return <span className="font-mono text-xs italic text-slate-400">{said.nullValue}</span>;
  }

  if (typeof value === 'number') {
    return <span className={`block font-mono tabular-nums ${align ? 'text-right' : ''}`}>{value}</span>;
  }

  if (typeof value === 'object' && 'blob' in value) {
    return <span className="whitespace-nowrap rounded bg-slate-100 px-1.5 py-0.5 font-mono text-[11px] text-slate-500 dark:bg-ink-850">{said.bytes(bytes(value.blob))}</span>;
  }

  const text = typeof value === 'string' ? value : value.text;
  const moment = friendly ? readable(text, tag) : null;

  return <span className="block truncate" title={text.length > 200 ? `${text.slice(0, 200)}…` : text}>{moment ?? text}</span>;
}

/**
 * A value in full, for a row opened on its own: JSON laid out in the font of
 * code, other text wrapped in the font of the page, and what was cut said.
 */
function Whole({ value, friendly }: { value: DatabaseValue; friendly: boolean }) {
  const said = useEditorT().data.database;
  const tag = tagOf(useLanguage());

  if (value === null || typeof value === 'number' || (typeof value === 'object' && 'blob' in value)) {
    return <Cell value={value} align={false} />;
  }

  const text = typeof value === 'string' ? value : value.text;
  const cut = typeof value === 'object' ? value.length : null;
  const json = laidOut(text);
  const moment = friendly ? readable(text, tag) : null;

  return (
    <div className="space-y-1">
      <Copyable text={text}>
        {json !== null ? (
          <pre className="whitespace-pre-wrap break-words font-mono text-[12.5px] leading-relaxed text-ink-900 dark:text-slate-200">{json}</pre>
        ) : (
          <p className="whitespace-pre-wrap break-words text-[13px] leading-relaxed text-ink-900 dark:text-slate-200" title={moment ? text : undefined}>
            {moment ?? text}
          </p>
        )}
      </Copyable>
      {cut !== null && <p className="text-xs text-slate-500">{said.cut(cut)}</p>}
    </div>
  );
}

/** JSON indented, where the text is JSON; nothing otherwise. */
function laidOut(text: string): string | null {
  const trimmed = text.trim();

  if ((trimmed.startsWith('{') && trimmed.endsWith('}')) || (trimmed.startsWith('[') && trimmed.endsWith(']'))) {
    try {
      return JSON.stringify(JSON.parse(trimmed), null, 2);
    } catch {
      // not JSON after all, which is fine
    }
  }

  return null;
}

/** A value with a way to copy it, which is what somebody reading a record often wants next. */
function Copyable({ text, children }: { text: string; children: ReactNode }) {
  const said = useEditorT().data.database;
  const [copied, setCopied] = useState(false);

  return (
    <div className="group relative pr-7">
      {children}
      <button
        type="button"
        onClick={async () => {
          try {
            await navigator.clipboard.writeText(text);
            setCopied(true);
            window.setTimeout(() => setCopied(false), 1500);
          } catch {
            // nothing to copy to is not worth an error
          }
        }}
        className="absolute right-0 top-0 rounded-full p-1 text-slate-400 opacity-60 hover:text-slate-700 group-hover:opacity-100 dark:hover:text-slate-200"
        aria-label={said.copy}
        title={said.copy}
      >
        {copied ? <IconCheck className="h-3.5 w-3.5 text-emerald-500" /> : <IconCopy className="h-3.5 w-3.5" />}
      </button>
    </div>
  );
}

/** The columns of a table: what each is called, what it holds, and what is asked of it. */
function Structure({ columns }: { columns: DatabaseColumn[] }) {
  const said = useEditorT().data.database;

  return (
    <ul className="surface grid gap-x-6 gap-y-1.5 p-3 text-[13px] sm:grid-cols-2 xl:grid-cols-3">
      {columns.map((column) => (
        <li key={column.name} className="flex min-w-0 flex-wrap items-baseline gap-x-2">
          <span className="truncate font-mono font-medium">{column.name}</span>
          <span className="font-mono text-xs uppercase text-slate-500">{column.type || said.untyped}</span>
          {column.primaryKey && <span className="text-[11px] text-amber-600 dark:text-amber-400">{said.primaryKey}</span>}
          {column.notNull && !column.primaryKey && <span className="text-[11px] text-slate-500">{said.required}</span>}
          {column.default != null && <span className="truncate text-[11px] text-slate-500">{said.defaultsTo(column.default)}</span>}
        </li>
      ))}
    </ul>
  );
}

/** How the code reaches the database, for whoever writes it - with the lines to copy. */
function HowTo() {
  const said = useEditorT().data.database;

  return (
    <aside className="border-l-2 border-slate-300 pl-3 text-[13px] text-slate-600 dark:border-ink-700 dark:text-slate-400">
      <h3 className="font-medium text-slate-700 dark:text-slate-300">{said.howTo}</h3>
      <p className="mt-1 leading-relaxed">{said.howToText((text) => <Snippet text={text} />)}</p>
    </aside>
  );
}

function Snippet({ text }: { text: string }) {
  const said = useEditorT().data.database;
  const [copied, setCopied] = useState(false);

  return (
    <span className="inline-flex items-center gap-1 whitespace-nowrap bg-slate-100 px-1.5 py-0.5 align-baseline dark:bg-ink-850">
      <code className="font-mono text-[12px] text-ink-900 dark:text-slate-200">{text}</code>
      <button
        type="button"
        onClick={async () => {
          try {
            await navigator.clipboard.writeText(text);
            setCopied(true);
            window.setTimeout(() => setCopied(false), 1500);
          } catch {
            // nothing to copy to is not worth an error
          }
        }}
        className="text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
        aria-label={said.copy}
        title={said.copy}
      >
        {copied ? <IconCheck className="h-3 w-3 text-emerald-500" /> : <IconCopy className="h-3 w-3" />}
      </button>
    </span>
  );
}
