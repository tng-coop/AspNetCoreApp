#!/usr/bin/env bash
set -e  # Exit on error

# Ensure clear-db.sh is executable
chmod +x ./clear-db.sh

# Run the clearing script
./clear-db.sh

# Apply Entity Framework migrations
dotnet ef database update

# Verify seeded data
EXPECTED_EMAIL="simon.peter@example.com"
ACTUAL_EMAIL=$(PGPASSWORD="postgres" psql -X -A -t \
  --host="127.0.0.1" \
  --port="5432" \
  --username="postgres" \
  -c 'SELECT "Email" FROM "Members" WHERE "Id"=1;' "coop-members")

if [ "$ACTUAL_EMAIL" = "$EXPECTED_EMAIL" ]; then
    echo "✓ Seed data verification passed (Member with email $EXPECTED_EMAIL exists)."
else
    echo "✗ Seed data verification failed (Expected email: $EXPECTED_EMAIL, got: $ACTUAL_EMAIL)."
    exit 1
fi

echo -e "\n✅ Database reset, seeded, and ready for ASP.NET Entity Framework!"
