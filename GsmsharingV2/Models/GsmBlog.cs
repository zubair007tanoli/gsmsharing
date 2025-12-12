namespace GsmsharingV2.Models
{
    public class GsmBlog
    {
        public int BlogId { get; set; }
        public string UserId { get; set; }
        public string BlogTitle { get; set; }
        public string BlogDiscription { get; set; }
        public string BlogKeywords { get; set; }
        public string BlogContent { get; set; }
        public int? BlogViews { get; set; }
        public int? BlogLikes { get; set; }
        public int? BlogDisLikes { get; set; }
        public bool? Publish { get; set; }
        public DateTime? PublishDate { get; set; }
        public int? CategoryId { get; set; }
        public string ThumbNailLink { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public virtual ICollection<BlogSEO> BlogSEO { get; set; }
    }
}

