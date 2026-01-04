# MCP Server - Python Installer & Configuration Script
# This script helps you install Python and configure MCP servers on Windows

Write-Host "================================================" -ForegroundColor Green
Write-Host "MCP Server Python Configuration" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

# Check if running as Administrator
$isAdmin = [bool]([Security.Principal.WindowsIdentity]::GetCurrent().Groups -match "S-1-5-32-544")
if (-not $isAdmin) {
    Write-Host "??  WARNING: This script should ideally run as Administrator" -ForegroundColor Yellow
    Write-Host "   Some operations may fail without admin privileges." -ForegroundColor Yellow
    Write-Host ""
}

# Step 1: Check current Python installation
Write-Host "Step 1: Checking Python Installation..." -ForegroundColor Cyan
Write-Host ""

$pythonFound = $false
$pythonPath = ""
$pythonVersion = ""

# Try to get Python path
try {
    $pythonPath = (python -c "import sys; print(sys.executable)" 2>$null)
    if ($pythonPath) {
        $pythonVersion = (python --version 2>&1)
        Write-Host "? Python found at: $pythonPath" -ForegroundColor Green
        Write-Host "? Version: $pythonVersion" -ForegroundColor Green
        $pythonFound = $true
    }
}
catch {
    # Python not found or is a stub
}

if (-not $pythonFound) {
    Write-Host "? Python not found or is a Windows Store stub" -ForegroundColor Red
    Write-Host ""
    Write-Host "You need to install Python from https://www.python.org/downloads/" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Follow these steps:" -ForegroundColor Yellow
    Write-Host "1. Go to https://www.python.org/downloads/" -ForegroundColor White
    Write-Host "2. Download Python 3.12 (or latest)" -ForegroundColor White
    Write-Host "3. Run the installer" -ForegroundColor White
    Write-Host "4. ? IMPORTANT: Check 'Add Python to PATH'" -ForegroundColor Yellow
    Write-Host "5. Click 'Install Now'" -ForegroundColor White
    Write-Host "6. After installation, close and re-run this script" -ForegroundColor White
    Write-Host ""
    
    Read-Host "Press Enter to continue (open Python website in browser, or skip)"
    
    # Optionally open Python download page
    $openBrowser = Read-Host "Open Python download page in browser? (y/n)"
    if ($openBrowser -eq 'y' -or $openBrowser -eq 'Y') {
        Start-Process "https://www.python.org/downloads/"
    }
    
    exit 1
}

Write-Host ""
Write-Host "Step 2: Checking MCP Dependencies..." -ForegroundColor Cyan
Write-Host ""

# Check if FastAPI is installed
try {
    python -c "import fastapi" 2>$null
    Write-Host "? FastAPI is installed" -ForegroundColor Green
}
catch {
    Write-Host "? FastAPI not installed" -ForegroundColor Red
    Write-Host "   Installing MCP dependencies..." -ForegroundColor Yellow
    
    # Install dependencies
    $seoPath = Join-Path (Get-Location) "discussionspot9\mcp-servers\seo-automation"
    
    if (Test-Path $seoPath) {
        cd $seoPath
        Write-Host "Installing from: $seoPath" -ForegroundColor White
        pip install -r requirements.txt
        cd ..
    }
    else {
        Write-Host "? Could not find seo-automation directory" -ForegroundColor Red
        Write-Host "   Please run this script from the project root directory" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host ""
Write-Host "Step 3: Updating appsettings.json..." -ForegroundColor Cyan
Write-Host ""

if ($pythonFound -and $pythonPath) {
    Write-Host "Python path to be used:" -ForegroundColor White
    Write-Host "  $pythonPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "This path should be added to appsettings.json:" -ForegroundColor Cyan
    
    # Convert backslashes to double backslashes for JSON
    $jsonPath = $pythonPath -replace '\\', '\\'
    Write-Host ""
    Write-Host "{" -ForegroundColor Gray
    Write-Host '  "Python": {' -ForegroundColor Gray
    Write-Host "    `"ExecutablePath`": `"$jsonPath`"," -ForegroundColor Yellow
    Write-Host '    "ScriptTimeout": 30' -ForegroundColor Gray
    Write-Host "  }" -ForegroundColor Gray
    Write-Host "}" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "Update appsettings.json manually or:" -ForegroundColor Cyan
    $updateConfig = Read-Host "Update appsettings.json now? (y/n)"
    
    if ($updateConfig -eq 'y' -or $updateConfig -eq 'Y') {
        $configPath = "appsettings.json"
        
        if (Test-Path $configPath) {
            # Read the JSON file
            $content = Get-Content $configPath -Raw
            
            # Replace the Python section
            # This is a simple approach - for production you'd use JSON parsers
            $pattern = '"Python":\s*\{\s*"ExecutablePath":\s*"[^"]*",'
            $replacement = "`"Python`": { `"ExecutablePath`": `"$jsonPath`","
            
            $newContent = $content -replace $pattern, $replacement
            
            # Write back
            $newContent | Set-Content $configPath
            Write-Host "? appsettings.json updated" -ForegroundColor Green
        }
        else {
            Write-Host "? appsettings.json not found" -ForegroundColor Red
            Write-Host "   Current directory: $(Get-Location)" -ForegroundColor White
        }
    }
}

Write-Host ""
Write-Host "Step 4: Final Checks..." -ForegroundColor Cyan
Write-Host ""

# Verify Python can import required modules
Write-Host "Testing Python imports..." -ForegroundColor White
python -c "import fastapi; print('? FastAPI OK')" 2>$null
python -c "import uvicorn; print('? uvicorn OK')" 2>$null
python -c "import pydantic; print('? pydantic OK')" 2>$null

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Verify appsettings.json was updated with Python path" -ForegroundColor White
Write-Host "2. Restart your application (F5 in Visual Studio)" -ForegroundColor White
Write-Host "3. Check logs for: 'Server SeoAutomation started on port 5001'" -ForegroundColor White
Write-Host "4. Test health: http://localhost:5001/health" -ForegroundColor White
Write-Host ""

Read-Host "Press Enter to close this window"
