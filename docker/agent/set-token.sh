#!/usr/bin/env bash
#
# Gives the agent a long-lived token of its own, so it stops living on a copy
# of this machine's login.
#
# A copy cannot last: both sides refresh, a refresh rotates the refresh token,
# and whichever goes first revokes the other. In practice that is hours, and
# it fails in front of whoever is using the build page. A token from
# "claude setup-token" does not refresh and does not rotate anything, so the
# two stop fighting.
#
# It spends the same subscription as your own sessions - that is the point of
# using it rather than an API key - so a busy day on the build page can
# throttle your own work.
#
#   claude setup-token     # somewhere with a browser; it prints a token
#   ./docker/agent/set-token.sh
#
# The token is read without echo and written straight to .env, which is
# gitignored. It is never printed.
set -euo pipefail

cd "$(dirname "$0")/../.."

[ -f .env ] || { echo "No .env here." >&2; exit 1; }

printf 'Paste the token from "claude setup-token" (it will not be shown): '
read -rs TOKEN
printf '\n'

[ -n "$TOKEN" ] || { echo "Nothing pasted; leaving .env alone." >&2; exit 1; }

case "$TOKEN" in
  *[![:print:]]*) echo "That does not look like a token." >&2; exit 1 ;;
esac

# replace any line that is already there rather than stacking them up
if grep -q '^CLAUDE_CODE_OAUTH_TOKEN=' .env; then
  grep -v '^CLAUDE_CODE_OAUTH_TOKEN=' .env > .env.next
  mv .env.next .env
fi

printf 'CLAUDE_CODE_OAUTH_TOKEN=%s\n' "$TOKEN" >> .env
chmod 600 .env
unset TOKEN

echo "Written to .env. Restarting the agent on it."

docker compose -f docker-compose.yml -f docker-compose.ioxide.yml -f docker-compose.agent.yml up -d agent

echo "Checking it can reach the model..."

docker compose -f docker-compose.yml -f docker-compose.agent.yml exec -T agent \
  sh -lc 'cd "$(mktemp -d)" && claude -p "Reply with exactly: ready" --model claude-haiku-4-5-20251001 --max-turns 1 --permission-mode dontAsk' < /dev/null

echo
echo "If that said 'ready', the copied credentials no longer matter and"
echo "seed-credentials.sh is not needed again."
