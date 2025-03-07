#!/usr/bin/env bash
set -e  # Exit on any error

# Load Neon credentials safely from neon.env
source ./neon.env

# --- Helper function to run SQL commands ---
run_psql() {
  PGPASSWORD="$PGPASSWORD" psql -X -A -t \
    --host="$PGHOST" \
    --username="$PGUSER" \
    -c "$1" "$PGDATABASE" \
    -p 5432
}

# --- Clear existing Neon DB ---
echo "Clearing existing Neon database..."
run_psql "DROP SCHEMA public CASCADE; CREATE SCHEMA public;" || {
  echo "⚠️ Schema reset encountered an issue but continuing."
}

# --- Apply EF migrations ---
echo "Applying EF migrations to Neon..."
dotnet ef database update --connection "Host=$PGHOST;Database=$PGDATABASE;Username=$PGUSER;Password=$PGPASSWORD;Ssl Mode=Require;Trust Server Certificate=true;"

# --- Verify seeded data ---
EXPECTED_EMAIL="simon.peter@example.com"
ACTUAL_EMAIL=$(run_psql 'SELECT "Email" FROM "Members" WHERE "Id"=1;')

if [ "$ACTUAL_EMAIL" = "$EXPECTED_EMAIL" ]; then
    echo "✅ Seed data verification passed (Member with email $EXPECTED_EMAIL exists)."
else
    echo "❌ Seed data verification failed (Expected email: $EXPECTED_EMAIL, got: $ACTUAL_EMAIL)."
    exit 1
fi

echo "🚀 Neon database published and seeded successfully!"
