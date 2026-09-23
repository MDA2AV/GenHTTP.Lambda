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

pull=no; check_only=no; assume_yes=no
for arg in "$@"; do
  case "$arg" in
    --pull)  pull=yes ;;
    --check) check_only=yes ;;
    -y|--yes) assume_yes=yes ;;
    -h|--help) sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
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

verify() {
  local failed=0

  local status
  status=$(docker inspect "$CONTAINER" --format '{{.State.Health.Status}}' 2>/dev/null || echo missing)
  [ "$status" = healthy ] && ok "container healthy" || { bad "container is '$status'"; failed=1; }

  local code
  code=$(curl -s --max-time 20 -o /dev/null -w '%{http_code}' "$SITE/" || echo 000)
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

say "Building and restarting (the image builds the frontend too, so this takes a few minutes)"
docker compose "${FILES[@]}" up -d --build lambda

say "Waiting for it to come back"
# Wait for the health status as well as the site, not just the site. The server
# answers HTTP well before Docker records its first health probe, so waiting on
# HTTP alone makes the check below report "container is 'starting'" on a deploy
# that is in fact perfectly fine - a false alarm every time, which is the kind
# that teaches you to ignore the real one.
for _ in $(seq 1 60); do
  code=$(curl -s --max-time 5 -o /dev/null -w '%{http_code}' "$SITE/" || echo 000)
  health=$(docker inspect "$CONTAINER" --format '{{.State.Health.Status}}' 2>/dev/null || echo missing)
  [ "$code" = 200 ] && [ "$health" = healthy ] && break
  sleep 3
done

say "Verifying"
if verify; then
  say "Deployed. $(git log --oneline -1)"
else
  say "Deployed, but the checks above did not all pass. Do not walk away from this."
  exit 1
fi
