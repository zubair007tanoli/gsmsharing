public class PostDetailViewModelDeta
{
    // Existing properties
    public required string CommunitySlug { get; set; }
    public required string CommunityName { get; set; }

    // Add this property to fix the error
    public string CommunityUrl => $"/r/{CommunitySlug}";
}