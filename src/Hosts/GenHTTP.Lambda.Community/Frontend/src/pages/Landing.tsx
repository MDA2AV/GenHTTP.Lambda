import type { ReactNode } from 'react';

import { isOwn, useBrand } from '@genhttp-lambda/kit/brand';
import {
  IconBook, IconBranch, IconCode, IconDatabase, IconDraft, IconEye, IconFolder, IconGitHub, IconGlobe, IconKey, IconLayers,
  IconLock, IconStar,
} from '@genhttp-lambda/kit/components/Icons';
import { Link } from '@genhttp-lambda/kit/i18n/links';
import { REPOSITORY } from '@genhttp-lambda/kit/installation';
import { usePublicPage } from '@genhttp-lambda/kit/meta';
import { useProfile } from '@genhttp-lambda/kit/profile';

import { useT } from '../i18n';
import { EnterpriseLanding } from './EnterpriseLanding';
import { Aurora, Heading, useBuildOffered } from './parts';

/** What whoever runs an installation reads: its configuration, its operations, every route. */
const REFERENCE = `${REPOSITORY}/blob/main/docs/reference.md`;

/** What each thing every app gets is drawn with, in the order the words are in. */
const FEATURE_ICONS = [IconGlobe, IconDatabase, IconFolder, IconKey, IconDraft, IconBranch, IconEye, IconCode];

/**
 * The front page of an installation somebody runs for themselves - or, where
 * an organization runs it for its people, the page its colleagues are
 * addressed on (see EnterpriseLanding): they come to have their tools built,
 * not to run the platform. The menu is the same on both.
 */
export function Landing() {
  usePublicPage('/');

  return useProfile() === 'enterprise' ? <EnterpriseLanding /> : <OperatorLanding />;
}

/**
 * The front page for whoever runs the installation, or is deciding to.
 *
 * Nothing on it is selling: whoever opens it has the platform already, or is
 * deciding whether to run it. So it says what it does in a screen, leads to
 * the two ways to build - the workspace (/build), where there is an agent
 * behind it, and one's own agent over MCP (/ship) - and then what every app
 * gets, how it runs and where to look next.
 */
function OperatorLanding() {
  const said = useT().landing;
  const brand = useBrand();
  const available = useBuildOffered();

  return (
    <div className="relative w-full">
      <Aurora />

      <section className="relative z-10 px-5 pb-20 pt-16 sm:pt-24">
        <div className="mx-auto w-full max-w-4xl text-center">
          {/* what this installation is: the platform's edition, or the name its operator gave it */}
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
            {available !== false && (
              <Link to="/build" className="btn-primary px-6 py-3 text-base">
                {said.describe}
              </Link>
            )}

            <Link to="/ship" className={`${available !== false ? 'btn-ghost' : 'btn-primary'} px-5 py-3 text-base`}>
              {said.connect}
            </Link>

            <Link to="/editor/create" className="btn-ghost px-5 py-3 text-base">
              <IconCode className="h-4 w-4" />
              {said.write}
            </Link>
          </div>

        </div>
      </section>

      {/* ------------------------------------------------------- what every app gets */}
      <section className="relative z-10 px-5 py-14">
        <div className="mx-auto w-full max-w-4xl">
          <Heading>{said.featuresTitle}</Heading>

          <ul className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {said.features.map((feature, index) => {
              const Icon = FEATURE_ICONS[index] ?? IconStar;

              return (
                <li key={feature.title} className="surface h-full p-4">
                  <Icon className="h-5 w-5 text-accent-600 dark:text-accent-400" />
                  <h3 className="mt-3 text-sm font-medium">{feature.title}</h3>
                  <p className="mt-1.5 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{feature.text}</p>
                </li>
              );
            })}
          </ul>
        </div>
      </section>

      {/* ------------------------------------------------------- how it runs */}
      <section className="relative z-10 px-5 py-14">
        <div className="mx-auto w-full max-w-4xl">
          <Heading>{said.runsTitle}</Heading>

          <dl className="mt-6 grid gap-x-10 gap-y-6 sm:grid-cols-2">
            {said.runs.map((fact) => (
              <div key={fact.title} className="border-l-2 border-accent-500/50 pl-4">
                <dt className="text-sm font-medium">{fact.title}</dt>
                <dd className="mt-1 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{fact.text}</dd>
              </div>
            ))}
          </dl>

          <a href={REPOSITORY} target="_blank" rel="noreferrer" className="btn-ghost mt-8 inline-flex">
            <IconGitHub className="h-4 w-4" />
            {said.repository}
          </a>
        </div>
      </section>

      {/* ------------------------------------------------------- where to look next */}
      <section className="relative z-10 px-5 pb-20 pt-14">
        <div className="mx-auto w-full max-w-4xl">
          <Heading>{said.aroundTitle}</Heading>

          <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <Card to="/editor/create" icon={<IconCode className="h-5 w-5" />} {...said.around.demos} />
            <Card to="/showcase" icon={<IconStar className="h-5 w-5" />} {...said.around.showcase} />
            <Card to="/api/v1/scalar/" icon={<IconLayers className="h-5 w-5" />} {...said.around.api} />
            <Card to="/source" icon={<IconBranch className="h-5 w-5" />} {...said.around.sources} />
            <Card to="/admin" icon={<IconLock className="h-5 w-5" />} {...said.around.admin} />
            <Card href={REFERENCE} icon={<IconBook className="h-5 w-5" />} {...said.around.reference} />
          </div>
        </div>
      </section>
    </div>
  );
}

type CardProps = { icon: ReactNode; title: string; text: string } & ({ to: string; href?: never } | { href: string; to?: never });

/** A page of this installation to look at next, or - with href - one elsewhere. */
function Card(props: CardProps) {
  const { icon, title, text } = props;

  const inside = (
    <>
      <span className="flex items-center gap-2 text-accent-600 dark:text-accent-400">
        {icon}
        <span className="text-sm font-medium text-grey-900 group-hover:underline dark:text-grey-100">{title}</span>
      </span>
      <span className="mt-2 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{text}</span>
    </>
  );

  const look = 'surface group flex h-full flex-col p-4 transition hover:border-accent-500/50';

  return props.href !== undefined ? (
    <a href={props.href} target="_blank" rel="noreferrer" className={look}>
      {inside}
    </a>
  ) : (
    <Link to={props.to} className={look}>
      {inside}
    </Link>
  );
}
