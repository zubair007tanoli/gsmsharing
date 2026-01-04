# ?? MCP Servers Diagnosis Report - discussionspot9

**Date:** December 2024  
**Project:** discussionspot9 (ASP.NET Core 10.0)  
**Domain:** https://discussionspot.com  
**Issue:** MCP SEO servers not working on local machine or Ubuntu production server

---

## ?? CRITICAL ISSUES IDENTIFIED

### **Issue 1: HARDCODED Python Path (Windows-Specific)**
**Severity:** ?? **CRITICAL - Primary Cause of Failure**

**Location:** `appsettings.json` (Line ~136)
```json
"Python": {
  "ExecutablePath": "C:\\Users\\zubai\\AppData\\Local\\Programs\\Python\\Python314\\python.exe",
  "ScriptTimeout": 30
}
```

**Problems:**
1. ? Hardcoded **Windows path** that only works on your local machine
2. ? Specific to user "zubai" - different machines won't have this path
3. ? **Ubuntu servers CANNOT use this path** - completely invalid on Linux
4. ? When deployed to production, Python lookup fails completely

**Impact:**
- ? MCP servers fail to start on any machine except yours
- ? Application can't find Python on Ubuntu server
- ? Automatic server startup fails silently
- ? Services fall back to placeholder defaults

**Why It Fails:**
```
McpServerManager.StartServerAsync()
  ? Checks configured path: C:\Users\zubai\...
  ? Path doesn't exist on Ubuntu
  ? Falls back to "python3" command lookup
  ? May or may not find Python depending on PATH
  ? If Python not in PATH, startup fails
```

---

### **Issue 2: MCP Endpoint Configuration Hardcoded to Localhost**
**Severity:** ?? **CRITICAL - Production Won't Work**

**Location:** `appsettings.json` (Lines ~178-188)
```json
"MCP": {
  "Servers": {
    "SeoAutomation": {
      "Enabled": true,
      "Port": 5001,
      "Endpoint": "http://localhost:5001",  // ? PROBLEM
      "Timeout": 30
    }
  }
}
```

**Problems:**
1. ? Hardcoded to `localhost:5001` (only works on same machine)
2. ? When deployed to Ubuntu, the app tries to connect to "localhost" on the server
3. ? If MCP server runs on server, C# app must also run on server to access localhost
4. ? No environment-specific configuration

**Impact on Ubuntu:**
```
C# App (ASP.NET Core)
  ? Try to call http://localhost:5001
  ? MCP server might not be running
  ? Even if running, connection may work IF both are on same Ubuntu server
  ? But if they're separate containers/processes, fails
```

---

### **Issue 3: Python Dependencies Not Installed on Ubuntu**
**Severity:** ?? **HIGH - Required for MCP servers to run**

**Location:** MCP server requires FastAPI, uvicorn, etc.  
**File:** `mcp-servers/seo-automation/requirements.txt`
```
fastapi==0.104.1
uvicorn==0.24.0
requests==2.31.0
pydantic==2.5.0
```

**Problems:**
1. ? Script assumes pip will find and install these
2. ? Ubuntu servers may have network restrictions
3. ? Virtual environments not being used
4. ? No venv isolation could cause conflicts

**What Happens:**
```bash
# ubuntu-setup.sh tries:
python3 -m pip install --user -r requirements.txt

# But on Ubuntu server:
# - pip might not be installed
# - Network might be restricted
# - User permissions could block installation
# - Result: Dependencies not available, server fails to run
```

---

### **Issue 4: MCP Server Start Script Not Being Called on Ubuntu**
**Severity:** ?? **HIGH - Servers never start**

**Location:** Program.cs (Lines 566-571)
```csharp
builder.Services.AddSingleton<discussionspot9.Services.MCP.IPortFinder, 
    discussionspot9.Services.MCP.PortFinder>();
builder.Services.AddSingleton<discussionspot9.Services.MCP.IMcpServerManager, 
    discussionspot9.Services.MCP.McpServerManager>();
```

