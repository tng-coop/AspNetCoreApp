#!/bin/bash

set -e

# Create the AspNetCoreApp directory
mkdir -p AspNetCoreApp

# Move all application-specific files and folders into AspNetCoreApp
mv Areas AspNetCoreApp/ || true
mv Client AspNetCoreApp/ || true
mv Controllers AspNetCoreApp/ || true
mv Data AspNetCoreApp/ || true
mv Migrations AspNetCoreApp/ || true
mv Models AspNetCoreApp/ || true
mv Pages AspNetCoreApp/ || true
mv Properties AspNetCoreApp/ || true
mv Services AspNetCoreApp/ || true
mv wwwroot AspNetCoreApp/ || true
mv bin AspNetCoreApp/ || true
mv obj AspNetCoreApp/ || true

# Move application-specific files
mv Program.cs AspNetCoreApp/ || true
mv AspNetCoreApp.csproj AspNetCoreApp/ || true
mv appsettings*.json AspNetCoreApp/ || true
mv *.crt *.csr *.key *.pem *.pfx *.srl AspNetCoreApp/ || true

# Move npm files
mv package.json AspNetCoreApp/ || true
mv package-lock.json AspNetCoreApp/ || true
mv node_modules AspNetCoreApp/ || true

# Confirm move
ls -altr AspNetCoreApp