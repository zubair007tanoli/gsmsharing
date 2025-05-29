namespace discussionspot9.Models.Domain
{
    public class SavePostResult
    {
        public bool Success { get; set; }
        public bool IsSaved { get; set; }
        public string? Message { get; set; }
    }
}
