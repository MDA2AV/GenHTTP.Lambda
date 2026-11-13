import { useEffect, useRef, useState, type ReactNode } from 'react';

import { BuildBox } from '@genhttp-lambda/kit/components/BuildBox';
import { ConnectAgent } from '@genhttp-lambda/kit/components/ConnectAgent';
import {
  IconBook, IconBranch, IconChevronDown, IconCode, IconDatabase, IconDraft, IconEye, IconFolder, IconGlobe, IconKey, IconLayers, IconLock,
  IconSpark, IconStar,
} from '@genhttp-lambda/kit/components/Icons';
import { Link } from '@genhttp-lambda/kit/i18n/links';
import { usePublicPage } from '@genhttp-lambda/kit/meta';
import { askPlatform, useOrigin } from '@genhttp-lambda/kit/site';

import { useT } from '../i18n';

/** Where the source of the platform is, for whoever wants to read it. */
const REPOSITORY = 'https://github.com/MDA2AV/GenHTTP.Lambda';

/** What each thing every app gets is drawn with, in the order the words are in. */
const FEATURE_ICONS = [IconGlobe, IconDatabase, IconFolder, IconKey, IconDraft, IconBranch, IconEye, IconCode];

/**
 * The front page of an installation somebody runs for themselves.
 *
 * Nothing on it is selling: whoever opens it has the platform already, or is
 * deciding whether to run it. So it says what it does in a screen, offers the
 * two ways to build - the box, where there is an agent behind it, and one's
 * own agent over MCP - and then what every app gets, how it runs and where to
 * look next.
 */
export function Landing() {
  usePublicPage('/');

  const said = useT().landing;
  const { origin } = useOrigin();

  const build = useRef<HTMLDivElement>(null);
  const agents = useRef<HTMLDivElement>(null);

  // whether the box has an agent behind it; until the server says, it is
  // offered, so the page is rendered with it for whoever reads it unscripted
  const [available, setAvailable] = useState<boolean | null>(null);

  useEffect(() => {
    askPlatform()
      .then((platform) => setAvailable(platform.build.available))
      .catch(() => setAvailable(false));
  }, []);

  // anchors, so they lead somewhere without a script and for whoever reads
  // the page with a keyboard or aloud - and scroll smoothly where one runs
  const to = (target: React.RefObject<HTMLDivElement | null>) => (event: React.MouseEvent) => {
    if (target.current) {
      event.preventDefault();
      target.current.scrollIntoView({ behavior: 'smooth', block: 'start' });
      history.replaceState(null, '', `#${target.current.id}`);
    }
  };

  return (
    <div className="relative w-full">
      <div className="aurora-field" aria-hidden="true">
        <div className="aurora aurora-a" />
        <div className="aurora aurora-b" />
        <div className="aurora-clearing" />
      </div>

      <section className="relative z-10 px-5 pb-20 pt-16 sm:pt-24">
        <div className="mx-auto w-full max-w-4xl text-center">
          <p className="rise text-balance text-xs font-medium uppercase tracking-[0.2em] text-accent-600 dark:text-accent-400">
            {said.eyebrow}
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
              <a href="#build" onClick={to(build)} className="btn-primary px-6 py-3 text-base">
                {said.describe}
              </a>
            )}

            <a href="#agents" onClick={to(agents)} className={`${available !== false ? 'btn-ghost' : 'btn-primary'} px-5 py-3 text-base`}>
              {said.connect}
            </a>

            <Link to="/editor/create" className="btn-ghost px-5 py-3 text-base">
              <IconCode className="h-4 w-4" />
              {said.write}
            </Link>
          </div>

          <button
            type="button"
            onClick={to(available !== false ? build : agents)}
            className="rise group mx-auto mt-12 flex w-fit flex-col items-center gap-2 text-xs text-grey-700 hover:text-accent-500 dark:text-grey-300 dark:hover:text-accent-400"
            style={{ animationDelay: '260ms' }}
            aria-label={available !== false ? said.describe : said.connect}
          >
            <IconChevronDown className="h-4 w-4 transition-transform group-hover:translate-y-0.5" />
          </button>
        </div>
      </section>

      {/* ------------------------------------------------------- the box */}
      <section ref={build} id="build" className="relative z-10 scroll-mt-20 px-5 py-14">
        <div className="mx-auto w-full max-w-2xl">
          <Heading icon={<IconSpark className="h-5 w-5" />}>{said.buildTitle}</Heading>

          {available === false ? (
            <p className="mt-3 text-sm text-grey-700 dark:text-grey-300">{said.noAgent}</p>
          ) : (
            <>
              <p className="mt-3 max-w-xl text-sm text-grey-700 dark:text-grey-300">{said.buildText}</p>

              {/* the page says how to connect an agent itself, right below */}
              <BuildBox own={false} />
            </>
          )}
        </div>
      </section>

      {/* ------------------------------------------------------- an agent of one's own */}
      <section ref={agents} id="agents" className="relative z-10 scroll-mt-20 px-5 py-14">
        <div className="mx-auto w-full max-w-2xl">
          <Heading icon={<IconLayers className="h-5 w-5" />}>{said.connectTitle}</Heading>

          <p className="mb-5 mt-3 max-w-xl text-sm text-grey-700 dark:text-grey-300">{said.connectText}</p>

          <ConnectAgent origin={origin} />

          <p className="mt-5 text-sm text-grey-700 dark:text-grey-300">{said.connectThen}</p>
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
            <IconBranch className="h-4 w-4" />
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
            <Card to="/docs" icon={<IconBook className="h-5 w-5" />} {...said.around.guide} />
            <Card to="/api/v1/scalar/" icon={<IconLayers className="h-5 w-5" />} {...said.around.api} />
            <Card to="/source" icon={<IconBranch className="h-5 w-5" />} {...said.around.sources} />
            <Card to="/admin" icon={<IconLock className="h-5 w-5" />} {...said.around.admin} />
          </div>
        </div>
      </section>
    </div>
  );
}

function Heading({ children, icon }: { children: ReactNode; icon?: ReactNode }) {
  return (
    <h2 className="flex items-center gap-2.5 text-2xl font-light tracking-tight">
      {icon && <span className="text-accent-600 dark:text-accent-400">{icon}</span>}
      {children}
    </h2>
  );
}

function Card({ to, icon, title, text }: { to: string; icon: ReactNode; title: string; text: string }) {
  return (
    <Link to={to} className="surface group flex h-full flex-col p-4 transition hover:border-accent-500/50">
      <span className="flex items-center gap-2 text-accent-600 dark:text-accent-400">
        {icon}
        <span className="text-sm font-medium text-grey-900 group-hover:underline dark:text-grey-100">{title}</span>
      </span>
      <span className="mt-2 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{text}</span>
    </Link>
  );
}
