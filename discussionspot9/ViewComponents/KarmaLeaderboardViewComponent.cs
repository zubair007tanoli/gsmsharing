using Microsoft.AspNetCore.Mvc;
using discussionspot9.Services;

namespace discussionspot9.ViewComponents
{
    public class KarmaLeaderboardViewComponent : ViewComponent
    {
        private readonly IKarmaService _karmaService;

        public KarmaLeaderboardViewComponent(IKarmaService karmaService)
        {
            _karmaService = karmaService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 10)
        {
            var leaderboard = await _karmaService.GetKarmaLeaderboardAsync("all", count);
            return View(leaderboard);
        }
    }
}

