namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class StoryViewModel
    {
        public int StoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string PostTitle { get; set; } = string.Empty;
        public string PostSlug { get; set; } = string.Empty;
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public int SlideCount { get; set; }
        public string? PosterImageUrl { get; set; }
    }

    public class StoryDetailViewModel
    {
        public int StoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string PostTitle { get; set; } = string.Empty;
        public string PostSlug { get; set; } = string.Empty;
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public List<StorySlideViewModel> Slides { get; set; } = new();
    }

    public class StoryEditViewModel
    {
        public int StoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        
        // Story-level SEO and metadata fields
        public string? PosterImageUrl { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? PublisherName { get; set; }
        public string? PublisherLogo { get; set; }
        
        public List<StorySlideEditViewModel> Slides { get; set; } = new();
    }

    public class StorySlideViewModel
    {
        public int SlideId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int SlideOrder { get; set; }
        public string SlideType { get; set; } = string.Empty;
    }

    public class StorySlideEditViewModel
    {
        public int StorySlideId { get; set; }
        public int SlideId { get; set; }
        
        // Core slide fields (matching StorySlide model)
        public string Headline { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public int? MediaId { get; set; }
        public int OrderIndex { get; set; }
        public int SlideOrder { get; set; }
        public string SlideType { get; set; } = string.Empty;
        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public string? FontSize { get; set; }
        public string? Alignment { get; set; }
        public int Duration { get; set; }
        
        // Legacy/backward compatibility fields
        public string Title { get; set; } = string.Empty; // Maps to Headline
        public string Content { get; set; } = string.Empty; // Maps to Text
        public string? ImageUrl { get; set; } // Maps to MediaUrl
        public string? BackgroundImageUrl { get; set; } // Not used in StorySlide, but kept for view compatibility
        public string? LinkUrl { get; set; } // For links: maps to MediaUrl when MediaType is internal_link/external_link
    }

    public class StoriesIndexViewModel
    {
        public List<StoryViewModel> Stories { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalStories { get; set; }
    }

    public class CreateStoryViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Style { get; set; } = "informative";
        public string Length { get; set; } = "medium";
        public string? Keywords { get; set; }
        public bool UseAI { get; set; } = true;
        public bool AutoGenerate { get; set; } = true;
    }
}
