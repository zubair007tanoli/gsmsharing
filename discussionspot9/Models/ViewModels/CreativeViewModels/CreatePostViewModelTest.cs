using discussionspot9.Models.ViewModels.PollViewModels;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    [Flags]
    public enum PostContentTypes
    {
        None = 0,
        Text = 1,
        Link = 2,
        Image = 4,
        Video = 8,
        Poll = 16
    }
    public class CreatePostViewModelTest : IValidatableObject
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 300 characters.")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Post Type(s)")]
        [Required(ErrorMessage = "At least one post type must be selected.")]
        // Defines the primary and supplementary types of the post.
        // Can be a combination of "text", "link", "image", "video", "poll" using bitwise flags.
        public PostContentTypes PostType { get; set; } = PostContentTypes.Text;

        // Content for text posts. This is required if PostType includes 'Text'.
        [DataType(DataType.MultilineText)]
        public string? Content { get; set; }

        // URL for link posts. This is required if PostType includes 'Link'.
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string? Url { get; set; }

        // List of tags associated with the post.
        [Display(Name = "Tags")]
        public List<string> Tags { get; set; } = new();

        // Input field for tags (e.g., comma-separated string) to be parsed in the controller.
        public string TagsInput { get; set; } = string.Empty;

        // Indicates if the post contains Not Safe For Work content.
        [Display(Name = "NSFW")]
        public bool IsNSFW { get; set; }

        // Indicates if the post contains spoilers.
        [Display(Name = "Spoiler")]
        public bool IsSpoiler { get; set; }

        // Summary for the post, typically used for previews.
        [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters.")]
        public string? Summary { get; set; }

        // Status of the post (e.g., "published", "draft", "pending", "archived").
        [Display(Name = "Status")]
        public string Status { get; set; } = "published";

        // Indicates if the post should be pinned (e.g., to a user's profile or community).
        [Display(Name = "Pin to Profile")]
        public bool IsPinned { get; set; }

        // Indicates if comments on the post should be locked.
        [Display(Name = "Lock Comments")]
        public bool IsLocked { get; set; }

        // Optional publication date for scheduling posts.
        [Display(Name = "Publication Date")]
        [DataType(DataType.DateTime)]
        public DateTime? PublicationDate { get; set; }

        // --- Poll Configuration (Relevant if PostType includes "Poll") ---
        [Display(Name = "Poll")]
        public PollViewModel Poll { get; set; } = new PollViewModel();

        [Display(Name = "Poll Options")]
        public List<PollOptionViewModel> PollOptions { get; set; } = new();

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

        // --- Media Uploads (Relevant if PostType includes "Image" or "Video") ---
        // IFormFileCollection allows multiple files to be uploaded.
        [Display(Name = "Media Files")]
        public IFormFileCollection? MediaFiles { get; set; }

        // Used for a single featured image, if applicable (e.g., for link previews or general post thumbnail).
        [Display(Name = "Featured Image")]
        public IFormFile? FeaturedImage { get; set; }

        [Display(Name = "Media Caption")]
        [StringLength(500, ErrorMessage = "Caption cannot exceed 500 characters.")]
        public string? MediaCaption { get; set; }

        [Display(Name = "Alt Text")]
        [StringLength(500, ErrorMessage = "Alt text cannot exceed 500 characters.")]
        public string? MediaAltText { get; set; }

        // --- SEO Metadata (Optional for all post types) ---
        [Display(Name = "Meta Title")]
        [StringLength(200, ErrorMessage = "Meta title cannot exceed 200 characters.")]
        public string? MetaTitle { get; set; }

        [Display(Name = "Meta Description")]
        [StringLength(500, ErrorMessage = "Meta description cannot exceed 500 characters.")]
        public string? MetaDescription { get; set; }

        [Display(Name = "Canonical URL")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string? CanonicalUrl { get; set; }

        [Display(Name = "Keywords")]
        public string? Keywords { get; set; }

        [Display(Name = "Social Media Image")]
        public IFormFile? OgImage { get; set; }

        // --- Properties to be set by the Controller or Service Layer ---
        [Required(ErrorMessage = "Community is required.")]
        public int CommunityId { get; set; }
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        public string? UserId { get; set; } // User ID of the post creator

        // Custom validation logic based on the selected PostType.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // If no PostType is selected other than None, it's invalid.
            if (PostType == PostContentTypes.None)
            {
                yield return new ValidationResult("At least one post content type must be selected.", new[] { nameof(PostType) });
            }

            // Validate content based on included PostContentTypes
            if (PostType.HasFlag(PostContentTypes.Text))
            {
                if (string.IsNullOrWhiteSpace(Content))
                {
                    yield return new ValidationResult("Content is required for text content.", new[] { nameof(Content) });
                }
            }

            if (PostType.HasFlag(PostContentTypes.Link))
            {
                if (string.IsNullOrWhiteSpace(Url))
                {
                    yield return new ValidationResult("URL is required for link content.", new[] { nameof(Url) });
                }
                else if (!Uri.TryCreate(Url, UriKind.Absolute, out _)) // Basic URL format check
                {
                    yield return new ValidationResult("Please enter a valid URL.", new[] { nameof(Url) });
                }
            }

            // Validate media files if Image or Video content is selected
            if (PostType.HasFlag(PostContentTypes.Image) || PostType.HasFlag(PostContentTypes.Video))
            {
                if (MediaFiles == null || MediaFiles.Count == 0)
                {
                    yield return new ValidationResult("At least one media file is required for image or video content.", new[] { nameof(MediaFiles) });
                }
                else
                {
                    foreach (var file in MediaFiles)
                    {
                        // Example: Max file size 10MB
                        if (file.Length > 10 * 1024 * 1024)
                        {
                            yield return new ValidationResult($"File '{file.FileName}' exceeds the maximum allowed size (10MB).", new[] { nameof(MediaFiles) });
                        }

                        // Example: Only allow certain image/video types based on selected flag
                        if (PostType.HasFlag(PostContentTypes.Image))
                        {
                            var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                            if (!allowedImageTypes.Contains(file.ContentType))
                            {
                                yield return new ValidationResult($"File '{file.FileName}' is not a supported image type (JPEG, PNG, GIF).", new[] { nameof(MediaFiles) });
                            }
                        }
                        if (PostType.HasFlag(PostContentTypes.Video))
                        {
                            var allowedVideoTypes = new[] { "video/mp4", "video/webm" };
                            if (!allowedVideoTypes.Contains(file.ContentType))
                            {
                                yield return new ValidationResult($"File '{file.FileName}' is not a supported video type (MP4, WebM).", new[] { nameof(MediaFiles) });
                            }
                        }
                    }
                }
            }

            // Validate poll options if Poll content is selected
            if (PostType.HasFlag(PostContentTypes.Poll))
            {
                if (PollOptions == null || PollOptions.Count < 2)
                {
                    yield return new ValidationResult("At least 2 options are required for polls.", new[] { nameof(PollOptions) });
                }
                if (PollOptions.Any(string.IsNullOrWhiteSpace))
                {
                    yield return new ValidationResult("Poll options cannot be empty.", new[] { nameof(PollOptions) });
                }
                if (PollEndDate.HasValue && PollEndDate.Value <= DateTime.UtcNow)
                {
                    yield return new ValidationResult("Poll end date must be in the future.", new[] { nameof(PollEndDate) });
                }
            }

            // Optional: Validate FeaturedImage separately if it's meant to be a general thumbnail
            if (FeaturedImage != null)
            {
                // Example: Max file size for featured image 5MB
                if (FeaturedImage.Length > 5 * 1024 * 1024)
                {
                    yield return new ValidationResult($"Featured image '{FeaturedImage.FileName}' exceeds the maximum allowed size (5MB).", new[] { nameof(FeaturedImage) });
                }
                var allowedFeaturedImageTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedFeaturedImageTypes.Contains(FeaturedImage.ContentType))
                {
                    yield return new ValidationResult($"Featured image '{FeaturedImage.FileName}' is not a supported image type (JPEG, PNG).", new[] { nameof(FeaturedImage) });
                }
            }

            // Optional: Validate OgImage separately
            if (OgImage != null)
            {
                // Example: Max file size for OG image 5MB
                if (OgImage.Length > 5 * 1024 * 1024)
                {
                    yield return new ValidationResult($"Social media image '{OgImage.FileName}' exceeds the maximum allowed size (5MB).", new[] { nameof(OgImage) });
                }
                var allowedOgImageTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedOgImageTypes.Contains(OgImage.ContentType))
                {
                    yield return new ValidationResult($"Social media image '{OgImage.FileName}' is not a supported image type (JPEG, PNG).", new[] { nameof(OgImage) });
                }
            }
        }
    }
}
