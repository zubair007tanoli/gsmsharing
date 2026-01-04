# ?? PYTHON NOT FOUND - WINDOWS STORE STUBS DETECTED

## Problem

```
Failed to start SeoAutomation. Error: Python not found.
Both 'python' and 'python3' appear to be Windows Store stubs.
```

**Root Cause:** You have Windows Store Python shortcuts that are NOT real Python installations.

---

## ? What's Happening

When you type `python` in Command Prompt or PowerShell, Windows opens the Microsoft Store instead of running Python. This is because:

1. ? Real Python is NOT installed on your system
2. ? Windows has installed fake "stub" shortcuts that open the Store
3. ? MCP server can't run because there's no actual Python executable

---

## ? Solution: Install Real Python

### **Option 1: Direct Python Installation (RECOMMENDED)**

#### Step 1: Remove Windows Store Python Stubs

**In PowerShell (as Administrator):**
```powershell
# Run PowerShell as Administrator
# Press Win+X, select "Windows PowerShell (Admin)"

# Remove Python stub shortcuts
Remove-Item -Path "$env:LOCALAPPDATA\Microsoft\WindowsApps\python.exe" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$env:LOCALAPPDATA\Microsoft\WindowsApps\python3.exe" -Force -ErrorAction SilentlyContinue
```

#### Step 2: Download Python from official website

1. Go to: **https://www.python.org/downloads/**
2. Click: **"Download Python 3.12.1"** (or latest stable version)
3. **Important:** Check this box during installation:
   - ? **"Add Python to PATH"** (CRITICAL!)

#### Step 3: Install Python

**Installation Steps:**
1. Run the downloaded installer
2. **Check:** ? "Add Python 3.x to PATH" (MUST DO THIS!)
3. Click: **"Install Now"** (or "Customize Installation" for advanced options)
4. Wait for installation to complete
5. Click: **"Disable path length limit"** (recommended)

#### Step 4: Verify Installation

**In Command Prompt (NOT PowerShell):**
```cmd
python --version
# Should show: Python 3.12.1 (or your version)

python -c "import sys; print(sys.executable)"
# Should show: C:\Users\YourUsername\AppData\Local\Programs\Python\Python312\python.exe
```

**Get the exact path (use this in appsettings.json):**
```cmd
python -c "import sys; print(sys.executable)"
```

Copy that output - you'll need it next.

---

### **Option 2: Quick Install via Microsoft Store (IF you want)**

If you prefer the Microsoft Store method:

1. Open **Microsoft Store**
2. Search for **"Python 3.12"** (or latest)
3. Click **"Get"** or **"Install"**
4. Wait for installation
5. Verify: `python --version` in Command Prompt

---

## ?? Update appsettings.json

After installing Python, update your configuration:

### Step 1: Find Your Python Path

**In Command Prompt:**
```cmd
python -c "import sys; print(sys.executable)"
```

**You'll get something like:**
```
C:\Users\zubai\AppData\Local\Programs\Python\Python312\python.exe
```

### Step 2: Update appsettings.json

**Edit:** `discussionspot9\appsettings.json`

**Find this section:**
```json
"Python": {
  "ExecutablePath": "",
  "ScriptTimeout": 30
}
```

**Change to (use YOUR path from step 1):**
```json
"Python": {
  "ExecutablePath": "C:\\Users\\zubai\\AppData\\Local\\Programs\\Python\\Python312\\python.exe",
  "ScriptTimeout": 30
}
```

**Note:** Use double backslashes `\\` in JSON!

### Step 3: Install MCP Dependencies

**In PowerShell or Command Prompt:**
```powershell
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt

cd ..\web-story-validator
pip install -r requirements.txt
```

### Step 4: Restart Application

- Visual Studio: Press **F5** to restart
- Or: Close and re-run `dotnet run`

### Step 5: Verify in Logs

Look for:
```
? Server SeoAutomation started on port 5001
```

---

## ?? Step-by-Step Walkthrough

### For Windows 11/10:

1. **Remove stubs** (PowerShell as Admin)
   ```powershell
   Remove-Item -Path "$env:LOCALAPPDATA\Microsoft\WindowsApps\python.exe" -Force -ErrorAction SilentlyContinue
   Remove-Item -Path "$env:LOCALAPPDATA\Microsoft\WindowsApps\python3.exe" -Force -ErrorAction SilentlyContinue
   ```

