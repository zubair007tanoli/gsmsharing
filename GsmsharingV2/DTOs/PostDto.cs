namespace GsmsharingV2.DTOs
{
    public class PostDto
    {
        public int PostID { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
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
        public string CommunityName { get; set; }
        public string CommunitySlug { get; set; }
        public int CommentCount { get; set; }
        public int ReactionCount { get; set; }
    }

    public class CreatePostDto
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string Excerpt { get; set; }
        public string Tags { get; set; }
        public string FeaturedImage { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string OgTitle { get; set; }
        public string OgDescription { get; set; }
        public string OgImage { get; set; }
        public string CanonicalUrl { get; set; }
        public string FocusKeyword { get; set; }
        public string PostStatus { get; set; }
        public int? CommunityID { get; set; }
        public bool? AllowComments { get; set; }
        public bool? IsPromoted { get; set; }
        public bool? IsFeatured { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
    }

    public class UpdatePostDto
    {
        public int PostID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public string FeaturedImage { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string PostStatus { get; set; }
        public int? CommunityID { get; set; }
        public bool? AllowComments { get; set; }
        public bool? IsPromoted { get; set; }
        public bool? IsFeatured { get; set; }
    }
}

