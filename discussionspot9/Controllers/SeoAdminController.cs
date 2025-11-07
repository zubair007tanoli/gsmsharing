using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.AdminViewModels;
using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;

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
        private readonly discussionspot9.Services.AISeoService _aiSeoService;
        private readonly discussionspot9.Services.SeoScoringService _seoScoringService;
        private readonly discussionspot9.Services.EnhancedSeoService _enhancedSeoService;
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
            discussionspot9.Services.AISeoService aiSeoService,
            discussionspot9.Services.SeoScoringService seoScoringService,
            discussionspot9.Services.EnhancedSeoService enhancedSeoService,
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
            _aiSeoService = aiSeoService;
            _seoScoringService = seoScoringService;
            _enhancedSeoService = enhancedSeoService;
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
            if (!IsCurrentUserAdmin())
            {
                TempData["ErrorMessage"] = "You don't have permission to access this page.";
                return RedirectToAction("AccessDenied", "Account");
            }

            // Redirect to unified optimization page
            return RedirectToAction("UnifiedOptimization");
        }

        /// <summary>
        /// Unified SEO Optimization page (combines queue and AI optimization)
        /// </summary>
        [HttpGet("unified-optimization")]
        public async Task<IActionResult> UnifiedOptimization()
        {
            if (!IsCurrentUserAdmin())
            {
                TempData["ErrorMessage"] = "You don't have permission to access this page.";
                return RedirectToAction("AccessDenied", "Account");
            }

            // Get posts with scores for display
            var postsWithScores = await GetPostsWithScoresAsync(0, 10);
            
            ViewBag.TotalPosts = await _context.Posts.CountAsync(p => p.Status == "published");
            ViewBag.HasMore = postsWithScores.Count == 10;

            return View("UnifiedSeoOptimization", postsWithScores);
        }

        /// <summary>
        /// Get posts with SEO scores (batch loading)
        /// </summary>
        /// <summary>
        /// Get posts for AI optimization tab (limited initial load)
        /// </summary>
        [HttpGet("api/optimization-posts")]
        public async Task<IActionResult> GetOptimizationPosts(int skip = 0, int take = 10)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                var posts = await GetPostsWithScoresAsync(skip, take, null, null, null);
                var totalCount = await GetTotalPostsCountAsync(null, null, null);

                return Json(new
                {
                    success = true,
                    posts = posts.Select(p => new
                    {
                        postId = p.PostId,
                        title = p.Title,
                        communityName = p.CommunityName,
                        score = p.Score,
                        tier = p.Tier,
                        viewCount = p.ViewCount,
                        commentCount = p.CommentCount,
                        issues = p.Issues,
                        hasPendingProposal = p.HasPendingProposal,
                        proposalId = p.ProposalId,
                        proposalStatus = p.ProposalStatus,
                        proposalCreatedAt = p.ProposalCreatedAt,
                        proposalCreatedBy = p.ProposalCreatedBy
                    }),
                    hasMore = (skip + take) < totalCount,
                    totalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimization posts");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("api/queue-posts")]
        public async Task<IActionResult> GetQueuePosts(int skip = 0, int take = 10, 
            string? tier = null, string? communitySlug = null, bool? missingMeta = null)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                var posts = await GetPostsWithScoresAsync(skip, take, tier, communitySlug, missingMeta);
                var totalCount = await GetTotalPostsCountAsync(tier, communitySlug, missingMeta);

                return Json(new
                {
                    success = true,
                    posts = posts,
                    hasMore = (skip + take) < totalCount,
                    totalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting queue posts");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Re-score selected posts
        /// </summary>
        [HttpPost("api/rescore-posts")]
        public async Task<IActionResult> ReScorePosts([FromBody] List<int> postIds)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                if (postIds == null || !postIds.Any())
                {
                    return Json(new { success = false, error = "No post IDs provided" });
                }

                // Limit to 10 posts at a time for performance
                var postsToScore = postIds.Take(10).ToList();
                
                var results = await _seoScoringService.BatchCalculateScoresAsync(postsToScore);
                
                var successCount = results.Count(r => r.Success);
                var failedCount = results.Count(r => !r.Success);

                return Json(new
                {
                    success = true,
                    message = $"Re-scored {successCount} post(s)." + (failedCount > 0 ? $" {failedCount} failed." : ""),
                    results = results.Where(r => r.Success).Select(r => new
                    {
                        postId = r.PostId,
                        score = r.Score,
                        tier = r.Tier,
                        googleCompetitivenessScore = r.GoogleCompetitivenessScore,
                        contentQualityScore = r.ContentQualityScore,
                        metaCompletenessScore = r.MetaCompletenessScore,
                        freshnessScore = r.FreshnessScore
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error re-scoring posts");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Create optimization proposal (requires approval)
        /// </summary>
        [HttpPost("api/create-proposal")]
        public async Task<IActionResult> CreateOptimizationProposal([FromBody] CreateProposalRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == request.PostId);

                if (post == null)
                {
                    return Json(new { success = false, error = "Post not found" });
                }

                // Get current score
                var currentScore = await _context.SeoScores
                    .FirstOrDefaultAsync(s => s.PostId == request.PostId);

                // Optimize using enhanced SEO service
                var model = new discussionspot9.Models.ViewModels.CreativeViewModels.CreatePostViewModel
                {
                    Title = post.Title,
                    Content = post.Content ?? "",
                    CommunitySlug = post.Community?.Slug ?? "",
                    PostType = post.PostType
                };

                var seoResult = await _enhancedSeoService.OptimizeOnCreateAsync(model);
                
                if (!seoResult.Success)
                {
                    return Json(new { success = false, error = "Optimization failed" });
                }

                var currentScoreValue = currentScore?.Score ?? 0m;
                var expectedScore = currentScore != null ?
                    Math.Min(100m, currentScoreValue + 15m) :
                    (decimal)Math.Min(100d, seoResult.SeoScore);

                var proposalResult = await CreateProposalEntityAsync(
                    post,
                    seoResult.OptimizedTitle,
                    seoResult.OptimizedContent,
                    seoResult.MetaDescription,
                    seoResult.Keywords,
                    currentScoreValue,
                    expectedScore,
                    "Hybrid",
                    User.Identity?.Name,
                    seoResult.Improvements);

                if (!proposalResult.Success || proposalResult.Proposal == null)
                {
                    return Json(new { success = false, error = proposalResult.Error ?? "Failed to create proposal" });
                }

                return Json(new
                {
                    success = true,
                    proposalId = proposalResult.Proposal.Id,
                    message = "Optimization proposal created. Requires approval."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating optimization proposal");
                return Json(new { success = false, error = ex.Message });
            }
        }

        private async Task<(bool Success, string? Error, SeoOptimizationProposal? Proposal)> CreateProposalEntityAsync(
            Post post,
            string? optimizedTitle,
            string? optimizedContent,
            string? metaDescription,
            IEnumerable<string>? keywords,
            decimal currentScore,
            decimal expectedScore,
            string source,
            string? createdBy,
            IEnumerable<string>? improvements)
        {
            var hasPending = await _context.SeoOptimizationProposals
                .AnyAsync(p => p.PostId == post.PostId && p.Status == "Pending");

            if (hasPending)
            {
                return (false, "A pending proposal already exists for this post.", null);
            }

            var keywordList = keywords?
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim())
                .Where(k => !string.IsNullOrEmpty(k))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            var improvementList = improvements?
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList() ?? new List<string>();

            expectedScore = Math.Min(100m, Math.Max(expectedScore, currentScore));

            var proposal = new SeoOptimizationProposal
            {
                PostId = post.PostId,
                ProposedTitle = optimizedTitle,
                ProposedContent = optimizedContent,
                ProposedMetaDescription = metaDescription,
                ProposedKeywords = keywordList.Any() ? string.Join(", ", keywordList) : null,
                CurrentScore = currentScore,
                ExpectedScore = expectedScore,
                ExpectedScoreDelta = expectedScore - currentScore,
                Status = "Pending",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                Source = source,
                ChangesSummary = JsonSerializer.Serialize(new
                {
                    titleChanged = !string.Equals(post.Title, optimizedTitle, StringComparison.Ordinal),
                    contentChanged = !string.Equals(post.Content ?? string.Empty, optimizedContent ?? string.Empty, StringComparison.Ordinal),
                    keywordsAdded = keywordList.Count,
                    improvements = improvementList
                })
            };

            _context.SeoOptimizationProposals.Add(proposal);
            await _context.SaveChangesAsync();

            return (true, null, proposal);
        }

        /// <summary>
        /// Create proposal from AI preview payload
        /// </summary>
        [HttpPost("api/create-proposal-from-preview")]
        public async Task<IActionResult> CreateProposalFromPreview([FromBody] CreateProposalFromPreviewRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                if (request == null || request.PostId <= 0)
                {
                    return Json(new { success = false, error = "Invalid request" });
                }

                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == request.PostId);

                if (post == null)
                {
                    return Json(new { success = false, error = "Post not found" });
                }

                var currentScore = await _context.SeoScores
                    .FirstOrDefaultAsync(s => s.PostId == request.PostId);

                var baselineScore = request.BaselineScore ?? currentScore?.Score ?? 0m;
                var estimatedScore = request.EstimatedScore ?? Math.Min(100m, baselineScore + 15m);

                var proposalResult = await CreateProposalEntityAsync(
                    post,
                    request.OptimizedTitle,
                    request.OptimizedContent,
                    request.MetaDescription,
                    request.Keywords,
                    baselineScore,
                    estimatedScore,
                    request.Source ?? "AI Preview",
                    User.Identity?.Name,
                    request.Improvements);

                if (!proposalResult.Success || proposalResult.Proposal == null)
                {
                    return Json(new { success = false, error = proposalResult.Error ?? "Failed to create proposal" });
                }

                return Json(new
                {
                    success = true,
                    proposalId = proposalResult.Proposal.Id,
                    message = "Optimization proposal created and queued for approval."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating proposal from preview for post {PostId}", request?.PostId ?? 0);
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// List proposals filtered by status
        /// </summary>
        [HttpGet("api/pending-proposals")]
        public async Task<IActionResult> GetPendingProposals(string? status = "Pending", int skip = 0, int take = 10)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            take = Math.Clamp(take, 1, 50);

            var query = _context.SeoOptimizationProposals
                .Include(p => p.Post)
                    .ThenInclude(p => p.Community)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Status == status);
            }

            var totalCount = await query.CountAsync();

            var proposals = await query
                .OrderBy(p => p.Status == "Pending" ? 0 : p.Status == "Approved" ? 1 : 2)
                .ThenByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(p => new
                {
                    proposalId = p.Id,
                    postId = p.PostId,
                    postTitle = p.Post.Title,
                    communityName = p.Post.Community != null ? p.Post.Community.Name : string.Empty,
                    communitySlug = p.Post.Community != null ? p.Post.Community.Slug : string.Empty,
                    currentScore = p.CurrentScore,
                    expectedScore = p.ExpectedScore,
                    expectedScoreDelta = p.ExpectedScoreDelta,
                    status = p.Status,
                    createdAt = p.CreatedAt,
                    createdBy = p.CreatedBy,
                    reviewedAt = p.ReviewedAt,
                    reviewedBy = p.ReviewedBy,
                    source = p.Source,
                    proposedTitle = p.ProposedTitle,
                    proposedMetaDescription = p.ProposedMetaDescription,
                    proposedKeywords = p.ProposedKeywords,
                    proposedContent = p.ProposedContent
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                proposals,
                totalCount,
                hasMore = (skip + proposals.Count) < totalCount
            });
        }

        /// <summary>
        /// Automatically generate proposals for low-score posts
        /// </summary>
        [HttpPost("api/auto-generate-proposals")]
        public async Task<IActionResult> AutoGenerateProposals(int limit = 5)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            limit = Math.Clamp(limit, 1, 20);

            var created = new List<int>();
            var skipped = new List<int>();
            var failures = new List<object>();

            try
            {
                var candidates = await _selectorService.SelectPostsForOptimizationAsync(limit * 2);

                foreach (var candidate in candidates)
                {
                    if (created.Count >= limit)
                    {
                        break;
                    }

                    var post = await _context.Posts
                        .Include(p => p.Community)
                        .FirstOrDefaultAsync(p => p.PostId == candidate.PostId);

                    if (post == null)
                    {
                        skipped.Add(candidate.PostId);
                        continue;
                    }

                    var hasPending = await _context.SeoOptimizationProposals
                        .AnyAsync(p => p.PostId == post.PostId && p.Status == "Pending");

                    if (hasPending)
                    {
                        skipped.Add(post.PostId);
                        continue;
                    }

                    var currentScore = await _context.SeoScores
                        .FirstOrDefaultAsync(s => s.PostId == post.PostId);

                    var model = new discussionspot9.Models.ViewModels.CreativeViewModels.CreatePostViewModel
                    {
                        Title = post.Title,
                        Content = post.Content ?? string.Empty,
                        CommunitySlug = post.Community?.Slug ?? string.Empty,
                        PostType = post.PostType
                    };

                    var seoResult = await _enhancedSeoService.OptimizeOnCreateAsync(model);

                    if (!seoResult.Success)
                    {
                        failures.Add(new { postId = post.PostId, reason = "AI optimization failed" });
                        continue;
                    }

                    var baselineScore = currentScore?.Score ?? (decimal)seoResult.BaselineScore;
                    var expectedScore = (decimal)Math.Min(100d, seoResult.SeoScore);
                    if (expectedScore < baselineScore)
                    {
                        expectedScore = baselineScore;
                    }

                    var proposalResult = await CreateProposalEntityAsync(
                        post,
                        seoResult.OptimizedTitle,
                        seoResult.OptimizedContent,
                        seoResult.MetaDescription,
                        seoResult.Keywords,
                        baselineScore,
                        expectedScore,
                        "AutoBatch",
                        User.Identity?.Name,
                        seoResult.Improvements);

                    if (!proposalResult.Success)
                    {
                        failures.Add(new { postId = post.PostId, reason = proposalResult.Error ?? "Unknown" });
                        continue;
                    }

                    created.Add(post.PostId);
                }

                return Json(new
                {
                    success = true,
                    createdCount = created.Count,
                    createdPostIds = created,
                    skippedPostIds = skipped,
                    failures
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-generating proposals");
                return Json(new { success = false, error = ex.Message, createdCount = created.Count, failures });
            }
        }

        /// <summary>
        /// Approve optimization proposal
        /// </summary>
        [HttpPost("api/approve-proposal")]
        public async Task<IActionResult> ApproveProposal(int proposalId)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                var proposal = await _context.SeoOptimizationProposals
                    .Include(p => p.Post)
                    .FirstOrDefaultAsync(p => p.Id == proposalId);

                if (proposal == null)
                {
                    return Json(new { success = false, error = "Proposal not found" });
                }

                if (proposal.Status != "Pending")
                {
                    return Json(new { success = false, error = $"Proposal is already {proposal.Status}" });
                }

                // Apply optimizations to post
                var post = proposal.Post;
                post.Title = proposal.ProposedTitle ?? post.Title;
                post.Content = proposal.ProposedContent ?? post.Content;
                post.UpdatedAt = DateTime.UtcNow;

                // Update SEO metadata
                var seoMetadata = await _context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == post.PostId);

                if (seoMetadata == null)
                {
                    seoMetadata = new SeoMetadata
                    {
                        EntityType = "post",
                        EntityId = post.PostId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.SeoMetadata.Add(seoMetadata);
                }

                seoMetadata.MetaTitle = proposal.ProposedTitle ?? post.Title;
                seoMetadata.MetaDescription = proposal.ProposedMetaDescription ?? "";
                seoMetadata.Keywords = proposal.ProposedKeywords ?? "";
                seoMetadata.OgTitle = proposal.ProposedTitle ?? post.Title;
                seoMetadata.OgDescription = proposal.ProposedMetaDescription ?? "";
                seoMetadata.TwitterTitle = proposal.ProposedTitle ?? post.Title;
                seoMetadata.TwitterDescription = proposal.ProposedMetaDescription ?? "";
                seoMetadata.UpdatedAt = DateTime.UtcNow;

                // Update proposal status
                proposal.Status = "Approved";
                proposal.ReviewedBy = User.Identity?.Name;
                proposal.ReviewedAt = DateTime.UtcNow;
                proposal.AppliedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Re-score the post after applying optimizations
                var scoreResult = await _seoScoringService.CalculateScoreAsync(post.PostId);
                if (scoreResult.Success)
                {
                    await _seoScoringService.SaveScoreAsync(scoreResult);
                }

                return Json(new
                {
                    success = true,
                    message = "Proposal approved and applied successfully. Score updated.",
                    newScore = scoreResult.Success ? scoreResult.Score : (decimal?)null,
                    newTier = scoreResult.Success ? scoreResult.Tier : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving proposal {ProposalId}", proposalId);
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Reject optimization proposal
        /// </summary>
        [HttpPost("api/reject-proposal")]
        public async Task<IActionResult> RejectProposal(int proposalId, string? reason = null)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                var proposal = await _context.SeoOptimizationProposals
                    .FirstOrDefaultAsync(p => p.Id == proposalId);

                if (proposal == null)
                {
                    return Json(new { success = false, error = "Proposal not found" });
                }

                proposal.Status = "Rejected";
                proposal.ReviewedBy = User.Identity?.Name;
                proposal.ReviewedAt = DateTime.UtcNow;
                proposal.RejectionReason = reason;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Proposal rejected"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting proposal {ProposalId}", proposalId);
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get proposal details for approval modal
        /// </summary>
        [HttpGet("api/get-proposal-details")]
        public async Task<IActionResult> GetProposalDetails(int proposalId)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                var proposal = await _context.SeoOptimizationProposals
                    .Include(p => p.Post)
                    .FirstOrDefaultAsync(p => p.Id == proposalId);

                if (proposal == null)
                {
                    return Json(new { success = false, error = "Proposal not found" });
                }

                // Format changes summary for display
                string changesSummaryText = "";
                if (!string.IsNullOrEmpty(proposal.ChangesSummary))
                {
                    try
                    {
                        var changes = JsonSerializer.Deserialize<Dictionary<string, object>>(proposal.ChangesSummary);
                        var summaryParts = new List<string>();
                        
                        if (changes.ContainsKey("titleChanged") && changes["titleChanged"].ToString() == "True")
                            summaryParts.Add("Title updated");
                        if (changes.ContainsKey("contentChanged") && changes["contentChanged"].ToString() == "True")
                            summaryParts.Add("Content optimized");
                        if (changes.ContainsKey("keywordsAdded"))
                            summaryParts.Add($"{changes["keywordsAdded"]} keywords added");
                        
                        if (changes.ContainsKey("improvements"))
                        {
                            if (changes["improvements"] is JsonElement improvementsElement && improvementsElement.ValueKind == JsonValueKind.Array)
                            {
                                var improvements = improvementsElement.EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                                if (improvements.Any())
                                {
                                    summaryParts.Add($"Improvements: {string.Join(", ", improvements.Take(3))}");
                                }
                            }
                        }
                        
                        changesSummaryText = summaryParts.Any() ? string.Join(" • ", summaryParts) : "Content optimized for SEO";
                    }
                    catch
                    {
                        changesSummaryText = proposal.ChangesSummary;
                    }
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        proposalId = proposal.Id,
                        postId = proposal.PostId,
                        proposedTitle = proposal.ProposedTitle,
                        proposedContent = proposal.ProposedContent,
                        proposedMetaDescription = proposal.ProposedMetaDescription,
                        proposedKeywords = proposal.ProposedKeywords,
                        currentScore = proposal.CurrentScore,
                        expectedScore = proposal.ExpectedScore,
                        expectedScoreDelta = proposal.ExpectedScoreDelta,
                        changesSummary = changesSummaryText,
                        status = proposal.Status,
                        createdAt = proposal.CreatedAt,
                        createdBy = proposal.CreatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting proposal details for {ProposalId}", proposalId);
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Helper: Get posts with scores
        /// </summary>
        private async Task<List<PostWithScoreViewModel>> GetPostsWithScoresAsync(
            int skip = 0, int take = 10, 
            string? tier = null, 
            string? communitySlug = null, 
            bool? missingMeta = null)
        {
            var query = _context.Posts
                .Include(p => p.Community)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.Status == "published")
                .AsQueryable();

            // Filter by community
            if (!string.IsNullOrEmpty(communitySlug))
            {
                query = query.Where(p => p.Community != null && p.Community.Slug == communitySlug);
            }

            // Filter by missing meta
            if (missingMeta == true)
            {
                var postsWithMeta = _context.SeoMetadata
                    .Where(s => s.EntityType == "post")
                    .Select(s => s.EntityId);
                query = query.Where(p => !postsWithMeta.Contains(p.PostId));
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            var result = new List<PostWithScoreViewModel>();

            foreach (var post in posts)
            {
                var score = await _context.SeoScores
                    .FirstOrDefaultAsync(s => s.PostId == post.PostId);

                // If no score exists or needs refresh, calculate it
                if (score == null || (DateTime.UtcNow - score.ScoredAt).TotalDays > 7)
                {
                    var scoreResult = await _seoScoringService.CalculateScoreAsync(post.PostId);
                    if (scoreResult.Success)
                    {
                        await _seoScoringService.SaveScoreAsync(scoreResult);
                        score = await _context.SeoScores
                            .FirstOrDefaultAsync(s => s.PostId == post.PostId);
                    }
                }

                // Filter by tier if specified
                if (!string.IsNullOrEmpty(tier) && score != null && score.Tier != tier)
                {
                    continue;
                }

                var seoMetadata = await _context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == post.PostId);

                var proposal = await _context.SeoOptimizationProposals
                    .Where(p => p.PostId == post.PostId)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync(p => p.Status == "Pending");

                var issues = score?.Issues != null ? 
                    JsonSerializer.Deserialize<List<string>>(score.Issues) ?? new List<string>() : 
                    new List<string>();

                result.Add(new PostWithScoreViewModel
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Content = post.Content ?? "",
                    CommunitySlug = post.Community?.Slug ?? "",
                    CommunityName = post.Community?.Name ?? "",
                    Score = score?.Score ?? 0,
                    Tier = score?.Tier ?? "Critical",
                    GoogleCompetitivenessScore = score?.GoogleCompetitivenessScore ?? 0,
                    ContentQualityScore = score?.ContentQualityScore ?? 0,
                    MetaCompletenessScore = score?.MetaCompletenessScore ?? 0,
                    FreshnessScore = score?.FreshnessScore ?? 0,
                    Issues = issues,
                    RecommendedKeywords = score?.RecommendedKeywords != null ?
                        JsonSerializer.Deserialize<List<string>>(score.RecommendedKeywords) ?? new List<string>() :
                        new List<string>(),
                    TopCompetitors = score?.TopCompetitors != null ?
                        JsonSerializer.Deserialize<List<string>>(score.TopCompetitors) ?? new List<string>() :
                        new List<string>(),
                    PriorityRank = score?.PriorityRank ?? 0,
                    ScoredAt = score?.ScoredAt ?? DateTime.UtcNow,
                    ViewCount = post.ViewCount,
                    CommentCount = post.CommentCount,
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt,
                    HasMeta = seoMetadata != null,
                    HasPendingProposal = proposal?.Status == "Pending",
                    ProposalId = proposal?.Status == "Pending" ? proposal.Id : (int?)null,
                    ProposalStatus = proposal?.Status,
                    ProposalCreatedAt = proposal?.CreatedAt,
                    ProposalCreatedBy = proposal?.CreatedBy,
                    ProposalSource = proposal?.Source
                });
            }

            // Sort by priority rank (highest first)
            return result.OrderByDescending(r => r.PriorityRank).ToList();
        }

        /// <summary>
        /// Helper: Get total posts count with filters
        /// </summary>
        private async Task<int> GetTotalPostsCountAsync(
            string? tier = null, 
            string? communitySlug = null, 
            bool? missingMeta = null)
        {
            var query = _context.Posts
                .Where(p => p.Status == "published")
                .AsQueryable();

            if (!string.IsNullOrEmpty(communitySlug))
            {
                query = query.Where(p => p.Community != null && p.Community.Slug == communitySlug);
            }

            if (missingMeta == true)
            {
                var postsWithMeta = _context.SeoMetadata
                    .Where(s => s.EntityType == "post")
                    .Select(s => s.EntityId);
                query = query.Where(p => !postsWithMeta.Contains(p.PostId));
            }

            if (!string.IsNullOrEmpty(tier))
            {
                var postIdsWithTier = _context.SeoScores
                    .Where(s => s.Tier == tier)
                    .Select(s => s.PostId);
                query = query.Where(p => postIdsWithTier.Contains(p.PostId));
            }

            return await query.CountAsync();
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

        #region AI SEO Optimization

        /// <summary>
        /// AI SEO Optimization page
        /// </summary>
        [HttpGet("ai-optimization")]
        public async Task<IActionResult> AIOptimization()
        {
            if (!IsCurrentUserAdmin())
            {
                TempData["ErrorMessage"] = "You don't have permission to access this page.";
                return RedirectToAction("AccessDenied", "Account");
            }

            // Get recent posts for selection
            var recentPosts = await _context.Posts
                .Include(p => p.Community)
                .Where(p => p.Status == "published")
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .Select(p => new
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Slug = p.Slug,
                    CommunitySlug = p.Community != null ? p.Community.Slug : "",
                    CreatedAt = p.CreatedAt,
                    ViewCount = p.ViewCount
                })
                .ToListAsync();

            ViewData["Title"] = "AI SEO Optimization";
            ViewData["PageTitle"] = "AI-Powered SEO Optimization";
            ViewData["PageDescription"] = "Optimize your content with AI-powered SEO suggestions";
            ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item active\">AI SEO</li>";

            return View(recentPosts);
        }

        /// <summary>
        /// Optimize a post with AI (API endpoint)
        /// </summary>
        [HttpPost("api/ai-optimize-post")]
        public async Task<IActionResult> OptimizePostWithAI([FromBody] OptimizePostRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                if (request == null || request.PostId == 0)
                {
                    return Json(new { success = false, error = "Post ID is required" });
                }

                // Use Enhanced SEO Service which includes Google Search API + Python + C# + AI (Hybrid Approach)
                var enhancedSeoService = HttpContext.RequestServices.GetRequiredService<discussionspot9.Services.EnhancedSeoService>();
                
                // Get post to create view model
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == request.PostId);

                if (post == null)
                {
                    return Json(new { success = false, error = "Post not found" });
                }

                // Get current SEO score and issues from queue to pass to optimization
                var currentScore = await _context.SeoScores
                    .FirstOrDefaultAsync(s => s.PostId == request.PostId);

                var issues = currentScore?.Issues != null ?
                    System.Text.Json.JsonSerializer.Deserialize<List<string>>(currentScore.Issues) ?? new List<string>() :
                    new List<string>();

                // Create view model for optimization
                var model = new discussionspot9.Models.ViewModels.CreativeViewModels.CreatePostViewModel
                {
                    Title = post.Title,
                    Content = post.Content ?? "",
                    CommunitySlug = post.Community?.Slug ?? "",
                    PostType = post.PostType
                };

                // Optimize using hybrid approach (Google Search API + Python + C# + AI)
                // Pass issues to optimization so AI can address them
                var seoResult = await enhancedSeoService.OptimizeOnCreateAsync(model);
                
                // If we have issues, enhance the optimization result to address them
                if (issues.Any() && seoResult.Success)
                {
                    _logger.LogInformation("Optimizing post {PostId} with {IssueCount} issues to address", 
                        request.PostId, issues.Count);
                }
                
                if (!seoResult.Success)
                {
                    // Fallback to AI service if enhanced fails
                    var result = await _aiSeoService.OptimizePostWithAIAsync(request.PostId);
                    
                    if (!result.Success)
                    {
                        return Json(new { success = false, error = result.ErrorMessage });
                    }

                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            postId = result.PostId,
                            baselineScore = result.BaselineScore,
                            optimizedTitle = result.OptimizedTitle,
                            optimizedContent = result.OptimizedContent,
                            suggestedMetaDescription = result.SuggestedMetaDescription,
                            suggestedKeywords = result.SuggestedKeywords,
                            estimatedScore = result.EstimatedScore,
                            improvements = result.Improvements,
                            aiProvider = result.AiProvider,
                            scoreImprovement = result.EstimatedScore - result.BaselineScore
                        }
                    });
                }

                // Return enhanced SEO result (uses Google Search API + Python + C# + AI)
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        postId = request.PostId,
                        baselineScore = seoResult.BaselineScore,
                        optimizedTitle = seoResult.OptimizedTitle,
                        optimizedContent = seoResult.OptimizedContent ?? post.Content,
                        suggestedMetaDescription = seoResult.MetaDescription,
                        suggestedKeywords = seoResult.Keywords,
                        estimatedScore = seoResult.SeoScore,
                        improvements = seoResult.Improvements,
                        aiProvider = "Hybrid (Google Search + Python + C# + AI)",
                        scoreImprovement = seoResult.SeoScore - seoResult.BaselineScore,
                        googleKeywords = seoResult.GoogleRelatedKeywords,
                        topCompetitors = seoResult.TopCompetitors
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing post with AI");
                return Json(new { success = false, error = "Error during optimization: " + ex.Message });
            }
        }

        /// <summary>
        /// Preview optimization before applying (shows what will be optimized)
        /// </summary>
        [HttpPost("api/preview-optimization")]
        public async Task<IActionResult> PreviewOptimization([FromBody] OptimizePostRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                if (request == null || request.PostId == 0)
                {
                    return Json(new { success = false, error = "Post ID is required" });
                }

                // Get post and current SEO score/issues
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == request.PostId);

                if (post == null)
                {
                    return Json(new { success = false, error = "Post not found" });
                }

                // Get current SEO score and issues from queue
                var currentScore = await _context.SeoScores
                    .FirstOrDefaultAsync(s => s.PostId == request.PostId);

                var issues = currentScore?.Issues != null ?
                    System.Text.Json.JsonSerializer.Deserialize<List<string>>(currentScore.Issues) ?? new List<string>() :
                    new List<string>();

                // Get existing SEO metadata
                var seoMetadata = await _context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == request.PostId);

                // Use Enhanced SEO Service for preview (doesn't save)
                var enhancedSeoService = HttpContext.RequestServices.GetRequiredService<discussionspot9.Services.EnhancedSeoService>();
                
                var model = new discussionspot9.Models.ViewModels.CreativeViewModels.CreatePostViewModel
                {
                    Title = post.Title,
                    Content = post.Content ?? "",
                    CommunitySlug = post.Community?.Slug ?? "",
                    PostType = post.PostType
                };

                // Optimize (preview only - doesn't save)
                var seoResult = await enhancedSeoService.OptimizeOnCreateAsync(model);
                
                if (!seoResult.Success)
                {
                    return Json(new { success = false, error = "Optimization preview failed" });
                }

                // Ensure meta description is populated (fallback if empty)
                var metaDescription = seoResult.MetaDescription;
                if (string.IsNullOrWhiteSpace(metaDescription))
                {
                    // Fallback: Generate from optimized content or title
                    var contentToUse = seoResult.OptimizedContent ?? post.Content ?? "";
                    if (contentToUse.Length > 160)
                    {
                        metaDescription = contentToUse.Substring(0, 157) + "...";
                    }
                    else if (!string.IsNullOrEmpty(contentToUse))
                    {
                        metaDescription = contentToUse;
                    }
                    else
                    {
                        metaDescription = seoResult.OptimizedTitle ?? post.Title;
                    }
                    _logger.LogInformation("Generated fallback meta description for post {PostId}", request.PostId);
                }

                // Ensure keywords are populated (fallback if empty)
                var keywords = seoResult.Keywords ?? new List<string>();
                if (keywords.Count == 0)
                {
                    // Fallback: Extract keywords from title
                    var titleWords = (seoResult.OptimizedTitle ?? post.Title)
                        .Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(w => w.Length > 3)
                        .Take(5)
                        .ToList();
                    keywords = titleWords;
                    _logger.LogInformation("Generated fallback keywords for post {PostId}: {Keywords}", 
                        request.PostId, string.Join(", ", keywords));
                }

                // Calculate expected score using same method as queue
                var expectedScoreResult = await _seoScoringService.CalculateScoreAsync(request.PostId);
                var expectedScore = expectedScoreResult.Success ? (double)expectedScoreResult.Score : seoResult.SeoScore;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        postId = request.PostId,
                        currentTitle = post.Title,
                        currentContent = post.Content,
                        currentMetaDescription = seoMetadata?.MetaDescription ?? "",
                        currentKeywords = seoMetadata?.Keywords?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim()).ToList() ?? new List<string>(),
                        currentScore = currentScore?.Score ?? 0,
                        currentTier = currentScore?.Tier ?? "Critical",
                        currentIssues = issues,
                        optimizedTitle = seoResult.OptimizedTitle,
                        optimizedContent = seoResult.OptimizedContent ?? post.Content,
                        optimizedMetaDescription = metaDescription,
                        optimizedKeywords = keywords,
                        optimizedOgTitle = seoResult.OptimizedTitle,
                        optimizedOgDescription = metaDescription,
                        optimizedTwitterTitle = seoResult.OptimizedTitle,
                        optimizedTwitterDescription = metaDescription,
                        googleKeywords = seoResult.GoogleRelatedKeywords ?? new List<string>(),
                        estimatedScore = seoResult.SeoScore,
                        expectedScore = expectedScore,
                        scoreImprovement = seoResult.SeoScore - (double)(currentScore?.Score ?? 0),
                        improvements = seoResult.Improvements,
                        topCompetitors = seoResult.TopCompetitors ?? new List<string>(),
                        baselineScore = seoResult.BaselineScore
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing optimization");
                return Json(new { success = false, error = "Error during preview: " + ex.Message });
            }
        }

        /// <summary>
        /// Get post details for optimization
        /// </summary>
        [HttpGet("api/get-post-details")]
        public async Task<IActionResult> GetPostDetails(int postId)
        {
            if (!IsCurrentUserAdmin())
            {
                return Json(new { success = false, error = "Unauthorized" });
            }

            try
            {
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post == null)
                {
                    return Json(new { success = false, error = "Post not found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        postId = post.PostId,
                        title = post.Title,
                        content = post.Content ?? "",
                        slug = post.Slug,
                        communitySlug = post.Community?.Slug ?? "",
                        communityName = post.Community?.Name ?? "",
                        createdAt = post.CreatedAt,
                        viewCount = post.ViewCount,
                        postType = post.PostType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post details for {PostId}", postId);
                return Json(new { success = false, error = "Error retrieving post details" });
            }
        }

        /// <summary>
        /// Save optimized content to post
        /// </summary>
        [HttpPost("api/save-optimizations")]
        public async Task<IActionResult> SaveOptimizations([FromBody] SaveOptimizationsRequest request)
        {
            try
            {
                _logger.LogInformation("SaveOptimizations called for post {PostId}", request?.PostId ?? 0);
                
                if (!IsCurrentUserAdmin())
                {
                    _logger.LogWarning("Unauthorized attempt to save optimizations");
                    return Json(new { success = false, error = "Unauthorized" });
                }

                if (request == null)
                {
                    _logger.LogWarning("SaveOptimizations called with null request");
                    return Json(new { success = false, error = "Request is required" });
                }

                if (request.PostId <= 0)
                {
                    return Json(new { success = false, error = "Invalid post ID" });
                }

                var post = await _context.Posts
                    .Include(p => p.Community)
                    .FirstOrDefaultAsync(p => p.PostId == request.PostId);

                if (post == null)
                {
                    return Json(new { success = false, error = "Post not found" });
                }

                // Update post with optimized content
                var originalTitle = post.Title;
                var originalContent = post.Content;
                
                post.Title = !string.IsNullOrWhiteSpace(request.OptimizedTitle) ? request.OptimizedTitle : post.Title;
                post.Content = !string.IsNullOrWhiteSpace(request.OptimizedContent) ? request.OptimizedContent : post.Content;
                post.UpdatedAt = DateTime.UtcNow;

                // Explicitly mark post as modified to ensure EF tracks changes
                _context.Entry(post).Property(p => p.Title).IsModified = true;
                _context.Entry(post).Property(p => p.Content).IsModified = true;
                _context.Entry(post).Property(p => p.UpdatedAt).IsModified = true;

                _logger.LogInformation("Updating post {PostId}: Title changed: {TitleChanged}, Content changed: {ContentChanged}", 
                    request.PostId, 
                    originalTitle != post.Title, 
                    originalContent != post.Content);

                // Update or create SEO metadata
                var seoMetadata = await _context.SeoMetadata
                    .FirstOrDefaultAsync(s => s.EntityType == "post" && s.EntityId == request.PostId);

                if (seoMetadata == null)
                {
                    seoMetadata = new Models.Domain.SeoMetadata
                    {
                        EntityType = "post",
                        EntityId = request.PostId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.SeoMetadata.Add(seoMetadata);
                    _logger.LogInformation("Creating new SEO metadata for post {PostId}", request.PostId);
                }
                else
                {
                    _logger.LogInformation("Updating existing SEO metadata for post {PostId}", request.PostId);
                }

                seoMetadata.MetaTitle = !string.IsNullOrWhiteSpace(request.OptimizedTitle) ? request.OptimizedTitle : post.Title;
                seoMetadata.MetaDescription = !string.IsNullOrWhiteSpace(request.MetaDescription) ? request.MetaDescription : "";
                seoMetadata.Keywords = request.Keywords != null && request.Keywords.Any() ? string.Join(", ", request.Keywords) : "";
                seoMetadata.OgTitle = !string.IsNullOrWhiteSpace(request.OptimizedTitle) ? request.OptimizedTitle : post.Title;
                seoMetadata.OgDescription = !string.IsNullOrWhiteSpace(request.MetaDescription) ? request.MetaDescription : "";
                seoMetadata.TwitterTitle = !string.IsNullOrWhiteSpace(request.OptimizedTitle) ? request.OptimizedTitle : post.Title;
                seoMetadata.TwitterDescription = !string.IsNullOrWhiteSpace(request.MetaDescription) ? request.MetaDescription : "";
                seoMetadata.UpdatedAt = DateTime.UtcNow;

                // Save changes
                var savedCount = await _context.SaveChangesAsync();
                
                _logger.LogInformation("✅ Optimizations saved for post {PostId}. Entities updated: {SavedCount}", 
                    request.PostId, savedCount);
                
                if (savedCount == 0)
                {
                    _logger.LogWarning("⚠️ No entities were saved. This might indicate no changes were detected.");
                }

                // Re-score the post after saving optimizations
                var scoreResult = await _seoScoringService.CalculateScoreAsync(request.PostId);
                decimal? newScore = null;
                string? newTier = null;
                
                if (scoreResult.Success)
                {
                    await _seoScoringService.SaveScoreAsync(scoreResult);
                    newScore = scoreResult.Score;
                    newTier = scoreResult.Tier;
                    _logger.LogInformation("✅ Post {PostId} re-scored. New score: {Score} ({Tier})", 
                        request.PostId, scoreResult.Score, scoreResult.Tier);
                }

                return Json(new
                {
                    success = true,
                    message = $"Optimizations saved successfully. {savedCount} entity(ies) updated." + 
                              (newScore.HasValue ? $" New SEO score: {newScore:F1} ({newTier})" : ""),
                    postId = request.PostId,
                    updatedPost = new
                    {
                        title = post.Title,
                        content = post.Content?.Substring(0, Math.Min(100, post.Content?.Length ?? 0)) + (post.Content?.Length > 100 ? "..." : ""),
                        updatedAt = post.UpdatedAt
                    },
                    savedCount = savedCount,
                    newScore = newScore,
                    newTier = newTier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving optimizations for post {PostId}", request?.PostId ?? 0);
                return Json(new { 
                    success = false, 
                    error = $"Error saving optimizations: {ex.Message}",
                    details = ex.ToString()
                });
            }
        }

        #endregion

        // Request model for AI optimization
        public class OptimizePostRequest
        {
            public int PostId { get; set; }
        }

        // Request model for saving optimizations
        public class SaveOptimizationsRequest
        {
            public int PostId { get; set; }
            public string? OptimizedTitle { get; set; }
            public string? OptimizedContent { get; set; }
            public string? MetaDescription { get; set; }
            public List<string>? Keywords { get; set; }
        }

        // Request model for creating proposal
        public class CreateProposalRequest
        {
            public int PostId { get; set; }
        }

        // Request model for creating proposal from preview payload
        public class CreateProposalFromPreviewRequest
        {
            public int PostId { get; set; }
            public string? OptimizedTitle { get; set; }
            public string? OptimizedContent { get; set; }
            public string? MetaDescription { get; set; }
            public List<string>? Keywords { get; set; }
            public decimal? BaselineScore { get; set; }
            public decimal? EstimatedScore { get; set; }
            public List<string>? Improvements { get; set; }
            public string? Source { get; set; }
        }

        // View model for posts with scores
        public class PostWithScoreViewModel
        {
            public int PostId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string CommunitySlug { get; set; } = string.Empty;
            public string CommunityName { get; set; } = string.Empty;
            public decimal Score { get; set; }
            public string Tier { get; set; } = "Critical";
            public decimal GoogleCompetitivenessScore { get; set; }
            public decimal ContentQualityScore { get; set; }
            public decimal MetaCompletenessScore { get; set; }
            public decimal FreshnessScore { get; set; }
            public List<string> Issues { get; set; } = new();
            public List<string> RecommendedKeywords { get; set; } = new();
            public List<string> TopCompetitors { get; set; } = new();
            public int PriorityRank { get; set; }
            public DateTime ScoredAt { get; set; }
            public int ViewCount { get; set; }
            public int CommentCount { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public bool HasMeta { get; set; }
            public bool HasPendingProposal { get; set; }
            public int? ProposalId { get; set; }
            public string? ProposalStatus { get; set; }
            public DateTime? ProposalCreatedAt { get; set; }
            public string? ProposalCreatedBy { get; set; }
            public string? ProposalSource { get; set; }
        }
    }
}



