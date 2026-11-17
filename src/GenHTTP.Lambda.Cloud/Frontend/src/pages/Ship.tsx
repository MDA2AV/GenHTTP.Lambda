import type { ReactNode } from 'react';

import { AgentGuide } from '@genhttp-lambda/kit/components/AgentGuide';
import { Questions } from '@genhttp-lambda/kit/components/Questions';
import { Link } from '@genhttp-lambda/kit/i18n/links';
import { usePublicPage } from '@genhttp-lambda/kit/meta';
import { useLifetimes, useOrigin } from '@genhttp-lambda/kit/site';

import { useT } from '../i18n';

/**
 * For people who already built something with an agent and have it running
 * on their own machine: a guide in three steps to connecting that agent and
 * having it publish the app here (the kit's AgentGuide), then what they get
 * that a static host does not give them - a database, and everybody on the
 * same page live - and the questions they ask before they do it.
 *
 * It was a page that sold this, with a scene, a comparison and a domain of
 * its own; the owner had it cut down to the guide.
 */
export function Ship() {
  usePublicPage('/ship');

  const said = useT().ship;
  const { origin } = useOrigin();
  const { offlineDays: offline, retentionDays: removed } = useLifetimes();

  return (
    <AgentGuide words={said} origin={origin}>
      <section aria-labelledby="together" className="border-t border-grey-300 pt-10 dark:border-ink-800">
        <h2 id="together" className="text-xl font-bold tracking-tight sm:text-2xl">
          {said.togetherTitle}
        </h2>
        <p className="mt-3 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.together}</p>
      </section>

      <Questions
        title={said.questionsTitle}
        questions={said.questions(offline, removed, inline('/showcase'), inline('/terms'), inline('/self-hosting'))}
      />
    </AgentGuide>
  );
}

/** A link in the middle of a sentence, around whatever words the language has for it. */
function inline(to: string) {
  return (text: string): ReactNode => (
    <Link to={to} className="text-accent-600 hover:underline dark:text-accent-400">
      {text}
    </Link>
  );
}
