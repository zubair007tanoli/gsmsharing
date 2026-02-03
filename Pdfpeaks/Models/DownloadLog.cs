using System.ComponentModel.DataAnnotations;

namespace Pdfpeaks.Models;

/// <summary>
/// Log of file downloads
/// </summary>
public class DownloadLog
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// User ID who downloaded the file
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to user
    /// </summary>
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Related file processing log
    /// </summary>
    public long? FileProcessingLogId { get; set; }

    /// <summary>
    /// Navigation property to file processing log
    /// </summary>
    public FileProcessingLog? FileProcessingLog { get; set; }

    /// <summary>
    /// File name that was downloaded
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// File type: PDF, Image, etc.
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Download type: Single or Batch
    /// </summary>
    public DownloadType DownloadType { get; set; }

    /// <summary>
    /// Whether the user has a paid subscription
    /// </summary>
    public bool IsPaidUser { get; set; }

    /// <summary>
    /// IP address of the downloader
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Download timestamp
    /// </summary>
    public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types of downloads
/// </summary>
public enum DownloadType
{
    Single = 1,
    Batch = 2
}
