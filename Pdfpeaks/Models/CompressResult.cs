namespace Pdfpeaks.Models
{
    /// <summary>
    /// Extended result returned by compress operations.
    /// Contains additional size information beyond the basic <see cref="ProcessResult"/>.
    /// </summary>
    public class CompressResult : ProcessResult
    {
        /// <summary>Original file size in bytes before compression.</summary>
        public long OriginalBytes { get; set; }
        /// <summary>Resulting file size in bytes after compression.</summary>
        public long OutputBytes { get; set; }

        public CompressResult() { }

        public CompressResult(bool success, string outputPath, string message, long originalBytes, long outputBytes)
            : base(success, outputPath, message)
        {
            OriginalBytes = originalBytes;
            OutputBytes = outputBytes;
        }
    }
}
