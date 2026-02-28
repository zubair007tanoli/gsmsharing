namespace Pdfpeaks.Models;

/// <summary>
/// Request model for merging PDF files
/// </summary>
public class MergePdfRequest
{
    /// <summary>
    /// List of file names to merge (files must already be uploaded)
    /// </summary>
    public List<string> FileNames { get; set; } = new();
    
    /// <summary>
    /// Optional sort order for the files (0-based indices)
    /// </summary>
    public List<int>? SortOrder { get; set; }
    
    /// <summary>
    /// Output file name for the merged PDF
    /// </summary>
    public string OutputFileName { get; set; } = "merged.pdf";
}
