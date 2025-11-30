# 🔧 Google Auth & MCP Servers - Complete Fix Guide

## 🎯 Issues Fixed

### 1. ✅ Google Auth Login
- Enhanced cookie configuration
- Better redirect URI handling
- Improved error handling

### 2. ✅ MCP Servers Path Resolution
- Multiple path search strategies
- Parent directory detection
- Better error messages

---

## 🔐 Google Auth Fix

### What Was Fixed:

1. **External Cookie Configuration:**
   - Added explicit cookie name
   - Set proper expiry time
   - Improved SameSite settings

2. **Google OAuth Options:**
   - Added `AccessType = "offline"`
   - Added `Prompt = "select_account"`

### How to Test:

1. **Go to login page:**
   ```
   http://localhost:5099/Account/Auth
   ```

2. **Click "Sign in with Google"**

3. **Check for errors:**
   - If redirect fails, check browser console
   - Check application logs
   - Verify redirect URI in Google Console

### Common Issues:

**Issue: "redirect_uri_mismatch"**
- **Fix:** Make sure Google Console has:
  - `http://localhost:5099/signin-google` (for development)
  - `https://discussionspot.com/signin-google` (for production)

**Issue: Cookie not set**
- **Fix:** Check browser settings
- Clear cookies and try again
- Check if SameSite is blocking

**Issue: "Error loading external login information"**
- **Fix:** Check if Google credentials are loaded
- Verify `Secrets/AuthKeys.json` exists
- Check application logs for credential errors

---

## 🖥️ MCP Servers Fix

### What Was Fixed:

1. **Enhanced Path Resolution:**
   - Searches multiple possible paths
   - Handles bin/Debug and bin/Release folders
   - Checks parent directories
   - Better error messages

2. **Diagnostic Endpoint:**
   - New endpoint: `/admin/mcp-status/diagnostics`
   - Shows Python installation status
   - Shows path information
   - Shows configuration status

### How to Test:

1. **Check Diagnostics:**
   ```
   http://localhost:5099/admin/mcp-status/diagnostics
   ```

2. **Check Admin Dashboard:**
   ```
   http://localhost:5099/admin/mcp-status
   ```
   - Look at Diagnostics section
   - Check if Python is installed
   - Check if script path exists

3. **Try Manual Start:**
   - Click "Start SEO Server" button
   - Check for error messages
   - Check application logs

### Common Issues:

**Issue: "Server script not found"**
- **Fix:** Check diagnostics endpoint
- Verify path in diagnostics
- Make sure `mcp-servers/seo-automation/main.py` exists

**Issue: "Python not found"**
- **Fix:** Install Python from https://www.python.org/downloads/
- Make sure to check "Add Python to PATH"
- Restart application after installing

**Issue: "Module not found"**
- **Fix:** Install dependencies:
  ```powershell
  cd mcp-servers\seo-automation
  pip install -r requirements.txt
  ```

**Issue: "Port already in use"**
- **Fix:** Find and kill process:
  ```powershell
  netstat -ano | findstr :5001
  taskkill /PID <PID> /F
  ```

---

## 🧪 Testing Steps

### Test Google Auth:

1. **Clear browser cookies**
2. **Go to:** `http://localhost:5099/Account/Auth`
3. **Click:** "Sign in with Google"
4. **Should:** Redirect to Google login
5. **After login:** Should redirect back and sign you in

### Test MCP Servers:

1. **Check diagnostics:**
   ```
   http://localhost:5099/admin/mcp-status/diagnostics
   ```

2. **Check status page:**
   ```
   http://localhost:5099/admin/mcp-status
   ```

3. **Try manual start:**
   - Click "Start SEO Server"
   - Wait a few seconds
   - Refresh status
   - Should show "Online"

4. **Test health endpoint:**
   ```powershell
   curl http://localhost:5001/health
   ```

---

## 📊 Diagnostic Information

### Check Diagnostics Endpoint:

```json
{
  "pythonInstalled": true,
  "pythonVersion": "Python 3.14.0",
  "serverScriptExists": true,
  "serverScriptPath": "...",
  "contentRootPath": "...",
  "baseDirectory": "...",
  "currentDirectory": "...",
  "autoStartEnabled": true,
  "seoServerEnabled": true,
  "googleAuthConfigured": true
}
```

### What to Look For:

- ✅ `pythonInstalled: true` - Python is available
- ✅ `serverScriptExists: true` - Script file found
- ✅ `autoStartEnabled: true` - Auto-start is on
- ✅ `googleAuthConfigured: true` - Google Auth is configured

---

## 🔍 Troubleshooting

### Google Auth Not Working:

1. **Check credentials:**
   - Verify `Secrets/AuthKeys.json` exists
   - Check ClientId and ClientSecret are correct

2. **Check redirect URI:**
   - Must match exactly in Google Console
   - `http://localhost:5099/signin-google`

3. **Check logs:**
   - Look for "Google OAuth" messages
   - Check for credential errors

4. **Test manually:**
   - Try clearing cookies
   - Try incognito mode
   - Check browser console for errors

### MCP Servers Not Starting:

1. **Check diagnostics:**
   - Go to `/admin/mcp-status/diagnostics`
   - See what's missing

2. **Check Python:**
   ```powershell
   python --version
   ```

3. **Check dependencies:**
   ```powershell
   cd mcp-servers\seo-automation
   pip install -r requirements.txt
   ```

4. **Check logs:**
   - Look for "MCP Server Manager" messages
   - Check for Python errors
   - Check for path errors

5. **Try manual start:**
   ```powershell
   cd mcp-servers\seo-automation
   python main.py
   ```

---

## ✅ Success Indicators

### Google Auth Working:
- ✅ Can click "Sign in with Google"
- ✅ Redirects to Google login
- ✅ After login, redirects back
- ✅ User is signed in
- ✅ No error messages

### MCP Servers Working:
- ✅ Status shows "Online"
- ✅ Response time < 100ms
- ✅ Health endpoint returns 200
- ✅ No errors in logs
- ✅ Diagnostics show all green

---

## 🎯 Quick Fixes

### If Google Auth Fails:
1. Check `Secrets/AuthKeys.json` exists
2. Verify redirect URI in Google Console
3. Clear browser cookies
4. Restart application

### If MCP Servers Offline:
1. Check diagnostics endpoint
2. Install Python if missing
3. Install dependencies: `pip install -r requirements.txt`
4. Try manual start button
5. Check application logs

---

## 📚 Related Documentation

- **MCP Setup:** `docs/MCP_SERVER_IMPLEMENTATION_GUIDE.md`
- **Troubleshooting:** `docs/MCP/TROUBLESHOOTING.md`
- **Auto-Start:** `docs/MCP/AUTO_START_GUIDE.md`

