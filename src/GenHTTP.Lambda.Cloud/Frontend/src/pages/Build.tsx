import { BuildBox } from '@genhttp-lambda/kit/components/BuildBox';
import { usePublicPage } from '@genhttp-lambda/kit/meta';
import { useLifetimes } from '@genhttp-lambda/kit/site';

import { useT } from '../i18n';

/**
 * The page that creates a website: one text box (see the kit's BuildBox),
 * and around it what somebody who has never heard of this would want to know
 * first - in the words they would search for it with.
 */
export function Build() {
  usePublicPage('/build');

  const said = useT().build;
  const { offlineDays, retentionDays } = useLifetimes();

  const points = (
    <ul className="mt-12 grid gap-4 sm:grid-cols-3">
      {said.points.map((point) => (
        <li key={point.title} className="surface p-4">
          <h2 className="text-sm font-medium">{point.title}</h2>
          <p className="mt-1.5 text-sm leading-relaxed text-slate-500">{point.text}</p>
        </li>
      ))}
    </ul>
  );

  const questions = (
    <section className="mt-16" aria-labelledby="questions">
      <h2 id="questions" className="text-xl font-light tracking-tight">
        {said.questionsTitle}
      </h2>

      <div className="mt-4 divide-y divide-slate-200 border-t border-slate-200 dark:divide-slate-800 dark:border-slate-800">
        {said.questions(offlineDays, retentionDays).map(([question, answer]) => (
          <details key={question} className="group">
            <summary className="flex min-h-[3rem] cursor-pointer list-none items-center justify-between gap-4 py-3 text-sm font-medium [&::-webkit-details-marker]:hidden">
              {question}
              <span aria-hidden="true" className="text-lg leading-none text-slate-400 transition-transform group-open:rotate-45">+</span>
            </summary>
            <p className="pb-4 pr-8 text-sm leading-relaxed text-slate-500">{answer}</p>
          </details>
        ))}
      </div>
    </section>
  );

  return (
    <main className="mx-auto w-full max-w-2xl px-6 py-16 sm:py-24">
      <h1 className="text-center text-4xl font-light tracking-tight sm:text-5xl">{said.title}</h1>

      <p className="mx-auto mt-4 max-w-lg text-center text-slate-500">{said.intro}</p>

      <BuildBox points={points} questions={questions} />
    </main>
  );
}
