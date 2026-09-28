import { useEffect, useRef } from 'react';
import { useLocation } from 'react-router-dom';

import { CopyField } from '../components/CopyField';
import { IconChat, IconChevronDown, IconMail } from '../components/Icons';
import { Reveal } from '../components/Reveal';
import { CONTACT_MAIL, DISCORD } from '../contact';
import { useT } from '../i18n';
import { Link } from '../i18n/links';
import { usePublicPage } from '../meta';
import { useOrigin } from '../site';

/**
 * The front page, written for somebody who has an agent and an idea rather
 * than a compiler. The C# is still underneath and the editor still shows it,
 * but nobody has to read it to get something online - so the page talks about
 * the thing they want to exist, the link they get, and the fact that they can
 * keep coming back to change it.
 */

/** The pictures of the three steps, in the order the words are in. */
const STEP_IMAGES = ['/media/prompt.webp', '/media/app.webp', '/media/editor.webp'];

/** What each agent is set up with, beside its words; only Claude Code has a command. */
const AGENT_COMMANDS: (((origin: string) => string) | undefined)[] = [
  undefined,
  (origin) => `claude mcp add --transport http genhttp ${origin}/mcp`,
  undefined,
];

export function Landing() {
  usePublicPage('/');

  const t = useT();
  const said = t.landing;

  const location = useLocation();
  const rest = useRef<HTMLDivElement>(null);
  const agents = useRef<HTMLDivElement>(null);
  const { origin } = useOrigin();

  /*
   * Snapping belongs to whatever actually scrolls, and that is the document -
   * a class on a div inside it does nothing. It is set here and taken away
   * again, because the editor and the panels want to be scrolled normally.
   */
  useEffect(() => {
    document.documentElement.classList.add('snap-page');

    return () => document.documentElement.classList.remove('snap-page');
  }, []);

  /*
   * A link from the header carries a hash, which the router does not act on by
   * itself. Waited a frame because the sections are revealed as they arrive and
   * scrolling to one that has not been laid out yet lands short of it.
   */
  useEffect(() => {
    if (location.hash !== '#agents') {
      return;
    }

    const timer = window.setTimeout(() => agents.current?.scrollIntoView({ block: 'start' }), 80);

    return () => window.clearTimeout(timer);
  }, [location]);

  return (
    <div className="relative w-full">
      {/* as tall as the page, so it never ends, and travelling with it */}
      <div className="aurora-field" aria-hidden="true">
        <div className="aurora aurora-a" />
        <div className="aurora aurora-b" />
        <div className="aurora aurora-c" />
        <div className="aurora-clearing" />
      </div>

      {/*
        The first screen is one sentence and one button. Everything else is
        below it, because a visitor who is already convinced should not have to
        read past the example to find the way in. It reaches up behind the bar,
        so the colour is the page's and not a panel's.
      */}
      <section className="snap-stop relative z-10 -mt-[3.75rem] flex min-h-screen flex-col justify-center px-5 pb-24 pt-[3.75rem]">
        <div className="relative mx-auto w-full max-w-4xl text-center">
          <p className="rise text-xs font-medium uppercase tracking-[0.2em] text-accent-600 dark:text-accent-400">
            {said.eyebrow}
          </p>

          <h1
            className="rise mt-5 text-4xl font-bold leading-[1.05] tracking-tight sm:text-6xl lg:text-7xl"
            style={{ animationDelay: '60ms' }}
          >
            {said.headline}
            {/* a line of its own rather than after a break, so each sentence
                is balanced on its own lines (see index.css) */}
            <span className="block text-accent-700 dark:text-accent-400">{said.headlineAccent}</span>
          </h1>

          <p
            className="rise mx-auto mt-5 max-w-2xl text-base leading-relaxed text-grey-800 sm:mt-7 sm:text-lg dark:text-grey-200"
            style={{ animationDelay: '120ms' }}
          >
            {said.intro}
          </p>

          <div
            className="rise mt-8 flex flex-wrap items-center justify-center gap-x-6 gap-y-3 sm:mt-10"
            style={{ animationDelay: '200ms' }}
          >
            <Link to="/build" className="btn-primary px-7 py-3.5 text-base">
              {said.build}
            </Link>

            <button
              type="button"
              onClick={() => agents.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })}
              className="btn-ghost px-5 py-3.5 text-base"
            >
              {said.ownAgent}
            </button>
          </div>

          <p
            className="rise mt-5 text-sm text-grey-700 dark:text-grey-300"
            style={{ animationDelay: '260ms' }}
          >
            {said.free}
          </p>
        </div>

        {/* the way down, for anyone who would rather see it first */}
        <button
          type="button"
          onClick={() => rest.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })}
          className="rise group absolute inset-x-0 bottom-8 mx-auto flex w-fit flex-col items-center gap-2 text-xs text-grey-700 hover:text-accent-500 dark:text-grey-300 dark:hover:text-accent-400"
          style={{ animationDelay: '320ms' }}
        >
          {said.seeIt}
          <IconChevronDown className="h-4 w-4 transition-transform group-hover:translate-y-0.5" />
        </button>
      </section>

      {/* ------------------------------------------------------------ the video */}

      <div ref={rest} className="snap-stop relative z-10 mx-auto w-full max-w-5xl px-5 pb-24 pt-10">
        <Reveal>
          <h2 className="text-center text-2xl font-bold tracking-tight sm:text-3xl">
            {said.videoTitle}
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            {said.videoText}
          </p>
        </Reveal>

        <Reveal delay={120} className="mt-8">
          <figure className="surface overflow-hidden rounded-xl shadow-lg">
            <video
              className="block aspect-[16/10] w-full bg-ink-950"
              src="/media/build.mp4"
              poster="/media/build-poster.webp"
              autoPlay
              muted
              loop
              playsInline
              controls
              preload="metadata"
            />
          </figure>
          <p className="mt-3 text-center text-xs text-grey-500">
            {said.videoNote}
          </p>
        </Reveal>

        <Reveal delay={200}>
          <div className="mt-8 flex justify-center">
            <Link to="/build" className="btn-primary px-6 py-3">
              {said.tryIt}
            </Link>
          </div>
        </Reveal>
      </div>

      {/* ------------------------------------------------------- not a one-shot */}

      <div className="snap-stop relative z-10 mx-auto w-full max-w-6xl px-5 pb-24 pt-10">
        <Reveal>
          <h2 className="text-center text-2xl font-bold tracking-tight sm:text-3xl">
            {said.oneShotTitle}
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            {said.oneShotText}
          </p>
        </Reveal>

        <div className="mt-10 grid gap-6 lg:grid-cols-3">
          {said.steps.map((step, i) => (
            <Reveal key={step.title} delay={100 + i * 90}>
              <figure className="surface flex h-full flex-col overflow-hidden rounded-xl">
                <img
                  src={STEP_IMAGES[i]}
                  alt={step.alt}
                  loading="lazy"
                  className="aspect-[16/10] w-full border-b border-grey-300 object-cover object-top dark:border-ink-800"
                />
                <figcaption className="p-5">
                  <div className="flex items-baseline gap-3">
                    <span className="font-mono text-sm text-accent-500 dark:text-accent-400">{i + 1}</span>
                    <h3 className="font-semibold">{step.title}</h3>
                  </div>
                  <p className="mt-2 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{step.body}</p>
                </figcaption>
              </figure>
            </Reveal>
          ))}
        </div>

        {/* what the third step sounds like, since it is the part people do not expect */}
        <Reveal delay={200}>
          <div className="mx-auto mt-10 max-w-2xl space-y-3">
            <p className="text-center text-xs uppercase tracking-wide text-grey-500">{said.weekLater}</p>
            <div className="ml-auto w-fit max-w-[90%] rounded-2xl rounded-br-sm bg-accent-500 px-4 py-2.5 text-sm text-white">
              {said.weekAsk}
            </div>
            <div className="surface w-fit max-w-[90%] rounded-2xl rounded-bl-sm px-4 py-2.5 text-sm text-grey-800 dark:text-grey-200">
              {said.weekAnswer}
            </div>
          </div>
        </Reveal>
      </div>

      {/* ------------------------------------------------------- your own agent */}

      <div
        id="agents"
        ref={agents}
        className="snap-stop relative z-10 mx-auto w-full max-w-6xl px-5 pb-24 pt-10"
      >
        <Reveal>
          <h2 className="text-center text-2xl font-bold tracking-tight sm:text-3xl">
            {said.agentsTitle}
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            {said.agentsText}
          </p>
        </Reveal>

        <Reveal delay={120}>
          <div className="mx-auto mt-8 max-w-xl">
            <CopyField value={`${origin}/mcp`} tone="accent" />
          </div>
        </Reveal>

        <div className="mt-8 grid gap-4 md:grid-cols-3">
          {said.agents.map((agent, i) => (
            <Reveal key={agent.name} delay={160 + i * 80}>
              <div className="surface h-full rounded-xl p-5">
                <h3 className="font-semibold">{agent.name}</h3>
                <p className="mt-2 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{agent.how}</p>
                {AGENT_COMMANDS[i] && (
                  <pre className="mt-3 whitespace-pre-wrap break-all rounded-md bg-ink-950 p-3 text-xs text-grey-200">
                    {AGENT_COMMANDS[i](origin)}
                  </pre>
                )}
              </div>
            </Reveal>
          ))}
        </div>

        <Reveal delay={300}>
          <p className="mt-6 text-center text-sm text-grey-700 dark:text-grey-300">
            {said.thenAsk((text) => <em>{text}</em>)}
          </p>
        </Reveal>
      </div>

      {/* --------------------------------------------------------------- contact */}

      <div className="snap-stop relative z-10 mx-auto w-full max-w-4xl px-5 pb-16 pt-10">
        <Reveal>
          <h2 className="text-center text-2xl font-bold tracking-tight sm:text-3xl">{said.contactTitle}</h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            {said.contactText}
          </p>
        </Reveal>

        <div className="mt-8 grid gap-4 sm:grid-cols-2">
          <Reveal delay={100}>
            <a
              href={`mailto:${CONTACT_MAIL}`}
              className="surface group flex h-full items-start gap-4 rounded-xl p-5 transition hover:border-accent-500/60"
            >
              <IconMail className="mt-0.5 h-6 w-6 shrink-0 text-accent-500 dark:text-accent-400" />
              <span>
                <span className="block font-semibold">{said.mailTitle}</span>
                <span className="mt-1 block text-sm text-grey-700 dark:text-grey-300">{said.mailText}</span>
                <span className="mt-2 block text-sm text-accent-500 group-hover:underline dark:text-accent-400">
                  {CONTACT_MAIL}
                </span>
              </span>
            </a>
          </Reveal>

          <Reveal delay={180}>
            <a
              href={DISCORD}
              target="_blank"
              rel="noreferrer"
              className="surface group flex h-full items-start gap-4 rounded-xl p-5 transition hover:border-accent-500/60"
            >
              <IconChat className="mt-0.5 h-6 w-6 shrink-0 text-accent-500 dark:text-accent-400" />
              <span>
                <span className="block font-semibold">{said.discordTitle}</span>
                <span className="mt-1 block text-sm text-grey-700 dark:text-grey-300">{said.discordText}</span>
                <span className="mt-2 block text-sm text-accent-500 group-hover:underline dark:text-accent-400">
                  {said.discordLink}
                </span>
              </span>
            </a>
          </Reveal>
        </div>
      </div>
    </div>
  );
}
