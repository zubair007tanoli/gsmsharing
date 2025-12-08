namespace GsmsharingV2.Models
{
    public class AffiliationProduct
    {
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string ProductDiscription { get; set; }
        public string Keywords { get; set; }
        public string Content { get; set; }
        public int? Views { get; set; }
        public int? Likes { get; set; }
        public int? DisLikes { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? Price { get; set; }
        public string ImageLink { get; set; }
        public string BuyLink { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
    }
}

