using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsersApiController> _logger;

        public UsersApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<UsersApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("{userId}/Posts")]
        public async Task<IActionResult> GetUserPosts(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var posts = await _context.Posts
                    .Where(p => p.UserId == userId && p.PostStatus == "published")
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        postID = p.PostID,
                        title = p.Title,
                        slug = p.Slug,
                        createdAt = p.CreatedAt,
                        viewCount = p.ViewCount ?? 0,
                        upvoteCount = p.UpvoteCount,
                        downvoteCount = p.DownvoteCount,
                        commentCount = p.CommentCount
                    })
                    .ToListAsync();

                var totalCount = await _context.Posts
                    .CountAsync(p => p.UserId == userId && p.PostStatus == "published");

                return Ok(new
                {
                    posts = posts,
                    totalCount = totalCount,
                    currentPage = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user posts for {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while loading posts" });
            }
        }

        [HttpGet("{userId}/Comments")]
        public async Task<IActionResult> GetUserComments(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var comments = await _context.Comments
                    .Where(c => c.UserId == userId && (c.IsDeleted == false || c.IsDeleted == null))
                    .Include(c => c.Post)
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        commentID = c.CommentID,
                        postID = c.PostID,
                        postTitle = c.Post != null ? c.Post.Title : null,
                        content = c.Content,
                        createdAt = c.CreatedAt,
                        upvoteCount = c.UpvoteCount,
                        downvoteCount = c.DownvoteCount
                    })
                    .ToListAsync();

                var totalCount = await _context.Comments
                    .CountAsync(c => c.UserId == userId && (c.IsDeleted == false || c.IsDeleted == null));

                return Ok(new
                {
                    comments = comments,
                    totalCount = totalCount,
                    currentPage = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user comments for {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while loading comments" });
            }
        }

        [HttpGet("{userId}/SavedPosts")]
        [Authorize]
        public async Task<IActionResult> GetSavedPosts(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                if (currentUserId != userId)
                {
                    return Forbid();
                }

                var savedPosts = await _context.SavedPosts
                    .Where(sp => sp.UserId == userId)
                    .Include(sp => sp.Post)
                    .ThenInclude(p => p.User)
                    .OrderByDescending(sp => sp.SavedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(sp => new
                    {
                        postID = sp.Post.PostID,
                        title = sp.Post.Title,
                        slug = sp.Post.Slug,
                        user = new
                        {
                            userId = sp.Post.UserId,
                            userName = sp.Post.User != null ? sp.Post.User.UserName : "Anonymous"
                        },
                        savedAt = sp.SavedAt
                    })
                    .ToListAsync();

                var totalCount = await _context.SavedPosts
                    .CountAsync(sp => sp.UserId == userId);

                return Ok(new
                {
                    posts = savedPosts,
                    totalCount = totalCount,
                    currentPage = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading saved posts for {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while loading saved posts" });
            }
        }

        [HttpGet("{userId}/Stats")]
        public async Task<IActionResult> GetUserStats(string userId)
        {
            try
            {
                var postsCount = await _context.Posts
                    .CountAsync(p => p.UserId == userId && p.PostStatus == "published");

                var commentsCount = await _context.Comments
                    .CountAsync(c => c.UserId == userId && (c.IsDeleted == false || c.IsDeleted == null));

                // Calculate karma (sum of upvotes on posts and comments - downvotes)
                var postUpvotes = await _context.Posts
                    .Where(p => p.UserId == userId)
                    .SumAsync(p => p.UpvoteCount);
                
                var postDownvotes = await _context.Posts
                    .Where(p => p.UserId == userId)
                    .SumAsync(p => p.DownvoteCount);

                var commentUpvotes = await _context.Comments
                    .Where(c => c.UserId == userId)
                    .SumAsync(c => c.UpvoteCount);

                var commentDownvotes = await _context.Comments
                    .Where(c => c.UserId == userId)
                    .SumAsync(c => c.DownvoteCount);

                var karma = (postUpvotes + commentUpvotes) - (postDownvotes + commentDownvotes);

                return Ok(new
                {
                    postsCount = postsCount,
                    commentsCount = commentsCount,
                    karma = karma
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user stats for {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while loading stats" });
            }
        }
    }
}

