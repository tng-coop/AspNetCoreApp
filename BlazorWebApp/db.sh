#!/usr/bin/env bash
set -euo pipefail

# db.sh: Dump or restore PostgreSQL databases for local and Neon environments.
# Supports environment numbering (0-9) for local only.
# Usage:
#   ./db.sh dump   local  [<env_num>] [<dump_file>]
#   ./db.sh restore local  [<env_num>] [<dump_file>]
#   ./db.sh dump   neon   [<dump_file>]
#   ./db.sh restore neon  [<dump_file>]
#
# env_num: integer [0-9] selects local database suffix "asp-members-yasu-<env_num>";
#          if omitted, uses ConnectionStrings:DefaultConnection from user-secrets
# dump_file: optional SQL file name; defaults to <DB_NAME>_dump.sql or asp-members_neon_dump.sql
# neon.sh: should define NEON_HOST, NEON_DB, NEON_USER, NEON_PASSWORD at ~/co/tng-admin/passwords/projects/AspNetCoreApp/neon.sh

function usage() {
  cat <<EOF >&2
Usage:
  $0 dump   local   [<env_num>] [<dump_file>]
  $0 restore local [<env_num>] [<dump_file>]
  $0 dump   neon    [<dump_file>]
  $0 restore neon   [<dump_file>]

Examples:
  ./db.sh dump local               # uses default from user-secrets
  ./db.sh dump local 2 custom.sql  # env 2 local DB to custom.sql
  ./db.sh dump neon                # sources neon.sh, writes asp-members_neon_dump.sql
  ./db.sh restore neon restore.sql # loads into Neon from restore.sql
EOF
  exit 1
}

# Validate arg count
if [[ $# -lt 2 || $# -gt 4 ]]; then
  usage
fi

CMD=$1      # dump or restore
TARGET=$2   # local or neon
shift 2

ENV_NUM=""
FILE=""

# Parse optional ENV_NUM and FILE
if [[ "$TARGET" == "local" ]]; then
  if [[ -n $1 && $1 =~ ^[0-9]$ ]]; then
    ENV_NUM=$1
    shift
  fi
  if [[ -n $1 ]]; then
    FILE=$1
  fi
elif [[ "$TARGET" == "neon" ]]; then
  if [[ -n $1 ]]; then
    FILE=$1
  fi
else
  usage
fi

# Setup based on target
case "$TARGET" in
  local)
    if [[ -n "$ENV_NUM" ]]; then
      LOCAL_HOST="localhost"
      LOCAL_USER="postgres"
      LOCAL_PASS="postgres"
      DB_NAME="asp-members-yasu-$ENV_NUM"
    else
      CONN_STR=$(dotnet user-secrets list | \
        grep '^ConnectionStrings:DefaultConnection' | \
        sed 's/^ConnectionStrings:DefaultConnection = //')
      IFS=';' read -ra KV <<< "$CONN_STR"
      for kv in "${KV[@]}"; do
        IFS='=' read -r key val <<< "$kv"
        case "$key" in
          Host) LOCAL_HOST="$val";;
          Database) DB_NAME="$val";;
          Username) LOCAL_USER="$val";;
          Password) LOCAL_PASS="$val";;
        esac
      done
    fi
    : ${FILE:="${DB_NAME}_dump.sql"}
    DUMP_FILE="$FILE"
    export PGPASSWORD="$LOCAL_PASS"
    ;;

  neon)
    NEON_FILE="~/co/tng-admin/passwords/projects/AspNetCoreApp/neon.sh"
    NEON_FILE=$(eval echo $NEON_FILE)
    if [[ ! -f "$NEON_FILE" ]]; then
      echo "Error: neon.sh not found at expected path ($NEON_FILE)" >&2
      exit 2
    fi
    source "$NEON_FILE"
    export PGPASSWORD="$NEON_PASSWORD"
    export PGSSLMODE=require
    DB_NAME="$NEON_DB"
    : ${FILE:="asp-members_neon_dump.sql"}
    DUMP_FILE="$FILE"
    ;;

  *) usage ;;
esac

# Execute dump or restore
case "$CMD" in
  dump)
    echo "Dumping '$TARGET' [$DB_NAME] -> $DUMP_FILE"
    if [[ "$TARGET" == "local" ]]; then
      pg_dump -h "$LOCAL_HOST" -U "$LOCAL_USER" -d "$DB_NAME" -F p -v > "$DUMP_FILE"
    else
      pg_dump -h "$NEON_HOST" -U "$NEON_USER" -d "$DB_NAME" -F p -v > "$DUMP_FILE"
    fi
    echo "Dump complete: $DUMP_FILE"
    ;;

  restore)
    echo "Restoring '$TARGET' [$DB_NAME] <- $DUMP_FILE"
    if [[ "$TARGET" == "local" ]]; then
      dropdb -h "$LOCAL_HOST" -U "$LOCAL_USER" "$DB_NAME" || true
      createdb -h "$LOCAL_HOST" -U "$LOCAL_USER" "$DB_NAME"
      psql -h "$LOCAL_HOST" -U "$LOCAL_USER" -d "$DB_NAME" < "$DUMP_FILE"
    else
      psql "postgresql://$NEON_USER:$NEON_PASSWORD@$NEON_HOST/$NEON_DB?sslmode=require" \
        -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"
      psql "postgresql://$NEON_USER:$NEON_PASSWORD@$NEON_HOST/$NEON_DB?sslmode=require" \
        < "$DUMP_FILE"
    fi
    echo "Restore complete: $DB_NAME"
    ;;

  *) usage ;;
esac
