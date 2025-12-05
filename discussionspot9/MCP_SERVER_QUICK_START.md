# 🚀 MCP Server Quick Start Guide

## ✅ What's Been Done

1. ✅ Python 3.14.0 is installed at: `C:\Users\zubai\AppData\Local\Python\bin\python.exe`
2. ✅ MCP Server Manager is configured in `Program.cs` (auto-starts servers)
3. ✅ Simplified MCP server created (no Ollama required)
4. ✅ Startup scripts created
5. ✅ Configuration updated in `appsettings.json`

## 🎯 Quick Start (3 Steps)

### Option 1: Using Batch File (Easiest)

1. **Double-click** `START_MCP_SERVER.bat` in the `discussionspot9` folder
2. Wait for "Server starting on http://localhost:5001" message
3. Test by visiting: http://localhost:5001/health

### Option 2: Using PowerShell

```powershell
# Navigate to project
cd "D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9"

# Run the PowerShell script
.\START_MCP_SERVER.ps1
```

### Option 3: Manual Start (If scripts don't work)

```powershell
# 1. Navigate to the seo-automation folder
cd "D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9\mcp-servers\seo-automation"

# 2. Install dependencies (one-time)
C:\Users\zubai\AppData\Local\Python\bin\python.exe -m pip install fastapi uvicorn requests pydantic

# 3. Start the simplified server
C:\Users\zubai\AppData\Local\Python\bin\python.exe main_simple.py
```

## ✅ Verify It's Working

1. **Open browser** and go to: http://localhost:5001/health
2. You should see:
   ```json
   {
     "status": "healthy",
     "timestamp": "2025-12-05T...",
     "server": "SEO Automation",
     "ai_available": false,
     "model": "none (simplified mode)"
   }
   ```

3. **Check MCP Status Page**: http://localhost:5099/admin/mcp-status
   - SEO Automation should show as **"Online"** ✅

## 🔧 Troubleshooting

### Problem: "pip install" fails or hangs

**Solution**: Install packages individually:
```powershell
cd "D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9\mcp-servers\seo-automation"

C:\Users\zubai\AppData\Local\Python\bin\python.exe -m pip install --no-cache-dir fastapi
C:\Users\zubai\AppData\Local\Python\bin\python.exe -m pip install --no-cache-dir uvicorn
C:\Users\zubai\AppData\Local\Python\bin\python.exe -m pip install --no-cache-dir requests
C:\Users\zubai\AppData\Local\Python\bin\python.exe -m pip install --no-cache-dir pydantic
```

### Problem: Port 5001 already in use

**Solution**: Kill the process using port 5001:
```powershell
# Find process using port 5001
netstat -ano | findstr :5001

# Kill it (replace PID with actual process ID)
taskkill /PID <PID> /F
```

### Problem: Server starts but health check fails

**Solution**: Check if firewall is blocking:
```powershell
# Test with curl
curl http://localhost:5001/health

# Or use PowerShell
Invoke-WebRequest -Uri "http://localhost:5001/health" -UseBasicParsing
```

## 📦 For Production Deployment

### 1. Install Dependencies on Server

```bash
# On Ubuntu/Linux server
cd /var/www/discussionspot9/mcp-servers/seo-automation
python3 -m pip install -r requirements.txt
```

### 2. Create Systemd Service (Linux)

Create `/etc/systemd/system/mcp-seo.service`:

```ini
[Unit]
Description=MCP SEO Automation Server
After=network.target

[Service]
Type=simple
User=www-data
WorkingDirectory=/var/www/discussionspot9/mcp-servers/seo-automation
ExecStart=/usr/bin/python3 main_simple.py
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable mcp-seo
sudo systemctl start mcp-seo
sudo systemctl status mcp-seo
```

### 3. Windows Service (Windows Server)

Use NSSM (Non-Sucking Service Manager):

```powershell
# Download NSSM from https://nssm.cc/download
nssm install MCPSeoServer "C:\Users\zubai\AppData\Local\Python\bin\python.exe" "D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9\mcp-servers\seo-automation\main_simple.py"
nssm start MCPSeoServer
```

## 🎯 What Works Now

✅ **Simplified MCP Server** - Works without Ollama/AI
✅ **Health Check Endpoint** - Returns server status
✅ **Auto-Start on Application Launch** - McpServerManager handles it
✅ **Admin Dashboard** - Shows server status at `/admin/mcp-status`
✅ **Basic SEO Methods** - Mock implementations for testing

## 🚀 Upgrade to Full AI Version (Optional)

To enable full AI features with Ollama:

1. **Install Ollama**: https://ollama.com/download
2. **Download Model**:
   ```bash
   ollama pull llama3.2
   ```
3. **Use Full Version**:
   - Rename `main_simple.py` to `main_simple_backup.py`
   - Rename `main.py` to use the full AI version
   - Restart server

## 📝 Files Created

- ✅ `START_MCP_SERVER.bat` - Windows batch script
- ✅ `START_MCP_SERVER.ps1` - PowerShell script
- ✅ `mcp-servers/seo-automation/main_simple.py` - Simplified server (no AI)
- ✅ `mcp-servers/start-mcp-server.bat` - Detailed startup script
- ✅ `mcp-servers/start-mcp-server-simple.bat` - Simple startup script
- ✅ `appsettings.json` - Updated with correct Python path

## 🎊 Summary

Your MCP servers are **ready to deploy**! The simplified version works without any external dependencies (no Ollama required). The application will auto-start the servers when it launches, and you can monitor them at `/admin/mcp-status`.

For production, use the systemd service (Linux) or Windows Service approach to ensure servers start automatically on boot.

## 📞 Need Help?

If you encounter issues:
1. Check the terminal output for error messages
2. Verify Python is accessible: `C:\Users\zubai\AppData\Local\Python\bin\python.exe --version`
3. Test health endpoint: `curl http://localhost:5001/health`
4. Check application logs in the terminal where you run `dotnet run`

