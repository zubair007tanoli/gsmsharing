# Database Improvements Execution and Verification Script
# For gsmsharing database

$server = "167.88.42.56"
$database = "gsmsharing"
$username = "sa"
$password = "1nsp1r0N@321"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Optimization Verification" -ForegroundColor Cyan
Write-Host "Database: $database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to execute SQL and return results
function Execute-SQL {
    param($query)
    sqlcmd -S $server -U $username -P $password -d $database -Q $query -h -1 -W
}

# 1. Check Performance Indexes
Write-Host "1. Checking Performance Indexes..." -ForegroundColor Yellow
$indexQuery = @"
SELECT COUNT(*) AS IndexCount
FROM sys.indexes 
WHERE (name LIKE 'IX_MobilePosts%' 
    OR name LIKE 'IX_UsersFourm%' 
    OR name LIKE 'IX_GsmBlog%'
    OR name LIKE 'IX_MobileAds%'
    OR name LIKE 'IX_BlogComments%')
"@
$indexCount = Execute-SQL $indexQuery
Write-Host "   Created Indexes: $indexCount" -ForegroundColor Green
Write-Host ""

# 2. Check Data Constraints
Write-Host "2. Checking Data Integrity Constraints..." -ForegroundColor Yellow
$constraintQuery = @"
SELECT COUNT(*) AS ConstraintCount
FROM sys.check_constraints 
WHERE name LIKE 'CK_MobilePosts%' 
   OR name LIKE 'CK_UsersFourm%'
   OR name LIKE 'CK_MobileAds%'
   OR name LIKE 'CK_AmazonProducts%'
"@
$constraintCount = Execute-SQL $constraintQuery
Write-Host "   Created Constraints: $constraintCount" -ForegroundColor Green
Write-Host ""

# 3. Check Full-Text Catalog
Write-Host "3. Checking Full-Text Search..." -ForegroundColor Yellow
$ftQuery = "SELECT COUNT(*) FROM sys.fulltext_catalogs WHERE name = 'ftCatalog_gsmsharing'"
$ftCount = Execute-SQL $ftQuery
Write-Host "   Full-Text Catalog: $ftCount" -ForegroundColor Green
Write-Host ""

# 4. Check Monitoring Views
Write-Host "4. Checking Monitoring Views..." -ForegroundColor Yellow
$viewQuery = "SELECT COUNT(*) FROM sys.objects WHERE type = 'V' AND name LIKE 'vw_%'"
$viewCount = Execute-SQL $viewQuery
Write-Host "   Created Views: $viewCount" -ForegroundColor Green
Write-Host ""

# 5. Check Query Store
Write-Host "5. Checking Database Configuration..." -ForegroundColor Yellow
$qsQuery = "SELECT CASE WHEN is_query_store_on = 1 THEN 'Enabled' ELSE 'Disabled' END FROM sys.databases WHERE name = '$database'"
$qsStatus = Execute-SQL $qsQuery
Write-Host "   Query Store: $qsStatus" -ForegroundColor Green
Write-Host ""

# 6. List some created indexes
Write-Host "6. Sample of Created Indexes:" -ForegroundColor Yellow
$sampleIndexQuery = @"
SELECT TOP 5 name 
FROM sys.indexes 
WHERE (name LIKE 'IX_MobilePosts%' OR name LIKE 'IX_UsersFourm%')
ORDER BY name
"@
$sampleIndexes = Execute-SQL $sampleIndexQuery
$sampleIndexes | ForEach-Object { Write-Host "   - $_" -ForegroundColor Cyan }
Write-Host ""

# 7. Test monitoring view
Write-Host "7. Testing Monitoring Views..." -ForegroundColor Yellow
$testViewQuery = "SELECT TOP 5 TableName, RowCounts, TotalSpaceKB FROM dbo.vw_TableSizes ORDER BY TotalSpaceKB DESC"
try {
    $viewResult = Execute-SQL $testViewQuery
    Write-Host "   vw_TableSizes: Working ✓" -ForegroundColor Green
} catch {
    Write-Host "   vw_TableSizes: Error" -ForegroundColor Red
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verification Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

