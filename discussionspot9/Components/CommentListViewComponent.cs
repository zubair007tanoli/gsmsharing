using discussionspot9.Interfaces; // Assuming your ICommentService is here
using discussionspot9.Models.ViewModels.CreativeViewModels; // Assuming your CommentViewModel and CommentTreeViewModel are here
using discussionspot9.Services.ServiceResults; // Assuming you have ServiceResults
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.ViewComponents
{
    // You'll need to create an ICommentService and a corresponding CommentService
    // to handle actual data retrieval and building the comment tree.
    // For now, this uses hardcoded data as per your request.
    public class CommentListViewComponent : ViewComponent
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentListViewComponent> _logger; // Add logger

        public CommentListViewComponent(ICommentService commentService, ILogger<CommentListViewComponent> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int postId)
        {
            try
            {
                var comments2 = await _commentService.GetPostCommentsAsync(postId, "best", 1);
                var commentTrees = comments2.Select(c => new CommentTreeViewModel
                {
                    Comment = new CommentViewModel
                    {
                        CommentId = c.Comment.CommentId,
                        Content = c.Comment.Content,
                        CreatedAt = c.Comment.CreatedAt,
                        AuthorDisplayName = c.Comment.AuthorDisplayName,
                        AuthorInitials = GetInitials(c.Comment.AuthorDisplayName),
                        UpvoteCount = c.Comment.UpvoteCount,           // Added
                        DownvoteCount = c.Comment.DownvoteCount,       // Added
                        CurrentUserVote = c.Comment.CurrentUserVote,
                        IsEdited = c.Comment.IsEdited
                    },
                    Depth = c.Depth,
                    Children = MapChildren(c.Children)
                }).ToList();

                return View(commentTrees);

                //var comments = new List<CommentTreeViewModel>
                //{
                //    new()
                //    {
                //        Comment = new CommentViewModel
                //        {
                //            CommentId = 1,
                //            Content = "This is truly groundbreaking research. I've been following this team's work for years, and they've consistently pushed the boundaries of what's possible with neural networks.",
                //            CreatedAt = DateTime.UtcNow.AddHours(-3),
                //            AuthorDisplayName = "AIenthusiast",
                //            AuthorInitials = "A",
                //            UpvoteCount = 24,
                //            CurrentUserVote = 1 // Example: User has upvoted
                //        },
                //        Depth = 0,
                //        Children = new List<CommentTreeViewModel>
                //        {
                //            new()
                //            {
                //                Comment = new CommentViewModel
                //                {
                //                    CommentId = 2,
                //                    Content = "While impressive, I think we need to be cautious about claims of \"human-level\" performance. These models still struggle with certain types of reasoning that humans find intuitive.",
                //                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                //                    AuthorDisplayName = "techskeptic",
                //                    AuthorInitials = "T",
                //                    UpvoteCount = 8,
                //                    CurrentUserVote = null // Example: User has not voted
                //                },
                //                Depth = 1,
                //                Children = new List<CommentTreeViewModel>
                //                {
                //                    new()
                //                    {
                //                        Comment = new CommentViewModel
                //                        {
                //                            CommentId = 3,
                //                            Content = "Fair point. I should have been more specific. The model shows human-level performance on the specific benchmarks mentioned in the paper, but you're right that general intelligence is still a far-off goal.",
                //                            CreatedAt = DateTime.UtcNow.AddHours(-1),
                //                            AuthorDisplayName = "AIenthusiast",
                //                            AuthorInitials = "A",
                //                            UpvoteCount = 5,
                //                            CurrentUserVote = null
                //                        },
                //                        Depth = 2
                //                    }
                //                }
                //            },
                //            new()
                //            {
                //                Comment = new CommentViewModel
                //                {
                //                    CommentId = 4,
                //                    Content = "I'm one of the co-authors of this paper. Happy to answer any questions about our methodology or findings!",
                //                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                //                    AuthorDisplayName = "researcher42",
                //                    AuthorInitials = "R",
                //                    UpvoteCount = 15,
                //                    CurrentUserVote = null
                //                },
                //                Depth = 1
                //            }
                //        }
                //    },
                //    new()
                //    {
                //        Comment = new CommentViewModel
                //        {
                //            CommentId = 5,
                //            Content = "We need to have serious discussions about the ethical implications of these advancements. As AI approaches human-level capabilities, we need robust frameworks to ensure these systems are developed and deployed responsibly.",
                //            CreatedAt = DateTime.UtcNow.AddHours(-4),
                //            AuthorDisplayName = "ethicist",
                //            AuthorInitials = "E",
                //            UpvoteCount = 19,
                //            CurrentUserVote = null
                //        },
                //        Depth = 0
                //    }
                //};
                
                // --- End hardcoded data ---

                // Simulate sorting if needed, or pass sort parameter to service
                // comments = comments.OrderByDescending(c => c.Comment.Score).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CommentListViewComponent for postId: {PostId}", postId);
                // Return an empty list or an error view in case of failure
                return View(new List<CommentTreeViewModel>());
            }
        }

        // Helper method to generate initials
        private static string GetInitials(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return "?";

            var names = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return names.Length switch
            {
                0 => "?",
                1 => names[0][0].ToString(),
                _ => $"{names[0][0]}{names[^1][0]}"
            };
        }

        // Recursive method to map child comments
        // Fixed MapChildren method
        private List<CommentTreeViewModel> MapChildren(IEnumerable<CommentTreeViewModel> children)
        {
            if (children == null)
                return new List<CommentTreeViewModel>(); // Return empty list instead of null

            return children.Select(c => new CommentTreeViewModel
            {
                Comment = new CommentViewModel
                {
                    CommentId = c.Comment.CommentId,
                    Content = c.Comment.Content,
                    CreatedAt = c.Comment.CreatedAt,
                    AuthorDisplayName = c.Comment.AuthorDisplayName,
                    AuthorInitials = GetInitials(c.Comment.AuthorDisplayName),
                    UpvoteCount = c.Comment.UpvoteCount,
                    DownvoteCount = c.Comment.DownvoteCount,
                    CurrentUserVote = c.Comment.CurrentUserVote,
                    IsEdited = c.Comment.IsEdited
                },
                Depth = c.Depth,
                Children = MapChildren(c.Children) // Recursive call
            }).ToList();
        }
    }
}
