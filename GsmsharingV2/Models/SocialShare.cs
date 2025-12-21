namespace GsmsharingV2.Models
{
    public class SocialShare
    {
        public int ShareID { get; set; }
        public string ContentType { get; set; } // post, comment, forum, blog, product
        public int ContentID { get; set; }
        public string Platform { get; set; } // facebook, twitter, linkedin, reddit, whatsapp, telegram, email, copy_link, other
        public string SharedBy { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public ApplicationUser User { get; set; }
    }
}

