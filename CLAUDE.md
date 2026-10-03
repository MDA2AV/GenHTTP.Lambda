# GenHTTP Lambda - steering file for agents

This file holds the product and architectural decisions of this repository.
Follow it, and when a decision is made or changed - by the owner in a
conversation, in a review, anywhere - **put it here in the same change**. A rule
that only lives in a chat is lost. The [README](README.md) has the reference
(routes, API, configuration, operations); this file has the *why* and the rules.

`docker/agent/AGENTS.md` is something else: it is the brief of the build agent
inside its throwaway container, not a steering file for people working on this
repository. Do not merge the two.

## What the build agent is for

The build agent (`/build` and the editor's Change section) builds or changes a
lambda through the platform's tools, and nothing else. That is its system
prompt (`PURPOSE` in `docker/agent/builder.mjs`), kept apart from the brief
because the brief carries a request typed by anybody. Before calling any tool
it declines, with one `DECLINED: …` line in the language of the request:

- anything that is not an application - questions, texts, homework, chat;
- anything about the platform or the machine rather than an application on it,
  including code that would read the server's environment, files or network;
- anything meant to do harm, to this server or to others.

The builder reports it as `declined` (`reason: "declined"`), and the pages
show the agent's sentence. Ordinary applications - ones that call public APIs,
keep data, have accounts, or merely sound alarming - are not declined. Keep
`PURPOSE`, `AGENTS.md` ("What not to build") and the README saying the same.

## What this is

A platform on which somebody describes an app - or an agent writes a C# snippet
that returns a GenHTTP `IHandler` - and gets a public URL that serves it. The
snippet is compiled with Roslyn at runtime and hosted by the same GenHTTP
server (`GenHTTP.Full.Ioxide`, .NET 11) that serves this application.

## The number one rule: asymmetric interfaces

This is a platform for **agentic coding**. We do not assume that every actor
works in the same tools with the same workflow. Instead:

- **Agents get the best interface for producing and changing functionality**:
  the MCP tools, the REST API, `platform_guide`, the demos, small targeted
  changes (`change_code`), compile checks, logs.
- **Humans get the best interface for controlling and reviewing what was
  generated**: the editor, the history with the change and the specification of
  every version, diffs, previews, a way back, the dashboard.

Every design decision is measured against this before anything else. Do not
build a symmetric view where a human and an agent use the same screen for the
same job. If a feature makes the agent's job harder to make the human's
easier, or the reverse, find the interface that serves each of them.

## Two target groups

| | No-code "vibers" | Developers |
|---|---|---|
| Who | Have an app built and want it changed; do not write code | Have their own coding agent and code |
| Entry | `/build`, the **Change** section of the editor | `/ship`, connecting an agent via MCP |
| Editor | the **simple view** (`view: Simple`) | the **full view** (`view: Full`) |
| Agent | the built-in prompt boxes | their own, through `/mcp` |
| Cares about | what the app does, whether it is online, what changed | control: code, files, versions, deployments, logs |

Give both a good experience. Concretely:

- **Nothing in the simple view or on `/build` uses developer slang.** Not
  "merge", "base", "branch", "rebase", "commit", "compile error", and no files
  or logs. The vocabulary is fixed: a feature is a **draft**, its data copy is
  **test data**, merging it is **putting it online**, a feature behind the newest
  version is **out of date**, and a version is described as the **change** it
  made. A compile error is the agent's to fix, not something the owner reads.
  Say "version" and its number only where the owner needs to name which one goes
  back online or which one a draft becomes (the draft dialogs do), never as the
  way to describe a change. The database is **records** (a table of them, a
  row is a record), secrets are **keys and passwords**, the workspace is **what
  your app saved**.
- What the agent is doing reaches the pages as **steps** - facts the runner
  records per tool call (`begin` in `docker/agent/builder.mjs`), and the
  lines the agent writes between them - never as English sentences from the
  runner. `/build` and the Change section draw a run with the **same parts**
  (`components/AgentRun.tsx`): what was asked, the step it is on and the clock
  against its limit, what the agent said, and how it ended. `/build` and the
  simple view show the agent's own lines, which both briefs ask for short and
  in the language of the request; `/build` names the step it is on in its own
  words (getting ready, choosing an address for your website, checking it for
  mistakes). A new tool the agent may call gets a step kind and words on both
  pages.
- The simple view shows the **Data** section only once the app keeps something
  (a table, a saved file, a secret) or its code waits for a secret, and then
  only the kinds that hold something: no switches, no folders, no code, no
  column types - its records table by table to read, a plain list of what was
  saved, and the keys to enter or replace. Entering one there switches secrets
  on.
- The full view, `/ship`, the API, MCP tools, the guide and the README use the
  precise words (feature, version, merge, base).
- The view a lambda opens in is a default only (`view`, set at creation and via
  `PATCH`). The build agent creates `Simple` lambdas, everything else `Full`.
  Whoever switches views has chosen for themselves, in their browser.

### The words each page is found by

The public pages are written in the words people type into a search engine -
checked against what Google suggests in each language, not guessed - and each
page has words of its own, so they do not compete:

- `/` - an **AI app builder with free hosting**, for both groups. "Agentic
  coding" is searched by developers choosing a coding agent, so it stays out
  of the title.
- `/build` - **create a website with AI**, free, **no sign-up**, in the word
  each language uses for a website (ホームページ, 홈페이지, página web …);
  searches for an "app" mostly mean a phone app.
- `/ship` - **host a vibe-coded app**, publish it from Claude Code, Codex or
  Cursor, from **localhost**, with a database and multiplayer built in. "Ship"
  is nobody's search word, and "publish an app" means the app stores, so the
  verb comes with "web" or "online".
- What nobody should need - a server, a hosting plan, a domain, a sign-up, a
  credit card - is said as **not their concern**: "hosting included", "no
  sign-up", and the questions people ask ("Do I need a server?") answered in
  a folded list. Words nobody searches for (Kubernetes, devops, IDE) stay off.
- The words go in the title and description (`pages.json`), the H1 and the
  H2s; the rest of the copy stays as short as it was. Nothing is claimed that
  the platform does not keep: no "unlimited", no "forever".

## Domain model

### Keys and ownership

A lambda has a **public key** (part of its URL, may be changed) and a **private
key** (the editor link). Whoever holds the private key owns the lambda, and
**editing is only possible with it**. The private key is shown only to the
creator. It travels in a brief, never in a log line the platform writes about
what was done. The request line is the exception, decided by the owner: it logs
the path as requested, so `/api/v1/lambdas/{privateKey}/…` carries the key, and
that is fine - the log is behind the admin token. There are no accounts.

The one place that authenticates differently is `/admin`: `X-Admin-Token`, see
the README. It is GenHTTP's API key authentication (`AdminAuthentication`): 401
without the header, 403 with the wrong token, and the routes are not there at
all (404) on an installation without a token. Behind it are the panel, the
server's telemetry and the log. **Only owners and the operator see telemetry**
- the owner a lambda's own in the editor, the operator everybody's; nothing is
public.

### Versions, data, and how they differ

- A **version** is the program: code *and* assets (a SPA, for example) - and
  what is written about it, its documentation and tests (below). It never
  changes once saved. Different versions may have different code and different
  assets.
- **Data** is what the program keeps: the database, the workspace and the
  secrets. It belongs to the lambda and is **shared by all versions**.
  Deploys, rollbacks and merges never touch it.
- Records go in the database and files in the workspace, never in assets. The
  front end goes in the version, never in the data. API keys and passwords go
  in the secrets, never in code, assets, the workspace or the database.
- Every kind of data is switched on before a lambda can use it. An agent may
  switch one on (`enable_data`) when what it builds needs it; **switching off
  deletes, so only the owner does it**. All kinds share one **Data** section
  in the editor, one pill each, drawn the same way; only what a kind holds is
  shown its own way. A new kind joins `DataKinds` and that section - it does
  not get a menu entry of its own.

### Secrets

- **A value is written and never read back** - not by the editor, the API, MCP
  or the owner. Only the lambda reads it (`Secret.Read`, `Secret.Exists`). No
  answer, log line or note may contain a value; name only.
- **Off by default.** Names are environment variable names (letters, digits,
  underscore, no leading digit, case sensitive), because an exported project
  reads `Secret.Read("X")` from `$X`. Values are not exported.
- Stored in SQLite, sealed with AES-GCM under a key made from the
  installation's key (`LAMBDA_SECRETS_KEY`, or `secrets.key` in the data
  directory - never in the database) and a random per-lambda salt; the name is
  associated data. Backup and migration = the database + the installation's
  key. Do not put the installation's key in the database, and do not describe
  the encryption as isolating lambdas from each other: they share a process.
