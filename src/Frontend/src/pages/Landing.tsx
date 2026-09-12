import { Link } from 'react-router-dom';

import { IconHistory, IconKey, IconSpark } from '../components/Icons';

const example = `var books = new List<Book> { new(1, "Dune") };

var api = Inline.Create()
                .Get(() => books)
                .Get(":id", (int id) => books.First(b => b.Id == id));

return Layout.Create()
             .Add("books", api)
             .AddOpenApi()
             .AddSwaggerUi();

record Book(int Id, string Title);`;

export function Landing() {
  return (
    <div className="mx-auto w-full max-w-5xl px-5 pb-20 pt-12 sm:pt-20">
      <section className="grid items-center gap-10 lg:grid-cols-[1.05fr_1fr]">
        <div>
          <span className="chip bg-accent-500/10 text-accent-500">
            <IconSpark className="h-3.5 w-3.5" />
            Powered by GenHTTP 11
          </span>

          <h1 className="mt-5 text-4xl font-bold leading-[1.1] tracking-tight sm:text-5xl">
            Write a handler.
            <br />
            <span className="text-accent-500">Get a URL.</span>
          </h1>

          <p className="mt-5 max-w-xl text-[15px] leading-relaxed text-slate-600 dark:text-slate-400">
            A GenHTTP Lambda is a snippet of C# that returns an <code className="font-mono text-[13px]">IHandler</code>.
            Paste it in the editor, hit deploy, and it is online under a URL of your choosing - a REST service,
            an OpenAPI document, a redirect, a page, whatever the module API can build.
          </p>

          <div className="mt-8 flex flex-wrap items-center gap-3">
            <Link to="/editor/create" className="btn-primary px-5 py-2.5 text-[15px]">
              Create a GenHTTP Lambda
            </Link>
            <a
              href="https://genhttp.org/documentation/content/frameworks/functional/"
              target="_blank"
              rel="noreferrer"
              className="btn-ghost px-5 py-2.5 text-[15px]"
            >
              Read the docs
            </a>
          </div>
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
        </div>
      </section>

      <section className="mt-20 grid gap-5 sm:grid-cols-3">
        <Feature icon={<IconKey className="h-5 w-5" />} title="Two keys, no account">
          A public key is where your lambda is hosted. A private one opens the editor. There is nothing
          else to sign up for.
        </Feature>
        <Feature icon={<IconHistory className="h-5 w-5" />} title="Every save is a version">
          Editing and deploying are separate steps. Go back to any earlier version and put it back
          online whenever you want.
        </Feature>
        <Feature icon={<IconSpark className="h-5 w-5" />} title="The whole module API">
          Webservices, controllers, OpenAPI, server sent events, static content - every GenHTTP module
          is already imported for you.
        </Feature>
      </section>

      <section className="surface mt-12 px-6 py-5 text-sm text-slate-600 dark:text-slate-400">
        <strong className="font-semibold text-ink-900 dark:text-slate-200">The free tier, in short.</strong>{' '}
        A deployment stays online for a day, and a lambda nobody touches is removed after a month.
        Code runs on shared infrastructure, so no malware, no crypto miners, and nothing that attacks
        other systems.
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
