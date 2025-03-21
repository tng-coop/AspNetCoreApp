#!/bin/bash

# Check required variables
required_vars=(Authentication__GitHub__ClientId Authentication__GitHub__ClientSecret)

for var in "${required_vars[@]}"; do
  if [ -z "${!var}" ]; then
    echo "❌ Environment variable '$var' is not set in asp.sh!"
    exit 1
  fi
done

# Set GitHub secrets securely
gh secret set AUTH_GITHUB_CLIENT_ID --body "$Authentication__GitHub__ClientId"
gh secret set AUTH_GITHUB_CLIENT_SECRET --body "$Authentication__GitHub__ClientSecret"

echo "✅ GitHub Actions secrets set successfully!"
