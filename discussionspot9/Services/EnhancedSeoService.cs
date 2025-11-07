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
    /// Enhanced SEO Service combining Google Search API + AI + Python Analyzer
    /// Provides real-time SEO optimization for post creation and editing
    /// </summary>
    public class EnhancedSeoService
    {
        private readonly GoogleSearchService _googleSearchService;
        private readonly AISeoService _aiSeoService;
        private readonly GoogleSearchSeoService _googleSeoService;
        private readonly ISeoAnalyzerService _pythonAnalyzer;
        private readonly HybridSeoService _hybridSeoService;
        private readonly ImageSeoOptimizer _imageSeoOptimizer;
        private readonly ImageStructuredDataService _imageStructuredDataService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnhancedSeoService> _logger;

        public EnhancedSeoService(
            GoogleSearchService googleSearchService,
            AISeoService aiSeoService,
            GoogleSearchSeoService googleSeoService,
            ISeoAnalyzerService pythonAnalyzer,
            HybridSeoService hybridSeoService,
            ImageSeoOptimizer imageSeoOptimizer,
            ImageStructuredDataService imageStructuredDataService,
            ApplicationDbContext context,
            ILogger<EnhancedSeoService> logger)
        {
            _googleSearchService = googleSearchService;
            _aiSeoService = aiSeoService;
            _googleSeoService = googleSeoService;
            _pythonAnalyzer = pythonAnalyzer;
            _hybridSeoService = hybridSeoService;
            _imageSeoOptimizer = imageSeoOptimizer;
            _imageStructuredDataService = imageStructuredDataService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Optimize post during creation - combines all SEO services (HYBRID APPROACH)
        /// </summary>
        public async Task<SeoOptimizationResult> OptimizeOnCreateAsync(CreatePostViewModel model)
        {
            try
            {
                _logger.LogInformation("Starting HYBRID SEO optimization for post creation: {Title}", model.Title);

                // Step 1: Try Hybrid SEO Service first (Python + All RapidAPI services)
                HybridSeoResult? hybridResult = null;
                try
                {
                    hybridResult = await _hybridSeoService.OptimizeAsync(model);
                    if (hybridResult.Success)
                    {
                        _logger.LogInformation("✅ Hybrid SEO optimization successful. Score: {Score}/100", hybridResult.FinalSeoScore);
                        
                        // Use hybrid result as primary
                        return new SeoOptimizationResult
                        {
                            Success = true,
                            OptimizedTitle = hybridResult.OptimizedTitle,
                            OptimizedContent = hybridResult.OptimizedContent,
                            MetaDescription = hybridResult.MetaDescription,
                            Keywords = hybridResult.Keywords,
                            SeoScore = hybridResult.FinalSeoScore,
                            BaselineScore = hybridResult.SeoScore,
                            EstimatedScore = hybridResult.FinalSeoScore,
                            Improvements = hybridResult.Improvements,
                            GoogleRelatedKeywords = hybridResult.Keywords.Take(10).ToList()
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Hybrid SEO optimization failed, falling back to individual services");
                }

                // Step 2: Fallback to individual services if hybrid fails
                _logger.LogInformation("Falling back to individual SEO services");

                // Extract initial keywords from title and content
                var initialKeywords = ExtractKeywords(model.Title, model.Content ?? "");

                // Google Search API - Get related keywords and insights
                var topKeyword = initialKeywords.FirstOrDefault() ?? model.Title;
                var googleSearchResult = await _googleSearchService.SearchAsync(
                    topKeyword, 
                    limit: 10, 
                    includeRelatedKeywords: true);

                var topicInsights = await _googleSearchService.GetTopicInsightsAsync(topKeyword);

                // Python Analyzer - Baseline SEO analysis
                var pythonResult = await _pythonAnalyzer.AnalyzePostAsync(model);

                // AI Service - Content optimization
                AISeoOptimizationResult? aiResult = null;
                try
                {
                    // Get community name for AI context
                    var communityName = model.CommunitySlug ?? "";
                    if (!string.IsNullOrEmpty(model.CommunitySlug))
                    {
                        var community = await _context.Communities
                            .FirstOrDefaultAsync(c => c.Slug == model.CommunitySlug);
                        communityName = community?.Name ?? communityName;
                    }

                    aiResult = await _aiSeoService.OptimizeContentAsync(
                        model.Title,
                        model.Content ?? "",
                        communityName,
                        pythonResult);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AI optimization failed, continuing with other optimizations");
                }

                // Step 5: Combine all results
                var optimizedKeywords = CombineKeywords(
                    initialKeywords,
                    googleSearchResult?.RelatedKeywords?.Keywords.Select(k => k.Keyword).ToList() ?? new(),
                    pythonResult.SuggestedKeywords,
                    aiResult?.SuggestedKeywords ?? new()
                );

                var optimizedTitle = aiResult?.OptimizedTitle ?? pythonResult.OptimizedTitle ?? model.Title;
                var optimizedContent = aiResult?.OptimizedContent ?? model.Content;
                var metaDescription = aiResult?.SuggestedMetaDescription 
                    ?? pythonResult.SuggestedMetaDescription 
                    ?? GenerateMetaDescription(model.Title, model.Content ?? "");

                // Step 6: Optimize content structure (H1/H2/H3, keyword placement)
                optimizedContent = OptimizeContentStructure(optimizedContent, optimizedTitle, optimizedKeywords);

                // Step 7: Calculate final SEO score
                var seoScore = CalculateFinalSeoScore(
                    pythonResult.SeoScore,
                    aiResult?.EstimatedScore ?? 0,
                    googleSearchResult != null,
                    optimizedKeywords.Count,
                    optimizedContent
                );

                var improvements = CombineImprovements(pythonResult.ImprovementsMade, aiResult?.Improvements ?? new());
                
                // Add content structure improvements
                var structureImprovements = AnalyzeContentStructure(optimizedContent, optimizedTitle, optimizedKeywords);
                improvements.AddRange(structureImprovements);

                return new SeoOptimizationResult
                {
                    Success = true,
                    OptimizedTitle = optimizedTitle,
                    OptimizedContent = optimizedContent,
                    MetaDescription = metaDescription,
                    Keywords = optimizedKeywords,
                    SeoScore = seoScore,
                    GoogleRelatedKeywords = googleSearchResult?.RelatedKeywords?.Keywords.Select(k => k.Keyword).ToList() ?? new(),
                    TopCompetitors = topicInsights.TopRankingDomains,
                    Improvements = improvements,
                    BaselineScore = pythonResult.SeoScore,
                    EstimatedScore = seoScore,
                    ContentStructureScore = CalculateContentStructureScore(optimizedContent, optimizedTitle),
                    KeywordDensity = CalculateKeywordDensity(optimizedContent, optimizedKeywords),
                    InternalLinksCount = CountInternalLinks(optimizedContent)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing post during creation");
                return new SeoOptimizationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Optimize post during editing - includes existing SEO data
        /// </summary>
        public async Task<SeoOptimizationResult> OptimizeOnEditAsync(EditPostViewModel model, int postId)
        {
            try
            {
                _logger.LogInformation("Starting SEO optimization for post edit: PostId={PostId}, Title={Title}", 
                    postId, model.Title);

                // Load existing post and SEO metadata
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return new SeoOptimizationResult
                    {
                        Success = false,
                        ErrorMessage = "Post not found"
                    };
                }

                var existingSeo = await _context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == postId);

                // Create view model for analysis
                var createViewModel = new CreatePostViewModel
                {
                    Title = model.Title,
                    Content = model.Content,
                    CommunitySlug = post.Community?.Slug ?? "",
                    PostType = model.PostType
                };

                // Run optimization (same as create, but with existing data context)
                var result = await OptimizeOnCreateAsync(createViewModel);

                // Include existing SEO data in result
                if (existingSeo != null)
                {
                    result.ExistingMetaTitle = existingSeo.MetaTitle;
                    result.ExistingMetaDescription = existingSeo.MetaDescription;
                    result.ExistingKeywords = existingSeo.Keywords?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(k => k.Trim()).ToList() ?? new();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing post during edit: PostId={PostId}", postId);
                return new SeoOptimizationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Optimize and save SEO metadata for a post
        /// </summary>
        public async Task<SeoOptimizationResult> OptimizeAndSaveAsync(int postId)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return new SeoOptimizationResult
                    {
                        Success = false,
                        ErrorMessage = "Post not found"
                    };
                }

                var model = new CreatePostViewModel
                {
                    Title = post.Title,
                    Content = post.Content ?? "",
                    CommunitySlug = post.Community?.Slug ?? "",
                    PostType = post.PostType
                };

                var result = await OptimizeOnCreateAsync(model);

                if (result.Success)
                {
                    await SaveSeoMetadataAsync(postId, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing and saving SEO for post: PostId={PostId}", postId);
                return new SeoOptimizationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Save SEO metadata to database
        /// </summary>
        public async Task SaveSeoMetadataAsync(int postId, SeoOptimizationResult result)
        {
            try
            {
                var seoMetadata = await _context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == postId);

                if (seoMetadata == null)
                {
                    seoMetadata = new SeoMetadata
                    {
                        EntityType = "post",
                        EntityId = postId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.SeoMetadata.Add(seoMetadata);
                }

                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                seoMetadata.MetaTitle = result.OptimizedTitle;
                seoMetadata.MetaDescription = result.MetaDescription;
                seoMetadata.Keywords = string.Join(", ", result.Keywords);
                seoMetadata.UpdatedAt = DateTime.UtcNow;

                // Generate canonical URL
                if (post != null && post.Community != null)
                {
                    seoMetadata.CanonicalUrl = $"/r/{post.Community.Slug}/posts/{post.Slug}";
                }

                // Generate OG and Twitter tags
                seoMetadata.OgTitle = result.OptimizedTitle;
                seoMetadata.OgDescription = result.MetaDescription;
                seoMetadata.TwitterTitle = result.OptimizedTitle;
                seoMetadata.TwitterDescription = result.MetaDescription;

                // Generate comprehensive structured data (Article schema)
                var structuredData = await GenerateArticleStructuredData(postId, result, post);

                seoMetadata.StructuredData = JsonSerializer.Serialize(structuredData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await _context.SaveChangesAsync();
                _logger.LogInformation("SEO metadata saved for post: PostId={PostId}", postId);

                // Optimize images if post exists
                if (post != null)
                {
                    try
                    {
                        var imageResult = await _imageSeoOptimizer.OptimizePostImagesAsync(postId);
                        if (imageResult.Success && imageResult.ImagesOptimized > 0)
                        {
                            _logger.LogInformation("✅ Optimized {Count} images for post {PostId}", 
                                imageResult.ImagesOptimized, postId);
                            
                            // Generate image structured data
                            await _imageStructuredDataService.GenerateImageSchemaAsync(postId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Image optimization failed for post {PostId}, continuing...", postId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving SEO metadata for post: PostId={PostId}", postId);
                throw;
            }
        }

        #region Helper Methods

        private List<string> ExtractKeywords(string title, string content)
        {
            var text = $"{title} {content}".ToLower();
            var words = text.Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '-', '(', ')', '[', ']' },
                                   StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 4)
                .ToList();

            var stopWords = new HashSet<string>
            {
                "this", "that", "these", "those", "what", "which", "who", "when",
                "where", "why", "how", "with", "from", "have", "been", "will",
                "would", "could", "should", "may", "might", "must", "can"
            };

            return words
                .Where(w => !stopWords.Contains(w))
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();
        }

        private List<string> CombineKeywords(
            List<string> initial,
            List<string> google,
            List<string> python,
            List<string> ai)
        {
            var combined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Priority order: Google (real search data) > AI > Python > Initial
            foreach (var k in google.Take(5))
                combined.Add(k);

            foreach (var k in ai.Take(3))
                combined.Add(k);

            foreach (var k in python.Take(3))
                combined.Add(k);

            foreach (var k in initial.Take(2))
                combined.Add(k);

            return combined.Take(10).ToList();
        }

        private string GenerateMetaDescription(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return title;

            // Take first 155 characters of content, or title if content is too short
            var description = content.Length > 155 
                ? content.Substring(0, 152) + "..." 
                : content;

            return description;
        }


        private List<string> CombineImprovements(List<string> python, List<string> ai)
        {
            var combined = new HashSet<string>();
            foreach (var item in python)
                combined.Add(item);
            foreach (var item in ai)
                combined.Add(item);
            return combined.ToList();
        }

        /// <summary>
        /// Optimize content structure (H1/H2/H3, keyword placement)
        /// </summary>
        private string OptimizeContentStructure(string? content, string title, List<string> keywords)
        {
            if (string.IsNullOrEmpty(content))
                return content ?? "";

            var optimized = content;
            var primaryKeyword = keywords.FirstOrDefault() ?? "";

            // Remove HTML tags for analysis
            var textOnly = Regex.Replace(content, "<[^>]+>", " ");

            // Check if H1 exists (should only be in page title, not content)
            var h1Count = Regex.Matches(content, "<h1[^>]*>", RegexOptions.IgnoreCase).Count;
            
            if (h1Count > 0)
            {
                // Remove H1 from content (H1 should be page title only)
                optimized = Regex.Replace(optimized, 
                    "<h1[^>]*>.*?</h1>", "", 
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            // Check if H2 exists
            var h2Count = Regex.Matches(optimized, "<h2[^>]*>", RegexOptions.IgnoreCase).Count;
            
            // If no H2 and content is long enough, add one
            if (h2Count == 0 && textOnly.Length > 300)
            {
                var firstSentence = textOnly.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault()?.Trim();
                
                if (!string.IsNullOrEmpty(firstSentence) && firstSentence.Length > 20)
                {
                    var h2Heading = firstSentence.Length > 60 
                        ? firstSentence.Substring(0, 57) + "..." 
                        : firstSentence;
                    
                    optimized = $"<h2>{System.Net.WebUtility.HtmlEncode(h2Heading)}</h2>\n\n{optimized}";
                }
            }

            // Ensure keyword appears in first paragraph
            if (!string.IsNullOrEmpty(primaryKeyword) && 
                textOnly.Length > 200 &&
                !textOnly.Substring(0, Math.Min(200, textOnly.Length)).ToLower().Contains(primaryKeyword.ToLower()))
            {
                // Try to add keyword naturally to first paragraph
                var firstParagraphMatch = Regex.Match(
                    optimized, @"<p[^>]*>(.*?)</p>", 
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
                
                if (firstParagraphMatch.Success)
                {
                    var firstPara = firstParagraphMatch.Groups[1].Value;
                    if (!firstPara.ToLower().Contains(primaryKeyword.ToLower()))
                    {
                        // Prepend keyword context to first paragraph
                        var enhancedPara = $"{System.Net.WebUtility.HtmlEncode(primaryKeyword)}: {firstPara}";
                        optimized = optimized.Replace(firstParagraphMatch.Value, 
                            firstParagraphMatch.Value.Replace(firstPara, enhancedPara));
                    }
                }
            }

            return optimized;
        }

        /// <summary>
        /// Analyze content structure and return improvements
        /// </summary>
        private List<string> AnalyzeContentStructure(string? content, string title, List<string> keywords)
        {
            var improvements = new List<string>();
            
            if (string.IsNullOrEmpty(content))
                return improvements;

            var textOnly = Regex.Replace(content, "<[^>]+>", " ");

            // Check H1 count (should be 0 in content, H1 is page title)
            var h1Count = Regex.Matches(content, "<h1[^>]*>", RegexOptions.IgnoreCase).Count;
            if (h1Count > 0)
            {
                improvements.Add($"Removed {h1Count} H1 tag(s) from content (H1 should be page title only)");
            }

            // Check H2 count
            var h2Count = Regex.Matches(content, "<h2[^>]*>", RegexOptions.IgnoreCase).Count;
            if (h2Count == 0 && textOnly.Length > 300)
            {
                improvements.Add("Added H2 heading for better content structure");
            }
            else if (h2Count > 0)
            {
                improvements.Add($"Content has {h2Count} H2 heading(s) - good structure");
            }

            // Check H3 count
            var h3Count = Regex.Matches(content, "<h3[^>]*>", RegexOptions.IgnoreCase).Count;
            if (h3Count > 0)
            {
                improvements.Add($"Content has {h3Count} H3 heading(s) - excellent hierarchy");
            }

            // Check keyword placement
            if (keywords.Any())
            {
                var primaryKeyword = keywords.FirstOrDefault() ?? "";
                if (!string.IsNullOrEmpty(primaryKeyword))
                {
                    var keywordInFirstPara = textOnly.Substring(0, Math.Min(200, textOnly.Length))
                        .ToLower().Contains(primaryKeyword.ToLower());
                    
                    if (keywordInFirstPara)
                    {
                        improvements.Add("Primary keyword appears in first paragraph - optimal placement");
                    }
                }
            }

            return improvements;
        }

        /// <summary>
        /// Calculate content structure score (0-100)
        /// </summary>
        private double CalculateContentStructureScore(string? content, string title)
        {
            if (string.IsNullOrEmpty(content))
                return 0;

            var score = 0.0;
            var textOnly = System.Text.RegularExpressions.Regex.Replace(content, "<[^>]+>", " ");

            // H1 check (should be 0, H1 is page title) - 20 points
            var h1Count = Regex.Matches(content, "<h1[^>]*>", RegexOptions.IgnoreCase).Count;
            score += h1Count == 0 ? 20 : 0;

            // H2 check - 30 points
            var h2Count = Regex.Matches(content, "<h2[^>]*>", RegexOptions.IgnoreCase).Count;
            if (h2Count >= 2)
                score += 30;
            else if (h2Count == 1)
                score += 20;
            else if (textOnly.Length > 300)
                score += 5; // Missing but needed

            // H3 check - 20 points
            var h3Count = Regex.Matches(content, "<h3[^>]*>", RegexOptions.IgnoreCase).Count;
            if (h3Count >= 2)
                score += 20;
            else if (h3Count == 1)
                score += 10;

            // Paragraph structure - 15 points
            var pCount = Regex.Matches(content, "<p[^>]*>", RegexOptions.IgnoreCase).Count;
            if (pCount >= 3)
                score += 15;
            else if (pCount >= 1)
                score += 10;

            // Lists - 10 points
            var listCount = Regex.Matches(content, "<[ou]l[^>]*>", RegexOptions.IgnoreCase).Count;
            if (listCount >= 1)
                score += 10;

            // Links - 5 points
            var linkCount = Regex.Matches(content, "<a[^>]*href", RegexOptions.IgnoreCase).Count;
            if (linkCount >= 1)
                score += 5;

            return Math.Min(100, score);
        }

        /// <summary>
        /// Calculate keyword density (percentage)
        /// </summary>
        private double CalculateKeywordDensity(string? content, List<string> keywords)
        {
            if (string.IsNullOrEmpty(content) || !keywords.Any())
                return 0;

            var textOnly = Regex.Replace(content, "<[^>]+>", " ").ToLower();
            
            var words = textOnly.Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '-', '(', ')', '[', ']' },
                StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2)
                .ToList();

            if (words.Count == 0)
                return 0;

            var primaryKeyword = keywords.FirstOrDefault()?.ToLower() ?? "";
            if (string.IsNullOrEmpty(primaryKeyword))
                return 0;

            var keywordCount = words.Count(w => w.Contains(primaryKeyword) || primaryKeyword.Contains(w));
            return (keywordCount / (double)words.Count) * 100;
        }

        /// <summary>
        /// Count internal links in content
        /// </summary>
        private int CountInternalLinks(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return 0;

            var linkMatches = Regex.Matches(content, 
                @"<a[^>]*href\s*=\s*[""']([^""']+)[""']", 
                RegexOptions.IgnoreCase);

            var internalLinkCount = 0;
            foreach (Match match in linkMatches)
            {
                var href = match.Groups[1].Value;
                // Check if it's an internal link (relative URL or same domain)
                if (!href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    href.Contains("/r/") || href.Contains("/posts/"))
                {
                    internalLinkCount++;
                }
            }

            return internalLinkCount;
        }

        /// <summary>
        /// Generate Article structured data (Schema.org)
        /// </summary>
        private async Task<object> GenerateArticleStructuredData(int postId, SeoOptimizationResult result, Post? post)
        {
            var baseUrl = "https://discussionspot.com"; // TODO: Get from configuration
            var articleUrl = post != null && post.Community != null
                ? $"{baseUrl}/r/{post.Community.Slug}/posts/{post.Slug}"
                : $"{baseUrl}/posts/{postId}";

            // Load post with user if not already loaded
            if (post == null || post.User == null)
            {
                post = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Media)
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);
            }

            var authorName = "DiscussionSpot User";
            if (post?.User != null && !string.IsNullOrEmpty(post.User.UserName))
            {
                authorName = post.User.UserName;
            }

            var structuredData = new
            {
                @context = "https://schema.org",
                @type = "Article",
                headline = result.OptimizedTitle,
                description = result.MetaDescription,
                image = post?.Media?.Where(m => m.MediaType == "image").Select(m => m.Url).Take(5).ToList() ?? new List<string>(),
                datePublished = post?.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                dateModified = post?.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                author = new
                {
                    @type = "Person",
                    name = authorName
                },
                publisher = new
                {
                    @type = "Organization",
                    name = "DiscussionSpot",
                    logo = new
                    {
                        @type = "ImageObject",
                        url = $"{baseUrl}/images/logo.png"
                    }
                },
                mainEntityOfPage = new
                {
                    @type = "WebPage",
                    @id = articleUrl
                },
                keywords = result.Keywords,
                articleSection = post?.Community?.Name ?? "General",
                wordCount = (result.OptimizedContent ?? "").Split(new[] { ' ', '\n', '\r', '\t' },
                    StringSplitOptions.RemoveEmptyEntries).Length,
                inLanguage = "en-US",
                // Enhanced SEO data
                optimization_source = "enhanced_seo_service",
                google_search_keywords = result.GoogleRelatedKeywords,
                top_competitors = result.TopCompetitors,
                seo_score = result.SeoScore,
                baseline_score = result.BaselineScore,
                content_structure_score = result.ContentStructureScore,
                keyword_density = result.KeywordDensity,
                internal_links_count = result.InternalLinksCount,
                improvements = result.Improvements,
                optimized_at = DateTime.UtcNow
            };

            return structuredData;
        }

        private double CalculateFinalSeoScore(
            double pythonScore,
            double aiScore,
            bool hasGoogleData,
            int keywordCount,
            string? optimizedContent)
        {
            // Weighted average: Python (35%) + AI (35%) + Google data bonus (15%) + Structure bonus (15%)
            var baseScore = (pythonScore * 0.35) + (aiScore * 0.35);
            
            // Bonus for Google Search data
            if (hasGoogleData)
                baseScore += 10;

            // Bonus for keyword count (max 5 points)
            var keywordBonus = Math.Min(keywordCount * 0.5, 5);
            baseScore += keywordBonus;

            // Bonus for content structure (max 10 points)
            if (!string.IsNullOrEmpty(optimizedContent))
            {
                var structureScore = CalculateContentStructureScore(optimizedContent, "");
                baseScore += structureScore * 0.1;
            }

            return Math.Min(100, baseScore);
        }

        #endregion
    }

    /// <summary>
    /// Comprehensive SEO optimization result
    /// </summary>
    public class SeoOptimizationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Optimized content
        public string OptimizedTitle { get; set; } = string.Empty;
        public string? OptimizedContent { get; set; }
        public string MetaDescription { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();

        // SEO scores
        public double SeoScore { get; set; }
        public double BaselineScore { get; set; }
        public double EstimatedScore { get; set; }
        public double ContentStructureScore { get; set; }
        public double KeywordDensity { get; set; }
        public int InternalLinksCount { get; set; }

        // Google Search data
        public List<string> GoogleRelatedKeywords { get; set; } = new();
        public List<string> TopCompetitors { get; set; } = new();

        // Improvements
        public List<string> Improvements { get; set; } = new();

        // Existing data (for editing)
        public string? ExistingMetaTitle { get; set; }
        public string? ExistingMetaDescription { get; set; }
        public List<string> ExistingKeywords { get; set; } = new();
    }
}

