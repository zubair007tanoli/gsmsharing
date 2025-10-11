namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class RightSidebarViewModel
    {
        public List<RelatedPostViewModel> RelatedPosts { get; set; } = new List<RelatedPostViewModel>();
    }

    public class RelatedPostViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string CommunityName { get; set; } = string.Empty;
        public int UpvoteCount { get; set; }
        public int CommentCount { get; set; }
        public string PostType { get; set; } = "text";

        public string PostTypeIcon
        {
            get
            {
                return PostType switch
                {
                    "image" => "fas fa-image",
                    "video" => "fas fa-video",
                    "link" => "fas fa-link",
                    "poll" => "fas fa-poll",
                    _ => "fas fa-file-alt"
                };
            }
        }
    }
}

