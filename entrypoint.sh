#!/usr/bin/env bash
set -e

# Start the Blazor app in background
dotnet BlazorWebApp.dll &  
app_pid=$!

# Wait until port 8080 is open
echo "Waiting for server on port 8080…"
while ! nc -z localhost 8080; do
  sleep 1
done

# Once it’s up:
echo "hello"

# Now hand control back to the server process
wait $app_pid
