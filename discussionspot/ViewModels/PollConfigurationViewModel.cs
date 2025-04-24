namespace discussionspot.ViewModels
{
    public class PollConfigurationViewModel
    {
        public bool AllowMultipleChoices { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ShowResultsBeforeVoting { get; set; }
        public bool ShowResultsBeforeEnd { get; set; }
        public bool AllowAddingOptions { get; set; }
        public int TotalVotes { get; set; }
        public bool IsExpired { get; set; }
    }
}
