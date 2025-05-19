using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using discussionspot.Models.ViewModels;
using discussionspot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace discussionspot.Controllers
{
    public class DiscussionPostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommunityService _communityService;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly ILogger<DiscussionPostController> _logger;

      public DiscussionPostController(
       IPostService postService,
       ICommunityService communityService,
       UserManager<ApplicationUsers> userManager,
       ILogger<DiscussionPostController> logger)
        {
            _postService = postService;
            _communityService = communityService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts(string sortBy = "hot", string timeFilter = "week", int? communityId = null)
        {
            try
            {
                var viewModel = new AllPostsViewModel
                {
                    SortBy = sortBy,
                    TimeFilter = timeFilter,
                    CurrentPage = 1,
                    PageSize = 20
                };

                if (communityId.HasValue)
                {
                    var community = await _communityService.GetCommunityByIdAsync(communityId.Value);
                    if (community == null)
                        return NotFound();

                    viewModel.Community = await _communityService.GetCommunityEntityAsync(communityId.Value);
                    viewModel.Posts = await _postService.GetPostsByCommunityAsync(communityId.Value, viewModel.CurrentPage, viewModel.PageSize);
                }
                else
                {
                    viewModel.Posts = await _postService.GetPostsAsync(sortBy, viewModel.CurrentPage, viewModel.PageSize);
                }

                // Get popular communities for sidebar
                viewModel.PopularCommunities = await _communityService.GetPopularCommunitiesAsync(5);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Post(int id)
        {
            try
            {
                var post = await _postService.GetPostByIdAsync(id);
                if (post == null)
                    return NotFound();

                // Increment view count
                // In a real app, you'd want to prevent duplicate counts from the same user
                await _postService.IncrementViewCountAsync(id);

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post with ID {PostId}", id);
                return View("Error");
            }
        }

        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> CreatePost()
        {
            try
            {
                // Create a new view model with default values
                var model = new PostCreateViewModel
                {
                    PostType = "text",
                    PollOptions = new List<string> { "", "" },
                    IsCommentAllowed = true
                };

                // Get communities from database for dropdown
                var communities = await _communityService.GetCommunitiesAsync();
                model.Communities = communities.Select(c => new SelectListItem
                {
                    Value = c.CommunityId.ToString(),
                    Text = $"r/{c.Name}"
                }).ToList();

                // Initialize the post types dropdown
                model.InitializePostTypes();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing create post form");
                return View("Error");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(PostCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Re-populate dropdowns and return view with errors
                    var communities = await _communityService.GetCommunitiesAsync();
                    model.Communities = communities.Select(c => new SelectListItem
                    {
                        Value = c.CommunityId.ToString(),
                        Text = $"r/{c.Name}"
                    }).ToList();

                    model.InitializePostTypes();
                    return View(model);
                }

                // Get current user ID
                var userId = _userManager.GetUserId(User);

                // Create the post
                var postId = await _postService.CreatePostAsync(model, userId);

                // Redirect to the new post
                return RedirectToAction(nameof(Post), new { id = postId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                ModelState.AddModelError("", "An error occurred while creating the post. Please try again.");

                // Re-populate dropdowns
                var communities = await _communityService.GetCommunitiesAsync();
                model.Communities = communities.Select(c => new SelectListItem
                {
                    Value = c.CommunityId.ToString(),
                    Text = $"r/{c.Name}"
                }).ToList();

                model.InitializePostTypes();
                return View(model);
            }
        }
    }
  }
