// Models/ViewModels/HomePage/HomePageViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// Main ViewModel for the homepage containing all sections
    /// </summary>
    public class HomePageViewModel
    {
        /// <summary>
        /// Random posts displayed at the top of the homepage
        /// </summary>
        public List<RandomPostViewModel> RandomPosts { get; set; } = new();

        /// <summary>
        /// Categories with their statistics
        /// </summary>
        public List<CategoryViewModel> Categories { get; set; } = new();

        /// <summary>
        /// Recent discussions/posts
        /// </summary>
        public List<RecentPostViewModel> RecentPosts { get; set; } = new();

        /// <summary>
        /// Sidebar data including trending topics, online users, etc.
        /// </summary>
        public SidebarViewModel Sidebar { get; set; } = new();

        /// <summary>
        /// Check if the homepage has any content to display
        /// </summary>
        public bool HasContent => RandomPosts.Any() || Categories.Any() || RecentPosts.Any();

        /// <summary>
        /// Get total number of items loaded for performance metrics
        /// </summary>
        public int TotalItemsLoaded => RandomPosts.Count + Categories.Count + RecentPosts.Count;
    }
}