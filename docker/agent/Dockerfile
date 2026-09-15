# The build agent, in a box of its own.
#
# This runs Claude Code against whatever a stranger typed into a text box on
# the landing page, so the interesting part of it is not what it can do but
# what it cannot. It holds no source, no secrets beyond its own credentials,
# no docker socket and no route to the internet except through the proxy
# beside it. The only thing it can reach that matters is the MCP endpoint of
# the lambda server, which is exactly the capability it is meant to have:
# create a lambda, write code into it, deploy it.
#
# The CLI version is pinned deliberately. The tool allowlist is enforced by
# the CLI, but a handful of built-in tools are available whatever the
# allowlist says - messaging other sessions on the machine was one of them -
# so they are named in a denylist as well, and a denylist is only correct for
# the version it was written against. Upgrading means re-running
# verify-tools.sh and reading what it prints before changing this number.
FROM node:22-slim

ARG CLAUDE_VERSION=2.1.236

RUN npm install -g @anthropic-ai/claude-code@${CLAUDE_VERSION} \
 && npm cache clean --force

# nothing here runs as root
RUN useradd --create-home --uid 1002 builder

# its own configuration directory, on a volume, because the OAuth token
# refreshes itself and a read-only credential stops working the moment it
# expires
ENV CLAUDE_CONFIG_DIR=/home/builder/.claude \
    HOME=/home/builder \
    XDG_RUNTIME_DIR=/home/builder/run \
    NODE_ENV=production

RUN mkdir -p /home/builder/.claude /home/builder/run /home/builder/jobs \
 && chown -R builder:builder /home/builder

COPY --chown=builder:builder builder.mjs /app/builder.mjs
COPY --chown=builder:builder proxy.mjs /app/proxy.mjs

USER builder
WORKDIR /home/builder

EXPOSE 8401

CMD ["node", "/app/builder.mjs"]
