using System;
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class AnnouncementViewModel
    {
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "info";

        public string Icon { get; set; } = "fa-info-circle";

        [Url(ErrorMessage = "Please enter a valid URL")]
        [StringLength(500)]
        public string? LinkUrl { get; set; }

        [StringLength(100)]
        public string? LinkText { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDismissible { get; set; } = true;

        [Range(0, 100, ErrorMessage = "Priority must be between 0 and 100")]
        public int Priority { get; set; } = 0;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}

