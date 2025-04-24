namespace discussionspot.ViewModels
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string UserAvatarUrl { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int Score { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public int TreeLevel { get; set; }
        public int? UserVote { get; set; } // 1, -1, or null
        public List<CommentViewModel> ChildComments { get; set; }
        public List<CommentAwardViewModel> Awards { get; set; }
    }
}
