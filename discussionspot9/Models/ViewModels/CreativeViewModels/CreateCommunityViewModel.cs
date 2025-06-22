using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

        [Required(ErrorMessage = "Short description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Short description must be between 10 and 500 characters")]
        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        [Display(Name = "Full Description")]
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

        [Display(Name = "Theme Color")]
        public string ThemeColor { get; set; } = "#0079D3";

        // File upload properties
        [Display(Name = "Community Icon")]
        public IFormFile? IconFile { get; set; }

        [Display(Name = "Community Banner")]
        public IFormFile? BannerFile { get; set; }

        // URL properties (for storing the uploaded file URLs)
        [Display(Name = "Banner Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? BannerUrl { get; set; }

        [Display(Name = "Icon Image URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? IconUrl { get; set; }

        // Generated slug
        public string? Slug { get; set; }

        // For the form
        public List<CategoryDropdownItem> AvailableCategories { get; set; } = new();

        // Set by controller
        public string? CreatorId { get; set; }
    }
}