@echo off
REM ========================================
REM Quick Start MCP Server (Simplified)
REM ========================================

echo.
echo ========================================
echo Starting MCP SEO Automation Server
echo ========================================
echo.

cd /d "%~dp0mcp-servers\seo-automation"

echo Installing dependencies (if needed)...
C:\Users\zubai\AppData\Local\Python\bin\python.exe -m pip install -q fastapi uvicorn requests pydantic 2>nul

echo.
echo Starting server on http://localhost:5001
echo Health check: http://localhost:5001/health
echo.
echo Press Ctrl+C to stop the server
echo ========================================
echo.

C:\Users\zubai\AppData\Local\Python\bin\python.exe main_simple.py

pause

