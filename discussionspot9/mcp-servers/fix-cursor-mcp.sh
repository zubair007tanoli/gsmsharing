#!/bin/bash
# Fix Cursor MCP Configuration Script
# This script ensures the MCP server files exist and creates the Cursor config

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}🔧 Fixing Cursor MCP Configuration${NC}\n"

# Detect paths
APP_DIR="/var/www/discussionspot"
MCP_DIR="$APP_DIR/mcp-servers"
CURSOR_CONFIG_DIR="$APP_DIR/.cursor"
CURSOR_CONFIG="$CURSOR_CONFIG_DIR/mcp.json"

# Also check alternative paths
ALT_PATHS=(
    "/var/www/discussionspot9/mcp-servers"
    "$HOME/discussionspot/mcp-servers"
    "$(pwd)/mcp-servers"
)

echo -e "${YELLOW}Checking for MCP server files...${NC}"

# Find where the MCP servers actually are
FOUND_PATH=""
for path in "$MCP_DIR" "${ALT_PATHS[@]}"; do
    if [ -f "$path/seo-automation/main.py" ]; then
        FOUND_PATH="$path"
        echo -e "${GREEN}✓ Found MCP servers at: $FOUND_PATH${NC}"
        break
    fi
done

if [ -z "$FOUND_PATH" ]; then
    echo -e "${RED}✗ MCP server files not found!${NC}"
    echo -e "${YELLOW}Searched in:${NC}"
    echo "  - $MCP_DIR"
    for path in "${ALT_PATHS[@]}"; do
        echo "  - $path"
    done
    echo -e "\n${YELLOW}Please ensure MCP server files are copied to one of these locations.${NC}"
    echo -e "${YELLOW}Run: ./manual-copy-fix.sh or ./ubuntu-setup.sh${NC}"
    exit 1
fi

# Verify files exist
if [ ! -f "$FOUND_PATH/seo-automation/main.py" ]; then
    echo -e "${RED}✗ seo-automation/main.py not found${NC}"
    exit 1
fi

echo -e "${GREEN}✓ SEO automation server found${NC}"

if [ -f "$FOUND_PATH/web-story-validator/main.py" ]; then
    echo -e "${GREEN}✓ Web story validator server found${NC}"
    HAS_VALIDATOR=true
else
    echo -e "${YELLOW}⚠ Web story validator not found (optional)${NC}"
    HAS_VALIDATOR=false
fi

# Create Cursor config directory
echo -e "\n${YELLOW}Creating Cursor MCP configuration...${NC}"
mkdir -p "$CURSOR_CONFIG_DIR"

# Create MCP config
cat > "$CURSOR_CONFIG" <<EOF
{
  "mcpServers": {
    "seo-automation": {
      "command": "python3",
      "args": [
        "$FOUND_PATH/seo-automation/main.py"
      ],
      "env": {
        "PYTHONUNBUFFERED": "1"
      }
    }
EOF

if [ "$HAS_VALIDATOR" = true ]; then
    cat >> "$CURSOR_CONFIG" <<EOF
    ,
    "web-story-validator": {
      "command": "python3",
      "args": [
        "$FOUND_PATH/web-story-validator/main.py"
      ],
      "env": {
        "PYTHONUNBUFFERED": "1"
      }
    }
EOF
fi

cat >> "$CURSOR_CONFIG" <<EOF
  }
}
EOF

echo -e "${GREEN}✓ Created Cursor MCP config at: $CURSOR_CONFIG${NC}"

# Verify Python can run the script
echo -e "\n${YELLOW}Verifying Python can execute the script...${NC}"
if python3 -c "import sys; sys.path.insert(0, '$FOUND_PATH/seo-automation'); import main" 2>/dev/null; then
    echo -e "${GREEN}✓ Python can import the script${NC}"
else
    echo -e "${YELLOW}⚠ Python import check failed (this is OK if dependencies aren't installed)${NC}"
    echo -e "${YELLOW}  Install dependencies: cd $FOUND_PATH/seo-automation && pip3 install -r requirements.txt${NC}"
fi

# Check if script is executable
if [ -x "$FOUND_PATH/seo-automation/main.py" ]; then
    echo -e "${GREEN}✓ Script is executable${NC}"
else
    echo -e "${YELLOW}⚠ Making script executable...${NC}"
    chmod +x "$FOUND_PATH/seo-automation/main.py"
    echo -e "${GREEN}✓ Script is now executable${NC}"
fi

# Summary
echo -e "\n${GREEN}✅ Configuration Complete!${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo -e "${GREEN}MCP Server Path:${NC} $FOUND_PATH"
echo -e "${GREEN}Cursor Config:${NC} $CURSOR_CONFIG"
echo -e "\n${YELLOW}Next steps:${NC}"
echo "1. Restart Cursor to load the new configuration"
echo "2. Go to Cursor Settings → MCP Servers"
echo "3. Click 'Autostart' for seo-automation"
echo "4. Check diagnostics to verify it's working"

# Show the config file
echo -e "\n${BLUE}Configuration file contents:${NC}"
cat "$CURSOR_CONFIG"

