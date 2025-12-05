@echo off
REM Simple MCP Server Startup - No Ollama Required for Testing

echo Starting MCP SEO Automation Server (Simple Mode)...
cd /d "%~dp0seo-automation"

REM Use Python from PATH or specific location
set PYTHON_EXE=C:\Users\zubai\AppData\Local\Python\bin\python.exe

REM Quick dependency install
%PYTHON_EXE% -m pip install -q fastapi uvicorn requests pydantic 2>nul

REM Start server
echo Server starting on http://localhost:5001
echo Health check: http://localhost:5001/health
echo.
%PYTHON_EXE% -m uvicorn main:app --host 0.0.0.0 --port 5001 --reload

pause

