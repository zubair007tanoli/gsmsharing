using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class PostCreateViewModel
    {
        [Required]
        [StringLength(300, MinimumLength = 3)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Content")]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Community")]
        public int CommunityId { get; set; }

        [Required]
        [Display(Name = "Post Type")]
        public string PostType { get; set; } = "text";

        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [Display(Name = "Tags")]
        public string TagsString { get; set; }

        [Display(Name = "NSFW Content")]
        public bool IsNSFW { get; set; }

        [Display(Name = "Mark as Spoiler")]
        public bool IsSpoiler { get; set; }

        // Media uploads
        [Display(Name = "Images")]
        public List<IFormFile> ImageFiles { get; set; }

        // For poll posts
        [Display(Name = "Create Poll")]
        public bool HasPoll { get; set; }

        [Display(Name = "Poll Options")]
        public List<string> PollOptions { get; set; }

        [Display(Name = "Allow Multiple Choices")]
        public bool AllowMultipleChoices { get; set; }

        [Display(Name = "Poll End Date")]
        public DateTime? PollEndsAt { get; set; }

        [Display(Name = "Show Results Before Voting")]
        public bool ShowResultsBeforeVoting { get; set; } = true;

        // For dropdowns in the view
        public List<SelectListItem> Communities { get; set; }
        public List<SelectListItem> PostTypes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem("Text", "text"),
            new SelectListItem("Link", "link"),
            new SelectListItem("Image", "image"),
            new SelectListItem("Video", "video"),
            new SelectListItem("Poll", "poll")
        };
    }
}
