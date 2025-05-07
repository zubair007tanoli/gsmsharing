using discussionspot.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace discussionspot.ViewModels
{
    /// <summary>
    /// View model for displaying all posts
    /// </summary>
    public class AllPostsViewModel
    {
        /// <summary>
        /// List of posts to display
        /// </summary>
        public IEnumerable<PostViewModel> Posts { get; set; } = new List<PostViewModel>();

        /// <summary>
        /// Current sort method (hot, new, top)
        /// </summary>
        public string SortBy { get; set; } = "hot";

        /// <summary>
        /// Time filter (day, week, month, year, all)
        /// </summary>
        public string TimeFilter { get; set; } = "week";

        /// <summary>
        /// Filter by community ID
        /// </summary>
        public int? CommunityId { get; set; }

        /// <summary>
        /// Popular communities for sidebar
        /// </summary>
        public IEnumerable<CommunityViewModel> PopularCommunities { get; set; } = new List<CommunityViewModel>();

        /// <summary>
        /// All categories for sidebar navigation
        /// </summary>
        public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

        /// <summary>
        /// Pagination information
        /// </summary>
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPosts { get; set; } = 0;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Filter by category
        /// </summary>
        public Category? Category { get; set; }

        /// <summary>
        /// Filter by community
        /// </summary>
        public Community? Community { get; set; }

        /// <summary>
        /// Filter by tag
        /// </summary>
        public Tag? Tag { get; set; }

        /// <summary>
        /// Search query
        /// </summary>
        public string? SearchQuery { get; set; }

        /// <summary>
        /// Current sort method (for URL generation)
        /// </summary>
        public string? CurrentSort { get; set; }

        // View settings
        public string ViewMode { get; set; } = "card"; // card, compact
        public List<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "hot", Text = "Hot" },
            new SelectListItem { Value = "new", Text = "New" },
            new SelectListItem { Value = "top", Text = "Top" },
            new SelectListItem { Value = "comments", Text = "Most Comments" },
            new SelectListItem { Value = "rising", Text = "Rising" }
        };

        /// <summary>
        /// Gets the URL parameters for sorting (for generating links)
        /// </summary>
        public string GetSortUrlParams(string sort)
        {
            var parameters = new List<string> { $"sort={sort}" };

            if (this.Category is not null)
                parameters.Add($"categoryId={this.Category.CategoryId}");

            if (Community is not null)
                parameters.Add($"communityId={Community.CommunityId}");

            if (Tag is not null)
                parameters.Add($"tagId={Tag.TagId}");

            if (!string.IsNullOrEmpty(SearchQuery))
                parameters.Add($"q={SearchQuery}");

            return string.Join("&", parameters);
        }

        // Filter methods
        public void InitializeSortOptions()
        {
            foreach (var option in SortOptions)
            {
                option.Selected = option.Value == CurrentSort;
            }
        }

        /// <summary>
        /// Gets the URL parameters for pagination (for generating links)
        /// </summary>
        public string GetPageUrlParams(int page)
        {
            var parameters = new List<string> { $"page={page}" };

            if (!string.IsNullOrEmpty(CurrentSort) && CurrentSort != "hot")
                parameters.Add($"sort={CurrentSort}");

            if (this.Category is not null)
                parameters.Add($"categoryId={this.Category.CategoryId}");

            if (Community is not null)
                parameters.Add($"communityId={Community.CommunityId}");

            if (Tag is not null)
                parameters.Add($"tagId={Tag.TagId}");

            if (!string.IsNullOrEmpty(SearchQuery))
                parameters.Add($"q={SearchQuery}");

            return string.Join("&", parameters);
        }
    }
}