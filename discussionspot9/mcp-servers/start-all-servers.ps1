# PowerShell script to start all MCP servers
# Run this script to start all MCP servers at once

Write-Host "🚀 Starting MCP Servers..." -ForegroundColor Green
Write-Host ""

# Check if Python is installed
$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    Write-Host "❌ Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Python from https://www.python.org/downloads/" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Python found: $($python.Source)" -ForegroundColor Green
Write-Host ""

# Change to script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# Start SEO Automation Server (Port 5001)
Write-Host "📦 Starting SEO Automation Server (Port 5001)..." -ForegroundColor Cyan
Start-Process python -ArgumentList "seo-automation/main.py" -WindowStyle Normal
Start-Sleep -Seconds 2

# Start Performance Server (Port 5002) - Not created yet, so skip
# Write-Host "📦 Starting Performance Server (Port 5002)..." -ForegroundColor Cyan
# Start-Process python -ArgumentList "performance/main.py" -WindowStyle Normal
# Start-Sleep -Seconds 2

# Start User Preferences Server (Port 5003) - Not created yet, so skip
# Write-Host "📦 Starting User Preferences Server (Port 5003)..." -ForegroundColor Cyan
# Start-Process python -ArgumentList "user-preferences/main.py" -WindowStyle Normal
# Start-Sleep -Seconds 2

Write-Host ""
Write-Host "✅ MCP Servers starting..." -ForegroundColor Green
Write-Host ""
Write-Host "📋 Server Status:" -ForegroundColor Yellow
Write-Host "  - SEO Automation: http://localhost:5001" -ForegroundColor White
Write-Host "  - Performance: http://localhost:5002 (Not implemented yet)" -ForegroundColor Gray
Write-Host "  - User Preferences: http://localhost:5003 (Not implemented yet)" -ForegroundColor Gray
Write-Host ""
Write-Host "💡 Check admin dashboard at: http://localhost:5099/admin/mcp-status" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️  Note: Make sure Ollama is running for SEO server to work!" -ForegroundColor Yellow
Write-Host "   Run: ollama serve" -ForegroundColor Yellow

