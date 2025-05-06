using Microsoft.Data.SqlClient.Server;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents a community/subreddit where posts are shared
    /// </summary>
    public class Community : BaseEntity
    {
        [Key]
        public int CommunityId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Community Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        [Display(Name = "URL Slug")]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Page Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Full Description")]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Short Description")]
        public string? ShortDescription { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Creator")]
        public string? CreatorId { get; set; }

        [StringLength(20)]
        [Display(Name = "Community Type")]
        public string CommunityType { get; set; } = "public";  // public, private, restricted

        [StringLength(2048)]
        [Display(Name = "Icon URL")]
        public string? IconUrl { get; set; }

        [StringLength(2048)]
        [Display(Name = "Banner URL")]
        public string? BannerUrl { get; set; }

        [StringLength(20)]
        [Display(Name = "Theme Color")]
        public string? ThemeColor { get; set; }

        [Display(Name = "Member Count")]
        public int MemberCount { get; set; } = 0;

        [Display(Name = "Post Count")]
        public int PostCount { get; set; } = 0;

        [Display(Name = "Community Rules")]
        public string? Rules { get; set; }

        [Display(Name = "NSFW")]
        public bool IsNSFW { get; set; } = false;

        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("CreatorId")]
        public virtual ApplicationUsers? Creator { get; set; }

        public virtual ICollection<CommunityMember>? Members { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }

        public virtual SeoMetadata? SeoMetadata { get; set; }

        // Helper methods
        /// <summary>
        /// Generates a valid slug based on the community name
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
        /// Gets the icon initial if no icon URL is provided
        /// </summary>
        [NotMapped]
        public string IconInitial => !string.IsNullOrEmpty(Name)
            ? Name.Substring(0, 1).ToUpper()
            : "C";

        /// <summary>
        /// Gets formattable URL string for this community
        /// </summary>
        [NotMapped]
        public string Url => $"/r/{Slug}";
    }
}
