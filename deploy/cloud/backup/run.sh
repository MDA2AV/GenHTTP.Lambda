#!/bin/sh
#
# Runs inside the backup container, on the server's data volume at /data.
# deploy/cloud/backup.sh is what an operator calls; this is what it starts.
#
#   backup init                  make the repository, once
#   backup backup                take tonight's snapshot (the default)
#   backup snapshots             list what is kept
#   backup check [subset]        verify the repository, reading a part (5%)
#   backup restore [snapshot]    restore into the empty volume at /restore
#
# Nothing here writes to /data. The databases are read with SQLite's own
# backup, read-only, so the server goes on writing while they are taken.
set -eu

DATA=/data
SNAPSHOT=/staging/snapshot
RESTORE=/restore

# The host restic files the snapshots under. A container's own name changes
# with every run, and restic finds the snapshot before by host and paths -
# under a new name each night it would read every file again.
HOST=${BACKUP_HOST:-genhttp.dev}

say()  { printf '%s %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$*"; }
fail() { say "FAILED: $*" >&2; exit 1; }

#region The databases

# Every SQLite file the platform keeps, and only those: its records, the
# analytics, each lambda's database and each draft's copy. Named one by one
# rather than by "*.db", since a lambda may well keep a file called that in its
# workspace, and that one is a file like any other.
databases() {
  for db in "$DATA/lambda.db" "$DATA/analytics.db" \
            "$DATA"/databases/*/database.db \
            "$DATA"/features/*/*/database.db; do
    [ -f "$db" ] && printf '%s\n' "$db"
  done
}

# A copy of each, consistent at one moment, through SQLite's online backup:
# copying the file would catch a page half written, and leave out what is
# still in the write-ahead log. Read-only, so closing it never checkpoints the
# server's log either.
snapshot_databases() {
  rm -rf "$SNAPSHOT"
  mkdir -p "$SNAPSHOT"

  count=0
  for db in $(databases "$DATA"); do
    relative=${db#"$DATA"/}
    target="$SNAPSHOT/$relative"
    mkdir -p "$(dirname "$target")"

    if ! sqlite3 -readonly -bail -cmd ".timeout 30000" "$db" ".backup '$target'" ||
       [ "$(sqlite3 "$target" 'PRAGMA quick_check;')" != ok ]; then
      case "$relative" in
        # A draft deleted while this runs takes its copy with it, and a
        # draft's test data is the least of what is kept: said, not fatal.
        features/*) say "skipped $relative, which could not be read whole"; rm -f "$target"; continue ;;
        *) fail "could not take $relative" ;;
      esac
    fi

    count=$((count + 1))
  done

  say "took $count databases"
}

#endregion

#region What is kept

# What is not taken from /data as it lies.
#
# The databases come from the snapshot above instead. Everything else left out
# is made again by the server or is not to leave the machine:
#
#   assemblies, resources   compiled and written out from the versions
#   sources, staging        a cache of packed sources, files on their way out
#   geo, crawlers           lists downloaded again
#   dumps                   a crashed process's memory: keys, tokens, addresses
#   secrets.key             kept apart - see backup.sh
#
# code (versions and their content), git (commits are made once and must stay
# the same), workspaces, features, branding and showcase are what this is for.
excludes() {
  cat <<EOF
$DATA/assemblies
$DATA/resources
$DATA/sources
$DATA/staging
$DATA/geo
$DATA/crawlers
$DATA/dumps
$DATA/assets
$DATA/secrets.key
$DATA/lambda.db*
$DATA/analytics.db*
$DATA/databases/*/database.db*
$DATA/features/*/*/database.db*
EOF
}

backup() {
  [ -f "$DATA/lambda.db" ] || fail "no lambda.db in $DATA - is the server's volume mounted there?"

  say "taking the databases"
  snapshot_databases

  excludes > /tmp/excludes

  # The databases first and the files after: a version the snapshot of
  # lambda.db names is then always among the files, and a file newer than it
  # is merely something the records do not know of yet.
  say "sending the snapshot"
  status=0
  restic backup --host "$HOST" --tag nightly --exclude-file /tmp/excludes \
    --no-scan "$DATA" "$SNAPSHOT" || status=$?

  rm -rf "$SNAPSHOT"

  # 3: the snapshot was made, but some file could not be read. Kept, and
  # reported, because a file left out quietly is how backups fail.
  case "$status" in
    0) ;;
    3) say "the snapshot misses files that could not be read - see above" ;;
    *) fail "restic backup ended with $status" ;;
  esac

  say "forgetting what is past keeping"
  # shellcheck disable=SC2086 # the policy is a list of options
  restic forget --host "$HOST" ${BACKUP_KEEP:---keep-daily 7 --keep-weekly 4 --keep-monthly 3}

  # Pruning reads and rewrites packs over WebDAV, so once a week, not nightly.
  if [ "$(date -u +%u)" = "${BACKUP_PRUNE_WEEKDAY:-7}" ]; then
    say "pruning and checking a part of the repository"
    restic prune
    restic check --read-data-subset=5%
  fi

  [ "$status" = 0 ] || exit "$status"
  say "done"
}

#endregion

#region Restoring

# Into an empty volume, never over the one the server runs on: putting it
# online is the operator's step, after looking at it.
restore() {
  snapshot=${1:-latest}

  [ -z "$(find "$RESTORE" -mindepth 1 -maxdepth 1 ! -name lost+found)" ] ||
    fail "$RESTORE is not empty - restore into a new volume"

  say "restoring $snapshot"
  restic restore "$snapshot:$DATA" --host "$HOST" --target "$RESTORE"
  restic restore "$snapshot:$SNAPSHOT" --host "$HOST" --target "$RESTORE"

  for db in $(databases "$RESTORE"); do
    [ "$(sqlite3 "$db" 'PRAGMA quick_check;')" = ok ] || fail "${db#"$RESTORE"/} is damaged"
  done

  say "restored $snapshot; secrets.key is not part of it - put the installation's key back before starting the server on it"
}

#endregion

command=${1:-backup}
[ $# -gt 0 ] && shift

case "$command" in
  init)      restic init ;;
  backup)    backup ;;
  snapshots) restic snapshots --host "$HOST" ;;
  check)     restic check --read-data-subset="${1:-5%}" ;;
  restore)   restore "$@" ;;
  *)         fail "unknown command '$command' (init, backup, snapshots, check, restore)" ;;
esac
