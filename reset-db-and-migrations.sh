#!/usr/bin/env bash
set -e  # Exit on error

# --- Configuration ---
DB_NAME="coop-members"
DB_USER="postgres"
PSQL_HOST="127.0.0.1"
PSQL_PORT="5432"
PSQL_USER="postgres"
PSQL_PASS="postgres"

# --- Helper function ---
run_psql() {
  PGPASSWORD="$PSQL_PASS" psql -X -A -t \
    --host="$PSQL_HOST" \
    --port="$PSQL_PORT" \
    --username="$PSQL_USER" \
    -c "$1" "$2"
}

# --- Clear database ---
run_psql "DROP DATABASE IF EXISTS \"$DB_NAME\";" postgres
run_psql "CREATE DATABASE \"$DB_NAME\" OWNER \"$DB_USER\";" postgres
echo "✅ Database cleared and recreated."

# --- Remove migrations ---
if [ -d "./Migrations" ]; then
  rm -rf ./Migrations
echo "✅ Old migrations directory removed."
fi

# --- Create new migration ---
dotnet ef migrations add InitialCreate
echo "✅ New migration created."

# --- Update database ---
dotnet ef database update
echo "✅ Database updated with new migrations."

# --- Verify seed data ---
EXPECTED_EMAIL="simon.peter@example.com"
ACTUAL_EMAIL=$(run_psql 'SELECT "Email" FROM "Members" WHERE "Id"=1;' "$DB_NAME")

if [ "$ACTUAL_EMAIL" = "$EXPECTED_EMAIL" ]; then
    echo "✓ Seed data verification passed (Member with email $EXPECTED_EMAIL exists)."
else
    echo "✗ Seed data verification failed (Expected email: $EXPECTED_EMAIL, got: $ACTUAL_EMAIL)."
    exit 1
fi

echo -e "\n✅ Database and migrations fully reset and ready!"
