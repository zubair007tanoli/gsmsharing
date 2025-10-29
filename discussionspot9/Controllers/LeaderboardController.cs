using discussionspot9.Services;
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly IKarmaService _karmaService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(IKarmaService karmaService, ILogger<LeaderboardController> logger)
        {
            _karmaService = karmaService;
            _logger = logger;
        }

        [HttpGet("/leaderboard")]
        public async Task<IActionResult> Index(string timeframe = "all", int count = 50)
        {
            try
            {
                var users = await _karmaService.GetKarmaLeaderboardAsync(timeframe, count);
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading leaderboard");
                return View(new List<UserKarmaViewModel>());
            }
        }
    }
}


