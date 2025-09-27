namespace discussionspot9.Models.ViewModels.PollViewModels
{
    public class PollViewModel
    {
        public int PostId { get; set; }
        public string Question { get; set; }
        public List<PollOptionViewModel> Options { get; set; } = new();
        public bool AllowMultipleChoices { get; set; }
        public bool ShowResultsBeforeVoting { get; set; }
        public bool ShowResultsBeforeEnd { get; set; }
        public bool AllowAddingOptions { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsExpired { get; set; }
        public int TotalVotes { get; set; }
        public bool HasUserVoted { get; set; }
        public List<int> UserVotedOptionIds { get; set; } = new();
        public List<int> UserVotes { get; set; } = new();
        public int MinOptions { get; set; }
        public int MaxOptions { get; set; }
        // Add these properties if missing:
        public string? PollDescription { get; set; }
        public string? ClosedByUserId { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}