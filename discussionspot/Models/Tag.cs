using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(120)]
        public string Slug { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int PostCount { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<PostTag> Posts { get; set; }
    }
}
