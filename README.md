# GenHTTP Lambda

Write a C# snippet that returns a GenHTTP `IHandler`, press deploy, and get a
public URL that serves it. The snippet is compiled with Roslyn at runtime and
the resulting handler is hosted by the same [GenHTTP][genhttp] server that
serves this application.

```csharp
return Inline.Create()
             .Get(() => "Hello from my lambda!");
```

## Quick start

```bash
docker compose up --build
```

The application is then available at <http://localhost:8080/>. Its data - the
SQLite database, the stored code, and the workspaces and databases of the
lambdas - lives in a named volume mounted at `/data`.

## Developing locally

Two processes: the .NET server, and Vite serving the frontend with hot reload.

```bash
# the server, on http://localhost:8080/
dotnet run --project src/GenHTTP.Lambda

# the frontend, on http://localhost:5173/ (proxies /api, /lambda and /features to the server)
cd src/Frontend && npm install && npm run dev
```

Work against <http://localhost:5173/> while developing. The server on its own
answers with a placeholder page until a frontend has been built into its web
root, which is what `npm run build` does:

```bash
cd src/Frontend && npm run build   # writes src/GenHTTP.Lambda/wwwroot
```

English is the source of every page; the other languages are translated by
script and agent, so that only what changed is read and written:

```bash
cd src/Frontend
npm run translations -- extract   # a request per language in .translations/
# one `translator` agent per request (.claude/agents/translator.md, on Sonnet)
npm run translations -- apply     # the answers into the catalogs, checked
npm run translations -- check     # every language against English
```

CLAUDE.md ("How a change is translated") has the rules.

Build and test everything the way CI does:

```bash
dotnet build GenHTTP.Lambda.slnx -c Release
dotnet test GenHTTP.Lambda.slnx -c Release
```

The tests compile and execute real lambdas against a real server on a random
port, each against its own temporary data directory.

## Routes

| Route                | Serves                                                    |
|----------------------|-----------------------------------------------------------|
| `/`                  | the landing page                                          |
| `/build`             | the text box for somebody who wants an app built, no code |
| `/ship`              | for developers: connect an agent and publish what it built |
| `/docs`              | the guide for owners                                      |
| `/showcase`          | the lambdas their owners chose to show                    |
| `/source`            | the lambdas whose owners published their code             |
| `/source/:publicKey` | the published code of one: its files, documentation, tests and changes, per version |
| `/source/:publicKey.git` | the published code as a git repository, read only     |
| `/enterprise`, `/terms`, `/privacy`, `/imprint` | the company pages              |
| `/editor/create`     | the creation assistant                                    |
| `/editor/:privateKey`| the editor for one lambda                                 |
| `/editor/:privateKey/:name.git` | the lambda as a git repository, to clone and push to (any name; the editor offers the public key) |
| `/lambda/:publicKey` | the deployed handler                                      |
| `/features/:feature` | the preview of a feature, while it is online              |
| any path, at a lambda's own domain | the deployed handler of a premium lambda with that domain |
| `/api/v1/`           | everything the editor calls, see below                    |
| `/mcp`               | the same, for agents                                      |
| `/admin`             | every lambda on the server, for whoever runs it            |

### API

Browsable at `/api/v1/scalar/`. A lambda is addressed by its editor key, which
is also what authorizes the call. Things that can be listed, read, created or
changed are resources; what is a process rather than a thing is a verb in the
path.

| Endpoint                                              | Does                                      |
|-------------------------------------------------------|-------------------------------------------|
| `POST /lambdas`                                       | creates a lambda (optionally with `view`: `Full` or `Simple`) |
| `GET / PATCH / DELETE /lambdas/:privateKey`           | reads, changes (its key, its `view`), removes it |
| `GET /lambdas/:privateKey/export`                     | the newest version as a runnable .NET 10 project with a Dockerfile (zip), with its database, see below |
| `GET / POST /lambdas/:privateKey/versions`            | lists versions, saves a new one (optionally with `specification` and `change`) |
| `GET /lambdas/:privateKey/versions/:version`          | reads one version (`?folder=docs/` for its documentation alone, `resources/` for its resources) |
| `GET /lambdas/:privateKey/versions/:version/zip`      | one version's files as a zip (`?layout=project` laid out as a clone) |
| `POST /lambdas/:privateKey/versions/zip`              | saves a zip of all files as a new version (`?layout=project` for one laid out as a clone) |
| `POST /lambdas/:privateKey/versions/changes`          | changes some files of the newest version, as a new one |
| `GET /lambdas/:privateKey/deployment`                 | what is online, and until when            |
| `POST /lambdas/:privateKey/deployment/start` / `stop` | puts a version online, takes it off       |
| `GET /lambdas/:privateKey/deployment/history`         | every stretch it was online, and what ended it |
| `GET /lambdas/:privateKey/summary`                    | the dashboard: state, traffic, problems, storage, what the documentation says |
| `GET /lambdas/:privateKey/traffic`                    | requests by minute and quarter hour, statuses, paths |
| `GET /lambdas/:privateKey/logs`                       | its own log, followed with `?since=`; no visitor addresses |
| `GET /lambdas/:privateKey/agent`                      | the agent's change of it - under way, or the last one - and how many are left today |
| `POST /lambdas/:privateKey/agent/start` / `stop`      | asks the agent for a change, stops it     |
| `GET /lambdas/:privateKey/data`                       | the kinds of data it keeps, and how full each is |
| `GET / PUT / DELETE /lambdas/:privateKey/data/:kind`  | one kind (`database`, `workspace`, `secrets`); switches it on; switches it off and deletes what it held |
| `GET /lambdas/:privateKey/database`                   | its database: tables and views, their columns and rows, how full it is, whether the code connects |
| `GET /lambdas/:privateKey/database/tables/:table`     | a page of one table's rows, newest first (`offset`, `limit`, `order`, `descending`) |
| `GET /lambdas/:privateKey/secrets`                    | the secrets by name, which names the code reads, and which of those have no value |
| `PUT / DELETE /lambdas/:privateKey/secrets/:name`     | stores a value (`{ "value": … }`), never read back; removes one |
| `GET /lambdas/:privateKey/files`                      | lists the workspace                       |
| `GET / PUT / DELETE /lambdas/:privateKey/files/:path` | one file, its path encoded (`a%2Fb.txt`), as base64 up to 32 MB |
| `GET / PUT /lambdas/:privateKey/files/:path/content`  | one file as it is, streamed, however large |
| `PUT /lambdas/:privateKey/folders/:path`              | makes a folder                            |
| `GET / POST /lambdas/:privateKey/features`            | lists the features, starts one (`name`, `specification`, `base`) |
| `GET / PATCH / DELETE /lambdas/:privateKey/features/:feature` | one feature with its files (`?folder=` as for a version); changes its name, notes or `base`; deletes it |
| `PUT /lambdas/:privateKey/features/:feature/files`    | replaces its files (`?deploy=true` puts its preview online) |
| `POST /lambdas/:privateKey/features/:feature/changes` | changes some of its files                 |
| `GET / PUT /lambdas/:privateKey/features/:feature/zip`| its files as a zip; a zip put back into it (both with `?layout=project`) |
| `POST /lambdas/:privateKey/features/:feature/preview/start` / `stop` | puts its preview online, takes it off |
| `POST /lambdas/:privateKey/features/:feature/merge`   | makes it the next version and deletes it (`deploy` to put that online) |
| `GET /lambdas/:privateKey/features/:feature/logs`     | what its preview has been doing           |
| `GET /lambdas/:privateKey/features/:feature/data`, `POST …/data/refresh` | its copy of the data; a fresh copy of the lambda's |
| `…/features/:feature/workspace[/:path[/content]]`, `…/folders/:path` | its copy of the workspace, as `files` and `folders` are the lambda's |
| `…/features/:feature/secrets[/:name]`                 | its copy of the secrets, as `secrets` are the lambda's |
| `…/features/:feature/database[/tables/:table]`        | its copy of the database, as `database` is the lambda's |
| `POST /lambdas/:privateKey/code/check`                | compiles without saving                   |
| `POST /lambdas/:privateKey/code/semantics`, `completions`, `definition` | what the editor asks the compiler |
| `GET /keys/:publicKey`                                | whether a key is free, and if not, online |
| `GET /demos`                                          | the demos, and the keys to read them with |
| `GET / PUT / DELETE /lambdas/:privateKey/showcase`    | its entry on the showcase: read, list it (title, description, picture), take it off |
| `GET /lambdas/:privateKey/domain`, `PUT / DELETE`     | the domain of a premium lambda, whether it is served, and what it resolves to |
| `GET /showcases`, `GET /showcases/:publicKey/image`   | what `/showcase` lists, and a picture |
| `GET / PUT / DELETE /lambdas/:privateKey/source`      | whether its code is published and under which license, the licenses on offer; publishes it or changes the license (`license`, `author`); takes it down |
| `GET /sources`                                        | what `/source` lists (`search`, `order`: `stars`, `updated`, `published`, `skip`, `take`) |
| `GET /sources/:publicKey`                             | a published source: what it is, its license, where it runs, every version and what it changed, a ticket to star it with |
| `GET /sources/:publicKey/versions/:version`           | the files of a version as its project has them, packed on the first request |
| `GET /sources/:publicKey/versions/:version/files/:path`, `…/raw/:path` | one file, its path encoded: its text; as it is (`?download=1` to save it) |
| `GET /sources/:publicKey/versions/:version/zip`       | a version as a project to download        |
| `POST /sources/:publicKey/star`                       | stars it, or takes the star back (`ticket`, `starred`) |
| `POST /builds`, `GET /builds/:id`                     | the text box on `/build`                  |
| `GET /system`                                         | terms, limits, starters, build agent      |
| `GET /telemetry`, `/logs`, `/admin/...`               | for whoever runs the installation         |

Creating a lambda asks what somebody would like to build and offers the demos
in those words - "Keep track of things", "Let people sign up" - next to an empty
lambda. Picking a demo starts the new lambda as a copy of it, which is theirs to
change. The files are in `Resources/Templates` and listed in `TemplateCatalog`.

A lambda has two keys. The public one is part of its URL and may be changed;
the private one is the editor link and is shown only to whoever created the
lambda - anyone holding it can edit, deploy and delete.

A lambda may also return a websocket rather than a document, in any of the
three flavours the module offers - `Websocket.Functional()`, `.Reactive()` and
`.Imperative()`. The handshake is an ordinary request and is bounded by the
execution timeout like any other; everything after it belongs to the
connection, which is why a socket may stay open far longer than a lambda is
given to answer. `FrameType`, which the imperative flavour reads off every
frame, is one of the names a lambda is given, so no import is needed.

The editor says when both free tier timers run out: a hint under the public URL
for the deployment, and a chip beside the buttons for the lambda itself, which
opens an explanation of how each one is extended.

### Versions, features and data

A lambda keeps three kinds of things, and they live differently.

A **version** is the program: its **code** - the C# it is compiled from, and
whatever else is kept with it, its documentation and its tests among it - and
its **resources**, which it reads and serves while it runs, the front end and
the database's migrations among them (see [Code and resources](#code-and-resources)).
A version never changes once it is saved, which is
what makes every one worth keeping - any of them can be compared with, and put back online
exactly as it was. Saving files (`POST …/versions`, `write_code`) makes a new
one.

A **feature** is a change worked on beside the lambda. It branches off a
version - the newest, unless another is named - with a copy of its files and a
copy of the lambda's data, and is changed in place as often as it takes; it has
no versions of its own. Its preview is built from its files against its copy of
the data and answers at `/features/{feature}/`, a random key nobody guesses,
told to search engines with `X-Robots-Tag: noindex` and a `Disallow` in
`robots.txt`. It serves what was deployed until it is deployed again, a restart
included, is counted neither in the lambda's traffic nor in its log, and on the
free tier goes offline like a deployment does when nobody works on it. Once it
does what was asked it is **merged**: its files become the next version, with
its notes, and the feature goes - preview, copy of the data and all; the
lambda's own data is not touched. Only a feature based on the newest version
can be merged, so that a merge never undoes a version saved after the feature
began - by the owner, an agent, or the merge of another feature. There is no
merging or rebasing machinery: whoever works on the feature brings a newer
version's changes in and then says so by moving its `base` (`PATCH`, or
`update_feature`), and the merge takes the lambda's lock and checks the base
once more, so two merges cannot both win. A lambda may have ten features open
(a limit of its tier, set in the panel); a demo has none. Each holds a full copy of the
workspace and of the database, taken on a thread of its own and swapped in
whole, so ten features of a premium lambda with a full workspace take ten times
its room on disk.

