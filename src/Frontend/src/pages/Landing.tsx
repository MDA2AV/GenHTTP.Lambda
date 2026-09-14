import { useEffect, useRef } from 'react';
import { Link, useLocation } from 'react-router-dom';

import { Agents } from '../components/Agents';
import { CopyField } from '../components/CopyField';
import { Explainer } from '../components/Explainer';
import { ReportAbuse } from '../components/ReportAbuse';
import { IconChevronDown } from '../components/Icons';
import { Reveal } from '../components/Reveal';

export function Landing() {
  const location = useLocation();
  const rest = useRef<HTMLDivElement>(null);
  const agents = useRef<HTMLDivElement>(null);

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
          <h1 className="rise text-5xl font-bold leading-[1.05] tracking-tight sm:text-6xl lg:text-7xl">
            Host a small web service
            <br />
            <span className="text-accent-700 dark:text-accent-400">without hosting anything.</span>
          </h1>

          <p
            className="rise mx-auto mt-7 max-w-xl text-lg leading-relaxed text-grey-800 dark:text-grey-200"
            style={{ animationDelay: '90ms' }}
          >
            Write a bit of C# in your browser, press deploy, and get a public HTTPS address you can hand
            to anyone.
          </p>

          <div className="rise mt-10" style={{ animationDelay: '180ms' }}>
            <Link to="/editor/create" className="btn-primary px-7 py-3.5 text-base">
              Put something online
            </Link>
          </div>
        </div>

        {/* the way down, for anyone who would rather see it first */}
        <button
          type="button"
          onClick={() => rest.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })}
          className="rise group absolute inset-x-0 bottom-8 mx-auto flex w-fit flex-col items-center gap-2 text-xs text-grey-700 hover:text-accent-500 dark:text-grey-300 dark:hover:text-accent-400"
          style={{ animationDelay: '320ms' }}
        >
          See what that looks like
          <IconChevronDown className="h-4 w-4 transition-transform group-hover:translate-y-0.5" />
        </button>
      </section>

      <div ref={rest} className="snap-stop relative z-10 mx-auto w-full max-w-6xl px-5 pb-24 pt-10">
        <Reveal>
          <h2 className="text-center text-2xl font-bold tracking-tight sm:text-3xl">
            A lambda is one handler
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            Whatever you return is what the world gets. Here is one, and what it answers.
          </p>
        </Reveal>

        <Reveal delay={120} className="mt-8">
          <Explainer />
        </Reveal>

        <Reveal delay={200}>
          <div className="mt-10 flex flex-wrap items-center justify-center gap-x-6 gap-y-3">
            <Link to="/editor/create" className="btn-primary px-6 py-3">
              Put something online
            </Link>

            <a
              href="https://genhttp.org/documentation/content/"
              target="_blank"
              rel="noreferrer"
              className="text-sm text-accent-500 hover:underline dark:text-accent-400"
            >
              What you can build
            </a>
          </div>
        </Reveal>

      </div>

      <div
        id="agents"
        ref={agents}
        className="snap-stop relative z-10 mx-auto w-full max-w-6xl px-5 pb-24 pt-10"
      >
        <Reveal>
          <h2 className="text-center text-2xl font-bold tracking-tight sm:text-3xl">
            An agent can do this too
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-center text-[15px] leading-relaxed text-grey-700 dark:text-grey-300">
            There is a Model Context Protocol endpoint here, so something that is not a person can read what
            this platform is, write a lambda and put it online.
          </p>
        </Reveal>

        <Reveal delay={120} className="mt-8">
          <Agents />
        </Reveal>

        <Reveal delay={200}>
          <div className="mx-auto mt-10 max-w-xl">
            <p className="mb-2 text-center text-xs uppercase tracking-wide text-grey-500">
              Point your client at
            </p>
            <CopyField value={`${window.location.origin}/mcp`} tone="accent" />
            <p className="mt-3 text-center text-sm text-grey-700 dark:text-grey-300">
              No key and no account to reach it. The lambda it makes belongs to whoever it hands the editor
              link to.
            </p>
          </div>
        </Reveal>

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
