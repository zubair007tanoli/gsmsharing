using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.AdminViewModels;
using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers
{
    [Authorize] // Require authentication, check admin in actions
    [Route("admin/seo")]
    public class SeoAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SmartPostSelectorService _selectorService;
        private readonly MultiSiteAdSenseService _multiSiteAdsenseService;
        private readonly GoogleSearchConsoleService _searchConsoleService;
        private readonly EmailNotificationService _emailService;
        private readonly discussionspot9.Services.GoogleSearchService _googleSearchService;
        private readonly discussionspot9.Services.GoogleSearchSeoService _googleSeoService;
        private readonly discussionspot9.Services.ImageSeoOptimizer _imageSeoOptimizer;
        private readonly discussionspot9.Services.ImageStructuredDataService _imageStructuredDataService;
        private readonly ILogger<SeoAdminController> _logger;

        public SeoAdminController(
            ApplicationDbContext context,
            SmartPostSelectorService selectorService,
            MultiSiteAdSenseService multiSiteAdsenseService,
            GoogleSearchConsoleService searchConsoleService,
            EmailNotificationService emailService,
            discussionspot9.Services.GoogleSearchService googleSearchService,
            discussionspot9.Services.GoogleSearchSeoService googleSeoService,
            discussionspot9.Services.ImageSeoOptimizer imageSeoOptimizer,
            discussionspot9.Services.ImageStructuredDataService imageStructuredDataService,
            ILogger<SeoAdminController> logger)
        {
            _context = context;
            _selectorService = selectorService;
            _multiSiteAdsenseService = multiSiteAdsenseService;
            _searchConsoleService = searchConsoleService;
            _emailService = emailService;
            _googleSearchService = googleSearchService;
            _googleSeoService = googleSeoService;
            _imageSeoOptimizer = imageSeoOptimizer;
            _imageStructuredDataService = imageStructuredDataService;
            _logger = logger;
        }

        private bool IsCurrentUserAdmin()
        {
            var userEmail = User.Identity?.Name;
            return User.IsInRole("Admin") || userEmail == "zubair007tanoli@gmail.com";
        }

        [HttpGet("dashboard")]
        [HttpGet("")]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsCurrentUserAdmin())
            {
                TempData["ErrorMessage"] = "You don't have permission to access this page.";
                return RedirectToAction("AccessDenied", "Account");
            }
            
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-30);

            // Get revenue from both sites
            var totalRevenue = await _multiSiteAdsenseService.GetTotalRevenueAsync(startDate, endDate);
            var todayRevenue = await _multiSiteAdsenseService.GetTotalRevenueAsync(endDate, endDate);
            var revenueBySite = await _multiSiteAdsenseService.GetRevenueBySiteAsync(startDate, endDate);

            // Get top earning posts across all sites
            var topEarningData = await _multiSiteAdsenseService.GetTopEarningPostsAsync(10, 30);
            var topEarningPosts = new List<TopEarningPost>();

            foreach (var postData in topEarningData)
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postData.PostId);

                if (post != null)
                {
                    topEarningPosts.Add(new TopEarningPost
                    {
                        PostId = post.PostId,
                        Title = post.Title,
                        Slug = post.Slug,
                        CommunitySlug = post.Community?.Slug ?? "",
                        Earnings = postData.TotalEarnings,
                        Views = postData.TotalPageViews,
                        RPM = postData.AverageRPM,
                        SiteDomain = postData.SiteDomain
                    });
                }
            }

            var model = new DashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                RevenueBySite = revenueBySite,
                PendingOptimizations = await _context.PostSeoQueues.CountAsync(q => q.Status == "Pending"),
                CompletedOptimizations = await _context.SeoOptimizationLogs.CountAsync(l => l.OptimizedAt >= startDate),
                TotalPosts = await _context.Posts.CountAsync(p => p.Status == "published"),
                TopEarningPosts = topEarningPosts
            };

            return View(model);
        }

        [HttpGet("queue")]
        public async Task<IActionResult> OptimizationQueue()
        {
            var queue = await _context.PostSeoQueues
                .Include(q => q.Post)
                .Where(q => q.Status == "Pending" || q.Status == "Processing")
                .OrderBy(q => q.Priority)
                .ThenByDescending(q => q.EstimatedRevenueImpact)
                .ToListAsync();

            return View(queue);
        }

        [HttpGet("history")]
        public async Task<IActionResult> OptimizationHistory()
        {
            var history = await _context.SeoOptimizationLogs
                .Include(l => l.Post)
                .OrderByDescending(l => l.OptimizedAt)
                .Take(100)
                .ToListAsync();

            return View(history);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> Revenue()
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-30);

            var revenue = await _context.AdSenseRevenues
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            var topEarningPostIds = await _multiSiteAdsenseService.GetTopEarningPostsAsync(20, 30);
            var topEarningPosts = new List<discussionspot9.Models.ViewModels.AdminViewModels.TopEarningPost>();

            foreach (var postData in topEarningPostIds)
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postData.PostId);

                if (post != null)
                {
                    topEarningPosts.Add(new discussionspot9.Models.ViewModels.AdminViewModels.TopEarningPost
                    {
                        PostId = post.PostId,
                        Title = post.Title,
                        Slug = post.Slug,
                        CommunitySlug = post.Community?.Slug ?? "",
                        SiteDomain = postData.SiteDomain,
                        Earnings = postData.TotalEarnings,
                        Views = postData.TotalPageViews,
                        RPM = postData.AverageRPM
                    });
                }
            }

            var model = new RevenueViewModel
            {
                RevenueData = revenue,
                TopEarningPosts = topEarningPosts,
                TotalRevenue = revenue.Sum(r => r.Earnings),
                AvgDailyRevenue = revenue.Any() ? revenue.Average(r => r.Earnings) : 0
            };

            return View(model);
        }

        [HttpPost("approve-optimization/{id}")]
        public async Task<IActionResult> ApproveOptimization(int id)
        {
            var queueItem = await _context.PostSeoQueues.FindAsync(id);
            
            if (queueItem == null)
                return NotFound();

            queueItem.RequiresApproval = false;
            queueItem.Status = "Pending"; // Will be picked up by next optimization cycle
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Optimization approved and queued for processing";
            return RedirectToAction(nameof(OptimizationQueue));
        }

        [HttpPost("reject-optimization/{id}")]
        public async Task<IActionResult> RejectOptimization(int id)
        {
            var queueItem = await _context.PostSeoQueues.FindAsync(id);
            
            if (queueItem == null)
                return NotFound();

            queueItem.Status = "Skipped";
            queueItem.ProcessedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Optimization rejected";
            return RedirectToAction(nameof(OptimizationQueue));
        }

        [HttpPost("sync-adsense")]
        public async Task<IActionResult> SyncAdSense()
        {
            try
            {
                await _multiSiteAdsenseService.SyncAllSitesRevenueAsync();
                TempData["SuccessMessage"] = "AdSense data synced successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to sync: {ex.Message}";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("sync-search-console")]
        public async Task<IActionResult> SyncSearchConsole()
        {
            try
            {
                await _searchConsoleService.SyncAllPostsPerformanceAsync();
                TempData["SuccessMessage"] = "Search Console data synced successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to sync: {ex.Message}";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail()
        {
            try
            {
                await _emailService.SendWeeklyOptimizationSummaryAsync();
                TempData["SuccessMessage"] = "Test email sent successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to send: {ex.Message}";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("populate-queue")]
        public async Task<IActionResult> PopulateQueue(int maxPosts = 20)
        {
            try
            {
                _logger.LogInformation("📋 Manually populating SEO queue with {MaxPosts} posts...", maxPosts);

                // Step 1: Select posts for optimization
                var candidates = await _selectorService.SelectPostsForOptimizationAsync(maxPosts);

                if (!candidates.Any())
                {
                    TempData["InfoMessage"] = "No posts need optimization at this time. All posts are either recently optimized or don't meet the criteria.";
                    return RedirectToAction(nameof(OptimizationQueue));
                }

                // Step 2: Queue them
                await _selectorService.QueuePostsForOptimizationAsync(candidates);

                TempData["SuccessMessage"] = $"Successfully queued {candidates.Count} posts for SEO optimization!";
                _logger.LogInformation("✅ Successfully queued {Count} posts", candidates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error populating SEO queue");
                TempData["ErrorMessage"] = $"Failed to populate queue: {ex.Message}";
            }

            return RedirectToAction(nameof(OptimizationQueue));
        }

        [HttpGet("trending-queries")]
        public async Task<IActionResult> TrendingQueries()
        {
            var queries = await _searchConsoleService.GetTrendingQueriesAsync(7, 50);
            return View(queries);
        }

        [HttpGet("declining-pages")]
        public async Task<IActionResult> DecliningPages()
        {
            var pages = await _searchConsoleService.GetDecliningPagesAsync(14);
            return View(pages);
        }

        /// <summary>
        /// Google Keyword Research page
        /// </summary>
        [HttpGet("google-keyword-research")]
        public IActionResult GoogleKeywordResearch()
        {
            return View();
        }

        #region Image SEO Optimization

        /// <summary>
        /// Get image SEO status
        /// </summary>
        [HttpGet("api/image-seo-status")]
        public async Task<IActionResult> GetImageSeoStatus()
        {
            try
            {
                var status = await _imageSeoOptimizer.GetImageSeoStatusAsync();
                
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        totalImages = status.TotalImages,
                        imagesWithAltText = status.ImagesWithAltText,
                        imagesWithoutAltText = status.ImagesWithoutAltText,
                        imagesWithCaption = status.ImagesWithCaption,
                        optimizationPercentage = status.OptimizationPercentage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image SEO status");
                return Json(new { success = false, error = "Error retrieving image SEO status" });
            }
        }

        /// <summary>
        /// Optimize images for a specific post
        /// </summary>
        [HttpPost("api/optimize-post-images")]
        public async Task<IActionResult> OptimizePostImages(int postId)
        {
            try
            {
                var result = await _imageSeoOptimizer.OptimizePostImagesAsync(postId);
                
                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Optimized {result.ImagesOptimized} of {result.TotalImages} images",
                        data = new
                        {
                            postId = result.PostId,
                            totalImages = result.TotalImages,
                            imagesOptimized = result.ImagesOptimized,
                            failedImages = result.FailedImages,
                            keywords = result.Keywords,
                            optimizedImages = result.OptimizedImages
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, error = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing images for post {PostId}", postId);
                return Json(new { success = false, error = "Error optimizing images" });
            }
        }

        /// <summary>
        /// Batch optimize images without alt text
        /// </summary>
        [HttpPost("api/batch-optimize-images")]
        public async Task<IActionResult> BatchOptimizeImages(int limit = 100)
        {
            try
            {
                var result = await _imageSeoOptimizer.BatchOptimizeImagesWithoutAltTextAsync(limit);
                
                return Json(new
                {
                    success = true,
                    message = $"Batch optimization complete: {result.SuccessCount} successful, {result.FailureCount} failed",
                    data = new
                    {
                        totalProcessed = result.TotalImagesProcessed,
                        successCount = result.SuccessCount,
                        failureCount = result.FailureCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch image optimization");
                return Json(new { success = false, error = "Error in batch optimization" });
            }
        }

        /// <summary>
        /// Generate image structured data for a post
        /// </summary>
        [HttpPost("api/generate-image-schema")]
        public async Task<IActionResult> GenerateImageSchema(int postId)
        {
            try
            {
                var success = await _imageStructuredDataService.GenerateImageSchemaAsync(postId);
                
                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Image schema generated successfully"
                    });
                }
                else
                {
                    return Json(new { success = false, error = "Failed to generate image schema" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating image schema for post {PostId}", postId);
                return Json(new { success = false, error = "Error generating schema" });
            }
        }

        /// <summary>
        /// Get article schema with images
        /// </summary>
        [HttpGet("api/article-schema")]
        public async Task<IActionResult> GetArticleSchema(int postId)
        {
            try
            {
                var schema = await _imageStructuredDataService.GenerateArticleSchemaWithImagesAsync(postId);
                
                if (schema != null)
                {
                    return Content(schema, "application/json");
                }
                else
                {
                    return Json(new { success = false, error = "Failed to generate schema" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article schema for post {PostId}", postId);
                return Json(new { success = false, error = "Error retrieving schema" });
            }
        }

        #endregion

        #region Google Search API Integration - NEW

        /// <summary>
        /// Real-time keyword suggestions for post creation (MAIN AUTOMATION ENDPOINT)
        /// </summary>
        [HttpGet("api/suggest-keywords-realtime")]
        public async Task<IActionResult> SuggestKeywordsRealtime(string? title, string? content)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { success = false, error = "Title is required" });
            }

            try
            {
                // Get keywords from Google Search API
                var keywords = await _googleSearchService.GetRelatedKeywordsAsync(title);
                
                // Calculate quick SEO score
                int seoScore = CalculateQuickSeoScore(title, content, keywords);
                
                // Generate suggestions
                var suggestions = new List<string>();
                
                if (title.Length < 30)
                    suggestions.Add("⚠️ Title is too short. Aim for 50-60 characters.");
                else if (title.Length > 60)
                    suggestions.Add("⚠️ Title is too long. Keep it under 60 characters.");
                    
                if (string.IsNullOrWhiteSpace(content))
                    suggestions.Add("💡 Add content to improve SEO score.");
                else if (content.Length < 300)
                    suggestions.Add("📝 Content is short. Aim for at least 300 words for better SEO.");
                    
                if (keywords.Count > 0)
                {
                    var firstKeyword = keywords.First();
                    var titleLower = title.ToLower();
                    var contentLower = content?.ToLower() ?? "";
                    
                    if (!titleLower.Contains(firstKeyword.ToLower()))
                        suggestions.Add($"🎯 Consider adding '{firstKeyword}' to your title.");
                        
                    if (!string.IsNullOrWhiteSpace(content) && !contentLower.Contains(firstKeyword.ToLower()))
                        suggestions.Add($"✨ Include '{firstKeyword}' in your content for better SEO.");
                }
                else
                {
                    suggestions.Add("🔍 No related keywords found. Try a more specific title.");
                }
                
                return Json(new
                {
                    success = true,
                    keywords = keywords.Take(10).ToList(),
                    seoScore = seoScore,
                    suggestions = suggestions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in real-time keyword suggestion for title: {Title}", title);
                return Json(new { success = false, error = "Error analyzing content. Please try again." });
            }
        }

        /// <summary>
        /// Calculate quick SEO score for real-time feedback
        /// </summary>
        private int CalculateQuickSeoScore(string title, string? content, List<string> keywords)
        {
            int score = 0;

            // Title scoring (30 points)
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (title.Length >= 30 && title.Length <= 60)
                    score += 20;
                else if (title.Length >= 20 && title.Length < 70)
                    score += 10;
                    
                // Bonus for keywords in title
                if (keywords.Count > 0 && title.ToLower().Contains(keywords.First().ToLower()))
                    score += 10;
            }

            // Content scoring (40 points)
            if (!string.IsNullOrWhiteSpace(content))
            {
                if (content.Length >= 300)
                    score += 20;
                else if (content.Length >= 150)
                    score += 10;
                    
                // Keyword density
                if (keywords.Count > 0)
                {
                    var contentLower = content.ToLower();
                    var keywordCount = keywords.Take(3).Count(k => contentLower.Contains(k.ToLower()));
                    score += Math.Min(keywordCount * 7, 20);
                }
            }

            // Keywords availability (30 points)
            if (keywords.Count >= 5)
                score += 30;
            else if (keywords.Count >= 3)
                score += 20;
            else if (keywords.Count >= 1)
                score += 10;

            return Math.Min(100, score);
        }

        /// <summary>
        /// Search Google for keywords and related keywords
        /// </summary>
        [HttpGet("api/google-search")]
        public async Task<IActionResult> GoogleSearch(string query, int limit = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return Json(new { success = false, error = "Query is required" });
                }

                var result = await _googleSearchService.SearchAsync(query, limit, includeRelatedKeywords: true);
                
                if (result == null)
                {
                    return Json(new { success = false, error = "No data found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        searchTerm = result.SearchTerm,
                        results = result.Results.Select(r => new
                        {
                            position = r.Position,
                            url = r.Url,
                            title = r.Title,
                            description = r.Description
                        }),
                        relatedKeywords = result.RelatedKeywords?.Keywords.Select(k => k.Keyword).ToList(),
                        totalResults = result.Results.Count
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Google for: {Query}", query);
                return Json(new { success = false, error = "Error calling Google Search API" });
            }
        }

        /// <summary>
        /// Get topic insights from Google Search
        /// </summary>
        [HttpGet("api/topic-insights")]
        public async Task<IActionResult> GetTopicInsights(string topic)
        {
            try
            {
                if (string.IsNullOrEmpty(topic))
                {
                    return Json(new { success = false, error = "Topic is required" });
                }

                var insights = await _googleSearchService.GetTopicInsightsAsync(topic);
                
                if (!insights.Success)
                {
                    return Json(new { success = false, error = "Failed to get insights" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        topic = insights.Topic,
                        relatedKeywords = insights.RelatedKeywords,
                        topRankingDomains = insights.TopRankingDomains,
                        commonTitlePatterns = insights.CommonTitlePatterns,
                        commonDescriptionPatterns = insights.CommonDescriptionPatterns,
                        avgTitleLength = insights.AverageTitleLength,
                        avgDescriptionLength = insights.AverageDescriptionLength,
                        suggestedKeywords = insights.SuggestedKeywords
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting topic insights for: {Topic}", topic);
                return Json(new { success = false, error = "Error getting insights" });
            }
        }

        /// <summary>
        /// Optimize post with Google Search + Python AI (HYBRID)
        /// </summary>
        [HttpPost("api/google-optimize-post")]
        public async Task<IActionResult> GoogleOptimizePost(int postId)
        {
            try
            {
                var result = await _googleSeoService.OptimizePostAsync(postId);
                
                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Post optimized with Google Search + AI",
                        data = new
                        {
                            postId = result.PostId,
                            originalTitle = result.OriginalTitle,
                            optimizedTitle = result.OptimizedTitle,
                            originalKeywords = result.OriginalKeywords,
                            googleRelatedKeywords = result.GoogleRelatedKeywords,
                            optimizedKeywords = result.OptimizedKeywords,
                            seoScore = result.SeoScore,
                            metaDescription = result.MetaDescription,
                            topCompetitors = result.TopCompetitors,
                            improvementsMade = result.ImprovementsMade
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, error = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing post {PostId} with Google Search", postId);
                return Json(new { success = false, error = "Error during optimization" });
            }
        }

        /// <summary>
        /// Get competitor insights from Google Search
        /// </summary>
        [HttpGet("api/google-competitors")]
        public async Task<IActionResult> GetGoogleCompetitors(string query, int limit = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return Json(new { success = false, error = "Query is required" });
                }

                var competitors = await _googleSearchService.GetCompetitorInsightsAsync(query, limit);
                
                return Json(new
                {
                    success = true,
                    data = competitors.Select(c => new
                    {
                        position = c.Position,
                        domain = c.Domain,
                        url = c.Url,
                        title = c.Title,
                        description = c.Description,
                        titleLength = c.TitleLength,
                        descriptionLength = c.DescriptionLength
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting competitors for: {Query}", query);
                return Json(new { success = false, error = "Error getting competitors" });
            }
        }

        #endregion
    }
}



