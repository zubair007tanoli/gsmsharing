#!/bin/bash
# Deploy MCP Servers to Ubuntu - Complete Setup
# This script ensures files are in place before running setup

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}🚀 MCP Server Deployment to Ubuntu${NC}\n"

APP_DIR="/var/www/discussionspot"
MCP_DIR="$APP_DIR/mcp-servers"
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo -e "${YELLOW}Current directory: $(pwd)${NC}"
echo -e "${YELLOW}Script directory: $SCRIPT_DIR${NC}"
echo -e "${YELLOW}Target directory: $MCP_DIR${NC}\n"

# Step 1: Check if we're in the right location
echo -e "${GREEN}Step 1: Checking source files...${NC}"

if [ -f "$SCRIPT_DIR/seo-automation/main.py" ]; then
    echo -e "${GREEN}✓ Found seo-automation in script directory${NC}"
    SOURCE_DIR="$SCRIPT_DIR"
elif [ -f "seo-automation/main.py" ]; then
    echo -e "${GREEN}✓ Found seo-automation in current directory${NC}"
    SOURCE_DIR="$(pwd)"
elif [ -f "../mcp-servers/seo-automation/main.py" ]; then
    echo -e "${GREEN}✓ Found seo-automation in parent/mcp-servers${NC}"
    SOURCE_DIR="$(cd .. && pwd)/mcp-servers"
else
    echo -e "${RED}✗ Cannot find seo-automation/main.py${NC}"
    echo -e "${YELLOW}Please run this script from:${NC}"
    echo -e "${YELLOW}  - The mcp-servers directory, OR${NC}"
    echo -e "${YELLOW}  - The project root (discussionspot9/)${NC}"
    exit 1
fi

# Step 2: Create destination directory
echo -e "\n${GREEN}Step 2: Creating destination directory...${NC}"
sudo mkdir -p "$MCP_DIR"
sudo chown -R $USER:$USER "$MCP_DIR"
echo -e "${GREEN}✓ Directory created: $MCP_DIR${NC}"

# Step 3: Copy files
echo -e "\n${GREEN}Step 3: Copying MCP server files...${NC}"

# Copy SEO automation
if [ -d "$SOURCE_DIR/seo-automation" ]; then
    echo "Copying seo-automation..."
    if [ -d "$MCP_DIR/seo-automation" ]; then
        echo "  Removing existing directory..."
        rm -rf "$MCP_DIR/seo-automation"
    fi
    cp -r "$SOURCE_DIR/seo-automation" "$MCP_DIR/"
    echo -e "${GREEN}✓ SEO automation copied${NC}"
    
    # Verify
    if [ -f "$MCP_DIR/seo-automation/main.py" ]; then
        echo -e "${GREEN}  ✓ Verified: main.py exists${NC}"
    else
        echo -e "${RED}  ✗ Error: main.py not found after copy${NC}"
        exit 1
    fi
else
    echo -e "${RED}✗ seo-automation directory not found in $SOURCE_DIR${NC}"
    exit 1
fi

# Copy web-story-validator (optional)
if [ -d "$SOURCE_DIR/web-story-validator" ]; then
    echo "Copying web-story-validator..."
    if [ -d "$MCP_DIR/web-story-validator" ]; then
        echo "  Removing existing directory..."
        rm -rf "$MCP_DIR/web-story-validator"
    fi
    cp -r "$SOURCE_DIR/web-story-validator" "$MCP_DIR/"
    echo -e "${GREEN}✓ Web story validator copied${NC}"
else
    echo -e "${YELLOW}⚠ web-story-validator not found (optional)${NC}"
fi

# Step 4: Set permissions
echo -e "\n${GREEN}Step 4: Setting permissions...${NC}"
chmod +x "$MCP_DIR/seo-automation/main.py" 2>/dev/null || true
chmod +x "$MCP_DIR/web-story-validator/main.py" 2>/dev/null || true
echo -e "${GREEN}✓ Permissions set${NC}"

# Step 5: Install dependencies
echo -e "\n${GREEN}Step 5: Installing Python dependencies...${NC}"

if [ -f "$MCP_DIR/seo-automation/requirements.txt" ]; then
    echo "Installing dependencies for seo-automation..."
    cd "$MCP_DIR/seo-automation"
    python3 -m pip install --user -r requirements.txt
    echo -e "${GREEN}✓ SEO automation dependencies installed${NC}"
fi

if [ -f "$MCP_DIR/web-story-validator/requirements.txt" ]; then
    echo "Installing dependencies for web-story-validator..."
    cd "$MCP_DIR/web-story-validator"
    python3 -m pip install --user -r requirements.txt
    echo -e "${GREEN}✓ Web story validator dependencies installed${NC}"
fi

# Step 6: Create Cursor MCP config
echo -e "\n${GREEN}Step 6: Creating Cursor MCP configuration...${NC}"
CURSOR_CONFIG_DIR="$APP_DIR/.cursor"
mkdir -p "$CURSOR_CONFIG_DIR"

cat > "$CURSOR_CONFIG_DIR/mcp.json" <<EOF
{
  "mcpServers": {
    "seo-automation": {
      "command": "python3",
      "args": [
        "$MCP_DIR/seo-automation/main.py"
      ],
      "env": {
        "PYTHONUNBUFFERED": "1"
      }
    }
EOF

if [ -f "$MCP_DIR/web-story-validator/main.py" ]; then
    cat >> "$CURSOR_CONFIG_DIR/mcp.json" <<EOF
    ,
    "web-story-validator": {
      "command": "python3",
      "args": [
        "$MCP_DIR/web-story-validator/main.py"
      ],
      "env": {
        "PYTHONUNBUFFERED": "1"
      }
    }
EOF
fi

cat >> "$CURSOR_CONFIG_DIR/mcp.json" <<EOF
  }
}
EOF

echo -e "${GREEN}✓ Cursor MCP config created: $CURSOR_CONFIG_DIR/mcp.json${NC}"

# Step 7: Final verification
echo -e "\n${GREEN}Step 7: Final verification...${NC}"

if [ -f "$MCP_DIR/seo-automation/main.py" ]; then
    echo -e "${GREEN}✓ SEO automation: $MCP_DIR/seo-automation/main.py${NC}"
else
    echo -e "${RED}✗ SEO automation NOT found${NC}"
    exit 1
fi

if command -v python3 &> /dev/null; then
    PYTHON_VER=$(python3 --version)
    echo -e "${GREEN}✓ Python: $PYTHON_VER${NC}"
else
    echo -e "${RED}✗ Python not found${NC}"
    exit 1
fi

echo -e "\n${GREEN}✅ Deployment Complete!${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo -e "${GREEN}MCP Server Location:${NC} $MCP_DIR/seo-automation/main.py"
echo -e "${GREEN}Cursor Config:${NC} $CURSOR_CONFIG_DIR/mcp.json"
echo -e "\n${YELLOW}Next steps:${NC}"
echo "1. Restart Cursor"
echo "2. Go to Settings → MCP Servers"
echo "3. Click 'Autostart' for seo-automation"
echo "4. Check diagnostics to verify"

