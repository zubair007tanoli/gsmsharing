#!/bin/bash
# Ubuntu 22.04 MCP Server Setup Script
# This script installs Python, dependencies, and sets up MCP servers

set -e  # Exit on error

echo "🚀 Starting MCP Server Setup for Ubuntu 22.04..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
APP_DIR="/var/www/discussionspot"
MCP_DIR="$APP_DIR/mcp-servers"

echo -e "${YELLOW}📁 Detected paths:${NC}"
echo "  Script directory: $SCRIPT_DIR"
echo "  App directory: $APP_DIR"
echo "  MCP directory: $MCP_DIR"

# Step 1: Install Python 3 and pip
echo -e "\n${GREEN}Step 1: Installing Python 3 and pip...${NC}"
if command -v python3 &> /dev/null; then
    PYTHON_VERSION=$(python3 --version)
    echo -e "${GREEN}✓ Python already installed: $PYTHON_VERSION${NC}"
else
    echo "Installing Python 3..."
    sudo apt-get update
    sudo apt-get install -y python3 python3-pip python3-venv
    echo -e "${GREEN}✓ Python 3 installed${NC}"
fi

# Verify Python installation
PYTHON_CMD=$(which python3)
echo -e "${GREEN}✓ Python found at: $PYTHON_CMD${NC}"

# Step 2: Create MCP servers directory if it doesn't exist
echo -e "\n${GREEN}Step 2: Setting up MCP servers directory...${NC}"
if [ ! -d "$MCP_DIR" ]; then
    echo "Creating MCP directory: $MCP_DIR"
    sudo mkdir -p "$MCP_DIR"
    sudo chown -R $USER:$USER "$MCP_DIR"
fi

# Step 3: Copy MCP server files
echo -e "\n${GREEN}Step 3: Copying MCP server files...${NC}"

# Check multiple possible source locations
SOURCE_DIRS=(
    "$SCRIPT_DIR"
    "$(dirname "$SCRIPT_DIR")/mcp-servers"
    "$APP_DIR/mcp-servers"
    "/var/www/discussionspot9/mcp-servers"
    "$HOME/discussionspot/mcp-servers"
)

FOUND_SOURCE=""
for dir in "${SOURCE_DIRS[@]}"; do
    if [ -f "$dir/seo-automation/main.py" ]; then
        FOUND_SOURCE="$dir"
        echo -e "${GREEN}✓ Found seo-automation source at: $FOUND_SOURCE${NC}"
        break
    fi
done

# Copy SEO automation
if [ -n "$FOUND_SOURCE" ] && [ -f "$FOUND_SOURCE/seo-automation/main.py" ]; then
    echo "Copying seo-automation server from $FOUND_SOURCE..."
    if [ -d "$MCP_DIR/seo-automation" ]; then
        echo "  Removing existing directory..."
        rm -rf "$MCP_DIR/seo-automation"
    fi
    cp -r "$FOUND_SOURCE/seo-automation" "$MCP_DIR/"
    echo -e "${GREEN}✓ SEO automation server copied${NC}"
elif [ -f "$MCP_DIR/seo-automation/main.py" ]; then
    echo -e "${GREEN}✓ SEO automation server already exists at destination${NC}"
else
    echo -e "${RED}✗ seo-automation not found${NC}"
    echo -e "${YELLOW}Searched in:${NC}"
    for dir in "${SOURCE_DIRS[@]}"; do
        echo -e "${YELLOW}  - $dir/seo-automation/main.py${NC}"
    done
    echo -e "\n${YELLOW}⚠ Files need to be copied to the server first.${NC}"
    echo -e "${YELLOW}  Option 1: Deploy via git (git pull)${NC}"
    echo -e "${YELLOW}  Option 2: Copy files manually via SCP/SFTP${NC}"
    echo -e "${YELLOW}  Option 3: Run from the directory containing mcp-servers/${NC}"
fi

# Copy web-story-validator
if [ -n "$FOUND_SOURCE" ] && [ -f "$FOUND_SOURCE/web-story-validator/main.py" ]; then
    echo "Copying web-story-validator server from $FOUND_SOURCE..."
    if [ -d "$MCP_DIR/web-story-validator" ]; then
        echo "  Removing existing directory..."
        rm -rf "$MCP_DIR/web-story-validator"
    fi
    cp -r "$FOUND_SOURCE/web-story-validator" "$MCP_DIR/"
    echo -e "${GREEN}✓ Web story validator server copied${NC}"
