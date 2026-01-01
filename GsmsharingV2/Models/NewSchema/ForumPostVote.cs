namespace GsmsharingV2.Models.NewSchema
{
    /// <summary>
    /// User votes on forum posts (upvote/downvote) - Reddit-style voting
    /// </summary>
    public class ForumPostVote
    {
        public long VoteID { get; set; }
        public long ForumPostID { get; set; }
        public long UserId { get; set; }
        public string VoteType { get; set; } // 'upvote', 'downvote'
        public DateTime? CreatedAt { get; set; }
        
        // Navigation Properties
        public ForumPost ForumPost { get; set; }
    }
}







