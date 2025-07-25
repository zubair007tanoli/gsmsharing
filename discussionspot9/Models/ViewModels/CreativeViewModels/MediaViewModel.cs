namespace discussionspot9.Models.ViewModels.CreativeViewModels
{
    public class MediaViewModel
    {
        public int MediaId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public string? Caption { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }


    }
}
