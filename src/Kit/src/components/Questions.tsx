import type { ReactNode } from 'react';

/**
 * Questions people ask before they start, folded away under a heading: there
 * for whoever wants to know first and for a search engine, which reads them
 * in the page as it is sent, without pushing the page's own business down.
 */
export function Questions({ title, questions, id = 'questions' }: { title: string; questions: [string, ReactNode][]; id?: string }) {
  return (
    <section className="mt-16" aria-labelledby={id}>
      <h2 id={id} className="text-xl font-light tracking-tight">
        {title}
      </h2>

      <div className="mt-4 divide-y divide-slate-200 border-t border-slate-200 dark:divide-ink-800 dark:border-ink-800">
        {questions.map(([question, answer]) => (
          <details key={question} className="group">
            <summary className="flex min-h-[3rem] cursor-pointer list-none items-center justify-between gap-4 py-3 text-sm font-medium [&::-webkit-details-marker]:hidden">
              {question}
              <span aria-hidden="true" className="text-lg leading-none text-slate-400 transition-transform group-open:rotate-45">
                +
              </span>
            </summary>
            <div className="pb-4 pr-8 text-sm leading-relaxed text-slate-500">{answer}</div>
          </details>
        ))}
      </div>
    </section>
  );
}
