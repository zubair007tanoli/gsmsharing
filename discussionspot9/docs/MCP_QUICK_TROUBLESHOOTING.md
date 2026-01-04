# ?? MCP SERVERS NOT STARTING - QUICK TROUBLESHOOTING

## 3 Steps to Find the Problem

### **Step 1: Run Diagnostic Script**

Double-click this file:
```
discussionspot9\diagnose-mcp.bat
```

This will:
- Check if Python is installed ? or ?
- Check if FastAPI is installed ? or ?
- Check if dependencies are installed ? or ?
- Try to start the server manually
- Show you any errors

---

### **Step 2: Read the Diagnostic Output**

The script will show you things like:

? means it's working
? means it's broken

**Example output:**
```
[?] Python is installed
    Version: Python 3.12.1
    Path: C:\Users\zubai\AppData\Local\Programs\Python\Python312\python.exe
    
[?] FastAPI NOT installed
    Run: pip install fastapi
    
[?] Port 5001 is available
```

---

### **Step 3: Fix the ? Items**

| If you see ? | Do this |
|---|---|
| `[?] Python NOT found` | Install from https://www.python.org/downloads/ |
| `[?] FastAPI NOT installed` | Run: `pip install -r requirements.txt` |
| `[?] Uvicorn NOT installed` | Run: `pip install -r requirements.txt` |
| `[?] seo-automation\main.py NOT found` | Copy mcp-servers folder or git pull |

---

## What Each ? Means

### **? Python NOT found**
**Problem:** Python not installed or not in PATH  
**Fix:** https://www.python.org/downloads/ (check "Add to PATH")

### **? FastAPI NOT installed**
**Problem:** Missing Python dependencies  
**Fix:**
```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

### **? Port 5001 is in use**
**Problem:** Another app is using port 5001  
**Fix:** Change port in appsettings.json or close the other app

### **? main.py NOT found**
**Problem:** MCP server files not copied  
**Fix:** Git pull or copy mcp-servers folder

---

## If Diagnostic Shows Errors

When you run the diagnostic script, if it tries to start the server and shows errors, those errors will tell you exactly what's wrong.

**Common errors:**

```
ModuleNotFoundError: No module named 'fastapi'
? Run: pip install fastapi

Permission denied
? Run as Administrator or check file permissions

Address already in use
? Change port in appsettings.json
```

---

## What to Do Next

1. **Run:** `discussionspot9\diagnose-mcp.bat`
2. **Read the output** - look for ? and ?
3. **Fix any ? items** using the table above
4. **Restart the application** (F5 in Visual Studio)
5. **Check app logs** for "Server SeoAutomation started"

---

## If You See Errors in Diagnostic

**Tell me:**
1. The full error message you see
2. Which ? and ? items show in the diagnostic
3. The output when it tries to start the server

With that info, I can give you the exact fix!

---

## Quick Commands Reference

```cmd
REM Check Python
python --version
where python

REM Check FastAPI
pip list | find "fastapi"

REM Install dependencies
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt

REM Start server manually
python main.py

REM Check if port is in use
netstat -ano | find ":5001"

REM Test health endpoint
curl http://localhost:5001/health
```

---

## Most Common Fix

99% of the time, the fix is:
```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

Then restart the application.

If that doesn't work, run the diagnostic and tell me what it shows!
