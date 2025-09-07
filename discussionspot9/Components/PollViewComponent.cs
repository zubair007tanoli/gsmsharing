using discussionspot9.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace discussionspot9.Components
{
    [ViewComponent(Name = "Poll")]
    public class PollViewComponent : ViewComponent
    {
        private readonly IPostService _postService;

        public PollViewComponent(IPostService postService)
        {
            _postService = postService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int postId)
        {
            // Get the user ID from the claims principal
            var userId = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Fetch all poll data, including user-specific votes, in a single call
            var pollData = await _postService.GetPollDetailsAsync(postId, userId);

            // If no poll data is found for the post, render nothing.
            if (pollData == null)
            {
                return Content("");
            }

            // Pass the poll data to the component's view. 
            // The view is located at /Views/Shared/Components/Poll/Default.cshtml
            return View(pollData);
        }
    }
}
