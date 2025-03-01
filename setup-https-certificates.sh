#!/bin/bash

set -e

# Variables
CERT_PASSWORD="yourpassword"
CA_NAME="LocalhostDevelopmentCA"

# Explicitly verify .NET project files
[ -f Program.cs ] && echo "✅ Program.cs exists." || { echo "❌ Program.cs missing."; exit 1; }
[ -f *.csproj ] && echo "✅ Project file (.csproj) exists." || { echo "❌ Project file (.csproj) missing."; exit 1; }
[ -f Properties/launchSettings.json ] && echo "✅ launchSettings.json exists." || { echo "❌ launchSettings.json missing."; exit 1; }
[ -f appsettings.Development.json ] && echo "✅ appsettings.Development.json exists." || { echo "❌ appsettings.Development.json missing."; exit 1; }

# Remove existing CA and certificate files explicitly
rm -f localhost-ca.* localhost.* *.pfx *.pem *.csr *.srl

# Reset NSS DB explicitly
rm -rf $HOME/.pki/nssdb
mkdir -p $HOME/.pki/nssdb
certutil -d sql:$HOME/.pki/nssdb -N --empty-password && echo "✅ NSS DB reset."

# Generate new Root CA
openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes \
  -keyout localhost-ca.key -out localhost-ca.crt \
  -subj "/CN=${CA_NAME}" -addext "basicConstraints=critical,CA:true"

[ -f localhost-ca.crt ] && echo "✅ localhost-ca.crt created." || { echo "❌ localhost-ca.crt missing."; exit 1; }
[ -f localhost-ca.key ] && echo "✅ localhost-ca.key created." || { echo "❌ localhost-ca.key missing."; exit 1; }

# Trust CA cert (Linux store)
sudo cp localhost-ca.crt /usr/local/share/ca-certificates/${CA_NAME}.crt
sudo update-ca-certificates || true

# Trust CA cert in Chrome NSS DB
certutil -d sql:$HOME/.pki/nssdb -A -t "CT,C,C" -n "${CA_NAME}" -i localhost-ca.crt && echo "✅ Root CA trusted in NSS DB."

# Generate localhost cert and CSR
openssl req -newkey rsa:4096 -nodes \
  -keyout localhost.key -out localhost.csr \
  -subj "/CN=localhost"

[ -f localhost.csr ] && echo "✅ localhost.csr created." || { echo "❌ localhost.csr missing."; exit 1; }
[ -f localhost.key ] && echo "✅ localhost.key created." || { echo "❌ localhost.key missing."; exit 1; }

# Sign localhost cert
openssl x509 -req -in localhost.csr -CA localhost-ca.crt -CAkey localhost-ca.key \
  -CAcreateserial -out localhost.crt -days 3650 -sha256 \
  -extfile <(echo "subjectAltName=DNS:localhost")

[ -f localhost.crt ] && echo "✅ localhost.crt created." || { echo "❌ localhost.crt missing."; exit 1; }
[ -f localhost-ca.srl ] && echo "✅ localhost-ca.srl created." || { echo "❌ localhost-ca.srl missing."; exit 1; }

# Create PFX
openssl pkcs12 -export -out localhost.pfx -inkey localhost.key \
  -in localhost.crt -passout pass:"${CERT_PASSWORD}"

[ -f localhost.pfx ] && echo "✅ localhost.pfx created." || { echo "❌ localhost.pfx missing."; exit 1; }

# Convert to PEM
openssl pkcs12 -in localhost.pfx -out localhost.pem -nodes -passin pass:"${CERT_PASSWORD}"

[ -f localhost.pem ] && echo "✅ localhost.pem created." || { echo "❌ localhost.pem missing."; exit 1; }

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

[ -f appsettings.json ] && echo "✅ appsettings.json updated." || { echo "❌ appsettings.json missing."; exit 1; }

# Kill existing server
existing_pid=$(lsof -t -i:5001 || true)
if [ -n "$existing_pid" ]; then
    kill -9 $existing_pid && echo "✅ Existing server on port 5001 terminated."
fi

# Start ASP.NET Core app
dotnet run &
SERVER_PID=$!

# Wait until server responds
TIMEOUT=30
until curl -fsSL --cacert localhost-ca.crt https://localhost:5001/swagger/index.html &>/dev/null; do
    if [ "$TIMEOUT" -le 0 ]; then
        echo "❌ Server did not start within expected time."
        kill $SERVER_PID || true
        exit 1
    fi
    echo "Waiting for server to start..."
    sleep 1
    TIMEOUT=$((TIMEOUT - 1))
done
echo "✅ ASP.NET Core server started successfully."

# Verify Swagger UI explicitly via curl
curl -fsSL --cacert localhost-ca.crt https://localhost:5001/swagger/index.html | grep -q '<title>Swagger UI</title>' && echo "✅ Swagger UI verified via curl." || { echo "❌ Swagger UI failed to load via curl."; exit 1; }

# Verify Swagger UI explicitly via Chrome
if google-chrome --headless --disable-gpu --no-sandbox --dump-dom https://localhost:5001/swagger/index.html | grep -q 'swagger-ui'; then
    echo "✅ Swagger UI verified via Chrome headless."
else
    echo "❌ Swagger UI failed to load via Chrome headless."
    kill $SERVER_PID || true
    exit 1
fi

# Cleanup
kill $SERVER_PID && echo "✅ ASP.NET Core server stopped."