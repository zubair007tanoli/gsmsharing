# ⚡ Quick Start: Free Local AI for Post Optimization

## 🎯 Goal
Set up **100% FREE** AI for post optimization in **5 minutes**!

---

## 📦 Step 1: Install Ollama (2 minutes)

### Windows:
1. Go to: https://ollama.com/download
2. Download and run `OllamaSetup.exe`
3. Done! ✅

### Verify:
```powershell
ollama --version
```

---

## 🤖 Step 2: Download AI Model (2 minutes)

```bash
# Recommended: Fast and good quality
ollama pull llama3.2
```

**Alternative models:**
- `ollama pull mistral` - Better quality (needs 8GB RAM)
- `ollama pull tinyllama` - Fastest (works on 2GB RAM)

---

## ✅ Step 3: Test It Works (1 minute)

```bash
ollama run llama3.2 "Optimize this title for SEO: python tutorial"
```

You should see an optimized title! ✅

---

## 🚀 Step 4: Start MCP Server (Optional)

```bash
cd discussionspot9/mcp-servers/seo-automation
pip install -r requirements.txt
python main.py
```

Server runs on: `http://localhost:5001`

---

## 💻 Step 5: Use in Your Code

### In PostController or PostService:

```csharp
// Inject LocalAIService
private readonly ILocalAIService _localAI;

// When creating/editing post:
var optimizedTitle = await _localAI.OptimizeTitleAsync(model.Title);
var metaDescription = await _localAI.GenerateMetaDescriptionAsync(
    model.Content, 
    optimizedTitle
);
var keywords = await _localAI.ExtractKeywordsAsync(model.Content, 10);

// Use optimized values
model.Title = optimizedTitle ?? model.Title;
model.MetaDescription = metaDescription ?? model.MetaDescription;
model.Keywords = string.Join(", ", keywords);
```

---

## 🎉 Done!

You now have:
- ✅ **FREE AI** for post optimization
- ✅ **No API costs**
- ✅ **Unlimited usage**
- ✅ **100% private** (runs locally)

---

## 📊 What You Can Do

1. **Optimize Titles** - Make them SEO-friendly
2. **Generate Meta Descriptions** - Auto-create compelling descriptions
3. **Extract Keywords** - Find relevant keywords automatically
4. **Optimize Content** - Improve structure and SEO
5. **Analyze SEO** - Get scores and suggestions

---

## 💡 Tips

- **Keep Ollama running** - Faster responses
- **Use llama3.2** - Best balance of speed/quality
- **GPU helps** - Much faster if you have NVIDIA/AMD GPU
- **Batch requests** - Process multiple posts together

---

## ❓ Troubleshooting

**Ollama not found?**
- Make sure it's installed and running
- Check: `ollama list`

**Model not found?**
- Run: `ollama pull llama3.2`

**Slow responses?**
- Use smaller model (`llama3.2` instead of `llama3.1`)
- Close other applications
- Use GPU if available

**Out of memory?**
- Use `tinyllama` (smallest model)
- Reduce max tokens in config

---

## 🎯 Next Steps

1. ✅ Test with a few posts
2. ✅ Integrate with post creation form
3. ✅ Add to post editing
4. ✅ Monitor performance
5. ✅ Enjoy FREE AI! 🎉

