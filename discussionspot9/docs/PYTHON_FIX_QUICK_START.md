# ?? MCP SERVER FIX - PYTHON NOT FOUND (WINDOWS STORE STUBS)

## ?? The Problem

```
Failed to start SeoAutomation. Error: Python not found.
Both 'python' and 'python3' appear to be Windows Store stubs.
```

**What this means:** You don't have real Python installed. Windows has stub shortcuts that open the Microsoft Store instead.

---

## ? The Solution (4 Simple Steps)

### **Step 1: Remove Windows Store Python Stubs** ?? 2 minutes

**Open PowerShell as Administrator:**
- Press `Win+X` on your keyboard
- Click: **"Windows PowerShell (Admin)"** (or Terminal)

**Run these commands:**
```powershell
Remove-Item -Path "$env:LOCALAPPDATA\Microsoft\WindowsApps\python.exe" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$env:LOCALAPPDATA\Microsoft\WindowsApps\python3.exe" -Force -ErrorAction SilentlyContinue
```

---

### **Step 2: Download & Install Real Python** ?? 5 minutes

1. **Go to:** https://www.python.org/downloads/
2. **Click:** "Download Python 3.12.1" (or latest stable)
3. **Run the installer**
4. **IMPORTANT:** Check this box:
   - ? **"Add Python to PATH"** (CRITICAL!)
5. **Click:** "Install Now"
6. **Wait for completion**

---

### **Step 3: Verify Python Installation** ?? 1 minute

**Open Command Prompt and run:**
```cmd
python --version
```

**You should see:**
```
Python 3.12.1
```

**If you see the Microsoft Store opening:** You didn't install real Python. Go back to Step 1.

---

### **Step 4: Get Your Python Path** ?? 1 minute

**In Command Prompt, run:**
```cmd
python -c "import sys; print(sys.executable)"
```

**You'll see something like:**
```
C:\Users\zubai\AppData\Local\Programs\Python\Python312\python.exe
```

**Copy this entire path - you'll need it next.**

---

## ?? Update appsettings.json

### **Step 1: Open appsettings.json**
- File: `discussionspot9\appsettings.json`
- Open in Visual Studio or any text editor

### **Step 2: Find this section:**
```json
"Python": {
  "ExecutablePath": "",
  "ScriptTimeout": 30
}
```

### **Step 3: Replace with YOUR path** (from Step 4 above):
```json
"Python": {
  "ExecutablePath": "C:\\Users\\zubai\\AppData\\Local\\Programs\\Python\\Python312\\python.exe",
  "ScriptTimeout": 30
}
```

**?? IMPORTANT:** Use **double backslashes** `\\` in JSON!

### **Step 4: Install MCP Dependencies**

**In Command Prompt:**
```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt

cd ..\web-story-validator
pip install -r requirements.txt
```

---

## ?? Restart & Test

### **Restart Application**
- Visual Studio: Press **F5**
- Command Line: Run `dotnet run` again

### **Check Application Logs**
Look for this message:
```
? Auto-starting SeoAutomation...
? Server SeoAutomation started on port 5001
```

### **Test Health Endpoint**
Open browser and go to:
```
http://localhost:5001/health
```

You should see:
```json
{
  "status": "healthy",
  "server": "SEO Automation",
  "ai_available": true
}
```

---

## ?? Automated Setup Scripts

We've created scripts to help you:

### **Option A: Batch Script (Easy)**
```cmd
discussionspot9\setup-mcp-python.bat
```
Just double-click and follow prompts.

### **Option B: PowerShell Script (More Features)**
```powershell
# Run from PowerShell (must run as Admin):
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\discussionspot9\setup-mcp-python.ps1
```

---

## ?? Checklist

- [ ] **Removed Windows Store stubs** (PowerShell commands)
- [ ] **Downloaded Python** from python.org
- [ ] **Installed Python** with "Add to PATH" checked
- [ ] **Verified:** `python --version` shows version number
- [ ] **Got Python path:** `python -c "import sys; print(sys.executable)"`
- [ ] **Updated appsettings.json** with correct path (double backslashes)
- [ ] **Installed MCP dependencies:** `pip install -r requirements.txt`
- [ ] **Restarted application** (F5)
- [ ] **Verified logs** show "Server SeoAutomation started"
- [ ] **Tested:** http://localhost:5001/health returns OK

---

## ?? Total Time

- **Step 1 (Remove stubs):** 2 minutes
- **Step 2 (Download & Install):** 5 minutes
- **Step 3 (Verify):** 1 minute
- **Step 4 (Get path):** 1 minute
- **Update Config:** 2 minutes
- **Install Dependencies:** 2 minutes
- **Restart & Test:** 2 minutes

**Total: ~15 minutes**

---

## ? Troubleshooting

### Problem: "Still says Python not found"

**Check 1:** Is real Python installed?
```cmd
python --version
# Should show version, NOT open Microsoft Store
```

**Check 2:** Is Python in PATH?
```cmd
where python
# Should show: C:\Users\...\Python312\python.exe
```

**Check 3:** Did you use double backslashes in appsettings.json?
```json
"ExecutablePath": "C:\\Users\\zubai\\..."  ?
"ExecutablePath": "C:\Users\zubai\..."    ?
```

---

### Problem: "FastAPI not found"

**Solution:**
```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

---

### Problem: "Permission denied" installing packages

**Solution:** Run Command Prompt as Administrator

---

## ?? Additional Resources

- **Detailed Guide:** `discussionspot9\docs\PYTHON_NOT_FOUND_FIX.md`
- **MCP Fix Guide:** `discussionspot9\docs\MCP_FIX_GUIDE.md`
- **Setup Scripts:** `setup-mcp-python.bat`, `setup-mcp-python.ps1`

---

## ?? Key Points

? **You need REAL Python, not just the shortcut**
? **Download from https://www.python.org/downloads/**
? **Check "Add to PATH" during installation**
? **Use double backslashes in appsettings.json**
? **Run pip install for MCP dependencies**

---

## ? Success Criteria

After completing all steps:

1. ? `python --version` shows a version number
2. ? Application logs show "Server SeoAutomation started"
3. ? Health endpoint: http://localhost:5001/health works
4. ? No "Python not found" errors

---

**Status:** Ready to implement  
**Time to fix:** ~15 minutes  
**Difficulty:** Easy (just follow the steps)

**Next Action:** Install Python from https://www.python.org/downloads/ and come back here!
