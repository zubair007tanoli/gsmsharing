namespace GsmsharingV2.DTOs
{
    public class CommentDto
    {
        public int CommentID { get; set; }
        public int PostID { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public int? ParentCommentID { get; set; }
        public string Content { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ReplyCount { get; set; }
        public int ReactionCount { get; set; }
        public List<CommentDto> Replies { get; set; } = new();
    }

    public class CreateCommentDto
    {
        public int PostID { get; set; }
        public int? ParentCommentID { get; set; }
        public string Content { get; set; }
    }

    public class UpdateCommentDto
    {
        public int CommentID { get; set; }
        public string Content { get; set; }
    }
}

