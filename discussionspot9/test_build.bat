@echo off
cd /d "%~dp0"
echo Stopping processes...
taskkill /F /IM dotnet.exe /T >nul 2>&1
timeout /t 2 /nobreak >nul
echo Building...
dotnet build
if %ERRORLEVEL% EQU 0 (
    echo ✅ BUILD SUCCESSFUL!
) else (
    echo ❌ BUILD FAILED!
)
pause

