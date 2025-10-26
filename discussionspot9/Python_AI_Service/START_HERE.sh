#!/bin/bash

echo "========================================"
echo "Python AI Content Enhancement Service"
echo "========================================"
echo ""

echo "[1/5] Checking Python installation..."
python3 --version
if [ $? -ne 0 ]; then
    echo "ERROR: Python not found! Please install Python 3.9 or higher."
    exit 1
fi
echo ""

echo "[2/5] Creating virtual environment..."
if [ ! -d "venv" ]; then
    python3 -m venv venv
    echo "Virtual environment created."
else
    echo "Virtual environment already exists."
fi
echo ""

echo "[3/5] Activating virtual environment..."
source venv/bin/activate
echo ""

echo "[4/5] Installing dependencies..."
pip install -r requirements.txt
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to install dependencies!"
    exit 1
fi
echo ""

echo "[5/5] Downloading spaCy language model..."
python -m spacy download en_core_web_sm
echo ""

echo "========================================"
echo "Setup Complete!"
echo "========================================"
echo ""
echo "Starting the AI service..."
echo "Service will be available at: http://localhost:8000"
echo ""
echo "Press Ctrl+C to stop the service."
echo ""
python content_enhancer.py

