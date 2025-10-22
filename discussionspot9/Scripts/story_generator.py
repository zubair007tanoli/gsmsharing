#!/usr/bin/env python3
"""
AI-Powered Web Story Generator
Generates engaging Web Stories from posts using AI
"""

import json
import sys
import os
import asyncio
import aiohttp
from typing import Dict, List, Optional
from dataclasses import dataclass
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

@dataclass
class StorySlide:
    caption: str
    headline: str
    text: str
    duration: int
    slide_type: str
    background_color: str
    text_color: str
    font_size: int
    alignment: str
    order_index: int

@dataclass
class StoryGenerationRequest:
    post_id: int
    title: str
    content: str
    post_type: str
    style: str
    length: str
    keywords: Optional[str] = None
    media_urls: List[str] = None

@dataclass
class StoryGenerationResponse:
    success: bool
    story_id: Optional[int] = None
    slides: List[StorySlide] = None
    error: Optional[str] = None

class AIStoryGenerator:
    def __init__(self, openai_api_key: str = None):
        self.openai_api_key = openai_api_key or os.getenv('OPENAI_API_KEY')
        self.base_url = "https://api.openai.com/v1"
        
    async def generate_story_content(self, request: StoryGenerationRequest) -> StoryGenerationResponse:
        """Generate story content using AI"""
        try:
            if not self.openai_api_key:
                logger.warning("No OpenAI API key provided, using fallback generation")
                return await self._fallback_generation(request)
            
            # Generate story using OpenAI
            story_content = await self._generate_with_openai(request)
            
            # Create slides
            slides = self._create_slides_from_content(story_content, request)
            
            return StoryGenerationResponse(
                success=True,
                slides=slides
            )
            
        except Exception as e:
            logger.error(f"Error generating story: {e}")
            return StoryGenerationResponse(
                success=False,
                error=str(e)
            )
    
    async def _generate_with_openai(self, request: StoryGenerationRequest) -> Dict:
        """Generate story content using OpenAI API"""
        prompt = self._create_prompt(request)
        
        headers = {
            "Authorization": f"Bearer {self.openai_api_key}",
            "Content-Type": "application/json"
        }
        
        data = {
            "model": "gpt-3.5-turbo",
            "messages": [
                {
                    "role": "system",
                    "content": "You are an expert content creator who creates engaging Web Stories. Generate content that is optimized for mobile viewing and SEO."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            "max_tokens": 2000,
            "temperature": 0.7
        }
        
        async with aiohttp.ClientSession() as session:
            async with session.post(
                f"{self.base_url}/chat/completions",
                headers=headers,
                json=data
            ) as response:
                if response.status == 200:
                    result = await response.json()
                    content = result['choices'][0]['message']['content']
                    return json.loads(content)
                else:
                    raise Exception(f"OpenAI API error: {response.status}")
    
    def _create_prompt(self, request: StoryGenerationRequest) -> str:
        """Create a prompt for AI story generation"""
        slide_count = self._get_slide_count(request.length)
        
        prompt = f"""
        Create a Web Story from this post:
        
        Title: {request.title}
        Content: {request.content}
        Post Type: {request.post_type}
        Style: {request.style}
        Keywords: {request.keywords or 'None'}
        
        Generate {slide_count} slides with the following structure:
        1. Title slide with engaging headline
        2. Content slides that tell the story
        3. Call-to-action slide
        
        For each slide, provide:
        - caption: Short description
        - headline: Main headline
        - text: Slide content
        - duration: Seconds to display (3-5)
        - slide_type: title, content, cta, image
        - background_color: Hex color
        - text_color: Hex color
        - font_size: 16-24
        - alignment: left, center, right
        
        Return as JSON with this structure:
        {{
            "slides": [
                {{
                    "caption": "...",
                    "headline": "...",
                    "text": "...",
                    "duration": 3,
                    "slide_type": "title",
                    "background_color": "#007bff",
                    "text_color": "#ffffff",
                    "font_size": 24,
                    "alignment": "center",
                    "order_index": 0
                }}
            ]
        }}
        """
        
        return prompt
    
    async def _fallback_generation(self, request: StoryGenerationRequest) -> StoryGenerationResponse:
        """Fallback generation without AI"""
        slides = []
        
        # Title slide
        slides.append(StorySlide(
            caption=request.title,
            headline=request.title,
            text=request.content[:100] + "..." if len(request.content) > 100 else request.content,
            duration=3,
            slide_type="title",
            background_color="#007bff",
            text_color="#ffffff",
            font_size=24,
            alignment="center",
            order_index=0
        ))
        
        # Content slides
        slide_count = self._get_slide_count(request.length)
        content_slides = self._split_content(request.content, slide_count - 2)
        
        for i, content in enumerate(content_slides):
            slides.append(StorySlide(
                caption=f"Key Point {i + 1}",
                headline=f"Point {i + 1}",
                text=content,
                duration=4,
                slide_type="content",
                background_color="#f8f9fa",
                text_color="#333333",
                font_size=18,
                alignment="left",
                order_index=i + 1
            ))
        
        # Call-to-action slide
        slides.append(StorySlide(
            caption="Join the discussion",
            headline="What do you think?",
            text="Share your thoughts in the comments!",
            duration=3,
            slide_type="cta",
            background_color="#28a745",
            text_color="#ffffff",
            font_size=20,
            alignment="center",
            order_index=len(slides)
        ))
        
        return StoryGenerationResponse(
            success=True,
            slides=slides
        )
    
    def _create_slides_from_content(self, content: Dict, request: StoryGenerationRequest) -> List[StorySlide]:
        """Create slides from AI-generated content"""
        slides = []
        
        for i, slide_data in enumerate(content.get('slides', [])):
            slides.append(StorySlide(
                caption=slide_data.get('caption', ''),
                headline=slide_data.get('headline', ''),
                text=slide_data.get('text', ''),
                duration=slide_data.get('duration', 3),
                slide_type=slide_data.get('slide_type', 'content'),
                background_color=slide_data.get('background_color', '#f8f9fa'),
                text_color=slide_data.get('text_color', '#333333'),
                font_size=slide_data.get('font_size', 18),
                alignment=slide_data.get('alignment', 'left'),
                order_index=slide_data.get('order_index', i)
            ))
        
        return slides
    
    def _get_slide_count(self, length: str) -> int:
        """Get slide count based on length preference"""
        return {
            'short': 5,
            'medium': 10,
            'long': 15
        }.get(length, 10)
    
    def _split_content(self, content: str, max_slides: int) -> List[str]:
        """Split content into slides"""
        if not content:
            return []
        
        sentences = content.split('. ')
        slides = []
        
        for i in range(0, min(len(sentences), max_slides)):
            if i < len(sentences):
                slides.append(sentences[i].strip())
        
        return slides

class StoryGeneratorService:
    def __init__(self):
        self.ai_generator = AIStoryGenerator()
    
    async def process_story_request(self, request_data: Dict) -> StoryGenerationResponse:
        """Process a story generation request"""
        try:
            request = StoryGenerationRequest(
                post_id=request_data['post_id'],
                title=request_data['title'],
                content=request_data['content'],
                post_type=request_data['post_type'],
                style=request_data.get('style', 'informative'),
                length=request_data.get('length', 'medium'),
                keywords=request_data.get('keywords'),
                media_urls=request_data.get('media_urls', [])
            )
            
            return await self.ai_generator.generate_story_content(request)
            
        except Exception as e:
            logger.error(f"Error processing story request: {e}")
            return StoryGenerationResponse(
                success=False,
                error=str(e)
            )

async def main():
    """Main function for command-line usage"""
    if len(sys.argv) < 2:
        print("Usage: python story_generator.py <request_json>")
        sys.exit(1)
    
    try:
        request_data = json.loads(sys.argv[1])
        service = StoryGeneratorService()
        response = await service.process_story_request(request_data)
        
        print(json.dumps({
            'success': response.success,
            'story_id': response.story_id,
            'slides': [
                {
                    'caption': slide.caption,
                    'headline': slide.headline,
                    'text': slide.text,
                    'duration': slide.duration,
                    'slide_type': slide.slide_type,
                    'background_color': slide.background_color,
                    'text_color': slide.text_color,
                    'font_size': slide.font_size,
                    'alignment': slide.alignment,
                    'order_index': slide.order_index
                } for slide in (response.slides or [])
            ],
            'error': response.error
        }))
        
    except Exception as e:
        print(json.dumps({
            'success': False,
            'error': str(e)
        }))

if __name__ == "__main__":
    asyncio.run(main())
