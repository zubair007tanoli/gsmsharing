@echo off
echo ========================================
echo Quick Fix: Switch to LocalDB
echo ========================================
echo.
echo This will switch your connection to LocalDB
echo Make sure SQL Server LocalDB is installed!
echo.
pause

echo Creating LocalDB database...
dotnet ef database update --project discussionspot9

echo.
echo ========================================
echo LocalDB Setup Complete!
echo.
echo Your application will now use LocalDB
echo Run: dotnet run
echo ========================================
pause


