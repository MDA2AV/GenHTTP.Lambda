#!/usr/bin/env bash
#
# Backs genhttp.dev up to a Nextcloud, over WebDAV: the server's volume, its
# databases taken while it runs, deduplicated and encrypted by restic before
# anything leaves the machine. genhttp-backup.timer runs it nightly.
#
#   sudo genhttp-backup                     take a snapshot now (what the timer runs)
#   sudo genhttp-backup --setup             the Nextcloud account and the repository, once
#   sudo genhttp-backup --list              the snapshots that are kept
#   sudo genhttp-backup --check [subset]    read a part of the repository back (5%)
#   sudo genhttp-backup --restore [id]      restore into a new volume, the server untouched
#   sudo genhttp-backup --keys              print what has to be kept apart from the server
#
# Two things are not in the repository and have to be kept somewhere else, a
# password manager say: the repository's password (RESTIC_PASSWORD), without
# which nothing in it can be read, and the installation's secrets.key, without
# which every secret of every lambda is gone while the rest restores. --keys
# prints both.
set -euo pipefail

HERE=$(dirname "$(readlink -f "$0")")
COMPOSE=(docker compose -f "$HERE/docker-compose.yml" --profile backup)
VOLUME=genhttplambda_lambda-data

say() { printf '\n\033[1m%s\033[0m\n' "$*"; }

if [ "$(id -u)" -ne 0 ]; then
  echo "This needs root, for docker and for .env." >&2
  echo "Run:  sudo $(basename "$0") $*" >&2
  exit 1
fi

[ -f "$HERE/.env" ] || { echo "No .env beside $HERE/docker-compose.yml." >&2; exit 1; }

# a value of .env, without sourcing a file other things write to
setting() { grep -E "^$1=" "$HERE/.env" | tail -n1 | cut -d= -f2- || true; }

# writes a value into .env, replacing the line that is there
remember() {
  grep -v "^$1=" "$HERE/.env" > "$HERE/.env.next" || true
  printf '%s=%s\n' "$1" "$2" >> "$HERE/.env.next"
  mv "$HERE/.env.next" "$HERE/.env"
  chmod 600 "$HERE/.env"
}

# Built apart and quietly, not with run --build: that writes the build's
# progress to stdout whenever there is no terminal, into whatever reads it.
build() { "${COMPOSE[@]}" build --quiet backup; }

run() { build && "${COMPOSE[@]}" run --rm backup "$@"; }

configured() {
  for name in NEXTCLOUD_URL NEXTCLOUD_USER NEXTCLOUD_PASS RESTIC_PASSWORD; do
    [ -n "$(setting "$name")" ] || { echo "$name is not set in .env - run --setup first." >&2; exit 1; }
  done
}

setup() {
  say "The Nextcloud account the backups go to"
  cat <<'EOF'
  Use an account of its own, or a folder no person syncs: the repository is
  restic's, nothing in it is readable as files, and a desktop client syncing it
  only moves it around. Make an app password for it - Settings, Security,
  Devices & sessions - rather than giving the account's own.
EOF
  read -rp $'\n  Address of the Nextcloud (https://cloud.example.org): ' base
  read -rp '  User: ' user
  read -rsp '  App password (not shown): ' pass; printf '\n'
  read -rp '  Folder for the repository [genhttp-dev-backup]: ' folder

  [ -n "$base" ] && [ -n "$user" ] && [ -n "$pass" ] || { echo "  Nothing written." >&2; exit 1; }

  # rclone takes the password obscured, not in plain - which is no
  # encryption, only keeps it from being read over a shoulder in .env. Asked
  # of the image directly, since compose adds lines of its own to stdout.
  build
  obscured=$(docker run --rm -i --entrypoint rclone genhttp-backup:latest obscure - <<<"$pass")

  # what rclone prints is base64 for addresses; anything else is not it
  [[ "$obscured" =~ ^[A-Za-z0-9_-]+$ ]] || { echo "  Could not obscure the password; nothing written." >&2; exit 1; }

  remember NEXTCLOUD_URL "${base%/}/remote.php/dav/files/$user"
  remember NEXTCLOUD_USER "$user"
  remember NEXTCLOUD_PASS "$obscured"
  remember BACKUP_FOLDER "${folder:-genhttp-dev-backup}"
  unset pass obscured

  if [ -z "$(setting RESTIC_PASSWORD)" ]; then
    remember RESTIC_PASSWORD "$(openssl rand -base64 33)"
    say "Made a password for the repository"
  fi

  say "Making the repository"
  if ! run init; then
    cat >&2 <<EOF

  The repository was not made; what is in .env stays, so --setup can simply be
  run again. If the Nextcloud answered 401 or 404: the address takes the
  account's user id, which need not be the name signed in with - the Nextcloud
  shows the exact WebDAV address in Files, Files settings (bottom left).
  Written now: $(setting NEXTCLOUD_URL)
EOF
    exit 1
  fi

  say "Done. Now keep the two keys apart from this machine:"
  printf '  sudo %s --keys\n' "$(basename "$0")"
}

keys() {
  local data
  data=$(docker volume inspect "$VOLUME" --format '{{.Mountpoint}}')

  say "Keep both somewhere that is not this machine and not the Nextcloud"
  printf '\n  RESTIC_PASSWORD  %s\n' "$(setting RESTIC_PASSWORD)"
  if [ -f "$data/secrets.key" ]; then
    printf '  secrets.key      %s   (base64; restore with base64 -d)\n' "$(base64 -w0 "$data/secrets.key")"
  else
    printf '  secrets.key      none in the volume - LAMBDA_SECRETS_KEY in .env is the key then\n'
  fi
}

restore() {
  local target
  target="genhttplambda_restored-$(date -u +%Y%m%d-%H%M%S)"

  docker volume create "$target" > /dev/null
  say "Restoring ${1:-latest} into the volume $target"
  build
  "${COMPOSE[@]}" run --rm -v "$target:/restore" backup restore "${1:-latest}"

  cat <<EOF

  The server still runs on $VOLUME; nothing of it changed.

  To put this online instead:
    1. put secrets.key back into the restored volume (from --keys, base64 -d),
       owned by 1001:1001, mode 600
    2. docker compose -f $HERE/docker-compose.yml stop lambda
    3. keep what it ran on, then replace it:
       docker run --rm -v $VOLUME:/from -v genhttplambda_before-restore:/to alpine cp -a /from/. /to/
       docker run --rm -v $target:/from -v $VOLUME:/to alpine \\
         sh -c 'find /to -mindepth 1 -delete && cp -a /from/. /to/'
    4. docker compose -f $HERE/docker-compose.yml start lambda
       sudo genhttp-deploy --check
EOF
}

case "${1:-}" in
  "")          configured; run backup ;;
  --setup)     setup ;;
  --list)      configured; run snapshots ;;
  --check)     configured; run check "${2:-5%}" ;;
  --restore)   configured; restore "${2:-}" ;;
  --keys)      keys ;;
  -h|--help)   sed -n '2,18p' "$0" | sed 's/^# \{0,1\}//' ;;
  *)           echo "unknown option: $1 (try --help)" >&2; exit 2 ;;
esac
