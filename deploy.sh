#!/usr/bin/env bash
#
# Rebuilds and restarts the GenHTTP.Lambda server.
#
# There is one reason this script exists rather than a line in a README: the
# stack is spread over three compose files, and leaving one out does not fail.
# It silently produces a half-configured server - drop the agent overlay and the
# site comes back looking healthy while /build reports "no build agent"; drop
# the ioxide overlay and it quietly falls back to the slower engine. So the file
# list lives here, once, and the checks at the end prove the result rather than
# assuming it.
#
# The build agent is rebuilt with the server, because the two change together:
# the runner reads the answers of the server's tools, and a runner left behind
# reads them as they used to be. Its container is only recreated when its image
# changed, and only once the builds running at that moment are done - they live
# in the runner, and recreating it would lose them.
#
#   sudo genhttp-deploy            rebuild and restart what is on disk
#   sudo genhttp-deploy --pull     fetch origin/main first, then do that
#   sudo genhttp-deploy --check    verify the running server, change nothing
#   sudo genhttp-deploy -y         skip the "this disconnects people" prompt
#
set -euo pipefail

REPO=/root/GenHTTP.Lambda
SITE=https://genhttp.dev
CONTAINER=genhttplambda-lambda-1
FILES=(-f docker-compose.yml -f docker-compose.ioxide.yml -f docker-compose.agent.yml)

# the image the runner, the egress proxy and every build are started from
AGENT_IMAGE=genhttp-agent:latest

# how long to wait for running builds before recreating the runner anyway: a
# build is killed by the runner after ten minutes, so this is past every one
BUILD_WAIT_SECONDS=660

pull=no; check_only=no; assume_yes=no
for arg in "$@"; do
  case "$arg" in
    --pull)  pull=yes ;;
    --check) check_only=yes ;;
    -y|--yes) assume_yes=yes ;;
    -h|--help) sed -n '2,22p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "unknown option: $arg (try --help)" >&2; exit 2 ;;
  esac
done

if [ "$(id -u)" -ne 0 ]; then
  echo "This needs root, for docker and for the repo under /root." >&2
  echo "Run:  sudo $(basename "$0") $*" >&2
  exit 1
fi

cd "$REPO"

say()  { printf '\n\033[1m%s\033[0m\n' "$*"; }
ok()   { printf '  \033[32mok\033[0m   %s\n' "$*"; }
bad()  { printf '  \033[31mFAIL\033[0m %s\n' "$*"; }

# the image the runner was started from, and the one built last; they differ
# after a build that changed something, until the runner is recreated
runner_image() {
  local id
  id=$(docker compose "${FILES[@]}" ps -q agent 2>/dev/null || true)
  [ -n "$id" ] && docker inspect "$id" --format '{{.Image}}' 2>/dev/null || echo none
}

built_image() { docker image inspect "$AGENT_IMAGE" --format '{{.Id}}' 2>/dev/null || echo none; }

# the containers the runner started for builds, which it names build-<job>
running_builds() { docker ps --format '{{.Names}}' | grep -c '^build-' || true; }

verify() {
  local failed=0

  local status
  status=$(docker inspect "$CONTAINER" --format '{{.State.Health.Status}}' 2>/dev/null || echo missing)
  [ "$status" = healthy ] && ok "container healthy" || { bad "container is '$status'"; failed=1; }

  # -L because the root answers with a redirect to the visitor's language
  # (/ -> /en); without it this is a 302 on a perfectly healthy site.
  local code
  code=$(curl -sL --max-time 20 -o /dev/null -w '%{http_code}' "$SITE/" || echo 000)
  [ "$code" = 200 ] && ok "site answering (HTTP $code)" || { bad "site returned $code"; failed=1; }

  # The check that catches a missing agent overlay, which is otherwise
  # invisible. Availability moved from /build to /system when the API was
  # reorganised, so both are tried - this script has to keep working across a
  # deploy that is itself the thing moving the endpoint.
  local build
  build=$(curl -s --max-time 20 "$SITE/api/v1/system" || echo '{}')
  grep -q '"available"' <<<"$build" || build=$(curl -s --max-time 20 "$SITE/api/v1/build" || echo '{}')
  if grep -q '"available":true' <<<"$build"; then
    ok "build agent reachable"
  elif grep -q '"available"' <<<"$build"; then
    bad "build agent NOT configured - the agent compose file was probably missed"
    failed=1
  else
    bad "could not read build availability from /system or /build"
    printf '       %s\n' "${build:0:160}"
    failed=1
  fi

  # A runner older than the image built last is a deploy that stopped halfway:
  # the server is new and the runner reads its answers the old way.
  local runner
  runner=$(runner_image)
  if [ "$runner" = none ]; then
    bad "build agent's runner is not running"
    failed=1
  elif [ "$runner" = "$(built_image)" ]; then
    ok "build agent's runner is on the newest image"
  else
    bad "build agent's runner is older than $AGENT_IMAGE - deploy again to recreate it"
    failed=1
  fi

  local engine
  engine=$(docker inspect "$CONTAINER" --format '{{range .Config.Env}}{{println .}}{{end}}' 2>/dev/null \
           | grep -i '^LAMBDA_ENGINE=' | cut -d= -f2 || true)
  [ -n "$engine" ] && ok "engine: $engine" || printf '  ---- engine not pinned in env (compose default)\n'

  return $failed
}

