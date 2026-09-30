# GenHTTP Lambda - steering file for agents

This file holds the product and architectural decisions of this repository.
Follow it, and when a decision is made or changed - by the owner in a
conversation, in a review, anywhere - **put it here in the same change**. A rule
that only lives in a chat is lost. The [README](README.md) has the reference
(routes, API, configuration, operations); this file has the *why* and the rules.

`docker/agent/AGENTS.md` is something else: it is the brief of the build agent
inside its throwaway container, not a steering file for people working on this
repository. Do not merge the two.

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
  way to describe a change.
- The full view, `/ship`, the API, MCP tools, the guide and the README use the
  precise words (feature, version, merge, base).
- The view a lambda opens in is a default only (`view`, set at creation and via
  `PATCH`). The build agent creates `Simple` lambdas, everything else `Full`.
  Whoever switches views has chosen for themselves, in their browser.

## Domain model

### Keys and ownership

A lambda has a **public key** (part of its URL, may be changed) and a **private
key** (the editor link). Whoever holds the private key owns the lambda, and
**editing is only possible with it**. The private key is shown only to the
creator. It travels in a brief, never in a log line. There are no accounts.

The one place that authenticates differently is `/admin`: `X-Admin-Token`, see
the README.

### Versions, data, and how they differ

- A **version** is the program: code *and* assets (a SPA, for example). It never
  changes once saved. Different versions may have different code and different
  assets.
- **Data** is what the program keeps (today: the workspace; a database and
  secrets are meant to follow). It belongs to the lambda and is **shared by all
  versions**. Deploys, rollbacks and merges never touch it.
- User data goes in the workspace, never in assets. The front end goes in the
  version, never in the workspace.

Because data outlives versions, a rollback or a newer version reads data written
by a different one. **Agents find a good compromise between data compatibility
across versions and the effort it costs**: prefer additive, tolerant formats
(optional fields, defaults for what is missing) over migrations, and do not
build machinery for compatibility that nobody needs.

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
  stands in for the platform in `Platform/`, and a `Dockerfile`. `Program.cs`
  opens with a short note that it was a lambda on genhttp.dev, its metadata,
  and a link to the GenHTTP documentation. A test builds the export of every
  demo; keep it passing.

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
  it makes sense**, so agents learn it by reading them. They keep their data in
  JSON files in the workspace on purpose - do not show a database the platform
  does not offer.

### Tiers

`Free`, `Premium`, `Demo` (`LambdaTier`), assigned by an administrator and never
chosen by the owner.

- **Free** is for trying: taken offline when unused, removed when abandoned.
- **Premium** is for running something in production, for example a website on a
  custom domain: kept online, larger limits.
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
  body-taking method (see `AdminGateConcern`). A new instance of the same kind of
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
  add its row.
- The forms above describe the copy as written today; when translating new text,
  look at the neighbouring strings of the same file rather than at this table
  alone.

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
