namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommentViewModel
    {
        public int PostId { get; set; }
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public int TreeLevel { get; set; }



        // Author
        public string? UserId { get; set; }
        public string AuthorDisplayName { get; set; } = string.Empty;
        public string AuthorInitials { get; set; } = string.Empty;
        public bool IsAuthorVerified { get; set; }

        // Parent comment
        public int? ParentCommentId { get; set; }

        // Current user interaction
        public int? CurrentUserVote { get; set; }
        public bool IsCurrentUserAuthor { get; set; }

        // Child comments
        public List<CommentViewModel> ChildComments { get; set; } = new();

        // Calculated properties
        public int Score => UpvoteCount - DownvoteCount;
        public string TimeAgo => GetTimeAgo(IsEdited ? UpdatedAt : CreatedAt);
        public string AuthorUrl => $"/u/{AuthorDisplayName}";
        public bool HasChildren => ChildComments.Any();

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();
            return timeSpan.TotalSeconds switch
            {
                < 60 => "just now",
                < 3600 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 86400 => $"{(int)timeSpan.TotalHours}h ago",
                _ => dateTime.ToString("MMM dd, yyyy")
            };
        }
    }
}
