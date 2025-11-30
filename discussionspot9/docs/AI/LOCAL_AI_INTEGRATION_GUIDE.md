# 🆓 Local AI Integration Guide

## ✅ What's Been Created

### 1. **LocalAIService.cs** (.NET Service)
- Free AI service using Ollama
- Methods for title optimization, meta descriptions, keyword extraction
- No API costs!

### 2. **local_ai_service.py** (Python Service)
- Python wrapper for Ollama
- Can be used in Python scripts or MCP servers
- Same functionality as .NET service

### 3. **MCP SEO Server Template**
- Ready-to-use FastAPI server
- Integrated with Ollama
- Runs on port 5001

---

## 🚀 Setup Steps

### Step 1: Install Ollama

**Windows:**
1. Download: https://ollama.com/download
2. Install and run
3. Verify: `ollama --version`

**Linux/Mac:**
```bash
curl -fsSL https://ollama.com/install.sh | sh
```

### Step 2: Download AI Model

```bash
# Recommended for speed (needs 4GB RAM)
ollama pull llama3.2

# Or for better quality (needs 8GB RAM)
ollama pull mistral
```

### Step 3: Test Ollama

```bash
ollama run llama3.2 "Write a short title about Python"
```

### Step 4: Start MCP Server (Optional)

```bash
cd discussionspot9/mcp-servers/seo-automation
pip install -r requirements.txt
python main.py
```

---

## 💻 Using Local AI in Your Code

### In C# (.NET):

```csharp
// Inject service
private readonly ILocalAIService _localAI;

// Optimize title
var optimizedTitle = await _localAI.OptimizeTitleAsync("python tutorial");

// Generate meta description
var metaDesc = await _localAI.GenerateMetaDescriptionAsync(postContent, postTitle);

// Extract keywords
var keywords = await _localAI.ExtractKeywordsAsync(postContent, maxKeywords: 10);

// Optimize content
var optimizedContent = await _localAI.OptimizeContentAsync(postContent, postTitle);
```

### In Python:

```python
from local_ai_service import LocalAIService

service = LocalAIService()

# Optimize title
title = service.optimize_title("python tutorial")

# Generate meta description
meta = service.generate_meta_description(content, title)

# Extract keywords
keywords = service.extract_keywords(content, max_keywords=10)
```

---

## 🔧 Integration with Existing Services

### Update SEO Services to Use Local AI:

**Option 1: Replace Paid APIs**
- Update `AISeoService.cs` to use `ILocalAIService` instead of OpenAI
- Update `GoogleSearchSeoService.cs` to use local AI for content optimization

**Option 2: Hybrid Approach**
- Use local AI for basic optimization
- Use paid APIs only for advanced features (if needed)

---

## 📊 Performance Comparison

| Feature | Paid API (OpenAI) | Local AI (Ollama) |
|---------|-------------------|-------------------|
| **Cost** | $0.01-0.10 per request | **FREE** |
| **Speed** | 2-5 seconds | 3-10 seconds |
| **Quality** | Excellent | Very Good |
| **Privacy** | Data sent to API | **100% Local** |
| **Rate Limits** | Yes | **None** |
| **Offline** | No | **Yes** |

---

## 🎯 Recommended Usage

### Use Local AI For:
- ✅ Post title optimization
- ✅ Meta description generation
- ✅ Keyword extraction
- ✅ Content structure improvement
- ✅ Basic SEO suggestions

### Keep Paid APIs For (Optional):
- Advanced content generation
- Complex analysis
- When local AI quality isn't sufficient

---

## ⚙️ Configuration

**appsettings.json:**
```json
{
  "LocalAI": {
    "Provider": "ollama",
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "Model": "llama3.2",
      "Timeout": 60,
      "Enabled": true
    }
  }
}
```

**Change Model:**
- `llama3.2` - Fast, good quality (recommended)
- `mistral` - Better quality, slower
- `llama3.1` - Best quality, slowest
- `tinyllama` - Fastest, lower quality (low RAM)

---

## 🔍 Troubleshooting

### Ollama Not Running:
```bash
# Check if running
ollama list

# Start Ollama
ollama serve
```

### Model Not Found:
```bash
# List available models
ollama list

# Pull model if missing
ollama pull llama3.2
```

### Slow Responses:
- Use smaller model (`llama3.2` instead of `llama3.1`)
- Reduce max tokens
- Use GPU if available

### Out of Memory:
- Use `tinyllama` (smallest model)
- Close other applications
- Reduce batch size

---

## ✅ Next Steps

1. ✅ Install Ollama
2. ✅ Download model (`llama3.2` recommended)
3. ✅ Test with command line
4. ✅ Start MCP server (optional)
5. ✅ Integrate with post creation/editing
6. ✅ Replace paid API calls with local AI

---

## 💡 Tips

- **Start with llama3.2** - Best balance of speed and quality
- **Keep Ollama running** - Faster responses
- **Use GPU if available** - Much faster
- **Batch requests** - Process multiple posts together
- **Cache results** - Don't re-optimize same content

---

## 🎉 Benefits

- ✅ **$0 Cost** - No API fees
- ✅ **Unlimited Usage** - No rate limits
- ✅ **Privacy** - All data stays local
- ✅ **Offline** - Works without internet
- ✅ **Fast** - No network latency
- ✅ **Customizable** - Use any Ollama model

