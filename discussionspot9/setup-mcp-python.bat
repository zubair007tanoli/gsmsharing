@echo off
REM MCP Server Python Configuration - Windows Batch Script
REM This script helps diagnose and fix Python issues for MCP servers

setlocal enabledelayedexpansion

echo.
echo ================================================
echo MCP Server Python Configuration - Windows
echo ================================================
echo.

REM Check Python installation
echo Checking Python installation...
python --version >nul 2>&1
if !errorlevel! equ 0 (
    echo [OK] Python found
    for /f "tokens=*" %%i in ('python --version 2^>^&1') do set PYTHON_VERSION=%%i
    echo Version: !PYTHON_VERSION!
    
    REM Get Python path
    for /f "tokens=*" %%i in ('python -c "import sys; print(sys.executable)" 2^>nul') do set PYTHON_PATH=%%i
    echo Path: !PYTHON_PATH!
) else (
    echo [ERROR] Python not found or is a Windows Store stub
    echo.
    echo Solution: Install Python from https://www.python.org/downloads/
    echo.
    echo Installation Steps:
    echo 1. Go to https://www.python.org/downloads/
    echo 2. Download Python 3.12 ^(or latest^)
    echo 3. Run installer
    echo 4. CHECK: "Add Python to PATH"
    echo 5. Click "Install Now"
    echo 6. Run this script again
    echo.
    pause
    exit /b 1
)

echo.
echo Checking FastAPI installation...
python -c "import fastapi" >nul 2>&1
if !errorlevel! equ 0 (
    echo [OK] FastAPI found
) else (
    echo [INFO] Installing MCP dependencies...
    cd discussionspot9\mcp-servers\seo-automation
    pip install -r requirements.txt
    if !errorlevel! equ 0 (
        echo [OK] Dependencies installed
    ) else (
        echo [ERROR] Failed to install dependencies
        pause
        exit /b 1
    )
)

echo.
echo ================================================
echo Python Configuration
echo ================================================
echo.
echo Python Path to use in appsettings.json:
echo !PYTHON_PATH!
echo.
echo Update your appsettings.json with this path:
echo.
echo {
echo   "Python": {
echo     "ExecutablePath": "!PYTHON_PATH!",
echo     "ScriptTimeout": 30
echo   }
echo }
echo.
echo Note: Use double backslashes in JSON: C:\\Users\\...\\python.exe
echo.
echo ================================================
echo Next Steps:
echo ================================================
echo 1. Edit appsettings.json with the path above
echo 2. Restart Visual Studio ^(F5^)
echo 3. Check application logs
echo 4. Look for: "Server SeoAutomation started on port 5001"
echo 5. Test: http://localhost:5001/health
echo.
pause
