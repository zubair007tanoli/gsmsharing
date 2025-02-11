using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace gsmsharing.ViewModels
{
    public class CommunityViewModel
    {
        [Required(ErrorMessage = "Community name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 5 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category selection is required")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [StringLength(2000, ErrorMessage = "Rules cannot exceed 2000 characters")]
        public string Rules { get; set; }

        [Display(Name = "Cover Image")]
        public IFormFile CoverImage { get; set; }

        [Display(Name = "Icon Image")]
        public IFormFile IconImage { get; set; }
      
        [RegularExpression(@"^[a-zA-Z0-9-]+$", ErrorMessage = "URL slug can only contain letters, numbers, and hyphens")]
        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters")]
        public string Slug { get; set; }

        public bool IsPrivate { get; set; }
        public bool IsVerified { get; set; }

        // SEO Properties
        [StringLength(60, ErrorMessage = "Meta title cannot exceed 60 characters")]
        public string MetaTitle { get; set; }

        [StringLength(160, ErrorMessage = "Meta description cannot exceed 160 characters")]
        public string MetaDescription { get; set; }

        [StringLength(60, ErrorMessage = "OG title cannot exceed 60 characters")]
        public string OgTitle { get; set; }

        // Navigation property for dropdown
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
