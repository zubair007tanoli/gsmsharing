namespace discussionspot9.Models.ViewModels.PollViewModels
{
    public class PollOptionViewModel
    {
        public int PollOptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;  // Changed from Text to OptionText
        public int VoteCount { get; set; }
        public decimal VotePercentage { get; set; }
        public bool IsUserSelection { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public bool IsSelected { get; set; } = false;
        public bool AllowMultipleChoices { get; set; } = false;
        public bool HasUserVoted { get; set; }
  

    }
}
