namespace gsmsharing.Models.APIModels
{
    public class AiContentResult
    {
        public string OriginalTitle { get; set; }
        public string Title { get; set; }        // Add this property
        public string Description { get; set; }
        public string[] Keywords { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
