using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class TagCreateViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Tag Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 2)]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
