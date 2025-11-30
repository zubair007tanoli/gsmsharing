# 🚀 MCP Servers - Quick Start Guide

## 📋 What Are MCP Servers?

MCP (Multi-Cloud Platform) servers are **Python FastAPI microservices** that provide specialized functionality:
- **SEO Automation** (Port 5001) - Post optimization using FREE local AI
- **Performance** (Port 5002) - Performance monitoring (coming soon)
- **User Preferences** (Port 5003) - User preferences (coming soon)

---

## ⚡ Quick Start (5 Minutes)

### Step 1: Install Python Dependencies

```powershell
cd seo-automation
pip install -r requirements.txt
cd ..
```

### Step 2: Start SEO Server

**Option A: Use PowerShell Script (Easiest)**
```powershell
.\start-seo-server.ps1
```

**Option B: Manual Start**
```powershell
cd seo-automation
python main.py
```

### Step 3: Verify It's Running

Open browser: http://localhost:5001/health

You should see:
```json
{
  "status": "healthy",
  "timestamp": "...",
  "server": "SEO Automation",
  "ai_available": true,
  "model": "llama3.2"
}
```

---

## 🆓 FREE AI Setup (Required for SEO Server)

The SEO server uses **Ollama** (free local AI). You need to:

### 1. Install Ollama
- Download: https://ollama.com/download
- Or: `curl -fsSL https://ollama.com/install.sh | sh`

### 2. Download AI Model
```bash
ollama pull llama3.2
```

### 3. Test Ollama
```bash
ollama run llama3.2 "test"
```

### 4. Keep Ollama Running
```bash
ollama serve
```

---

## ✅ Check Server Status

### In Admin Dashboard:
1. Go to: http://localhost:5099/admin/mcp-status
2. You should see SEO Automation as "Online" ✅

### Via Command Line:
```powershell
# Test health endpoint
curl http://localhost:5001/health

# Test SEO optimization
curl -X POST http://localhost:5001/mcp `
  -H "Content-Type: application/json" `
  -d '{\"jsonrpc\":\"2.0\",\"method\":\"optimize_title\",\"params\":{\"title\":\"python tutorial\"},\"id\":\"1\"}'
```

---

## 🔧 Troubleshooting

### Server Won't Start

**Error: "Module not found"**
```powershell
cd seo-automation
pip install -r requirements.txt
```

**Error: "Port already in use"**
- Another process is using port 5001
- Change port in `main.py`: `uvicorn.run(app, host="0.0.0.0", port=5001)`

**Error: "Ollama connection failed"**
- Make sure Ollama is running: `ollama serve`
- Check if model is downloaded: `ollama list`
- Download model: `ollama pull llama3.2`

### Server Shows as Offline

1. **Check if server is running:**
   ```powershell
   # Should return JSON response
   curl http://localhost:5001/health
   ```

2. **Check firewall:**
   - Windows Firewall might be blocking port 5001
   - Allow Python through firewall

3. **Check configuration:**
   - Verify `appsettings.json` has correct endpoint:
   ```json
   "MCP": {
     "Servers": {
       "SeoAutomation": {
         "Endpoint": "http://localhost:5001"
       }
     }
   }
   ```

---

## 📊 Server Endpoints

### SEO Automation Server (Port 5001)

**Health Check:**
```
GET http://localhost:5001/health
```

**MCP Protocol:**
```
POST http://localhost:5001/mcp
```

**Available Methods:**
- `optimize_title` - Optimize post title
- `generate_meta_description` - Generate SEO meta description
- `extract_keywords` - Extract keywords from content
- `optimize_content` - Optimize post content
- `analyze_seo` - Get SEO score and suggestions

---

## 🎯 Next Steps

1. ✅ Start SEO server
2. ✅ Verify it's online in admin dashboard
3. ✅ Test with a post creation
4. ✅ Check logs for any errors

---

## 💡 Tips

- **Keep servers running** - Start them in separate terminal windows
- **Use PowerShell scripts** - Easier to start/stop
- **Check logs** - Server output shows what's happening
- **Ollama must be running** - SEO server needs Ollama for AI features

---

## 📚 More Information

- Setup Guide: `docs/AI/LOCAL_AI_SETUP_GUIDE.md`
- Integration Guide: `docs/AI/LOCAL_AI_INTEGRATION_GUIDE.md`
- MCP Implementation: `docs/MCP_SERVER_IMPLEMENTATION_GUIDE.md`

