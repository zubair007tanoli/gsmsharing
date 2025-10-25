# PowerShell script to run the comment migration
# Run this from the discussionspot9 folder

Write-Host "Running Comment System Migration..." -ForegroundColor Cyan

# Update these variables to match your setup
$Server = "localhost"
$Database = "discussionspot9"

# Check if sqlcmd is available
$sqlcmdExists = Get-Command sqlcmd -ErrorAction SilentlyContinue

if ($sqlcmdExists) {
    Write-Host "Using sqlcmd to run migration..." -ForegroundColor Yellow
    sqlcmd -S $Server -d $Database -i "ADD_COMMENT_PIN_EDIT_COLUMNS.sql" -E
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migration completed successfully!" -ForegroundColor Green
        Write-Host "Next step: Restart your application" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Migration failed. Check error messages above." -ForegroundColor Red
    }
} else {
    Write-Host "sqlcmd not found. Please use SQL Server Management Studio instead." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Manual Steps:" -ForegroundColor Cyan
    Write-Host "1. Open SQL Server Management Studio" -ForegroundColor White
    Write-Host "2. Connect to your server: $Server" -ForegroundColor White
    Write-Host "3. Open file: ADD_COMMENT_PIN_EDIT_COLUMNS.sql" -ForegroundColor White
    Write-Host "4. Execute the script" -ForegroundColor White
    Write-Host "5. Restart your application" -ForegroundColor White
}

Write-Host ""
Read-Host "Press Enter to exit"