**Data** is what the program keeps: the database, a SQLite database for its
records; the workspace, a private directory of files; and the secrets. It
belongs to the lambda rather than to a version - every version reads and
writes the same data, and deploying, rolling back or merging never touches it.
It goes with the lambda, or when that kind of data is switched off, which
deletes what it held - the copies features work on included. Each kind is
switched on before the lambda can use it (`PUT …/data/:kind`, `enable_data`):
the workspace is on unless it was switched off, and a lambda whose workspace
is off is compiled with one that refuses every call and says why; the database
and the secrets are off until somebody switches them on. An agent may switch a kind on when what it builds
needs one; switching off deletes, so only the owner does that. The database
and the workspace share one allowance of room, a limit of the tier: each may
grow as far as the other leaves it. The editor has a **Data** section of its
own, beside **Versions**. Every kind is one view of it, picked from a row of
pills under its title, and each is shown the same way - what it is, whether it
is on, how full it is - with only what it holds drawn its own way.

Deploying picks a version (the latest by default), builds it and makes it live,
and a lambda has at most one deployment at a time. A deployment that does not
compile leaves the one before it standing - its resources included, which are
written back after the failed attempt replaced them.

The editor is written for people who had an app built rather than for
developers, so it calls a feature a **draft**, its copy of the data **test
data**, and merging it **putting it online** - which merges it and deploys the
version it becomes in one step. Nothing in it says merge, base or branch; a
feature behind the newest version is **out of date**, and the agent is offered
to bring it up to date. The drafts have a section of their own, shown once
there is one, and opened on one the editor becomes the draft's: the sidebar
holds it, with the buttons that try it and put it online, and its views are
its documentation, its code, its tests, its test data and its preview's log. Showcase, domain, figures and
deployments stay with the lambda. A version offers to start a draft from it,
the save dialog of the code offers to save what was typed as a new draft
instead of a version, and a change the agent leaves as a draft can be tried
and put online from the Change section.

### Code and resources

A version is any number of files, in two parts:

```
lambda.cs           the snippet: what it returns is served
Store.cs            more C#, compiled beside it
docs/, tests/       what is written about it (see below)
frontend/, …        anything else: kept with the version, never compiled or served
resources/          what it reads and serves while it runs
```

Its **code** is every file that is not a resource. The `.cs` files at the top
are compiled into the lambda, and nothing else is: every other file, in any
folder and of any kind - its documentation and its tests, what a front end is
built from, scripts, configuration - is kept with the version and never
compiled or served. A C# file below a folder is such a file too, a test in C#
for one. How the rest of the code is arranged is the agent's to decide; the
editor only reads `docs/` and `tests/` as what they are (below).

Its **resources**, in `resources/`, are what it reads and serves while it
runs: pages, scripts, styles, pictures, the database's migrations. The
snippet reaches them through `Resources`, which is the other half of
`Workspace` - `Resources.App("web")` serves `resources/web/` as a single page
application, `Resources.Root + "migrations"` is where Evolve finds the
migrations. A version saved before resources were called that still compiles:
`Assets` is the same thing under its old name.

