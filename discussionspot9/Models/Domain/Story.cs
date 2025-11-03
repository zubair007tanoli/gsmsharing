using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    public class Story
    {
        public int StoryId { get; set; }
        
        // Link back to originating post for traffic and attribution
        public int? PostId { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(300, ErrorMessage = "Title cannot exceed 300 characters")]
        public string Title { get; set; } = null!;
        
        [Required(ErrorMessage = "Slug is required")]
        [StringLength(320, ErrorMessage = "Slug cannot exceed 320 characters")]
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public string? UserId { get; set; }
        public int? CommunityId { get; set; }
        public string Status { get; set; } = "draft"; // draft, published, archived
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ViewCount { get; set; }
        public int SlideCount { get; set; }
        public int TotalDuration { get; set; } // Total duration in milliseconds
        public string? PosterImageUrl { get; set; }
        public string? PublisherLogo { get; set; }
        public string? PublisherName { get; set; }
        public bool IsAmpEnabled { get; set; } = true;
        public string? CanonicalUrl { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        
        // Navigation properties
        public virtual Post? Post { get; set; }
        public virtual IdentityUser? User { get; set; }
        public virtual Community? Community { get; set; }
        
        [NotMapped]
        public string? BackgroundAudioUrl { get; set; }
        public virtual ICollection<StorySlide> Slides { get; set; } = new List<StorySlide>();
    }
}
