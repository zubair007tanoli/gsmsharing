"""
SEO Automation MCP Server with FREE Local AI (Ollama)
No API costs - runs entirely on your machine!
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from datetime import datetime
import sys
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
            result = ai_service.optimize_content(content, title)
            
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
    
    # Log to stdout for process manager
    print("🚀 Starting SEO Automation MCP Server...", flush=True)
    print("📦 Using FREE Local AI (Ollama)", flush=True)
    print(f"🤖 Model: {ai_config.model}", flush=True)
    print("✅ Server running on http://localhost:5001", flush=True)
    print("", flush=True)
    
    try:
        uvicorn.run(app, host="0.0.0.0", port=5001, log_level="info")
    except KeyboardInterrupt:
        print("\n⚠️  Server stopped by user", flush=True)
        sys.exit(0)
    except Exception as e:
        print(f"❌ Server error: {e}", flush=True, file=sys.stderr)
        sys.exit(1)

