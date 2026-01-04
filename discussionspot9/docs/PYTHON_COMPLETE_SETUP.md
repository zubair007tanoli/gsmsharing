# ?? PYTHON SETUP - COMPLETE SOLUTION

## Current Status
? MCP servers are now **DISABLED** in appsettings.json
? Your application will work fine WITHOUT Python for now
? We'll fix Python and re-enable MCP servers

---

## ?? Why MCP Servers Were Failing

The MCP servers (SEO automation tools) require Python. Since Python wasn't working correctly, MCP servers couldn't start.

**Solution:** Fix Python first, then re-enable MCP servers.

---

## ? STEP 1: Install Python Properly

### **Option A: Easiest - Use Python Installer (RECOMMENDED)**

1. **Go to:** https://www.python.org/downloads/
2. **Download:** Python 3.12.1 (or latest 3.12.x)
3. **Run the installer**
4. **ON FIRST SCREEN - IMPORTANT:**
   - ? Check: **"Add Python to PATH"**
   - ? Check: **"Install for all users"** (optional but recommended)
5. **Click:** "Install Now"
6. **Wait for completion** (2-3 minutes)
7. **Restart your computer** (important!)

### **Option B: Use Microsoft Store (If preferred)**

1. **Open:** Microsoft Store
2. **Search:** "Python 3.12"
3. **Click:** "Get" or "Install"
4. **Wait for installation**
5. **Restart your computer**

---

## ? STEP 2: Verify Python Installation

**After installing, open Command Prompt and run:**

```cmd
python --version
```

**You should see:**
```
Python 3.12.1
```

**If you don't see a version number, Python isn't installed correctly.**

---

## ? STEP 3: Add Python to Your PATH (If Needed)

**If `python --version` doesn't work after reboot:**

1. **Right-click Start Menu**
2. **Search:** "Environment Variables"
3. **Click:** "Edit environment variables for your account"
4. **Click:** "Environment Variables" button
5. **Under "System variables", find "Path"**
6. **Click:** "Edit"
7. **Click:** "New"
8. **Add:** `C:\Users\YOUR_USERNAME\AppData\Local\Programs\Python\Python312`
   - Replace `YOUR_USERNAME` with your actual username
   - Replace `Python312` with your Python version if different
9. **Click:** "OK" three times
10. **Restart Command Prompt and test:** `python --version`

---

## ? STEP 4: Install MCP Dependencies

**Open Command Prompt and run:**

```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

**Expected output:**
```
Successfully installed fastapi-0.104.1 uvicorn-0.24.0 pydantic-2.5.0 requests-2.31.0
```

**Also install web-story-validator dependencies:**
```cmd
cd ..\web-story-validator
pip install -r requirements.txt
```

---

## ? STEP 5: Verify Installation

**Run these commands:**

```cmd
python --version
pip list
```

**You should see:**
- Python 3.8 or higher
- fastapi in the pip list
- uvicorn in the pip list
- pydantic in the pip list
- requests in the pip list

---

## ? STEP 6: Test MCP Server Manually

**Run:**
```cmd
cd discussionspot9\mcp-servers\seo-automation
python main.py
```

**You should see:**
```
INFO:     Uvicorn running on http://0.0.0.0:5001
```

**If you see errors, write them down** (or take a screenshot) and we can fix them.

**To stop the server, press `Ctrl+C`**

---

## ? STEP 7: Re-enable MCP Servers

**Once you've verified Python works:**

1. **Edit:** `discussionspot9\appsettings.json`
2. **Find:** `"Enabled": false,` in the MCP section
3. **Change to:** `"Enabled": true,`
4. **Save the file**

**Your MCP servers will now start automatically!**

---

## ?? Checklist

- [ ] Downloaded Python 3.12+ from python.org
- [ ] Installed Python (with "Add to PATH" checked)
- [ ] Restarted computer after installation
- [ ] Verified: `python --version` shows version number
- [ ] Ran: `pip install -r requirements.txt` in seo-automation folder
- [ ] Verified: `pip list` shows fastapi, uvicorn, pydantic, requests
- [ ] Tested: `python main.py` in seo-automation folder
- [ ] Server shows: "Uvicorn running on http://0.0.0.0:5001"
- [ ] Re-enabled MCP servers in appsettings.json

---

## ?? Troubleshooting

### Problem: `python --version` doesn't work
**Solution:**
1. Make sure you installed Python from python.org (not Microsoft Store)
2. Restart your computer
3. Make sure "Add Python to PATH" was checked during installation
4. If still doesn't work, follow Step 3 above to manually add to PATH

### Problem: "ModuleNotFoundError: fastapi"
**Solution:**
```cmd
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

### Problem: "Permission denied"
**Solution:** Run Command Prompt as Administrator

### Problem: Server starts but then crashes
**Solution:** Take a screenshot of the error or write it down, we can fix it

---

## ?? After Fixing Python

1. **Re-enable MCP servers:**
   - Edit appsettings.json
   - Set "Enabled": true in MCP section

2. **Restart the application** (F5 in Visual Studio)

3. **Verify in logs:**
   - Look for: "Server SeoAutomation started on port 5001"

4. **Test health endpoint:**
   - http://localhost:5001/health

---

## ?? Time Needed

- Install Python: 5 minutes
- Install dependencies: 3 minutes
- Test: 2 minutes
- Re-enable MCP: 1 minute

**Total: ~10 minutes**

---

## ?? Key Points

? Python MUST be installed from https://www.python.org/downloads/
? "Add Python to PATH" MUST be checked during installation
? Computer MUST be restarted after installation
? Dependencies MUST be installed with pip

---

## ?? Need Help?

If you still have issues after following these steps:

1. Run: `python main.py` in seo-automation folder
2. Take a screenshot of any error messages
3. Tell me the error - I can provide exact fix

You can also run the diagnostic script we created:
```
Double-click: discussionspot9\diagnose-mcp.bat
```

This will show you exactly what's missing!

---

**Status:** Python setup guide ready to follow  
**Next Action:** Install Python from python.org and follow the steps above  
**Support:** All tools and guides are in discussionspot9\docs\
