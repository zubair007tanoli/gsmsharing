using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using discussionspot.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace discussionspot.Controllers
{
    public class DiscussionPostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly IPostService _postService;
        private readonly ICommunityService _communityService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DiscussionPostController(
            ApplicationDbContext context,
            UserManager<ApplicationUsers> userManager,
            IPostService postService,
            ICommunityService communityService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _postService = postService;
            _communityService = communityService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Gets all posts with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPosts(string sortBy = "hot", string timeFilter = "week", int? communityId = null)
        {
            try
            {
                // Get posts with filters
                var posts = await _postService.GetPostsAsync(sortBy, timeFilter, communityId);

                // Get communities for sidebar
                var popularCommunities = await _communityService.GetPopularCommunitiesAsync(10);

                // Create view model
                var viewModel = new AllPostsViewModel
                {
                    Posts = posts,
                    SortBy = sortBy,
                    TimeFilter = timeFilter,
                    CommunityId = communityId,
                    PopularCommunities = popularCommunities,
                    Categories = await _communityService.GetCategoriesAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                return View("Error", new ErrorViewModel
                {
                    RequestId = HttpContext.TraceIdentifier,
                    ErrorMessage = "An error occurred while fetching posts."
                });
            }
        }

        /// <summary>
        /// Displays a single post with comments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Post(int id)
        {
            try
            {
                // Get post by ID
                var post = await _postService.GetPostByIdAsync(id);

                if (post == null)
                {
                    return NotFound();
                }

                // Get comments for the post
                var comments = await _postService.GetCommentsForPostAsync(id);

                // Check if user has voted on this post
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                bool? userVote = null;

                if (!string.IsNullOrEmpty(userId))
                {
                    userVote = await _postService.GetUserVoteForPostAsync(id, userId);
                }

                // Create view model
                var viewModel = new PostDetailViewModel
                {
                    Post = post,
                    Comments = comments,
                    UserVote = userVote,
                    RelatedPosts = await _postService.GetRelatedPostsAsync(id, 5)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                return View("Error", new ErrorViewModel
                {
                    RequestId = HttpContext.TraceIdentifier,
                    ErrorMessage = "An error occurred while fetching the post."
                });
            }
        }

        /// <summary>
        /// Displays the create post form
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreatePost()
        {
            try
            {
                // Initialize view model
                PostCreateViewModel model = new PostCreateViewModel();

                // Initialize post types
                model.InitializePostTypes();

                // Get communities for dropdown
                var communities = await _communityService.GetCommunitiesForUserAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                model.Communities = communities.Select(c => new SelectListItem
                {
                    Value = c.CommunityId.ToString(),
                    Text = c.Name,
                    Selected = c.CommunityId == post.CommunityId
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error and add a model error
                ModelState.AddModelError("", "An error occurred while updating the post. Please try again.");

                // Re-initialize dropdown data
                model.InitializePostTypes();

                return View(model);
            }
        }
    }
  }
