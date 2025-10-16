using discussionspot9.Interfaces;
using discussionspot9.Models.Semrush;
using discussionspot9.Models.Domain;
using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace discussionspot9.Services
{
    /// <summary>
    /// Enhanced SEO service that integrates Semrush data with existing SEO workflow
    /// </summary>
    public class EnhancedSeoService
    {
        private readonly ISemrushService _semrushService;
        private readonly ISeoAnalyzerService _seoAnalyzerService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnhancedSeoService> _logger;

        public EnhancedSeoService(
            ISemrushService semrushService,
            ISeoAnalyzerService seoAnalyzerService,
            ApplicationDbContext context,
            ILogger<EnhancedSeoService> logger)
        {
            _semrushService = semrushService;
            _seoAnalyzerService = seoAnalyzerService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Generate comprehensive SEO analysis with Semrush data for a post
        /// </summary>
        public async Task<EnhancedSeoResult> GenerateEnhancedSeoAsync(int postId)
        {
            try
            {
                // Get the post
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    throw new ArgumentException($"Post with ID {postId} not found");
                }

                // Extract initial keywords from post content
                var initialKeywords = ExtractKeywordsFromPost(post);

                // Get Semrush data for top keywords
                var semrushData = await GetSemrushDataForKeywords(initialKeywords.Take(5).ToList());

                // Run enhanced Python SEO analysis with Semrush data
                var seoAnalysis = await _seoAnalyzerService.AnalyzePostAsync(new discussionspot9.Models.ViewModels.CreativeViewModels.CreatePostViewModel
                {
                    Title = post.Title,
                    Content = post.Content ?? "",
                    CommunitySlug = post.Community?.Slug ?? "",
                    PostType = post.PostType
                });

                // Generate enhanced SEO metadata
                var enhancedSeo = await GenerateEnhancedSeoMetadata(post, seoAnalysis, semrushData);

                // Save to database
                await SaveEnhancedSeoMetadata(postId, enhancedSeo);

                return new EnhancedSeoResult
                {
                    PostId = postId,
                    SeoAnalysis = seoAnalysis,
                    SemrushData = semrushData,
                    EnhancedMetadata = enhancedSeo,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating enhanced SEO for post {PostId}", postId);
                return new EnhancedSeoResult
                {
                    PostId = postId,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Get keyword suggestions for a post based on Semrush data
        /// </summary>
        public async Task<List<KeywordSuggestion>> GetKeywordSuggestionsAsync(int postId, int limit = 10)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return new List<KeywordSuggestion>();
                }

                // Extract seed keywords from post
                var seedKeywords = ExtractKeywordsFromPost(post);
                var suggestions = new List<KeywordSuggestion>();

                // Get suggestions for top seed keywords
                foreach (var seedKeyword in seedKeywords.Take(3))
                {
                    var keywordSuggestions = await _semrushService.GetKeywordSuggestionsAsync(seedKeyword, limit: limit / 3);
                    suggestions.AddRange(keywordSuggestions);
                }

                // Remove duplicates and sort by relevance
                return suggestions
                    .GroupBy(s => s.Keyword)
                    .Select(g => g.First())
                    .OrderByDescending(s => s.SearchVolume)
                    .Take(limit)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting keyword suggestions for post {PostId}", postId);
                return new List<KeywordSuggestion>();
            }
        }

        /// <summary>
        /// Analyze competitor keywords for a domain
        /// </summary>
        public async Task<List<CompetitorKeyword>> AnalyzeCompetitorKeywordsAsync(string domain, int limit = 20)
        {
            try
            {
                return await _semrushService.GetCompetitorKeywordsAsync(domain, limit: limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing competitor keywords for domain {Domain}", domain);
                return new List<CompetitorKeyword>();
            }
        }

        private List<string> ExtractKeywordsFromPost(Post post)
        {
            var text = $"{post.Title} {post.Content}";
            var words = text.ToLower()
                .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '-', '(', ')', '[', ']', '{', '}' }, 
                       StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 4)
                .ToList();

            // Remove common stop words
            var stopWords = new HashSet<string>
            {
                "this", "that", "these", "those", "what", "which", "who", "when",
                "where", "why", "how", "with", "from", "have", "has", "had",
                "will", "would", "could", "should", "can", "may", "might",
                "the", "and", "or", "but", "in", "on", "at", "to", "for",
                "of", "as", "by", "an", "be", "is", "are", "was", "were",
                "been", "being", "do", "does", "did", "just", "about",
                "into", "through", "during", "before", "after", "above",
                "below", "between", "under", "again", "further", "then",
                "once", "here", "there", "all", "both", "each", "few",
                "more", "most", "other", "some", "such", "only", "own",
                "same", "than", "too", "very", "now", "also", "back",
                "well", "even", "much", "still", "way", "many", "any",
                "new", "old", "first", "last", "long", "great", "little",
                "own", "other", "right", "big", "high", "different",
                "small", "large", "next", "early", "young", "important",
                "few", "public", "bad", "same", "able"
            };

            var filteredWords = words.Where(w => !stopWords.Contains(w)).ToList();

            // Count word frequency
            var wordFreq = filteredWords
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(10)
                .ToList();

            return wordFreq;
        }

        private async Task<Dictionary<string, KeywordOverviewResult>> GetSemrushDataForKeywords(List<string> keywords)
        {
            var semrushData = new Dictionary<string, KeywordOverviewResult>();

            foreach (var keyword in keywords)
            {
                try
                {
                    var result = await _semrushService.GetKeywordOverviewAsync(keyword);
                    if (result != null)
                    {
                        semrushData[keyword] = result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get Semrush data for keyword: {Keyword}", keyword);
                }
            }

            return semrushData;
        }

        private async Task<EnhancedSeoMetadata> GenerateEnhancedSeoMetadata(
            Post post, 
            SeoAnalysisResult seoAnalysis, 
            Dictionary<string, KeywordOverviewResult> semrushData)
        {
            // Get best performing keywords from Semrush data
            var bestKeywords = semrushData
                .Where(kv => kv.Value.SearchVolume > 100 && kv.Value.KeywordDifficulty < 70)
                .OrderByDescending(kv => kv.Value.SearchVolume)
                .Take(5)
                .Select(kv => kv.Key)
                .ToList();

            // Combine with Python analyzer suggestions
            var allKeywords = bestKeywords
                .Concat(seoAnalysis.SuggestedKeywords)
                .Distinct()
                .Take(10)
                .ToList();

            // Generate enhanced meta description with high-value keywords
            var enhancedDescription = GenerateEnhancedMetaDescription(
                seoAnalysis.SuggestedMetaDescription, 
                bestKeywords);

            return new EnhancedSeoMetadata
            {
                MetaTitle = seoAnalysis.OptimizedTitle,
                MetaDescription = enhancedDescription,
                Keywords = string.Join(", ", allKeywords),
                StructuredData = JsonSerializer.Serialize(new
                {
                    semrush_analysis = semrushData,
                    python_analysis = new
                    {
                        seo_score = seoAnalysis.SeoScore,
                        issues_found = seoAnalysis.IssuesFound,
                        improvements_made = seoAnalysis.ImprovementsMade
                    },
                    top_keywords = bestKeywords,
                    generated_at = DateTime.UtcNow
                }),
                OgTitle = seoAnalysis.OptimizedTitle,
                OgDescription = enhancedDescription,
                TwitterTitle = seoAnalysis.OptimizedTitle,
                TwitterDescription = enhancedDescription
            };
        }

        private string GenerateEnhancedMetaDescription(string baseDescription, List<string> highValueKeywords)
        {
            if (string.IsNullOrEmpty(baseDescription))
            {
                return string.Join(", ", highValueKeywords.Take(3));
            }

            // Try to naturally incorporate high-value keywords
            var description = baseDescription;
            var maxLength = 160;

            foreach (var keyword in highValueKeywords.Take(2))
            {
                if (!description.ToLower().Contains(keyword.ToLower()) && 
                    description.Length + keyword.Length + 2 <= maxLength)
                {
                    description += $", {keyword}";
                }
            }

            return description.Length > maxLength 
                ? description.Substring(0, maxLength - 3) + "..." 
                : description;
        }

        private async Task SaveEnhancedSeoMetadata(int postId, EnhancedSeoMetadata metadata)
        {
            try
            {
                var existingSeo = await _context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == postId);

                if (existingSeo != null)
                {
                    // Update existing
                    existingSeo.MetaTitle = metadata.MetaTitle;
                    existingSeo.MetaDescription = metadata.MetaDescription;
                    existingSeo.Keywords = metadata.Keywords;
                    existingSeo.StructuredData = metadata.StructuredData;
                    existingSeo.OgTitle = metadata.OgTitle;
                    existingSeo.OgDescription = metadata.OgDescription;
                    existingSeo.TwitterTitle = metadata.TwitterTitle;
                    existingSeo.TwitterDescription = metadata.TwitterDescription;
                    existingSeo.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new
                    var newSeo = new SeoMetadata
                    {
                        EntityType = "post",
                        EntityId = postId,
                        MetaTitle = metadata.MetaTitle,
                        MetaDescription = metadata.MetaDescription,
                        Keywords = metadata.Keywords,
                        StructuredData = metadata.StructuredData,
                        OgTitle = metadata.OgTitle,
                        OgDescription = metadata.OgDescription,
                        TwitterTitle = metadata.TwitterTitle,
                        TwitterDescription = metadata.TwitterDescription,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.SeoMetadata.Add(newSeo);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Saved enhanced SEO metadata for post {PostId}", postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving enhanced SEO metadata for post {PostId}", postId);
                throw;
            }
        }
    }

    /// <summary>
    /// Enhanced SEO analysis request
    /// </summary>
    public class SeoAnalysisRequest
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string PostType { get; set; } = string.Empty;
        public Dictionary<string, KeywordOverviewResult>? SemrushData { get; set; }
    }

    /// <summary>
    /// Enhanced SEO result
    /// </summary>
    public class EnhancedSeoResult
    {
        public int PostId { get; set; }
        public SeoAnalysisResult? SeoAnalysis { get; set; }
        public Dictionary<string, KeywordOverviewResult>? SemrushData { get; set; }
        public EnhancedSeoMetadata? EnhancedMetadata { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Enhanced SEO metadata
    /// </summary>
    public class EnhancedSeoMetadata
    {
        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
        public string StructuredData { get; set; } = string.Empty;
        public string OgTitle { get; set; } = string.Empty;
        public string OgDescription { get; set; } = string.Empty;
        public string TwitterTitle { get; set; } = string.Empty;
        public string TwitterDescription { get; set; } = string.Empty;
    }
}