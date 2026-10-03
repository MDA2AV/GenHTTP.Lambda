import { useEffect, useState } from 'react';

import { CSharp } from '../components/CSharp';
import { useT } from '../i18n';
import { Link } from '../i18n/links';
import type { Kit } from '../locales/kit';
import { usePublicPage } from '../meta';

/**
 * How to use the thing, in the order somebody meets it.
 *
 * Written as one page rather than a set of them. Everything here is short
 * enough to read in a sitting, and a reader who wants the third step should
 * be able to see that there is a fourth without navigating to find out.
 */

const PARTS = [
  'what',
  'first',
  'editor',
  'why',
  'written',
  'features',
  'files',
  'page',
  'spa',
  'storage',
  'database',
  'keeping',
  'secrets',
  'sockets',
  'limits',
  'away',
  'open',
  'agents',
] as const;

/** How the guide marks up the words in its sentences, in every language. */
const KIT: Kit = {
  code: (text) => <Code>{text}</Code>,
  b: (text) => <b>{text}</b>,
  em: (text) => <em>{text}</em>,
  link: (to, text) => (
    <Link to={to} className="text-accent-500 hover:underline">
      {text}
    </Link>
  ),
};

/** The samples that follow a step of building a front end, by the step they follow. */
const SPA_SAMPLES: Record<number, string> = {
  3: `return Layout.Create().Add(Assets.App("site"));`,
  5: `var api = Inline.Create().Get("notes", () => notes);

return Layout.Create()
             .Add("api", api)
             .Add(Assets.App("site"));`,
};

