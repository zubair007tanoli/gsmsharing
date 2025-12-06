namespace GsmsharingV2.Models
{
    public class ForumThread
    {
        public int UserFourmID { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public string MetaDiscription { get; set; }
        public int? Views { get; set; }
        public int? Likes { get; set; }
        public int? Dislikes { get; set; }
        public int? ParantId { get; set; }
        public byte? Publish { get; set; }
        public DateTime? CreationDate { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public ICollection<ForumCategory> Categories { get; set; }
        public ICollection<ForumReply> Replies { get; set; }
    }
}

