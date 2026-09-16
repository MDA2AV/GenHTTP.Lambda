#!/usr/bin/env bash
#
# Copies this machine's Claude Code credentials into the agent's own volume.
#
# This is the stop-gap, not the answer.
#
# A copy of your login cannot last. Both sides refresh, and a refresh rotates
# the refresh token, so whichever of you goes first revokes the other. In
# practice that is hours, and the failure lands on whoever is using the build
# page at the time.
#
# The answer is a credential of the agent's own, which needs no copying and
# fights with nothing:
#
#     claude setup-token                       # then put it in .env as
#     CLAUDE_CODE_OAUTH_TOKEN=...              # and restart the agent
#
# or an ANTHROPIC_API_KEY, which bills the API account rather than the
# subscription. Either is picked up from .env by docker-compose.agent.yml.
#
# Until then, run this whenever builds start failing on authentication. Note
# that the agent's usage counts against the same subscription as your own
# sessions while it shares your login.
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
