# 🔧 MCP Server Troubleshooting Guide

## ⚠️ Servers Showing as Offline?

### Quick Fix Steps:

1. **Check Diagnostics in Admin Dashboard**
   - Go to: `http://localhost:5099/admin/mcp-status`
   - Look at the "Diagnostics" section
   - Check if Auto-Start is enabled
   - Check if Python is configured

2. **Try Manual Start**
   - Click "Start SEO Server" button in admin dashboard
   - Check for error messages

3. **Check Application Logs**
   - Look for MCP Server Manager logs
   - Check for Python errors
   - Check for path errors

---

## 🔍 Common Issues

### Issue 1: Python Not Found

**Symptoms:**
- Error: "Python not found"
- Server won't start

**Solution:**
```powershell
# Check if Python is installed
python --version

# If not found, install Python from https://www.python.org/downloads/
# Make sure to check "Add Python to PATH" during installation
```

### Issue 2: Python Dependencies Missing

**Symptoms:**
- Error: "ModuleNotFoundError: No module named 'fastapi'"
- Server starts but crashes immediately

**Solution:**
```powershell
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

### Issue 3: Script Not Found

**Symptoms:**
- Error: "Server script not found"
- Logs show path search failures

**Solution:**
1. Check if `mcp-servers/seo-automation/main.py` exists
2. Check application logs for searched paths
3. Make sure you're running from the project root directory

### Issue 4: Port Already in Use

**Symptoms:**
- Error: "Address already in use"
- Server won't start

**Solution:**
```powershell
# Find what's using port 5001
netstat -ano | findstr :5001

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Or change port in appsettings.json
```

### Issue 5: Ollama Not Running

**Symptoms:**
- Server starts but shows "degraded" status
- AI features don't work

**Solution:**
```bash
# Start Ollama
ollama serve

# Download model if needed
ollama pull llama3.2

# Test Ollama
ollama run llama3.2 "test"
```

### Issue 6: Auto-Start Not Working

**Symptoms:**
- Servers don't start automatically
- No logs about server startup

**Solution:**
1. Check `appsettings.json`:
   ```json
   {
     "MCP": {
       "AutoStart": {
         "Enabled": true
       }
     }
   }
   ```

2. Check application logs for "MCP Server Manager" messages

3. Try manual start from admin dashboard

---

## 🧪 Testing Steps

### Step 1: Test Python
```powershell
python --version
# Should show Python 3.x.x
```

### Step 2: Test Dependencies
```powershell
python -c "import fastapi; print('FastAPI OK')"
python -c "import uvicorn; print('Uvicorn OK')"
python -c "import requests; print('Requests OK')"
```

### Step 3: Test Server Manually
```powershell
cd discussionspot9\mcp-servers\seo-automation
python main.py
```

You should see:
```
🚀 Starting SEO Automation MCP Server...
✅ Server running on http://localhost:5001
```

### Step 4: Test Health Endpoint
```powershell
curl http://localhost:5001/health
```

Should return JSON with `"status": "healthy"`

### Step 5: Test from Admin Dashboard
- Go to: `http://localhost:5099/admin/mcp-status`
- Click "Start SEO Server" button
- Check if status changes to "Online"

---

## 📊 Diagnostic Information

### Check in Admin Dashboard:

1. **Diagnostics Section:**
   - Auto-Start Enabled: Should be "Yes" ✅
   - Python Path: Should show "python" or full path
   - SEO Server Enabled: Should be "Yes" ✅
   - Manager Available: Should be "Yes" ✅

2. **Server Status:**
   - Should show "Online" with green badge
   - Response time should be < 100ms
   - Endpoint should be correct

### Check Application Logs:

Look for these messages:
```
🚀 Starting MCP Server Manager...
Auto-starting SeoAutomation on port 5001...
✅ SeoAutomation started successfully on port 5001
```

If you see errors:
```
❌ Failed to start SeoAutomation
Server script not found: ...
Python not found...
```

---

## 🛠️ Manual Fixes

### Fix 1: Install Python Dependencies
```powershell
cd discussionspot9\mcp-servers\seo-automation
pip install fastapi uvicorn requests pydantic
```

### Fix 2: Install Ollama
1. Download: https://ollama.com/download
2. Install and run
3. Download model: `ollama pull llama3.2`
4. Keep running: `ollama serve`

### Fix 3: Check Paths
Make sure these files exist:
- `discussionspot9/mcp-servers/seo-automation/main.py`
- `discussionspot9/mcp-servers/seo-automation/local_ai_service.py`
- `discussionspot9/mcp-servers/seo-automation/requirements.txt`

### Fix 4: Restart Application
After fixing issues, restart your application:
```powershell
# Stop application (Ctrl+C)
# Then restart
dotnet run
```

---

## ✅ Success Indicators

When everything is working:
- ✅ Admin dashboard shows servers as "Online"
- ✅ Response time < 100ms
- ✅ No errors in application logs
- ✅ Health endpoint returns 200 OK
- ✅ "Start Server" button works (if needed)

---

## 📞 Still Having Issues?

1. **Check all logs:**
   - Application logs
   - Python server output
   - Browser console (F12)

2. **Verify configuration:**
   - `appsettings.json` has correct settings
   - Python is in PATH
   - Dependencies are installed

3. **Test manually:**
   - Start server manually
   - Test health endpoint
   - Check for errors

4. **Check system:**
   - Firewall settings
   - Port availability
   - Python version compatibility

---

## 🎯 Quick Checklist

- [ ] Python installed and in PATH
- [ ] Python dependencies installed (`pip install -r requirements.txt`)
- [ ] Server script exists (`mcp-servers/seo-automation/main.py`)
- [ ] Ollama installed and running
- [ ] Auto-start enabled in `appsettings.json`
- [ ] Port 5001 not in use
- [ ] Application restarted after changes
- [ ] Checked admin dashboard diagnostics

