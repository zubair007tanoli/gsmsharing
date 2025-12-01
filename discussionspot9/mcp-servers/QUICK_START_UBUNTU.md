# 🚀 Quick Start: Ubuntu MCP Server Setup

## Step-by-Step Instructions

### Option 1: Deploy via Git (Recommended)

If your code is in a git repository:

```bash
# SSH into your Ubuntu server
ssh user@your-server-ip

# Navigate to your application directory
cd /var/www/discussionspot

# Pull the latest code (includes the setup scripts)
git pull origin main  # or your branch name

# Run the setup script
chmod +x mcp-servers/ubuntu-setup.sh
./mcp-servers/ubuntu-setup.sh
```

### Option 2: Copy Files Manually

If you need to copy files manually:

**On Windows (your current machine):**

1. **Copy the entire mcp-servers directory to Ubuntu:**
   ```powershell
   # Using SCP (if you have it installed)
   scp -r discussionspot9\mcp-servers user@your-server-ip:/var/www/discussionspot/
   
   # Or use WinSCP, FileZilla, or any SFTP client
   # Upload: discussionspot9/mcp-servers/* to /var/www/discussionspot/mcp-servers/
   ```

2. **Or create the script directly on Ubuntu:**
   ```bash
   # SSH into Ubuntu
   ssh user@your-server-ip
   
   # Create the directory
   mkdir -p /var/www/discussionspot/mcp-servers
   
   # Copy the script content (you can copy-paste from the file)
   nano /var/www/discussionspot/mcp-servers/ubuntu-setup.sh
   # Paste the script content, save (Ctrl+X, Y, Enter)
   ```

### Option 3: One-Line Setup (If you have direct access)

```bash
# SSH into your server and run:
cd /var/www/discussionspot && \
git pull && \
chmod +x mcp-servers/ubuntu-setup.sh && \
./mcp-servers/ubuntu-setup.sh
```

---

## 📋 What the Script Does

The `ubuntu-setup.sh` script will:

1. ✅ **Install Python 3** (if not already installed)
2. ✅ **Create MCP servers directory** at `/var/www/discussionspot/mcp-servers`
3. ✅ **Copy server files** (seo-automation, web-story-validator)
4. ✅ **Install Python dependencies** (fastapi, uvicorn, etc.)
5. ✅ **Set proper permissions** on scripts
6. ✅ **Create systemd service** (optional, for auto-start)
7. ✅ **Verify installation** and show you what was found

---

## 🎯 After Running the Script

### 1. Verify Installation

```bash
# Run the verification script
chmod +x mcp-servers/verify-installation.sh
./mcp-servers/verify-installation.sh
```

This will check:
- Python installation
- Server script locations
- Dependencies
- Permissions

### 2. Test the Server Manually

```bash
# Start the SEO automation server
cd /var/www/discussionspot/mcp-servers/seo-automation
python3 main.py
```

You should see:
```
INFO:     Started server process
INFO:     Uvicorn running on http://0.0.0.0:5001
```

Press `Ctrl+C` to stop it.

### 3. Check Cursor MCP Diagnostics

1. Open Cursor
2. Go to Settings → MCP Servers
3. Check Diagnostics
4. You should now see:
   - ✅ Python Installed: Yes
   - ✅ Python Version: 3.10.x (or similar)
   - ✅ Server Script Exists: Yes

---

## 🔧 Troubleshooting

### Script says "Permission Denied"

```bash
chmod +x /var/www/discussionspot/mcp-servers/ubuntu-setup.sh
```

### Script can't find files

Make sure you're in the right directory:
```bash
cd /var/www/discussionspot
ls -la mcp-servers/  # Should show seo-automation, web-story-validator
```

### Python dependencies fail to install

```bash
# Install pip if missing
sudo apt-get install python3-pip

# Try installing with sudo (if user install fails)
sudo pip3 install -r mcp-servers/seo-automation/requirements.txt
```

### Cursor still shows servers offline

1. **Check if the script path is correct:**
   ```bash
   ls -la /var/www/discussionspot/mcp-servers/seo-automation/main.py
   ```

2. **Check Python path:**
   ```bash
   which python3
   # Should return: /usr/bin/python3
   ```

3. **Restart Cursor** after running the setup script

---

## 📝 Next Steps

After setup is complete:

1. ✅ **Start servers** (manually or via systemd)
2. ✅ **Verify in Cursor** MCP diagnostics
3. ✅ **Test SEO features** in your application
4. ✅ **Set up auto-start** (optional, using systemd)

---

## 💡 Quick Reference

```bash
# Check Python
python3 --version

# Check if script exists
ls -la /var/www/discussionspot/mcp-servers/seo-automation/main.py

# Run setup
./mcp-servers/ubuntu-setup.sh

# Verify
./mcp-servers/verify-installation.sh

# Start server manually
python3 /var/www/discussionspot/mcp-servers/seo-automation/main.py
```

---

## 🆘 Still Need Help?

1. Check the full guide: `docs/deployment/UBUNTU_MCP_SETUP.md`
2. Run verification: `./mcp-servers/verify-installation.sh`
3. Check logs: Look at the script output for any errors

