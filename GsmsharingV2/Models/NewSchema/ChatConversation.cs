namespace GsmsharingV2.Models.NewSchema
{
    public class ChatConversation
    {
        public long ConversationID { get; set; }
        public long? AdID { get; set; }
        public long BuyerID { get; set; }
        public long SellerID { get; set; }
        public DateTime LastMessageDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ClassifiedAd Ad { get; set; }
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}

