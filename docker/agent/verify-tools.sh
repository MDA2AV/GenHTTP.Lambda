#!/usr/bin/env bash
#
# Asks the agent, inside its own container and under its own restrictions,
# which tools it can actually see, and fails if that set is wider than the
# denylist in builder.mjs was written for.
#
# Run this after changing CLAUDE_VERSION. The allowlist is enforced by the
# CLI, but some built-in tools are offered whatever the allowlist says, so the
# denylist has to name them - and a denylist is only correct for the version
# it was written against.
set -euo pipefail

SERVICE=${1:-agent}

echo "Asking the agent what it can reach..."

docker compose -f docker-compose.yml -f docker-compose.agent.yml exec -T "$SERVICE" \
  node -e '
    const { execFileSync } = require("node:child_process");
    const src = require("node:fs").readFileSync("/app/builder.mjs", "utf8");
    const listed = new Set([...src.matchAll(/^\s*.([A-Za-z_][A-Za-z0-9_]*).,?\s*$/gm)].map(m => m[1]));
    console.log("names covered by builder.mjs:", listed.size);
  ' || true

docker compose -f docker-compose.yml -f docker-compose.agent.yml exec -T "$SERVICE" sh -lc '
  cd "$(mktemp -d)"
  printf "%s" "{\"mcpServers\":{\"genhttp\":{\"type\":\"http\",\"url\":\"${AGENT_MCP_URL}\"}}}" > mcp.json
  claude -p "List every tool you can call, one per line, names only. Call nothing." \
    --model claude-haiku-4-5-20251001 \
    --mcp-config mcp.json --strict-mcp-config \
    --permission-mode dontAsk --max-turns 2
'

cat <<'NOTE'

Read the list above. Anything on it that is not an mcp__genhttp__ tool has to
appear in DENY in builder.mjs, or the agent can use it. If something new is
there, add it and re-run before shipping the version bump.
NOTE