What a version may come to is one allowance of room, its code and its
resources together - a limit of its tier, set in the panel (see
[Limits](#limits)). There is no separate limit for C#: the room bounds what
the compiler is given as well.

The C# at the top is held to letters, digits, dashes and underscores, starting
with a letter and ending in `.cs`, 40 characters at most. A resource keeps the
rules files that are served have always had: letters, digits, dashes,
underscores and dots, an extension, no name starting with a dot, six folders
deep at most. The rest of the code may be named as tools name their files -
dot files, and the brackets, parentheses, plus signs and the like some tools
read meaning off - with no spaces, no `.git` folder, sixteen folders deep at
most. The names a project gives its own files at the top (`Program.cs`,
`Dockerfile`, `.gitignore`, `.dockerignore`, `LICENSE`, `AGENTS.md`,
`CLAUDE.md`, a project file, and the folders `Platform`, `bin`, `obj`,
`workspace` and `database`) are refused, since an export or a clone has them;
so are `.lambda/` and `assets/`, which name where the documentation and the
served files used to be, with an answer that says where they go now.

Only the compiled C# and where the resources are identify a build, so a
version that changes nothing else - its documentation, its tests, what it is
built from - builds its handler from the assembly already loaded rather than
compiling again.

**Lambdas saved before this layout** kept their served files at the top of a
version and their documentation, tests and build folder in `.lambda/`
(`docs/`, `tests/`, `build/`). Nothing on disk was rewritten: a version is
read in the layout it was saved in, so the old ones are translated as they are
read - `site/app.js` becomes `resources/site/app.js`, `.lambda/docs/product.md`
becomes `docs/product.md`, `.lambda/build/package.json` becomes
`build/package.json` - and saved in the new one when they are next saved (an
envelope with `"version": 2`, `LambdaSource.Parse`). A feature's files are read
the same way. In git, a lambda's history keeps the commits its clones already
have; the move is a commit of its own on top of each (see
[Working with git](#working-with-git)).

### What it is built from

Some of a lambda may be made by a build tool rather than written as it is
served or compiled: a front end that is compiled or bundled, styles compiled
from another language, code generated from a description. What the tool makes
is part of the program like anything else - resources, or code - and what it
makes them from is part of the code, in a folder of its own that the agent
names. Nothing about those files is assumed: they are whatever the tool works
from, in whatever layout it wants.

```
frontend/              what it is built from, in whatever shape the tool wants
frontend/README.md     how it is built, and where the build goes
frontend/.gitignore    what the build installs or keeps for itself, left out
resources/web/         what the build wrote, if it makes resources
*.cs                   what it wrote, if it makes code
```

**The platform builds nothing.** It never runs a build tool or installs what
one needs, on a push or anywhere else, and never checks that the program is
what those files build to. Whoever changes it - an agent with a shell, in a
clone most of the time - changes them, runs the build where it works, and
saves both together, in one version or one feature. A change to the sources
that is not built changes nothing; a change made in what the build wrote is
undone by the next build. This keeps the platform what it is - a server that
hosts what it is given - and the tools the agent's, whichever they are.

What a build installs, caches or writes for itself - `node_modules`, `target`
or `.venv`, for example - is no part of a version, and every tool names it
differently, so the folder says what it is, in its own `.gitignore` files. A
clone follows them because git does; a zip put back through the API follows
them too, the way `git add` would (`IgnoredPaths`, git's rules: patterns at any
depth or anchored, folders only, `!`, `**`, the deeper file having the last
word, nothing coming back from a folder left out) - but keeps what the lambda
already has, as git keeps what it tracks. Only the `.gitignore` files of the
code count: a resource never has a name starting with a dot. A zip
is counted as it is sent, before any `.gitignore` leaves something out, so it
holds what a commit would - never what a build installed, which is no part of
it and would only make the upload large. A save that names a file keeps it, as
git keeps a file added on purpose; one that runs over the allowance names the
largest folder of the code, and says that its `.gitignore` keeps out what a
build installed.

**One layout to build in.** A build writes by paths relative to itself - from
`frontend/`, into `../resources/web` - and a clone, an export and the zip of a
version or a feature lay the files out the same way, so it finds them wherever
it runs. The zip can be read back as a commit of a clone would be read too,
`?layout=project` on the download and on the upload: the platform's files
skipped, what the repository's `.gitignore` leaves out left out (`bin/`,
`obj/`), anything with no place in a lambda refused, and `Project.cs` that was
not touched kept as the snippet it was, to the byte. Agents that build are
steered to a clone or to that layout; agents with only the tools cannot build,
and never need it. A save through the tools that replaces every file says so
when it left out a folder of the code the version had - the sources of a
front end, most often - since those are dropped with it.

The build agent behind `/build` has no shell: it writes its front ends as plain
HTML, CSS and JavaScript, and leaves the sources of one it finds alone. The
demos have none either: they teach how a lambda is put together, with front
ends of plain files a reader follows without a build tool.

### Documentation and tests

Every version keeps what is written about it beside its program, so that
whoever changes it next - an agent, most of the time - starts from what the
app is for and what has to keep working, which the code does not say:

| File | What it holds |
|---|---|
| `docs/product.md` | what the app is, who it is for, what people do with it and why - in the terms of whoever asked for it; its first paragraph is the app in a sentence or two |
| `docs/decisions.md` | the technical decisions, and why they were made |
| `docs/…` | more pages, and pictures the pages show |
| `tests/README.md` | how the app is tested automatically: what has to keep working, how each of it is checked, how to run the scripts |
| `tests/…` | the scripts and the test data they use |

They are files of the version rather than fields in the database, because
they describe that version of the program: saved, compared in the history,
rolled back, copied into a feature and merged with it like any other file, in
the zip and in the export, with no table and no migration. Rolling back brings
back the documentation that was true of the version. They are code like any
other file that is not a resource - never compiled, whatever they are called
(a test in C# is not compiled), never served, left out of what identifies a
build, and counted towards what a version may come to. `docs/` and `tests/`
are a convention, not a kind of file: nothing about them is special but that
the editor reads them as what they are, and the agents are told to keep them.

A zip of a version or a feature holds them, which is how an agent working
locally reads and writes them without further calls. The agents are told everywhere they decide something - the instructions,
the tool descriptions, the answers and `platform_guide`
(`documentationAndTests`) - to read them before changing a lambda, to write
all three pages with a new lambda, to update what a change affects in the same
save, and to run the tests against a feature's preview before merging it -
all of it in proportion to the lambda: a small one gets a few lines a page and
one quick check, not a test suite. `read_lambda` hands them over first and apart from the program's files, within
a budget of their own, and every save, deployment and preview that leaves a
page out says which. Every demo has all three and a script its tests run,
checked by a test.

In the editor they are two sections. **Documentation** is second in the
sidebar, after the overview, in both views; **Tests** is in the full view only.
Both are one component over their folder: the pages rendered to be read, each a
pill, with the other files behind a **Files** pill, and the version picked as
it is in the code. A page the version changed is marked, and shows the
difference on request. On the newest version or in a draft a page can be
edited, beside a preview of it; saved, it is the next version (a draft saves in
place), put online with it when the newest version was online. The simple view
calls the documentation **About** and shows the product page alone, with a way
to have the agent correct it rather than to edit it. The overview of either
view opens with the product page's first paragraph, and the full one says which
of the three pages the version has.

With fifteen sections and more, the full view's sidebar is in groups: the
overview and the documentation on their own, then how people find it and what
they get to see of it (showcase, open source, domain) - what the lambda is to
everybody else, right under what it is - then where a change is made, under
Develop (Change, the drafts, the code, the tests), the program and what it
keeps (versions, data), and how it runs (deployments, stats, logs). On a phone
the groups are a rule apart in the row of sections.

**Code**, in the full view only, is every file of a version: the code and the
resources as two trees beside the editor, each with what it comes to and
whether the public can reach it, and under them what the version comes to of
its allowance. It opens on the newest version, or a draft's files in a draft;
an older one is picked above it. A file of either part can be added, uploaded,
removed and edited there, and saved as the next version (a draft saves in
place and puts its preview online). Links to the sections it replaced -
`/files` and `/build` - open it.

### Databases

Records - entries, accounts, orders, votes - go in the database: a SQLite file
of the lambda's own, which its code opens a connection to and reads and writes
through [Entity Framework Core][efcore] - or in plain SQL, where that is
simpler. Its schema is migrations shipped with the version and applied by
[Evolve][evolve] as the lambda starts, never Entity Framework's own:

```csharp
// resources/migrations/V1__Create_notes.sql: CREATE TABLE notes (id INTEGER PRIMARY KEY, text TEXT NOT NULL);
using (var connection = Database.GetConnection())
{
    new Evolve(connection) { Locations = [Resources.Root + "migrations"], IsEraseDisabled = true }.Migrate();
}

return Inline.Create().Get("count", () =>
{
    using var db = new Notes(Database.GetConnection());

    return db.Entries.Count();
});

public class Note
{
    public long Id { get; set; }

    public string Text { get; set; }
}

// maps the table the migration made, on the connection it is handed
public class Notes(SqliteConnection connection) : DbContext
{
    public DbSet<Note> Entries => Set<Note>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite(connection, contextOwnsConnection: true);

    protected override void OnModelCreating(ModelBuilder model) => model.Entity<Note>().ToTable("notes");
}
```

`Database.GetConnection()` hands out an open `SqliteConnection` of
Microsoft.Data.Sqlite, taken from a pool, which the caller disposes of - or a
context it is handed, with `contextOwnsConnection: true` - one per request,
never one shared between requests. The connection comes into the context
from outside, because inside a `DbContext`, `Database` is the context's own
(the compiler's message says so, and names `LambdaEnvironment.Database`).
`Microsoft.Data.Sqlite`, `Microsoft.EntityFrameworkCore` and `EvolveDb` are
imported in every file, like the GenHTTP modules. Agents are told to use it
synchronously - `ToList` and `SaveChanges`, never `ToListAsync` or
`SaveChangesAsync` - because SQLite answers synchronously whatever the method
is called, and the engine has an asynchronous model of its own that a blocking
call here does not disturb; this is said, not enforced. The demos all keep
their records like this, `demo-crud` the plainest of them.

- **Off until switched on.** Switching it on (the editor, `PUT …/data/database`,
  `enable_data`) makes it: an empty file, in write-ahead logging, so readers
  never wait for a writer. Switching it off deletes it, the copies of the
  features included. Either way the lambda and its previews are started again
  on their next request, so what they do as they start - migrating - is done
  against what is there now. A lambda created as a copy of a demo that keeps
  one starts with its own, switched on.
- **The one file it opens.** A lambda does not make connections of its own.
  The code guard refuses `new SqliteConnection(...)` - also as a target-typed
  `new(...)`, through an alias or a class derived from it, which is asked of
  the compiler rather than read off the text - and the names that would point
  a connection elsewhere or load native code into it (`ConnectionString`,
  `SetConnectionString`, `LoadExtension`, `DbProviderFactories`, …). Of Entity
  Framework, a lambda reaches the namespace a context is written in and the
  three describing a model takes (`Metadata.Builders`, `ChangeTracking`,
  `Storage.ValueConversion`), not its `Infrastructure`, `Storage`, `Internal`
  or the rest, where a context's services and options are. `UseSqlite` is
  refused with anything but a connection, and so are `EnsureCreated`,
  `EnsureDeleted` and Entity Framework's `Migrate` - the schema is Evolve's -
  all asked of the compiler. `dynamic` is refused too: it binds members at
  runtime, which is everything the guard reads the code for, skipped. The
  connection it is handed carries an authorizer SQLite asks about every
  statement: `ATTACH` and `VACUUM INTO`, which reach other files, are refused,
  as are the pragmas that would point it at other directories or lift its
  quota, and SQLite's defensive mode is on. The handle underneath is a type
  the code cannot name.
- **Its room.** 256 MB, and 2 GB for a premium lambda, by default - a
  [limit](#limits) of its tier - enforced by SQLite with `max_page_count` on
  every connection: past it a write fails with *database or disk is full*. A
  limit the operator lowers holds from the next connection; nothing stored is
  removed.
- **Features get a copy.** Taken with SQLite's backup, which reads the database
  in one transaction while the lambda goes on writing, so it is the database
  as it was at one moment. A preview migrates its copy as it starts, which is
  where a migration is tried first; merging throws the copy away.
- **Read by the owner, written by the lambda.** The editor shows its tables,
  their columns and rows under Data (`GET …/database`); `read_database` does
  the same for an agent. Nothing but the lambda writes to it.
- **Versions share it.** A rollback runs an older version against a schema a
  newer one migrated; Evolve lets a version start whose migrations are fewer
  than the database's, and agents are told to change a schema additively, and
  never to edit a migration that was applied.

The databases are not encrypted. SQLite has no encryption of its own: it
would take replacing the SQLite of the whole process with a build that has
(SQLite3 Multiple Ciphers was tried, and is maintained by one person for
.NET), and the key would sit in the platform's database on the same volume -
so it would protect a copy of `/data/databases` on its own and nothing more.
Protect the data volume and its backups instead.

### Secrets

API keys, passwords and tokens are data, never code: code and resources live in
every version, every export and every reader of the history, so a key put
there can never be taken back. A lambda reads a secret by name:

```csharp
var stripe = new System.Net.Http.HttpClient();
stripe.DefaultRequestHeaders.Authorization = new("Bearer", Secret.Read("STRIPE_KEY"));
```

`Secret.Read` throws, saying how to set it, when there is no such secret;
`Secret.Exists` answers whether there is one, and false while secrets are
switched off, for code that works without. `Secret` is there in every file,
like `Workspace`.

The rules are the usual ones for secrets:

- **Written, never read back.** A value goes in through the editor, the API
  (`PUT …/secrets/:name`) or MCP (`set_secret`), and nothing gives it out
  again - not the editor, not the API, not an agent. Only the lambda reads
  it. So a secret is replaced rather than edited, and whoever holds the editor
  link can use a key without being able to copy it. Answers repeat the name,
  never the value, and the log records the name only.
- **Off until switched on.** A lambda has no secrets until somebody switches
  them on - the owner in the editor, or an agent whose code needs one.
  Switching them off deletes every value, the copies of the features included.
- **Names are environment variables.** Letters, digits and underscores, not
  starting with a digit, case sensitive: an exported project reads
  `Secret.Read("STRIPE_KEY")` from `$STRIPE_KEY`. Up to 100 per lambda, 32 KB
  each - a private key fits, a file belongs in the workspace.
- **What the code waits for is said.** The names the code reads with
  `Secret.Read("…")` or `Secret.Exists("…")` as a literal - in the version
  online and in the newest one, or in a feature's files - are listed beside
  what is stored: a name read and not stored is *missing* (reading it fails),
  one only asked about with `Exists` is *optional*. The editor offers to set
  what is missing, the overview asks for it, and `read_lambda` and
  `list_secrets` tell an agent. That is how the two sides meet: an agent
  writes `Secret.Read("WEATHER_API_KEY")` and switches secrets on, and the
  owner enters the value without ever reading the code - and without the
  value passing through the agent.
- **Read at runtime, not compiled in.** The generated class is handed a
  function to read with once the lambda is loaded (a private field the code
  of the lambda cannot name, and reflection is refused), so a value changed in
  the editor is what the next call reads, without a deploy, and no value is
  ever written into an assembly on disk. What a lambda reads is held in memory
  sealed, per lambda and feature, and opened on each read.
- **Features get a copy.** Like the workspace, a feature starts with a copy of
  the lambda's secrets, its preview reads the copy, a value set in the feature
  (a sandbox key, say) stays there, and merging throws the copy away.

They are stored in the SQLite database, table `secrets`, sealed with AES-256-GCM.
The key is made with HKDF from two halves: the installation's, which is not in
the database - `LAMBDA_SECRETS_KEY` (32 random bytes in base64, or a passphrase
of at least 32 characters), or else a random key written on first use to
`secrets.key` in the data directory, readable by the server's user only - and
the lambda's own random salt (`lambdas.secret_salt`), made with its first
secret and dropped when its secrets are switched off. The name is bound to the
value as associated data. So the database alone opens nothing, a value copied
under another name or into another lambda does not open either, and a
feature's copy is copied sealed, without being opened.

Backing up and moving follow from that: the database and the installation's
key are all it takes. Copy the data volume as a whole - `secrets.key` goes
with it - or set `LAMBDA_SECRETS_KEY` on the new server to the key the
database was written with. Keep a copy of the key apart from database
backups; without it the secrets are gone, while everything else restores. The
server remembers a fingerprint of the key in `settings` and logs an error
when it is started with another one, and a secret that does not open says why.

None of this keeps a secret from the lambda it belongs to, or from code
running in the same process: the code guard is not a sandbox. The encryption
is about the database and its backups; the isolation between lambdas is what
the guard makes expensive, and no more than that.

The demo `demo-registration` peppers its password hashes with a
`PASSWORD_PEPPER` that the seeder gives it as a random secret nobody knows,
and a copy of it works without one (`Secret.Exists`). Its accounts remember
whether they were peppered, so accounts made before still sign in.

### Changing a lambda by asking

Where the installation runs the build agent, the editor has a **Change**
section: the owner says what should be different, and the same agent that
builds things on `/build` changes the lambda. It works in a feature - a new
one, or one the owner picks to go on with - reads the code and the history,
changes only what was asked with `change_code`, and tries it at the feature's
preview until it works. Then it merges the feature into the next version and
puts that online; switched off, it leaves the feature with its preview online
for the owner to try and merge. The next request defaults to the feature the
last one left open, so asking for a tweak after trying the preview goes on
with the same feature. It is told to leave every other feature alone.

The section shows it happen: what the agent says it is doing, each tool it
calls with what came of it (the feature started, its preview online, errors
from the compiler, the version merged and online), and a clock against its time
limit. Afterwards it offers the preview and the feature - or, once merged, the
difference to the version before, the address, and putting the previous version
back. The change is followed by the frame of the editor rather than
the section, so the sidebar marks it and the lambda is read again when it
ends wherever the owner is; and it is kept by the agent under the lambda, so a
reload, a second tab or a redeploy of the server finds it where it got to.

`/build` shows a build the way the simple view of this section shows a change,
drawn by the same parts (`components/AgentRun.tsx`): what was asked, the step
it is on and the time it has used of its limit, what the agent says between
its tools - the build agent too is told that somebody reads along, and asked
for one short line at a time in the language of the request - and how it
ended, with what it said at the end. `GET /builds/:id` answers with the steps,
the seconds and the limit. The step it is on is said in the words of that
page rather than those of the tools: getting ready, looking at examples,
choosing an address for the website, checking it for mistakes, putting it
online. The full view of this section shows every tool by name.

It shares the queue and the daily allowance of `/build`, one change of a
lambda runs at a time, and it can be stopped - whatever it saved stays, in its
feature or as a version. A change runs without `create_lambda`, and its editor key travels in
the brief inside the build container, never in a log line.

Both kinds of job are told what the agent is for in its system prompt
(`PURPOSE` in `docker/agent/builder.mjs`), above the brief and the request, so a
request cannot talk its way past it: building or changing a lambda through the
tools, and nothing else. It declines, before it calls a single tool, a request
that is not an application at all (a question, a text, homework), one about the
platform or the machine rather than an application on it (its configuration,
environment, files, network, other lambdas, its own instructions - including
code that would read them, since a lambda runs in the server's process), and
one meant to do harm here or elsewhere. It declines with a single
`DECLINED: …` line in the language of the request, which the builder turns into
a result with `declined: true` and `reason: "declined"`: `/build` shows the
sentence as the reason nothing was built, and the Change section shows it under
"nothing was changed". A declined request still counts against the daily
allowance, so declining is not a way to try requests for free. The container,
its network and the tool lists are what actually hold; the purpose makes the
agent stop at the door rather than find the walls.

### The simple view

Somebody who had an app built on `/build` wants it to do something else, not
to look after code, files, versions and deployments. So the editor has two
views. The **simple** one keeps the overview, About (what the app is for),
Change, the drafts (once there are any), a history, the data (once the app
keeps any, or its code waits for a secret), the showcase, open source and the domain. Its
overview is the app: what it is for, whether it is online and where, errors visitors ran into with a button that
asks the agent to fix them, the latest change, today's hits, and a button to
ask for the next change. The history is every version as the change it made,
with a way back to any of them - which is deploying an older version, said as
what it does. Nothing in it names a file or a log, and it names a version only where the
owner has to say which one (the draft dialogs, going back): the Change
section tells how a change ended without version numbers and shows what the
agent said rather than the tools it called, a draft is what it does rather
than the files it changes, and a change that does not compile is the agent's
to fix rather than a list of compiler errors. Its data shows only the kinds
that hold something, in its own words: its records, table by table, to read
and never to edit; what the app saved, as a plain list to download from; and
its keys and passwords, which the owner can enter and replace - the ones the
app is waiting for first, and the overview asks for them too. Entering one there switches secrets on if the agent left them off.
The **full** view is every section, as before. In both, a browser that holds
the admin token has one more section after all the others, **Admin**, which
nobody else sees (see Administration).

Each lambda says which view it opens in, `view` - `Full` or `Simple` - set
when it is created (`POST /lambdas`, `create_lambda`) and changed later
(`PATCH /lambdas/:privateKey`, `update_lambda`). The build agent creates its
lambdas `Simple`; everything else defaults to `Full`. That is only the
default: whoever switches at the foot of the sidebar (in the menu on a phone)
has chosen for themselves, for that lambda, which is kept in their browser and
never sent anywhere - so an operator looking at somebody's lambda in the full
view leaves it simple for its owner.

### Taking a lambda away

The code belongs to whoever made the lambda, so `GET …/export` (the editor's
Download) packs its newest version into an ordinary .NET 10 project that runs
without this platform (`Services/Deployment/ProjectPacker.cs`):

| File | What it is |
|---|---|
| `Program.cs` | the default GenHTTP host, `Host.Create().Handler(Project.Create()).Defaults().RunAsync()`, under a header naming the lambda, its version and change, the export date, where to read about GenHTTP, and the environment variables its secrets become (as `-e` flags in the `docker run` line too) |
| `Project.cs` | `lambda.cs`: its statements are the body of `Project.Create()` (`CreateAsync()` when they `await`), its types sit beside the class, made public as on the platform |
| `*.cs` | the other C# at the top, its code unchanged, the first letter of the name capitalized |
| `Platform/` | what the platform provided: `Workspace` and `Resources` as folders (with `Assets` beside it where the code still says that), `Secret` reading environment variables of the same name (the values are never exported), `Database` opening `database/database.db` where the code uses one, the switch that turns what `Project` returns into a handler, and the imports every lambda gets as global usings |
| `resources/` | its resources, copied beside the program on build |
| `docs/`, `tests/`, … | the rest of its code where it was - its documentation, its tests, what it is built from; the project compiles only the C# at the top (`EnableDefaultItems` is off), and the image takes only what it compiles and copies |
| `database/database.db` | the lambda's database, an ordinary SQLite file - its records go with it |
| `Dockerfile` | builds and runs it; the workspace is `/app/workspace`, the database folder is mounted at `/app/database` |

It references `GenHTTP.Full` - the internal engine, which runs wherever .NET
does - at the version this server runs, so it behaves as the lambda did, and
`Microsoft.Data.Sqlite`, `Microsoft.EntityFrameworkCore.Sqlite` and `Evolve`
only where the code uses them - Entity Framework where it has a `DbContext`,
at the release of the SQLite library for .NET 10. Of the data,
the database comes along - copied with SQLite's backup, so it is whole even
while the lambda writes to it, and any SQLite tool reads it - while the
workspace starts empty and the values of the secrets stay here. `database/` is
in `.gitignore` and `.dockerignore`: records are nobody's source, and the image
mounts them rather than carrying them. The archive is packed into a temporary
file and streamed from there, since a database may be as large as the tier
allows. A test builds the export of every demo with the .NET SDK, so it needs
the package feed.

### Publishing the source

An owner may publish the code of a lambda as open source: under **Open
source** in the editor, in both views, right after the showcase, or with the
`open_source` tool - and only with the editor key, like everything else about
a lambda. Nothing is published until they ask, and taking it down again is one
switch. Publishing asks for a license, MIT unless another is picked from a
short list - MIT, Apache 2.0, BSD 3-Clause, MPL 2.0, GPL 3.0 or later, AGPL
3.0 or later, the Unlicense (`SourceLicenses`) - and, optionally, the name the
license gives as the holder of the copyright; without one it names "the
authors of" the lambda, since there are no accounts to take a name from.

What is published is the program and what is written about it: **every
version**, as the export packs it - `Project.cs`, the rest of its code,
`resources/`, `Platform/`, the project file, the `Dockerfile` - with the
license in `LICENSE` and named in the header of `Program.cs`. The history says
what each version changed, in the line it was saved with. What is never
published:

- **The data.** No database - not even an empty one - no workspace, no value
  of a secret. `ProjectPacker.Publish` takes no database to pack, so there is
  nothing to leave in by mistake, and a test looks for a record, a saved file
  and a secret's value in every file of the download.
- **What only the owner may know.** Not the specification - what was asked for,
  in the owner's words - not the editor key, and nothing about who uses it: no
  traffic, no log, no visitor.

`/source` lists every published source, searched by its key, what its
documentation says it is and what the showcase says about it where it is
there, ordered by stars, by the newest version or by when it was published.
`/source/:publicKey` is one: what it is, its license, who holds it, where it
runs - its own domain while it has one - and four views of the version being
read: its **code**, a tree of the files marked by what each is to the lambda
with the file open beside it, coloured by highlight.js with the editor's
colours, and lines to link to (`#L12-L20`); its **documentation**, the pages
of `docs/` rendered as the editor renders them, pictures beside them
included; its **tests**, `tests/README.md` with the scripts and data beside
it; and its **changes**, every version newest first. The newest version is
read unless another is picked, which the address keeps (`?version=7`) from one
view to the next. Any version downloads as a zip. There are no issues, pull
requests or wikis, and no way to act on the lambda from there; somebody who
wants to build on it downloads it, and `platform_guide` tells an agent how a
download becomes a lambda again. A small notice says what a lambda is and
leads to `/build`. The pages are a part of the frontend of their own
(`src/Frontend/src/source`), in a frame of their own without the site's menu,
fetched with their words and highlighter only by whoever opens them, in every
language under the same addresses as every other public page. They are not
prerendered: the server names each after its lambda, describes it with the
first paragraph of its documentation, marks it up as `SoftwareSourceCode` for
search engines, and lists every published source in the sitemap in every
language. Since what a page shows is fetched from `/api/v1/sources/`,
`robots.txt` allows crawlers that part of the API (and the showcase pictures),
while it keeps them out of the rest of it and out of the zips - otherwise a
crawler that renders the page would find it named, and empty. A source that is
not published is not found, the same as a key nobody has.

A version is packed once, on the first request for it, and kept below
`/data/sources/{lambda}` - a cache and nothing more, since the versions are
what the source is. What it is packed with besides the version - the license,
the holder, the key, the build of the packer - is hashed into the name of the
file, so changing the license packs again and the old one goes. Packing reads
and compresses a whole version, so one version is packed once however many ask
for it at the same moment, two at a time for the whole server, and the cache
is held to `LAMBDA_SOURCE_CACHE_BYTES` by letting go of the projects read least
recently. The page shows that a version is being packed, and a download waits
for it before it starts. A file of a version is read out of its zip as it is
sent; one sent as it is (`…/raw/`) comes from the site's origin, so it is sent
as text or bytes - a picture as a picture - never sniffed, and under a
sandboxing content security policy, so a page a lambda ships cannot run here.

Anybody may star a source. A star is a `POST` with a ticket the page was handed
with the source, signed by the server and at least a second old, so a link, a
crawler or a script posting blind stars nothing. An address stars a source
once and changes only so many stars in ten minutes; which address starred what
is kept in memory as a keyed hash, never on disk, and forgotten on a restart.
The count is in the database (`sources.stars`) and survives taking the source
down and publishing it again.

The export of a lambda whose source is published carries the same `LICENSE`.
The demos are published under MIT by the installation, being there to be read
and built on; nobody else can change that, and their editor shows no Open
source section.

### Working with git

Every lambda is a git repository as well, so whoever works on it with their
own tools and agent clones it, changes it and pushes - and whoever reads a
published source clones every version of it. Both are served by the
[GenHTTP.Modules.Git][git-module] package from what the platform already
keeps: there is no repository on disk, only the commits the versions and the
features are.

```bash
git clone https://genhttp.dev/editor/<editor key>/my-app.git
cd my-app && dotnet run                     # the app on http://localhost:8080/
# change it, commit
git push -o deploy                          # the next version, online
```

The owner's address is the editor's with a name added, which only names the
folder a clone makes: any name answers, so a clone keeps working once the
public key changes. It holds the editor key like the editor's address does -
there are no accounts, and the key is what lets a push in. The published
source is `/source/<public key>.git`, read only, while it is published. Both
answer without the `.git`, and opened in a browser they lead to their page.
The editor offers the address under **Clone** - in the full view only, on the
overview and beside the code, and in a draft with its branch - and `/source`
in the **Code** menu beside the star, where the zip is too.

**What is in it.** The project the lambda is exported as, made for a
repository: `Project.cs` is the snippet as the body of `BuildAsync()` - the
method the platform runs it in - with its types beside the class, the other
`.cs` files at the top keep their code and are named the .NET way, and every
other file is where the lambda has it: the rest of its code in its folders,
its resources in `resources/`.
Around them are the platform's files: `Program.cs`, the project file,
`Platform/`, the `Dockerfile`, `.gitignore`, `.dockerignore`, `AGENTS.md` -
how an agent works in the repository, with the rules of the platform in git's
terms - `CLAUDE.md`, which points Claude Code to it, and `LICENSE` while the
source is published. Those are the same in every commit until the platform
changes them - nothing in them depends on the version, which is why the
project references SQLite, Entity Framework and Evolve whether the code uses
them or not, compiles the C# at the top and nothing else of the code, and
makes the snippet asynchronous whether it awaits or not - so a push that
leaves them alone is always right. The `.gitignore` at the root is the
platform's, and takes `build/` back in, which many ignore everywhere; what a
tool installs and builds in a folder of the code is kept out by that folder's
own. The data is never in it: no
database, no workspace, no secret. Nor is what only the owner may know - no
specification, no editor key - so the commits the owner reads are the commits
a published source shows.

**The history.** `main` is the newest version, each version a commit tagged
`v1`, `v2` and so on, its message the change line; a version pruned is gone
from the history too, which a clone is told is shallow from there. Each
feature is a branch, named once when it starts - after the branch that was
pushed, or after its name in the words git allows (`dark-mode`) - and kept
when it is renamed (`features.branch`); a save of the feature is a commit on
it, and moving its base past what it holds is a merge commit bringing that
version in. The commits the platform makes are made when the repository is
read, not when a version is saved, so saving stays what it was and a lambda
nobody clones costs nothing; each is made once from what it stands for - the
files, the time it was saved, its change - and kept below
`/data/git/{lambda}` with the files it holds, by the id git gives them, so it
is the same commit for everybody who reads it later, a newer platform
included. A commit whose code did not change keeps the `Project.cs` of the
one before it, so a class somebody pushed in a way of their own is not
written over by a save in the editor. The published source is the same
history without the features: their branches are not there, and their
commits are not found by id either.

**Pushing to main.** Every commit becomes the next version, in order, with
the origin `git`, the first line of its message as the change and the rest
as the specification. `main` only moves forward one commit after another - no
force push, no merge commit, no deletion - and a version saved meanwhile in
the editor or by an agent makes the push "fetch first". A push holding more
commits than the tier keeps versions is refused, asking for them to be
squashed. The newest is compiled first, the code guard included, and a push
that does not compile is refused - its problems come back with the file and
line in the repository, `Project.cs(14,9)`. `-o deploy` puts the newest
online. A feature whose branch is now in `main` is merged and goes, as
merging it would have done.

**Pushing a branch.** A new branch starts a feature from the newest version
among its commits, with a copy of the lambda's data; pushing it again
replaces its files, a force push included, and a branch that holds none of
the versions is refused. Every push puts its preview online - nothing a
visitor sees - and answers with its address. `-o merge` with a push merges
it as one version, `-o deploy` beside it puts that online, and deleting the
branch deletes the feature. A feature changed in the editor since the client
fetched refuses the push rather than losing that change.

**What is refused.** Any file, in any folder, is a file of the lambda's code
or its resources, so a push may add whatever the code is to keep - a
`README.md` at the root, the sources of a front end, an editor's settings. A
push that changes, adds or removes one of the platform's files, or adds a file
with a name a lambda cannot hold - spaces, `.lambda/`, `assets/` - is refused
and says which and how to undo it: changing the platform's would change
nothing of the lambda, and silently dropping the change would leave the commit
saying something the lambda does not. So is
an executable file or a link, a tag, `Project.cs` with something in `class
Project` besides `BuildAsync()`, and anything the services refuse a save -
names, sizes, a demo. Everything a push does goes through the services the
editor and the agents use, under the same rules.

**What it says.** A push is answered as it goes, in the lines git prints after
`remote:`: the versions or the feature it became, problems the compiler
found, what went online and where, and what to do next. They never hold the
editor key - whoever pushed has it, and whoever reads their build log should
not. Options travel only with a push that changes something, so a version
already pushed is deployed in the editor or with
`POST /api/v1/lambdas/:privateKey/deployment/start`, which the answer names.

**Lambdas from before.** A lambda's history was laid out as the project was
exported then - what it served in `assets/`, and the platform's files around
it saying so. Commits are never made again, so none of those changed; instead
the newest version gets a commit of its own on top, which moves `assets/` to
`resources/` and brings the platform's files up to date, and `main` is that
commit (`GitHistory.MoveAsync`, recorded as `moved` in the index). A version
saved afterwards is a commit on top of it, and a feature's branch gets the
same commit on top of its tip. A clone made before pulls it like any other
commit; a push built on a commit laid out the old way is refused, saying to
pull first.

**Threads and limits.** Reading a lambda for the first time reads every
version, so the commits are made on the pool, in the lambda's turn - a
semaphore, one request at a time per lambda. The server packs what a clone
asks for, and applies a push, while it writes its answer, so that answer is
written on the pool into a pipe the reactor copies to the connection
(`OffloadedContent`). A push is unpacked in memory, up to
`LAMBDA_GIT_MAX_PUSH_BYTES`, and only two are applied at once.

## How it is put together

A single .NET 11 project hosted by `GenHTTP.Full.Ioxide`, wired up in
[`Application.cs`](src/GenHTTP.Lambda/Application.cs) and divided into
services, a folder each below `Services/`. From outside its folder a service
is taken only by its interfaces - by the API resources, the MCP tools and the
other services alike - and the classes a service is made of stay in its
folder; `ServiceBoundaryTests` fails on a constructor or method elsewhere that
takes one of them. Where other services work through a lower layer of a
folder rather than the service the API calls, that layer has an interface of
its own (`IDatabaseVault`, `ISecretVault`, `IDomainRegistry`, `ILogBook`,
`IRunLog`).

- **Meta** (`Services/Meta`) - lambdas and their versions, the only component
  that speaks to the database. Its public surface is DTOs, mapped by hand.
  `MetaService` is what the API, MCP and the other services call. It finds
  the lambda, checks it may be changed, takes the lambda's turn for a change,
  and leaves the rest to units: `LambdaHosting` changes the key, the domain
  and the tier, `LambdaHistory` writes the versions, the stretches online and
  the events and keeps each to its bound, `LambdaLifetime` says when a lambda
  goes offline and away and runs the sweep that does it, `LambdaRemoval`
  removes a lambda with everything that hangs on it, and `LambdaDescriber`
  maps the DTOs. `LambdaGuard` (who may change a lambda - not a demo) and
  `VersionInput` (what a version may hold) are rules other services apply too.
- **Storage** (`Services/Storage`) - the code itself, on the file system, one
  file per version. Never in the database.
- **Data** (`Services/Data`) - which kinds of data a lambda keeps, switched on
  and off by its owner. Only the choice is stored: a lambda without a row for a
  kind has its default, so a kind added to `DataKinds` needs no migration to
  exist. Switching a kind off deletes what it held.
- **Workspace** (`Services/Workspace`) - the private directory of a lambda,
  reached from the editor. The same directory the generated `Workspace` class
  writes to from inside a lambda, under the same limits, so a file put there by
  hand behaves like one the lambda wrote itself. Only the room it takes is
  limited - in any number of files of any size - counted in blocks of 4 KB as
  the disk counts it, so an empty file or a folder is not free. It shares the
  room of the lambda's data with its database: the data's allowance is
  compiled into the lambda - so moving it to another tier builds it again on
  its next request - and the size of the database is asked as a file is
  written, through a function handed to it. `WorkspaceVault`
  (`IWorkspaceVault`) measures what a workspace takes, keeping the answer a
  few seconds, and is what the database asks in turn for the room it may
  grow to (`DatabaseVault.RoomOf`).
- **Secrets** (`Services/Secrets`) - the secrets of the lambdas, sealed in the
  database. `SecretCipher` holds the installation's key and seals and opens
  values, `SecretVault` stores them and is what a running lambda reads through
  and the other services keep in step with a lambda (`ISecretVault`), and
  `SecretService` is what the API and MCP call - by name, never by value.
- **Databases** (`Services/Databases`) - the database of each lambda, a SQLite
  file below `/data/databases/{lambda}`, and a copy per feature beside its
  files. `DatabaseVault` makes, copies, exports and deletes them and is what a
  running lambda connects through (`IDatabaseVault` for the other services),
  `ConnectionGuard` is the authorizer and the
  limits every connection it hands out carries, and `DatabaseService` is what
  the API and MCP read tables and rows with.
- **Features** (`Services/Features`) - changes worked on beside a lambda. A
  row per feature says what it is based on and whether its preview is online;
  its files (`files.json`), what its preview serves (`preview.json`), its copy
  of the workspace and its preview's resources live below
  `/data/features/{lambda}/{feature}`, so deleting a feature is deleting a
  folder. Merging goes through Meta, under the same lock every save takes.
- **Source** (`Services/Source`) - whether the owner published the code of a
  lambda and under which license, and its stars (`sources`); `SourceCache`
  packs a version into the project a visitor reads and downloads, once, below
  `/data/sources/{lambda}`, and `StarGuard` hands out the tickets a star is
  given with and remembers who starred what. Kept apart from Meta like the
  showcase: nothing about building a lambda reads or changes it.
- **Git** (`Services/Git`) - every lambda as a git repository (see
  [Working with git](#working-with-git)). `GitService` finds the lambda and
  takes its turn, `GitHistory` makes the commits the versions and features
  are, `GitPushes` makes versions and features of what is pushed, `GitStore`
  keeps the commits and their files below `/data/git/{lambda}`, and
  `ProjectTree` says what a lambda is as a tree and back. The door is
  `Api/Git/GitRoutes.cs`, beside the pages below `/editor/` and `/source/`.
- **Building** (`Services/Building`) - the jobs of the build agent: builds
  asked for on `/build` and changes asked for in the editor's Change section.
  `BuildService` decides what may be asked for and in which order that is
  checked, and leaves the rest to units of their own: `AgentClient` is the
  only thing that talks to the agent's runner and holds the shared secret,
  `BuildAllowance` counts the jobs of each address per day, `ModelGate` says
  who may have the second model, and `AgentInput` checks the prompt and the
  language a page sends.
- **Deployment** (`Services/Deployment`) - wraps a snippet in a method body,
  compiles it with Roslyn, loads the assembly and calls `PrepareAsync()` on
  the resulting handler. Compiled once, then cached - one handler for what a
  lambda has online, and one for the preview of each of its features. An
  assembly stays loaded for the life of the process, so a build is known by
  its compiled C#: a version that changes only resources or the rest of its
  code - documentation, tests - gets a new handler from the assembly already
  loaded rather than being compiled
  again. Lambdas are compiled against the metadata of the platform's
  assemblies, read once and kept - the metadata alone, not the whole files.
- **Execution** (`Services/Execution`) - an `IHandler`, not a web service: it
  looks up the handler for a request and runs it.
- **Protection** (`Services/Protection`) - concerns in front of execution that
  resolve the lambda, rate limit per client and cap concurrency. A lambda is
  resolved from memory (`Services/Meta/ResolutionCache.cs`), which every write
  to the database makes stale (`Data/DatabaseChanges.cs`), so serving a lambda
  does not query the database.

A concern that needs nothing but services is a dependent concern of GenHTTP
(`IDependentConcern`, added with `Dependent.Concern<T>()`): a singleton in the
container, resolved per request from the scope that the outermost concern of
the host opens. That is the line each request is logged with, the server's
telemetry, and in front of every lambda the throttle, the mark on what it
prints, its activity and the rate limit. The lookup of the lambda is built by
hand, once per route, because its locator is what tells the routes apart.
`Infrastructure/ServerRegistry.cs` holds the running server for the one thing
that prepares handlers outside a request: the deployment service, compiling
what the demo seeder, a deployment or a merge asks for. Whatever answers a
request reads `IRequest.Server`.

What is printed to the console - what a lambda prints, and what the engine
says about its reactors - is kept by `Services/Diagnostics/ConsoleCapture.cs`,
which the application owns: it puts a tee over the console, once per process,
and files what is printed outside any lambda into the book of every
application running, until that application is disposed of. The logger
writes its lines to the console it was given before the tee existed, and takes
the lock of `Console.Out` first - the console takes that lock itself under
every write, and in the other order a print and a log line waited for each
other for good.
- **Background** (`Services/Background`) - a small scheduler that undeploys
  free tier lambdas a day after their last deployment and removes them after
  30 days without a save.

The editor asks the compiler what the code means rather than guessing from how
it is spelled. `Services/Deployment/Compilation/SemanticClassifier.cs` says
what each name is, so a type, a method, a parameter and a local are coloured
apart, and `CompletionResolver.cs` answers what may be written where the caret
sits - including what follows a dot, which needs the type of what precedes it
and therefore needs a compiler. Both bind without emitting; the browser's own
grammar colours each keystroke in the meantime so nothing waits on the network.

Snippets are compiled against the GenHTTP module surface with its namespaces
already imported, so no `using` directives are needed. A guard
(`Compilation/CodeGuard.cs`) rejects code that reaches for the file system,
processes, reflection, sockets or the internals of this application before it
is ever compiled, and a lambda that does need files gets a directory of its
own under `/data/workspaces`.

It also refuses code that waits for a task: `.Result`, `.Wait()`,
`.GetAwaiter().GetResult()`, `Task.WaitAll`, `Task.WaitAny` and
`SemaphoreSlim.Wait()`, asked of the compiler so a `Result` of the code's own
is not mistaken for one. Requests run on one thread per core, and a task
finishes on the thread that would be waiting for it - so it never would, and
that core would serve nothing again. Agents are told to await instead, or to
call the synchronous method where there is one, as with the database.

On the ioxide engine requests run on its reactors, one thread per core. The
platform's database is SQLite, which answers synchronously, so the services,
resources and MCP tools are synchronous: plain methods, synchronous EF calls,
plain locks. What takes long - compiling and binding with Roslyn, packing,
copying a workspace or a database - is handed to the thread pool in one hop
(`Infrastructure/Offload.cs`) and the request resumes on its reactor
afterwards. `src/GenHTTP.Lambda/BannedSymbols.txt` refuses the asynchronous EF
calls, waiting for a task and `Task.Run` when the server is compiled, so every
hop to the pool goes through `Offload`; the two pieces of work started in the
background rather than awaited - the scheduler's jobs and seeding the demos -
say so where they start. CLAUDE.md has the rules.

Compiled lambdas run in the server process. The guard raises the cost of
misbehaving; it is not a sandbox, which is why the container runs unprivileged
and the seccomp profile is left alone.

## Configuration

Everything is read from the environment on startup, see
[`LambdaOptions`](src/GenHTTP.Lambda/Configuration/LambdaOptions.cs) - apart
from the limits of the tiers, which are the product's promises rather than the
server's configuration and are set in the administration panel while it runs
(see [Limits](#limits)).

| Variable                            | Default          | Meaning                                     |
|-------------------------------------|------------------|---------------------------------------------|
| `LAMBDA_PORT`                       | `8080`           | port of the root server                     |
| `LAMBDA_ENGINE`                     | `ioxide`         | `ioxide` or `kestrel` (see below)           |
| `LAMBDA_RECEIVE_QUEUE_ENTRIES`      | `4096`           | reads of 32 KB a connection may have waiting on io_uring |
| `LAMBDA_DATA_DIRECTORY`             | `./data`         | database, code and workspaces               |
| `LAMBDA_WEB_ROOT`                   | `./wwwroot`      | the built frontend                          |
| `LAMBDA_DEVELOPMENT`                | `false`          | verbose error pages and debug logging       |
| `LAMBDA_MAINTENANCE_INTERVAL_HOURS` | `0.25`           | how often expired lambdas are looked for    |
| `LAMBDA_SOURCE_CACHE_BYTES`         | `2147483648`     | what the published sources may take on disk, packed; the least recently read go first |
| `LAMBDA_GIT_MAX_PUSH_BYTES`         | `268435456`      | how large a `git push` may be; it is unpacked in memory |
| `LAMBDA_MAX_CONCURRENCY`            | `64`             | lambda requests executed at once            |
| `LAMBDA_EXECUTION_TIMEOUT_SECONDS`  | `15`             | before an invocation is aborted             |
| `LAMBDA_TELEMETRY_INTERVAL_SECONDS` | `30`             | how often a reading is taken                |
| `LAMBDA_TELEMETRY_SAMPLES`          | `2880`           | how many readings are kept                  |
| `LAMBDA_ADMIN_TOKEN`                | -                | enables the panel, and closes the figures   |
| `LAMBDA_LOG_HISTORY`                | `4000`           | log lines the panel can read back, to 10^6  |
| `LAMBDA_LOG_MEMORY_MB`              | derived          | what their text may cost; whichever runs out first |
| `LAMBDA_LOG_LAMBDA_OUTPUT`          | `true`           | keep what lambdas print, filed under them   |
| `LAMBDA_LOG_REPEAT_WINDOW_SECONDS`  | `10`             | fold identical lines; zero writes every one |
| `LAMBDA_LOG_CLIENT_ADDRESS`         | `true`           | record the address a request came from      |
| `LAMBDA_LOG_GEO`                    | `true`           | say which country a range is registered in  |
| `LAMBDA_LOG_GEO_REFRESH_HOURS`      | `168`            | how often the registry files are refetched  |
| `LAMBDA_LOG_GEO_PLACES`             | `true`           | also fetch the town and network databases   |
| `LAMBDA_LOG_MAX_LINES_PER_REQUEST`  | `200`            | before one request's output is cut off      |
| `LAMBDA_MCP_ORIGINS`                | -                | hosts a browser may use `/mcp` from         |
| `LAMBDA_SECRETS_KEY`                | `secrets.key`    | the installation's half of the key secrets are sealed with, see [Secrets](#secrets) |
| `LAMBDA_PUBLIC_URL`                 | -                | canonical address, enables the sitemap      |
| `LAMBDA_TLS_PORT`                   | `0`              | port for TLS, zero leaves it off            |
| `LAMBDA_CERTIFICATE`                | -                | PEM chain or PKCS#12 archive                |
| `LAMBDA_CERTIFICATE_KEY`            | -                | private key, for a PEM pair                 |
| `LAMBDA_CERTIFICATE_PASSWORD`       | -                | password of the PKCS#12 archive             |
| `LAMBDA_CERTIFICATE_DIRECTORY`      | -                | further certificates, one folder per name   |
| `LAMBDA_ACME_DIRECTORY`             | -                | web root an ACME client writes challenges to |

The io_uring engine is the default and the faster one, but container runtimes
block the syscall in their default seccomp profile - so the image defaults to
`LAMBDA_ENGINE=kestrel`. Set it back to `ioxide` where io_uring is available
and allowed.

```bash
docker compose -f docker-compose.yml -f docker-compose.ioxide.yml up -d
```

That override runs the server on io_uring under `seccomp-ioxide.json`, which is
the default profile of the runtime with `io_uring_setup`, `io_uring_enter` and
`io_uring_register` added and nothing else. Without it the engine stops at
`io_uring_setup failed: -1` before the first request. The hole is small but it
is real: io_uring reaches further into the kernel than ordinary sockets, and
this platform runs code written by strangers in the same process.

The engine does not slow a sender down while a request is read more slowly
than it arrives. It keeps what came in, and once `LAMBDA_RECEIVE_QUEUE_ENTRIES`
reads are waiting it drops the connection with `recv queue overflow` on stderr.
Its own default of 64 is two megabytes, which a JSON body of a few megabytes
outruns while it is being parsed - an upload of 24 MB was dropped even at
5 MB/s. The reads come out of the 4096 each reactor shares, and when those run
out it waits for them rather than dropping anybody - so the default here is
4096, at which a connection is only ever slowed down: 200 MB went through at
the full speed of the loopback, where 1024 dropped 100 MB sent at 50 MB/s. The
price is that one large upload can hold the reads of its reactor while it is
consumed, and the other connections on that reactor wait for them. Kestrel
slows the sender down and needs none of this.

A workspace file sent as it is, to `…/files/:path/content`, is written to the
disk as it arrives: 200 MB went through with the server growing by 160 MB,
where base64 in JSON costs several times the file. A connection dropped in the
middle of a body looks like a body that ended, so such an upload is checked
against its `Content-Length` and not kept unless all of it arrived - with 1024
reads, the partial file had been kept as if it were the whole.

### Redeploying

An installation that also runs the build agent is spread over three compose
files, and leaving one out does not fail loudly - it produces a server that
looks healthy while some part of it is switched off. Drop the agent overlay and
`/build` reports "no build agent"; drop the ioxide overlay and the engine
quietly falls back to Kestrel. So there is a script that knows the list and
checks the result:

```bash
sudo ./deploy.sh              # rebuild and restart what is on disk
sudo ./deploy.sh --pull       # fetch origin/main first, then do that
sudo ./deploy.sh --check      # verify the running server, change nothing
```

`--check` is safe at any time and is the quickest way to answer "is the build
agent actually up?". A deploy with nothing to change recreates nothing, so it
costs no downtime; when there is something to change it restarts the container,
which drops every open connection - including anyone using a hosted lambda at
that moment.

Linking it onto the path makes it available to anyone with sudo:

```bash
sudo ln -sf "$PWD/deploy.sh" /usr/local/bin/genhttp-deploy
```

## Telemetry

`/stats` shows what the process is doing, and `/api/v1/telemetry` is where it
comes from. A reading is taken every `LAMBDA_TELEMETRY_INTERVAL_SECONDS` and
`LAMBDA_TELEMETRY_SAMPLES` of them are kept, which is a day at the defaults.

The readings live in memory and go with the process. That suits what they are
for - a restart ends the run they were measuring - but it does mean a redeploy
starts the graph over.

It also lists what each lambda has served, busiest first. Only the public key
identifies one there - the editor key, the code and the visitors are not part
of it.

Both are for the operator only, behind the administration token (see
[Administration](#administration)); the owner of a lambda sees its own figures
in the editor, and nobody sees everybody's. What it is for is
the shape of the memory curve. A managed heap that climbs across gen 2
collections is a leak; committed bytes climbing while the managed heap stays
flat is the heap keeping pages it could return, which is not.

## TLS

The server terminates TLS itself, so nothing has to sit in front of it. Point
`LAMBDA_CERTIFICATE` at a certificate and set `LAMBDA_TLS_PORT`, and the plain
port starts answering with a redirect to the secure one.

```bash
LAMBDA_TLS_PORT=8443
LAMBDA_CERTIFICATE=/certs/fullchain.pem
LAMBDA_CERTIFICATE_KEY=/certs/privkey.pem
```

A PKCS#12 archive works just as well - leave the key empty and set
`LAMBDA_CERTIFICATE_PASSWORD` instead. The certificate is read again when the
files change, so a renewal is picked up without a restart.

### Shipping resources

A lambda is not only C#. What it reads and serves while it runs - pages,
scripts, styles, pictures, the database's migrations - are its resources, in
`resources/` of a version (see [Code and resources](#code-and-resources)):
served as they are, never compiled.

```
lambda.cs                the snippet
Types.cs                 compiled beside it
resources/index.html     a resource
resources/www/app.css    a resource, in a folder
resources/logo.png       a resource, sent as base64
```

The snippet reaches them through `Resources`, which is the other half of
`Workspace` - what the lambda shipped, against what it has written since:

```csharp
return Layout.Create()
             .Add("api", api)
             .Add(Resources.App());
```

`Resources.App()` is a single page application over them: `index.html` is the
shell and a path matching no file is answered with it. `Resources.Tree()` and
`Resources.Files()` are there for anything less opinionated, and content types
come from the extension; each takes a folder below `resources/`, as in
`Resources.App("web")`, and a deploy that names one the version does not have
is refused. `Resources.Files()` serves from the directory itself rather than
through a tree, which on the ioxide engine is its native file handler -
descriptors opened once, read off the ring, never walked again, since the
resources of a build do not change while it is served. The workspace is
written while the lambda runs, so `Workspace.Files()` stays a tree. `Assets`
is the same as `Resources`, under the name it had before, so the code of a
lambda saved then compiles as it did.

The directory is rewritten from the version being deployed, so a resource
dropped from a version stops being served rather than lingering. What a
version may come to, its code and its resources together, is a
[limit](#limits) of the lambda's tier. How many files there are is not
limited, and neither is the number of C# files.

Budget the disk for the premium tier. A version is kept whole, its files as
text or base64 in JSON, so each version saved at the limit takes up to half as
much again on disk - 184 MB for 128 MB of pictures - and fifty of them are
kept by default: at the defaults, a premium lambda that is saved over and over
at its limit can come to around 9 GB of history.

Budget the memory too. A version is read and written whole: with the 128 MB
the premium tier allows, saving one took the server to 2.3 GB and deploying it
to 2.8 GB. Even the 32 MB of a free lambda took a server idling at 350 MB to
950 MB to save, and anybody can create a free lambda. Set what a version may
come to, for both tiers, in the panel's **Limits** to what the machine can
carry until resources are stored apart from the versions.

### More than one hostname

A certificate is only good for the names it carries, so a server answering to
several needs one per name. `LAMBDA_CERTIFICATE_DIRECTORY` points at a folder
holding the rest, a subdirectory per name in the layout an ACME client already
keeps them in:

```
/certs/fullchain.pem              the default, for anything unrecognised
/certs/privkey.pem
/certs/example.com/fullchain.pem  presented to clients asking for example.com
/certs/example.com/privkey.pem
```

The names come from the certificates themselves rather than the folder names,
including wildcards, and a client asking for a name none of them covers is
answered with the default one.

Note that this needs a PEM pair per name when running on the io_uring engine.
Kestrel terminates TLS in .NET and is handed a loaded certificate, but the
io_uring engine reads the files itself and has no way to be given an archive.

The issuers in the file are published into the certificate store of the user
the server runs as, because a client needs the chain and not just the leaf to
reach a root. Kestrel papers over a missing chain by fetching the issuer over
the network mid-handshake; the io_uring engine sends what it was given, so a
server that never published its issuers answers it with a certificate nobody
can verify.

There is no ACME client built in. With certbot, the private key stays readable
by root only while the server runs unprivileged, so a deploy hook publishes the
renewed files where the container can read them:

```bash
certbot certonly --standalone -d your.host.name

install -m 0644 -o root -g 1001 /etc/letsencrypt/live/your.host.name/fullchain.pem /opt/genhttp-lambda/certs/
install -m 0640 -o root -g 1001 /etc/letsencrypt/live/your.host.name/privkey.pem   /opt/genhttp-lambda/certs/
```

Because the server holds port 80, the standalone authenticator needs it back
for the few seconds a renewal takes - a pre hook stops the container and a post
hook starts it again.

### Issuing certificates while it runs

The server can answer the challenges itself instead. Point
`LAMBDA_ACME_DIRECTORY` at the folder the ACME client writes them into, and
`GET /.well-known/acme-challenge/{token}` is served from
`{folder}/.well-known/acme-challenge/{token}` - for every host the server
receives, a lambda's own domain included, where the path would otherwise be
the lambda's. Nothing else in the folder is served.

```bash
mkdir -p /opt/genhttp-lambda/acme     # mounted read only at /acme, see docker-compose.yml

certbot certonly --webroot -w /opt/genhttp-lambda/acme -d your.host.name
```

The challenge is asked for over plain HTTP, and it is the one request the
upgrade to HTTPS lets through rather than redirecting.

### Custom domains

A lambda in the premium tier can answer at a domain of its own, which its owner
sets in the editor after pointing the domain's A and AAAA records at the
server. Plain requests to it are redirected to HTTPS on the same domain like
every other request, so it needs a certificate - issued by hand for now, with
the web root above, into a folder of its own:

```bash
certbot certonly --webroot -w /opt/genhttp-lambda/acme -d shop.example.com

mkdir -p /opt/genhttp-lambda/certs/shop.example.com
install -m 0644 -o root -g 1001 /etc/letsencrypt/live/shop.example.com/fullchain.pem /opt/genhttp-lambda/certs/shop.example.com/
install -m 0640 -o root -g 1001 /etc/letsencrypt/live/shop.example.com/privkey.pem   /opt/genhttp-lambda/certs/shop.example.com/
```

The folder is only looked through on startup - the io_uring engine learns the
names it holds certificates for when it opens the TLS port - so a new
certificate is served after the next restart of the container. Renewals of a
certificate the server knows are picked up without one. Until the certificate is there, visitors of the
domain are redirected to HTTPS and shown the default certificate, which does
not carry their name.

## For agents

There is a Model Context Protocol endpoint at `/mcp`, so an agent can build
something here and put it online without a person driving the editor. It is
JSON-RPC over a single path, taking POST; a GET is declined with 405, because
this server answers rather than streams.

```
https://genhttp.dev/mcp
```

The tools are the shape of the job: `create_lambda`, `update_lambda`, `write_code`, `change_code`,
`check_code`, `deploy`, `read_lambda`, `read_logs`, the feature tools
`create_feature`, `update_feature`, `merge_feature` and `delete_feature`, the
data tools `upload_file`, `list_files`, `delete_file`, `enable_data`,
`read_database`, `set_secret`, `list_secrets` and `delete_secret`, `showcase` for listing
a lambda on the public showcase and `open_source` for publishing its code -
only when its owner asks for either - and
`list_demos` for reading something that already works. `platform_guide` is the one to call first
- it opens with how versions, features and data live, then says what a snippet
has to return, what is imported, what is refused, and the handful of things
that catch people out.

Agents used to leave a version behind for every attempt, and put every attempt
online. A lambda that exists is now changed in a feature: `write_code`,
`change_code`, `deploy`, `read_lambda`, `read_logs` and the data tools take an
optional `feature` and then act on the feature - its files, its preview, its
copy of the data - instead of the lambda, so the same few tools do both. The
answers name the feature, its `previewUrl` and whether it is `mergeable`, and
never carry `onlineUntil`, which only a deployment of the lambda does.
`merge_feature` refuses a feature that is behind with how to get it there;
`read_lambda` with `feature` lists the `newerVersions` it would have to take
in. The same rules are said wherever an agent decides something - in the
instructions it reads on connecting, in the tool descriptions and answers, and
in the guide: a new lambda is written as versions, one that exists is changed
in a feature, and the front end goes in the version, records in the database
and uploaded files in the workspace, never the other way round - and an API key
in the secrets, never in the code. Agents are steered to the database for
anything they would otherwise keep in a JSON file: switched on with
`enable_data`, its schema as Evolve migrations in `resources/migrations/`, a `DbContext`
of Entity Framework Core on a connection per request, synchronously - and `read_database`
to see that it worked. `read_lambda` also lists the open features, says which
data the lambda has switched on, and names the secrets it keeps and the ones
its code reads that are missing. An agent is steered to leave the value to the owner,
who enters it in the editor where the value never passes through the agent;
`set_secret` is for a value the user handed it, and its answer never repeats
it. The build agent may switch secrets on and list them, and has no tool to
set or remove one.

`write_code` takes an optional `specification` (what the user wants and why) and `change` (one
line on what the version does). `change_code` changes only the files it names,
or a passage within one, and either takes `deploy: true` or `check: true` -
the latter compiles what it saved and answers with the diagnostics, without
sending every file again the way `check_code` needs. A deployment that is
refused names the version it refused and the one that is `stillOnline`. They are kept with the version and shown next
to its diff in the control center, and `read_lambda` hands the recent history
back so the next agent can read why before it changes anything. `read_logs`
lets an agent see how what it deployed is answering, stack traces included.

Every version keeps its documentation and tests in `docs/` and `tests/` of its
code (see [Documentation and tests](#documentation-and-tests)), and the agents
are the ones who write them: `read_lambda` hands them over first, and every
save that leaves a page out says which. How a version is laid out - its code,
the C# at the top compiled and the rest kept, and its resources in
`resources/` - is said in a line of the instructions, under `code` and
`resources` in the guide, and in the tool descriptions.

A page meant to be found or shared - a website, a shop, a landing page - is
given a title, a description, its language, an icon and a social preview,
and says what it is in the HTML it is served with rather than only in what a
script renders, since crawlers and agents mostly run none. The instructions say
so in a line, `platform_guide` says how under `beingFound`, and the build
agent's brief says the same; a tool for a few people needs a title and nothing
more. `og:image` takes a full address - social networks do not resolve a
relative one - so the warning a feature's deploy gives for a link to
`/lambda/{publicKey}/` leaves meta tags and the canonical link out: they name
the page, and the page never follows them. A lambda with a domain of its own
names the domain as canonical, so search engines list it rather than
`/lambda/{publicKey}/`.

A page that shows what changes while it is open - what others do, a count, a
feed, a game - is pushed the change by the server, with server-sent events when
it only listens (`demo-live`) or a websocket when it talks back (`demo-game`).
Agents are told never to poll, fetching on a timer to see whether anything
changed: a poll is a request whether it did or not, on a server every lambda
shares. The instructions say so in a line, `platform_guide` under
`liveUpdates`, and the build agent's brief the same.

Agents are asked - not required - to put a small "Made with GenHTTP Lambda"
line at the foot of the pages they build, linking to `LAMBDA_PUBLIC_URL` (or
the address the agent called, where it is not set). The link's words are the
name and nothing else. On a lambda with a domain of its own the line is plain
text: there the link would come from another site, and the same link in the
footers of many sites is what search engines count as link spam - below
`/lambda/` it is a link within the site. It is the user's to refuse: the agent says that it added
it, leaves it out or takes it out when asked, and notes in `decisions.md` that
it was not wanted, so the next agent does not put it back. A change does not
add one to a lambda that has none. The instructions say so in a line,
`platform_guide` under `backlink`, and the build agent's brief the same.

`open_source` publishes a lambda's code, changes its license or takes it down;
with only the editor key it says how things are and changes nothing. It is not
part of building, and its description says so: only when the user asks, and
asking which license when they did not say. Once a lambda is published,
`read_lambda` says so in `openSource`, with a note that everything saved from
then on is public, the versions before included - so an agent keeps keys,
passwords and personal data out of the files, where they never belong anyway.

What a build tool makes the resources or code from is kept in a folder of the
code (see [What it is built from](#what-it-is-built-from)), and the agents are
told so under `code` in the guide (`builtWithATool`) - the flow, building
where the files are laid out as in a clone, a `.gitignore`, a README that says
how it is built, nothing secret in it, and the pitfalls (what a build writes
refers to its files relatively; what it wrote anew is saved with it) - in
`AGENTS.md` of a clone, and in the tool descriptions. `write_code`, which
replaces every file, says so when what it replaced had a folder of code the
save has none of - `change_code` keeps every file it is not told about.

An agent that can run git works in a clone instead (see
[Working with git](#working-with-git)): `create_lambda` and `read_lambda`
answer with the `gitUrl`, features name their `branch`, the instructions say
so in a line and `platform_guide` how under `git`. In the clone, `AGENTS.md` is
its guide: what is what in the repository, how pushing to `main` and to a
branch works and what is refused, and the rules of the platform said in git's
terms - the same rules, worded the same way, as the instructions, the guide
and the build agent's brief.

`read_lambda` sends every file of the program while together they come to 30,000
characters, and names them with their lengths beyond that - the C# and the
resources first, then the rest of the code, and after two hundred files named
only how many more there are; `file` then fetches one in full,
up to a megabyte, and anything larger is left to the zip of the version - a
hundred megabytes of base64 is nothing an agent can read. It also
says which tier the lambda is in and what that allows, and `platform_guide`
lays out both tiers. The guide, `write_code` and `upload_file` also steer a
large file that is data rather than program - a model, a dataset, media - into
the workspace, where it is kept once instead of in every version, and a
refusal for a version over the limit says the same.

### Demos

The installation keeps a handful of finished lambdas online in the `Demo` tier,
each showing one way to build something: `demo-crud` (a REST API over records in
its database), `demo-registration` (accounts, login and a members page),
`demo-game` (a websocket game), `demo-files` (uploads in the workspace, what is
known about them in the database) and `demo-live` (server-sent events). Each
keeps its records in its database, with its schema as Evolve migrations in
`resources/migrations/`, and the seeder switches it on; a lambda created from one starts
with a database of its own. Their editor key is their public key, and it is meant to
be announced: `read_lambda`, `list_files` and `read_logs` work on a demo exactly
as on an agent's own lambda, and the editor at `/editor/demo-crud` shows it.

Everything that would change a demo - saving, deploying, stopping, moving,
deleting, the workspace, the showcase, its license - is refused by its tier,
whichever way the request comes in. Their code is published on `/source` under
MIT by the seeder, so anybody can read and download a demo there as well. `create_lambda` with a demo's id as its template starts a
lambda of one's own from a copy. Keys starting with `demo-` cannot be claimed.

The demos are seeded in the background after startup from the hidden templates
of the same name in `Resources/Templates`, redeployed when their template
changes, and retired when they leave `DemoCatalog`. Nobody can move a lambda
into the tier or out of it, the operator included.

Nothing is created until `acceptTerms` is true, and the editor key that comes
back is the only way into what was made. There is no session and nothing is
remembered between calls: everything a call needs is in its arguments.

`LAMBDA_MCP_ORIGINS` lists the hosts a browser may drive the endpoint from. An
agent speaking HTTP sends no `Origin` and is never checked against it; the list
is there because a browser does, and a page on another site could otherwise aim
somebody's browser at this endpoint.

## Administration

`/admin` lists every lambda, when it was created, whether it is online, and
what its code says - and takes them offline or removes them. `/stats` is what
the process is holding and what the engine is carrying. Both are reached
through the **Admin** menu in the header, which is where the token is entered:
it is kept in the browser's local storage, so every tab of that browser is
unlocked - the editor's **Admin** section included, see below - until **Lock
again** is used or the server turns the token away.

This is the one part that authenticates. Everywhere else the editor link is
the credential and it only ever reaches one lambda; this reads code that belongs
to other people and can take their work away, so it asks for `LAMBDA_ADMIN_TOKEN`
in an `X-Admin-Token` header (GenHTTP's API key authentication). A request
without the header is answered with 401, one with the wrong token with 403.
Until that variable is set there is no panel at all: `/admin`, `/telemetry`
and `/logs` are not there, and answer 404.

The server figures and the log follow the same token. Only owners and the
operator see telemetry: the owner of a lambda its own, in the editor, and the
operator everybody's.

### Limits

What a lambda may have in its tier is set in the panel's **Limits** section
(`GET / PUT /admin/limits`), not by environment variables: these are the
product's promises, and changing one should not take a restart. One form: the
limits of the tiers as a table with a column per tier, and the limits that have
no tier in a block of their own below it.

| Limit             | Free     | Premium  | What it bounds                                   |
|-------------------|----------|----------|--------------------------------------------------|
| Build             | 32 MB    | 128 MB   | a version: its code - the C#, and everything kept with it - and its resources, together |
| Data              | 512 MB   | 4 GB     | what a lambda keeps: its database and its workspace, together |
| Versions kept     | 50       | 50       | older ones are removed, never the one online     |
| Features open     | 10       | 10       | each holds a copy of the workspace and database  |

A free lambda also goes offline after 720 hours without visits or edits and is
removed after 2160; a premium one stays online and is kept. Three limits are
counted per caller rather than per lambda and have no tier: the size of a
showcase picture (3 MB), requests per second and client to the lambdas (250),
and builds and changes per address and day from the build agent (10).

They are kept in the `settings` table beside the panel's switches, a row each,
and held in memory. A missing row is the default - the table above, or what
the environment variable that used to set it says. Saving writes every value,
refuses one that is not greater than zero and a premium tier that allows less
than the free one, and logs a line per limit that changed
(`Changed limit limits.free.build-bytes from 33554432 to 16777216 by operator`).

A change applies to what is checked next - a save, an upload, a deploy, a new
feature, a connection to a database - and takes nothing away: a lambda already
over a lowered limit keeps what it has and cannot save a version until it fits;
a database over its new size keeps its records and grows no further. The
data's allowance is compiled into a lambda's workspace, so changing it compiles
every lambda of that tier again on its next request.

There were four of the two once: the characters of C# apart from the bytes of
assets, and the room of the workspace apart from the database's. A value an
operator saved for one of them is carried over as the sum of the pair - the
other counting as its old default - and the old rows are removed with the next
save. The variables below are carried over the same way.

The variables that used to set them - `LAMBDA_MAX_CODE_LENGTH`,
`LAMBDA_MAX_ASSET_BYTES`, `LAMBDA_WORKSPACE_BYTES`, `LAMBDA_DATABASE_BYTES` and
their `LAMBDA_PREMIUM_` twins, `LAMBDA_DEPLOYMENT_LIFETIME_HOURS`,
`LAMBDA_RETENTION_HOURS`, `LAMBDA_MAX_VERSIONS`, `LAMBDA_MAX_FEATURES`,
`LAMBDA_MAX_SHOWCASE_IMAGE_BYTES`, `LAMBDA_RATE_LIMIT` and
`LAMBDA_AGENT_BUILDS_PER_DAY` - still give the defaults for this release, and
the server warns on startup for each that is set. A value saved in the panel
wins. They are removed in the next release.

`/logs` is the tail of this run, live. It holds everything the server logged
and everything a lambda printed while it was serving a request - the two are
told apart because the console is shared but the attribution is not: the
concern that serves a lambda marks the request, the mark travels with it
through every await into the code of the user, and the writer over the console
reads it back to decide whose line it is. Constructing a lambda is marked the
same way, so a print at the top of a snippet is filed under it as well.

Identical lines are folded before they are ever written: the same request from
the same caller answered the same way is counted rather than repeated, and one
line every `LAMBDA_LOG_REPEAT_WINDOW_SECONDS` says how many there were. What
the window bounds is how stale a count may be, not how many are folded into
it - and a line already read never changes underneath the reader, which is what
lets it work with a cursor that only moves forwards. A run that stops has its
last count written when somebody next reads, because otherwise the tail of a
burst would sit counted and unseen.

**Fold repeats** in the panel does the same thing to what is already loaded,
turning the list into one row per distinct line with a count on it, which is what makes a log worth reading when something is repeating: a
scanner knocking on five paths every two seconds is five rows rather than
fifteen hundred. Each survivor sits where it last happened rather than where it
started, because a log is read from the bottom. The same line from two
different callers stays two rows - which of them is doing it is usually the
question.

**Callers** lists everyone the log still holds something about - address, where
from, how many requests, how many failed, first and last seen - over the whole
ring rather than over the page on screen, because who is out there is a
question about the run and not about the last screenful.

The effect is that one lambda can be read on its own, which is what the **Log**
button beside each row in `/admin` does, and so can one caller - clicking an
address narrows to it. The find box takes bare words that must all appear,
`!word` for one that must not, and `"a phrase"` to keep it together; it matches
the text, the source, the lambda and the caller, so `!Requests` leaves only
what the server and the lambdas said. A warning the server logs *about* a
lambda - the one the error handler writes when it throws - is filed under that
lambda too, with the stack trace that was kept from the visitor.

Besides the request line, every call to the API that changes something, and
every MCP tool an agent calls, logs what it did, verb first and tersely:
`Published source of lambda quiz license MIT`, `Merged feature 'Dark mode' of
lambda quiz as version 4`, `Set secret SHOP_KEY of lambda quiz`,
`Failed to deploy lambda quiz version 5`. The source says which door it came
through - a `…Resource` for the API, `McpTools` for an agent - and the line
carries the caller like any other. Each value is a property of its own with the
same name in every line - `{Lambda}` the public key, `{Feature}` the feature's
name, `{Version}`, `{Path}` - so the find box and a structured sink can both
pick them out; the services below the doors know a lambda by its id and write
`lambda #12`. A lambda is named by its public key and a feature by its name,
never by the editor key or the feature's key; a secret by its name, never its
value; and code, file contents, pictures, specifications and descriptions are
left out - which is also why no line logs a model as a whole. The one exception
is what people ask the build agent for: the prompt of a build on `/build`, and
of a change asked for in the editor, is logged in full, because it is what tells
the operator what this platform is being used for. Reads through the API are
not logged, apart from exports and downloads - the editor reads all the time,
and those lines would bury the rest; an agent's reads are
(`Read lambda quiz version 3`), since what it looks at is how it goes about the
job. A refused request is logged by its request line alone. `OperationLog`
holds the helpers and says why.

Every line says which protocol it arrived over, which country the address is
registered in, and which address it was being written for. That is what turns a
path being hit five hundred times a minute from a mystery into a question, and
it applies to more than the request line: a lambda's print and an error the
server logged both carry the caller, because the request is marked once at the
outside and everything below reads the mark back. A request that arrived
through a proxy is recorded as the address it claimed *and* the hop it came
from, since the forwarded header is written by whoever sent it - taking it at
face value would let anyone write any address into this log. Addresses and
user agents are pooled, so a million lines from a few dozen callers is a few
dozen strings rather than a million. It is personal data, it is only ever
served behind the token, and `LAMBDA_LOG_CLIENT_ADDRESS=false` leaves every
line in place with nothing personal on it.

The country comes from the delegation files the five regional registries
publish - the same records that say who was given which block - fetched weekly
and cached on the data volume, so a server that restarts while they are
unreachable still knows what it knew last week and one that has never reached
them simply shows no countries. Nothing is asked of a third party, because
doing this by lookup would mean handing somebody else the address of every
visitor to the installation.

On top of that country, `LAMBDA_LOG_GEO_PLACES` adds a town and the name of
the network an address is on - "Aveiro, PT · MEO" rather than "PT" - from the
free DB-IP databases. They are about 130 MB on the data volume, refetched
monthly, and cost nothing in memory: the format is a search trie and the files
are memory mapped, so what they use is page cache the kernel can drop rather
than heap that has to be paid for. Lookups are remembered by address, because
a scanner is one address and five hundred requests: measured, a fresh address
costs about thirty-five microseconds and six hundred bytes, one already seen
costs seven hundredths of a microsecond and nothing. So the price is paid per
caller rather than per request - a thousand requests from fifty callers is two
microseconds each, and from a thousand callers is thirty-five.

Read it as the registration of a range and not the location of a person. Two
results from this machine make the point: its IPv4 address is registered in
Austria and its IPv6 address in Germany, and `1.1.1.1` - a resolver that
answers from everywhere at once - reads as Australia, because that is where
the block is registered. It tells a Portuguese visitor from a Singaporean
scanner. It does not put anybody on a map, and `LAMBDA_LOG_GEO=false` stops it
downloading anything at all.

The town is a guess rather than a record - measured and inferred by a third
party, right about most consumer connections and wrong about most
infrastructure - so it is shown as one and never as a position. Country and
town data by [DB-IP](https://db-ip.com) under CC BY 4.0, which the panel
credits; the registry data is published by the RIRs themselves.

Three bounds keep it honest. The ring holds `LAMBDA_LOG_HISTORY` lines, up to
a million, with a cap on the length of each; `LAMBDA_LOG_MEMORY_MB` caps what
their text may cost, and whichever runs out first decides, so a lambda in a
print loop costs a fixed amount of memory rather than a growing one and long
lines simply mean fewer of them; and one request may only contribute
`LAMBDA_LOG_MAX_LINES_PER_REQUEST` before the rest is counted and dropped, so
it evicts its own output rather than everybody else's. A million ordinary
request lines measured at about 220 MB, which is the number to budget against.
Nothing here survives a restart. The container's stdout still has the whole run and is
what to read when the question is about something that happened before the
process started - reading the log through the panel is a convenience, not the
record.

The panel also says how the run before this one ended, where it did not end
cleanly. The server cannot answer "did it just crash" about itself - whatever
it would have said went down with it - so each run leaves a note on the data
volume, refreshed on the telemetry tick, and the next one reads it. A note with
no stop stamped on it is a process that went away between one heartbeat and the
next, and the memory, socket count and request count it was carrying at that
beat are usually most of the answer. Being asked to stop is stamped as the
signal arrives rather than once the stopping is done, so a run that was told to
go and died partway can be told from one that nothing asked at all. An
unhandled exception is written into the note before the process goes down,
because in a container that restarts immediately, stderr is easy to lose.

A note says that a run died, not where. A native crash leaves .NET no chance to
write anything - heap corruption is one line from glibc and an abort - so the
ioxide overlay also has the runtime dump the process as it dies:
`/data/dumps/crash.dmp`, with every thread's stack and the heaps, for
`dotnet-dump analyze`, and `crash.dmp.crashreport.json` beside it with the same
stacks as JSON, readable without a debugger. The name is fixed, so each crash
replaces the last: a lambda can take the process down with unbounded recursion,
and a dump per crash would let it fill the disk. A dump is the whole memory of
the process - keys, tokens and the log with every caller's address - so copy it
off, read it, and delete it. The core the kernel writes as well stays inside the
container at `/app/core.1` until the next deploy, and a debugger cannot show a
managed stack from it.

`LAMBDA_LOG_LAMBDA_OUTPUT=false` turns off the gathering of what lambdas
print, for an installation that would rather not hold a stranger's output in
memory at all. Their console still reaches stdout; it is simply not collected.
The route is behind the token without the exception the figures get: aggregates
name nobody, and this is whatever somebody's code decided to print.

The listing links the editor of each lambda, which is the whole of the editor
key and therefore the whole of the credential. That is a deliberate trade: an
administrator who has to decide whether something is abusive needs to read it,
and reading it in the editor is also how it gets emptied or corrected rather
than only deleted. It is one more reason the token belongs to a person and not
in a browser somebody else uses.

Whoever holds the token also decides which lambdas the sitemap names. In a
browser that holds it, the editor of every lambda has one more section,
**Admin**, after all of the owner's in both views; nobody else sees it, and the
server asks for the token again behind it (`PUT /api/v1/admin/lambdas/:publicKey/sitemap`
with `{ "listed": true }`, and `sitemap` in what `GET /api/v1/admin/lambdas/:publicKey`
answers). Its switch, off for every lambda until the operator turns it on, has
`/sitemap.xml` name the root of the lambda's address - `https://genhttp.dev/lambda/quiz/` -
for as long as the lambda is online. An offline lambda answers its address
with a 404, which is no use to a crawler, so it is left out until it is back.
Never its own domain, even where it answers at one, since a sitemap names
pages of its own host; and without a `lastmod`, since what a lambda serves
changes with its data as well as its versions, and a date that is sometimes
wrong teaches a search engine to ignore the others. Neither an owner nor an
agent can list a lambda. The section is in English, like the panel, and a
token the server turns away is forgotten there as the panel forgets it, which
takes the section with it.

## Database

SQLite through EF Core, migrated on startup by [Evolve][evolve] from the SQL
files in `Data/Migrations`. To change the schema, add the next
`V<n>__<name>.sql` next to them - they are embedded resources, and the
existing ones are never edited.

[genhttp]: https://genhttp.org/
[evolve]: https://evolve-db.netlify.app/
[efcore]: https://learn.microsoft.com/ef/core/
[git-module]: https://github.com/Kaliumhexacyanoferrat/virtual-git-server
