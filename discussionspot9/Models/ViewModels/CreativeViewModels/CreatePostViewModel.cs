using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 300 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a community")]
        public int CommunityId { get; set; }

        [Display(Name = "Post Type")]
        public string PostType { get; set; } = "text";

        [DataType(DataType.MultilineText)]
        public string? Content { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? Url { get; set; }

        public List<string> Tags { get; set; } = new();

        public string? TagsInput { get; set; }

        public bool IsNSFW { get; set; }
        public bool IsSpoiler { get; set; }
        public string? Summary { get; set; }
        public string Status { get; set; } = "published";
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? PublicationDate { get; set; }

        // Updated Poll-related properties
        [Display(Name = "Poll Question")]
        [StringLength(500, ErrorMessage = "Poll question cannot exceed 500 characters")]
        public string? PollQuestion { get; set; }

        [Display(Name = "Poll Description")]
        [StringLength(1000, ErrorMessage = "Poll description cannot exceed 1000 characters")]
        public string? PollDescription { get; set; }

        public List<string> PollOptions { get; set; } = new();

        [Display(Name = "Allow Multiple Choices")]
        public bool AllowMultipleChoices { get; set; }

        [Display(Name = "Show Results Before Voting")]
        public bool ShowResultsBeforeVoting { get; set; } = true;

        [Display(Name = "Show Results Before End")]
        public bool ShowResultsBeforeEnd { get; set; } = true;

        [Display(Name = "Allow Adding Options")]
        public bool AllowAddingOptions { get; set; }

        [Display(Name = "Poll End Date")]
        [DataType(DataType.DateTime)]
        public DateTime? PollEndDate { get; set; }

        [Range(2, int.MaxValue, ErrorMessage = "Minimum options must be at least 2")]
        public int MinOptions { get; set; } = 2;

        [Range(2, int.MaxValue, ErrorMessage = "Maximum options must be greater than minimum options")]
        public int MaxOptions { get; set; } = 10;

        // Existing Media-related properties
        public IFormFileCollection? MediaFiles { get; set; }
        public IFormFile? FeaturedImage { get; set; }
        public string? MediaCaption { get; set; }
        public string? MediaAltText { get; set; }
        public List<string> MediaUrls { get; set; } = new();

        // SEO-related properties
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? Keywords { get; set; }
        public IFormFile? OgImage { get; set; }

        // Community-related properties
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public List<CommunityViewModel> UserCommunities { get; set; } = new();
        public List<CommunityViewModel> SuggestedCommunities { get; set; } = new();
        
        // SEO Analysis Result (not saved to DB, just for processing)
        public SeoMetadataViewModel? SeoMetadata { get; set; }
        
        // Story Generation Properties
        [Display(Name = "Auto-Generate Web Story")]
        public bool AutoGenerateStory { get; set; } = true;
        
        [Display(Name = "Use AI-Enhanced Content")]
        public bool UseAIContent { get; set; } = true;
        
        [Display(Name = "Story Style")]
        public string? StoryStyle { get; set; } = "informative";
        
        [Display(Name = "Story Length")]
        public string? StoryLength { get; set; } = "medium";
        
        [Display(Name = "Story Keywords")]
        public string? StoryKeywords { get; set; }
        
        // FIXED: Don't clear any content - users can add multiple content types in one post!
        // Only trim and clean up data, but preserve everything the user provides
        public void SanitizeDataByPostType()
        {
            // Just trim strings and convert empty to null - DON'T clear based on PostType
            Title = Title?.Trim();
            Content = string.IsNullOrWhiteSpace(Content) ? null : Content.Trim();
            Url = string.IsNullOrWhiteSpace(Url) ? null : Url.Trim();
            TagsInput = string.IsNullOrWhiteSpace(TagsInput) ? null : TagsInput.Trim();
            PollQuestion = string.IsNullOrWhiteSpace(PollQuestion) ? null : PollQuestion.Trim();
            PollDescription = string.IsNullOrWhiteSpace(PollDescription) ? null : PollDescription.Trim();
            
            // Only clean up poll options (remove empty ones)
            if (PollOptions != null && PollOptions.Any())
            {
                PollOptions = PollOptions
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.Trim())
                    .ToList();
            }
            
            // NOTE: We no longer clear Content, Url, or other fields based on PostType
            // Users should be able to create posts with multiple content types:
            // - Link post with content (commentary about the link)
            // - Image post with URL (product link + image)
            // - Poll with content (explanation of the poll)
            // The PostType is just a "primary type" indicator for display purposes
        }
        
        // Validation - RELAXED to allow multi-content posts
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Title))
            {
                yield return new ValidationResult("Title is required", new[] { nameof(Title) });
            }
            
            if (CommunityId <= 0)
            {
                yield return new ValidationResult("Please select a community", new[] { nameof(CommunityId) });
            }

            // CHANGED: Don't validate based on PostType anymore
            // Users can add any combination of content
            
            // If URL is provided, validate it's a proper URL (but it's not required)
            if (!string.IsNullOrWhiteSpace(Url))
            {
                if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
                {
                    yield return new ValidationResult("Please enter a valid URL", new[] { nameof(Url) });
                }
            }
            
            // If poll options are provided, validate them (but they're not required)
            var validOptions = PollOptions?.Where(o => !string.IsNullOrWhiteSpace(o)).Count() ?? 0;
            if (validOptions > 0)
            {
                // Only validate if user actually added poll options
                if (!string.IsNullOrEmpty(PollQuestion))
                {
                    // User is trying to create a poll
                    if (validOptions < 2)
                    {
                        yield return new ValidationResult("At least 2 poll options are required for polls", new[] { nameof(PollOptions) });
                    }

                    if (validOptions > MaxOptions)
                    {
                        yield return new ValidationResult($"Maximum {MaxOptions} poll options are allowed", new[] { nameof(PollOptions) });
                    }

                    if (PollEndDate.HasValue && PollEndDate.Value <= DateTime.UtcNow)
                    {
                        yield return new ValidationResult("Poll end date must be in the future", new[] { nameof(PollEndDate) });
                    }
                }
            }
            
            // NOTE: We removed PostType-based validation
            // This allows users to create posts with multiple content types:
            // - Content + URL + Images + Poll all together
            // The PostType field is just a hint for display priority
        }
    }
}
