#!/usr/bin/env bash
set -e  # Exit on error

# --- Configuration ---
DB_NAME="coop-members"
DB_USER="postgres"
DB_PASSWORD="postgres" # Development only

# Connection parameters
PSQL_HOST="127.0.0.1"
PSQL_PORT="5432"
PSQL_USER="postgres"
PSQL_PASS="postgres"

# Helper function
run_psql() {
  PGPASSWORD="$PSQL_PASS" psql -X -A -t \
    --host="$PSQL_HOST" \
    --port="$PSQL_PORT" \
    --username="$PSQL_USER" \
    -c "$1" "$2"
}

# Drop database
run_psql "DROP DATABASE IF EXISTS \"$DB_NAME\";" postgres

# Create database
run_psql "CREATE DATABASE \"$DB_NAME\" OWNER \"$DB_USER\";" postgres

# Verify database exists
db_exists=$(run_psql "SELECT 1 FROM pg_database WHERE datname='$DB_NAME';" postgres)
[ "$db_exists" = "1" ] && echo "✓ Database '$DB_NAME' exists." || { echo "✗ Database '$DB_NAME' missing!"; exit 1; }

echo "✅ Database cleared and recreated successfully."
