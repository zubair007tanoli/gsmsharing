# ========================================
# Quick Start MCP Server (Simplified)
# PowerShell Version
# ========================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting MCP SEO Automation Server" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Set-Location "$PSScriptRoot\mcp-servers\seo-automation"

Write-Host "Installing dependencies (if needed)..." -ForegroundColor Yellow
& "C:\Users\zubai\AppData\Local\Python\bin\python.exe" -m pip install -q fastapi uvicorn requests pydantic 2>$null

Write-Host ""
Write-Host "Starting server on http://localhost:5001" -ForegroundColor Green
Write-Host "Health check: http://localhost:5001/health" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

& "C:\Users\zubai\AppData\Local\Python\bin\python.exe" main_simple.py

Read-Host "Press Enter to exit"

