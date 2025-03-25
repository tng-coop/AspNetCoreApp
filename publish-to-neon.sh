#!/usr/bin/env bash
scriptdir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
set -e  # Exit on any error

# --- Helper function to run SQL commands ---
run_psql() {
  PGPASSWORD="$NEON_PASSWORD" psql -X -A -t \
    --host="$NEON_HOST" \
    --username="$NEON_USER" \
    -c "$1" "$NEON_DB" \
    -p 5432
}

# --- Clear existing Neon DB ---
echo "Clearing existing Neon database..."
run_psql "DROP SCHEMA public CASCADE; CREATE SCHEMA public;" || {
  echo "⚠️ Schema reset encountered an issue but continuing."
}

# --- Apply EF migrations ---
cd $scriptdir/AspNetCoreApp
echo "Applying EF migrations to Neon..."
dotnet ef database update --connection "Host=$NEON_HOST;Database=$NEON_DB;Username=$NEON_USER;Password=$NEON_PASSWORD;Ssl Mode=Require;Trust Server Certificate=true;"

# # --- Verify seeded data ---
# EXPECTED_EMAIL="simon.peter@example.com"
# ACTUAL_EMAIL=$(run_psql 'SELECT "Email" FROM "Members" WHERE "Id"=1;')

# if [ "$ACTUAL_EMAIL" = "$EXPECTED_EMAIL" ]; then
#     echo "✅ Seed data verification passed (Member with email $EXPECTED_EMAIL exists)."
# else
#     echo "❌ Seed data verification failed (Expected email: $EXPECTED_EMAIL, got: $ACTUAL_EMAIL)."
#     exit 1
# fi

echo "🚀 Neon database published and seeded successfully!"
