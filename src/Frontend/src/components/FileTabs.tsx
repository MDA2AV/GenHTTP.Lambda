import { useRef, useState } from 'react';

import type { LambdaFile } from '../api';
import { encodeBytes, readable } from '../bytes';
import { pill } from '../control/ui';
import { IconPlus, IconTrash, IconUpload } from './Icons';

/**
 * The file the snippet lives in, which cannot be renamed or removed: it is the
 * one that returns a handler, and a lambda without it has nothing to run.
 */
export const ENTRY = 'lambda.cs';

/** As many C# files as the server will store for one lambda. */
const MAX_FILES = 12;

/** As many other files, which are counted apart. */
const MAX_ASSETS = 60;

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
  const picker = useRef<HTMLInputElement>(null);
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

    if (wanted.endsWith('.cs') && files.filter((file) => file.name.endsWith('.cs')).length >= MAX_FILES) {
      setProblem(`A lambda has at most ${MAX_FILES} C# files.`);
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

  /*
   * Anything that is not text - an image, a font - cannot be typed into the
   * editor, so it comes in here. It lands beside the file that is open when
   * that is an asset in a folder, which is where a picture for a page goes.
   */
  async function upload(chosen: FileList | null) {
    if (!chosen) {
      return;
    }

    const folder = !active.endsWith('.cs') && active.includes('/') ? active.slice(0, active.lastIndexOf('/') + 1) : '';

    const added: LambdaFile[] = [];

    for (const file of Array.from(chosen)) {
      const wanted = `${folder}${file.name}`;

      if ([...files, ...added].some((one) => one.name.toLowerCase() === wanted.toLowerCase())) {
        setProblem(`${wanted} is already there.`);
        continue;
      }

      const bytes = new Uint8Array(await file.arrayBuffer());

      added.push(readable(bytes)
        ? { name: wanted, code: new TextDecoder().decode(bytes) }
        : { name: wanted, code: encodeBytes(bytes), encoding: 'base64' });
    }

    if (picker.current) {
      picker.current.value = '';
    }

    if (added.length > 0) {
      onChange([...files, ...added]);
      onSelect(added[0].name);
    }
  }

  const code = files.filter((file) => file.name.endsWith('.cs')).length;
  const assets = files.length - code;

  return (
    <div className="flex flex-wrap items-center gap-1.5">
      {files.map((file) => {
        const open = file.name === active;

        return (
          <span key={file.name} className={`${pill(open)} !py-0.5 !pr-1.5`}>
            <button
              type="button"
              onClick={() => onSelect(file.name)}
              className="py-0.5 font-mono text-[12.5px]"
              title={file.name === ENTRY ? 'The snippet: what it returns is what gets served' : file.name}
            >
              {file.name}
              {faulty?.has(file.name) && <span className="ml-1.5 text-red-500" aria-label="has errors">•</span>}
            </button>

            {/* the same room on every pill, shown or not, so opening a file
                does not widen its pill and push the others along */}
            {file.name !== ENTRY ? (
              <button
                type="button"
                onClick={() => remove(file.name)}
                className={`rounded-full p-0.5 text-slate-400 hover:text-red-500 ${open ? '' : 'invisible'}`}
                aria-label={`Remove ${file.name}`}
                title="Remove this file"
                tabIndex={open ? 0 : -1}
              >
                <IconTrash className="h-3 w-3" />
              </button>
            ) : (
              <span className="w-4" aria-hidden="true" />
            )}
          </span>
        );
      })}

      {adding ? (
        <form onSubmit={add} className="flex items-center gap-2">
          <input
            autoFocus
            value={name}
            onChange={(event) => setName(event.target.value)}
            onBlur={() => { setAdding(false); setProblem(null); }}
            placeholder="Types.cs or site/index.html"
            className="w-48 rounded-full border border-slate-300 bg-white px-3 py-1 font-mono text-xs dark:border-ink-700 dark:bg-ink-900"
          />
        </form>
      ) : (
        (code < MAX_FILES || assets < MAX_ASSETS) && (
          <button
            type="button"
            onClick={() => setAdding(true)}
            className="rounded-full p-1.5 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500"
            title="New file"
            aria-label="New file"
          >
            <IconPlus className="h-3.5 w-3.5" />
          </button>
        )
      )}

      {assets < MAX_ASSETS && (
        <>
          <input ref={picker} type="file" multiple className="hidden" onChange={(event) => upload(event.target.files)} />
          <button
            type="button"
            onClick={() => picker.current?.click()}
            className="rounded-full p-1.5 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500"
            title="Upload a file - an image, a font, a page"
            aria-label="Upload a file"
          >
            <IconUpload className="h-3.5 w-3.5" />
          </button>
        </>
      )}

      {problem && <span className="text-xs text-red-500">{problem}</span>}
    </div>
  );
}
