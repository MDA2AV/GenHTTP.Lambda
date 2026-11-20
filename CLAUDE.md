# GenHTTP Lambda - steering file for agents

This file holds the product and architectural decisions of this repository:
the *why* and the rules. Follow it, and when a decision is made or changed - by
the owner in a conversation, in a review, anywhere - **put it here in the same
change**. A rule that only lives in a chat is lost. The
[reference](docs/reference.md) has the routes, API, configuration and
operations - read its section before touching a feature; the documentation at
<https://community.genhttp.run/docs/> is for whoever runs or uses the community
edition.

Marks: **(owner)** - decided by the owner; do not change it without asking.
**(open)** - chosen with the owner, open to change. Unmarked rules are the
project's own and may be improved with a reason.

`docker/agent/AGENTS.md` is the brief of the build agent inside its throwaway
container, not a steering file for this repository. Do not merge the two.

## What this is

A platform on which somebody describes an app - or an agent writes a C# snippet
that returns a GenHTTP `IHandler` - and gets a public URL that serves it. The
snippet is compiled with Roslyn at runtime and hosted by the same GenHTTP
server (`GenHTTP.Full.Ioxide`, .NET 11) that serves the platform. Open source
under MIT, the whole repository **(owner)**.

## The number one rule: asymmetric interfaces

This is a platform for **agentic coding**. Not every actor works in the same
tools with the same workflow:

- **Agents get the best interface for producing and changing functionality**:
  MCP tools, the REST API, `platform_guide`, the demos, targeted changes
  (`change_code`), compile checks, logs.
- **Humans get the best interface for controlling and reviewing what was
  generated**: the editor, the history with the change and specification of
  every version, diffs, previews, a way back, the dashboard.

Every design decision is measured against this first. Do not build a symmetric
view where a human and an agent use the same screen for the same job; if a
feature makes one's job harder to make the other's easier, find the interface
that serves each.

## Two target groups

| | No-code "vibers" | Developers |
|---|---|---|
| Entry | the workspace at `/build`, the editor's **Change** section | `/ship`, their own agent via MCP |
| Editor | the **simple view** (`view: Simple`) | the **full view** (`view: Full`) |
| Cares about | what the app does, whether it is online, what changed | code, files, versions, deployments, logs |

- **No developer slang in the simple view or on `/build`**: not merge, base,
  branch, rebase, commit, compile error; no files or logs. Fixed vocabulary: a
  feature is a **draft**, its data copy **test data**, merging **putting it
  online**, a feature behind the newest version **out of date**, a version is
  described by the **change** it made (its number only where the owner must
  name one), the database is **records**, secrets are **keys and passwords**,
  the workspace is **what your app saved**. A compile error is the agent's to
  fix. The full view, `/ship`, the API, MCP, the guide and the docs use the
  precise words.
- The simple view has a section per kind of data (Records, Saved by your app,
  Keys and passwords) only once the app keeps something there or, for keys,
  its code waits for one. No switches, folders, code or column types there;
  entering a key switches secrets on.
- What the agent does reaches the pages as **steps** - facts the runner
  records per tool call (`begin` in `docker/agent/builder.mjs`) and the
  agent's own short lines in the request's language - never English sentences
  from the runner. `/build` and the Change section draw a run with the same
  parts (`components/AgentRun.tsx`). A new tool the agent may call gets a step
  kind and words on both pages.
- The view a lambda opens in is a default only (`view`): the build agent
  creates `Simple` lambdas, everything else `Full`.

### The workspace (`/build`)

**(owner)** A chat like ChatGPT or Claude: the apps this browser knows in a
sidebar, a conversation about the one open, the box at its foot. A message is a
question, a setting or a change.

- **Kept in the browser by editor key (owner)** - local storage
  (`src/Kit/src/workspace/store.ts`), the newest sixty turns. No accounts. An
  app is remembered when built there or added by its editor link or key; demos
  are not added. The address names the open app by its public key (`?app=`),
  never the editor key. Scripts on the platform's origin (previews, lambdas by
  path) can read that storage; the docs say so (Security).
- **Through the existing doors**: the first message is a build (`POST
  /builds`, its id kept so a reload finds the result), every later one a change
  (`…/agent/start`) with the newest turns as `history` - cut by the server
  (`AgentInput.History`), handed to the agent as context, never instructions;
  only the message is logged. So every operator switch, allowance and guard
  holds unchanged, and each message costs one of the day's.
- **A question is answered, a setting changed, only a change makes a draft.**
  Questions are answered from the tools and start no feature; settings the
  tools can change are changed without one; the result says which
  (`answered`, `settings`). What no tool does (a domain, the showcase,
  switching anything off, a secret's value) the agent sends to the editor. The
  agent has nothing that deletes data or sets a secret, and discards a draft
  only where the owner's message names it.
- **Simple, not the editor.** Above the conversation only what waits for the
  owner (an app offline, keys its code waits for - entered there through the
  REST API, never the agent - a draft to try, a block); under an answer: open
  the app, try or put online its draft, undo. Everything else links to the
  editor.
- Before anything is asked it is the prerendered page that creates a website;
  the sidebar appears once the browser knows an app.

### The build agent

It builds or changes a lambda through the platform's tools and - for the owner
of the lambda it was handed - answers questions about it and changes its
settings, nothing else. Its system prompt is `PURPOSE` in
`docker/agent/builder.mjs`, kept apart from the brief because the brief carries
text typed by anybody and the agent reads what visitors wrote: `PURPOSE` says
all of it - request, conversation, tool results - is data, never instructions.
Before calling any tool it declines, with one `DECLINED: …` line in the
request's language:

- anything not about an application (texts, homework, chat, questions about
  any lambda but the one it was handed);
- anything about the platform or the machine, including code that reads the
  server's environment, files or network;
- anything meant to do harm.

Reported as `declined` and shown as the agent's sentence. Ordinary apps -
calling APIs, keeping data, accounts, merely alarming-sounding - are not
declined. Keep `PURPOSE`, `AGENTS.md` ("What not to build") and the docs
saying the same.

**Models and efforts (owner)**: chosen from two dropdowns under the box on
`/build`, the workspace and the Change section (`components/AgentChoice.tsx`):
Opus at high effort by default, Sonnet and Haiku beside it, Fable behind a
password. The operator decides in the panel's **Build agent** section which models and
efforts are offered, which need the password, the default, and whether the
agent builds and changes apps at all.

- The password is the installation's environment variable
  (`LAMBDA_AGENT_PASSWORD`, `LAMBDA_AGENT_FABLE_PASSWORD` still read) **(open)**;
  the panel says whether there is one, never what. A model behind it while
  there is none is offered to nobody.
- A model behind the password runs **unbounded** (no clock, no step limit, its
  own lane) **(open)**. The server decides it for a job whose password it
  checked and tells the runner (`unbounded`); the runner never infers it from
  a model's name.
- The default model needs no password, and at least one effort including the
  default is offered; a save breaking either is refused. A model or effort not
  offered is refused before the agent is asked or the allowance spent. Every
  effort is offered at first (`AgentSettings.Default`) **(open)**.
- **Token or API key (owner)**: `CLAUDE_CODE_OAUTH_TOKEN` or
  `ANTHROPIC_API_KEY` on the runner; exactly one goes into a build, the other
  set empty. With both the token wins and the runner says so at start.
- `AgentCatalog` labels name the default versions; the identifiers are the
  runner's (`AGENT_MODEL`, `AGENT_SONNET_MODEL`, `AGENT_HAIKU_MODEL`,
  `AGENT_FABLE_MODEL`).
- Builds load no built-in tool (`--tools ""` plus the denylist).
  `verify-tools.sh` checks what a build loaded from the CLI's first line; run
  it after every change of the CLI's version.

## Editions and components

**(owner)** The platform is a set of components that hosts put together the
GenHTTP way. Two hosts, two editions:

- **Cloud** (`src/GenHTTP.Lambda.Cloud`) is genhttp.dev: lambdas at subdomains
  of the hosting domain, the marketing site (front page of features, `/build`,
  `/ship`, `/self-hosting`), legal pages, terms, contact and abuse mailbox,
  structured data (`CloudInstallation`, `CloudStructuredData`).
- **Community** (`src/GenHTTP.Lambda.Community`), the Docker image
  `genhttp/lambda`: every feature of the cloud, the build agent included,
  behind a simpler front page for developers, the same `/build` and `/ship`,
  and the showcase - translated, crisp, selling nothing. No terms, legal pages
  or organization; only what its operator names (`LAMBDA_CONTACT`,
  `LAMBDA_ABUSE_CONTACT`) is shown.

