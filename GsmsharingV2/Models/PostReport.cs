namespace GsmsharingV2.Models
{
    public class PostReport
    {
        public int ReportID { get; set; }
        public int PostID { get; set; }
        public string ReporterUserId { get; set; }
        public string ReportReason { get; set; } // spam, harassment, inappropriate, copyright, misinformation, other
        public string ReportDetails { get; set; }
        public string Status { get; set; } = "pending"; // pending, reviewed, dismissed, action_taken
        public string ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public Post Post { get; set; }
        public ApplicationUser Reporter { get; set; }
        public ApplicationUser Reviewer { get; set; }
    }
}

