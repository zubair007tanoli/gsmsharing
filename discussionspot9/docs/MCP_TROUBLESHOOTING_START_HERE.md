# ?? MCP SERVER TROUBLESHOOTING - START HERE

Since you can't copy the browser error, here's how to diagnose the problem:

---

## ? Quick Fix (Most Likely to Work)

**Open Command Prompt and run:**

```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

**Then restart Visual Studio (F5)**

---

## ?? If That Doesn't Work - Run Diagnostic

**Double-click this file:**
```
discussionspot9\diagnose-mcp.bat
```

This script will:
- ? Check if Python is installed
- ? Check if FastAPI is installed
- ? Check if port 5001 is available
- ? Try to start the server and show any errors

**Look for ? symbols in the output** - those are your problems.

---

## ?? What the Diagnostic Checks

| Check | Status | What to do if ? |
|-------|--------|-----------------|
| Python installed | ? or ? | Install from python.org |
| FastAPI installed | ? or ? | Run `pip install -r requirements.txt` |
| Uvicorn installed | ? or ? | Run `pip install -r requirements.txt` |
| Pydantic installed | ? or ? | Run `pip install -r requirements.txt` |
| Port 5001 free | ? or ? | Close other app or change port |
| main.py exists | ? or ? | Copy mcp-servers folder |

---

## ?? Most Common Issues & Fixes

### **Issue 1: FastAPI/Dependencies Missing** (80% of cases)
**Fix:**
```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

### **Issue 2: Python Not Installed**
**Fix:** Download from https://www.python.org/downloads/

### **Issue 3: Port 5001 Already In Use**
**Fix:** Check `netstat -ano | find ":5001"` or change port in appsettings.json

### **Issue 4: MCP Files Missing**
**Fix:** Run `git pull` or manually copy mcp-servers folder

---

## ?? Step by Step

1. **Run diagnostic:**
   ```
   Double-click: discussionspot9\diagnose-mcp.bat
   ```

2. **Check output for ? symbols**

3. **Fix each ?:**
   - FastAPI missing? ? `pip install -r requirements.txt`
   - Python missing? ? Install from python.org
   - Port in use? ? Change port in config
   - Files missing? ? Git pull

4. **Restart app** (F5)

5. **Check logs** for "Server SeoAutomation started"

---

## ?? Full Guides

- **Quick Troubleshooting:** `discussionspot9\docs\MCP_QUICK_TROUBLESHOOTING.md`
- **Diagnostic Details:** `discussionspot9\docs\MCP_DIAGNOSTIC_NO_ERROR_COPY.md`
- **Python Issues:** `discussionspot9\docs\PYTHON_NOT_FOUND_FIX.md`

---

## ?? What to Tell Me If It Still Doesn't Work

Run this and tell me the output:
```cmd
cd discussionspot9\mcp-servers\seo-automation
python main.py
```

Just copy the first few lines of output - that will tell me exactly what's wrong!

---

**TL;DR: Run `diagnose-mcp.bat` and fix any ? symbols**
