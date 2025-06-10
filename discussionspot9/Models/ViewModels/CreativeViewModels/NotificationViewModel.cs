using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class NotificationViewModel
    {
        public Guid NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(500)]
        public string? Message { get; set; }

        [StringLength(50)]
        public string? EntityType { get; set; }

        [StringLength(450)]
        public string? EntityId { get; set; }

        public bool IsRead { get; set; }

        public string? Url { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }
}
