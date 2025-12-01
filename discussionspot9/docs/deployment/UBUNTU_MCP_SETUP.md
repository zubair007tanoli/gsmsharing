# Ubuntu 22.04 MCP Server Setup Guide

## 🚀 Quick Setup

### Step 1: Run the Setup Script

```bash
cd /var/www/discussionspot/mcp-servers
chmod +x ubuntu-setup.sh
./ubuntu-setup.sh
```

This script will:
- ✅ Install Python 3 and pip
- ✅ Create MCP servers directory structure
- ✅ Copy server files to correct location
- ✅ Install Python dependencies
- ✅ Set proper permissions
- ✅ Create systemd service files (optional)

### Step 2: Verify Installation

```bash
chmod +x verify-installation.sh
./verify-installation.sh
```

---

## 📋 Manual Setup (If Script Fails)

### 1. Install Python 3

```bash
sudo apt-get update
sudo apt-get install -y python3 python3-pip python3-venv
```

Verify installation:
```bash
python3 --version
# Should show: Python 3.10.x or higher
```

### 2. Create MCP Servers Directory

```bash
sudo mkdir -p /var/www/discussionspot/mcp-servers
sudo chown -R $USER:$USER /var/www/discussionspot/mcp-servers
```

### 3. Copy MCP Server Files

If your application is deployed to `/var/www/discussionspot`, copy the MCP servers:

```bash
# If deploying from source
cp -r /path/to/source/mcp-servers/* /var/www/discussionspot/mcp-servers/

# Or if using git
cd /var/www/discussionspot
git pull  # This should include mcp-servers directory
```

### 4. Install Python Dependencies

```bash
cd /var/www/discussionspot/mcp-servers/seo-automation
python3 -m pip install --user -r requirements.txt

cd ../web-story-validator
python3 -m pip install --user -r requirements.txt
```

### 5. Set Permissions

```bash
chmod +x /var/www/discussionspot/mcp-servers/seo-automation/main.py
chmod +x /var/www/discussionspot/mcp-servers/web-story-validator/main.py
```

---

## 🔍 Troubleshooting

### Issue: "Python Installed: No"

**Solution:**
```bash
# Check if Python is installed
python3 --version

# If not found, install it
sudo apt-get update
sudo apt-get install -y python3 python3-pip

# Verify PATH includes Python
which python3
```

### Issue: "Server Script Not Found"

The error shows it's looking in:
- `/var/www/discussionspot/mcp-servers/seo-automation/main.py`
- `/var/www/discussionspot9/mcp-servers/seo-automation/main.py`

**Solution:**

1. **Check where your app is deployed:**
   ```bash
   # Find your application directory
   find /var/www -name "discussionspot*" -type d
   ```

2. **Copy MCP servers to the correct location:**
   ```bash
   # If app is at /var/www/discussionspot
   sudo mkdir -p /var/www/discussionspot/mcp-servers
   sudo cp -r /path/to/source/mcp-servers/* /var/www/discussionspot/mcp-servers/
   sudo chown -R $USER:$USER /var/www/discussionspot/mcp-servers
   ```

3. **Or create a symlink:**
   ```bash
   # If your source is elsewhere
   sudo ln -s /path/to/source/mcp-servers /var/www/discussionspot/mcp-servers
   ```

### Issue: "Module not found" when running server

**Solution:**
```bash
cd /var/www/discussionspot/mcp-servers/seo-automation
python3 -m pip install --user -r requirements.txt
```

If using a virtual environment:
```bash
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
```

### Issue: Permission Denied

**Solution:**
```bash
# Make scripts executable
chmod +x /var/www/discussionspot/mcp-servers/*/main.py

# Fix ownership
sudo chown -R $USER:$USER /var/www/discussionspot/mcp-servers
```

---

## 🎯 Running MCP Servers

### Option 1: Manual Start (Development)

```bash
cd /var/www/discussionspot/mcp-servers/seo-automation
python3 main.py
```

### Option 2: Systemd Service (Production)

The setup script creates a systemd service. To use it:

```bash
# Enable service
sudo systemctl enable mcp-seo-automation

# Start service
sudo systemctl start mcp-seo-automation

# Check status
sudo systemctl status mcp-seo-automation

# View logs
sudo journalctl -u mcp-seo-automation -f
```

### Option 3: Screen/Tmux (Simple Production)

```bash
# Install screen
sudo apt-get install screen

# Start server in screen session
screen -S mcp-seo
cd /var/www/discussionspot/mcp-servers/seo-automation
python3 main.py

# Detach: Ctrl+A, then D
# Reattach: screen -r mcp-seo
```

---

## ✅ Verification Checklist

After setup, verify:

- [ ] Python 3 is installed: `python3 --version`
- [ ] Server script exists: `ls /var/www/discussionspot/mcp-servers/seo-automation/main.py`
- [ ] Dependencies installed: `python3 -c "import fastapi; print('OK')"`
- [ ] Script is executable: `test -x /var/www/discussionspot/mcp-servers/seo-automation/main.py`
- [ ] Server can start: `python3 /var/www/discussionspot/mcp-servers/seo-automation/main.py`

---

## 📝 Cursor Configuration

After setup, Cursor should automatically detect the MCP servers. If not:

1. **Check Cursor MCP Diagnostics:**
   - Open Cursor Settings
   - Go to MCP Servers
   - Check diagnostics

2. **Verify Paths:**
   - Cursor looks for: `/var/www/discussionspot/mcp-servers/seo-automation/main.py`
   - Or: `/var/www/discussionspot9/mcp-servers/seo-automation/main.py`

3. **Check Python Path:**
   - Cursor needs to find `python3` in PATH
   - Verify: `which python3` returns a path

---

## 🔄 Updating MCP Servers

When you update your code:

```bash
cd /var/www/discussionspot
git pull  # Or copy updated files

# Reinstall dependencies if requirements.txt changed
cd mcp-servers/seo-automation
python3 -m pip install --user -r requirements.txt

# Restart service if using systemd
sudo systemctl restart mcp-seo-automation
```

---

## 📚 Additional Resources

- [MCP Server README](../../mcp-servers/README.md)
- [Python SEO Setup Guide](../features/seo/PYTHON_SEO_SETUP_GUIDE.md)
- [Troubleshooting Guide](../MCP/TROUBLESHOOTING.md)

---

## 🆘 Still Having Issues?

1. Run verification script: `./verify-installation.sh`
2. Check logs: `sudo journalctl -u mcp-seo-automation -n 50`
3. Test manually: `python3 /var/www/discussionspot/mcp-servers/seo-automation/main.py`
4. Check file permissions: `ls -la /var/www/discussionspot/mcp-servers/`

