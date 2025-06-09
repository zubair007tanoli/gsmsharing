public class PostDetailViewModelDeta
{
    // Existing properties
    public string CommunitySlug { get; set; }
    public string CommunityName { get; set; }

    // Add this property to fix the error
    public string CommunityUrl => $"/r/{CommunitySlug}";
}