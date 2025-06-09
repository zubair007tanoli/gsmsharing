using discussionspot9.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Components
{
    public class CommentThreadViewComponent : ViewComponent
    {
        private readonly ICommentService _commentService;

        public CommentThreadViewComponent(ICommentService commentService)
        {
            _commentService = commentService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int postId, string sort = "best", int page = 1)
        {
            var comments = await _commentService.GetPostCommentsAsync(postId, sort, page);

            ViewBag.PostId = postId;
            ViewBag.Sort = sort;
            ViewBag.Page = page;

            return View();
        }
    }
}
