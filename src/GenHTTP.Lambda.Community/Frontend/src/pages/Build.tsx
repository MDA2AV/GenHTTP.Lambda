import { Link } from '@genhttp-lambda/kit/i18n/links';
import { usePublicPage } from '@genhttp-lambda/kit/meta';
import { Workspace } from '@genhttp-lambda/kit/workspace/Workspace';

import { useT } from '../i18n';

/**
 * The workspace of an installation somebody runs for themselves (see the
 * kit's Workspace): its build agent makes an app from a sentence and answers
 * about it afterwards. Where it has none, the box says so and leads to /ship.
 */
export function Build() {
  usePublicPage('/build');

  const said = useT().build;

  const intro = (
    <>
      <h1 className="text-balance text-center text-3xl font-light tracking-tight sm:text-4xl">{said.title}</h1>
      <p className="mx-auto mt-4 max-w-lg text-center text-slate-500">{said.intro}</p>
    </>
  );

  const link = (to: string) => (text: string) => (
    <Link to={to} className="text-accent-600 hover:underline dark:text-accent-400">
      {text}
    </Link>
  );

  const after = <p className="mt-10 text-center text-sm text-slate-500">{said.other(link('/ship'), link('/editor/create'))}</p>;

  return <Workspace intro={intro} after={after} />;
}
