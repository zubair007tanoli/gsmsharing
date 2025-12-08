namespace GsmsharingV2.DTOs
{
    public class ReactionDto
    {
        public int ReactionID { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int? PostID { get; set; }
        public int? CommentID { get; set; }
        public string ReactionType { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateReactionDto
    {
        public int? PostID { get; set; }
        public int? CommentID { get; set; }
        public string ReactionType { get; set; }
    }

    public class ReactionSummaryDto
    {
        public string ReactionType { get; set; }
        public int Count { get; set; }
        public bool UserHasReacted { get; set; }
    }
}

