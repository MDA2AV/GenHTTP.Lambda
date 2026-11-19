# PostgreSQL

The community edition keeps its data in SQLite files in its data volume unless
it is told otherwise. Told so, it keeps every database in one PostgreSQL
database instead: the platform's own records, its analytics, and the database
of every app and every draft. The apps' files and the platform's code stay in
the data volume, and the apps' secrets stay sealed in the platform's records -
in PostgreSQL, then.

Choose it when you would rather back up, replicate and watch a PostgreSQL
server than a volume of files, or already run one. SQLite stays the simpler
choice: one container, one volume, nothing else.

genhttp.dev keeps its data in SQLite; the option is the community edition's.

## Choose it before you start

The database is chosen as the installation starts, and what one database holds
is not moved into another. Start a new installation on PostgreSQL rather than
switching one that has apps: an installation switched over starts empty, its
SQLite files left where they were.

The apps themselves need no change. An app reaches its database with
`Database.GetConnection()` and `options.UseDatabase(connection)`, which mean
SQLite on one installation and PostgreSQL on another, and agents are taught
to write migrations in plain SQL both accept. Code written before that, for
SQLite in particular (`SqliteConnection`, `UseSqlite`, `SqliteException`),
compiles only where SQLite keeps the database.

## Run it with Docker Compose

`samples/community` has the server, and an overlay with a PostgreSQL server
beside it:

```bash
cd samples/community
POSTGRES_PASSWORD=a-long-random-password \
  docker compose -f docker-compose.yml -f docker-compose.postgres.yml up -d
```

The overlay sets two variables on the server and starts `postgres:17` with a
volume of its own (`postgres-data`). The user the image makes is the database's
owner and may create roles, which is what the platform needs.

## Run it against a server of your own

Set two variables:

```bash
docker run -d --name lambda -p 8080:8080 -v lambda-data:/data \
  -e LAMBDA_DATABASE=postgresql \
  -e LAMBDA_POSTGRES_CONNECTION="Host=db.example.com;Database=lambda;Username=lambda;Password=...;SSL Mode=Require" \
  genhttp/lambda
```

| Variable | Default | Meaning |
|---|---|---|
| `LAMBDA_DATABASE` | `sqlite` | `postgresql` keeps the databases in PostgreSQL |
| `LAMBDA_POSTGRES_CONNECTION` | - | how to reach the database, as [Npgsql](https://www.npgsql.org/doc/connection-string-parameters.html) reads it - required with `postgresql` |
| `LAMBDA_POSTGRES_ROLE_PREFIX` | `lambda` | what the role of every app is called before its id (`lambda_42`) |
| `LAMBDA_POSTGRES_POOL_SIZE` | `10` | connections an app or a draft keeps open at most |

The data volume is still needed: it holds the code of the apps, their files,
what they serve, the logo and the pictures of the showcase, and the
installation's key for the secrets (`secrets.key`,
unless `LAMBDA_SECRETS_KEY` is set). Back up both the volume and the database,
and keep the key with them: without it, no secret can be read.

### What the user needs

The user in the connection string needs to:

- create schemas in the database - own it, or have `CREATE` on it;
- create, change and drop roles on the server - `CREATEROLE`.

A database made for the installation and a user that owns it and may create
roles is enough:

```sql
CREATE ROLE lambda LOGIN CREATEROLE PASSWORD '...';
CREATE DATABASE lambda OWNER lambda;
```

A superuser works too, and is what the official Docker image makes - but the
platform needs no more than the above.

## What it keeps where

| Schema | Holds | Owner |
|---|---|---|
| `platform` | the lambdas, their versions, drafts, settings and sealed secrets | the installation's user |
| `analytics` | what was counted of the visits | the installation's user |
| `lambda_{id}` | the database of one app | the installation's user; its tables the app's role |
| `lambda_{id}_feature_{draft}` | the copy a draft works on | as above |

Every app connects as a role of its own, `{prefix}_{id}`, which may use and
create in its own schemas and nothing else: it cannot read the platform's
records or another app's, nor make a schema of its own. Its schema is the only
one on its search path, so its SQL names no schema. The role's password is
made anew every time the server starts and set when the role is first used; it
is not kept anywhere. What every role can see is PostgreSQL's catalog - the
names of schemas, tables and roles - not what they hold.

On PostgreSQL 14 and older, every role may create tables in the schema
`public`; the platform takes that right away from `PUBLIC` as it starts, where
its user may.

Two installations may share a server: give each a database and a role prefix
of its own (`LAMBDA_POSTGRES_ROLE_PREFIX`), since roles belong to the server.

## Connections

Every app and every draft has a pool of its own, of up to
`LAMBDA_POSTGRES_POOL_SIZE` connections, closed after a minute unused - so the
server's `max_connections` has to cover the apps busy at the same time, plus a
few for the platform. 100, PostgreSQL's default, is enough for a handful; the
overlay sets 200. With many busy apps, put PgBouncer in front of the server in
session mode, or lower the pool size.

The platform asks the database the way it asks SQLite - synchronously, on the
thread serving the request - so a slow or distant server slows every request
that waits on it. Keep it close.

## What differs from SQLite

- **The room of an app's data** is the same allowance, its database and its
  files together. PostgreSQL cannot hold a schema to a size, so it governs
  rather than enforces: once a database is past its room, the app's next
  connections only read, and writes fail with *cannot execute … in a
  read-only transaction*. Code could switch that back. Unlike on SQLite, that
  includes deleting: an app past its room cannot delete its way back under
  it, and a migration that has to write fails as it starts. Give it more room
  - another tier, or the tier's limit in the panel - or switch its database
  off, which deletes it. Its size is measured every few seconds, so a single
  connection may write past the room before that.
- **A draft's copy** holds the tables with their indexes, constraints and
  identities, their rows, the sequences, the foreign keys between them and
  the views - not functions, triggers or types an app made itself. The guide
  says so to agents.
- **Names** fold to lower case, as PostgreSQL folds a name written without
  quotes: the column Entity Framework asks for as `Title` is the `title` a
  migration made. A property for a column of another name says so with
  `HasColumnName`.
- **An export** carries no records: an exported project keeps them in a SQLite
  file, and what PostgreSQL holds is taken with
  `pg_dump --schema=lambda_{id}`.
- **The demos** run on either: they name no database.

The panel's **Integrations** section shows which database the installation
keeps its data in, and where - never the password.
