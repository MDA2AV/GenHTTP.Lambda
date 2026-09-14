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
export function FileTabs({ files, active, onSelect, onChange, faulty }: Props) {
  const [adding, setAdding] = useState(false);
  const [name, setName] = useState('');
  const [problem, setProblem] = useState<string | null>(null);

  function add(event: React.FormEvent) {
    event.preventDefault();

    const wanted = name.trim().endsWith('.cs') ? name.trim() : `${name.trim()}.cs`;

    if (!/^[A-Za-z][A-Za-z0-9_-]*\.cs$/.test(wanted)) {
      setProblem('Letters, digits, dashes and underscores, ending in .cs');
      return;
    }

    if (files.some((file) => file.name.toLowerCase() === wanted.toLowerCase())) {
      setProblem('There is already a file with that name.');
      return;
    }

    // a new file starts as a comment rather than empty, because an empty file
    // compiles and therefore says nothing about what it is for
    onChange([...files, { name: wanted, code: `// Types for ${wanted.replace('.cs', '')}.\n// Everything here is compiled beside the snippet and needs no using.\n` }]);
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
            placeholder="Types.cs"
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
