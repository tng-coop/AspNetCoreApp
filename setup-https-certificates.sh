#!/bin/bash

set -e

# --- Variables ---
CERT_PASSWORD="yourpassword"
CA_NAME="LocalhostDevelopmentCA"

# --- Function to gracefully terminate server ---
cleanup() {
    echo "🛑 Stopping ASP.NET Core server..."
    if [[ -n "$SERVER_PID" ]] && ps -p "$SERVER_PID" > /dev/null; then
        kill -SIGTERM "$SERVER_PID"
        wait "$SERVER_PID"
        echo "✅ Server stopped gracefully."
    fi
    exit 1
}

# Trap interrupt signals (Ctrl+C and termination)
trap cleanup SIGINT SIGTERM

# --- Ensure required .NET files exist ---
for file in Program.cs *.csproj Properties/launchSettings.json appsettings.Development.json; do
  [ -f $file ] && echo "✅ $file exists." || { echo "❌ $file missing."; exit 1; }
done

# --- Reset Database ---
chmod +x ./reset-db.sh
./reset-db.sh

# --- Cleanup existing certificates ---
rm -f localhost-ca.* localhost.* *.pfx *.pem *.csr *.srl

# --- Reset NSS DB ---
rm -rf "$HOME/.pki/nssdb"
mkdir -p "$HOME/.pki/nssdb"
certutil -d sql:"$HOME/.pki/nssdb" -N --empty-password && echo "✅ NSS DB reset."

# --- Generate Root CA ---
openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes \
  -keyout localhost-ca.key -out localhost-ca.crt \
  -subj "/CN=${CA_NAME}" -addext "basicConstraints=critical,CA:true"

# --- Trust CA (system & NSS DB) ---
sudo cp localhost-ca.crt /usr/local/share/ca-certificates/${CA_NAME}.crt
sudo update-ca-certificates || true
certutil -d sql:"$HOME/.pki/nssdb" -A -t "CT,C,C" -n "${CA_NAME}" -i localhost-ca.crt && echo "✅ Root CA trusted."

# --- Generate localhost CSR ---
openssl req -newkey rsa:4096 -nodes \
  -keyout localhost.key -out localhost.csr \
  -subj "/CN=localhost"

# --- Sign cert with SAN for localhost only ---
openssl x509 -req -in localhost.csr -CA localhost-ca.crt -CAkey localhost-ca.key \
  -CAcreateserial -out localhost.crt -days 3650 -sha256 \
  -extfile <(echo "subjectAltName=DNS:localhost")

# --- Create PFX and PEM certificates ---
openssl pkcs12 -export -out localhost.pfx -inkey localhost.key \
  -in localhost.crt -passout pass:"${CERT_PASSWORD}"
openssl pkcs12 -in localhost.pfx -out localhost.pem -nodes -passin pass:"${CERT_PASSWORD}"

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
until curl -fsSL --cacert localhost-ca.crt https://localhost:5001/swagger/index.html &>/dev/null || [ $TIMEOUT -le 0 ]; do
    echo "Waiting for server to start..."
    sleep 1
    ((TIMEOUT--))
done

if [ $TIMEOUT -le 0 ]; then
    echo "❌ Server failed to start."
    cleanup
fi

echo "✅ Server running."

# --- Verify Swagger UI ---
curl -fsSL --cacert localhost-ca.crt https://localhost:5001/swagger/index.html | grep -q '<title>Swagger UI</title>' && \
  echo "✅ Swagger UI (curl) OK." || { echo "❌ Swagger UI (curl) failed."; cleanup; }

google-chrome --headless --disable-gpu --no-sandbox --dump-dom https://localhost:5001/swagger/index.html | grep -q 'swagger-ui' && \
  echo "✅ Swagger UI (Chrome) OK." || { echo "❌ Swagger UI (Chrome) failed."; cleanup; }

# --- Run Playwright tests ---
cd tests
npm ci
npx playwright install chromium --with-deps

if npx playwright test; then
    echo "✅ Playwright tests passed."
else
    echo "❌ Playwright tests failed."
    cleanup
fi

# --- Cleanup ---
cleanup
