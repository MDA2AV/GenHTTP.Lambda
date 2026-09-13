import { useRef } from 'react';
import { Link } from 'react-router-dom';

import { CSharp } from '../components/CSharp';
import { IconChevronDown } from '../components/Icons';
import { Reveal } from '../components/Reveal';

const example = `var books = new List<Book> { new(1, "Dune") };

var api = Inline.Create()
                .Get(() => books)
                .Get(":id", (int id) => Find(id));

Book Find(int id) => books.First(b => b.Id == id);

return Layout.Create()
             .Add("books", api)
             .AddOpenApi()
             .AddScalar();

record Book(int Id, string Title);`;

export function Landing() {
  const rest = useRef<HTMLDivElement>(null);

  return (
    <div className="w-full">
      {/*
        The first screen is one sentence and one button. Everything else is
        below it, because a visitor who is already convinced should not have to
        read past the example to find the way in.
      */}
      {/*
        The first screen reaches up behind the bar, so the colour is the page's
        and not a panel's. Negative margin pulls it under the header the bar
        would otherwise have pushed it below.
      */}
      <section className="relative -mt-[3.75rem] flex min-h-screen flex-col justify-center overflow-hidden px-5 pb-24 pt-[3.75rem]">
        <div className="aurora aurora-a" aria-hidden="true" />
        <div className="aurora aurora-b" aria-hidden="true" />
        <div className="aurora aurora-c" aria-hidden="true" />
        <div className="aurora-grain" aria-hidden="true" />
        <div className="aurora-clearing" aria-hidden="true" />

        <div className="relative mx-auto w-full max-w-4xl text-center">
          <h1 className="rise text-5xl font-bold leading-[1.05] tracking-tight sm:text-6xl lg:text-7xl">
            Host a small web service
            <br />
            <span className="text-accent-500">without hosting anything.</span>
          </h1>

          <p
            className="rise mx-auto mt-7 max-w-xl text-lg leading-relaxed text-grey-700 dark:text-grey-200"
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

      <div ref={rest} className="mx-auto w-full max-w-5xl scroll-mt-20 px-5 pb-24">
        <Reveal>
          <div className="surface overflow-hidden shadow-xl">
            <div className="flex items-stretch border-b border-grey-300 bg-grey-50 dark:border-ink-800 dark:bg-ink-950">
              <span className="border-b-2 border-accent-500 bg-white px-4 py-2 font-mono text-xs text-grey-900 dark:border-accent-400 dark:bg-ink-900 dark:text-grey-200">
                lambda.cs
              </span>
              <span className="ml-auto self-center px-4 font-mono text-[11px] uppercase tracking-wide text-grey-500">
                C#
              </span>
            </div>

            <div className="flex overflow-x-auto font-mono text-[12.5px] leading-[1.7]">
              <div
                aria-hidden="true"
                className="shrink-0 select-none border-r border-grey-300 bg-grey-50 px-3 py-4 text-right text-grey-500 dark:border-ink-800 dark:bg-ink-950"
              >
                {example.split('\n').map((_, line) => (
                  <div key={line}>{line + 1}</div>
                ))}
              </div>

              <pre className="px-4 py-4 text-grey-900 dark:text-grey-300">
                <CSharp code={example} />
              </pre>
            </div>

            <p className="border-t border-grey-300 px-4 py-2.5 text-xs text-grey-500 dark:border-ink-800">
              The whole file. It answers on <code className="font-mono">/books/</code> and documents itself.
            </p>
          </div>
        </Reveal>

        <Reveal delay={120}>
          <div className="mt-10 flex flex-wrap items-center gap-x-6 gap-y-3">
            <Link to="/editor/create" className="btn-primary px-5 py-2.5">
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

            <span className="text-xs text-slate-500">
              Runs on{' '}
              <a href="https://genhttp.org/" target="_blank" rel="noreferrer" className="underline">
                GenHTTP
              </a>
              . Knowing it is not a prerequisite - the editor suggests everything you can use.
            </span>
          </div>
        </Reveal>
      </div>
    </div>
  );
}
