#!/usr/bin/env python3
"""
Story Enhancement Service
Hybrid C# and Python approach for optimal story generation
"""

import json
import sys
import os
import re
from typing import Dict, List, Any, Optional
from dataclasses import dataclass
import requests
from datetime import datetime

@dataclass
class StorySlide:
    """Represents a single slide in a story"""
    order_index: int
    slide_type: str
    headline: str
    text: str
    caption: str
    background_color: str
    text_color: str
    alignment: str
    font_size: str
    duration: int
    media_url: Optional[str] = None
    media_type: Optional[str] = None

@dataclass
class StoryContent:
    """Represents the content to be converted into a story"""
    title: str
    content: str
    post_type: str
    tags: List[str]
    media_urls: List[str]
    community_name: str
    author_name: str

@dataclass
class StoryOptions:
    """Configuration options for story generation"""
    style: str = "informative"
    length: str = "medium"
    use_ai: bool = True
    keywords: Optional[str] = None
    auto_generate: bool = True

class StoryEnhancer:
    """Main class for enhancing story generation with AI capabilities"""
    
    def __init__(self):
        self.styles = {
            "informative": self._create_informative_style,
            "engaging": self._create_engaging_style,
            "visual": self._create_visual_style,
            "dynamic": self._create_dynamic_style,
            "educational": self._create_educational_style,
            "entertaining": self._create_entertaining_style
        }
        
        self.lengths = {
            "short": (3, 5),
            "medium": (6, 10),
            "long": (11, 20)
        }
    
    def enhance_story(self, content: StoryContent, options: StoryOptions) -> List[StorySlide]:
        """Main method to enhance story generation"""
        
        # Analyze content
        content_analysis = self._analyze_content(content)
        
        # Determine optimal settings
        optimal_style = self._determine_optimal_style(content, content_analysis)
        optimal_length = self._determine_optimal_length(content, content_analysis)
        
        # Override with user preferences
        if options.style != "auto":
            optimal_style = options.style
        if options.length != "auto":
            optimal_length = options.length
        
        # Generate slides
        slides = self._generate_slides(content, optimal_style, optimal_length, content_analysis)
        
        # Apply AI enhancements if enabled
        if options.use_ai:
            slides = self._apply_ai_enhancements(slides, content, optimal_style)
        
        return slides
    
    def _analyze_content(self, content: StoryContent) -> Dict[str, Any]:
        """Analyze content to determine optimal story settings"""
        analysis = {
            "word_count": len(content.content.split()),
            "has_media": len(content.media_urls) > 0,
            "media_types": self._get_media_types(content.media_urls),
            "content_type": content.post_type,
            "sentiment": self._analyze_sentiment(content.content),
            "key_topics": self._extract_key_topics(content.content, content.tags),
            "complexity": self._assess_complexity(content.content)
        }
        return analysis
    
    def _get_media_types(self, media_urls: List[str]) -> List[str]:
        """Determine types of media in the content"""
        media_types = []
        for url in media_urls:
            if any(url.lower().endswith(ext) for ext in ['.jpg', '.jpeg', '.png', '.gif', '.webp']):
                media_types.append('image')
            elif any(url.lower().endswith(ext) for ext in ['.mp4', '.webm', '.ogg', '.avi', '.mov']):
                media_types.append('video')
            else:
                media_types.append('unknown')
        return media_types
    
    def _analyze_sentiment(self, text: str) -> str:
        """Simple sentiment analysis"""
        positive_words = ['good', 'great', 'amazing', 'wonderful', 'excellent', 'fantastic', 'love', 'like', 'enjoy']
        negative_words = ['bad', 'terrible', 'awful', 'hate', 'dislike', 'horrible', 'worst', 'disappointed']
        
        text_lower = text.lower()
        positive_count = sum(1 for word in positive_words if word in text_lower)
        negative_count = sum(1 for word in negative_words if word in text_lower)
        
        if positive_count > negative_count:
            return 'positive'
        elif negative_count > positive_count:
            return 'negative'
        else:
            return 'neutral'
    
    def _extract_key_topics(self, content: str, tags: List[str]) -> List[str]:
        """Extract key topics from content and tags"""
        # Simple keyword extraction
        words = re.findall(r'\b[a-zA-Z]{4,}\b', content.lower())
        word_freq = {}
        for word in words:
            if word not in ['this', 'that', 'with', 'from', 'they', 'have', 'been', 'were', 'said', 'each', 'which', 'their', 'time', 'will', 'about', 'there', 'could', 'other', 'after', 'first', 'well', 'also', 'where', 'much', 'some', 'very', 'when', 'come', 'here', 'just', 'like', 'long', 'make', 'many', 'over', 'such', 'take', 'than', 'them', 'these', 'through', 'under', 'water', 'would', 'write', 'your']:
                word_freq[word] = word_freq.get(word, 0) + 1
        
        # Get top words
        top_words = sorted(word_freq.items(), key=lambda x: x[1], reverse=True)[:5]
        topics = [word for word, freq in top_words]
        
        # Add tags
        topics.extend(tags)
        return list(set(topics))
    
    def _assess_complexity(self, content: str) -> str:
        """Assess content complexity"""
        sentences = content.split('.')
        avg_sentence_length = sum(len(s.split()) for s in sentences) / len(sentences) if sentences else 0
        
        if avg_sentence_length > 20:
            return 'high'
        elif avg_sentence_length > 10:
            return 'medium'
        else:
            return 'low'
    
    def _determine_optimal_style(self, content: StoryContent, analysis: Dict[str, Any]) -> str:
        """Determine optimal story style based on content analysis"""
        if content.post_type == 'poll':
            return 'engaging'
        elif content.post_type in ['image', 'video']:
            return 'visual'
        elif analysis['has_media'] and 'video' in analysis['media_types']:
            return 'dynamic'
        elif 'tutorial' in content.title.lower() or 'how' in content.title.lower():
            return 'educational'
        elif analysis['sentiment'] == 'positive' and analysis['complexity'] == 'low':
            return 'entertaining'
        else:
            return 'informative'
    
    def _determine_optimal_length(self, content: StoryContent, analysis: Dict[str, Any]) -> str:
        """Determine optimal story length based on content analysis"""
        word_count = analysis['word_count']
        has_media = analysis['has_media']
        
        if content.post_type == 'poll':
            return 'short'
        elif word_count < 100:
            return 'short'
        elif word_count < 500:
            return 'medium' if has_media else 'short'
        else:
            return 'long' if has_media else 'medium'
    
    def _generate_slides(self, content: StoryContent, style: str, length: str, analysis: Dict[str, Any]) -> List[StorySlide]:
        """Generate slides based on content and style"""
        min_slides, max_slides = self.lengths[length]
        
        # Create style-specific generator
        style_generator = self.styles.get(style, self._create_informative_style)
        slides = style_generator(content, min_slides, max_slides, analysis)
        
        return slides
    
    def _create_informative_style(self, content: StoryContent, min_slides: int, max_slides: int, analysis: Dict[str, Any]) -> List[StorySlide]:
        """Create informative style slides"""
        slides = []
        
        # Title slide
        slides.append(StorySlide(
            order_index=0,
            slide_type="title",
            headline=content.title,
            text=f"By {content.author_name}",
            caption=f"Posted in r/{content.community_name}",
            background_color="#667eea",
            text_color="#ffffff",
            alignment="center",
            font_size="large",
            duration=5000
        ))
        
        # Content slides
        content_sentences = content.content.split('. ')
        slides_needed = min(max_slides - 1, len(content_sentences))
        
        for i in range(slides_needed):
            if i < len(content_sentences):
                slides.append(StorySlide(
                    order_index=i + 1,
                    slide_type="content",
                    headline=f"Key Point {i + 1}",
                    text=content_sentences[i],
                    caption="",
                    background_color="#f8f9fa",
                    text_color="#333333",
                    alignment="left",
                    font_size="medium",
                    duration=4000
                ))
        
        return slides
    
    def _create_engaging_style(self, content: StoryContent, min_slides: int, max_slides: int, analysis: Dict[str, Any]) -> List[StorySlide]:
        """Create engaging style slides"""
        slides = []
        
        # Hook slide
        slides.append(StorySlide(
            order_index=0,
            slide_type="hook",
            headline="Did you know?",
            text=content.title,
            caption="Let's explore this together!",
            background_color="#ff6b6b",
            text_color="#ffffff",
            alignment="center",
            font_size="large",
            duration=3000
        ))
        
        # Content with questions
        content_parts = content.content.split('. ')
        for i, part in enumerate(content_parts[:max_slides-1]):
            slides.append(StorySlide(
                order_index=i + 1,
                slide_type="content",
                headline=f"What do you think?",
                text=part,
                caption="Share your thoughts!",
                background_color="#4ecdc4",
                text_color="#ffffff",
                alignment="center",
                font_size="medium",
                duration=4000
            ))
        
        return slides
    
    def _create_visual_style(self, content: StoryContent, min_slides: int, max_slides: int, analysis: Dict[str, Any]) -> List[StorySlide]:
        """Create visual style slides"""
        slides = []
        
        # Media-focused slides
        if content.media_urls:
            for i, media_url in enumerate(content.media_urls[:max_slides]):
                media_type = 'video' if any(media_url.lower().endswith(ext) for ext in ['.mp4', '.webm', '.ogg']) else 'image'
                slides.append(StorySlide(
                    order_index=i,
                    slide_type="media",
                    headline=content.title if i == 0 else "",
                    text=content.content.split('. ')[i] if i < len(content.content.split('. ')) else "",
                    caption=f"Visual {i + 1}",
                    background_color="#764ba2",
                    text_color="#ffffff",
                    alignment="center",
                    font_size="large",
                    duration=6000,
                    media_url=media_url,
                    media_type=media_type
                ))
        else:
            # Fallback to text with visual styling
            return self._create_informative_style(content, min_slides, max_slides, analysis)
        
        return slides
    
    def _create_dynamic_style(self, content: StoryContent, min_slides: int, max_slides: int, analysis: Dict[str, Any]) -> List[StorySlide]:
        """Create dynamic style slides"""
        slides = []
        
        # High-energy slides with animations
        slides.append(StorySlide(
            order_index=0,
            slide_type="intro",
            headline="🚀 " + content.title,
            text="Get ready for an exciting journey!",
            caption="Let's dive in!",
            background_color="#667eea",
            text_color="#ffffff",
            alignment="center",
            font_size="large",
            duration=2000
        ))
        
        # Fast-paced content
        content_parts = content.content.split('. ')
        for i, part in enumerate(content_parts[:max_slides-1]):
            slides.append(StorySlide(
                order_index=i + 1,
                slide_type="content",
                headline=f"⚡ Point {i + 1}",
                text=part,
                caption="Keep up!",
                background_color="#f093fb" if i % 2 == 0 else "#f5576c",
                text_color="#ffffff",
                alignment="center",
                font_size="medium",
                duration=3000
            ))
        
        return slides
    
    def _create_educational_style(self, content: StoryContent, min_slides: int, max_slides: int, analysis: Dict[str, Any]) -> List[StorySlide]:
        """Create educational style slides"""
        slides = []
        
        # Learning objective
        slides.append(StorySlide(
            order_index=0,
            slide_type="objective",
            headline="Learning Objective",
            text=content.title,
            caption="What you'll learn today",
            background_color="#4ecdc4",
            text_color="#ffffff",
            alignment="center",
            font_size="large",
            duration=4000
        ))
        
        # Step-by-step content
        content_parts = content.content.split('. ')
        for i, part in enumerate(content_parts[:max_slides-1]):
            slides.append(StorySlide(
                order_index=i + 1,
                slide_type="step",
                headline=f"Step {i + 1}",
                text=part,
                caption=f"Key learning point {i + 1}",
                background_color="#44a08d",
                text_color="#ffffff",
                alignment="left",
                font_size="medium",
                duration=5000
            ))
        
        return slides
    
    def _create_entertaining_style(self, content: StoryContent, min_slides: int, max_slides: int, analysis: Dict[str, Any]) -> List[StorySlide]:
        """Create entertaining style slides"""
        slides = []
        
        # Fun intro
        slides.append(StorySlide(
            order_index=0,
            slide_type="intro",
            headline="🎉 " + content.title,
            text="Let's have some fun!",
            caption="Get ready to be entertained!",
            background_color="#ff9a9e",
            text_color="#ffffff",
            alignment="center",
            font_size="large",
            duration=3000
        ))
        
        # Entertaining content
        content_parts = content.content.split('. ')
        for i, part in enumerate(content_parts[:max_slides-1]):
            emoji = ["😄", "🎭", "🎪", "🎨", "🎵", "🎮", "🎯", "🎊"][i % 8]
            slides.append(StorySlide(
                order_index=i + 1,
                slide_type="content",
                headline=f"{emoji} Fun Fact {i + 1}",
                text=part,
                caption="Isn't this interesting?",
                background_color="#a8edea" if i % 2 == 0 else "#fed6e3",
                text_color="#333333",
                alignment="center",
                font_size="medium",
                duration=4000
            ))
        
        return slides
    
    def _apply_ai_enhancements(self, slides: List[StorySlide], content: StoryContent, style: str) -> List[StorySlide]:
        """Apply AI enhancements to slides"""
        enhanced_slides = []
        
        for slide in slides:
            # Enhance headlines with AI suggestions
            slide.headline = self._enhance_headline(slide.headline, content, style)
            
            # Improve text readability
            slide.text = self._improve_text_readability(slide.text)
            
            # Optimize colors for accessibility
            slide.background_color, slide.text_color = self._optimize_colors(slide.background_color, slide.text_color)
            
            # Adjust duration based on content length
            slide.duration = self._optimize_duration(slide.text, slide.duration)
            
            enhanced_slides.append(slide)
        
        return enhanced_slides
    
    def _enhance_headline(self, headline: str, content: StoryContent, style: str) -> str:
        """Enhance headline with AI suggestions"""
        # Simple headline enhancement based on style
        if style == "engaging":
            if not headline.startswith(("What", "How", "Why", "Did you know")):
                headline = f"Did you know: {headline}"
        elif style == "educational":
            if not headline.startswith(("Learn", "Step", "How to")):
                headline = f"Learn: {headline}"
        elif style == "entertaining":
            if not any(emoji in headline for emoji in ["🎉", "😄", "🎭", "🎪"]):
                headline = f"🎉 {headline}"
        
        return headline
    
    def _improve_text_readability(self, text: str) -> str:
        """Improve text readability"""
        # Simple readability improvements
        text = re.sub(r'\s+', ' ', text)  # Remove extra spaces
        text = text.strip()
        
        # Ensure sentences end with periods
        if text and not text.endswith(('.', '!', '?')):
            text += '.'
        
        return text
    
    def _optimize_colors(self, bg_color: str, text_color: str) -> tuple:
        """Optimize colors for accessibility"""
        # Simple color optimization
        if bg_color == "#ffffff" and text_color == "#ffffff":
            text_color = "#333333"
        elif bg_color == "#000000" and text_color == "#000000":
            text_color = "#ffffff"
        
        return bg_color, text_color
    
    def _optimize_duration(self, text: str, current_duration: int) -> int:
        """Optimize slide duration based on text length"""
        word_count = len(text.split())
        # Base duration: 3 seconds + 0.1 seconds per word
        optimal_duration = max(3000, min(8000, 3000 + word_count * 100))
        return optimal_duration

def main():
    """Main function for command-line usage"""
    if len(sys.argv) != 2:
        print("Usage: python story_enhancer.py <input_json>")
        sys.exit(1)
    
    try:
        # Load input data
        with open(sys.argv[1], 'r') as f:
            data = json.load(f)
        
        # Create objects
        content = StoryContent(**data['content'])
        options = StoryOptions(**data['options'])
        
        # Enhance story
        enhancer = StoryEnhancer()
        slides = enhancer.enhance_story(content, options)
        
        # Convert slides to dictionary format
        result = {
            'slides': [
                {
                    'order_index': slide.order_index,
                    'slide_type': slide.slide_type,
                    'headline': slide.headline,
                    'text': slide.text,
                    'caption': slide.caption,
                    'background_color': slide.background_color,
                    'text_color': slide.text_color,
                    'alignment': slide.alignment,
                    'font_size': slide.font_size,
                    'duration': slide.duration,
                    'media_url': slide.media_url,
                    'media_type': slide.media_type
                }
                for slide in slides
            ],
            'metadata': {
                'generated_at': datetime.now().isoformat(),
                'total_slides': len(slides),
                'style': options.style,
                'length': options.length
            }
        }
        
        # Output result
        print(json.dumps(result, indent=2))
        
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
