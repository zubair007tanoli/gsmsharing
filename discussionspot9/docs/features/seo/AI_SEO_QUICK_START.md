# 🚀 AI SEO Bot - Quick Start Guide

## ✅ What's Been Created

1. **AISeoService.cs** - Main AI SEO optimization service
2. **AISeoController.cs** - API endpoints for AI optimization
3. **Service Registration** - Added to `Program.cs`
4. **Implementation Guide** - Comprehensive documentation

---

## 📋 Setup Steps

### Step 1: Get API Keys

Choose one or more AI providers:

#### Option A: OpenAI (Recommended)
1. Go to https://platform.openai.com
2. Sign up / Log in
3. Navigate to API Keys
4. Create a new secret key
5. Copy the key (starts with `sk-`)

#### Option B: Anthropic Claude
1. Go to https://console.anthropic.com
2. Sign up / Log in
3. Navigate to API Keys
4. Create a new key
5. Copy the key (starts with `sk-ant-`)

#### Option C: Google Gemini (Free tier available)
1. Go to https://makersuite.google.com/app/apikey
2. Create API key
3. Copy the key

---

### Step 2: Configure appsettings.json

Add this section to your `appsettings.json`:

```json
{
  "AI": {
    "Provider": "openai",  // Options: "openai", "anthropic", "gemini", "none"
    "OpenAI": {
      "ApiKey": "sk-YOUR_OPENAI_KEY_HERE",
      "Model": "gpt-4-turbo-preview",
      "MaxTokens": 2000,
      "Temperature": 0.7
    },
    "Anthropic": {
      "ApiKey": "sk-ant-YOUR_ANTHROPIC_KEY_HERE",
      "Model": "claude-3-opus-20240229",
      "MaxTokens": 2000
    },
    "GoogleGemini": {
      "ApiKey": "YOUR_GEMINI_KEY_HERE",
      "Model": "gemini-pro"
    }
  }
}
```

**⚠️ Security Note:** For production, use:
- Azure Key Vault
- AWS Secrets Manager
- Environment variables
- User Secrets (for development)

---

### Step 3: Test the Integration

#### Option A: Via API (Recommended)

```bash
# POST request to optimize a post
curl -X POST https://your-domain.com/api/ai-seo/optimize/123 \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

#### Option B: Via Controller Action

Navigate to: `https://your-domain.com/ai-seo/suggest/123`

(Replace `123` with an actual post ID)

---

## 🎯 Usage Examples

### Example 1: Optimize Post via C# Code

```csharp
// In your controller or service
public class PostController : Controller
{
    private readonly AISeoService _aiSeoService;
    
    public PostController(AISeoService aiSeoService)
    {
        _aiSeoService = aiSeoService;
    }
    
    [HttpPost("optimize/{postId}")]
    public async Task<IActionResult> OptimizePost(int postId)
    {
        var result = await _aiSeoService.OptimizePostWithAIAsync(postId);
        
        if (result.Success)
        {
            // Apply optimizations
            // result.OptimizedTitle
            // result.OptimizedContent
            // result.SuggestedMetaDescription
            // result.SuggestedKeywords
            
            return Ok(result);
        }
        
        return BadRequest(result.ErrorMessage);
    }
}
```

### Example 2: Add to Post Creation Flow

```csharp
// In PostController.Create
[HttpPost]
public async Task<IActionResult> Create(CreatePostViewModel model)
{
    // ... existing validation ...
    
    // Optional: Get AI suggestions before saving
    if (Request.Form["getAiSuggestions"] == "true")
    {
        var tempPost = await _postService.CreatePostAsync(model);
        var aiResult = await _aiSeoService.OptimizePostWithAIAsync(tempPost.PostId);
        
        // Return suggestions to user
        ViewBag.AISuggestions = aiResult;
        return View("Create", model);
    }
    
    // ... rest of creation logic ...
}
```

### Example 3: Background Optimization

```csharp
// In a background service or queue
public class BackgroundAIOptimizationService
{
    private readonly AISeoService _aiSeoService;
    
    public async Task OptimizePostInBackground(int postId)
    {
        var result = await _aiSeoService.OptimizePostWithAIAsync(postId);
        
        if (result.Success && result.EstimatedScore > result.BaselineScore + 10)
        {
            // Auto-apply if significant improvement
            await ApplyOptimizations(postId, result);
        }
    }
}
```

---

## 🔧 Customization

### Change AI Model

Edit `appsettings.json`:

```json
{
  "AI": {
    "OpenAI": {
      "Model": "gpt-3.5-turbo"  // Cheaper, faster
      // or "gpt-4" for better quality
    }
  }
}
```

### Adjust Temperature (Creativity)

```json
{
  "AI": {
    "OpenAI": {
      "Temperature": 0.3  // Lower = more focused
      // 0.7 = balanced
      // 1.0 = more creative
    }
  }
}
```

### Customize Prompts

Edit `AISeoService.cs` → `BuildOptimizationPrompt()` method to customize how AI optimizes content.

---

## 📊 Expected Results

After optimization, you should see:

- **SEO Score Improvement:** 10-30 points increase
- **Better Title:** More keyword-rich, compelling
- **Enhanced Content:** Better structure, headings, readability
- **Meta Description:** Optimized 150-160 character description
- **Keyword Suggestions:** Relevant, high-value keywords

---

## 🐛 Troubleshooting

### Issue: "AI provider not configured"
**Solution:** Check `appsettings.json` has correct API keys and `Provider` setting.

### Issue: "No response from OpenAI"
**Solution:** 
- Verify API key is valid
- Check account has credits
- Verify network connectivity
- Check rate limits

### Issue: "Timeout error"
**Solution:**
- Increase timeout in `AISeoService.cs`
- Use faster model (gpt-3.5-turbo instead of gpt-4)
- Reduce `MaxTokens` in config

### Issue: "API rate limit exceeded"
**Solution:**
- Implement request queuing
- Add caching for similar content
- Use batch processing
- Upgrade API plan

---

## 💰 Cost Management

### Monitor Usage

```csharp
// Add logging to track API calls
_logger.LogInformation("AI optimization called. PostId: {PostId}, Provider: {Provider}", 
    postId, _aiProvider);
```

### Implement Caching

```csharp
// Cache results for similar content
var cacheKey = $"ai_seo_{postId}_{titleHash}";
var cached = await _cache.GetAsync<AISeoOptimizationResult>(cacheKey);
if (cached != null) return cached;
```

### Rate Limiting

```csharp
// Limit AI calls per user/day
var dailyLimit = 10;
var todayCount = await GetAICallCount(userId, DateTime.Today);
if (todayCount >= dailyLimit)
{
    return BadRequest("Daily AI optimization limit reached");
}
```

---

## 🚀 Next Steps

1. ✅ **Test with real posts** - Try optimizing existing content
2. ✅ **Compare results** - See improvements vs baseline
3. ✅ **Add UI** - Create frontend for AI suggestions
4. ✅ **Track performance** - Monitor SEO score improvements
5. ✅ **Fine-tune prompts** - Customize for your content type

---

## 📚 Additional Resources

- [Full Implementation Guide](./AI_SEO_BOT_IMPLEMENTATION_GUIDE.md)
- [OpenAI API Docs](https://platform.openai.com/docs)
- [Anthropic Claude Docs](https://docs.anthropic.com)
- [Google Gemini Docs](https://ai.google.dev/docs)

---

**Ready to optimize!** 🎉