2. **Download Python**
   - Go to: https://www.python.org/downloads/
   - Download latest 3.x version (e.g., 3.12.1)

3. **Install Python**
   - Run installer
   - ? Check "Add Python to PATH"
   - Click "Install Now"

4. **Verify**
   ```cmd
   python --version
   python -c "import sys; print(sys.executable)"
   ```

5. **Update appsettings.json**
   - Copy the path from step 4
   - Paste into `"ExecutablePath"`
   - Use double backslashes

6. **Install dependencies**
   ```cmd
   cd discussionspot9\mcp-servers\seo-automation
   pip install -r requirements.txt
   ```

7. **Restart application**
   - F5 in Visual Studio

8. **Test**
   - Check logs for "Server SeoAutomation started"
   - Visit: http://localhost:5001/health

---

## ? FAQ

**Q: Why do I have Windows Store stubs?**
A: Microsoft added Python shortcuts to Windows that open the Store instead of running Python. You have the shortcut but not the actual Python.

**Q: Can I use the Microsoft Store Python?**
A: Yes, but the official Python.org version is recommended. Use whichever you install.

**Q: How do I know which Python version I have?**
A: Run `python --version` in Command Prompt after installation.

**Q: What if it still says "Python not found"?**
A: Make sure you:
- [ ] Installed real Python (not just the shortcut)
- [ ] Used the correct path in appsettings.json
- [ ] Used double backslashes `\\` in JSON
- [ ] Restarted the application

**Q: Can I use the auto-detect method without specifying the path?**
A: Yes, if Python is added to PATH during installation. But explicit path is more reliable.

---

## ?? Troubleshooting

### Issue: "Still can't find Python"

**Check 1: Is Python really installed?**
```cmd
python --version
```
- If this works, Python is installed
- If you get Microsoft Store pop-up, Python is NOT installed (remove stubs and reinstall)

**Check 2: Is Python in PATH?**
```cmd
where python
# Should show: C:\Users\...\Python312\python.exe
```
- If not found, Python isn't in PATH. Reinstall with "Add to PATH" checked.

**Check 3: Are you using correct path in appsettings.json?**
```cmd
python -c "import sys; print(sys.executable)"
# Copy this exact path (with double backslashes) into appsettings.json
```

### Issue: "FastAPI module not found"

**Solution:**
```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

### Issue: "Permission denied" when installing dependencies

**Solution:**
```powershell
# Run Command Prompt as Administrator
# Then try pip install again
```

---

## ?? Checklist

- [ ] Removed Windows Store Python stubs (PowerShell as Admin)
- [ ] Downloaded Python from python.org
- [ ] Installed Python with "Add to PATH" checked
- [ ] Verified: `python --version` shows version number
- [ ] Got exact path: `python -c "import sys; print(sys.executable)"`
- [ ] Updated appsettings.json with correct path (double backslashes)
- [ ] Installed MCP dependencies: `pip install -r requirements.txt`
- [ ] Restarted application (F5)
- [ ] Checked logs for "Server SeoAutomation started"
- [ ] Tested: http://localhost:5001/health

---

## ? Success Criteria

After completing these steps, you should see:

1. **Command Prompt:**
   ```
   python --version
   Python 3.12.1 (or your version)
   ```

2. **Application Logs:**
   ```
   ? Auto-starting SeoAutomation...
   ? Found Python: C:\Users\zubai\AppData\Local\Programs\Python\Python312\python.exe
   ? Server SeoAutomation started on port 5001
   ```

3. **Health Endpoint:**
   ```
   curl http://localhost:5001/health
   {"status":"healthy","server":"SEO Automation",...}
   ```

---

## ?? Next Steps

1. **Right Now:** Install real Python from python.org
2. **Then:** Update appsettings.json with exact path
3. **Then:** Install MCP dependencies with pip
4. **Finally:** Restart application and verify

**Time needed:** ~10 minutes

---

**Reference:** This issue is documented in McpServerManager.cs where it detects Windows Store stubs and provides this exact error message.
