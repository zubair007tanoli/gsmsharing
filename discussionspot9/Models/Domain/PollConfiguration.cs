namespace discussionspot9.Models.Domain
{
    public class PollConfiguration
    {
        public int PostId { get; set; }
        public bool AllowMultipleChoices { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ShowResultsBeforeVoting { get; set; }
        public bool ShowResultsBeforeEnd { get; set; }
        public bool AllowAddingOptions { get; set; }
        public int MinOptions { get; set; }
        public int MaxOptions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Post Post { get; set; } = null!;
    }
}
