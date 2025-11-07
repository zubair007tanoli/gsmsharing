using discussionspot9.Data.DbContext;
using discussionspot9.Extensions;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Models.ViewModels.PollViewModels;
using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace discussionspot9.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostTest _postTest;
        private readonly IPostService _postService;
        private readonly ICommunityService _communityService;
        private readonly ICommentService _commentService;
        private readonly ILogger<PostController> _logger;
        private readonly INotificationService _notificationService;
        private readonly ILinkMetadataService _metadataService;
        private readonly ISeoAnalyzerService _seoAnalyzerService;
        private readonly IBackgroundSeoService _backgroundSeoService;
        private readonly ApplicationDbContext _context;
        private readonly discussionspot9.Services.GoogleSearchSeoService _googleSeoService;
        private readonly discussionspot9.Services.EnhancedSeoService _enhancedSeoService;
        private readonly IStoryGenerationService _storyGenerationService;
        private readonly IReportService _reportService;
        private readonly IFileStorageService _fileStorageService;
        
        public PostController(
            IPostService postService,
            ICommunityService communityService,
            ICommentService commentService,
            ILogger<PostController> logger,
            INotificationService notificationService,
            ILinkMetadataService metadataService,
            ApplicationDbContext context,
            IPostTest postTest,
            ISeoAnalyzerService seoAnalyzerService,
            IBackgroundSeoService backgroundSeoService,
            discussionspot9.Services.GoogleSearchSeoService googleSeoService,
            discussionspot9.Services.EnhancedSeoService enhancedSeoService,
            IStoryGenerationService storyGenerationService,
            IReportService reportService,
            IFileStorageService fileStorageService)
        {
            _postService = postService;
            _communityService = communityService;
            _commentService = commentService;
            _logger = logger;
            _notificationService = notificationService;
            _metadataService = metadataService;
            _context = context;
            _postTest = postTest;
            _seoAnalyzerService = seoAnalyzerService;
            _backgroundSeoService = backgroundSeoService;
            _googleSeoService = googleSeoService;
            _enhancedSeoService = enhancedSeoService;
            _storyGenerationService = storyGenerationService;
            _reportService = reportService;
            _fileStorageService = fileStorageService;
        }
        [HttpGet]
        [Route("r/{communitySlug}/posts/{postSlug}")]
        public async Task<IActionResult> DetailTestPage(string communitySlug, string postSlug)
        {
            var postDetails = await _postService.GetPostDetailsUpdateAsync(communitySlug, postSlug);
            if (postDetails == null)
                return NotFound();
         

            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                postDetails.CurrentUserVote = await _postService.GetUserVoteAsync(postDetails.PostId, userId);
                postDetails.IsCurrentUserAuthor = postDetails.UserId == userId;
                postDetails.IsSavedByUser = await _postService.IsPostSavedByUserAsync(postDetails.PostId, userId);
            }

            var communityDetailsTask = _communityService.GetCommunityDetailsAsync(communitySlug);
            var commentsTask = _commentService.GetPostCommentsAsync(postDetails.PostId);
            //var relatedPostsTask = _postService.GetRelatedPostsAsync(postDetails.PostId, communitySlug, 3);

            await Task.WhenAll(communityDetailsTask, commentsTask);

            var model = new PostDetailPageViewModelCopy
            {
                Post = postDetails,
                CommunitySlug = communitySlug,
                PostSlug = postSlug,
                Community = communityDetailsTask.Result,
                Comments = commentsTask.Result,
                category = communityDetailsTask.Result?.CategoryName != null ? await _context.Categories.FirstOrDefaultAsync(c => c.Name == communityDetailsTask.Result.CategoryName) : null,
                CategorySlug = communityDetailsTask.Result?.CategorySlug ?? string.Empty,
                RelatedPosts = new List<PostCardViewModel>() // Will be populated later if needed
            };
            ViewBag.PostId = model.Post.PostId;
            
            // Set IsPostAuthor for comment pin functionality
            ViewData["IsPostAuthor"] = userId != null && postDetails.UserId == userId;
            
            // FIXED: Fetch link metadata whenever URL exists, regardless of PostType
            // This supports mixed content: posts can have URL + content + media + poll together
            if (!string.IsNullOrWhiteSpace(model.Post.Url))
            {
                try
                {
                    // Validate that URL is external (not pointing to localhost or the same site)
                    var uri = new Uri(model.Post.Url);
                    var requestHost = Request.Host.Host.ToLower();
                    var urlHost = uri.Host.ToLower();
                    
                    // Only fetch metadata if URL is external (not localhost or same domain)
                    if (urlHost != requestHost && 
                        urlHost != "localhost" && 
                        urlHost != "127.0.0.1" &&
                        !urlHost.StartsWith("localhost:") &&
                        !urlHost.StartsWith("127.0.0.1:"))
                    {
                        var metadata = await _metadataService.GetMetadataAsync(model.Post.Url);
                        model.Post.LinkModel = new LinkPreviewViewModel
                        {
                            Title = metadata.Title ?? string.Empty,
                            Description = metadata.Description ?? string.Empty,
                            ThumbnailUrl = metadata.ThumbnailUrl ?? string.Empty,
                            Domain = metadata.Domain ?? string.Empty,
                            Url = metadata.Url ?? string.Empty,
                            FaviconUrl = metadata.FaviconUrl ?? string.Empty,
                            ImageUrl = metadata.ImageUrl ?? string.Empty
                        };
                        _logger.LogInformation("External link metadata fetched for post {PostId}: {Url}", model.Post.PostId, model.Post.Url);
                    }
                    else
                    {
                        _logger.LogWarning("Skipping metadata fetch for internal/localhost URL: {Url}", model.Post.Url);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching link metadata for post {PostId}: {Url}", model.Post.PostId, model.Post.Url);
                    // Continue without link preview on error
                }
            }

            if (postDetails.PostType == "poll" && postDetails.HasPoll)
            {
                var pollDetails = await _postService.GetPollDetailsAsync(postDetails.PostId, userId);
                if (pollDetails != null)
                {
                    postDetails.Poll = new PollViewModel
                    {
                        PostId = postDetails.PostId,
                        Question = pollDetails.Question ?? string.Empty,
                        Options = pollDetails.Options.Select(option => new PollOptionViewModel
                        {
                            OptionText = option.OptionText,
                            VoteCount = option.VoteCount
                        }).ToList()
                    };
                }
            }

            await _postService.IncrementViewCountAsync(postDetails.PostId);

            return View(model);
        }

        // NEW: Reddit-Style Detail Page (Better than Reddit)
        [HttpGet]
        [Route("r/{communitySlug}/posts/{postSlug}/reddit")]
        public async Task<IActionResult> DetailRedditStyle(string communitySlug, string postSlug)
        {
            var postDetails = await _postService.GetPostDetailsUpdateAsync(communitySlug, postSlug);
            if (postDetails == null)
                return NotFound();

            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                postDetails.CurrentUserVote = await _postService.GetUserVoteAsync(postDetails.PostId, userId);
                postDetails.IsCurrentUserAuthor = postDetails.UserId == userId;
                postDetails.IsSavedByUser = await _postService.IsPostSavedByUserAsync(postDetails.PostId, userId);
            }

            var communityDetailsTask = _communityService.GetCommunityDetailsAsync(communitySlug);
            var commentsTask = _commentService.GetPostCommentsAsync(postDetails.PostId);

            await Task.WhenAll(communityDetailsTask, commentsTask);

            var model = new PostDetailPageViewModelCopy
            {
                Post = postDetails,
                CommunitySlug = communitySlug,
                PostSlug = postSlug,
                Community = communityDetailsTask.Result,
                Comments = commentsTask.Result,
                category = communityDetailsTask.Result?.CategoryName != null ? await _context.Categories.FirstOrDefaultAsync(c => c.Name == communityDetailsTask.Result.CategoryName) : null,
                CategorySlug = communityDetailsTask.Result?.CategorySlug ?? string.Empty,
                RelatedPosts = new List<PostCardViewModel>()
            };

            ViewBag.PostId = model.Post.PostId;

            // FIXED: Fetch link metadata whenever URL exists, regardless of PostType
            // This supports mixed content: posts can have URL + content + media + poll together
            if (!string.IsNullOrWhiteSpace(model.Post.Url))
            {
                try
                {
                    // Validate that URL is external (not pointing to localhost or the same site)
                    var uri = new Uri(model.Post.Url);
                    var requestHost = Request.Host.Host.ToLower();
                    var urlHost = uri.Host.ToLower();
                    
                    // Only fetch metadata if URL is external (not localhost or same domain)
                    if (urlHost != requestHost && 
                        urlHost != "localhost" && 
                        urlHost != "127.0.0.1" &&
                        !urlHost.StartsWith("localhost:") &&
                        !urlHost.StartsWith("127.0.0.1:"))
                    {
                        var metadata = await _metadataService.GetMetadataAsync(model.Post.Url);
                        model.Post.LinkModel = new LinkPreviewViewModel
                        {
                            Title = metadata.Title ?? string.Empty,
                            Description = metadata.Description ?? string.Empty,
                            ThumbnailUrl = metadata.ThumbnailUrl ?? string.Empty,
                            Domain = metadata.Domain ?? string.Empty,
                            Url = metadata.Url ?? string.Empty,
                            FaviconUrl = metadata.FaviconUrl ?? string.Empty,
                            ImageUrl = metadata.ImageUrl ?? string.Empty
                        };
                        _logger.LogInformation("External link metadata fetched for post {PostId}: {Url}", model.Post.PostId, model.Post.Url);
                    }
                    else
                    {
                        _logger.LogWarning("Skipping metadata fetch for internal/localhost URL: {Url}", model.Post.Url);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching link metadata for post {PostId}: {Url}", model.Post.PostId, model.Post.Url);
                    // Continue without link preview on error
                }
            }

            if (postDetails.PostType == "poll" && postDetails.HasPoll)
            {
                var pollDetails = await _postService.GetPollDetailsAsync(postDetails.PostId, userId);
                if (pollDetails != null)
                {
                    postDetails.Poll = new PollViewModel
                    {
                        PostId = postDetails.PostId,
                        Question = pollDetails.Question ?? string.Empty,
                        Options = pollDetails.Options.Select(option => new PollOptionViewModel
                        {
                            OptionText = option.OptionText,
                            VoteCount = option.VoteCount
                        }).ToList()
                    };
                }
            }

            await _postService.IncrementViewCountAsync(postDetails.PostId);

            return View(model);
        }

        //[HttpGet]
        //[Route("c/{categorySlug}/posts/{postSlug}")]
        //public async Task<IActionResult> DetailByCategory(string categorySlug, string postSlug)
        //{
        //    var postDetails = await _postService.GetPostDetailsByCategoryAsync(categorySlug, postSlug);
        //    if (postDetails == null)
        //        return NotFound();

        //    string? userId = null;
        //    if (User.Identity?.IsAuthenticated == true)
        //    {
        //        userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //        postDetails.CurrentUserVote = await _postService.GetUserVoteAsync(postDetails.PostId, userId);
        //        postDetails.IsCurrentUserAuthor = postDetails.UserId == userId;
        //        postDetails.IsSavedByUser = await _postService.IsPostSavedByUserAsync(postDetails.PostId, userId);
        //    }

        //    var categoryDetailsTask = _categoryService.GetCategoryDetailsAsync(categorySlug);
        //    var commentsTask = _commentService.GetPostCommentsAsync(postDetails.PostId);

        //    await Task.WhenAll(categoryDetailsTask, commentsTask);

        //    var model = new PostDetailPageViewModel
        //    {
        //        Post = postDetails,
        //        CategorySlug = categorySlug,
        //        PostSlug = postSlug,
        //        category = categoryDetailsTask.Result,
        //        Comments = commentsTask.Result
        //    };

        //    if (model.Post.PostType == "link")
        //    {
        //        var metadata = await _metadataService.GetMetadataAsync(model.Post.Url);
        //        model.Post.LinkModel = new LinkPreviewViewModel
        //        {
        //            Title = metadata.Title,
        //            Description = metadata.Description,
        //            ThumbnailUrl = metadata.ThumbnailUrl,
        //            Domain = metadata.Domain
        //        };
        //    }

        //    if (postDetails.PostType == "poll" && postDetails.HasPoll)
        //    {
        //        var pollDetails = await _postService.GetPollDetailsAsync(postDetails.PostId, userId);
        //        postDetails.Poll = new PollViewModel
        //        {
        //            Options = pollDetails.Options.Select(option => new PollOptionViewModel
        //            {
        //                OptionText = option.OptionText,
        //                VoteCount = option.VoteCount
        //            }).ToList()
        //        };
        //    }

        //    await _postService.IncrementViewCountAsync(postDetails.PostId);

        //    return View("DetailByCategory", model);
        //}

        public async Task<IActionResult> AllPostTestAsync(string sort = "hot", string time = "all", int page = 1)
        {
            try
            {
                var model = await _postService.GetAllPostsAsync(sort, time, page);

                ViewData["CurrentSort"] = sort;
                ViewData["CurrentTime"] = time;
                ViewData["CurrentPage"] = page;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all posts");
                TempData["ErrorMessage"] = "An error occurred while loading posts.";
                return RedirectToAction("Index", "Home");
            }
        }
        /// <summary>
        /// Display all posts with sorting options
        /// </summary>
        public async Task<IActionResult> All(string sort = "hot", string time = "all", int page = 1)
        {
            try
            {
                var model = await _postService.GetAllPostsAsync(sort, time, page);

                // Get current user ID for checking saved posts
                string? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                // Enrich post data with additional information
                foreach (var post in model.Posts)
                {
                    // Check if post is saved by current user (only if user is authenticated)
                    if (!string.IsNullOrEmpty(userId))
                    {
                        post.IsSavedByUser = await _postService.IsPostSavedByUserAsync(post.PostId, userId);
                        post.CurrentUserVote = await _postService.GetUserVoteAsync(post.PostId, userId);
                    }

                    // NOTE: MediaUrl, LinkUrl, and LinkDomain are already set by MapToPostCardViewModel
                    // in PostService which now supports mixed content properly.
                    // No need to override here - the service layer handles it correctly.
                }

                // Get trending topics for sidebar
                //ViewBag.TrendingTopics = await _postService.GetTrendingTopicsAsync();
                model.TrendingTopics = await _postService.GetTrendingTopicsAsync();

                ViewData["CurrentSort"] = sort;
                ViewData["CurrentTime"] = time;
                ViewData["CurrentPage"] = page;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all posts");
                TempData["ErrorMessage"] = "An error occurred while loading posts.";
                return RedirectToAction("Index", "Home");
            }
        }
        private string DeterminePostType(string mediaUrl)
        {
            var extension = Path.GetExtension(mediaUrl)?.ToLower();

            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
                ".mp4" or ".webm" or ".mov" => "video",
                _ => "text"
            };
        }

        // Test action for story generation
        [HttpGet]
        [Route("test-story/{postId}")]
        public async Task<IActionResult> TestStoryGeneration(int postId)
        {
            try
            {
                var storyOptions = new StoryGenerationOptions
                {
                    AutoGenerate = true,
                    UseAI = true,
                    Style = "informative",
                    Length = "medium"
                };
                
                await _storyGenerationService.QueueStoryGenerationAsync(postId, storyOptions);
                
                return Json(new { success = true, message = $"Story generation queued for post {postId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing story generation for post {PostId}", postId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Add new action for saving posts
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleSave([FromBody] ToggleSaveRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("ToggleSave called by unauthenticated user");
                    return Json(new { success = false, message = "User not authenticated" });
                }

                _logger.LogInformation($"ToggleSave called: PostId={request.PostId}, UserId={userId}");

                var result = await _postService.ToggleSavePostAsync(request.PostId, userId);

                _logger.LogInformation($"ToggleSave result: Success={result.Success}, IsSaved={result.IsSaved}, Message={result.Message}");

                return Json(new
                {
                    success = result.Success,
                    isSaved = result.IsSaved,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling save status for post {request.PostId}");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        
        public class ToggleSaveRequest
        {
            public int PostId { get; set; }
        }
        
        /// <summary>
        /// Report a post
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Report([FromBody] SubmitReportRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Report called by unauthenticated user");
                    return Json(new { success = false, message = "You must be logged in to report a post" });
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return Json(new { success = false, message = "Please select a reason for reporting" });
                }

                _logger.LogInformation($"Report submission: PostId={request.PostId}, UserId={userId}, Reason={request.Reason}");

                var result = await _reportService.CreateReportAsync(
                    request.PostId,
                    userId,
                    request.Reason,
                    request.Details
                );

                if (result.Success)
                {
                    _logger.LogInformation($"Report created successfully for post {request.PostId}");
                    return Json(new
                    {
                        success = true,
                        message = "Thank you for your report. Our moderation team will review it shortly."
                    });
                }

                _logger.LogWarning($"Failed to create report: {result.ErrorMessage}");
                return Json(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting report for post {request.PostId}");
                return Json(new { success = false, message = "An error occurred while submitting your report. Please try again." });
            }
        }
        
        /// <summary>
        /// Display single post with comments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(string communitySlug, string postSlug)
        {
            if (string.IsNullOrEmpty(communitySlug) || string.IsNullOrEmpty(postSlug))
                return NotFound();

            try
            {
                var post = await _postService.GetPostDetailsAsync(communitySlug, postSlug);
                if (post == null)
                    return NotFound();

                // Increment view count
                await _postService.IncrementViewCountAsync(post.PostId);

                string? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        post.CurrentUserVote = await _postService.GetUserVoteAsync(post.PostId, userId);
                        post.IsCurrentUserAuthor = (post.UserId == userId);
                        post.IsSavedByUser = await _postService.IsPostSavedByUserAsync(post.PostId, userId);
                    }
                }

                // Load poll data if it's a poll post
                if (post.PostType == "poll" && post.HasPoll)
                {
                    var pollDetails = await _postService.GetPollDetailsAsync(post.PostId, userId);
                    if (pollDetails != null)
                    {
                        post.Poll = new PollViewModel
                        {
                            Options = pollDetails.Options.Select(option => new PollOptionViewModel
                            {
                                OptionText = option.OptionText,
                                VoteCount = option.VoteCount
                            }).ToList()
                        };
                    }
                }

                // Load awards
                post.Awards = await _postService.GetPostAwardsAsync(post.PostId);

                // Load comments
                var comments = await _commentService.GetPostCommentsAsync(post.PostId, "best");

                var viewModel = new PostDetailPageViewModelCopy
                {
                    Post = post,
                    Comments = comments,
                    CommunitySlug = communitySlug,
                    PostSlug = postSlug,
                    category = post.CommunityId > 0 ? await _context.Communities
                        .Where(c => c.CommunityId == post.CommunityId)
                        .Select(c => c.Category)
                        .FirstOrDefaultAsync() : null,
                    CategorySlug = string.Empty,
                    RelatedPosts = new List<PostCardViewModel>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching post details: {CommunitySlug}/{PostSlug}", communitySlug, postSlug);
                TempData["ErrorMessage"] = "An error occurred while loading the post.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Add new actions for polls and awards
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> VotePoll(int postId, List<int> optionIds)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("VotePoll called by unauthenticated user");
                    return Json(new { success = false, message = "Please log in to vote" });
                }

                _logger.LogInformation("User {UserId} voting on poll {PostId} with options: {OptionIds}", 
                    userId, postId, string.Join(", ", optionIds));

                var result = await _postService.VotePollAsync(postId, userId, optionIds);
                
                if (result.Success)
                {
                    // Get updated poll results for real-time update
                    var pollDetails = await _postService.GetPollDetailsAsync(postId, userId);
                    
                    var pollResults = pollDetails.Options.Select(opt => new
                    {
                        optionId = opt.PollOptionId,
                        voteCount = opt.VoteCount,
                        percentage = opt.VotePercentage
                    }).ToList();
                    
                    _logger.LogInformation("Vote successful. Returning updated results for poll {PostId}", postId);
                    
                    return Json(new
                    {
                        success = true,
                        message = "Vote recorded successfully!",
                        pollResults = pollResults,
                        totalVotes = pollDetails.TotalVotes
                    });
                }
                
                _logger.LogWarning("Vote failed for poll {PostId}: {Message}", postId, result.Message);
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on poll");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GiveAward(int postId, int awardId, string? message)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "User not authenticated" });

                var result = await _postService.GiveAwardAsync(postId, awardId, userId, message);

                // Create notification for post author
                if (result.Success)
                {
                    await _notificationService.CreateAwardNotificationAsync(postId, awardId, userId);
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error giving award");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Create new post - GET
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create(string? communitySlug = null, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(communitySlug))
            {
                // If no community specified, show community selection page
                return await CreateTest(communitySlug, returnUrl);
            }

            try
            {
                var community = await _communityService.GetCommunityBySlugAsync(communitySlug);
                if (community == null)
                {
                    return NotFound();
                }

                var model = new CreatePostViewModel
                {
                    CommunityId = community.CommunityId,
                    CommunityName = community.Name,
                    CommunitySlug = communitySlug
                };

                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create post page");
                TempData["ErrorMessage"] = "An error occurred.";
                return RedirectToAction("Details", "Community", new { slug = communitySlug });
            }
        }


        [HttpGet]
        [Authorize]
        [Route("create")]
        [Route("r/{communitySlug}/create")]
        public async Task<IActionResult> CreateTest(string? communitySlug, string? returnUrl = null)
        {
            // If no community specified, show enhanced create page with community selection
            if (string.IsNullOrEmpty(communitySlug))
            {
                var model = new CreatePostViewModel();
                model.CommunitySlug = communitySlug;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    model.UserCommunities = await _communityService.GetUserJoinedCommunitiesAsync(userId);
                    model.SuggestedCommunities = await _communityService.GetSuggestedCommunitiesAsync(userId);
                }
                else
                {
                    model.UserCommunities = new List<CommunityViewModel>();
                    model.SuggestedCommunities = await _communityService.GetSuggestedCommunitiesAsync(string.Empty);
                }
                
                // Set SEO metadata for enhanced create page
                ViewData["Title"] = "Create Post - DiscussionSpot";
                ViewData["Description"] = "Create engaging posts and automatically generate Web Stories for better SEO and reach";
                ViewData["Keywords"] = "create post, web stories, SEO, content creation, community";
                ViewData["ReturnUrl"] = returnUrl;
                
                return View("Create", model); // Use the enhanced Create view
            }
            
            // If community specified, use the original CreateTest logic
            var modelWithCommunity = new CreatePostViewModel();
            modelWithCommunity.CommunitySlug = communitySlug;
            var userIdWithCommunity = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdWithCommunity))
            {
                modelWithCommunity.UserCommunities = await _communityService.GetUserJoinedCommunitiesAsync(userIdWithCommunity);
                modelWithCommunity.SuggestedCommunities = await _communityService.GetSuggestedCommunitiesAsync(userIdWithCommunity);
            }
            else
            {
                modelWithCommunity.SuggestedCommunities = await _communityService.GetSuggestedCommunitiesAsync(string.Empty);
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            return View(modelWithCommunity);
        }

        //[HttpGet]
        //public IActionResult CreateTest(string communitySlug)
        //{
        //    CreatePostViewModel createPostView = new CreatePostViewModel();
            
        //    if (!string.IsNullOrEmpty(communitySlug))
        //    {
        //        createPostView.CommunitySlug = communitySlug;
        //    }
       
        //    return View(createPostView);
        //}

        /// <summary>
        /// Create new post - POST Working
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreatePostViewModel model, string? returnUrl = null)
        {
            // CRITICAL: Log received data to verify content is being sent
            _logger.LogInformation("🚀 === POST CREATION DEBUG ===");
            _logger.LogInformation("Title: {Title}", model.Title);
            _logger.LogInformation("PostType: {PostType}", model.PostType);
            _logger.LogInformation("Content length: {Length}", model.Content?.Length ?? 0);
            _logger.LogInformation("Content preview: {Preview}", model.Content?.Substring(0, Math.Min(200, model.Content?.Length ?? 0)) ?? "NULL");
            _logger.LogInformation("Has Content: {HasContent}", !string.IsNullOrWhiteSpace(model.Content));
            _logger.LogInformation("MediaFiles: {Count}", model.MediaFiles?.Count ?? 0);
            if (model.MediaFiles != null && model.MediaFiles.Count > 0)
            {
                foreach (var file in model.MediaFiles)
                {
                    _logger.LogInformation("  📁 File: {Name}, Size: {Size} bytes, Type: {Type}", 
                        file.FileName, file.Length, file.ContentType);
                }
            }
            _logger.LogInformation("MediaUrls: {Count}", model.MediaUrls?.Count ?? 0);
            if (model.MediaUrls != null && model.MediaUrls.Count > 0)
            {
                foreach (var url in model.MediaUrls)
                {
                    _logger.LogInformation("  🔗 URL: {Url}", url);
                }
            }
            _logger.LogInformation("URL field: {Url}", model.Url);
            _logger.LogInformation("PollOptions: {Count}", model.PollOptions?.Count ?? 0);
            _logger.LogInformation("Tags: {Tags}", model.TagsInput);
            _logger.LogInformation("Meta Keywords: {Keywords}", model.Keywords);
            _logger.LogInformation("================================");
            
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    var key = state.Key;
                    var errors = state.Value.Errors;

                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation error on '{key}': {error.ErrorMessage}");
                    }
                }
                
                // Preserve return URL on validation error
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("CreateTest", new { communitySlug = model.CommunitySlug, returnUrl });
                }
                return RedirectToAction("CreateTest", new { communitySlug = model.CommunitySlug });
            }
            
            try
            {                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                model.UserId = userId;
                
                // Step 1: Optimize SEO before creating post
                var seoResult = await _enhancedSeoService.OptimizeOnCreateAsync(model);
                if (seoResult.Success)
                {
                    // Apply optimized values to model
                    model.Title = seoResult.OptimizedTitle;
                    if (!string.IsNullOrEmpty(seoResult.OptimizedContent))
                        model.Content = seoResult.OptimizedContent;
                    model.Keywords = string.Join(", ", seoResult.Keywords);
                    model.MetaDescription = seoResult.MetaDescription;
                    
                    _logger.LogInformation("✅ SEO optimization completed. Score: {Score}/100", seoResult.SeoScore);
                }
                else
                {
                    _logger.LogWarning("SEO optimization failed: {Error}", seoResult.ErrorMessage);
                }
                
                // Step 2: Create post with optimized content
                var result = await _postTest.CreatePostUpdatedAsync(model);
                //var result = await _postService.CreatePostUpdatedAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Post created successfully!";
                    
                    // FIXED: Store values BEFORE starting background tasks
                    // Don't query the DbContext after background tasks start!
                    var postIdForBackground = result.PostId; // Get from result instead of querying DB
                    var communityIdForBackground = model.CommunityId;
                    var postSlugForRedirect = result.PostSlug;
                    var communitySlugForRedirect = model.CommunitySlug;
                    
                    // Step 3: Save SEO metadata (already optimized above)
                    if (seoResult.Success)
                    {
                        await _enhancedSeoService.SaveSeoMetadataAsync(postIdForBackground, seoResult);
                    }
                    else
                    {
                        // Fallback: Queue background SEO if real-time optimization failed
                        _backgroundSeoService.ProcessPostSeoAsync(postIdForBackground, model, communityIdForBackground);
                    }
                    
                    _logger.LogInformation("✅ Post {PostId} created successfully with SEO optimization", postIdForBackground);
                    
                    // Redirect to return URL if provided and valid, otherwise go to post details
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Details", new
                    {
                        communitySlug = communitySlugForRedirect,
                        postSlug = postSlugForRedirect
                    });
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create post.");
                
                // Reload communities for dropdown
                if (!string.IsNullOrEmpty(userId))
                {
                    model.UserCommunities = await _communityService.GetUserJoinedCommunitiesAsync(userId);
                    model.SuggestedCommunities = await _communityService.GetSuggestedCommunitiesAsync(userId);
                }
                
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the post.");
                
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }
        }

        /// <summary>
        /// Handle post voting (AJAX)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Vote(int postId, int voteType)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _postService.VotePostAsync(postId, userId, voteType);

                if (result.Success)
                {
                    var voteCount = await _postService.GetPostVoteCountAsync(postId);
                    return Json(new
                    {
                        success = true,
                        voteCount = voteCount,
                        userVote = result.UserVote
                    });
                }

                return Json(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on post");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Handle post sharing (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Share(int postId, string platform)
        {
            try
            {
                await _postService.IncrementShareCountAsync(postId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing post");
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Edit post - GET (only for post owner)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("=== EDIT POST GET - PostID: {PostId} ===", id);
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated - redirecting to login");
                    return RedirectToAction("Login", "Account");
                }

                // Get the post with all details
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .Include(p => p.Media)
                    .Include(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefaultAsync(p => p.PostId == id);
                
                if (post == null)
                {
                    _logger.LogWarning("Post not found: {PostId}", id);
                    return NotFound();
                }

                // Check ownership
                if (post.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to edit post {PostId} owned by {OwnerId}", 
                        userId, id, post.UserId);
                    return Forbid();
                }

                // Convert to edit view model
                var viewModel = new EditPostViewModel
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Content = post.Content,
                    PostType = post.PostType,
                    Url = post.Url,
                    CommunityId = post.CommunityId,
                    CommunitySlug = post.Community?.Slug,
                    Status = post.Status,
                    IsNSFW = post.IsNSFW,
                    IsSpoiler = post.IsSpoiler,
                    IsPinned = post.IsPinned,
                    AllowComments = !post.IsLocked,
                    TagsInput = string.Join(", ", post.PostTags.Select(pt => pt.Tag.Name)),
                    ExistingMediaUrls = post.Media.Select(m => m.Url).ToList(),
                    ImageAction = "keep" // Default to keeping existing images
                };

                _logger.LogInformation("✅ Loaded post for editing: {Title}", viewModel.Title);
                
                // Pass slug to view for back button
                ViewBag.PostSlug = post.Slug;
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit page for post {PostId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the post for editing.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Edit post - POST (only for post owner)
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPostViewModel model)
        {
            try
            {
                _logger.LogInformation("=== EDIT POST POST - PostID: {PostId} ===", id);
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Verify ownership
                var post = await _context.Posts
                    .Include(p => p.Community)
                    .Include(p => p.Media)
                    .Include(p => p.PostTags)
                    .FirstOrDefaultAsync(p => p.PostId == id);
                
                if (post == null)
                {
                    return NotFound();
                }

                if (post.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to edit post {PostId} owned by {OwnerId}", 
                        userId, id, post.UserId);
                    return Forbid();
                }

                // Step 1: Optimize SEO before updating post
                var seoResult = await _enhancedSeoService.OptimizeOnEditAsync(model, id);
                if (seoResult.Success)
                {
                    // Apply optimized values
                    post.Title = seoResult.OptimizedTitle;
                    if (!string.IsNullOrEmpty(seoResult.OptimizedContent))
                        post.Content = seoResult.OptimizedContent;
                    post.UpdatedAt = DateTime.UtcNow;
                    
                    // Save SEO metadata
                    await _enhancedSeoService.SaveSeoMetadataAsync(id, seoResult);
                    
                    _logger.LogInformation("✅ SEO optimization completed for post {PostId}. Score: {Score}/100", 
                        id, seoResult.SeoScore);
                }
                else
                {
                    // Fallback: Update without SEO optimization
                    post.Title = model.Title;
                    post.Content = model.Content;
                    post.UpdatedAt = DateTime.UtcNow;
                    _logger.LogWarning("SEO optimization failed for post {PostId}: {Error}", id, seoResult.ErrorMessage);
                }
                
                // Update other post fields
                post.PostType = model.PostType;
                post.Url = model.Url;
                post.Status = model.Status ?? "published";
                post.IsNSFW = model.IsNSFW;
                post.IsSpoiler = model.IsSpoiler;
                post.IsPinned = model.IsPinned;
                post.IsLocked = !model.AllowComments;

                // Handle image actions
                if (model.ImageAction == "remove")
                {
                    _logger.LogInformation("🗑️ Removing all media for post {PostId}", id);
                    var existingMedia = await _context.Media.Where(m => m.PostId == id).ToListAsync();
                    _context.Media.RemoveRange(existingMedia);
                }
                else if (model.ImageAction == "replace" && model.MediaFiles != null && model.MediaFiles.Any())
                {
                    _logger.LogInformation("📸 Replacing media for post {PostId}", id);
                    
                    // Remove old media
                    var existingMedia = await _context.Media.Where(m => m.PostId == id).ToListAsync();
                    _context.Media.RemoveRange(existingMedia);
                    
                    // Add new media (handled below)
                }
                // If "keep", don't touch existing media

                // Handle new media files if provided
                if (model.ImageAction == "replace" && model.MediaFiles != null && model.MediaFiles.Any())
                {
                    foreach (var file in model.MediaFiles)
                    {
                        try
                        {
                            var mediaType = file.ContentType.StartsWith("image/") ? "image" : "video";
                            var folder = mediaType == "image" ? "posts/images" : "posts/videos";
                            var savedUrl = await _fileStorageService.SaveFileAsync(file, folder);
                            
                            var media = new Media
                            {
                                PostId = post.PostId,
                                Url = savedUrl,
                                MediaType = mediaType,
                                ContentType = file.ContentType,
                                FileName = file.FileName,
                                FileSize = file.Length,
                                UploadedAt = DateTime.UtcNow,
                                UserId = userId
                            };
                            _context.Media.Add(media);
                            _logger.LogInformation("✅ New media added: {Url}", savedUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error uploading media file: {FileName}", file.FileName);
                            // Continue with other files
                        }
                    }
                }

                // Handle media URLs if provided
                if (model.MediaUrls != null && model.MediaUrls.Any())
                {
                    foreach (var url in model.MediaUrls)
                    {
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            var media = new Media
                            {
                                PostId = post.PostId,
                                Url = url,
                                MediaType = "image",
                                UploadedAt = DateTime.UtcNow,
                                UserId = userId
                            };
                            _context.Media.Add(media);
                        }
                    }
                }

                // Update tags
                if (!string.IsNullOrEmpty(model.TagsInput))
                {
                    // Remove existing tags
                    var existingPostTags = await _context.PostTags
                        .Where(pt => pt.PostId == id)
                        .ToListAsync();
                    _context.PostTags.RemoveRange(existingPostTags);

                    // Add new tags
                    var tagNames = model.TagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim().ToLower())
                        .Distinct();

                    foreach (var tagName in tagNames)
                    {
                        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                        if (tag == null)
                        {
                            tag = new Tag
                            {
                                Name = tagName,
                                Slug = tagName.ToSlug(),
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Tags.Add(tag);
                            await _context.SaveChangesAsync();
                        }

                        _context.PostTags.Add(new PostTag
                        {
                            PostId = post.PostId,
                            TagId = tag.TagId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Post updated successfully: {PostId}", post.PostId);
                
                TempData["SuccessMessage"] = "Post updated successfully!";
                
                return RedirectToAction("DetailTestPage", new { 
                    communitySlug = post.Community?.Slug, 
                    postSlug = post.Slug 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the post.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Delete post (only for post owner or moderators)
        /// Supports both JSON (AJAX) and form submissions
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] DeletePostRequest? request, int? postId, string? returnUrl = null)
        {
            try
            {
                _logger.LogInformation("=== DELETE POST - Request: {Request}, PostId: {PostId} ===", request, postId);
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    var errorMsg = "You must be logged in to delete a post.";
                    
                    // Return JSON for AJAX requests
                    if (Request.ContentType?.Contains("application/json") == true)
                    {
                        return Json(new { success = false, message = errorMsg });
                    }
                    
                    TempData["ErrorMessage"] = errorMsg;
                    return RedirectToAction("Index", "Home");
                }
                
                // Get postId from either JSON body or form parameter
                var postIdToDelete = request?.PostId ?? postId;
                
                if (!postIdToDelete.HasValue)
                {
                    _logger.LogWarning("No postId provided in delete request");
                    return Json(new { success = false, message = "Post ID is required" });
                }
                
                _logger.LogInformation("Deleting post {PostId} by user {UserId}", postIdToDelete, userId);
                
                var result = await _postService.DeletePostAsync(postIdToDelete.Value, userId);

                // Return JSON for AJAX requests
                if (Request.ContentType?.Contains("application/json") == true || request != null)
                {
                    return Json(new { 
                        success = result.Success, 
                        message = result.Success ? "Post deleted successfully" : result.ErrorMessage 
                    });
                }

                // Traditional form submission
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Post deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete post.";
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post");
                
                // Return JSON for AJAX requests
                if (Request.ContentType?.Contains("application/json") == true || request != null)
                {
                    return Json(new { success = false, message = "An error occurred while deleting the post" });
                }
                
                TempData["ErrorMessage"] = "An error occurred while deleting the post.";
                return RedirectToAction("Index", "Home");
            }
        }
        
        public class DeletePostRequest
        {
            public int PostId { get; set; }
        }

        private async Task AddTagsToPostAsync(Post post, IEnumerable<string> tagNames)
        {
            foreach (var tagName in tagNames)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (tag == null)
                {
                    tag = new Tag
                    {
                        Name = tagName,
                        Slug = tagName.ToSlug(),
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync(); // Ensure TagId is generated
                }

                _context.PostTags.Add(new PostTag
                {
                    PostId = post.PostId,
                    TagId = tag.TagId
                });
            }
        }
    }
}