# Python SEO Analyzer

This directory contains Python scripts used by the ASP.NET Core application for SEO analysis.

## Files

### seo_analyzer.py
Main SEO analysis script that:
- Analyzes post titles and content
- Optimizes for search engines
- Generates meta descriptions
- Extracts keywords
- Calculates SEO scores

## Requirements

**Python Version:** 3.7 or higher

**Dependencies:** None! Uses only Python standard library.

## Installation

### Windows:
```powershell
# Download from python.org or use winget
winget install Python.Python.3.12

# Verify
python --version
```

### Linux:
```bash
sudo apt-get install python3
python3 --version
```

### Mac:
```bash
brew install python3
python3 --version
```

## Testing

### Test the script directly:

```bash
# Navigate to this directory
cd PythonScripts

# Test with sample input
echo '{"title": "how to code", "content": "This is a short post about coding", "communitySlug": "programming", "postType": "text"}' | python seo_analyzer.py
```

### Expected Output:
```json
{
  "original_title": "how to code",
  "optimized_title": "How to code?",
  "seo_score": 45.0,
  "issues_found": [
    "Title too short (min 30 chars)",
    "Content too short for SEO (min 300 chars recommended)"
  ],
  "improvements_made": [
    "Capitalized first letter",
    "Added question mark to question-format title"
  ],
  "title_changed": true,
  "content_changed": false
}
```

## Configuration

The C# application looks for Python at these locations (in order):
1. Path specified in `appsettings.json` → `Python:ExecutablePath`
2. System PATH → `python` command
3. Falls back gracefully if not found

## Integration

This script is called by `PythonSeoAnalyzerService.cs` which:
1. Serializes post data to JSON
2. Launches Python process
3. Pipes JSON to stdin
4. Reads result from stdout
5. Deserializes JSON result
6. Applies optimizations to post

## Error Handling

The script handles errors gracefully:
- Invalid JSON input → Returns error response
- Missing fields → Uses defaults
- Exceptions → Returns error with original content
- Timeout (30s) → Process killed, error returned

## Security

- ✅ No shell execution
- ✅ Input sanitization with `html.escape()`
- ✅ Process isolation
- ✅ No file system access
- ✅ No network access
- ✅ Read-only operations

## Performance

- **Typical execution:** 100-500ms
- **Memory usage:** <50MB
- **CPU usage:** Low (text processing only)
- **Scalability:** Handles posts up to 100KB

## Maintenance

### Updating SEO Rules:

Edit `seo_analyzer.py` class `SeoAnalyzer`:

```python
def __init__(self):
    self.min_title_length = 30  # Minimum title length
    self.max_title_length = 60  # Maximum title length
    self.min_content_length = 300  # Min content for good SEO
    # Add more rules here
```

### Adding New Features:

1. Add method to `SeoAnalyzer` class
2. Call from `analyze_post()` method
3. Update `SeoAnalysisResult` dataclass if needed
4. Update C# `SeoAnalysisResult` class to match

## Troubleshooting

### Script Not Found:
```bash
# Check file exists
ls -la PythonScripts/seo_analyzer.py  # Linux/Mac
dir PythonScripts\seo_analyzer.py  # Windows
```

### Python Not Found:
```bash
# Find Python location
which python  # Linux/Mac
where python  # Windows

# Add to PATH or use full path in appsettings.json
```

### Permission Denied:
```bash
# Linux/Mac: Make script executable
chmod +x seo_analyzer.py
```

## Future Enhancements

Possible additions:
- [ ] Grammar checking
- [ ] Sentiment analysis
- [ ] Readability scoring (Flesch-Kincaid)
- [ ] Plagiarism detection
- [ ] AI-powered suggestions
- [ ] Multi-language support
- [ ] Image alt-text generation

---

Made with ❤️ for DiscussionSpot9

