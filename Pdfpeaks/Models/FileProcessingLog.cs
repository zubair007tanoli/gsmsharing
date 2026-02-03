using System.ComponentModel.DataAnnotations;

namespace Pdfpeaks.Models;

/// <summary>
/// Log of file processing operations
/// </summary>
public class FileProcessingLog
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// User ID who performed the operation
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to user
    /// </summary>
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Type of operation: Merge, Split, Compress, Convert, etc.
    /// </summary>
    public FileOperationType OperationType { get; set; }

    /// <summary>
    /// Source file format
    /// </summary>
    public string? SourceFormat { get; set; }

    /// <summary>
    /// Target file format
    /// </summary>
    public string? TargetFormat { get; set; }

    /// <summary>
    /// Original file name
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// Original file size in bytes
    /// </summary>
    public long OriginalFileSize { get; set; }

    /// <summary>
    /// Processed file size in bytes
    /// </summary>
    public long ProcessedFileSize { get; set; }

    /// <summary>
    /// Compression ratio (if applicable)
    /// </summary>
    public decimal? CompressionRatio { get; set; }

    /// <summary>
    /// Number of pages (for PDF operations)
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// Processing status
    /// </summary>
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public long? ProcessingTimeMs { get; set; }

    /// <summary>
    /// IP address of the user
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Whether the file was downloaded
    /// </summary>
    public bool WasDownloaded { get; set; } = false;

    /// <summary>
    /// When the file was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the file expires (for temporary files)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Types of file operations
/// </summary>
public enum FileOperationType
{
    Merge = 1,
    Split = 2,
    Compress = 3,
    ConvertToWord = 4,
    ConvertToExcel = 5,
    ConvertToPowerPoint = 6,
    ConvertFromWord = 7,
    ConvertFromExcel = 8,
    ConvertFromPowerPoint = 9,
    ConvertToJpg = 10,
    ConvertFromJpg = 11,
    ConvertToPng = 12,
    Organize = 13,
    Edit = 14,
    Rotate = 15,
    Unlock = 16,
    Protect = 17,
    ImageResize = 18,
    ImageCrop = 19,
    ImageRotate = 20,
    ImageFlip = 21,
    ImageConvert = 22,
    ImageCompress = 23,
    ImageToPdf = 24,
    PdfToImage = 25
}

/// <summary>
/// Processing status
/// </summary>
public enum ProcessingStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
