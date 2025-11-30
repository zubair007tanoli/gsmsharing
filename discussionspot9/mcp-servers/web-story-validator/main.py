"""
Web Story Validator and Optimizer MCP Server
Validates AMP Web Stories and provides optimization suggestions
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from datetime import datetime
from typing import Optional, List, Dict
import re

app = FastAPI(title="Web Story Validator MCP Server")

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


class ValidationRequest(BaseModel):
    html_content: str
    story_url: Optional[str] = None


class ValidationResponse(BaseModel):
    is_valid: bool
    errors: List[str]
    warnings: List[str]
    suggestions: List[str]


class OptimizationRequest(BaseModel):
    title: str
    description: Optional[str] = None
    content: Optional[str] = None
    keywords: Optional[List[str]] = None


class OptimizationResponse(BaseModel):
    optimized_title: str
    optimized_description: str
    keywords: List[str]
    suggestions: List[str]


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "timestamp": datetime.utcnow().isoformat(),
        "server": "Web Story Validator"
    }


@app.post("/mcp")
async def mcp_endpoint(request: dict):
    """JSON-RPC endpoint for MCP services"""
    try:
        method = request.get("method")
        params = request.get("params", {})
        request_id = request.get("id", "1")

        if method == "validate_story":
            html_content = params.get("html_content", "")
            story_url = params.get("story_url")
            
            result = validate_amp_story(html_content)
            
            return {
                "jsonrpc": "2.0",
                "id": request_id,
                "result": {
                    "is_valid": result["is_valid"],
                    "errors": result["errors"],
                    "warnings": result["warnings"],
                    "suggestions": result["suggestions"]
                }
            }

        elif method == "optimize_story":
            title = params.get("title", "")
            description = params.get("description")
            content = params.get("content")
            keywords = params.get("keywords", [])
            
            result = optimize_story_seo(title, description, content, keywords)
            
            return {
                "jsonrpc": "2.0",
                "id": request_id,
                "result": {
                    "optimized_title": result["optimized_title"],
                    "optimized_description": result["optimized_description"],
                    "keywords": result["keywords"],
                    "suggestions": result["suggestions"]
                }
            }

        else:
            raise HTTPException(status_code=404, detail="Method not found")

    except Exception as e:
        return {
            "jsonrpc": "2.0",
            "id": request.get("id", "1"),
            "error": {
                "code": -32000,
                "message": str(e)
            }
        }


def validate_amp_story(html_content: str) -> Dict:
    """Validate AMP Web Story HTML"""
    errors = []
    warnings = []
    suggestions = []

    # Check for required elements
    if "<amp-story" not in html_content:
        errors.append("Missing required <amp-story> tag")
    
    if "<amp-story-page" not in html_content:
        errors.append("Story must have at least one <amp-story-page>")
    
    # Check for viewport meta tag
    if 'name="viewport"' not in html_content:
        errors.append("Missing required viewport meta tag")
    
    # Check for amp-story-bookend position
    bookend_pos = html_content.find("<amp-story-bookend")
    story_close = html_content.rfind("</amp-story>")
    
    if bookend_pos != -1 and story_close != -1:
        # Check if bookend is before closing tag
        if bookend_pos > story_close:
            errors.append("amp-story-bookend must be the last child of amp-story")
        else:
            # Check if there's content after bookend
            content_after = html_content[bookend_pos:story_close]
            if "</amp-story-bookend>" in content_after:
                after_bookend = content_after.split("</amp-story-bookend>")[1]
                if after_bookend.strip() and "<amp-" in after_bookend:
                    errors.append("amp-story-bookend must be the absolute last child of amp-story")
    
    # Check for amp-story-page-attachment structure
    attachment_pattern = r'<amp-story-page-attachment[^>]*>(.*?)</amp-story-page-attachment>'
    attachments = re.findall(attachment_pattern, html_content, re.DOTALL)
    
    for i, attachment_content in enumerate(attachments):
        # Check if it has proper structure
        if not attachment_content.strip():
            warnings.append(f"amp-story-page-attachment {i+1} is empty")
        elif attachment_content.count("<") > 5:  # Too many nested tags
            warnings.append(f"amp-story-page-attachment {i+1} may have too many nested elements")
    
    # Check for required attributes
    if 'publisher-logo-src' not in html_content:
        errors.append("Missing required publisher-logo-src attribute")
    
    if 'poster-portrait-src' not in html_content:
        errors.append("Missing required poster-portrait-src attribute")
    
    # Check for minimum pages
    page_count = html_content.count("<amp-story-page")
    if page_count < 4:
        warnings.append(f"Story has {page_count} pages. Google recommends at least 4 pages.")
    
    # Check for proper HTML structure
    if html_content.count("<html") != 1:
        errors.append("Document must have exactly one <html> tag")
    
    if html_content.count("<body") != 1:
        errors.append("Document must have exactly one <body> tag")
    
    # Suggestions
    if "amp-analytics" not in html_content:
        suggestions.append("Consider adding amp-analytics for tracking")
    
    if "amp-story-page-outlink" not in html_content and "amp-story-page-attachment" not in html_content:
        suggestions.append("Consider adding call-to-action links to drive engagement")
    
    return {
        "is_valid": len(errors) == 0,
        "errors": errors,
        "warnings": warnings,
        "suggestions": suggestions
    }


def optimize_story_seo(title: str, description: Optional[str] = None, 
                       content: Optional[str] = None, keywords: Optional[List[str]] = None) -> Dict:
    """Optimize story for SEO"""
    optimized_title = title.strip()
    
    # Ensure title is not too long (60 chars recommended)
    if len(optimized_title) > 60:
        optimized_title = optimized_title[:57] + "..."
    
    # Generate description if not provided
    if not description:
        if content:
            description = content[:157] + "..." if len(content) > 160 else content
        else:
            description = f"Read {title} on DiscussionSpot. Join the conversation."
    
    # Ensure description is not too long (160 chars)
    if len(description) > 160:
        description = description[:157] + "..."
    
    # Extract keywords if not provided
    if not keywords:
        keywords = extract_keywords(title + " " + (content or ""))
    
    suggestions = []
    
    if len(title) > 60:
        suggestions.append("Title should be under 60 characters for optimal SEO")
    
    if len(description) < 120:
        suggestions.append("Meta description should be 120-160 characters for best results")
    
    return {
        "optimized_title": optimized_title,
        "optimized_description": description,
        "keywords": keywords[:10],  # Limit to 10 keywords
        "suggestions": suggestions
    }


def extract_keywords(text: str) -> List[str]:
    """Extract keywords from text"""
    # Simple keyword extraction
    words = re.findall(r'\b[a-z]{4,}\b', text.lower())
    
    # Remove common stop words
    stop_words = {'this', 'that', 'with', 'from', 'have', 'been', 'will', 'your', 'they', 'them'}
    keywords = [w for w in words if w not in stop_words]
    
    # Count frequency
    from collections import Counter
    word_freq = Counter(keywords)
    
    # Return top keywords
    return [word for word, _ in word_freq.most_common(10)]


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5004)

