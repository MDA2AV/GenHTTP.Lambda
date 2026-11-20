# GenHTTP Lambda with Docker Compose

The community edition on <http://localhost:8080>, with everything it keeps in
the volume `lambda-data`.

```bash
cp .env.example .env
docker compose up -d
```

Open <http://localhost:8080>, and the panel at <http://localhost:8080/admin>
with the password `secure-password`. Change it (`LAMBDA_ADMIN_TOKEN` in `.env`)
before anybody else can reach the server. Then connect your agent - for Claude
Code:

```bash
claude mcp add --transport http genhttp http://localhost:8080/mcp
```

Then ask it for an app: *"Build a guestbook, with a page where I approve the
entries."*

## With the build agent

The box on `/build` builds an app from a sentence once the build agent runs.
It needs a Claude subscription or an API key, and Docker on this machine. In
`.env`:

1. `CLAUDE_CODE_OAUTH_TOKEN` - a token made with `claude setup-token`; or
   `LAMBDA_AGENT_API_KEY` - an API key of your organization's account;
2. `LAMBDA_AGENT_TOKEN` - any long random string: `openssl rand -hex 32`;
3. `LAMBDA_AGENT_URL=http://agent:8401` - uncomment it;
4. `DOCKER_GID` - the group of the docker socket: `getent group docker | cut -d: -f3`.

```bash
docker compose --profile agent up -d
```

Every build runs in a container of its own, which reaches this server and
Anthropic and nothing else, and is thrown away when it ends. Which models and
efforts people may choose is set in the panel, under **Build agent**.

## With PostgreSQL

The databases - the platform's records, its analytics and every app's - in a
PostgreSQL server beside it rather than in SQLite files. Choose it before you
create apps; nothing is moved from one to the other.

```bash
POSTGRES_PASSWORD=a-long-random-password \
  docker compose -f docker-compose.yml -f docker-compose.postgres.yml up -d
```

[docs/postgresql.md](../../docs/postgresql.md) has a server of your own, and
what differs from SQLite.

## Elsewhere than localhost

Set `LAMBDA_PUBLIC_URL` to the address people reach the server at - the
addresses of the apps are made from it - and put a reverse proxy in front of
port 8080 for HTTPS. The [README](../../README.md) has the rest: the settings,
your data, updating, and what to know before you let strangers build apps here.
