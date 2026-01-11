# MCP Server Fixes Applied - January 2025

## Issues Identified and Fixed

### ✅ Issue 1: MCP AutoStart Disabled
**Problem:** MCP AutoStart was set to `false` in `appsettings.json`, preventing servers from starting automatically.

**Fix Applied:**
- Changed `MCP:AutoStart:Enabled` from `false` to `true` in `appsettings.json`
- Updated `appsettings.Production.json` to include MCP configuration with AutoStart enabled

**Location:** `discussionspot9/appsettings.json` line 176

### ✅ Issue 2: SeoAutomation Server Disabled
**Problem:** The SeoAutomation MCP server was disabled in configuration.

**Fix Applied:**
- Changed `MCP:Servers:SeoAutomation:Enabled` from `false` to `true` in `appsettings.json`
- Added SeoAutomation server configuration to `appsettings.Production.json`

**Location:** `discussionspot9/appsettings.json` line 183

### ✅ Issue 3: Missing Production Configuration
**Problem:** `appsettings.Production.json` did not include MCP server configuration.

**Fix Applied:**
- Added complete MCP configuration section to `appsettings.Production.json`
- Configured Python path for Ubuntu: `/usr/bin/python3`
- Enabled AutoStart and SeoAutomation server for production

**Location:** `discussionspot9/appsettings.Production.json`

## Current Configuration

### Local Development (Windows)
- **Python Path:** Empty (auto-detects `python` from PATH)
- **MCP AutoStart:** Enabled
- **SeoAutomation Server:** Enabled on port 5001
- **Script Priority:** 
  1. `test_server.py` (no dependencies - uses Python stdlib only)
  2. `main_simple.py` (requires FastAPI, but no Ollama)
  3. `main.py` (full version with Ollama support)

### Production (Ubuntu)
- **Python Path:** `/usr/bin/python3` (explicitly configured)
- **MCP AutoStart:** Enabled
- **SeoAutomation Server:** Enabled on port 5001
- **Endpoint:** `http://localhost:5001` (both app and MCP server run on same machine)

## Verification Steps

### On Local Machine (Windows)
1. Start the application
2. Check logs for: `🚀 Starting MCP Server Manager...`
3. Look for: `✅ SeoAutomation started successfully on port 5001`
4. Test health endpoint: `curl http://localhost:5001/health` or visit in browser

### On Ubuntu Server
1. Ensure Python 3 is installed: `python3 --version`
2. Ensure MCP server files are present: `/var/www/discussionspot/mcp-servers/seo-automation/`
3. For `test_server.py`: No dependencies needed (uses Python stdlib only)
4. For `main_simple.py` or `main.py`: Install dependencies:
   ```bash
   cd /var/www/discussionspot/mcp-servers/seo-automation
   pip3 install -r requirements.txt
   ```
5. Deploy updated application with new configuration
6. Check application logs for MCP server startup messages
7. Test health endpoint: `curl http://localhost:5001/health`

## Script Priority Order

The `McpServerManager` tries scripts in this order:
1. **test_server.py** - No dependencies, uses Python standard library only
2. **main_simple.py** - Requires FastAPI/uvicorn, but no Ollama
3. **main.py** - Full version with Ollama AI support

This ensures the server will start even if dependencies aren't installed.

## Troubleshooting

### If MCP servers still don't start:

1. **Check Python Installation:**
   - Windows: `python --version` or `python3 --version`
   - Ubuntu: `python3 --version` and `which python3`

2. **Check Script Paths:**
   - Verify `mcp-servers/seo-automation/test_server.py` exists
   - Check application logs for path resolution errors

3. **Check Port Availability:**
   - Ensure port 5001 is not already in use
   - The `PortFinder` service will automatically find an available port if 5001 is taken

4. **Check Logs:**
   - Look for error messages in application startup logs
   - Check for Python-related errors
   - Verify script execution errors

5. **Manual Test:**
   ```bash
   # On Ubuntu
   cd /var/www/discussionspot/mcp-servers/seo-automation
   python3 test_server.py
   # Should see: "✅ Server running on http://localhost:5001"
   ```

## Next Steps for Ubuntu Deployment

1. **SSH into Ubuntu server:**
   ```bash
   ssh user@discussionspot.com
   ```

2. **Verify Python installation:**
   ```bash
   python3 --version
   which python3
   ```

3. **Verify MCP server files exist:**
   ```bash
   ls -la /var/www/discussionspot/mcp-servers/seo-automation/
   ```

4. **If using main_simple.py or main.py, install dependencies:**
   ```bash
   cd /var/www/discussionspot/mcp-servers/seo-automation
   pip3 install -r requirements.txt
   ```

5. **Deploy updated application:**
   - Pull latest code with updated `appsettings.Production.json`
   - Restart application service

6. **Monitor logs:**
   ```bash
   # Check application logs for MCP startup messages
   tail -f /var/www/discussionspot/logs/*.log
   # Or if using systemd:
   sudo journalctl -u your-app-service -f
   ```

## Files Modified

1. `discussionspot9/appsettings.json`
   - Enabled MCP AutoStart
   - Enabled SeoAutomation server

2. `discussionspot9/appsettings.Production.json`
   - Added MCP configuration section
   - Configured for Ubuntu deployment

## Expected Behavior After Fixes

### On Application Startup:
```
🚀 Starting MCP Server Manager...
Found server script at: [path]/mcp-servers/seo-automation/test_server.py
Auto-starting SeoAutomation on preferred port 5001 using test_server.py...
Python found: Python 3.x.x (using python3)
Starting server from: [path]/mcp-servers/seo-automation, File: test_server.py, Port: 5001
✅ SeoAutomation started successfully on port 5001
```

### Health Check Response:
```json
{
  "status": "healthy",
  "timestamp": "2025-01-XX...",
  "server": "SEO Automation (Test Mode)",
  "ai_available": false,
  "model": "none (test server)"
}
```

---

**Status:** ✅ All configuration issues fixed  
**Date:** January 2025  
**Next Action:** Deploy to Ubuntu server and verify MCP servers start automatically
