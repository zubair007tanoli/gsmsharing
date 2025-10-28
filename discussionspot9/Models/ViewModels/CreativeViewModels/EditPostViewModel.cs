using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class EditPostViewModel
    {
        public int PostId { get; set; }
        
        [Required]
        [StringLength(300, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;
        
        public string? Content { get; set; }
        
        [Required]
        public int CommunityId { get; set; }
        
        public string? CommunitySlug { get; set; }
        
        public string PostType { get; set; } = "text";
        
        [Url]
        public string? Url { get; set; }
        
        // Post settings
        public bool IsNSFW { get; set; }
        public bool IsSpoiler { get; set; }
        public bool IsPinned { get; set; }
        public bool AllowComments { get; set; } = true;
        
        // Status
        public string Status { get; set; } = "published";
        
        // Media handling
        public List<IFormFile>? MediaFiles { get; set; }
        public List<string>? MediaUrls { get; set; }
        public List<string>? ExistingMediaUrls { get; set; }
        
        // Image action: keep, replace, remove
        public string ImageAction { get; set; } = "keep";
        
        // Tags
        public string? TagsInput { get; set; }
        
        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? Keywords { get; set; }
    }
}

