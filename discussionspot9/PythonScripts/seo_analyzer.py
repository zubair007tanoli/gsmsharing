"""
SEO Analyzer for Post Content
Analyzes and optimizes post content for SEO
"""
import json
import sys
import re
from typing import Dict, List, Any
from dataclasses import dataclass, asdict
import html


@dataclass
class SeoAnalysisResult:
    """Result of SEO analysis"""
    original_title: str
    optimized_title: str
    original_content: str
    optimized_content: str
    suggested_meta_description: str
    suggested_keywords: List[str]
    seo_score: float
    issues_found: List[str]
    improvements_made: List[str]
    title_changed: bool
    content_changed: bool


class SeoAnalyzer:
    """Analyzes and optimizes content for SEO"""
    
    def __init__(self):
        self.min_title_length = 30
        self.max_title_length = 60
        self.min_content_length = 300
        self.max_meta_description_length = 160
        self.target_keyword_density = 0.02  # 2%
        
    def analyze_post(self, data: Dict[str, Any]) -> SeoAnalysisResult:
        """Main analysis function"""
        title = data.get('title', '').strip()
        content = data.get('content', '').strip()
        community_slug = data.get('communitySlug', '')
        post_type = data.get('postType', 'text')
        
        issues = []
        improvements = []
        
        # Analyze and optimize title
        optimized_title, title_issues, title_improvements = self._optimize_title(title, community_slug)
        issues.extend(title_issues)
        improvements.extend(title_improvements)
        
        # Analyze and optimize content
        optimized_content, content_issues, content_improvements = self._optimize_content(
            content, title, post_type
        )
        issues.extend(content_issues)
        improvements.extend(content_improvements)
        
        # Generate meta description
        meta_description = self._generate_meta_description(optimized_content, optimized_title)
        
        # Extract keywords
        keywords = self._extract_keywords(optimized_title, optimized_content)
        
        # Calculate SEO score
        seo_score = self._calculate_seo_score(
            optimized_title, optimized_content, len(issues)
        )
        
        return SeoAnalysisResult(
            original_title=title,
            optimized_title=optimized_title,
            original_content=content,
            optimized_content=optimized_content,
            suggested_meta_description=meta_description,
            suggested_keywords=keywords,
            seo_score=seo_score,
            issues_found=issues,
            improvements_made=improvements,
            title_changed=(title != optimized_title),
            content_changed=(content != optimized_content)
        )
    
    def _optimize_title(self, title: str, community: str) -> tuple:
        """Optimize title for SEO"""
        issues = []
        improvements = []
        optimized = title
        
        if not title:
            issues.append("Title is empty")
            optimized = f"New Post in {community}" if community else "New Discussion Post"
            improvements.append("Generated default title")
        
        # Check length
        if len(title) < self.min_title_length:
            issues.append(f"Title too short (min {self.min_title_length} chars)")
            # Don't auto-lengthen, just flag it
        elif len(title) > self.max_title_length:
            issues.append(f"Title too long (max {self.max_title_length} chars)")
            optimized = title[:self.max_title_length - 3] + "..."
            improvements.append("Truncated title to optimal length")
        
        # Capitalize first letter if needed
        if optimized and not optimized[0].isupper():
            optimized = optimized[0].upper() + optimized[1:]
            improvements.append("Capitalized first letter")
        
        # Remove excessive punctuation
        if optimized.count('!') > 1 or optimized.count('?') > 1:
            optimized = re.sub(r'[!?]{2,}', '!', optimized)
            improvements.append("Removed excessive punctuation")
        
        # Add question mark if title is a question
        question_words = ['how', 'what', 'why', 'when', 'where', 'who', 'which']
        if any(optimized.lower().startswith(word) for word in question_words):
            if not optimized.endswith('?'):
                optimized += '?'
                improvements.append("Added question mark to question-format title")
        
        return optimized, issues, improvements
    
    def _optimize_content(self, content: str, title: str, post_type: str) -> tuple:
        """Optimize content for SEO"""
        issues = []
        improvements = []
        optimized = content
        
        if not content and post_type == 'text':
            issues.append("Content is empty")
            return optimized, issues, improvements
        
        # Remove HTML tags for analysis (but keep them in optimized version)
        text_only = re.sub(r'<[^>]+>', '', content)
        
        # Check content length
        if len(text_only) < self.min_content_length and post_type == 'text':
            issues.append(f"Content too short for SEO (min {self.min_content_length} chars recommended)")
        
        # Add heading if content doesn't have one and is long enough
        if len(text_only) > 200 and not re.search(r'<h[1-6]>', content, re.IGNORECASE):
            if title:
                # Create a subheading from first sentence or first 60 chars
                first_sentence = text_only.split('.')[0][:60]
                optimized = f"<h2>{html.escape(first_sentence)}</h2>\n\n{optimized}"
                improvements.append("Added H2 heading for better structure")
        
        # Add paragraphs if content is just plain text
        if '<p>' not in content and '<br>' not in content and len(text_only) > 100:
            paragraphs = [p.strip() for p in text_only.split('\n\n') if p.strip()]
            if len(paragraphs) > 1:
                optimized = '\n'.join([f"<p>{html.escape(p)}</p>" for p in paragraphs])
                improvements.append("Added paragraph tags for better readability")
        
        # Check for keyword usage (title words in content)
        if title:
            title_words = set(re.findall(r'\b\w{4,}\b', title.lower()))
            content_words = set(re.findall(r'\b\w+\b', text_only.lower()))
            keyword_usage = len(title_words & content_words) / max(len(title_words), 1)
            
            if keyword_usage < 0.5:
                issues.append("Low keyword relevance between title and content")
        
        return optimized, issues, improvements
    
    def _generate_meta_description(self, content: str, title: str) -> str:
        """Generate SEO-friendly meta description"""
        # Remove HTML tags
        text = re.sub(r'<[^>]+>', '', content)
        text = text.strip()
        
        if not text:
            return title[:self.max_meta_description_length]
        
        # Take first sentence or paragraph
        first_sentence = text.split('.')[0]
        
        if len(first_sentence) > self.max_meta_description_length:
            return first_sentence[:self.max_meta_description_length - 3] + "..."
        
        # If first sentence is too short, add more
        if len(first_sentence) < 100 and len(text) > len(first_sentence):
            description = text[:self.max_meta_description_length - 3] + "..."
        else:
            description = first_sentence + "."
        
        return description
    
    def _extract_keywords(self, title: str, content: str) -> List[str]:
        """Extract relevant keywords"""
        # Remove HTML and combine text
        text = title + " " + re.sub(r'<[^>]+>', '', content)
        text = text.lower()
        
        # Extract words (4+ letters)
        words = re.findall(r'\b\w{4,}\b', text)
        
        # Common stop words to exclude
        stop_words = {
            'this', 'that', 'these', 'those', 'what', 'which', 'who', 'when',
            'where', 'why', 'how', 'with', 'from', 'have', 'has', 'had',
            'will', 'would', 'could', 'should', 'can', 'may', 'might',
            'the', 'and', 'or', 'but', 'in', 'on', 'at', 'to', 'for',
            'of', 'as', 'by', 'an', 'be', 'is', 'are', 'was', 'were',
            'been', 'being', 'have', 'has', 'had', 'do', 'does', 'did',
            'just', 'about', 'into', 'through', 'during', 'before', 'after',
            'above', 'below', 'between', 'under', 'again', 'further', 'then',
            'once', 'here', 'there', 'all', 'both', 'each', 'few', 'more',
            'most', 'other', 'some', 'such', 'only', 'own', 'same', 'than',
            'too', 'very', 'can', 'will', 'just', 'don', 'should', 'now'
        }
        
        # Filter and count
        word_freq = {}
        for word in words:
            if word not in stop_words and len(word) >= 4:
                word_freq[word] = word_freq.get(word, 0) + 1
        
        # Get top keywords
        sorted_words = sorted(word_freq.items(), key=lambda x: x[1], reverse=True)
        keywords = [word for word, count in sorted_words[:10]]
        
        return keywords
    
    def _calculate_seo_score(self, title: str, content: str, issues_count: int) -> float:
        """Calculate SEO score (0-100)"""
        score = 100.0
        
        # Title scoring
        if not title:
            score -= 30
        elif len(title) < self.min_title_length:
            score -= 15
        elif len(title) > self.max_title_length:
            score -= 10
        
        # Content scoring
        text = re.sub(r'<[^>]+>', '', content)
        if not text:
            score -= 30
        elif len(text) < self.min_content_length:
            score -= 20
        
        # Structure scoring
        if '<h' not in content.lower() and len(text) > 200:
            score -= 10
        
        if '<p>' not in content.lower() and len(text) > 100:
            score -= 5
        
        # Issues penalty
        score -= (issues_count * 5)
        
        return max(0.0, min(100.0, score))


def main():
    """Main entry point"""
    try:
        # Read input from stdin
        if len(sys.argv) > 1:
            # Input passed as argument
            input_json = sys.argv[1]
        else:
            # Input passed via stdin
            input_json = sys.stdin.read()
        
        # Parse input
        data = json.loads(input_json)
        
        # Analyze
        analyzer = SeoAnalyzer()
        result = analyzer.analyze_post(data)
        
        # Output result as JSON
        output = asdict(result)
        print(json.dumps(output, indent=2, ensure_ascii=False))
        
        return 0
        
    except Exception as e:
        error_result = {
            'error': True,
            'message': str(e),
            'original_title': data.get('title', '') if 'data' in locals() else '',
            'optimized_title': data.get('title', '') if 'data' in locals() else '',
            'original_content': data.get('content', '') if 'data' in locals() else '',
            'optimized_content': data.get('content', '') if 'data' in locals() else '',
            'suggested_meta_description': '',
            'suggested_keywords': [],
            'seo_score': 0.0,
            'issues_found': [f'Python script error: {str(e)}'],
            'improvements_made': [],
            'title_changed': False,
            'content_changed': False
        }
        print(json.dumps(error_result, indent=2))
        return 1


if __name__ == '__main__':
    sys.exit(main())

