# Telemetry

The community edition of GenHTTP Lambda sends a usage report once a day,
starting shortly after the server starts, to
`https://telemetry.genhttp.run/api/reports`. The reports show how many
installations exist and how they are set up, which helps decide what to work
on next.

## What a report contains

- a random installation ID, created on first use and stored in the
  installation's database, so that the reports of one installation can be
  told apart from those of another;
- the version of GenHTTP Lambda;
- the operating system, processor architecture and .NET version, whether the
  server runs in a container, the number of processors, the memory (the
  machine's, or the container's limit if lower), and the size and free space
  of the data volume;
- the server engine and GenHTTP version, whether the server terminates TLS
  itself, its uptime, the number of requests served since it started and the
  memory the process uses;
- which options are enabled: how apps are addressed (by path or by
  subdomain), a hosting domain, the build agent and its input boxes, the
  administration panel, terms, contact addresses, legal pages, the number of
  MCP origins, the logging options, whether the operator gave the
  installation a name or logo of its own (never the name or the logo), and
  what keeps its data: the database (`sqlite` or `postgresql`), the apps'
  files and their secrets, by name only - never where they are or how they
  are reached;
- the number of apps by tier (`sandbox`, `production`, `system` - and `free`
  and `premium`, the sandbox and production under the names they had before,
  which older collectors read), online, blocked, created and active in the
  last 30 days, and opening in the simple view, together with the total number
  of their versions and drafts;
- the total size of the apps' databases, workspaces, versions and drafts, and
  the size of the largest app's data;
- the number of apps that use each feature: database, workspace, secrets,
  analytics, showcase, published source, custom domain, connectivity (allowed
  and requested) and git;
- the number of versions saved in the last 30 days, by origin: an agent over
  MCP, the REST API or the editor, a git push, or a new app.

The demo apps, which every installation has, are not counted.

## What a report never contains

Nothing that identifies the installation, an app or a person: not the address
the installation is reached at (`LAMBDA_PUBLIC_URL`) or any other of its
settings' values, no app names, keys or domains, no code, descriptions,
records, files or secrets, no visitor data, no log lines and no IP addresses.
The collecting endpoint does not store the address a report came from.

## Seeing what is sent

The administration panel shows the last report exactly as it was sent, under
*Usage report*, together with where it was sent and whether it arrived. The
server also logs a line on startup while reports are enabled.

## Disabling it

Set `LAMBDA_USAGE_REPORTS=false` (`0`, `no` and `off` work as well) and
restart the server. `DO_NOT_TRACK=1` is honoured too. While reports are
disabled, nothing is collected.

## Behaviour without network access

Reports are sent by a background job, never during startup or while a request
is served. If the endpoint cannot be reached, for example because of a
firewall, the attempt gives up after 30 seconds, the server logs a single
warning and continues normally, and the next attempt is made a day later.
Nothing is retried in between.

## Format

A report is a single JSON document, sent with `POST` and
`Content-Type: application/json`. Its `schema` field is the version of the
format; fields are added over time, never renamed. The types that make it up
are in
[`UsageReport.cs`](../src/GenHTTP.Lambda.Core/Services/UsageReports/UsageReport.cs).
