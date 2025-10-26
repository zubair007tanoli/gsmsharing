"""
AI Content Enhancement Service
FastAPI microservice for content analysis and improvement
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Dict, Optional
import spacy
import re
from collections import Counter

# Initialize
app = FastAPI(title="Content Enhancement API")

# Add CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Configure for production
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Load spaCy model
try:
    nlp = spacy.load("en_core_web_sm")
except OSError:
    print("Downloading spaCy model...")
    import subprocess
    subprocess.run(["python", "-m", "spacy", "download", "en_core_web_sm"])
    nlp = spacy.load("en_core_web_sm")

# Models
class ContentRequest(BaseModel):
    content: str
    title: str
    tags: Optional[str] = None

class EnhancementResponse(BaseModel):
    keywords: List[str]
    entities: List[str]
    seo_score: int
    readability_score: float
    enhanced_content: str
    suggestions: List[str]
    word_replacements: Dict[str, str]
    sentiment: str

# SEO Word Replacements
SEO_REPLACEMENTS = {
    # Weak → Strong
    "very good": "excellent",
    "very bad": "terrible",
    "very big": "massive",
    "very small": "tiny",
    "very happy": "delighted",
    "very sad": "devastated",
    
    # Vague → Specific
    "thing": "element",
    "stuff": "material",
    "lots of": "numerous",
    "a lot": "many",
    
    # Passive → Active
    "is done by": "completes",
    "was created by": "created",
    "can be seen": "appears",
    
    # Weak verbs → Strong verbs
    "get": "obtain",
    "make": "create",
    "use": "utilize",
    "do": "perform",
    "help": "assist",
    "give": "provide",
    "take": "acquire",
    "put": "place",
    "show": "demonstrate",
    "find": "discover",
    
    # Filler words (remove)
    "basically": "",
    "actually": "",
    "literally": "",
    "just": "",
    "really": "",
    "very": "",
}

# Stop words for keyword extraction
CUSTOM_STOP_WORDS = {
    'the', 'a', 'an', 'and', 'or', 'but', 'in', 'on', 'at', 'to', 'for',
    'of', 'with', 'by', 'from', 'as', 'is', 'was', 'are', 'were', 'been',
    'be', 'have', 'has', 'had', 'do', 'does', 'did', 'will', 'would',
    'could', 'should', 'may', 'might', 'must', 'can', 'this', 'that',
    'these', 'those', 'i', 'you', 'he', 'she', 'it', 'we', 'they'
}

@app.post("/enhance", response_model=EnhancementResponse)
async def enhance_content(req: ContentRequest):
    """
    Enhance content with AI analysis and improvements
    """
    try:
        # Extract keywords
        keywords = extract_keywords(req.content, req.title)
        
        # Extract named entities
        entities = extract_entities(req.content)
        
        # Calculate SEO score
        seo_score = calculate_seo_score(req.content, req.title)
        
        # Calculate readability
        readability = calculate_readability(req.content)
        
        # Enhance content
        enhanced_content, replacements = enhance_word_choice(req.content)
        
        # Generate suggestions
        suggestions = generate_suggestions(req.content, req.title, seo_score)
        
        # Sentiment analysis
        sentiment = analyze_sentiment(req.content)
        
        return EnhancementResponse(
            keywords=keywords,
            entities=entities,
            seo_score=seo_score,
            readability_score=readability,
            enhanced_content=enhanced_content,
            suggestions=suggestions,
            word_replacements=replacements,
            sentiment=sentiment
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

def extract_keywords(content: str, title: str) -> List[str]:
    """Extract important keywords using NLP"""
    doc = nlp(content)
    title_doc = nlp(title)
    
    # Combine title and content for analysis
    all_text = f"{title} {content}"
    combined_doc = nlp(all_text)
    
    # Extract noun chunks
    noun_chunks = [chunk.text.lower() for chunk in combined_doc.noun_chunks]
    
    # Extract important single words (nouns, verbs, adjectives)
    important_words = [
        token.lemma_.lower() for token in combined_doc
        if (token.pos_ in ['NOUN', 'VERB', 'ADJ'] 
            and not token.is_stop 
            and len(token.text) > 3
            and token.text.lower() not in CUSTOM_STOP_WORDS)
    ]
    
    # Count frequency
    word_freq = Counter(important_words)
    
    # Get top keywords
    top_keywords = [word for word, count in word_freq.most_common(15)]
    
    # Add important noun chunks
    chunk_freq = Counter(noun_chunks)
    top_chunks = [chunk for chunk, count in chunk_freq.most_common(5) if len(chunk.split()) > 1]
    
    # Combine and deduplicate
    keywords = list(dict.fromkeys(top_chunks + top_keywords))[:20]
    
    return keywords

def extract_entities(content: str) -> List[str]:
    """Extract named entities (people, places, organizations)"""
    doc = nlp(content)
    entities = [ent.text for ent in doc.ents if ent.label_ in ['PERSON', 'ORG', 'GPE', 'PRODUCT']]
    return list(set(entities))[:10]

def calculate_seo_score(content: str, title: str) -> int:
    """Calculate SEO score (0-100)"""
    score = 0
    
    # Title length (optimal: 50-60 characters)
    title_len = len(title)
    if 50 <= title_len <= 60:
        score += 15
    elif 40 <= title_len <= 70:
        score += 10
    elif 30 <= title_len <= 80:
        score += 5
    
    # Content length (optimal: 300+ words)
    word_count = len(content.split())
    if word_count >= 1000:
        score += 25
    elif word_count >= 500:
        score += 20
    elif word_count >= 300:
        score += 15
    elif word_count >= 150:
        score += 10
    else:
        score += 5
    
    # Keyword usage (title words in content)
    title_words = set(word.lower() for word in title.split() if len(word) > 3)
    content_lower = content.lower()
    keyword_density = sum(1 for word in title_words if word in content_lower)
    score += min(keyword_density * 3, 15)
    
    # Heading structure (check for markdown headers or HTML)
    if re.search(r'<h[2-4]>|#{2,4}\s', content):
        score += 10
    
    # List usage
    if re.search(r'<[ou]l>|^\s*[\-\*\d]+\.?\s', content, re.MULTILINE):
        score += 10
    
    # Link inclusion
    if re.search(r'https?://|<a\s+href', content):
        score += 10
    
    # Readability
    avg_sentence_length = calculate_avg_sentence_length(content)
    if 15 <= avg_sentence_length <= 25:
        score += 15
    elif 10 <= avg_sentence_length <= 30:
        score += 10
    else:
        score += 5
    
    return min(score, 100)

def calculate_readability(content: str) -> float:
    """Calculate Flesch Reading Ease score"""
    sentences = re.split(r'[.!?]+', content)
    sentences = [s.strip() for s in sentences if s.strip()]
    
    if not sentences:
        return 0.0
    
    words = content.split()
    word_count = len(words)
    sentence_count = len(sentences)
    
    if sentence_count == 0 or word_count == 0:
        return 0.0
    
    # Count syllables (simple approximation)
    syllable_count = sum(count_syllables(word) for word in words)
    
    # Flesch Reading Ease
    # 206.835 - 1.015 * (words/sentences) - 84.6 * (syllables/words)
    avg_words_per_sentence = word_count / sentence_count
    avg_syllables_per_word = syllable_count / word_count
    
    score = 206.835 - (1.015 * avg_words_per_sentence) - (84.6 * avg_syllables_per_word)
    
    return round(max(0, min(100, score)), 2)

def count_syllables(word: str) -> int:
    """Simple syllable counter"""
    word = word.lower()
    vowels = 'aeiouy'
    syllable_count = 0
    previous_was_vowel = False
    
    for char in word:
        is_vowel = char in vowels
        if is_vowel and not previous_was_vowel:
            syllable_count += 1
        previous_was_vowel = is_vowel
    
    # Adjust for silent 'e'
    if word.endswith('e'):
        syllable_count -= 1
    
    # Ensure at least 1 syllable
    if syllable_count == 0:
        syllable_count = 1
    
    return syllable_count

def calculate_avg_sentence_length(content: str) -> float:
    """Calculate average sentence length"""
    sentences = re.split(r'[.!?]+', content)
    sentences = [s.strip() for s in sentences if s.strip()]
    
    if not sentences:
        return 0.0
    
    total_words = sum(len(s.split()) for s in sentences)
    return total_words / len(sentences)

def enhance_word_choice(content: str) -> tuple[str, Dict[str, str]]:
    """Replace weak words with stronger alternatives"""
    enhanced = content
    replacements_made = {}
    
    for weak, strong in SEO_REPLACEMENTS.items():
        if weak in enhanced.lower():
            pattern = re.compile(re.escape(weak), re.IGNORECASE)
            if strong:  # If not empty (i.e., not a filler word)
                enhanced = pattern.sub(strong, enhanced)
            else:  # Remove filler words
                enhanced = pattern.sub('', enhanced)
            replacements_made[weak] = strong if strong else "[removed]"
    
    # Clean up extra spaces
    enhanced = re.sub(r'\s+', ' ', enhanced)
    
    return enhanced, replacements_made

def generate_suggestions(content: str, title: str, seo_score: int) -> List[str]:
    """Generate improvement suggestions"""
    suggestions = []
    
    word_count = len(content.split())
    title_len = len(title)
    
    # Title suggestions
    if title_len < 40:
        suggestions.append("📝 Title is too short. Aim for 50-60 characters for better SEO.")
    elif title_len > 80:
        suggestions.append("📝 Title is too long. Keep it under 70 characters.")
    
    # Content length
    if word_count < 300:
        suggestions.append(f"📄 Content is short ({word_count} words). Aim for 300+ words for better SEO.")
    elif word_count > 2000:
        suggestions.append("📄 Consider breaking this into multiple posts or adding a table of contents.")
    
    # Structure
    if not re.search(r'<h[2-4]>|#{2,4}\s', content):
        suggestions.append("📑 Add headings (H2, H3) to improve structure and readability.")
    
    if not re.search(r'<[ou]l>|^\s*[\-\*\d]+\.?\s', content, re.MULTILINE):
        suggestions.append("📋 Use bullet points or numbered lists for better readability.")
    
    # Links
    link_count = len(re.findall(r'https?://|<a\s+href', content))
    if link_count == 0:
        suggestions.append("🔗 Add relevant links to increase credibility and SEO.")
    elif link_count > 10:
        suggestions.append("🔗 Too many links might look spammy. Keep it under 10.")
    
    # Images
    if not re.search(r'<img|!\[.*\]\(', content):
        suggestions.append("🖼️ Add images to make content more engaging.")
    
    # Readability
    readability = calculate_readability(content)
    if readability < 50:
        suggestions.append("📖 Content is hard to read. Use shorter sentences and simpler words.")
    elif readability > 80:
        suggestions.append("📖 Content might be too simple. Consider adding more depth.")
    
    # Keywords
    title_words = set(word.lower() for word in title.split() if len(word) > 3)
    content_lower = content.lower()
    keyword_usage = sum(1 for word in title_words if content_lower.count(word) > 0)
    
    if keyword_usage < 2:
        suggestions.append("🎯 Use more keywords from your title in the content.")
    
    # Overall
    if seo_score < 50:
        suggestions.append("⚠️ Low SEO score. Focus on length, structure, and keywords.")
    elif seo_score >= 80:
        suggestions.append("✅ Excellent SEO score! Your content is well-optimized.")
    
    return suggestions[:8]  # Limit to top 8 suggestions

def analyze_sentiment(content: str) -> str:
    """Simple sentiment analysis"""
    doc = nlp(content)
    
    # Count positive and negative words (simple approach)
    positive_words = {'good', 'great', 'excellent', 'amazing', 'wonderful', 'fantastic',
                      'love', 'best', 'perfect', 'awesome', 'beautiful', 'happy'}
    negative_words = {'bad', 'terrible', 'awful', 'worst', 'hate', 'horrible', 'poor',
                      'disappointing', 'failed', 'wrong', 'sad', 'angry'}
    
    content_lower = content.lower()
    positive_count = sum(1 for word in positive_words if word in content_lower)
    negative_count = sum(1 for word in negative_words if word in content_lower)
    
    if positive_count > negative_count * 1.5:
        return "Positive"
    elif negative_count > positive_count * 1.5:
        return "Negative"
    else:
        return "Neutral"

@app.get("/")
async def root():
    return {
        "service": "Content Enhancement API",
        "version": "1.0.0",
        "status": "running",
        "endpoints": {
            "/enhance": "POST - Enhance content with AI analysis",
            "/health": "GET - Health check"
        }
    }

@app.get("/health")
async def health():
    return {"status": "healthy", "spacy_loaded": nlp is not None}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)

