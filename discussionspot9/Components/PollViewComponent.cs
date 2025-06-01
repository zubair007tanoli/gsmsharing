using discussionspot9.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace discussionspot9.Components
{
    public class PollViewComponent : ViewComponent
    {
        private readonly IPostService _postService;

        public PollViewComponent(IPostService postService)
        {
            _postService = postService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int postId)
        {
            var pollData = await _postService.GetPollDataAsync(postId);
            if (pollData == null)
                return Content("");

            var userId = UserClaimsPrincipal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                pollData.HasUserVoted = await _postService.HasUserVotedInPollAsync(postId, userId);
                pollData.UserVotes = await _postService.GetUserPollVotesAsync(postId, userId);
            }

            return View(pollData);
        }
    }
}
