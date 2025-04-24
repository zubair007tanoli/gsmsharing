using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class SearchViewModel
    {
        [Required]
        [StringLength(100)]
        public string Query { get; set; }

        public string SearchType { get; set; } = "all"; // all, posts, communities, users

        public int? CommunityId { get; set; }

        public string SortBy { get; set; } = "relevance"; // relevance, new, top

        public string TimeFrame { get; set; } = "all"; // all, day, week, month, year

        public int Page { get; set; } = 1;
    }
}