- Read at runtime through a function handed to the compiled lambda, never
  compiled in, so a changed value applies without a deploy.
- A feature gets a copy (copied sealed); merging throws it away.
- Agents are steered to write `Secret.Read("NAME")`, switch secrets on and let
  the **owner enter the value in the editor**, so it never passes through the
  agent. The platform lists the names the code reads that have no value
  (`missing`; only-`Exists` ones are `optional`), and the editor and the
  simple overview ask the owner for them. That is the asymmetric interface for
  secrets: the agent declares the need, the human supplies the value. The build
  agent may switch secrets on and list them, and cannot set or delete one.

### Databases

- **A SQLite file per lambda**, reached with `Database.GetConnection()` (an open
  Microsoft.Data.Sqlite connection, pooled, one per request, disposed of).
  **Off by default**; switching it on makes an empty file, switching it off
  deletes it and the features' copies. Either switch restarts the lambda on its
  next request, so its startup migrations run against what is there.
- **Schema by Evolve migrations** shipped as assets in `migrations/`, applied as
  the lambda starts - never Entity Framework's migrations, `EnsureCreated` or
  `Migrate`, which the code guard refuses. A migration that was applied is
  never edited.
- **Records through Entity Framework Core - decided.** A lambda writes a
  `DbContext` of its own that maps the tables the migrations make, on the
  connection it is handed: `new Records(Database.GetConnection())`, configured
  with `UseSqlite(connection, contextOwnsConnection: true)`, one per request.
  Plain SQL on the connection stays possible. The demos use EF Core, and agents
  are steered to it. They are steered to use it **synchronously** - `ToList`,
  `SaveChanges`, never the `Async` forms: SQLite answers synchronously anyway
  and the Ioxide engine has its own async model. This is a hint, not enforced.
