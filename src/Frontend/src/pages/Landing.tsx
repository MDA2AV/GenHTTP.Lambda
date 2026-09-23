import { useEffect, useRef } from 'react';
import { Link, useLocation } from 'react-router-dom';

import { CopyField } from '../components/CopyField';
import { ReportAbuse } from '../components/ReportAbuse';
import { IconChat, IconChevronDown, IconMail } from '../components/Icons';
import { Reveal } from '../components/Reveal';

const CONTACT_MAIL = 'solutions@genhttp.dev';
const DISCORD = 'https://discord.gg/PRkwKrnrB4';

/**
 * The front page, written for somebody who has an agent and an idea rather
 * than a compiler. The C# is still underneath and the editor still shows it,
 * but nobody has to read it to get something online - so the page talks about
 * the thing they want to exist, the link they get, and the fact that they can
 * keep coming back to change it.
 */

/** How one app goes, from the sentence to the third change. */
const STEPS = [
  {
    title: 'Say what you want',
    body: 'Describe it in plain language, either to the agent on this site or to the one you already use. No code, no setup and no account required.',
    image: '/media/prompt.webp',
    alt: 'The build page with a request for a lunch poll typed in',
  },
  {
    title: 'Get a working app and a link',
    body: 'The app is built, deployed and returned as a public address you can share. It keeps its data - votes, scores, messages - so everyone who opens it sees the same state.',
    image: '/media/app.webp',
    alt: 'The finished lunch poll, open in a browser',
  },
  {
    title: 'Keep improving it',
    body: 'Every app comes with a private editor link. Hand it to your agent along with the next change, or open it yourself. Each change becomes a new version, and the address stays the same.',
    image: '/media/editor.webp',
    alt: 'The poll\'s control center: its versions, each with what was asked for, what it changed and the difference to the one before',
  },
];

/** Where to paste the address, for the agents people actually have. */
const AGENTS = [
  {
    name: 'Claude on the web or desktop',
    how: 'Open Settings, then Connectors, and choose Add custom connector. Paste the address above - no API key or sign-in required.',
  },
  {
    name: 'Claude Code',
    how: 'Run this once in a terminal:',
    command: (origin: string) => `claude mcp add --transport http genhttp ${origin}/mcp`,
  },
  {
    name: 'Other MCP clients',
    how: 'Cursor, VS Code, Codex and other MCP clients support remote servers. Configure them with the same address.',
  },
];

