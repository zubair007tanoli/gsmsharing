namespace GsmsharingV2.Models.NewSchema
{
    public class FileCategory
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryID { get; set; }
        public string Slug { get; set; }

        // Navigation properties
        public FileCategory ParentCategory { get; set; }
        public ICollection<FileCategory> SubCategories { get; set; } = new List<FileCategory>();
        public ICollection<FileRepository> Files { get; set; } = new List<FileRepository>();
    }
}

