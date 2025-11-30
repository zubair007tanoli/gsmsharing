# 🤖 MCP Server Auto-Start System

## ✅ What's Been Created

### Automated MCP Server Manager
- ✅ **Auto-starts** MCP servers when application starts
- ✅ **Auto-retries** if servers fail to start (up to 3 times)
- ✅ **Health monitoring** - Checks every 30 seconds
- ✅ **Auto-restart** - Restarts crashed servers automatically
- ✅ **Background service** - Runs continuously

---

## 🚀 How It Works

### When Application Starts:
1. **McpServerManager** starts automatically
2. Checks which servers are enabled in `appsettings.json`
3. Starts each enabled server automatically
4. Retries up to 3 times if startup fails
5. Monitors health every 30 seconds

### Health Monitoring:
- Checks server health every 30 seconds
- If server is down, automatically restarts it
- Logs all events for debugging

---

## ⚙️ Configuration

### appsettings.json:

```json
{
  "MCP": {
    "AutoStart": {
      "Enabled": true,                    // Enable/disable auto-start
      "RetryDelaySeconds": 10,            // Wait 10s between retries
      "MaxRetries": 3,                    // Try 3 times before giving up
      "HealthCheckIntervalSeconds": 30    // Check health every 30s
    },
    "Servers": {
      "SeoAutomation": {
        "Enabled": true,                  // Auto-start this server
        "Endpoint": "http://localhost:5001"
      }
    }
  }
}
```

---

## 📋 Features

### ✅ Auto-Start
- Servers start automatically when application starts
- No manual intervention needed

### ✅ Auto-Retry
- If server fails to start, waits 10 seconds and tries again
- Retries up to 3 times before giving up
- Logs all attempts

### ✅ Health Monitoring
- Checks server health every 30 seconds
- Detects crashed servers
- Automatically restarts failed servers

### ✅ Logging
- All events are logged
- Check logs to see server status
- Debug startup issues easily

---

## 🔍 Monitoring

### Check Logs:

```csharp
// In your application logs, you'll see:
🚀 Starting MCP Server Manager...
Auto-starting SeoAutomation on port 5001...
✅ SeoAutomation started successfully on port 5001
```

### Check Admin Dashboard:

Go to: `http://localhost:5099/admin/mcp-status`

You should see servers as **"Online"** ✅

---

## 🛠️ Troubleshooting

### Server Not Starting?

1. **Check Logs:**
   - Look for error messages in application logs
   - Check for Python errors

2. **Check Configuration:**
   - Verify `MCP:AutoStart:Enabled` is `true`
   - Verify `MCP:Servers:SeoAutomation:Enabled` is `true`

3. **Check Python:**
   - Make sure Python is installed and in PATH
   - Test: `python --version`

4. **Check Dependencies:**
   - Make sure requirements are installed:
   ```powershell
   cd mcp-servers/seo-automation
   pip install -r requirements.txt
   ```

5. **Check Ollama:**
   - SEO server needs Ollama running
   - Start: `ollama serve`

### Server Keeps Crashing?

1. **Check Server Logs:**
   - Look at server output in application logs
   - Check for Python errors

2. **Check Port:**
   - Make sure port 5001 is not in use
   - Check firewall settings

3. **Check Ollama:**
   - Make sure Ollama is running
   - Test: `ollama list`

### Disable Auto-Start:

```json
{
  "MCP": {
    "AutoStart": {
      "Enabled": false
    }
  }
}
```

---

## 📊 Status Indicators

### In Logs:
- `✅ Server started successfully` - Server is running
- `⚠️ Server appears to have crashed` - Server went down, restarting
- `❌ Server has exceeded max retries` - Server failed to start after 3 tries

### In Admin Dashboard:
- **Green "Online"** - Server is healthy
- **Red "Offline"** - Server is down (will auto-restart)

---

## 🎯 Manual Control

### Start Server Manually:
```csharp
// In your code
await _mcpServerManager.StartServerAsync("SeoAutomation", scriptPath, 5001);
```

### Stop Server:
```csharp
await _mcpServerManager.StopServerAsync("SeoAutomation");
```

### Restart Server:
```csharp
await _mcpServerManager.RestartServerAsync("SeoAutomation", scriptPath, 5001);
```

### Check Status:
```csharp
var isRunning = await _mcpServerManager.IsServerRunningAsync(5001);
```

---

## 💡 Best Practices

1. **Keep Auto-Start Enabled** - Let the system manage servers
2. **Monitor Logs** - Check logs regularly for issues
3. **Keep Ollama Running** - Required for SEO server
4. **Check Admin Dashboard** - Visual status monitoring
5. **Test After Changes** - Restart app after config changes

---

## 🎉 Benefits

- ✅ **Zero Manual Work** - Servers start automatically
- ✅ **Self-Healing** - Auto-restarts crashed servers
- ✅ **Reliable** - Retries ensure servers start
- ✅ **Monitoring** - Always know server status
- ✅ **Production Ready** - Handles failures gracefully

---

## 📚 Related Documentation

- **MCP Setup:** `docs/MCP_SERVER_IMPLEMENTATION_GUIDE.md`
- **Quick Fix:** `docs/MCP_QUICK_FIX.md`
- **Local AI:** `docs/AI/LOCAL_AI_SETUP_GUIDE.md`

