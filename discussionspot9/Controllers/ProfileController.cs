using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using discussionspot9.Interfaces;

namespace discussionspot9.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProfileController> _logger;
        private readonly IFollowService _followService;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<ProfileController> logger,
            IFollowService followService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _followService = followService;
        }

        // GET: Profile
        public async Task<IActionResult> Index(string? userId = null)
        {
            try
            {
                // If no userId provided, use current user
                if (string.IsNullOrEmpty(userId))
                {
                    userId = _userManager.GetUserId(User);
                }

                var userProfile = await _context.UserProfiles
                    .Include(up => up.User)
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (userProfile == null)
                {
                    // Create profile if it doesn't exist for current user
                    if (userId == _userManager.GetUserId(User))
                    {
                        var user = await _userManager.GetUserAsync(User);
                        if (user != null)
                        {
                            userProfile = new UserProfile
                            {
                                UserId = user.Id,
                                DisplayName = user.UserName ?? "User",
                                JoinDate = DateTime.UtcNow,
                                LastActive = DateTime.UtcNow,
                                KarmaPoints = 0,
                                IsVerified = false
                            };

                            _context.UserProfiles.Add(userProfile);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }
                }

                var viewModel = new ProfileViewModel
                {
                    UserId = userProfile.UserId,
                    DisplayName = userProfile.DisplayName,
                    Bio = userProfile.Bio,
                    AvatarUrl = userProfile.AvatarUrl,
                    BannerUrl = userProfile.BannerUrl,
                    Website = userProfile.Website,
                    Location = userProfile.Location,
                    JoinDate = userProfile.JoinDate,
                    LastActive = userProfile.LastActive,
                    KarmaPoints = userProfile.KarmaPoints,
                    IsVerified = userProfile.IsVerified
                };

                // Get user statistics
                var postCount = await _context.Posts.CountAsync(p => p.UserId == userId);
                var commentCount = await _context.Comments.CountAsync(c => c.UserId == userId);

                ViewBag.PostCount = postCount;
                ViewBag.CommentCount = commentCount;
                ViewBag.IsCurrentUser = userId == _userManager.GetUserId(User);

                // Get follow counts
                var followersCount = await _followService.GetFollowerCountAsync(userId);
                var followingCount = await _followService.GetFollowingCountAsync(userId);

                ViewData["FollowersCount"] = followersCount;
                ViewData["FollowingCount"] = followingCount;
                ViewData["IsOwnProfile"] = userId == _userManager.GetUserId(User);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile for user {UserId}", userId);
                TempData["ErrorMessage"] = "Error loading profile. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                // Ensure user can only update their own profile
                if (model.UserId != currentUserId)
                {
                    return Forbid();
                }

                if (ModelState.IsValid)
                {
                    var userProfile = await _context.UserProfiles
                        .FirstOrDefaultAsync(up => up.UserId == currentUserId);

                    if (userProfile == null)
                    {
                        return NotFound();
                    }

                    // Update profile fields
                    userProfile.DisplayName = model.DisplayName;
                    userProfile.Bio = model.Bio;
                    userProfile.AvatarUrl = model.AvatarUrl;
                    userProfile.BannerUrl = model.BannerUrl;
                    userProfile.Website = model.Website;
                    userProfile.Location = model.Location;

                    _context.UserProfiles.Update(userProfile);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // If model is invalid, reload the page with validation errors
                var postCount = await _context.Posts.CountAsync(p => p.UserId == currentUserId);
                var commentCount = await _context.Comments.CountAsync(c => c.UserId == currentUserId);

                ViewBag.PostCount = postCount;
                ViewBag.CommentCount = commentCount;
                ViewBag.IsCurrentUser = true;

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", model.UserId);
                TempData["ErrorMessage"] = "Error updating profile. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Profile/GetTabContent
        public async Task<IActionResult> GetTabContent(string tab, string userId)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var isCurrentUser = userId == currentUserId;

                switch (tab.ToLower())
                {
                    case "activity":
                        return await GetActivityContent(userId, isCurrentUser);
                    case "posts":
                        return await GetPostsContent(userId, isCurrentUser);
                    case "comments":
                        return await GetCommentsContent(userId, isCurrentUser);
                    case "saved":
                        return await GetSavedContent(userId, isCurrentUser);
                    default:
                        return PartialView("_EmptyContent");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tab content {Tab} for user {UserId}", tab, userId);
                return PartialView("_ErrorContent");
            }
        }

        private async Task<IActionResult> GetActivityContent(string userId, bool isCurrentUser)
        {
            var activities = new List<object>();

            // Get recent posts
            var recentPosts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.Community)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new
                {
                    Type = "post",
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    CommunityName = p.Community!.Name,
                    UpvoteCount = p.UpvoteCount,
                    CommentCount = p.CommentCount,
                    Url = $"/r/{p.Community.Slug}/posts/{p.Slug}"
                })
                .ToListAsync();

            // Get recent comments
            var recentComments = await _context.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.Post)
                .ThenInclude(p => p!.Community)
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .Select(c => new
                {
                    Type = "comment",
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    PostTitle = c.Post!.Title,
                    CommunityName = c.Post.Community!.Name,
                    UpvoteCount = c.UpvoteCount,
                    Url = $"/r/{c.Post.Community.Slug}/posts/{c.Post.Slug}#comment-{c.CommentId}"
                })
                .ToListAsync();

            // Combine and sort activities
            activities.AddRange(recentPosts);
            activities.AddRange(recentComments);

            ViewBag.Activities = activities.OrderByDescending(a =>
                a.GetType().GetProperty("CreatedAt")?.GetValue(a)).Take(20).ToList();

            return PartialView("_ActivityContent");
        }

        private async Task<IActionResult> GetPostsContent(string userId, bool isCurrentUser)
        {
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.Community)
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();

            ViewBag.Posts = posts;
            return PartialView("_PostsContent");
        }

        private async Task<IActionResult> GetCommentsContent(string userId, bool isCurrentUser)
        {
            var comments = await _context.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.Post)
                .ThenInclude(p => p!.Community)
                .OrderByDescending(c => c.CreatedAt)
                .Take(20)
                .ToListAsync();

            ViewBag.Comments = comments;
            return PartialView("_CommentsContent");
        }

        private async Task<IActionResult> GetSavedContent(string userId, bool isCurrentUser)
        {
            if (!isCurrentUser)
            {
                return PartialView("_PrivateContent");
            }

            var savedPosts = await _context.SavedPosts
                .Where(sp => sp.UserId == userId)
                .Include(sp => sp.Post)
                .ThenInclude(p => p!.Community)
                .OrderByDescending(sp => sp.SavedAt)
                .Take(20)
                .ToListAsync();

            ViewBag.SavedPosts = savedPosts;
            return PartialView("_SavedContent");
        }

        // POST: Profile/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return NotFound();
                }

                // Delete user profile and related data
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == currentUserId);

                if (userProfile != null)
                {
                    _context.UserProfiles.Remove(userProfile);
                }

                // Note: Related posts, comments, etc. will be handled by cascade delete
                // or set to null based on the database configuration

                await _context.SaveChangesAsync();

                // Delete the identity user
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Error deleting account" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account for user {UserId}", _userManager.GetUserId(User));
                return Json(new { success = false, message = "Error deleting account" });
            }
        }

        // GET: Profile/Settings
        public async Task<IActionResult> Settings()
        {
            var currentUserId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == currentUserId);

            if (userProfile == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
            {
                UserId = userProfile.UserId,
                DisplayName = userProfile.DisplayName,
                Bio = userProfile.Bio,
                AvatarUrl = userProfile.AvatarUrl,
                BannerUrl = userProfile.BannerUrl,
                Website = userProfile.Website,
                Location = userProfile.Location,
                JoinDate = userProfile.JoinDate,
                LastActive = userProfile.LastActive,
                KarmaPoints = userProfile.KarmaPoints,
                IsVerified = userProfile.IsVerified
            };

            return View(viewModel);
        }

        // GET: Profile/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // POST: Profile/UnsavePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsavePost([FromBody] UnsavePostRequest request)
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                var savedPost = await _context.SavedPosts
                    .FirstOrDefaultAsync(sp => sp.UserId == currentUserId && sp.PostId == request.PostId);

                if (savedPost != null)
                {
                    _context.SavedPosts.Remove(savedPost);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Post not found in saved items" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsaving post {PostId} for user {UserId}", request.PostId, _userManager.GetUserId(User));
                return Json(new { success = false, message = "Error unsaving post" });
            }
        }
    }

    public class UnsavePostRequest
    {
        public int PostId { get; set; }
    }
}
