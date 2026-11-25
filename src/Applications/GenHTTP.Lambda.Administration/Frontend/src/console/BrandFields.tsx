import { useRef, useState, type ReactNode } from 'react';

import type { AdminBranding, SharedFile } from '@genhttp-lambda/kit/api';
import { IconClose, IconSpinner, IconUpload } from '@genhttp-lambda/kit/components/Icons';
import { actionButton, bytes } from './parts';

/*
 * The fields of the branding: a logo, a colour, a font. Each says what it
 * holds and what may be done about it, and leaves saving to the form.
 */

/** A setting: what it is on the left, its field on the right. */
export function Row({ id, title, note, children }: { id: string; title: string; note: ReactNode; children: ReactNode }) {
  return (
    <div className="grid gap-3 p-4 sm:grid-cols-[1fr_minmax(0,24rem)] sm:items-start sm:gap-6">
      <div className="min-w-0">
        <label htmlFor={id} className="text-[15px] font-medium">{title}</label>
        <p className="mt-1 text-[13px] text-slate-500">{note}</p>
      </div>

      <div className="min-w-0">{children}</div>
    </div>
  );
}

/** The logo as the form holds it: as saved, a picture chosen and not uploaded yet, or taken away. */
export type LogoDraft = { kind: 'saved' } | { kind: 'chosen'; file: Blob; name: string; url: string } | { kind: 'removed' };

/**
 * The logo as it is, or as chosen: a tile with the picture, on which a file
 * may be dropped too, a button to choose one - from this computer, or one of
 * the shared pictures - and one to take it away.
 */
export function LogoField({ saved, draft, types, pictures, onChoose, onShared, onRemove }: {
  saved: AdminBranding;
  draft: LogoDraft;
  types: string[];
  pictures: SharedFile[];
  onChoose: (file: File | undefined) => void;
  onShared: (file: SharedFile) => void;
  onRemove: () => void;
}) {
  const input = useRef<HTMLInputElement>(null);
  const [over, setOver] = useState(false);

  const shown = draft.kind === 'chosen' ? draft.url : draft.kind === 'saved' && saved.logo !== null ? saved.picture : null;

  const described = draft.kind === 'chosen'
    ? `${draft.name}, ${bytes(draft.file.size)} - not saved yet`
    : draft.kind === 'removed'
      ? 'Taken away - not saved yet'
      : saved.logo !== null
        ? `${saved.logo.type.replace('image/', '').replace('+xml', '').toUpperCase()}, ${bytes(saved.logo.bytes)}`
        : 'None';

  return (
    <div className="flex items-start gap-3">
      <div
        onDragOver={(event) => {
          event.preventDefault();
          setOver(true);
        }}
        onDragLeave={() => setOver(false)}
        onDrop={(event) => {
          event.preventDefault();
          setOver(false);
          onChoose(event.dataTransfer.files[0]);
        }}
        className={`flex h-16 w-16 shrink-0 items-center justify-center border border-dashed bg-[repeating-conic-gradient(#e8eaed_0%_25%,#fff_0%_50%)] bg-[length:12px_12px] dark:bg-[repeating-conic-gradient(#303134_0%_25%,#292a2d_0%_50%)] ${
          over ? 'border-accent-500' : 'border-grey-300 dark:border-ink-700'
        }`}
        title="Drop a picture here"
      >
        {shown ? <img src={shown} alt="The logo" className="max-h-12 max-w-12 object-contain" /> : <IconUpload className="h-5 w-5 text-slate-400" />}
      </div>

      <div className="min-w-0 flex-1">
        <p className="truncate text-[13px] text-slate-600 dark:text-slate-400">{described}</p>

        <div className="mt-1.5 flex flex-wrap gap-1.5">
          <button type="button" onClick={() => input.current?.click()} className={`btn-ghost border border-grey-300 dark:border-ink-700 ${actionButton}`}>
            {shown ? 'Choose another' : 'Choose a picture'}
          </button>

          {shown && (
            <button type="button" onClick={onRemove} className={`btn-ghost ${actionButton}`}>
              Take it away
            </button>
          )}
        </div>

        {pictures.length > 0 && (
          <select
            aria-label="Take a shared picture"
            value=""
            onChange={(event) => {
              const file = pictures.find((picture) => picture.name === event.target.value);

              if (file) {
                onShared(file);
              }
            }}
            className="field mt-1.5 py-1.5 text-[13px]"
          >
            <option value="">Take a shared picture…</option>
            {pictures.map((picture) => (
              <option key={picture.name} value={picture.name}>{picture.name}</option>
            ))}
          </select>
        )}

        <input
          ref={input}
          id="branding-logo"
          type="file"
          accept={types.join(',')}
          className="sr-only"
          tabIndex={-1}
          onChange={(event) => {
            onChoose(event.target.files?.[0]);
            event.target.value = '';
          }}
        />
      </div>
    </div>
  );
}

