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
| `/api/v1/`           | everything the editor calls                               |
| `/api/v1/start`      | creates a lambda and redirects into its editor            |

The assistant asks what the lambda should do before it asks for a key: a
service that answers requests, or a socket that stays open - and then which of
the examples in `Resources/Templates` to start from. A new one is a file next
to those, listed in `TemplateCatalog`.

Another page can hand someone a working lambda with a link:

```html
<a href="https://your.host/api/v1/start?template=websocket-functional">Try it online</a>
```

That creates one with a generated key and redirects into the editor. Ask for
`application/json` instead and the lambda is described rather than redirected
to, for a caller that would rather send the browser itself.

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
| `LAMBDA_DEPLOYMENT_LIFETIME_HOURS`  | `24`             | how long a free tier deployment stays up    |
| `LAMBDA_RETENTION_HOURS`            | `720`            | how long an untouched lambda is kept        |
| `LAMBDA_MAINTENANCE_INTERVAL_HOURS` | `0.25`           | how often expired lambdas are looked for    |
| `LAMBDA_MAX_CODE_LENGTH`            | `65536`          | largest snippet accepted                    |
| `LAMBDA_MAX_VERSIONS`               | `50`             | versions kept per lambda                    |
| `LAMBDA_RATE_LIMIT`                 | `240`            | lambda requests per minute and client       |
| `LAMBDA_MAX_CONCURRENCY`            | `64`             | lambda requests executed at once            |
| `LAMBDA_EXECUTION_TIMEOUT_SECONDS`  | `15`             | before an invocation is aborted             |
| `LAMBDA_TELEMETRY_INTERVAL_SECONDS` | `30`             | how often a reading is taken                |
| `LAMBDA_TELEMETRY_SAMPLES`          | `2880`           | how many readings are kept                  |
| `LAMBDA_PUBLIC_ACTIVITY`            | `true`           | serve the per lambda activity to anyone     |
| `LAMBDA_TLS_PORT`                   | `0`              | port for TLS, zero leaves it off            |
| `LAMBDA_CERTIFICATE`                | -                | PEM chain or PKCS#12 archive                |
| `LAMBDA_CERTIFICATE_KEY`            | -                | private key, for a PEM pair                 |
| `LAMBDA_CERTIFICATE_PASSWORD`       | -                | password of the PKCS#12 archive             |

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

## Database

SQLite through EF Core, migrated on startup by [Evolve][evolve] from the SQL
files in `Data/Migrations`. To change the schema, add the next
`V<n>__<name>.sql` next to them - they are embedded resources, and the
existing ones are never edited.

[genhttp]: https://genhttp.org/
[evolve]: https://evolve-db.netlify.app/
