@echo off
echo ========================================
echo Starting MCP Test Server (No Dependencies!)
echo ========================================
echo.

cd /d "%~dp0mcp-servers\seo-automation"

echo Starting server on http://localhost:5001
echo Health check: http://localhost:5001/health
echo.
echo This server uses ONLY Python standard library
echo No pip install needed!
echo.
echo Press Ctrl+C to stop
echo ========================================
echo.

C:\Users\zubai\AppData\Local\Python\bin\python.exe test_server.py

pause

