import { useEffect, useLayoutEffect, useRef, type ReactNode, type RefObject } from 'react';

import { IconArrowUp, IconSpinner, IconStop } from '../components/Icons';
import { useT } from '../i18n';

/** Measured before it is drawn in a browser; a page rendered on the server has nothing to measure. */
const useMeasure = typeof window === 'undefined' ? useEffect : useLayoutEffect;

/**
 * The box a message is typed into, at the foot of a conversation - or in the
 * middle of the page, before there is one.
 *
 * Enter sends and Shift + Enter starts a new line, as in every chat people
 * know; on a phone, whose keyboard has no Shift worth the name, Enter is a new
 * line and the button sends. It grows with what is typed, up to a point.
 */
export function Composer({
  value,
  onChange,
  onSend,
  onStop,
  placeholder,
  ready,
  busy = false,
  running = false,
  stopping = false,
  disabled = false,
  large = false,
  field,
  footer,
  label,
}: {
  value: string;
  onChange: (text: string) => void;
  onSend: () => void;
  /** Stops what the agent is doing; offered in place of sending while it works. */
  onStop?: () => void;
  placeholder: string;
  /** Whether what is in the box can be sent. */
  ready: boolean;
  /** Sending: the box keeps what it has until it was taken. */
  busy?: boolean;
  /** The agent is working on the last message. */
  running?: boolean;
  stopping?: boolean;
  disabled?: boolean;
  /** The first box of a page, before anything was said: taller, with larger words. */
  large?: boolean;
  field?: RefObject<HTMLTextAreaElement>;
  /** What goes under the box: switches, the allowance. */
  footer?: ReactNode;
  /** What the box is, for whoever cannot see the placeholder. */
  label: string;
}) {
  const said = useT().workspace;
  const own = useRef<HTMLTextAreaElement>(null);
  const box = field ?? own;

  useMeasure(() => {
    const area = box.current;

    if (area) {
      area.style.height = 'auto';
      area.style.height = `${Math.min(area.scrollHeight, large ? 320 : 240)}px`;
    }
  }, [value, box, large]);

  // a phone's keyboard sends with its button; Enter there is a new line
  const coarse = typeof window !== 'undefined' && window.matchMedia?.('(pointer: coarse)').matches;

  return (
    <div>
      <div
        className={`surface flex items-end gap-2 p-2 shadow-sm transition-colors focus-within:border-accent-500 focus-within:ring-1 focus-within:ring-accent-500 dark:focus-within:border-accent-400 dark:focus-within:ring-accent-400 ${
          disabled ? 'opacity-60' : ''
        }`}
      >
        <textarea
          ref={box}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' && !event.shiftKey && !coarse && !event.nativeEvent.isComposing) {
              event.preventDefault();

              if (ready && !running) {
                onSend();
              }
            }
          }}
          rows={large ? 3 : 1}
          maxLength={2000}
          disabled={disabled}
          aria-label={label}
          placeholder={placeholder}
          className={`block w-full resize-none bg-transparent px-3 py-2 outline-none placeholder:text-slate-400 disabled:cursor-not-allowed dark:placeholder:text-slate-500 ${
            large ? 'min-h-[5.5rem] text-lg leading-relaxed' : 'min-h-[2.5rem] text-[15px] leading-relaxed'
          }`}
        />

        {running && onStop ? (
          <button
            type="button"
            onClick={onStop}
            disabled={stopping}
            aria-label={stopping ? said.stopping : said.stop}
            title={stopping ? said.stopping : said.stop}
            className="mb-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-slate-800 text-white transition hover:bg-slate-700 disabled:opacity-50 dark:bg-slate-200 dark:text-ink-950 dark:hover:bg-white"
          >
            {stopping ? <IconSpinner className="h-4 w-4" /> : <IconStop className="h-3.5 w-3.5" />}
          </button>
        ) : (
          <button
            type="button"
            onClick={onSend}
            disabled={!ready || running || disabled}
            aria-label={said.send}
            title={said.send}
            className="mb-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-accent-600 text-white transition hover:bg-accent-700 disabled:bg-slate-300 disabled:text-white dark:bg-accent-500 dark:hover:bg-accent-400 dark:disabled:bg-ink-700 dark:disabled:text-slate-500"
          >
            {busy ? <IconSpinner className="h-4 w-4" /> : <IconArrowUp className="h-4 w-4" />}
          </button>
        )}
      </div>

      {footer && <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-2 px-1 text-xs text-slate-500">{footer}</div>}
    </div>
  );
}

/** Which of the offered models answers, where a second one is offered behind a password. */
export function ModelChoice({
  model,
  onModel,
  password,
  onPassword,
}: {
  model: 'opus' | 'fable';
  onModel: (model: 'opus' | 'fable') => void;
  password: string;
  onPassword: (password: string) => void;
}) {
  const t = useT();

  return (
    <span className="flex items-center gap-1.5">
      <span className="text-slate-400">{t.workspace.model}</span>
      {(
        [
          ['opus', 'Opus 5.5'],
          ['fable', 'Fable 5.1'],
        ] as const
      ).map(([id, name]) => (
        <button
          key={id}
          type="button"
          onClick={() => onModel(id)}
          aria-pressed={model === id}
          className={`rounded-full border px-2 py-px ${
            model === id
              ? 'border-accent-500 bg-accent-500/10 text-accent-700 dark:border-accent-400 dark:text-accent-400'
              : 'border-slate-200 hover:text-slate-800 dark:border-ink-800 dark:hover:text-slate-200'
          }`}
        >
          {name}
        </button>
      ))}
      {model === 'fable' && (
        <input
          type="password"
          value={password}
          onChange={(event) => onPassword(event.target.value)}
          placeholder={t.buildBox.password}
          autoComplete="off"
          className="field !h-auto !w-28 !py-0.5 text-xs"
        />
      )}
    </span>
  );
}
