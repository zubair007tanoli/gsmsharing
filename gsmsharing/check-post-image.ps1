# PowerShell script to check if a specific post has an image
# Usage: .\check-post-image.ps1

Write-Host "=== Checking Post Image Status ===" -ForegroundColor Cyan
Write-Host ""

# Check if directory exists
$uploadDir = "wwwroot\uploads\posts\featured"
Write-Host "Checking upload directory..." -ForegroundColor Yellow
if (Test-Path $uploadDir) {
    Write-Host "✅ Directory exists: $uploadDir" -ForegroundColor Green
    
    # List files
    $files = Get-ChildItem $uploadDir -File -ErrorAction SilentlyContinue
    if ($files) {
        Write-Host "Files in directory:" -ForegroundColor Yellow
        $files | ForEach-Object {
            Write-Host "  📁 $($_.Name) - $($_.Length) bytes - Created: $($_.CreationTime)" -ForegroundColor Cyan
        }
    } else {
        Write-Host "⚠️  No files found in directory" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ Directory does not exist: $uploadDir" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Database Query Instructions ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "To check the database, run one of these commands:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Option 1 - Using SQL file:" -ForegroundColor Green
Write-Host '  sqlcmd -S localhost -d gsmsharing_dev -E -i CHECK_POST_IMAGES.sql' -ForegroundColor White
Write-Host ""
Write-Host "Option 2 - Direct query:" -ForegroundColor Green
Write-Host '  sqlcmd -S localhost -d gsmsharing_dev -E -Q "SELECT TOP 5 PostID, Title, Slug, FeaturedImage, CreatedAt FROM Posts ORDER BY CreatedAt DESC"' -ForegroundColor White
Write-Host ""
Write-Host "Option 3 - Check specific post:" -ForegroundColor Green
Write-Host '  sqlcmd -S localhost -d gsmsharing_dev -E -Q "SELECT * FROM Posts WHERE Slug LIKE ''%apple-liquid-glass-tint%''"' -ForegroundColor White
Write-Host ""

# Check if sqlcmd is available
if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
    Write-Host "✅ sqlcmd is available" -ForegroundColor Green
    Write-Host ""
    Write-Host "Running automatic check..." -ForegroundColor Yellow
    
    try {
        # Try to query the database
        $query = "SELECT TOP 5 PostID, Title, LEFT(FeaturedImage, 50) AS FeaturedImage, CreatedAt FROM Posts ORDER BY CreatedAt DESC"
        sqlcmd -S localhost -d gsmsharing_dev -E -Q $query -h -1 -W 2>&1 | Write-Host
    } catch {
        Write-Host "⚠️  Could not automatically query database. Please run manually." -ForegroundColor Yellow
        Write-Host "Error: $_" -ForegroundColor Red
    }
} else {
    Write-Host "⚠️  sqlcmd not found. Install SQL Server command line tools to query the database." -ForegroundColor Yellow
    Write-Host "Or use SQL Server Management Studio (SSMS) to run the queries." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Next Steps ===" -ForegroundColor Cyan
Write-Host "1. Restart the application: dotnet run" -ForegroundColor White
Write-Host "2. Create a test post with an image" -ForegroundColor White
Write-Host "3. Check the console logs for detailed information" -ForegroundColor White
Write-Host "4. Verify the image displays in the post" -ForegroundColor White
Write-Host ""

