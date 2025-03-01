#!/usr/bin/env bash
set -e  # Exit on error

# --- Configuration ---
DB_NAME="coop-members"
DB_USER="app"
DB_PASSWORD="Password123!" # Change this password in production

# Connection parameters (customize as needed)
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
    -c "$1" postgres
}

# Drop database
run_psql "DROP DATABASE IF EXISTS \"$DB_NAME\";"

# Drop user
run_psql "DROP ROLE IF EXISTS \"$DB_USER\";"

# Recreate user
run_psql "CREATE ROLE \"$DB_USER\" WITH LOGIN PASSWORD '$DB_PASSWORD';"

# Create database owned by user
run_psql "CREATE DATABASE \"$DB_NAME\" OWNER \"$DB_USER\";"

# Verify user exists
user_exists=$(run_psql "SELECT 1 FROM pg_roles WHERE rolname='$DB_USER';")
[ "$user_exists" = "1" ] && echo "✓ Role '$DB_USER' exists." || { echo "✗ Role '$DB_USER' missing!"; exit 1; }

# Verify database exists
db_exists=$(run_psql "SELECT 1 FROM pg_database WHERE datname='$DB_NAME';")
[ "$db_exists" = "1" ] && echo "✓ Database '$DB_NAME' exists." || { echo "✗ Database '$DB_NAME' missing!"; exit 1; }

echo "\n✅ Database setup for ASP.NET Entity Framework completed successfully!"
