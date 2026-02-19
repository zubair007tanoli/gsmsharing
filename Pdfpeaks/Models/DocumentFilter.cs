namespace Pdfpeaks.Models
{
    /// <summary>
    /// Filters that can be applied during a document scan pipeline.
    /// </summary>
    public enum DocumentFilter
    {
        /// <summary>No filter applied; raw image returned.</summary>
        None,
        /// <summary>Enhanced filter (default) for better contrast/sharpness.</summary>
        Enhanced,
        /// <summary>Black &amp; white filter.</summary>
        BlackWhite
    }
}
