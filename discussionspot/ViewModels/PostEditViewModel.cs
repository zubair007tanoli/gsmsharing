using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    /// <summary>
    /// View model for editing a post
    /// </summary>
    public class PostEditViewModel
    {
        // Post ID
        [Required]
        public int PostId { get; set; }

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

        [Required]
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
        public bool IsPinned { get; set; } = false;

        // Poll post properties
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

        /// <summary>
        /// Initializes post type options
        /// </summary>
        public void InitializePostTypes()
        {
            PostTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "text", Text = "Text Post", Selected = PostType == "text" },
                new SelectListItem { Value = "link", Text = "Link Post", Selected = PostType == "link" },
                new SelectListItem { Value = "image", Text = "Image Post", Selected = PostType == "image" },
                new SelectListItem { Value = "video", Text = "Video Post", Selected = PostType == "video" },
                new SelectListItem { Value = "poll", Text = "Poll Post", Selected = PostType == "poll" }
            };
        }
    }
}
