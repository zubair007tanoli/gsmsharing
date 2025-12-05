"""
SEO Automation MCP Server - SIMPLIFIED VERSION (No Ollama Required)
This version works without Ollama for testing purposes
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from datetime import datetime
import uvicorn

app = FastAPI(title="SEO Automation MCP Server")

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "timestamp": datetime.utcnow().isoformat(),
        "server": "SEO Automation",
        "ai_available": False,
        "model": "none (simplified mode)"
    }


@app.post("/mcp")
async def mcp_endpoint(request: dict):
    """
    MCP Protocol endpoint for SEO automation (simplified)
    """
    method = request.get("method")
    params = request.get("params", {})
    request_id = request.get("id")
    
    try:
        result = None
        
        if method == "analyze_keywords":
            content = params.get("content", "")
            # Simple keyword extraction (mock)
            words = content.lower().split()
            keywords = list(set([w for w in words if len(w) > 5]))[:10]
            result = keywords
            
        elif method == "optimize_content":
            content = params.get("content", "")
            title = params.get("title", "")
            # Return content as-is (mock)
            result = {
                "optimized_content": content,
                "optimized_title": title,
                "improvements": ["Simplified mode - no AI optimization"]
            }
            
        elif method == "get_competitor_insights":
            # Mock competitor insights
            result = {
                "score": 75,
                "suggestions": ["Add more keywords", "Improve title", "Add meta description"],
                "issues": ["Content too short"]
            }
            
        elif method == "generate_meta_description":
            content = params.get("content", "")
            # Simple meta description
            result = content[:150] + "..." if len(content) > 150 else content
            
        elif method == "optimize_title":
            title = params.get("title", "")
            # Return title as-is
            result = title
            
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
                "code": -32603,
                "message": str(e)
            },
            "id": request_id
        }


if __name__ == "__main__":
    print("=" * 60)
    print("🚀 SEO Automation MCP Server - SIMPLIFIED MODE")
    print("=" * 60)
    print("✅ Server starting on http://localhost:5001")
    print("✅ Health check: http://localhost:5001/health")
    print("⚠️  AI features disabled (Ollama not required)")
    print("=" * 60)
    print()
    
    uvicorn.run(app, host="0.0.0.0", port=5001, log_level="info")

