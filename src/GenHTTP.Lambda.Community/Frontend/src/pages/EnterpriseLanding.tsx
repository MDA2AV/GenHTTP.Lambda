import { isOwn, useBrand } from '@genhttp-lambda/kit/brand';
import { IconBook, IconHistory, IconList, IconPencil, IconStar } from '@genhttp-lambda/kit/components/Icons';
import { Questions } from '@genhttp-lambda/kit/components/Questions';
import { Link } from '@genhttp-lambda/kit/i18n/links';

import { useT } from '../i18n';
import { Aurora, Heading, useBuildOffered } from './parts';

/** What each kind of app is drawn with, in the order the words are in. */
const EXAMPLE_ICONS = [IconPencil, IconHistory, IconList, IconBook];

/**
 * The front page of an installation an organization runs for its people, for
 * the colleagues who come to have their tools built - most of whom do not
 * write code. It says what they can have built for their work and how that
 * goes, leads to the box on /build - or, for whoever works with an agent of
 * their own, to /ship - and answers what they ask first. Nothing about
 * running the platform, nor a link to its project: that is the operators'.
 */
export function EnterpriseLanding() {
  const said = useT().enterpriseLanding;
  const brand = useBrand();
  const available = useBuildOffered();

  const showcase = (text: string) => (
    <Link to="/showcase" className="text-accent-600 hover:underline dark:text-accent-400">
      {text}
    </Link>
  );

  const describe = (
    <Link to="/build" className="btn-primary px-6 py-3 text-base">
      {said.describe}
    </Link>
  );

  return (
    <div className="relative w-full">
      <Aurora />

      <section className="relative z-10 px-5 pb-16 pt-16 sm:pt-24">
        <div className="mx-auto w-full max-w-4xl text-center">
          {/* whose it is: the name its operator gave it, or what it is for */}
          <p className="rise text-balance text-xs font-medium uppercase tracking-[0.2em] text-accent-600 dark:text-accent-400">
            {isOwn(brand) ? brand.name : said.eyebrow}
          </p>

          <h1 className="rise mt-5 text-4xl font-bold leading-[1.05] tracking-tight sm:text-6xl" style={{ animationDelay: '60ms' }}>
            {said.headline}
            <span className="block text-accent-700 dark:text-accent-400">{said.headlineAccent}</span>
          </h1>

          <p
            className="rise mx-auto mt-6 max-w-2xl text-base leading-relaxed text-grey-800 sm:text-lg dark:text-grey-200"
            style={{ animationDelay: '120ms' }}
          >
            {said.intro}
          </p>

          <div className="rise mt-9 flex flex-wrap items-center justify-center gap-x-4 gap-y-3" style={{ animationDelay: '200ms' }}>
            {available !== false && describe}

            <Link to="/ship" className={`${available !== false ? 'btn-ghost' : 'btn-primary'} px-5 py-3 text-base`}>
              {said.own}
            </Link>
          </div>
        </div>
      </section>

      {/* ------------------------------------------------------- what people build here */}
      <section className="relative z-10 px-5 py-12">
        <div className="mx-auto w-full max-w-4xl">
          <Heading>{said.examplesTitle}</Heading>

          <ul className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {said.examples.map((example, index) => {
              const Icon = EXAMPLE_ICONS[index] ?? IconStar;

              return (
                <li key={example.title} className="surface h-full p-4">
                  <Icon className="h-5 w-5 text-accent-600 dark:text-accent-400" />
                  <h3 className="mt-3 text-sm font-medium">{example.title}</h3>
                  <p className="mt-1.5 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{example.text}</p>
                </li>
              );
            })}
          </ul>
        </div>
      </section>

      {/* ------------------------------------------------------- how it works */}
      <section className="relative z-10 px-5 py-12">
        <div className="mx-auto w-full max-w-4xl">
          <Heading>{said.stepsTitle}</Heading>

          <ol className="mt-6 grid gap-6 sm:grid-cols-3">
            {said.steps.map((step, index) => (
              <li key={step.title} className="flex gap-4 sm:block">
                <span
                  aria-hidden="true"
                  className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-accent-500/10 text-base font-semibold tabular-nums text-accent-700 dark:bg-accent-400/10 dark:text-accent-400"
                >
                  {index + 1}
                </span>

                <div className="min-w-0 sm:mt-3">
                  <h3 className="text-sm font-medium">{step.title}</h3>
                  <p className="mt-1.5 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{step.text}</p>
                </div>
              </li>
            ))}
          </ol>
        </div>
      </section>

      {/* ------------------------------------------------------- what people ask first */}
      <section className="relative z-10 px-5 pb-20">
        <div className="mx-auto w-full max-w-4xl">
          <Questions title={said.questionsTitle} questions={said.questions(showcase)} />

          {available !== false && <div className="mt-12 text-center">{describe}</div>}
        </div>
      </section>
    </div>
  );
}
