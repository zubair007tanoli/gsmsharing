using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents a category in the community hierarchy
    /// </summary>
    public class Category : BaseEntity
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("ParentCategoryId")]
        public virtual Category? ParentCategory { get; set; }

        public virtual ICollection<Category>? ChildCategories { get; set; }

        public virtual ICollection<Community>? Communities { get; set; }

        // Helper methods
        /// <summary>
        /// Generates a valid slug based on the category name
        /// </summary>
        public void GenerateSlug()
        {
            if (string.IsNullOrEmpty(Slug) && !string.IsNullOrEmpty(Name))
            {
                Slug = Name.ToLower()
                    .Replace(" ", "-")
                    .Replace("&", "and")
                    .Replace("/", "-")
                    .Replace("\\", "-")
                    .Replace(".", "")
                    .Replace(",", "")
                    .Replace(":", "")
                    .Replace(";", "")
                    .Replace("?", "")
                    .Replace("!", "")
                    .Replace("'", "")
                    .Replace("\"", "")
                    .Replace("(", "")
                    .Replace(")", "");
            }
        }

        /// <summary>
        /// Gets the full category path (parent > child > etc)
        /// </summary>
        [NotMapped]
        public string FullPath
        {
            get
            {
                if (ParentCategory == null)
                    return Name;

                return $"{ParentCategory.FullPath} > {Name}";
            }
        }
    }
}
