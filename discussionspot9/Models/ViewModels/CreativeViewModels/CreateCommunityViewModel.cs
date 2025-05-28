using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CreateCommunityViewModel
    {
        [Required(ErrorMessage = "Community name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Name can only contain letters, numbers, and underscores")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [Display(Name = "Community Type")]
        public string CommunityType { get; set; } = "public";

        [Display(Name = "NSFW Content")]
        public bool IsNSFW { get; set; }

        [Display(Name = "Community Rules")]
        [DataType(DataType.MultilineText)]
        public string? Rules { get; set; }

        [Display(Name = "Banner Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? BannerUrl { get; set; }

        [Display(Name = "Icon Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? IconUrl { get; set; }

        // For the form
        public List<CategoryDropdownItem> AvailableCategories { get; set; } = new();

        // Set by controller
        public string? CreatorId { get; set; }
    }
}
