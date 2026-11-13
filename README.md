# GenHTTP Lambda

[![Cloud: genhttp.dev](https://img.shields.io/badge/cloud-genhttp.dev-blue)](https://genhttp.dev/)
[![Community: community.genhttp.run](https://img.shields.io/badge/community-community.genhttp.run-green)](https://community.genhttp.run/)
[![Docker Hub: genhttp/lambda](https://img.shields.io/docker/pulls/genhttp/lambda?label=genhttp%2Flambda&logo=docker)](https://hub.docker.com/r/genhttp/lambda)

**A self-hosted platform for the apps your coding agent builds.**

Coding agents such as Claude Code, Codex and Cursor can write a small web app
in minutes. Deploying it usually takes far longer: a container, a port, a
reverse proxy entry, a database and backups, repeated for every new app.
GenHTTP Lambda gives these apps a single home on your own server. Once your
agent is connected, every app it builds is online at its own address within
seconds, with a database, a version history and rollback. You operate one
container.

```bash
docker run -d -p 8080:8080 -v lambda-data:/data -e LAMBDA_ADMIN_TOKEN=secure-password genhttp/lambda
```

Then open <http://localhost:8080>.

![The front page of a GenHTTP Lambda server](docs/images/community.png)

## Why GenHTTP Lambda

- **A single container.** No database server, Redis, message queue or
  Kubernetes is required. The server, all apps and their data run in one
  container and are stored in one volume, so a backup is a copy of that
  volume.
- **Hundreds of apps per installation.** All apps run in the same process,
  each at its own address, without a separate container, port, proxy entry or
  database server. [genhttp.dev](https://genhttp.dev), which runs the same
  code, hosts more than a hundred apps this way.
- **Deployment is handled by your agent.** Any agent that supports MCP can
  create an app, write and check its code, test a change on a copy, publish it
  and read its logs, without you having to open a terminal.
- **You stay in control.** Every app has an editor that shows what changed and
  why, restores any earlier version in one click, and gives you access to its
  records, its visitors and its code.

## Use cases

GenHTTP Lambda is designed for small apps that often go unhosted because
deploying them takes more effort than writing them:

- a chore list for a family, a booking page for a club, a sign-up sheet for a
  team;
- a dashboard for an API you use, a webhook receiver for your automations, a
  small API for your scripts;
- an event page, a guestbook, a poll, a multiplayer game.

A request such as *"Build a shared task list for the team"* produces an app
that is online. A follow-up such as *"Give every task the day it is due"* is
tested in a draft before it goes live.

## Screenshots

<table>
  <tr>
    <td width="50%" valign="top"><img src="docs/images/agents.png" alt="Connecting an agent"><br><sub><b>Connecting an agent</b> - a single command for Claude Code, and the endpoint address for any other MCP client.</sub></td>
    <td width="50%" valign="top"><img src="docs/images/app.png" alt="A task list built by an agent"><br><sub><b>A generated app</b> - served at its own address, here <code>/lambda/tasks/</code>.</sub></td>
  </tr>
  <tr>
    <td valign="top"><img src="docs/images/draft.png" alt="A draft of a change"><br><sub><b>Drafts</b> - a change is tested with a copy of the data at a separate address before it is published.</sub></td>
    <td valign="top"><img src="docs/images/versions.png" alt="The versions of an app"><br><sub><b>Versions</b> - what each change did and why, with rollback to any of them.</sub></td>
  </tr>
  <tr>
    <td valign="top"><img src="docs/images/records.png" alt="The records of an app"><br><sub><b>Records</b> - each app can have its own database, which can be browsed in the editor.</sub></td>
    <td valign="top"><img src="docs/images/analytics.png" alt="The visitors of an app"><br><sub><b>Analytics</b> - page views and referrers, counted on the server without cookies.</sub></td>
  </tr>
  <tr>
    <td valign="top"><img src="docs/images/code.png" alt="The code of an app"><br><sub><b>Code</b> - available when you need it: read it, edit it, or clone it with git.</sub></td>
    <td valign="top"><img src="docs/images/panel.png" alt="The administration panel"><br><sub><b>Administration panel</b> - every app on the server, its traffic and errors, the limits and the log.</sub></td>
  </tr>
</table>

## What every app gets

- **An address** below `/lambda/` on your server or at a subdomain of its own,
  and a custom domain once the app is made *premium* in the panel.
- **Data that is independent of versions**: a SQLite database for records, a
  folder for saved files, and secrets for the API keys and passwords of the
  services it uses. Secret values are entered in the editor; the agent only
  ever sees their names.
- **Drafts and versions**: a change is tested in a draft with a copy of the
  data and then published, and earlier versions remain available for
  rollback.
- **Git**: every app is a repository. Pushing a commit to `main` saves a
  version, and pushing a branch starts a draft.
- **Analytics and logs**: page views, referrers and countries once enabled,
  and the app's output for each request.
- **No lock-in**: any app can be downloaded as a standard .NET project and run
  anywhere else. Nothing ties it to this server.

Under the hood, every app is a small C# program, a [GenHTTP][genhttp] handler,
which the server compiles and starts within seconds. Writing one by hand is
optional:

```csharp
return Inline.Create()
             .Get(() => "Hello from my app!");
```

## Running it

```bash
docker run -d --name lambda -p 8080:8080 -v lambda-data:/data \
  -e LAMBDA_PUBLIC_URL=https://lambda.example.com \
  -e LAMBDA_ADMIN_TOKEN=a-long-password \
  genhttp/lambda
```

- **One container, one port, one volume.** No other services are required:
  the platform's data and every app's database are SQLite files in `/data`.
- **`LAMBDA_PUBLIC_URL`** is the address at which you and your agents reach
  the server, and the apps' addresses are derived from it. If it is not set,
  the server answers at any host name and the addresses use
  `http://localhost:8080`.
- **`LAMBDA_ADMIN_TOKEN`** is the password for the administration panel at
  `/admin`. If it is not set, the panel is disabled.
- Images are published for **amd64 and arm64**.

The [sample in samples/community](samples/community) provides the same setup
with Docker Compose and can optionally include the build agent:

```bash
cd samples/community
cp .env.example .env     # set at least LAMBDA_ADMIN_TOKEN
docker compose up -d
```

### Configuration

| Variable | Description |
|---|---|
| `LAMBDA_PUBLIC_URL` | The address at which people and agents reach the installation, such as `https://lambda.example.com` - a host name or an IPv4 address. The apps' addresses are derived from it, and once it is set the server answers only at that name and at `localhost`. If it is not set, the server answers at any name and the addresses use `http://localhost:8080`. |
| `LAMBDA_ADMIN_TOKEN` | The password for the administration panel at `/admin`. If it is not set, the panel is disabled. |
| `LAMBDA_HOSTING_URL` | A domain for the apps, such as `https://apps.example.com`. Each app then answers at its own subdomain (`https://my-app.apps.example.com/`) instead of below `/lambda/`. Requires a wildcard DNS record and a wildcard certificate. |
| `LAMBDA_CONTACT` | An email address the pages offer for questions, such as requests for premium. |
| `LAMBDA_ABUSE_CONTACT` | An email address the pages offer for reporting a harmful app. |
| `LAMBDA_AGENT_URL`, `LAMBDA_AGENT_TOKEN` | The address of the build agent and the secret shared with it (see below). |

Further settings, covering limits, logging, TLS and analytics, are described
in [the reference](docs/reference.md#configuration).

### HTTPS

The image serves plain HTTP on port 8080. Place it behind your existing
reverse proxy, such as Caddy, nginx or Traefik, with a single entry for the
whole server, and set `LAMBDA_PUBLIC_URL` to the `https://` address. The
server can also serve TLS itself; the reference describes how.

### Data and backups

The installation stores all of its data in the volume at `/data`: its
database, every version of every app, the apps' records and files, their git
history, and `secrets.key`, the key the apps' secrets are encrypted with. Back
up the entire volume, including `secrets.key`. Without that file, the stored
secrets cannot be decrypted.

### Updating

```bash
docker pull genhttp/lambda
docker compose up -d     # or: docker rm -f lambda, then run it again
```

The database is migrated when the server starts. Apps and their data are left
unchanged.

## Connecting an agent

The front page explains how to connect each agent. For Claude Code, a single
command is sufficient:

```bash
claude mcp add --transport http genhttp http://localhost:8080/mcp
```

Other agents can add a remote MCP server at `http://localhost:8080/mcp` (the
value of `LAMBDA_PUBLIC_URL` followed by `/mcp`).

You can then describe what you need, for example *"Build a guestbook, with a
page where I approve the entries."* The agent creates the app, publishes it
and returns its address and its editor link. **Store the editor link
safely**: it grants full control over the app and cannot be recovered.

Agents that can make HTTP requests or use git can work that way instead: the
REST API is documented at `/api/v1/scalar/`, and each app's git address is
shown in its editor.

## Building apps from a description: the build agent (optional)

With the build agent enabled, the front page offers an input box: people
describe the app they want, and an agent on your server writes it, publishes
it and returns a link for changing it. No coding agent of their own is
required. The build agent is optional and is the only part of the setup that
runs outside the main container. It requires:

- a Claude subscription, with a token created by `claude setup-token`;
- Docker on the same machine: for every build, the agent starts a short-lived
  container that can reach only this server and Anthropic.

In `samples/community/.env`, set `CLAUDE_CODE_OAUTH_TOKEN`, a random
`LAMBDA_AGENT_TOKEN` (`openssl rand -hex 32`), `LAMBDA_AGENT_URL=http://agent:8401`
and `DOCKER_GID` (`getent group docker | cut -d: -f3`), then start it:

```bash
docker compose --profile agent up -d
```

## Security and operations

- **Apps share the platform's address.** Below `/lambda/`, an app runs on the
  same origin as the editor and the administration panel, so its pages could
  read what those store in your browser. The editor does not send its address
  to apps, and the panel keeps its password for the current tab only, but this
  setup is intended for apps built by you and your team. To host apps for
  people you do not know, give the installation a separate domain for them
  (`LAMBDA_HOSTING_URL`); each app is then a site of its own.
- **Apps run inside the server process.** This is what makes hosting hundreds
  of them inexpensive. Their code is checked for prohibited operations before
  it runs, but it shares the server's process: this is not a sandbox. Only run
  code you would be prepared to run yourself.
- **Connecting to other servers requires approval.** An app's code may connect
  to other servers, for example to call an API, open a socket or send email,
  only once you have allowed it in the panel's *Connectivity* section, where
  its owner's request is listed together with its purpose. *Premium* apps may
  connect without approval. Without this rule, anybody could route their
  traffic through your server with an app that fetches whatever address it is
  given. Pages that fetch from other sites in the browser need no approval,
  and agents are instructed to use that approach wherever possible.
- **Unused apps are taken offline.** A new app is *free*: by default, one that
  is neither visited nor changed for 30 days is taken offline, and removed 90
  days after that. Making an app *premium* in the panel exempts it; how long
  free apps last can be changed under *Limits*.

## For developers

GenHTTP Lambda is a set of components (the platform's services, the REST API,
the MCP endpoint, the runner that serves the apps, the editor, the
administration panel and the published sources) which a host composes in the
same way as any other GenHTTP application:

```csharp
var site = Layout.Create()
                 .Add(PlatformRoutes.Api, RestApi.Create(platform))
                 .Add(PlatformRoutes.Mcp, McpServer.Create(platform))
                 .Add(PlatformRoutes.Editor, Editor.Create(platform))
                 .Add(PlatformRoutes.Admin, AdminPanel.Create())
                 .Add(SourcePages.Create(platform))
                 .Add(LandingPage.Create(platform));

return await LambdaServer.Create(platform)
                         .Settings(settings)
                         .Handler(Runner.Create(platform).Site(site))
                         .RunAsync();
```

This is `src/GenHTTP.Lambda.Community/Program.cs`; genhttp.dev runs
`src/GenHTTP.Lambda.Cloud` alongside it. Building and running from source
requires the .NET 11 SDK and Node 22:

```bash
npm install && npm run build                              # the frontends
dotnet run --project src/GenHTTP.Lambda.Community -c Release
```

- [docs/reference.md](docs/reference.md) - routes, the API, configuration,
  operations and architecture.
- [CLAUDE.md](CLAUDE.md) - the design decisions behind the project and the
  rules for changing it.

## License

[MIT](LICENSE).

[genhttp]: https://genhttp.org
