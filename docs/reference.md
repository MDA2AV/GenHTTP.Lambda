# GenHTTP Lambda reference

The technical reference of GenHTTP Lambda: its routes, the API, the
configuration, how it is operated and how it is put together. It covers both
editions - the cloud edition that runs genhttp.dev (`src/GenHTTP.Lambda.Cloud`)
and the community edition anybody runs on a server of their own
(`src/GenHTTP.Lambda.Community`, the image `genhttp/lambda`). Both are the same
platform, put together by two hosts, and where they differ it says so. How to
run the community edition is in the [README](../README.md); the decisions
behind all of it are in [CLAUDE.md](../CLAUDE.md). Paths of the platform's
code - `Services/`, `Data/`, `Resources/`, `Infrastructure/` - are below
`src/GenHTTP.Lambda.Core/`.

Write a C# snippet that returns a GenHTTP `IHandler`, press deploy, and get a
public URL that serves it. The snippet is compiled with Roslyn at runtime and
the resulting handler is hosted by the same [GenHTTP][genhttp] server that
serves this application.

```csharp
return Inline.Create()
             .Get(() => "Hello from my lambda!");
```

## Quick start

The community edition, as the [README](../README.md) runs it:

```bash
docker run -d --name lambda -p 8080:8080 -v lambda-data:/data genhttp/lambda
```

