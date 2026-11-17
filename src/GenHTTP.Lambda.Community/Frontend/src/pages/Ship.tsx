import { AgentGuide } from '@genhttp-lambda/kit/components/AgentGuide';
import { Questions } from '@genhttp-lambda/kit/components/Questions';
import { Link } from '@genhttp-lambda/kit/i18n/links';
import { usePublicPage } from '@genhttp-lambda/kit/meta';
import { useOrigin } from '@genhttp-lambda/kit/site';

import { useT } from '../i18n';

/**
 * How to connect an agent of one's own to an installation somebody runs for
 * themselves, and have it build and publish apps there: the guide every
 * installation has (the kit's AgentGuide), and what a developer asks next.
 */
export function Ship() {
  usePublicPage('/ship');

  const said = useT().ship;
  const { origin } = useOrigin();

  const code = (text: string) => (
    <Link to="/editor/create" className="text-accent-600 hover:underline dark:text-accent-400">
      {text}
    </Link>
  );

  return (
    <AgentGuide words={said} origin={origin}>
      <Questions title={said.questionsTitle} questions={said.questions(code)} />
    </AgentGuide>
  );
}
