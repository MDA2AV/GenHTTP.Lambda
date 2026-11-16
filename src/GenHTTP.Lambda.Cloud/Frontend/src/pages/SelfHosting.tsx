import type { ReactNode } from 'react';

import { useCopy } from '@genhttp-lambda/kit/components/ConnectAgent';
import { IconBranch, IconCheck, IconCopy, IconGitHub, IconGlobe, IconLayers, IconSend } from '@genhttp-lambda/kit/components/Icons';
import { Reveal } from '@genhttp-lambda/kit/components/Reveal';
import { Link } from '@genhttp-lambda/kit/i18n/links';
import { REPOSITORY } from '@genhttp-lambda/kit/installation';
import { usePublicPage } from '@genhttp-lambda/kit/meta';
import { useLifetimes } from '@genhttp-lambda/kit/site';

import { useT } from '../i18n';

/** The command the README starts the community edition with. */
const RUN = 'docker run -d -p 8080:8080 -v lambda-data:/data -e LAMBDA_ADMIN_TOKEN=secure-password genhttp/lambda';

const README = `${REPOSITORY}#running-it`;

const DOCKER_HUB = 'https://hub.docker.com/r/genhttp/lambda';

/**
 * genhttp.dev and the community edition side by side, so whoever reads the
 * site learns that the platform can be run elsewhere and what that changes.
 *
 * Neither is sold over the other - decided by the owner: the two are drawn
 * alike, in the same order, with the same weight of button, and every row of
 * the comparison says a fact of each. What suits somebody is theirs to tell.
 */
