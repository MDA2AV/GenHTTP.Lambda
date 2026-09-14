import { useState } from 'react';

/**
 * What an agent does here, shown the same way a lambda is.
 *
 * The calls on the left and what comes back on the right are one exchange
 * twice, and pointing at either lights up its other half. It is the same shape
 * as the Explainer above it on purpose: somebody who has just learnt to read
 * that one should not have to learn a second way of reading this.
 */

interface Step {
  id: string;
  title: string;
  body: string;
}

const STEPS: Step[] = [
  {
    id: 'learn',
    title: 'It finds out what this is',
    body: 'The server introduces itself and says to read the guide first. platform_guide is what a snippet has to return, what is imported, what is refused, and the handful of things that catch people out - a bare string body binds to null, a websocket cannot read the request it was upgraded from. The examples are readable in full, as running code rather than documentation.',
  },
  {
    id: 'make',
    title: 'It asks for somewhere to put it',
    body: 'create_lambda hands back a public address and an editor key. Nothing happens until acceptTerms is true, and the answer says plainly that the key is the only way back in - to be given to the person the agent is acting for, not kept.',
  },
  {
    id: 'write',
    title: 'It writes, and finds out if it was wrong',
    body: 'Code goes in as files: lambda.cs is the snippet, the rest are ordinary C# holding types. check_code compiles without storing anything and answers with the compiler’s complaints, each carrying the file and the line - which is what an agent needs to fix one rather than guess.',
  },
  {
    id: 'ship',
    title: 'It puts it online',
    body: 'deploy answers with the address it is now being served at. Nothing is reachable before that, and deploying again extends how long it stays. From here it is an ordinary lambda: the same editor, the same public URL, the same day online.',
  },
];

/** The calls, in the order they happen. */
const CALLS: { id: string; method: string; detail: string }[] = [
  { id: 'learn', method: 'initialize', detail: 'who you are talking to' },
  { id: 'learn', method: 'platform_guide', detail: 'what compiles here' },
  { id: 'learn', method: 'read_example', detail: 'something that works' },
  { id: 'make', method: 'create_lambda', detail: '{ acceptTerms: true }' },
  { id: 'write', method: 'write_code', detail: '[ lambda.cs, Types.cs ]' },
  { id: 'write', method: 'check_code', detail: 'before spending a deploy' },
  { id: 'ship', method: 'deploy', detail: 'and it is reachable' },
];

/** What comes back, as the agent sees it. */
const BACK: { id: string; label: string; value: string }[] = [
  { id: 'learn', label: 'instructions', value: 'start with platform_guide' },
  { id: 'learn', label: 'imports', value: 'every GenHTTP module, no usings' },
  { id: 'make', label: 'publicUrl', value: '/lambda/your-key/' },
  { id: 'make', label: 'privateKey', value: 'the only way back in' },
  { id: 'write', label: 'diagnostics', value: 'Types.cs:3 — the name does not exist' },
  { id: 'ship', label: 'onlineUntil', value: 'a day, extended by deploying again' },
];

export function Agents() {
  const [active, setActive] = useState<string | null>(null);

  const step = STEPS.find((s) => s.id === active) ?? null;
  const lit = (id: string) => active === id;

  return (
    <div className="grid gap-6 lg:grid-cols-[1.15fr_1fr] lg:items-start">
      <div className="surface overflow-hidden">
        <div className="flex items-stretch border-b border-grey-300 bg-grey-50 dark:border-ink-800 dark:bg-ink-950">
          <span className="border-b-2 border-accent-500 bg-white px-4 py-2 font-mono text-xs text-grey-900 dark:border-accent-400 dark:bg-ink-900 dark:text-grey-200">
            /mcp
          </span>
          <span className="ml-auto self-center px-4 font-mono text-[11px] uppercase tracking-wide text-grey-500">
            JSON-RPC
          </span>
        </div>

        <div className="py-2 font-mono text-[12.5px] leading-[1.9]">
          {CALLS.map((call) => (
            <div
              key={call.method}
              onMouseEnter={() => setActive(call.id)}
              onMouseLeave={() => setActive(null)}
              onClick={() => setActive(call.id === active ? null : call.id)}
              className={`flex cursor-default items-baseline gap-3 px-4 transition-colors duration-200 ${
                lit(call.id) ? 'bg-accent-500/10 dark:bg-accent-400/10' : ''
              }`}
            >
              <span
                aria-hidden="true"
                className={`select-none transition-colors duration-200 ${
                  lit(call.id) ? 'text-accent-500 dark:text-accent-400' : 'text-grey-500'
                }`}
              >
                →
              </span>
              <span className="text-grey-900 dark:text-grey-200">{call.method}</span>
              <span className="truncate text-grey-500">{call.detail}</span>
            </div>
          ))}
        </div>
      </div>

      <div className="space-y-4">
        <div className="surface overflow-hidden">
          <div className="border-b border-grey-300 px-4 py-2 text-xs uppercase tracking-wide text-grey-500 dark:border-ink-800">
            what comes back
          </div>

          <div className="py-2">
            {BACK.map((item) => (
              <div
                key={item.label + item.value}
                onMouseEnter={() => setActive(item.id)}
                onMouseLeave={() => setActive(null)}
                onClick={() => setActive(item.id === active ? null : item.id)}
                className={`flex cursor-default items-baseline gap-3 px-4 py-0.5 transition-colors duration-200 ${
                  lit(item.id) ? 'bg-accent-500/10 dark:bg-accent-400/10' : ''
                }`}
              >
                <span
                  className={`font-mono text-[12px] transition-colors duration-200 ${
                    lit(item.id) ? 'text-accent-500 dark:text-accent-400' : 'text-grey-600 dark:text-grey-400'
                  }`}
                >
                  {item.label}
                </span>
                <span className="truncate text-[13px] text-grey-700 dark:text-grey-300">{item.value}</span>
              </div>
            ))}
          </div>
        </div>

        {/* one box that changes rather than four that appear, as above */}
        <div className="surface relative min-h-[9.5rem] p-4">
          <div className={`transition-opacity duration-200 ${step === null ? 'opacity-100' : 'opacity-0'}`}>
            <div className="text-sm font-medium">Point at any call</div>
            <p className="mt-1.5 text-sm leading-relaxed text-grey-700 dark:text-grey-300">
              An agent can do everything the editor does, over one endpoint. These are the calls it makes and
              what it gets back, in the order it makes them.
            </p>
          </div>

          {STEPS.map((candidate) => (
            <div
              key={candidate.id}
              aria-hidden={candidate.id !== active}
              className={`absolute inset-0 p-4 transition-opacity duration-200 ${
                candidate.id === active ? 'opacity-100' : 'pointer-events-none opacity-0'
              }`}
            >
              <div className="text-sm font-medium text-accent-500 dark:text-accent-400">{candidate.title}</div>
              <p className="mt-1.5 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{candidate.body}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
