namespace GsmsharingV2.Models
{
    public class PostTag
    {
        public int PostID { get; set; }
        public int TagID { get; set; }

        // Navigation properties
        public Post Post { get; set; }
        public Tags Tag { get; set; }
    }
}

