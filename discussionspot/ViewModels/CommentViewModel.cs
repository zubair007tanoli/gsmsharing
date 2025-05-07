namespace discussionspot.ViewModels
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string UserAvatarUrl { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int Score { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public int TreeLevel { get; set; }

        // User's interaction with this comment if authenticated
        public bool? UserVoted { get; set; } // null if not voted, true for upvote, false for downvote

        // Hierarchical structure
        public List<CommentViewModel> ChildComments { get; set; } = new List<CommentViewModel>();

        // Helper for displaying relative time
        public string RelativeTime
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                if (timeSpan.TotalMinutes < 1)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes}m ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours}h ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays}d ago";
                if (timeSpan.TotalDays < 365)
                    return $"{(int)(timeSpan.TotalDays / 30)}mo ago";
                return $"{(int)(timeSpan.TotalDays / 365)}y ago";
            }
        }

        // Helper method to add child comment to appropriate parent in hierarchy
        public static void AddToHierarchy(List<CommentViewModel> flatComments)
        {
            var commentDict = flatComments.ToDictionary(c => c.CommentId);
            foreach (var comment in flatComments)
            {
                if (comment.ParentCommentId.HasValue &&
                    commentDict.ContainsKey(comment.ParentCommentId.Value))
                {
                    commentDict[comment.ParentCommentId.Value].ChildComments.Add(comment);
                }
            }
        }
    }
}
