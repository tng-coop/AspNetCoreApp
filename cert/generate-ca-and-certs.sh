#!/bin/bash

scriptdir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd ${scriptdir} || exit 1

set -euo pipefail

# --- Variables ---
CERT_PASSWORD="yourpassword"
CA_NAME="AspNetLanDevelopmentCA"
DOMAIN="aspnet.test"

# --- Cleanup existing certificates ---
rm -f ${DOMAIN}-ca.* ${DOMAIN}.* *.pfx *.pem *.csr *.srl

# --- Reset NSS DB ---
rm -rf "$HOME/.pki/nssdb"
mkdir -p "$HOME/.pki/nssdb"
certutil -d sql:"$HOME/.pki/nssdb" -N --empty-password && echo "✅ NSS DB reset."

# --- Generate Root CA ---
openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes \
  -keyout ${DOMAIN}-ca.key -out ${DOMAIN}-ca.crt \
  -subj "/CN=${CA_NAME}" -addext "basicConstraints=critical,CA:true"

# --- Trust CA (system & NSS DB) ---
sudo cp ${DOMAIN}-ca.crt /usr/local/share/ca-certificates/${CA_NAME}.crt
sudo update-ca-certificates || true
certutil -d sql:"$HOME/.pki/nssdb" -A -t "CT,C,C" -n "${CA_NAME}" -i ${DOMAIN}-ca.crt && echo "✅ Root CA trusted."

# --- Generate aspnet.test CSR ---
openssl req -newkey rsa:4096 -nodes \
  -keyout ${DOMAIN}.key -out ${DOMAIN}.csr \
  -subj "/CN=${DOMAIN}"

# --- Sign cert with SAN for aspnet.test ---
openssl x509 -req -in ${DOMAIN}.csr -CA ${DOMAIN}-ca.crt -CAkey ${DOMAIN}-ca.key \
  -CAcreateserial -out ${DOMAIN}.crt -days 3650 -sha256 \
  -extfile <(echo "subjectAltName=DNS:${DOMAIN}")

# --- Create PFX and PEM certificates ---
openssl pkcs12 -export -out ${DOMAIN}.pfx -inkey ${DOMAIN}.key \
  -in ${DOMAIN}.crt -passout pass:"${CERT_PASSWORD}"
openssl pkcs12 -in ${DOMAIN}.pfx -out ${DOMAIN}.pem -nodes -passin pass:"${CERT_PASSWORD}"


echo "✅ Certificates for ${DOMAIN} generated successfully."
