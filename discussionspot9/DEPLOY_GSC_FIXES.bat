@echo off
echo ============================================
echo   DEPLOYING GOOGLE SEARCH CONSOLE FIXES
echo ============================================
echo.

cd /d "%~dp0"

echo [1/3] Stopping application...
taskkill /F /IM dotnet.exe /T >nul 2>&1
timeout /t 2 /nobreak >nul

echo [2/3] Building with GSC fixes...
dotnet build --no-incremental

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ BUILD FAILED!
    pause
    exit /b 1
)

echo.
echo ✅ BUILD SUCCESSFUL!
echo.
echo [3/3] Starting application...
echo.
echo ============================================
echo   GSC FIXES APPLIED:
echo ============================================
echo   ✅ Custom 404 error page
echo   ✅ Custom 500 error page  
echo   ✅ Canonical URL middleware
echo   ✅ Improved sitemap filtering
echo   ✅ Error logging enabled
echo.
echo 🧪 Test URLs:
echo   - Test 404: http://localhost:5099/nonexistent-page
echo   - Test Sitemap: http://localhost:5099/sitemap.xml
echo.

dotnet run --no-build

pause

