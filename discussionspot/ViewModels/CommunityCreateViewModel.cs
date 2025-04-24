using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class CommunityCreateViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Community Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 3)]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Community Type")]
        public string CommunityType { get; set; } = "public";

        [Display(Name = "Icon")]
        public IFormFile IconImage { get; set; }

        [Display(Name = "Banner")]
        public IFormFile BannerImage { get; set; }

        [StringLength(20)]
        [Display(Name = "Theme Color")]
        public string ThemeColor { get; set; }

        [Display(Name = "Community Rules")]
        public string Rules { get; set; }

        [Display(Name = "Not Safe For Work")]
        public bool IsNSFW { get; set; } = false;

        // SEO fields
        [StringLength(200)]
        [Display(Name = "Meta Title")]
        public string MetaTitle { get; set; }

        [StringLength(500)]
        [Display(Name = "Meta Description")]
        public string MetaDescription { get; set; }

        [StringLength(500)]
        [Display(Name = "Keywords")]
        public string Keywords { get; set; }

        // For dropdowns in the view
        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> CommunityTypes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem("Public", "public"),
            new SelectListItem("Private", "private"),
            new SelectListItem("Restricted", "restricted")
        };
    }
}