elif [ -f "$MCP_DIR/web-story-validator/main.py" ]; then
    echo -e "${GREEN}✓ Web story validator server already exists at destination${NC}"
else
    echo -e "${YELLOW}⚠ web-story-validator not found (optional)${NC}"
fi

# Step 4: Install Python dependencies for each server
echo -e "\n${GREEN}Step 4: Installing Python dependencies...${NC}"

# SEO Automation Server
if [ -d "$MCP_DIR/seo-automation" ]; then
    echo "Installing dependencies for seo-automation..."
    cd "$MCP_DIR/seo-automation"
    if [ -f "requirements.txt" ]; then
        python3 -m pip install --user -r requirements.txt
        echo -e "${GREEN}✓ SEO automation dependencies installed${NC}"
    else
        echo -e "${RED}✗ requirements.txt not found in seo-automation${NC}"
    fi
fi

# Web Story Validator Server
if [ -d "$MCP_DIR/web-story-validator" ]; then
    echo "Installing dependencies for web-story-validator..."
    cd "$MCP_DIR/web-story-validator"
    if [ -f "requirements.txt" ]; then
        python3 -m pip install --user -r requirements.txt
        echo -e "${GREEN}✓ Web story validator dependencies installed${NC}"
    else
        echo -e "${RED}✗ requirements.txt not found in web-story-validator${NC}"
    fi
fi

# Step 5: Set proper permissions
echo -e "\n${GREEN}Step 5: Setting permissions...${NC}"
chmod +x "$MCP_DIR/seo-automation/main.py" 2>/dev/null || true
chmod +x "$MCP_DIR/web-story-validator/main.py" 2>/dev/null || true
echo -e "${GREEN}✓ Permissions set${NC}"

# Step 6: Verify installation
echo -e "\n${GREEN}Step 6: Verifying installation...${NC}"

# Check Python
if command -v python3 &> /dev/null; then
    PYTHON_VER=$(python3 --version)
    echo -e "${GREEN}✓ Python: $PYTHON_VER${NC}"
else
    echo -e "${RED}✗ Python not found${NC}"
    exit 1
fi

# Check if main.py exists
if [ -f "$MCP_DIR/seo-automation/main.py" ]; then
    echo -e "${GREEN}✓ SEO automation server script found${NC}"
else
    echo -e "${RED}✗ SEO automation server script NOT found at: $MCP_DIR/seo-automation/main.py${NC}"
    echo -e "${YELLOW}  Searched paths:${NC}"
    echo "    - $MCP_DIR/seo-automation/main.py"
    echo "    - $APP_DIR/mcp-servers/seo-automation/main.py"
    echo "    - $SCRIPT_DIR/seo-automation/main.py"
fi

# Check if web-story-validator exists
if [ -f "$MCP_DIR/web-story-validator/main.py" ]; then
    echo -e "${GREEN}✓ Web story validator server script found${NC}"
else
    echo -e "${YELLOW}⚠ Web story validator server script not found (optional)${NC}"
fi

# Step 7: Create systemd service files (optional)
echo -e "\n${GREEN}Step 7: Creating systemd service files...${NC}"
echo -e "${YELLOW}Note: This is optional. You can run servers manually or use systemd.${NC}"

# Create service file for SEO automation
SERVICE_FILE="/etc/systemd/system/mcp-seo-automation.service"
if [ ! -f "$SERVICE_FILE" ]; then
    echo "Creating systemd service for SEO automation..."
    sudo tee "$SERVICE_FILE" > /dev/null <<EOF
[Unit]
Description=MCP SEO Automation Server
After=network.target

[Service]
Type=simple
User=$USER
WorkingDirectory=$MCP_DIR/seo-automation
Environment="PATH=/usr/bin:/usr/local/bin"
ExecStart=$PYTHON_CMD $MCP_DIR/seo-automation/main.py
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF
    echo -e "${GREEN}✓ Service file created${NC}"
    echo -e "${YELLOW}  To enable: sudo systemctl enable mcp-seo-automation${NC}"
    echo -e "${YELLOW}  To start: sudo systemctl start mcp-seo-automation${NC}"
else
    echo -e "${YELLOW}⚠ Service file already exists${NC}"
fi

echo -e "\n${GREEN}✅ Setup complete!${NC}"
echo -e "\n${YELLOW}Next steps:${NC}"
echo "1. Verify Python: python3 --version"
echo "2. Test SEO server: python3 $MCP_DIR/seo-automation/main.py"
echo "3. Check MCP diagnostics in Cursor"
echo "4. (Optional) Start as service: sudo systemctl start mcp-seo-automation"

