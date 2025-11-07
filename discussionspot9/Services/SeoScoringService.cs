using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.GoogleSearch;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace discussionspot9.Services
{
    /// <summary>
    /// Comprehensive SEO Scoring Service
    /// Calculates SEO scores (0-100) with Google Search competitiveness focus
    /// </summary>
    public class SeoScoringService
    {
        private readonly GoogleSearchService _googleSearchService;
        private readonly ISeoAnalyzerService _pythonAnalyzer;
        private readonly EnhancedSeoService _enhancedSeoService;
        private readonly ImageSeoOptimizer _imageSeoOptimizer;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeoScoringService> _logger;

        public SeoScoringService(
            GoogleSearchService googleSearchService,
            ISeoAnalyzerService pythonAnalyzer,
            EnhancedSeoService enhancedSeoService,
            ImageSeoOptimizer imageSeoOptimizer,
            ApplicationDbContext context,
            ILogger<SeoScoringService> logger)
        {
            _googleSearchService = googleSearchService;
            _pythonAnalyzer = pythonAnalyzer;
            _enhancedSeoService = enhancedSeoService;
            _imageSeoOptimizer = imageSeoOptimizer;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Calculate comprehensive SEO score for a post
        /// </summary>
        public async Task<SeoScoreResult> CalculateScoreAsync(int postId)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return new SeoScoreResult
                    {
                        Success = false,
                        ErrorMessage = "Post not found"
                    };
                }

                // Get existing SEO metadata
                var seoMetadata = await _context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == postId);

                // Step 1: Google Search Competitiveness (30% weight)
                var googleScore = await CalculateGoogleCompetitivenessAsync(post);

                // Step 2: Content Quality (25% weight) - Enhanced with structure analysis
                var contentScore = CalculateContentQuality(post);

                // Step 3: Meta Completeness (10% weight)
                var metaScore = CalculateMetaCompleteness(seoMetadata, post);

                // Step 4: Freshness & Engagement (10% weight)
                var freshnessScore = CalculateFreshnessScore(post);

                // Step 5: Image SEO (10% weight) - NEW
                var imageScore = await CalculateImageSeoScoreAsync(post);

                // Step 6: Technical SEO (10% weight) - NEW (URL, structure, etc.)
                var technicalScore = CalculateTechnicalSeoScore(post, seoMetadata);

                // Step 7: Content Structure (5% weight) - NEW (H1/H2/H3 hierarchy)
                var structureScore = CalculateContentStructureScore(post);

                // Calculate weighted final score (updated weights)
                var finalScore = (googleScore * 0.30m) + 
                                (contentScore * 0.25m) + 
                                (metaScore * 0.10m) + 
                                (freshnessScore * 0.10m) +
                                (imageScore * 0.10m) +
                                (technicalScore * 0.10m) +
                                (structureScore * 0.05m);

                // Determine tier
                var tier = finalScore >= 80 ? "OK" : finalScore >= 50 ? "NeedsImprovement" : "Critical";

                // Collect issues (enhanced with new factors)
                var issues = await CollectIssues(post, seoMetadata, googleScore, contentScore, metaScore, freshnessScore, imageScore, technicalScore, structureScore);

                // Get recommended keywords from Google Search
                var keywords = await GetRecommendedKeywordsAsync(post);

                // Get top competitors
                var competitors = await GetTopCompetitorsAsync(post);

                // Calculate priority rank (higher = more urgent)
                var priorityRank = CalculatePriorityRank(finalScore, googleScore, post);

                return new SeoScoreResult
                {
                    Success = true,
                    PostId = postId,
                    Score = finalScore,
                    Tier = tier,
                    GoogleCompetitivenessScore = googleScore,
                    ContentQualityScore = contentScore,
                    MetaCompletenessScore = metaScore,
                    FreshnessScore = freshnessScore,
                    ImageSeoScore = imageScore,
                    TechnicalSeoScore = technicalScore,
                    ContentStructureScore = structureScore,
                    Issues = issues,
                    RecommendedKeywords = keywords,
                    TopCompetitors = competitors,
                    PriorityRank = priorityRank
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating SEO score for post {PostId}", postId);
                return new SeoScoreResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Calculate Google Search competitiveness score (0-100, 40% weight)
        /// </summary>
        private async Task<decimal> CalculateGoogleCompetitivenessAsync(Post post)
        {
            try
            {
                // Extract main keyword from title
                var mainKeyword = ExtractMainKeyword(post.Title);
                if (string.IsNullOrEmpty(mainKeyword))
                    return 30m; // Low score if no keyword

                // Get Google Search data
                var searchResult = await _googleSearchService.SearchAsync(mainKeyword, limit: 10, includeRelatedKeywords: true);
                if (searchResult == null)
                    return 30m;

                var topicInsights = await _googleSearchService.GetTopicInsightsAsync(mainKeyword);
                if (!topicInsights.Success)
                    return 30m;

                var score = 0m;

                // Factor 1: Related keywords availability (0-30 points)
                var relatedKeywordsCount = searchResult.RelatedKeywords?.Keywords?.Count ?? 0;
                score += Math.Min(30m, relatedKeywordsCount * 2m);

                // Factor 2: Competition analysis (0-25 points)
                // Lower competition = higher score
                var topDomains = topicInsights.TopRankingDomains;
                var highAuthorityCount = topDomains.Count(d => 
                    d.Contains("wikipedia.org") || 
                    d.Contains("amazon.com") || 
                    d.Contains("youtube.com") || 
                    d.Contains("reddit.com"));
                
                // More high-authority domains = harder competition = lower score
                var competitionScore = 25m - (highAuthorityCount * 5m);
                score += Math.Max(0m, competitionScore);

                // Factor 3: Title match with top results (0-25 points)
                var titleMatchScore = CalculateTitleMatchScore(post.Title, searchResult.Results);
                score += titleMatchScore;

                // Factor 4: Keyword search volume indicator (0-20 points)
                // If we have many related keywords, it suggests good search volume
                if (relatedKeywordsCount > 5)
                    score += 20m;
                else if (relatedKeywordsCount > 2)
                    score += 10m;

                return Math.Min(100m, score);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating Google competitiveness for post {PostId}", post.PostId);
                return 30m; // Default low score on error
            }
        }

        /// <summary>
        /// Calculate content quality score (0-100, 30% weight)
        /// </summary>
        private decimal CalculateContentQuality(Post post)
        {
            var score = 0m;

            // Factor 1: Title quality (0-30 points)
            var titleLength = post.Title?.Length ?? 0;
            if (titleLength >= 30 && titleLength <= 60)
                score += 30m;
            else if (titleLength >= 20 && titleLength < 30)
                score += 20m;
            else if (titleLength > 60 && titleLength <= 70)
                score += 15m;
            else
                score += 5m;

            // Factor 2: Content length (0-30 points)
            var contentLength = post.Content?.Length ?? 0;
            if (contentLength >= 1000)
                score += 30m;
            else if (contentLength >= 500)
                score += 20m;
            else if (contentLength >= 300)
                score += 10m;
            else
                score += 5m;

            // Factor 3: Content structure (0-20 points) - Enhanced
            var structureScore = CalculateContentStructureBasic(post);
            score += structureScore;

            // Factor 4: Tags/keywords (0-20 points)
            var tagCount = post.PostTags?.Count ?? 0;
            if (tagCount >= 3)
                score += 20m;
            else if (tagCount >= 1)
                score += 10m;

            return Math.Min(100m, score);
        }

        /// <summary>
        /// Calculate meta completeness score (0-100, 15% weight)
        /// </summary>
        private decimal CalculateMetaCompleteness(SeoMetadata? seoMetadata, Post post)
        {
            var score = 0m;

            // Factor 1: Meta description (0-40 points)
            var metaDesc = seoMetadata?.MetaDescription ?? "";
            var metaDescLength = metaDesc.Length;
            if (metaDescLength >= 120 && metaDescLength <= 160)
                score += 40m;
            else if (metaDescLength >= 80 && metaDescLength < 120)
                score += 25m;
            else if (metaDescLength > 0)
                score += 10m;

            // Factor 2: Keywords (0-30 points)
            var keywords = seoMetadata?.Keywords ?? "";
            if (!string.IsNullOrEmpty(keywords))
            {
                var keywordCount = keywords.Split(',').Length;
                if (keywordCount >= 5)
                    score += 30m;
                else if (keywordCount >= 3)
                    score += 20m;
                else
                    score += 10m;
            }

            // Factor 3: OG tags (0-20 points)
            var hasOgTags = !string.IsNullOrEmpty(seoMetadata?.OgTitle) || 
                           !string.IsNullOrEmpty(seoMetadata?.OgDescription);
            score += hasOgTags ? 20m : 0m;

            // Factor 4: Twitter tags (0-10 points)
            var hasTwitterTags = !string.IsNullOrEmpty(seoMetadata?.TwitterTitle) || 
                                !string.IsNullOrEmpty(seoMetadata?.TwitterDescription);
            score += hasTwitterTags ? 10m : 0m;

            return Math.Min(100m, score);
        }

        /// <summary>
        /// Calculate freshness & engagement score (0-100, 15% weight)
        /// </summary>
        private decimal CalculateFreshnessScore(Post post)
        {
            var score = 0m;

            // Factor 1: Recency (0-50 points)
            var daysSinceUpdate = (DateTime.UtcNow - post.UpdatedAt).Days;
            if (daysSinceUpdate <= 7)
                score += 50m;
            else if (daysSinceUpdate <= 30)
                score += 30m;
            else if (daysSinceUpdate <= 90)
                score += 15m;
            else
                score += 5m;

            // Factor 2: Views/engagement (0-30 points)
            var viewCount = post.ViewCount;
            if (viewCount >= 1000)
                score += 30m;
            else if (viewCount >= 500)
                score += 20m;
            else if (viewCount >= 100)
                score += 10m;
            else
                score += 5m;

            // Factor 3: Comments (0-20 points)
            var commentCount = post.CommentCount;
            if (commentCount >= 10)
                score += 20m;
            else if (commentCount >= 5)
                score += 10m;
            else if (commentCount > 0)
                score += 5m;

            return Math.Min(100m, score);
        }

        /// <summary>
        /// Calculate Image SEO score (0-100, 10% weight)
        /// </summary>
        private async Task<decimal> CalculateImageSeoScoreAsync(Post post)
        {
            try
            {
                var images = await _context.Media
                    .Where(m => m.PostId == post.PostId && m.MediaType == "image")
                    .ToListAsync();

                if (images.Count == 0)
                    return 100m; // No images = no penalty

                var score = 0m;
                var imagesWithAlt = 0;
                var imagesWithCaption = 0;
                var imagesWithGoodAlt = 0;

                foreach (var image in images)
                {
                    // Alt text check (40 points per image)
                    if (!string.IsNullOrEmpty(image.AltText) && image.AltText.Length >= 10)
                    {
                        imagesWithAlt++;
                        if (image.AltText.Length >= 20 && image.AltText.Length <= 125)
                            imagesWithGoodAlt++;
                    }

                    // Caption check (20 points per image)
                    if (!string.IsNullOrEmpty(image.Caption))
                        imagesWithCaption++;
                }

                // Calculate score based on percentage
                var altPercentage = images.Count > 0 ? (imagesWithAlt / (decimal)images.Count) * 100 : 0;
                var goodAltPercentage = images.Count > 0 ? (imagesWithGoodAlt / (decimal)images.Count) * 100 : 0;
                var captionPercentage = images.Count > 0 ? (imagesWithCaption / (decimal)images.Count) * 100 : 0;

                score = (altPercentage * 0.6m) + (goodAltPercentage * 0.2m) + (captionPercentage * 0.2m);

                return Math.Min(100m, score);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating image SEO score for post {PostId}", post.PostId);
                return 50m; // Default score on error
            }
        }

        /// <summary>
        /// Calculate Technical SEO score (0-100, 10% weight)
        /// </summary>
        private decimal CalculateTechnicalSeoScore(Post post, SeoMetadata? seoMetadata)
        {
            var score = 0m;

            // Factor 1: URL/Slug quality (0-40 points)
            var slug = post.Slug ?? "";
            if (!string.IsNullOrEmpty(slug))
            {
                // Check slug length (optimal: 50-100 chars)
                if (slug.Length >= 30 && slug.Length <= 100)
                    score += 20m;
                else if (slug.Length > 0)
                    score += 10m;

                // Check if slug is keyword-rich (contains words, not just numbers)
                var hasWords = Regex.IsMatch(slug, @"[a-zA-Z]{3,}");
                score += hasWords ? 20m : 5m;
            }

            // Factor 2: Canonical URL (0-30 points)
            if (!string.IsNullOrEmpty(seoMetadata?.CanonicalUrl))
                score += 30m;

            // Factor 3: Meta tags completeness (0-30 points)
            var hasOgImage = !string.IsNullOrEmpty(seoMetadata?.OgImageUrl);
            var hasTwitterImage = !string.IsNullOrEmpty(seoMetadata?.TwitterImageUrl);
            if (hasOgImage && hasTwitterImage)
                score += 30m;
            else if (hasOgImage || hasTwitterImage)
                score += 15m;

            return Math.Min(100m, score);
        }

        /// <summary>
        /// Calculate Content Structure score (0-100, 5% weight)
        /// </summary>
        private decimal CalculateContentStructureScore(Post post)
        {
            if (string.IsNullOrEmpty(post.Content))
                return 0m;

            var score = 0m;
            var content = post.Content;

            // H1 check (should be 0, H1 is page title) - 30 points
            var h1Count = Regex.Matches(content, "<h1[^>]*>", RegexOptions.IgnoreCase).Count;
            score += h1Count == 0 ? 30m : 0m;

            // H2 check - 30 points
            var h2Count = Regex.Matches(content, "<h2[^>]*>", RegexOptions.IgnoreCase).Count;
            if (h2Count >= 2)
                score += 30m;
            else if (h2Count == 1)
                score += 20m;

            // H3 check - 20 points
            var h3Count = Regex.Matches(content, "<h3[^>]*>", RegexOptions.IgnoreCase).Count;
            if (h3Count >= 1)
                score += 20m;

            // Heading hierarchy check - 20 points (H2 should come before H3)
            if (h2Count > 0 && h3Count > 0)
            {
                // Simple check: if we have both H2 and H3, structure is good
                score += 20m;
            }
            else if (h2Count > 0)
            {
                score += 10m;
            }

            return Math.Min(100m, score);
        }

        /// <summary>
        /// Calculate basic content structure for content quality scoring
        /// </summary>
        private decimal CalculateContentStructureBasic(Post post)
        {
            if (string.IsNullOrEmpty(post.Content))
                return 5m;

            var score = 0m;
            var content = post.Content;

            // Check for headings
            var hasHeadings = Regex.IsMatch(content, "<h[2-6][^>]*>", RegexOptions.IgnoreCase);
            if (hasHeadings)
                score += 10m;

            // Check for paragraphs
            var hasParagraphs = content.Contains("<p>") || content.Contains("\n\n");
            if (hasParagraphs)
                score += 5m;

            // Check for lists
            var hasLists = Regex.IsMatch(content, "<[ou]l[^>]*>", RegexOptions.IgnoreCase);
            if (hasLists)
                score += 5m;

            return score;
        }

        /// <summary>
        /// Collect SEO issues (enhanced version)
        /// </summary>
        private async Task<List<string>> CollectIssues(Post post, SeoMetadata? seoMetadata, 
            decimal googleScore, decimal contentScore, decimal metaScore, decimal freshnessScore,
            decimal imageScore, decimal technicalScore, decimal structureScore)
        {
            var issues = new List<string>();

            // Title issues
            var titleLength = post.Title?.Length ?? 0;
            if (titleLength < 30)
                issues.Add($"Title too short ({titleLength} chars, recommended: 30-60)");
            else if (titleLength > 70)
                issues.Add($"Title too long ({titleLength} chars, recommended: 30-60)");

            // Content issues
            var contentLength = post.Content?.Length ?? 0;
            if (contentLength < 300)
                issues.Add($"Content too short ({contentLength} chars, recommended: 300+)");
            if (contentLength < 1000 && contentLength >= 300)
                issues.Add($"Content could be longer for better SEO ({contentLength} chars, recommended: 1000+)");

            // Meta issues
            if (string.IsNullOrEmpty(seoMetadata?.MetaDescription))
                issues.Add("Missing meta description");
            else
            {
                var metaLength = seoMetadata.MetaDescription.Length;
                if (metaLength < 120 || metaLength > 160)
                    issues.Add($"Meta description length not optimal ({metaLength} chars, recommended: 120-160)");
            }

            if (string.IsNullOrEmpty(seoMetadata?.Keywords))
                issues.Add("Missing keywords");

            // Google competitiveness issues
            if (googleScore < 50)
                issues.Add("Low Google Search competitiveness - need better keyword targeting");

            // Content quality issues
            if (contentScore < 50)
                issues.Add("Content quality needs improvement");

            // Freshness issues
            var daysSinceUpdate = (DateTime.UtcNow - post.UpdatedAt).Days;
            if (daysSinceUpdate > 90)
                issues.Add($"Content is outdated (last updated {daysSinceUpdate} days ago)");

            // Image SEO issues
            if (imageScore < 70)
            {
                var imageCount = await _context.Media.CountAsync(m => m.PostId == post.PostId && m.MediaType == "image");
                if (imageCount > 0)
                    issues.Add($"{imageCount} image(s) missing or have poor alt text");
            }

            // Technical SEO issues
            if (technicalScore < 50)
            {
                if (string.IsNullOrEmpty(seoMetadata?.CanonicalUrl))
                    issues.Add("Missing canonical URL");
                if (string.IsNullOrEmpty(post.Slug) || post.Slug.Length < 10)
                    issues.Add("URL slug needs improvement (should be keyword-rich and readable)");
            }

            // Content structure issues
            if (structureScore < 50)
            {
                var h1Count = Regex.Matches(post.Content ?? "", "<h1[^>]*>", RegexOptions.IgnoreCase).Count;
                if (h1Count > 0)
                    issues.Add($"Content contains {h1Count} H1 tag(s) - H1 should only be page title");
                
                var h2Count = Regex.Matches(post.Content ?? "", "<h2[^>]*>", RegexOptions.IgnoreCase).Count;
                if (h2Count == 0 && (post.Content?.Length ?? 0) > 300)
                    issues.Add("Content missing H2 headings - add section headings for better structure");
            }

            return issues.Take(8).ToList(); // Increased from 5 to 8 to show more issues
        }

        /// <summary>
        /// Get recommended keywords from Google Search
        /// </summary>
        private async Task<List<string>> GetRecommendedKeywordsAsync(Post post)
        {
            try
            {
                var mainKeyword = ExtractMainKeyword(post.Title);
                if (string.IsNullOrEmpty(mainKeyword))
                    return new List<string>();

                var searchResult = await _googleSearchService.SearchAsync(mainKeyword, limit: 10, includeRelatedKeywords: true);
                if (searchResult?.RelatedKeywords?.Keywords == null)
                    return new List<string>();

                return searchResult.RelatedKeywords.Keywords
                    .Take(10)
                    .Select(k => k.Keyword)
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Get top competitors from Google Search
        /// </summary>
        private async Task<List<string>> GetTopCompetitorsAsync(Post post)
        {
            try
            {
                var mainKeyword = ExtractMainKeyword(post.Title);
                if (string.IsNullOrEmpty(mainKeyword))
                    return new List<string>();

                var topicInsights = await _googleSearchService.GetTopicInsightsAsync(mainKeyword);
                if (!topicInsights.Success)
                    return new List<string>();

                return topicInsights.TopRankingDomains.Take(5).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Calculate title match score with top search results
        /// </summary>
        private decimal CalculateTitleMatchScore(string title, List<SearchResult> results)
        {
            if (results == null || !results.Any())
                return 10m;

            var titleWords = title.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var matchCount = 0;

            foreach (var result in results.Take(5))
            {
                var resultTitle = result.Title.ToLower();
                var matches = titleWords.Count(word => resultTitle.Contains(word));
                if (matches > 0)
                    matchCount++;
            }

            // Score based on how many top results have matching words
            return Math.Min(25m, matchCount * 5m);
        }

        /// <summary>
        /// Extract main keyword from title
        /// </summary>
        private string ExtractMainKeyword(string title)
        {
            if (string.IsNullOrEmpty(title))
                return "";

            // Remove common stop words and get first significant word
            var stopWords = new[] { "the", "a", "an", "how", "what", "when", "where", "why", "is", "are", "was", "were" };
            var words = title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !stopWords.Contains(w.ToLower()))
                .Where(w => w.Length > 3)
                .ToList();

            return words.FirstOrDefault() ?? title.Split(' ').FirstOrDefault() ?? title;
        }

        /// <summary>
        /// Calculate priority rank (higher = more urgent)
        /// </summary>
        private int CalculatePriorityRank(decimal finalScore, decimal googleScore, Post post)
        {
            var rank = 0;

            // Critical posts with high Google competitiveness get highest priority
            if (finalScore < 50 && googleScore > 40)
                rank += 1000; // High traffic potential but low SEO

            // Low score = higher priority
            rank += (int)(100 - finalScore) * 10;

            // High Google competitiveness = higher priority (more traffic potential)
            rank += (int)googleScore * 2;

            // Missing meta = higher priority (quick win)
            var hasMeta = _context.SeoMetadata.Any(s => s.EntityType == "post" && s.EntityId == post.PostId);
            if (!hasMeta)
                rank += 500;

            // High views but low score = high priority
            if (post.ViewCount > 500 && finalScore < 70)
                rank += 300;

            return rank;
        }

        /// <summary>
        /// Save score to database
        /// </summary>
        public async Task SaveScoreAsync(SeoScoreResult result)
        {
            try
            {
                var existingScore = await _context.SeoScores
                    .FirstOrDefaultAsync(s => s.PostId == result.PostId);

                if (existingScore == null)
                {
                    existingScore = new SeoScore
                    {
                        PostId = result.PostId,
                        ScoredAt = DateTime.UtcNow
                    };
                    _context.SeoScores.Add(existingScore);
                }

                existingScore.Score = result.Score;
                existingScore.Tier = result.Tier;
                existingScore.GoogleCompetitivenessScore = result.GoogleCompetitivenessScore;
                existingScore.ContentQualityScore = result.ContentQualityScore;
                existingScore.MetaCompletenessScore = result.MetaCompletenessScore;
                existingScore.FreshnessScore = result.FreshnessScore;
                existingScore.ImageSeoScore = result.ImageSeoScore;
                existingScore.TechnicalSeoScore = result.TechnicalSeoScore;
                existingScore.ContentStructureScore = result.ContentStructureScore;
                existingScore.Issues = JsonSerializer.Serialize(result.Issues);
                existingScore.RecommendedKeywords = JsonSerializer.Serialize(result.RecommendedKeywords);
                existingScore.TopCompetitors = JsonSerializer.Serialize(result.TopCompetitors);
                existingScore.PriorityRank = result.PriorityRank;
                existingScore.Source = "Hybrid";
                existingScore.ScoredAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving SEO score for post {PostId}", result.PostId);
                throw;
            }
        }

        /// <summary>
        /// Batch calculate scores for multiple posts
        /// </summary>
        public async Task<List<SeoScoreResult>> BatchCalculateScoresAsync(List<int> postIds)
        {
            var results = new List<SeoScoreResult>();

            foreach (var postId in postIds)
            {
                try
                {
                    var result = await CalculateScoreAsync(postId);
                    if (result.Success)
                    {
                        await SaveScoreAsync(result);
                    }
                    results.Add(result);

                    // Rate limiting - delay between requests
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating score for post {PostId}", postId);
                    results.Add(new SeoScoreResult
                    {
                        Success = false,
                        PostId = postId,
                        ErrorMessage = ex.Message
                    });
                }
            }

            return results;
        }
    }

    /// <summary>
    /// SEO Score calculation result
    /// </summary>
    public class SeoScoreResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int PostId { get; set; }
        public decimal Score { get; set; }
        public string Tier { get; set; } = "Critical"; // OK, NeedsImprovement, Critical
        public decimal GoogleCompetitivenessScore { get; set; }
        public decimal ContentQualityScore { get; set; }
        public decimal MetaCompletenessScore { get; set; }
        public decimal FreshnessScore { get; set; }
        public decimal ImageSeoScore { get; set; }
        public decimal TechnicalSeoScore { get; set; }
        public decimal ContentStructureScore { get; set; }
        public List<string> Issues { get; set; } = new();
        public List<string> RecommendedKeywords { get; set; } = new();
        public List<string> TopCompetitors { get; set; } = new();
        public int PriorityRank { get; set; }
    }
}

