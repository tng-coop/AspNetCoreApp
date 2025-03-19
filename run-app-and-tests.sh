#!/bin/bash

set -e

# Create logs directory if it doesn't exist
mkdir -p logs
LOGFILE="logs/aspnet-server-log-$(date +"%Y%m%d-%H%M%S").log"

cleanup() {
    EXIT_CODE=${1:-0}
    echo "🛑 Stopping ASP.NET Core server..."
    if [ -n "$SERVER_PID" ] && kill -0 "$SERVER_PID" &>/dev/null; then
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

for file in Program.cs *.csproj Properties/launchSettings.json appsettings.Development.json; do
  [[ -f "$file" ]] || { echo "❌ Required file '$file' missing."; exit 1; }
done

chmod +x "$scriptdir/reset-db.sh"
"$scriptdir/reset-db.sh"

# Extract port from environment variable (default to 5001 if not set)
APP_URL="${Kestrel__Endpoints__Https__Url:-https://0.0.0.0:5001}"
APP_PORT=$(echo "$APP_URL" | sed -E 's/.*:([0-9]+)$/\1/')

# Kill existing process on the defined port
existing_pid=$(lsof -t -i:$APP_PORT || true)
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

# Start ASP.NET Core app, logs only to file, NOT console
dotnet run > "$LOGFILE" 2>&1 &
SERVER_PID=$!

TIMEOUT=40
until curl -fsSL --cacert localhost-ca.crt "$APP_URL/swagger/index.html" &>/dev/null || [ $TIMEOUT -le 0 ]; do
    echo "Waiting for server to start..."
    sleep 1
    ((TIMEOUT--))
done

if [ $TIMEOUT -le 0 ]; then
    echo "❌ Server failed to start."
    cleanup 1
fi

echo "✅ Server running on $APP_URL"

# --- Verify Swagger UI ---
curl -fsSL --cacert localhost-ca.crt "$APP_URL/swagger/index.html" | grep -q '<title>Swagger UI</title>' && \
  echo "✅ Swagger UI (curl) OK." || { echo "❌ Swagger UI (curl) failed."; cleanup 1; }

npm ci

output=$("$scriptdir/fetch-html.sh" "$APP_URL/swagger/index.html")
echo "$output" | grep -q '<title>Swagger UI</title>'

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
