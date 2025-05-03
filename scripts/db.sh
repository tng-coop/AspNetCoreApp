#!/usr/bin/env bash
set -euo pipefail

# db.sh: Dump or restore PostgreSQL databases for local and Neon environments.
# Files are stored under a centralized dump directory with timestamps for uniqueness.
# Supports environment numbering (0-9) for local only.
# Usage:
#   ./db.sh dump   local  [<env_num>] [<dump_file>]
#   ./db.sh restore local [<env_num>] [<dump_file>]
#   ./db.sh dump   neon   [<dump_file>]
#   ./db.sh restore neon  [<dump_file>]

function usage() {
  cat <<EOF >&2
Usage:
  $0 dump   local   [<env_num>] [<dump_file>]
  $0 restore local [<env_num>] [<dump_file>]
  $0 dump   neon    [<dump_file>]
  $0 restore neon   [<dump_file>]

Examples:
  ./db.sh dump local               # default from user-secrets, timestamped
  ./db.sh dump local 2 custom.sql  # env 2 local DB, custom base name, timestamped
  ./db.sh dump neon                # neon, writes to dumps/<name>_timestamp.sql
  ./db.sh restore neon restore.sql # loads Neon from dumps/<given>_timestamp.sql
EOF
  exit 1
}

# ----------------------------------------------------------------------------
# Setup
# ----------------------------------------------------------------------------

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# Root of BlazorWebApp assumed one level up
APP_DIR="$SCRIPT_DIR/../BlazorWebApp"
cd "$APP_DIR" || exit 1

# Create dump directory if missing
DUMP_DIR="$SCRIPT_DIR/../dumps"
mkdir -p "$DUMP_DIR"

# ----------------------------------------------------------------------------
# Parse arguments
# ----------------------------------------------------------------------------

if [[ $# -lt 2 || $# -gt 4 ]]; then
  usage
fi

CMD=$1      # dump or restore
TARGET=$2   # local or neon
shift 2

ENV_NUM=""
FILE_BASE=""

if [[ "$TARGET" == "local" ]]; then
  # local may have env number
  if [[ $# -ge 1 && "$1" =~ ^[0-9]$ ]]; then
    ENV_NUM=$1; shift
  fi
  [[ $# -ge 1 ]] && FILE_BASE=$1
elif [[ "$TARGET" == "neon" ]]; then
  [[ $# -ge 1 ]] && FILE_BASE=$1
else
  usage
fi

# ----------------------------------------------------------------------------
# Determine DB connection and default file name
# ----------------------------------------------------------------------------

time_stamp=$(date +"%Y%m%d_%H%M%S")

case "$TARGET" in
  local)
    if [[ -n "$ENV_NUM" ]]; then
      LOCAL_HOST="localhost"
      LOCAL_USER="postgres"
      LOCAL_PASS="postgres"
      DB_NAME="asp-members-yasu-$ENV_NUM"
    else
      # Pull from dotnet user-secrets
      conn=$(dotnet user-secrets list | grep '^ConnectionStrings:DefaultConnection' | sed 's/^.*= //')
      IFS=';' read -ra kv <<< "$conn"
      for pair in "${kv[@]}"; do
        IFS='=' read -r key val <<< "$pair"
        case "$key" in
          Host)     LOCAL_HOST="$val";;
          Database) DB_NAME="$val";;
          Username) LOCAL_USER="$val";;
          Password) LOCAL_PASS="$val";;
        esac
      done
    fi
    : ${FILE_BASE:="${DB_NAME}_dump"}
    export PGPASSWORD="$LOCAL_PASS"
    ;;

  neon)
    # Load Neon credentials
    neon_sh="$HOME/co/tng-admin/passwords/projects/AspNetCoreApp/neon.sh"
    if [[ ! -f "$neon_sh" ]]; then
      echo "Error: neon.sh not found at '$neon_sh'" >&2
      exit 2
    fi
    source "$neon_sh"
    export PGPASSWORD="$NEON_PASSWORD"
    export PGSSLMODE=require
    DB_NAME="$NEON_DB"
    : ${FILE_BASE:="asp-members_neon_dump"}
    ;;

  *) usage ;;
esac

# Final file path (with timestamp and .sql extension)
DUMP_FILE="$DUMP_DIR/${FILE_BASE}_${time_stamp}.sql"

# ----------------------------------------------------------------------------
# Run dump or restore
# ----------------------------------------------------------------------------

echo "----------------------------------------"
echo "Action:   $CMD"
echo "Target:   $TARGET"
echo "Database: $DB_NAME"
echo "File:     $DUMP_FILE"
echo "Timestamp: $time_stamp"
echo "----------------------------------------"

case "$CMD" in
  dump)
    echo "Starting dump..."
    if [[ "$TARGET" == "local" ]]; then
      pg_dump -h "$LOCAL_HOST" -U "$LOCAL_USER" -d "$DB_NAME" -F p -v > "$DUMP_FILE"
    else
      pg_dump -h "$NEON_HOST" -U "$NEON_USER" -d "$DB_NAME" -F p -v > "$DUMP_FILE"
    fi
    echo "Dump complete: $DUMP_FILE"
    ;;

  restore)
    echo "Starting restore..."
    if [[ "$TARGET" == "local" ]]; then
      dropdb -h "$LOCAL_HOST" -U "$LOCAL_USER" "$DB_NAME" || true
      createdb -h "$LOCAL_HOST" -U "$LOCAL_USER" "$DB_NAME"
      psql -h "$LOCAL_HOST" -U "$LOCAL_USER" -d "$DB_NAME" < "$DUMP_FILE"
    else
      conn="postgresql://$NEON_USER:$NEON_PASSWORD@$NEON_HOST/$NEON_DB?sslmode=require"
      psql "$conn" -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"
      psql "$conn" < "$DUMP_FILE"
    fi
    echo "Restore complete: $DB_NAME"
    ;;

  *) usage ;;
esac
