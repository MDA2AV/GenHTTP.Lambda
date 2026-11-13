# GenHTTP Lambda

Describe an app - a guestbook, a poll, a booking page, a small API - and get it
online at an address you can share. The build agent writes it from a sentence,
or your own coding agent (Claude Code, Codex, Cursor, …) builds it for you over
MCP. Every change is kept as a version you can go back to, and every app can be
downloaded as an ordinary .NET project.

This is the **community edition**: the whole platform, to run on your own
server with Docker. It is the same code that runs [genhttp.dev](https://genhttp.dev).

![The front page of a community installation](docs/images/community.png)

## What you can do

- **Have an app built from a sentence.** Type what you want into the box on
  the front page; the build agent writes the app, puts it online and gives you
  the link to change it later.
- **Build with your own agent.** Connect Claude Code, Codex, Cursor or any
  other agent that speaks MCP. It gets the platform as tools: create an app,
  write and check its code, try a change, put it online, read its logs.
- **Change it without breaking it.** A change is tried in a *draft* with a
  copy of the app's data and a link of its own to try it. Once it works, it is
  put online. Every version is kept - one click goes back to an earlier one.
- **Keep data.** Every app can have a database for its records, a folder for
  the files it saves, and keys and passwords for the services it talks to.
  You enter those in the editor; the agent only learns their names.
- **Give an app its own domain.** Make the app *premium* in the panel, point
  a domain at the server and enter it in the editor.
- **See who visits.** Page views, where visitors came from and their
  countries - counted on the server, without cookies, once you switch it on.
- **Work with git.** Every app is a repository: clone it, push a commit to put
  it online, push a branch to start a draft.
- **Share it.** Show an app on the showcase, or publish its code for others
  to read, star and download.
- **Take it with you.** Download any app as a .NET project and run it
  anywhere - nothing ties it to this platform.
- **Run the installation.** The panel at `/admin` lists every app, sets the
  limits, shows what the server is doing and its log, and blocks what does
  harm.

Every app is a small C# program - a [GenHTTP][genhttp] handler - which the
server compiles and runs in seconds:

```csharp
return Inline.Create()
             .Get(() => "Hello from my app!");
```

You never have to write one yourself, but you can: the editor has the code,
and so does every version.

## Run it with Docker

```bash
docker run -d --name lambda -p 8080:8080 -v lambda-data:/data genhttp/lambda
```

Open <http://localhost:8080>. Your apps answer below it, each at an address
of its own: `http://localhost:8080/lambda/my-app/`.

To reach it from other machines, tell it where it is reached - the addresses
of the apps are made from that:

```bash
docker run -d --name lambda -p 8080:8080 -v lambda-data:/data \
  -e LAMBDA_PUBLIC_URL=https://lambda.example.com \
  -e LAMBDA_ADMIN_TOKEN=a-long-password \
  genhttp/lambda
```

The [sample in samples/community](samples/community) does the same with
Docker Compose, and adds the build agent:

```bash
cd samples/community
cp .env.example .env     # and set LAMBDA_ADMIN_TOKEN, at least
docker compose up -d
```

### Settings

| Variable | What it does |
|---|---|
| `LAMBDA_PUBLIC_URL` | Where people and agents reach the installation. The addresses of the apps are made from it. Default `http://localhost:8080`. |
| `LAMBDA_ADMIN_TOKEN` | The password of the panel at `/admin`. Without one, the panel is off. |
| `LAMBDA_HOSTING_URL` | A domain for the apps, such as `https://apps.example.com`: each app then answers at a subdomain of its own (`https://my-app.apps.example.com/`) instead of below `/lambda/`. Needs a wildcard DNS record and certificate. |
| `LAMBDA_CONTACT` | A mail address the pages offer for questions - asking for premium, for one. |
| `LAMBDA_ABUSE_CONTACT` | A mail address the pages offer for reporting an app that does harm. |
| `LAMBDA_AGENT_URL`, `LAMBDA_AGENT_TOKEN` | Where the build agent runs, and the secret the two share (see below). |

There are more - limits, logging, TLS, analytics. They are all in
[the reference](docs/reference.md#configuration).

### HTTPS

The image answers plain HTTP on port 8080. Put it behind the reverse proxy you
already use - Caddy, nginx, Traefik - and set `LAMBDA_PUBLIC_URL` to the
`https://` address. It can also serve TLS itself; the reference says how.

### Your data

Everything the installation keeps is in the volume at `/data`: its database,
every version of every app, the apps' records and files, their git history -
and `secrets.key`, which the keys and passwords of the apps are sealed with.
Back up the whole volume, and keep `secrets.key` with it: without it, nobody
can read the keys and passwords again.

### Updating

```bash
docker pull genhttp/lambda
docker compose up -d     # or: docker rm -f lambda, and run it again
```

The database is brought up to date when the server starts. Your apps and
their data stay as they are.

## Connect your agent

The front page shows how, for each agent. For Claude Code, it is one line:

```bash
claude mcp add --transport http genhttp http://localhost:8080/mcp
```

Other agents add a remote MCP server with the address
`http://localhost:8080/mcp` (your `LAMBDA_PUBLIC_URL` followed by `/mcp`).

Then ask for something - *"Build a guestbook, with a page where I approve the
entries."* The agent creates the app, puts it online and gives you its address
and its editor link. **Keep the editor link**: it is the key to the app, and it
cannot be recovered.

Agents that can run HTTP requests or git can use those instead: the REST API
is described at `/api/v1/scalar/`, and every app's git address is in its editor.

## The build agent

The box on the front page has an agent behind it once you run one. It needs:

- a Claude subscription - make a token for it with `claude setup-token`;
- Docker on the same machine: the agent starts a short-lived container for
  every build, which can reach this server and Anthropic and nothing else.

In `samples/community/.env`, set `CLAUDE_CODE_OAUTH_TOKEN`, a random
`LAMBDA_AGENT_TOKEN` (`openssl rand -hex 32`), `LAMBDA_AGENT_URL=http://agent:8401`
and `DOCKER_GID` (`getent group docker | cut -d: -f3`), then:

```bash
docker compose --profile agent up -d
```

## Good to know

- **Apps share the address of the platform.** Below `/lambda/`, an app runs
  on the same address as the editor and the panel, so a page of an app could
  read what those keep in your browser - the admin token, for one. That is fine
  for apps you and your team build. To host apps of people you do not know,
  give the installation a domain for them (`LAMBDA_HOSTING_URL`): each app is
  then a site of its own.
- **Apps run inside the server.** Their code is checked for what it must not
  do before it runs, but it shares the server's process - this is no sandbox.
  Run code you would run yourself.
- **Unused apps are taken offline.** A new app is *free*: one that nobody
  visits or changes for 30 days is taken offline, and removed 90 days after
  that. In the panel, make an app *premium* to keep it for good, or change how
  long free apps last under *Limits*.

## For developers

GenHTTP Lambda is a set of components - the platform's services, the REST
API, the MCP endpoint, the runner that serves the apps, the editor, the panel
and the published sources - which a host puts together the way GenHTTP
applications are put together:

```csharp
var site = Layout.Create()
                 .Add("api", RestApi.Create(platform))
                 .Add("mcp", McpServer.Create(platform))
                 .Add("editor", Editor.Create(platform))
                 .Add("admin", AdminPanel.Create())
                 .Add(SourcePages.Create(platform))
                 .Add(LandingPage.Create(platform));

return await LambdaServer.Create(platform)
                         .Settings(settings)
                         .Handler(Runner.Create(platform).Site(site))
                         .RunAsync();
```

That is `src/GenHTTP.Lambda.Community/Program.cs`; genhttp.dev runs
`src/GenHTTP.Lambda.Cloud` beside it. To build and run from source you need
the .NET 11 SDK and Node 22:

```bash
npm install && npm run build                              # the frontends
dotnet run --project src/GenHTTP.Lambda.Community -c Release
```

- [docs/reference.md](docs/reference.md) - routes, the API, configuration,
  operations and how it is put together.
- [CLAUDE.md](CLAUDE.md) - the decisions behind it, and the rules for
  changing it.

## License

[MIT](LICENSE).

[genhttp]: https://genhttp.org
