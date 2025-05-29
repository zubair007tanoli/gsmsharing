using discussionspot9.Interfaces;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace discussionspot9.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommunityService _communityService;
        private readonly ICommentService _commentService;
        private readonly ILogger<PostController> _logger;

        public PostController(
            IPostService postService,
            ICommunityService communityService,
            ICommentService commentService,
            ILogger<PostController> logger)
        {
            _postService = postService;
            _communityService = communityService;
            _commentService = commentService;
            _logger = logger;
        }
        public IActionResult Test()
        {
            return View();
        }

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
                    // Check if post is saved by current user
                    if (!string.IsNullOrEmpty(userId))
                    {
                        post.IsSavedByUser = await _postService.IsPostSavedByUserAsync(post.PostId, userId);
                        post.CurrentUserVote = await _postService.GetUserVoteAsync(post.PostId, userId);
                    }

                    // Set media URL and link URL based on post type
                    if (post.PostType == "image" || post.PostType == "video")
                    {
                        post.MediaUrl = post.Url; // Assuming URL contains the media URL
                    }
                    else if (post.PostType == "link")
                    {
                        post.LinkUrl = post.Url;
                        if (!string.IsNullOrEmpty(post.LinkUrl))
                        {
                            try
                            {
                                post.LinkDomain = new Uri(post.LinkUrl).Host;
                            }
                            catch
                            {
                                post.LinkDomain = "External Link";
                            }
                        }
                    }
                }

                // Get trending topics for sidebar
                ViewBag.TrendingTopics = await _postService.GetTrendingTopicsAsync();

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
            {
                return NotFound();
            }

            try
            {
                var post = await _postService.GetPostDetailsAsync(communitySlug, postSlug);
                if (post == null)
                {
                    return NotFound();
                }

                // Increment view count
                await _postService.IncrementViewCountAsync(post.PostId);

                // Get current user's vote status if logged in
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId)) // Ensure userId is not null or empty
                    {
                        post.CurrentUserVote = await _postService.GetUserVoteAsync(post.PostId, userId);
                    }
                }

                // Load comments
                var comments = await _commentService.GetPostCommentsAsync(post.PostId, "best");

                var viewModel = new PostDetailPageViewModel
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
                _logger.LogError(ex, "Error fetching post details: {CommunitySlug}/{PostSlug}",
                    communitySlug, postSlug);
                TempData["ErrorMessage"] = "An error occurred while loading the post.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Create new post - GET
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create(string communitySlug)
        {
            if (string.IsNullOrEmpty(communitySlug))
            {
                return RedirectToAction("Index", "Community");
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

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create post page");
                TempData["ErrorMessage"] = "An error occurred.";
                return RedirectToAction("Details", "Community", new { slug = communitySlug });
            }
        }

        /// <summary>
        /// Create new post - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                model.UserId = userId;

                var result = await _postService.CreatePostAsync(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Post created successfully!";
                    return RedirectToAction("Details", new
                    {
                        communitySlug = model.CommunitySlug,
                        postSlug = result.PostSlug
                    });
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create post.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the post.");
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
    }
}