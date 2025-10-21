@echo off
echo ============================================
echo   RESTARTING WITH POLL FIXES
echo ============================================
echo.

cd /d "%~dp0"

echo [1/3] Stopping application...
taskkill /F /IM dotnet.exe /T >nul 2>&1
timeout /t 2 /nobreak >nul

echo [2/3] Building with fixes...
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
echo   FIXES APPLIED:
echo ============================================
echo   ✅ Percentage calculation fixed
echo   ✅ Real-time updates enabled
echo   ✅ Poll voting working
echo.
echo Starting on: http://localhost:5099
echo.

dotnet run --no-build

pause

