#!/bin/bash
# Pdfpeaks Ubuntu Server Setup Script
# Run this script as: sudo bash setup_ubuntu.sh

set -e

echo "======================================"
echo "Pdfpeaks Ubuntu Server Setup"
echo "======================================"

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo "Please run as root or with sudo"
    exit 1
fi

echo "[1/7] Updating package lists..."
apt-get update -y

echo "[2/7] Installing Python runtime tooling..."
apt-get install -y \
    python3 \
    python3-pip \
    python3-venv

echo "[3/7] Installing LibreOffice and fonts..."
apt-get install -y \
    libreoffice \
    libreoffice-writer \
    libreoffice-calc \
    libreoffice-impress \
    fonts-noto \
    fonts-noto-cjk \
    fonts-dejavu \
    fonts-freefont \
    fonts-linux-libertine \
    fonts-liberation \
    fonts-ubuntu \
    fonts-arphic-uming \
    fonts-arphic-ukai

echo "[4/7] Refreshing font cache..."
fc-cache -f -v

echo "[5/7] Installing Python dependencies for conversion scripts..."
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REQ_FILE="$SCRIPT_DIR/requirements.txt"
if [ -f "$REQ_FILE" ]; then
    python3 -m pip install --upgrade pip
    python3 -m pip install -r "$REQ_FILE"
else
    echo "Warning: requirements.txt not found at $REQ_FILE"
fi

echo "[6/7] Verifying LibreOffice installation..."
if command -v libreoffice &> /dev/null; then
    echo "LibreOffice installed successfully:"
    libreoffice --version
else
    echo "Warning: LibreOffice not found in PATH"
fi

echo "[7/7] Creating necessary directories..."
mkdir -p /var/www/pdfpeaks/temp_files
mkdir -p /var/www/pdfpeaks/logs
chown -R www-data:www-data /var/www/pdfpeaks

echo "======================================"
echo "Setup completed successfully!"
echo "======================================"
echo ""
echo "Next steps:"
echo "1. Build and run the Docker container"
echo "2. Or deploy the .NET application directly"
echo ""
