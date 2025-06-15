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

        [Display(Name = "Tags (comma separated)")]
        public string? TagsInput { get; set; }

        [Display(Name = "NSFW")]
        public bool IsNSFW { get; set; }

        [Display(Name = "Spoiler")]
        public bool IsSpoiler { get; set; }

        // Poll fields
        [Display(Name = "Poll Options")]
        public List<string> PollOptions { get; set; } = new();

        [Display(Name = "Allow Multiple Choices")]
        public bool AllowMultipleChoices { get; set; }

        [Display(Name = "Poll End Date")]
        [DataType(DataType.DateTime)]
        public DateTime? PollEndDate { get; set; }

        // Set by controller
        public int CommunityId { get; set; }
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string? UserId { get; set; }

        // Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PostType == "text" && string.IsNullOrWhiteSpace(Content))
            {
                yield return new ValidationResult("Content is required for text posts", new[] { nameof(Content) });
            }

            if (PostType == "link" && string.IsNullOrWhiteSpace(Url))
            {
                yield return new ValidationResult("URL is required for link posts", new[] { nameof(Url) });
            }

            if (PostType == "poll" && (PollOptions == null || PollOptions.Count < 2))
            {
                yield return new ValidationResult("At least 2 options are required for polls", new[] { nameof(PollOptions) });
            }
        }
    }
}
