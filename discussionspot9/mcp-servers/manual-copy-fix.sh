#!/bin/bash
# Manual Fix Script - Copy MCP Server Files
# Use this if ubuntu-setup.sh didn't copy the files correctly

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

MCP_DIR="/var/www/discussionspot/mcp-servers"

echo -e "${YELLOW}🔧 Manual MCP Server File Copy${NC}\n"

# Check if we're in the right directory
if [ -f "seo-automation/main.py" ]; then
    SCRIPT_DIR="$(pwd)"
    echo -e "${GREEN}✓ Found MCP servers in current directory: $SCRIPT_DIR${NC}"
elif [ -f "mcp-servers/seo-automation/main.py" ]; then
    SCRIPT_DIR="$(pwd)/mcp-servers"
    echo -e "${GREEN}✓ Found MCP servers in mcp-servers subdirectory${NC}"
else
    echo -e "${RED}✗ MCP server files not found in current directory${NC}"
    echo -e "${YELLOW}Please run this script from the directory containing mcp-servers/ or seo-automation/${NC}"
    echo -e "${YELLOW}Current directory: $(pwd)${NC}"
    exit 1
fi

# Create destination directory
echo -e "\n${YELLOW}Creating destination directory...${NC}"
sudo mkdir -p "$MCP_DIR"
sudo chown -R $USER:$USER "$MCP_DIR"

# Copy SEO automation
if [ -f "$SCRIPT_DIR/seo-automation/main.py" ]; then
    echo -e "\n${YELLOW}Copying seo-automation...${NC}"
    if [ -d "$MCP_DIR/seo-automation" ]; then
        echo "  Removing existing directory..."
        rm -rf "$MCP_DIR/seo-automation"
    fi
    cp -r "$SCRIPT_DIR/seo-automation" "$MCP_DIR/"
    echo -e "${GREEN}✓ Copied seo-automation to $MCP_DIR/seo-automation${NC}"
    
    # Verify
    if [ -f "$MCP_DIR/seo-automation/main.py" ]; then
        echo -e "${GREEN}✓ Verified: main.py exists${NC}"
    else
        echo -e "${RED}✗ Error: main.py not found after copy${NC}"
        exit 1
    fi
else
    echo -e "${RED}✗ seo-automation/main.py not found in $SCRIPT_DIR${NC}"
fi

# Copy web-story-validator
if [ -f "$SCRIPT_DIR/web-story-validator/main.py" ]; then
    echo -e "\n${YELLOW}Copying web-story-validator...${NC}"
    if [ -d "$MCP_DIR/web-story-validator" ]; then
        echo "  Removing existing directory..."
        rm -rf "$MCP_DIR/web-story-validator"
    fi
    cp -r "$SCRIPT_DIR/web-story-validator" "$MCP_DIR/"
    echo -e "${GREEN}✓ Copied web-story-validator to $MCP_DIR/web-story-validator${NC}"
else
    echo -e "${YELLOW}⚠ web-story-validator not found (optional)${NC}"
fi

# Set permissions
echo -e "\n${YELLOW}Setting permissions...${NC}"
chmod +x "$MCP_DIR/seo-automation/main.py" 2>/dev/null || true
chmod +x "$MCP_DIR/web-story-validator/main.py" 2>/dev/null || true

# Final verification
echo -e "\n${GREEN}✅ Final Verification${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ -f "$MCP_DIR/seo-automation/main.py" ]; then
    echo -e "${GREEN}✓ SEO automation: $MCP_DIR/seo-automation/main.py${NC}"
    ls -lh "$MCP_DIR/seo-automation/main.py"
else
    echo -e "${RED}✗ SEO automation NOT found${NC}"
fi

if [ -f "$MCP_DIR/web-story-validator/main.py" ]; then
    echo -e "${GREEN}✓ Web story validator: $MCP_DIR/web-story-validator/main.py${NC}"
else
    echo -e "${YELLOW}⚠ Web story validator not found (optional)${NC}"
fi

echo -e "\n${GREEN}✅ Copy complete!${NC}"
echo -e "\n${YELLOW}Next steps:${NC}"
echo "1. Install dependencies: cd $MCP_DIR/seo-automation && python3 -m pip install --user -r requirements.txt"
echo "2. Test server: python3 $MCP_DIR/seo-automation/main.py"
echo "3. Check Cursor MCP diagnostics"

