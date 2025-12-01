#!/bin/bash
# MCP Server Installation Verification Script
# This script checks if MCP servers are properly installed and configured

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}🔍 MCP Server Installation Verification${NC}\n"

# Common paths to check
PATHS=(
    "/var/www/discussionspot/mcp-servers/seo-automation/main.py"
    "/var/www/discussionspot9/mcp-servers/seo-automation/main.py"
    "$(pwd)/mcp-servers/seo-automation/main.py"
    "$HOME/discussionspot/mcp-servers/seo-automation/main.py"
)

# Check Python
echo -e "${YELLOW}Checking Python installation...${NC}"
if command -v python3 &> /dev/null; then
    PYTHON_VER=$(python3 --version)
    PYTHON_PATH=$(which python3)
    echo -e "${GREEN}✓ Python installed: $PYTHON_VER${NC}"
    echo -e "${GREEN}  Path: $PYTHON_PATH${NC}"
else
    echo -e "${RED}✗ Python 3 not found${NC}"
    echo -e "${YELLOW}  Install with: sudo apt-get install python3 python3-pip${NC}"
    exit 1
fi

# Check pip
echo -e "\n${YELLOW}Checking pip installation...${NC}"
if command -v pip3 &> /dev/null || python3 -m pip --version &> /dev/null; then
    echo -e "${GREEN}✓ pip is available${NC}"
else
    echo -e "${RED}✗ pip not found${NC}"
    echo -e "${YELLOW}  Install with: sudo apt-get install python3-pip${NC}"
fi

# Check for MCP server scripts
echo -e "\n${YELLOW}Checking for MCP server scripts...${NC}"
FOUND=false
for path in "${PATHS[@]}"; do
    if [ -f "$path" ]; then
        echo -e "${GREEN}✓ Found: $path${NC}"
        FOUND=true
        
        # Check if it's executable
        if [ -x "$path" ]; then
            echo -e "${GREEN}  ✓ File is executable${NC}"
        else
            echo -e "${YELLOW}  ⚠ File is not executable (run: chmod +x $path)${NC}"
        fi
        
        # Check directory permissions
        DIR=$(dirname "$path")
        if [ -r "$DIR" ] && [ -w "$DIR" ]; then
            echo -e "${GREEN}  ✓ Directory is readable/writable${NC}"
        else
            echo -e "${RED}  ✗ Directory permissions issue${NC}"
        fi
        break
    fi
done

if [ "$FOUND" = false ]; then
    echo -e "${RED}✗ MCP server script not found in any of these locations:${NC}"
    for path in "${PATHS[@]}"; do
        echo -e "${YELLOW}  - $path${NC}"
    done
    echo -e "\n${YELLOW}To fix:${NC}"
    echo "1. Run: ./ubuntu-setup.sh"
    echo "2. Or manually copy mcp-servers directory to /var/www/discussionspot/"
fi

# Check Python dependencies
echo -e "\n${YELLOW}Checking Python dependencies...${NC}"
if [ "$FOUND" = true ]; then
    SERVER_DIR=$(dirname "$path")
    if [ -f "$SERVER_DIR/requirements.txt" ]; then
        echo -e "${GREEN}✓ requirements.txt found${NC}"
        echo -e "${YELLOW}  Checking installed packages...${NC}"
        
        # Check for key dependencies
        DEPS=("fastapi" "uvicorn" "pydantic")
        for dep in "${DEPS[@]}"; do
            if python3 -c "import $dep" 2>/dev/null; then
                echo -e "${GREEN}    ✓ $dep installed${NC}"
            else
                echo -e "${RED}    ✗ $dep NOT installed${NC}"
                echo -e "${YELLOW}      Install with: pip3 install -r $SERVER_DIR/requirements.txt${NC}"
            fi
        done
    else
        echo -e "${RED}✗ requirements.txt not found${NC}"
    fi
fi

# Check if servers are running
echo -e "\n${YELLOW}Checking if servers are running...${NC}"
if curl -s http://localhost:5001/health > /dev/null 2>&1; then
    echo -e "${GREEN}✓ SEO Automation server is running on port 5001${NC}"
else
    echo -e "${YELLOW}⚠ SEO Automation server is not running${NC}"
    echo -e "${YELLOW}  Start with: python3 $SERVER_DIR/main.py${NC}"
fi

# Summary
echo -e "\n${BLUE}📊 Summary${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if [ "$FOUND" = true ] && command -v python3 &> /dev/null; then
    echo -e "${GREEN}✓ Installation looks good!${NC}"
    echo -e "\n${YELLOW}To start the server:${NC}"
    echo "  cd $(dirname "$path")"
    echo "  python3 main.py"
else
    echo -e "${RED}✗ Installation incomplete${NC}"
    echo -e "\n${YELLOW}Run the setup script:${NC}"
    echo "  ./ubuntu-setup.sh"
fi

