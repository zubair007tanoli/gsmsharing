using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers.Api
{
    [ApiController]
    [Route("api/stats")]
    public class StatsController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IPresenceService _presenceService;
        private readonly ILogger<StatsController> _logger;

        public StatsController(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IPresenceService presenceService,
            ILogger<StatsController> logger)
        {
            _contextFactory = contextFactory;
            _presenceService = presenceService;
            _logger = logger;
        }

        [HttpGet("online-count")]
        public async Task<IActionResult> GetOnlineCount()
        {
            try
            {
                var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);
                
                await using var context = await _contextFactory.CreateDbContextAsync();
                var count = await context.UserPresences
                    .Where(p => p.LastSeen > fifteenMinutesAgo)
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();

                return Ok(new { success = true, count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online count");
                
                // Fallback to UserProfiles
                try
                {
                    await using var context = await _contextFactory.CreateDbContextAsync();
                    var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);
                    var count = await context.UserProfiles
                        .CountAsync(u => u.LastActive > fifteenMinutesAgo);
                    
                    return Ok(new { success = true, count });
                }
                catch
                {
                    return Ok(new { success = true, count = 0 });
                }
            }
        }
    }
}

