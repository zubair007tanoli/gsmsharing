#!/usr/bin/env python3
"""
AI Content Enhancement Service
Provides AI-powered content enhancement, SEO optimization, and related content discovery
"""

import json
import sys
import os
from typing import Dict, List, Any, Optional
import re
from dataclasses import dataclass

# Add the current directory to Python path for imports
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

try:
    import openai
    from openai import OpenAI
    OPENAI_AVAILABLE = True
except ImportError:
    OPENAI_AVAILABLE = False
    print("Warning: OpenAI not available. Using fallback AI responses.")

@dataclass
class AIResponse:
    success: bool
    data: Optional[Dict[str, Any]] = None
    error_message: Optional[str] = None

class AIContentEnhancer:
    def __init__(self):
        self.client = None
        if OPENAI_AVAILABLE:
            try:
                # Try to get API key from environment or config
                api_key = os.getenv('OPENAI_API_KEY')
                if api_key:
                    self.client = OpenAI(api_key=api_key)
                else:
                    print("Warning: OpenAI API key not found. Using fallback responses.")
            except Exception as e:
                print(f"Warning: Failed to initialize OpenAI client: {e}")
    
    def enhance_content(self, content: str, title: str, post_type: str) -> AIResponse:
        """Enhance post content for better readability and engagement"""
        try:
            if self.client:
                return self._enhance_with_openai(content, title, post_type)
            else:
                return self._enhance_fallback(content, title, post_type)
        except Exception as e:
            return AIResponse(success=False, error_message=str(e))
    
    def optimize_seo(self, content: str, title: str, keywords: List[str]) -> AIResponse:
        """Optimize content for SEO"""
        try:
            if self.client:
                return self._optimize_seo_with_openai(content, title, keywords)
            else:
                return self._optimize_seo_fallback(content, title, keywords)
        except Exception as e:
            return AIResponse(success=False, error_message=str(e))
    
    def find_related(self, content: str, title: str, max_results: int = 5) -> AIResponse:
        """Find related content based on semantic similarity"""
        try:
            if self.client:
                return self._find_related_with_openai(content, title, max_results)
            else:
                return self._find_related_fallback(content, title, max_results)
        except Exception as e:
            return AIResponse(success=False, error_message=str(e))
    
    def analyze_content(self, content: str, title: str) -> AIResponse:
        """Analyze content for sentiment, topics, and quality"""
        try:
            if self.client:
                return self._analyze_with_openai(content, title)
            else:
                return self._analyze_fallback(content, title)
        except Exception as e:
            return AIResponse(success=False, error_message=str(e))
    
    def _enhance_with_openai(self, content: str, title: str, post_type: str) -> AIResponse:
        """Enhance content using OpenAI API"""
        prompt = f"""
        Enhance the following {post_type} post for better readability and engagement:
        
        Title: {title}
        Content: {content}
        
        Please provide:
        1. Enhanced content that is more engaging and readable
        2. Suggestions for improvement
        3. Readability score (1-100)
        4. Engagement score (1-100)
        
        Return as JSON with keys: enhanced_content, suggestions, readability_score, engagement_score
        """
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-3.5-turbo",
                messages=[{"role": "user", "content": prompt}],
                max_tokens=1000,
                temperature=0.7
            )
            
            result = json.loads(response.choices[0].message.content)
            return AIResponse(success=True, data=result)
        except Exception as e:
            return AIResponse(success=False, error_message=str(e))
    
    def _optimize_seo_with_openai(self, content: str, title: str, keywords: List[str]) -> AIResponse:
        """Optimize content for SEO using OpenAI API"""
        prompt = f"""
        Optimize the following content for SEO:
        
        Title: {title}
        Content: {content}
        Keywords: {', '.join(keywords)}
        
        Please provide:
        1. Optimized title
        2. Optimized content
        3. Meta description
        4. Suggested keywords
        5. SEO score (1-100)
        6. Improvement suggestions
        
        Return as JSON with keys: optimized_title, optimized_content, meta_description, suggested_keywords, seo_score, improvements
        """
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-3.5-turbo",
                messages=[{"role": "user", "content": prompt}],
                max_tokens=1000,
                temperature=0.7
            )
            
            result = json.loads(response.choices[0].message.content)
            return AIResponse(success=True, data=result)
        except Exception as e:
            return AIResponse(success=False, error_message=str(e))
    
    def _find_related_with_openai(self, content: str, title: str, max_results: int) -> AIResponse:
        """Find related content using OpenAI API"""
        prompt = f"""
        Find {max_results} related content topics for this post:
        
        Title: {title}
        Content: {content}
        
        For each related topic, provide:
        1. Title
        2. URL (placeholder)
        3. Relevance score (0-1)
        4. Snippet
        5. Content type
        
        Return as JSON with key "related_content" containing an array of objects.
        """
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-3.5-turbo",
                messages=[{"role": "user", "content": prompt}],
                max_tokens=1000,
                temperature=0.7
            )
            
            result = json.loads(response.choices[0].message.content)
            return AIResponse(success=True, data=result)
        except Exception as e:
            return AIResponse(success=False, error_message=str(e))
    
    def _analyze_with_openai(self, content: str, title: str) -> AIResponse:
        """Analyze content using OpenAI API"""
        prompt = f"""
        Analyze the following content:
        
        Title: {title}
        Content: {content}
        
        Provide:
        1. Sentiment (positive, negative, neutral)
        2. Main topics
        3. Keywords
        4. Readability level (beginner, intermediate, advanced)
        5. Word count
        6. Estimated read time (minutes)
        7. Quality score (1-100)
        8. Improvement suggestions
        
        Return as JSON with keys: sentiment, topics, keywords, readability_level, word_count, estimated_read_time, quality_score, suggestions
        """
        
        try:
            response = self.client.chat.completions.create(
                model="gpt-3.5-turbo",
                messages=[{"role": "user", "content": prompt}],
                max_tokens=1000,
                temperature=0.7
            )
            
            result = json.loads(response.choices[0].message.content)
            return AIResponse(success=True, data=result)
        except Exception as e:
            return AIResponse(success=False, error_message=str(e))
    
    # Fallback methods when OpenAI is not available
    def _enhance_fallback(self, content: str, title: str, post_type: str) -> AIResponse:
        """Fallback content enhancement without AI"""
        enhanced_content = self._improve_readability(content)
        suggestions = [
            "Add more specific examples",
            "Use shorter sentences",
            "Include a call-to-action",
            "Add relevant images or media"
        ]
        
        return AIResponse(
            success=True,
            data={
                "enhanced_content": enhanced_content,
                "suggestions": suggestions,
                "readability_score": self._calculate_readability_score(content),
                "engagement_score": self._calculate_engagement_score(content)
            }
        )
    
    def _optimize_seo_fallback(self, content: str, title: str, keywords: List[str]) -> AIResponse:
        """Fallback SEO optimization without AI"""
        optimized_title = self._optimize_title(title, keywords)
        meta_description = content[:150] + "..." if len(content) > 150 else content
        
        return AIResponse(
            success=True,
            data={
                "optimized_title": optimized_title,
                "optimized_content": content,
                "meta_description": meta_description,
                "suggested_keywords": keywords + self._extract_keywords(content),
                "seo_score": self._calculate_seo_score(content, title, keywords),
                "improvements": [
                    "Add more relevant keywords",
                    "Improve meta description",
                    "Add internal links",
                    "Optimize images with alt text"
                ]
            }
        )
    
    def _find_related_fallback(self, content: str, title: str, max_results: int) -> AIResponse:
        """Fallback related content discovery without AI"""
        topics = self._extract_topics(content)
        related_content = []
        
        for i, topic in enumerate(topics[:max_results]):
            related_content.append({
                "title": f"Related: {topic}",
                "url": f"/related/{topic.lower().replace(' ', '-')}",
                "relevance_score": 0.8 - (i * 0.1),
                "snippet": f"Content related to {topic}",
                "content_type": "post"
            })
        
        return AIResponse(
            success=True,
            data={"related_content": related_content}
        )
    
    def _analyze_fallback(self, content: str, title: str) -> AIResponse:
        """Fallback content analysis without AI"""
        word_count = len(content.split())
        estimated_read_time = max(1, word_count // 200)  # Assuming 200 words per minute
        
        return AIResponse(
            success=True,
            data={
                "sentiment": self._analyze_sentiment(content),
                "topics": self._extract_topics(content),
                "keywords": self._extract_keywords(content),
                "readability_level": self._get_readability_level(content),
                "word_count": word_count,
                "estimated_read_time": estimated_read_time,
                "quality_score": self._calculate_quality_score(content),
                "suggestions": [
                    "Add more specific details",
                    "Improve paragraph structure",
                    "Add relevant examples"
                ]
            }
        )
    
    # Helper methods
    def _improve_readability(self, content: str) -> str:
        """Basic readability improvements"""
        # Split long sentences
        sentences = content.split('. ')
        improved_sentences = []
        for sentence in sentences:
            if len(sentence) > 100:
                # Split long sentences
                words = sentence.split()
                mid_point = len(words) // 2
                improved_sentences.append(' '.join(words[:mid_point]) + '.')
                improved_sentences.append(' '.join(words[mid_point:]))
            else:
                improved_sentences.append(sentence)
        return '. '.join(improved_sentences)
    
    def _calculate_readability_score(self, content: str) -> int:
        """Calculate basic readability score"""
        words = content.split()
        sentences = content.split('.')
        avg_words_per_sentence = len(words) / len(sentences) if sentences else 0
        
        # Simple readability calculation
        if avg_words_per_sentence < 15:
            return 85
        elif avg_words_per_sentence < 20:
            return 70
        else:
            return 55
    
    def _calculate_engagement_score(self, content: str) -> int:
        """Calculate engagement score"""
        score = 50  # Base score
        
        # Check for engagement indicators
        if '?' in content:
            score += 10
        if '!' in content:
            score += 5
        if len(content.split()) > 100:
            score += 10
        if any(word in content.lower() for word in ['you', 'your', 'we', 'us']):
            score += 15
        
        return min(100, score)
    
    def _optimize_title(self, title: str, keywords: List[str]) -> str:
        """Basic title optimization"""
        if not keywords:
            return title
        
        # Add primary keyword if not in title
        primary_keyword = keywords[0]
        if primary_keyword.lower() not in title.lower():
            return f"{title} - {primary_keyword}"
        
        return title
    
    def _extract_keywords(self, content: str) -> List[str]:
        """Extract keywords from content"""
        # Simple keyword extraction
        words = re.findall(r'\b[a-zA-Z]{4,}\b', content.lower())
        word_freq = {}
        for word in words:
            word_freq[word] = word_freq.get(word, 0) + 1
        
        # Return most frequent words
        return [word for word, freq in sorted(word_freq.items(), key=lambda x: x[1], reverse=True)[:5]]
    
    def _calculate_seo_score(self, content: str, title: str, keywords: List[str]) -> int:
        """Calculate basic SEO score"""
        score = 0
        
        # Title length check
        if 30 <= len(title) <= 60:
            score += 20
        
        # Content length check
        if len(content) > 300:
            score += 20
        
        # Keyword density check
        content_lower = content.lower()
        for keyword in keywords:
            if keyword.lower() in content_lower:
                score += 10
        
        return min(100, score)
    
    def _extract_topics(self, content: str) -> List[str]:
        """Extract topics from content"""
        # Simple topic extraction based on common words
        words = re.findall(r'\b[a-zA-Z]{5,}\b', content.lower())
        common_topics = ['technology', 'business', 'health', 'education', 'science', 'art', 'sports', 'travel']
        
        topics = []
        for topic in common_topics:
            if topic in content.lower():
                topics.append(topic.title())
        
        return topics[:5]
    
    def _analyze_sentiment(self, content: str) -> str:
        """Basic sentiment analysis"""
        positive_words = ['good', 'great', 'excellent', 'amazing', 'wonderful', 'fantastic', 'love', 'like']
        negative_words = ['bad', 'terrible', 'awful', 'hate', 'dislike', 'horrible', 'worst']
        
        content_lower = content.lower()
        positive_count = sum(1 for word in positive_words if word in content_lower)
        negative_count = sum(1 for word in negative_words if word in content_lower)
        
        if positive_count > negative_count:
            return 'positive'
        elif negative_count > positive_count:
            return 'negative'
        else:
            return 'neutral'
    
    def _get_readability_level(self, content: str) -> str:
        """Determine readability level"""
        words = content.split()
        sentences = content.split('.')
        avg_words_per_sentence = len(words) / len(sentences) if sentences else 0
        
        if avg_words_per_sentence < 15:
            return 'beginner'
        elif avg_words_per_sentence < 20:
            return 'intermediate'
        else:
            return 'advanced'
    
    def _calculate_quality_score(self, content: str) -> int:
        """Calculate content quality score"""
        score = 50  # Base score
        
        # Length check
        if len(content) > 200:
            score += 20
        
        # Structure check
        if '\n\n' in content:  # Has paragraphs
            score += 15
        
        # Engagement check
        if any(char in content for char in ['?', '!']):
            score += 15
        
        return min(100, score)

def main():
    """Main function to handle command line arguments"""
    if len(sys.argv) < 4:
        print(json.dumps({"success": False, "error_message": "Invalid arguments"}))
        sys.exit(1)
    
    endpoint = sys.argv[1]
    request_data = json.loads(sys.argv[2])
    
    enhancer = AIContentEnhancer()
    
    try:
        if endpoint == "enhance_content":
            result = enhancer.enhance_content(
                request_data.get("content", ""),
                request_data.get("title", ""),
                request_data.get("post_type", "text")
            )
        elif endpoint == "optimize_seo":
            result = enhancer.optimize_seo(
                request_data.get("content", ""),
                request_data.get("title", ""),
                request_data.get("keywords", [])
            )
        elif endpoint == "find_related":
            result = enhancer.find_related(
                request_data.get("content", ""),
                request_data.get("title", ""),
                request_data.get("max_results", 5)
            )
        elif endpoint == "analyze_content":
            result = enhancer.analyze_content(
                request_data.get("content", ""),
                request_data.get("title", "")
            )
        else:
            result = AIResponse(success=False, error_message="Unknown endpoint")
        
        # Convert to JSON and print
        response = {
            "success": result.success,
            "data": result.data,
            "error_message": result.error_message
        }
        print(json.dumps(response))
        
    except Exception as e:
        error_response = {
            "success": False,
            "error_message": str(e)
        }
        print(json.dumps(error_response))

if __name__ == "__main__":
    main()