export function Guide() {
  usePublicPage('/docs');

  const said = useT().guide;

  const [at, setAt] = useState<string>(PARTS[0]);

  // the contents follows the reading rather than the clicking, so somebody who
  // scrolled past three sections can see which one they are in
  useEffect(() => {
    const spotter = new IntersectionObserver(
      (entries) => {
        const showing = entries.filter((entry) => entry.isIntersecting);

        if (showing.length > 0) {
          setAt(showing[0].target.id);
        }
      },
      { rootMargin: '-80px 0px -70% 0px' },
    );

    for (const part of PARTS) {
      const node = document.getElementById(part);

      if (node) {
        spotter.observe(node);
      }
    }

    return () => spotter.disconnect();
  }, []);

  const k = KIT;

  return (
    <div className="mx-auto w-full max-w-6xl px-4 pb-24 pt-10 sm:px-6">
      <header className="max-w-2xl">
        <h1 className="text-3xl font-semibold tracking-tight">{said.title}</h1>
        <p className="mt-3 text-slate-600 dark:text-slate-300">{said.intro}</p>
      </header>

      <div className="mt-10 gap-10 lg:flex">
        <nav aria-label={said.contents} className="mb-8 shrink-0 lg:sticky lg:top-20 lg:mb-0 lg:h-fit lg:w-56">
          <ol className="space-y-1 text-sm">
            {PARTS.map((part, index) => (
              <li key={part}>
                <a
                  href={`#${part}`}
                  className={`flex gap-2.5 rounded px-2 py-1 transition-colors ${
                    at === part
                      ? 'bg-accent-500/10 text-accent-600 dark:bg-accent-400/10 dark:text-accent-400'
                      : 'text-slate-500 hover:text-slate-900 dark:hover:text-slate-200'
                  }`}
                >
                  <span className="tabular-nums text-slate-400">{index + 1}</span>
                  {said.parts[part]}
                </a>
              </li>
            ))}
          </ol>
        </nav>

        <div className="min-w-0 flex-1 space-y-14">
          <Section id="what" title={said.parts.what}>
            <p>{said.what[0](k)}</p>

            <Sample code={`return Content.From(Resource.FromString("hello"));`} />

            <p>{said.what[1](k)}</p>

            <Aside>{said.whatAside(k)}</Aside>
          </Section>

          <Section id="first" title={said.parts.first}>
            <Steps steps={said.first.map((step) => step(k))} />
          </Section>

          <Section id="editor" title={said.parts.editor}>
            <p>{said.editor(k)}</p>
            <Bits bits={said.bits.map(([term, text]) => [term, text(k)])} />
            <p>{said.sections(k)}</p>
            <Aside>{said.editorAside}</Aside>
          </Section>

          <Section id="why" title={said.parts.why}>
            <p>{said.why(k)}</p>
            <Sample code={`POST /api/v1/lambdas/{editorKey}/versions
{
  "files": [ { "name": "lambda.cs", "code": "..." } ],
  "specification": ${JSON.stringify(said.whySample.specification)},
  "change": ${JSON.stringify(said.whySample.change)}
}`} />
            <p>{said.why2(k)}</p>
          </Section>

          <Section id="written" title={said.parts.written}>
            <p>{said.written(k)}</p>
            <dl className="surface divide-y divide-slate-200 dark:divide-ink-800">
              {said.writtenFiles.map(([name, what]) => (
                <div key={name} className="flex flex-col gap-0.5 px-3 py-2 sm:flex-row sm:gap-4">
                  <dt className="shrink-0 font-mono text-[13px] text-slate-900 sm:w-56 dark:text-slate-100">{name}</dt>
                  <dd className="min-w-0 flex-1 text-sm">{what}</dd>
                </div>
              ))}
            </dl>
            <p>{said.written2(k)}</p>
            <p>{said.written3(k)}</p>
            <Aside>{said.writtenAside}</Aside>
          </Section>

          <Section id="features" title={said.parts.features}>
            <p>{said.features(k)}</p>
            <Steps steps={said.featureSteps.map((step) => step(k))} />
            <Sample code={`POST /api/v1/lambdas/{editorKey}/features
{ "name": ${JSON.stringify(said.featureSample)} }

PUT  /api/v1/lambdas/{editorKey}/features/{feature}/files?deploy=true
POST /api/v1/lambdas/{editorKey}/features/{feature}/merge
{ "deploy": true }`} />
            <Aside>{said.featuresAside(k)}</Aside>
          </Section>

          <Section id="files" title={said.parts.files}>
            <p>{said.files(k)}</p>

            <Two
              left={['lambda.cs', `var shelf = new Shelf();

return Inline.Create()
             .Get(() => shelf.All())
             .Post((Book book) => shelf.Add(book));`]}
              right={['Shelf.cs', `public sealed class Shelf
{
    private readonly List<Book> _books = [];

    public IEnumerable<Book> All() => _books;

    public Book Add(Book book)
    {
        _books.Add(book);
        return book;
    }
}

public record Book(string Title, string Author);`]}
            />
          </Section>

          <Section id="page" title={said.parts.page}>
            <p>{said.page}</p>

            <h3 className="pt-2 text-sm font-semibold">{said.inlineTitle}</h3>
            <p>{said.inline}</p>
            <Sample code={`var page = Resource.FromString("""
                              <!doctype html>
                              <title>Mine</title>
                              <h1>It works</h1>
                              """)
                   .Type(new ContentType("text/html; charset=utf-8"));

return Content.From(page);`} />

            <h3 className="pt-2 text-sm font-semibold">{said.folderTitle}</h3>
            <p>{said.folder}</p>
            <Sample code={`return Layout.Create()
             .Add("api", api)
             .Add(Assets.App("site"));`} />

            <h3 className="pt-2 text-sm font-semibold">{said.workspaceTitle}</h3>
            <p>{said.workspace}</p>
            <Sample code={`return Layout.Create()
             .Add("api", api)
             .Add("uploads", Workspace.Files("uploads"))
             .Add(Assets.App("site"));`} />
          </Section>

          <Section id="spa" title={said.parts.spa}>
            <p>{said.spa(k)}</p>

            <Steps
              steps={said.spaSteps.map((step, index) => (
                <>
                  {step(k)}
                  {SPA_SAMPLES[index] !== undefined && <Sample code={SPA_SAMPLES[index]} />}
                </>
              ))}
            />
          </Section>

          <Section id="storage" title={said.parts.storage}>
            <p>{said.storage(k)}</p>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead className="text-xs uppercase tracking-wide text-slate-500">
                  <tr>
                    <th className="py-2 pr-4 font-medium"> </th>
                    <th className="py-2 pr-4 font-medium">{said.savedWithCode}</th>
                    <th className="py-2 font-medium">{said.workspaceColumn}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-200 dark:divide-ink-800">
                  {[
                    ...said.table,
                    [said.reachedAs, <Code key="a">Assets</Code>, <Code key="b">Workspace</Code>] as const,
                  ].map(([label, a, b], index) => (
                    <tr key={index}>
                      <td className="py-2 pr-4 text-slate-500">{label}</td>
                      <td className="py-2 pr-4">{a}</td>
                      <td className="py-2">{b}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <Aside>{said.storageAside}</Aside>
          </Section>

          <Section id="database" title={said.parts.database}>
            <p>{said.database(k)}</p>

            <Sample code={`// migrations/V1__Create_notes.sql:
//   CREATE TABLE notes (id INTEGER PRIMARY KEY, text TEXT NOT NULL);

using (var connection = Database.GetConnection())
{
    new Evolve(connection) { Locations = [Assets.Root + "migrations"] }.Migrate();
}

return Inline.Create()
             .Get("notes", () =>
             {
                 using var db = new Notes(Database.GetConnection());

                 return db.Entries.OrderBy(n => n.Id).Select(n => n.Text).ToList();
             })
             .Post("notes", (NoteInput input) =>
             {
                 using var db = new Notes(Database.GetConnection());

                 db.Entries.Add(new Note { Text = input.Text });

                 return db.SaveChanges();
             });

record NoteInput(string Text);

class Note
{
    public long Id { get; set; }

    public string Text { get; set; }
}

// maps the table the migration made, on the connection it is handed
class Notes(SqliteConnection connection) : DbContext
{
    public DbSet<Note> Entries => Set<Note>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite(connection, contextOwnsConnection: true);

    protected override void OnModelCreating(ModelBuilder model)
        => model.Entity<Note>().ToTable("notes");
}`} />

            <p>{said.database2(k)}</p>
            <p>{said.database3(k)}</p>

            <Aside>{said.databaseAside(k)}</Aside>
          </Section>

          <Section id="keeping" title={said.parts.keeping}>
            <p>{said.keeping(k)}</p>

            <Sample code={`// uploads go into a folder of their own, served as they are
Workspace.CreateFolder("photos");

return Layout.Create()
             .Add("photos", Workspace.Files("photos"))
             .Add("upload", Inline.Create().Post(async (Stream body) =>
             {
                 using var content = new MemoryStream();

                 await body.CopyToAsync(content);

                 Workspace.WriteBytes($"photos/{Guid.NewGuid():N}.jpg", content.ToArray());
             }));`} />

            <p>{said.keeping2(k)}</p>
          </Section>

          <Section id="secrets" title={said.parts.secrets}>
            <p>{said.secrets(k)}</p>

            <Sample code={`var weather = new System.Net.Http.HttpClient();

weather.DefaultRequestHeaders.Add("X-Api-Key", Secret.Read("WEATHER_API_KEY"));

return Inline.Create()
             .Get("today", async () => await weather.GetStringAsync("https://weather.example/today"));`} />

            <p>{said.secrets2(k)}</p>

            <Aside>{said.secretsAside(k)}</Aside>
          </Section>

          <Section id="sockets" title={said.parts.sockets}>
            <p>{said.sockets(k)}</p>

            <Sample code={`var room = new ConcurrentDictionary<IReactiveConnection, string>();

var socket = Websocket.Functional()
                      .OnOpen(c => { room[c] = "someone"; return ValueTask.CompletedTask; })
                      .OnMessage(async (c, text) =>
                      {
                          foreach (var other in room.Keys)
                          {
                              await other.WritePayloadAsync(text);
                          }
                      })
                      .OnClose((c, _) => { room.TryRemove(c, out _); return ValueTask.CompletedTask; });

return Layout.Create().Add("chat", socket);`} />

            <p>{said.sockets2(k)}</p>

            <Aside>{said.socketsAside(k)}</Aside>
          </Section>

          <Section id="limits" title={said.parts.limits}>
            <p>{said.limits}</p>
            <p>{said.limits2}</p>
          </Section>

          <Section id="away" title={said.parts.away}>
            <p>{said.away(k)}</p>
            <p>{said.away2(k)}</p>
            <Aside>{said.awayAside}</Aside>
          </Section>

          <Section id="open" title={said.parts.open}>
            <p>{said.open(k)}</p>
            <p>{said.open2(k)}</p>
            <Aside>{said.openAside}</Aside>
          </Section>

          <Section id="agents" title={said.parts.agents}>
            <p>{said.agents(k)}</p>
            <p>{said.agents2(k)}</p>
            <p>
              <Link to="/#agents" className="text-accent-500 hover:underline">
                {said.more}
              </Link>
            </p>
          </Section>

          <div className="border-t border-slate-200 pt-8 dark:border-ink-800">
            <Link to="/editor/create" className="btn-primary">
              {said.make}
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}

function Section({ id, title, children }: { id: string; title: string; children: React.ReactNode }) {
  return (
    <section id={id} className="scroll-mt-24">
      <h2 className="text-xl font-semibold tracking-tight">{title}</h2>
      <div className="mt-3 space-y-3 text-[15px] leading-relaxed text-slate-600 dark:text-slate-300">
        {children}
      </div>
    </section>
  );
}

function Sample({ code }: { code: string }) {
  // a pre, because the highlighter emits spans and nothing else - the line
  // breaks in the sample are only line breaks if something keeps them
  return (
    <pre className="surface overflow-x-auto p-3 font-mono text-[13px] leading-relaxed">
      <CSharp code={code} />
    </pre>
  );
}

function Code({ children }: { children: React.ReactNode }) {
  return (
    <code className="rounded bg-slate-100 px-1.5 py-0.5 font-mono text-[0.85em] text-slate-800 dark:bg-ink-900 dark:text-slate-200">
      {children}
    </code>
  );
}

function Aside({ children }: { children: React.ReactNode }) {
  return (
    <p className="border-l-2 border-accent-500/50 pl-4 text-sm text-slate-500 dark:border-accent-400/50">
      {children}
    </p>
  );
}

function Steps({ steps }: { steps: React.ReactNode[] }) {
  return (
    <ol className="space-y-4">
      {steps.map((step, index) => (
        <li key={index} className="flex gap-3.5">
          <span className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-accent-500/10 text-xs font-medium tabular-nums text-accent-600 dark:bg-accent-400/10 dark:text-accent-400">
            {index + 1}
          </span>
          <div className="min-w-0 flex-1 space-y-2">{step}</div>
        </li>
      ))}
    </ol>
  );
}

function Bits({ bits }: { bits: [string, React.ReactNode][] }) {
  return (
    <dl className="divide-y divide-slate-200 dark:divide-ink-800">
      {bits.map(([term, said]) => (
        <div key={term} className="flex flex-col gap-1 py-2.5 sm:flex-row sm:gap-4">
          <dt className="shrink-0 font-mono text-sm text-slate-900 sm:w-28 dark:text-slate-100">{term}</dt>
          <dd className="min-w-0 flex-1 text-sm">{said}</dd>
        </div>
      ))}
    </dl>
  );
}

function Two({ left, right }: { left: [string, string]; right: [string, string] }) {
  return (
    <div className="grid gap-3 md:grid-cols-2">
      {[left, right].map(([name, code]) => (
        <div key={name} className="surface overflow-hidden">
          <div className="border-b border-slate-200 px-3 py-1.5 font-mono text-xs text-slate-500 dark:border-ink-800">
            {name}
          </div>
          <pre className="overflow-x-auto p-3 font-mono text-[13px] leading-relaxed">
            <CSharp code={code} />
          </pre>
        </div>
      ))}
    </div>
  );
}