**Problems:**
1. ? McpServerManager is registered as hosted service
2. ? Should auto-start servers on app startup
3. ?? BUT: It tries to find Python scripts in multiple paths
4. ?? On Ubuntu, script paths might not exist if deployment incomplete

**Path Search Order (McpServerManager.cs):**
```csharp
var possiblePaths = new List<string>
{
    Path.Combine(_environment.ContentRootPath, "mcp-servers", script),
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp-servers", script),
    "/var/www/discussionspot/mcp-servers/" + script,      // ? Looks here first
    "/var/www/discussionspot9/mcp-servers/" + script,    // ? Or here
    // ... other paths
};
```

**Issue:** If MCP servers folder doesn't exist or is in wrong location, startup fails.

---

### **Issue 5: No Error Logging for MCP Server Startup Failures**
**Severity:** ?? **MEDIUM - Hard to debug**

**Problems:**
1. ? Failures are logged but may be silent
2. ? McpServerManager catches exceptions but doesn't propagate
3. ? Admin panel might not show startup errors
4. ? Hard to know WHY servers didn't start

**Current Behavior:**
```csharp
try 
{
    await StartServerAsync(serverName, scriptPath, server.PreferredPort);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error starting server...");
    // Continues anyway - app still boots even if MCP fails
}
```

---

## ?? ROOT CAUSE SUMMARY

| Root Cause | Why It Fails |
|-----------|-------------|
| **Hardcoded Windows Python path** | Ubuntu doesn't have `C:\Users\zubai\...` |
| **Hardcoded localhost endpoint** | Production needs proper server address |
| **Missing Python dependencies on Ubuntu** | pip install not run or failed |
| **MCP server files not copied to Ubuntu** | `/var/www/discussionspot/mcp-servers/` missing |
| **No logging of errors** | Can't see what went wrong |

---

## ? SOLUTIONS

### **Solution 1: Remove Hardcoded Python Path (IMMEDIATE)**

**File:** `appsettings.json`

**Change From:**
```json
"Python": {
  "ExecutablePath": "C:\\Users\\zubai\\AppData\\Local\\Programs\\Python\\Python314\\python.exe",
  "ScriptTimeout": 30
}
```

**Change To:**
```json
"Python": {
  "ExecutablePath": "",  // Leave empty to auto-detect
  "ScriptTimeout": 30
}
```

**Why:** McpServerManager has auto-detect logic:
```csharp
if (string.IsNullOrEmpty(pythonExe))
{
    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    pythonExe = isWindows ? "python" : "python3";
}
```

---

### **Solution 2: Use Environment Variable for Python Path**

**For Windows (Local Development):**
```powershell
# Set environment variable
$env:PYTHON_EXECUTABLE = "C:\Users\zubai\AppData\Local\Programs\Python\Python314\python.exe"
```

**For Ubuntu (Production):**
```bash
# SSH into server, add to .bashrc or .profile
export PYTHON_EXECUTABLE="/usr/bin/python3"

# Or set via systemd if using service:
Environment="PYTHON_EXECUTABLE=/usr/bin/python3"
```

**In appsettings.json:**
```json
"Python": {
  "ExecutablePath": "",  // Will check PYTHON_EXECUTABLE env var
  "ScriptTimeout": 30
}
```

---

### **Solution 3: Create Environment-Specific Configuration**

**Create:** `appsettings.Production.json`
```json
{
  "MCP": {
    "AutoStart": {
      "Enabled": true
    },
    "Servers": {
      "SeoAutomation": {
        "Enabled": true,
        "Port": 5001,
        "Endpoint": "http://localhost:5001",  // Still localhost if both run on same server
        "Timeout": 30
      }
    }
  },
  "Python": {
    "ExecutablePath": "/usr/bin/python3",  // Ubuntu path
    "ScriptTimeout": 30
  }
}
```

