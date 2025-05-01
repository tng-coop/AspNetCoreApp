#!/bin/bash
# Determine the script's directory
scriptdir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
dotnet tool restore
set -e

# Create logs directory if it doesn't exist
mkdir -p logs
LOGFILE="$scriptdir/logs/aspnet-server-log-$(date +"%Y%m%d-%H%M%S").log"

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

cd $scriptdir/BlazorWebApp
for file in Program.cs *.csproj Properties/launchSettings.json appsettings.Development.json; do
  [[ -f "$file" ]] || { echo "❌ Required file '$file' missing."; exit 1; }
done

chmod +x "$scriptdir/reset-db.sh"
"$scriptdir/reset-db.sh"

# Run xUnit tests explicitly before starting the app
cd $scriptdir/BlazorWebApp.Tests
if dotnet test; then
    echo "✅ xUnit tests passed."
else
    echo "❌ xUnit tests failed."
    cleanup 1
fi

cd $scriptdir/BlazorWebApp

# ─── Require the HTTPS URL secret and export it as the ASP.NET nested env var ───
SECRET_LINE=$(dotnet user-secrets list | grep '^Kestrel:Endpoints:Https:Url ' || true)
if [ -z "$SECRET_LINE" ]; then
    echo "❌ Secret 'Kestrel:Endpoints:Https:Url' is not set in user-secrets."
    exit 1
fi

# parse "Key = Value"
APP_URL=$(printf "%s" "$SECRET_LINE" | cut -d '=' -f2- | sed 's/^ *//')
APP_PORT=${APP_URL##*:}

# this tells ASP.NET Core (and any child process) to use that URL
export Kestrel__Endpoints__Https__Url="$APP_URL"

# Kill any existing process on that port
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

# Start ASP.NET Core app, logging only to file
dotnet run > "$LOGFILE" 2>&1 &
SERVER_PID=$!

TIMEOUT=40
until curl -fsSL --cacert "$scriptdir/cert/aspnet.lan-ca.crt" "$APP_URL" &>/dev/null || [ $TIMEOUT -le 0 ]; do
    echo "Waiting for server to start..."
    sleep 1
    ((TIMEOUT--))
done

if [ $TIMEOUT -le 0 ]; then
    echo "❌ Server failed to start."
    cleanup 1
fi

echo "✅ Server running on $APP_URL"

cd $scriptdir
npm ci

output=$("$scriptdir/fetch-html.sh" "$APP_URL")
echo "$output" | grep -q '<title>Home</title>'

if [ "${PIPESTATUS[1]}" -eq 0 ]; then
  echo "✅ Swagger UI (Chrome) OK."
else
  echo "❌ Swagger UI (Chrome) failed."
  cleanup 1
fi

# --- Run Playwright tests (now sees Kestrel__Endpoints__Https__Url) ---
cd PlaywrightTests 
# npx playwright install chromium --with-deps

if npx playwright test; then
    echo "✅ Playwright tests passed."
    cleanup 0
else
    echo "❌ Playwright tests failed."
    cleanup 1
fi
