namespace gsmsharing.Models
{
    public class UserProfile
    {
        int UserProfileID { get; set; }
        string UserId { get; set; }
        string Bio { get; set; }
        string ProfileImage { get; set; }
        string CoverImage { get; set; }
        string Location { get; set; }
        string Website { get; set; }
        string TwitterHandle { get; set; }
        string FacebookUrl { get; set; }
        string LinkedInUrl { get; set; }
        string DisplayName { get; set; }
        DateTime? LastLoginAt { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        bool IsActive { get; set; }
    }
}