- **No connection of the lambda's own** (another file, the workspace, a
  connection string - EF's `UseSqlite(string)` included). The code guard
  refuses them - making a `SqliteConnection` is checked on the compilation, so a
  target-typed `new()` or a derived class is caught, and so is `UseSqlite`
  without a connection - and the connection handed out carries an authorizer
  (`ConnectionGuard`) that refuses `ATTACH`, `VACUUM INTO`, the directory
  pragmas and lifting `max_page_count`, which is the quota. Of EF, only the
  root namespace and `Metadata.Builders`, `ChangeTracking` and
  `Storage.ValueConversion` are reachable - its `Infrastructure`, `Storage` and
  `Internal` hold a context's services and options, and with them another
  connection. `dynamic` is refused for the same reason: it binds members the
  guard never sees. Keep all of this when touching any of it.
- **Not encrypted - decided.** SQLite cannot encrypt; it takes replacing the
  SQLite of the whole process with a build that can (SQLite3 Multiple Ciphers
  was tried: well kept, but one maintainer and a small .NET package, while
  SQLitePCLRaw 3 dropped free encryption builds), and the key would sit in the
  platform's database on the same volume. Do not bring it back without the
  owner asking.
- **The export carries it** as `database/database.db`, an ordinary SQLite file:
  the owner takes their data away with their code.
- A feature gets a copy (SQLite backup, consistent); merging throws it away.
- The editor **reads** it - tables, columns, rows - and never edits it. A change
  to records is a change to the app.

Because data outlives versions, a rollback or a newer version reads data written
by a different one. **Agents find a good compromise between data compatibility
across versions and the effort it costs**: prefer additive, tolerant formats
(optional fields, defaults for what is missing) over migrations, and do not
build machinery for compatibility that nobody needs.

### Documentation and tests

Every version keeps what is written about it beside its program, in
`.lambda/`: `docs/product.md` (what the app is, for whom, why, what people do
with it - in the user's terms; its first paragraph is the app in a sentence or
two), `docs/decisions.md` (the technical decisions and why), `tests/README.md`
(how it is tested automatically), and beside those more pages, pictures, test
scripts and test data.

- **Files of the version, not fields in the database.** They describe that
  version, so they are saved, diffed, rolled back, copied into a feature and
  merged with it like any file, and travel in the zip and the export. Do not
  move them into SQLite or into the workspace.
- Documentation and tests are **one kind of file** (context,
  `LambdaSource.IsContext`), stored and shown by one mechanism: markdown pages
  with files beside them. Do not build them as two features.
- Never compiled (whatever they are called), never served, left out of what
  identifies a build, counted towards the asset allowance.
- **Agents write them, humans read them** - the asymmetric interface again.
  Agents write all three with a new lambda, update what a change affects in
  the same save, read them before changing a lambda, and run the tests against
  a feature's preview before merging.
- **In proportion to the lambda - decided.** A small lambda gets a short
  paragraph a page and one quick check, not a test suite, extra pages or seeded
  test data; they grow with the app. Agents overdid it for small lambdas, so
  this is said wherever they are told to write them. The demos have more,
  because they teach. This is said the same way in the MCP
  instructions, the tool descriptions and answers, `platform_guide`
  (`documentationAndTests`), the build agent's brief, the guide and the README -
  keep them the same.
- The editor: **Documentation** is the second section after the overview, in
  both views; **Tests** is in the full view only. The simple view calls the
  documentation **About** and shows the product page alone, to be corrected by
  telling the agent rather than by editing it.

### Pages meant to be found

Agents are steered to make a page that is meant to be found or shared - by a
search engine, an AI agent, a link in a chat - findable, in proportion: a
title, a meta description, the language, an icon, a social preview (Open
Graph), what the page is about in the HTML as served rather than only in what
a script renders, and the domain as canonical where the lambda has one. A tool
for a few people gets a title and nothing more. This is said in the MCP
instructions (one line), `platform_guide` (`beingFound`), the build agent's
brief, the README and `/docs` - keep them the same. The platform writes none
of it into a lambda's pages; the agent does.

