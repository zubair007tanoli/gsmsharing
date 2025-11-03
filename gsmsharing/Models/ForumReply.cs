namespace gsmsharing.Models
{
    public class ForumReply
    {
        public int Id { get; set; }
        public int? ThreadId { get; set; }
        public string UserId { get; set; }
        public string ForumContent { get; set; }
        public int? Like { get; set; }
        public int? DisLike { get; set; }
        public int? Views { get; set; }
        public DateTime? PublishDate { get; set; }

        // Navigation properties
        public ForumThread Thread { get; set; }
        public ApplicationUser User { get; set; }
    }
}