export function SelfHosting() {
  usePublicPage('/self-hosting');

  const t = useT();
  const said = t.selfHosting;

  const { offlineDays } = useLifetimes();
  const [copied, copy] = useCopy(RUN);

  const github = (text: string) => (
    <a href={REPOSITORY} target="_blank" rel="noreferrer" className="text-accent-600 hover:underline dark:text-accent-400">
      {text}
    </a>
  );

  return (
    <div className="relative w-full overflow-x-clip">
      <div className="aurora-field" aria-hidden="true">
        <div className="aurora aurora-a" />
        <div className="aurora aurora-b" />
        <div className="aurora-clearing" />
      </div>

      {/* ------------------------------------------------------------- the hero */}

      <section className="relative z-10 -mt-[3.75rem] px-5 pb-14 pt-[calc(3.75rem+3.5rem)] text-center sm:px-6 sm:pt-[calc(3.75rem+5rem)]">
        <p className="rise text-balance text-xs font-medium uppercase tracking-[0.2em] text-accent-600 dark:text-accent-400">{said.eyebrow}</p>

        <h1 className="rise mx-auto mt-5 max-w-4xl text-4xl font-bold leading-[1.05] tracking-tight sm:text-6xl" style={{ animationDelay: '60ms' }}>
          {said.title}
          <span className="block text-accent-700 dark:text-accent-400">{said.titleAccent}</span>
        </h1>

        <p
          className="rise mx-auto mt-6 max-w-2xl text-base leading-relaxed text-grey-800 sm:text-lg dark:text-grey-200"
          style={{ animationDelay: '120ms' }}
        >
          {said.intro}
        </p>
      </section>

      {/* ------------------------------------------------------------ the two */}

      <section className="relative z-10 mx-auto grid w-full max-w-5xl gap-6 px-5 sm:px-6 md:grid-cols-2">
        <Reveal>
          <Edition
            icon={<IconGlobe className="h-5 w-5" />}
            {...said.cloud}
            action={
              <Link to="/build" className="btn-primary px-6 py-3">
                {said.cloud.action}
              </Link>
            }
          />
        </Reveal>
        <Reveal delay={100}>
          <Edition
            icon={<IconLayers className="h-5 w-5" />}
            {...said.community}
            action={
              <a href={README} target="_blank" rel="noreferrer" className="btn-primary px-6 py-3">
                {said.community.action}
              </a>
            }
          />
        </Reveal>
      </section>

      {/* -------------------------------------------------------- the same in both */}

      <section className="relative z-10 mx-auto w-full max-w-5xl px-5 pt-24 sm:px-6" aria-labelledby="same">
        <Reveal>
          <h2 id="same" className="text-2xl font-bold tracking-tight sm:text-3xl">
            {said.sameTitle}
          </h2>
          <p className="mt-3 max-w-2xl text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.sameText}</p>
          <ul className="mt-6 grid gap-x-8 gap-y-3 sm:grid-cols-2 lg:grid-cols-3">
            {said.same.map((thing) => (
              <li key={thing} className="flex gap-2.5 text-sm text-grey-800 dark:text-grey-200">
                <IconCheck className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
                {thing}
              </li>
            ))}
          </ul>
        </Reveal>
      </section>

      {/* ------------------------------------------------------------ what differs */}

      <section className="relative z-10 mx-auto w-full max-w-5xl px-5 pt-24 sm:px-6" aria-labelledby="differ">
        <Reveal>
          <h2 id="differ" className="text-2xl font-bold tracking-tight sm:text-3xl">
            {said.differTitle}
          </h2>

          {/* a table where there is room for three columns, and a card per question where there is not */}
          <table className="mt-8 hidden w-full border-collapse text-left text-sm md:table">
            <thead>
              <tr className="border-b border-grey-300 dark:border-ink-800">
                <th scope="col" className="w-[26%] py-3 pr-6 font-medium text-grey-600 dark:text-grey-400">
                  <span className="sr-only">{said.aspect}</span>
                </th>
                {[said.cloud, said.community].map((edition) => (
                  <th key={edition.name} scope="col" className="w-[37%] py-3 pr-6 align-bottom">
                    <span className="block text-[11px] font-medium uppercase tracking-[0.15em] text-grey-600 dark:text-grey-400">{edition.kicker}</span>
                    <span className="mt-0.5 block text-base font-semibold">{edition.name}</span>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {said.rows(offlineDays).map(([aspect, cloud, community]) => (
                <tr key={aspect} className="border-b border-grey-200 align-top dark:border-ink-800">
                  <th scope="row" className="py-3.5 pr-6 font-medium text-grey-800 dark:text-grey-200">
                    {aspect}
                  </th>
                  <td className="py-3.5 pr-6 leading-relaxed text-grey-700 dark:text-grey-300">{cloud}</td>
                  <td className="py-3.5 pr-6 leading-relaxed text-grey-700 dark:text-grey-300">{community}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <dl className="mt-6 divide-y divide-grey-200 border-y border-grey-200 md:hidden dark:divide-ink-800 dark:border-ink-800">
            {said.rows(offlineDays).map(([aspect, cloud, community]) => (
              <div key={aspect} className="py-4">
                <dt className="text-sm font-medium">{aspect}</dt>
                <dd className="mt-2 grid grid-cols-2 gap-4 text-sm leading-relaxed text-grey-700 dark:text-grey-300">
                  <span>
                    <span className="block text-[11px] font-medium uppercase tracking-[0.12em] text-grey-600 dark:text-grey-400">{said.cloud.name}</span>
                    {cloud}
                  </span>
                  <span>
                    <span className="block text-[11px] font-medium uppercase tracking-[0.12em] text-grey-600 dark:text-grey-400">{said.community.name}</span>
                    {community}
                  </span>
                </dd>
              </div>
            ))}
          </dl>
        </Reveal>
      </section>

      {/* --------------------------------------------------------- moving between */}

      <section className="relative z-10 mx-auto w-full max-w-5xl px-5 pt-20 sm:px-6" aria-labelledby="move">
        <Reveal>
          <div className="surface flex flex-col gap-4 p-6 sm:flex-row sm:items-start sm:gap-5 sm:p-8">
            <IconBranch className="h-6 w-6 shrink-0 text-accent-600 dark:text-accent-400" />
            <div>
              <h2 id="move" className="text-lg font-semibold">
                {said.moveTitle}
              </h2>
              <p className="mt-2 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.move}</p>
            </div>
          </div>
        </Reveal>
      </section>

      {/* -------------------------------------------------------- which one suits */}

      <section className="relative z-10 mx-auto w-full max-w-5xl px-5 pt-24 sm:px-6" aria-labelledby="pick">
        <Reveal>
          <h2 id="pick" className="text-2xl font-bold tracking-tight sm:text-3xl">
            {said.pickTitle}
          </h2>
          <div className="mt-8 grid gap-10 md:grid-cols-2">
            {[said.pickCloud, said.pickCommunity].map((pick) => (
              <div key={pick.title} className="border-t-2 border-accent-500 pt-5 dark:border-accent-400">
                <h3 className="font-semibold">{pick.title}</h3>
                <ul className="mt-3 space-y-2.5">
                  {pick.points.map((point) => (
                    <li key={point} className="flex gap-2.5 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
                      <span aria-hidden="true" className="mt-2.5 h-1.5 w-1.5 shrink-0 rounded-full bg-grey-400 dark:bg-ink-600" />
                      {point}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </Reveal>
      </section>

      {/* ---------------------------------------------------------- getting started */}

      <section className="relative z-10 mx-auto w-full max-w-5xl px-5 pt-24 sm:px-6" aria-labelledby="start">
        <Reveal>
          <h2 id="start" className="text-2xl font-bold tracking-tight sm:text-3xl">
            {said.startTitle}
          </h2>

          <div className="mt-8 grid gap-10 md:grid-cols-2">
            <div>
              <h3 className="font-semibold">{said.startCloud.title}</h3>
              <p className="mt-2 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.startCloud.text}</p>
              <div className="mt-5 flex flex-wrap gap-3">
                <Link to="/build" className="btn-ghost">
                  <IconGlobe />
                  {said.cloud.action}
                </Link>
                <Link to="/ship" className="btn-ghost">
                  <IconSend />
                  {said.connect}
                </Link>
              </div>
            </div>

            <div>
              <h3 className="font-semibold">{said.startCommunity.title}</h3>
              <p className="mt-2 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.startCommunity.text}</p>

              <div className="mt-5 bg-ink-950 text-grey-200">
                <div className="flex items-center justify-between gap-3 border-b border-white/10 pl-4 pr-1">
                  <span className="truncate font-mono text-xs text-grey-500">{t.connect.terminal}</span>
                  <button
                    type="button"
                    onClick={copy}
                    className="flex h-10 shrink-0 items-center gap-1.5 px-3 text-xs font-medium text-grey-400 transition-colors hover:text-white"
                  >
                    {copied ? <IconCheck className="h-4 w-4 text-emerald-400" /> : <IconCopy />}
                    {copied ? t.common.copied : t.common.copy}
                  </button>
                </div>
                <pre className="overflow-x-auto whitespace-pre-wrap break-all p-4 font-mono text-[13px] leading-relaxed">{RUN}</pre>
              </div>

              <div className="mt-5 flex flex-wrap gap-3">
                <a href={README} target="_blank" rel="noreferrer" className="btn-ghost">
                  <IconGitHub />
                  {said.readme}
                </a>
                <a href={DOCKER_HUB} target="_blank" rel="noreferrer" className="btn-ghost">
                  <IconLayers />
                  {said.dockerHub}
                </a>
              </div>
            </div>
          </div>
        </Reveal>
      </section>

      {/* -------------------------------------------------------------- the code */}

      <section className="relative z-10 mx-auto w-full max-w-5xl px-5 pb-24 pt-24 sm:px-6" aria-labelledby="source">
        <Reveal>
          <div className="flex flex-col items-start gap-4 border-t border-grey-300 pt-8 sm:flex-row sm:items-center sm:justify-between dark:border-ink-800">
            <div>
              <h2 id="source" className="text-lg font-semibold">
                {said.sourceTitle}
              </h2>
              <p className="mt-1 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{said.source(github)}</p>
            </div>
            <a href={REPOSITORY} target="_blank" rel="noreferrer" className="btn-ghost shrink-0">
              <IconGitHub />
              MDA2AV/GenHTTP.Lambda
            </a>
          </div>
        </Reveal>
      </section>
    </div>
  );
}

interface EditionProps {
  icon: ReactNode;
  kicker: string;
  name: string;
  summary: string;
  points: string[];
  action: ReactNode;
}

/** One edition, drawn exactly like the other. */
function Edition({ icon, kicker, name, summary, points, action }: EditionProps) {
  return (
    <div className="surface flex h-full flex-col p-6 sm:p-8">
      <span className="flex items-center gap-2 text-accent-600 dark:text-accent-400">
        {icon}
        <span className="text-[11px] font-medium uppercase tracking-[0.15em]">{kicker}</span>
      </span>
      <h2 className="mt-3 text-2xl font-bold tracking-tight">{name}</h2>
      <p className="mt-3 text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">{summary}</p>
      <ul className="mt-5 flex-1 space-y-2.5">
        {points.map((point) => (
          <li key={point} className="flex gap-2.5 text-sm text-grey-800 dark:text-grey-200">
            <IconCheck className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600 dark:text-emerald-400" />
            {point}
          </li>
        ))}
      </ul>
      <div className="mt-7">{action}</div>
    </div>
  );
}
