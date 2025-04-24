using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(120)]
        public string Slug { get; set; }

        public string Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ParentCategoryId))]
        public virtual Category ParentCategory { get; set; }

        // Navigation properties
        public virtual ICollection<Category> ChildCategories { get; set; }
        public virtual ICollection<Community> Communities { get; set; }
    }
}
