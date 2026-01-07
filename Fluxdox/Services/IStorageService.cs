using System.Threading.Tasks;

namespace Fluxdox.Services
{
    /// <summary>
    /// Defines the contract for interacting with file storage (e.g., S3/Blob Storage).
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Generates a presigned URL for direct client-side upload to storage.
        /// </summary>
        /// <param name="key">The unique key/path for the file in storage.</param>
        /// <param name="durationMinutes">The duration for which the URL is valid.</param>
        /// <returns>A presigned URL string.</returns>
        Task<string> GetPresignedUploadUrlAsync(string key, int durationMinutes = 5);
        
        // Additional methods like DownloadAsync, DeleteAsync would be here
    }
}