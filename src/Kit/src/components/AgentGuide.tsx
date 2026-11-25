import type { ReactNode } from 'react';

import { useT } from '../i18n';
import { Link } from '../i18n/links';
import { ConnectAgent, useCopy } from './ConnectAgent';
import { IconCheck, IconCopy } from './Icons';

/**
 * /ship: how to connect an agent of one's own and have it publish an app here
 * - a guide in three steps and nothing else, the same on every installation,
 * with the words its site gives it.
 *
 * The first step is the one place the address of the MCP endpoint is set out
 * for every agent people have (ConnectAgent); the second says what to say to
 * the agent, each line copied with a press; the third what comes back. What
 * the site adds - why it is worth it, the questions people ask - follows.
 */
export interface GuideWords {
  eyebrow: string;
  title: string;
  intro: string;
  /** Short facts under the introduction, each with a tick. */
  facts?: string[];
  stepsTitle: string;
  step: (n: number) => string;
  steps: { title: string; body: string }[];
  /** Above what to say to the agent, in the second step. */
  sayLike: string;
  asks: string[];
  /** The way to the workspace, for whoever has no agent of their own. */
  noAgent: string;
}

export function AgentGuide({ words, origin, children }: { words: GuideWords; origin: string; children?: ReactNode }) {
  return (
    <div className="relative w-full overflow-x-clip">
      <div className="aurora-field" aria-hidden="true">
        <div className="aurora aurora-a" />
        <div className="aurora aurora-b" />
        <div className="aurora-clearing" />
      </div>

      <section className="relative z-10 px-5 pb-12 pt-14 text-center sm:px-6 sm:pt-20">
        <h1 className="rise mx-auto max-w-4xl text-[2.4rem] font-bold leading-[1.05] tracking-tight sm:text-6xl">
          <span className="mb-5 block text-balance font-sans text-xs font-medium uppercase leading-normal tracking-[0.2em] text-accent-600 dark:text-accent-400">
            {words.eyebrow}
          </span>
          {words.title}
        </h1>

        <p className="rise mx-auto mt-6 max-w-2xl text-base leading-relaxed text-grey-800 sm:text-lg dark:text-grey-200" style={{ animationDelay: '80ms' }}>
          {words.intro}
        </p>

        {words.facts && (
          <ul className="rise mt-6 flex flex-wrap justify-center gap-x-5 gap-y-2 text-sm font-medium text-grey-800 dark:text-grey-200" style={{ animationDelay: '140ms' }}>
            {words.facts.map((fact) => (
              <li key={fact} className="flex items-center gap-1.5">
                <IconCheck className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
                {fact}
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="relative z-10 mx-auto w-full max-w-3xl px-5 pb-16 sm:px-6" aria-labelledby="steps">
        <h2 id="steps" className="text-2xl font-bold tracking-tight sm:text-3xl">
          {words.stepsTitle}
        </h2>

        <ol className="mt-8 space-y-12">
          {words.steps.map((step, index) => (
            <li key={step.title} className="grid gap-x-5 sm:grid-cols-[3rem_1fr]">
              <span
                aria-hidden="true"
                className="flex h-10 w-10 items-center justify-center rounded-full bg-accent-500/10 text-lg font-semibold tabular-nums text-accent-700 dark:bg-accent-400/10 dark:text-accent-400"
              >
                {index + 1}
              </span>

              <div className="mt-3 min-w-0 sm:mt-1.5">
                <p className="sr-only">{words.step(index + 1)}</p>
                <h3 className="text-lg font-semibold">{step.title}</h3>
                <p className="mt-2 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{step.body}</p>

                {index === 0 && (
                  <div className="mt-5">
                    <ConnectAgent origin={origin} />
                  </div>
                )}

                {index === 1 && (
                  <div className="mt-5">
                    <p className="text-sm font-medium text-grey-800 dark:text-grey-200">{words.sayLike}</p>
                    <div className="mt-3 grid gap-2.5">
                      {words.asks.map((ask) => (
                        <SayThis key={ask} text={ask} />
                      ))}
                    </div>
                  </div>
                )}
              </div>
            </li>
          ))}
        </ol>
      </section>

      <div className="relative z-10 mx-auto w-full max-w-3xl px-5 pb-20 sm:px-6">
        {children}

        <p className="mt-16 text-center text-sm text-grey-700 dark:text-grey-300">
          <Link to="/build" className="btn-ghost">
            {words.noAgent}
          </Link>
        </p>
      </div>
    </div>
  );
}

/** Something to say to the agent, which copies itself when pressed. */
function SayThis({ text }: { text: string }) {
  const [copied, copy] = useCopy(text);
  const t = useT();

  return (
    <button
      type="button"
      onClick={copy}
      className="group flex w-full items-center gap-3 bg-accent-fill px-4 py-3 text-left text-[15px] text-accent-ink shadow-sm transition-colors hover:bg-accent-hover"
    >
      <span className="flex-1">{text}</span>
      <span className="flex shrink-0 items-center gap-1 text-xs font-medium opacity-80 group-hover:opacity-100">
        {copied ? <IconCheck className="h-4 w-4" /> : <IconCopy />}
        <span className="hidden sm:inline">{copied ? t.common.copied : t.common.copy}</span>
      </span>
    </button>
  );
}
