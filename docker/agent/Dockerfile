# Two things are built from this image, and the difference between them is the
# whole of the security story.
#
#   the runner    long lived, runs builder.mjs, holds the docker socket, and
#                 never executes a line the model wrote
#   a build       one per request, started by the runner, runs nothing but the
#                 CLI, holds no socket, and is thrown away when it ends
#
# It used to be one container doing both. That meant a prompt from a stranger
# ran in the same filesystem as the thing orchestrating it, sharing /tmp, the
# configuration directory and whatever the last build left behind - and it is
# why the socket could not be given to it. Split, the socket goes to the half
# that only ever runs code from this repository.
#
# The CLI version is pinned deliberately. The tool allowlist is enforced by
# the CLI, but a handful of built-in tools are available whatever the
# allowlist says - messaging other sessions on the machine was one of them -
# so they are named in a denylist as well, and a denylist is only correct for
# the version it was written against. Upgrading means re-running
# verify-tools.sh and reading what it prints before changing this number.
FROM node:22-slim

ARG CLAUDE_VERSION=2.1.280

RUN apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates curl gnupg \
 && install -m 0755 -d /etc/apt/keyrings \
 && curl -fsSL https://download.docker.com/linux/debian/gpg -o /etc/apt/keyrings/docker.asc \
 && chmod a+r /etc/apt/keyrings/docker.asc \
 && echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/debian $(. /etc/os-release && echo $VERSION_CODENAME) stable" > /etc/apt/sources.list.d/docker.list \
 && apt-get update \
 && apt-get install -y --no-install-recommends docker-ce-cli \
 && apt-get purge -y gnupg \
 && apt-get autoremove -y \
 && rm -rf /var/lib/apt/lists/*

RUN npm install -g @anthropic-ai/claude-code@${CLAUDE_VERSION} \
 && npm cache clean --force

# nothing here runs as root
RUN useradd --create-home --uid 1002 builder

ENV CLAUDE_CONFIG_DIR=/home/builder/.claude \
    HOME=/home/builder \
    XDG_RUNTIME_DIR=/home/builder/run \
    NODE_ENV=production

RUN mkdir -p /home/builder/.claude /home/builder/run /home/builder/jobs \
 && chown -R builder:builder /home/builder

COPY --chown=builder:builder builder.mjs /app/builder.mjs
COPY --chown=builder:builder proxy.mjs /app/proxy.mjs
# what a build is told about the room it is in; the runner writes it into each
# container's working directory so the CLI picks it up as project context
COPY --chown=builder:builder AGENTS.md /app/AGENTS.md

USER builder
WORKDIR /home/builder

EXPOSE 8401

CMD ["node", "/app/builder.mjs"]
