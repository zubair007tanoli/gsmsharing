namespace discussionspot.ViewModels
{
    public class CommentAwardViewModel
    {
        public int CommentAwardId { get; set; }
        public int CommentId { get; set; }
        public int AwardId { get; set; }
        public string AwardName { get; set; }
        public string AwardIconUrl { get; set; }
        public string AwardedByUserId { get; set; }
        public string AwardedByUsername { get; set; }
        public DateTime AwardedAt { get; set; }
        public string Message { get; set; }
        public bool IsAnonymous { get; set; }
    }
}
