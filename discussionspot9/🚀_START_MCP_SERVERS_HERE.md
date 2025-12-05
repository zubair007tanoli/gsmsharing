# 🚀 START MCP SERVERS - FINAL INSTRUCTIONS

## ⚠️ IMPORTANT: Two Workspace Locations Detected

Your project exists in TWO locations:
1. **Workspace (where files are edited)**: `C:\Users\zubai\.cursor\worktrees\gsmsharing\oek\discussionspot9`
2. **Terminal (where app runs)**: `D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9`

## ✅ SOLUTION: Start Server from Workspace Location

### Option 1: Double-Click Batch File (EASIEST)

1. Navigate to: `C:\Users\zubai\.cursor\worktrees\gsmsharing\oek\discussionspot9`
2. Double-click: `START_TEST_SERVER.bat`
3. A window will open showing "Server running on http://localhost:5001"
4. **Keep this window open** - don't close it!

### Option 2: PowerShell Command

```powershell
# Open PowerShell and run:
cd "C:\Users\zubai\.cursor\worktrees\gsmsharing\oek\discussionspot9\mcp-servers\seo-automation"
C:\Users\zubai\AppData\Local\Python\bin\python.exe test_server.py
```

### Option 3: From Terminal Location

```powershell
cd "D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9\mcp-servers\seo-automation"
C:\Users\zubai\AppData\Local\Python\bin\python.exe test_server.py
```

## ✅ Verify It's Working

1. **Keep the server window open**
2. Open a **NEW** PowerShell window
3. Run:
   ```powershell
   Invoke-RestMethod -Uri "http://localhost:5001/health"
   ```
4. You should see:
   ```json
   {
     "status": "healthy",
     "server": "SEO Automation (Test Mode)",
     ...
   }
   ```

## ✅ Check in Browser

1. Open browser: http://localhost:5001/health
2. You should see JSON response
3. Then go to: http://localhost:5099/admin/mcp-status
4. **SEO Automation should show as ONLINE** ✅

## 🎯 What's Been Fixed

✅ **Python 3.14.0 installed and configured**
✅ **Test server created** - No dependencies required (uses Python standard library only)
✅ **Startup scripts created** in both locations
✅ **Configuration updated** - Python path set in appsettings.json
✅ **Auto-start configured** - McpServerManager will try to start servers automatically

## 🔧 Files Created

### In Workspace (`oek`):
- ✅ `START_TEST_SERVER.bat` - Double-click to start
- ✅ `START_MCP_SERVER.bat` - Full version (requires dependencies)
- ✅ `START_MCP_SERVER.ps1` - PowerShell version
- ✅ `mcp-servers/seo-automation/test_server.py` - **NO DEPENDENCIES!**
- ✅ `mcp-servers/seo-automation/main_simple.py` - Simplified version
- ✅ `MCP_SERVER_QUICK_START.md` - Full documentation

### Configuration:
- ✅ `appsettings.json` - Python path updated
- ✅ `Program.cs` - McpServerManager registered and auto-starts

## 🚨 Troubleshooting

### Server Won't Start?

1. **Check Python**:
   ```powershell
   C:\Users\zubai\AppData\Local\Python\bin\python.exe --version
   ```
   Should show: `Python 3.14.0`

2. **Check Port 5001**:
   ```powershell
   netstat -ano | findstr :5001
   ```
   If something is using it, kill it:
   ```powershell
   taskkill /PID <PID> /F
   ```

3. **Test Python HTTP Server**:
   ```powershell
   cd "C:\Users\zubai\.cursor\worktrees\gsmsharing\oek\discussionspot9\mcp-servers\seo-automation"
   C:\Users\zubai\AppData\Local\Python\bin\python.exe -m http.server 5001
   ```

### Still Not Working?

The application has **auto-start** enabled. When you run `dotnet run`, it will automatically try to start the MCP servers. Check the terminal output for:
- "🚀 Starting MCP Server Manager..."
- "Auto-starting SeoAutomation on port 5001..."
- "✅ SeoAutomation started successfully on port 5001"

## 📦 For Deployment

### Production (Linux/Ubuntu):

1. Copy files to server
2. Install dependencies:
   ```bash
   cd /var/www/discussionspot9/mcp-servers/seo-automation
   python3 -m pip install -r requirements.txt
   ```
3. Create systemd service (see `MCP_SERVER_QUICK_START.md`)

### Production (Windows Server):

1. Use NSSM to create Windows Service
2. Or use Task Scheduler to run on startup
3. See `MCP_SERVER_QUICK_START.md` for details

## 🎊 Summary

Your MCP servers are **READY**! The test server requires **NO dependencies** and will work immediately. Just:

1. **Open PowerShell**
2. **Run**: 
   ```powershell
   cd "C:\Users\zubai\.cursor\worktrees\gsmsharing\oek\discussionspot9\mcp-servers\seo-automation"
   C:\Users\zubai\AppData\Local\Python\bin\python.exe test_server.py
   ```
3. **Keep window open**
4. **Test**: http://localhost:5001/health
5. **Check dashboard**: http://localhost:5099/admin/mcp-status

## 📞 Next Steps

1. Start the test server (above)
2. Verify it's working (health check)
3. Check admin dashboard
4. For production, upgrade to full version with dependencies
5. Deploy to server using systemd/Windows Service

---

**Need Help?** Check `MCP_SERVER_QUICK_START.md` for detailed troubleshooting and deployment instructions.

