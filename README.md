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
SQLite database, the stored code and the lambda workspaces - lives in a named
volume mounted at `/data`.

## Developing locally

Two processes: the .NET server, and Vite serving the frontend with hot reload.

```bash
# the server, on http://localhost:8080/
dotnet run --project src/GenHTTP.Lambda

# the frontend, on http://localhost:5173/ (proxies /api and /lambda to the server)
cd src/Frontend && npm install && npm run dev
```

Work against <http://localhost:5173/> while developing. The server on its own
answers with a placeholder page until a frontend has been built into its web
root, which is what `npm run build` does:

```bash
cd src/Frontend && npm run build   # writes src/GenHTTP.Lambda/wwwroot
```

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
| `/editor/create`     | the creation assistant                                    |
| `/editor/:privateKey`| the editor for one lambda                                 |
| `/lambda/:publicKey` | the deployed handler                                      |
| `/start`             | opens the creation assistant with a template chosen      |
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
| `POST /lambdas`                                       | creates a lambda                          |
| `GET / PATCH / DELETE /lambdas/:privateKey`           | reads, changes (its key), removes it      |
| `GET /lambdas/:privateKey/export`                     | the lambda as a runnable project (zip)    |
| `GET / POST /lambdas/:privateKey/versions`            | lists versions, saves a new one           |
| `GET /lambdas/:privateKey/versions/:version`          | reads one version                         |
| `GET /lambdas/:privateKey/versions/:version/zip`      | one version's files as a zip              |
| `POST /lambdas/:privateKey/versions/zip`              | saves a zip of all files as a new version |
| `POST /lambdas/:privateKey/versions/changes`          | changes some files of the newest version  |
| `GET /lambdas/:privateKey/deployment`                 | what is online, and until when            |
| `POST /lambdas/:privateKey/deployment/start` / `stop` | puts a version online, takes it off       |
| `GET /lambdas/:privateKey/files`                      | lists the workspace                       |
| `GET / PUT / DELETE /lambdas/:privateKey/files/:path` | one file, its path encoded (`a%2Fb.txt`)  |
| `PUT /lambdas/:privateKey/folders/:path`              | makes a folder                            |
| `POST /lambdas/:privateKey/code/check`                | compiles without saving                   |
| `POST /lambdas/:privateKey/code/semantics`, `completions`, `definition` | what the editor asks the compiler |
| `GET /keys/:publicKey`                                | whether a key is free, and if not, online |
| `GET /examples`, `/examples/:id`                      | the examples the installation runs        |
| `POST /builds`, `GET /builds/:id`                     | the text box on `/build`                  |
| `GET /system`                                         | terms, limits, templates, build agent     |
| `GET /telemetry`, `/logs`, `/admin/...`               | for whoever runs the installation         |

The assistant asks what the lambda should do before it asks for a key: a
service that answers requests, or a socket that stays open - and then which of
the examples in `Resources/Templates` to start from. A new one is a file next
to those, listed in `TemplateCatalog`.

Another page can hand someone a working lambda with a link:

```html
<a href="https://your.host/start?template=websocket-functional">Try it online</a>
```

That opens the creation assistant with the template chosen. Nothing is created
until the visitor has seen the terms and submitted it, so a crawler following
the link leaves nothing behind.

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

Editing and deploying are separate: saving creates a version, deploying picks
one (the latest by default) and makes it live. A lambda has at most one
deployment at a time, and older versions stay available to deploy again.

## How it is put together

A single .NET 11 project hosted by `GenHTTP.Full.Ioxide`, wired up in
[`Application.cs`](src/GenHTTP.Lambda/Application.cs) and divided into
services that the API resources talk to through interfaces:

- **Meta** (`Services/Meta`) - lambdas and their versions, the only component
  that speaks to the database. Its public surface is DTOs, mapped by hand.
- **Storage** (`Services/Storage`) - the code itself, on the file system, one
  file per version. Never in the database.
- **Workspace** (`Services/Workspace`) - the private directory of a lambda,
  reached from the editor. The same directory the generated `Workspace` class
  writes to from inside a lambda, under the same limits, so a file put there by
  hand behaves like one the lambda wrote itself.
- **Deployment** (`Services/Deployment`) - wraps a snippet in a method body,
  compiles it with Roslyn, loads the assembly and calls `PrepareAsync()` on
  the resulting handler. Compiled once, then cached.
- **Execution** (`Services/Execution`) - an `IHandler`, not a web service: it
  looks up the handler for a request and runs it.
- **Protection** (`Services/Protection`) - concerns in front of execution that
  resolve the lambda, rate limit per client and cap concurrency.
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

Compiled lambdas run in the server process. The guard raises the cost of
misbehaving; it is not a sandbox, which is why the container runs unprivileged
and the seccomp profile is left alone.

## Configuration

Everything is read from the environment on startup, see
[`LambdaOptions`](src/GenHTTP.Lambda/Configuration/LambdaOptions.cs).

