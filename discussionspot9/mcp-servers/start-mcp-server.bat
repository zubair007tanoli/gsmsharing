@echo off
REM MCP Server Startup Script for Windows
REM This script starts the SEO Automation MCP Server

echo ========================================
echo Starting MCP SEO Automation Server
echo ========================================
echo.

REM Set Python path (update if your Python is in a different location)
set PYTHON_EXE=C:\Users\zubai\AppData\Local\Python\bin\python.exe

REM Check if Python exists
if not exist "%PYTHON_EXE%" (
    echo ERROR: Python not found at %PYTHON_EXE%
    echo Please update the PYTHON_EXE variable in this script
    pause
    exit /b 1
)

echo Python found: %PYTHON_EXE%
%PYTHON_EXE% --version
echo.

REM Navigate to SEO automation directory
cd /d "%~dp0seo-automation"

echo Current directory: %CD%
echo.

REM Check if requirements are installed
echo Checking dependencies...
%PYTHON_EXE% -m pip list | findstr /i "fastapi uvicorn requests pydantic" >nul
if errorlevel 1 (
    echo Installing dependencies...
    %PYTHON_EXE% -m pip install -r requirements.txt
    if errorlevel 1 (
        echo ERROR: Failed to install dependencies
        pause
        exit /b 1
    )
) else (
    echo Dependencies already installed
)
echo.

REM Check if Ollama is running (optional)
echo Checking if Ollama is available...
curl -s http://localhost:11434/api/tags >nul 2>&1
if errorlevel 1 (
    echo WARNING: Ollama is not running or not installed
    echo The server will start but AI features won't work
    echo To install Ollama: https://ollama.com/download
    echo.
) else (
    echo Ollama is running!
    echo.
)

REM Start the server
echo Starting SEO Automation server on port 5001...
echo Press Ctrl+C to stop the server
echo.
%PYTHON_EXE% main.py

REM If server exits, pause to see error
if errorlevel 1 (
    echo.
    echo ERROR: Server exited with error code %errorlevel%
    pause
)

