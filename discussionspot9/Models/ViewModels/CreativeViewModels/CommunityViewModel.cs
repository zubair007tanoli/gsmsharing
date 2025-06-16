namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CommunityViewModel : CommunityDetailViewModel
    {
      
        public int OnlineCount { get; set; }
        public int Depth { get; set; }   
        public bool IsMember { get; set; }
    }
}
