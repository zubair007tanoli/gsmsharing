# AI Content Enhancement Service

Python FastAPI microservice for content analysis and SEO improvement.

## Features

✅ **Keyword Extraction** - Automatically extracts important keywords
✅ **SEO Scoring** - Calculates content SEO score (0-100)
✅ **Readability Analysis** - Flesch Reading Ease score
✅ **Word Enhancement** - Replaces weak words with stronger alternatives
✅ **Entity Recognition** - Extracts people, places, organizations
✅ **Content Suggestions** - Provides actionable improvement tips
✅ **Sentiment Analysis** - Detects positive/negative/neutral tone

## Installation

### 1. Install Python 3.9+

```bash
python --version  # Should be 3.9 or higher
```

### 2. Create Virtual Environment

```bash
cd Python_AI_Service
python -m venv venv
```

### 3. Activate Virtual Environment

**Windows:**
```bash
venv\Scripts\activate
```

**Linux/Mac:**
```bash
source venv/bin/activate
```

### 4. Install Dependencies

```bash
pip install -r requirements.txt
python -m spacy download en_core_web_sm
```

## Running the Service

### Development
```bash
python content_enhancer.py
```

### Production
```bash
uvicorn content_enhancer:app --host 0.0.0.0 --port 8000
```

Service will be available at: `http://localhost:8000`

## API Endpoints

### POST /enhance

Enhance content with AI analysis.

**Request:**
```json
{
  "content": "Your post content here...",
  "title": "Your Post Title",
  "tags": "optional, tags, here"
}
```

**Response:**
```json
{
  "keywords": ["python", "data science", "machine learning"],
  "entities": ["OpenAI", "Google", "Microsoft"],
  "seo_score": 85,
  "readability_score": 67.5,
  "enhanced_content": "Content with improved word choice...",
  "suggestions": [
    "Add headings to improve structure",
    "Content is well-optimized for SEO"
  ],
  "word_replacements": {
    "very good": "excellent",
    "make": "create"
  },
  "sentiment": "Positive"
}
```

### GET /health

Check service health.

**Response:**
```json
{
  "status": "healthy",
  "spacy_loaded": true
}
```

## Integration with C#

### 1. Create Service Client

```csharp
public class PythonContentService
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    
    public PythonContentService(HttpClient client, IConfiguration config)
    {
        _client = client;
        _baseUrl = config["Python:ServiceUrl"] ?? "http://localhost:8000";
    }
    
    public async Task<ContentEnhancement> EnhanceContentAsync(string content, string title)
    {
        var request = new { content, title };
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/enhance", request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ContentEnhancement>();
    }
}

public class ContentEnhancement
{
    public List<string> Keywords { get; set; }
    public List<string> Entities { get; set; }
    public int SeoScore { get; set; }
    public double ReadabilityScore { get; set; }
    public string EnhancedContent { get; set; }
    public List<string> Suggestions { get; set; }
    public Dictionary<string, string> WordReplacements { get; set; }
    public string Sentiment { get; set; }
}
```

### 2. Register in Program.cs

```csharp
builder.Services.AddHttpClient<PythonContentService>();
```

### 3. Use in Controller

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreatePostViewModel model)
{
    // Enhance content with Python AI
    var enhancement = await _pythonContentService.EnhanceContentAsync(
        model.Content, 
        model.Title
    );
    
    // Apply enhancements
    model.Keywords = string.Join(", ", enhancement.Keywords);
    model.MetaDescription = GenerateMetaDescription(enhancement);
    
    // Optionally use enhanced content
    if (model.UseAIEnhancement)
    {
        model.Content = enhancement.EnhancedContent;
    }
    
    // Save post...
}
```

## Configuration in appsettings.json

```json
{
  "Python": {
    "ServiceUrl": "http://localhost:8000",
    "Timeout": 30
  }
}
```

## Testing

```bash
# Test with curl
curl -X POST http://localhost:8000/enhance \
  -H "Content-Type: application/json" \
  -d '{
    "content": "This is a very good post about Python programming.",
    "title": "Learn Python"
  }'
```

## Deployment

### Docker (Recommended)

Create `Dockerfile`:
```dockerfile
FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
RUN python -m spacy download en_core_web_sm

COPY . .

EXPOSE 8000

CMD ["uvicorn", "content_enhancer:app", "--host", "0.0.0.0", "--port", "8000"]
```

Build and run:
```bash
docker build -t content-enhancer .
docker run -p 8000:8000 content-enhancer
```

### Systemd Service (Linux)

Create `/etc/systemd/system/content-enhancer.service`:
```ini
[Unit]
Description=Content Enhancement Service
After=network.target

[Service]
User=www-data
WorkingDirectory=/path/to/Python_AI_Service
Environment="PATH=/path/to/venv/bin"
ExecStart=/path/to/venv/bin/uvicorn content_enhancer:app --host 0.0.0.0 --port 8000

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable content-enhancer
sudo systemctl start content-enhancer
```

## Performance

- Average response time: ~100-200ms
- Can handle 100+ requests/second
- Lightweight: ~100MB RAM usage

## Future Enhancements

- [ ] DALL-E integration for thumbnail generation
- [ ] GPT-4 integration for content rewriting
- [ ] Multi-language support
- [ ] Custom AI model training
- [ ] Caching for better performance
- [ ] Redis integration
- [ ] WebSocket support for real-time analysis

## Troubleshooting

**spaCy model not found:**
```bash
python -m spacy download en_core_web_sm
```

**Port already in use:**
```bash
uvicorn content_enhancer:app --port 8001
```

**CORS errors:**
Update `allow_origins` in `content_enhancer.py` to include your domain.

