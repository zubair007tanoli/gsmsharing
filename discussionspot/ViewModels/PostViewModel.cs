namespace discussionspot.ViewModels
{
    public class PostViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public int CommunityId { get; set; }
        public string CommunityName { get; set; }
        public string PostType { get; set; }
        public string Url { get; set; }
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
        public bool HasPoll { get; set; }
        public IEnumerable<PollOptionViewModel> PollOptions { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<MediaViewModel> Media { get; set; }
    }
}
