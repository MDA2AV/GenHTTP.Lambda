# syntax=docker/dockerfile:1

# The community edition, published as genhttp/lambda: every component, with the
# lambdas below /lambda/ on the one address the installation is run at (or at
# subdomains, given LAMBDA_HOSTING_URL), and a front page of its own. See
# samples/community for how to run it.
#
# Built from the root of the repository:
#   docker build -f src/GenHTTP.Lambda.Community/Dockerfile .

# The bundles of the components with pages, from the npm workspace at the
# root: the kit they share, and each component's own frontend. Built where the
# image is built, whatever it is for - a bundle is the same on every machine.
FROM --platform=$BUILDPLATFORM node:26-trixie-slim AS frontend

WORKDIR /repo

COPY package.json package-lock.json tsconfig.base.json ./
COPY src/Kit/package.json src/Kit/
COPY src/GenHTTP.Lambda.Editing/Frontend/package.json src/GenHTTP.Lambda.Editing/Frontend/
COPY src/GenHTTP.Lambda.Administration/Frontend/package.json src/GenHTTP.Lambda.Administration/Frontend/
COPY src/GenHTTP.Lambda.Sources/Frontend/package.json src/GenHTTP.Lambda.Sources/Frontend/
COPY src/GenHTTP.Lambda.Cloud/Frontend/package.json src/GenHTTP.Lambda.Cloud/Frontend/
COPY src/GenHTTP.Lambda.Community/Frontend/package.json src/GenHTTP.Lambda.Community/Frontend/

RUN npm ci

COPY src/Kit/ src/Kit/
COPY src/GenHTTP.Lambda.Editing/Frontend/ src/GenHTTP.Lambda.Editing/Frontend/
COPY src/GenHTTP.Lambda.Administration/Frontend/ src/GenHTTP.Lambda.Administration/Frontend/
COPY src/GenHTTP.Lambda.Sources/Frontend/ src/GenHTTP.Lambda.Sources/Frontend/
COPY src/GenHTTP.Lambda.Community/Frontend/ src/GenHTTP.Lambda.Community/Frontend/

RUN npm run build -w @genhttp-lambda/editor -w @genhttp-lambda/admin -w @genhttp-lambda/sources -w @genhttp-lambda/community

# The server, published as a framework dependent application - portable, so
# one build serves every architecture the image is made for.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0 AS backend

WORKDIR /repo

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Directory.Build.props src/BannedSymbols.txt src/
COPY src/GenHTTP.Lambda.Integrations/GenHTTP.Lambda.Integrations.csproj src/GenHTTP.Lambda.Integrations/
COPY src/GenHTTP.Lambda.Integrations.Sqlite/GenHTTP.Lambda.Integrations.Sqlite.csproj src/GenHTTP.Lambda.Integrations.Sqlite/
COPY src/GenHTTP.Lambda.Integrations.Local/GenHTTP.Lambda.Integrations.Local.csproj src/GenHTTP.Lambda.Integrations.Local/
COPY src/GenHTTP.Lambda.Integrations.PostgreSql/GenHTTP.Lambda.Integrations.PostgreSql.csproj src/GenHTTP.Lambda.Integrations.PostgreSql/
COPY src/GenHTTP.Lambda.Core/GenHTTP.Lambda.Core.csproj src/GenHTTP.Lambda.Core/
COPY src/GenHTTP.Lambda.Web/GenHTTP.Lambda.Web.csproj src/GenHTTP.Lambda.Web/
COPY src/GenHTTP.Lambda.Execution/GenHTTP.Lambda.Execution.csproj src/GenHTTP.Lambda.Execution/
COPY src/GenHTTP.Lambda.Api/GenHTTP.Lambda.Api.csproj src/GenHTTP.Lambda.Api/
COPY src/GenHTTP.Lambda.Mcp/GenHTTP.Lambda.Mcp.csproj src/GenHTTP.Lambda.Mcp/
COPY src/GenHTTP.Lambda.Server/GenHTTP.Lambda.Server.csproj src/GenHTTP.Lambda.Server/
COPY src/GenHTTP.Lambda.Editing/GenHTTP.Lambda.Editing.csproj src/GenHTTP.Lambda.Editing/
COPY src/GenHTTP.Lambda.Administration/GenHTTP.Lambda.Administration.csproj src/GenHTTP.Lambda.Administration/
COPY src/GenHTTP.Lambda.Sources/GenHTTP.Lambda.Sources.csproj src/GenHTTP.Lambda.Sources/
COPY src/GenHTTP.Lambda.Community/GenHTTP.Lambda.Community.csproj src/GenHTTP.Lambda.Community/

