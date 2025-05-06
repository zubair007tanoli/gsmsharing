using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents a tag used to categorize posts
    /// </summary>
    public class Tag : BaseEntity
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tag Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Post Count")]
        public int PostCount { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<PostTag>? PostTags { get; set; }

        // Helper methods
        /// <summary>
        /// Generates a valid slug based on the tag name
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
        /// Gets the URL for browsing this tag
        /// </summary>
        [NotMapped]
        public string Url => $"/tag/{Slug}";
    }
}
