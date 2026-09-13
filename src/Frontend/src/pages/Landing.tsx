import { Link } from 'react-router-dom';

import { CSharp } from '../components/CSharp';

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
  return (
    <div className="mx-auto w-full max-w-5xl px-5 pb-20 pt-12 sm:pt-20">
      <section className="grid items-center gap-10 lg:grid-cols-[1.05fr_1fr]">
        <div>
          <h1 className="rise text-4xl font-bold leading-[1.1] tracking-tight sm:text-5xl">
            Host a small web service
            <br />
            <span className="text-accent-500">without hosting anything.</span>
          </h1>

          <p
            className="rise mt-5 max-w-xl text-[15px] leading-relaxed text-slate-600 dark:text-slate-400"
            style={{ animationDelay: '80ms' }}
          >
            Write a bit of C# in your browser, press deploy, and get a public HTTPS address you can hand
            to anyone. A REST API, a webhook, a mock backend, a websocket. No account, no server, no
            pipeline.
          </p>

          <div className="rise mt-8 flex flex-wrap items-center gap-3" style={{ animationDelay: '160ms' }}>
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

          <p className="rise mt-4 text-xs text-slate-500" style={{ animationDelay: '220ms' }}>
            Runs on <a href="https://genhttp.org/" target="_blank" rel="noreferrer" className="underline">GenHTTP</a>.
            Knowing it is not a prerequisite - the editor suggests everything you can use.
          </p>
        </div>

        <div className="rise surface overflow-hidden shadow-xl" style={{ animationDelay: '280ms' }}>
          {/* a tab rather than a window: this is a file, not an application */}
          <div className="flex items-stretch border-b border-grey-300 bg-grey-50 dark:border-ink-800 dark:bg-ink-950">
            <span className="border-b-2 border-accent-500 bg-white px-4 py-2 font-mono text-xs text-grey-900 dark:border-accent-400 dark:bg-ink-900 dark:text-grey-200">
              lambda.cs
            </span>
            <span className="ml-auto self-center px-4 font-mono text-[11px] uppercase tracking-wide text-grey-500">
              C#
            </span>
          </div>

          <div className="flex overflow-x-auto font-mono text-[12.5px] leading-[1.7]">
            {/* the gutter is not selectable, so copying the example copies code */}
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
      </section>

    </div>
  );
}
