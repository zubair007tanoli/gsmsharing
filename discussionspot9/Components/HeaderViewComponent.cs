using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Components
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HeaderViewComponent(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Disable caching for authentication-dependent content
            ViewContext.HttpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ViewContext.HttpContext.Response.Headers["Pragma"] = "no-cache";
            ViewContext.HttpContext.Response.Headers["Expires"] = "0";
            
            var model = new HeaderViewModel();

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    model.IsAuthenticated = true;
                    model.Email = user.Email;
                    model.UserId = user.Id;

                    // Get user profile data
                    var userProfile = await _context.UserProfiles
                        .AsNoTracking()  // Add AsNoTracking for better performance
                        .FirstOrDefaultAsync(p => p.UserId == user.Id);

                    if (userProfile != null)
                    {
                        model.DisplayName = userProfile.DisplayName;
                        model.AvatarUrl = userProfile.AvatarUrl;
                        model.KarmaPoints = userProfile.KarmaPoints;
                        model.IsVerified = userProfile.IsVerified;
                    }
                    else
                    {
                        model.DisplayName = user.Email?.Split('@')[0];
                    }

                    // Get unread notifications count
                    model.UnreadNotifications = await _context.Notifications
                        .CountAsync(n => n.UserId == user.Id && !n.IsRead);

                    // Get recent notifications (last 5)
                    var notifications = await _context.Notifications
                        .Where(n => n.UserId == user.Id)
                        .OrderByDescending(n => n.CreatedAt)
                        .Take(5)
                        .ToListAsync();

                    model.RecentNotifications = notifications.Select(n => new NotificationViewModel
                    {
                        NotificationId = Guid.Parse(n.NotificationId.ToString()),
                        UserId = n.UserId,
                        Type = n.Type,
                        Title = n.Title,
                        Message = n.Message,
                        EntityType = n.EntityType,
                        EntityId = n.EntityId,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt,
                        Url = GenerateNotificationUrl(n) // Add URL generation
                    }).ToList();
                }
            }

            return View(model);
        }

        private string GenerateNotificationUrl(dynamic notification)
        {
            var urlHelper = Url;

            switch (notification.EntityType?.ToLower())
            {
                case "post":
                    // For posts, we need community slug and post slug
                    // You'll need to get these from your database
                    return GetPostUrl(notification.EntityId, urlHelper);

                case "community":
                    // For communities: r/{slug}
                    return GetCommunityUrl(notification.EntityId, urlHelper);

                case "user":
                    // For users: u/{displayName}
                    return GetUserUrl(notification.EntityId, urlHelper);

                case "comment":
                    // For comments, link to the post with comment anchor
                    return GetCommentUrl(notification.EntityId, urlHelper);

                default:
                    // Default fallback - you might want to create a notifications route
                    return urlHelper.Action("Index", "Home");
            }
        }

        private string GetPostUrl(string postId, IUrlHelper urlHelper)
        {
            // You'll need to fetch the post and community details from database
            var postDetails = _context.Posts
                .Include(p => p.Community)
                .FirstOrDefault(p => p.PostId.ToString() == postId);

            if (postDetails != null)
            {
                return urlHelper.RouteUrl("community_posts", new
                {
                    communitySlug = postDetails.Community.Slug,
                    postSlug = postDetails.Slug
                });
            }

            return urlHelper.Action("Index", "Home");
        }

        private string GetCommunityUrl(string communityId, IUrlHelper urlHelper)
        {
            var community = _context.Communities
                .FirstOrDefault(c => c.CommunityId.ToString() == communityId);

            if (community != null)
            {
                return urlHelper.RouteUrl("community_detail", new { slug = community.Slug });
            }

            return urlHelper.Action("Index", "Community");
        }

        private string GetUserUrl(string userId, IUrlHelper urlHelper)
        {
            var userProfile = _context.UserProfiles
                .FirstOrDefault(u => u.UserId == userId);

            if (userProfile != null)
            {
                return urlHelper.RouteUrl("user_profile", new { displayName = userProfile.DisplayName });
            }

            return urlHelper.Action("Index", "Home");
        }

        private string GetCommentUrl(string commentId, IUrlHelper urlHelper)
        {
            // Get comment and its associated post
            var comment = _context.Comments
                .Include(c => c.Post)
                .ThenInclude(p => p.Community)
                .FirstOrDefault(c => c.CommentId.ToString() == commentId);

            if (comment != null)
            {
                var postUrl = urlHelper.RouteUrl("community_posts", new
                {
                    communitySlug = comment.Post.Community.Slug,
                    postSlug = comment.Post.Slug
                });

                return $"{postUrl}#comment-{commentId}";
            }

            return urlHelper.Action("Index", "Home");
        }
    }
}