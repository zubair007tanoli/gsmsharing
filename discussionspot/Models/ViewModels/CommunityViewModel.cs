namespace discussionspot.Models.ViewModels
{
    public class CommunityViewModel
    {
        public int CommunityId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string CommunityType { get; set; } // public, private, restricted
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string IconUrl { get; set; }
        public string BannerUrl { get; set; }
        public string ThemeColor { get; set; }
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsMember { get; set; }
        public string MemberRole { get; set; } // member, moderator, admin

        // Additional community metrics
        public int DailyActiveUsers { get; set; }
        public int OnlineUsers { get; set; }
        public double PostsPerDay { get; set; }

        // Community type readable description
        public string CommunityTypeDescription
        {
            get
            {
                return CommunityType switch
                {
                    "public" => "Public - Anyone can view and post",
                    "restricted" => "Restricted - Anyone can view, only approved users can post",
                    "private" => "Private - Only approved users can view and post",
                    _ => "Unknown"
                };
            }
        }

        // Community rules
        public List<CommunityRuleViewModel> Rules { get; set; } = new List<CommunityRuleViewModel>();

        // Moderators
        public List<UserViewModel> Moderators { get; set; } = new List<UserViewModel>();

        // Related communities
        public List<CommunityViewModel> RelatedCommunities { get; set; } = new List<CommunityViewModel>();
    }
}