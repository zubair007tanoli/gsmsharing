"""
SEO Automation MCP Server with FREE Local AI (Ollama)
No API costs - runs entirely on your machine!
"""

import sys
import io

# Fix Windows console encoding issues
if sys.platform == 'win32':
    # Set UTF-8 encoding for stdout/stderr on Windows
    if hasattr(sys.stdout, 'reconfigure'):
        sys.stdout.reconfigure(encoding='utf-8', errors='replace')
        sys.stderr.reconfigure(encoding='utf-8', errors='replace')
    else:
        # Fallback for older Python versions
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from datetime import datetime
import os
import uvicorn

# Add parent directory to path for imports
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from local_ai_service import LocalAIService, LocalAIConfig

app = FastAPI(title="SEO Automation MCP Server")

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Configure properly for production
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize local AI service
ai_config = LocalAIConfig(
    base_url="http://localhost:11434",
    model="llama3.2"  # Change to mistral or llama3.1 for better quality
)
ai_service = LocalAIService(ai_config)

# Lightweight local cleaners/humanizers to ensure output even if AI is unavailable
NOISE_TOKENS = ["colum", "column", "asdf", "demo", "lorem", "sample"]

def sanitize_content(content: str):
    """Strip stray tags and scaffolding, return cleaned text and issues."""
    if not content:
        return "", []
    issues = []
    cleaned = content
    cleaned = cleaned.replace("</strong>:", ": ").replace("</b>:", ": ")
    cleaned = cleaned.replace("<strong>", "").replace("</strong>", "")
    cleaned = cleaned.replace("<b>", "").replace("</b>", "")
    # Remove Column X scaffolding
    if "column" in cleaned.lower():
        issues.append("Found 'Column X' scaffolding – replace with real headings.")
        cleaned = __import__("re").sub(r'Column\s+\d+\s*:\s*', '', cleaned, flags=__import__("re").IGNORECASE)
    # Remove residual HTML tags
    cleaned = __import__("re").sub(r'<[^>]+>', '', cleaned)
    # Normalize whitespace
    cleaned = " ".join(cleaned.split())
    return cleaned.strip(), issues

def humanize_content(text: str, tone: str = "medium"):
    """Filler removal, contractions, cadence tweaks."""
    if not text:
        return text
    import re
    replacements = [
        (r"in today's fast-paced world", ''),
        (r"in today's digital age", ''),
        (r'\bmoreover\b', 'On top of that,'),
        (r'\badditionally\b', 'Also,'),
        (r'\bfurthermore\b', 'Plus,'),
        (r'\butilize\b', 'use'),
        (r'\bapproximately\b', 'about'),
        (r'\bdue to the fact that\b', 'because'),
        (r'\bat this point in time\b', 'now'),
        (r'\bit is\b', "it's"),
        (r'\bdo not\b', "don't"),
        (r'\bdoes not\b', "doesn't"),
        (r'\bcan not\b', "can't"),
        (r'\bare not\b', "aren't"),
        (r'\bis not\b', "isn't"),
        (r'\bwill not\b', "won't"),
    ]
    for pat, rep in replacements:
        text = re.sub(pat, rep, text, flags=re.IGNORECASE)
    text = re.sub(r'\s{2,}', ' ', text).strip()
    sentences = re.split(r'(?<=[.!?])\s+', text)
    max_len = 95 if tone == "medium" else 70 if tone == "bold" else 120
    softened = []
    for s in sentences:
        if len(s) <= max_len:
            softened.append(s)
            continue
        parts = re.split(r',| and ', s)
        parts = [p.strip() for p in parts if p.strip()]
        softened.append('. '.join(parts))
    joined = '. '.join(softened).replace('..', '.').strip()
    if tone == "bold":
        joined = re.sub(r'\bwe\b', 'I', joined, flags=re.IGNORECASE)
        joined = re.sub(r'\bour\b', 'my', joined, flags=re.IGNORECASE)
        if not re.search(r'hands-on|takeaway|bottom line|my view', joined, flags=re.IGNORECASE):
            joined += ' From my hands-on checks, this still feels like the smart pick.'
    return joined


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    ai_available = ai_service.is_available()
    return {
        "status": "healthy" if ai_available else "degraded",
        "timestamp": datetime.utcnow().isoformat(),
        "server": "SEO Automation",
        "ai_available": ai_available,
        "model": ai_config.model
    }


