using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
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
        private readonly IChatService _chatService;

        public HeaderViewComponent(UserManager<IdentityUser> userManager, ApplicationDbContext context, IChatService chatService)
        {
            _userManager = userManager;
            _context = context;
            _chatService = chatService;
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
                try
                {
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user != null)
                    {
                        model.IsAuthenticated = true;
                        model.Email = user.Email;
                        model.UserId = user.Id;

                        try
                        {
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
                                model.DisplayName = user.Email?.Split('@')[0] ?? string.Empty;
                            }
                        }
                        catch
                        {
                            // If database query fails, use email as fallback
                            model.DisplayName = user.Email?.Split('@')[0] ?? string.Empty;
                        }

                        try
                        {
                            // Get unread notifications count
                            model.UnreadNotifications = await _context.Notifications
                                .CountAsync(n => n.UserId == user.Id && !n.IsRead);
                        }
                        catch
                        {
                            model.UnreadNotifications = 0; // Fail gracefully
                        }

                        // Get unread messages count
                        try
                        {
                            model.UnreadMessagesCount = await _chatService.GetUnreadCountAsync(user.Id);
                        }
                        catch
                        {
                            // Log but don't fail - database connection issues should not break the page
                            model.UnreadMessagesCount = 0; // Fail gracefully
                        }

                        try
                        {
                            // Get recent notifications (last 5)
                            var notifications = await _context.Notifications
                                .AsNoTracking()
                                .Where(n => n.UserId == user.Id)
                                .OrderByDescending(n => n.CreatedAt)
                                .Take(5)
                                .ToListAsync();

                            model.RecentNotifications = notifications.Select(n => new NotificationViewModel
                            {
                                // Notification.NotificationId is an int; do not use null-conditional on value types
                                NotificationId = Guid.TryParse(n.NotificationId.ToString(), out var tmpGuid) ? tmpGuid : Guid.Empty,
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
                        catch
                        {
                            // If notifications query fails, return empty list
                            model.RecentNotifications = new List<NotificationViewModel>();
                        }
                    }
                }
                catch
                {
                    // If user lookup fails, return basic model
                    model.IsAuthenticated = false;
                }
            }

            return View(model);
        }

        private string GenerateNotificationUrl(dynamic notification)
        {
            var urlHelper = Url;

            var entityType = (notification.EntityType as string)?.ToLower();

            switch (entityType)
            {
                case "post":
                    // For posts, we need community slug and post slug
                    // You'll need to get these from your database
                    return GetPostUrl(notification.EntityId as string ?? string.Empty, urlHelper);

                case "community":
                    // For communities: r/{slug}
                    return GetCommunityUrl(notification.EntityId as string ?? string.Empty, urlHelper);

                case "user":
                    // For users: u/{displayName}
                    return GetUserUrl(notification.EntityId as string ?? string.Empty, urlHelper);

                case "comment":
                    // For comments, link to the post with comment anchor
                    return GetCommentUrl(notification.EntityId as string ?? string.Empty, urlHelper);

                default:
                    // Default fallback - you might want to create a notifications route
                    return urlHelper.Action("Index", "Home") ?? "/";
            }
        }

        private string GetPostUrl(string postId, IUrlHelper urlHelper)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(postId))
                    return urlHelper.Action("Index", "Home") ?? "/";

                // Use AsNoTracking and async-safe synchronous call with timeout protection
                var postDetails = _context.Posts
                    .AsNoTracking()
                    .Include(p => p.Community)
                    .FirstOrDefault(p => p.PostId.ToString() == postId);

                if (postDetails?.Community?.Slug is string communitySlug && !string.IsNullOrEmpty(postDetails.Slug))
                {
                    return urlHelper.RouteUrl("community_posts", new
                    {
                        communitySlug = communitySlug,
                        postSlug = postDetails.Slug
                    }) ?? (urlHelper.Action("Index", "Home") ?? "/");
                }
            }
            catch
            {
                // Fail gracefully if database query fails
            }

            return urlHelper.Action("Index", "Home") ?? "/";
        }

        private string GetCommunityUrl(string communityId, IUrlHelper urlHelper)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(communityId))
                    return urlHelper.Action("Index", "Community") ?? "/";

                var community = _context.Communities
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CommunityId.ToString() == communityId);

                if (community?.Slug is string slug)
                {
                    return urlHelper.RouteUrl("community_detail", new { slug = slug }) ?? (urlHelper.Action("Index", "Community") ?? "/");
                }
            }
            catch
            {
                // Fail gracefully if database query fails
            }

            return urlHelper.Action("Index", "Community") ?? "/";
        }

        private string GetUserUrl(string userId, IUrlHelper urlHelper)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return urlHelper.Action("Index", "Home") ?? "/";

                var userProfile = _context.UserProfiles
                    .AsNoTracking()
                    .FirstOrDefault(u => u.UserId == userId);

                if (userProfile?.DisplayName is string displayName)
                {
                    return urlHelper.RouteUrl("user_profile", new { displayName = displayName }) ?? (urlHelper.Action("Index", "Home") ?? "/");
                }
            }
            catch
            {
                // Fail gracefully if database query fails
            }

            return urlHelper.Action("Index", "Home") ?? "/";
        }

        private string GetCommentUrl(string commentId, IUrlHelper urlHelper)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commentId))
                    return urlHelper.Action("Index", "Home") ?? "/";

                // Get comment and its associated post
                var comment = _context.Comments
                    .AsNoTracking()
                    .Include(c => c.Post)
                    .ThenInclude(p => p.Community)
                    .FirstOrDefault(c => c.CommentId.ToString() == commentId);

                if (comment?.Post?.Community?.Slug is string communitySlug && comment.Post.Slug is string postSlug)
                {
                    var postUrl = urlHelper.RouteUrl("community_posts", new
                    {
                        communitySlug = communitySlug,
                        postSlug = postSlug
                    }) ?? (urlHelper.Action("Index", "Home") ?? "/");

                    return $"{postUrl}#comment-{commentId}";
                }
            }
            catch
            {
                // Fail gracefully if database query fails
            }

            return urlHelper.Action("Index", "Home") ?? "/";
        }
    }
}