| Variable                            | Default          | Meaning                                     |
|-------------------------------------|------------------|---------------------------------------------|
| `LAMBDA_PORT`                       | `8080`           | port of the root server                     |
| `LAMBDA_ENGINE`                     | `ioxide`         | `ioxide` or `kestrel` (see below)           |
| `LAMBDA_DATA_DIRECTORY`             | `./data`         | database, code and workspaces               |
| `LAMBDA_WEB_ROOT`                   | `./wwwroot`      | the built frontend                          |
| `LAMBDA_DEVELOPMENT`                | `false`          | verbose error pages and debug logging       |
| `LAMBDA_DEPLOYMENT_LIFETIME_HOURS`  | `720`            | unused for this long and it goes offline       |
| `LAMBDA_RETENTION_HOURS`            | `2160`           | unused for this long and it is removed        |
| `LAMBDA_MAINTENANCE_INTERVAL_HOURS` | `0.25`           | how often expired lambdas are looked for    |
| `LAMBDA_MAX_ASSET_BYTES`            | `2097152`        | what the shipped assets may come to         |
| `LAMBDA_MAX_CODE_LENGTH`            | `131072`         | largest snippet accepted                    |
| `LAMBDA_MAX_VERSIONS`               | `50`             | versions kept per lambda                    |
| `LAMBDA_RATE_LIMIT`                 | `5000`           | lambda requests per second and client       |
| `LAMBDA_MAX_CONCURRENCY`            | `64`             | lambda requests executed at once            |
| `LAMBDA_EXECUTION_TIMEOUT_SECONDS`  | `15`             | before an invocation is aborted             |
| `LAMBDA_TELEMETRY_INTERVAL_SECONDS` | `30`             | how often a reading is taken                |
| `LAMBDA_TELEMETRY_SAMPLES`          | `2880`           | how many readings are kept                  |
| `LAMBDA_PUBLIC_ACTIVITY`            | `true`           | serve the per lambda activity to anyone     |
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
| `LAMBDA_TLS_PORT`                   | `0`              | port for TLS, zero leaves it off            |
| `LAMBDA_CERTIFICATE`                | -                | PEM chain or PKCS#12 archive                |
| `LAMBDA_CERTIFICATE_KEY`            | -                | private key, for a PEM pair                 |
| `LAMBDA_CERTIFICATE_PASSWORD`       | -                | password of the PKCS#12 archive             |
| `LAMBDA_CERTIFICATE_DIRECTORY`      | -                | further certificates, one folder per name   |

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

## Telemetry

`/stats` shows what the process is doing, and `/api/v1/telemetry` is where it
comes from. A reading is taken every `LAMBDA_TELEMETRY_INTERVAL_SECONDS` and
`LAMBDA_TELEMETRY_SAMPLES` of them are kept, which is a day at the defaults.

The readings live in memory and go with the process. That suits what they are
for - a restart ends the run they were measuring - but it does mean a redeploy
starts the graph over.

It also lists what each lambda has served, busiest first. Only the public key
identifies one there - the editor key, the code and the visitors are not part
of it - and `LAMBDA_PUBLIC_ACTIVITY=false` stops that list being served without
stopping anything being counted.

The page is otherwise aggregates only: no key, no code and no client address
leaves through it, which is what makes it safe to serve to anyone. What it is for is
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

### Shipping assets

A lambda is not only C#. Any file whose name does not end in `.cs` is an asset:
it is served as it is, never compiled, and costs none of the code budget.

```
lambda.cs      the snippet
Types.cs       compiled beside it
index.html     an asset
www/app.css    an asset, in a folder
logo.png       an asset, sent as base64
```

The snippet reaches them through `Assets`, which is the other half of
`Workspace` - what the lambda shipped, against what it has written since:

```csharp
return Layout.Create()
             .Add("api", api)
             .Add(Assets.App());
```

`Assets.App()` is a single page application over them: `index.html` is the
shell and a path matching no file is answered with it. `Assets.Tree()` and
`Assets.Files()` are there for anything less opinionated, and content types
come from the extension.

The directory is rewritten from the version being deployed, so an asset dropped
from a version stops being served rather than lingering. `LAMBDA_MAX_ASSET_BYTES`
is what they may come to in total.

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

## For agents

There is a Model Context Protocol endpoint at `/mcp`, so an agent can build
something here and put it online without a person driving the editor. It is
JSON-RPC over a single path, taking POST; a GET is declined with 405, because
this server answers rather than streams.

```
https://genhttp.dev/mcp
```

The tools are the shape of the job: `create_lambda`, `write_code`, `check_code`,
`deploy`, `read_lambda`, and `list_examples` / `read_example` for reading
something that already works. `platform_guide` is the one to call first - it
says what a snippet has to return, what is imported, what is refused, and the
handful of things that catch people out.

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
it is kept in session storage, so closing the tab locks it again.

This is the one part that authenticates. Everywhere else the editor link is
the credential and it only ever reaches one lambda; this reads code that belongs
to other people and can take their work away, so it asks for `LAMBDA_ADMIN_TOKEN`
in an `X-Admin-Token` header. Until that variable is set there is no panel at
all, and a wrong token is answered exactly like a missing one - an installation
that has no panel and one that is guarding it look the same from outside.

The server figures follow the same token, on the rule that an installation with
an administrator keeps them to them. Where no token is configured there is
nothing to check a request against, so requiring one would only mean nobody
could ever read them - there they stay public, as they were before there was a
panel to put them behind, and `LAMBDA_PUBLIC_ACTIVITY` still decides the per
lambda figures.

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

## Database

SQLite through EF Core, migrated on startup by [Evolve][evolve] from the SQL
files in `Data/Migrations`. To change the schema, add the next
`V<n>__<name>.sql` next to them - they are embedded resources, and the
existing ones are never edited.

[genhttp]: https://genhttp.org/
[evolve]: https://evolve-db.netlify.app/