RUN dotnet restore src/GenHTTP.Lambda.Community/GenHTTP.Lambda.Community.csproj

COPY src/ src/
COPY --from=frontend /repo/src/GenHTTP.Lambda.Editing/bundle src/GenHTTP.Lambda.Editing/bundle
COPY --from=frontend /repo/src/GenHTTP.Lambda.Administration/bundle src/GenHTTP.Lambda.Administration/bundle
COPY --from=frontend /repo/src/GenHTTP.Lambda.Sources/bundle src/GenHTTP.Lambda.Sources/bundle
COPY --from=frontend /repo/src/GenHTTP.Lambda.Community/bundle src/GenHTTP.Lambda.Community/bundle

# The .NET 11 async runtime is on by default; this is the way to build without
# it, which is the first thing to try when something asynchronous misbehaves.
ARG RUNTIME_ASYNC=true

# The version of the release (the workflow passes it), which the server reports
# as its own; without, it is the one in Directory.Build.props.
ARG VERSION=

RUN dotnet publish src/GenHTTP.Lambda.Community/GenHTTP.Lambda.Community.csproj --no-restore -c Release -o /app \
    -p:RuntimeAsync=$RUNTIME_ASYNC ${VERSION:+-p:Version=$VERSION}

# The runtime the server ships on, chiseled: no shell, no package manager,
# nothing but what .NET needs to run - with ICU and the time zones (-extra),
# for the languages the pages speak and the clocks the platform reads. Named
# here so that the next stage makes its files for it.
FROM mcr.microsoft.com/dotnet/aspnet:11.0-resolute-chiseled-extra AS runtime

# What the image would make with a shell, which it has none of: the user the
# server runs as - uid and gid 1001, whom the volumes of installations from
# before belong to, and the group certificates are made readable for - its
# home, and the folder of the data.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0 AS rootfs

COPY --from=runtime /etc/passwd /etc/group /rootfs/etc/

# the database, the stored code and the workspaces of the lambdas - and
# dumps/, where the runtime writes a dump if the process dies (see
# deploy/cloud/docker-compose.yml): createdump will not make the folder itself, and a
# volume only takes it from the image when the volume is new
# the home directory is not decoration: .NET keeps the certificate store it
# builds TLS chains from underneath it, and a server that cannot read the
# issuers of its own certificate cannot send them
RUN echo "lambda:x:1001:1001::/home/lambda:/usr/sbin/nologin" >> /rootfs/etc/passwd \
 && echo "lambda:x:1001:" >> /rootfs/etc/group \
 && mkdir -p /rootfs/data/dumps /rootfs/home/lambda

# What is actually shipped. The ASP.NET Core framework is required because
# GenHTTP.Full ships the Kestrel engine next to the ioxide one; lambdas are
# compiled with Roslyn at runtime and reference the assemblies found here.
FROM runtime

# GenHTTP's internal engine rather than the (faster) io_uring one: container
# runtimes block io_uring in their default seccomp profile, and a platform that
# runs code written by strangers is the last place to hand out a weaker one.
# Set LAMBDA_ENGINE=ioxide where io_uring is available and allowed, or kestrel
# for ASP.NET Core's.
#
# No LAMBDA_PUBLIC_URL: an installation that names its address answers at that
# name only, so one reached by its IP address before it was set would answer
# nothing. Without it every name is the platform's, and the addresses it hands
# out read localhost until the operator says where it is reached.
ENV HOME=/home/lambda \
    LAMBDA_PORT=8080 \
    LAMBDA_DATA_DIRECTORY=/data \
    LAMBDA_ENGINE=internal \
    DOTNET_gcServer=1

WORKDIR /app

COPY --link --from=rootfs /rootfs/etc/passwd /rootfs/etc/group /etc/
COPY --link --from=rootfs --chown=1001:1001 /rootfs/data /data
COPY --link --from=rootfs --chown=1001:1001 /rootfs/home/lambda /home/lambda

# the server, owned by root: it reads what is here and never writes beside
# itself, so the user it runs as cannot change it either. Copied last and
# once - changing the owner after copying wrote every file into a second
# layer, the size of the application again.
COPY --link --from=backend /app /app/

USER lambda

VOLUME ["/data"]

EXPOSE 8080 8443

# no shell to ask with: the server's own binary asks the server, and any
# answer below a server error is healthy - a redirect where TLS is set up, a
# 401 where a host puts the platform behind a login
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s \
    CMD ["/app/GenHTTP.Lambda.Community", "--health"]

# set LAMBDA_PUBLIC_URL to where the installation is reached, and
# LAMBDA_ADMIN_TOKEN to use the panel at /admin
ENTRYPOINT ["./GenHTTP.Lambda.Community"]
