namespace GsmsharingV2.Models.NewSchema
{
    public class ChatMessage
    {
        public long MessageID { get; set; }
        public long ConversationID { get; set; }
        public long SenderID { get; set; }
        public string MessageContent { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ChatConversation Conversation { get; set; }
    }
}

