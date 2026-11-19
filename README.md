# GenHTTP Lambda

[![Cloud: genhttp.dev](https://img.shields.io/badge/cloud-genhttp.dev-blue)](https://genhttp.dev/)
[![Community: community.genhttp.run](https://img.shields.io/badge/community-community.genhttp.run-green)](https://community.genhttp.run/)
[![Documentation](https://img.shields.io/badge/docs-community.genhttp.run%2Fdocs-purple)](https://community.genhttp.run/docs/)
[![Docker Hub: genhttp/lambda](https://img.shields.io/docker/pulls/genhttp/lambda?label=genhttp%2Flambda&logo=docker)](https://hub.docker.com/r/genhttp/lambda)

**A self-hosted platform for the apps your coding agent builds.**

Coding agents such as Claude Code, Codex and Cursor can write a small web app
in minutes. Deploying it usually takes far longer: a container, a port, a
reverse proxy entry, a database and backups, repeated for every new app.
GenHTTP Lambda gives these apps a single home on your own server. Once your
agent is connected, every app it builds is online at its own address within
seconds, with a database, a version history and rollback. You operate one
container.

![An app being built from a description on GenHTTP Lambda](https://genhttp.dev/media/platform/build-dark.webp)

## Documentation

**[community.genhttp.run/docs](https://community.genhttp.run/docs/)** covers
everything you need to run and use GenHTTP Lambda:

- [Quick start](https://community.genhttp.run/docs/quick-start/) - from an empty server to an app online in five minutes
- [Concepts](https://community.genhttp.run/docs/concepts/apps/) - apps and keys, versions, drafts, data, tiers and connectivity
- [Connect an agent](https://community.genhttp.run/docs/build/mcp/) - Claude Code, Cursor, VS Code and any MCP client, plus git and the REST API
- [The editor](https://community.genhttp.run/docs/editor/) - looking after an app: its history, drafts, records, keys and visitors
- [Running a server](https://community.genhttp.run/docs/hosting/install/) - configuration, HTTPS and domains, the admin panel, security, backups, PostgreSQL and the build agent
- [Extending](https://community.genhttp.run/docs/extending/custom-host/) - a host of your own and integrations for where data is kept

## Quick start

```bash
docker run -d -p 8080:8080 -v lambda-data:/data -e LAMBDA_ADMIN_TOKEN=secure-password genhttp/lambda
claude mcp add --transport http genhttp http://localhost:8080/mcp
```

Then ask your agent for an app - *"Build a shared task list for the team"* -
and it answers with the app's address and its editor link. Other agents, a
public address with HTTPS and everything else are in the
[documentation](https://community.genhttp.run/docs/quick-start/).

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

GenHTTP Lambda is designed for small tools that often go unhosted because
deploying them takes more effort than writing them: in a company, the tools
that replace a shared spreadsheet - an inventory, an on-call planner, a
tracker for requests; at home, the pieces around your home automation - a
dashboard for your sensors, a webhook receiver, a small API for your scripts.

Every app is a small C# program, a [GenHTTP][genhttp] handler, which the
server compiles and starts within seconds - written by your agent, and yours
to read, change, clone with git or download as an ordinary .NET project:

```csharp
return Inline.Create()
             .Get(() => "Hello from my app!");
```

## Good to know

- **Apps run inside the server's process.** Their code is checked before it
  runs, but this is not a sandbox: host apps of people you trust, and read
  [Security](https://community.genhttp.run/docs/hosting/security/) before
  others build on your installation.
- **Telemetry.** The community edition sends a usage report once a day -
  counts and switches, never anything about an app or a person.
  `LAMBDA_USAGE_REPORTS=false` turns it off; [docs/telemetry.md](docs/telemetry.md)
  lists what a report contains.

## For developers

GenHTTP Lambda is a set of components - the platform's services, the REST API,
the MCP endpoint, the runner that serves the apps, the editor, the
administration panel and the published sources - which a host composes the
way any GenHTTP application is composed. `src/GenHTTP.Lambda.Community` is
the community edition's host, `src/GenHTTP.Lambda.Cloud` genhttp.dev's;
[A host of your own](https://community.genhttp.run/docs/extending/custom-host/)
shows how to write another. Building and running from source requires the
.NET 11 SDK and Node 22:

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
