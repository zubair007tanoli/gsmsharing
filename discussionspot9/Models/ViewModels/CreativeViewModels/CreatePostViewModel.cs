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
        public string? Question { get; set; }
        public List<string> PollOptions { get; set; } = new();
        public bool AllowMultipleChoices { get; set; }
        public bool ShowResultsBeforeVoting { get; set; } = true;
        public bool ShowResultsBeforeEnd { get; set; } = true;
        public bool AllowAddingOptions { get; set; }
        public DateTime? PollEndDate { get; set; }
        public IFormFileCollection? MediaFiles { get; set; }
        public IFormFile? FeaturedImage { get; set; }
        public string? MediaCaption { get; set; }
        public string? MediaAltText { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? Keywords { get; set; }
        public IFormFile? OgImage { get; set; }
        public List<string> MediaUrls { get; set; } = new();
        public DateTime? PollExpiresAt { get; set; }
        public int MaxOptions { get; set; } = 10;
        public int MinOptions { get; set; } = 1;
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public List<CommunityViewModel> UserCommunities { get; set; } = new List<CommunityViewModel>();
        public List<CommunityViewModel> SuggestedCommunities { get; set; } = new List<CommunityViewModel>();
        
        // Override the default validation
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
        }
    }
}
