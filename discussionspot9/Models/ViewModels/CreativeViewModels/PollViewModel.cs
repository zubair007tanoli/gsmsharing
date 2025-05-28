namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class PollViewModel
    {
        public List<PollOptionViewModel> Options { get; set; } = new();
        public bool AllowMultipleChoices { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ShowResultsBeforeVoting { get; set; }
        public bool HasUserVoted { get; set; }
    }
}
