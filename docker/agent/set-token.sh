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

# An authorization code is not a token, and it is the easy mistake to make:
# the browser hands you "<code>#<state>", which goes back into the prompt that
# setup-token is waiting at - and only then does it print the token this wants.
# Written here unchecked, it lands in .env, the agent restarts on it happily,
# and the first build fails with "401 Invalid bearer token" a long way from the
# cause.
case "$TOKEN" in
  sk-ant-oat*) : ;;
  *'#'*)
    echo "That is the authorization code from the browser, not the token." >&2
    echo "Paste it back into the 'claude setup-token' prompt; what that prints" >&2
    echo "afterwards - starting sk-ant-oat - is what belongs here." >&2
    exit 1 ;;
  *)
    echo "That does not start with sk-ant-oat, so it is not what" >&2
    echo "'claude setup-token' produces. Nothing written." >&2
    exit 1 ;;
esac

# replace any line that is already there rather than stacking them up
if grep -q '^CLAUDE_CODE_OAUTH_TOKEN=' .env; then
  grep -v '^CLAUDE_CODE_OAUTH_TOKEN=' .env > .env.next
  mv .env.next .env
fi

printf 'CLAUDE_CODE_OAUTH_TOKEN=%s\n' "$TOKEN" >> .env
chmod 600 .env

echo "Written to .env. Restarting the agent on it."

docker compose -f docker-compose.yml -f docker-compose.ioxide.yml -f docker-compose.agent.yml up -d agent

# Checked in a build container rather than in the agent.
#
# The agent sits on agent-net alone and shares no network with the egress
# proxy, on purpose - it orchestrates and never talks to the model itself. A
# check run inside it fails with a DNS error however good the token is, which
# reads exactly like a rejected token and is not one.
echo "Checking a build container can reach the model..."

docker run --rm --network "${LAMBDA_AGENT_BUILD_NETWORK:-genhttp-build-net}" \
  -e "CLAUDE_CODE_OAUTH_TOKEN=$TOKEN" -e "ANTHROPIC_API_KEY=" \
  -e HTTPS_PROXY=http://egress-proxy:3128 -e HTTP_PROXY=http://egress-proxy:3128 \
  -e NO_PROXY=genhttp.dev,lambda,localhost,127.0.0.1 \
  --entrypoint sh "${LAMBDA_AGENT_IMAGE:-genhttp-agent:latest}" \
  -c 'cd "$(mktemp -d)" && claude -p "Reply with exactly: ready" --model claude-haiku-4-5-20251001 --max-turns 1 --permission-mode dontAsk' < /dev/null

echo
echo "If that said 'ready', the copied credentials no longer matter and"
echo "seed-credentials.sh is not needed again."

unset TOKEN