- `og:image` takes a full address - social networks do not resolve a relative
  one - and is the exception to relative links, with `og:url` and the
  canonical link. The warning a feature's deploy gives for a link to
  `/lambda/{publicKey}/` (`McpTools.Leaks`) leaves meta tags and the canonical
  link out: they name the page, and the page never follows them.
- **No renderer - decided for now.** Rendering a lambda in a headless browser
  on the server (for the agent to look at its page, a picture proposed in the
  showcase tab, an `og:image`) was considered and left out: it runs strangers'
  JavaScript in a browser beside the platform, and costs a container,
  Chromium and fonts. Nor are tools added to the build agent's image - it has
  no shell to run them, and must not get one, since its token is in its
  environment. Do not bring either back without the owner asking.

### Pushing, not polling

Agents are told never to poll: a page does not fetch on a timer to see
whether something changed. What changes while a page is open is pushed by the
server - server-sent events where the page only listens (`demo-live`), a
websocket where it talks back (`demo-game`). A poll is a request whether
anything changed or not, on a server every lambda shares. This is said in the
MCP instructions (one line), `platform_guide` (`liveUpdates`), the build
agent's brief, the README and `/docs` - keep them the same.

### The link back

**Decided by the owner:** agents are asked - not required - to put a small
"Made with GenHTTP Lambda" line at the foot of the pages they build.

- **A link without a domain, plain text with one.** Below `/lambda/` the line
  links to the installation's public address (`LAMBDA_PUBLIC_URL`, else the
  address the agent called) - a link within the same site, which no
  link-spam policy is about. On a lambda with a domain of its own it is plain
  text: there it would be a link from another site, and the same link in the
  footers of many sites is what search engines count as link spam. A lambda
  whose line links and that gets a domain has it made plain text with its next
  change.

- **The user may refuse.** The agent says that it added the link, leaves it
  out or takes it out when asked, and notes in `decisions.md` that it was not
  wanted, so the next agent does not put it back. A change does not add one to
  a lambda that has none.
- **The name and nothing else** as the link's words: no keywords, no badge, no
  script, nothing hidden.
- The platform never writes it into a lambda, and nothing checks for it.
- This is said in the MCP instructions (one line), `platform_guide`
  (`backlink`), the build agent's brief, the README and `/docs` - keep them
  the same.

### Features ("Drafts")

New functionality is developed in a **feature** (a *draft* in the editor):

- It is based on an existing version and is edited in place as a single version
  until it is done. It has a **copy of the data**, and is **published on its own
  URL** (`/features/{key}/`) so people can try it.
- When ready, it is converted into a new version (merge).
- **Only a feature based on the newest version can be merged.** If the lambda
  has newer versions, the agent must bring those changes into the feature,
  then move its `base` (`update_feature`), and only then may it merge.
- **GenHTTP Lambda provides no merging or rebasing functionality, and must not
  grow any.** The agent does that work. The platform only checks the base, under
  the lambda's lock.
- A lambda that exists is changed in a feature; a new lambda is written as
  versions. This is said the same way in the MCP instructions, the tool
  descriptions and answers, the guide, and the README - keep them the same.

### Showcase, export, ownership of the code

- **Showcasing is the owner's choice**: opt-in, never automatic.
- **The code belongs to whoever created the lambda.** We do not interfere with
  it. They can export it as a runnable C# project (`GET …/export`) and run it
  elsewhere. Do not add lock-in, and keep the export working.
- The export is **as small as a GenHTTP project can be**: .NET 10,
  `GenHTTP.Full` (the internal engine, not Ioxide) at the version the server
  runs, the default hosting snippet (`Defaults()`, no port, `RunAsync()`, no
  console output of our own), the snippet in a static `Project` class returned
  by `Project.Create()`, the other files named the .NET way, everything that
  stands in for the platform in `Platform/`, and a `Dockerfile`. SQLite, EF Core
  and Evolve are referenced only where the code uses them, and the database comes
  along in `database/`. `Program.cs`
  opens with a short note that it was a lambda on genhttp.dev, its metadata,
  and a link to the GenHTTP documentation. A test builds the export of every
  demo; keep it passing.

### Open source

An owner may publish the code of a lambda at `/source/{publicKey}`, for anybody
to read, star and download.

- **Publishing is the owner's choice**: opt-in, never automatic, and only with
  the private key - the editor's **Open source** section (both views, after the
  showcase), `PUT …/source`, or the `open_source` tool, which agents use only
  when the user asks. A license is picked from a short list (`SourceLicenses`),
  **MIT by default**.
- **What is published is the program and what is written about it**: every
  version, as the export packs it, with a `LICENSE`. **Never the data** - no
  database, no workspace, no secret's value; `ProjectPacker.Publish` takes no
  database, keep it that way - and **never what only the owner may know**: the
  specification (their words), the editor key, traffic, logs, visitors. A
  version is described by its change line.
