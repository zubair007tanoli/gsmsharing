# ?? MCP Server Fix Guide - Step by Step

## ?? Overview

Your MCP servers are failing because:
1. ? **Hardcoded Python path** (C:\Users\zubai\...) doesn't work on other machines or Ubuntu
2. ? **Python dependencies not installed** on Ubuntu server
3. ? **MCP server files might not be deployed** to correct location on Ubuntu

This guide walks you through fixing all issues.

---

## ? Fix for Local Machine (Windows)

### Step 1: Update appsettings.json

**Edit:** `discussionspot9\appsettings.json`

**Find this section:**
```json
"Python": {
  "ExecutablePath": "C:\\Users\\zubai\\AppData\\Local\\Programs\\Python\\Python314\\python.exe",
  "ScriptTimeout": 30
}
```

**Change to:**
```json
"Python": {
  "ExecutablePath": "",
  "ScriptTimeout": 30
}
```

**Save the file.**

### Step 2: Run the Fix Script

**Option A: Automatic (Recommended)**
```powershell
cd discussionspot9\mcp-servers
.\fix-mcp-windows.bat
```

**Option B: Manual**
```powershell
# Install dependencies
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt

cd ..\web-story-validator
pip install -r requirements.txt
```

### Step 3: Restart Application

**In Visual Studio:**
- Press `F5` to restart debug session

**Or with dotnet:**
```powershell
cd discussionspot9
dotnet run
```

### Step 4: Verify in Logs

Look for this message in the application output:
```
? Server SeoAutomation started on port 5001
```

### Step 5: Test Health Endpoint

Open browser and go to:
```
http://localhost:5001/health
```

You should see:
```json
{
  "status": "healthy",
  "timestamp": "2024-12-XX...",
  "server": "SEO Automation",
  "ai_available": true,
  "model": "llama3.2"
}
```

? **Windows setup is complete!**

---

## ? Fix for Ubuntu Server

### Step 1: SSH to Your Server

```bash
ssh user@discussionspot.com
```

### Step 2: Install Python and Dependencies

```bash
# Update package manager
sudo apt-get update

# Install Python 3 and pip
sudo apt-get install -y python3 python3-pip python3-venv

# Verify installation
python3 --version
pip3 --version
```

### Step 3: Verify MCP Files Are Deployed

Check if files exist on server:
```bash
ls -la /var/www/discussionspot/mcp-servers/seo-automation/
```

**If files don't exist, copy them:**

#### Option A: Via Git (Recommended)
```bash
cd /var/www/discussionspot
git pull origin master  # This pulls your latest code including mcp-servers
```

#### Option B: Via SCP (from Windows)
```powershell
# From Windows PowerShell
scp -r "discussionspot9\mcp-servers" user@discussionspot.com:/var/www/discussionspot/
```

#### Option C: Via SFTP Client
- Use FileZilla, WinSCP, or similar
- Upload `discussionspot9/mcp-servers/*` to `/var/www/discussionspot/mcp-servers/`

### Step 4: Run the Ubuntu Fix Script

```bash
cd /var/www/discussionspot/mcp-servers

# Make script executable
chmod +x fix-mcp-ubuntu.sh

# Run the setup script
./fix-mcp-ubuntu.sh
```

This script will:
- ? Verify Python is installed
- ? Install pip dependencies
- ? Set file permissions
- ? Test the server startup

### Step 5: Create/Update appsettings.Production.json

**Edit:** `/var/www/discussionspot/appsettings.Production.json`

**Add or verify these sections:**
```json
{
  "Python": {
    "ExecutablePath": "/usr/bin/python3",
    "ScriptTimeout": 30
  },
  "MCP": {
    "AutoStart": {
      "Enabled": true,
      "RetryDelaySeconds": 10,
      "MaxRetries": 3,
      "HealthCheckIntervalSeconds": 30
    },
    "Servers": {
      "SeoAutomation": {
        "Enabled": true,
        "Port": 5001,
        "Endpoint": "http://localhost:5001",
        "Timeout": 30
      },
      "WebStoryValidator": {
        "Enabled": true,
        "Port": 5004,
        "Endpoint": "http://localhost:5004",
        "Timeout": 30
      }
    }
  }
}
```

### Step 6: Deploy Updated Application

**If using automated deployment (GitHub Actions, etc.):**
- Push your changes to GitHub
- Deployment will automatically pull latest code

**If deploying manually:**
```bash
cd /var/www/discussionspot

# Stop old application
sudo systemctl stop your-app-service  # or manually stop it

# Pull latest code
git pull origin master

# Publish latest build
dotnet publish -c Release -o ./publish

# Start application
sudo systemctl start your-app-service  # or manually start it
```

