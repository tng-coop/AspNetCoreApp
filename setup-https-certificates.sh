#!/bin/bash

set -e

# Run database setup first
chmod +x ./reset-db.sh
./reset-db.sh

# Variables
CERT_PASSWORD="yourpassword"
CA_NAME="LocalhostDevelopmentCA"

# Verify .NET project files explicitly
[ -f Program.cs ] && echo "✅ Program.cs exists." || { echo "❌ Program.cs missing."; exit 1; }
[ -f *.csproj ] && echo "✅ Project file (.csproj) exists." || { echo "❌ Project file (.csproj) missing."; exit 1; }
[ -f Properties/launchSettings.json ] && echo "✅ launchSettings.json exists." || { echo "❌ launchSettings.json missing."; exit 1; }
[ -f appsettings.Development.json ] && echo "✅ appsettings.Development.json exists." || { echo "❌ appsettings.Development.json missing."; exit 1; }

# Remove existing certificates
rm -f localhost-ca.* localhost.* *.pfx *.pem *.csr *.srl

# Reset NSS DB
rm -rf $HOME/.pki/nssdb
mkdir -p $HOME/.pki/nssdb
certutil -d sql:$HOME/.pki/nssdb -N --empty-password && echo "✅ NSS DB reset."

# Generate new Root CA
openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes \
  -keyout localhost-ca.key -out localhost-ca.crt \
  -subj "/CN=${CA_NAME}" -addext "basicConstraints=critical,CA:true"

# Trust CA
sudo cp localhost-ca.crt /usr/local/share/ca-certificates/${CA_NAME}.crt
sudo update-ca-certificates || true

# Trust CA in Chrome NSS DB
certutil -d sql:$HOME/.pki/nssdb -A -t "CT,C,C" -n "${CA_NAME}" -i localhost-ca.crt && echo "✅ Root CA trusted in NSS DB."

# Generate localhost cert and CSR
openssl req -newkey rsa:4096 -nodes \
  -keyout localhost.key -out localhost.csr \
  -subj "/CN=localhost"

# Sign localhost cert
openssl x509 -req -in localhost.csr -CA localhost-ca.crt -CAkey localhost-ca.key \
  -CAcreateserial -out localhost.crt -days 3650 -sha256 \
  -extfile <(echo "subjectAltName=DNS:localhost")

# Create PFX
openssl pkcs12 -export -out localhost.pfx -inkey localhost.key \
  -in localhost.crt -passout pass:"${CERT_PASSWORD}"

# Convert to PEM
openssl pkcs12 -in localhost.pfx -out localhost.pem -nodes -passin pass:"${CERT_PASSWORD}"

# Update appsettings.json explicitly
cat > appsettings.json <<EOF
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "localhost.pfx",
          "Password": "${CERT_PASSWORD}"
        }
      }
    }
  }
}
EOF

# Kill existing server gracefully
existing_pid=$(lsof -t -i:5001 || true)
if [ -n "$existing_pid" ]; then
    kill -SIGTERM "$existing_pid"
    TIMEOUT=10
    while kill -0 "$existing_pid" >/dev/null 2>&1; do
        if [ $TIMEOUT -le 0 ]; then
            echo "⚠️ Server didn't stop gracefully; forcing shutdown."
            kill -9 "$existing_pid"
            break
        fi
        echo "Waiting for graceful shutdown..."
        sleep 1
        TIMEOUT=$((TIMEOUT - 1))
    done
    echo "✅ Existing server on port 5001 terminated gracefully."
fi

# Start ASP.NET Core app
dotnet run &
SERVER_PID=$!

# Wait until server responds
TIMEOUT=30
until curl -fsSL --cacert localhost-ca.crt https://localhost:5001/swagger/index.html &>/dev/null; do
    if [ "$TIMEOUT" -le 0 ]; then
        echo "❌ Server did not start within expected time."
        kill "$SERVER_PID" || true
        exit 1
    fi
    echo "Waiting for server to start..."
    sleep 1
    TIMEOUT=$((TIMEOUT - 1))
done
echo "✅ ASP.NET Core server started successfully."

# Verify Swagger UI
curl -fsSL --cacert localhost-ca.crt https://localhost:5001/swagger/index.html | grep -q '<title>Swagger UI</title>' && echo "✅ Swagger UI verified via curl." || { echo "❌ Swagger UI failed via curl."; kill "$SERVER_PID"; exit 1; }

# Verify Swagger UI via Chrome
google-chrome --headless --disable-gpu --no-sandbox --dump-dom https://localhost:5001/swagger/index.html | grep -q 'swagger-ui' && echo "✅ Swagger UI verified via Chrome headless." || { echo "❌ Swagger UI failed via Chrome headless."; kill "$SERVER_PID"; exit 1; }

# 👇 Now continue with Playwright setup and tests:

cd tests
npm ci
npx playwright install chromium --with-deps

# Run Playwright tests
if npx playwright test; then
    echo "✅ Playwright tests passed."
else
    echo "❌ Playwright tests failed."
    kill "$SERVER_PID" || true
    exit 1
fi

# Graceful cleanup
kill -SIGTERM "$SERVER_PID"
wait "$SERVER_PID" || true
echo "✅ ASP.NET Core server stopped gracefully."
