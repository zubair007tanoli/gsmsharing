using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    /// <summary>
    /// Enhanced SEO service with click-worthy meta descriptions and 3-tier keywords
    /// </summary>
    public class EnhancedSeoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnhancedSeoService> _logger;
        private readonly GoogleKeywordPlannerService _keywordService;

        // Power words for click-worthy descriptions
        private static readonly string[] PowerWords = new[]
        {
            "Ultimate", "Essential", "Proven", "Amazing", "Complete", "Comprehensive",
            "Expert", "Advanced", "Beginner-Friendly", "Step-by-Step", "Actionable",
            "Powerful", "Effective", "Revolutionary", "Game-Changing", "Must-Know",
            "Secret", "Hidden", "Insider", "Professional", "Master", "Discover",
            "Unlock", "Transform", "Boost", "Skyrocket"
        };

        // Emotional triggers
        private static readonly string[] EmotionalTriggers = new[]
        {
            "You Need", "Don't Miss", "Before It's Too Late", "Shocking Truth",
            "What Nobody Tells You", "Finally Revealed", "The Real Story",
            "Everything You Need to Know", "Why You Should Care", "Stop Making This Mistake"
        };

        public EnhancedSeoService(
            ApplicationDbContext context,
            ILogger<EnhancedSeoService> logger,
            GoogleKeywordPlannerService keywordService)
        {
            _context = context;
            _logger = logger;
            _keywordService = keywordService;
        }

        /// <summary>
        /// Generate enhanced SEO metadata for a post
        /// </summary>
        public async Task<EnhancedSeoResult> GenerateEnhancedSeoAsync(CreatePostViewModel model)
        {
            try
            {
                _logger.LogInformation("🎯 Generating enhanced SEO for: {Title}", model.Title);

                // 1. Get keyword research data
                var keywords = await _keywordService.GetKeywordIdeasAsync(model.Title);
                var classification = _keywordService.ClassifyKeywords(keywords, model.Title);

                // 2. Generate click-worthy meta description
                var metaDescription = GenerateClickWorthyMetaDescription(
                    model.Title,
                    model.Content ?? string.Empty,
                    classification.Primary.FirstOrDefault()?.Keyword ?? model.Title
                );

                // 3. Calculate SEO metrics
                var readabilityScore = CalculateReadabilityScore(model.Content ?? string.Empty);
                var keywordDensity = CalculateKeywordDensity(
                    model.Content ?? string.Empty,
                    classification.Primary.Select(k => k.Keyword).ToList()
                );

                // 4. Generate SERP preview
                var serpPreview = GenerateSerpPreview(model.Title, metaDescription);

                // 5. Calculate overall SEO score
                var seoScore = CalculateSeoScore(
                    model.Title,
                    model.Content ?? string.Empty,
                    metaDescription,
                    classification,
                    readabilityScore,
                    keywordDensity
                );

                var result = new EnhancedSeoResult
                {
                    OptimizedMetaDescription = metaDescription,
                    PrimaryKeywords = classification.Primary,
                    SecondaryKeywords = classification.Secondary,
                    LsiKeywords = classification.LSI,
                    TotalSearchVolume = classification.TotalSearchVolume,
                    ReadabilityScore = readabilityScore,
                    KeywordDensity = keywordDensity,
                    SeoScore = seoScore,
                    SerpPreview = serpPreview,
                    PowerWordsUsed = ExtractPowerWords(metaDescription),
                    EmotionalTriggersUsed = ExtractEmotionalTriggers(metaDescription),
                    PredictedCtrImprovement = CalculatePredictedCtrImprovement(metaDescription, seoScore)
                };

                _logger.LogInformation("✅ Enhanced SEO generated. Score: {Score}, Keywords: {Count}, Search Volume: {Volume}",
                    seoScore, classification.Primary.Count + classification.Secondary.Count + classification.LSI.Count,
                    classification.TotalSearchVolume);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to generate enhanced SEO");
                return new EnhancedSeoResult { SeoScore = 0 };
            }
        }

        /// <summary>
        /// Generate click-worthy meta description with power words
        /// </summary>
        private string GenerateClickWorthyMetaDescription(string title, string content, string primaryKeyword)
        {
            var random = new Random();
            var powerWord = PowerWords[random.Next(PowerWords.Length)];
            
            // Extract first meaningful sentence from content
            var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var firstSentence = sentences.FirstOrDefault()?.Trim() ?? title;
            if (firstSentence.Length > 80) firstSentence = firstSentence.Substring(0, 80).Trim();

            // Template variations for click-worthy descriptions
            var templates = new[]
            {
                $"{powerWord} guide to {primaryKeyword}: {firstSentence}. Everything you need to know in 2025.",
                $"Discover the {powerWord.ToLower()} secrets of {primaryKeyword}. {firstSentence} | Expert insights & proven strategies.",
                $"{firstSentence} - Learn the {powerWord.ToLower()} techniques for {primaryKeyword} that experts don't want you to know.",
                $"{powerWord} {primaryKeyword} guide: {firstSentence}. Master it faster with our step-by-step approach.",
                $"Want to master {primaryKeyword}? {firstSentence} Discover {powerWord.ToLower()} tips & actionable advice here."
            };

            var description = templates[random.Next(templates.Length)];

            // Ensure it's within 155-160 characters (optimal for SERP)
            if (description.Length > 160)
            {
                description = description.Substring(0, 157) + "...";
            }

            return description;
        }

        /// <summary>
        /// Calculate readability score (Flesch-Kincaid)
        /// </summary>
        private decimal CalculateReadabilityScore(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return 0;

            var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var syllables = words.Sum(w => CountSyllables(w));

            if (sentences.Length == 0 || words.Length == 0) return 0;

            // Flesch Reading Ease: 206.835 - 1.015 * (words/sentences) - 84.6 * (syllables/words)
            var avgWordsPerSentence = (decimal)words.Length / sentences.Length;
            var avgSyllablesPerWord = (decimal)syllables / words.Length;

            var score = 206.835m - (1.015m * avgWordsPerSentence) - (84.6m * avgSyllablesPerWord);

            // Normalize to 0-100 scale
            return Math.Max(0, Math.Min(100, score));
        }

        private int CountSyllables(string word)
        {
            word = word.ToLower().Trim();
            if (word.Length <= 3) return 1;

            var vowels = "aeiouy";
            var syllableCount = 0;
            var previousWasVowel = false;

            foreach (var c in word)
            {
                var isVowel = vowels.Contains(c);
                if (isVowel && !previousWasVowel)
                {
                    syllableCount++;
                }
                previousWasVowel = isVowel;
            }

            // Adjust for silent 'e'
            if (word.EndsWith("e")) syllableCount--;

            return Math.Max(1, syllableCount);
        }

        /// <summary>
        /// Calculate keyword density
        /// </summary>
        private decimal CalculateKeywordDensity(string content, List<string> keywords)
        {
            if (string.IsNullOrWhiteSpace(content) || keywords.Count == 0) return 0;

            var contentLower = content.ToLower();
            var totalWords = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (totalWords == 0) return 0;

            var keywordOccurrences = keywords.Sum(keyword =>
            {
                var count = 0;
                var index = 0;
                while ((index = contentLower.IndexOf(keyword.ToLower(), index)) != -1)
                {
                    count++;
                    index += keyword.Length;
                }
                return count;
            });

            return ((decimal)keywordOccurrences / totalWords) * 100;
        }

        /// <summary>
        /// Calculate overall SEO score (0-100)
        /// </summary>
        private int CalculateSeoScore(
            string title,
            string content,
            string metaDescription,
            KeywordClassification keywords,
            decimal readabilityScore,
            decimal keywordDensity)
        {
            var score = 0;

            // Title optimization (20 points)
            if (title.Length >= 30 && title.Length <= 60) score += 10;
            if (keywords.Primary.Any(k => title.ToLower().Contains(k.Keyword.ToLower()))) score += 10;

            // Content quality (25 points)
            if (content.Length >= 300) score += 10;
            if (readabilityScore >= 60) score += 10;
            if (keywordDensity >= 1 && keywordDensity <= 3) score += 5;

            // Meta description (15 points)
            if (metaDescription.Length >= 120 && metaDescription.Length <= 160) score += 10;
            if (PowerWords.Any(pw => metaDescription.Contains(pw, StringComparison.OrdinalIgnoreCase))) score += 5;

            // Keyword research (30 points)
            if (keywords.Primary.Count >= 1) score += 10;
            if (keywords.Secondary.Count >= 3) score += 10;
            if (keywords.LSI.Count >= 5) score += 10;

            // Search volume (10 points)
            if (keywords.TotalSearchVolume > 10000) score += 10;
            else if (keywords.TotalSearchVolume > 5000) score += 5;

            return Math.Min(100, score);
        }

        /// <summary>
        /// Generate SERP (Search Engine Result Page) preview
        /// </summary>
        private string GenerateSerpPreview(string title, string metaDescription)
        {
            return $"<div class='serp-preview'><h3>{title}</h3><p>{metaDescription}</p></div>";
        }

        private List<string> ExtractPowerWords(string text)
        {
            return PowerWords.Where(pw => text.Contains(pw, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<string> ExtractEmotionalTriggers(string text)
        {
            return EmotionalTriggers.Where(et => text.Contains(et, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private decimal CalculatePredictedCtrImprovement(string metaDescription, int seoScore)
        {
            var baseImprovement = (decimal)seoScore / 10; // Base on SEO score

            // Bonus for power words
            var powerWordCount = ExtractPowerWords(metaDescription).Count;
            var powerWordBonus = powerWordCount * 5;

            // Bonus for emotional triggers
            var emotionalTriggerCount = ExtractEmotionalTriggers(metaDescription).Count;
            var emotionalBonus = emotionalTriggerCount * 10;

            return Math.Min(100, baseImprovement + powerWordBonus + emotionalBonus);
        }

        /// <summary>
        /// Save enhanced SEO metadata to database
        /// </summary>
        public async Task SaveEnhancedSeoAsync(int postId, EnhancedSeoResult seoResult)
        {
            try
            {
                var existing = await _context.EnhancedSeoMetadata
                    .FirstOrDefaultAsync(e => e.PostId == postId);

                if (existing != null)
                {
                    // Update existing
                    existing.OptimizedMetaDescription = seoResult.OptimizedMetaDescription;
                    existing.PrimaryKeywords = string.Join(", ", seoResult.PrimaryKeywords.Select(k => k.Keyword));
                    existing.SecondaryKeywords = string.Join(", ", seoResult.SecondaryKeywords.Select(k => k.Keyword));
                    existing.LsiKeywords = string.Join(", ", seoResult.LsiKeywords.Select(k => k.Keyword));
                    existing.TotalSearchVolume = seoResult.TotalSearchVolume;
                    existing.ReadabilityScore = seoResult.ReadabilityScore;
                    existing.SeoScore = seoResult.SeoScore;
                    existing.KeywordDensity = seoResult.KeywordDensity;
                    existing.PowerWords = string.Join(", ", seoResult.PowerWordsUsed);
                    existing.EmotionalTriggers = string.Join(", ", seoResult.EmotionalTriggersUsed);
                    existing.PredictedCtrImprovement = seoResult.PredictedCtrImprovement;
                    existing.SerpPreview = seoResult.SerpPreview;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new
                    var metadata = new EnhancedSeoMetadata
                    {
                        PostId = postId,
                        OptimizedMetaDescription = seoResult.OptimizedMetaDescription,
                        PrimaryKeywords = string.Join(", ", seoResult.PrimaryKeywords.Select(k => k.Keyword)),
                        SecondaryKeywords = string.Join(", ", seoResult.SecondaryKeywords.Select(k => k.Keyword)),
                        LsiKeywords = string.Join(", ", seoResult.LsiKeywords.Select(k => k.Keyword)),
                        TotalSearchVolume = seoResult.TotalSearchVolume,
                        ReadabilityScore = seoResult.ReadabilityScore,
                        SeoScore = seoResult.SeoScore,
                        KeywordDensity = seoResult.KeywordDensity,
                        PowerWords = string.Join(", ", seoResult.PowerWordsUsed),
                        EmotionalTriggers = string.Join(", ", seoResult.EmotionalTriggersUsed),
                        PredictedCtrImprovement = seoResult.PredictedCtrImprovement,
                        SerpPreview = seoResult.SerpPreview,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.EnhancedSeoMetadata.Add(metadata);
                }

                // Save keywords separately
                await SaveKeywordsAsync(postId, seoResult);

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Enhanced SEO saved for post {PostId}", postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to save enhanced SEO for post {PostId}", postId);
            }
        }

        private async Task SaveKeywordsAsync(int postId, EnhancedSeoResult seoResult)
        {
            // Remove existing keywords
            var existing = await _context.PostKeywords.Where(k => k.PostId == postId).ToListAsync();
            _context.PostKeywords.RemoveRange(existing);

            var priority = 1;

            // Add primary keywords
            foreach (var keyword in seoResult.PrimaryKeywords)
            {
                _context.PostKeywords.Add(new PostKeyword
                {
                    PostId = postId,
                    Keyword = keyword.Keyword,
                    KeywordType = "Primary",
                    SearchVolume = keyword.SearchVolume,
                    Competition = keyword.Competition,
                    SuggestedBidLow = keyword.LowBid,
                    SuggestedBidHigh = keyword.HighBid,
                    DifficultyScore = keyword.CompetitionIndex,
                    Priority = priority++
                });
            }

            // Add secondary keywords
            foreach (var keyword in seoResult.SecondaryKeywords)
            {
                _context.PostKeywords.Add(new PostKeyword
                {
                    PostId = postId,
                    Keyword = keyword.Keyword,
                    KeywordType = "Secondary",
                    SearchVolume = keyword.SearchVolume,
                    Competition = keyword.Competition,
                    SuggestedBidLow = keyword.LowBid,
                    SuggestedBidHigh = keyword.HighBid,
                    DifficultyScore = keyword.CompetitionIndex,
                    Priority = priority++
                });
            }

            // Add LSI keywords
            foreach (var keyword in seoResult.LsiKeywords)
            {
                _context.PostKeywords.Add(new PostKeyword
                {
                    PostId = postId,
                    Keyword = keyword.Keyword,
                    KeywordType = "LSI",
                    SearchVolume = keyword.SearchVolume,
                    Competition = keyword.Competition,
                    SuggestedBidLow = keyword.LowBid,
                    SuggestedBidHigh = keyword.HighBid,
                    DifficultyScore = keyword.CompetitionIndex,
                    Priority = priority++
                });
            }
        }
    }

    // Result class
    public class EnhancedSeoResult
    {
        public string OptimizedMetaDescription { get; set; } = string.Empty;
        public List<KeywordData> PrimaryKeywords { get; set; } = new();
        public List<KeywordData> SecondaryKeywords { get; set; } = new();
        public List<KeywordData> LsiKeywords { get; set; } = new();
        public long TotalSearchVolume { get; set; }
        public decimal ReadabilityScore { get; set; }
        public decimal KeywordDensity { get; set; }
        public int SeoScore { get; set; }
        public string SerpPreview { get; set; } = string.Empty;
        public List<string> PowerWordsUsed { get; set; } = new();
        public List<string> EmotionalTriggersUsed { get; set; } = new();
        public decimal PredictedCtrImprovement { get; set; }
    }
}

