using discussionspot9.Models.ViewModels.CreativeViewModels;

namespace discussionspot9.Interfaces
{
    /// <summary>
    /// Interface for background SEO processing service
    /// </summary>
    public interface IBackgroundSeoService
    {
        /// <summary>
        /// Process SEO optimization for a post in the background
        /// </summary>
        /// <param name="postId">The post ID to optimize</param>
        /// <param name="model">The original post view model</param>
        /// <param name="communityId">The community ID</param>
        void ProcessPostSeoAsync(int postId, CreatePostViewModel model, int communityId);

        /// <summary>
        /// Batch process SEO for multiple posts
        /// </summary>
        /// <param name="postIds">List of post IDs to process</param>
        void ProcessMultiplePostsSeoAsync(List<int> postIds);
    }
}

