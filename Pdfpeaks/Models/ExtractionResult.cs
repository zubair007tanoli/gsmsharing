namespace Pdfpeaks.Models;

/// <summary>
/// Result model for PDF text extraction operations
/// </summary>
public class ExtractionResult
{
    /// <summary>
    /// Indicates whether the extraction was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Total number of pages in the PDF
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Total character count in the extracted text
    /// </summary>
    public int CharCount { get; set; }

    /// <summary>
    /// Total word count in the extracted text
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// Full concatenated text from all pages
    /// </summary>
    public string FullText { get; set; } = string.Empty;

    /// <summary>
    /// Text from each page keyed by page number (1-indexed)
    /// </summary>
    public Dictionary<int, string> PageTexts { get; set; } = new();

    /// <summary>
    /// Error message if extraction failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
