#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"


# Check required variables
required_vars=(Authentication__GitHub__ClientId Authentication__GitHub__ClientSecret)

for var in "${required_vars[@]}"; do
  if [ -z "${!var}" ]; then
    echo "❌ Environment variable '$var' is not set in asp.sh!"
    exit 1
  fi
done

# Set .NET user secrets securely
dotnet user-secrets set "Authentication:GitHub:ClientId" "$Authentication__GitHub__ClientId"
dotnet user-secrets set "Authentication:GitHub:ClientSecret" "$Authentication__GitHub__ClientSecret"

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=${DB_NAME:-asp-members};Username=postgres;Password=postgres"


echo "✅ .NET user secrets set successfully."
