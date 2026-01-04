#!/bin/bash
# MCP Server Fix for Ubuntu - Run this after deployment

echo "?? MCP Server Configuration Fix for Ubuntu"
echo "=========================================="

# Step 1: Verify Python installation
echo -e "\n?? Step 1: Checking Python installation..."
if ! command -v python3 &> /dev/null; then
    echo "? Python3 not found. Installing..."
    sudo apt-get update
    sudo apt-get install -y python3 python3-pip python3-venv
else
    python3 --version
    echo "? Python3 found"
fi

# Step 2: Verify pip installation
echo -e "\n?? Step 2: Checking pip installation..."
if ! command -v pip3 &> /dev/null; then
    echo "? pip3 not found. Installing..."
    sudo apt-get install -y python3-pip
else
    pip3 --version
    echo "? pip3 found"
fi

# Step 3: Install MCP server dependencies
echo -e "\n?? Step 3: Installing MCP server dependencies..."

APP_DIR="/var/www/discussionspot"

if [ ! -d "$APP_DIR/mcp-servers/seo-automation" ]; then
    echo "? SEO Automation server files not found at $APP_DIR/mcp-servers/seo-automation"
    echo "   Please ensure mcp-servers folder is copied to $APP_DIR"
    exit 1
fi

# Install dependencies
cd "$APP_DIR/mcp-servers/seo-automation"
echo "Installing seo-automation dependencies..."
pip3 install --user -r requirements.txt

if [ -d "$APP_DIR/mcp-servers/web-story-validator" ]; then
    cd "$APP_DIR/mcp-servers/web-story-validator"
    echo "Installing web-story-validator dependencies..."
    pip3 install --user -r requirements.txt
fi

echo "? Dependencies installed"

# Step 4: Make scripts executable
echo -e "\n?? Step 4: Setting script permissions..."
chmod +x "$APP_DIR/mcp-servers/seo-automation/main.py"
if [ -f "$APP_DIR/mcp-servers/web-story-validator/main.py" ]; then
    chmod +x "$APP_DIR/mcp-servers/web-story-validator/main.py"
fi
echo "? Permissions set"

# Step 5: Test Python import
echo -e "\n?? Step 5: Testing Python dependencies..."
python3 -c "import fastapi; print('? FastAPI is installed')" || echo "? FastAPI not found"
python3 -c "import uvicorn; print('? uvicorn is installed')" || echo "? uvicorn not found"

# Step 6: Update appsettings.Production.json (if it exists)
echo -e "\n?? Step 6: Verifying appsettings configuration..."
if [ -f "$APP_DIR/appsettings.Production.json" ]; then
    if grep -q '"ExecutablePath": ""' "$APP_DIR/appsettings.Production.json" 2>/dev/null; then
        echo "? Python path is empty (will auto-detect)"
    elif grep -q '"/usr/bin/python3"' "$APP_DIR/appsettings.Production.json" 2>/dev/null; then
        echo "? Python path is set to /usr/bin/python3"
    else
        echo "??  Check appsettings.Production.json Python path"
        echo "   Recommended: 'ExecutablePath': '/usr/bin/python3'"
    fi
fi

# Step 7: Test manual server startup
echo -e "\n?? Step 7: Testing MCP server startup (manual)..."
echo "Starting SEO Automation server for test (30 seconds)..."
timeout 5 python3 "$APP_DIR/mcp-servers/seo-automation/main.py" &
STARTUP_PID=$!
sleep 3

# Test health endpoint
if curl -s http://localhost:5001/health > /dev/null 2>&1; then
    echo "? SEO server health check passed!"
else
    echo "??  Could not reach health endpoint (server might still be starting)"
fi

wait $STARTUP_PID 2>/dev/null

echo -e "\n" 
echo "=========================================="
echo "? MCP Server setup complete!"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. Restart your ASP.NET application"
echo "2. Check application logs for 'Server SeoAutomation started'"
echo "3. The servers should auto-start with the application"
echo ""
echo "To manually test:"
echo "  curl http://localhost:5001/health"
echo ""
