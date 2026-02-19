namespace Pdfpeaks.Models;

/// <summary>
/// Standard result object used for processing operations that return a file path.
/// </summary>
public class ProcessResult
{
    /// <summary>
    /// Whether the operation completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Full path of the output file produced by the operation.
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable message describing success or failure details.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="ProcessResult"/>.
    /// </summary>
    public ProcessResult()
    {
    }

    /// <summary>
    /// Convenience constructor.
    /// </summary>
    public ProcessResult(bool success, string outputPath, string message)
    {
        Success = success;
        OutputPath = outputPath;
        Message = message;
    }
}