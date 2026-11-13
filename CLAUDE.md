# GenHTTP Lambda - steering file for agents

This file holds the product and architectural decisions of this repository.
Follow it, and when a decision is made or changed - by the owner in a
conversation, in a review, anywhere - **put it here in the same change**. A rule
that only lives in a chat is lost. The [reference](docs/reference.md) has the
routes, API, configuration and operations, and the [README](README.md) is the
guide for whoever runs the community edition; this file has the *why* and the
rules.

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

It is **open source under MIT - decided by the owner** (`LICENSE`), the whole
repository.

## Editions and components

**Decided by the owner:** the platform is a set of components that hosts put
together the GenHTTP way, and there are two hosts - two editions:

- **Cloud** (`src/GenHTTP.Lambda.Cloud`) is genhttp.dev: lambdas at
  subdomains of the hosting domain, the marketing site (landing, `/build`,
  `/ship`), the legal pages, its terms of use, its contact and abuse mailbox
  and its description of itself to search engines (`CloudInstallation`,
  `CloudStructuredData`). There is no page of prices any more.
- **Community** (`src/GenHTTP.Lambda.Community`) is what anybody runs on
  their own server, published as the Docker image `genhttp/lambda`: every
  feature of the cloud, the build agent included, behind a simpler front page
  for developers (`LandingPage`: what it does, the build box, connecting an
  agent, what every app gets, how it runs) with the guide and the showcase -
  translated like everything else, crisp, and selling nothing. No terms, no
  legal pages, no imprint, no organization behind it; what its operator names
  (`LAMBDA_CONTACT`, `LAMBDA_ABUSE_CONTACT`) is shown, and nothing else.
- **Lambdas answer by path in the community edition - decided by the owner**:
  `/lambda/{key}/` on the one address an installation runs at
  (`LambdaAddressing.Paths`, `LAMBDA_PUBLIC_URL`), with domains of their own
  served as in the cloud. Giving it `LAMBDA_HOSTING_URL` switches it to
  subdomains; `LAMBDA_ADDRESSING` decides over both. **The trade-off is said,
  not hidden**: by path, a lambda shares the platform's origin and can read
  what the editor and the panel keep in that browser. So the editor's pages
  send no referrer (its key is in their address) and are not indexed, the
  panel keeps its token for the tab only (session storage) wherever the
  lambdas answer on its origin - local storage only once `/system` says they
  do not - and the README says who should switch to subdomains.
- **Nothing in the pages names the cloud.** What differs per installation -
  where a lambda answers, the git address, where a domain has to point (the
  server resolves its own public name, `DomainResponse.Target`), the page
  with the build box (`LambdaOptions.StartPage`: `/build` on the cloud, `/`
  elsewhere) - is asked of the installation, never written into a bundle.
- **By path, the "Made with" line is a plain link** - a link within the same
  site, as it was before the subdomains - and nofollow at subdomains; the
  guide says whichever holds (`platform_guide`, `backlink`).

The components, each a project with a builder in front (`X.Create(...)`, the
GenHTTP style), and what they need given to the builder rather than read from
the environment:

| Component | Project | Builder | Mounted at |
|---|---|---|---|
| The platform: services, database, jobs, log book | `GenHTTP.Lambda.Core` | `LambdaPlatform.Create().Options(...)` | - |
| The REST API | `GenHTTP.Lambda.Api` | `RestApi.Create(platform)` | `/api` |
| The MCP endpoint | `GenHTTP.Lambda.Mcp` | `McpServer.Create(platform)` | `/mcp` |
| The editor, the create page, the owner's git | `GenHTTP.Lambda.Editing` | `Editor.Create(platform)` | `/editor` |
| The operator's panel | `GenHTTP.Lambda.Administration` | `AdminPanel.Create()` | `/admin` |
| The published sources, their public git | `GenHTTP.Lambda.Sources` | `SourcePages.Create(platform)` | `/{lang}/source` (no path of its own) |
| Serving the lambdas | `GenHTTP.Lambda.Execution` | `Runner.Create(platform).Site(site)` | around the site |
| Serving a bundle's pages | `GenHTTP.Lambda.Web` | `SitePages`, `ClientApplication`, `Bundle` | - |
| Engine, ports, TLS, the environment | `GenHTTP.Lambda.Server` | `LambdaServer.Create(platform)` | - |

- **A host's `Program.cs` is the composition, and stays short and plain**:
  read the settings, build the platform, put the components into a layout,
  hand it to the runner and the server. Read both before adding a component.
- **Nothing below the hosts reads the environment - decided by the owner.**
  `LambdaSettings.FromEnvironment()` (in `Server`) reads the `LAMBDA_*`
  variables into the platform's options (`LambdaOptions`, the DTO the
  components share) and the server's (`ServerSettings`: engine, ports,
  certificates, ACME); a host adds what is its own (`CloudInstallation.Of`).
  The variables are the same as before the split, so an installation's
  configuration did not change - but for `LAMBDA_WEB_ROOT`, which went with
  the one folder of pages it named, and three that were added
  (`LAMBDA_ADDRESSING`, `LAMBDA_CONTACT`, `LAMBDA_ABUSE_CONTACT`).
- **The mount paths are fixed** (`PlatformRoutes`): the components link to
  one another - the API hands out the editor's address, a clone keeps its
  git address - so a host mounts each at its name and nowhere else.
- **The runner goes around the site**, not into it: a lambda's own host owns
  every path there. On the platform's host it takes `/features/` and, by path,
  `/lambda/`, and leaves the rest to the site.
