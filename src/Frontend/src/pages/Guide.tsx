import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { CSharp } from '../components/CSharp';
import { PAGES, usePageMeta } from '../meta';

/**
 * How to use the thing, in the order somebody meets it.
 *
 * Written as one page rather than a set of them. Everything here is short
 * enough to read in a sitting, and a reader who wants the third step should
 * be able to see that there is a fourth without navigating to find out.
 */

interface Part {
  id: string;
  title: string;
}

const PARTS: Part[] = [
  { id: 'what', title: 'What a lambda is' },
  { id: 'first', title: 'Your first one' },
  { id: 'editor', title: 'The control center' },
  { id: 'why', title: 'Saying why' },
  { id: 'files', title: 'More than one file' },
  { id: 'page', title: 'Serving a page' },
  { id: 'spa', title: 'A front end, step by step' },
  { id: 'storage', title: 'The two places files live' },
  { id: 'keeping', title: 'Keeping data' },
  { id: 'sockets', title: 'Websockets' },
  { id: 'limits', title: 'What it will not let you do' },
  { id: 'away', title: 'Taking it away' },
  { id: 'agents', title: 'Letting an agent do it' },
];

export function Guide() {
  usePageMeta(PAGES['/docs']);

  const [at, setAt] = useState(PARTS[0].id);

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
      const node = document.getElementById(part.id);

      if (node) {
        spotter.observe(node);
      }
    }

    return () => spotter.disconnect();
  }, []);

  return (
    <div className="mx-auto w-full max-w-6xl px-4 pb-24 pt-10 sm:px-6">
      <header className="max-w-2xl">
        <h1 className="text-3xl font-semibold tracking-tight">How this works</h1>
        <p className="mt-3 text-slate-600 dark:text-slate-300">
          You write a snippet of C#. Whatever it returns is hosted at a public address, over HTTPS,
          in a few seconds. This is the whole of it, in the order you will meet it.
        </p>
      </header>

      <div className="mt-10 gap-10 lg:flex">
        <nav className="mb-8 shrink-0 lg:sticky lg:top-20 lg:mb-0 lg:h-fit lg:w-56">
          <ol className="space-y-1 text-sm">
            {PARTS.map((part, index) => (
              <li key={part.id}>
                <a
                  href={`#${part.id}`}
                  className={`flex gap-2.5 rounded px-2 py-1 transition-colors ${
                    at === part.id
                      ? 'bg-accent-500/10 text-accent-600 dark:bg-accent-400/10 dark:text-accent-400'
                      : 'text-slate-500 hover:text-slate-900 dark:hover:text-slate-200'
                  }`}
                >
                  <span className="tabular-nums text-slate-400">{index + 1}</span>
                  {part.title}
                </a>
              </li>
            ))}
          </ol>
        </nav>

        <div className="min-w-0 flex-1 space-y-14">
          <Section id="what" title="What a lambda is">
            <p>
              A lambda is a snippet that returns a GenHTTP handler. The platform compiles it,
              loads it, and mounts whatever it returned under your own address. There is no
              project, no build file and no <Code>using</Code> statement. Every GenHTTP module is
              already imported for you.
            </p>

            <Sample code={`return Content.From(Resource.FromString("hello"));`} />

            <p>
              That is a complete lambda. Deployed at <Code>/lambda/your-key/</Code>, it answers
              every request with the word hello.
            </p>

            <Aside>
              The snippet is <em>statements</em>, not a class. The last thing it does is return
              something that can serve requests: a handler, or a builder for one.
            </Aside>
          </Section>

          <Section id="first" title="Your first one">
            <Steps
              steps={[
                <>
                  Press <b>Create a lambda</b>. You get a public address and an editor key. The key
                  is the only way back in, so keep it. Nobody can recover it for you.
                </>,
                <>
                  You land in its control center, with a small REST service already written as the
                  first version. It is only a starting point.
                </>,
                <>
                  Hand the editor key to an agent and tell it what to build - it writes new
                  versions through <Link to="/agentic-coding" className="text-accent-500 hover:underline">MCP</Link>.
                  Or open <b>Code</b> and write it yourself: <b>Check</b> compiles without
                  storing anything and tells you what the compiler thinks, file and line.
                </>,
                <>
                  Press <b>Deploy</b>. Now it is online. Nothing is reachable before that, and
                  deploying again extends how long it stays.
                </>,
              ]}
            />
          </Section>

          <Section id="editor" title="The control center">
            <p>
              The editor link opens a control center rather than a text box: most of the code here is
              written by agents, so the first thing on the screen is how your lambda is doing. The sidebar
              holds the lambda - whether it is online, its address, and a button when a newer version is
              waiting to go online - and its sections. Anything done rarely, like changing the address or
              deleting it, is behind the <b>⋯</b> menu there.
            </p>
            <Bits
              bits={[
                ['Overview', <>Whether it is online, how many requests it had today and how many failed, the latest change, and how much room is left.</>],
                ['Files', <>The files of a version, and its data - what the lambda saves while it runs. A lock or a globe says whether the public can reach them.</>],
                ['Versions', <>What each version changed and what was asked for, and the difference to the one before. Deploy or roll back from here.</>],
                ['Deployments', <>What was online when, and what took it down.</>],
                ['Stats', <>Requests, failures, response times and the most asked-for paths, over the last hour or day.</>],
                ['Logs', <>Its requests, what it printed, and the stack trace of anything that went wrong, as it happens.</>],
                ['Code', <>Writing it by hand. <b>Check</b> compiles, <b>Save</b> makes a version, <b>Deploy</b> puts it online. <Code>Ctrl-S</Code> saves; <Code>F12</Code> goes to a declaration.</>],
              ]}
            />
            <p>
              Every section works the same way: its title, an <b>ⓘ</b> that explains it, its actions on the
              right, and - where it has more than one view - a row of pills underneath. The pills of the code
              are its files.
            </p>
            <Aside>
              The traffic and the log are held in memory, for watching rather than keeping: a restart of the
              server begins them again. Versions and the deployment history are stored.
            </Aside>
          </Section>

          <Section id="why" title="Saying why">
            <p>
              A version is the code, and optionally two notes about it: <b>the prompt</b>, what was asked
              for in the words it was asked in, and <b>the change</b>, one line on what the version does.
              They are shown beside the diff in the version history, so the <em>why</em> survives next to
              the <em>what</em> - for you, and for the next agent that reads the history before changing
              anything.
            </p>
            <Sample code={`POST /api/v1/lambdas/{editorKey}/versions
{
  "files": [ { "name": "lambda.cs", "code": "..." } ],
  "prompt": "a guest book people can sign",
  "change": "Keeps entries in the workspace so they survive a restart"
}`} />
            <p>
              Agents pass the same two fields to <Code>write_code</Code>. In <b>Code</b>, saving asks
              for the change. Both are optional; a long prompt is cut at 4000
              characters and a change at 500 rather than refused.
            </p>
          </Section>

          <Section id="files" title="More than one file">
            <p>
              Types do not have to sit underneath the code that uses them. In <b>Code</b>, press{' '}
              <b>+</b> beside the files and it is compiled beside the snippet, in the same namespace, so nothing has to be
              imported to be reached. A name with no extension is taken to be C#.
            </p>

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

          <Section id="page" title="Serving a page">
            <p>There are three ways, and which one you want depends on where the page lives.</p>

            <h3 className="pt-2 text-sm font-semibold">One page, written inline</h3>
            <p>Fine for something small. The page is part of the snippet.</p>
            <Sample code={`var page = Resource.FromString("""
                              <!doctype html>
                              <title>Mine</title>
                              <h1>It works</h1>
                              """)
                   .Type(new ContentType("text/html; charset=utf-8"));

return Content.From(page);`} />

            <h3 className="pt-2 text-sm font-semibold">A folder of real files</h3>
            <p>
              What you want for anything with a stylesheet and a script. The files are added the
              same way a C# file is, and served exactly as written. Nothing compiles them.
            </p>
            <Sample code={`return Layout.Create()
             .Add("api", api)
             .Add(Assets.App("site"));`} />

            <h3 className="pt-2 text-sm font-semibold">From the workspace</h3>
            <p>
              When the page is uploaded rather than written, and should be changeable without
              redeploying.
            </p>
            <Sample code={`return Layout.Create().Add(Workspace.App());`} />
          </Section>

          <Section id="spa" title="A front end, step by step">
            <p>
              The second of those, in full. The finished thing is the{' '}
              <Link to="/examples/site" className="text-accent-500 hover:underline">
                front end in a folder
              </Link>{' '}
              example, which you can clone.
            </p>

            <Steps
              steps={[
                <>
                  In <b>Code</b>, press <b>+</b> beside the files and type <Code>site/index.html</Code>. A name with a
                  slash in it puts the file in a folder; a name with an extension is taken as the
                  file it says it is.
                </>,
                <>
                  Add <Code>site/app.css</Code> and <Code>site/app.js</Code> the same way. Your page
                  refers to them by name, as in <Code>href="app.css"</Code>, because the folder is
                  the root of what gets served rather than part of the address.
                </>,
                <>
                  For anything that is not text, like an image or a font, open a file in{' '}
                  <Code>site</Code> and press the upload button beside the files: it lands in the same
                  folder. A PNG cannot be typed into a text editor, so that is the way in.
                </>,
                <>
                  In <Code>lambda.cs</Code>, serve the folder:
                  <Sample code={`return Layout.Create().Add(Assets.App("site"));`} />
                </>,
                <>
                  Press <b>Deploy</b>. <Code>site/index.html</Code> answers at <Code>/</Code>,{' '}
                  <Code>site/app.css</Code> at <Code>/app.css</Code>, and any address matching no
                  file is answered with the page, so a front end that does its own routing still
                  works when somebody reloads on a deep link.
                </>,
                <>
                  Add an API beside it and the page has something to talk to:
                  <Sample code={`var api = Inline.Create().Get("notes", () => notes);

return Layout.Create()
             .Add("api", api)
             .Add(Assets.App("site"));`} />
                </>,
              ]}
            />
          </Section>

          <Section id="storage" title="The two places files live">
            <p>
              The <b>Files</b> section shows both - the files of a version, and the workspace as{' '}
              <b>Data</b> - and says which of them the public can reach. Files of the code are changed in{' '}
              <b>Code</b>; data can be uploaded and deleted in <b>Files</b>. They are not the same thing
              though, and the difference is <em>when each changes</em>.
            </p>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead className="text-xs uppercase tracking-wide text-slate-500">
                  <tr>
                    <th className="py-2 pr-4 font-medium"> </th>
                    <th className="py-2 pr-4 font-medium">Saved with your code</th>
                    <th className="py-2 font-medium">Workspace</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-200 dark:divide-ink-800">
                  {[
                    ['what it holds', 'every file of your lambda, the C# included', 'whatever has been written or uploaded'],
                    ['when it changes', 'when you press Save or Deploy', 'the moment something is written to it'],
                    ['a deploy', 'replaces all of it', 'never touches it'],
                    ['rolling back a version', 'brings the old files back', 'no effect'],
                    ['cloning the lambda', 'comes along', 'does not'],
                    ['reached from code as', <Code key="a">Assets</Code>, <Code key="b">Workspace</Code>],
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

            <Aside>
              They cannot be one directory. If they were, a deploy would either wipe everything your
              lambda had written since, or nothing could ever be removed from what it ships. A game
              that keeps a leaderboard wants the second; the page it serves wants the first.
            </Aside>
          </Section>

          <Section id="keeping" title="Keeping data">
            <p>
              <Code>Workspace</Code> is a private directory your lambda may read and write. It is
              the place for anything that has to outlive a request, or a deployment.
            </p>

            <Sample code={`var notes = new List<string>();

if (Workspace.Exists("notes.json"))
{
    notes.AddRange(JsonSerializer.Deserialize<List<string>>(Workspace.ReadText("notes.json")) ?? []);
}

return Inline.Create()
             .Get("notes", () => notes)
             .Post("notes", (string text) =>
             {
                 notes.Add(text);
                 Workspace.WriteText("notes.json", JsonSerializer.Serialize(notes));
                 return notes.Count;
             });`} />

            <p>
              There is also <Code>ReadBytes</Code>, <Code>WriteBytes</Code>, <Code>Delete</Code>,{' '}
              <Code>List</Code>, <Code>CreateFolder</Code>, and <Code>Tree</Code>/<Code>Files</Code>
              /<Code>App</Code> for serving it. Nothing else on the file system is reachable.
            </p>
          </Section>

          <Section id="sockets" title="Websockets">
            <p>
              Supported, and not an afterthought. The{' '}
              <Link to="/examples/arena" className="text-accent-500 hover:underline">
                arena
              </Link>{' '}
              example holds a world and broadcasts to everybody twenty times a second. The simplest
              form is three callbacks:
            </p>

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

            <Aside>
              One thing catches everybody: a browser cannot set headers on a websocket handshake.
              Pass what the handler needs in the query, where it reads it
              from <Code>connection.Request.Header.Query</Code>, or send secrets as the first message.
            </Aside>
          </Section>

          <Section id="limits" title="What it will not let you do">
            <p>
              Your code runs on a shared server, so some of C# is refused before it compiles:
              starting processes, opening sockets of your own, loading assemblies, reaching the file
              system outside your workspace, and reflection used to get around any of that.
            </p>
            <p>
              Everything else is there, including the whole of the GenHTTP module API. If something
              is refused you are told which line and why, not simply that it failed.
            </p>
          </Section>

          <Section id="away" title="Taking it away">
            <p>
              <b>Download</b> in the editor gives you the whole thing as a .NET project: a solution
              you can open, <Code>dotnet run</Code>, and keep. It has one package reference and no
              trace of this platform in it.
            </p>
            <p>
              Your snippet becomes the body of <Code>Program.cs</Code>, wrapped in a host that
              serves what it returns. Your other files come across exactly as you wrote them.{' '}
              <Code>Workspace</Code> and <Code>Assets</Code> become two folders beside the code, with
              the same methods, so nothing in your code has to change.
            </p>
            <Aside>
              Worth knowing before you build anything here: what you write is yours and it leaves
              whole. Nothing about running it on this machine locks it to this machine.
            </Aside>
          </Section>

          <Section id="agents" title="Letting an agent do it">
            <p>
              There is an MCP endpoint at <Code>/mcp</Code>. Point an agent at it and it can do
              everything the editor does: read the guide, read an example in full, write files,
              compile them, and deploy. It is the same API underneath.
            </p>
            <p>
              It says why as it goes - <Code>write_code</Code> takes the prompt and the change - and
              it can look at what it deployed: <Code>read_logs</Code> answers with the lambda's recent
              requests, what it printed and the stack trace of anything it threw, which is how an agent
              finds out its code works rather than assuming it. You watch the same thing in the
              control center.
            </p>
            <p>
              <Link to="/#agents" className="text-accent-500 hover:underline">
                More about that →
              </Link>
            </p>
          </Section>

          <div className="border-t border-slate-200 pt-8 dark:border-ink-800">
            <Link to="/editor/create" className="btn-primary">
              Make one
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
