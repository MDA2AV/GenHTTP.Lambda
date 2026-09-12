import type { Diagnostic } from '../api';
import { IconAlert, IconCheck } from './Icons';

interface Props {
  diagnostics: Diagnostic[];
  state: 'idle' | 'clean';
  onSelect: (diagnostic: Diagnostic) => void;
}

/** The build output of the last check or deployment. */
export function Diagnostics({ diagnostics, state, onSelect }: Props) {
  if (diagnostics.length === 0) {
    return (
      <div className="flex items-center gap-2 px-4 py-3 text-sm text-slate-500">
        {state === 'clean' ? (
          <>
            <IconCheck className="h-4 w-4 text-emerald-500" />
            The code compiles.
          </>
        ) : (
          'No messages yet. Check or deploy to compile your code.'
        )}
      </div>
    );
  }

  return (
    <ul className="divide-y divide-slate-200 overflow-y-auto dark:divide-ink-800">
      {diagnostics.map((diagnostic, index) => {
        const isError = diagnostic.severity !== 'Warning';

        return (
          <li key={`${diagnostic.id}-${index}`}>
            <button
              type="button"
              onClick={() => onSelect(diagnostic)}
              disabled={diagnostic.line === 0}
              className="flex w-full items-start gap-2.5 px-4 py-2.5 text-left text-sm transition-colors hover:bg-slate-50 disabled:cursor-default disabled:hover:bg-transparent dark:hover:bg-ink-850 dark:disabled:hover:bg-transparent"
            >
              <IconAlert
                className={`mt-0.5 h-4 w-4 shrink-0 ${isError ? 'text-red-500' : 'text-amber-500'}`}
              />
              <span className="min-w-0 flex-1">
                <span className="break-words">{diagnostic.message}</span>
                <span className="ml-2 whitespace-nowrap font-mono text-xs text-slate-500">
                  {diagnostic.id}
                  {diagnostic.line > 0 && ` · line ${diagnostic.line}`}
                </span>
              </span>
            </button>
          </li>
        );
      })}
    </ul>
  );
}
