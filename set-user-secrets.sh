#!/bin/bash

# Check required variables
required_vars=(
  Authentication__GitHub__ClientId 
  Authentication__GitHub__ClientSecret
  Authentication__LINE__ClientId 
  Authentication__LINE__ClientSecret
  Authentication__Google__ClientId 
  Authentication__Google__ClientSecret
)

# Verify each required environment variable is set
for var in "${required_vars[@]}"; do
  if [ -z "${!var}" ]; then
    echo "❌ Environment variable '$var' is not set!"
    exit 1
  fi
done

# Initialize user secrets if not already initialized
dotnet user-secrets init >/dev/null 2>&1

# Set user secrets securely
dotnet user-secrets set "Authentication:GitHub:ClientId" "$Authentication__GitHub__ClientId"
dotnet user-secrets set "Authentication:GitHub:ClientSecret" "$Authentication__GitHub__ClientSecret"
dotnet user-secrets set "Authentication:LINE:ClientId" "$Authentication__LINE__ClientId"
dotnet user-secrets set "Authentication:LINE:ClientSecret" "$Authentication__LINE__ClientSecret"
dotnet user-secrets set "Authentication:Google:ClientId" "$Authentication__Google__ClientId"
dotnet user-secrets set "Authentication:Google:ClientSecret" "$Authentication__Google__ClientSecret"

echo "✅ dotnet user secrets set successfully!"
