@echo off
echo ========================================
echo Python AI Content Enhancement Service
echo ========================================
echo.

echo [1/5] Checking Python installation...
python --version
if %errorlevel% neq 0 (
    echo ERROR: Python not found! Please install Python 3.9 or higher.
    pause
    exit /b 1
)
echo.

echo [2/5] Creating virtual environment...
if not exist "venv" (
    python -m venv venv
    echo Virtual environment created.
) else (
    echo Virtual environment already exists.
)
echo.

echo [3/5] Activating virtual environment...
call venv\Scripts\activate.bat
echo.

echo [4/5] Installing dependencies...
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo ERROR: Failed to install dependencies!
    pause
    exit /b 1
)
echo.

echo [5/5] Downloading spaCy language model...
python -m spacy download en_core_web_sm
echo.

echo ========================================
echo Setup Complete!
echo ========================================
echo.
echo Starting the AI service...
echo Service will be available at: http://localhost:8000
echo.
echo Press Ctrl+C to stop the service.
echo.
python content_enhancer.py

pause

