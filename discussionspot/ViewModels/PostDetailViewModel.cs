namespace discussionspot.ViewModels
{
    public class PostDetailViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string PostType { get; set; }
        public string Url { get; set; }  // For link posts
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string UserAvatarUrl { get; set; }
        public int CommunityId { get; set; }
        public string CommunityName { get; set; }
        public string CommunitySlug { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public int CommentCount { get; set; }
        public int Score { get; set; }
        public string Status { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsSpoiler { get; set; }
        public int ViewCount { get; set; }
        public List<MediaViewModel> Media { get; set; }
        public List<CommentViewModel> Comments { get; set; }
        public List<TagViewModel> Tags { get; set; }
        public int? UserVote { get; set; } // 1, -1, or null if no vote
        public List<PostAwardViewModel> Awards { get; set; }

        // Poll-specific properties
        public bool HasPoll { get; set; }
        public List<PollOptionViewModel> PollOptions { get; set; }
        public PollConfigurationViewModel PollConfiguration { get; set; }
        public bool UserHasVoted { get; set; }
        public bool PollIsActive { get; set; }
    }
}
