using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace gsmsharing.ViewModels
{
    /// <summary>
    /// View model for creating a post
    /// </summary>
    public class PostCreateViewModel
    {
        // Basic post information
        [Required]
        [StringLength(300, MinimumLength = 3)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Content")]
        public string? Content { get; set; }

        [Required]
        [Display(Name = "Community")]
        public int CommunityId { get; set; }

        [StringLength(20)]
        [Display(Name = "Post Type")]
        public string PostType { get; set; } = "text"; // text, link, image, video, poll

        [Url]
        [StringLength(2048)]
        [Display(Name = "URL")]
        public string? Url { get; set; } // For link posts

        // Post settings
        [Display(Name = "NSFW")]
        public bool IsNSFW { get; set; } = false;

        [Display(Name = "Spoiler")]
        public bool IsSpoiler { get; set; } = false;

        [Display(Name = "Pinned")]
        public bool IsPined { get; set; } = false;

        [Display(Name = "Allow Comments")]
        public bool IsCommentAllowed { get; set; } = true;

        // Image post properties
        [Display(Name = "Images")]
        public List<IFormFile>? ImageFiles { get; set; }

        [StringLength(500)]
        [Display(Name = "Image Caption")]
        public string? ImageCaption { get; set; }

        // Poll post properties
        [Display(Name = "Has Poll")]
        public bool HasPoll { get; set; } = false;

        [Display(Name = "Poll Options")]
        public List<string>? PollOptions { get; set; }

        [Display(Name = "Multiple Choices")]
        public bool AllowMultipleChoices { get; set; } = false;

        [Display(Name = "Poll End Date")]
        public DateTime? PollEndsAt { get; set; }

        // Tags
        [Display(Name = "Tags")]
        public string? TagsString { get; set; }

        // SEO Properties
        [StringLength(200)]
        [Display(Name = "Meta Title")]
        public string? MetaTitle { get; set; }

        [StringLength(500)]
        [Display(Name = "Meta Description")]
        public string? MetaDescription { get; set; }

        [StringLength(500)]
        [Display(Name = "Keywords")]
        public string? Keywords { get; set; }

        // For select lists and dropdowns
        [Display(Name = "Communities")]
        public List<SelectListItem>? Communities { get; set; }

        [Display(Name = "Post Types")]
        public List<SelectListItem>? PostTypes { get; set; }

        // Return URL - Used primarily for redirecting
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Initializes post type options
        /// </summary>
        public void InitializePostTypes()
        {
            PostTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "text", Text = "Text Post" },
                new SelectListItem { Value = "link", Text = "Link Post" },
                new SelectListItem { Value = "image", Text = "Image Post" },
                new SelectListItem { Value = "video", Text = "Video Post" },
                new SelectListItem { Value = "poll", Text = "Poll Post" }
            };
        }
    }
}
