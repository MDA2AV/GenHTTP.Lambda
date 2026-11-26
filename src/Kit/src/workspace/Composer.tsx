import { useEffect, useLayoutEffect, useRef, useState, type ReactNode, type RefObject } from 'react';

import { FileList } from '../components/AgentRun';
import { IconAlert, IconArrowUp, IconPaperclip, IconSpinner, IconStop } from '../components/Icons';
import type { Attachments } from '../components/useAttachments';
import { useT } from '../i18n';
import { bytes } from '../ui/format';

/** Measured before it is drawn in a browser; a page rendered on the server has nothing to measure. */
const useMeasure = typeof window === 'undefined' ? useEffect : useLayoutEffect;

/**
 * The box a message is typed into, at the foot of a conversation - or in the
 * middle of the page, before there is one.
 *
 * Enter sends and Shift + Enter starts a new line, as in every chat people
 * know; on a phone, whose keyboard has no Shift worth the name, Enter is a new
 * line and the button sends. It grows with what is typed, up to a point.
 *
 * Where the agent takes files, a paper clip picks them - pasting or dropping
 * one works as well - and they are shown above what is typed until sent.
 * Where it takes none there is no paper clip, rather than one that refuses.
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
  attachments,
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
  /** The files the message goes with, where the agent takes any. */
  attachments?: Attachments | null;
}) {
  const said = useT().workspace;
  const own = useRef<HTMLTextAreaElement>(null);
  const box = field ?? own;
  const picker = useRef<HTMLInputElement>(null);
  const [dragging, setDragging] = useState(false);

  const policy = attachments?.policy ?? null;
  const full = policy != null && attachments != null && attachments.files.length >= policy.maxCount;

  const take = (files: FileList | null | undefined) => {
    if (attachments && files && files.length > 0) {
      attachments.add([...files]);
    }
  };

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
        onDragOver={(event) => {
          if (policy && !disabled && event.dataTransfer.types.includes('Files')) {
            event.preventDefault();
            setDragging(true);
          }
        }}
        onDragLeave={() => setDragging(false)}
        onDrop={(event) => {
          if (policy && !disabled) {
            event.preventDefault();
            setDragging(false);
            take(event.dataTransfer.files);
          }
        }}
        className={`surface p-2 shadow-sm transition-colors focus-within:border-accent-500 focus-within:ring-1 focus-within:ring-accent-500 dark:focus-within:border-accent-400 dark:focus-within:ring-accent-400 ${
          disabled ? 'opacity-60' : ''
        } ${dragging ? 'border-accent-500 ring-1 ring-accent-500' : ''}`}
      >
        {attachments && (attachments.files.length > 0 || attachments.uploading) && (
          <div className="flex flex-wrap items-center gap-2 px-1 pb-2 pt-1">
            <FileList files={attachments.files} download={said.files.save} onRemove={attachments.remove} remove={said.files.remove} />
            {attachments.uploading && <IconSpinner className="h-4 w-4 text-slate-400" aria-label={said.files.uploading} />}
          </div>
        )}

        <div className="flex items-end gap-2">
        {policy && (
          <>
            <input
              ref={picker}
              type="file"
              multiple={policy.maxCount > 1}
              accept={attachments?.accept}
              className="hidden"
              tabIndex={-1}
              onChange={(event) => {
                take(event.target.files);
                event.target.value = '';
              }}
            />
            <button
              type="button"
              onClick={() => picker.current?.click()}
              disabled={disabled || full}
              aria-label={said.files.attach}
              title={said.files.attachHint(policy.maxCount, bytes(policy.maxBytes))}
              className="mb-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-slate-500 transition hover:bg-slate-100 hover:text-slate-800 disabled:opacity-40 dark:hover:bg-ink-850 dark:hover:text-slate-200"
            >
              <IconPaperclip className="h-[18px] w-[18px]" />
            </button>
          </>
        )}

        <textarea
          ref={box}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          onPaste={(event) => {
            // a picture pasted goes with the message, where the agent takes files
            if (policy && event.clipboardData.files.length > 0) {
              event.preventDefault();
              take(event.clipboardData.files);
            }
          }}
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
            className="btn-send mb-0.5 h-9 w-9 shrink-0"
          >
            {busy ? <IconSpinner className="h-4 w-4" /> : <IconArrowUp className="h-4 w-4" />}
          </button>
        )}
        </div>
      </div>

      {attachments?.problem && (
        <p className="mt-2 flex items-start gap-1.5 px-1 text-xs text-amber-700 dark:text-amber-400">
          <IconAlert className="mt-px h-3.5 w-3.5 shrink-0" />
          {attachments.problem}
        </p>
      )}

      {footer && <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-2 px-1 text-xs text-slate-500">{footer}</div>}
    </div>
  );
}
