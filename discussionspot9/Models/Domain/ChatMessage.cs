using Microsoft.AspNetCore.Identity;

namespace discussionspot9.Models.Domain
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public string SenderId { get; set; } = null!;
        public string? ReceiverId { get; set; } // Null for room messages
        public int? ChatRoomId { get; set; } // Null for direct messages
        public string Content { get; set; } = null!;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; } // image, file, etc.
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? ReplyToMessageId { get; set; } // For threading

        // Navigation properties
        public virtual IdentityUser Sender { get; set; } = null!;
        public virtual IdentityUser? Receiver { get; set; }
        public virtual ChatRoom? ChatRoom { get; set; }
    }
}