export function Landing() {
  const location = useLocation();
  const rest = useRef<HTMLDivElement>(null);
  const agents = useRef<HTMLDivElement>(null);
  const origin = window.location.origin;

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
            An agentic coding platform
          </p>

          <h1
            className="rise mt-5 text-4xl font-bold leading-[1.05] tracking-tight sm:text-6xl lg:text-7xl"
            style={{ animationDelay: '60ms' }}
          >
            Describe an app.
            <br />
            <span className="text-accent-700 dark:text-accent-400">Your agent puts it online.</span>
          </h1>

          <p
            className="rise mx-auto mt-5 max-w-2xl text-base leading-relaxed text-grey-800 sm:mt-7 sm:text-lg dark:text-grey-200"
            style={{ animationDelay: '120ms' }}
          >
            Polls, guestbooks, leaderboards, small shops. Describe what you need to our agent or to the one
            you already use, and receive a working app with a shareable link. Your app stays editable, so you
            can keep refining it long after the first version.
          </p>

          <div
            className="rise mt-8 flex flex-wrap items-center justify-center gap-x-6 gap-y-3 sm:mt-10"
            style={{ animationDelay: '200ms' }}
          >
            <Link to="/build" className="btn-primary px-7 py-3.5 text-base">
              Build something
            </Link>

            <button
              type="button"
              onClick={() => agents.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })}
              className="btn-ghost px-5 py-3.5 text-base"
            >
              Use your own agent
            </button>
          </div>

          <p
            className="rise mt-5 text-sm text-grey-700 dark:text-grey-300"
            style={{ animationDelay: '260ms' }}
          >
            Free to use. No account, nothing to install.
          </p>
        </div>

        {/* the way down, for anyone who would rather see it first */}
        <button
          type="button"
          onClick={() => rest.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })}
          className="rise group absolute inset-x-0 bottom-8 mx-auto flex w-fit flex-col items-center gap-2 text-xs text-grey-700 hover:text-accent-500 dark:text-grey-300 dark:hover:text-accent-400"
          style={{ animationDelay: '320ms' }}
        >
          See it in action
          <IconChevronDown className="h-4 w-4 transition-transform group-hover:translate-y-0.5" />
        </button>
      </section>

      {/* ------------------------------------------------------------ the video */}

      <div ref={rest} className="snap-stop relative z-10 mx-auto w-full max-w-5xl px-5 pb-24 pt-10">
        <Reveal>
          <h2 className="text-center text-2xl font-bold tracking-tight sm:text-3xl">
            From a sentence to a live app
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            A private browser window, no account, and a single request on the build page - followed by the
            finished app, opened from its link just as any visitor would.
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
            The build is shown sped up. Everything else is in real time.
          </p>
        </Reveal>

        <Reveal delay={200}>
          <div className="mt-8 flex justify-center">
            <Link to="/build" className="btn-primary px-6 py-3">
              Try it yourself
            </Link>
          </div>
        </Reveal>
      </div>

      {/* ------------------------------------------------------- not a one-shot */}

      <div className="snap-stop relative z-10 mx-auto w-full max-w-6xl px-5 pb-24 pt-10">
        <Reveal>
          <h2 className="text-center text-2xl font-bold tracking-tight sm:text-3xl">
            Not a one-shot
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            Most generators produce a result and leave you with it. Here the app keeps running where it
            was built, so you and your agent can continue working on it.
          </p>
        </Reveal>

        <div className="mt-10 grid gap-6 lg:grid-cols-3">
          {STEPS.map((step, i) => (
            <Reveal key={step.title} delay={100 + i * 90}>
              <figure className="surface flex h-full flex-col overflow-hidden rounded-xl">
                <img
                  src={step.image}
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
            <p className="text-center text-xs uppercase tracking-wide text-grey-500">A week later</p>
            <div className="ml-auto w-fit max-w-[90%] rounded-2xl rounded-br-sm bg-accent-500 px-4 py-2.5 text-sm text-white">
              Here is the editor link for my lunch poll. Please close voting at 11 on Fridays and show
              the winner at the top.
            </div>
            <div className="surface w-fit max-w-[90%] rounded-2xl rounded-bl-sm px-4 py-2.5 text-sm text-grey-800 dark:text-grey-200">
              Done. Version 4 is live at the same address, and version 3 is still available if you
              want to roll back.
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
            Bring your favourite agent
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            Already working with Claude or another assistant? Connect it to this address and it can
            build, deploy and update apps here - directly from the conversation you already have open.
          </p>
        </Reveal>

        <Reveal delay={120}>
          <div className="mx-auto mt-8 max-w-xl">
            <CopyField value={`${origin}/mcp`} tone="accent" />
          </div>
        </Reveal>

        <div className="mt-8 grid gap-4 md:grid-cols-3">
          {AGENTS.map((agent, i) => (
            <Reveal key={agent.name} delay={160 + i * 80}>
              <div className="surface h-full rounded-xl p-5">
                <h3 className="font-semibold">{agent.name}</h3>
                <p className="mt-2 text-sm leading-relaxed text-grey-700 dark:text-grey-300">{agent.how}</p>
                {agent.command && (
                  <pre className="mt-3 whitespace-pre-wrap break-all rounded-md bg-ink-950 p-3 text-xs text-grey-200">
                    {agent.command(origin)}
                  </pre>
                )}
              </div>
            </Reveal>
          ))}
        </div>

        <Reveal delay={300}>
          <p className="mt-6 text-center text-sm text-grey-700 dark:text-grey-300">
            Then simply ask: <em>build a sign-up sheet for our team event and put it online</em>.{' '}
            <Link to="/agentic-coding" className="text-accent-500 hover:underline dark:text-accent-400">
              Learn more about using your own agent
            </Link>
          </p>
        </Reveal>
      </div>

      {/* --------------------------------------------------------------- contact */}

      <div className="snap-stop relative z-10 mx-auto w-full max-w-4xl px-5 pb-16 pt-10">
        <Reveal>
          <h2 className="text-center text-2xl font-bold tracking-tight sm:text-3xl">Talk to us</h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            Need help, planning something larger, or looking for a solution built for you? We would be
            glad to hear from you.
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
                <span className="block font-semibold">Email us</span>
                <span className="mt-1 block text-sm text-grey-700 dark:text-grey-300">
                  For projects, enquiries and anything you would prefer to discuss privately.
                </span>
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
                <span className="block font-semibold">Join the Discord</span>
                <span className="mt-1 block text-sm text-grey-700 dark:text-grey-300">
                  Share what you have built, get help with the next step, and talk directly with the team.
                </span>
                <span className="mt-2 block text-sm text-accent-500 group-hover:underline dark:text-accent-400">
                  The GenHTTP Discord
                </span>
              </span>
            </a>
          </Reveal>
        </div>

        {/*
          Small, at the bottom, and on the front page rather than behind a
          lambda: whoever needs it is here because something hosted here did
          something to them, and they have no reason to know the rest of this.
        */}
        <footer className="mt-16 flex flex-wrap items-center justify-center gap-x-5 gap-y-2 border-t border-grey-300 pt-6 text-xs text-grey-700 dark:border-ink-800 dark:text-grey-300">
          <ReportAbuse />

          <Link to="/terms" className="text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400">
            Terms of service
          </Link>

          <Link to="/editor/create" className="text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400">
            Write the code yourself
          </Link>

          <a
            href={`mailto:${CONTACT_MAIL}`}
            className="text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400"
          >
            Contact
          </a>

          <a
            href={DISCORD}
            target="_blank"
            rel="noreferrer"
            className="text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400"
          >
            Discord
          </a>

          <a
            href="https://github.com/MDA2AV/GenHTTP.Lambda"
            target="_blank"
            rel="noreferrer"
            className="text-slate-500 hover:text-accent-600 hover:underline dark:hover:text-accent-400"
          >
            Source
          </a>
        </footer>
      </div>
    </div>
  );
}