The application is then available at <http://localhost:8080/>. Its data - the
SQLite database, the stored code, and the workspaces and databases of the
lambdas - lives in a named volume mounted at `/data` (or, the databases, in
[PostgreSQL](postgresql.md) where it is told so). The lambdas answer
below it, at `/lambda/{key}/` (see
[Where the lambdas answer](#where-the-lambdas-answer)). `samples/community`
does the same with Docker Compose, the build agent beside it as a profile.
The image is built from `src/GenHTTP.Lambda.Community/Dockerfile` and
published with the build agent's, `genhttp/lambda-agent` (`docker/agent`), by
`.github/workflows/release.yml`, for amd64 and arm64, on a tag `v*` or by
hand.

genhttp.dev is the cloud edition: `deploy/cloud/docker-compose.yml` builds it
from `src/GenHTTP.Lambda.Cloud/Dockerfile`, with the build agent beside it (see
[Redeploying](#redeploying)), and its lambdas answer at subdomains of
`LAMBDA_HOSTING_URL`, one each.

## Developing locally

The frontends are an npm workspace at the root of the repository, and every
component with pages builds its bundle into its project, which a host copies
into its output when it is built - so the frontends are built first:

```bash
npm install && npm run build                      # every bundle, into <project>/bundle/

# the server, on http://localhost:8080/ - the community edition or the cloud one
dotnet run --project src/GenHTTP.Lambda.Community
dotnet run --project src/GenHTTP.Lambda.Cloud
```

A host serves the bundles it was built with: build it again after building a
frontend anew, and a component whose bundle was never built answers with a
page that says how to build it. To work on one bundle with hot reload, run its
dev server beside the server - on <http://localhost:5173/>, below the path the
component is mounted at (`/editor/`, `/admin/`, `/source/`, or `/` for a site),
passing everything else to `LAMBDA_API` (`http://localhost:8080` unless set):

```bash
npm run dev --workspace @genhttp-lambda/editor    # or admin, sources, cloud, community
```

The community edition's lambdas answer below the server -
<http://localhost:8080/lambda/quiz/> - and the cloud's at subdomains of
localhost named after their keys - <http://quiz.localhost:8080/> - which
browsers resolve without a record.

English is the source of every page; the other languages are translated by
script and agent, so that only what changed is read and written. The commands
run at the root:

```bash
npm run translations -- extract   # a request per language in .translations/
# one `translator` agent per request (.claude/agents/translator.md, on Sonnet)
npm run translations -- apply     # the answers into the catalogs, checked
npm run translations -- check     # every language against English
```

CLAUDE.md ("How a change is translated") has the rules.

Build and test everything the way CI does:

```bash
npm run build
dotnet build GenHTTP.Lambda.slnx -c Release
dotnet test GenHTTP.Lambda.slnx -c Release
```

The tests compile and execute real lambdas against a real server on a random
port, each against its own temporary data directory.

## Routes

Each component is mounted at a path of its own, the same in both editions
(`PlatformRoutes`): the components link to one another, so a host mounts each
at its name and nowhere else. The runner around them takes the lambdas and
the previews, and the host's site the rest (see
[How it is put together](#how-it-is-put-together)).

| Route                | Serves                                                    |
|----------------------|-----------------------------------------------------------|
| `/`                  | the front page: genhttp.dev's (`Website`) shows what every app gets, each with a screenshot of one example app; a community installation's (`LandingPage`) is simpler |
| `/build`             | the workspace: the apps this browser knows and a conversation with the build agent about each, for somebody who does not write code (see [The workspace](#the-workspace)) |
| `/ship`              | a guide for developers: connect an agent of one's own over MCP and have it publish what it built |
| `/platform`          | genhttp.dev: leads to `/` for good - the features were a page of their own until they became the front page |
| `/self-hosting`      | genhttp.dev: genhttp.dev and the community edition side by side, neither preferred |
| `/showcase`          | the lambdas their owners chose to show                    |
| `/source`            | the lambdas whose owners published their code (`SourcePages`) |
| `/source/:publicKey` | the published code of one: its files, documentation, tests and changes, per version |
| `/source/:publicKey.git` | the published code as a git repository, read only     |
| `/terms`, `/privacy`, `/imprint` | genhttp.dev: the legal pages                  |
| `/robots.txt`, `/sitemap.xml`, `/llms.txt` | what the site says about its host (`SitePages`); genhttp.dev also describes its MCP server and API at `/.well-known/ai-catalog.json`, with the API's description at `/api/v1/scalar/` as its documentation |
| `/editor/create`     | the creation assistant (`Editor`)                         |
| `/editor/:privateKey`| the editor for one lambda                                 |
| `/editor/:privateKey/:name.git` | the lambda as a git repository, to clone and push to (any name; the editor offers the public key) |
| `/lambda/:publicKey/…` | where the lambdas answer by path, the deployed handler - or, while it answers at a domain of its own, a permanent redirect there; `/lambda/:publicKey` without the slash is sent on to the address with it. Where they answer at subdomains, the address a lambda answered at once: a permanent redirect to where it answers now, with the rest of the path and the query |
| `/features/:feature` | the preview of a feature, while it is online              |
| any path, at `{publicKey}.{hosting domain}` | where the lambdas answer at subdomains, the deployed handler - or, while the lambda answers at a domain of its own, a permanent redirect there |
| any path, at a lambda's own domain | the deployed handler of the lambda with that domain, in a tier that has domains of its own (production, system) |
| the hosting domain itself | where the lambdas answer at subdomains, a page saying what is hosted there, leading to the site |
| `/api/v1/`           | everything the editor and the panel call, see below (`RestApi`) |
| `/mcp`               | the same, for agents (`McpServer`)                        |
| `/admin`             | the operator's panel: every lambda on the server, for whoever runs it (`AdminPanel`) |

### API

Browsable at `/api/v1/scalar/`, described by `/api/v1/openapi.json` (or
`.yaml`). The description is the reference for whoever calls the API: it opens
with an introduction (getting started, keys, files, limits, errors), groups
the operations by what they are about, and says of each what it does, what it
leaves alone and every way it is refused. It is written with GenHTTP's
documentation attributes beside the code (see `ApiDocumentation`), and
`ApiDocumentationTests` keeps it complete.

A lambda is addressed by its editor key, which is also what authorizes the
call. Things that can be listed, read, created or changed are resources; what
is a process rather than a thing is a verb in the path. A refusal is answered
with a status and `{ status, error, message }`, where `error` is `Invalid`
(400), `Forbidden` (403), `NotFound` (404), `Conflict` (409) or `TooMany` (429).

| Endpoint                                              | Does                                      |
|-------------------------------------------------------|-------------------------------------------|
| `POST /lambdas`                                       | creates a lambda (`acceptedTerms` where the installation has terms; optionally with `view`: `Full` or `Simple`) |
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
| `GET / PUT / DELETE /lambdas/:privateKey/showcase`    | its entry on the showcase: read, list it (title, description, picture as base64), take it off - agents too, since no tool does it |
| `GET /lambdas/:privateKey/domain`, `PUT / DELETE`     | the domain of a lambda in production or the system tier, whether it is served, and what it resolves to |
| `GET / PUT / DELETE /lambdas/:privateKey/connectivity`      | whether its code may connect to other servers; asks the operator for it (`purpose`); takes the request back or gives it up, see [Connectivity](#connectivity) |
| `GET /showcases`, `GET /showcases/:publicKey/image`   | what `/showcase` lists, and a picture |
| `GET / PUT / DELETE /lambdas/:privateKey/analytics`   | who visits it, by day, over the last `days` (at most what its tier shows); switches counting on; switches it off and deletes what was counted, see [Analytics](#analytics) |
| `GET / PUT / DELETE /lambdas/:privateKey/source`      | whether its code is published and under which license, the licenses on offer; publishes it or changes the license (`license`, `author`); takes it down |
| `GET /sources`                                        | what `/source` lists (`search`, `order`: `stars`, `updated`, `published`, `skip`, `take`) |
| `GET /sources/:publicKey`                             | a published source: what it is, its license, where it runs, every version and what it changed, a ticket to star it with |
| `GET /sources/:publicKey/versions/:version`           | the files of a version as its project has them, packed on the first request |
| `GET /sources/:publicKey/versions/:version/files/:path`, `…/raw/:path` | one file, its path encoded: its text; as it is (`?download=1` to save it) |
| `GET /sources/:publicKey/versions/:version/zip`       | a version as a project to download        |
| `POST /sources/:publicKey/star`                       | stars it, or takes the star back (`ticket`, `starred`) |
| `POST /builds`, `GET /builds/:id`                     | a new app, built from the workspace on `/build` |
| `GET /system`                                         | terms (only where the installation has any), limits, starters, build agent (`build`: whether it is offered, and the models and efforts it offers), where a lambda answers (`lambdaUrl`), who runs it (`operator`: `contact`, `abuse`, `legal`), how it presents itself (`brand`: its name, the picture beside it, whether its operator may change them), and whom it is for (`profile`: `cloud`, `home` or `enterprise`, see [Who it is for](#who-it-is-for)) |
| `GET /system/logo`                                    | the picture beside the installation's name, where its operator branded it, see [Branding](#branding) |
| `GET /telemetry`, `/logs`, `/admin/...`               | for whoever runs the installation; `/admin/branding` only where the installation may be branded |

Creating a lambda asks what somebody would like to build and offers the demos
in those words - "Keep track of things", "Let people sign up" - next to an empty
lambda. Picking a demo starts the new lambda as a copy of it, which is theirs to
change. The files are in `Resources/Templates` and listed in `TemplateCatalog`.

A lambda has two keys. The public one names its address - `quiz` answers at
`https://quiz.genhttp.run/`, or at `/lambda/quiz/` where lambdas answer by
path - and may be changed; the private one is the
editor link and is shown only to whoever created the lambda - anyone holding
it can edit, deploy and delete. What the API answers about a lambda carries
both its address - at its subdomain of the hosting domain or below
`/lambda/` - `publicUrl`, and where to link to
it, `address` - its own domain while it answers at one, `publicUrl` otherwise
(see [Where the lambdas answer](#where-the-lambdas-answer)).

A lambda may also return a websocket rather than a document, in any of the
three flavours the module offers - `Websocket.Functional()`, `.Reactive()` and
`.Imperative()`. The handshake is an ordinary request and is bounded by the
execution timeout like any other; everything after it belongs to the
connection, which is why a socket may stay open far longer than a lambda is
given to answer. `FrameType`, which the imperative flavour reads off every
frame, is one of the names a lambda is given, so no import is needed.

A lambda may offer tools to AI agents itself, over the Model Context Protocol:
`Tools.Create()` with a class whose methods are marked `[Tool]`, added at a
path such as `mcp`, which an agent then connects to. And a webservice of its
own describes itself as OpenAPI, in the words of `[Summary]`, `[Remarks]`,
`[Tag]` and `[Response]` on its routes and records. The MCP module and those
attributes are imported for every lambda and in its export; `demo-crud` shows
both, and the guide says how (`toolsForAgents`, `describingAnApi`).

The editor says when both timers of a sandbox lambda run out: a hint under the public URL
for the deployment, and a chip beside the buttons for the lambda itself, which
opens an explanation of how each one is extended.

### Where the lambdas answer

A lambda answers at an address of its own, made in one of two ways
(`LambdaOptions.Addressing`): at a subdomain of the hosting domain, as on
genhttp.dev, or below `/lambda/` on the platform's own host, as in the
community edition unless it is given a hosting domain. `LAMBDA_ADDRESSING`
(`subdomains` or `paths`) decides over both. Either way a lambda answers at
the root of its address - below a path the runner steps past its key - so
the same code runs at both, and a domain of its own is served the same.

**At subdomains**, every lambda answers at a subdomain of one domain, the
hosting domain, named after its public key: `https://quiz.genhttp.run/`, the
whole of the host its own. `LAMBDA_HOSTING_URL` names that domain with its
scheme and port; left empty it is this server on localhost -
`http://quiz.localhost:8080/` - which browsers resolve without a record in
the DNS.

A host of its own rather than a path below the platform's, so that what
somebody built is a site of its own: a page of it cannot read what the
platform keeps in a browser - the admin token, for one - nor what another
lambda keeps there, and its paths start at the root of its host, so a
browser asking for `/favicon.ico` asks the lambda. The editor, the API, the
agents' endpoint and git stay where they are, on the platform.

**Below a path**, a lambda answers at `/lambda/{key}/` of `LAMBDA_PUBLIC_URL`,
or of this server on localhost without it:
`http://localhost:8080/lambda/quiz/`. That takes nothing but the one name - no
wildcard record, no wildcard certificate. `/lambda/{key}` without the slash is
sent on to the address with
it, since a page answered there would resolve its relative links against
`/lambda/` and load none of its scripts. No subdomain is a lambda's, and there
is no page of a hosting domain.

The price is the origin. Below a path every lambda shares the platform's: a
page of one can read what the editor and the panel keep in that browser - the
admin token, for one - and what every other lambda keeps there. The editor's
pages are sent with `Referrer-Policy: no-referrer`, so the editor key in their
address is never named to a lambda they link to or load, and with
`X-Robots-Tag: noindex, nofollow`. The panel keeps its token for the tab only
(session storage) where the lambdas share its origin. And a lambda answering on
the platform's host - by path, and every feature's preview - may not widen a
service worker's scope: an answer with `Service-Worker-Allowed` is refused with
a 500 that says why, since a worker for `/` would see every page of the
platform that browser opens afterwards. A lambda with a domain of its own is
sent there from its path however the path is spelled. That suits an
installation whose lambdas are trusted - one run for oneself or a team. One that lets strangers build
gives the lambdas hosts of their own with `LAMBDA_HOSTING_URL`, which switches
the community edition to subdomains.

- **Its old address.** Where the lambdas answer at subdomains,
  `/lambda/{key}/…` on the platform - where every lambda answered once - is
  answered with a permanent redirect to where the lambda answers now, with
  the rest of the path and the query as they were sent, byte for byte - a signature over
  them still matches - Moved Permanently for a read, Permanent Redirect for
  anything else, which a client sends again with the same method and body.
  It is answered before anything else the server does: no request line, no
  telemetry, no upgrade to HTTPS, since the address it sends to is secure
  already. A browser keeps it for a day rather than for good, because where
  a lambda answers changes when it gets a domain or loses one. A key nobody
  has is sent on as well - the address moved, whether or not something is
  there - while a path below `/lambda/` that could never have been a key is
  not found.
- **A domain of its own.** A lambda with a domain, in a tier that has domains
  of its own - production or the system tier - answers there and only there: its subdomain - or below a path, `/lambda/{key}/…` - sends
  visitors on to the domain the same way, so there is one address for search
  engines to keep and for visitors to pass on. Removing the domain, or the
  tier that serves it, has the subdomain or the path answer again. For the
  same reason `www.` in front of the domain sends its
  visitors on to it - `www.shop.example.com` to `shop.example.com` - unless
  the owner entered the domain with `www.`, which is then where it answers,
  and nothing is sent there from the name without; or unless that `www.` is
  the domain of another lambda, or one of the platform's own names.
- **Nothing there.** A subdomain nothing is online at - a key nobody has, a
  lambda that is offline, a name no key can be - or below a path,
  `/lambda/{key}/`, is told so in a plain page that leads to the site, or in
  a line of JSON for anything but a browser.
  The hosting domain itself, and `www.` in front of it, has a page of its own
  that says what is hosted there and leads to the site, in the language the
  browser prefers. Their words are the runner's own, in
  `src/GenHTTP.Lambda.Execution/Resources/hosting.json` (`hosting:/` and
  `hosting:/:key`), and translated with the rest. Both link the site's legal
  notice and privacy policy, named as the site names them, where it has them
  (`LambdaOptions.LegalPages`, which genhttp.dev sets). Neither is one of
  the site's pages: none of the site's scripts is ever served at the hosting
  domain.
- **A host nobody has.** Where `LAMBDA_PUBLIC_URL` is set, the platform
  answers only at its own names - `localhost`, the host of that address and
  the hosts in `LAMBDA_MCP_ORIGINS` - and a host that is neither one of those
  nor a lambda's is told nothing is online there, in the same page, with a
  404 on every path. That is usually a domain pointed here before its owner
  entered it, or after it left the lambda, or the lambda left a tier with domains. Served the
  site, it was a copy of the site under somebody else's name, which crawlers
  kept and kept asking for once the domain was a lambda's. The challenges of
  an ACME client are answered there all the same. An installation without
  `LAMBDA_PUBLIC_URL` cannot tell its own names from anybody else's, and the
  platform answers at every host. Below a path no subdomain is a lambda's, so
  one is a host nobody has like any other.
- **Keys are host names.** A key is a label of the hosting domain, so the
  names mail clients and servers look for below a domain (`mail`,
  `autoconfig`, `autodiscover`, `mta-sts`) cannot be claimed, nor can a key
  starting with `xn--`, which a browser reads as an international name. A
  lambda is found by its key as it is spelled, whatever the rules for
  claiming one say today, so a rule made later takes nothing offline.
- **Previews stay on the platform.** The preview of a feature answers at
  `/features/{feature}/`, as it did, which is why a lambda links relatively.
  At subdomains it is the one place a lambda's code still runs on the
  platform's host - with what the platform keeps in that browser in reach of
  its scripts - until previews get hosts of their own as well; below a path,
  every lambda does.

The request log names the host in front of the path - `GET
quiz.genhttp.run/api/items` - as it does for a domain of its own, and the
traffic of a lambda counts its visitors by the host they came to.

At subdomains, what it takes is a wildcard record in the DNS -
`*.genhttp.run`, and `genhttp.run` for the page at its root, pointing at the
server - and a
certificate for both names (see [The hosting domain](#the-hosting-domain)).
The hosting domain belongs on the [Public Suffix List](https://publicsuffix.org/):
until it is there, a lambda can set a cookie for the whole domain, which the
browser then sends to every other lambda as well.

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
included, is counted neither in the lambda's traffic nor in its log, and in the
sandbox goes offline like a deployment does when nobody works on it - measured,
like the lambda's own clocks, from no earlier than the lambda's move into the
sandbox. Once it
does what was asked it is **merged**: its files become the next version, with
its notes, and the feature goes - preview, copy of the data and all; the
lambda's own data is not touched. Only a feature based on the newest version
can be merged, so that a merge never undoes a version saved after the feature
began - by the owner, an agent, or the merge of another feature. There is no
merging or rebasing machinery: whoever works on the feature brings a newer
version's changes in and then says so by moving its `base` (`PATCH`, or
`update_feature`), and the merge takes the lambda's lock and checks the base
once more, so two merges cannot both win. A lambda may have ten features open
(a limit of the installation, the same in every tier, set in the panel); a demo has none. Each holds a full copy of the
workspace and of the database, taken on a thread of its own and swapped in
whole, so ten features of a lambda in production with a full workspace take ten times
its room on disk.

**Data** is what the program keeps: the database, for its records; the
workspace, its private files; and the secrets. It
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
grow as far as the other leaves it. In the editor each kind is a section of
its own - **Database**, **Workspace** and **Secrets**, beside **Versions** in
the group of the program and its data - and each is shown the same way: what it
is, whether it is on, how full it is and its switch, with only what it holds
drawn its own way. The simple view names them **Records**, **Saved by your
app** and **Keys and passwords**, and has each once the app keeps something in
it, or waits for a key. An address from before, `/data` or `/data/:kind`,
opens the kind it names, or the first one the lambda keeps.

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
its documentation, its code, its tests, its preview's log and - under **Test
data** - a section for each kind of its copy of the data. Showcase, domain, figures and
deployments stay with the lambda. A version offers to start a draft from it,
the save dialog of the code offers to save what was typed as a new draft
instead of a version, and a change the agent leaves as a draft can be tried
and put online from the Change section.

### Code and resources

A version is any number of files, in two parts:

```
lambda.cs           the snippet: what it returns is served
Store.cs            more C#, compiled beside it
models/Item.cs      C# in a folder, compiled the same
docs/, tests/       what is written about it (see below)
frontend/, …        anything else: kept with the version, never compiled or served
resources/          what it reads and serves while it runs
```

Its **code** is every file that is not a resource. Its `.cs` files are
compiled into the lambda, in whichever folder they are, as a C# project
compiles its own - so the types can be laid out in folders - and nothing else
is: every other file, of any kind - its documentation and its tests, what a
front end is built from, scripts, configuration - is kept with the version and
never compiled or served. C# that is not part of the lambda, a test or a tool
of its own, therefore does not belong in the code as a `.cs` file. How the
code is arranged is the agent's to decide; the editor only reads `docs/` and
`tests/` as what they are (below).

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

A C# file is named with letters, digits, dashes, underscores and dots,
starting with a letter and ending in `.cs`, 40 characters at most, in folders
named as the rest of the code. A resource keeps the
rules files that are served have always had: letters, digits, dashes,
underscores and dots, an extension, no name starting with a dot, six folders
deep at most. The rest of the code may be named as tools name their files -
dot files, and the brackets, parentheses, plus signs and the like some tools
read meaning off - with no spaces, no `.git` folder, sixteen folders deep at
most. The names a project gives its own files at the top (`Dockerfile`,
`.gitignore`, `.dockerignore`, `LICENSE`, `AGENTS.md`,
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
envelope with `"version": 2`, `LambdaSource.Parse`). C# that was in `.lambda/`
was never compiled and would be now, so it keeps what it holds under a name
that is not: `.lambda/tests/Smoke.cs` becomes `tests/Smoke.cs.txt`. A
feature's files are read the same way. In git, a lambda's history keeps the commits its clones already
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
(a test written in C# is kept as a script, not as a `.cs` file), never served, left out of what identifies a
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
view opens with the product page's first paragraph. The full one ends with how
much of its two allowances the lambda uses - its code and resources, and its
data - each a meter that opens the section it counts.

With twenty sections and more, the full view's sidebar is in groups: the
overview and the documentation on their own, then how people find it and what
they get to see of it (showcase, open source, domain) - what the lambda is to
everybody else, right under what it is - then where a change is made, under
Develop (Change, the drafts, the code, the tests), the program and what it
keeps (versions, database, workspace, secrets), and how it runs (deployments,
connectivity, stats, analytics, logs). Where the window is too short for all
of them, the sections scroll under the lambda's name. On a phone there is no
room for a column beside the page, so a bar at its top names the section open
and stays there as the page moves; it opens every section at once, in the
same groups, two columns of them, with the switch between the views at the
foot. What a section has to say for itself - a problem in the log, a key the
code waits for, a change that ended - is a dot beside its name, and the bar
shows the most urgent of them while the list is closed.

**Code**, in the full view only, is every file of a version: the code and the
resources as two trees beside the editor, each with what it comes to and
whether the public can reach it, and under them what the version comes to of
its allowance. It opens on the newest version, or a draft's files in a draft;
an older one is picked above it. A file of either part can be added, uploaded,
removed and edited there, and saved as the next version (a draft saves in
place and puts its preview online). Links to the sections it replaced -
`/files` and `/build` - open it.

### Databases

Records - entries, accounts, orders, votes - go in the database: the lambda's
own, which its code opens a connection to and reads and writes through
[Entity Framework Core][efcore] - or in plain SQL, where that is simpler. Its
schema is migrations shipped with the version and applied by [Evolve][evolve]
as the lambda starts, never Entity Framework's own. The code names no
database: which one keeps it is the installation's [database
integration](#integrations) - SQLite files by default, PostgreSQL where the
community edition is told so - and the same code runs on either.

```csharp
// resources/migrations/V1__Create_notes.sql:
// CREATE TABLE notes (id TEXT NOT NULL PRIMARY KEY, text TEXT NOT NULL, done BOOLEAN NOT NULL DEFAULT FALSE);
await using (var connection = await Database.GetConnectionAsync())
{
    new Evolve(connection) { Locations = [Resources.Root + "migrations"], IsEraseDisabled = true }.Migrate();
}

return Inline.Create().Get("count", async () =>
{
    await using var db = new Notes(await Database.GetConnectionAsync());

    return await db.Entries.CountAsync();
});

public class Note
{
    public string Id { get; set; }

    public string Text { get; set; }

    public bool Done { get; set; }
}

// maps the table the migration made, on the connection it is handed
public class Notes(DbConnection connection) : DbContext
{
    public DbSet<Note> Entries => Set<Note>();

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseDatabase(connection);

    protected override void OnModelCreating(ModelBuilder model) => model.Entity<Note>().ToTable("notes");
}
```

`Database.GetConnectionAsync()` opens a `DbConnection`, taken from a pool,
which the caller disposes of - or a context it is handed, which owns it - one
per request, never one shared between requests. `UseDatabase` is generated
into every lambda and configures the context the way the database
integration says (`UseSqlite` or `UseNpgsql` underneath). The connection
comes into the context from outside, because inside a `DbContext`, `Database`
is the context's own (the compiler's message says so, and names
`LambdaEnvironment.Database`). `System.Data.Common`,
`Microsoft.EntityFrameworkCore` and `EvolveDb` are imported in every file,
like the GenHTTP modules. Agents are told to await what goes to the database
- `ToListAsync`, `SaveChangesAsync` - in routes that are async: a database on
another machine is then waited for without holding a thread, and SQLite
answers at once either way. This is said, not enforced. The demos all keep
their records like this, `demo-crud` the plainest of them.

`Database.GetConnection()`, the plain call lambdas had before, opens the same
connection and keeps code written then working. It waits for nothing where
the database answers at once - SQLite always does - and holds the thread
until it is open where it is on another machine; Entity Framework's
synchronous methods hold it for every statement.

What a lambda writes for any database: migrations of plain SQL every database
accepts - `TEXT`, `INTEGER`, `BIGINT`, `BOOLEAN`, `DOUBLE PRECISION`, keys the
code makes (a `Guid` as text) or natural ones rather than `AUTOINCREMENT` or
`SERIAL`, names without quotes in lower case - times as ISO 8601 text through
a converter, and a taken unique value told by looking whether the row is there
after a `DbUpdateException`, not by an error code of one database. Names are
matched whatever their case: SQLite does not mind it, and on PostgreSQL the
names Entity Framework asks for are folded to lower case, as PostgreSQL folds
an unquoted name.

Code written before lambdas were handed a `DbConnection` keeps working where
SQLite keeps the database: there `Database.GetConnectionAsync()` and
`GetConnection()` are declared to return a `SqliteConnection`, and `Microsoft.Data.Sqlite` is imported and
referenced, so `UseSqlite(connection, contextOwnsConnection: true)` and
`SqliteException` compile as they did. Where PostgreSQL keeps it, no client
library is handed to a lambda at all.

- **Off until switched on.** Switching it on (the editor, `PUT …/data/database`,
  `enable_data`) makes it, empty. Switching it off deletes it, the copies of
  the features included. Either way the lambda and its previews are started
  again on their next request, so what they do as they start - migrating - is
  done against what is there now. A lambda created as a copy of a demo that
  keeps one starts with its own, switched on.
- **Its own and nothing else.** A lambda does not make connections of its own.
  The code guard refuses making any `DbConnection` - also as a target-typed
  `new(...)`, through an alias or a class derived from one, which is asked of
  the compiler rather than read off the text - and the names that would point
  a connection elsewhere or load native code into it (`ConnectionString`,
  `SetConnectionString`, `LoadExtension`, `DbProviderFactories`, …). The client
  library of a database is handed to the compiler only where the integration
  names it, so a lambda on SQLite cannot name Npgsql, which the community image
  carries. Of Entity Framework, a lambda reaches the namespace a context is
  written in and the three describing a model takes (`Metadata.Builders`,
  `ChangeTracking`, `Storage.ValueConversion`), not its `Infrastructure`,
  `Storage`, `Internal` or the rest, where a context's services and options
  are. `UseSqlite` is refused with anything but a connection, and so are
  `EnsureCreated`, `EnsureDeleted` and Entity Framework's `Migrate` - the
  schema is Evolve's - all asked of the compiler. `dynamic` is refused too: it
  binds members at runtime, which is everything the guard reads the code for,
  skipped. What keeps the SQL sent over the connection inside the lambda's
  database is the integration's - see below.
- **Its room.** The room of the lambda's data - 512 MB in the sandbox, 4 GB
  in production, by default, a [limit](#limits) of its tier - less what its
  workspace takes. A limit the operator lowers, or a move to a tier with less,
  holds from the next connection; nothing stored is removed, and a database
  already larger is read as before and grows no further.
- **Features get a copy,** of the database as it was at one moment while the
  lambda goes on writing. A preview migrates its copy as it starts, which is
  where a migration is tried first; merging throws the copy away.
- **Read by the owner, written by the lambda.** The editor shows its tables,
  their columns and rows under Database (`GET …/database`); `read_database` does
  the same for an agent. Nothing but the lambda writes to it.
- **Versions share it.** A rollback runs an older version against a schema a
  newer one migrated; Evolve lets a version start whose migrations are fewer
  than the database's, and agents are told to change a schema additively, and
  never to edit a migration that was applied.

On **SQLite** (`GenHTTP.Lambda.Integrations.Sqlite`) a database is a file of
its own, `databases/{id}/database.db`, and a feature's copy
`features/{id}/{feature}/database.db`, in write-ahead logging so readers never
wait for a writer. The connection a lambda is handed carries an authorizer
SQLite asks about every statement: `ATTACH` and `VACUUM INTO`, which reach
other files, are refused, as are the pragmas that would point it at other
directories or lift its quota, and SQLite's defensive mode is on; the handle
underneath is a type the code cannot name. The room is enforced by SQLite with
`max_page_count` on every connection: past it a write fails with *database or
disk is full*. A feature's copy is taken with SQLite's backup, and an export
carries the database as an ordinary SQLite file.

On **PostgreSQL** (`GenHTTP.Lambda.Integrations.PostgreSql`, the community
edition only - see [PostgreSQL](postgresql.md)) a database is the schema
`lambda_{id}`, and a feature's copy `lambda_{id}_feature_{feature}`, in the one
database the installation is given. A lambda connects as a role of its own,
`{prefix}_{id}`, which may use and create in its schemas and nothing else - not
the platform's schemas, not another lambda's - with its schema alone on the
search path; the platform owns the schemas and drops them with what is in
them. A role's password is made anew as the platform starts and set when the
role is first used, and kept nowhere. Each lambda and draft has a pool of its
own (10 connections by default), closed after a minute unused. PostgreSQL
cannot hold a schema to a size, so the room governs rather than enforces: a
connection opened while the database is past its room only reads
(`default_transaction_read_only`), which code could switch back - and which
refuses a delete as well, so a lambda past its room stays there until it is
given more room or its database is switched off, and a migration that has to
write fails as it starts. A feature's
copy is made by the lambda's role in one transaction that sees the source at
one moment: its sequences, its tables with what they declare of themselves
(`LIKE … INCLUDING ALL`), their rows, the foreign keys between them and its
views - not functions, triggers or types of the lambda's own, which the guide
tells an agent. An export carries no records; what the schema holds is taken
with `pg_dump --schema=lambda_{id}`.

The databases are not encrypted. SQLite has no encryption of its own: it
would take replacing the SQLite of the whole process with a build that has
(SQLite3 Multiple Ciphers was tried, and is maintained by one person for
.NET), and the key would sit in the platform's database on the same volume -
so it would protect a copy of `/data/databases` on its own and nothing more.
Protect the data volume and its backups instead - on PostgreSQL, the server's.

### Secrets

API keys, passwords and tokens are data, never code: code and resources live in
every version, every export and every reader of the history, so a key put
there can never be taken back. A lambda reads a secret by name:

```csharp
var stripe = new System.Net.Http.HttpClient();
stripe.DefaultRequestHeaders.Authorization = new("Bearer", await Secret.ReadAsync("STRIPE_KEY"));
```

(Calling Stripe from the server is connecting to another one, which a lambda
may only do where it has connectivity - see [Connectivity](#connectivity).)

`Secret.ReadAsync` throws, saying how to set it, when there is no such secret;
`Secret.ExistsAsync` answers whether there is one, and false while secrets are
switched off, for code that works without. `Secret` is there in every file,
like `Workspace`. A value is read from memory unless it changed since; what
the secrets integration is asked then is awaited. `Secret.Read` and
`Secret.Exists`, the plain calls of code written before, read the same and
wait for the integration where it does not answer at once.

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
  `Secret.ReadAsync("STRIPE_KEY")` from `$STRIPE_KEY`. Up to 100 per lambda, 32 KB
  each - a private key fits, a file belongs in the workspace.
- **What the code waits for is said.** The names the code reads with
  `Secret.ReadAsync("…")` or `Secret.ExistsAsync("…")` - or their plain
  forms - as a literal - in the version
  online and in the newest one, or in a feature's files - are listed beside
  what is stored: a name read and not stored is *missing* (reading it fails),
  one only asked about with `Exists` is *optional*. The editor offers to set
  what is missing, the overview asks for it, and `read_lambda` and
  `list_secrets` tell an agent. That is how the two sides meet: an agent
  writes `Secret.ReadAsync("WEATHER_API_KEY")` and switches secrets on, and the
  owner enters the value without ever reading the code - and without the
  value passing through the agent.
- **Read at runtime, not compiled in.** The generated class is handed
  functions to read with once the lambda is loaded (a private field the code
  of the lambda cannot name, and reflection is refused), so a value changed in
  the editor is what the next call reads, without a deploy, and no value is
  ever written into an assembly on disk. What a lambda reads is held in memory
  sealed, per lambda and feature, and opened on each read.
- **Features get a copy.** Like the workspace, a feature starts with a copy of
  the lambda's secrets, its preview reads the copy, a value set in the feature
  (a sandbox key, say) stays there, and merging throws the copy away.

They are kept by the secrets integration (see [Integrations](#integrations)):
the platform's own, which every installation has, stores them in the
platform's database, table `secrets`, sealed with AES-256-GCM.
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
and a copy of it works without one (`Secret.ExistsAsync`). Its accounts remember
whether they were peppered, so accounts made before still sign in.

### Connectivity

Whether a lambda's code may connect to other servers - call an API with
`HttpClient`, open a socket, a WebSocket or mail client, look up a name. Every
lambda shares the server, and lambdas were used as proxies - an API of their
own that fetched whatever address it was handed - so it is a grant of the
lambda's, **decided by the operator**. The tier only says what holds while
nothing was decided about the lambda (`TierPolicy.Connectivity`, as each
installation sets it): production connects by default, the sandbox does not.
A decision about the lambda comes first either way - allowed is allowed in any
tier, declined is declined even in production - and a request that waits
connects nothing until it is allowed (`ConnectivityRule`).

- **Refused as it compiles.** Without connectivity a lambda is compiled without the
  assemblies that connect - HTTP, sockets, WebSocket and QUIC clients, the
  requests of old, DNS, ping, the host's network interfaces and pipes
  (`ReferenceProvider`) - so their types cannot be named, nor reached as the
  type of something else. The code guard refuses the names and namespaces with
  `LAMBDA0003` and a message that says how to ask, rather than leaving the
  compiler to say a type is missing; and what other assemblies would fetch on
  the code's behalf - XML read from an address, a `DataSet` read from one, the
  keys of a bearer token's issuer, `Resource.FromWeb`, a mail client - is
  asked of the compilation (`CodeGuard.InspectConnectivity`). What shares a name
  with them stays: an address family, a mail address, XML parsed from a string,
  a name of the lambda's own. It is governance like the rest of the guard, not
  a sandbox.
- **Part of the build.** Whether a lambda may connect is compiled in
  (`BuildSettings`), so a decision of the operator that takes it away, or a
  change of tier or of a tier's default that does, builds a lambda that was
  built with it again on its next request - the lambda and the previews of its
  features alike - without a deploy. A build made without connectivity keeps serving once the lambda may
  connect: it compiled without the means to, so it does not, and moving a
  lambda to a tier that connects by default does not start it again. Taken
  back, code that connects stops compiling and its requests are answered with
  an error until it changes.
- **Asked by the owner, decided by the operator.** `PUT /lambdas/{privateKey}/connectivity`
  with a `purpose` - what it connects to and why - or the `request_connectivity`
  tool, or the editor's **Connectivity** section (the full view) and a tile on the
  simple overview, which appears once the code connects, or a request waits
  or was declined. A purpose is required. Asked again while it waits, the
  purpose is replaced; after a decline, it waits anew. `DELETE` takes the
  request back, or gives up what was allowed. Moving a lambda to a tier that
  connects by default forgets a request that waits. Demos and blocked lambdas cannot
  ask. The answer, and `connectivity` of every lambda, say `allowed`, `included` (what
  its tier allows while nothing was decided), `status` (`Requested`, `Allowed`, `Declined`), since when, the
  purpose and the operator's `note`, which the owner reads in their editor.
- **The panel** lists what waits in its **Connectivity** section, the longest
  waiting first, with the purpose and whether the code connects at all, and
  what was allowed below it (`GET /admin/connectivity`); the count of what waits is
  beside the section's name. A lambda's page has the same card. Allowing or
  declining - which takes back what was allowed, and what the tier allows by
  default as well - is `PUT /admin/lambdas/{publicKey}/connectivity` with `allow`
  and an optional `note`. A lambda whose tier connects by default is not listed
  unless something was asked or decided about it.
- **What the code does is said.** The summary's `connects` says whether the
  version online or the newest connects, read as the guard reads it; the
  editor marks the section, and the refusal of a deployment for it says so in
  either view with the way to ask.
- **The browser needs none of it.** What a lambda's pages fetch is fetched by
  the visitor's browser, and agents - the build agent included - are steered
  to call a public API from the page wherever it allows that, and to ask only
  where the server has to make the call.
- **Nothing was allowed for the lambdas that existed** when this came: a
  free lambda - a sandbox one now - that connects stops compiling at its next
  build until the operator allows it. When the tiers were named by purpose
  (V25), a decline or a request standing for a lambda in production was
  forgotten, since its tier had included connectivity whatever was decided.

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
drawn by the same parts (`src/Kit/src/components/AgentRun.tsx`): what was asked, the step
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
feature or as a version. It runs on the model and at the effort the owner
chose, of those the operator offers (see [The build agent](#the-build-agent)). A change runs without `create_lambda`, and its editor key travels in
the brief inside the build container, never in a log line.

Not every message is a change. The agent first works out what it was asked:
a **question** about the lambda - who visits it, what was entered, whether it
fails, how something works - is answered from the tools (`analytics`,
`read_database`, `read_logs`, `read_lambda`) without starting a feature; a
**setting** the tools can change is changed without one - analytics switched on,
a kind of data switched on, an earlier version put back online, a feature merged
or one the owner names deleted, the source published (`open_source`); and only
a **change** of the application goes into a feature. The result says which:
`answered` for a question, `settings` for what was switched, published or
discarded. What no tool does - a domain, the showcase, switching anything off,
the value of a secret - the agent tells the owner to do in the editor.

A message may carry the conversation before it (`history`, from the workspace):
the server keeps the newest turns that fit - eight at most, each message cut to
600 characters and each answer to 1000 - and the brief hands them over as
context, never as instructions. Only the message itself is logged; the agent
keeps nothing between messages.

Both kinds of job are told what the agent is for in its system prompt
(`PURPOSE` in `docker/agent/builder.mjs`), above the brief and the request, so a
request cannot talk its way past it: building or changing a lambda through the
tools - and, for the owner of the one it was handed, answering questions about
it - and nothing else. It declines, before it calls a single tool, a request
that is not about an application at all (a question about anything but the
lambda it was handed, a text, homework), one about the
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

### The workspace

`/build` is a workspace in the shape of a chat, for people who do not write
code: the apps this browser knows on the left, a conversation with the build
agent about one of them in the middle, the box at its foot. Before anything is
asked - and as the page is prerendered for search engines - it is the page
that creates a website: what the site says about it, the box, ideas, and the
questions people ask first.

- **The first message builds an app** (`POST /builds`). While it runs, its id
  is kept in the browser, so a reload within the hour the agent keeps a build
  finds it again - and with it the editor key it makes. Once built, the app
  joins the list and its conversation goes on from the build.
- **Every message after is a change** (`POST /lambdas/:privateKey/agent/start`)
  with the newest turns of the conversation as `history`, so the operator's
  switches (building new apps, and asking about the rest), the models and
  efforts offered (see [The build agent](#the-build-agent)), the daily
  allowance and one change at a time hold as everywhere else. A change asked for
  in the editor's Change section shows up in the conversation too.
- **The list is kept in the browser** - local storage, on every installation
  (decided by the owner) - by each app's editor key, with what it is, whether it
  was online and the conversation, the newest sixty turns. An app is added when
  it is built here, or with its editor link or key ("Add an app"); a demo is not
  added. Removing it from the list forgets it in this browser only. A script on
  the platform's own origin can read that storage: a draft's preview at
  `/features/…`, and every lambda of an installation that answers by path.
- **Which app is open is in the address by its public key** (`?app=…`), never
  its editor key.
- **It is not the editor.** Above the conversation it asks only for what waits
  for the owner: putting an app that went offline back online, the keys and
  passwords its code waits for (entered here, to the API, never through the
  agent), and a draft to try and put online. Under an answer: open the app, try
  or put online the draft it left, undo a change that went online. Everything
  else is the editor, a link away.

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
The **full** view is every section, as before. Neither has a section for the
operator: a lambda is blocked on its page in the panel (see
[Blocking](#blocking)).

Each lambda says which view it opens in, `view` - `Full` or `Simple` - set
when it is created (`POST /lambdas`, `create_lambda`) and changed later
(`PATCH /lambdas/:privateKey`, `update_lambda`). The build agent creates its
lambdas `Simple`; everything else defaults to `Full`. That is only the
default: whoever switches at the foot of the sidebar (of the list of sections on a phone)
has chosen for themselves, for that lambda, which is kept in their browser and
never sent anywhere - so an operator looking at somebody's lambda in the full
view leaves it simple for its owner.

### Analytics

An owner can have the visits of a lambda counted, and reads them in the
editor's **Analytics** section, in both views: visitors, page views and
requests per day, and the pages, referring sites, countries, devices,
browsers, operating systems and crawlers behind them - search engines, AI
assistants, chats fetching a preview. Opt-in: nothing is counted until the
owner switches it on (`PUT …/analytics`, or the `analytics` tool when the
user asks an agent), and switching it off (`DELETE …/analytics`) deletes
what was counted. A demo's visitors are not counted, and neither is a
feature's preview.

- **By day, in UTC, kept for a year.** How much of the year the owner sees is
  a limit of the tier - seven days in the sandbox, all of it in production and
  the system tier, by default (see [Limits](#limits)) - so a lambda moved to a
  tier that shows more shows what was counted before it was. A range of the same length before is summed for a
  comparison only where the tier shows it as well.
- **Counted in front of the lambda, written behind it.** A request to a
  lambda or a preview is made into one visit (`LambdaVisit`: its status, its
  time, its bytes, its path, its caller's country and agent) by
  `LambdaVisitConcern`, which only makes it and leaves it on the request.
  Whatever wants to know about requests to lambdas reads it rather than
  measuring again: the overview's figures (`LambdaActivityConcern`), the
  request log (`CallerConcern`), and `AnalyticsConcern`, which puts it on a
  lock-free queue where the owner counts visits. Each leaves out what is not
  its own - the figures and the analytics previews. `AnalyticsJob` empties the queue every
  `LAMBDA_ANALYTICS_INTERVAL_SECONDS` (five minutes), sums it into a row
  per lambda, day and value, and writes those in one transaction. Nothing is
  written while a visitor waits; visits not yet written are lost when the
  server stops. The queue holds 250,000 visits; what does not fit is dropped
  and logged.
- **What a visit is counted as.** A *hit* is every request answered; a *view*
  is somebody opening a page - a browser's navigation (`Sec-Fetch-Dest:
  document`, a revalidation included) or, from a program that does not send
  that header, a successful `GET` answered with HTML - unless it asked for
  a file that is no page (`/favicon.ico`, an image, a script, a style
  sheet, …), which a single page app answers with its page too. Of the views, those
  of programs - crawlers, and whatever names no browser - are counted apart
  as well (`bot_views`, `botViews` in the answer), everywhere a view is: the
  editor shows the pages people opened and adds the bots' behind a switch
  (**Include bots**), off by default and remembered in the browser, worked
  out from the one answer so it switches at once. The referring sites stay
  people's either way: a bot names no page it came from and would be counted
  as typed in. A path is counted where
  it was answered without an error, at most 200 a day, and the rest under
  `(other)`, so a scanner cannot fill a day. A referrer is kept as its host,
  without `www.`, and only where it is another site than the one visited (100
  a day). Country, device, browser and system come from the address and the
  `User-Agent` the request line already has, and from the client hints
  (`Sec-CH-UA`, `-Mobile`, `-Platform`) where a Chromium browser sends them -
  its agent no longer says which phone or system it is on. Hints are only
  believed from an agent that is Chromium's, and only where they name a
  browser or system that is known, so nobody writes words of their own into
  an owner's figures (`VisitorClassifier`).
- **Two groups in the editor, by what they count.** The pages, the referring
  sites, the countries, the devices, the browsers and the systems are counted
  in page views, under that heading; what was requested by path and the
  crawlers in requests, under theirs. The two can be far apart - a crawler
  asks for a hundred things that are no page - so a card never leaves the
  reader to guess which it is. A lambda without pages people open has the
  requests group only.
- **Crawlers, hardened.** A program is told from a person by
  [crawler-user-agents](https://github.com/monperrus/crawler-user-agents)
  (MIT, `Resources/Crawlers`), a maintained list of about 1,500 crawlers with a
  kind each - search engine, AI crawler, link preview, monitoring, scanner,
  HTTP library and so on - and counted under that kind, never as a browser
  or a visitor. Google, Bing, OpenAI, Apple and Perplexity publish the
  addresses their crawlers come from; a visit is only counted as theirs when
  the connection came from one of them, and as an unconfirmed claim otherwise
  - anybody can send `Googlebot`. The address checked is the connection's,
  never a forwarding header's, so an installation behind a proxy confirms
  none. `CrawlerJob` fetches the list and the addresses daily into
  `/data/crawlers` (`LAMBDA_ANALYTICS_CRAWLER_LISTS`,
  `LAMBDA_ANALYTICS_CRAWLER_REFRESH_HOURS`), the list of the release
  standing in until it has. Matching is linear in the length of the agent
  whatever it says: the list's plain text is looked for in one pass
  (`SearchValues`), its few expressions run without backtracking, and only
  500 characters of an agent are read.
- **Visitors without keeping anybody.** A visitor is their address and their
  agent, hashed with a key made at random per day and lambda, held in memory
  for today and yesterday only (`DailyVisitors`), and only a person who
  opened a page is one: a request for anything else - a script, an API call,
  a path a scanner guessed - makes nobody a visitor, whatever agent it names,
  so there are never more visitors than page views. So a visitor counts once a day; a restart counts somebody again who comes back the same day, and an
  installation that keeps no addresses (`LAMBDA_LOG_CLIENT_ADDRESS=false`)
  counts no visitors. No cookie, no script in the lambda.
- **A database of its own.** `analytics.db` beside `lambda.db`, a star:
  `lambdas`, and a table of facts per statistic - `days` (the totals),
  `pages`, `referrers`, `countries`, `devices`, `browsers`, `systems`,
  `crawlers` - keyed by lambda, year, month and day (and value), so a
  lambda's days are a range of the key. Migrated by Evolve from
  `Services/Analytics/Migrations`. Only the switch is in `lambda.db`
  (`lambdas.analytics_since`), read with everything else that serves a
  request. Deleting a lambda deletes its rows.

### Taking a lambda away

The code belongs to whoever made the lambda, so `GET …/export` (the editor's
Download) packs its newest version into an ordinary .NET 10 project that runs
without this platform (`Services/Deployment/ProjectPacker.cs`):

| File | What it is |
|---|---|
| `Program.cs` | the default GenHTTP host, `Host.Create().Handler(Project.Create()).Defaults().RunAsync()`, under a header naming the lambda, its version and change, the export date, where to read about GenHTTP, and the environment variables its secrets become (as `-e` flags in the `docker run` line too) |
| `Project.cs` | `lambda.cs`: its statements are the body of `Project.Create()` (`CreateAsync()` when they `await`), its types sit beside the class, made public as on the platform |
| `*.cs` | the other C#, in its folders, its code unchanged, the first letter of the name capitalized |
| `Platform/` | what the platform provided: `Workspace` and `Resources` as folders (with `Assets` beside it where the code still says that), `Secret` reading environment variables of the same name (the values are never exported), `Database` opening `database/database.db` where the code uses one and `UseDatabase` configuring a context on it with SQLite, the switch that turns what `Project` returns into a handler, and the imports every lambda gets as global usings |
| `resources/` | its resources, copied beside the program on build |
| `docs/`, `tests/`, … | the rest of its code where it was - its documentation, its tests, what it is built from; the project compiles every `.cs` file but the resources and what dotnet and the app write (`EnableDefaultItems` is off), and the image takes only what it compiles and copies |
| `database/database.db` | the lambda's database, an ordinary SQLite file - its records go with it |
| `Dockerfile` | builds and runs it; the workspace is `/app/workspace`, the database folder is mounted at `/app/database` |

It references `GenHTTP.Full` - the internal engine, which runs wherever .NET
does - at the version this server runs, so it behaves as the lambda did, and
`Microsoft.Data.Sqlite`, `Microsoft.EntityFrameworkCore.Sqlite` and `Evolve`
only where the code uses them - Entity Framework where it has a `DbContext`,
at the release of the SQLite library for .NET 10. Of the data,
the database comes along - copied with SQLite's backup, so it is whole even
while the lambda writes to it, and any SQLite tool reads it - while the
workspace starts empty and the values of the secrets stay here. An exported
project keeps its records in a SQLite file whatever kept them on the
platform, since the code names no database; where PostgreSQL kept them, the
export carries none, and `pg_dump` of the lambda's schema takes them along. `database/` is
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
leads to `/build`. The pages are a component of their own (`SourcePages`, its
bundle in `src/GenHTTP.Lambda.Sources/Frontend`), in a frame of their own
without the site's menu,
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
`.cs` files keep their code and are named the .NET way, and every
other file is where the lambda has it: the rest of its code in its folders,
its resources in `resources/`.
Around them are the platform's files: `Program.cs`, the project file,
`Platform/`, the `Dockerfile`, `.gitignore`, `.dockerignore`, `AGENTS.md` -
how an agent works in the repository, with the rules of the platform in git's
terms - `CLAUDE.md`, which points Claude Code to it, and `LICENSE` while the
source is published. Those are the same in every commit until the platform
changes them - nothing in them depends on the version, which is why the
project references SQLite, Entity Framework and Evolve whether the code uses
them or not, compiles every `.cs` file of the code and nothing among the resources, and
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
commits than the installation keeps versions is refused, asking for them to be
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

A set of .NET 11 projects below `src/`, hosted by `GenHTTP.Full.Ioxide`: the
platform, the components over it - each with a builder in front, the way
GenHTTP's modules are made (`X.Create(...)`) - and two hosts that put them
together.

| Project | What it is |
|---|---|
| `GenHTTP.Lambda.Core` | the platform: `LambdaPlatform`, its services (`Services/`), its database and migrations (`Data/`), the demos, the licenses, the files of an export and the API's introduction (`Resources/`), its options (`Configuration/LambdaOptions.cs`) and its logging (`PlatformLogging`) |
| `GenHTTP.Lambda.Web` | what serving pages takes, for every component that has some: `Bundle`, a built frontend on disk, found below `bundles/<name>/` beside the host; `ClientApplication`, a bundle drawn in the browser (the editor, the panel); `SitePages`, a site's public pages named and prerendered per language, with `robots.txt`, `sitemap.xml`, `llms.txt` and, where asked, `/.well-known/ai-catalog.json`; `SiteMeta`, `SitePrerender` and `SiteLanguages`; `PageBranding`, which names every index page for the installation as its operator branded it |
| `GenHTTP.Lambda.Execution` | `Runner.Create(platform).Site(site)`: serves the lambdas - at their own domains, at subdomains, below `/lambda/` where they answer by path, and the previews below `/features/` - and hands every other request to the site; `HostingPages`, with their words in its own `Resources/hosting.json` |
| `GenHTTP.Lambda.Api` | `RestApi.Create(platform)`, mounted at `/api` |
| `GenHTTP.Lambda.Mcp` | `McpServer.Create(platform)`, mounted at `/mcp` |
| `GenHTTP.Lambda.Editing` | `Editor.Create(platform)`, mounted at `/editor`: the editor, the page that creates a lambda, and the owner's git repositories `/editor/{key}/{name}.git` |
| `GenHTTP.Lambda.Administration` | `AdminPanel.Create(platform)`, mounted at `/admin`: the operator's panel, pages only, over the REST API - of the platform it takes only how the installation presents itself, to name its page |
| `GenHTTP.Lambda.Sources` | `SourcePages.Create(platform)`, without a path of its own: the pages at `/{language}/source` and the public git repositories `/source/{key}.git` |
| `GenHTTP.Lambda.Integrations` | the contracts of what the platform keeps its data in - `IDatabaseProvider`, `IFileProvider`, `ISecretProvider` - and nothing of the platform's (see [Integrations](#integrations)) |
| `GenHTTP.Lambda.Integrations.Sqlite` | the database every installation has unless it chooses another: SQLite files in the data directory (`SqliteDatabase.Create()`) |
| `GenHTTP.Lambda.Integrations.PostgreSql` | one PostgreSQL database, a schema per lambda (`PostgreSqlDatabase.Create(connection)`); referenced by the community host only |
| `GenHTTP.Lambda.Server` | `LambdaServer.Create(platform)`: the engine, the ports, TLS and ACME, the platform's concerns around every request (`PlatformHosting`), the signals and the note about the run; `ServerSettings`; `LambdaSettings.FromEnvironment()`, the only place below the hosts that reads the environment |
| `GenHTTP.Lambda.Cloud` | the host of genhttp.dev: `Program.cs`, `Website` (its own pages), `CloudInstallation` (its terms, its contact and abuse mailboxes, its legal pages), `CloudStructuredData`, its `Dockerfile` |
| `GenHTTP.Lambda.Community` | the host of the community edition: `Program.cs`, which lets its operator brand it, `LandingPage`, its `Dockerfile`, published as `genhttp/lambda` |

A host's `Program.cs` is the whole composition. The community edition's:

```csharp
// the LAMBDA_* variables, with the lambdas below /lambda/ on this address
// unless LAMBDA_HOSTING_URL names a domain for them, the daily usage report
// on unless LAMBDA_USAGE_REPORTS=false, and the data in SQLite unless
// LAMBDA_DATABASE=postgresql
var settings = LambdaSettings.FromEnvironment(LambdaAddressing.Paths, usageReports: true, databases: CommunityDatabase.Offered);

// whoever runs it may give it a name and a logo of their own, in the panel,
// and say whom it is for: themselves (home) or an organization (enterprise)
await using var platform = await LambdaPlatform.Create()
                                               .Options(settings.Platform with
                                               {
                                                   Branding = true,
                                                   Profile = InstallationProfile.Home,
                                                   Profiles = [InstallationProfile.Home, InstallationProfile.Enterprise]
                                               })
                                               .Database(CommunityDatabase.Of(settings.Database))
                                               .BuildAsync();

var site = Layout.Create()
                 .Add("api", RestApi.Create(platform))
                 .Add("mcp", McpServer.Create(platform))
                 .Add("editor", Editor.Create(platform))
                 .Add("admin", AdminPanel.Create(platform))
                 .Add(SourcePages.Create(platform))
                 .Add(LandingPage.Create(platform));

return await LambdaServer.Create(platform)
                         .Settings(settings)
                         .Handler(Runner.Create(platform).Site(site))
                         .RunAsync();
```

The cloud's reads the environment with the lambdas at subdomains, no usage
report and SQLite alone (`LambdaSettings.FromEnvironment()`, which refuses
`LAMBDA_DATABASE=postgresql` there), adds what is its own to the options
(`CloudInstallation.Of(settings.Platform)`, which makes it the cloud) - but
not the branding nor a choice of profile, genhttp.dev being GenHTTP Lambda -
and has `Website` where this has `LandingPage`.

`LambdaPlatform.Create().Options(...).BuildAsync()` builds the integrations,
migrates the platform's records and its analytics through the database
integration, builds the services into one container and reads what every
request and page needs before the first request - the limits, the operator's
switches, the guide's section, the domains served, the blocked addresses, the
brand, the pictures moved out of the records they were kept in before - once
per process. It is awaited: on
PostgreSQL that is where the server is first reached; every component's
builder is handed the platform and takes what it needs from it, and makes its
own singletons - the MCP tools, the pages of a bundle - from those services,
so the container registers nothing of the components. `LambdaServer` starts
the platform's background jobs once it is listening, and seeds the demos
after that. Disposing of the platform stops the jobs and the services, and
gives the console back. Since nothing below the hosts reads the environment, a
component runs the same in a test (`LambdaFixture`, which puts them together
the way the cloud does, or the community edition) as in a host.

Every request passes the platform's concerns in front of the runner, added by
`PlatformHosting` - from the outside in:

1. `AddressBlockConcern` - a blocked address is refused, the panel excepted
   (see [Blocking](#blocking));
2. `LambdaRedirectConcern` - an address a lambda no longer answers at is sent
   on to where it does;
3. the scope of the request, which every dependent concern is resolved from;
4. `CallerConcern` - the request line, with who asked;
5. GenHTTP's defaults - compression, the upgrade to HTTPS and the rest;
6. `TelemetryConcern`;

then the answers to ACME challenges, where `LAMBDA_ACME_DIRECTORY` is set, and
the runner. Its `DomainRouter` decides once whose host a request is for: a
lambda's own domain or its subdomain go to that lambda, the hosting domain to
its page, a host nobody has to the page that says nothing is there, and the
platform's names to the previews, the lambdas below `/lambda/` where they
answer by path, and the site.

The frontends are an npm workspace at the root (`package.json`,
`tsconfig.base.json`). What they share is source, not a build: `src/Kit`
(`@genhttp-lambda/kit`) has the shell and the components, the i18n machinery
and the words several bundles use, the API client, the workspace of
`/build`, the guide of `/ship`, the showcase, the Tailwind preset, the Vite configuration every bundle is
built with (`vite.ts`) and the scripts that translate and prerender
(`scripts/translations.mjs`, `scripts/prerender.mjs`). Every component with
pages has a `Frontend/` folder of its own - Editing, Administration, Sources,
Cloud and Community - which `npm run build` at the root builds into `bundle/`
beside its project; the project copies that into the output of every host
below `bundles/<name>/` (`editor`, `admin`, `sources`, `cloud`, `community`),
where `Bundle.Beside` finds it, and a builder's `.Bundle(directory)` points
elsewhere. No component serves another's bundle. The sites' public pages are
prerendered per language as they are built.

The platform is divided into services, a folder each below `Services/` of
`GenHTTP.Lambda.Core`. From outside its folder a service is taken only by its
interfaces - by the API resources, the MCP tools, the other components and
the other services alike - and the classes a service is made of stay in its
folder; `ServiceBoundaryTests` fails on a constructor or method elsewhere, in
the assembly of any component, that takes one of them. Where other services
work through a lower layer of a
folder rather than the service the API calls, that layer has an interface of
its own (`IDatabaseVault`, `ISecretVault`, `IDomainRegistry`, `ILogBook`,
`IRunLog`).

- **Meta** (`Services/Meta`) - lambdas and their versions, the only part of
  the platform that speaks to the database. Its public surface is DTOs, mapped by hand.
  `MetaService` is what the API, MCP and the other services call. It finds
  the lambda, checks it may be changed, takes the lambda's turn for a change,
  and leaves the rest to units: `LambdaHosting` changes the key, the domain
  and the tier, `LambdaHistory` writes the versions, the stretches online and
  the events and keeps each to its bound, `LambdaLifetime` says when a lambda
  goes offline and away and runs the sweep that does it, `LambdaRemoval`
  removes a lambda with everything that hangs on it, and `LambdaDescriber`
  maps the DTOs. `LambdaGuard` (who may change a lambda - not a demo) and
  `VersionInput` (what a version may hold) are rules other services apply too.
- **Settings** (`Services/Settings`) - the panel's switches, whom the
  installation is for (`SiteSettings.Profile`, among what its host offers,
  see [Who it is for](#who-it-is-for)) and the limits, and what each tier
  means. `TierPolicies` maps a tier to a `TierPolicy` -
  what a version and the data may come to, the days of analytics shown,
  whether it connects by default, has domains of its own, expires when unused,
  is the installation's own or read only, and its label - from what the
  operator set (`LimitsService`), and is the one place a tier is compared with
  a value: every check reads `ILimitsService.PolicyOf`, a query of the tiers
  whose policy says so `TiersWhere`, and a refusal names the tier above with
  `Above`. The demo seeder is the exception, since it is what puts a lambda
  among the demos.
- **Guide** (`Services/Guide`) - what `platform_guide` answers: one Markdown
  document for agents (`GuideDocument`), the same for MCP and the operator's
  routes, with the section the operator wrote last. The section is two
  settings held in memory once read, and `GuideChecks` says what it may be
  (see [Rules for agents](#rules-for-agents)).
- **Branding** (`Services/Branding`) - how the installation presents itself:
  the name and the logo its operator gave it, where its host lets them
  (`LambdaOptions.Branding`), held in memory as a `Brand` once read.
  `BrandChecks` says what a name and a logo may be, `Monogram` draws the first
  letter of a name for an installation named without a logo (see
  [Branding](#branding)).
- **Storage** (`Services/Storage`) - the code itself, on the file system, one
  file per version. Never in the database.
- **Data** (`Services/Data`) - which kinds of data a lambda keeps, switched on
  and off by its owner. Only the choice is stored: a lambda without a row for a
  kind has its default, so a kind added to `DataKinds` needs no migration to
  exist. Switching a kind off deletes what it held.
- **Workspace** (`Services/Workspace`) - the private files of a lambda, kept
  by the files integration. The generated `Workspace` class of a lambda holds
  functions the platform hands it once the lambda is loaded and does nothing
  with files itself: `LambdaWorkspace` resolves the names, holds a write to
  the room and asks the integration - under the same rules the editor's
  uploads meet (`WorkspacePaths`), so a file put there by hand behaves like one
  the lambda wrote itself. Only the room it takes is limited - in any number of
  files of any size - counted in blocks of 4 KB as the disk counts it, so an
  empty file or a folder is not free. It shares the room of the lambda's data
  with its database: the data's allowance, as the lambda's tier allows it now,
  and the size of the database are asked as a file is written
  (`IDatabaseVault.DataRoomFor`), so moving the lambda to another tier, or
  changing what a tier allows, leaves it running as it is. `WorkspaceVault`
  (`IWorkspaceVault`) hands a lambda its functions, copies and removes a
  workspace, measures what one takes, keeping the answer a few seconds, and is
  what the database asks in turn for the room it may grow to
  (`DatabaseVault.RoomOf`).
- **Secrets** (`Services/Secrets`) - the secrets of the lambdas, kept by the
  secrets integration. `SecretVault` decides what a name and a value may be,
  how many a lambda keeps and whether its secrets are on, holds what a running
  lambda reads, and is what the other services keep in step with a lambda
  (`ISecretVault`); `SecretService` is what the API and MCP call - by name,
  never by value.
- **Databases** (`Services/Databases`) - the database of each lambda and the
  copy per feature, kept by the database integration. `DatabaseVault` decides
  whether a lambda has one and how large it may grow, has them made, copied,
  exported and deleted and is what a running lambda connects through
  (`IDatabaseVault` for the other services), and `DatabaseService` is what the
  API and MCP read tables and rows with - the catalog asked of the integration,
  the page of rows and how a value travels decided here.
- **Integrations** (`Services/Integrations`) - which integrations the
  installation has (`PlatformIntegrations`, built with the platform), and what
  the operator and the usage report are told of them (`IIntegrationService`).
  See [Integrations](#integrations).
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
  `GitRoutes.cs`, which the editor mounts below `/editor/` and the published
  sources below `/source/`.
- **Analytics** (`Services/Analytics`) - the visits counted for an owner who
  switched analytics on (see [Analytics](#analytics)). `AnalyticsService` is
  what the API and MCP call and what writes a batch down, `AnalyticsQueue`
  holds the visits until then, `VisitTally` sums them into rows,
  `VisitorClassifier` says what a visitor was - with `CrawlerList` and
  `CrawlerRanges`, kept current by `CrawlerJob` - `DailyVisitors` who, and
  `AnalyticsStore` keeps `analytics.db` - `IAnalyticsStore` for Meta, which
  forgets a deleted lambda's rows.
- **Quiet hours** (`Services/QuietHours`) - when the installation and each
  lambda are usually quiet (see [Quiet hours](#quiet-hours)).
  `QuietHoursConcern` counts every request into `HourlyCounts`, without a
  lock, `QuietHoursJob` has `QuietHoursService` write the counts down hourly,
  and `QuietProfile` is the typical day a row keeps and what is read off it.
- **Optimization** (`Services/Optimization`) - keeping the databases in shape
  (see [Optimizing the databases](#optimizing-the-databases)).
  `OptimizationService` decides which database is due, asks the database
  integration and writes down what each came to; `OptimizationSchedule` says
  when a database is due and whether now is its hour, from the quiet hours.
- **Building** (`Services/Building`) - the jobs of the build agent: builds
  asked for on `/build` and changes asked for in the editor's Change section.
  `BuildService` decides what may be asked for and in which order that is
  checked, and leaves the rest to units of their own: `AgentClient` is the
  only thing that talks to the agent's runner and holds the shared secret,
  `BuildAllowance` counts the jobs of each address per day, `ModelGate` says
  which model and effort a job runs on and who may have the models behind the
  password, `AgentSettingsStore` keeps what the operator decided about them,
  and `AgentInput` checks the prompt and the language a page sends.
- **Deployment** (`Services/Deployment`) - wraps a snippet in a method body,
  compiles it with Roslyn, loads the assembly and calls `PrepareAsync()` on
  the resulting handler. Compiled once, then cached - one handler for what a
  lambda has online, and one for the preview of each of its features - on
  the first request, or as the server starts for the tiers kept warm
  (`LambdaWarmup` in `Services/Meta`, see [Tiers](#tiers)). One snippet is
  compiled at a time. An
  assembly stays loaded for the life of the process, so a build is known by
  its compiled C#: a version that changes only resources or the rest of its
  code - documentation, tests - gets a new handler from the assembly already
  loaded rather than being compiled
  again. Lambdas are compiled against the metadata of the platform's
  assemblies, read once and kept - the metadata alone, not the whole files.
- **Execution** (the runner, `GenHTTP.Lambda.Execution`) - not a service but
  the component that serves the lambdas: `LambdaRoute` puts the concerns in
  front of every lambda around `LambdaExecutionHandler`, an `IHandler` that
  looks up the handler for a request and runs it, once per way a lambda is
  found - its domain, its subdomain, its path, a feature's preview.
- **Hosting** (`Services/Hosting`) - where the lambdas answer (see
  [Where the lambdas answer](#where-the-lambdas-answer)). `LambdaAddresses`
  (`ILambdaAddresses`) builds the address of a lambda - at its subdomain or
  below `/lambda/` - and reads the lambda off a host, `DomainRegistry` holds
  the domains of their own and decides once per request whose host it is,
  `PlatformRoutes` names where each component is mounted, and
  `LambdaRedirectConcern`, in front of everything the server does but the
  address block, sends an address a lambda no longer answers at on to where
  it does. The runner's `DomainRouter` hands a request to a lambda, to the
  page of the hosting domain, to the platform or - for a host nobody has - to
  the page that says nothing is there.
- **UsageReports** (`Services/UsageReports`) - the daily usage report (see
  [Usage reports](#usage-reports)). `UsageReportService` makes it, sends it
  and keeps how the last one fared for the panel, `UsageSurvey` gathers the
  figures - from the platform's database, the vaults and a few folders of the
  data directory - `UsageReportSender` posts it, with a time limit, and
  `UsageReportJob` asks for one a day.
- **Protection** (`Services/Protection`) - concerns in front of execution that
  resolve the lambda, rate limit per client and cap concurrency. A lambda is
  resolved from memory (`Services/Meta/ResolutionCache.cs`), which every write
  to the database makes stale (`Data/DatabaseChanges.cs`), so serving a lambda
  does not query the database.

A concern that needs nothing but services is a dependent concern of GenHTTP
(`IDependentConcern`, added with `Dependent.Concern<T>()`): a singleton in the
container, resolved per request from the scope that `PlatformHosting` opens
around every request. That is the line each request is logged with, the server's
telemetry, and in front of every lambda the throttle, the mark on what it
prints, its activity and the rate limit. The lookup of the lambda is built by
hand, once per route, because its locator is what tells the routes apart.
`Infrastructure/ServerRegistry.cs` holds the running server for the one thing
that prepares handlers outside a request: the deployment service, compiling
what the demo seeder, a deployment or a merge asks for. Whatever answers a
request reads `IRequest.Server`.

What is printed to the console - what a lambda prints, and what the engine
says about its reactors - is kept by `Services/Diagnostics/ConsoleCapture.cs`,
which the platform owns: it puts a tee over the console, once per process,
and files what is printed outside any lambda into the book of every
platform running, until that platform is disposed of. The logger
writes its lines to the console it was given before the tee existed, and takes
the lock of `Console.Out` first - the console takes that lock itself under
every write, and in the other order a print and a log line waited for each
other for good.
- **Background** (`Services/Background`) - a small scheduler that runs the
  sweeps: it takes sandbox lambdas offline once nobody has visited or edited
  them for the window of the [limits](#limits), and removes them later.

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
is ever compiled, and a lambda that does need files gets a workspace of its
own, which it reaches through the platform.

It also refuses code that waits for a task: `.Result`, `.Wait()`,
`.GetAwaiter().GetResult()`, `Task.WaitAll`, `Task.WaitAny` and
`SemaphoreSlim.Wait()`, asked of the compiler so a `Result` of the code's own
is not mistaken for one. Requests run on one thread per core, and a task
finishes on the thread that would be waiting for it - so it never would, and
that core would serve nothing again. Agents are told to await instead - and
the database, the workspace and the secrets have asynchronous members for it
(`Database.GetConnectionAsync()`, `Workspace.ReadTextAsync`,
`Secret.ReadAsync`), which are what they are taught.

On the ioxide engine requests run on its reactors, one thread per core, and
what a request awaits is resumed on its reactor; on Kestrel and the internal
engine they run on the thread pool. What the platform asks of an integration
is awaited - the contracts return `ValueTask`s - so an integration that waits
on the network does so without holding a thread, and one that answers at
once (SQLite, local directories, the platform's own secrets) completes on the
thread that asked, which on ioxide never leaves its reactor. The platform's
own records are awaited the same way, through Entity Framework: over SQLite
every query and save completes on the thread that asks, over PostgreSQL it
waits for the server without holding a thread - so on a distant server the
editor, the panel and the API wait for it without taking a thread from
anybody else. What every request reads - which lambda answers to a key, the
limits, the brand, the domains served - is kept in memory: read as the
platform starts, or by the first request after a write. What takes long -
compiling and binding with Roslyn, packing, copying or exporting a workspace
or a database - is handed to the thread pool in one hop
(`Infrastructure/Offload.cs`) and the request resumes on its reactor
afterwards. The plain calls a lambda kept from before its members were
awaited - `Database.GetConnection()`, `Workspace.ReadText`, `Secret.Read` - go
through `Infrastructure/Synchronous.cs`, which takes the synchronization
context off the thread before the work starts, so what it awaits is never
posted to the reactor it blocks, and waits for nothing where the integration
answered at once. `src/BannedSymbols.txt` refuses the synchronous EF
calls, waiting for a task and `Task.Run` when any project below `src/` is
compiled - every component runs on the same reactors - so every
hop to the pool goes through `Offload` and the one wait through
`Synchronous`; the pieces of work started in the background rather than
awaited - the scheduler's jobs, seeding the demos, warming up the lambdas -
say so where they start. A query of the platform's records enumerated
synchronously compiles all the same; it is logged as it runs, a warning per
statement (`Data/SynchronousRecords.cs`). CLAUDE.md has the rules.

Compiled lambdas run in the server process. The guard raises the cost of
misbehaving; it is not a sandbox, which is why the container runs unprivileged
and the seccomp profile is left alone.

### Integrations

What the platform keeps its data in is an integration of one of three kinds,
each behind a contract of `GenHTTP.Lambda.Integrations` - a project that takes
nothing of the platform's, so an integration can be built and exchanged on
its own:

| Kind | Contract | Keeps | The platform's own | Others |
|---|---|---|---|---|
| database | `IDatabaseProvider` | the platform's records and analytics (`DatabaseStore.Platform` and `Analytics`), and the database of every lambda and draft | SQLite files in the data directory (`SqliteDatabase`) | PostgreSQL (`PostgreSqlDatabase`), community edition only |
| files | `IFileProvider` | the workspace of every lambda and draft, and the platform's pictures (`FileStore.Branding` and `Showcase`) | directories in the data directory (`LocalFiles`, `GenHTTP.Lambda.Integrations.Local`) | - |
| secrets | `ISecretProvider` | the secrets of every lambda and draft | sealed in the platform's own records (`DatabaseSecrets`), wherever the database integration keeps them | - |

A host chooses them on the platform's builder - `LambdaPlatform.Create()
.Database(...).Files(...).Secrets(...)` - and the platform uses its own for a
kind it is given nothing for. Each is built with what the platform hands it
(`IntegrationContext`: the data directory, the loggers) before anything is
migrated - awaited, `IIntegrationBuilder<T>.BuildAsync`, so one may reach its
server as it starts - and disposed of with the platform. One of each per installation:
moving what one holds into another is not something the platform does.

**An integration keeps; the platform decides.** Whether a lambda has a
database, how much room its data takes, what a secret may be called and how
many there are, which names a workspace refuses, who may read what - all of it
is decided in the services (`DatabaseVault`, `SecretVault`, `WorkspaceService`
and `LambdaWorkspace`) before an integration is asked, so exchanging one
changes none of the rules. Addressed by the ids of a lambda and a feature
(`DataScope`), never by keys.

**Asynchronous, as the infrastructure needs it.** Every member that reaches
the infrastructure returns a `ValueTask` and takes a `CancellationToken`;
what an integration says of itself (`Describe`, `Guide`) and what only
configures (`ConfigureStore`, `ConfigureContext`, `Binding`, `Quote`, `Tree`)
is synchronous. How it answers is the integration's own. One that waits on the
network - a database server, object storage, a vault - awaits it, and Kestrel
and the internal engine lose no thread to it. One that answers at once returns
tasks that have completed: SQLite and the local directories are synchronous
inside, and so the platform's own secrets are on SQLite, which await its
records - which is what the Ioxide engine wants,
since a request runs on the reactor of its core and what it awaits is
resumed there. PostgreSQL awaits Npgsql, which works on Ioxide but does not
complete on the reactor. A whole database or workspace - a draft's copy, an
export - is asked for away from the reactor (`Offload`), so an integration
that answers at once may take its time there.

**Kinds, dialects and stores are enums** - `IntegrationKind`,
`DatabaseDialect`, `DatabaseStore`, `FileStore` - and what one is called where
data is kept - a file, a schema, a directory - is the integration's to spell
out, and stays what it was: SQLite keeps the stores in `lambda.db` and
`analytics.db`, PostgreSQL in the schemas `platform` and `analytics`, the
local files in `branding/` and `showcase/`.

**No picture is kept in a database.** The pictures the platform keeps for
itself are files of the files integration, in stores as the database's are
(`FileStore`): the logo at `branding/logo`, the picture of a showcase
entry at `showcase/{lambda}`. Their rows say what a picture is and how large
(`image_bytes`, V27). An installation from before kept them in their rows: the
platform moves them out once as it starts (`PictureMove`), reading a picture
from its row until it is moved, and the emptied columns are dropped in a later
release. A picture goes with its entry, and with its lambda.

**A lambda names none of them.** It reaches its database through
`Database.GetConnectionAsync()` - a `DbConnection` - and `options.UseDatabase(connection)`,
both generated into it and filled in by the platform once it is loaded; its
files through `Workspace`, whose functions the platform hands it
(`WorkspaceBinding`); its secrets through `Secret.ReadAsync`. So its code runs
the same whatever keeps its data, and the plain calls of code written before
(`GetConnection()`, `ReadText`, `Secret.Read`) wait only where the integration
does not answer at once. The SQL of its migrations is the one thing the
database decides, which is why agents are taught plain SQL every database
accepts. What a database integration adds for code written before
(`DatabaseBinding`: SQLite's `SqliteConnection` and its library) is handed to
the compiler only on that database, and the client library of every other
database is kept from it.

**Keeping a database in shape is the database's to know; when, the
platform's.** A database integration optimizes a store or a lambda's
database as that database is kept in shape (`IDatabaseProvider.OptimizeAsync`)
and says what it did, in its own terms (`DatabaseOptimization`: the room
before and after, the statements run, what it left out and why). Whether a
database is optimized at all, which and when is decided by the platform (see
[Optimizing the databases](#optimizing-the-databases)), which asks only from
its background job. SQLite runs `PRAGMA optimize`, rebuilds a file with
`VACUUM` where that gives enough back at a bearable cost, and truncates the
journal; PostgreSQL runs `VACUUM (ANALYZE, SKIP_LOCKED)` on every table, as
the role that owns it. Neither makes a database that is not there.

**What agents are told.** An integration may add lines to the end of the
guide's section of its kind (`IIntegration.Guide`): SQLite says what it
refuses and how a full database answers, PostgreSQL that names fold to lower
case, what a draft's copy holds and what the room means there.

**What the operator is told.** Each says what it is and where it keeps what it
keeps (`IIntegration.Describe`) - never a password, a key or a connection
string - and is named by the assembly it is in, which the panel's
**Integrations** section shows (`GET
/admin/integrations`), and the usage report names (`database`, `files`,
`secrets`).

**The platform's own schema** is kept once per dialect a database
integration may speak (`IDatabaseProvider.Dialect`): `Data/Migrations` and
`Services/Analytics/Migrations` for SQLite, `Data/PostgreSql` and
`Services/Analytics/PostgreSql` for PostgreSQL, each numbered on its own. A
change to the schema is a migration in every set. The analytics' commands are
written once, in SQL both accept.

**What it takes to add one.** Implement the contract in a project of its own
that references `GenHTTP.Lambda.Integrations`, offer a builder
(`IIntegrationBuilder<T>`), and have a host hand it to the platform's builder.
Await what the infrastructure is asked; where it answers at once, return
completed tasks rather than hop - an installation on Ioxide keeps its
reactors that way. A database integration keeps its databases in shape when
it is asked (`OptimizeAsync`), and may answer that it did nothing where its
database looks after itself. A database speaking a new dialect also needs the
platform's schema written in it, and a value of `DatabaseDialect` (see above). A files integration that keeps nothing on the local disk -
object storage, say - is what the contract is shaped for: a lambda reaches its
files only through the platform, and nothing hands it a path.

## Configuration

Everything is read from the environment on startup, by
[`LambdaSettings.FromEnvironment()`](../src/GenHTTP.Lambda.Server/LambdaSettings.cs) -
the only place below the hosts that reads it - apart from the limits of the
tiers, which are the product's promises rather than the server's
configuration and are set in the administration panel while it runs (see
[Limits](#limits)); a host gives their defaults (`LambdaOptions.Sandbox`,
`Production`, `System`, `DefaultTier`), as genhttp.dev gives the sandbox and
production the labels *Free* and *Premium*. The variables are read into two: the platform's options
([`LambdaOptions`](../src/GenHTTP.Lambda.Core/Configuration/LambdaOptions.cs)),
which every component is built with, and the host's (`ServerSettings`), which
only `LambdaServer` reads - `LAMBDA_ENGINE`, `LAMBDA_HTTP_PROTOCOLS`,
`LAMBDA_RECEIVE_QUEUE_ENTRIES`, `LAMBDA_TLS_PORT`, the `LAMBDA_CERTIFICATE`
variables and `LAMBDA_ACME_DIRECTORY`. `LAMBDA_PORT` goes into both, since the
platform makes addresses on localhost with it. A host adds what is its own:
genhttp.dev's terms and legal pages, which no variable sets, and its contact
and abuse mailboxes in place of `LAMBDA_CONTACT` and `LAMBDA_ABUSE_CONTACT`
(`CloudInstallation`); the community edition's, that its operator may give it
a name and a logo of their own - which they do in the panel, not with a
variable (see [Branding](#branding)).

| Variable                            | Default          | Meaning                                     |
|-------------------------------------|------------------|---------------------------------------------|
| `LAMBDA_PORT`                       | `8080`           | port of the root server                     |
| `LAMBDA_ENGINE`                     | `ioxide`         | `ioxide`, `kestrel` or `internal` (see below) |
| `LAMBDA_HTTP_PROTOCOLS`             | `Http1`          | the HTTP versions both ports answer on; `Http1AndHttp2` tries HTTP/2 again |
| `LAMBDA_RECEIVE_QUEUE_ENTRIES`      | `4096`           | reads of 32 KB a connection may have waiting on io_uring |
| `LAMBDA_DATA_DIRECTORY`             | `./data`         | database, code and workspaces               |
| `LAMBDA_DEVELOPMENT`                | `false`          | verbose error pages and debug logging       |
| `LAMBDA_MAINTENANCE_INTERVAL_HOURS` | `0.25`           | how often expired lambdas are looked for    |
| `LAMBDA_SOURCE_CACHE_BYTES`         | `2147483648`     | what the published sources may take on disk, packed; the least recently read go first |
| `LAMBDA_GIT_MAX_PUSH_BYTES`         | `268435456`      | how large a `git push` may be; it is unpacked in memory |
| `LAMBDA_MAX_CONCURRENCY`            | `64`             | lambda requests executed at once            |
| `LAMBDA_EXECUTION_TIMEOUT_SECONDS`  | `15`             | before an invocation is aborted             |
| `LAMBDA_TELEMETRY_INTERVAL_SECONDS` | `30`             | how often a reading is taken                |
| `LAMBDA_TELEMETRY_SAMPLES`          | `2880`           | how many readings are kept                  |
| `LAMBDA_USAGE_REPORTS`              | `true` (community edition) | send the daily usage report; `false`, `0`, `no` or `off` sends none, see [Usage reports](#usage-reports) |
| `DO_NOT_TRACK`                      | -                | set (`1`), no usage report is sent either   |
| `LAMBDA_ANALYTICS_INTERVAL_SECONDS` | `300`            | how often the visits counted for the owners' analytics are written down |
| `LAMBDA_ANALYTICS_CRAWLER_LISTS`    | `true`           | fetch the list of crawlers and the addresses Google, Bing, OpenAI, Apple and Perplexity crawl from |
| `LAMBDA_ANALYTICS_CRAWLER_REFRESH_HOURS` | `24`        | how often they are fetched again            |
| `LAMBDA_ADMIN_TOKEN`                | -                | enables the panel, and closes the figures   |
| `LAMBDA_LOG_HISTORY`                | `4000`           | log lines the panel can read back, to 10^6  |
| `LAMBDA_LOG_MEMORY_MB`              | derived          | what their text may cost; whichever runs out first |
| `LAMBDA_LOG_LAMBDA_OUTPUT`          | `true`           | keep what lambdas print, filed under them   |
| `LAMBDA_LOG_REPEAT_WINDOW_SECONDS`  | `10`             | fold identical lines; zero writes every one |
| `LAMBDA_LOG_CLIENT_ADDRESS`         | `true`           | record the address a request came from      |
| `LAMBDA_UNBLOCKABLE_NETWORKS`       | own networks     | never recorded against a lambda nor blocked: loopback, the private ranges, link local and shared space by default; a list of networks, or `none` (see [Blocking](#blocking)) |
| `LAMBDA_LOG_GEO`                    | `true`           | say which country a range is registered in  |
| `LAMBDA_LOG_GEO_REFRESH_HOURS`      | `168`            | how often the registry files are refetched  |
| `LAMBDA_LOG_GEO_PLACES`             | `true`           | also fetch the town and network databases   |
| `LAMBDA_LOG_MAX_LINES_PER_REQUEST`  | `200`            | before one request's output is cut off      |
| `LAMBDA_MCP_ORIGINS`                | -                | hosts (or origins) a browser may use `/mcp` from |
| `LAMBDA_SECRETS_KEY`                | `secrets.key`    | the installation's half of the key secrets are sealed with, see [Secrets](#secrets) |
| `LAMBDA_DATABASE`                   | `sqlite`         | what keeps the databases: `sqlite`, or `postgresql` on the community edition (see [PostgreSQL](postgresql.md)); another the host does not offer is refused as it starts |
| `LAMBDA_POSTGRES_CONNECTION`        | -                | with `postgresql`: how to reach the database, as Npgsql reads it - `Host=db;Database=lambda;Username=lambda;Password=...`; the user creates schemas in it and roles on the server |
| `LAMBDA_POSTGRES_ROLE_PREFIX`       | `lambda`         | what the role of every lambda is called before its id; one per installation on a server |
| `LAMBDA_POSTGRES_POOL_SIZE`         | `10`             | connections a lambda or a draft keeps open at most, each closed after a minute unused |
| `LAMBDA_PUBLIC_URL`                 | - (the community image: `http://localhost:8080`) | canonical address, enables the sitemap; set, the platform answers only at its own names; below a path, the lambdas' addresses are made from it, see [Where the lambdas answer](#where-the-lambdas-answer) |
| `LAMBDA_HOSTING_URL`                | `http://localhost:{LAMBDA_PORT}` | the domain the lambdas answer below, a subdomain each: `https://genhttp.run`; set, the community edition answers at subdomains as well |
| `LAMBDA_ADDRESSING`                 | the host's       | `subdomains` or `paths`, over what the host and `LAMBDA_HOSTING_URL` say: genhttp.dev answers at subdomains, the community edition by path unless `LAMBDA_HOSTING_URL` is set |
| `LAMBDA_CONTACT`                    | -                | where to write to whoever runs the installation, which the pages offer - about moving an app to production, for one; without, they offer nobody |
| `LAMBDA_ABUSE_CONTACT`              | -                | where a lambda that does harm is reported; without, the pages offer no way to report one |
| `LAMBDA_AGENT_URL`                  | -                | where the build agent listens; without, nothing is built from a request and its text boxes are not offered |
| `LAMBDA_AGENT_TOKEN`                | -                | the secret the server sends the agent, so only it can ask |
| `LAMBDA_AGENT_PASSWORD`             | -                | the password for the models of the build agent the panel puts behind one (Fable, at first); without, those are not offered. `LAMBDA_AGENT_FABLE_PASSWORD`, its name before, is still read (see [The build agent](#the-build-agent)) |
| `LAMBDA_TLS_PORT`                   | `0`              | port for TLS, zero leaves it off            |
| `LAMBDA_CERTIFICATE`                | -                | PEM chain or PKCS#12 archive                |
| `LAMBDA_CERTIFICATE_KEY`            | -                | private key, for a PEM pair                 |
| `LAMBDA_CERTIFICATE_PASSWORD`       | -                | password of the PKCS#12 archive             |
| `LAMBDA_CERTIFICATE_DIRECTORY`      | -                | further certificates, one folder per name   |
| `LAMBDA_ACME_DIRECTORY`             | -                | web root an ACME client writes challenges to |

The io_uring engine is the default and the faster one, but container runtimes
block the syscall in their default seccomp profile - so the community image
defaults to `LAMBDA_ENGINE=internal`, GenHTTP's own engine on .NET sockets,
and the cloud's image to `kestrel`. Set it back to `ioxide` where io_uring is
available and allowed; genhttp.dev does, in `deploy/cloud/docker-compose.yml`.
The internal engine speaks HTTP/1.1 only, whatever `LAMBDA_HTTP_PROTOCOLS`
says beyond it.

It runs the server on io_uring under `deploy/cloud/seccomp-ioxide.json`, which is
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
consumed, and the other connections on that reactor wait for them. Kestrel and
the internal engine slow the sender down and need none of this.

A workspace file sent as it is, to `…/files/:path/content`, is written to the
disk as it arrives: 200 MB went through with the server growing by 160 MB,
where base64 in JSON costs several times the file. A connection dropped in the
middle of a body looks like a body that ended, so such an upload is checked
against its `Content-Length` and not kept unless all of it arrived - with 1024
reads, the partial file had been kept as if it were the whole.

### Redeploying

genhttp.dev, the server and the build agent, is one compose file in
`deploy/cloud`, its secret settings in `.env` beside it. A script beside both
builds, restarts and checks the result:

```bash
sudo deploy/cloud/deploy.sh              # rebuild and restart what is on disk
sudo deploy/cloud/deploy.sh --pull       # fetch origin/main first, then do that
sudo deploy/cloud/deploy.sh --check      # verify the running server, change nothing
```

`--check` is safe at any time and is the quickest way to answer "is the build
agent actually up?". A deploy with nothing to change recreates nothing, so it
costs no downtime; when there is something to change it restarts the container,
which drops every open connection - including anyone using a hosted lambda at
that moment.

The build agent is rebuilt with the server, because its runner reads the
answers of the server's tools and a runner left behind reads them as they used
to be. Both images are built before anything restarts. The runner's container
is recreated only when its image changed, and only once the builds running at
that moment have finished (ten minutes at most): a running build lives in the
runner, and recreating it would lose the build. New builds start from the new
image straight away. `--check` fails while the runner is older than the image
built last.

Linking it onto the path makes it available to anyone with sudo:

```bash
sudo ln -sf "$PWD/deploy/cloud/deploy.sh" /usr/local/bin/genhttp-deploy
```

The builds sign in with a token of the subscription, made with
`claude setup-token`; `sudo deploy/cloud/set-token.sh` writes it into `.env`
and restarts the agent on it. The runner takes an API key instead
(`ANTHROPIC_API_KEY`, see [The build agent](#the-build-agent)); genhttp.dev
passes none.

The agent's image pins the version of Claude Code its builds run
(`CLAUDE_VERSION` in `docker/agent/Dockerfile`). A build loads no built-in
tool of the CLI (`--tools ""`) and names them in a denylist as well, which is
only known to hold for the version it was checked against: after changing the
version, `docker/agent/verify-tools.sh` starts the CLI in the agent's
container with the flags and lists of a build, prints the tools it loaded -
read from the first line it writes, before it asks the model anything - and
fails on any that is not one of the platform's MCP tools.

## Telemetry

The panel's **Server** section shows what the process is doing, and
`/api/v1/telemetry` is where it comes from. A reading is taken every `LAMBDA_TELEMETRY_INTERVAL_SECONDS` and
`LAMBDA_TELEMETRY_SAMPLES` of them are kept, which is a day at the defaults.

The readings live in memory and go with the process. That suits what they are
for - a restart ends the run they were measuring - but it does mean a redeploy
starts the graph over.

It also lists what each lambda has served, busiest first - since the server
came up, and over its last hour and day. Only the public key identifies one
there - the editor key, the code and the visitors are not part of it. What
happened on the platform per day - lambdas created, versions saved and put
online, lambdas deleted - comes with it (`days`), and is drawn under
**Statistics** rather than **Server**, being about the lambdas rather than
the machine.

Both are for the operator only, behind the administration token (see
[Administration](#administration)); the owner of a lambda sees its own figures
in the editor, and nobody sees everybody's. What it is for is
the shape of the memory curve. A managed heap that climbs across gen 2
collections is a leak; committed bytes climbing while the managed heap stays
flat is the heap keeping pages it could return, which is not.

## Usage reports

The community edition sends a usage report once a day - the first shortly
after the server starts - to `https://telemetry.genhttp.run/api/reports`, a
lambda on genhttp.dev whose front page says what is kept of it. It is what
tells the project how many installations there are and how they are run. It
is on by default; `LAMBDA_USAGE_REPORTS=false` (or `DO_NOT_TRACK=1`) switches
it off, and then nothing is gathered at all. genhttp.dev sends none, and
neither does a host of somebody's own unless it asks to
(`LambdaSettings.FromEnvironment(…, usageReports: true)` sets
`LambdaOptions.UsageReportUrl`).

A report is one JSON document (`UsageReport`, `schema` 1), and
[telemetry.md](telemetry.md) says what is in it - the list whoever runs an
installation reads. Counts and switches only, and the demos are never counted.
A report names no lambda, owner or visitor, nor the installation itself: no
key, domain, title, code, specification, record, file, secret, address or log
line, and not `LAMBDA_PUBLIC_URL` - the tests check that a report holds none
of the keys of the lambdas it counts, nor the installation's address. Whether
its operator branded it is a switch (`configuration.branded`), never the name
or the logo, and so is whether the platform optimizes the databases
(`configuration.optimizeDatabases`); whom it is for is its profile
(`configuration.profile`: `home` or `enterprise`), never whose it is; what keeps its data is named (`configuration.database`, `files`,
`secrets`: `sqlite` or `postgresql`, `local`, `database`), never where it is. The
installation's identifier is random, made the first time and kept in the
platform's database (`settings`, key `installation`).

The job (`UsageReportJob`) runs on the background scheduler, which starts its
jobs once the server answers, each on its own, so a report never holds up the
start: a firewall that drops the connection costs one attempt of at most 30
seconds a day. Nothing is retried; the next day's report says the same with
newer figures. A report that arrived is logged at information level, one that
did not as a warning once and then only at debug level until one arrives again.
Startup says it once: `Usage reports are on: …`.

The panel's **Usage report** section (`GET /api/v1/admin/usage-report`) shows
whether reports are on, where they go, how the last one fared, and the last
report exactly as it was sent.

## Quiet hours

When the installation, and each lambda, is usually quiet: an indication that
work which can wait is planned by - today the optimization of the databases
(see below), and whatever else can wait later. It decides nothing itself
(`IQuietHoursService`); whoever plans work asks it.

Every request the server answers is counted by the hour of the day, in UTC,
it was answered in, and every request a lambda answered - not its drafts'
previews - for that lambda as well, from the visit the request was. That is
a concern at the root of the host beside the telemetry (`QuietHoursConcern`),
and all it costs a request is two increments of a fixed array, without a
lock (`HourlyCounts`); nothing is written while a request is served.

Once an hour a job (`QuietHoursJob`) folds what was counted into a typical
day for the installation and for every lambda that was visited, in one write
to the platform's records (`quiet_hours`, V28, V3 on PostgreSQL): 24 numbers,
the requests of each hour of the day, as an average in which a week ago
counts half as much as today. Every number shrinks with time the same way, so
a lambda nobody visits keeps the shape of its day and costs no write, and a
burst that happened once fades within a few weeks. What is read is put right
for what the numbers alone get wrong - each hour is read as it was when it
was last added to, and against how often it was counted (`QuietProfile`) - so
the figures are requests on a typical day from the first day on.

The quiet hours are the quietest three hours in a row, which may go past
midnight. They are known once every hour of the day was counted at least
once - the installation's row keeps which were (`covered`), and a lambda's
hours counted are the installation's - over a day at the least, with a
request an hour on average; until then `known` is false and nothing should be
planned by them. It is an indication, not a measurement: what was counted
in the hour before a restart is lost, an hour the server was down counts as
one nobody asked for anything in, and a weekend is not told from a weekday.

The installation's are at `GET /api/v1/admin/quiet-hours` and in the panel's
**Quiet hours** section (Platform), drawn on the operator's clock, in the time
zone of their browser. A lambda's typical day is `typicalDay` of `GET
/api/v1/admin/lambdas/{publicKey}`, and its page shows it the other way
round, as **When it is used**: the hours it is busy - every hour with at
least half the requests of its busiest - and its busiest hour, since that is
what matters about an app; its quiet hours are maintenance's business.

## Optimizing the databases

The platform keeps the databases in shape itself: its own records and
analytics, and the database of every lambda that keeps one - not the copies
the drafts work on, which go when a draft is merged. A job looks every
quarter of an hour (`OptimizationJob`, `IOptimizationService`) for what is
due; how a database is kept in shape is its integration's
(`IDatabaseProvider.OptimizeAsync`, see [Integrations](#integrations)).

- **When.** Each database at most once a day (20 hours apart, so a quiet
  stretch that comes a little earlier is not missed), in its quiet hours: a
  lambda's own where they are known, else the installation's, which are also
  when the platform's own are optimized. Where the installation's are not
  known yet - its first day - from 03:00 to 06:00 on the server's clock. Not
  in the first ten minutes after the server started, which go to serving and
  warming up the lambdas.
- **Which.** A lambda's database only where it is switched on and the lambda
  was visited since it was last optimized (`last_seen`, written by the
  maintenance): nothing else writes to it, so one nobody uses stays as it was
  left. The longest unoptimized first, one database at a time; a run that has
  optimized for five minutes leaves the rest to the next, a quarter of an
  hour later and still in the quiet hours.
- **On SQLite.** `PRAGMA optimize = 0x10002`, with `PRAGMA analysis_limit =
  1000`, which takes the statistics the planner reads where they are missing
  or stale. `VACUUM` where a quarter of the file and a megabyte at the least
  are free pages - the room deleted records left, which counts against the
  lambda's data until the file is rebuilt - unless the file is larger than
  256 MB (a rebuild holds the write lock, and a lambda's connections wait ten
  seconds for it), the volume has no room for SQLite's copy in its temporary
  directory (`SQLITE_TMPDIR`, `TMPDIR`, then `/var/tmp`) or for twice the
  file beside it (the rewritten pages go through the journal), or a table has
  neither an `INTEGER PRIMARY KEY` nor an index, whose rows a rebuild would
  number anew. Then `PRAGMA wal_checkpoint(TRUNCATE)`, which gives the room
  the journal grew to back. Over a connection of its own that never makes a
  file; a step a writer keeps from running is skipped rather than failed.
- **On PostgreSQL.** `VACUUM (ANALYZE, SKIP_LOCKED)` of every table, one
  statement each: the platform's as the user the installation connects as, a
  lambda's as its role, which owns its tables, over a connection that is not
  pooled. A table somebody holds a lock on is skipped, with PostgreSQL's
  warning passed on. Not `VACUUM FULL`, which locks out every reader.
- **What is kept.** A row per database (`database_optimizations`, V28, V3 on
  PostgreSQL; `lambda_id` 0 for the platform's own): when, how long, the room
  before and after, what was done and what was left out and why, and why it
  failed - written once per run, since every write to the platform's records
  makes what is held in memory about the lambdas stale. A database that failed
  is tried again the next day. One line per run is logged at information
  level (`Optimized databases count …`), and a warning per failure.
- **Switched off** under **Settings** in the panel (`optimizeDatabases`,
  stored as `database-optimization`) - for an operator whose database is
  looked after by its own tools, such as a PostgreSQL server with
  maintenance of its own. On by default.

`GET /api/v1/admin/optimization` says whether it is on, what the last
optimization of the platform's own did, and how many lambdas' databases were
optimized in the last day, what they gave back and how many failed; a
lambda's last is `optimization` in `GET /api/v1/admin/lambdas/{publicKey}`.

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
`LAMBDA_CERTIFICATE_PASSWORD` instead. On Kestrel and the internal engine the
certificate is read again when the files change, so a renewal is picked up
without a restart; the
io_uring engine takes the files when it opens its port, and is restarted to
serve a renewal.

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
written while the lambda runs, and kept by the files integration, so
`Workspace.Files()` stays a tree the integration hands over. `Assets`
is the same as `Resources`, under the name it had before, so the code of a
lambda saved then compiles as it did.

The directory is rewritten from the version being deployed, so a resource
dropped from a version stops being served rather than lingering. What a
version may come to, its code and its resources together, is a
[limit](#limits) of the lambda's tier. How many files there are is not
limited, and neither is the number of C# files.

Budget the disk for production. A version is kept whole, its files as
text or base64 in JSON, so each version saved at the limit takes up to half as
much again on disk - 184 MB for 128 MB of pictures - and fifty of them are
kept by default: at the defaults, a lambda in production that is saved over and over
at its limit can come to around 9 GB of history.

Budget the memory too. A version is read and written whole: with the 128 MB
production allows, saving one took the server to 2.3 GB and deploying it
to 2.8 GB. Even the 32 MB of a sandbox lambda took a server idling at 350 MB to
950 MB to save, and anybody can create one. Set what a version may
come to, for every tier, in the panel's **Limits** to what the machine can
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
answered with the default one - or with the wildcard of the hosting domain,
where one of them is that (see below).

Note that this needs a PEM pair per name when running on the io_uring engine.
Kestrel and the internal engine terminate TLS in .NET and are handed a loaded
certificate, but the io_uring engine reads the files itself and has no way to
be given an archive.

The issuers in the file are published into the certificate store of the user
the server runs as, because a client needs the chain and not just the leaf to
reach a root. Kestrel and the internal engine paper over a missing chain by
fetching the issuer over the network mid-handshake; the io_uring engine sends
what it was given, so a
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
mkdir -p /opt/genhttp-lambda/acme     # mounted read only at /acme, see deploy/cloud/docker-compose.yml

certbot certonly --webroot -w /opt/genhttp-lambda/acme -d your.host.name
```

The challenge is asked for over plain HTTP, and it is the one request the
upgrade to HTTPS lets through rather than redirecting.

### The hosting domain

Where the lambdas answer at subdomains, every lambda answers at a subdomain
of the hosting domain (see
[Where the lambdas answer](#where-the-lambdas-answer)), named after a key
that may be claimed at any moment, so its certificate is a wildcard - one
certificate for `*.genhttp.run`, and for `genhttp.run` as well, which a
wildcard does not cover and where the hosting domain has its page:

```
LAMBDA_HOSTING_URL=https://genhttp.run
```

```
/certs/fullchain.pem               the site's, genhttp.dev - the default
/certs/genhttp.run/fullchain.pem   *.genhttp.run and genhttp.run
/certs/genhttp.run/privkey.pem
```

The io_uring engine matches the name a client asks for exactly, against the
names it was told when it opened the port, and a wildcard is no such name:
`quiz.genhttp.run` is never in that list. So the certificate whose names
include `*.` and the hosting domain is presented to every name no
certificate is listed for, in place of the default one - which keeps being
presented, by name, to the names it carries. Kestrel and the internal engine
ask per connection and find the wildcard by the name anyway; they are given
the same.

A wildcard is issued only against a challenge in the DNS - the HTTP challenge
the server answers proves one host, not all of them - so it takes the DNS
plugin of certbot for whoever runs the domain's DNS, or a manual challenge:

```bash
certbot certonly --dns-<provider> -d genhttp.run -d '*.genhttp.run'

mkdir -p /opt/genhttp-lambda/certs/genhttp.run
install -m 0644 -o root -g 1001 /etc/letsencrypt/live/genhttp.run/fullchain.pem /opt/genhttp-lambda/certs/genhttp.run/
install -m 0640 -o root -g 1001 /etc/letsencrypt/live/genhttp.run/privkey.pem   /opt/genhttp-lambda/certs/genhttp.run/
```

Like every certificate in the folder, it is found on startup. Kestrel and the
internal engine read a renewal as it is written; the io_uring engine takes the
files when it opens
its port, so the hook that installs a renewal restarts the container. A
server whose certificates include no wildcard of the hosting domain presents
the default one to every name, as it always did - and says so on startup, as
it says when no certificate names the hosting domain itself.

### Custom domains

A lambda in a tier with domains of its own - production or the system tier - can answer at a domain of its own instead of its
subdomain of the hosting domain or its path, which its owner sets in the editor after
pointing the domain's A and AAAA records at the server. Plain requests to it are redirected to HTTPS on the same domain like
every other request, so it needs a certificate - issued by hand for now, with
the web root above, into a folder of its own:

```bash
certbot certonly --webroot -w /opt/genhttp-lambda/acme -d shop.example.com -d www.shop.example.com

mkdir -p /opt/genhttp-lambda/certs/shop.example.com
install -m 0644 -o root -g 1001 /etc/letsencrypt/live/shop.example.com/fullchain.pem /opt/genhttp-lambda/certs/shop.example.com/
install -m 0640 -o root -g 1001 /etc/letsencrypt/live/shop.example.com/privkey.pem   /opt/genhttp-lambda/certs/shop.example.com/
```

The folder is only looked through on startup - the io_uring engine learns the
names it holds certificates for when it opens the TLS port - so a new
certificate is served after the next restart of the container, and so is a
renewal on that engine. Until the certificate is there, visitors of the
domain are redirected to HTTPS and shown the certificate every unknown name
is shown, which does not carry their name.

The `www.` name goes on the same certificate: a visitor who types it is sent
on to the domain, and has to trust the server before they can be told so.
Leave it out where its DNS does not point here, or certbot fails for both
names. The challenge for `www.` is sent on to the domain like any other
request once the domain is set, which the certificate authority follows.

Mind the order: once the domain is set, the lambda's subdomain sends its
visitors to the domain at once. The challenge is answered for any host as
soon as its DNS points here, whether the domain is set or not, so the
certificate can be issued and the container restarted before the owner
enters it. Until they do, the domain is a host nobody has, and visitors are
told nothing is online there.

## For agents

There is a Model Context Protocol endpoint at `/mcp`, so an agent can build
something here and put it online without a person driving the editor. It is
GenHTTP's MCP module (`GenHTTP.Modules.Mcp`): JSON-RPC over a single path,
taking POST, in the stateless revision of the protocol and in the ones before
it that start with a handshake, which it answers without keeping a session. A
GET is declined with 405, because this server answers rather than streams.

The tools are the methods of `McpTools` marked `[Tool]`: their `[Summary]` is
what an agent reads, their parameters are the arguments and their schema, and
a fixed limit - a length, a range, the values an argument takes - is an
attribute on the parameter, advertised and checked before the tool runs. A
tool answers with an object, as text for the model and as structured content
for a program. When the platform refuses - a key nobody has, a demo that
cannot be changed, a name that is taken - the answer is an error that says
why in a sentence, as is one for an argument that does not fit; a failed
deployment or merge is an error that carries its diagnostics as structured
content.

```
https://genhttp.dev/mcp
```

The tools are the shape of the job: `create_lambda`, `update_lambda`, `write_code`, `change_code`,
`check_code`, `deploy`, `read_lambda`, `read_logs`, the feature tools
`create_feature`, `update_feature`, `merge_feature` and `delete_feature`, the
data tools `upload_file`, `list_files`, `delete_file`, `enable_data`,
`read_database`, `set_secret`, `list_secrets` and `delete_secret`, `open_source`
for publishing its code - only when its owner asks for it - `analytics` for
reading who visits a lambda, and switching that on when the user asks, and
`list_demos` for reading something that already works. No tool lists a lambda
on the showcase: its picture, base64 in a tool call, was too large for the
agents that tried, so the instructions point to `platform_guide` (`showcase`),
which sends them to `PUT /lambdas/{privateKey}/showcase` with the body written
to a file, and the user to the editor where they cannot make requests. Where the installation
may be branded and has an admin token, `customize_installation` names it and
gives it a logo, for an agent its operator handed the token (see
[Branding](#branding)). `platform_guide` is the one to call first. It answers
with one Markdown document, sent once as text rather than as an object (which
the MCP module sends twice), written for a model and held under 28 KB by a
test - at 65 KB, clients wrote it to a file instead of the conversation. It
opens with what a lambda is and how versions, features and data live, then
git and the REST API, code and resources, paths, compiling, requests and the
data, and ends with documentation, pages meant to be found, the link back,
the limits and the terms. Its sections are headings named the way the tools,
the instructions and the briefs point at them ("platform_guide,
connectivity"), and what a tool's description or a demo already says is not
repeated in it. Where the operator wrote rules for agents, they come last,
after the platform's own sections (see [Rules for agents](#rules-for-agents));
the budget is the platform's, without them.

Agents used to leave a version behind for every attempt, and put every attempt
online. A lambda that exists is now changed in a feature: `write_code`,
`change_code`, `deploy`, `read_lambda`, `read_logs` and the data tools take an
optional `feature` and then act on the feature - its files, its preview, its
copy of the data - instead of the lambda, so the same few tools do both. The
answers name the feature, its `previewUrl` and whether it is `mergeable`, and
never carry `online: true`, which only a deployment of the lambda does - with
its `onlineUntil`, unless its tier keeps it online and it has none.
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
of Entity Framework Core on a connection per request, awaited - and `read_database`
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
its C# compiled in any folder and the rest kept, and its resources in
`resources/` - is said in a line of the instructions, under `code` and
`resources` in the guide, and in the tool descriptions.

A page meant to be found or shared - a website, a shop, a landing page - is
given a title, a description, its language, an icon and a social preview,
and says what it is in the HTML it is served with rather than only in what a
script renders, since crawlers and agents mostly run none. The instructions say
so in a line, `platform_guide` says how under `beingFound`, and the build
agent's brief says the same; a tool for a few people needs a title and nothing
more. `og:image` takes a full address - social networks do not resolve a
relative one - so the warning a feature's deploy gives for a link to the
lambda's own address (or to its old path below `/lambda/`, which leads
there) leaves meta tags and the canonical link out: they name the page, and
the page never follows them. A lambda with a domain of its own names the
domain as canonical and in `og:url`, so what is listed and shared names the
domain.

A page that shows what changes while it is open - what others do, a count, a
feed, a game - is pushed the change by the server, with server-sent events when
it only listens (`demo-live`) or a websocket when it talks back (`demo-game`).
Agents are told never to poll, fetching on a timer to see whether anything
changed: a poll is a request whether it did or not, on a server every lambda
shares. The instructions say so in a line, `platform_guide` under
`liveUpdates`, and the build agent's brief the same.

Agents are asked - not required - to put a small "Made with" line naming the
installation at the foot of the pages they build - "Made with GenHTTP Lambda",
or the name its operator gave it (see [Branding](#branding)) - linking to
`LAMBDA_PUBLIC_URL` (or the address the agent called, where it is not set)
with `rel="nofollow"`. The link's words are the name and nothing else. Every lambda is a site of
its own below the hosting domain, so the link comes from another site, and
the same link in the footers of many sites is what search engines count as
link spam unless it says it is no recommendation - which `nofollow` does; a
line written while the lambdas answered below the site, without it, is given
it with the next change. On a lambda with a domain of its own the line is
plain text. It is the user's to refuse: the agent says that it added
it, leaves it out or takes it out when asked, and notes in `decisions.md` that
it was not wanted, so the next agent does not put it back. A change does not
add one to a lambda that has none. The instructions say so in a line,
`platform_guide` under `backlink`, and the build agent's brief the same.

`analytics` answers what the owner sees in the editor's Analytics section -
the totals, the days and each breakdown - over `days`, at most what the tier
shows; while it is off it says so and that the owner switches it on.
`enable: true` switches it on, only when the user asks; there is no way for an
agent to switch it off, since that deletes what was counted. `platform_guide`
says under `analytics` that the platform counts this in front of the lambda,
so an agent does not build a counter of its own.

`open_source` publishes a lambda's code, changes its license or takes it down;
with only the editor key it says how things are and changes nothing. It is not
part of building, and its description says so: only when the user asks, and
asking which license when they did not say. Once a lambda is published,
`read_lambda` says so in `openSource`, with a note that everything saved from
then on is public, the versions before included - so an agent keeps keys,
passwords and personal data out of the files, where they never belong anyway.

What a build tool makes the resources or code from is kept in a folder of the
code (see [What it is built from](#what-it-is-built-from)), and the agents are
told so under `code` in the guide (`builtWithATool`) - the flow, a
`.gitignore`, a README that says how it is built, and the pitfalls (what a
build writes refers to its files relatively; what it wrote anew is saved with
it) - in
`AGENTS.md` of a clone, and in the tool descriptions. `write_code`, which
replaces every file, says so when what it replaced had a folder of code the
save has none of - `change_code` keeps every file it is not told about - and
either says so of migrations kept at the top of the code, where a lambda had
them before its resources were a folder of their own: as code they are never
read, and Evolve looks in `resources/migrations/`.

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
says which tier the lambda is in (`tier`, and `tierLabel`, what the
installation calls it), what that allows and what the tier above allows
(`limits`), and `platform_guide` lays out the tiers as this installation set
them, by their identifiers and their labels. The guide, `write_code` and `upload_file` also steer a
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
deleting, the workspace, the showcase, its license - is refused by its tier
(`TierPolicy.ReadOnly`), whichever way the request comes in. Their code is published on `/source` under
MIT by the seeder, so anybody can read and download a demo there as well. `create_lambda` with a demo's id as its template starts a
lambda of one's own from a copy. Keys starting with `demo-` cannot be claimed.

The demos are seeded in the background after startup from the hidden templates
of the same name in `Resources/Templates`, redeployed when their template
changes, and retired when they leave `DemoCatalog`. Nobody can move a lambda
into the tier or out of it, the operator included.

Where the installation has terms - genhttp.dev has, a community installation
none - nothing is created until `acceptTerms` is true, and the editor key that
comes back is the only way into what was made. There is no session and nothing is
remembered between calls: everything a call needs is in its arguments.

`LAMBDA_MCP_ORIGINS` lists the hosts a browser may drive the endpoint from: a
host stands for its page over HTTPS (`genhttp.dev` for
`https://genhttp.dev`), and an entry with a scheme for the origin it names,
port included (`http://localhost:5173`); an entry that names neither is left
out, with a warning on startup. An agent speaking HTTP sends no
`Origin` and is never checked against it; the list is there because a browser
does, and a page on another site could otherwise aim somebody's browser at
this endpoint.

## Administration

`/admin` is the operator's panel, a component of its own (`AdminPanel`):
pages only, in English, which call the operator's routes of the REST API with
the token. Its sections are in groups down a sidebar, the editor's navigation
(`Navigation` in the kit) - a bar that opens them all on a phone:

| Group         | Section               | What it is                                                                 |
|---------------|-----------------------|----------------------------------------------------------------------------|
|               | **Dashboard**         | what waits for the operator - a request to connect, lambdas that failed in the last hour, the volume running out of room, their own address blocked, a usage report that did not arrive, data beyond its allowance - and the installation at a glance: its lambdas, the newest of them, the server and the room left |
| Lambdas       | **Statistics**        | the lambdas counted and drawn: by state, tier and view, which of the platform's features they use, where their versions came from, what people did per day, the busiest, the most failing, the largest and what expires this week - each leading to the lambdas it counts |
|               | **All lambdas**       | every lambda, narrowed by a search and by tier, state (online, offline, blocked, expiring this week), the features it uses, its view and its age, in an order; the filters are in the address, so the dashboards link into it |
| Security      | **Connectivity**      | the lambdas that ask to connect to other servers, and those allowed to (see [Connectivity](#connectivity)) |
|               | **Blocked addresses** | the addresses refused, and lifting them (see [Blocking](#blocking))        |
| Performance   | **Server**            | what the process is holding and what the engine is carrying (see [Telemetry](#telemetry)) |
|               | **Storage**           | the free space on the volume of the data directory, read as it is asked for, and what takes the room - by part and by lambda, measured hourly by a walk over every file (`StorageCensusJob`), or at once with **Measure again** - with the lambdas whose data is beyond their tier's allowance |
|               | **Log**               | what the server and the lambdas print, live, from `/api/v1/logs`                     |
| Platform      | **Branding**, **Guide**, **Build agent**, **Settings**, **Limits**, **Integrations**, **Quiet hours**, **Usage report** | the name and the logo its pages show - where its host lets the operator brand it (see [Branding](#branding)); the guide agents read first over MCP - rendered or as the Markdown they are handed - with a section of the operator's own in it for the rules of this installation (see [Rules for agents](#rules-for-agents)); whether the build agent builds and changes apps, on which models and efforts, which need the password, and what its runner says of itself (see [The build agent](#the-build-agent)); whom the installation is for, where its host lets the operator choose (see [Who it is for](#who-it-is-for)), and what the platform does by itself - optimizing the databases (see [Optimizing the databases](#optimizing-the-databases)), with how its last runs went; what a lambda may have in its tier and for every lambda; what keeps the databases, the files and the secrets, with where - read only, chosen by the host as it starts (see [Integrations](#integrations)); when the installation is usually quiet, by the hour of a typical day (see [Quiet hours](#quiet-hours)); and, last, what it reports about itself once a day (see [Usage reports](#usage-reports)) |

A lambda's page sums up everything its editor would show - what its
documentation says it is and what was asked for, its traffic and problems,
which features it uses (its database, workspace and secrets with what they
hold, analytics, showcase entry, published source, domain, connectivity,
drafts and view), what it keeps against its allowances, when it is used on
a typical day, its versions, deployments, lifetime and the addresses it was
changed from - so the editor
is opened to read or change the code, not to find out how a lambda is set
up. What the operator decides about it is in a **Manage** card beside that:
whether it is online, its tier, its domain, its connectivity, a block, and
removing it.

**What is shown and what is done are kept apart, in every section.**
Information is in cards and may link on; actions are buttons only in a
section's header, a card titled as such (a lambda's Manage) and the last
cell of a row, and each that changes something says what it will do in a
dialog first. A setting is the exception: its switch or field is what it is.
What waits for the operator is a notice that leads to where it is decided.

The routes the panel draws its dashboards from:

| Route                                    | What it answers                                                       |
|------------------------------------------|-----------------------------------------------------------------------|
| `GET /admin/overview`                    | the lambdas counted (the demos apart and in nothing else), what waits for a decision, the volume's size and free space, the newest lambdas and those that failed in the last hour |
| `GET /admin/storage`, `POST /admin/storage/measure` | the volume, and what the data directory holds by part and by lambda as last measured; measures again at once |
| `GET /admin/lambdas`                     | a page of lambdas (`search`, `tier`, `state`, `uses`, `view`, `created`, `sort`, `page`), each with which features it uses; the demos are left out unless `tier=Demo` or `demos=true` asks for them, so every count of the overview leads to a list of as many |
| `GET /admin/lambdas/{publicKey}`         | one lambda in full: the owner's summary, what it uses, its showcase entry, published source and drafts beside its versions and deployments |
| `GET /admin/integrations`                | what keeps the database, the files and the secrets: each one's name, the assembly it is in, and where it keeps what it keeps - never a password, a key or a connection string |
| `GET /admin/quiet-hours`                 | the installation's typical day: the requests of each hour in UTC, the quietest three, and whether enough was counted to know them |
| `GET /admin/optimization`                | whether the platform optimizes the databases, what the last optimization of its own records did, and the lambdas' databases optimized in the last day |
| `GET / PUT /admin/agent`                 | whether the build agent builds and changes apps, the models and efforts offered and the defaults, whether there is a password, and what the runner says of itself (see [The build agent](#the-build-agent)) |

The counts are the platform's database, read as they are asked for
(`ICensusService`, `LambdaTally`); the workspace counts as switched on unless
its owner switched it off, as it comes. The token is entered on the panel's
own page - no other page links there - and kept in the browser until **Lock
again** is used or the server turns it away (for the tab only where the
lambdas answer on the panel's own address, see above). The panel no longer
appears inside the editor: what only the operator decides about one lambda -
its block, its tier, its connectivity - is on its page in the panel.

This is the one part that authenticates. Everywhere else the editor link is
the credential and it only ever reaches one lambda; this reads code that belongs
to other people and can take their work away, so it asks for `LAMBDA_ADMIN_TOKEN`
in an `X-Admin-Token` header (GenHTTP's API key authentication). A request
without the header is answered with 401, one with the wrong token with 403.
The concern says so in the API description itself: every operation behind it
requires the `adminToken` security scheme, which Scalar asks for once.
Until that variable is set there is no administration at all: the operator's
routes - `/api/v1/admin`, `/api/v1/telemetry` and `/api/v1/logs` - are not
there and answer 404, and the panel says the installation has none.

The server figures and the log follow the same token. Only owners and the
operator see telemetry: the owner of a lambda its own, in the editor, and the
operator everybody's.

### Tiers

A lambda's tier is what it is for, and only the operator moves a lambda
between them - on its page in the panel, **Tier**
(`PUT /admin/lambdas/{publicKey}/tier`). Its owner cannot choose.

| Tier         | For                                   | Kept while unused | Domain of its own | Connects by default | Compiled as the server starts | Blocked | Showcase | Deleted with its editor key |
|--------------|---------------------------------------|-------------------|-------------------|---------------------|-------------------------------|---------|----------|-----------------------------|
| `Sandbox`    | trying something; where a new lambda starts | no - offline after 30 days, removed after 90 | no | no | no - on its first request | yes | yes | yes |
| `Production` | an app people use                     | yes               | yes               | yes                 | yes                           | yes     | yes      | yes |
| `System`     | what the installation itself depends on, such as the collector of usage reports | yes | yes | yes | yes | never | never | no - only by the operator |
| `Demo`       | the installation's demos, read only, never assigned by hand | yes | no | never | no - the seeder compiles what changed | never | never | no |

What a tier means is said in one place (`TierPolicies`, see
[How it is put together](#how-it-is-put-together)); how much a lambda in it may
have, whether it connects by default and what the pages call it are the
operator's to set (see [Limits](#limits)). A system lambda is the
installation's own: editor keys and every door work for it as for any other,
but it cannot be blocked - a block is refused, and a blocked lambda is not
moved there until the block is lifted - it is never listed in the showcase,
its entry kept and back once it leaves the tier, and it is deleted only by the
operator (`DELETE /admin/lambdas/{publicKey}`). An address block refuses
requests to it like to every other. It is also where running a lambda apart
from the others or ahead of them would hang, which nothing does yet: every
lambda shares the process.

- **Production and system lambdas are warm after a restart.** What they have
  online is compiled as the server starts (`TierPolicy.WarmedUp`,
  `LambdaWarmup`), so their first visitor does not wait for it: in the
  background once the server answers, one lambda after another - the one
  visited last first - through the same single compilation every deployment
  takes, so a request to a lambda not warm yet waits for one compilation at
  most and is served as before. `Warmed up 3 lambda(s) in 2140 ms` is logged
  at the end, a lambda that fails as a warning; it is then built on its first
  request, as every other lambda is. A blocked or offline lambda is not
  compiled. An installation whose new lambdas start in production compiles
  and loads all of them on every start, and a lambda's startup - its
  migrations, a call it makes - runs then rather than on its first visit.
- **Moving a lambda changes its policy and nothing else.** It keeps running:
  the room of its data is read as it writes, and a build made without
  connectivity keeps serving where the new tier connects by default - it is
  built again only where connectivity is taken away. Nothing of its data
  moves.
- **Moved to a tier that allows less**, it keeps what it has. Its versions
  can still be put online and rolled back to; a new save, a merge or a push
  that is larger is refused, naming the limit and the tier above it as the
  installation calls them (`The code and the resources of a version must not
  exceed 32 MB together in the tier 'Free'; … In the tier 'Premium' a version
  may come to 128 MB`). Data beyond its new room stays and is read; writes that
  would grow it fail, as they do when the data is full. A domain is kept but
  not served in a tier without domains.
- **Moved into the sandbox**, it is given the whole time from the move: its
  clocks and those of its features' previews never count from before it
  (`tier_changed`), when nothing would have taken it offline.
- **Labels.** An installation may call a tier otherwise on its pages - a
  word shown as written in every language: genhttp.dev calls the sandbox
  *Free* and production *Premium* (`CloudInstallation`). The pages, refusals
  and `platform_guide` say the label; the API and the code name the tiers by
  their identifiers only, and `tierPolicy.label` of a lambda says what it is
  called. Without a label a tier goes by its own name, which the pages translate.
- **What the API says.** `tier` of a lambda is its identifier, `tierPolicy`
  what that tier lets it have here, with the tier above (`above`,
  `aboveLabel`) - what the editor offers and refusals name. `GET /system`
  has the tier a new lambda starts in (`defaultTier`) and every tier the
  operator assigns (`tiers`). `Free` and `Premium`, what the sandbox and
  production were called before, are still read by `PUT …/tier` and the
  panel's `?tier=` filter; nothing answers with them.
- **From before.** Migration V25 renamed what was stored: `Free` became
  `Sandbox` and `Premium` became `Production`, the limits the panel saved went
  with them, the versions kept and features open - the same in every tier -
  became one setting each, the larger where two were saved, and a decline or a
  request standing for a lambda in production was forgotten, since its tier
  had included connectivity whatever was decided. Nothing became a system
  lambda: that is the operator's to decide, lambda by lambda.

### Blocking

What the operator does about a lambda that breaks the terms - phishing,
malware, attacks on others. **Block** is on its page in the panel
(`POST /admin/lambdas/{publicKey}/block`), and **Lift the block** beside it
(`DELETE …/block`).

- **A blocked lambda is offline and frozen.** Blocking takes it offline, and
  the previews of its features, and from then on every change is refused -
  through the editor, the API, MCP and git alike - with `403` and the kind
  `Blocked`, whose message tells an agent not to try again. The operator's own
  actions on it are refused too, but two: lifting the block, and deleting it.
  Its owner can still read and export it; the code is theirs. They are told it
  was blocked and why: the operator's reason, as they wrote it, is in the
  banner of its editor and in `blockReason` of the lambda - not in the
  refusals, which may end up in a build log others read. Its published
  source is not found while the block lasts, and a blocked lambda is never
  removed for going unused - removed by itself, its key would be free again for
  whoever it was blocked for. Lifting the block leaves it offline.
- **Not the installation's own.** A system lambda or a demo cannot be
  blocked; an operator who means to stop a system lambda moves it to another
  tier first, as a decision of its own. A blocked lambda is not moved into
  the system tier.
- **The addresses it was changed from.** Where the server keeps addresses
  (`LAMBDA_LOG_CLIENT_ADDRESS`), every lambda remembers the addresses it was
  created, saved, put online or otherwise changed from with its editor key,
  through any door, for 30 days after each was last seen (`EditorAddresses`,
  written at most hourly). The panel shows them with the lambda, and blocking
  it can block them as well - all of them are offered but the operator's own,
  which is marked. The operator's requests in the panel are not recorded.
- **Blocked addresses** are refused before anything else is done for them -
  the outermost concern of the host (`AddressBlockConcern`), ahead of the
  redirects, the request log and every lambda: a plain `403`, counted against
  its block in memory instead of logged. An IPv4 address is blocked as it is,
  an IPv6 address with its /64, since a device picks a new address within it
  by itself. Each block is let go of two weeks after it was made, by a job that
  runs hourly; the panel's **Blocked addresses** section (`GET /admin/blocks`,
  `DELETE /admin/blocks/{id}`) lists them with why, which lambda, until when and
  how many requests each refused, and lifts them.
- **The panel stays reachable from a blocked address** - its bundle below
  `/admin`, `/api/v1/system` and the API behind the token - so an operator who blocked
  themselves lifts it there; the panel says when their own address is blocked.
  Nothing else is let through, and the token is asked for as always.
- **The installation's own networks are never recorded nor blocked**
  (`LAMBDA_UNBLOCKABLE_NETWORKS`): the build agent reaches the MCP from a
  container on one, with a fresh address each build, and a proxy in front of
  the server would be one. An installation reached through a proxy on its own
  network therefore cannot block addresses - the connection is the proxy's,
  and a forwarded header is written by whoever sends it.

### Connectivity requests

Whether a lambda may connect to other servers is the operator's to decide, in
the panel's **Connectivity** section or on the lambda's page - see [Connectivity](#connectivity).

### Limits

What a lambda may have in its tier, and what holds for every lambda, is set in
the panel's **Limits** section (`GET / PUT /admin/limits`), not by environment
variables: these are the product's promises, and changing one should not take
a restart. One form: the tiers as a table with a column each, what holds for
every lambda in a block below it, and the limits that are counted per caller
in another.

| Limit               | Sandbox  | Production | System   | What it bounds                                   |
|---------------------|----------|------------|----------|--------------------------------------------------|
| Called              | -        | -          | -        | what the pages call the tier, as written in every language - *Free* and *Premium* on genhttp.dev; empty for its own name, at most 24 characters |
| Build               | 32 MB    | 128 MB     | 128 MB   | a version: its code - the C#, and everything kept with it - and its resources, together |
| Data                | 512 MB   | 4 GB       | 4 GB     | what a lambda keeps: its database and its workspace, together |
| Analytics shown     | 7 days   | 365 days   | 365 days | how far back the owner sees what was counted of their visitors; a year is kept of every lambda, so at most 365 |
| Connects by default | no       | yes        | yes      | whether its code may connect to other servers while nothing was decided about the lambda, see [Connectivity](#connectivity) |
| Offline after       | 720 h    | never      | never    | without visits or edits; the sandbox only |
| Removed after       | 2160 h   | never      | never    | without visits or edits; the sandbox only |

A demo is held to the sandbox's sizes, and never connects. For every lambda
alike: new lambdas start in the sandbox (or in production, for an
installation run for a team rather than for strangers; never in a tier of the
installation's own), the newest 50 versions are kept and 10 features may be
open. Three limits are counted per caller rather than per lambda and have no
tier: the size of a showcase picture (3 MB), requests per second and client to
the lambdas (250), and builds and changes per address and day from the build
agent (10).

They are kept in the `settings` table beside the panel's switches, a row each
(`limits.sandbox.build-bytes`, `limits.versions`, `limits.default-tier`, …),
and held in memory. A missing row is the default - the table above, or what
the host gives (`LambdaOptions`). Saving writes every value, refuses a number
that is not greater than zero, production allowing less than the sandbox, a
label longer than 24 characters and a new lambda starting in a tier of the
installation's own, and logs a line per limit that changed
(`Changed limit limits.sandbox.build-bytes from 33554432 to 16777216 by operator`).

A change applies to what is checked next - a save, an upload, a write to a
workspace, a new feature, a connection to a database, the next sweep - and
takes nothing away: a lambda already over a lowered limit keeps what it has
and cannot save a version until it fits; a database over its new size keeps
its records and grows no further. Nothing is compiled again for a changed
size, since the workspace reads its room as it writes; a tier whose
connectivity default is taken away builds its lambdas that relied on it again
on their next request.

There were four of the two once: the characters of C# apart from the bytes of
assets, and the room of the workspace apart from the database's. A value an
operator saved for one of them is carried over as the sum of the pair - the
other counting as its old default - and the old rows are removed with the next
save.

The environment variables that set them before the panel did
(`LAMBDA_MAX_CODE_LENGTH`, `LAMBDA_RATE_LIMIT`, `LAMBDA_AGENT_BUILDS_PER_DAY`
and the others) are no longer read.

### Rules for agents

Whoever runs an installation - either edition - may write rules for the agents
that build there into the guide they read first: `platform_guide`, which the
MCP instructions send every agent to before anything else, and the build agent
calls as its first step. It is a section of the operator's own, a title and a
text in Markdown, written in the panel's **Guide** section - which shows the
guide exactly as an agent reads it - or through the API with the admin token.
It is read from the next call to the guide on; nothing restarts.

| Route                 | What it does                                                          |
|-----------------------|-----------------------------------------------------------------------|
| `GET /admin/guide`    | the guide as an agent calling `platform_guide` at this address reads it now, its size in bytes, the operator's section and what it may be |
| `PUT /admin/guide`    | writes the section (`{ "title": …, "text": … }`), or replaces it; answers with the section alone |
| `DELETE /admin/guide` | takes it out: agents read the platform's guide alone again             |

The section comes last in the guide, after the platform's own sections and
its terms - what the platform requires matters most to an agent - under the
operator's title and a line saying it is theirs: agents follow it wherever the
guide leaves them the choice, and it changes nothing the platform requires or
refuses, since the code guard, connectivity, the limits and blocks hold
whatever it says. What operators write there is mostly how lambdas are built
on the installation: an architecture, a language, a look, data not to keep. The title is one line of at most 60 characters and none of
the platform's own sections, which the tools point agents to by name; the
text is at most 6 KB in UTF-8 - counted in bytes, since a client is handed
bytes - with headings of the third level and below, as `#` and `##` would end
the section early, and no code block left open (a line in a fenced code
block is no heading). So a guide
with a full section stays a tool result an agent reads in full: the
platform's under 28 KB, the operator's 6 KB, under 34 KB together.

Kept as two settings (`guide.title`, `guide.text`) and read only where the
installation has an admin token: a copy of another installation's data hands
no agent rules nobody here could see or take away. The rules reach agents
through `platform_guide` only - not the MCP instructions, which are fixed when
the endpoint starts, nor the `AGENTS.md` of a lambda's repository.

### The build agent

Where the installation runs the build agent (`LAMBDA_AGENT_URL`), the panel's
**Build agent** section (`GET / PUT /admin/agent`) decides what it does here
and on what. Each switch and list is saved as it changes, and applies to the
next job asked for; a job under way runs on as it was asked for.

- **Whether it builds and changes apps**: building new apps on `/build`, and
  owners asking it about theirs - in the workspace and the editor's Change
  section. Off, each page leads to connecting an agent of one's own instead.
  Both were under **Settings** until the models could be chosen, and are kept
  in the same rows (`build-box`, `change-box`), so a choice made then holds.
- **Which models people may choose**, each for everybody, for whoever knows
  the installation's password, or for nobody: Opus 5.5, Sonnet 5.5 and Haiku
  5.5 for everybody and Fable 5.2 behind the password, until the operator
  decides otherwise.
- **Which efforts** - how much the model thinks and how many steps it takes:
  `low`, `medium`, `high`, `xhigh` and `max`, all of them at first - and what
  a job runs on where nobody chose, Opus at `high` at first. The default model
  is everybody's, without the password, and at least one effort is offered,
  the default among them; a change that breaks either is refused with 400 and
  changes nothing.

They are rows of the `settings` table (`agent.model.opus` = `open`,
`password` or `off`, `agent.default-model`, `agent.efforts`,
`agent.default-effort`), read as the platform starts and held in memory; a
missing row is the default above. A change is logged as `Updated agent
settings builds true changes true models opus:open,sonnet:open,haiku:open,fable:password
model opus efforts low,medium,high,xhigh,max effort high by operator`.

The pages offer what was decided. `GET /system` (`build.offer`) and the
Change section's `GET /lambdas/:privateKey/agent` (`offer`) list the models
offered, each with its label and whether it needs the password, the efforts,
and the defaults; `/build`, the workspace and the Change section draw them as
two lists under the box (`components/AgentChoice.tsx` in the kit) and a field
for the password where the model chosen needs it. A list with nothing to
choose from is left out. The browser remembers the model and the effort last
chosen, never the password. A build or a change names them (`model`,
`effort`; left out, the defaults), and one the operator does not offer is
refused with 400, a wrong password with 403 - before the agent is asked and
before the allowance is spent. A started build says what it runs on
(`model`, `effort`), and so does a change (`job.model`, `job.effort`), which
the Change section shows beside the request; both are logged with the
prompt.

**The password** is the installation's: `LAMBDA_AGENT_PASSWORD` - and
`LAMBDA_AGENT_FABLE_PASSWORD`, which is still read, from when it unlocked
Fable alone. The panel says whether there is one, never what it is, and
without one a model behind it is offered to nobody rather than to everybody.
A job on a model behind it runs without the clock and the limit on its steps,
in a lane of its own beside the one everybody shares. The server tells the
runner so (`unbounded`), and only for a job whose password it checked, so a
model taken from behind the password runs on the clock like every other.

**The runner** maps each model to an identifier of Anthropic's -
`AGENT_MODEL` for Opus (named before there was a choice), `AGENT_SONNET_MODEL`,
`AGENT_HAIKU_MODEL` and `AGENT_FABLE_MODEL`, `claude-opus-5-5`,
`claude-sonnet-5-5`, `claude-haiku-5-5` and `claude-fable-5-2` by default,
which the labels name - and hands the CLI the effort (`--effort`). It signs
its builds in with one of two:

| Variable of the runner      | What it is                                                    |
|-----------------------------|---------------------------------------------------------------|
| `CLAUDE_CODE_OAUTH_TOKEN`   | a token of a Claude subscription, made with `claude setup-token` |
| `ANTHROPIC_API_KEY`         | an API key of an organization's account at Anthropic, billed by the token |

Exactly one goes into a build, and the other is set empty there rather than
left to the CLI's order of precedence. Where both are set the token wins, and
the runner says so as it starts: the key is the one spent by the token, and a
key that reached its environment by accident - exported in the shell a
deploy ran from - should not take over every build unseen. `samples/community`
passes the key as `LAMBDA_AGENT_API_KEY` for the same reason, so a shell's own
`ANTHROPIC_API_KEY` is not picked up. genhttp.dev builds on its subscription.

The section shows what the runner says of itself, from its health, asked with
three seconds to answer: where it listens, whether it answers, what its
builds sign in with - the kind, never the credential - the identifier each
model runs as, the version of Claude Code its builds run and its queue.

Offer only what the credential runs. A job on a model it cannot run - one a
subscription does not include, or one a key has no access to - ends as the
CLI ends it, with an error rather than a closing note: the build or the change
fails with the CLI's words in `detail`, rather than being taken for an answer
or a run that ran out of time.

### Who it is for

An installation says whom it is for - its **profile**: `cloud`, a public
platform anybody builds on (genhttp.dev); `home`, somebody, their household
and their friends; or `enterprise`, an organization and its people. It changes
how the installation speaks and nothing else: the examples its pages offer and
a few of their words, and what agents read about the apps built here in
`platform_guide`. No limit, tier, rule of connectivity, block or refusal
depends on it; those stay the operator's in their sections. An installation
run for a team that wants every app kept starts new lambdas in production
under **Limits** as well.

Which profiles there may be is its host's to decide: `LambdaOptions.Profile`
is the one it has until the operator chooses, `LambdaOptions.Profiles` those
they may choose among. The community edition is `home` and offers `home` and
`enterprise` under **Settings** in the panel; genhttp.dev is `cloud` and
offers none. There is no environment variable: like the limits, it is changed
while the server runs, and applies from the next page served and the next
guide an agent reads. It is a row of `settings` (`profile`), read once as the
platform starts and held in memory, and read only where the host offers a
choice - so a copy of a community installation's data brings nothing of it to
the cloud.

| Route                 | What it does                                                                                     |
|-----------------------|--------------------------------------------------------------------------------------------------|
| `GET /admin/settings` | `profile`, and `profiles`: those the operator may choose among - none where the host decided      |
| `PUT /admin/settings` | `{ "profile": "enterprise" }`; what the body leaves out stays as it is, and a profile the host does not offer is refused (400) |
| `GET /system`         | public: `profile`                                                                                |

What it changes:

- **The words of the pages.** A section of a catalog says what a profile
  says its own way under `profiles` - only the sentences that differ - and
  the kit's catalogs hand every page the words of the installation's profile
  (`profile.ts`), so no component asks which it is. Today only `enterprise`
  has words of its own: the menu's way to `/build` and the ideas and the
  placeholder of the box there, which call what is built an app rather than
  a website; the requests to
  copy on the community's `/ship`; and the examples in the editor's Change
  box, in the fields of a new draft and in the showcase's title and
  description. `home` reads as genhttp.dev does.
- **Prerendered for each.** The community site is prerendered once more for
  every profile it names, as GenHTTP Lambda and as a brand, keeping only the
  pages that differ (`profiles` in `prerender.json`); `#site-facts` says which
  profile a page was rendered for, so the browser draws the same words. A
  build that names no profiles - genhttp.dev's - is the same for every
  profile, and a profile that a build naming some has nothing for is sent
  without content, for the browser to draw. The editor, which is not
  prerendered, asks `GET /system`.
- **The guide.** Its introduction says in a sentence whom the apps built here
  are for - for an organization, tools for colleagues: plain and businesslike,
  keeping no more about people than the work needs. The build agent's brief
  sends it there first.
- **The usage report** says which (`configuration.profile`).

### Branding

Whoever runs the community edition may give it a name and a logo of their
own, which its pages show in place of GenHTTP Lambda's - in the panel's
**Branding** section, or through the API and an MCP tool with the admin
token. There is no environment variable for either: like the limits, they
are changed while the server runs, and show from the next page anybody opens.
Whether an installation may be branded is its host's to decide
(`LambdaOptions.Branding`): the community edition's host switches it on,
genhttp.dev's leaves it off - there the routes are not there at all (404) and
the panel has no such section.

| Route                                    | What it does                                                          |
|------------------------------------------|-----------------------------------------------------------------------|
| `GET /admin/branding`                    | the name and the logo the operator gave the installation, where the pages load the picture beside the name from, and what a name and a logo may be |
| `PUT /admin/branding`                    | names it (`{ "name": … }`): one line of at most 40 characters; empty gives back `GenHTTP Lambda` |
| `PUT /admin/branding/logo`               | uploads the logo, the body being the picture as it is: PNG, JPEG, GIF, WebP or SVG, at most 512 KB, told apart by its content |
| `DELETE /admin/branding/logo`            | takes the logo away; the name stays                                    |
| `DELETE /admin/branding`                 | takes both away: the pages show GenHTTP Lambda again                   |
| `GET /system/logo`                       | public: the picture beside the name - the logo, or the first letter of the name on a square plate where there is none; cached for good by the versioned address `brand.logo` of `GET /system` names |

An agent the operator handed the token does the same with these routes, or
with the MCP tool `customize_installation` (`adminToken`, `name`, `logo` - an
SVG's markup as it is, or a picture base64 encoded). The tool is added to the
endpoint only where the installation may be branded and has an admin token,
so no other agent is shown a tool it could not use; the token is an argument,
compared as the API compares it (`OperatorToken`), and never repeated.

Where it shows: the bar of every page - the front page, the showcase, the
editor, the published sources, the panel - the end of every title and the name
a link preview gives the site, the icon of the browser tab and of the site on a
phone's home screen (`manifest.json` names it too), the heading of
`llms.txt`, and the eyebrow of the community front page. The picture beside
the name is drawn at the height of a line; a square mark reads best, and the
panel previews it in both themes, since one drawn for a light background may
vanish on the dark one. Link previews of the site's own pages show the logo
where it is a raster image, small beside the words, and no picture otherwise -
their own pictures have GenHTTP Lambda drawn into them; a lambda's showcase
picture stays the lambda's.

**Wherever the pages or an app say what the installation is, they say its
name - decided by the owner**: the "Made with" line agents put at the foot of
the apps they build (`platform_guide`'s `backlink`, a repository's
`AGENTS.md` and the build agent's brief; the MCP instructions, fixed when the
endpoint starts, leave the words to the guide), the sentences of the
translations that say what an app was built with - the hosting domain's page,
what a lambda is and the description of a published source - which take it as
`{platform}` (the glossary tells every translator to write them so they read
right with any name), and a lambda's repository: its `AGENTS.md`, `CLAUDE.md`
and `Program.cs` name the installation as it is called when a commit is made.
A commit is kept as it was made and a push is compared with the commits it
builds on, so a rename changes the platform's files of the commits made after
it and refuses no push; a line an app already carries is given the name with
the next change to its page. Where the software is meant, GenHTTP Lambda stays
named: the link to its source and its community, the API's description, the
MCP server, and the project a lambda is exported as.

How it reaches a page: every index page a component sends - the site's, the
editor's, the panel's, the published sources' - is named for the installation
on the server before it is sent (`PageBranding`): its title, the names its link
preview is given, its icons, and `#lambda-brand`, from which the kit draws the
bar (`brand.ts`) - so neither a tab nor the first moment of a page shows the
platform's. An installation nobody branded is sent its pages exactly as they
were built. The public pages of the community site are prerendered twice,
once as GenHTTP Lambda and once with the name and the picture as placeholders
(`branded` in `prerender.json`), since a brand is other markup than the
platform's mark rather than another value in it; the server takes the second
for a branded installation and fills it in, and sends a page without content
for the browser to draw where a build has none.

The name is a setting (`branding.name`), the logo a file of the files
integration (`branding/logo`) with a row of `branding_pictures` saying what it
is (V26), both held in memory once read (`Services/Branding`). An SVG is checked
to be one a browser draws as a picture - well formed, its root in the SVG
namespace - and is served with `X-Content-Type-Options: nosniff` and a
`Content-Security-Policy` that runs nothing, should anybody open it on its own;
as a picture on a page, nothing in it runs anyway. Nothing of it is read where
the host does not let its operator brand the installation, so a copy of a
branded installation's data on another host brings nothing of its looks along.

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

The sitemap names no lambda: at subdomains every lambda answers at a host of
its own, and a sitemap names pages of the host it is served from; below a
path it lists none either.

## Database

The platform's records, through EF Core, in whichever database the
installation keeps them (see [Integrations](#integrations)) - `lambda.db` on
SQLite, the schema `platform` on PostgreSQL - migrated by [Evolve][evolve] as
the platform is built (`LambdaPlatform`), from the SQL files embedded for the
dialect the database speaks: `Data/Migrations` for SQLite, `Data/PostgreSql`
for PostgreSQL, both of `GenHTTP.Lambda.Core`. To change the schema, add the
next `V<n>__<name>.sql` to each set - every set is numbered on its own, and
the existing ones are never edited. The PostgreSQL set starts at the schema
the SQLite one had reached, written once.

The analytics are kept apart - `analytics.db` on SQLite, the schema
`analytics` on PostgreSQL - migrated the same way from
`Services/Analytics/Migrations` and `Services/Analytics/PostgreSql`, and spoken
to with plain commands both accept. Back it up with the rest, or leave it out:
nothing else depends on it, and an installation without it starts counting
again from nothing.

[genhttp]: https://genhttp.org/
[evolve]: https://evolve-db.netlify.app/
[efcore]: https://learn.microsoft.com/ef/core/
[git-module]: https://github.com/Kaliumhexacyanoferrat/virtual-git-server