- **Just the source.** No issues, pull requests, wikis or projects, and nothing
  on `/source` acts on the lambda. Whoever wants to build on it downloads it;
  `platform_guide` (`openSource.startingFromOne`) tells an agent how a download
  becomes a lambda again.
- `/source` is a part of the frontend of its own (`src/Frontend/src/source`),
  in a frame of its own, not in the landing page's menu - but in the site's
  colours, languages and addresses per language. Not prerendered: the server
  names each page after its lambda, marks it up as `SoftwareSourceCode`, and
  lists every published source in the sitemap. An unpublished source is not
  found, the same as a key nobody has.
- **The versions are the single source of truth.** A version is packed once
  into `/data/sources/{lambda}` and served from there - a cache, held to
  `LAMBDA_SOURCE_CACHE_BYTES`, keyed by everything it is packed with.
- **Stars are a `POST` with a signed ticket** the page was handed at least a
  second before, never a link; each address stars once, remembered in memory
  as a keyed hash and never on disk. The count is in the database and survives
  taking the source down.
- The demos are published under MIT by the installation, being there to be
  read and built on; nobody can change that.

### Demos

The demos (`DemoCatalog`, files in `Resources/Templates`) exist to show agents
how to write apps and to give them a good starting point.

- They are **read-only by definition** (tier `Demo`, enforced in
  `MetaService.EnsureEditable`, whichever door the request comes in). Their
  editor key is public. Anyone starts an editable copy with `create_lambda` and
  a demo as template.
- They follow the **best practices of the GenHTTP framework**
  (<https://genhttp.org/documentation/content/>) and use its **high-level APIs
  wherever possible** rather than inventing their own functionality.
- **When you add functionality to the platform, add it to the demos as well if
  it makes sense**, so agents learn it by reading them. Every demo has the three
  pages of its documentation and tests and a script its tests run
  (`EveryDemoSaysWhatItIsWhyAndHowItIsTested`); a new demo gets them too. They
  keep their records in the database, migrated with Evolve and read and
  written through Entity Framework Core, and files
  (`demo-files`' uploads) in the workspace; the seeder switches the database on
  (`LambdaDemo.Database`), and a copy starts with one of its own. A demo that
  reads a secret is given a random value by the seeder (`LambdaDemo.Secrets`)
  and works without one, so a copy runs before its owner switches secrets on
  (`demo-registration`, `PASSWORD_PEPPER`).

### Tiers

`Free`, `Premium`, `Demo` (`LambdaTier`), assigned by an administrator and never
chosen by the owner.

- **Free** is for trying: taken offline when unused, removed when abandoned.
- **Premium** is for running something in production, for example a website on a
  custom domain: kept online, larger limits. The simple view's overview offers
  it to every lambda that is not premium (or a demo), in a tile beside the
  latest change; its only action is a mail to us naming the app by its public
  address, never the editor key - the tier stays the operator's to assign.
- **The limits of a tier are product settings, not server configuration -
  decided.** Code, assets, workspace, database, versions and features per
  tier, the free tier's lifetime, and the per-caller limits (showcase picture,
  requests per second, builds per day) are set in the panel's **Limits**
  (`LimitsService`, rows in `settings`): one form, the tier limits as a table
  with a column per tier and the per-caller ones in a block of their own, so
  which is which is plain without a switch hiding half of it.
  A change applies to what is checked next and takes nothing away. Ports,
  directories, keys, buffers, `LAMBDA_MAX_CONCURRENCY` and the execution
  timeout stay environment variables: they protect the process. The old
  limit variables give the defaults for one release, with a startup warning,
  and are then removed. Callers ask `LimitsService`, never `LambdaOptions`,
  for a limit.
- **Enterprise** - a customer running their own instance, on-prem or in the
  cloud - is planned but **do not anticipate enterprise features unless asked**.
  It is not decided that the enterprise code base will be the same as the cloud
  one, so do not shape the cloud code for it. (`/enterprise` is only a page for
  now.)

## Architecture

Read the README section "How it is put together" before touching services. The
rules that matter:

- One .NET project, divided into services behind interfaces; the API resources
  talk to the interfaces. `Meta` is the only component that speaks to the
  database, and its public surface is hand-mapped DTOs. Code lives on the file
  system (`Storage`), never in the database.
- **A service orchestrates, and the concerns it would mix are units of their
  own - decided in the architecture review.** A protocol, a count, a check:
  each goes into a class beside the service, in its folder, and the service
  keeps deciding what is done and in which order. Not a class per method - a
  unit is a concern somebody can review on its own. The resources still talk
  to the service. `Services/Building` is the template: `BuildService` over
  `AgentClient`, `BuildAllowance`, `ModelGate` and `AgentInput`.
- Compiled lambdas run in the server process. The `CodeGuard` raises the cost of
  misbehaving; **it is not a sandbox.** Do not describe it as one.
- Database: SQLite, EF Core, migrated by Evolve from `Data/Migrations/V<n>__*.sql`.
  Existing migrations are never edited. **Before pushing a new migration check
  the highest `V` on `origin/main`** - a duplicate fails every test in CI. New
  files under `src/GenHTTP.Lambda/Data/` are hidden by the `data/` rule in
  `.gitignore` on Windows: check with `git status --ignored` and `git add -f`.
- The frontend is a React single page application in `src/Frontend`, built into
  `src/GenHTTP.Lambda/wwwroot` (not committed). Public pages are prerendered per
  language.
- Links inside a lambda's front end are relative, never `/lambda/...`: a lambda
  also answers at a domain of its own, and a feature at `/features/{key}/`.
- The full view's sidebar is **grouped** (overview and documentation; sharing -
  showcase, open source, domain - second, as the owner decided; build; program
  and data; run). A new section joins the group it belongs to rather than the
  end of the list.
