using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels
{
    public class CreateStoryViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(300, ErrorMessage = "Title cannot exceed 300 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "Meta description cannot exceed 500 characters")]
        public string? MetaDescription { get; set; }

        [StringLength(500, ErrorMessage = "Meta keywords cannot exceed 500 characters")]
        public string? MetaKeywords { get; set; }

        [StringLength(100, ErrorMessage = "Publisher name cannot exceed 100 characters")]
        public string? PublisherName { get; set; }

        [StringLength(2048, ErrorMessage = "Poster image URL cannot exceed 2048 characters")]
        public string? PosterImageUrl { get; set; }

        [StringLength(2048, ErrorMessage = "Publisher logo URL cannot exceed 2048 characters")]
        public string? PublisherLogo { get; set; }

        [StringLength(2048, ErrorMessage = "Canonical URL cannot exceed 2048 characters")]
        public string? CanonicalUrl { get; set; }

        public string Status { get; set; } = "draft";
        public bool IsAmpEnabled { get; set; } = true;

        // Slides
        public List<CreateStorySlideViewModel> Slides { get; set; } = new List<CreateStorySlideViewModel>();
    }

    public class CreateStorySlideViewModel
    {
        public int? MediaId { get; set; }
        
        [StringLength(500, ErrorMessage = "Caption cannot exceed 500 characters")]
        public string? Caption { get; set; }

        [StringLength(200, ErrorMessage = "Headline cannot exceed 200 characters")]
        public string? Headline { get; set; }

        [StringLength(2000, ErrorMessage = "Text cannot exceed 2000 characters")]
        public string? Text { get; set; }

        [Range(1000, 30000, ErrorMessage = "Duration must be between 1000 and 30000 milliseconds")]
        public int Duration { get; set; } = 5000;

        [Required(ErrorMessage = "Order index is required")]
        public int OrderIndex { get; set; }

        [Required(ErrorMessage = "Slide type is required")]
        [StringLength(50, ErrorMessage = "Slide type cannot exceed 50 characters")]
        public string SlideType { get; set; } = "media";

        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Background color must be a valid hex color (e.g., #FF0000)")]
        public string? BackgroundColor { get; set; } = "#000000";

        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Text color must be a valid hex color (e.g., #FFFFFF)")]
        public string? TextColor { get; set; } = "#ffffff";

        [StringLength(10, ErrorMessage = "Font size cannot exceed 10 characters")]
        public string? FontSize { get; set; } = "24px";

        [Required(ErrorMessage = "Alignment is required")]
        [StringLength(20, ErrorMessage = "Alignment cannot exceed 20 characters")]
        public string Alignment { get; set; } = "center";

        // For form binding
        [Url(ErrorMessage = "Media URL must be a valid URL")]
        public string? MediaUrl { get; set; }
    }
}
