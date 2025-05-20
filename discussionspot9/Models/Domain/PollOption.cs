namespace discussionspot9.Models.Domain
{
    public class PollOption
    {
        public int PollOptionId { get; set; }
        public int PostId { get; set; }
        public string OptionText { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public int VoteCount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Post Post { get; set; } = null!;
        public virtual ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
    }
}