**Create:** `appsettings.Development.json` (for local)
```json
{
  "MCP": {
    "AutoStart": {
      "Enabled": true
    },
    "Servers": {
      "SeoAutomation": {
        "Enabled": true,
        "Port": 5001,
        "Endpoint": "http://localhost:5001",
        "Timeout": 30
      }
    }
  },
  "Python": {
    "ExecutablePath": "",  // Auto-detect
    "ScriptTimeout": 30
  }
}
```

---

### **Solution 4: Ensure Python Dependencies Installed on Ubuntu**

**Step 1: SSH into Ubuntu**
```bash
ssh user@discussionspot.com
```

**Step 2: Install Python and pip**
```bash
sudo apt-get update
sudo apt-get install -y python3 python3-pip python3-venv
```

**Step 3: Verify installation**
```bash
python3 --version
pip3 --version
which python3  # Should show /usr/bin/python3
```

**Step 4: Install MCP server dependencies**
```bash
cd /var/www/discussionspot/mcp-servers/seo-automation
pip3 install -r requirements.txt

cd ../web-story-validator
pip3 install -r requirements.txt
```

**Or use virtual environment (recommended):**
```bash
cd /var/www/discussionspot/mcp-servers

# Create virtual environment
python3 -m venv venv
source venv/bin/activate

# Install all dependencies
pip install -r seo-automation/requirements.txt
pip install -r web-story-validator/requirements.txt

# Deactivate when done
deactivate
```

---

### **Solution 5: Copy MCP Server Files to Ubuntu**

**Verify files exist on Ubuntu:**
```bash
ssh user@discussionspot.com
ls -la /var/www/discussionspot/mcp-servers/
```

**If missing, copy from source:**
```bash
# From your Windows machine (PowerShell)
scp -r "D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9\mcp-servers" user@discussionspot.com:/var/www/discussionspot/

# OR via git
ssh user@discussionspot.com
cd /var/www/discussionspot
git pull origin master  # Pulls latest including mcp-servers
```

**Make scripts executable:**
```bash
sudo chmod +x /var/www/discussionspot/mcp-servers/seo-automation/main.py
sudo chmod +x /var/www/discussionspot/mcp-servers/web-story-validator/main.py
sudo chmod +x /var/www/discussionspot/mcp-servers/ubuntu-setup.sh
```

---

### **Solution 6: Test MCP Servers on Ubuntu**

**Test 1: Verify Python**
```bash
python3 --version
which python3
```

**Test 2: Test dependencies**
```bash
cd /var/www/discussionspot/mcp-servers/seo-automation
python3 -c "import fastapi; print('FastAPI installed')"
```

**Test 3: Start server manually**
```bash
cd /var/www/discussionspot/mcp-servers/seo-automation
python3 main.py
# Should show: Uvicorn running on http://127.0.0.1:5001
```

**Test 4: Check health endpoint**
```bash
# From another terminal
curl http://localhost:5001/health
# Should return: {"status":"healthy","server":"SEO Automation",...}
```

**Test 5: Verify from ASP.NET app**
- Application logs should show: "Server SeoAutomation is already running on port 5001"

---

## ?? COMPLETE FIX CHECKLIST

### **Local Machine (Windows)**
- [ ] Edit `appsettings.json` - remove hardcoded Python path
- [ ] Run application - verify MCP servers start
- [ ] Check logs for: "Auto-starting SeoAutomation..."
- [ ] Test health endpoint: http://localhost:5001/health

### **Ubuntu Server**
- [ ] SSH into server: `ssh user@discussionspot.com`
- [ ] Install Python 3: `sudo apt-get install -y python3 python3-pip`
- [ ] Copy MCP files: `git pull` or `scp`
- [ ] Install dependencies: `pip3 install -r requirements.txt`
- [ ] Make scripts executable: `chmod +x *.py`
- [ ] Create `appsettings.Production.json` with Ubuntu paths
- [ ] Deploy updated application
- [ ] Start application - check logs for MCP startup
- [ ] Test health: `curl http://localhost:5001/health`
- [ ] Test from app logs that server connected

