#!/usr/bin/env bash
#
# Copies this machine's Claude Code credentials into the agent's own volume.
#
# The agent needs its own copy rather than a read-only mount of yours: the
# OAuth token refreshes itself, and a credential it cannot write stops working
# the day it expires. Two consequences worth knowing:
#
#   - the agent's usage counts against the same subscription as your own
#     sessions, so a busy day on the landing page can throttle your own work
#   - if the refresh token rotates, this copy goes stale and has to be
#     re-seeded by running this again
#
# Run it once after first bringing the agent up, and again if builds start
# failing on authentication.
set -euo pipefail

SOURCE="${CLAUDE_CONFIG_DIR:-$HOME/.claude}/.credentials.json"

[ -f "$SOURCE" ] || { echo "No credentials at $SOURCE - log in with 'claude' first." >&2; exit 1; }

cd "$(dirname "$0")/../.."

echo "Seeding the agent's credentials from $SOURCE"

docker compose -f docker-compose.yml -f docker-compose.agent.yml exec -T agent \
  sh -c 'mkdir -p "$CLAUDE_CONFIG_DIR" && cat > "$CLAUDE_CONFIG_DIR/.credentials.json" && chmod 600 "$CLAUDE_CONFIG_DIR/.credentials.json"' \
  < "$SOURCE"

echo "Done. Checking the agent can reach the model..."

docker compose -f docker-compose.yml -f docker-compose.agent.yml exec -T agent \
  sh -lc 'cd "$(mktemp -d)" && claude -p "Reply with exactly: ready" --model claude-haiku-4-5-20251001 --max-turns 1 --permission-mode dontAsk'
