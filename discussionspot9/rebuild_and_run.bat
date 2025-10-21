@echo off
echo ============================================
echo  REBUILDING DISCUSSIONSPOT9 WITH POLL FIXES
echo ============================================
echo.

cd /d "%~dp0"

echo [1/4] Stopping any running dotnet processes...
taskkill /F /IM dotnet.exe /T >nul 2>&1
timeout /t 2 /nobreak >nul

echo [2/4] Cleaning old build artifacts...
dotnet clean

echo [3/4] Building project with poll voting fixes...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ BUILD FAILED! Check the errors above.
    echo.
    pause
    exit /b 1
)

echo.
echo ✅ BUILD SUCCESSFUL!
echo.
echo [4/4] Starting application...
echo.
echo ============================================
echo  APPLICATION STARTING
echo ============================================
echo.
echo Navigate to: http://localhost:5099/r/askdiscussion/posts/posinting-content-test
echo.
echo Press Ctrl+C to stop the application
echo.

dotnet run --no-build

pause

