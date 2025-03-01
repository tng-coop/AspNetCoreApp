#!/bin/bash
set -e

# Variables (modify as needed)
SA_PASSWORD="YourStrong!Passw0rd"
MSSQL_EDITION="Express"

# Install required dependencies
sudo apt update
sudo apt install -y libldap-common curl libldap2-dev

# Manually install missing OpenLDAP libraries for Ubuntu 24.04 compatibility
if ! dpkg -l | grep -q liblber-2.5-0; then
    wget -q http://archive.ubuntu.com/ubuntu/pool/main/o/openldap/libldap-2.5-0_2.5.16+dfsg-0ubuntu0.22.04.1_amd64.deb
    wget -q http://archive.ubuntu.com/ubuntu/pool/main/o/openldap/liblber-2.5-0_2.5.16+dfsg-0ubuntu0.22.04.1_amd64.deb
    sudo dpkg -i liblber-2.5-0_*.deb libldap-2.5-0_*.deb || sudo apt install -f -y
fi

# Stop SQL Server if running
if systemctl is-active --quiet mssql-server; then
    echo "Stopping SQL Server..."
    sudo systemctl stop mssql-server
fi

# Uninstall existing SQL Server packages if present
if dpkg -l | grep -q mssql-server; then
    echo "Removing existing SQL Server installation..."
    sudo apt-get remove --purge -y mssql-server mssql-tools unixodbc-dev
    sudo rm -rf /var/opt/mssql
fi

# Clean residual files
sudo apt-get autoremove -y
sudo apt-get clean

# Add Microsoft SQL Server repository if not already added
if [ ! -f "/etc/apt/sources.list.d/mssql-server.list" ]; then
    echo "Adding Microsoft SQL Server repository..."
    curl -sSL https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list | sudo tee /etc/apt/sources.list.d/mssql-server.list
    curl -sSL https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.gpg | \
        sudo gpg --dearmor | sudo tee /usr/share/keyrings/mssql.gpg >/dev/null
    sudo sed -i 's|signed-by=.*|signed-by=/usr/share/keyrings/mssql.gpg|' /etc/apt/sources.list.d/mssql-server.list
fi

# Add Microsoft SQL Tools repository if not already added
if [ ! -f "/etc/apt/sources.list.d/msprod.list" ]; then
    echo "Adding Microsoft SQL Tools repository..."
    curl -sSL https://packages.microsoft.com/config/ubuntu/22.04/prod.list | sudo tee /etc/apt/sources.list.d/msprod.list
    curl -sSL https://packages.microsoft.com/config/ubuntu/22.04/prod.gpg | \
        sudo gpg --dearmor | sudo tee /usr/share/keyrings/msprod.gpg >/dev/null
    sudo sed -i 's|signed-by=.*|signed-by=/usr/share/keyrings/msprod.gpg|' /etc/apt/sources.list.d/msprod.list
fi

# Install SQL Server Express
sudo apt-get update
sudo apt-get install -y mssql-server

# Configure SQL Server Express (idempotent)
if [ ! -f "/var/opt/mssql/mssql.conf" ]; then
    echo "Configuring SQL Server Express..."
    sudo MSSQL_SA_PASSWORD="$SA_PASSWORD" \
         MSSQL_PID="$MSSQL_EDITION" \
         /opt/mssql/bin/mssql-conf -n setup accept-eula
fi

# Start SQL Server service
sudo systemctl start mssql-server

# Install SQL Server command-line tools if missing
if ! command -v sqlcmd &> /dev/null; then
    echo "Installing SQL Server Tools (sqlcmd)..."
    sudo ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev

    # Add sqlcmd to system-wide path
    echo 'export PATH="$PATH:/opt/mssql-tools/bin"' | sudo tee /etc/profile.d/mssql-tools.sh
    source /etc/profile.d/mssql-tools.sh
fi

# Verify SQL Server installation
sleep 10  # Wait for SQL Server to initialize
sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -Q "SELECT @@VERSION;"

echo "SQL Server Express setup completed successfully!"