if [ "$check_only" = yes ]; then
  say "Checking the running server"
  verify && { say "All good."; exit 0; } || { say "Something is wrong - see above."; exit 1; }
fi

say "About to redeploy"
printf '  repo:   %s\n' "$REPO"
printf '  commit: %s\n' "$(git log --oneline -1 2>/dev/null || echo 'not a git checkout')"
[ "$pull" = yes ] && printf '  will fetch origin/main first\n'
printf '\n  \033[33mThis restarts the server and drops every open connection.\033[0m\n'
printf '  Anyone using a hosted lambda right now - including arena players - is disconnected.\n'
printf '  The build agent is rebuilt as well; running builds are waited for, not cut off.\n'

if [ "$assume_yes" != yes ]; then
  read -rp $'\n  Continue? [y/N] ' answer
  [[ "$answer" =~ ^[Yy]$ ]] || { echo "  Nothing changed."; exit 0; }
fi

if [ "$pull" = yes ]; then
  say "Fetching origin/main"
  git fetch --quiet origin main
  git merge --ff-only origin/main
  printf '  now at: %s\n' "$(git log --oneline -1)"
fi

# Both images are built before anything restarts, so the server is down for
# its restart and not for a build as well. Building touches nothing running:
# the runner keeps the image it was started from.
say "Building the server and the build agent (the server's image builds the frontend too, so this takes a few minutes)"
docker compose "${FILES[@]}" build lambda agent

say "Restarting the server"
docker compose "${FILES[@]}" up -d lambda

say "Waiting for it to come back"
# Wait for the health status as well as the site, not just the site. The server
# answers HTTP well before Docker records its first health probe, so waiting on
# HTTP alone makes the check below report "container is 'starting'" on a deploy
# that is in fact perfectly fine - a false alarm every time, which is the kind
# that teaches you to ignore the real one.
for _ in $(seq 1 60); do
  code=$(curl -sL --max-time 5 -o /dev/null -w '%{http_code}' "$SITE/" || echo 000)
  health=$(docker inspect "$CONTAINER" --format '{{.State.Health.Status}}' 2>/dev/null || echo missing)
  [ "$code" = 200 ] && [ "$health" = healthy ] && break
  sleep 3
done

# A build that is running lives in the runner - its steps, its result, the page
# waiting for it - and in a container the runner started. Recreating the runner
# loses the first and orphans the second, so the builds are let finish. New
# builds use the new image at once, whatever the runner is.
if [ "$(runner_image)" = "$(built_image)" ]; then
  say "The build agent did not change"
else
  waited=0
  while [ "$(running_builds)" -gt 0 ] && [ "$waited" -lt "$BUILD_WAIT_SECONDS" ]; do
    [ $((waited % 30)) -eq 0 ] && say "Waiting for $(running_builds) running build(s) before restarting the build agent"
    sleep 5
    waited=$((waited + 5))
  done

  say "Restarting the build agent"
  docker compose "${FILES[@]}" up -d agent egress-proxy
fi

say "Verifying"
if verify; then
  say "Deployed. $(git log --oneline -1)"
else
  say "Deployed, but the checks above did not all pass. Do not walk away from this."
  exit 1
fi
