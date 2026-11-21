import { Questions } from '@genhttp-lambda/kit/components/Questions';
import { usePublicPage } from '@genhttp-lambda/kit/meta';
import { useLifetimes } from '@genhttp-lambda/kit/site';
import { Workspace } from '@genhttp-lambda/kit/workspace/Workspace';

import { useT } from '../i18n';

/**
 * The page that creates a website, and the workspace of whoever did (see the
 * kit's Workspace): before anything is asked, one text box, and around it what
 * somebody who has never heard of this would want to know first - in the
 * words they would search for it with. After, the apps they had built and a
 * conversation with the agent about each.
 */
export function Build() {
  usePublicPage('/build');

  const said = useT().build;
  const { offlineDays, retentionDays } = useLifetimes();

  const intro = (
    <>
      <h1 className="text-balance text-center text-4xl font-light tracking-tight sm:text-5xl">{said.title}</h1>
      <p className="mx-auto mt-4 max-w-lg text-center text-slate-500">{said.intro}</p>
    </>
  );

  const after = (
    <>
      <ul className="mt-14 grid gap-4 sm:grid-cols-3">
        {said.points.map((point) => (
          <li key={point.title} className="surface p-4">
            <h2 className="text-sm font-medium">{point.title}</h2>
            <p className="mt-1.5 text-sm leading-relaxed text-slate-500">{point.text}</p>
          </li>
        ))}
      </ul>

      <Questions title={said.questionsTitle} questions={said.questions(offlineDays, retentionDays)} />
    </>
  );

  return <Workspace intro={intro} after={after} />;
}
