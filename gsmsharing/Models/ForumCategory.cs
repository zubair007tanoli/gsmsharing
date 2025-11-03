namespace gsmsharing.Models
{
    public class ForumCategory
    {
        public int CategoryId { get; set; }
        public int? UserFourmID { get; set; }
        public string CategoryName { get; set; }
        public int? Parantid { get; set; }
        public DateTime? CreationDate { get; set; }

        // Navigation properties
        public ForumThread Thread { get; set; }
    }
}

