namespace discussionspot.ViewModels
{
    /// <summary>
    /// View model for poll configuration
    /// </summary>
    public class PollConfigurationViewModel
    {
        public int PostId { get; set; }
        public bool AllowMultipleChoices { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ShowResultsBeforeVoting { get; set; }
        public bool ShowResultsBeforeEnd { get; set; }
        public bool AllowAddingOptions { get; set; }
        public int MinOptions { get; set; }
        public int MaxOptions { get; set; }
        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.Now;
        public bool UserHasVoted { get; set; }
    }
}
