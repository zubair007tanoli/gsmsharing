using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace discussionspot.Models.ViewModels
{
    /// <summary>
    /// View model for creating a post
    /// </summary>
    public class PostCreateViewModel
    {
        /// <summary>
        /// View model for creating a post
         /// </summary>
      
            public PostCreateViewModel()
            {
                // Initialize default values
                PostType = "text";
                IsCommentAllowed = true;

                // Initialize collections
                PollOptions = new List<string>();
                InitializePostTypes();
            }

            // Basic post information
            [Required(ErrorMessage = "Title is required")]
            [StringLength(300, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 300 characters")]
            [Display(Name = "Title")]
            public string Title { get; set; } = string.Empty;

            [Display(Name = "Content")]
            public string? Content { get; set; }

            [Required(ErrorMessage = "Please select a community")]
            [Display(Name = "Community")]
            public int CommunityId { get; set; }

            [Required]
            [StringLength(20)]
            [Display(Name = "Post Type")]
            public string PostType { get; set; } = "text"; // text, link, image, video, poll

            [Url(ErrorMessage = "Please enter a valid URL")]
            [StringLength(2048)]
            [Display(Name = "URL")]
            public string? Url { get; set; } // For link posts

            // Post settings
            [Display(Name = "NSFW")]
            public bool IsNSFW { get; set; } = false;

            [Display(Name = "Spoiler")]
            public bool IsSpoiler { get; set; } = false;

            [Display(Name = "Pin to Profile")]
            public bool IsPinned { get; set; } = false;

            [Display(Name = "Allow Comments")]
            public bool IsCommentAllowed { get; set; } = true;

            // Image post properties
            [Display(Name = "Images")]
            public List<IFormFile>? ImageFiles { get; set; }

            [StringLength(500)]
            [Display(Name = "Image Caption")]
            public string? ImageCaption { get; set; }

            // Video post properties
            [Display(Name = "Video")]
            public IFormFile? VideoFile { get; set; }

            [StringLength(500)]
            [Display(Name = "Video Caption")]
            public string? VideoCaption { get; set; }

            // Poll post properties
            [Display(Name = "Has Poll")]
            public bool HasPoll { get; set; } = false;

            [Display(Name = "Poll Options")]
            public List<string> PollOptions { get; set; } = new List<string>();

            [Display(Name = "Multiple Choices")]
            public bool AllowMultipleChoices { get; set; } = false;

            [Display(Name = "Show Results Before Voting")]
            public bool ShowResultsBeforeVoting { get; set; } = true;

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

            // Social media image
            [Display(Name = "Social Media Image")]
            public IFormFile? OgImageFile { get; set; }

            // For select lists and dropdowns
            [Display(Name = "Communities")]
            public List<SelectListItem>? Communities { get; set; }

            [Display(Name = "Post Types")]
            public List<SelectListItem>? PostTypes { get; set; }

            // For creating post slugs automatically
            public string? Slug { get; set; }

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

            /// <summary>
            /// Initialize communities dropdown. This would be populated from database.
            /// </summary>
            public void InitializeCommunities(IEnumerable<dynamic> communitiesFromDatabase)
            {
                Communities = communitiesFromDatabase.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
            }

            /// <summary>
            /// Method to generate slug from title
            /// </summary>
            public void GenerateSlug()
            {
                if (!string.IsNullOrEmpty(Title))
                {
                    Slug = Title.ToLower()
                        .Replace(" ", "-")
                        .Replace("_", "-")
                        .Replace(".", "")
                        .Replace(",", "")
                        .Replace("!", "")
                        .Replace("?", "")
                        .Replace(":", "")
                        .Replace(";", "")
                        .Replace("\"", "")
                        .Replace("'", "")
                        .Replace("(", "")
                        .Replace(")", "");
                }
            }

            /// <summary>
            /// Method to parse tags from string
            /// </summary>
            public List<string> GetTagsList()
            {
                if (string.IsNullOrEmpty(TagsString))
                    return new List<string>();

                return TagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Take(5) // Limit to 5 tags
                    .ToList();
            }
        }
    
}