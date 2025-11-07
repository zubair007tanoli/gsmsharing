#!/usr/bin/env python3
"""
Hybrid SEO Optimizer - Combines multiple APIs for comprehensive optimization
Uses: AI Writer, UberSuggest, ChatGPT Vision, Google Search, Python SEO Analyzer
"""

import json
import sys
import os

# Add current directory to path for imports
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

try:
    from rapidapi_services import RapidAPIServices
except ImportError:
    # Fallback if import fails
    RapidAPIServices = None

try:
    from seo_analyzer import SeoAnalyzer
except ImportError:
    # Fallback if import fails
    SeoAnalyzer = None

class HybridSeoOptimizer:
    """Combines multiple SEO services for comprehensive optimization"""
    
    def __init__(self):
        if RapidAPIServices:
            self.rapidapi = RapidAPIServices()
        else:
            self.rapidapi = None
        
        if SeoAnalyzer:
            self.seo_analyzer = SeoAnalyzer()
        else:
            self.seo_analyzer = None
    
    def optimize(self, title: str, content: str, community_slug: str = "", post_type: str = "text") -> dict:
        """
        Comprehensive SEO optimization using hybrid approach
        """
        results = {
            "original_title": title,
            "original_content": content,
            "optimizations": {},
            "keywords": [],
            "meta_description": "",
            "seo_score": 0.0,
            "improvements": []
        }
        
        try:
            # Step 1: Python SEO Analyzer (Baseline)
            baseline_result = {}
            if self.seo_analyzer:
                try:
                    seo_input = {
                        "title": title,
                        "content": content,
                        "communitySlug": community_slug,
                        "postType": post_type
                    }
                    baseline_result = self.seo_analyzer.analyze_post(seo_input)
                    results["seo_score"] = baseline_result.get("seo_score", 0.0)
                    results["optimizations"]["baseline"] = baseline_result
                except Exception as e:
                    results["optimizations"]["baseline"] = {"success": False, "error": str(e)}
                    results["seo_score"] = 50.0  # Default score
            else:
                results["seo_score"] = 50.0
                results["optimizations"]["baseline"] = {"success": False, "error": "SEO Analyzer not available"}
            
            # Step 2: Google Search - Keyword Research
            if self.rapidapi:
                try:
                    google_result = self.rapidapi.google_search(title, limit=10, related_keywords=True)
                    if google_result.get("success"):
                        results["keywords"].extend([
                            kw.get("keyword", "") for kw in google_result.get("related_keywords", [])[:10]
                        ])
                        results["optimizations"]["google_search"] = {
                            "success": True,
                            "related_keywords_count": len(google_result.get("related_keywords", []))
                        }
                except Exception as e:
                    results["optimizations"]["google_search"] = {"success": False, "error": str(e)}
            else:
                results["optimizations"]["google_search"] = {"success": False, "error": "RapidAPI services not available"}
            
            # Step 3: UberSuggest - Advanced Keyword Research
            if self.rapidapi:
                try:
                    # Extract main keyword from title
                    main_keyword = title.split()[0] if title.split() else title
                    ubersuggest_result = self.rapidapi.ubersuggest_keywords(main_keyword)
                    if ubersuggest_result.get("success"):
                        ubersuggest_keywords = ubersuggest_result.get("related_keywords", [])
                        if isinstance(ubersuggest_keywords, list):
                            results["keywords"].extend([
                                kw.get("keyword", "") if isinstance(kw, dict) else str(kw)
                                for kw in ubersuggest_keywords[:10]
                            ])
                        results["optimizations"]["ubersuggest"] = {
                            "success": True,
                            "search_volume": ubersuggest_result.get("search_volume", 0),
                            "competition": ubersuggest_result.get("competition", "unknown")
                        }
                except Exception as e:
                    results["optimizations"]["ubersuggest"] = {"success": False, "error": str(e)}
            else:
                results["optimizations"]["ubersuggest"] = {"success": False, "error": "RapidAPI services not available"}
            
            # Step 4: AI Writer - Content Revision
            if self.rapidapi:
                try:
                    # Combine title and content for revision
                    text_to_revise = f"{title}\n\n{content}"[:2000]  # Limit to avoid token limits
                    ai_writer_result = self.rapidapi.ai_writer_revise(text_to_revise)
                    if ai_writer_result.get("success"):
                        revised_text = ai_writer_result.get("revised_text", "")
                        # Extract revised title and content
                        lines = revised_text.split("\n", 1)
                        if len(lines) >= 2:
                            results["optimized_title"] = lines[0].strip()
                            results["optimized_content"] = lines[1].strip()
                        else:
                            results["optimized_content"] = revised_text
                        results["optimizations"]["ai_writer"] = {"success": True}
                    else:
                        results["optimized_title"] = baseline_result.get("optimized_title", title) if baseline_result else title
                        results["optimized_content"] = content
                except Exception as e:
                    results["optimizations"]["ai_writer"] = {"success": False, "error": str(e)}
                    results["optimized_title"] = baseline_result.get("optimized_title", title) if baseline_result else title
                    results["optimized_content"] = content
            else:
                results["optimized_title"] = baseline_result.get("optimized_title", title) if baseline_result else title
                results["optimized_content"] = content
                results["optimizations"]["ai_writer"] = {"success": False, "error": "RapidAPI services not available"}
            
            # Step 5: Generate Meta Description
            if results.get("optimized_content"):
                meta_content = results["optimized_content"][:155]
                results["meta_description"] = meta_content + "..." if len(results["optimized_content"]) > 155 else meta_content
            else:
                results["meta_description"] = baseline_result.get("suggested_meta_description", title[:155])
            
            # Step 6: Deduplicate and rank keywords
            unique_keywords = list(dict.fromkeys(results["keywords"]))  # Preserve order, remove duplicates
            results["keywords"] = unique_keywords[:15]  # Top 15 keywords
            
            # Step 7: Calculate final SEO score
            score_boost = 0
            if results["optimizations"].get("google_search", {}).get("success"):
                score_boost += 10
            if results["optimizations"].get("ubersuggest", {}).get("success"):
                score_boost += 10
            if results["optimizations"].get("ai_writer", {}).get("success"):
                score_boost += 15
            
            results["final_seo_score"] = min(100.0, results["seo_score"] + score_boost)
            results["improvements"].extend(baseline_result.get("improvements_made", []))
            results["improvements"].append(f"Applied {len(results['keywords'])} optimized keywords")
            
            results["success"] = True
            
        except Exception as e:
            results["success"] = False
            results["error"] = str(e)
            # Fallback to baseline
            results["optimized_title"] = baseline_result.get("optimized_title", title) if 'baseline_result' in locals() else title
            results["optimized_content"] = content
            results["meta_description"] = title[:155]
        
        return results


def main():
    """Main entry point"""
    try:
        input_data = json.loads(sys.stdin.read())
        
        optimizer = HybridSeoOptimizer()
        
        result = optimizer.optimize(
            title=input_data.get("title", ""),
            content=input_data.get("content", ""),
            community_slug=input_data.get("communitySlug", ""),
            post_type=input_data.get("postType", "text")
        )
        
        print(json.dumps(result, indent=2))
    
    except Exception as e:
        print(json.dumps({
            "success": False,
            "error": str(e)
        }))
        sys.exit(1)


if __name__ == "__main__":
    main()

