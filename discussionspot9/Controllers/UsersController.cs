using discussionspot9.Data.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("/users")]
        public async Task<IActionResult> Index()
        {
            var users = await _db.UserProfiles
                .OrderByDescending(u => u.JoinDate)
                .Select(u => new SimpleUser
                {
                    UserId = u.UserId,
                    DisplayName = u.DisplayName,
                    AvatarUrl = u.AvatarUrl,
                    IsVerified = u.IsVerified,
                    Karma = u.KarmaPoints
                })
                .Take(100)
                .ToListAsync();
            return View(users);
        }
    }

    public class SimpleUser
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsVerified { get; set; }
        public int Karma { get; set; }
    }
}


