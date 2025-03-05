namespace gsmsharing.ViewModels
{
    public class ReactionSummary
    {
        public int TotalReactions { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }     
        public string FormattedLikeCount { get; set; }
        public string FormattedDislikeCount { get; set; }      
    }
}