- **The editor and the panel talk to the platform through the REST API
  only**; the API and MCP share logic through the services (`OperationLog`,
  `LambdaAccess`, the analytics' wire shape), never one door reaching into the
  other. The admin token is the platform's (`LambdaOptions.AdminToken`).
- **Every component with pages owns its frontend - decided by the owner.**
  Its sources are in its `Frontend/` folder, an npm workspace at the root
  builds each into its project's `bundle/`, and the project copies that beside
  the host's assemblies (`bundles/<name>/`, read by `Bundle.Beside`; a
  builder's `.Bundle(directory)` overrides it). No component serves another's
  bundle and there is no shared folder of pages. What they share is source,
  not a build: the kit (`src/Kit`, `@genhttp-lambda/kit`) - the shell, the
  components, the i18n machinery, the API client, the build box, the guide,
  the showcase, the Tailwind preset and the Vite helper. A link into another
  bundle loads it (`claim`, `owns`).
- **What only one installation says is the host's**: terms, contact, abuse
  mailbox, legal pages (`LambdaOptions.Terms`, `Contact`, `AbuseContact`,
  `LegalPages`). Without terms nothing is accepted on creating a lambda; the
  pages every installation shares read the rest from `/system` (`operator`),
  and leave out what is not there.
- **Docker**: each host has its Dockerfile (`src/<host>/Dockerfile`, built
  from the root); `docker-compose.yml` and `deploy.sh` build the cloud's,
  unchanged otherwise. `.github/workflows/release.yml` publishes the community
  image and the build agent (`genhttp/lambda-agent`) to the `genhttp` Docker
  Hub account for amd64 and arm64, on a tag `v*` or by hand - nothing is
  pushed otherwise. `samples/community` runs it with Compose, the build agent
  as a profile.
- **Shipped as project references for now**; the components are meant to
  become NuGet packages, so a component takes nothing from a host.

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

A lambda has a **public key** (it names its address, `{key}.genhttp.run`, and
may be changed) and a **private key** (the editor link). Whoever holds the private key owns the lambda, and
**editing is only possible with it**. The private key is shown only to the
creator. It travels in a brief, never in a log line the platform writes about
what was done. The request line is the exception, decided by the owner: it logs
the path as requested, so `/api/v1/lambdas/{privateKey}/…` carries the key, and
that is fine - the log is behind the admin token. There are no accounts.

The one place that authenticates differently is `/admin`: `X-Admin-Token`, see
the reference. It is GenHTTP's API key authentication (`AdminAuthentication`): 401
without the header, 403 with the wrong token, and the routes are not there at
all (404) on an installation without a token. Behind it are the panel, the
server's telemetry and the log. The token is **the platform's**
(`LambdaOptions.AdminToken`, decided by the owner): the REST API's operator
routes answer to it and the address block lets the panel through with it; the
panel (`AdminPanel`) is pages only, over the REST API. **The editor has no
Admin section any more - decided by the owner**, so the two components do not
reach into each other: what only the operator decides about one lambda - its
block (`console/BlockControl.tsx`), its tier - is on its page in the panel.
**Only owners and the operator see telemetry**
- the owner a lambda's own in the editor, the operator everybody's; nothing is
public.

### Where the lambdas answer

**Decided by the owner:** on genhttp.dev (the cloud edition) every lambda
answers at a subdomain of the hosting domain named after its public key -
`https://{key}.genhttp.run/` (`LAMBDA_HOSTING_URL`) - not below the site's own
host. The community edition answers **by path** instead (see Editions below):
`{public address}/lambda/{key}/` (`LambdaAddressing.Paths`), on the one address
an installation is run at, and switches to subdomains once it is given a
hosting URL. What follows is about subdomains unless it says otherwise.

- **A site of its own.** A host of its own keeps what a lambda runs in a
  browser away from what the site keeps there - the admin token - and from the
  other lambdas, and gives it the root of its host. The hosting domain
  belongs on the Public Suffix List, so a lambda cannot set cookies for the
  others; that is the operator's to apply for.
- **The control plane stays on the site**: the editor, the API, MCP, git,
  the panel. So do the previews of features at `/features/{key}/` - for now
  the one place a lambda's code still runs on the site's host; moving them is
  a decision of its own.
- **A lambda on the site's host may not widen a service worker's scope**
  (`LambdaExecutionHandler`): a preview always, a lambda by path too. A
  response with `Service-Worker-Allowed` is refused there with a 500 that
  says why, and a line in the log - a worker for `/` would see the editor
  and the panel in that browser afterwards. Refused rather than stripped:
  GenHTTP adds headers to a response and takes none away. At a host of its
  own, its scope is its own business.
- **`/lambda/{key}/…` redirects permanently** to where the lambda answers
  now - its domain while it has one - with the path and the query, before
  anything else the server does: no request line, no telemetry, no other
  middleware (`LambdaRedirectConcern`, added last on the host). Moved
  Permanently for a read, Permanent Redirect for the rest, cached for a day
  since a domain comes and goes.
- **The subdomain serves the lambda only while it has no domain of its
  own that its tier serves**; with one, it redirects there the same way. A
  domain of its own works as it did.
- **`www.` in front of a domain redirects to the domain - decided by the
  owner**, the same way, unless the owner entered the domain with `www.`:
  then that is where it answers, and the name without is not sent there.
  Never served at both names (two sites to a search engine), and never
  where the `www.` is another lambda's domain or the platform's own name.
- **A host nobody has is never the platform - decided by the owner.** With
  a public address (`LAMBDA_PUBLIC_URL`) the platform answers only at its
  own names - `localhost`, that address's host, the MCP origins - and any
  other host that is no lambda's gets the page of a subdomain with nothing
  online, 404, on every path (`UnknownHost`). A domain pointed here before
  its owner entered it served the site under its name, and crawlers kept
  asking for the site's paths there once it was a lambda's. The ACME
  challenge is still answered for every host. Without a public address the
  installation cannot tell its names from others', and every host is the
  platform's.
- **Finding a lambda by an address reads the key as it is spelled
  (`LambdaKeys.TryRead`), never by the rules for claiming one**
  (`TryNormalize`): a rule made later - a reserved name, `xn--` - must not
  take a lambda from before it offline.
- **The wildcard certificate is the fallback.** The io_uring engine matches
  names exactly, so the certificate of `*.{hosting domain}` is presented to
  every name no certificate is listed for (`CertificateLoader`); the site's
  keeps being presented by its names.
- **The hosting domain itself has a minimal page** that leads to the site, and
  a subdomain with nothing online a plain one (`HostingPages`). Both are
  rendered by the server, never the site's bundle - no script of the site is
  served at the hosting domain - and their words are in `pages.json`
  (`hosting:/`, `hosting:/:key`), so they are translated with the rest.
- **Addresses come from `ILambdaAddresses`**, never put together from strings
  elsewhere. The API hands out `publicUrl` (the subdomain) and `address`
  (where to link: the domain while it is served, the subdomain otherwise), the
  frontend `lambdaUrl` (`{key}` in the template) for what has no lambda yet.

### Versions, data, and how they differ

- A **version** is the program: its code *and* its resources (a SPA, for
  example) - and what is written about it, its documentation and tests, which
  are part of its code (below). It never changes once saved. Different
  versions may have different code and different resources.
- **Data** is what the program keeps: the database, the workspace and the
  secrets. It belongs to the lambda and is **shared by all versions**.
  Deploys, rollbacks and merges never touch it.
- Records go in the database and files in the workspace, never in the
  resources. The front end goes in the version, never in the data. API keys
  and passwords go in the secrets, never in code, resources, the workspace or
  the database.
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
- **Schema by Evolve migrations** shipped as resources in `resources/migrations/`, applied as
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

### Code and resources

**Decided by the owner:** a version is two parts, and nothing else.

- Its **code** is every file that is not a resource. Its `.cs` files are
  compiled, in any folder, as a C# project compiles its own - **decided by
  the owner**: sub folders work like in any C# project. Every other file, of
  any kind - the documentation, the tests, what a front end is built from,
  scripts, configuration - is kept with the version and never compiled or
  served. Its **resources**, in `resources/`,
  are what it reads and serves while it runs (`Resources`, formerly `Assets`,
  which stays as an alias so old code compiles).
- **No `.lambda/` and no special kinds.** The documentation and the tests are
  folders of the code by convention, `docs/` and `tests/`, which the editor
  reads for its Documentation and Tests sections and the agents are told to
  keep; nothing else about them is special. Do not bring back a kind of file
  (context, build) with rules of its own.
- **How the rest of the code is arranged is the agent's to decide** - a
  `frontend/` for the sources of a SPA, say. Nothing about it is assumed or
  detected, in the editor or anywhere else.
- **Two allowances, in bytes - decided by the owner.** What a version may
  come to, its code and its resources together (`build-bytes`), and what the
  lambda may keep, its database and its workspace together (`data-bytes`).
  There is no limit on characters of C#: the room bounds what the compiler is
  given. The workspace is compiled with the data's allowance and asks for the
  database's size as it writes; the database is given what the workspace
  leaves (`DatabaseVault.RoomOf`).
- **Lambdas saved before keep working, untouched on disk.** A version or a
  feature is read in the layout it was saved in and translated as it is read
  (`LambdaSource.Parse`: an envelope `"version": 1` is the old layout, 2 is
  today's) - assets at the top move below `resources/`, `.lambda/docs/`,
  `.lambda/tests/` and `.lambda/build/` to `docs/`, `tests/` and `build/`;
  C# that was in `.lambda/` was never compiled, so it keeps a `.txt` ending
  (`tests/Smoke.cs.txt`) rather than be compiled into a lambda online.
  Never rewrite stored versions for it. Their git history keeps its commits;
  the move is a commit of its own on top (see Git). A top-level `assets/` or
  `.lambda/` is refused with a hint where it goes now.
- Names: a C# file's own name as C# names (dots allowed), its folders as the
  rest of the code; a resource as files that are served
  always were (an extension, no dot names, six folders deep); the rest of the
  code as tools name files - dot files and `[]()+@$~{}` allowed, no spaces, no
  `.git`, sixteen folders deep. What a project has at its top
  (`ProjectPaths`, the `Platform` folder, `Dockerfile`, `AGENTS.md`, …) is
  refused there.
- **The platform builds nothing - decided by the owner.** No build on a push,
  no tool run, no check that the program is what its sources build to. The
  agent changes the files, builds where it works and saves them with what was
  built, in one version or feature. Do not add build machinery. The build
  agent has no shell: it writes plain front ends and leaves sources it finds
  alone. The demos have none: they teach with plain files.
- **What a build installs, caches or keeps for itself is kept out by the
  `.gitignore` files of the code - decided with the owner, so it works for any
  technology.** Nothing is refused by name: git follows them in a clone, a zip
  put back follows them (`IgnoredPaths`, git's rules) but keeps what the
  lambda already has, as git keeps what it tracks, and a save that names a
  file keeps it, as `git add -f` would. The root `.gitignore` of a repository
  stays the platform's, generic one, anchored to the root, taking `build/`
  back in (`!/build/`), since many ignore that name everywhere and the lambdas
  from before have their sources there.
- **A zip holds what a commit would - decided by the owner.** It is counted as
  it is sent, before any `.gitignore` leaves something out.
- **One layout everywhere.** A version, its zip, a clone, an export and a
  published source lay the files out the same way, so a build that writes by
  relative paths finds them wherever it runs; `?layout=project` reads a zip
  back as a commit of a clone (the platform's files skipped).
- In the editor, **Code** (full view, Develop group) is every file of a
  version: the code and the resources as two trees beside the editor, with
  the allowance under them - it replaced the Files and Build sections and the
  old code view, and their addresses lead to it. Nothing of the layout is in
  the simple view or on `/build`.
- This is said in the MCP instructions (a line), `platform_guide` (`code`,
  `resources`), the tool descriptions, a clone's `AGENTS.md`, the build
  agent's brief, the README and `/docs` - keep them the same.

### Documentation and tests

Every version keeps what is written about it in its code: `docs/product.md`
(what the app is, for whom, why, what people do with it - in the user's terms;
its first paragraph is the app in a sentence or two), `docs/decisions.md` (the
technical decisions and why), `tests/README.md` (how it is tested
automatically), and beside those more pages, pictures, test scripts and test
data.

- **Files of the version, not fields in the database.** They describe that
  version, so they are saved, diffed, rolled back, copied into a feature and
  merged with it like any file, and travel in the zip and the export. Do not
  move them into SQLite or into the workspace.
- **A convention, not a kind of file - decided by the owner.** `docs/` and
  `tests/` are folders of the code like any other: never compiled, never
  served, left out of what identifies a build, counted towards what a version
  may come to. Only the editor reads them as what they are - markdown pages
  with files beside them, one component for both. Do not build them as two
  features, nor give them rules the rest of the code does not have.
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
  (`documentationAndTests`), the build agent's brief, a repository's
  `AGENTS.md`, the guide and the README - keep them the same.
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
brief, a repository's `AGENTS.md`, the README and `/docs` - keep them the same.
The platform writes none
of it into a lambda's pages; the agent does.

- `og:image` takes a full address - social networks do not resolve a relative
  one - and is the exception to relative links, with `og:url` and the
  canonical link. The warning a feature's deploy gives for a link to the
  lambda's own address, or to its old path below `/lambda/` (`McpTools.Leaks`),
  leaves meta tags and the canonical link out: they name the page, and the
  page never follows them.
- **No renderer - decided for now.** Rendering a lambda in a headless browser
  on the server (for the agent to look at its page, a picture proposed in the
  showcase tab, an `og:image`) was considered and left out: it runs strangers'
  JavaScript in a browser beside the platform, and costs a container,
  Chromium and fonts. Nor are tools added to the build agent's image - it has
  no shell to run them, and must not get one, since its token is in its
  environment. Do not bring either back without the owner asking.
- **The sitemap names no lambda - decided by the owner.** The operator could
  list one, by its address below `/lambda/`; the switch went when the lambdas
  moved to hosts of their own (V20 drops `in_sitemap`), since a sitemap names
  pages of the host it is served from. Do not bring it back on the site's
  host.

### Analytics

An owner can have the visits of their lambda counted - visitors, page views
and requests per day, and the pages, referring sites, countries, devices,
browsers, systems and crawlers behind them - in the editor's **Analytics**
section.

- **Opt-in - decided by the owner.** Nothing is counted until the owner
  switches it on. Switching it off deletes what was counted, like switching
  off a kind of data, so an agent may switch it on when the user asks
  (`analytics` with `enable`) and never off. A demo and a feature's preview
  are not counted.
- **By day, kept for a year, shown per tier - decided by the owner.** Free
  shows seven days, premium the year; the window is a tier limit in the
  panel (`analytics-days`), the year is `AnalyticsStore.Kept`. What is kept
  beyond the window is shown once the lambda is premium, which is the point.
- **One visit per request, read by whoever needs it - decided by the
  owner.** `LambdaVisitConcern` makes the `LambdaVisit` once (status, time,
  bytes, path, caller's country and agent), for previews too, and does
  nothing else; the overview's figures (`LambdaActivityConcern`), the
  request log (`CallerConcern`) and the analytics (`AnalyticsConcern`) read
  it off the request and decide for themselves what is theirs. Anything new
  that wants to know about requests to lambdas reads the visit, and what is
  missing is added to it - nothing measures a request to a lambda again.
  What only analytics need (the referrer, whether a page was opened, the
  client hints) is read only for a lambda that counts visits; what is made of
  it (the referrer's host, the kind of device) is worked out by the job,
  never on the request.
- **Collected on the request, written by a job - decided by the owner.** A
  lock-free queue (`AnalyticsQueue`, bounded) and a job every five minutes
  that sums it and writes one transaction. Losing what is queued on a restart
  is accepted. Do not write to a database while a request is served.
- **A database of its own, a star - decided by the owner.** `analytics.db`,
  `lambdas` and a fact table per statistic keyed by lambda, year, month, day
  (and value). Plain commands, synchronous; migrated by Evolve from
  `Services/Analytics/Migrations`, numbered on their own. Only the switch is
  in the platform's database, so serving reads it from memory.
- **Crawlers by a maintained list and by their addresses, not by guessing -
  decided with the owner.** Bots are told from people by
  crawler-user-agents (MIT, refreshed daily, the release's copy as fallback)
  and counted by its kinds; Google, Bing, OpenAI, Apple and Perplexity are
  named only when the connection came from an address they publish - never
  a forwarding header's - and an unconfirmed claim otherwise. Visitors are
  people only. Browsers, systems and devices come from the agent's tokens
  and Chromium's client hints, and a value the sender wrote is only taken
  where it is one of a known few. Matching stays linear in the length of an
  agent: no backtracking expression ever reads one.
- **A visitor is a person who opened a page - decided by the owner.** One
  the classifier takes for a browser and whose visit was a page view,
  counted once a day with the first page they opened. A request for anything
  else - a script, an API call, `robots.txt`, a path that was not there -
  makes nobody a visitor, so there are never more visitors than page views.
- **Page views are people's, bots behind a switch - decided by the owner.**
  The views of programs are counted apart (`bot_views`, in every table), and
  the editor shows the pages people opened unless the owner switches on
  **Include bots** - beside the ranges, since it changes the figure, its chart
  and the page views' cards alike; remembered in their browser, and worked out
  from the one answer rather than asked for again, and marked "incl. bots"
  wherever it counts them. Where visitors came from is people's whatever the
  switch says - a bot comes from nowhere and would read as typed in - and
  requests and crawlers count every request either way. Days counted before `V2` keep their bots among
  the people of their pages, referrers and countries; their totals and devices
  were filled in.
- **Nothing that names a visitor is kept.** Counts only; the referrer as its
  host; visitors told apart for a day by a hash under a random key per day
  and lambda, held in memory and never written. No cookie, no script in the
  lambda. The privacy page says so; keep it saying so.
- **In both views, in the words of the simple one.** It is about the
  audience, not the program, so unlike the stats it is in the simple view as
  well (after the domain). Charts, not tables: the figures at the top pick
  what the columns below show, the breakdowns are ranked bars, the devices
  one bar of shares; every chart has its values as a table one click away.
  It does not poll - it reads again when the page is looked at again.
- **The breakdowns are grouped by what they count - decided by the owner.**
  Page views (pages, sites, countries, devices, browsers, systems) under one
  heading, requests (what was requested, crawlers) under another: the two can
  be far apart, and a card counting one beside a card counting the other
  read as figures that did not add up.
- This is said in the README (Analytics), `/docs` (Who visits it),
  `platform_guide` (`analytics`), the tool's description and the privacy
  page - keep them the same.

### Pushing, not polling

Agents are told never to poll: a page does not fetch on a timer to see
whether something changed. What changes while a page is open is pushed by the
server - server-sent events where the page only listens (`demo-live`), a
websocket where it talks back (`demo-game`). A poll is a request whether
anything changed or not, on a server every lambda shares. This is said in the
MCP instructions (one line), `platform_guide` (`liveUpdates`), the build
agent's brief, a repository's `AGENTS.md`, the README and `/docs` - keep them
the same.

### The link back

**Decided by the owner:** agents are asked - not required - to put a small
"Made with GenHTTP Lambda" line at the foot of the pages they build.

- **A nofollow link without a domain, plain text with one.** Below the
  hosting domain the line links to the installation's public address
  (`LAMBDA_PUBLIC_URL`, else the address the agent called) with
  `rel="nofollow"`. It was a plain link while the lambdas answered below
  `/lambda/` - a link within the same site, which no link-spam policy is
  about; at a host of its own every lambda is another site, and the same link
  in the footers of many sites is what search engines count as link spam
  unless it says it is no recommendation. A line from before is given the
  attribute with the next change. On a lambda with a domain of its own it is
  plain text: there it would be a link from its owner's site. A lambda whose
  line links and that gets a domain has it made plain text with its next
  change.

- **The user may refuse.** The agent says that it added the link, leaves it
  out or takes it out when asked, and notes in `decisions.md` that it was not
  wanted, so the next agent does not put it back. A change does not add one to
  a lambda that has none.
- **The name and nothing else** as the link's words: no keywords, no badge, no
  script, nothing hidden.
- The platform never writes it into a lambda, and nothing checks for it.
- This is said in the MCP instructions (one line), `platform_guide`
  (`backlink`), the build agent's brief, a repository's `AGENTS.md`, the README
  and `/docs` - keep them the same.

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
  descriptions and answers, the guide, a repository's `AGENTS.md`, and the
  README - keep them the same.

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
- `/source` is a component of its own (`SourcePages`, with its bundle in
  `src/GenHTTP.Lambda.Sources/Frontend`),
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

### Git

Every lambda is a git repository as well: `/editor/{privateKey}/{name}.git`
for its owner to clone and push to - the name only names the clone's folder,
the editor offers the public key - and `/source/{publicKey}.git` for anybody
to clone while its source is published. Served by GenHTTP.Modules.Git from
what the platform keeps (`Services/Git`); there is no repository on disk.

- **One history per lambda - decided.** `main` is the newest version, every
  version a commit tagged `v{n}`, every feature a branch. The published
  source is the same commits without the features' branches, whose commits
  it does not serve either. So nothing only the owner may know goes into a
  commit the platform makes: no specification, no editor key - the message is
  the change line.
- **A commit is made once and kept.** The platform makes the commits of
  versions and of features' saves when the repository is read, not when they
  are saved - saving stays as it was, and a lambda nobody clones costs
  nothing - and keeps their bytes and their files below `/data/git/{lambda}`.
  Never make one again from the version: a newer platform would make it
  differently, and every clone would see its history rewritten. Pushed
  commits are kept exactly as they came.
- **The tree is the project, the export's shape made for a repository.** What
  the platform puts around the lambda (`Program.cs`, the project file,
  `Platform/`, `Dockerfile`, the ignore files, `AGENTS.md`, `CLAUDE.md`,
  `LICENSE` while published) depends on nothing a version decides - so the
  project references SQLite, EF Core and Evolve always and the snippet is
  `async Task<object> BuildAsync()` always - and a push that leaves those files
  alone is right whatever it changed. Keep it that way. A commit whose
  lambda.cs did not change keeps the `Project.cs` before it.
- **A layout that changed is a commit on top, never history made again -
  decided.** The commits made before the code and the resources (`assets/`,
  the old platform files) are kept as they are; the newest version and each
  feature's tip get a commit on top that lays them out as today
  (`GitHistory.MoveAsync`, `moved` in the index), and `main` is that commit. A
  push built on a commit of the old layout is refused, saying to rebase onto
  `origin/main`. Do the same for any later change of the layout.
- **The platform's files are refused, not ignored - decided.** A push that
  changes, adds or removes one, or adds a file with a name a lambda cannot
  hold, is refused saying which and how to undo it; ignoring it would leave a
  commit that says what the lambda does not. Every other file is a file of the
  code or of the resources - a README at the root, an editor's settings.
- **A commit pushed to main is a version, each one.** `main` only moves
  forward, linear - no force, no merge commits, no deletion - and the newest
  is compiled first and refused when it does not compile, as a merge is.
  Pushes go through the services every other door uses (origin `git`), so
  their rules hold. A feature whose tip reaches `main` is merged.
- **A branch is a feature.** Pushed, it starts or replaces one - based on
  the newest version among its commits - and puts its preview online, which
  no visitor sees; `-o merge` merges it, `-o deploy` puts a version online.
  A feature's branch is named once (`features.branch`) and kept when it is
  renamed.
- **The answers never hold the editor key.** Whoever pushed has it, and a
  build log of theirs may be read by others.
- **For whoever works with code.** The clone popover is in the full view only
  - the overview's header, the code section, a draft - and on `/source` in the
  **Code** menu. Nothing about git is in the simple view or on `/build`.
- **`AGENTS.md` is a place the platform's rules are said** for an agent
  working in a clone (`Resources/Export/Agents.md.txt`), in git's terms: keep
  it saying what the MCP instructions and `platform_guide` say.
- A test clones every demo and builds the clone with the .NET SDK, as one
  builds every export; keep it passing.

### Demos

The demos (`DemoCatalog`, files in `Resources/Templates`) exist to show agents
how to write apps and to give them a good starting point.

- They are **read-only by definition** (tier `Demo`, enforced in
  `LambdaGuard.EnsureEditable`, whichever door the request comes in). Their
  editor key is public. Anyone starts an editable copy with `create_lambda` and
  a demo as template.
- They follow the **best practices of the GenHTTP framework**
  (<https://genhttp.org/documentation/content/>) and use its **high-level APIs
  wherever possible** rather than inventing their own functionality.
- **When you add functionality to the platform, add it to the demos as well if
  it makes sense**, so agents learn it by reading them. `demo-crud` is where a
  lambda offers tools to AI agents (`Tools.cs`, over the same store as its API)
  and describes its API in the words of the documentation attributes. Every demo has the three
  pages of its documentation and tests and a script its tests run
  (`EveryDemoSaysWhatItIsWhyAndHowItIsTested`); a new demo gets them too. They
  keep their records in the database, migrated with Evolve and read and
  written through Entity Framework Core, and files
  (`demo-files`' uploads) in the workspace; the seeder switches the database on
  (`LambdaDemo.Database`), and a copy starts with one of its own. A demo that
  reads a secret is given a random value by the seeder (`LambdaDemo.Secrets`)
  and works without one, so a copy runs before its owner switches secrets on
  (`demo-registration`, `PASSWORD_PEPPER`).

### Blocking

What the operator does about abuse - a lambda that phishes, spreads malware or
attacks others. It is the operator's alone, in the panel, in English like the
panel.

- **A blocked lambda is offline and frozen.** Blocking takes it offline with
  its features' previews, and every change is refused, through every door, in
  `LambdaGuard.EnsureUnblocked` - which a new door into a lambda calls too
  (`LambdaError.Blocked`, 403, a message that tells an agent to stop and the
  user). The operator is refused as well but for lifting the block and
  deleting it: anything else would put back online what it was blocked for.
- **Reading and the export stay**: the code belongs to its owner. The owner is
  told it was blocked - a banner in both views of the editor, which leaves out
  what would only be refused (the Change, Showcase and Open source sections,
  putting a version online) - **and why - decided by the owner**: the
  operator's reason, as they wrote it, is in that banner and in `blockReason`
  of the lambda, since an owner may understand why it was blocked. The panel
  tells the operator so as they write it. Not in the refusals, which may end
  up in a build log others read; they leave it to the editor.
- **What it shows the public goes with it**: offline, out of the showcase, its
  published source not found - kept, and back once the block is lifted.
- **Kept until the operator decides**: a blocked lambda is never removed for
  going unused, since removed its key would be free again for whoever it was
  blocked for. Lifting the block leaves it offline and starts a free lambda's
  time again. A demo cannot be blocked.
- **The addresses a lambda was changed from are recorded with it**
  (`EditorAddresses`, `editor_addresses`): wherever it is created, saved, put
  online or asked for as a lambda its owner may change, through any door, at
  most hourly, for 30 days after each was last seen - only where the server
  keeps addresses at all (`LAMBDA_LOG_CLIENT_ADDRESS`), never the operator's
  own requests in the panel. The privacy page says so; keep it saying so.
- **Never the installation's own networks** (`LAMBDA_UNBLOCKABLE_NETWORKS`,
  loopback and the private, link local and shared ranges by default): the
  build agent reaches the MCP from a container on one with a fresh address each
  build, and blocking it would stop every build. The connection's address,
  never a forwarded one: a header is written by whoever sends it.
- **A blocked address is refused before anything else** - the outermost
  concern (`AddressBlockConcern`), ahead of the redirects and the request log,
  every lambda's host as well as the platform's - with a plain 403 that is
  counted against its block in memory rather than logged. An IPv6 address is
  blocked with its /64. Held in memory and asked of every request, so it costs
  nothing while no block is held.
- **The panel stays reachable from a blocked address - decided by the owner**
  - its bundle below `/admin`, `/api/v1/system` and the API behind the token -
  so an operator who blocked themselves can lift it. Nothing else is let
  through.
- **Two weeks, by a job - decided by the owner.** `AddressBlockJob` lets go
  of every block two weeks after it was made (`IAddressBlocks.Kept`); blocking
  an address again starts its time again. Addresses change hands, so a block
  that held for good would one day refuse somebody who did nothing.
- The panel's **Blocked addresses** section lists the blocks with why, the
  lambda, until when and what each refused, marks the operator's own and lifts
  them. This is said in the reference (Blocking), `platform_guide`
  (`theLambda.blocked`), the API's introduction, `/docs` (What it will not let
  you do), the terms and the privacy page - keep them the same.

### Tiers

`Free`, `Premium`, `Demo` (`LambdaTier`), assigned by an administrator and never
chosen by the owner.

- **Free** is for trying: taken offline when unused, removed when abandoned.
- **Premium** is for running something in production, for example a website on a
  custom domain: kept online, larger limits. The simple view's overview offers
  it to every lambda that is not premium (or a demo), in a tile beside the
  latest change; its only action is a mail to the installation's contact
  (`LambdaOptions.Contact`) naming the app by its public address, never the
  editor key - the tier stays the operator's to assign. Where the installation
  names no contact - a community one, usually - the tile is not there.
- **Kept as they are in the community edition for now - decided by the owner**;
  they are to be reworked later. The README tells whoever runs one that a free
  app nobody uses is taken offline, and how to keep it.
- **The limits of a tier are product settings, not server configuration -
  decided.** What a version may come to (its code and resources together),
  what the data may (its database and workspace together), versions and
  features per tier, the free tier's lifetime, and the per-caller limits (showcase picture,
  requests per second, builds per day) are set in the panel's **Limits**
  (`LimitsService`, rows in `settings`): one form, the tier limits as a table
  with a column per tier and the per-caller ones in a block of their own, so
  which is which is plain without a switch hiding half of it.
  A change applies to what is checked next and takes nothing away. Ports,
  directories, keys, buffers, `LAMBDA_MAX_CONCURRENCY` and the execution
  timeout stay environment variables: they protect the process. The old
  limit variables give the defaults for one release, with a startup warning,
  and are then removed. Callers ask `ILimitsService`, never `LambdaOptions`,
  for a limit.
- **Enterprise** - a customer running their own instance, on-prem or in the
  cloud - is planned but **do not anticipate enterprise features unless asked**.
  It is not decided that the enterprise code base will be the same as the cloud
  one, so do not shape the cloud code for it. The page with its prices was
  removed from the cloud - decided by the owner, with the panel's switch for it
  (V23).

## Architecture

Read the reference's section "How it is put together" before touching
services. The rules that matter:

- The platform's services are one project, `GenHTTP.Lambda.Core`, divided into
  services behind interfaces; every component talks to the interfaces. `Meta`
  is the only part that speaks to the database, and its public surface is
  hand-mapped DTOs. Code lives on the file system (`Storage`), never in the
  database. The components over it, and the hosts that put them together, are
  in Editions and components above.
- **A service orchestrates, and the concerns it would mix are units of their
  own - decided in the architecture review.** A protocol, a count, a check:
  each goes into a class beside the service, in its folder, and the service
  keeps deciding what is done and in which order. Not a class per method - a
  unit is a concern somebody can review on its own. The resources still talk
  to the service. `Services/Building` is the template: `BuildService` over
  `AgentClient`, `BuildAllowance`, `ModelGate` and `AgentInput`. In
  `Services/Meta`, `MetaService` keeps the face (`IMetaService`) and the
  lambda's turn, and hands hosting, history, lifetime, removal and the DTOs to
  units of their own. A rule another service needs too - what a version may
  hold, who may change a lambda - goes into a unit both call, not a static of
  the service they reach into.
- **Outside its folder, a service is taken only by its interfaces - decided by
  the owner, enforced by `ServiceBoundaryTests`.** That holds for the API, MCP
  and every other service alike; the units of a service have no interface and
  stay in its folder (`Services/<Name>`). A folder has an interface per
  audience: the service the API and MCP call (`IDatabaseService`,
  `ISecretService`), and where other services work through a lower layer, that
  layer's own (`IDatabaseVault`, `ISecretVault`, `IDomainRegistry`,
  `ILogBook`). What another folder needs of a unit is added to the service's
  interface (`IMetaService.GetFacts`, `ISourceService.Star`), not taken from
  the unit. An exception is an entry in the test with its reason; today only
  the platform's composition (`LambdaPlatform`, its builder and
  `PlatformLogging`) has one. The test reads every component's assembly - a
  component is outside every folder of the services.
- Compiled lambdas run in the server process. The `CodeGuard` raises the cost of
  misbehaving; **it is not a sandbox.** Do not describe it as one.
- Database: SQLite, EF Core, migrated by Evolve from `Data/Migrations/V<n>__*.sql`.
  Existing migrations are never edited. **Before pushing a new migration check
  the highest `V` on `origin/main`** - a duplicate fails every test in CI. New
  files under `src/GenHTTP.Lambda.Core/Data/` are hidden by the `data/` rule in
  `.gitignore` on Windows: check with `git status --ignored` and `git add -f`.
- The frontends are React applications in an npm workspace at the root: the
  kit (`src/Kit`) and a bundle per component in its `Frontend/` folder (see
  Editions and components). The sites' public pages are prerendered per
  language.
- Links inside a lambda's front end are relative, never a full address: a
  lambda answers at the root of its address - its host, or `/lambda/{key}/`
  where lambdas answer by path - but a feature's preview below
  `/features/{key}/` on the site, where a leading slash leaves it and the full
  address is the live lambda.
- The full view's sidebar is **grouped** (overview and documentation; sharing -
  showcase, open source, domain - second, as the owner decided; develop -
  change, drafts, code, tests; program and data - data, versions; run). A new
  section joins the group it belongs to rather
  than the end of the list.
- **Wired through the container**, not by hand and not in statics. A concern
  that needs nothing but services is an `IDependentConcern`, registered as a
  singleton and added with `Dependent.Concern<T>()`; only a concern built with
  an argument of its own is built by hand (the lookup of a lambda, whose
  locator differs per route). The scope they are resolved from is opened by
  the outermost concern of the host (`AddDependencyInjection`, added by
  `PlatformHosting`), so nothing dependent goes outside it. A component's own
  singletons - the MCP tools, the pages of a bundle - are made once from the
  platform's services by its builder (`ActivatorUtilities`); the container is
  Core's and registers nothing of the components. What only a door needs -
  HTTP or MCP - stays at the door (`WorkspaceFiles`); what both doors share
  is the services' (`OperationLog`, `LambdaAccess`, the analytics' wire
  shape), never one door reaching into the other. What is done with the data
  (base64, a version's facts) is the service's. What is one per process, the
  console, is installed by a service the platform owns (`ConsoleCapture`), so
  tests own it as well.

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
- **Enforced when it compiles.** `src/BannedSymbols.txt`
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
- **The MCP module's own lines are filtered below warnings** (`PlatformLogging`): it
  writes one per tool call beside the tool's own, and one per refusal. A tool
  that fails is an error, and is written down.

### Frameworks: GenHTTP and Ioxide

- We run on GenHTTP and its Ioxide engine. **If you find a bug in either, flag it
  clearly** - say what it is, where, and a reproduction - **instead of working
  around it**, so it can be fixed at the source. Tell the owner in your answer;
  do not bury it in a comment.
- **Use what the framework offers the way it is meant to be used - decided by
  the owner** ("embrace it"). When GenHTTP brings a module for something the
  platform does by hand, move onto it rather than keep a protocol of our own
  beside it; what it lacks is a bug or a feature to ask for at the source.
- **The MCP endpoint is GenHTTP's MCP module.** `/mcp` is `McpEndpoint` over
  `GenHTTP.Modules.Mcp`, the tools are the methods of `McpTools` marked
  `[Tool]` - their `[Summary]` the description, their typed parameters the
  arguments - and a fixed limit of an argument is an attribute on it (`[Range]`,
  `[MaxLength]`, `[AllowedValues]`), advertised and checked by the module, not
  a check of our own. The module takes every exception but its sealed
  `ProviderException` for a fault, so a refusal of the platform
  (`LambdaException`) is turned into an error that says why in a sentence
  (`Refusing`), never left to be logged as a crash. Its protocol - versions,
  notifications, origins - is the framework's and is not tested here; what we
  configure (`LAMBDA_MCP_ORIGINS`, the instructions) is.
- **Do not write tests for the frameworks.** Test GenHTTP Lambda only.
- The framework source is normally checked out beside this repository
  (`../GenHTTP`). Read it instead of decompiling the NuGet packages.
- Known and already dealt with: GenHTTP releases the request headers once a body
  is bound, so check headers in a concern in front of the resource, not inside
  a body-taking method (see `AdminAuthentication`). Since 11.3 the framework
  binds path and query parameters before the body whatever their order, so
  the order of a method's parameters no longer matters. A new instance of the same kind of
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
npm run build      # at the root: type checks and builds every bundle; CI runs it too
```

- The fixture puts the components together the way the cloud host does, on a
  test host, with a stand-in bundle per component (`LambdaFixture.BundleOf`);
  path addressing is tested with `Addressing = LambdaAddressing.Paths`
  (`PathHostingTests`). Build the frontend before the server when a page is
  checked by hand: a project copies its bundle into the host's output when it
  is built.

On Windows the default Ioxide engine (io_uring) does not run: start the server
with `LAMBDA_ENGINE=Kestrel`.

## Translations

- **Everything a visitor or owner sees is translated into all supported
  languages, except the admin panel** (`/admin`, its bundle in
  `src/GenHTTP.Lambda.Administration/Frontend`), which is English only; the
  shell around it reads the kit's words. That includes both sites (the cloud's
  landing, `/build`, `/ship` and legal pages; the community's front page), the
  guide, the showcase, the published sources, the create page, the editor, the
  runner's hosting pages and the page metadata in every table of pages.
- **Every bundle has its own catalog** (`<component>/Frontend/src/locales/<language>/`),
  and the kit's (`src/Kit/src/locales`) holds what several share: the shell,
  what is said in many places, connecting an agent, the build box, the guide,
  the showcase and the licenses. A bundle's site catalog is the kit's words
  with its own (`defineSite`); the kit's components read the kit's part.
- Languages: `id de en es fr it nl pl pt pt-pt tr ja ko` (`src/Kit/src/i18n/languages.ts`,
  mirrored in `GenHTTP.Lambda.Web/SiteLanguages.cs`). English is the source;
  every other catalog is typed as it, so a missing string fails the frontend
  build. A new language is added in both places, in every catalog, in every
  table of pages and in the social images.
- The editor's words are a file per section and language
  (`src/GenHTTP.Lambda.Editing/Frontend/src/locales/<language>/`), named
  after its key, and an `index.ts` that puts them together - so a change to
  one section touches one file per language. A language's grammar helpers for
  the editor live beside them in `language.ts`. A new section gets its file in
  English, its line in every `index.ts`, and its translations from `apply`,
  which creates the other languages' files.
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

1. Write the English - the `en` folder of a catalog and the English titles
   and descriptions of a table of pages (the script lists them all: `CATALOGS`
   and `TABLES` in `src/Kit/scripts/translations.mjs`; a new catalog or table
   is added there). A new sentence whose place is not plain from its key gets
   a doc comment saying where it appears: the translator sees the comment,
   never the page.
2. `npm run translations -- extract` (at the root) compares the English
   with the base of the branch and writes requests into `.translations/`: per
   language its register from the table above, its fixed words from
   `src/Kit/src/locales/glossary.json`, and each new or changed sentence with what the
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
same change that changes the feature.** There are five places, and a feature
touches all that apply:

1. `docs/reference.md` - routes, API, configuration, operations, how it fits
   together. Where the other documents say "the reference", they mean it.
2. `README.md` - the community edition for whoever runs it: what it does, in
   plain words and as features rather than routes, how to run it with Docker,
   connect an agent and the build agent, and what to know before letting
   strangers build there. Not a reference: it links to one.
3. `/docs` (`src/Kit/src/locales/*/guide.tsx`) - for owners, in the
   vocabulary of the simple view, translated.
4. `platform_guide`, the MCP instructions and the tool descriptions
   (`GenHTTP.Lambda.Mcp`) - for agents; and `AGENTS.md` in every lambda's
   repository (`Resources/Export/Agents.md.txt`) for an agent working in a
   clone.
5. The API description (Scalar at `/api/v1/scalar/`, from the resources) -
   see below.

The same rule is worded the same way in all of them. Where an older rule
here says "the README" for a feature, it means the reference.

The MCP instructions (`McpEndpoint`) are **short**: how to get from nothing to
something online, how a lambda that exists is changed, and the rules an agent
would otherwise break before it reads the guide (where data goes, the
database, waiting for tasks, relative paths, secrets, documentation, pages
meant to be found). The rest
is in `platform_guide` and the tool descriptions; do not repeat it there.

### The API description

The OpenAPI document (`/api/v1/openapi.json`, browsed in Scalar) is written for
whoever calls the API - an agent or a developer writing a client - with
GenHTTP's documentation attributes beside the code they describe.

- **Behaviour, not just what - decided by the owner.** An operation says what
  it changes and what it leaves alone, every way it is refused and why
  (`[Response]` with `ErrorResponse` for the platform's own refusals), what it
  answers with (`[return: Summary]`, `[Response]` for 201, 202 and 422), and
  bodies and downloads that are not JSON (`[RequestBody]`,
  `ContentType`, `[ResponseHeader]`). Read the service before writing it.
- **The attributes for the caller, the XML comments for the maintainer.** A
  `[Summary]` and `[Remarks]` say what the caller needs; `<remarks>` keep the
  *why* for whoever changes the code. The same sentence is not in both: a
  record's fields are described by `[Summary]` on its parameters, not by
  `<param>`.
- **A summary is a short label** - Scalar lists the operations by it - and
  the rest goes into the remarks.
- **Tags are concepts, not classes**, in the order and groups of
  `ApiDocumentation` (`x-tagGroups`); a tag shared by several resources is
  described on one of them. The introduction everybody reads first is
  `src/GenHTTP.Lambda.Api/Resources/Introduction.md`. The wording every route shares (the editor
  key, a demo's refusal) is a constant of `ApiDocumentation`.
- **Limits the operator sets are never written as numbers**: say where to
  read them (`GET /system`, the summary's `limits`). Fixed ones (100 secrets,
  32 KB a value) may be. The same holds for the MCP tools, whose descriptions
  are fixed when the endpoint starts: a limit of the panel is named where to
  read it (`platform_guide`), never written into one.
- **Authentication describes itself.** The admin token's concern declares the
  `adminToken` security scheme and the 401 and 403 it answers; an operation
  behind it says nothing about the token of its own. Everywhere else the
  editor key in the path is the credential, and no operation names a scheme.
- `ApiDocumentationTests` refuses an operation without a summary, a parameter,
  answer, model or field without a description, a call about a lambda that
  does not say how an unknown key is answered, and a tag in no group. A new
  endpoint or field is documented in the same change.

## Working conventions

- The code is written to be read: XML documentation with `<remarks>` that say
  *why*, `#region` blocks per concern, braces on their own line. Match the file
  you are in.
- Comments say why, not what. Write prose, not marketing.
- Tailwind for styling, both light and dark themes, phones included.
- Check a change you cannot test in the browser by running the app (see above)
  and looking at it; type checks and tests are not proof that a UI works.
- PR screenshots: one branch `pr-assets`, unrelated to `main`, with a folder
  per topic (`<topic>/<file>.png`), a commit added on top for each PR and
  linked from the PR body by commit-pinned raw URLs - decided by the owner;
  no branch per topic.