- **Wired through the container**, not by hand and not in statics. A concern
  that needs nothing but services is an `IDependentConcern`, registered as a
  singleton and added with `Dependent.Concern<T>()`; only a concern built with
  an argument of its own is built by hand (the lookup of a lambda, whose
  locator differs per route). The scope they are resolved from is opened by
  the outermost concern of the host (`AddDependencyInjection`, added last), so
  nothing dependent goes outside it. What only a door needs - HTTP or MCP -
  stays at the door (`WorkspaceFiles`, `OperationLog`); what is done with the
  data (base64, a version's facts) is the service's. What is one per process,
  the console, is installed by a service the application owns
  (`ConsoleCapture`), so tests own it as well.

### Serving, and the threads it runs on

On Ioxide a request runs on a **reactor**: one thread per core that owns that
core's connections and resumes each request inline where its I/O completed.
Whatever a request does there, every other connection of that core waits for.
.NET `async` works on it (ioxide posts continuations back to the reactor), but
the platform's database is SQLite, which answers synchronously whatever the
method is called - so the code is written the way it runs:

- **Synchronous by default - decided.** Services, resources and MCP tools are
  plain methods returning plain values, and every query is a synchronous EF
  call (`ToList`, `FirstOrDefault`, `SaveChanges`, `CreateDbContext`). A
  method is `async` only where it awaits something that really leaves the
  thread: compiling, packing, copying a workspace or a database, reading a
  lambda's own database for the editor (it may be slow by design), a request
  body, the network. Do not add an async path beside a synchronous one.
- **Enforced when it compiles.** `src/GenHTTP.Lambda/BannedSymbols.txt`
  (Microsoft.CodeAnalysis.BannedApiAnalyzers, RS0030) refuses EF Core's async
  methods, waiting for a task, the `File.*Async` helpers and `Task.Run`, each
  with the reason. It lists every overload by its id, generated from the
  assemblies; a new overload in a new version of EF is added the same way.
- **Serving a lambda does not touch the database.** What answers to a key, an
  id or a preview key is held by `ResolutionCache`, and every write to the
  platform's database makes it stale (`DatabaseChanges`, an EF interceptor).
  So every write goes through EF; one that does not is never seen by the
  caches.
- **Long work leaves the reactor in one hop**, through `Offload`: compiling and
  binding with Roslyn, packing, copying a workspace or a database. Not in many
  small asynchronous steps (`File.ReadAllTextAsync` hops out and back per
  buffer), and not in place. A version's file is read and written in place:
  it is bounded by its tier's allowance, and saving one is rare. `Task.Run` is
  refused, so every hop is marked in `Offload` and can be given a scheduler of
  its own there; work started in the background rather than awaited (the
  scheduler's jobs, seeding the demos) is not a hop and suppresses RS0030
  where it starts, saying why.
- **Locks are plain locks (`Lock`), never held across an await.** What compiles
  under a lambda's or a feature's turn compiles first and takes the turn to
  write down what it came to (`MetaService.DeployAsync`,
  `FeatureService.DeployAsync`, `MergeAsync`); serving compares the build it
  finds with what is online and builds again where they differ, so the order
  two deployments finish in does not matter. A lock that has to be held across
  an await is a `SemaphoreSlim` taken with `WaitAsync` (copying a feature's
  data), never with `Wait()`.
- **Never wait on a task** (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`):
  on a reactor its continuation is posted to the very thread that is waiting,
  and that core deadlocks for good. Lambdas are held to the same by the code
  guard (`CodeGuard.InspectWaiting`), and agents are told so.
- **No lock that every request takes may be held for more than a moment.** The
  log ring's is the one every request takes: an append happens under it, and a
  reader walks back from the newest line only as far as its cursor.
- **The console's lock comes first.** The console takes the lock of
  `Console.Out` - the tee's, once it is installed - underneath every write to
  its stream, and a print through the tee holds it before it reaches the real
  console's writer. So whatever writes to the real console directly takes
  `Console.Out` first (`LogBookProvider`); the other order deadlocked the
  server on startup.
- What is read off a version is read once: a version never changes
  (`VersionFactsCache`).

### Logging what was done

Every API call that changes something, and every MCP tool call, logs at
information level **what it did** once it has done it - the operation and its
arguments, not the request. A new endpoint or tool gets its line in the same
change; the helpers are in `OperationLog`.

- **Terse, in the style backends log in - decided.** Past-tense verb first,
  then what it was done to and its key, then the arguments as pairs:
  `Published source of lambda {Lambda} license {License}`,
  `Deployed lambda {Lambda} version {Version}`,
  `Failed to merge feature '{Feature}' of lambda {Lambda}`. A line says what is
  known, not why or what it might mean. A name that may hold spaces - a
  feature's, a title - is quoted. The services log in the same shape.
- **The same property everywhere**: `{Lambda}` is a public key, `{Feature}` a
  feature's name, then `{Version}`, `{Path}`, `{Name}` (a secret's). A
  service that only has the id writes `lambda #{LambdaId}`. One template per
  case, never a fragment built beforehand (`of {Owner}`), so a structured sink
  can filter on each.
- A lambda is named by its **public key** (`PublicKeyOf`), a feature by its
  **name** (`NameOf`, or the name a service hands back). Never the editor key,
  never a feature's key.
- **Never a model as a whole.** A record's `ToString()` prints every property,
  and `LambdaInfo` carries the editor key, others code and specifications.
  Name the properties a line logs.
- Arguments that are sensitive or long are left out: secret values (the name
  only), code, file contents, pictures, specifications, descriptions.
- **The one exception: the prompts of the build agent** - on `/build` and in the
  editor's Change section - are logged in full, because the owner wants to know
  what people ask for. The privacy page says so; keep it saying so.
- API reads are not logged (the editor polls), apart from exports and
  downloads; an agent's reads through MCP are.
- A refused request is not logged here - its request line has the status.

### Frameworks: GenHTTP and Ioxide

- We run on GenHTTP and its Ioxide engine. **If you find a bug in either, flag it
  clearly** - say what it is, where, and a reproduction - **instead of working
  around it**, so it can be fixed at the source. Tell the owner in your answer;
  do not bury it in a comment.
- **Do not write tests for the frameworks.** Test GenHTTP Lambda only.
- The framework source is normally checked out beside this repository
  (`../GenHTTP`). Read it instead of decompiling the NuGet packages.
- Known and already dealt with: GenHTTP releases the request headers once a body
  is bound, so declare path and query parameters *before* the body parameter, and
  check headers in a concern in front of the resource, not inside a
  body-taking method (see `AdminAuthentication`). A new instance of the same kind of
  problem is a bug to flag, not to work around again.

## Testing

- **Every feature has functional tests written as acceptance criteria**: start
  the real application (`LambdaFixture`), do what a user or agent does through
  the API, MCP or a deployed lambda, and assert what they would see. Name tests
  after the behaviour (`AFeatureStartsAsACopyOfTheNewestVersion`).
- **Prefer these over unit tests with mocks.** There are none with mocks in this
  repository today; keep it that way. A fake is acceptable only for something
  outside our process (`FakeAgent` stands in for the build agent's runner).
- Each test has its own temporary data directory and runs in parallel.
- MSTest, `TreatWarningsAsErrors` is on.

```bash
dotnet build GenHTTP.Lambda.slnx -c Release
dotnet test GenHTTP.Lambda.slnx -c Release
cd src/Frontend && npm run build      # type checks and builds; CI runs it too
```

On Windows the default Ioxide engine (io_uring) does not run: start the server
with `LAMBDA_ENGINE=Kestrel`, and point `LAMBDA_WEB_ROOT` at
`src/GenHTTP.Lambda/wwwroot` to serve a fresh frontend build.

## Translations

- **Everything a visitor or owner sees is translated into all supported
  languages, except the admin panel** (`/admin`, `src/Frontend/src/console`),
  which is English only. That includes the landing, `/build`, `/ship`, `/docs`,
  `/showcase`, `/enterprise`, the legal pages, the create page, the editor, and
  the page metadata in `pages.json`.
- Languages: `id de en es fr it nl pl pt pt-pt tr ja ko` (`src/Frontend/src/i18n/languages.ts`,
  mirrored in `Web/SiteLanguages.cs`). English is the source; every other
  catalog is typed as it, so a missing string fails the frontend build. A new
  language is added in both places, in every catalog, in `pages.json` and in the
  social images.
- The editor's words are a folder per language, `locales/<language>/editor/`:
  a file for each section of the editor, named after its key, and an
  `index.ts` that puts them together - so a change to one section touches one
  file per language. A language's grammar helpers for the editor live beside
  them in `language.ts`. A new section gets its file in English, its line in
  every `index.ts`, and its translations from `apply`, which creates the
  other languages' files.
- **Use the register that is normal for a product in that country**, and stay in
  it. Professional, never chummy or slangy; do not carry English idioms over
  literally. The form each language uses today:

| Language | Form | Notes |
|---|---|---|
| de | formal - *Sie* | factual and precise, no marketing jokes |
| fr | formal - *vous* | |
| it | informal - *tu* everywhere except `/build`, which is formal (*Lei*) | the no-code page is the only one that addresses non-developers |
| es | informal - *tú* everywhere except `/build`, which is formal (*usted*) | as Italian |
| nl | informal - *je* | |
| pl | informal - *ty* | |
| pt | *você* (Brazil) | no colloquialisms |
| pt-pt | informal - *tu* (Portugal) | |
| tr | formal - *siz* | |
| ja | polite (です・ます) | |
| ko | polite (해요체 / 하세요) | |
| id | formal - *Anda* | |

  The informal choices for it, es, nl, pl and pt-pt are deliberate. If you change
  a language's form, change this table in the same commit; if you add a language,
  add its row. The translation script reads this table into every request, so
  keep its shape: one row per language, code | form | notes.
- The forms above describe the copy as written today. A request shows the
  sentences beside a new one, so a translator matches them rather than this
  table alone.

### How a change is translated

Nobody reads a whole catalog to translate a sentence. A script hands each
translator only what changed, and puts the answers back:

1. Write the English - the catalogs in `src/Frontend/src/locales/en` and the
   English titles and descriptions in `pages.json`. A new sentence whose place
   is not plain from its key gets a doc comment saying where it appears: the
   translator sees the comment, never the page.
2. `npm run translations -- extract` (in `src/Frontend`) compares the English
   with the base of the branch and writes requests into `.translations/`: per
   language its register from the table above, its fixed words from
   `src/locales/glossary.json`, and each new or changed sentence with what the
   English said before and what the language says now. A large round is split
   into parts.
3. One `translator` agent (`.claude/agents/translator.md`) per request, all at
   once, each told only the path of its request. It reads that file and writes
   its answer beside it - nothing else.
4. `npm run translations -- apply` puts the answers into the catalogs, refuses
   one that lost a parameter, an interpolation, a call of the kit or a link,
   removes what English no longer has, and gives French its non-breaking spaces.
   Then `npm run build`, and read the diff of the languages you know.

- **The translator runs on Sonnet - decided by the owner.** The model is pinned
  in the agent's definition; do not override it with the Agent tool's `model`.
  A prepared request is a small job, and Sonnet does it well at a fraction of
  the cost.
- **The fixed words of each language are in `glossary.json`.** A term that
  changes in a language changes there first, then in its catalogs; a new term of
  the platform gets a row in every language.
- What a branch has already translated counts as done. To ask again after
  changing English once more, commit and extract with `--base HEAD`.
  `--stale` also asks for every sentence whose English changed in a later
  commit than its translation, and `--redo <path>` for a sentence or a section
  to be checked against the English. `check` compares every language with
  English - CI fails on a sentence that lost a parameter or a link, and prints
  the rest for a person to look at - and `show <file> <path>` prints one
  sentence in every language.

## Documentation

**The documentation must contain every feature and be kept up to date in the
same change that changes the feature.** There are four places, and a feature
touches all that apply:

1. `README.md` - routes, API, configuration, operations, how it fits together.
2. `/docs` (`src/Frontend/src/locales/*/guide.tsx`) - for owners, in the
   vocabulary of the simple view, translated.
3. `platform_guide`, the MCP instructions and the tool descriptions
   (`Api/Mcp`) - for agents.
4. The API description (Scalar at `/api/v1/scalar/`, from the resources).

The same rule is worded the same way in all of them.

The MCP instructions (`McpHandler`) are **short**: how to get from nothing to
something online, how a lambda that exists is changed, and the rules an agent
would otherwise break before it reads the guide (where data goes, the
database, waiting for tasks, relative paths, secrets, documentation, pages
meant to be found). The rest
is in `platform_guide` and the tool descriptions; do not repeat it there.

## Working conventions

- The code is written to be read: XML documentation with `<remarks>` that say
  *why*, `#region` blocks per concern, braces on their own line. Match the file
  you are in.
- Comments say why, not what. Write prose, not marketing.
- Tailwind for styling, both light and dark themes, phones included.
- Check a change you cannot test in the browser by running the app (see above)
  and looking at it; type checks and tests are not proof that a UI works.
- PR screenshots: an orphan branch `pr-assets/<topic>` with the images at its
  root, linked from the PR body by commit-pinned raw URLs.
