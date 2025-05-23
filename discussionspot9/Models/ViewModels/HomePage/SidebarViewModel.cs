// Models/ViewModels/HomePage/SidebarViewModel.cs
namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// ViewModel for the homepage sidebar containing various widgets
    /// </summary>
    public class SidebarViewModel
    {
        /// <summary>
        /// Trending topics/posts
        /// </summary>
        public List<TrendingTopicViewModel> TrendingTopics { get; set; } = new();

        /// <summary>
        /// Online users information
        /// </summary>
        public OnlineUsersViewModel OnlineUsers { get; set; } = new();

        /// <summary>
        /// Forum statistics
        /// </summary>
        public ForumStatsViewModel ForumStats { get; set; } = new();

        /// <summary>
        /// Recently joined members
        /// </summary>
        public List<NewMemberViewModel> NewMembers { get; set; } = new();

        /// <summary>
        /// Whether the sidebar has any content to display
        /// </summary>
        public bool HasContent => TrendingTopics.Any() ||
                                 OnlineUsers.TotalOnline > 0 ||
                                 ForumStats.TotalMembers > 0 ||
                                 NewMembers.Any();

        /// <summary>
        /// Whether trending topics section should be displayed
        /// </summary>
        public bool HasTrendingTopics => TrendingTopics.Any();

        /// <summary>
        /// Whether online users section should be displayed
        /// </summary>
        public bool HasOnlineUsers => OnlineUsers.TotalOnline > 0;

        /// <summary>
        /// Whether forum stats section should be displayed
        /// </summary>
        public bool HasForumStats => ForumStats.TotalMembers > 0;

        /// <summary>
        /// Whether new members section should be displayed
        /// </summary>
        public bool HasNewMembers => NewMembers.Any();

        /// <summary>
        /// Get total count of all sidebar items for performance metrics
        /// </summary>
        public int TotalItemCount => TrendingTopics.Count + NewMembers.Count + 1; // +1 for stats/online users
    }
}