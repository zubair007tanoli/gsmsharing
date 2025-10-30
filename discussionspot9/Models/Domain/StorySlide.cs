using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    public class StorySlide
    {
        public int StorySlideId { get; set; }
        
        [Required(ErrorMessage = "StoryId is required")]
        public int StoryId { get; set; }
        public int? MediaId { get; set; }
        public string? Caption { get; set; }
        public string? Headline { get; set; }
        public string? Text { get; set; }
        public int Duration { get; set; } = 5000; // Duration in milliseconds
        public int OrderIndex { get; set; }
        public string SlideType { get; set; } = "media"; // media, text, video, image
        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public string? FontSize { get; set; }
        public string? Alignment { get; set; } = "center";
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // New fields for redesigned stories
        [NotMapped]
        public string? PosterUrl { get; set; }
        [NotMapped]
        public string? CtaText { get; set; }
        [NotMapped]
        public string? CtaUrl { get; set; }
        [NotMapped]
        public string? SlideLayout { get; set; } // e.g., media-headline, media-body, quote, cta

        // Navigation properties
        public virtual Story Story { get; set; } = null!;
        public virtual Media? Media { get; set; }
    }
}
