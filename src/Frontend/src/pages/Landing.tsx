import { Link } from 'react-router-dom';

import { IconHistory, IconKey, IconSpark } from '../components/Icons';

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
            Write a bit of C# in your browser and press deploy. You get a public HTTPS address you can
            hand to anyone - a REST API, a webhook receiver, a mock backend, a websocket, a page. No
            account to make, no server to rent, no pipeline to set up.
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
            Runs on <a href="https://genhttp.org/" target="_blank" rel="noreferrer" className="underline">GenHTTP</a>,
            an open source web server for .NET. Knowing it is not a prerequisite - the editor suggests
            everything you can use.
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
            That is the whole file. It answers on <code className="font-mono">/books/</code> and documents
            itself.
          </p>
        </div>
      </section>

      <section className="mt-20 grid gap-5 sm:grid-cols-3">
        <Feature icon={<IconKey className="h-5 w-5" />} title="Nothing to sign up for">
          You get two links: one to share, one to edit. Keep the editing link and you are the owner.
          There is no password and no email.
        </Feature>
        <Feature icon={<IconHistory className="h-5 w-5" />} title="Change it without breaking it">
          Saving and publishing are separate. Try something, and if it was worse, put an earlier
          version back online in one click.
        </Feature>
        <Feature icon={<IconSpark className="h-5 w-5" />} title="More than a hello world">
          JSON APIs, OpenAPI docs with a browser to try them, websockets, file uploads, static pages,
          server sent events - all available without installing anything.
        </Feature>
      </section>

      <section className="surface mt-12 px-6 py-5 text-sm text-slate-600 dark:text-slate-400">
        <strong className="font-semibold text-ink-900 dark:text-slate-200">Free, with two timers.</strong>{' '}
        Something you publish stays reachable for a day, and a lambda you have not opened for a month is
        cleaned up. Both reset the moment you touch it again, so anything you are actually using stays.
        It is shared infrastructure, so nothing that attacks, scans or floods other systems.
      </section>
    </div>
  );
}

function Feature({ icon, title, children }: { icon: React.ReactNode; title: string; children: React.ReactNode }) {
  return (
    <div className="surface px-5 py-5">
      <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-accent-500/10 text-accent-500">
        {icon}
      </div>
      <h2 className="mt-3.5 font-semibold">{title}</h2>
      <p className="mt-1.5 text-sm leading-relaxed text-slate-600 dark:text-slate-400">{children}</p>
    </div>
  );
}
