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
        private readonly IStoryGenerationService _storyGenerationService;
        
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
            IStoryGenerationService storyGenerationService)
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
            _storyGenerationService = storyGenerationService;
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
                //RelatedPosts = relatedPostsTask.Result
            };
            ViewBag.PostId = model.Post.PostId;
            if (model.Post.PostType == "link")
            {
                var metadata = await _metadataService.GetMetadataAsync(model.Post.Url);
                model.Post.LinkModel = new LinkPreviewViewModel
                {
                    Title = metadata.Title,
                    Description = metadata.Description,
                    ThumbnailUrl = metadata.ThumbnailUrl,
                    Domain = metadata.Domain
                };

            }

            if (postDetails.PostType == "poll" && postDetails.HasPoll)
            {
                var pollDetails = await _postService.GetPollDetailsAsync(postDetails.PostId, userId);
                if (pollDetails != null)
                {
                    postDetails.Poll = new PollViewModel
                    {
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
            };

            ViewBag.PostId = model.Post.PostId;

            if (model.Post.PostType == "link")
            {
                var metadata = await _metadataService.GetMetadataAsync(model.Post.Url);
                model.Post.LinkModel = new LinkPreviewViewModel
                {
                    Title = metadata.Title,
                    Description = metadata.Description,
                    ThumbnailUrl = metadata.ThumbnailUrl,
                    Domain = metadata.Domain
                };
            }

            if (postDetails.PostType == "poll" && postDetails.HasPoll)
            {
                var pollDetails = await _postService.GetPollDetailsAsync(postDetails.PostId, userId);
                if (pollDetails != null)
                {
                    postDetails.Poll = new PollViewModel
                    {
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

                    // Set media URL and link URL based on post type
                    if (post.PostType == "image" || post.PostType == "video")
                    {
                        post.MediaUrl = post.Url;
                    }
                    else if (post.PostType == "link" && !string.IsNullOrEmpty(post.Url))
                    {
                        post.LinkUrl = post.Url;
                        try
                        {
                            var uri = new Uri(post.Url);
                            post.LinkDomain = uri.Host;
                        }
                        catch
                        {
                            post.LinkDomain = "External Link";
                        }
                    }
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
        public async Task<IActionResult> ToggleSave(int postId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _postService.ToggleSavePostAsync(postId, userId);

                return Json(new
                {
                    success = result.Success,
                    isSaved = result.IsSaved,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling save status");
                return Json(new { success = false, message = "An error occurred" });
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
                    PostSlug = postSlug
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
                    return Json(new { success = false, message = "User not authenticated" });

                var result = await _postService.VotePollAsync(postId, userId, optionIds);
                return Json(result);
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
            _logger.LogInformation("MediaUrls: {Count}", model.MediaUrls?.Count ?? 0);
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
                
                // Create post immediately without waiting for SEO
                var result = await _postTest.CreatePostUpdatedAsync(model);
                //var result = await _postService.CreatePostUpdatedAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Post created successfully!";
                    
                    // Get the post ID for background SEO processing
                    var post = await _context.Posts
                        .FirstOrDefaultAsync(p => p.Slug == result.PostSlug && p.CommunityId == model.CommunityId);
                    
                    if (post != null)
                    {
                        // AUTO-OPTIMIZATION: Run Google Search + AI SEO in background
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                // Optimize with Google Search API + Python AI
                                var seoResult = await _googleSeoService.OptimizePostAsync(post.PostId);
                                
                                if (seoResult.Success)
                                {
                                    _logger.LogInformation(
                                        "Auto-optimized post {PostId}: SEO Score {Score}/100, Keywords: {Keywords}",
                                        post.PostId, seoResult.SeoScore, string.Join(", ", seoResult.OptimizedKeywords));
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Background SEO optimization failed for post {PostId}", post.PostId);
                            }
                        });
                        
                        // Also run legacy background SEO service
                        _backgroundSeoService.ProcessPostSeoAsync(post.PostId, model, model.CommunityId);
                    }
                    
                    // Redirect to return URL if provided and valid, otherwise go to post details
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Details", new
                    {
                        communitySlug = model.CommunitySlug,
                        postSlug = result.PostSlug
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
        /// Delete post (only for post owner or moderators)
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int postId, string? returnUrl = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "You must be logged in to delete a post.";
                    return RedirectToAction("Index", "Home");
                }
                var result = await _postService.DeletePostAsync(postId, userId);

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
                TempData["ErrorMessage"] = "An error occurred while deleting the post.";
                return RedirectToAction("Index", "Home");
            }
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