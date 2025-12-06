namespace GsmsharingV2.Models
{
    public class ForumComment
    {
        public int Commentid { get; set; }
        public string UserId { get; set; }
        public string Comment { get; set; }
        public DateTime? CreationDate { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
    }
}

