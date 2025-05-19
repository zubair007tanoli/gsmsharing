using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.ViewModels
{
    /// <summary>
    /// View model for creating/editing a community
    /// </summary>
    public class CommunityCreateViewModel
    {
        public int? CommunityId { get; set; } // Null for create, value for edit

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Community Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(120)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Only letters, numbers, underscores and hyphens are allowed in the slug.")]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Display Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Community Type")]
        public string CommunityType { get; set; } = "public"; // public, private, restricted

        [Display(Name = "Icon")]
        public IFormFile IconFile { get; set; }

        [Display(Name = "Banner")]
        public IFormFile BannerFile { get; set; }

        [StringLength(20)]
        [Display(Name = "Theme Color")]
        public string ThemeColor { get; set; }

        [Display(Name = "Rules")]
        public string Rules { get; set; }

        [Display(Name = "NSFW")]
        public bool IsNSFW { get; set; } = false;

        // For dropdown
        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> CommunityTypes { get; set; }

        // For edit mode
        public string IconUrl { get; set; }
        public string BannerUrl { get; set; }

        public void InitializeCommunityTypes()
        {
            CommunityTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "public", Text = "Public - Anyone can view and post" },
                new SelectListItem { Value = "restricted", Text = "Restricted - Anyone can view, but only approved users can post" },
                new SelectListItem { Value = "private", Text = "Private - Only approved users can view and post" }
            };
        }
    }
}
