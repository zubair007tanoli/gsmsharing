using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class PostEditViewModel
    {
        public int PostId { get; set; }

        [Required]
        [StringLength(300, MinimumLength = 3)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Content")]
        public string Content { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [Display(Name = "Tags")]
        public string TagsString { get; set; }

        [Display(Name = "NSFW Content")]
        public bool IsNSFW { get; set; }

        [Display(Name = "Mark as Spoiler")]
        public bool IsSpoiler { get; set; }

        // Existing images
        public List<MediaViewModel> ExistingMedia { get; set; }

        // New media uploads
        [Display(Name = "Add Images")]
        public List<IFormFile> NewImageFiles { get; set; }

        // For poll posts
        public bool HasPoll { get; set; }
        public List<PollOptionEditViewModel> PollOptions { get; set; }
        public bool AllowMultipleChoices { get; set; }
        public DateTime? PollEndsAt { get; set; }
        public bool ShowResultsBeforeVoting { get; set; }

        // SEO fields
        [StringLength(200)]
        [Display(Name = "Meta Title")]
        public string MetaTitle { get; set; }

        [StringLength(500)]
        [Display(Name = "Meta Description")]
        public string MetaDescription { get; set; }
    }
}
