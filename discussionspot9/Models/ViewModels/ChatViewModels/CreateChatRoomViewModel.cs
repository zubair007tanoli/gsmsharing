using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.ChatViewModels
{
    public class CreateChatRoomViewModel
    {
        [Required(ErrorMessage = "Room name is required")]
        [StringLength(100, ErrorMessage = "Room name cannot exceed 100 characters")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = true;
    }
}
