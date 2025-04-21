namespace discussionspot.Models.Posts
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty; // Default: Prevent null values
        public string Content { get; set; } = string.Empty; // Default: Prevent null values
        public string UserId { get; set; } // Allow null (foreign key constraint)
        public int CommunityId { get; set; } // Required
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default: Current UTC time
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Default: Current UTC time
        public int LikeCount { get; set; } = 0; // Default: No likes initially
        public int DislikeCount { get; set; } = 0; // Default: No dislikes initially
        public int CommentCount { get; set; } = 0; // Default: No comments initially
        public string Status { get; set; } = "published"; // Default value with allowed states
        public bool IsLocked { get; set; } = false; // Default: Unlocked post
    }
}
