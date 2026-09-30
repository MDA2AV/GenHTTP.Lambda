import { useEffect, useRef } from 'react';

interface Props {
  title: string;
  open: boolean;
  onClose: () => void;
  children: React.ReactNode;
  footer?: React.ReactNode;
  /** Room for more than a question: a record shown whole, say - wider, and scrolling where it is long. */
  wide?: boolean;
}

/** A small modal; closes on escape or a click outside the panel. */
export function Dialog({ title, open, onClose, children, footer, wide = false }: Props) {
  const panel = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    function onKey(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        onClose();
      }
    }

    window.addEventListener('keydown', onKey);

    return () => window.removeEventListener('keydown', onKey);
  }, [open, onClose]);

  if (!open) {
    return null;
  }

  return (
    <div
      className="fixed inset-0 z-40 flex items-center justify-center bg-black/40 p-4 backdrop-blur-sm"
      onMouseDown={(event) => {
        if (!panel.current?.contains(event.target as Node)) {
          onClose();
        }
      }}
    >
      <div ref={panel} role="dialog" aria-modal="true" aria-label={title} className={`surface w-full shadow-2xl ${wide ? 'max-w-2xl' : 'max-w-md'}`}>
        <h2 className="border-b border-slate-200 px-5 py-3.5 font-semibold dark:border-ink-800">{title}</h2>
        <div className={`space-y-3 px-5 py-4 text-sm ${wide ? 'max-h-[70vh] overflow-y-auto' : ''}`}>{children}</div>
        {footer && <div className="flex justify-end gap-2 border-t border-slate-200 px-5 py-3.5 dark:border-ink-800">{footer}</div>}
      </div>
    </div>
  );
}
