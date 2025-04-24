using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class CategoryCreateViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 2)]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // For dropdown in the view
        public List<SelectListItem> ParentCategories { get; set; }
    }
}
