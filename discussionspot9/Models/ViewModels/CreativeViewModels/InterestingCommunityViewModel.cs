namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class InterestingCommunityViewModel
    {
        public int CommunityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MemberCount { get; set; }
        public int PostCount { get; set; }
        public string? IconUrl { get; set; }
        public string CategoryName { get; set; } = "General";

        public string FormattedMemberCount
        {
            get
            {
                if (MemberCount >= 1000000)
                    return $"{MemberCount / 1000000.0:0.#}M";
                if (MemberCount >= 1000)
                    return $"{MemberCount / 1000.0:0.#}K";
                return MemberCount.ToString();
            }
        }

        public string FormattedPostCount
        {
            get
            {
                if (PostCount >= 1000000)
                    return $"{PostCount / 1000000.0:0.#}M";
                if (PostCount >= 1000)
                    return $"{PostCount / 1000.0:0.#}K";
                return PostCount.ToString();
            }
        }
    }
}

