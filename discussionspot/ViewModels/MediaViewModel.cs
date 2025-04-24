namespace discussionspot.ViewModels
{
    public class MediaViewModel
    {
        public int MediaId { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public string MediaType { get; set; }
        public string ContentType { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; }
        public string Caption { get; set; }
        public string AltText { get; set; }
    }
}
