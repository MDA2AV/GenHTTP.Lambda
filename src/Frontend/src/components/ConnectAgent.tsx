import { useRef, useState } from 'react';

import { CopyField } from './CopyField';
import { IconCheck, IconCopy } from './Icons';
import { useT } from '../i18n';

/*
 * How to connect an agent of one's own to this server over MCP: the address,
 * and what to do with it in the agents people actually have. The one way the
 * site explains it, wherever it does - the front page, /ship, /build and the
 * Change section of the editor. What to say to the agent afterwards differs
 * from page to page, so that is left to the page.
 */

interface Setup {
  id: 'claudeCode' | 'claude' | 'cursor' | 'vscode';
  name: string;
  /** What to paste, where there is something to paste. */
  code?: (origin: string) => string;
  where?: string;
}

const SETUPS: Setup[] = [
  {
    id: 'claudeCode',
    name: 'Claude Code',
    code: (origin) => `claude mcp add --transport http genhttp ${origin}/mcp`,
  },
  {
    id: 'claude',
    name: 'Claude',
  },
  {
    id: 'cursor',
    name: 'Cursor',
    where: '~/.cursor/mcp.json',
    code: (origin) => JSON.stringify({ mcpServers: { genhttp: { url: `${origin}/mcp` } } }, null, 2),
  },
  {
    id: 'vscode',
    name: 'VS Code',
    where: '.vscode/mcp.json',
    code: (origin) => JSON.stringify({ servers: { genhttp: { type: 'http', url: `${origin}/mcp` } } }, null, 2),
  },
];

interface Props {
  origin: string;
  /** The editor link of a lambda, for the agent to change it with. */
  editorLink?: string;
}

export function ConnectAgent({ origin, editorLink }: Props) {
  const said = useT().connect;

  const [active, setActive] = useState(SETUPS[0].id);
  const tabs = useRef<(HTMLButtonElement | null)[]>([]);

  const setup = SETUPS.find((candidate) => candidate.id === active) ?? SETUPS[0];

  const intro = setup.id === 'claude' ? said.setups.claude((text) => <strong>{text}</strong>) : said.setups[setup.id];

  // arrow keys move between the tabs, the way a tab list is expected to work
  const onKey = (event: React.KeyboardEvent, index: number) => {
    const step = event.key === 'ArrowRight' ? 1 : event.key === 'ArrowLeft' ? -1 : 0;

    if (step === 0) {
      return;
    }

    event.preventDefault();

    const next = (index + step + SETUPS.length) % SETUPS.length;

    setActive(SETUPS[next].id);
    tabs.current[next]?.focus();
  };

  return (
    <div className="space-y-4">
      <CopyField label={said.address} value={`${origin}/mcp`} tone="accent" />

      {editorLink && <CopyField label={said.editorLink} value={editorLink} />}

      <div className="border border-grey-200 bg-white/80 backdrop-blur dark:border-ink-800 dark:bg-ink-900/80">
        <div
          role="tablist"
          aria-label={said.yourAgent}
          className="flex overflow-x-auto border-b border-grey-200 [scrollbar-width:none] dark:border-ink-800 [&::-webkit-scrollbar]:hidden"
        >
          {SETUPS.map((candidate, i) => {
            const selected = candidate.id === active;

            return (
              <button
                key={candidate.id}
                ref={(element) => {
                  tabs.current[i] = element;
                }}
                type="button"
                role="tab"
                id={`connect-tab-${candidate.id}`}
                aria-selected={selected}
                aria-controls={`connect-panel-${candidate.id}`}
                tabIndex={selected ? 0 : -1}
                onClick={() => setActive(candidate.id)}
                onKeyDown={(event) => onKey(event, i)}
                className={`relative shrink-0 whitespace-nowrap px-4 py-3.5 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-accent-500 ${
                  selected
                    ? 'text-accent-600 dark:text-accent-400'
                    : 'text-grey-600 hover:text-grey-900 dark:text-grey-400 dark:hover:text-grey-100'
                }`}
              >
                {candidate.name}
                <span
                  aria-hidden="true"
                  className={`absolute inset-x-3 bottom-0 h-0.5 transition-opacity ${
                    selected ? 'bg-accent-500 opacity-100 dark:bg-accent-400' : 'opacity-0'
                  }`}
                />
              </button>
            );
          })}
        </div>

        <div
          role="tabpanel"
          id={`connect-panel-${setup.id}`}
          aria-labelledby={`connect-tab-${setup.id}`}
          className="min-h-[10rem] p-5 sm:p-6"
        >
          <p className="text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{intro}</p>
          {setup.code && <CodeBlock code={setup.code(origin)} caption={setup.where ?? said.terminal} />}
        </div>

        <p className="border-t border-grey-200 px-5 py-4 text-sm leading-relaxed text-grey-600 sm:px-6 dark:border-ink-800 dark:text-grey-400">
          {said.elsewhere}
        </p>
      </div>
    </div>
  );
}

/** Text to paste somewhere else, with a button that copies all of it. */
function CodeBlock({ code, caption }: { code: string; caption: string }) {
  const [copied, copy] = useCopy(code);
  const t = useT();

  return (
    <div className="mt-4 bg-ink-950 text-grey-200">
      <div className="flex items-center justify-between gap-3 border-b border-white/10 pl-4 pr-1">
        <span className="truncate font-mono text-xs text-grey-500">{caption}</span>
        <button
          type="button"
          onClick={copy}
          className="flex h-10 shrink-0 items-center gap-1.5 px-3 text-xs font-medium text-grey-400 transition-colors hover:text-white"
        >
          {copied ? <IconCheck className="h-4 w-4 text-emerald-400" /> : <IconCopy />}
          {copied ? t.common.copied : t.common.copy}
        </button>
      </div>
      <pre className="overflow-x-auto whitespace-pre-wrap break-all p-4 font-mono text-[13px] leading-relaxed sm:break-normal">
        {code}
      </pre>
    </div>
  );
}

/** Copies a value, and says so for a moment. */
export function useCopy(value: string): [boolean, () => void] {
  const [copied, setCopied] = useState(false);

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(value);
    } catch {
      return;
    }

    setCopied(true);
    window.setTimeout(() => setCopied(false), 1600);
  };

  return [copied, () => void copy()];
}
