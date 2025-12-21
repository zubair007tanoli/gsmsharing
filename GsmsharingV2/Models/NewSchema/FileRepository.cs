namespace GsmsharingV2.Models.NewSchema
{
    public class FileRepository
    {
        public long FileID { get; set; }
        public long? UserID { get; set; }
        public int? CategoryID { get; set; }

        public string FileName { get; set; }
        public string Description { get; set; }

        // Storage Logic
        public string StorageProvider { get; set; } = "GoogleDrive";
        public string ExternalUrl { get; set; }
        public string FilePassword { get; set; }

        // Technical Data
        public string FileSize { get; set; }
        public string MD5Checksum { get; set; }
        public string AndroidVersion { get; set; }
        public DateTime? SecurityPatchLevel { get; set; }

        // Access
        public bool IsPremium { get; set; } = false;
        public int CreditCost { get; set; } = 0;
        public decimal? PriceDisplay { get; set; }

        public int DownloadCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public FileCategory Category { get; set; }
    }
}

