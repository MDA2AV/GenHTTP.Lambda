# syntax=docker/dockerfile:1

# genhttp.dev: every component, with the lambdas at subdomains of LAMBDA_HOSTING_URL
# and the pages of the site that are its own. Started by docker-compose.yml.
#
# Built from the root of the repository:
#   docker build -f src/GenHTTP.Lambda.Cloud/Dockerfile .

# The bundles of the components with pages, from the npm workspace at the
# root: the kit they share, and each component's own frontend. Built where the
# image is built, whatever it is for - a bundle is the same on every machine.
FROM --platform=$BUILDPLATFORM node:22-bookworm-slim AS frontend

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
COPY src/GenHTTP.Lambda.Cloud/Frontend/ src/GenHTTP.Lambda.Cloud/Frontend/

RUN npm run build -w @genhttp-lambda/editor -w @genhttp-lambda/admin -w @genhttp-lambda/sources -w @genhttp-lambda/cloud

# The server, published as a framework dependent application - portable, so
# one build serves every architecture the image is made for.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0 AS backend

WORKDIR /repo

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Directory.Build.props src/BannedSymbols.txt src/
COPY src/GenHTTP.Lambda.Core/GenHTTP.Lambda.Core.csproj src/GenHTTP.Lambda.Core/
COPY src/GenHTTP.Lambda.Web/GenHTTP.Lambda.Web.csproj src/GenHTTP.Lambda.Web/
COPY src/GenHTTP.Lambda.Execution/GenHTTP.Lambda.Execution.csproj src/GenHTTP.Lambda.Execution/
COPY src/GenHTTP.Lambda.Api/GenHTTP.Lambda.Api.csproj src/GenHTTP.Lambda.Api/
COPY src/GenHTTP.Lambda.Mcp/GenHTTP.Lambda.Mcp.csproj src/GenHTTP.Lambda.Mcp/
COPY src/GenHTTP.Lambda.Server/GenHTTP.Lambda.Server.csproj src/GenHTTP.Lambda.Server/
COPY src/GenHTTP.Lambda.Editing/GenHTTP.Lambda.Editing.csproj src/GenHTTP.Lambda.Editing/
COPY src/GenHTTP.Lambda.Administration/GenHTTP.Lambda.Administration.csproj src/GenHTTP.Lambda.Administration/
COPY src/GenHTTP.Lambda.Sources/GenHTTP.Lambda.Sources.csproj src/GenHTTP.Lambda.Sources/
COPY src/GenHTTP.Lambda.Cloud/GenHTTP.Lambda.Cloud.csproj src/GenHTTP.Lambda.Cloud/

RUN dotnet restore src/GenHTTP.Lambda.Cloud/GenHTTP.Lambda.Cloud.csproj

COPY src/ src/
COPY --from=frontend /repo/src/GenHTTP.Lambda.Editing/bundle src/GenHTTP.Lambda.Editing/bundle
COPY --from=frontend /repo/src/GenHTTP.Lambda.Administration/bundle src/GenHTTP.Lambda.Administration/bundle
COPY --from=frontend /repo/src/GenHTTP.Lambda.Sources/bundle src/GenHTTP.Lambda.Sources/bundle
COPY --from=frontend /repo/src/GenHTTP.Lambda.Cloud/bundle src/GenHTTP.Lambda.Cloud/bundle

# The .NET 11 async runtime is on by default; this is the way to build without
# it, which is the first thing to try when something asynchronous misbehaves.
ARG RUNTIME_ASYNC=true

RUN dotnet publish src/GenHTTP.Lambda.Cloud/GenHTTP.Lambda.Cloud.csproj --no-restore -c Release -o /app \
    -p:RuntimeAsync=$RUNTIME_ASYNC

# What is actually shipped. The ASP.NET Core framework is required because
# GenHTTP.Full ships the Kestrel engine next to the ioxide one; lambdas are
# compiled with Roslyn at runtime and reference the assemblies found here.
FROM mcr.microsoft.com/dotnet/aspnet:11.0

# Kestrel rather than the (faster) io_uring engine: container runtimes block
# io_uring in their default seccomp profile, and a platform that runs code
# written by strangers is the last place to hand out a weaker one. Set
# LAMBDA_ENGINE=ioxide where io_uring is available and allowed.
ENV HOME=/home/lambda \
    LAMBDA_PORT=8080 \
    LAMBDA_DATA_DIRECTORY=/data \
    LAMBDA_ENGINE=kestrel \
    DOTNET_gcServer=1

WORKDIR /app

COPY --from=backend /app ./

# the database, the stored code and the workspaces of the lambdas - and
# dumps/, where the runtime writes a dump if the process dies (see
# docker-compose.ioxide.yml): createdump will not make the folder itself, and a
# volume only takes it from the image when the volume is new
# the home directory is not decoration: .NET keeps the certificate store it
# builds TLS chains from underneath it, and a server that cannot read the
# issuers of its own certificate cannot send them
RUN useradd --uid 1001 --home-dir /home/lambda --create-home --shell /usr/sbin/nologin lambda \
 && mkdir -p /data/dumps \
 && chown -R lambda:lambda /data /app /home/lambda

USER lambda

VOLUME ["/data"]

EXPOSE 8080 8443

# a Host header is mandatory: GenHTTP answers a request without one with a 400.
# 301 counts as healthy because a configured TLS endpoint makes the server
# upgrade plain requests instead of answering them.
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s \
    CMD bash -c 'exec 3<>/dev/tcp/127.0.0.1/${LAMBDA_PORT} && printf "GET /api/v1/system HTTP/1.0\r\nHost: localhost\r\nConnection: close\r\n\r\n" >&3 && head -1 <&3 | grep -qE " (200|301) "'

ENTRYPOINT ["./GenHTTP.Lambda.Cloud"]
