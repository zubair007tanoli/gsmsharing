using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class SeoMetadataViewModel
    {
        [Display(Name = "Meta Title")]
        [StringLength(200, ErrorMessage = "Meta title cannot exceed 200 characters")]
        public string? MetaTitle { get; set; }

        [Display(Name = "Meta Description")]
        [StringLength(500, ErrorMessage = "Meta description cannot exceed 500 characters")]
        public string? MetaDescription { get; set; }

        [Display(Name = "Canonical URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? CanonicalUrl { get; set; }

        [Display(Name = "Keywords")]
        [StringLength(500, ErrorMessage = "Keywords cannot exceed 500 characters")]
        public string? Keywords { get; set; }

        [Display(Name = "Open Graph Title")]
        [StringLength(200, ErrorMessage = "OG title cannot exceed 200 characters")]
        public string? OgTitle { get; set; }

        [Display(Name = "Open Graph Description")]
        [StringLength(500, ErrorMessage = "OG description cannot exceed 500 characters")]
        public string? OgDescription { get; set; }

        [Display(Name = "Social Media Image")]
        public IFormFile? OgImageFile { get; set; }

        [Display(Name = "Twitter Card Type")]
        [StringLength(20, ErrorMessage = "Twitter card type cannot exceed 20 characters")]
        public string? TwitterCard { get; set; } = "summary";

        [Display(Name = "Twitter Title")]
        [StringLength(200, ErrorMessage = "Twitter title cannot exceed 200 characters")]
        public string? TwitterTitle { get; set; }

        [Display(Name = "Twitter Description")]
        [StringLength(500, ErrorMessage = "Twitter description cannot exceed 500 characters")]
        public string? TwitterDescription { get; set; }

        // Hidden fields for URLs (populated after upload)
        public string? OgImageUrl { get; set; }
        public string? TwitterImageUrl { get; set; }
    }
}
