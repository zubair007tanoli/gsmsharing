using discussionspot.Data;
using discussionspot.Data.discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using discussionspot.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace discussionspot.Controllers
{
    public class DiscussionPostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<DiscussionPostController> _logger;
        public DiscussionPostController(
       ApplicationDbContext context,
       UserManager<ApplicationUsers> userManager,
       IWebHostEnvironment webHostEnvironment,
       ILogger<DiscussionPostController> logger)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }
        /// <summary>
        /// Gets all posts with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPosts(string sortBy = "hot", string timeFilter = "week", int? communityId = null)
        {
            return View();
        }

        /// <summary>
        /// Displays a single post with comments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Post(int id)
        {
            return View();
        }

        /// <summary>
        /// Displays the create post form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreatePost()
        {// Create new model with properly initialized collections to avoid null reference exceptions
            var model = new PostCreateViewModel
            {
                PostType = "text",  // Default post type
                PollOptions = new List<string> { "", "" },  // Initialize with two empty options
                IsCommentAllowed = true  // Default to allowing comments
            };

            // Get communities from database
            var communities = await _context.Communities
                .Where(c => c.IsDeleted == false)
                .Select(c => new
                {
                    Id = c.CommunityId,
                    Name = c.Name
                })
                .ToListAsync();

            // Add communities to selection list
            model.Communities = communities.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"r/{c.Name}"
            }).ToList();

            // Initialize the post types
            model.InitializePostTypes();

            return View(model);
        }
    }
  }
