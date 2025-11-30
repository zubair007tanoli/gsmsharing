# Install Python Dependencies for MCP Servers
Write-Host "📦 Installing Python Dependencies for MCP Servers..." -ForegroundColor Cyan
Write-Host ""

# Check if Python is installed
$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    Write-Host "❌ Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Python from https://www.python.org/downloads/" -ForegroundColor Yellow
    Write-Host "Make sure to check 'Add Python to PATH' during installation" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Python found: $($python.Source)" -ForegroundColor Green
Write-Host ""

# Change to SEO server directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location "$scriptDir\seo-automation"

Write-Host "Installing dependencies from requirements.txt..." -ForegroundColor Cyan
pip install -r requirements.txt

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Dependencies installed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Make sure Ollama is running: ollama serve" -ForegroundColor White
    Write-Host "2. Download model: ollama pull llama3.2" -ForegroundColor White
    Write-Host "3. Restart your application" -ForegroundColor White
    Write-Host "4. Check admin dashboard: http://localhost:5099/admin/mcp-status" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "❌ Failed to install dependencies" -ForegroundColor Red
    Write-Host "Try running manually: pip install -r requirements.txt" -ForegroundColor Yellow
}

Set-Location $scriptDir

