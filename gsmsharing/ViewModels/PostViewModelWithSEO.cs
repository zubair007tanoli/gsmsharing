namespace gsmsharing.ViewModels
{
    public class PostViewModelWithSEO
    {
        // Post properties
        public int PostID { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string FeaturedImage { get; set; }
        public int ViewCount { get; set; }
        public string FormattedViewCount { get; set; }
        public string PostStatus { get; set; }
        public bool IsPromoted { get; set; }
        public bool IsFeatured { get; set; }
        public bool AllowComments { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int CommunityID { get; set; }
        public string CreatedTime { get; set; }
        public string PublishedTime { get; set; }
        public string AuthorName { get; set; }
        public string CommunityName { get; set; }
        public string CommunitySlug { get; set; }
        public ReactionSummary Reactions { get; set; }
        public string CommentCount { get; set; }

        // SEO properties
        public int SEOId { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string OgTitle { get; set; }
        public string OgDescription { get; set; }
        public string OgImage { get; set; }
        public string TwitterCard { get; set; }
        public string TwitterTitle { get; set; }
        public string TwitterDescription { get; set; }
        public string TwitterImage { get; set; }
        public string CanonicalURL { get; set; }
        public string Robots { get; set; }
        public string Schema { get; set; }
    }
}
