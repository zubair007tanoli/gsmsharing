using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace gsmsharing.ViewModels
{
    public class PostViewModel
    {
     
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Tags is required")]
        [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
        public string Tags { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Community selection is required")]
        public int CommunityID { get; set; }

        [Display(Name = "Featured Image")]
        public IFormFile FeaturedImage { get; set; }

        public string FeaturedImagePath { get; set; }

        public string FeaturedImageUrl { get; set; }

        public bool AllowComments { get; set; } = true;

        // SEO Properties
        [StringLength(60, ErrorMessage = "Meta title cannot exceed 60 characters")]
        public string MetaTitle { get; set; }

        [StringLength(160, ErrorMessage = "Meta description cannot exceed 160 characters")]
        public string MetaDescription { get; set; } 

        [StringLength(60, ErrorMessage = "OG title cannot exceed 60 characters")]
        public string OgTitle { get; set; }

        [StringLength(160, ErrorMessage = "OG description cannot exceed 160 characters")]
        public string OgDescription { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9-]+$", ErrorMessage = "URL slug can only contain letters, numbers, and hyphens")]
        public string Slug { get; set; }

        public string PostStatus { get; set; } // Draft or Published

        // Navigation property for dropdown
        public IEnumerable<SelectListItem> Communities { get; set; }
    }
}
