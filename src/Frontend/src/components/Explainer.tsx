import { useState } from 'react';

import { CSharp } from './CSharp';

/**
 * What a lambda is, shown rather than described.
 *
 * The snippet on the left and the tree on the right are the same thing twice -
 * one as it is written, one as it answers - and pointing at either lights up
 * its other half. A visitor who has never seen GenHTTP can learn what a layout
 * does by moving a pointer, which is a smaller ask than reading an API.
 */

interface Part {
  id: string;
  lines: number[];
  title: string;
  body: string;
}

const CODE = [
  'var books = new List<Book> { new(1, "Dune") };',
  '',
  'var api = Inline.Create()',
  '                .Get(() => books)',
  '                .Get(":id", (int id) => Find(id));',
  '',
  'Book Find(int id) => books.First(b => b.Id == id);',
  '',
  'return Layout.Create()',
  '             .Add("books", api)',
  '             .AddOpenApi()',
  '             .AddScalar();',
  '',
  'record Book(int Id, string Title);',
];

const PARTS: Part[] = [
  {
    id: 'data',
    lines: [0, 6, 13],
    title: 'Ordinary C#',
    body: 'A list, a lookup, a record. Nothing here knows it is on the web - it is the same code you would write anywhere.',
  },
  {
    id: 'handler',
    lines: [2, 3, 4],
    title: 'A handler answers requests',
    body: 'Inline turns functions into routes. Return a value and it is serialised; take a parameter named in the path and it is parsed for you.',
  },
  {
    id: 'layout',
    lines: [8, 9],
    title: 'A layout is a tree of handlers',
    body: 'Add puts a handler under a path segment. Layouts nest, so a tree of them is a site - and the thing you return from a lambda is one handler, whatever it is made of.',
  },
  {
    id: 'extras',
    lines: [10, 11],
    title: 'Documentation, for free',
    body: 'AddOpenApi reads the routes you just built and writes the specification. AddScalar serves a browser for it, so the API can be tried without leaving the page.',
  },
];

/** What the code above actually serves, as a visitor would find it. */
const TREE = [
  { id: 'layout', path: '/lambda/your-key/', note: 'the layout', depth: 0 },
  { id: 'handler', path: 'books/', note: 'every book', depth: 1, method: 'GET' },
  { id: 'handler', path: 'books/1', note: 'one of them', depth: 1, method: 'GET' },
  { id: 'extras', path: 'openapi.json', note: 'the specification', depth: 1 },
  { id: 'extras', path: 'scalar/', note: 'try it in the browser', depth: 1 },
];

export function Explainer() {
  const [active, setActive] = useState<string | null>(null);

  const part = PARTS.find((p) => p.id === active) ?? null;
  const lit = (id: string) => active === id;

  const partOf = (line: number) => PARTS.find((p) => p.lines.includes(line))?.id ?? null;

  return (
    <div className="grid gap-6 lg:grid-cols-[1.15fr_1fr] lg:items-start">
      {/* the code, a line at a time so a line can be pointed at */}
      <div className="surface overflow-hidden">
        <div className="flex items-stretch border-b border-grey-300 bg-grey-50 dark:border-ink-800 dark:bg-ink-950">
          <span className="border-b-2 border-accent-500 bg-white px-4 py-2 font-mono text-xs text-grey-900 dark:border-accent-400 dark:bg-ink-900 dark:text-grey-200">
            lambda.cs
          </span>
          <span className="ml-auto self-center px-4 font-mono text-[11px] uppercase tracking-wide text-grey-500">
            C#
          </span>
        </div>

        <div className="overflow-x-auto py-3 font-mono text-[12.5px] leading-[1.75]">
          {CODE.map((line, index) => {
            const id = partOf(index);

            return (
              <div
                key={index}
                onMouseEnter={() => id && setActive(id)}
                onMouseLeave={() => setActive(null)}
                onClick={() => setActive(id === active ? null : id)}
                className={`flex cursor-default transition-colors duration-200 ${
                  id && lit(id) ? 'bg-accent-500/10 dark:bg-accent-400/10' : ''
                }`}
              >
                <span
                  aria-hidden="true"
                  className={`w-10 shrink-0 select-none pr-3 text-right transition-colors duration-200 ${
                    id && lit(id) ? 'text-accent-500 dark:text-accent-400' : 'text-grey-500'
                  }`}
                >
                  {index + 1}
                </span>
                <span className="whitespace-pre pr-4 text-grey-900 dark:text-grey-300">
                  <CSharp code={line} />
                </span>
              </div>
            );
          })}
        </div>
      </div>

      <div className="space-y-4">
        {/* the same thing as a visitor meets it */}
        <div className="surface p-4">
          <div className="mb-3 text-[11px] uppercase tracking-wide text-grey-500">What it serves</div>

          <ul className="space-y-1">
            {TREE.map((node, index) => (
              <li
                key={index}
                onMouseEnter={() => setActive(node.id)}
                onMouseLeave={() => setActive(null)}
                onClick={() => setActive(node.id === active ? null : node.id)}
                style={{ paddingLeft: `${node.depth * 1.25}rem` }}
                className={`flex cursor-default items-baseline gap-2 rounded-full px-2 py-1 transition-colors duration-200 ${
                  lit(node.id) ? 'bg-accent-500/10 dark:bg-accent-400/10' : ''
                }`}
              >
                {node.method && (
                  <span className="shrink-0 font-mono text-[10px] text-emerald-600 dark:text-emerald-400">
                    {node.method}
                  </span>
                )}
                <span
                  className={`font-mono text-xs transition-colors duration-200 ${
                    lit(node.id) ? 'text-accent-500 dark:text-accent-400' : 'text-grey-900 dark:text-grey-200'
                  }`}
                >
                  {node.path}
                </span>
                <span className="ml-auto shrink-0 text-[11px] text-grey-500">{node.note}</span>
              </li>
            ))}
          </ul>
        </div>

        {/*
          One box that changes rather than four that appear: a panel arriving
          under the pointer moves everything below it, and a page that reflows
          while it is being read is worse than one that says less.
        */}
        <div className="surface relative min-h-[8.5rem] p-4">
          <div
            className={`transition-opacity duration-200 ${part === null ? 'opacity-100' : 'opacity-0'}`}
          >
            <div className="text-sm font-medium">Point at any line</div>
            <p className="mt-1.5 text-sm leading-relaxed text-grey-700 dark:text-grey-300">
              The snippet and the tree are the same lambda twice - once as it is written, once as it answers.
              Hovering either shows the other.
            </p>
          </div>

          {PARTS.map((candidate) => (
            <div
              key={candidate.id}
              aria-hidden={candidate.id !== active}
              className={`absolute inset-0 p-4 transition-opacity duration-200 ${
                candidate.id === active ? 'opacity-100' : 'pointer-events-none opacity-0'
              }`}
            >
              <div className="text-sm font-medium text-accent-500 dark:text-accent-400">{candidate.title}</div>
              <p className="mt-1.5 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{candidate.body}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
