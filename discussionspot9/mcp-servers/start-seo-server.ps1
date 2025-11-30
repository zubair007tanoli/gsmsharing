# Quick start script for SEO Automation MCP Server
# This starts only the SEO server

Write-Host "🚀 Starting SEO Automation MCP Server..." -ForegroundColor Green
Write-Host ""

# Check if Python is installed
$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    Write-Host "❌ Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Python from https://www.python.org/downloads/" -ForegroundColor Yellow
    exit 1
}

# Check if required packages are installed
Write-Host "📦 Checking Python dependencies..." -ForegroundColor Cyan
$packages = @("fastapi", "uvicorn", "requests")
$missing = @()

foreach ($package in $packages) {
    $installed = python -c "import $package" 2>$null
    if (-not $installed) {
        $missing += $package
    }
}

if ($missing.Count -gt 0) {
    Write-Host "⚠️  Missing packages: $($missing -join ', ')" -ForegroundColor Yellow
    Write-Host "Installing missing packages..." -ForegroundColor Cyan
    Set-Location "seo-automation"
    pip install -r requirements.txt
    Set-Location ..
}

# Change to SEO server directory
Set-Location "seo-automation"

Write-Host ""
Write-Host "✅ Starting server on http://localhost:5001" -ForegroundColor Green
Write-Host ""

# Start the server
python main.py

