# ⚡ Quick Fix Checklist - Google Auth & MCP Servers

## 🔐 Google Auth Login - Quick Fix

### Step 1: Verify Credentials (30 seconds)
1. Check file exists: `discussionspot9/Secrets/AuthKeys.json` ✅
2. Verify it contains:
   - `ClientId`: Should start with numbers
   - `ClientSecret`: Should start with `GOCSPX-`
   - `redirect_uris`: Should include `http://localhost:5099/signin-google`

### Step 2: Check Google Console (2 minutes)
1. Go to: https://console.cloud.google.com/apis/credentials
2. Find your OAuth 2.0 Client ID
3. Verify **Authorized redirect URIs** includes:
   - `http://localhost:5099/signin-google`
   - `https://discussionspot.com/signin-google`

### Step 3: Test Login (1 minute)
1. Clear browser cookies
2. Go to: `http://localhost:5099/Account/Auth`
3. Click "Sign in with Google"
4. Should redirect to Google login

### If Still Not Working:
- Check application logs for errors
- Try incognito mode
- Check browser console (F12) for errors
- Verify redirect URI matches exactly

---

## 🖥️ MCP Servers - Quick Fix

### Step 1: Check Diagnostics (30 seconds)
1. Go to: `http://localhost:5099/admin/mcp-status`
2. Click "Diagnostics" button
3. Check what's missing

### Step 2: Install Python Dependencies (1 minute)
```powershell
cd discussionspot9\mcp-servers\seo-automation
pip install -r requirements.txt
```

### Step 3: Test Manual Start (1 minute)
1. In admin dashboard, click "Start SEO Server"
2. Wait 5 seconds
3. Click "Refresh Status"
4. Should show "Online"

### Step 4: Verify Server (30 seconds)
```powershell
curl http://localhost:5001/health
```
Should return JSON with `"status": "healthy"`

### If Still Offline:

**Check Python:**
```powershell
python --version
# Should show Python 3.x.x
```

**Check Script Exists:**
```powershell
Test-Path "discussionspot9\mcp-servers\seo-automation\main.py"
# Should return True
```

**Check Dependencies:**
```powershell
python -c "import fastapi; print('OK')"
python -c "import uvicorn; print('OK')"
python -c "import requests; print('OK')"
```

**Check Application Logs:**
- Look for "MCP Server Manager" messages
- Check for Python errors
- Check for path errors

---

## ✅ Success Checklist

### Google Auth:
- [ ] Can click "Sign in with Google"
- [ ] Redirects to Google login page
- [ ] After login, redirects back
- [ ] User is signed in
- [ ] No error messages

### MCP Servers:
- [ ] Diagnostics show all green
- [ ] Status shows "Online"
- [ ] Response time < 100ms
- [ ] Health endpoint returns 200
- [ ] No errors in logs

---

## 🆘 Still Having Issues?

### For Google Auth:
1. Check `Secrets/AuthKeys.json` file
2. Verify redirect URI in Google Console
3. Clear browser cookies
4. Restart application
5. Check application logs

### For MCP Servers:
1. Check diagnostics endpoint
2. Install Python if missing
3. Install dependencies
4. Try manual start
5. Check application logs
6. Verify script path exists

---

## 📞 Need More Help?

- **Google Auth Guide:** `docs/FIXES/GOOGLE_AUTH_AND_MCP_FIX.md`
- **MCP Troubleshooting:** `docs/MCP/TROUBLESHOOTING.md`
- **Auto-Start Guide:** `docs/MCP/AUTO_START_GUIDE.md`

