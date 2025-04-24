namespace discussionspot.ViewModels
{
    public class PostListItemViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string PostType { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string UserAvatarUrl { get; set; }
        public int CommunityId { get; set; }
        public string CommunityName { get; set; }
        public string CommunitySlug { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int CommentCount { get; set; }
        public int Score { get; set; }
        public string Status { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsSpoiler { get; set; }
        public string Excerpt { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool HasPoll { get; set; }
        public int PollVoteCount { get; set; }
        public int? UserVote { get; set; } // 1, -1, or null if no vote
    }
}
