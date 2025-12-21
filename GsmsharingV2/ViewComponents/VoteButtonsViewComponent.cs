using GsmsharingV2.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.ViewComponents
{
    public class VoteButtonsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<GsmsharingV2.Models.ApplicationUser> _userManager;

        public VoteButtonsViewComponent(
            ApplicationDbContext context,
            UserManager<GsmsharingV2.Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string contentType, int contentId, int? upvotes = null, int? downvotes = null)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var userVote = 0; // 0 = no vote, 1 = upvote, -1 = downvote

            if (!string.IsNullOrEmpty(userId))
            {
                if (contentType == "post")
                {
                    var vote = await _context.PostVotes
                        .FirstOrDefaultAsync(v => v.PostID == contentId && v.UserId == userId);
                    if (vote != null)
                    {
                        userVote = vote.VoteType;
                    }
                }
                else if (contentType == "comment")
                {
                    var vote = await _context.CommentVotes
                        .FirstOrDefaultAsync(v => v.CommentID == contentId && v.UserId == userId);
                    if (vote != null)
                    {
                        userVote = vote.VoteType;
                    }
                }
            }

            // Get actual counts if not provided
            if (!upvotes.HasValue || !downvotes.HasValue)
            {
                if (contentType == "post")
                {
                    var post = await _context.Posts.FindAsync(contentId);
                    if (post != null)
                    {
                        upvotes = post.UpvoteCount;
                        downvotes = post.DownvoteCount;
                    }
                }
                else if (contentType == "comment")
                {
                    var comment = await _context.Comments.FindAsync(contentId);
                    if (comment != null)
                    {
                        upvotes = comment.UpvoteCount;
                        downvotes = comment.DownvoteCount;
                    }
                }
            }

            var model = new VoteButtonsViewModel
            {
                ContentType = contentType,
                ContentID = contentId,
                Upvotes = upvotes ?? 0,
                Downvotes = downvotes ?? 0,
                UserVote = userVote,
                IsAuthenticated = !string.IsNullOrEmpty(userId)
            };

            return View(model);
        }
    }

    public class VoteButtonsViewModel
    {
        public string ContentType { get; set; }
        public int ContentID { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public int UserVote { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}

