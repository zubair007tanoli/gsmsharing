@echo off
REM MCP Server Diagnostic Script
REM This script checks everything needed for MCP servers to work

setlocal enabledelayedexpansion

echo.
echo ================================================
echo MCP SERVER DIAGNOSTIC REPORT
echo ================================================
echo.

echo Step 1: Check Python Installation
echo ================================================
python --version 2>nul
if !errorlevel! equ 0 (
    echo [?] Python is installed
    for /f "tokens=*" %%i in ('python --version 2^>^&1') do set PYTHON_VERSION=%%i
    echo    Version: !PYTHON_VERSION!
    
    for /f "tokens=*" %%i in ('where python') do set PYTHON_PATH=%%i
    echo    Path: !PYTHON_PATH!
) else (
    echo [?] Python NOT found or not in PATH
    echo    Install from: https://www.python.org/downloads/
)

echo.
echo Step 2: Check FastAPI Installation
echo ================================================
python -c "import fastapi; print('[?] FastAPI installed')" 2>nul
if !errorlevel! neq 0 (
    echo [?] FastAPI NOT installed
    echo    Run: pip install fastapi
)

echo.
echo Step 3: Check Uvicorn Installation
echo ================================================
python -c "import uvicorn; print('[?] Uvicorn installed')" 2>nul
if !errorlevel! neq 0 (
    echo [?] Uvicorn NOT installed
    echo    Run: pip install uvicorn
)

echo.
echo Step 4: Check Pydantic Installation
echo ================================================
python -c "import pydantic; print('[?] Pydantic installed')" 2>nul
if !errorlevel! neq 0 (
    echo [?] Pydantic NOT installed
    echo    Run: pip install pydantic
)

echo.
echo Step 5: Check Port 5001 Availability
echo ================================================
netstat -ano | find ":5001" >nul
if !errorlevel! equ 0 (
    echo [!] Port 5001 is in use
    echo    Check: netstat -ano | find ":5001"
) else (
    echo [?] Port 5001 is available
)

echo.
echo Step 6: Check MCP Server Files
echo ================================================
if exist "discussionspot9\mcp-servers\seo-automation\main.py" (
    echo [?] seo-automation\main.py found
) else (
    echo [?] seo-automation\main.py NOT found
)

if exist "discussionspot9\mcp-servers\seo-automation\requirements.txt" (
    echo [?] seo-automation\requirements.txt found
) else (
    echo [?] seo-automation\requirements.txt NOT found
)

echo.
echo Step 7: Test Manual Server Start (this will run for 10 seconds)
echo ================================================
cd discussionspot9\mcp-servers\seo-automation 2>nul
if !errorlevel! equ 0 (
    echo Starting server for test...
    echo (Press Ctrl+C to stop)
    timeout /t 2 /nobreak
    python main.py 2>&1 | timeout /t 5
    echo.
    echo [?] Server test completed
) else (
    echo [?] Could not change to seo-automation directory
)

echo.
echo ================================================
echo DIAGNOSTIC REPORT COMPLETE
echo ================================================
echo.
echo Summary:
echo - If all checks show [?], server should work
echo - If any show [?], that's your problem
echo - If you see errors when starting server, note them
echo.
pause
