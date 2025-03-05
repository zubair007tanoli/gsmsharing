namespace gsmsharing.ViewModels
{
    public class PostViewModelDisplay
    {
        public int PostID { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string FeaturedImage { get; set; }
        public string Keywords { get; set; }        
        public string Discription { get; set; }
        public string ViewCount { get; set; }
        public string AuthorName { get; set; }
        public string CommunityName { get; set; }
        public string CommunitySlug { get; set; }
        public string CreatedTime { get; set; }
        public string PublishedTime { get; set; }
        public ReactionSummary Reactions { get; set; }
    }
}
