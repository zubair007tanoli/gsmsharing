namespace GsmsharingV2.Models
{
    public class Post
    {
        public int PostID { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Tags { get; set; }
        public string Content { get; set; }
        public string FeaturedImage { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string OgTitle { get; set; }
        public string OgDescription { get; set; }
        public string OgImage { get; set; }
        public int? ViewCount { get; set; }
        public string PostStatus { get; set; }
        public bool? IsPromoted { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? AllowComments { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int? CommunityID { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public Community Community { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Reaction> Reactions { get; set; }
        public ICollection<PostTag> PostTags { get; set; }
    }
}

