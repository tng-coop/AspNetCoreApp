#!/bin/bash
set -e

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

echo "SQL Server removal and cleanup completed successfully!"
