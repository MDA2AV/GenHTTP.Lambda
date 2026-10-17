GenHTTP Lambda hosts small C# web applications. A **lambda** is a snippet of C#
that returns a [GenHTTP](https://genhttp.org/) handler - a page, an API, a
website, a websocket - plus whatever other files it needs. The platform compiles
it and serves it at an address of its own, `https://{publicKey}.{hosting domain}/`.
This API is everything the editor does: create a lambda, save its versions, put
one online, look at its data, its traffic and its logs.

## Getting started

```bash
# 1. create a lambda - keep its privateKey,
#    which cannot be recovered
curl -X POST {server}/lambdas \
  -H 'Content-Type: application/json' \
  -d '{ "acceptedTerms": true }'

# 2. save its code as a version, and put
#    it online in the same call
curl -X POST \
  '{server}/lambdas/{privateKey}/versions?deploy=true' \
  -H 'Content-Type: application/json' \
  -d '{
    "files": [ {
      "name": "lambda.cs",
      "code": "return Inline.Create().Get(() => \"Hi\");"
    } ],
    "change": "Says hi"
  }'

# 3. it answers at the address the answer
#    names (deployment.lambda.address)
```

The terms that step 1 accepts are in `GET /system`, along with this
installation's limits and the address template (`lambdaUrl`). The demos
(`GET /demos`) are finished lambdas to read, and to start from: pass a demo's
`id` as the `template` when creating a lambda.

## Who may do what

There are no accounts. A lambda has two keys:

- The **public key** names its address. It is not a secret, and its owner may
  change it.
- The **private key** (the *editor key*) is in the path of every call that reads
  or changes the lambda: `/lambdas/{privateKey}/…`. Holding it is the
  authorization. Whoever has it may change, deploy and delete the lambda, so treat
  it like a password: do not log it or put it in a URL a third party sees. It is
  handed out once, to whoever creates the lambda, and cannot be recovered.

A key that names no lambda is answered with `404`, the same way as one whose
lambda was deleted. The demos' editor keys are public. A demo can be read like
any other lambda, but every change is refused with `403`.

The operations under *Operating the installation* are for whoever runs the
server. They take the token of the installation in the `X-Admin-Token` header:
`401` without it, `403` with a wrong one. An installation without a token does not
have them at all (`404`).

## What a lambda is made of

- A **version** is the program: its code and its resources. It never changes
  once it is saved; a change is a new version. Saving one does not put it online,
  deploying does (`?deploy=true` on a save, or `POST …/deployment/start`).
  A new lambda starts with version 1, which is the template it was made from,
  and nothing is online until a version is deployed. Only so many versions are
  kept (see the limits); the oldest go first, but never the version that is
  online or one a feature is based on.
- The **data** is what the program keeps: its **database** (SQLite), its
  **workspace** (files it reads and writes) and its **secrets** (API keys,
  passwords). Data belongs to the lambda and is shared by every version.
  Deploying, rolling back and merging never touch it. Each kind of data is
  switched on before the lambda can use it (`PUT …/data/{kind}`). Switching a
  kind off deletes everything it held.
- A **feature** is a change worked on beside the lambda. It starts as a copy of
  a version and of the data, is saved in place as often as needed, has a preview
  at its own address, and becomes the next version when it is merged. Only a
  feature based on the newest version can be merged.

## Files

A version is sent and read as a list of files:

```json
{ "name": "lambda.cs", "code": "return Inline.Create()…;" }
{ "name": "resources/web/logo.png", "code": "iVBORw0KGgo…", "encoding": "base64" }
```

- `lambda.cs` comes first. It is the snippet that returns the handler.
- Every other `.cs` file, in any folder, is compiled with it.
- Files below `resources/` are the **resources**: what the lambda reads and
  serves while it runs (`Resources.App("web")` serves `resources/web/`).
- Every other file - `docs/product.md`, `tests/README.md`, the sources of a front
  end - is kept with the version and never compiled or served.
- Text is sent as it is. Anything else is sent base64 encoded with
  `"encoding": "base64"`. C# files are always text.
- Names are case sensitive, but two names that differ only by their case are
  refused, as are names that would not survive a clone or a zip on Windows. A
  refused save says which name it refused and why.

A save that only changes some files does not have to send all of them:
`POST …/versions/changes` and `POST …/features/{feature}/changes` add, replace and
remove files, or replace text inside them.

## Limits

What a version may come to (its code and resources together), the room its data
may take (database and workspace together), and how many versions and features
are kept are set by the operator of the installation, per tier, and may change.
Read them instead of assuming them: `GET /system` for a new lambda, and the
`limits` of `GET /lambdas/{privateKey}/summary` for an existing one. A save or
an upload that does not fit is refused with `400`, and a feature or a secret
more than a lambda may have with `409`. The message names the limit either
way.

The tier of a lambda (`Free`, `Premium` or `Demo`) is assigned by the operator.
A free lambda that nobody visits or edits for a while is taken offline, and
later removed. `deployedUntil` and `keptUntil` say when; both move forward with
every visit and every edit.

## Answers and errors

Requests and answers are JSON unless an operation says otherwise. Field names are
camelCase, times are ISO 8601 in UTC, and sizes are in bytes. Where a body is
optional, or a field is left out of a `PATCH`, what is left out stays as it is.

A request that is refused is answered with a status code and a JSON body:

```json
{
  "status": 409,
  "error": "Conflict",
  "message": "This key is already in use."
}
```

- `400` **Invalid**: the request cannot be carried out as it is. The message says
  what to change.
- `403` **Forbidden**: not for this lambda - it is a demo, or its tier does not
  include what was asked for.
- `404` **NotFound**: the lambda, version, feature, file or table does not exist.
- `409` **Conflict**: it collides with the state of the lambda - a key or a domain
  that is taken, a feature that is behind the newest version, a save based on a
  state that changed in the meantime. Read again, then retry.
- `429` **TooMany**: asked for too often lately. Try again later.

The `message` is written for a person or an agent to act on. Pass it on rather
than replacing it. Code that does not compile is not an error of the request:
deploying, previewing and merging answer it with `422` and the compiler's
`diagnostics`, and leave what was online as it was.

A request that fails before the platform's own checks - a body that is not valid
JSON, a build agent that cannot be reached - is answered with the framework's
error body, which has `status` and `message` but no `error`.

## Other ways in

The same platform is available to AI agents as an MCP server at `/mcp`. Every
lambda is also a git repository: clone it from the `gitPath` of the lambda, push
to `main` to save versions, and push a branch to start a feature.
