import { useState } from 'react';

import type { LambdaFile } from '../api';
import { IconPlus, IconTrash } from './Icons';

/**
 * The file the snippet lives in, which cannot be renamed or removed: it is the
 * one that returns a handler, and a lambda without it has nothing to run.
 */
export const ENTRY = 'lambda.cs';

/** As many as the server will store for one lambda. */
const MAX_FILES = 12;

interface Props {
  files: LambdaFile[];
  active: string;
  onSelect: (name: string) => void;
  onChange: (files: LambdaFile[]) => void;
  /** Files a diagnostic points at, so a mistake is visible before it is opened. */
  faulty?: Set<string>;
}

/**
 * The files of a lambda, as a strip above the editor.
 *
 * Types used to have to sit underneath the code that used them, because there
 * was only one file to put them in. The snippet is still first and still the
 * one that runs; everything else is ordinary C# compiled beside it, in the
 * same namespace, so nothing has to be imported to be reached.
 */
/** What a C# file may be called, which is narrow because the name reaches a
 *  line directive and a diagnostic. */
function checkCode(name: string): string | null {
  return /^[A-Za-z][A-Za-z0-9_-]*\.cs$/.test(name)
    ? null
    : 'Letters, digits, dashes and underscores, ending in .cs';
}

/**
 * What anything else may be called. Wider, because it becomes a real file in
 * a real directory - which is the point - so what matters is where it can end
 * up rather than how it reads. The same rule the server applies.
 */
function checkAsset(name: string): string | null {
  if (name.length > 120 || name.startsWith('/') || name.endsWith('/')) {
    return 'No leading or trailing slash, and under 120 characters.';
  }

  const parts = name.split('/');

  if (parts.length > 6) {
    return 'At most six folders deep.';
  }

  for (const part of parts) {
    if (!part || part.length > 60 || part.startsWith('.') || !/^[A-Za-z0-9._-]+$/.test(part)) {
      return 'Letters, digits, dashes, underscores and dots, separated by slashes.';
    }
  }

  return /\.[A-Za-z0-9]+$/.test(name) ? null : 'It needs an extension, so it can be served as the right thing.';
}

/**
 * What a new file starts as. Never empty: an empty file is valid and says
 * nothing about what it is for.
 */
function starterFor(name: string): string {
  if (name.endsWith('.cs')) {
    const stem = name.slice(0, -3);

    return `// Types for ${stem}.\n// Everything here is compiled beside the snippet and needs no using.\n`;
  }

  if (name.endsWith('.html') || name.endsWith('.htm')) {
    const folder = name.includes('/') ? name.slice(0, name.lastIndexOf('/')) : null;

    return [
      '<!doctype html>',
      '<html lang="en">',
      '<meta charset="utf-8">',
      '<meta name="viewport" content="width=device-width,initial-scale=1">',
      '<title>My app</title>',
      '',
      '<h1>It is served</h1>',
      '',
      folder
        ? `<!-- Serve this folder from lambda.cs with:\n     return Layout.Create().Add(Assets.App("${folder}")); -->`
        : '<!-- Serve these files from lambda.cs with:\n     return Layout.Create().Add(Assets.App()); -->',
      '',
    ].join('\n');
  }

  if (name.endsWith('.css')) {
    return 'body {\n  font: 16px/1.5 system-ui, sans-serif;\n  margin: 3rem auto;\n  max-width: 40rem;\n}\n';
  }

  if (name.endsWith('.js') || name.endsWith('.mjs')) {
    return '// Served as it is. Nothing here is compiled or inspected.\n';
  }

  return '';
}

export function FileTabs({ files, active, onSelect, onChange, faulty }: Props) {
  const [adding, setAdding] = useState(false);
  const [name, setName] = useState('');
  const [problem, setProblem] = useState<string | null>(null);

  function add(event: React.FormEvent) {
    event.preventDefault();

    /*
     * A name with no extension is taken to be C#, which is what it almost
     * always was. Anything else is a file to be served as it is - and it may
     * name a folder, because that is how a front end gets to be a folder
     * rather than a heap: site/index.html, site/app.js, and so on.
     */
    const typed = name.trim();

    const wanted = typed.includes('.') ? typed : `${typed}.cs`;

    const wrong = wanted.endsWith('.cs') ? checkCode(wanted) : checkAsset(wanted);

    if (wrong) {
      setProblem(wrong);
      return;
    }

    if (files.some((file) => file.name.toLowerCase() === wanted.toLowerCase())) {
      setProblem('There is already a file with that name.');
      return;
    }

    // a new file starts as a comment rather than empty, because an empty file
    // compiles and therefore says nothing about what it is for
    onChange([...files, { name: wanted, code: starterFor(wanted) }]);
    onSelect(wanted);

    setName('');
    setProblem(null);
    setAdding(false);
  }

  function remove(target: string) {
    if (target === ENTRY) {
      return;
    }

    if (!window.confirm(`Remove ${target}? Its contents go with it.`)) {
      return;
    }

    onChange(files.filter((file) => file.name !== target));

    if (active === target) {
      onSelect(ENTRY);
    }
  }

  return (
    <div className="flex flex-wrap items-center gap-1 border-b border-slate-200 bg-slate-50 px-2 py-1.5 dark:border-ink-800 dark:bg-ink-950">
      {files.map((file) => {
        const open = file.name === active;

        return (
          <div key={file.name} className="flex items-center">
            <button
              type="button"
              onClick={() => onSelect(file.name)}
              className={`px-3 py-1 font-mono text-xs ${
                open
                  ? 'bg-white text-slate-900 dark:bg-ink-900 dark:text-slate-100'
                  : 'text-slate-500 hover:bg-white/70 dark:hover:bg-ink-900/70'
              }`}
              title={file.name === ENTRY ? 'The snippet: what this returns is what gets hosted' : file.name}
            >
              {file.name}
              {faulty?.has(file.name) && <span className="ml-1.5 text-red-500" aria-label="has errors">•</span>}
            </button>

            {file.name !== ENTRY && open && (
              <button
                type="button"
                onClick={() => remove(file.name)}
                className="px-1.5 py-1 text-slate-400 hover:text-red-500"
                aria-label={`Remove ${file.name}`}
              >
                <IconTrash className="h-3.5 w-3.5" />
              </button>
            )}
          </div>
        );
      })}

      {adding ? (
        <form onSubmit={add} className="flex items-center gap-1">
          <input
            autoFocus
            value={name}
            onChange={(event) => setName(event.target.value)}
            onBlur={() => { setAdding(false); setProblem(null); }}
            placeholder="Types.cs or site/index.html"
            className="w-32 border border-slate-300 bg-white px-2 py-1 font-mono text-xs dark:border-ink-700 dark:bg-ink-900"
          />
          {problem && <span className="text-xs text-red-500">{problem}</span>}
        </form>
      ) : (
        files.length < MAX_FILES && (
          <button
            type="button"
            onClick={() => setAdding(true)}
            className="px-2 py-1 text-slate-400 hover:text-accent-500"
            title="Add a file"
            aria-label="Add a file"
          >
            <IconPlus className="h-3.5 w-3.5" />
          </button>
        )
      )}
    </div>
  );
}
