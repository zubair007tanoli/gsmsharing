# SEO Automation MCP Server (FREE AI)

## 🆓 100% Free - No API Costs!

This MCP server uses **Ollama** (local AI) for SEO optimization - completely free!

## 🚀 Quick Start

### 1. Install Ollama
```bash
# Download from https://ollama.com/download
# Or on Linux/Mac:
curl -fsSL https://ollama.com/install.sh | sh
```

### 2. Download AI Model
```bash
# Recommended: llama3.2 (fast, good quality)
ollama pull llama3.2

# Or for better quality (needs more RAM):
ollama pull mistral
```

### 3. Install Python Dependencies
```bash
pip install -r requirements.txt
```

### 4. Start Server
```bash
python main.py
```

Server will run on: `http://localhost:5001`

## ✅ Test

```bash
# Health check
curl http://localhost:5001/health

# Test SEO optimization
curl -X POST http://localhost:5001/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "optimize_title",
    "params": {"title": "python tutorial"},
    "id": "1"
  }'
```

## 📋 Available Methods

- `optimize_title` - Optimize post title for SEO
- `generate_meta_description` - Generate SEO meta description
- `extract_keywords` - Extract keywords from content
- `optimize_content` - Optimize post content for SEO
- `analyze_seo` - Get SEO score and suggestions

## ⚙️ Configuration

Edit `main.py` to change:
- Model: `llama3.2`, `mistral`, or `llama3.1`
- Port: Default is 5001

## 💡 Tips

- **Fast responses:** Use `llama3.2` (3B model)
- **Better quality:** Use `mistral` or `llama3.1` (7B-8B models)
- **Low RAM:** Use `tinyllama` (works on 2GB RAM)

