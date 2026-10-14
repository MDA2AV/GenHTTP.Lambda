#!/usr/bin/env bash
#
# Adds www. to the certificates of the lambdas' own domains, so a visitor who
# types www.example.com trusts the server long enough to be sent on to
# example.com.
#
# The server sends www. in front of a domain on to the domain, but a browser
# only follows that once the handshake succeeded - and a certificate issued
# for example.com alone does not carry www.example.com, so its visitors see a
# warning instead. This goes through the certificates in the folder the
# server reads, reissues every one that lacks the www. of one of its names
# with both and installs the files where the container reads them.
#
# It does not restart the server: the io_uring engine learns the names it has
# certificates for when it opens its port, so the new ones are served from the
# next restart on - the deploy that brings the redirect, say.
#
# A www. whose DNS does not point where its domain does is left out and named:
# certbot would fail for every name of the certificate. The wildcard of the
# hosting domain and the site's own certificate are left alone.
#
#   sudo ./reissue-www.sh             reissue and install what lacks www.
#   sudo ./reissue-www.sh --list      say what would be reissued, change nothing
#   sudo ./reissue-www.sh --dry-run   ask the staging CA, install nothing
#
set -euo pipefail

CERTS=${CERTS:-/opt/genhttp-lambda/certs}
ACME=${ACME:-/opt/genhttp-lambda/acme}
LIVE=${LIVE:-/etc/letsencrypt/live}

# the group the server runs as, which reads the private keys
GROUP=1001

list_only=no; dry_run=no
for arg in "$@"; do
  case "$arg" in
    --list)    list_only=yes ;;
    --dry-run) dry_run=yes ;;
    -h|--help) sed -n '2,25p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "unknown option: $arg (try --help)" >&2; exit 2 ;;
  esac
done

if [ "$(id -u)" -ne 0 ]; then
  echo "This needs root, for certbot and the certificates." >&2
  echo "Run:  sudo $0 $*" >&2
  exit 1
fi

say()  { printf '\n\033[1m%s\033[0m\n' "$*"; }
ok()   { printf '  \033[32mok\033[0m   %s\n' "$*"; }
skip() { printf '  \033[33mskip\033[0m %s\n' "$*"; }
bad()  { printf '  \033[31mFAIL\033[0m %s\n' "$*"; }

# the names a certificate carries, one per line, sorted
names_of() {
  { openssl x509 -in "$1" -noout -ext subjectAltName 2>/dev/null || true; } \
    | tr ',' '\n' | sed -n 's/^ *DNS://p' | sort -u
}

# the addresses a name resolves to, one per line, sorted - nothing for a name
# that does not resolve
addresses_of() { { getent ahosts "$1" 2>/dev/null || true; } | awk '{print $1}' | sort -u; }

# the certbot lineage whose certificate carries exactly these names - the
# folder name below the certificates is a convention, the names are not
lineage_of() {
  local wanted=$1 folder=$2 live
  for live in "$LIVE"/*/; do
    [ -f "$live/fullchain.pem" ] || continue
    if [ "$(names_of "$live/fullchain.pem")" = "$wanted" ]; then
      basename "$live"
      return
    fi
  done
  if [ -d "$LIVE/$folder" ]; then
    echo "$folder"
  fi
}

# what is to be done, one line each: folder, lineage, names separated by spaces
plan=()

say "Looking through $CERTS"

for dir in "$CERTS"/*/; do
  folder=$(basename "$dir")
  cert="$dir/fullchain.pem"

  [ -f "$cert" ] || continue

  names=$(names_of "$cert")

  if grep -q '^\*\.' <<<"$names"; then
    skip "$folder: a wildcard, which is the hosting domain's"
    continue
  fi

  missing=(); skipped=no

  for name in $names; do
    case "$name" in www.*) continue ;; esac

    grep -qx "www.$name" <<<"$names" && continue

    www_addresses=$(addresses_of "www.$name")
    own_addresses=$(addresses_of "$name")

    if [ -z "$www_addresses" ]; then
      skip "$folder: www.$name does not resolve"; skipped=yes
      continue
    fi

    # every address of the www. has to be one of the domain's, which points
    # here since its certificate is being served
    if [ -n "$(comm -23 <(echo "$www_addresses") <(echo "$own_addresses"))" ]; then
      skip "$folder: www.$name resolves to $(echo "$www_addresses" | paste -sd ' '), elsewhere than $name"; skipped=yes
      continue
    fi

    missing+=("www.$name")
  done

  if [ ${#missing[@]} -eq 0 ]; then
    [ "$skipped" = yes ] || ok "$folder: nothing to add"
    continue
  fi

  wanted=$(printf '%s\n' $names "${missing[@]}" | sort -u)

  lineage=$(lineage_of "$names" "$folder")

  if [ -z "$lineage" ]; then
    bad "$folder: no certificate in $LIVE carries $(echo "$names" | paste -sd ' ') - issue it by hand"
    continue
  fi

  printf '  \033[1madd\033[0m  %s: %s (certbot lineage %s)\n' "$folder" "${missing[*]}" "$lineage"
  plan+=("$folder $lineage $(echo "$wanted" | paste -sd ' ')")
done

if [ ${#plan[@]} -eq 0 ]; then
  say "Nothing to reissue."
  exit 0
fi

[ "$list_only" = yes ] && exit 0

installed=0; failed=0

for entry in "${plan[@]}"; do
  read -r folder lineage wanted <<<"$entry"

  domains=()
  for name in $wanted; do domains+=(-d "$name"); done

  say "Reissuing $folder for $wanted"

  # the challenges are answered by the server from the web root, for any
  # host - a www. it sends on is followed to its domain by the CA
  args=(certonly --non-interactive --cert-name "$lineage" --expand --webroot -w "$ACME" "${domains[@]}")
  [ "$dry_run" = yes ] && args+=(--dry-run)

  if ! certbot "${args[@]}"; then
    bad "$folder: certbot failed, the certificate is unchanged"
    failed=$((failed + 1))
    continue
  fi

  if [ "$dry_run" = yes ]; then
    ok "$folder: the staging CA would issue it"
    continue
  fi

  install -m 0644 -o root -g "$GROUP" "$LIVE/$lineage/fullchain.pem" "$CERTS/$folder/"
  install -m 0640 -o root -g "$GROUP" "$LIVE/$lineage/privkey.pem"   "$CERTS/$folder/"

  ok "$folder: installed"
  installed=$((installed + 1))
done

[ "$installed" -gt 0 ] && say "Installed $installed certificate(s), served from the next restart of the server on."

if [ "$failed" -gt 0 ]; then
  say "$failed certificate(s) could not be reissued - see above."
  exit 1
fi

say "Done."