/** A colour as it is entered: #rrggbb or #rgb, with or without the sign - or nothing, which is the platform's. */
export function parseColor(text: string): string | null | undefined {
  const digits = text.trim().replace(/^#/, '').toLowerCase();

  if (digits === '') {
    return null;
  }

  if (/^[0-9a-f]{3}$/.test(digits)) {
    return `#${[...digits].map((digit) => digit + digit).join('')}`;
  }

  return /^[0-9a-f]{6}$/.test(digits) ? `#${digits}` : undefined;
}

/**
 * A colour: the browser's own picker, the colour as text, and - since text in
 * it is drawn in a shade that can be read - what that comes to.
 */
export function ColorField({ id, value, platform, text, onChange }: {
  id: string;
  /** What was entered, whether or not it is a colour. */
  value: string;
  /** The platform's own, where nothing is entered. */
  platform: string;
  /** What text in it is drawn in on a light page and on a dark one. */
  text: { light: string; dark: string } | null;
  onChange: (value: string) => void;
}) {
  const parsed = parseColor(value);
  const shown = parsed ?? platform;
  const wrong = parsed === undefined;

  return (
    <div>
      <div className="flex items-center gap-2">
        <span className="relative h-[34px] w-[34px] shrink-0 border border-grey-300 dark:border-ink-700" style={{ backgroundColor: shown }}>
          <input
            type="color"
            aria-label="Pick the colour"
            value={shown}
            onChange={(event) => onChange(event.target.value)}
            className="absolute inset-0 h-full w-full cursor-pointer opacity-0"
          />
        </span>

        <input
          id={id}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          placeholder={platform}
          autoComplete="off"
          spellCheck={false}
          maxLength={7}
          aria-invalid={wrong}
          className={`field w-32 py-1.5 font-mono text-sm ${wrong ? '!border-red-500 !ring-red-500' : ''}`}
        />

        {value !== '' && (
          <button type="button" onClick={() => onChange('')} className={`btn-ghost ${actionButton}`}>
            The platform&rsquo;s
          </button>
        )}
      </div>

      {wrong && <p className="mt-1 text-xs text-red-500">A colour as #rrggbb.</p>}

      {!wrong && text && (
        <p className="mt-1.5 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-slate-500">
          As text
          <Swatch color={text.light} label="on light" />
          <Swatch color={text.dark} label="on dark" />
        </p>
      )}
    </div>
  );
}

function Swatch({ color, label }: { color: string; label: string }) {
  return (
    <span className="inline-flex items-center gap-1.5">
      <span className="h-3 w-3 border border-black/10 dark:border-white/20" style={{ backgroundColor: color }} />
      <span className="font-mono">{color}</span> {label}
    </span>
  );
}

/** The weights a font file draws, as a person reads them. */
export function drawn({ font }: SharedFile): string {
  if (!font) {
    return '';
  }

  const weight = font.from === font.to ? String(font.from) : `${font.from}–${font.to}`;

  return `${font.family} · ${weight}${font.italic ? ' italic' : ''}`;
}

/**
 * A font: the shared files it is made of, with what each draws, and two ways
 * to add one - a shared font, or a file of this computer, which is shared as
 * it is added.
 */
export function FontField({ id, chosen, offered, limit, busy, problem, accept, notes = [], onAdd, onRemove, onUpload }: {
  id: string;
  chosen: SharedFile[];
  /** The shared fonts that are not part of it. */
  offered: SharedFile[];
  /** How many files a font may be made of. */
  limit: number;
  busy: boolean;
  problem: string | null;
  accept: string;
  /** What is to be known about the font as it stands, where there is something. */
  notes?: (string | null)[];
  onAdd: (file: SharedFile) => void;
  onRemove: (file: SharedFile) => void;
  onUpload: (files: File[]) => void;
}) {
  const input = useRef<HTMLInputElement>(null);
  const [over, setOver] = useState(false);

  const full = chosen.length >= limit;

  return (
    <div
      onDragOver={(event) => {
        event.preventDefault();
        setOver(true);
      }}
      onDragLeave={() => setOver(false)}
      onDrop={(event) => {
        event.preventDefault();
        setOver(false);
        onUpload([...event.dataTransfer.files]);
      }}
      className={over ? 'outline-dashed outline-1 outline-offset-4 outline-accent-500' : ''}
    >
      {chosen.length > 0 && (
        <ul className="mb-2 divide-y divide-grey-300 border border-grey-300 dark:divide-ink-800 dark:border-ink-800">
          {chosen.map((file) => (
            <li key={file.name} className="flex items-center gap-2 px-3 py-1.5">
              <div className="min-w-0 flex-1">
                <p className="truncate font-mono text-[13px]" title={file.name}>{file.name}</p>
                <p className="truncate text-xs text-slate-500">{drawn(file)}</p>
              </div>
              <button
                type="button"
                onClick={() => onRemove(file)}
                className="rounded-full p-1 text-slate-400 hover:bg-accent-500/10 hover:text-accent-500"
                aria-label={`Take ${file.name} out`}
                title="Take it out"
              >
                <IconClose className="h-3.5 w-3.5" />
              </button>
            </li>
          ))}
        </ul>
      )}

      <div className="flex flex-wrap items-center gap-1.5">
        {offered.length > 0 && (
          <select
            id={id}
            aria-label="Add a shared font"
            value=""
            disabled={full}
            onChange={(event) => {
              const file = offered.find((font) => font.name === event.target.value);

              if (file) {
                onAdd(file);
              }
            }}
            className="field min-w-0 flex-1 py-1.5 text-[13px]"
          >
            <option value="">{chosen.length === 0 ? 'The system’s - or a shared font…' : 'Add a shared font…'}</option>
            {offered.map((file) => (
              <option key={file.name} value={file.name}>{file.name} - {drawn(file)}</option>
            ))}
          </select>
        )}

        <button
          type="button"
          id={offered.length > 0 ? undefined : id}
          onClick={() => input.current?.click()}
          disabled={busy || full}
          className={`btn-ghost border border-grey-300 dark:border-ink-700 ${actionButton}`}
        >
          {busy ? <IconSpinner className="h-4 w-4" /> : <IconUpload className="h-4 w-4" />}
          Upload
        </button>

        {chosen.length === 0 && offered.length === 0 && <span className="text-[13px] text-slate-500">None yet</span>}

        <input
          ref={input}
          type="file"
          accept={accept}
          multiple
          className="sr-only"
          tabIndex={-1}
          onChange={(event) => {
            onUpload([...(event.target.files ?? [])]);
            event.target.value = '';
          }}
        />
      </div>

      {notes.map((note) => note && <p key={note} className="mt-1.5 text-xs text-amber-600 dark:text-amber-400">{note}</p>)}
      {problem && <p className="mt-1.5 text-xs text-red-500">{problem}</p>}
    </div>
  );
}
