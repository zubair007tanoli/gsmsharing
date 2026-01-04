# ?? MCP SERVER STARTUP - DIAGNOSTIC & FIX GUIDE

## Current Status

- ? Python path is empty (auto-detect enabled)
- ? MCP AutoStart is enabled
- ? Configuration looks correct
- ? MCP servers still not starting

---

## ?? Common Causes & Solutions

### **Cause 1: Python Dependencies Not Installed** (MOST LIKELY)

**Check if FastAPI is installed:**
```cmd
pip list | find "fastapi"
```

If nothing shows up, FastAPI is NOT installed.

**Fix:**
```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

**Then check:**
```cmd
pip list | find "fastapi"
pip list | find "uvicorn"
```

Should show both packages.

---

### **Cause 2: Python Not in System PATH** (SECOND MOST LIKELY)

Even with auto-detect enabled, if Python isn't in PATH, it won't work.

**Check if Python is in PATH:**
```cmd
where python
```

**If nothing shows up:**
- You need to install Python from https://www.python.org/downloads/
- Make sure "Add Python to PATH" is checked during installation

**After installing, verify:**
```cmd
python --version
where python
```

---

### **Cause 3: Python Version Issues**

Some older Python versions might have issues with FastAPI.

**Check Python version:**
```cmd
python --version
```

Should be 3.8 or higher (3.10+ recommended).

**If older:** Install Python 3.12 from https://www.python.org/downloads/

---

### **Cause 4: Firewall Blocking Port 5001**

MCP server starts on port 5001, but firewall might block it.

**Check if port 5001 is available:**
```cmd
netstat -ano | find ":5001"
```

If nothing shows, the port is available.

**If something shows:** Another process is using port 5001. Change MCP port in appsettings.json.

---

### **Cause 5: MCP Server Script Has Errors**

The Python script itself might have errors.

**Test server manually:**
```cmd
cd discussionspot9\mcp-servers\seo-automation
python main.py
```

**You should see:**
```
INFO:     Uvicorn running on http://0.0.0.0:5001
```

**If you see errors:**
- Note the error message
- Install missing Python packages
- Check Python version compatibility

---

## ?? Step-by-Step Diagnostic Process

### **Step 1: Verify Python Installation**

```cmd
python --version
# Expected: Python 3.8+

where python
# Expected: C:\Users\...\Python312\python.exe

python -c "import sys; print(sys.executable)"
# Expected: Full path to python.exe
```

**If any fail:** Install Python from https://www.python.org/downloads/

---

### **Step 2: Verify MCP Dependencies**

```cmd
pip list
# Look for: fastapi, uvicorn, pydantic, requests
```

**If missing:** Run:
```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

---

### **Step 3: Test MCP Server Manually**

```cmd
cd discussionspot9\mcp-servers\seo-automation
python main.py
```

**You should see:**
```
INFO:     Uvicorn running on http://0.0.0.0:5001
```

**If you see errors:**
- Read the error message carefully
- Try to install the missing package
- Example: `pip install fastapi uvicorn pydantic requests`

---

### **Step 4: Test Health Endpoint**

**While server is running from Step 3, open another Command Prompt:**

```cmd
curl http://localhost:5001/health
```

**Expected response:**
```json
{"status":"healthy","server":"SEO Automation",...}
```

**If connection refused:**
- Server not running
- Wrong port
- Firewall blocking

---

### **Step 5: Check Application Logs**

**In Visual Studio or running app:**
- Look for lines mentioning "MCP" or "Server"
- Look for "SeoAutomation"
- Look for "Python not found" or similar errors

**If you see:**
- `"Server SeoAutomation started"` ? Server is working! ?
- `"Failed to start"` ? Something failed, check error details
- `"Python not found"` ? Python not installed or not in PATH

---

## ?? If Manual Test Works But App Doesn't

**If Step 3 shows server running, but app doesn't connect:**

1. **Port configuration might be wrong**
   - Edit `appsettings.json`
   - Check `MCP:Servers:SeoAutomation:Port` is `5001`
   - Check `MCP:Servers:SeoAutomation:Endpoint` is `http://localhost:5001`

2. **Server starts after app tries to connect**
   - Server takes time to start
   - App gives up before server is ready
   - Solution: Restart app after server is running, or increase retry settings

3. **Firewall issue**
   - Try: `netstat -ano | find ":5001"`
   - If shows: `LISTENING` ? Server is running
   - If shows: Nothing ? Server not listening (something wrong)

---

## ?? Quick Fix Checklist

Run these commands one by one:

```cmd
REM 1. Check Python
python --version
where python

REM 2. Check FastAPI
pip list | find "fastapi"

REM 3. Install if missing
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt

REM 4. Test server
python main.py

REM 5. In another terminal, test health (while server runs)
curl http://localhost:5001/health

REM 6. If manual works, restart app
REM (Stop the manual server with Ctrl+C first)
```

---

## ?? Common Error Messages & Fixes

| Error | Meaning | Fix |
|-------|---------|-----|
| `Python not found` | Python not installed or not in PATH | Install from python.org |
| `ModuleNotFoundError: fastapi` | FastAPI not installed | `pip install fastapi` |
| `Connection refused` | Server not listening on port | Start server or check port |
| `Address already in use` | Port 5001 taken by another app | Change port in config |
| `Permission denied` | Firewall blocking | Check firewall settings |

---

## ?? What to Tell Me If It Still Doesn't Work

If it still doesn't work after trying these, tell me:

1. **Output of:** `python --version`
2. **Output of:** `where python`
3. **Output of:** `pip list | find "fastapi"`
4. **Output of:** `cd discussionspot9\mcp-servers\seo-automation && python main.py`
5. **Any error messages from the app logs**

With this info, I can help you fix it immediately.

---

## ? Quick Summary

**Most likely fixes (in order):**
1. Install Python dependencies: `pip install -r requirements.txt`
2. Install Python if missing from python.org
3. Test manually: `python main.py` in seo-automation folder
4. Check application logs for specific errors

**If you can tell me what error you see when running `python main.py` manually, I can give you exact fix!**
