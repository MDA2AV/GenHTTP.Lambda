import { useState } from 'react';

import { IconCheck, IconCopy, IconExternal } from './Icons';

interface Props {
  value: string;
  href?: string;
  label?: string;
  tone?: 'default' | 'accent';
}

/** A read-only URL with a copy button, and optionally a link to open it. */
export function CopyField({ value, href, label, tone = 'default' }: Props) {
  const [copied, setCopied] = useState(false);

  async function copy() {
    try {
      await navigator.clipboard.writeText(value);
    } catch {
      return;
    }

    setCopied(true);
    window.setTimeout(() => setCopied(false), 1600);
  }

  return (
    <div>
      {label && <div className="mb-1.5 text-xs font-medium uppercase tracking-wide text-slate-500">{label}</div>}
      <div
        className={`flex items-stretch overflow-hidden rounded-lg border ${
          tone === 'accent'
            ? 'border-accent-500/40 bg-accent-500/5'
            : 'border-slate-200 bg-slate-50 dark:border-ink-700 dark:bg-ink-850'
        }`}
      >
        <code className="min-w-0 flex-1 truncate px-3 py-2 font-mono text-sm text-ink-800 dark:text-slate-200">
          {value}
        </code>
        <button
          type="button"
          onClick={copy}
          title="Copy to clipboard"
          aria-label="Copy to clipboard"
          className="border-l border-slate-200 px-3 text-slate-500 transition-colors hover:bg-slate-100 hover:text-ink-800 dark:border-ink-700 dark:hover:bg-ink-800 dark:hover:text-slate-100"
        >
          {copied ? <IconCheck className="h-4 w-4 text-emerald-500" /> : <IconCopy />}
        </button>
        {href && (
          <a
            href={href}
            target="_blank"
            rel="noreferrer"
            title="Open in a new tab"
            aria-label="Open in a new tab"
            className="flex items-center border-l border-slate-200 px-3 text-slate-500 transition-colors hover:bg-slate-100 hover:text-ink-800 dark:border-ink-700 dark:hover:bg-ink-800 dark:hover:text-slate-100"
          >
            <IconExternal />
          </a>
        )}
      </div>
    </div>
  );
}
