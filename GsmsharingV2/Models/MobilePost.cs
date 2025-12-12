namespace GsmsharingV2.Models
{
    public class MobilePost
    {
        public int BlogId { get; set; }
        public string UserId { get; set; }
        public string FileId { get; set; } // varchar(255) in database
        public string Title { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public string MetaDis { get; set; }
        public string FileSize { get; set; } // nvarchar(255) in database
        public string FileName { get; set; }
        public string WebLinks { get; set; }
        public int? views { get; set; }
        public int? likes { get; set; }
        public int? dislikes { get; set; }
        public byte? publish { get; set; } // tinyint in database (0 or 1)
        public DateTime? CreationDate { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public virtual ICollection<BlogComment> BlogComments { get; set; }
        public virtual ICollection<BlogSpecContainer> BlogSpecContainers { get; set; }
    }
}

