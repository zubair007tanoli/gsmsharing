#!/usr/bin/env python3
"""
RapidAPI Services Integration for SEO Optimization
Integrates: AI Writer, UberSuggest, ChatGPT Vision, Google Search
"""

import json
import sys
import urllib.request
import urllib.parse
from typing import Dict, List, Optional, Any

# RapidAPI Configuration
RAPIDAPI_KEY = "72206e7661msh5e52039ad555765p1c43d9jsn7442bae15ba5"

class RapidAPIServices:
    """Wrapper for RapidAPI services used in SEO optimization"""
    
    @staticmethod
    def ai_writer_revise(text: str) -> Dict[str, Any]:
        """
        Revise and improve text using AI Writer API
        API: https://rapidapi.com/ai-writer1/api/ai-writer1
        """
        try:
            url = f"https://ai-writer1.p.rapidapi.com/revise/?text={urllib.parse.quote(text)}"
            
            request = urllib.request.Request(url)
            request.add_header("x-rapidapi-key", RAPIDAPI_KEY)
            request.add_header("x-rapidapi-host", "ai-writer1.p.rapidapi.com")
            
            with urllib.request.urlopen(request, timeout=30) as response:
                result = json.loads(response.read().decode('utf-8'))
                return {
                    "success": True,
                    "revised_text": result.get("text", text),
                    "original_text": text
                }
        except Exception as e:
            return {
                "success": False,
                "error": str(e),
                "original_text": text
            }
    
    @staticmethod
    def ubersuggest_keywords(keyword: str, country: str = "in", language: str = "en") -> Dict[str, Any]:
        """
        Get keyword research data from UberSuggest API
        API: https://rapidapi.com/ubersuggest-keyword-ideas/api/ubersuggest-keyword-ideas
        """
        try:
            url = f"https://ubersuggest-keyword-ideas.p.rapidapi.com/keyword-research?keyword={urllib.parse.quote(keyword)}&country={country}&language_code={language}&network_type=search-plus-partners"
            
            request = urllib.request.Request(url)
            request.add_header("x-rapidapi-key", RAPIDAPI_KEY)
            request.add_header("x-rapidapi-host", "ubersuggest-keyword-ideas.p.rapidapi.com")
            
            with urllib.request.urlopen(request, timeout=30) as response:
                result = json.loads(response.read().decode('utf-8'))
                return {
                    "success": True,
                    "keyword": keyword,
                    "data": result,
                    "related_keywords": result.get("data", {}).get("related_keywords", []),
                    "search_volume": result.get("data", {}).get("search_volume", 0),
                    "competition": result.get("data", {}).get("competition", "low")
                }
        except Exception as e:
            return {
                "success": False,
                "error": str(e),
                "keyword": keyword
            }
    
    @staticmethod
    def chatgpt_vision_analyze(text: str, image_url: Optional[str] = None) -> Dict[str, Any]:
        """
        Analyze content using ChatGPT Vision API
        API: https://rapidapi.com/chatgpt-vision1/api/chatgpt-vision1
        """
        try:
            url = "https://chatgpt-vision1.p.rapidapi.com/matagvision2"
            
            # Build request payload
            payload = {
                "messages": [
                    {
                        "role": "user",
                        "content": [
                            {"type": "text", "text": text}
                        ]
                    }
                ],
                "web_access": False
            }
            
            # Add image if provided
            if image_url:
                payload["messages"][0]["content"].append({
                    "type": "image",
                    "url": image_url
                })
            
            data = json.dumps(payload).encode('utf-8')
            
            request = urllib.request.Request(url, data=data)
            request.add_header("x-rapidapi-key", RAPIDAPI_KEY)
            request.add_header("x-rapidapi-host", "chatgpt-vision1.p.rapidapi.com")
            request.add_header("Content-Type", "application/json")
            
            with urllib.request.urlopen(request, timeout=60) as response:
                result = json.loads(response.read().decode('utf-8'))
                return {
                    "success": True,
                    "analysis": result.get("choices", [{}])[0].get("message", {}).get("content", ""),
                    "raw_response": result
                }
        except Exception as e:
            return {
                "success": False,
                "error": str(e),
                "text": text
            }
    
    @staticmethod
    def google_search(query: str, limit: int = 10, related_keywords: bool = True) -> Dict[str, Any]:
        """
        Search Google and get results
        API: https://rapidapi.com/google-search74/api/google-search74
        """
        try:
            url = f"https://google-search74.p.rapidapi.com/?query={urllib.parse.quote(query)}&limit={limit}&related_keywords={str(related_keywords).lower()}"
            
            request = urllib.request.Request(url)
            request.add_header("x-rapidapi-key", RAPIDAPI_KEY)
            request.add_header("x-rapidapi-host", "google-search74.p.rapidapi.com")
            
            with urllib.request.urlopen(request, timeout=30) as response:
                result = json.loads(response.read().decode('utf-8'))
                return {
                    "success": True,
                    "query": query,
                    "results": result.get("results", []),
                    "related_keywords": result.get("related_keywords", {}).get("keywords", []) if related_keywords else []
                }
        except Exception as e:
            return {
                "success": False,
                "error": str(e),
                "query": query
            }


def main():
    """Main entry point for command-line usage"""
    if len(sys.argv) < 2:
        print(json.dumps({"error": "Usage: python rapidapi_services.py <service> <args>"}))
        sys.exit(1)
    
    service = sys.argv[1]
    services = RapidAPIServices()
    
    try:
        input_data = json.loads(sys.stdin.read())
        
        if service == "ai_writer":
            text = input_data.get("text", "")
            result = services.ai_writer_revise(text)
            print(json.dumps(result))
        
        elif service == "ubersuggest":
            keyword = input_data.get("keyword", "")
            country = input_data.get("country", "in")
            language = input_data.get("language", "en")
            result = services.ubersuggest_keywords(keyword, country, language)
            print(json.dumps(result))
        
        elif service == "chatgpt_vision":
            text = input_data.get("text", "")
            image_url = input_data.get("image_url")
            result = services.chatgpt_vision_analyze(text, image_url)
            print(json.dumps(result))
        
        elif service == "google_search":
            query = input_data.get("query", "")
            limit = input_data.get("limit", 10)
            related_keywords = input_data.get("related_keywords", True)
            result = services.google_search(query, limit, related_keywords)
            print(json.dumps(result))
        
        else:
            print(json.dumps({"error": f"Unknown service: {service}"}))
            sys.exit(1)
    
    except Exception as e:
        print(json.dumps({"error": str(e)}))
        sys.exit(1)


if __name__ == "__main__":
    main()