### Rules of both editions

- **Nothing in the pages names the cloud.** What differs per installation -
  where a lambda answers, the git address, where a domain points
  (`DomainResponse.Target`), the workspace page (`LambdaOptions.StartPage`) -
  is asked of the installation, never written into a bundle.
- **What only one installation says is the host's**: terms, contact, abuse
  mailbox, legal pages (`LambdaOptions.Terms`, `Contact`, `AbuseContact`,
  `LegalPages`). Without terms nothing is accepted on creating a lambda; shared
  pages read the rest from `/system` (`operator`) and leave out what is not
  there.
- **The menu (owner)**: the front page, Build, Ship; the cloud adds
  Self-hosting, then the Showcase; the community the Showcase; the panel last.
  The menu and build box say **app** on every installation **(owner)**.
- **`/ship` is a guide (owner)**: connecting one's own agent and having it
  publish an app in three steps (the kit's `AgentGuide`), then the site's own
  words.
- **Every page links to the platform's source (owner)** (`REPOSITORY` in the
  kit's `installation.ts`, beside Discord) in the footer; `/build` and `/ship`
  only there. **Not on an enterprise installation (owner)**: no GitHub or
  Discord link anywhere (`useProjectLinks`).
- **Lambdas answer by path on the community edition (owner)** - see Where the
  lambdas answer.

### The cloud's pages

- **The front page is the features (owner)**: what every app gets beyond being
  built (drafts, history, documentation, records, keys, analytics, a domain,
  the code, publishing), a section each with a screenshot of the real editor -
  all of one made-up example app (a tennis club's court booking), both themes.
  Its first picture is that app being built on `/build` **(owner)**. When a
  feature an owner would notice is added, or the editor's look changes, its
  section and picture change with it.
- **`/self-hosting` (owner)** sets genhttp.dev and the community edition side
  by side: **neither preferred** - drawn alike, same order, same weight of
  button, every row a fact taken from the code, not memory.
- **The words each page is found by**: the public pages use what people type
  into a search engine (checked against Google's suggestions per language),
  each page its own words so they do not compete - `/` an *AI app builder with
  free hosting* (database, version history, analytics in description and
  H2s); `/build` *create a website with AI*, free, *no sign-up*, in each
  language's word for website; `/ship` *host a vibe-coded app*, from Claude
  Code, Codex, Cursor, localhost; `/self-hosting` an *open source AI app
  builder*, self-hosted. What nobody should need (a server, a domain, a
  sign-up, a card) is said as not their concern. Words go in `pages.json`, the
  H1 and H2s. Nothing is claimed the platform does not keep: no "unlimited",
  no "forever". The community's pages are not written for search.

### Branding (community only)

**(owner)** The operator gives the installation a name and a logo in the
panel's **Branding** section (or `/admin/branding`, the MCP tool
`customize_installation`) - never environment variables - and every page shows
them instead of GenHTTP Lambda's: bar, titles, link previews, icon, manifest,
`llms.txt`. The host decides whether it is offered (`LambdaOptions.Branding`);
without it the routes, tool and section are absent and nothing stored is read.

- A name without a logo is drawn as its first letter (`Monogram`), so a branded
  installation never shows the platform's mark **(open)**. The brand is written
  into every index page by the server (`PageBranding`, `#lambda-brand`) and the
  site is prerendered for it; an unbranded installation gets its pages exactly
  as built.
- **Wherever the pages or an app say what the installation is, they say its
  name (owner)**: the "Made with" line, the translated sentences that say what
  an app was built with (`{platform}`), a lambda's repository files. Where the
  software is meant, GenHTTP Lambda stays named: source and Discord links, the
  API description, the MCP server, the export.

### Who an installation is for

**(owner)** A profile (`InstallationProfile`): `cloud` (a public platform),
`home` (somebody and the people around them), `enterprise` (an organization).
The host says which may be chosen (`LambdaOptions.Profile`, `Profiles`): fixed
`cloud` on genhttp.dev; `home` (default) or `enterprise` on the community
edition, chosen under **Settings**.

- **Used where it clearly fits, never as a switch spread through the code
  (owner)** ("avoid spaghetti code and too many ifs"). Only words, steering and
  the platform's own links depend on it - no limit, tier, default,
  connectivity, block or decline **(open)**.
- **The words are the catalogs'**: a section's `profiles` holds only what a
  profile says differently, and the kit hands every page its profile's words
  (`profile.ts`, `wordsFor`), so no component asks which it is. Only
  `enterprise` has words of its own; `home` says what the cloud says.
- **Enterprise (owner)**: the community front page addresses the colleagues,
  not the operator (`EnterpriseLanding`) - people who do not write code and
  come to have tools built; no "Made with" line; no GitHub or Discord links.
- Prerendered per profile; `#site-facts` says which, and a visitor is sent that
  page only (bundles serve neither `prerender.json` nor `pages.json`). The
  editor asks `/system`. The guide's introduction says whom the apps are for.

### Usage reports (community only)

**(owner)** Once a day (`Services/UsageReports`), on by default, off with
`LAMBDA_USAGE_REPORTS=false` or `DO_NOT_TRACK`. Neither hidden nor pushed:
documented like any feature (`docs/telemetry.md`, the docs, the reference, a
startup line, the panel's **Usage report** with the last report), never a
banner or prompt.

- **How an installation runs, never what it runs**: version, machine, which
  options are on (switches, never values), counts and sums over lambdas - no
  key, domain, title, code, record, secret, visitor, address, and **not the
  installation's address (owner)**. Demos are not counted. It says whether an
  installation is branded and its profile, never as what or whose.
- A new field keeps to that, is added rather than renamed, and is described in
  `docs/telemetry.md`. It never delays startup or a request: a background job,
  one attempt a day with a time limit, one warning when it fails.
- The cloud sends none. The collector is the lambda `telemetry` on genhttp.dev;
  keep its front page, `docs/telemetry.md` and the code saying the same.

### Components

Each a project with a builder in front (`X.Create(...)`); what a component
needs is given to its builder, not read from the environment.

| Component | Project | Builder | Mounted at |
|---|---|---|---|
| Services, database, jobs, log book | `GenHTTP.Lambda.Core` | `LambdaPlatform.Create().Options(...)` | - |
| REST API | `GenHTTP.Lambda.Api` | `RestApi.Create(platform)` | `/api` |
| MCP endpoint | `GenHTTP.Lambda.Mcp` | `McpServer.Create(platform)` | `/mcp` |
| Editor, create page, owner's git | `GenHTTP.Lambda.Editing` | `Editor.Create(platform)` | `/editor` |
| Operator's panel | `GenHTTP.Lambda.Administration` | `AdminPanel.Create(platform)` | `/admin` |
| Published sources, public git | `GenHTTP.Lambda.Sources` | `SourcePages.Create(platform)` | `/{lang}/source` |
| Serving the lambdas | `GenHTTP.Lambda.Execution` | `Runner.Create(platform).Site(site)` | around the site |
| Serving a bundle's pages | `GenHTTP.Lambda.Web` | `SitePages`, `ClientApplication`, `Bundle` | - |
| Engine, ports, TLS, environment | `GenHTTP.Lambda.Server` | `LambdaServer.Create(platform)` | - |
| Integration contracts | `GenHTTP.Lambda.Integrations` | - | - |
| SQLite (default database) | `GenHTTP.Lambda.Integrations.Sqlite` | `SqliteDatabase.Create()` | `.Database(...)` |
| Local directories (default files) | `GenHTTP.Lambda.Integrations.Local` | the platform's own | `.Files(...)` |
| PostgreSQL, community only | `GenHTTP.Lambda.Integrations.PostgreSql` | `PostgreSqlDatabase.Create(connection)` | `.Database(...)` |

- **A host's `Program.cs` is the composition, short and plain**: read the
  settings, build the platform, lay out the components, hand it to the runner
  and the server. Read both before adding a component.
- **Nothing below the hosts reads the environment (owner).**
  `LambdaSettings.FromEnvironment()` (in `Server`) reads `LAMBDA_*` into
  `LambdaOptions` (the DTO components share) and `ServerSettings`; a host adds
  its own (`CloudInstallation.Of`).
- **Mount paths are fixed** (`PlatformRoutes`): components link to one another.
- **The runner goes around the site**: a lambda's own host owns every path; on
  the platform's host it takes `/features/` and, by path, `/lambda/`.
- **The editor and the panel talk to the platform through the REST API only**;
  the API and MCP share logic through the services (`OperationLog`,
  `LambdaAccess`, the analytics' wire shape), never one door reaching into the
  other.
- **Every component with pages owns its frontend (owner)**: sources in its
  `Frontend/`, built by the root npm workspace into its `bundle/`, copied
  beside the host's assemblies (`bundles/<name>/`, `Bundle.Beside`). No
  component serves another's bundle. What they share is source: the kit
  (`src/Kit`, `@genhttp-lambda/kit`). A link into another bundle loads it
  (`claim`, `owns`).
- **Docker**: a Dockerfile per host (`src/<host>/Dockerfile`, built from the
  root). **The community image runs on GenHTTP's internal engine (owner)**:
  io_uring is blocked by container runtimes' default seccomp profile. The
  cloud's operations are in `deploy/cloud`, so the root holds nothing only
  genhttp.dev needs. `.github/workflows/release.yml` publishes `genhttp/lambda`
  and `genhttp/lambda-agent` (amd64, arm64) on a `v*` tag or by hand only.
- Shipped as project references for now; meant to become NuGet packages, so a
  component takes nothing from a host.

## Domain model

### Keys and ownership

A lambda has a **public key** (names its address, may be changed) and a
**private key** (the editor link). Whoever holds the private key owns the
lambda; **editing is only possible with it**. It is shown only to the creator,
travels in a brief, and never appears in a log line about what was done - the
request line, which logs the path as requested, is the exception **(owner)**:
the log is behind the admin token. There are no accounts.

`/admin` authenticates with `X-Admin-Token` (`AdminAuthentication`, GenHTTP's
API key auth): 401 without, 403 wrong, 404 on an installation without a
token. The token is the platform's (`LambdaOptions.AdminToken`) **(owner)**;
the REST API's operator routes answer to it. **The editor has no admin section
(owner)**: what only the operator decides about a lambda (block, tier) is on
its page in the panel. Telemetry is seen only by owners (their lambda's) and
the operator.

### Where the lambdas answer

**(owner)** On genhttp.dev every lambda answers at a subdomain of the hosting
domain (`https://{key}.genhttp.run/`, `LAMBDA_HOSTING_URL`). The community
edition answers **by path** (`{public address}/lambda/{key}/`,
`LambdaAddressing.Paths`) and switches to subdomains when given a hosting URL;
`LAMBDA_ADDRESSING` decides over both.

- **A host of its own** keeps a lambda's scripts away from what the site keeps
  in the browser and from other lambdas. The hosting domain belongs on the
  Public Suffix List (the operator applies).
- **By path, the trade-off is said, not hidden**: a lambda shares the
  platform's origin. So the editor's pages send no referrer and are not
  indexed, the panel keeps its token in session storage while lambdas share
  its origin, and the docs (Security) say who should switch to subdomains.
- **The control plane stays on the site**: editor, API, MCP, git, panel - and
  the previews at `/features/{key}/`, the one place a lambda's code still runs
  on the site's host; moving them is a decision of its own.
- **A lambda on the site's host may not widen a service worker's scope**
  (`LambdaExecutionHandler`): a response with `Service-Worker-Allowed` is
  refused there with a 500 and a log line.
- **`/lambda/{key}/…` redirects permanently** where lambdas have hosts of their
  own (to the domain while it has one), with path and query, before anything
  else the server does (`LambdaRedirectConcern`). The subdomain redirects to
  the lambda's own domain while its tier serves one.
- **`www.` redirects to the domain (owner)** unless the owner entered it with
  `www.`; never served at both names, never where the `www.` name is another
  lambda's or the platform's.
- **A host nobody has is never the platform (owner).** With a public address
  (`LAMBDA_PUBLIC_URL`) the platform answers only at its own names
  (`localhost`, that host, the MCP origins); any other unknown host gets a 404
  page on every path (`UnknownHost`). ACME challenges are answered for every
  host.
- **Finding a lambda by address reads the key as spelled**
  (`LambdaKeys.TryRead`), never by the rules for claiming one
  (`TryNormalize`): a later rule must not take an existing lambda offline.
- The wildcard certificate is presented to every name with no certificate of
  its own (`CertificateLoader`).
- The hosting domain's own page and a subdomain with nothing online
  (`HostingPages`) are rendered by the server - no script of the site at the
  hosting domain - with their words in `pages.json`.
- **Addresses come from `ILambdaAddresses`**, never assembled from strings.

### Versions and data

- A **version** is the program - code, resources, and the documentation and
  tests in its code. It never changes once saved.
- **Data** - database, workspace, secrets - belongs to the lambda and is
  **shared by all versions**; deploys, rollbacks and merges never touch it.
- Records go in the database, files in the workspace, never in resources. The
  front end goes in the version. Keys and passwords go in secrets, nowhere
  else.
- Every kind of data is switched on before use. An agent may switch one on
  (`enable_data`); **switching off deletes, so only the owner does it**. Each
  kind is a section of its own in the editor, all drawn alike (`DataTab`); a
  new kind joins `DataKinds`, the editor's `DATA_KINDS` and that group.
- Data outlives versions, so **agents find a compromise between compatibility
  across versions and its cost**: additive, tolerant formats over migrations,
  no machinery nobody needs.

### Code and resources

**(owner)** A version is two parts, nothing else.

- Its **code** is every file that is not a resource: `.cs` files compiled in
  any folder, as a C# project compiles; every other file (docs, tests, front
  end sources, scripts) kept and never compiled or served. Its **resources**
  in `resources/` are what it reads and serves (`Resources`; `Assets` stays an
  alias).
- **No `.lambda/`, no special kinds of file.** `docs/` and `tests/` are
  folders by convention; how the rest is arranged is the agent's to decide and
  nothing detects it.
- **Two allowances in bytes (owner)**: a version (code and resources,
  `build-bytes`) and the data (database and workspace, `data-bytes`). The
  database gets what the workspace leaves (`DatabaseVault.RoomOf`).
- **Lambdas saved in older layouts keep working, untouched on disk**: read and
  translated as read (`LambdaSource.Parse`). Never rewrite stored versions; git
  gets a commit on top (see Git). A top-level `assets/` or `.lambda/` is
  refused with a hint.
- Names: C# files as C# names them; resources with an extension, no dot
  names, six folders deep; the rest as tools name files (no spaces, no `.git`,
  sixteen deep). What a project has at its top (`ProjectPaths`) is refused
  there.
- **The platform builds nothing (owner).** No build on push, no tool run. The
  agent builds where it works and saves the result with its sources. Do not
  add build machinery. The build agent has no shell and writes plain front
  ends; the demos teach with plain files.
- **`.gitignore` files of the code keep out what a build installs or caches
  (owner)**, for any technology - nothing is refused by name. A zip put back
  follows them (`IgnoredPaths`) but keeps what the lambda has; a save naming a
  file keeps it. The root `.gitignore` stays the platform's, taking `build/`
  back in (`!/build/`). A zip holds what a commit would and is counted as sent.
- **One layout everywhere**: version, zip, clone, export and published source
  lay files out the same; `?layout=project` reads a zip as a clone's commit.
- Nothing of the layout is in the simple view or on `/build`.

### How versions are kept

**(owner)** A version is a **manifest over a content store per lambda**:
`v{n}.manifest.json` names the files (size, text or binary, SHA-256), and the
content is kept once per lambda below `code/{lambda}/blobs/` (`ContentStore`)
however many versions, features and previews hold it. Features and previews
are manifests over the same store, so starting, deploying and merging copy no
content. The store per lambda and the manifests' shape are **(open)**.

- **Git's ids are git's concern, kept as data (owner)**: a `blobs` row per
  piece of content holds its size and its git ids (SHA-1 and SHA-256) so a
  push reads no file and a commit needs no hashing. The store names content by
  the SHA-256 of its bytes, none of git's.
- **Services hand on `VersionFile`s, never a string of the whole version.**
  Content is read only where needed (compiling, the facts, deploying, zips,
  exports, git); `ContentReads` counts reads and `StoreTests` holds every path
  that needs no resource to reading none - keep it so when adding one.
- **Kept before the turn, named in it**: a save stores new content before
  taking the lambda's or feature's turn and writes the manifest inside it. The
  store's own turn is taken last and takes no other.
- **What nothing names is collected**: maintenance removes content no manifest
  names, leaving what was written or named in the last hour
  (`ContentStore.Fresh`).
- Versions from before are moved into the store at start (`StoreMigration`),
  a manifest beside the untouched old file **(owner)**, before anything is
  served **(open)**.
- **The git store stays apart (owner)**: commits are served as long as they
  are reachable, longer than a version may be kept.
- The doors (API, MCP, git, zips, export) keep their shapes.

### Documentation and tests

Every version keeps `docs/product.md` (what the app is, for whom, why, in the
user's terms; first paragraph = the app in a sentence or two),
`docs/decisions.md` (technical decisions and why) and `tests/README.md` (how it
is tested), and beside them more pages, pictures, scripts and test data.

- **Files of the version**, saved, diffed, rolled back and merged like any
  file. Never in the database or workspace.
- **A convention, not a kind of file (owner)**: never compiled or served, left
  out of what identifies a build, counted towards the version. Only the editor
  reads them as markdown pages (one component for both).
- **Agents write them, humans read them.** Agents write all three with a new
  lambda, update what a change affects in the same save, read them before
  changing a lambda, and run the tests against a preview before merging.
- **In proportion to the lambda**: a small lambda gets a short paragraph a page
  and one quick check, no test suite; they grow with the app. Agents overdid
  it, so this is said wherever they are told to write them.
- Editor: **Documentation** second after the overview in both views (**About**
  in the simple view, the product page only, corrected by telling the agent);
  **Tests** in the full view only.

### Secrets

- **A value is written and never read back** - not by the editor, API, MCP or
  owner. Only the lambda reads it (`Secret.ReadAsync`, `Secret.ExistsAsync`,
  plain `Read`/`Exists`). No answer, log line or note contains a value.
- Off by default. Names are environment variable names, since an export reads
  `Secret.ReadAsync("X")` from `$X`. Values are not exported.
- The platform's own integration (`DatabaseSecrets`) seals them with AES-GCM
  under a key derived from the installation's key (`LAMBDA_SECRETS_KEY` or
  `secrets.key` in the data directory - **never in the database**) and a
  per-lambda salt, the name as associated data. Backup = database + key. Do not
  describe it as isolating lambdas from each other: they share a process.
- Read at runtime through a function handed to the lambda, never compiled in;
  a changed value applies without a deploy.
- A feature gets a sealed copy; merging throws it away.
- **The agent declares the need, the human supplies the value**: agents write
  `Secret.ReadAsync("NAME")` and switch secrets on; the platform lists names
  without a value (`missing`, `optional` for `Exists`-only), and the editor
  asks the owner. The build agent may switch secrets on and list them, never
  set or delete one.

### Databases

- **One per lambda** from the database integration (a SQLite file or a
  PostgreSQL schema), reached with `await Database.GetConnectionAsync()` (an
  open, pooled `DbConnection`, one per request). Off by default; switching on
  makes it empty, off deletes it and the features' copies; either restarts the
  lambda on its next request.
- **Schema by Evolve migrations** in `resources/migrations/`, applied at start -
  never EF migrations, `EnsureCreated` or `Migrate` (the code guard refuses
  them). An applied migration is never edited.
- **Records through EF Core, awaited (owner)**: a `DbContext` of the lambda's
  own on the handed connection (`options.UseDatabase(connection)`), one per
  request, `ToListAsync`/`SaveChangesAsync` in async routes. A hint, not
  enforced.
- **A lambda names no database (owner)**: lambdas, demos and every text that
  teaches agents use a `DbConnection`, `UseDatabase`, migrations in plain SQL
  every database accepts (`TEXT`, `INTEGER`, `BIGINT`, `BOOLEAN`, `DOUBLE
  PRECISION`; keys the code makes; unquoted names), ISO 8601 text for times,
  and look for an existing row instead of catching a unique violation.
  PostgreSQL folds names to lower case (`LowerCaseNames`). Code written against
  SQLite (`SqliteConnection`, `UseSqlite`) keeps compiling where SQLite keeps
  the database (`DatabaseBinding`), nowhere else.
- **No connection of the lambda's own.** The code guard refuses making any
  `DbConnection` (checked on the compilation) and `UseSqlite` without a
  connection; a database's client library is handed to the compiler only
  where its integration names it (`ReferenceProvider`). On SQLite the handed
  connection carries an authorizer (`ConnectionGuard`) refusing `ATTACH`,
  `VACUUM INTO`, directory pragmas and lifting `max_page_count` (the quota); on
  PostgreSQL the lambda's role reaches its own schemas only. Of EF only the
  root namespace, `Metadata.Builders`, `ChangeTracking` and
  `Storage.ValueConversion` are reachable; `dynamic` is refused. Keep all of
  this when touching any of it.
- **Not encrypted - decided.** Do not bring encryption back without the owner
  asking (it means replacing the process's SQLite, and the key would sit on the
  same volume).
- **The export carries it** as `database/database.db` (SQLite); from
  PostgreSQL the export carries none and `pg_dump` of the schema takes it.
- A feature gets a copy consistent at one moment; merging throws it away.
- The editor **reads** it and never edits it: a change to records is a change
  to the app.

### Features ("Drafts")

New functionality is developed in a **feature** (a *draft* in the editor):
based on a version, edited in place until done, with a **copy of the data**
and its own preview (`/features/{key}/`); merging makes it a new version.

- **Only a feature based on the newest version can be merged.** Otherwise the
  agent brings the newer changes in and moves its `base` (`update_feature`).
- **GenHTTP Lambda provides no merging or rebasing and must not grow any.** The
  agent does that work; the platform checks the base under the lambda's lock.
- A lambda that exists is changed in a feature; a new lambda is written as
  versions.

### Analytics

Opt-in visit counts per lambda (visitors, page views, requests per day, and
pages, referrers, countries, devices, browsers, systems, crawlers), in the
editor's **Analytics** section.

- **Opt-in (owner).** Switching off deletes what was counted, so an agent may
  switch it on when asked (`analytics` with `enable`), never off. Demos and
  previews are not counted.
- **By day, kept a year, shown per tier (owner)**: the window is a tier limit
  (`analytics-days`); what is kept beyond it shows once the lambda moves up.
- **One visit per request (owner).** `LambdaVisitConcern` makes the
  `LambdaVisit` once; the overview figures, request log and analytics read it.
  Anything new that wants to know about requests to lambdas reads the visit,
  adding what is missing - nothing measures a request again. What only
  analytics need is read only for lambdas that count; derived values are
  worked out by the job, never on the request.
- **Collected on the request, written by a job (owner)**: a bounded lock-free
  queue (`AnalyticsQueue`), a job every five minutes, one transaction. Losing
  the queue on restart is accepted. Never write a database while serving.
- **A database of its own, a star (owner)**: `lambdas` and a fact table per
  statistic keyed by lambda and day, migrated from
  `Services/Analytics/Migrations` (numbered on their own). Only the switch is
  in the platform's database.
- **Crawlers by a maintained list and their addresses (owner)**:
  crawler-user-agents (refreshed daily); Google, Bing, OpenAI, Apple and
  Perplexity named only when the connection came from their published
  addresses, never a forwarded header's. A value the sender wrote is taken only
  if it is one of a known few. Matching stays linear: no backtracking
  expression reads a user agent.
- **A visitor is a person who opened a page (owner)**, counted once a day, so
  never more visitors than page views. **Bots' views are counted apart
  (owner)** (`bot_views`) and shown only with **Include bots**; referrers are
  people's either way.
- **Nothing that names a visitor is kept**: counts, the referrer's host, a
  daily hash under a random in-memory key. No cookie, no script in the lambda.
  The privacy page says so.
- In both views, in the simple view's words; charts, every one with its values
  as a table one click away; page-view and request breakdowns under separate
  headings **(owner)**. It does not poll.

### Quiet hours and optimizing databases

**(owner)** The platform knows when the installation and each lambda are
usually quiet (`IQuietHoursService`), independent of what uses it - it decides
nothing.

- **Light, an indication not a measurement (owner)**: two lock-free increments
  per request (`QuietHoursConcern`, `HourlyCounts`), a typical day of 24
  numbers per lambda (`QuietProfile`), written hourly (`QuietHoursJob`).
  UTC hours, quietest three in a row **(open)**.
- The panel shows the installation's quiet hours, and on a lambda's page **when
  it is used**, not when it is quiet **(owner)**. Nothing in the editor.

The platform optimizes its own records, analytics and every lambda's database
in the background - **not drafts' copies (owner)**.

- **The platform decides, the integration does (owner)**: whether, which and
  when are `OptimizationService`/`OptimizationSchedule`'s; how is
  `IDatabaseProvider.OptimizeAsync`, which judges what its database can bear.
- In the quiet hours (03:00-06:00 server time while unknown), never right after
  start; at most daily, only lambdas visited since, one at a time **(open)**.
- **Switched off under Settings (owner)** (`optimizeDatabases`, on by default),
  for DB engineers who do it themselves.
- **Never anything that changes what a database holds**: no SQLite `VACUUM` of
  a file with a table lacking an `INTEGER PRIMARY KEY` and an index (rowids
  renumbered), no `VACUUM FULL`. Optimizing never creates a database.
- Results are kept per database (`database_optimizations`), shown in the panel
  only, not on a lambda's page **(owner)**.

### Showcase, export, ownership of the code

- **Showcasing is the owner's choice**: opt-in, never automatic.
- **No MCP tool puts a lambda there (owner)**: a picture in base64 is too large
  for a tool call. Agents use `PUT /lambdas/{privateKey}/showcase` with the
  body written to a file (`platform_guide`, `showcase`); one that cannot make
  requests sends the user to the editor. Do not bring the tool back.
- **The code belongs to whoever created the lambda.** No lock-in; keep the
  export working.
- The export (`GET …/export`) is **as small as a GenHTTP project can be**: .NET
  10, `GenHTTP.Full` (internal engine) at the server's version, the default
  hosting snippet, the snippet in a static `Project` class, platform stand-ins
  in `Platform/`, a `Dockerfile`; SQLite, EF Core and Evolve only where used,
  the database in `database/`. A test builds the export of every demo.

### Open source

An owner may publish a lambda's code at `/source/{publicKey}` to read, star and
download.

- **Opt-in**, only with the private key (editor, `PUT …/source`, or
  `open_source`, which agents use only when asked). License from
  `SourceLicenses`, MIT by default.
- **Published: every version as the export packs it, with a `LICENSE`. Never
  the data** (`ProjectPacker.Publish` takes no database) **and never what only
  the owner may know**: specification, editor key, traffic, logs. A version is
  described by its change line.
- **Just the source**: no issues, PRs or wikis; nothing on `/source` acts on
  the lambda.
- `/source` is its own component (`SourcePages`), not prerendered: the server
  names each page, marks it up as `SoftwareSourceCode` and lists it in the
  sitemap. An unpublished source is not found, like an unknown key.
- Versions are the single source of truth; packs are a cache
  (`/data/sources`, `LAMBDA_SOURCE_CACHE_BYTES`).
- **Stars are a `POST` with a signed ticket** handed out at least a second
  before; one per address, remembered in memory as a keyed hash.
- Demos are published under MIT by the installation.

### Git

Every lambda is a repository: `/editor/{privateKey}/{name}.git` for its owner,
`/source/{publicKey}.git` for anybody while published. Served by
GenHTTP.Modules.Git from what the platform keeps (`Services/Git`); no
repository on disk.

- **One history per lambda.** `main` is the newest version, each version a
  commit tagged `v{n}`, each feature a branch. The public source has no
  feature branches. Commit messages are change lines - no specification or
  editor key.
- **A commit is made once and kept** (below `/data/git/{lambda}`), made lazily
  when the repository is read. Never make one again from the version: every
  clone would see its history rewritten. Pushed commits are kept as they came.
- **The tree is the export's shape.** What the platform puts around the lambda
  (`Program.cs`, project file, `Platform/`, `Dockerfile`, ignore files,
  `AGENTS.md`, `CLAUDE.md`, `LICENSE`) depends on nothing a version decides,
  so a push that leaves them alone is right. Keep it that way.
- **A changed layout is a commit on top, never rewritten history**
  (`GitHistory.MoveAsync`); a push built on the old layout is refused, saying
  to rebase onto `origin/main`. Do the same for any later layout change.
- **The platform's files are refused, not ignored**, with how to undo it.
- **Each commit pushed to main is a version**: linear, forward only; the newest
  must compile. Pushes go through the same services as every door.
- **A branch is a feature**: pushing starts or replaces it and deploys its
  preview; `-o merge` merges, `-o deploy` puts a version online. A feature's
  branch name is kept when it is renamed.
- **Answers never hold the editor key** (build logs may be read by others).
- Git appears in the full view and on `/source` only, never in the simple view
  or on `/build`.
- A test clones every demo and builds it with the .NET SDK.

### Demos

The demos (`DemoCatalog`, `Resources/Templates`) teach agents how to write apps
and give them a starting point.

- **Read-only** (tier `Demo`, `LambdaGuard.EnsureEditable`, every door); their
  editor key is public; `create_lambda` with a demo as template makes a copy.
- They follow **GenHTTP's best practices**
  (<https://genhttp.org/documentation/content/>) and its high-level APIs.
- **When you add functionality to the platform, add it to the demos where it
  makes sense.** `demo-crud` shows tools for AI agents (`Tools.cs`) and API
  documentation attributes. Every demo has its docs, tests and a test script
  (`EveryDemoSaysWhatItIsWhyAndHowItIsTested`). Records via Evolve + EF Core,
  files in the workspace, all awaited. A demo reading a secret gets a random
  value from the seeder (`LambdaDemo.Secrets`) and works without one.

### Pages meant to be found

Agents make a page meant to be found or shared findable, in proportion: title,
meta description, language, icon, Open Graph preview, content in the served
HTML, the domain as canonical. A tool for a few people gets a title only. The
platform writes none of it; the agent does.

- `og:image`, `og:url` and the canonical link take full addresses - the
  exception to relative links; the deploy warning for self-links
  (`McpTools.Leaks`) leaves them out.
- **No renderer - decided for now**: no headless browser on the server (it runs
  strangers' JavaScript beside the platform), and no tools or shell in the
  build agent's image (its token is in its environment). Not without the owner
  asking.
- **The sitemap names no lambda (owner)**: a sitemap names pages of its own
  host.

### The link back

**(owner)** Agents are asked - not required - to put a small "Made with
{installation name}" line at the foot of the pages they build.

- A **nofollow** link to the installation's public address at a subdomain; a
  plain link by path (same site); **plain text on a lambda with its own
  domain**. A line from before is corrected with the next change to the page.
- **The user may refuse**: the agent says it added the line, removes it when
  asked and notes in `decisions.md` that it is not wanted. A change does not
  add one to a lambda that has none.
- **None on an enterprise installation (owner)**: the agent adds none and
  removes it from pages it changes.
- The name only - no keywords, badge, script or anything hidden. The platform
  never writes it, nothing checks for it. The MCP instructions, fixed at
  start, leave the words and whether to add it to `platform_guide`
  (`backlink`).

### Pushing, not polling

Agents are told never to poll: what changes while a page is open is pushed -
server-sent events where the page listens (`demo-live`), a websocket where it
talks back (`demo-game`).

### Connectivity

Whether a lambda's code may connect to other servers (`HttpClient`, sockets,
WebSockets, mail, DNS). **Called connectivity everywhere (owner)**, not egress.
Lambdas were used as proxies, so **it is a grant per lambda, not a property of
its tier (owner)**: the operator allows or declines it; the tier only says what
holds while nothing was decided (`ConnectivityRule`,
`TierPolicy.Connectivity`).

- **Production connects by default (owner)**; the sandbox asks. A decision
  about the lambda comes first either way. Demos never connect; a blocked
  lambda cannot ask or be decided about.
- **The agent asks, the operator decides**: `request_connectivity` with a
  required purpose (servers and why); the owner can read, change or withdraw
  it in the editor; the operator allows or declines in the panel with a note.
- **Refused as it compiles, not filtered as it runs.** Without it, the
  connecting assemblies are not handed to the compiler (`ReferenceProvider`,
  `Network`); the code guard refuses their names with `LAMBDA0003` and binds
  what other assemblies fetch on the code's behalf (XML from a URL, a bearer
  issuer, `Resource.FromWeb`, mail clients; `CodeGuard.InspectConnectivity`).
  Governance, not a sandbox. Keep the three together: a new way to connect is
  excluded or refused, and named in the message.
- **Part of the build** (`BuildSettings`): taking it away rebuilds the lambda
  and previews on their next request; a build without it keeps serving once
  it may connect.
- **The browser first**: agents - the build agent above all - call public APIs
  from the page where the API allows it, and ask only where the server must
  make the call.
- **The words**: the API, MCP, the full view and the panel say connectivity;
  the simple view says the app wants to **use other services** and the owner
  asks for **permission**, in a tile shown only while there is something to
  ask, wait for or read.

### Tiers

**Named by purpose, not by price (owner)**, assigned by the operator, never
chosen by the owner (`LambdaTier`):

- **Sandbox** - trying something: offline when unused, then removed; no domain
  of its own; asks for connectivity; compiled on its first request.
- **Production** - an app people use: kept; a domain; connects by default;
  compiled as the server starts.
- **System** - what the installation depends on (the usage report collector):
  as production, never blocked, never in the showcase, deleted only by the
  operator. The intended hook for running a lambda apart from or ahead of the
  others, which nothing does yet.
- **Demo (owner)** - the installation's own demos, assigned by the seeder,
  read only.

- **One policy, resolved in one place.** `TierPolicies.Of` maps a tier and the
  installation's settings to a `TierPolicy` that every check reads
  (`ILimitsService.PolicyOf`, `TiersWhere`, `Above`). Nothing else compares a
  tier with a value. What a tier is *for* is fixed in code; its sizes,
  analytics window, connectivity default and **label** are the installation's
  (`TierSettings`, the panel's **Limits**). genhttp.dev labels them "Free" and
  "Premium" **(owner)**. The API, MCP and code use identifiers only; pages and
  refusals use labels.
- The default tier for new lambdas is the sandbox **(owner)**.
- **A move changes the policy and nothing else (owner)**: no restart, no data
  moved; what is beyond the new limits stays readable and does not grow. Moved
  into the sandbox, a lambda is measured from the move (`tier_changed`).
- **Production and system are compiled as the server starts (owner)**
  (`TierPolicy.WarmedUp`, `LambdaWarmup`), in the background after the server
  answers, one at a time (compiling is one snippet at a time for memory)
  **(open)**, each built as a request would (`ResolvedLambda.Settings`).
- **The simple view offers the tier above** in a tile whose only action is a
  mail to the installation's contact naming the public address, never the
  editor key; no contact, no tile.
- **Tier limits are product settings, not server configuration**: set in the
  panel's **Limits** (`LimitsService`, rows in `settings`); a change applies to
  what is checked next. Ports, directories, keys, buffers,
  `LAMBDA_MAX_CONCURRENCY` and the execution timeout stay environment
  variables: they protect the process. Callers ask `ILimitsService`, never
  `LambdaOptions`, for a limit.
- **Enterprise** customers running their own instance are planned, but **do not
  anticipate enterprise features unless asked**, and do not shape the cloud
  code for them.

### Blocking

What the operator does about abuse (phishing, malware, attacks), in the panel
only.

- **A blocked lambda is offline and frozen**: with its previews, and every
  change refused through every door in `LambdaGuard.EnsureUnblocked` (a new
  door calls it too; `LambdaError.Blocked`, 403). The operator may only lift
  the block or delete it.
- **Reading and the export stay.** The owner sees a banner **with the
  operator's reason (owner)** (`blockReason`); refusals leave the reason out,
  since build logs may be read by others.
- Offline, out of the showcase, source not found - all back once lifted.
- **Kept until the operator decides**, never removed for going unused (its key
  would be free for whoever it was blocked for). Demos and **system lambdas
  (owner)** cannot be blocked (`TierPolicy.Blockable`).
- **The addresses a lambda was changed from are recorded**
  (`EditorAddresses`), at most hourly, for 30 days, only where the server
  keeps addresses (`LAMBDA_LOG_CLIENT_ADDRESS`), never the operator's own. The
  privacy page says so.
- **Address blocks**: never the installation's own networks
  (`LAMBDA_UNBLOCKABLE_NETWORKS` - the build agent's container gets a fresh
  address each build); the connection's address, never a forwarded one; IPv6
  by its /64. Refused first of all (`AddressBlockConcern`, outermost) with a
  plain 403 counted in memory, on every host. **The panel stays reachable
  from a blocked address (owner)**, so an operator can unblock themselves.
  **Blocks lapse after two weeks (owner)** (`AddressBlockJob`): addresses
  change hands.

### The operator's panel

**(owner)** Sections in groups like the editor's (the kit's `Navigation`):
Dashboard; Lambdas; Security; Performance; Platform. A new section joins the
group it belongs to; an empty group is hidden. A mark beside a section says what
waits there (`GET /admin/overview`, read as the operator moves, never on a
timer).

- **What is shown and what is done are kept apart (owner).** Information in
  cards; actions only in a section's header, a card titled as such (**Manage**)
  or a row's last cell. Each action that changes something confirms in a
  dialog first; a setting's switch or field is the exception. Notices lead to
  where something is decided. Built from `console/parts.tsx`.
- **The operator never opens the editor to see how a lambda is set up
  (owner)**: its page sums up every editor section, read only. A feature the
  editor gains gets its line there (`console/uses.tsx`) and a census count.
- **The operator's actions**: online, tier, domain, connectivity, block,
  remove. Nothing that is the owner's (data, analytics, showcase, source).
- **Dashboards are drawn from counts**, never a walk while a request waits
  (`ICensusService`; storage walked hourly by `StorageCensusJob`). Demos are
  counted and listed apart. Free space is read every time and warned about
  (`diskState`).
- English only.

## Architecture

Read the reference's "How it is put together" before touching services.

- The services are one project, `GenHTTP.Lambda.Core`, behind interfaces.
  `Meta` alone speaks to the database, with hand-mapped DTOs. Code lives on the
  file system (`Storage`), never in the database.
- **A service orchestrates; the concerns it would mix are units of their own.**
  A protocol, a count, a check goes into a class beside the service in its
  folder - not a class per method. `Services/Building` is the template. A rule
  another service needs too goes into a unit both call, not a static of one
  service.
- **Outside its folder a service is taken only by its interfaces (owner)**,
  enforced by `ServiceBoundaryTests`, for the API, MCP and every other service
  and component. Units have no interface and stay in `Services/<Name>`. What
  another folder needs of a unit is added to the service's interface. An
  exception is an entry in the test with its reason.
- Compiled lambdas run in the server process. The `CodeGuard` raises the cost
  of misbehaving; **it is not a sandbox.** Do not describe it as one.
- **Platform schema**: EF Core migrated by Evolve from
  `Data/Migrations/V<n>__*.sql` (SQLite) and `Data/PostgreSql/V<n>__*.sql`;
  **a schema change is a migration in every set**, each numbered on its own.
  Existing migrations are never edited. **Before pushing a migration check the
  highest `V` on `origin/main`** - a duplicate fails every test. New files under
  `src/GenHTTP.Lambda.Core/Data/` are hidden by `.gitignore`'s `data/` on
  Windows: `git add -f`.
- Frontends: React in an npm workspace at the root - the kit and a bundle per
  component; public pages prerendered per language.
- **Links in a lambda's front end are relative**: a lambda answers at the root
  of its address or below `/lambda/{key}/`, a preview below `/features/{key}/`.
- The full view's sidebar is **grouped** (overview and docs; sharing; develop;
  program and data; run). A new section joins its group, not the end. On a
  phone the sections are a bar that opens all of them, never a sideways
  scrolling row **(owner)** (`components/Navigation.tsx`, shared with the
  panel).
- **Wired through the container**, not by hand or statics: a concern needing
  only services is an `IDependentConcern` added with `Dependent.Concern<T>()`.
  The scope is opened by the host's outermost concern. A component's
  singletons are made once by its builder (`ActivatorUtilities`); the
  container is Core's and registers nothing of the components. What only one
  door needs stays at the door (`WorkspaceFiles`). What is one per process
  (the console) is installed by a service the platform owns (`ConsoleCapture`).

### Integrations

**(owner)** The database, the files and the secrets are integrations that can
be exchanged (PostgreSQL, S3, Vault…) without the rules changing. Observability
and logging are not integrations, for now.

- One of each kind per installation, behind the contracts in
  `GenHTTP.Lambda.Integrations` (`IDatabaseProvider`, `IFileProvider`,
  `ISecretProvider`), which takes nothing of the platform's. A host chooses
  them on the builder (`.Database(...)`, `.Files(...)`, `.Secrets(...)`);
  defaults are SQLite, `LocalFiles` and `DatabaseSecrets` (in Core, since it
  needs the platform's database). **Data is never moved between integrations
  (owner)**.
- **An integration keeps; the platform decides (owner).** Whether a lambda has
  a kind of data, its room, names, values, counts, who reads what - all in the
  services (`DatabaseVault`, `SecretVault`, `WorkspaceService`,
  `LambdaWorkspace`). Addressed by ids (`DataScope`), never keys.
- **The contracts are asynchronous (owner)**: every member that reaches
  infrastructure returns a `ValueTask` and takes a `CancellationToken`; what
  describes (`Describe`, `Guide`) or configures stays synchronous. Whole
  copies and exports go through `Offload` in the platform.
- **An integration lives as a GenHTTP handler does**: its builder only
  configures (`Build`), `PrepareAsync` reaches the infrastructure once as the
  platform starts, and it is `IAsyncDisposable`, disposed of with the
  platform - also where it was never prepared.
- **Kinds, dialects and stores are enums, never strings (owner)**; where one
  becomes a name (file, schema, directory, JSON, env var) a mapper spells it
  out, never `ToString()`: stored names must not follow a code rename.
- **The platform uses them too (owner)**: its records and analytics are kept by
  the database integration in its dialect. **No picture is kept in a database
  (owner)**: the logo and showcase pictures are files of the files integration.
- **Towards a lambda nothing changes when an integration does (owner)**:
  `Database.GetConnectionAsync()`/`UseDatabase`, `Secret.ReadAsync`, and the
  generated `Workspace` class, which calls functions the platform hands it and
  touches no directory - so there is no `Workspace.Root` **(owner)**.
  `Resources.Root` stays: resources are the version's files on disk.
- An integration may add lines to the guide (`IIntegration.Guide`), counted
  in its budget.
- **The panel's Integrations section (owner)**: terse cards (kind, name,
  assembly, facts) from `Describe` - never a password, key or connection
  string. The usage report names them, never where they are.
- **PostgreSQL is the community edition's (owner)**; genhttp.dev keeps SQLite.
  Its shape is **(open)**: one database (`LAMBDA_POSTGRES_CONNECTION`), schemas
  `platform`, `analytics`, one per lambda and per draft, a role per lambda
  reaching its own schemas only (password made anew each start, kept
  nowhere), a pool per lambda, room enforced by read-only connections once
  over it, drafts copied from the catalog in one repeatable-read transaction.
- A new integration keeps to this, and `docs/reference.md` (Integrations) says
  how to add one.

### Serving, and the threads it runs on

On Ioxide a request runs on a **reactor**: one thread per core that owns that
core's connections and resumes requests inline. Whatever a request does there,
every other connection of that core waits for. `async` works on it (awaits are
posted back to the reactor; a completed task never leaves the thread). Kestrel
and the internal engine serve on the thread pool, where a blocked thread is
taken from every other request. So:

- **Async is fine in integrations (owner).** One that waits on the network
  awaits it. An integration meant for Ioxide answers on the asking thread,
  every task completed (SQLite, local files, the platform's secrets), so
  nothing leaves the reactor.
- **Whatever reaches an integration, the records, compiling, packing, a body or
  the network is `async` and awaited**, up the chain: vaults, services and
  their interfaces (`…Async`), resources and MCP tools (names kept - operation
  ids and tool names come from them), jobs, the platform's start
  (`LambdaPlatformBuilder.BuildAsync`). Anything else stays plain. **The
  platform's records are awaited (owner)** - `CreateDbContextAsync`, `await
  using`, `ToListAsync`, `SaveChangesAsync`, `ExecuteUpdateAsync`. Evolve at
  start stays synchronous. Do not add a synchronous path beside an async one.
- **What every request reads is read from memory**, loaded awaited before
  anything is served (`LambdaPlatform.PrepareAsync`) and written through on
  change: limits, switches, brand, guide section, domains, address blocks.
  Reading one before it was loaded is an error, not a query.
- **Serving a lambda does not touch the database**: `ResolutionCache` answers
  keys, made stale by every write (`DatabaseChanges`, an EF interceptor) - so
  every write goes through EF.
- **Lambdas are taught the async form (owner)**: `await
  Database.GetConnectionAsync()`, `Workspace.ReadTextAsync`, `await
  Secret.ReadAsync`, EF's async methods in async routes. The plain forms stay
  for old code, through `Synchronous`, which takes the synchronization context
  off before waiting. Only functions handed to lambdas go through it.
- **Enforced when compiling, noticed when running**: `src/BannedSymbols.txt`
  (RS0030) bans EF's synchronous methods, waiting on tasks, `File.*Async`
  helpers and `Task.Run`, every overload by id; only `Synchronous` and
  `Offload` suppress it (and background starts, saying why). A query
  enumerated synchronously compiles, so `SynchronousRecords` logs and counts
  it, and every test fixture fails on the count.
- **Long work leaves the reactor in one hop through `Offload`**: Roslyn,
  packing, copying or exporting a workspace or database, hashing large content,
  reading a lambda's database for the editor. Not many small async steps
  (`File.ReadAllTextAsync`), not in place. Small things (a manifest, a
  version's C#) are done in place.
- **A lock held across an await is a `SemaphoreSlim` with `WaitAsync`**, never
  `Wait()` or a plain `Lock`. Lambda and feature turns are striped semaphores
  (`Infrastructure/Turns.cs`); not re-entrant, so what holds a turn never asks
  for another of its kind, and they nest one way only: a feature's before its
  lambda's. Compiling happens before taking the turn, which only records the
  result; serving rebuilds where the build differs from what is online.
  Integration copies and removals wait until the turn is over.
- **Never wait on a task** (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`):
  on a reactor that core deadlocks for good. The code guard holds lambdas to it
  (`CodeGuard.InspectWaiting`).
- **No lock every request takes is held for more than a moment** (the log
  ring's: append under it, readers walk back only to their cursor).
- **The console's lock comes first**: whatever writes to the real console takes
  `Console.Out` first (`LogBookProvider`); the other order deadlocked startup.
- What is read off a version is read once (`VersionFactsCache`).

### Logging what was done

Every API call that changes something and every MCP tool call logs at
information level **what it did**, once done - the operation and arguments,
not the request. A new endpoint or tool gets its line in the same change
(`OperationLog`).

- **Terse, backend style**: past-tense verb, the object and its key, arguments
  as pairs - `Deployed lambda {Lambda} version {Version}`, `Failed to merge
  feature '{Feature}' of lambda {Lambda}`. Names that may hold spaces quoted.
  Services log the same way.
- **The same properties everywhere**: `{Lambda}` a public key
  (`PublicKeyOfAsync`; `lambda #{LambdaId}` where only the id is known),
  `{Feature}` a feature's name, `{Version}`, `{Path}`, `{Name}`. One template
  per case, never a prebuilt fragment. Never the editor key or a feature's key.
- **Never a model as a whole** (`ToString()` of a record prints the editor
  key); name the properties.
- No secret values, code, file contents, pictures, specifications or
  descriptions. **The exception: the build agent's prompts are logged in full
  (owner)**; the privacy page says so.
- API reads are not logged (but exports and downloads); MCP reads are. Refusals
  are not logged here (the request line has the status). The MCP module's own
  lines are filtered below warnings (`PlatformLogging`).

### Frameworks: GenHTTP and Ioxide

- **If you find a bug in GenHTTP or Ioxide, flag it clearly** - what, where, a
  reproduction - **instead of working around it**, in your answer to the owner,
  not a comment.
- **Use what the framework offers the way it is meant (owner)** ("embrace
  it"): where GenHTTP has a module for something done by hand, move onto it.
- **The MCP endpoint is GenHTTP's MCP module**: tools are `[Tool]` methods of
  `McpTools`, `[Summary]` their description; fixed argument limits are
  attributes (`[Range]`, `[MaxLength]`, `[AllowedValues]`), not checks of our
  own. A `LambdaException` is turned into an error that says why
  (`Refusing`), never a crash.
- **Do not write tests for the frameworks.**
- The framework source is beside this repository (`../GenHTTP`); read it rather
  than decompiling packages.
- GenHTTP releases request headers once a body is bound: check headers in a
  concern in front of the resource (`AdminAuthentication`). A new instance of
  this kind of problem is a bug to flag.

## Testing

- **Every feature has functional tests written as acceptance criteria**: start
  the real application (`LambdaFixture`), act as a user or agent through the
  API, MCP or a deployed lambda, assert what they would see. Name tests after
  the behaviour (`AFeatureStartsAsACopyOfTheNewestVersion`).
- **No mocks.** A fake only for what is outside our process: `FakeAgent` for
  the build agent's runner, `YieldingIntegrations` for infrastructure on
  another machine (yielding before every call, so awaiting and the plain
  calls' bridge are tested on incomplete tasks).
- A fixture fails where the platform asked for its records synchronously
  (`SynchronousRecords`); tests that touch the records await them too.
- Each test has its own data directory and runs in parallel. MSTest,
  `TreatWarningsAsErrors`.
- The fixture composes the components like the cloud host, with stand-in
  bundles (`LambdaFixture.BundleOf`); path addressing via `Addressing =
  LambdaAddressing.Paths`. PostgreSQL is tested against a real server where
  `LAMBDA_TEST_POSTGRES` names one (CI starts one; inconclusive without).

```bash
dotnet build GenHTTP.Lambda.slnx -c Release
dotnet test GenHTTP.Lambda.slnx -c Release
npm run build      # at the root: type checks and builds every bundle; CI runs it too
```

On Windows Ioxide does not run: start the server with `LAMBDA_ENGINE=Kestrel`.
Build the frontend before the server when checking a page by hand.

## Translations

- **Everything a visitor or owner sees is translated into all supported
  languages, except the admin panel** (English only). That includes both
  sites, the workspace, showcase, sources, editor, the runner's hosting pages,
  page metadata in every table of pages, and link previews (`cards.text.js`).
- **Every bundle has its own catalog** (`<component>/Frontend/src/locales/`);
  the kit's (`src/Kit/src/locales`) holds what several share (the workspace of
  `/build` among it, so formal in Italian and Spanish). Site catalogs combine
  both (`defineSite`).
- Languages: `id de en es fr it nl pl pt pt-pt tr ja ko`
  (`src/Kit/src/i18n/languages.ts`, mirrored in
  `GenHTTP.Lambda.Web/SiteLanguages.cs`). English is the source; every other
  catalog is typed as it, so a missing string fails the build. A new language
  goes into both lists, every catalog, every table of pages and the social
  images.
- A profile's own wording is a section's `profiles`, only what differs, with a
  doc comment saying for whom.
- The editor's words are a file per section and language plus an `index.ts`; a
  new section gets its English file, its line in every `index.ts`, and the
  rest from `apply`.
- **Use the register normal for a product in that country**, professional,
  never chummy; do not carry English idioms over. The form each language uses
  today:

| Language | Form | Notes |
|---|---|---|
| de | formal - *Sie* | factual and precise, no marketing jokes |
| fr | formal - *vous* | |
| it | informal - *tu* everywhere except `/build` and an enterprise installation's front page, which are formal (*Lei*) | the pages that address non-developers |
| es | informal - *tú* everywhere except `/build` and an enterprise installation's front page, which are formal (*usted*) | as Italian |
| nl | informal - *je* | |
| pl | informal - *ty* | |
| pt | *você* (Brazil) | no colloquialisms |
| pt-pt | informal - *tu* (Portugal) | |
| tr | formal - *siz* | |
| ja | polite (です・ます) | |
| ko | polite (해요체 / 하세요) | |
| id | formal - *Anda* | |

  The informal choices are deliberate. Changing a form or adding a language
  changes this table in the same commit. **The translation script parses this
  table**: keep one row per language, `code | form | notes`, and no other
  three-column table in this file whose first cell is a language code.

### How a change is translated

A script hands each translator only what changed and puts the answers back:

1. Write the English (the `en` catalogs and the English titles and
   descriptions of the tables of pages; the script lists them in `CATALOGS` and
   `TABLES` of `src/Kit/scripts/translations.mjs`). A sentence whose place is
   not plain from its key gets a doc comment saying where it appears.
2. `npm run translations -- extract` (at the root) writes requests into
   `.translations/`: the register above, the glossary
   (`src/Kit/src/locales/glossary.json`), each changed sentence with the old
   English and the current translation.
3. One `translator` agent (`.claude/agents/translator.md`) per request, all at
   once, each told only its request's path.
4. `npm run translations -- apply` puts the answers in, refuses one that lost a
   parameter, interpolation, kit call or link, removes what English dropped,
   and gives French its non-breaking spaces. Then `npm run build`.

- **The translator runs on Sonnet (owner)**, pinned in its definition; do not
  override it with the Agent tool's `model`.
- Fixed words per language are in `glossary.json`; a term changes there first.
- What a branch translated counts as done; after changing English again,
  commit and extract with `--base HEAD`. `--stale`, `--redo <path>`, `check`
  (CI fails on lost parameters or links) and `show <file> <path>` exist.

## Documentation

**The documentation contains every feature and changes in the same change as
the feature.** The places, each touched where it applies:

1. `docs/reference.md` - routes, API, configuration, operations, how it fits
   together ("the reference").
2. The user documentation at <https://community.genhttp.run/docs/> (the lambda
   `community` on genhttp.dev): concepts and every door as contracts, with
   screenshots. A change people would notice is followed there.
3. For agents: `platform_guide`, the MCP instructions and tool descriptions
   (`GenHTTP.Lambda.Mcp`), `AGENTS.md` of every lambda's repository
   (`Resources/Export/Agents.md.txt`), and the build agent's briefs.
4. The API description (Scalar at `/api/v1/scalar/`).

`README.md` is **short and leads to the documentation (owner)**: what GenHTTP
Lambda is, the teaser picture, the entry points, a two-command quick start, why
it exists, what to know first (not a sandbox; the usage report), building from
source. Its tone is a professional project README, not this file's. There is
no guide on the sites **(owner)**: agents have `platform_guide`, owners the
editor.

### Rules said to agents in several places

These rules reach agents through more than one door. Change them in every
place at once, worded the same: the MCP instructions (a line, where the rule
would be broken before the guide is read), the named `platform_guide` section,
the tool descriptions where they apply, the build agent's brief, a
repository's `AGENTS.md`, and the reference.

| Rule | `platform_guide` section |
|---|---|
| Code and resources, the platform builds nothing | `code`, `resources` |
| A lambda that exists is changed in a feature | - |
| Documentation and tests, in proportion | `documentationAndTests` |
| Pages meant to be found | `beingFound` |
| Pushing, not polling | `liveUpdates` |
| The "Made with" line | `backlink` |
| Connectivity: the browser first, ask with a purpose | `connectivity` |
| Asynchronous database, workspace and secrets | - |
| The showcase through the REST API | `showcase` |
| Analytics (also the privacy page and the cloud's front page) | `analytics` |
| Blocking (also the API's introduction, the terms, the privacy page) | `lifecycle` |

### MCP instructions and `platform_guide`

- **The MCP instructions (`McpEndpoint`) are short**: from nothing to something
  online, how an existing lambda is changed, the rules an agent would break
  before reading the guide, and what only the REST API does (the showcase).
  The rest is in `platform_guide` and the tool descriptions - not repeated.
- **`platform_guide` is written for agents, not people (owner)**: one Markdown
  document read from start to end (`GuideDocument` in `Services/Guide`), terse,
  each rule said once where the agent decides it, no internal detail without
  consequence for the agent, nothing a tool description or demo already says.
  Its headings are the names the pointers use ("platform_guide,
  connectivity"); renaming one renames every pointer. **A test holds it under
  28 KB (owner)** (`TheGuideFitsIntoOneToolResult`): a new rule makes room for
  itself.
- **The operator adds rules for agents (owner)** in the panel's **Guide**
  section (`/admin/guide`): one section, a title and Markdown, **last, after
  the platform's own (owner)**. It applies where the guide leaves the agent the
  choice and changes nothing the platform requires. Its title may not be one of
  the platform's sections; no `#`/`##` headings, no open code block, at most 6
  KB. It reaches agents through `platform_guide` only.
- Limits the operator sets are never written as numbers in the guide, tool
  descriptions or API description: say where to read them (`GET /system`,
  `limits`). Fixed ones may be.

### The API description

The OpenAPI document (`/api/v1/openapi.json`) is written for whoever calls the
API, with GenHTTP's documentation attributes beside the code.

- **Behaviour, not just shape (owner)**: what an operation changes and leaves
  alone, every refusal and why (`[Response]` with `ErrorResponse`), what it
  answers (`[return: Summary]`, `[Response]` for 201, 202, 422), non-JSON
  bodies and downloads. Read the service before writing it.
- **Attributes for the caller, XML comments for the maintainer**; the same
  sentence is not in both. A summary is a short label; the rest goes into
  remarks.
- **Tags are concepts, not classes**, ordered and grouped in `ApiDocumentation`
  (`x-tagGroups`). The introduction is
  `src/GenHTTP.Lambda.Api/Resources/Introduction.md`; shared wording is a
  constant of `ApiDocumentation`.
- **Authentication describes itself**: the admin token's concern declares the
  `adminToken` scheme and its 401/403; elsewhere the editor key in the path is
  the credential.
- `ApiDocumentationTests` refuses anything undocumented; a new endpoint or field
  is documented in the same change.

## Working conventions

- Code is written to be read: XML documentation with `<remarks>` that say
  *why*, `#region` blocks per concern, braces on their own line. Match the file
  you are in.
- Comments say why, not what. Prose, not marketing.
- Tailwind, light and dark themes, phones included.
- Check a UI change by running the app and looking at it; type checks and tests
  are no proof that a UI works.
- PR screenshots go through the `pr-screenshots` skill **(owner)**.
