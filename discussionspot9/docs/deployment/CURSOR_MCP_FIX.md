# Fix Cursor MCP Autostart "Server Script Not Found" Error

## 🔴 Problem

When clicking "Autostart" in Cursor for MCP servers, you get:
```
❌ Server script not found. Searched: 
/var/www/discussionspot/mcp-servers/seo-automation/main.py
```

## ✅ Solution

### Step 1: Run the Fix Script

On your Ubuntu server:

```bash
cd /var/www/discussionspot/mcp-servers
chmod +x fix-cursor-mcp.sh
./fix-cursor-mcp.sh
```

This script will:
- ✅ Find where your MCP server files actually are
- ✅ Create the Cursor MCP configuration file (`.cursor/mcp.json`)
- ✅ Set correct paths in the configuration
- ✅ Verify Python can run the scripts

### Step 2: Restart Cursor

After running the script:
1. **Close Cursor completely**
2. **Reopen Cursor**
3. Go to **Settings → MCP Servers**
4. Click **"Autostart"** for seo-automation
5. Check **Diagnostics** - should show ✅

---

## 🔧 Manual Fix (If Script Doesn't Work)

### 1. Find Where Your MCP Files Are

```bash
# Check common locations
ls -la /var/www/discussionspot/mcp-servers/seo-automation/main.py
ls -la /var/www/discussionspot9/mcp-servers/seo-automation/main.py

# Or search for it
find /var/www -name "main.py" -path "*/mcp-servers/seo-automation/*"
```

### 2. Create Cursor MCP Configuration

Create the file: `/var/www/discussionspot/.cursor/mcp.json`

```json
{
  "mcpServers": {
    "seo-automation": {
      "command": "python3",
      "args": [
        "/var/www/discussionspot/mcp-servers/seo-automation/main.py"
      ],
      "env": {
        "PYTHONUNBUFFERED": "1"
      }
    }
  }
}
```

**Important:** Replace the path in `args` with the **actual path** where your `main.py` file is located.

### 3. Verify the Path is Correct

```bash
# Test if the path works
python3 /var/www/discussionspot/mcp-servers/seo-automation/main.py

# If you get "Module not found", install dependencies:
cd /var/www/discussionspot/mcp-servers/seo-automation
pip3 install -r requirements.txt
```

---

## 📋 Common Issues

### Issue: "File not found" even after creating config

**Solution:**
1. Make sure the path in `mcp.json` matches exactly where the file is
2. Use absolute paths (starting with `/`)
3. Check file permissions: `ls -la /var/www/discussionspot/mcp-servers/seo-automation/main.py`

### Issue: "Permission denied"

**Solution:**
```bash
chmod +x /var/www/discussionspot/mcp-servers/seo-automation/main.py
```

### Issue: "Module not found" when starting

**Solution:**
```bash
cd /var/www/discussionspot/mcp-servers/seo-automation
pip3 install --user -r requirements.txt
```

### Issue: Cursor still can't find it after restart

**Solution:**
1. Check if `.cursor/mcp.json` exists: `ls -la /var/www/discussionspot/.cursor/mcp.json`
2. Verify JSON is valid: `python3 -m json.tool /var/www/discussionspot/.cursor/mcp.json`
3. Make sure you're opening Cursor in the correct workspace directory
4. Try creating the config in your home directory: `~/.cursor/mcp.json`

---

## 🎯 Configuration File Locations

Cursor looks for MCP config in this order:

1. **Project config:** `{workspace}/.cursor/mcp.json` ← **Use this**
2. **Global config:** `~/.cursor/mcp.json` (user home)

For a deployed app, use the project config at:
```
/var/www/discussionspot/.cursor/mcp.json
```

---

## ✅ Verification Checklist

After fixing:

- [ ] MCP server file exists: `ls /var/www/discussionspot/mcp-servers/seo-automation/main.py`
- [ ] Cursor config exists: `ls /var/www/discussionspot/.cursor/mcp.json`
- [ ] Python can run it: `python3 /var/www/discussionspot/mcp-servers/seo-automation/main.py`
- [ ] Cursor shows server in diagnostics
- [ ] Autostart works without errors

---

## 🆘 Still Not Working?

1. **Check Cursor logs:**
   - Open Cursor
   - Go to Help → Toggle Developer Tools
   - Check Console for MCP errors

2. **Test manually:**
   ```bash
   python3 /var/www/discussionspot/mcp-servers/seo-automation/main.py
   ```
   If this fails, the issue is with Python/dependencies, not Cursor config.

3. **Verify workspace:**
   - Make sure Cursor is opened in `/var/www/discussionspot`
   - Or adjust the paths in `mcp.json` to match your actual workspace

4. **Check file ownership:**
   ```bash
   ls -la /var/www/discussionspot/mcp-servers/seo-automation/main.py
   # Make sure your user can read/execute it
   ```

