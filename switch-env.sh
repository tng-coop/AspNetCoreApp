#!/usr/bin/env bash
#
# switch-env.sh — toggle between asp-members↔asp-members2

# Determine current (default to asp-members)
CURRENT_DB="${DB_NAME:-asp-members}"

if [ "$CURRENT_DB" = "asp-members" ]; then
  NEW_DB="asp-members2"
  NEW_URL="https://aspnet.lan:5002"
else
  NEW_DB="asp-members"
  NEW_URL="https://aspnet.lan:5001"
fi

export DB_NAME="$NEW_DB"
export ConnectionStrings__DefaultConnection="Host=localhost;Database=${NEW_DB};Username=postgres;Password=postgres"
export Kestrel__Endpoints__Https__Url="$NEW_URL"

echo "🔄 Switched to:"
echo "   • DB_NAME=$DB_NAME"
echo "   • ConnectionStrings__DefaultConnection=$ConnectionStrings__DefaultConnection"
echo "   • Kestrel__Endpoints__Https__Url=$Kestrel__Endpoints__Https__Url"
