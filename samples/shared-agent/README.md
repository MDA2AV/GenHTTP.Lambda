# Several installations, one build agent

Two installations of the community edition on one machine - `team-a` on
<http://localhost:8080>, `team-b` on <http://localhost:8081> - each with its
own apps, data and panel, and one build agent that builds for both. Their
builds run at the same time, and neither sees the other's.

```bash
cp .env.example .env    # a token per installation, and what the builds sign in with
docker compose up -d
```

Open either installation's `/build` and ask for an app. Each panel
(`/admin`, **Build agent**) shows its own jobs, how many builds run at once
and how many installations share the runner.

## What has to hold

The layout here is one compose project; the rules are the same for any other -
installations in compose projects of their own, more of them, real hostnames.

1. **A token per installation.** Each installation's `LAMBDA_AGENT_TOKEN` is
   its own - at least 16 characters, `openssl rand -hex 32` - and the runner
   tells the installations apart by it. Two with the same token stop the
   runner as it starts.
2. **The runner names every installation** in `AGENT_HOSTS` - or in a file
   that `AGENT_HOSTS_FILE` names, which suits secrets mounted as files and
   installations that come and go - the runner reads it again as it changes:

   ```json
   [
     { "name": "team-a", "token": "…", "mcp": "http://team-a:8080/mcp", "origin": "https://team-a.example.com" },
     { "name": "team-b", "token": "…", "mcp": "http://team-b:8080/mcp", "origin": "https://team-b.example.com" }
   ]
   ```

   - `name` - lower case letters, digits and dashes; it is only shown in the
     runner's log.
   - `token` - the installation's `LAMBDA_AGENT_TOKEN`.
   - `mcp` - where its builds reach its MCP endpoint: its name on the build
     network. Builds reach these addresses directly, never through the egress
     proxy; the runner adds them to `NO_PROXY` itself.
   - `origin` - where people open the installation (its `LAMBDA_PUBLIC_URL`):
     the editor link a build hands back is made of it.

   Nothing else may be in an entry, so a misspelt field stops the runner
   rather than being ignored. With `AGENT_HOSTS`, the runner does not read
   `AGENT_TOKEN`, `AGENT_MCP_URL` and `AGENT_PUBLIC_ORIGIN`, and warns where
   they are set.
3. **Every installation is on the build network** under the name its `mcp`
   uses, and lists that name in `LAMBDA_MCP_ORIGINS`: an installation with a
   public address answers only at its own names, and the builds come by this
   one.
4. **The runner and every installation share the agent network**, and each
   installation's `LAMBDA_AGENT_URL` points at the runner.
5. **One build network for the runner** (`AGENT_BUILD_NETWORK`), which the
   builds, the egress proxy and every installation are on, and nothing else.
6. **How many builds run at once** is the runner's `AGENT_CONCURRENCY` (1 by
   default), of all installations together; each build takes up to 2 GB and one
   and a half cores while it runs (`AGENT_BUILD_MEMORY`, `AGENT_BUILD_CPUS`). A
   free place goes to the installation with the fewest builds running, so a
   busy one does not keep the others waiting. A job on a model behind an
   installation's password runs without a clock, one per installation at a
   time, beside the others.

## Installations in compose projects of their own

Declare the two networks in the runner's project with fixed names, as this
file does (`genhttp-shared-agent-net`, `genhttp-shared-build-net`), and start
it first. Every installation's project then joins them:

```yaml
services:
  lambda:
    image: genhttp/lambda:latest
    environment:
      LAMBDA_PUBLIC_URL: https://team-c.example.com
      LAMBDA_MCP_ORIGINS: team-c
      LAMBDA_AGENT_URL: http://agent:8401
      LAMBDA_AGENT_TOKEN: ${LAMBDA_AGENT_TOKEN}
    networks:
      default:
      agent-net:
      build-net:
        aliases: [team-c]

networks:
  agent-net:
    name: genhttp-shared-agent-net
    external: true
  build-net:
    name: genhttp-shared-build-net
    external: true
```

and the runner gets an entry for it, with `"mcp": "http://team-c:8080/mcp"`: in
`AGENT_HOSTS`, followed by a restart of the runner - a build under way then is
lost, as it always was - or in the file `AGENT_HOSTS_FILE` names, which it
reads again without one.

An installation that terminates TLS itself and redirects plain HTTP, as
genhttp.dev does, is reached by HTTPS at its public name: give it that name as
an alias on the build network, and `https://` that name as its `mcp`.

## What they share, and what they do not

Shared: the credential the builds sign in to Anthropic with - one
subscription's limits or one account's bill for all of them - the models it
runs, and the machine's room for builds. Each installation still decides in
its own panel which models and efforts its people may choose, whether builds
and changes are offered, and its own password.

Not shared: everything else. A build is made at the MCP of the installation
that asked, so the app is that installation's; an installation is answered
only about its own jobs, and finds, waits for and stops only its own changes,
whatever its apps are numbered. The runner's health tells an installation its
own jobs, how many builds run at once and how many installations there are -
nothing of what the others do. The runner's log names the installation of
every job.

The reference ([The build agent](../../docs/reference.md#the-build-agent))
has the rest.
