namespace discussionspot9.Models.Domain
{
    public class Award
    {
        public int AwardId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string IconUrl { get; set; } = null!;
        public int CoinCost { get; set; }
        public int GiveKarma { get; set; }
        public int ReceiveKarma { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<PostAward> PostAwards { get; set; } = new List<PostAward>();
        public virtual ICollection<CommentAward> CommentAwards { get; set; } = new List<CommentAward>();
    }
}
