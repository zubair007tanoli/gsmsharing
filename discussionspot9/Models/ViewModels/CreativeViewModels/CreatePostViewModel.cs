using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 300 characters")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Post Type")]
        public string PostType { get; set; } = "text";

        [DataType(DataType.MultilineText)]
        public string? Content { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? Url { get; set; }

        [Display(Name = "Tags")]
        public List<string> Tags { get; set; } = new();

        public string TagsInput { get; set; } // Add this property

        [Display(Name = "NSFW")]
        public bool IsNSFW { get; set; }

        [Display(Name = "Spoiler")]
        public bool IsSpoiler { get; set; }

        // New fields from CreateTest.cshtml
        [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
        public string? Summary { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "published";

        [Display(Name = "Pin to Profile")]
        public bool IsPinned { get; set; }

        [Display(Name = "Lock Comments")]
        public bool IsLocked { get; set; }

        [Display(Name = "Publication Date")]
        [DataType(DataType.DateTime)]
        public DateTime? PublicationDate { get; set; }

        // Poll configuration
        [Display(Name = "Poll Options")]
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

        // Media uploads
        [Display(Name = "Media Files")]
        public IFormFileCollection? MediaFiles { get; set; }

        [Display(Name = "Featured Image")]
        public IFormFile? FeaturedImage { get; set; }

        [Display(Name = "Media Caption")]
        [StringLength(500, ErrorMessage = "Caption cannot exceed 500 characters")]
        public string? MediaCaption { get; set; }

        [Display(Name = "Alt Text")]
        [StringLength(500, ErrorMessage = "Alt text cannot exceed 500 characters")]
        public string? MediaAltText { get; set; }

        // SEO metadata
        [Display(Name = "Meta Title")]
        [StringLength(200, ErrorMessage = "Meta title cannot exceed 200 characters")]
        public string? MetaTitle { get; set; }

        [Display(Name = "Meta Description")]
        [StringLength(500, ErrorMessage = "Meta description cannot exceed 500 characters")]
        public string? MetaDescription { get; set; }

        [Display(Name = "Canonical URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? CanonicalUrl { get; set; }

        [Display(Name = "Keywords")]
        public string? Keywords { get; set; }

        [Display(Name = "Social Media Image")]
        public IFormFile? OgImage { get; set; }

        // Set by controller
        public int CommunityId { get; set; }
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string? UserId { get; set; }

        // Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Content validation
            if (PostType == "text" && string.IsNullOrWhiteSpace(Content))
            {
                yield return new ValidationResult("Content is required for text posts", new[] { nameof(Content) });
            }

            if (PostType == "link" && string.IsNullOrWhiteSpace(Url))
            {
                yield return new ValidationResult("URL is required for link posts", new[] { nameof(Url) });
            }

            // Media validation
            if (PostType == "image" && (MediaFiles == null || MediaFiles.Count == 0))
            {
                yield return new ValidationResult("At least one image is required", new[] { nameof(MediaFiles) });
            }

            if (PostType == "video" && (MediaFiles == null || MediaFiles.Count == 0))
            {
                yield return new ValidationResult("Video file is required", new[] { nameof(MediaFiles) });
            }

            // Poll validation
            if (PostType == "poll")
            {
                if (PollOptions == null || PollOptions.Count < 2)
                {
                    yield return new ValidationResult("At least 2 options are required for polls", new[] { nameof(PollOptions) });
                }

                if (PollOptions.Any(string.IsNullOrWhiteSpace))
                {
                    yield return new ValidationResult("Poll options cannot be empty", new[] { nameof(PollOptions) });
                }
            }
        }
    }
}
