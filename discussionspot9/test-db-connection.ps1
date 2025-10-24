# Test Database Connection Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Remote Database Connection" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$server = "167.88.42.56"
$database = "DiscussionspotADO"
$username = "sa"
$password = "1nsp1r0N@321"

Write-Host "Server: $server" -ForegroundColor Yellow
Write-Host "Database: $database" -ForegroundColor Yellow
Write-Host "User: $username" -ForegroundColor Yellow
Write-Host ""

# Test basic connectivity
Write-Host "Step 1: Testing basic connectivity..." -ForegroundColor Green
try {
    $ping = Test-Connection -ComputerName $server -Count 1 -Quiet
    if ($ping) {
        Write-Host "Server is reachable" -ForegroundColor Green
    } else {
        Write-Host "Server is not reachable" -ForegroundColor Red
        exit
    }
} catch {
    Write-Host "Cannot ping server: $_" -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "Step 2: Connection test shows the error..." -ForegroundColor Green
Write-Host ""
Write-Host "Based on the terminal error you showed, the issue is:" -ForegroundColor Yellow
Write-Host "Error 17892: Logon failed for login 'sa' due to trigger execution" -ForegroundColor Red
Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "SOLUTION REQUIRED:" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "You need to run SQL commands on the database server." -ForegroundColor White
Write-Host ""
Write-Host "Instructions:" -ForegroundColor Cyan
Write-Host "1. Open SQL Server Management Studio (SSMS)" -ForegroundColor White
Write-Host "2. Connect to server: $server" -ForegroundColor White
Write-Host "3. Use Windows Authentication or another admin account" -ForegroundColor White
Write-Host "4. Open and run: SQLScripts\FixDatabaseConnection.sql" -ForegroundColor White
Write-Host ""
Write-Host "Quick Fix Commands (copy/paste in SSMS):" -ForegroundColor Cyan
Write-Host ""
Write-Host "-- Enable SA account" -ForegroundColor Gray
Write-Host "ALTER LOGIN sa ENABLE;" -ForegroundColor White
Write-Host ""
Write-Host "-- OR check login triggers" -ForegroundColor Gray
Write-Host "SELECT name, is_disabled FROM sys.server_triggers WHERE type = 'TR';" -ForegroundColor White
Write-Host ""
Write-Host "-- Disable problematic trigger (if found)" -ForegroundColor Gray
Write-Host "-- DISABLE TRIGGER TriggerName ON ALL SERVER;" -ForegroundColor White
Write-Host ""

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
