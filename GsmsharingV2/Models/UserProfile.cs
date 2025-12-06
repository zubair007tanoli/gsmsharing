namespace GsmsharingV2.Models
{
    public class UserProfile
    {
        public int UserProfileID { get; set; }
        public string UserId { get; set; }
        public string Bio { get; set; }
        public string ProfileImage { get; set; }
        public string CoverImage { get; set; }
        public string Location { get; set; }
        public string Website { get; set; }
        public string TwitterHandle { get; set; }
        public string FacebookUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string DisplayName { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
    }
}