@app.post("/mcp")
async def mcp_endpoint(request: dict):
    """
    MCP Protocol endpoint for SEO automation
    """
    method = request.get("method")
    params = request.get("params", {})
    request_id = request.get("id")
    
    try:
        result = None
        
        if method == "analyze_keywords":
            content = params.get("content", "")
            max_keywords = params.get("max_keywords", 10)
            result = ai_service.extract_keywords(content, max_keywords)
            
        elif method == "optimize_content":
            content = params.get("content", "")
            title = params.get("title")
            cleaned, clean_issues = sanitize_content(content)

            ai_text = ai_service.optimize_content(cleaned, title) if ai_service.is_available() else None
            source = "ollama" if ai_text else "local_rules"
            optimized = ai_text or cleaned
            humanized = humanize_content(optimized, tone=params.get("tone", "medium"))
            result = {
                "optimized": humanized,
                "issues": clean_issues,
                "source": source
            }
            
        elif method == "get_competitor_insights":
            # Analyze content and provide competitor insights
            content = params.get("content", "")
            title = params.get("title", "")
            seo_analysis = ai_service.analyze_seo(title, content)
            result = {
                "score": seo_analysis.get("score", 0),
                "suggestions": seo_analysis.get("suggestions", []),
                "issues": seo_analysis.get("issues", [])
            }
            
        elif method == "generate_meta_description":
            content = params.get("content", "")
            title = params.get("title")
            result = ai_service.generate_meta_description(content, title)
            
        elif method == "optimize_title":
            title = params.get("title", "")
            context = params.get("context")
            result = ai_service.optimize_title(title, context)
        
        elif method == "audit_content":
            content = params.get("content", "")
            cleaned, clean_issues = sanitize_content(content)
            humanized = humanize_content(cleaned, tone=params.get("tone", "medium"))
            result = {
                "cleaned": cleaned,
                "humanized": humanized,
                "issues": clean_issues,
                "source": "local_rules"
            }
            
        else:
            raise HTTPException(status_code=400, detail=f"Unknown method: {method}")
        
        return {
            "jsonrpc": "2.0",
            "result": result,
            "id": request_id
        }
        
    except Exception as e:
        return {
            "jsonrpc": "2.0",
            "error": {
                "code": -32000,
                "message": str(e)
            },
            "id": request_id
        }


if __name__ == "__main__":
    import sys
    PORT = 5001
    
    # Allow port to be overridden via command line argument
    if len(sys.argv) > 1:
        try:
            PORT = int(sys.argv[1])
        except ValueError:
            print(f"Invalid port argument: {sys.argv[1]}, using default 5001", flush=True)
    
    # Safe print function that handles encoding errors
    def safe_print(text):
        try:
            print(text, flush=True)
        except UnicodeEncodeError:
            # Fallback to ASCII if encoding fails
            print(text.encode('ascii', errors='replace').decode('ascii'), flush=True)
    
    # Log to stdout for process manager
    safe_print("[MCP] Starting SEO Automation MCP Server...")
    safe_print("[INFO] Using FREE Local AI (Ollama)")
    safe_print(f"[MODEL] Model: {ai_config.model}")
    safe_print(f"[OK] Server running on http://localhost:{PORT}")
    safe_print("")
    
    try:
        uvicorn.run(app, host="0.0.0.0", port=PORT, log_level="info")
    except KeyboardInterrupt:
        print("\n⚠️  Server stopped by user", flush=True)
        sys.exit(0)
    except Exception as e:
        print(f"❌ Server error: {e}", flush=True, file=sys.stderr)
        sys.exit(1)

