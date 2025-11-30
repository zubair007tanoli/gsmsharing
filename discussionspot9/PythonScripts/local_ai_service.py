"""
Free Local AI Service using Ollama
No API costs - runs entirely on your machine!
"""

import requests
import json
import re
from typing import Optional, List, Dict, Any
from dataclasses import dataclass


@dataclass
class LocalAIConfig:
    """Configuration for local AI service"""
    base_url: str = "http://localhost:11434"
    model: str = "llama3.2"  # Recommended: llama3.2, mistral, or llama3.1
    timeout: int = 60


class LocalAIService:
    """Free local AI service using Ollama"""
    
    def __init__(self, config: Optional[LocalAIConfig] = None):
        self.config = config or LocalAIConfig()
        self.session = requests.Session()
        self.session.timeout = self.config.timeout
    
    def is_available(self) -> bool:
        """Check if Ollama is running and model is available"""
        try:
            response = self.session.get(f"{self.config.base_url}/api/tags")
            if response.status_code == 200:
                models = response.json().get("models", [])
                model_names = [m.get("name", "") for m in models]
                return any(self.config.model in name for name in model_names)
            return False
        except Exception:
            return False
    
    def generate(self, prompt: str, max_tokens: int = 500, temperature: float = 0.7) -> Optional[str]:
        """Generate text using Ollama"""
        try:
            payload = {
                "model": self.config.model,
                "prompt": prompt,
                "stream": False,
                "options": {
                    "temperature": temperature,
                    "num_predict": max_tokens
                }
            }
            
            response = self.session.post(
                f"{self.config.base_url}/api/generate",
                json=payload,
                timeout=self.config.timeout
            )
            response.raise_for_status()
            
            result = response.json()
            return result.get("response", "").strip()
        except Exception as e:
            print(f"Error calling Ollama: {e}")
            return None
    
    def optimize_title(self, title: str, context: Optional[str] = None) -> Optional[str]:
        """Optimize post title for SEO"""
        prompt = f"""Optimize this post title for SEO and engagement. Make it compelling, clear, and under 60 characters.

Original title: {title}
{f'Context: {context}' if context else ''}

Return ONLY the optimized title, nothing else."""
        
        return self.generate(prompt, max_tokens=100)
    
    def generate_meta_description(self, content: str, title: Optional[str] = None) -> Optional[str]:
        """Generate SEO meta description"""
        content_preview = content[:500] if len(content) > 500 else content
        prompt = f"""Generate a compelling SEO meta description (150-160 characters) for this content.

Title: {title or 'N/A'}
Content: {content_preview}

Return ONLY the meta description, nothing else. Make it engaging and include key information."""
        
        result = self.generate(prompt, max_tokens=200)
        if result and len(result) > 160:
            return result[:157] + "..."
        return result
    
    def extract_keywords(self, content: str, max_keywords: int = 10) -> List[str]:
        """Extract SEO keywords from content"""
        content_preview = content[:1000] if len(content) > 1000 else content
        prompt = f"""Extract the top {max_keywords} most relevant SEO keywords from this content. Return ONLY a comma-separated list, nothing else.

Content: {content_preview}"""
        
        result = self.generate(prompt, max_tokens=200)
        if not result:
            return []
        
        # Parse comma-separated keywords
        keywords = [k.strip() for k in result.split(',') if k.strip()]
        return keywords[:max_keywords]
    
    def optimize_content(self, content: str, title: Optional[str] = None) -> Optional[str]:
        """Optimize post content for SEO"""
        content_preview = content[:2000] if len(content) > 2000 else content
        prompt = f"""Optimize this post content for SEO and readability. Improve structure, add headings if needed, and ensure it's engaging.

Title: {title or 'N/A'}
Content: {content_preview}

Return the optimized content with proper HTML formatting (use <p>, <h2>, <h3> tags). Keep the same meaning but improve clarity and SEO."""
        
        return self.generate(prompt, max_tokens=1000)
    
    def generate_content(self, topic: str, keywords: Optional[str] = None) -> Optional[str]:
        """Generate SEO-optimized content"""
        prompt = f"""Write a well-structured, SEO-optimized blog post about: {topic}
{f'Include these keywords naturally: {keywords}' if keywords else ''}

Format with HTML tags (<h2>, <h3>, <p>). Make it informative and engaging. Minimum 500 words."""
        
        return self.generate(prompt, max_tokens=1500)
    
    def analyze_seo(self, title: str, content: str) -> Dict[str, Any]:
        """Analyze SEO score and provide suggestions"""
        prompt = f"""Analyze this post for SEO and provide a score (0-100) with specific improvement suggestions.

Title: {title}
Content: {content[:1000]}

Return a JSON object with:
- score: number (0-100)
- issues: array of issues found
- suggestions: array of improvement suggestions
- strengths: array of what's good

Return ONLY valid JSON, nothing else."""
        
        result = self.generate(prompt, max_tokens=500)
        if not result:
            return {"score": 0, "issues": [], "suggestions": [], "strengths": []}
        
        # Try to parse JSON from response
        try:
            # Extract JSON from response (might have extra text)
            json_match = re.search(r'\{.*\}', result, re.DOTALL)
            if json_match:
                return json.loads(json_match.group())
        except:
            pass
        
        # Fallback: return basic analysis
        return {
            "score": 50,
            "issues": ["Could not parse AI response"],
            "suggestions": ["Review content manually"],
            "strengths": []
        }


# Example usage
if __name__ == "__main__":
    service = LocalAIService()
    
    if service.is_available():
        print("✅ Ollama is available!")
        
        # Test title optimization
        title = service.optimize_title("python tutorial")
        print(f"Optimized title: {title}")
        
        # Test meta description
        meta = service.generate_meta_description("This is a tutorial about Python programming...")
        print(f"Meta description: {meta}")
        
        # Test keyword extraction
        keywords = service.extract_keywords("Python is a programming language used for web development...")
        print(f"Keywords: {keywords}")
    else:
        print("❌ Ollama is not available. Make sure it's running and the model is downloaded.")

