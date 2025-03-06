#!/bin/bash

set -e

# Create logs directory if it doesn't exist
mkdir -p logs

# Generate timestamped logfile name
LOGFILE="logs/aspnet-server-log-$(date +"%Y%m%d-%H%M%S").log"

# Redirect all stdout and stderr to logfile and console
exec > >(tee -a "$LOGFILE") 2>&1

cleanup() {
    EXIT_CODE=${1:-0}
    echo "🛑 Stopping ASP.NET Core server..."
    if [[ -n "$SERVER_PID" ]] && ps -p "$SERVER_PID" > /dev/null; then
        kill -SIGTERM "$SERVER_PID"
        wait "$SERVER_PID"
        echo "✅ Server stopped gracefully."
    fi
    exit $EXIT_CODE
}

# Trap interrupt signals (Ctrl+C and termination)
trap cleanup SIGINT SIGTERM

# Determine the script's directory
scriptdir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# --- Ensure required .NET files exist ---
for file in Program.cs *.csproj Properties/launchSettings.json appsettings.Development.json; do
  [ -f $file ] && echo "✅ $file exists." || { echo "❌ $file missing."; exit 1; }
done

# --- Reset Database ---
chmod +x ./reset-db.sh
./reset-db.sh

# --- Gracefully terminate existing server ---
existing_pid=$(lsof -t -i:5001 || true)
if [ -n "$existing_pid" ]; then
    kill -SIGTERM "$existing_pid"
    TIMEOUT=10
    while kill -0 "$existing_pid" &>/dev/null && [ $TIMEOUT -gt 0 ]; do
        echo "Waiting for graceful shutdown..."
        sleep 1
        ((TIMEOUT--))
    done
    kill -9 "$existing_pid" &>/dev/null || true
    echo "✅ Existing server terminated."
fi

# --- Start ASP.NET Core app ---
dotnet run &
SERVER_PID=$!

# --- Wait for server readiness ---
TIMEOUT=30
until curl -fsSL --insecure https://localhost:5001/swagger/index.html &>/dev/null || [ $TIMEOUT -le 0 ]; do
    echo "Waiting for server to start..."
    sleep 1
    ((TIMEOUT--))
done

if [ $TIMEOUT -le 0 ]; then
    echo "❌ Server failed to start."
    cleanup 1
fi

echo "✅ Server running."

# --- Verify Swagger UI ---
curl -fsSL --insecure https://localhost:5001/swagger/index.html | grep -q '<title>Swagger UI</title>' && \
  echo "✅ Swagger UI (curl) OK." || { echo "❌ Swagger UI (curl) failed."; cleanup 1; }

npm ci

"$scriptdir/fetch-html.sh" https://localhost:5001/swagger/index.html | grep -q '<title>Swagger UI</title>' || true
if [ "${PIPESTATUS[1]}" -eq 0 ]; then
  echo "✅ Swagger UI (Chrome) OK."
else
  echo "❌ Swagger UI (Chrome) failed."
  cleanup 1
fi

# --- Run Playwright tests ---
cd tests
# npx playwright install chromium --with-deps

if npx playwright test; then
    echo "✅ Playwright tests passed."
    cleanup 0
else
    echo "❌ Playwright tests failed."
    cleanup 1
fi
