namespace discussionspot.ViewModels
{
    public class CommunityEditViewModel : CommunityCreateViewModel
    {
        public int CommunityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CurrentIconUrl { get; set; }
        public string CurrentBannerUrl { get; set; }
    }
}
