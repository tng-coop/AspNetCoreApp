#!/usr/bin/env bash
set -e  # Exit on error

# Ensure clear-db.sh is executable and run it
chmod +x ./clear-db.sh
./clear-db.sh

# Remove old migrations
if [ -d "./Migrations" ]; then
  rm -rf ./Migrations
  echo "✅ Old migrations directory removed."
fi

# Create new migration
dotnet ef migrations add InitialCreate
echo "✅ New migration created."

# Apply new migration to database
dotnet ef database update
echo "✅ Database updated with new migrations."

# Verify seed data
EXPECTED_EMAIL="simon.peter@example.com"
ACTUAL_EMAIL=$(PGPASSWORD="postgres" psql -X -A -t \
  --host="127.0.0.1" \
  --port="5432" \
  --username="postgres" \
  -c 'SELECT "Email" FROM "Members" WHERE "Id"=1;' "asp-members")

if [ "$ACTUAL_EMAIL" = "$EXPECTED_EMAIL" ]; then
    echo "✓ Seed data verification passed (Member with email $EXPECTED_EMAIL exists)."
else
    echo "✗ Seed data verification failed (Expected email: $EXPECTED_EMAIL, got: $ACTUAL_EMAIL)."
    exit 1
fi

echo -e "\n✅ Database and migrations fully reset and ready!"
