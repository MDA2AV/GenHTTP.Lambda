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

# Checked in a build container rather than in the agent.
#
# The agent sits on agent-net alone and shares no network with the egress
# proxy, on purpose - it orchestrates and never talks to the model itself. A
# check run inside it fails with a DNS error however good the token is, which
# reads exactly like a rejected token and is not one.
echo "Checking a build container can reach the model..."

docker run --rm --network "${LAMBDA_AGENT_BUILD_NETWORK:-genhttp-build-net}" \
  -e CLAUDE_CODE_OAUTH_TOKEN -e "ANTHROPIC_API_KEY=" \
  -e HTTPS_PROXY=http://egress-proxy:3128 -e HTTP_PROXY=http://egress-proxy:3128 \
  -e NO_PROXY=genhttp.dev,lambda,localhost,127.0.0.1 \
  --entrypoint sh "${LAMBDA_AGENT_IMAGE:-genhttp-agent:latest}" \
  -c 'cd "$(mktemp -d)" && claude -p "Reply with exactly: ready" --model claude-haiku-4-5-20251001 --max-turns 1 --permission-mode dontAsk' < /dev/null