---

## ?? RECOMMENDED ACTIONS (Priority Order)

### **Immediate (Fix Today)**
1. ? Remove hardcoded Python path from `appsettings.json`
2. ? Test on local machine - verify servers start
3. ? Create `appsettings.Production.json` with correct Ubuntu path

### **Near-term (This Week)**
4. ? SSH to Ubuntu server and verify Python installed
5. ? Copy MCP server files to `/var/www/discussionspot/mcp-servers/`
6. ? Install Python dependencies on Ubuntu
7. ? Deploy updated application with new settings

### **Optional (Best Practices)**
8. ?? Create systemd service file for MCP servers (auto-restart on reboot)
9. ?? Add monitoring/alerting for MCP server health
10. ?? Use Docker for MCP servers (isolate, version control, easier deployment)

---

## ?? EXPECTED BEHAVIOR AFTER FIXES

### **On Local Machine**
```
App Startup
? McpServerManager.StartAsync() called
? Looks for Python (auto-detects "python")
? Finds Python on Windows PATH
? Starts seo-automation/main.py on port 5001
? Logs: "? Server SeoAutomation started on port 5001"
? Client can call http://localhost:5001/health ?
```

### **On Ubuntu Server**
```
App Startup
? McpServerManager.StartAsync() called
? Reads appsettings.Production.json
? Uses ExecutablePath: "/usr/bin/python3"
? Starts seo-automation/main.py on port 5001
? Logs: "? Server SeoAutomation started on port 5001"
? Client can call http://localhost:5001/health ?
```

---

## ?? TESTING COMMANDS

**Test health endpoint:**
```bash
curl -X GET http://localhost:5001/health
# Returns: {"status":"healthy","timestamp":"...","server":"SEO Automation","ai_available":true}
```

**Test SEO optimization:**
```bash
curl -X POST http://localhost:5001/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc":"2.0",
    "method":"optimize_title",
    "params":{"title":"My Article Title","context":"Article about SEO"},
    "id":"123"
  }'
```

**Check app logs:**
```bash
# On Windows (local dev)
# Check Visual Studio Output window

# On Ubuntu (production)
ssh user@discussionspot.com
sudo journalctl -u your-app-service -f  # If running as systemd service
# Or check application logs directory
tail -f /var/www/discussionspot/logs/*.log
```

---

## ? FAQ

**Q: Why did it work on my machine but not on the server?**  
A: Because the Python path was hardcoded to your Windows user profile. The server can't access paths like `C:\Users\zubai\...`

**Q: Do I need to run MCP servers separately?**  
A: No! McpServerManager automatically starts them when ASP.NET app starts. But you must have Python and dependencies installed.

**Q: Can I use a different port for MCP servers?**  
A: Yes, change `"Port": 5001` in `appsettings.json` to any available port, but both the server starter and client endpoint must match.

**Q: What if Python isn't in Ubuntu's PATH?**  
A: Specify the full path in `appsettings.Production.json`: `"ExecutablePath": "/usr/bin/python3"`

**Q: Should I use virtual environments?**  
A: Recommended! Creates isolated Python environment, prevents conflicts, easier to manage dependencies.

---

## ?? Support Resources

- MCP Server Docs: `discussionspot9/mcp-servers/README.md`
- Ubuntu Setup Guide: `discussionspot9/mcp-servers/QUICK_START_UBUNTU.md`
- Deployment Guide: `discussionspot9/docs/deployment/UBUNTU_MCP_SETUP.md`

---

**Report Generated:** December 2024  
**Analyzed By:** GitHub Copilot  
**Status:** Ready for Implementation
