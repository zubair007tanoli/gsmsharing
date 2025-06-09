using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Components
{
    public class RelatedPostsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int PostId)
        {
            var posts = new List<PostCardViewModel>
            {
                new() { Title = "New Neural Network Architecture Breaks Records", UpvoteCount = 876, CommentCount = 124 },
                new() { Title = "AI Ethics Board Releases Guidelines", UpvoteCount = 542, CommentCount = 87 },
                new() { Title = "Tech Companies Race to Develop AI", UpvoteCount = 1200, CommentCount = 203 }
            };

            return View();
        }
    }
}
