# ? MCP SERVER FIXES APPLIED - DEPLOYMENT CHECKLIST

**Date:** December 2024  
**Status:** ? FIXES APPLIED & BUILD SUCCESSFUL  
**Next Steps:** Deploy to production

---

## ?? WHAT WAS FIXED

### Configuration Files Updated

#### 1. **appsettings.json** ?
```json
"Python": {
  "ExecutablePath": "",
  "ScriptTimeout": 30
}
```
- ? Removed hardcoded Windows path
- ? Enabled auto-detection on all platforms
- ? Works on Windows, Linux, macOS

#### 2. **appsettings.Development.json** ?
```json
"Python": {
  "ExecutablePath": "",
  "ScriptTimeout": 30
}
```
- ? Windows development environment
- ? Auto-detects python or python3 from PATH

#### 3. **appsettings.Production.json** ?
```json
"Python": {
  "ExecutablePath": "/usr/bin/python3",
  "ScriptTimeout": 30
}
```
- ? Ubuntu production environment
- ? Explicitly set to standard Linux Python location

---

## ?? DEPLOYMENT STEPS

### For Windows (Local Development)

#### Step 1: Install Python Dependencies (if not already done)
```powershell
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt

cd ..\web-story-validator
pip install -r requirements.txt
```

#### Step 2: Start Application
```powershell
cd discussionspot9
dotnet run
# OR press F5 in Visual Studio
```

#### Step 3: Verify in Logs
Look for:
```
? Auto-starting SeoAutomation on preferred port 5001...
? Server SeoAutomation started on port 5001
```

#### Step 4: Test Health Endpoint
```powershell
curl http://localhost:5001/health
# Expected: {"status":"healthy","server":"SEO Automation",...}
```

---

### For Ubuntu Server

#### Step 1: SSH to Server
```bash
ssh user@discussionspot.com
```

#### Step 2: Install Python (if not present)
```bash
sudo apt-get update
sudo apt-get install -y python3 python3-pip python3-venv
python3 --version  # Verify
```

#### Step 3: Deploy Updated Code
```bash
cd /var/www/discussionspot
git pull origin master
```

#### Step 4: Install Python Dependencies
```bash
cd /var/www/discussionspot/mcp-servers/seo-automation
pip3 install -r requirements.txt

cd ../web-story-validator
pip3 install -r requirements.txt
```

#### Step 5: Build & Deploy Application
```bash
# If using .NET
cd /var/www/discussionspot
dotnet publish -c Release -o ./publish

# If using systemd service
sudo systemctl restart your-app-service

# Or restart manually
```

#### Step 6: Verify in Logs
```bash
# Check application logs
tail -f /var/www/discussionspot/logs/*.log

# Look for:
# ? Server SeoAutomation started on port 5001
```

#### Step 7: Test Health Endpoint
```bash
curl http://localhost:5001/health
# Expected: {"status":"healthy","server":"SEO Automation",...}
```

---

## ?? PRE-DEPLOYMENT CHECKLIST

- [ ] **Build successful:** ? Completed
- [ ] **Configuration files updated:** ? Completed
- [ ] **Python dependencies listed:** ? seo-automation/requirements.txt
- [ ] **Fix scripts available:** ? fix-mcp-windows.bat, fix-mcp-ubuntu.sh
- [ ] **Documentation created:** ? MCP_FIX_GUIDE.md, MCP_DIAGNOSIS_AND_SOLUTIONS.md
- [ ] **Local testing passed:** ? Ready to test
- [ ] **Ubuntu server setup:** ? Ready to deploy
- [ ] **Production testing:** ? After deployment

---

## ?? TESTING WORKFLOW

### 1. Local Windows Testing (Today)
```powershell
# Make sure you're in discussionspot9 directory
dotnet run

# Wait for server startup messages
# Check for: "Server SeoAutomation started on port 5001"

# Test in browser
http://localhost:5001/health
```

