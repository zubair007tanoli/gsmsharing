using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace discussionspot9.Components
{
    public class UserInterestPostsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserInterestPostsViewComponent(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 5)
        {
            var posts = new List<UserInterestPostViewModel>();

            try
            {
                // Check if user is authenticated
                var userId = UserClaimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Get communities the user is a member of
                    var userCommunityIds = await _context.CommunityMembers
                        .Where(cm => cm.UserId == userId)
                        .Select(cm => cm.CommunityId)
                        .ToListAsync();

                    if (userCommunityIds.Any())
                    {
                        // Get recent posts from user's communities
                        posts = await _context.Posts
                            .Where(p => p.Status == "published" && userCommunityIds.Contains(p.CommunityId))
                            .OrderByDescending(p => p.CreatedAt)
                            .Take(count)
                            .Select(p => new UserInterestPostViewModel
                            {
                                PostId = p.PostId,
                                Title = p.Title,
                                Slug = p.Slug,
                                CommunitySlug = p.Community.Slug,
                                CommunityName = p.Community.Name,
                                UpvoteCount = p.UpvoteCount,
                                CommentCount = p.CommentCount,
                                CreatedAt = p.CreatedAt,
                                PostType = p.PostType
                            })
                            .ToListAsync();
                    }
                }

                // If user is not logged in or has no communities, show trending posts
                if (!posts.Any())
                {
                    posts = await _context.Posts
                        .Where(p => p.Status == "published" && p.CreatedAt >= DateTime.Now.AddDays(-7))
                        .OrderByDescending(p => p.Score)
                        .ThenByDescending(p => p.ViewCount)
                        .Take(count)
                        .Select(p => new UserInterestPostViewModel
                        {
                            PostId = p.PostId,
                            Title = p.Title,
                            Slug = p.Slug,
                            CommunitySlug = p.Community.Slug,
                            CommunityName = p.Community.Name,
                            UpvoteCount = p.UpvoteCount,
                            CommentCount = p.CommentCount,
                            CreatedAt = p.CreatedAt,
                            PostType = p.PostType
                        })
                        .ToListAsync();
                }
            }
            catch (Exception)
            {
                // Return empty list on error
                posts = new List<UserInterestPostViewModel>();
            }

            return View(posts);
        }
    }
}

