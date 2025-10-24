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
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
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
        public string Headline { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public int OrderIndex { get; set; }
        public int SlideOrder { get; set; }
        public string SlideType { get; set; } = string.Empty;
        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public int Duration { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
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