### Step 7: Verify on Ubuntu

```bash
# Check if servers are running
curl http://localhost:5001/health

# Check application logs (if systemd service)
sudo journalctl -u your-app-service -f

# Or check log files directly
tail -f /var/www/discussionspot/logs/*.log
```

? **Ubuntu setup is complete!**

---

## ?? Troubleshooting

### Issue: "Python not found"

**Solution:**
```bash
# Check Python path
which python3
# Should show: /usr/bin/python3

# If not found, install it
sudo apt-get install -y python3 python3-pip

# Verify again
python3 --version
```

### Issue: "Module not found" (FastAPI, uvicorn, etc.)

**Solution:**
```bash
cd /var/www/discussionspot/mcp-servers/seo-automation

# Install dependencies
pip3 install -r requirements.txt

# Or with user flag if permission denied
pip3 install --user -r requirements.txt
```

### Issue: "Permission denied" when running script

**Solution:**
```bash
chmod +x /var/www/discussionspot/mcp-servers/*.py
chmod +x /var/www/discussionspot/mcp-servers/*/main.py
```

### Issue: "Server not responding" on http://localhost:5001/health

**Solution:**
```bash
# Test manually starting the server
cd /var/www/discussionspot/mcp-servers/seo-automation
python3 main.py

# You should see:
# INFO:     Uvicorn running on http://0.0.0.0:5001

# Press Ctrl+C to stop
```

### Issue: "Connection refused" in application logs

**Solutions:**
1. Check that MCP server is actually running:
   ```bash
   netstat -tlnp | grep 5001
   # Should show Python process on port 5001
   ```

2. Check firewall:
   ```bash
   sudo ufw status
   # If firewall is active, allow port 5001
   sudo ufw allow 5001
   ```

3. Check application can reach localhost:
   ```bash
   curl http://localhost:5001/health
   # From the server machine, this should work
   ```

---

## ?? Verification Checklist

### Local Machine (Windows)
- [ ] Edited `appsettings.json` - Python path is empty
- [ ] Ran `fix-mcp-windows.bat`
- [ ] Application starts without errors
- [ ] Logs show "Server SeoAutomation started on port 5001"
- [ ] `http://localhost:5001/health` returns OK

### Ubuntu Server
- [ ] SSH'd into server
- [ ] Installed Python 3 and pip
- [ ] Verified MCP files exist at `/var/www/discussionspot/mcp-servers/`
- [ ] Ran `fix-mcp-ubuntu.sh` successfully
- [ ] Updated `appsettings.Production.json` with `/usr/bin/python3` path
- [ ] Deployed latest application
- [ ] Application logs show "Server SeoAutomation started on port 5001"
- [ ] `curl http://localhost:5001/health` returns OK

---

## ?? Next Steps After Fixing

Once MCP servers are working:

1. **Test SEO Features**
   - Create a new post
   - Check if SEO optimization works
   - Verify keywords are being extracted

2. **Monitor Server Health**
   - Check application logs regularly
   - Set up alerts for server crashes
   - Monitor port 5001 availability

3. **Optional: Set Up Auto-Restart**
   ```bash
   # Create systemd service file (Ubuntu)
   sudo nano /etc/systemd/system/mcp-seo-server.service
   ```
   
   Add:
   ```ini
   [Unit]
   Description=MCP SEO Automation Server
   After=network.target

   [Service]
   Type=simple
   User=www-data
   WorkingDirectory=/var/www/discussionspot/mcp-servers/seo-automation
   ExecStart=/usr/bin/python3 main.py
   Restart=always
   RestartSec=10

   [Install]
   WantedBy=multi-user.target
   ```

   Then:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable mcp-seo-server
   sudo systemctl start mcp-seo-server
   ```

---

## ?? Still Having Issues?

1. **Check the diagnostic report:** `discussionspot9/docs/MCP_DIAGNOSIS_AND_SOLUTIONS.md`
2. **Enable debug logging in appsettings:**
   ```json
   "Logging": {
     "LogLevel": {
       "discussionspot9.Services.MCP": "Debug"
     }
   }
   ```
3. **Run manual server test:**
   ```bash
   cd mcp-servers/seo-automation
   python3 main.py
   # Check for error messages
   ```

---

## ?? Reference Documentation

- Full Diagnosis: `discussionspot9/docs/MCP_DIAGNOSIS_AND_SOLUTIONS.md`
- MCP README: `discussionspot9/mcp-servers/README.md`
- Ubuntu Setup: `discussionspot9/mcp-servers/QUICK_START_UBUNTU.md`
- Deployment Guide: `discussionspot9/docs/deployment/UBUNTU_MCP_SETUP.md`

---

**Last Updated:** December 2024  
**Status:** Ready to implement
