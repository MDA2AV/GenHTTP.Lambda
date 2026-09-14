# syntax=docker/dockerfile:1

# The single page application, built into the web root of the server project.
FROM node:22-bookworm-slim AS frontend

WORKDIR /src/Frontend

COPY src/Frontend/package.json src/Frontend/package-lock.json ./

RUN npm ci

COPY src/Frontend/ ./

RUN npm run build

# The server itself, published as a framework dependent application.
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS backend

WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/GenHTTP.Lambda/GenHTTP.Lambda.csproj src/GenHTTP.Lambda/

RUN dotnet restore src/GenHTTP.Lambda/GenHTTP.Lambda.csproj

COPY src/GenHTTP.Lambda/ src/GenHTTP.Lambda/
COPY --from=frontend /src/GenHTTP.Lambda/wwwroot src/GenHTTP.Lambda/wwwroot

# The .NET 11 async runtime is on by default; this is the way to build without
# it, which is the first thing to try when something asynchronous misbehaves.
ARG RUNTIME_ASYNC=true

RUN dotnet publish src/GenHTTP.Lambda/GenHTTP.Lambda.csproj --no-restore -c Release -o /app \
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

# the database, the stored code and the workspaces of the lambdas
# the home directory is not decoration: .NET keeps the certificate store it
# builds TLS chains from underneath it, and a server that cannot read the
# issuers of its own certificate cannot send them
RUN useradd --uid 1001 --home-dir /home/lambda --create-home --shell /usr/sbin/nologin lambda \
 && mkdir -p /data \
 && chown -R lambda:lambda /data /app /home/lambda

USER lambda

VOLUME ["/data"]

EXPOSE 8080 8443

# a Host header is mandatory: GenHTTP answers a request without one with a 400.
# 301 counts as healthy because a configured TLS endpoint makes the server
# upgrade plain requests instead of answering them.
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s \
    CMD bash -c 'exec 3<>/dev/tcp/127.0.0.1/${LAMBDA_PORT} && printf "GET /api/v1/system HTTP/1.0\r\nHost: localhost\r\nConnection: close\r\n\r\n" >&3 && head -1 <&3 | grep -qE " (200|301) "'

ENTRYPOINT ["./GenHTTP.Lambda"]
