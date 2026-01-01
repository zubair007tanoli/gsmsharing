# PowerShell script to run database migration
# This script executes CreateForumPostsAndAffiliateTables.sql on gsmsharingv4 database

param(
    [string]$Server = "167.88.42.56",
    [string]$Database = "gsmsharingv4",
    [string]$UserId = "sa",
    [string]$Password = "1nsp1r0N@321"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "GSMSharing V2 - Database Migration Script" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Get the script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$sqlFile = Join-Path $scriptPath "CreateForumPostsAndAffiliateTables.sql"

if (-not (Test-Path $sqlFile)) {
    Write-Host "ERROR: SQL file not found at: $sqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "SQL File: $sqlFile" -ForegroundColor Green
Write-Host "Target Database: $Database" -ForegroundColor Green
Write-Host "Server: $Server" -ForegroundColor Green
Write-Host ""

# Read SQL file
$sqlContent = Get-Content $sqlFile -Raw

# Build connection string
$connectionString = "Server=$Server;Database=$Database;User Id=$UserId;Password=$Password;TrustServerCertificate=True;"

Write-Host "Connecting to database..." -ForegroundColor Yellow

try {
    # Load SQL Server module if available, otherwise use sqlcmd
    if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
        Write-Host "Using sqlcmd..." -ForegroundColor Yellow
        
        # Create a temporary SQL file with USE statement
        $tempSqlFile = [System.IO.Path]::GetTempFileName() + ".sql"
        $fullSql = "USE [$Database];`nGO`n$sqlContent"
        Set-Content -Path $tempSqlFile -Value $fullSql
        
        # Execute using sqlcmd
        $sqlcmdArgs = @(
            "-S", $Server
            "-d", $Database
            "-U", $UserId
            "-P", $Password
            "-i", $tempSqlFile
            "-C"  # Trust server certificate
        )
        
        $result = & sqlcmd @sqlcmdArgs 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Migration completed successfully!" -ForegroundColor Green
            Write-Host $result
        } else {
            Write-Host "Migration failed!" -ForegroundColor Red
            Write-Host $result
            exit 1
        }
        
        # Clean up temp file
        Remove-Item $tempSqlFile -ErrorAction SilentlyContinue
        
    } elseif (Get-Module -ListAvailable -Name SqlServer) {
        Write-Host "Using SqlServer PowerShell module..." -ForegroundColor Yellow
        Import-Module SqlServer -ErrorAction Stop
        
        # Execute SQL
        Invoke-Sqlcmd -ServerInstance $Server -Database $Database -Username $UserId -Password $Password -InputFile $sqlFile -TrustServerCertificate
        
        Write-Host "Migration completed successfully!" -ForegroundColor Green
        
    } else {
        Write-Host "ERROR: Neither sqlcmd nor SqlServer PowerShell module found." -ForegroundColor Red
        Write-Host ""
        Write-Host "Please install one of the following:" -ForegroundColor Yellow
        Write-Host "1. SQL Server Command Line Utilities (sqlcmd)" -ForegroundColor Yellow
        Write-Host "2. SqlServer PowerShell module: Install-Module -Name SqlServer" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Or manually execute the SQL file in SQL Server Management Studio:" -ForegroundColor Yellow
        Write-Host "  File: $sqlFile" -ForegroundColor Cyan
        Write-Host "  Database: $Database" -ForegroundColor Cyan
        exit 1
    }
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Execute the SQL file manually in SQL Server Management Studio" -ForegroundColor Yellow
    Write-Host "  File: $sqlFile" -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Migration script execution completed!" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan







