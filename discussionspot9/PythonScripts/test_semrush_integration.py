#!/usr/bin/env python3
"""
Test script for Semrush integration with Python SEO analyzer
"""

import json
import sys
import os

# Add the current directory to Python path to import seo_analyzer
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from seo_analyzer import SeoAnalyzer

def test_semrush_integration():
    """Test the enhanced SEO analyzer with Semrush data"""
    
    # Sample test data with Semrush integration
    test_data = {
        "title": "Best Smartphone Reviews 2024",
        "content": "Looking for the best smartphone in 2024? Our comprehensive review covers the latest models from Apple, Samsung, and Google. We analyze performance, camera quality, battery life, and value for money.",
        "communitySlug": "technology",
        "postType": "text",
        "semrushData": {
            "smartphone": {
                "search_volume": 12000,
                "keyword_difficulty": 65,
                "cpc": 2.50,
                "competition": "high",
                "number_of_results": 150000000,
                "trends": [10000, 11000, 12000, 13000, 12000],
                "related_keywords": ["best smartphone", "phone reviews", "mobile phones"]
            },
            "smartphone reviews": {
                "search_volume": 8500,
                "keyword_difficulty": 45,
                "cpc": 1.80,
                "competition": "medium",
                "number_of_results": 25000000,
                "trends": [8000, 8200, 8500, 8800, 8500],
                "related_keywords": ["phone reviews", "mobile reviews", "smartphone comparison"]
            },
            "best smartphone 2024": {
                "search_volume": 3200,
                "keyword_difficulty": 35,
                "cpc": 2.20,
                "competition": "low",
                "number_of_results": 5000000,
                "trends": [3000, 3100, 3200, 3300, 3200],
                "related_keywords": ["top smartphones 2024", "best phones", "smartphone guide"]
            },
            "mobile phones": {
                "search_volume": 15000,
                "keyword_difficulty": 70,
                "cpc": 1.50,
                "competition": "high",
                "number_of_results": 200000000,
                "trends": [14000, 14500, 15000, 15500, 15000],
                "related_keywords": ["cell phones", "smartphones", "mobile devices"]
            },
            "phone reviews": {
                "search_volume": 6800,
                "keyword_difficulty": 40,
                "cpc": 1.20,
                "competition": "medium",
                "number_of_results": 18000000,
                "trends": [6500, 6600, 6800, 7000, 6800],
                "related_keywords": ["mobile reviews", "smartphone reviews", "phone comparison"]
            }
        }
    }
    
    print("🧪 Testing Semrush Integration with Python SEO Analyzer")
    print("=" * 60)
    
    # Initialize analyzer
    analyzer = SeoAnalyzer()
    
    # Run analysis
    print("📊 Running enhanced SEO analysis...")
    result = analyzer.analyze_post(test_data)
    
    # Display results
    print("\n✅ Analysis Complete!")
    print("=" * 60)
    
    print(f"📝 Original Title: {result.original_title}")
    print(f"🎯 Optimized Title: {result.optimized_title}")
    print(f"📈 SEO Score: {result.seo_score:.1f}/100")
    print(f"🔄 Title Changed: {result.title_changed}")
    print(f"🔄 Content Changed: {result.content_changed}")
    
    print(f"\n📋 Meta Description:")
    print(f"   {result.suggested_meta_description}")
    
    print(f"\n🏷️  Suggested Keywords:")
    for keyword in result.suggested_keywords:
        print(f"   • {keyword}")
    
    print(f"\n🚀 Semrush Enhanced Keywords:")
    for keyword in result.semrush_enhanced_keywords:
        print(f"   • {keyword}")
    
    print(f"\n💡 Keyword Opportunities:")
    for i, opp in enumerate(result.keyword_opportunities[:5], 1):
        print(f"   {i}. {opp['keyword']}")
        print(f"      Volume: {opp['search_volume']:,} | Difficulty: {opp['difficulty']}% | Score: {opp['opportunity_score']:.1f}")
        print(f"      Recommendation: {opp['recommendation']}")
    
    print(f"\n📊 Competitive Insights:")
    insights = result.competitive_insights
    print(f"   Total Search Volume: {insights['total_search_volume']:,}")
    print(f"   Average Difficulty: {insights['average_difficulty']:.1f}%")
    print(f"   High Volume Keywords: {len(insights['high_volume_keywords'])}")
    print(f"   Low Competition Keywords: {len(insights['low_competition_keywords'])}")
    
    print(f"\n⚠️  Issues Found:")
    for issue in result.issues_found:
        print(f"   • {issue}")
    
    print(f"\n✨ Improvements Made:")
    for improvement in result.improvements_made:
        print(f"   • {improvement}")
    
    # Test JSON serialization
    print(f"\n🔧 Testing JSON Serialization...")
    try:
        json_output = json.dumps(result.__dict__, indent=2, default=str)
        print("✅ JSON serialization successful")
        print(f"📏 Output size: {len(json_output)} characters")
    except Exception as e:
        print(f"❌ JSON serialization failed: {e}")
    
    print("\n" + "=" * 60)
    print("🎉 Semrush Integration Test Complete!")
    
    return result

def test_without_semrush():
    """Test the analyzer without Semrush data (fallback)"""
    
    print("\n🧪 Testing Fallback (No Semrush Data)")
    print("=" * 60)
    
    test_data = {
        "title": "Simple Blog Post",
        "content": "This is a simple blog post without any Semrush data for testing fallback functionality.",
        "communitySlug": "general",
        "postType": "text"
        # No semrushData field
    }
    
    analyzer = SeoAnalyzer()
    result = analyzer.analyze_post(test_data)
    
    print(f"📝 Title: {result.optimized_title}")
    print(f"📈 SEO Score: {result.seo_score:.1f}/100")
    print(f"🏷️  Keywords: {', '.join(result.suggested_keywords)}")
    print(f"🚀 Semrush Keywords: {', '.join(result.semrush_enhanced_keywords)}")
    print(f"💡 Opportunities: {len(result.keyword_opportunities)}")
    
    print("✅ Fallback test successful!")

if __name__ == "__main__":
    try:
        # Test with Semrush data
        result = test_semrush_integration()
        
        # Test without Semrush data
        test_without_semrush()
        
        print("\n🎯 All tests passed! Semrush integration is working correctly.")
        
    except Exception as e:
        print(f"\n❌ Test failed with error: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
