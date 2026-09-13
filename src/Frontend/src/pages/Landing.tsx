import { Link } from 'react-router-dom';

import { IconSpark } from '../components/Icons';

const example = `var books = new List<Book> { new(1, "Dune") };

var api = Inline.Create()
                .Get(() => books)
                .Get(":id", (int id) => books.First(b => b.Id == id));

return Layout.Create()
             .Add("books", api)
             .AddOpenApi()
             .AddScalar();

record Book(int Id, string Title);`;

export function Landing() {
  return (
    <div className="mx-auto w-full max-w-5xl px-5 pb-20 pt-12 sm:pt-20">
      <section className="grid items-center gap-10 lg:grid-cols-[1.05fr_1fr]">
        <div>
          <span className="chip bg-accent-500/10 text-accent-500">
            <IconSpark className="h-3.5 w-3.5" />
            Free · No account · Live in seconds
          </span>

          <h1 className="mt-5 text-4xl font-bold leading-[1.1] tracking-tight sm:text-5xl">
            Host a small web service
            <br />
            <span className="text-accent-500">without hosting anything.</span>
          </h1>

          <p className="mt-5 max-w-xl text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
            Write a bit of C# in your browser, press deploy, and get a public HTTPS address you can hand
            to anyone. A REST API, a webhook, a mock backend, a websocket. No account, no server, no
            pipeline.
          </p>

          <div className="mt-8 flex flex-wrap items-center gap-3">
            <Link to="/editor/create" className="btn-primary px-5 py-2.5 text-[15px]">
              Put something online
            </Link>
            <a
              href="https://genhttp.org/documentation/content/"
              target="_blank"
              rel="noreferrer"
              className="btn-ghost px-5 py-2.5 text-[15px]"
            >
              What you can build
            </a>
          </div>

          <p className="mt-4 text-xs text-slate-500">
            Runs on <a href="https://genhttp.org/" target="_blank" rel="noreferrer" className="underline">GenHTTP</a>.
            Knowing it is not a prerequisite - the editor suggests everything you can use.
          </p>
        </div>

        <div className="surface overflow-hidden shadow-xl">
          <div className="flex items-center gap-1.5 border-b border-slate-200 px-4 py-2.5 dark:border-ink-800">
            <span className="h-2.5 w-2.5 rounded-full bg-red-400/80" />
            <span className="h-2.5 w-2.5 rounded-full bg-amber-400/80" />
            <span className="h-2.5 w-2.5 rounded-full bg-emerald-400/80" />
            <span className="ml-2 font-mono text-xs text-slate-500">lambda.cs</span>
          </div>
          <pre className="overflow-x-auto px-4 py-4 font-mono text-[12.5px] leading-relaxed text-slate-700 dark:text-slate-300">
            {example}
          </pre>
          <p className="border-t border-slate-200 px-4 py-2.5 text-xs text-slate-500 dark:border-ink-800">
            The whole file. It answers on <code className="font-mono">/books/</code> and documents itself.
          </p>
        </div>
      </section>

      <p className="mt-14 text-sm text-slate-500">
        Free. What you publish stays up for a day and a lambda you have not opened is kept a month - both
        reset whenever you touch it.
      </p>
    </div>
  );
}
