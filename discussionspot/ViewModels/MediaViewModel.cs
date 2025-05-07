namespace discussionspot.ViewModels
{
    public class MediaViewModel
    {
        public int MediaId { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public string MediaType { get; set; } // image, video, document, audio
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public long? FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Duration { get; set; } // For video/audio in seconds
        public string Caption { get; set; }
        public string AltText { get; set; }
    }
}
