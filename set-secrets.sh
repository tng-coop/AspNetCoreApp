#!/bin/bash

# Load the smtp.env file
set -a
source smtp.env
set +a

# Verify required variables are set
required_vars=(SMTP_SERVER SMTP_PORT SMTP_USER SMTP_PASSWORD FROM_EMAIL)

for var in "${required_vars[@]}"; do
  if [ -z "${!var}" ]; then
    echo "❌ Environment variable '$var' is not set in smtp.env!"
    exit 1
  fi
done

# Set secrets using gh CLI
gh secret set SMTP_SERVER --body "$SMTP_SERVER"
gh secret set SMTP_PORT --body "$SMTP_PORT"
gh secret set SMTP_USER --body "$SMTP_USER"
gh secret set SMTP_PASSWORD --body "$SMTP_PASSWORD"
gh secret set FROM_EMAIL --body "$FROM_EMAIL"

echo "✅ GitHub Actions secrets set successfully from smtp.env!"
