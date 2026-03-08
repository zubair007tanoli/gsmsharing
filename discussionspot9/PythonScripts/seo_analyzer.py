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
    semrush_enhanced_keywords: List[str]
    keyword_opportunities: List[Dict[str, Any]]
    competitive_insights: Dict[str, Any]
    competitor_content: List[Dict[str, Any]]
    content_gaps: List[str]
    authority_signals: List[str]


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
        
        # Support both Semrush (legacy) and Google Search data
        semrush_data = data.get('semrushData', {})
        google_search_data = data.get('googleSearchData', {})
        
        issues = []
        improvements = []
        
        # Analyze and optimize title
        optimized_title, title_issues, title_improvements = self._optimize_title(title, community_slug)
        issues.extend(title_issues)
        improvements.extend(title_improvements)
        
        # Sanitize content before optimization (remove stray tags / scaffolding)
        cleaned_content, sanitize_issues = self._sanitize_content(content)
        issues.extend(sanitize_issues)

        # Analyze and optimize content
        optimized_content, content_issues, content_improvements = self._optimize_content(
            cleaned_content, title, post_type
        )
        issues.extend(content_issues)
        improvements.extend(content_improvements)
        
        # Generate meta description
        meta_description = self._generate_meta_description(optimized_content, optimized_title)
        
        # Extract keywords
        keywords = self._extract_keywords(optimized_title, optimized_content)
        
        # Process keyword data (Semrush or Google Search)
        keyword_data = google_search_data if google_search_data else semrush_data
        semrush_enhanced_keywords, keyword_opportunities, competitive_insights, competitor_content, content_gaps, authority_signals = self._process_keyword_data(
            keywords, keyword_data, optimized_title, optimized_content
        )
        
        # Calculate SEO score with enhancement
        seo_score = self._calculate_enhanced_seo_score(
            optimized_title, optimized_content, len(issues), keyword_data
        )
        
        if content_gaps:
            improvements.append(f"Cover missing search topics: {', '.join(content_gaps[:3])}")
        if authority_signals:
            improvements.append(f"Include authority signals: {', '.join(authority_signals[:3])}")
        
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
            content_changed=(content != optimized_content),
            semrush_enhanced_keywords=semrush_enhanced_keywords,
            keyword_opportunities=keyword_opportunities,
            competitive_insights=competitive_insights,
            competitor_content=competitor_content,
            content_gaps=content_gaps,
            authority_signals=authority_signals
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

        # Humanize cadence and tone (medium = balanced)
        optimized = self._humanize_content(optimized, tone='medium')

        return optimized, issues, improvements

    def _sanitize_content(self, content: str) -> tuple:
        """Clean stray tags, scaffolding labels, and capture issues"""
        if not content:
            return content, []
        issues = []
        cleaned = content
        # Remove strong/b/i/em wrappers before colons
        cleaned = re.sub(r'</?(strong|b|i|em)>\s*:', ': ', cleaned, flags=re.IGNORECASE)
        cleaned = re.sub(r'</?(strong|b|i|em)>', '', cleaned, flags=re.IGNORECASE)
        # Strip Column X scaffolding
        if re.search(r'column\s+\d+', cleaned, flags=re.IGNORECASE):
            issues.append("Found “Column X” scaffolding; convert to real subheadings.")
            cleaned = re.sub(r'column\s+\d+\s*:\s*', '', cleaned, flags=re.IGNORECASE)
        # Normalize final tip heading
        cleaned = re.sub(r'Final Pro-Tip for 20\d{2} Buyers', 'Final tip for buyers', cleaned, flags=re.IGNORECASE)
        return cleaned, issues

    def _humanize_content(self, content: str, tone: str = 'medium') -> str:
        """Humanize cadence and tone; conservative by default"""
        if not content:
            return content
        text = re.sub(r'</?[^>]+>', '', content)
        # Replace filler & add contractions
        replacements = [
            (r"in today's fast-paced world", ''),
            (r"in today's digital age", ''),
            (r'\bmoreover\b', 'On top of that,'),
            (r'\badditionally\b', 'Also,'),
            (r'\bfurthermore\b', 'Plus,'),
            (r'\butilize\b', 'use'),
            (r'\bapproximately\b', 'about'),
            (r'\bdue to the fact that\b', 'because'),
            (r'\bat this point in time\b', 'now'),
            (r'\bit is\b', "it's"),
            (r'\bdo not\b', "don't"),
            (r'\bdoes not\b', "doesn't"),
            (r'\bcan not\b', "can't"),
            (r'\bare not\b', "aren't"),
            (r'\bis not\b', "isn't"),
            (r'\bwill not\b', "won't")
        ]
        for pat, rep in replacements:
            text = re.sub(pat, rep, text, flags=re.IGNORECASE)
        text = re.sub(r'\s{2,}', ' ', text).strip()

        # Cadence
        sentences = re.split(r'(?<=[.!?])\s+', text)
        max_len = 95 if tone == 'medium' else 70 if tone == 'bold' else 120
        softened = []
        for s in sentences:
            s = s.strip()
            if len(s) <= max_len:
                softened.append(s)
                continue
            parts = re.split(r',| and ', s)
            parts = [p.strip() for p in parts if p.strip()]
            softened.append('. '.join(parts))
        joined = '. '.join(softened).replace('..', '.').strip()

        if tone == 'bold':
            joined = re.sub(r'\bwe\b', 'I', joined, flags=re.IGNORECASE)
            joined = re.sub(r'\bour\b', 'my', joined, flags=re.IGNORECASE)
            if not re.search(r'takeaway|bottom line|my view|hands-on', joined, flags=re.IGNORECASE):
                joined += ' From my hands-on checks, this still feels like the smart pick.'

        # Re-wrap into simple paragraphs
        paras = [p.strip() for p in re.split(r'\n{2,}', joined) if p.strip()]
        return '\n\n'.join(paras)
    
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
        text = title + " " + re.sub(r'<[^>]+>', ' ', content)
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

    def _process_keyword_data(self, keywords: List[str], keyword_data: Dict[str, Any], 
                             title: str, content: str) -> tuple:
        """Process keyword data (Google Search or Semrush) to enhance keyword analysis"""
        semrush_enhanced_keywords = []
        keyword_opportunities = []
        competitive_insights = {
            'high_volume_keywords': [],
            'low_competition_keywords': [],
            'trending_keywords': [],
            'average_difficulty': 0.0,
            'total_search_volume': 0
        }
        competitor_content = []
        content_gaps = []
        authority_signals: List[str] = []
        
        if not keyword_data:
            return semrush_enhanced_keywords, keyword_opportunities, competitive_insights, competitor_content, content_gaps, authority_signals
        
        # Check if this is Google Search data or Semrush data
        is_google_search = 'related_keywords' in keyword_data or 'search_results' in keyword_data
        
        if is_google_search:
            return self._process_google_search_data(keywords, keyword_data, title, content)
        
        # Process each keyword with Semrush data (legacy)
        for keyword, data in keyword_data.items():
            if isinstance(data, dict):
                search_volume = data.get('search_volume', 0)
                difficulty = data.get('keyword_difficulty', 0)
                cpc = data.get('cpc', 0)
                
                # Categorize keywords
                if search_volume > 1000:
                    competitive_insights['high_volume_keywords'].append({
                        'keyword': keyword,
                        'volume': search_volume,
                        'difficulty': difficulty
                    })
                
                if difficulty < 50 and search_volume > 100:
                    competitive_insights['low_competition_keywords'].append({
                        'keyword': keyword,
                        'volume': search_volume,
                        'difficulty': difficulty
                    })
                
                # Add to enhanced keywords if it meets criteria
                if search_volume > 100 and difficulty < 70:
                    semrush_enhanced_keywords.append(keyword)
                
                # Create keyword opportunities
                if search_volume > 500 and difficulty < 60:
                    keyword_opportunities.append({
                        'keyword': keyword,
                        'search_volume': search_volume,
                        'difficulty': difficulty,
                        'cpc': cpc,
                        'opportunity_score': self._calculate_opportunity_score(search_volume, difficulty),
                        'recommendation': self._get_keyword_recommendation(search_volume, difficulty)
                    })
                
                competitive_insights['total_search_volume'] += search_volume
        
        # Calculate average difficulty
        if semrush_data:
            difficulties = [data.get('keyword_difficulty', 0) for data in semrush_data.values() 
                          if isinstance(data, dict)]
            if difficulties:
                competitive_insights['average_difficulty'] = sum(difficulties) / len(difficulties)
        
        # Sort by opportunity score
        keyword_opportunities.sort(key=lambda x: x['opportunity_score'], reverse=True)
        
        return semrush_enhanced_keywords, keyword_opportunities, competitive_insights, competitor_content, content_gaps, authority_signals
    
    def _calculate_opportunity_score(self, search_volume: int, difficulty: float) -> float:
        """Calculate opportunity score for a keyword"""
        if search_volume == 0 or difficulty == 0:
            return 0.0
        
        # Higher volume and lower difficulty = better opportunity
        volume_score = min(search_volume / 10000, 1.0)  # Normalize to 0-1
        difficulty_score = 1.0 - (difficulty / 100)  # Invert difficulty (lower is better)
        
        return (volume_score * 0.6 + difficulty_score * 0.4) * 100
    
    def _get_keyword_recommendation(self, search_volume: int, difficulty: float) -> str:
        """Get recommendation for a keyword based on volume and difficulty"""
        if search_volume > 5000 and difficulty < 40:
            return "High Priority - High volume, low competition"
        elif search_volume > 1000 and difficulty < 60:
            return "Medium Priority - Good volume, moderate competition"
        elif search_volume > 500 and difficulty < 50:
            return "Consider - Moderate volume, low competition"
        elif search_volume > 2000 and difficulty > 70:
            return "Challenging - High volume but very competitive"
        else:
            return "Low Priority - Low volume or high competition"
    
    def _process_google_search_data(self, keywords: List[str], google_data: Dict[str, Any],
                                    title: str, content: str) -> tuple:
        """Process Google Search API data for SEO enhancement"""
        enhanced_keywords = []
        keyword_opportunities = []
        competitive_insights = {
            'related_keywords': [],
            'top_competitors': [],
            'common_patterns': [],
            'avg_title_length': 0,
            'avg_description_length': 0
        }
        competitor_content: List[Dict[str, Any]] = []
        aggregated_phrases: List[str] = []
        authority_signals: set[str] = set()
        content_lower = (title + " " + content).lower()
        
        # Extract related keywords from Google Search
        related = google_data.get('related_keywords') or google_data.get('relatedKeywords') or []
        if isinstance(related, list):
            competitive_insights['related_keywords'] = related[:10]
            enhanced_keywords.extend(related[:5])
        
        # Extract competitor data from search results
        competitors = google_data.get('competitors') or google_data.get('search_results') or google_data.get('searchResults') or []
        if not isinstance(competitors, list):
            competitors = []

        if competitors:
            top_domains = []
            title_lengths = []
            desc_lengths = []

            for result in competitors[:5]:
                if not isinstance(result, dict):
                    continue

                url = result.get('url', '')
                summary = result.get('contentSnippet') or result.get('description') or ''
                domain = result.get('domain') or self._extract_domain(url)
                key_phrases = result.get('keyPhrases') or []

                if url:
                    top_domains.append(domain)

                title_text = result.get('title', '')
                if title_text:
                    title_lengths.append(len(title_text))
                if summary:
                    desc_lengths.append(len(summary))

                if key_phrases:
                    aggregated_phrases.extend([phrase for phrase in key_phrases if isinstance(phrase, str)])

                snippet_lower = summary.lower()
                if any(token in snippet_lower for token in ['study', 'research', 'report', 'data']):
                    authority_signals.add("Reference authoritative research/data")
                if any(char.isdigit() for char in snippet_lower):
                    authority_signals.add("Include statistics or numerical evidence")
                if any(year in snippet_lower for year in ['2023', '2024', '2025']):
                    authority_signals.add("Highlight recent updates or timelines")

                competitor_content.append({
                    "domain": domain,
                    "url": url,
                    "title": title_text,
                    "summary": summary[:320] if summary else "",
                    "key_phrases": key_phrases[:10] if isinstance(key_phrases, list) else [],
                    "estimated_domain_authority": result.get('estimatedDomainAuthority', 0),
                    "estimated_url_authority": result.get('estimatedUrlAuthority', 0)
                })

            competitive_insights['top_competitors'] = list(dict.fromkeys(top_domains))
            competitive_insights['avg_title_length'] = sum(title_lengths) / len(title_lengths) if title_lengths else 0
            competitive_insights['avg_description_length'] = sum(desc_lengths) / len(desc_lengths) if desc_lengths else 0
        
        # Create keyword opportunities based on Google data
        for keyword in enhanced_keywords:
            keyword_opportunities.append({
                'keyword': keyword,
                'source': 'google_search',
                'recommendation': 'Related keyword from Google Search',
                'opportunity_score': 75  # Google related keywords are high value
            })

        content_gaps: List[str] = []
        if aggregated_phrases:
            for phrase in aggregated_phrases:
                if isinstance(phrase, str):
                    phrase_lower = phrase.lower()
                    if phrase_lower not in content_lower:
                        content_gaps.append(phrase)
            content_gaps = list(dict.fromkeys(content_gaps))[:10]
            competitive_insights['common_patterns'] = list(dict.fromkeys(aggregated_phrases))[:10]
        
        return enhanced_keywords, keyword_opportunities, competitive_insights, competitor_content, content_gaps, list(authority_signals)
    
    def _extract_domain(self, url: str) -> str:
        """Extract domain from URL"""
        try:
            if '//' in url:
                domain = url.split('//')[1].split('/')[0]
                return domain.replace('www.', '')
            return url
        except:
            return url
    
    def _calculate_enhanced_seo_score(self, title: str, content: str, issues_count: int, 
                                    keyword_data: Dict[str, Any]) -> float:
        """Calculate enhanced SEO score with Semrush data"""
        base_score = self._calculate_seo_score(title, content, issues_count)
        
        if not keyword_data:
            return base_score
        
        # Bonus for having keyword research data
        data_bonus = 5.0
        
        # Check if this is Google Search data
        is_google_search = 'related_keywords' in keyword_data or 'search_results' in keyword_data
        
        if is_google_search:
            # Google Search bonus based on related keywords count
            related_count = len(keyword_data.get('related_keywords', []))
            keyword_bonus = min(related_count * 2, 15.0)
            return base_score + data_bonus + keyword_bonus
        
        # Semrush data processing (legacy)
        high_value_keywords = 0
        total_volume = 0
        
        for keyword, data in keyword_data.items():
            if isinstance(data, dict):
                search_volume = data.get('search_volume', 0)
                difficulty = data.get('keyword_difficulty', 0)
                
                if search_volume > 1000 and difficulty < 60:
                    high_value_keywords += 1
                
                total_volume += search_volume
        
        # Volume bonus (capped at 10 points)
        volume_bonus = min(total_volume / 10000, 10.0)
        
        # High-value keyword bonus
        keyword_bonus = min(high_value_keywords * 2, 10.0)
        
        enhanced_score = base_score + semrush_bonus + volume_bonus + keyword_bonus
        
        return max(0.0, min(100.0, enhanced_score))


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
            'content_changed': False,
            'semrush_enhanced_keywords': [],
            'keyword_opportunities': [],
            'competitive_insights': {},
            'competitor_content': [],
            'content_gaps': [],
            'authority_signals': []
        }
        print(json.dumps(error_result, indent=2))
        return 1


if __name__ == '__main__':
    sys.exit(main())

