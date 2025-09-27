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
        
        // Validation
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

            // Poll-specific validation
            if (PostType == "poll")
            {
                if (string.IsNullOrEmpty(PollQuestion))
                {
                    yield return new ValidationResult("Poll question is required for poll posts", new[] { nameof(PollQuestion) });
                }

                if (PollOptions.Count < MinOptions)
                {
                    yield return new ValidationResult($"At least {MinOptions} poll options are required", new[] { nameof(PollOptions) });
                }

                if (PollOptions.Count > MaxOptions)
                {
                    yield return new ValidationResult($"Maximum {MaxOptions} poll options are allowed", new[] { nameof(PollOptions) });
                }

                if (PollEndDate.HasValue && PollEndDate.Value <= DateTime.UtcNow)
                {
                    yield return new ValidationResult("Poll end date must be in the future", new[] { nameof(PollEndDate) });
                }
            }
        }
    }
}
