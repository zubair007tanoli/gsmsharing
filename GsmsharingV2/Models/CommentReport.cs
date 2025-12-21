namespace GsmsharingV2.Models
{
    public class CommentReport
    {
        public int ReportID { get; set; }
        public int CommentID { get; set; }
        public string ReportedBy { get; set; }
        public string Reason { get; set; }
        public string Details { get; set; }
        public string Status { get; set; } = "pending";
        public string ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public Comment Comment { get; set; }
        public ApplicationUser Reporter { get; set; }
        public ApplicationUser Reviewer { get; set; }
    }
}

