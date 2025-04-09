#!/bin/bash

scriptdir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd ${scriptdir} || exit 1

set -euo pipefail

# --- Variables ---
CERT_PASSWORD="yourpassword"
CA_NAME="LocalhostDevelopmentCA"

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

echo "✅ Certificates generated successfully."