### 2. Ubuntu Deployment (When Ready)
```bash
# SSH to server
ssh user@discussionspot.com

# Run the automated fix script
cd /var/www/discussionspot/mcp-servers
chmod +x fix-mcp-ubuntu.sh
./fix-mcp-ubuntu.sh

# Deploy application
# Restart application service
```

### 3. Post-Deployment Verification
```bash
# Check health
curl http://localhost:5001/health

# Check logs
tail -f /var/www/discussionspot/logs/*.log

# Test SEO features in the app
# Create a new post and verify optimization works
```

---

## ?? EXPECTED BEHAVIOR

### Before Fixes
```
Local Machine:
  ? MCP servers might work (if Python path matches)

Ubuntu Server:
  ? MCP servers fail to start
  ? Python path hardcoded to Windows
  ? Silent failure - no clear error
```

### After Fixes
```
Local Machine:
  ? MCP servers auto-start
  ? Python auto-detected from PATH
  ? Clear log messages

Ubuntu Server:
  ? MCP servers auto-start
  ? Python found at /usr/bin/python3
  ? Health check: http://localhost:5001/health ? OK
```

---

## ??? TROUBLESHOOTING

### Issue: "Python not found"
**Solution:** 
```bash
# Ubuntu
sudo apt-get install -y python3 python3-pip

# Windows
# Ensure Python is in PATH
python --version
```

### Issue: "Module not found" (FastAPI, uvicorn)
**Solution:**
```bash
# Ubuntu
pip3 install -r /var/www/discussionspot/mcp-servers/seo-automation/requirements.txt

# Windows
pip install -r mcp-servers\seo-automation\requirements.txt
```

### Issue: "Connection refused"
**Solution:**
- Check if server is running: `curl http://localhost:5001/health`
- Check firewall: `sudo ufw allow 5001`
- Check application logs for errors

### Issue: "Server script not found"
**Solution:**
- Verify files: `ls -la /var/www/discussionspot/mcp-servers/seo-automation/main.py`
- Copy files: `git pull` or use `scp`

---

## ?? DOCUMENTATION REFERENCES

- **Quick Summary:** `discussionspot9/docs/MCP_ISSUE_SUMMARY.md`
- **Fix Guide:** `discussionspot9/docs/MCP_FIX_GUIDE.md`
- **Technical Details:** `discussionspot9/docs/MCP_DIAGNOSIS_AND_SOLUTIONS.md`
- **Complete Analysis:** `discussionspot9/docs/MCP_SERVERS_COMPLETE_ANALYSIS.md`

---

## ? SUCCESS CRITERIA

After deployment, you should see:

1. **Application Logs:**
   ```
   ? Auto-starting SeoAutomation on preferred port 5001...
   ? Server SeoAutomation started on port 5001
   ```

2. **Health Endpoint:**
   ```
   curl http://localhost:5001/health
   {
     "status": "healthy",
     "timestamp": "2024-12-XX...",
     "server": "SEO Automation",
     "ai_available": true,
     "model": "llama3.2"
   }
   ```

3. **SEO Features:**
   - Create a new post
   - Post optimization works
   - Keywords extracted
   - Meta descriptions generated

---

## ?? NEXT ACTIONS

1. **Test locally:**
   ```powershell
   cd discussionspot9
   dotnet run
   # Press F5 in Visual Studio
   ```

2. **Verify logs show success:**
   - Watch for "Server SeoAutomation started"

3. **Test health endpoint:**
   - Open browser: http://localhost:5001/health

4. **When ready to deploy:**
   - Push changes to GitHub
   - SSH to Ubuntu server
   - Run deployment steps above

---

## ?? COMMIT MESSAGE

Use this when committing:
```
fix: Apply MCP server fixes - auto-detect Python path

- Remove hardcoded Windows Python path from appsettings.json
- Update appsettings.Development.json for auto-detection
- Update appsettings.Production.json with /usr/bin/python3
- Enable MCP servers to work on all platforms (Windows, Linux, macOS)

Fixes #XXX (GitHub issue number if applicable)
```

---

**Status:** ? Ready for Deployment  
**Last Updated:** December 2024  
**Build Status:** ? Successful
