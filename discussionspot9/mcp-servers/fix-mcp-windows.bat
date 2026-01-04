@echo off
REM MCP Server Fix for Windows - Fix hardcoded Python path

echo.
echo ======================================
echo MCP Server Configuration Fix - Windows
echo ======================================
echo.

REM Check if Python is installed
echo Checking Python installation...
python --version >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Python found
    python --version
) else (
    python3 --version >nul 2>&1
    if %errorlevel% equ 0 (
        echo [OK] Python3 found
        python3 --version
    ) else (
        echo [ERROR] Python not found in PATH
        echo Please install Python and add it to PATH
        pause
        exit /b 1
    )
)

REM Navigate to project directory
echo.
echo Changing to project directory...
cd /d "%~dp0..\.."
echo Current directory: %cd%

REM Check appsettings.json
echo.
echo Checking appsettings.json...
if exist "appsettings.json" (
    echo [OK] appsettings.json found
    echo.
    echo ? IMPORTANT: Edit appsettings.json and change:
    echo.
    echo FROM:
    echo   "Python": {
    echo     "ExecutablePath": "C:\\Users\\zubai\\...",
    echo.
    echo TO:
    echo   "Python": {
    echo     "ExecutablePath": "",
    echo.
    echo This allows auto-detection of Python on any machine.
    echo Press any key to open appsettings.json...
    pause
    start notepad appsettings.json
) else (
    echo [ERROR] appsettings.json not found
)

REM Install MCP dependencies
echo.
echo Checking MCP server dependencies...
cd /d "%~dp0seo-automation"
if exist "requirements.txt" (
    echo Installing FastAPI and dependencies...
    pip install -r requirements.txt
    if %errorlevel% equ 0 (
        echo [OK] Dependencies installed
    )
)

cd /d "%~dp0web-story-validator"
if exist "requirements.txt" (
    echo Installing web-story-validator dependencies...
    pip install -r requirements.txt
)

echo.
echo ======================================
echo Setup complete!
echo ======================================
echo.
echo Next steps:
echo 1. Edit appsettings.json and set ExecutablePath to empty string
echo 2. Restart Visual Studio or dotnet run
echo 3. Check logs for 'Server SeoAutomation started'
echo.
pause
