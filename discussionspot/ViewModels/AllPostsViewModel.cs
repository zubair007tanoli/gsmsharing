using discussionspot.Models.Domain;

namespace discussionspot.ViewModels
{
    /// <summary>
    /// View model for displaying all posts
    /// </summary>
    public class AllPostsViewModel
    {
        // Existing code...

        /// <summary>
        /// Gets the URL parameters for sorting (for generating links)
        /// </summary>
        public string GetSortUrlParams(string sort)
        {
            var parameters = new List<string> { $"sort={sort}" };

            if (this.Category is not null) // Fixed null check for nullable type
                parameters.Add($"categoryId={this.Category.CategoryId}");

            if (Community is not null) // Fixed null check for nullable type
                parameters.Add($"communityId={Community.CommunityId}");

            if (Tag is not null) // Fixed null check for nullable type
                parameters.Add($"tagId={Tag.TagId}");

            if (!string.IsNullOrEmpty(SearchQuery))
                parameters.Add($"q={SearchQuery}");

            return string.Join("&", parameters);
        }

        /// <summary>
        /// Gets the URL parameters for pagination (for generating links)
        /// </summary>
        public string GetPageUrlParams(int page)
        {
            var parameters = new List<string> { $"page={page}" };

            if (!string.IsNullOrEmpty(CurrentSort) && CurrentSort != "hot")
                parameters.Add($"sort={CurrentSort}");

            if (this.Category is not null) // Fixed null check for nullable type
                parameters.Add($"categoryId={this.Category.CategoryId}");

            if (Community is not null) // Fixed null check for nullable type
                parameters.Add($"communityId={Community.CommunityId}");

            if (Tag is not null) // Fixed null check for nullable type
                parameters.Add($"tagId={Tag.TagId}");

            if (!string.IsNullOrEmpty(SearchQuery))
                parameters.Add($"q={SearchQuery}");

            return string.Join("&", parameters);
        }

        // Assuming Category is a property of this class
        public Category? Category { get; set; }
        public Community? Community { get; set; }
        public Tag? Tag { get; set; }
        public string? SearchQuery { get; set; }
        public string? CurrentSort { get; set; }
    }
}
