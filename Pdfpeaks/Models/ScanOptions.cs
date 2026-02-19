namespace Pdfpeaks.Models
{
    /// <summary>
    /// Options passed to the document scanning pipeline.
    /// </summary>
    public class ScanOptions
    {
        public DocumentFilter Filter { get; set; }
        public int SharpenLevel { get; set; }
        public bool RemoveShadows { get; set; }
        public bool AutoRotate { get; set; }
        public bool PerspectiveCorrect { get; set; }
        public (float x, float y)[]? Corners { get; set; }
    }
}
