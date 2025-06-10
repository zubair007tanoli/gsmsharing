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
                        CreatedAt = n.CreatedAt
                    }).ToList();
                }
            }

            return View(model);
        }
    }
}
