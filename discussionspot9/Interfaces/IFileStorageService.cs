using Microsoft.AspNetCore.Http;

namespace discussionspot9.Interfaces
{
    /// <summary>
    /// Service for handling file uploads, storage, and management
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves an uploaded file to the specified folder
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="folder">Subfolder within uploads directory (e.g., "posts", "communities/icons")</param>
        /// <param name="customFileName">Optional custom filename (without extension)</param>
        /// <returns>Relative URL to the saved file (e.g., "/uploads/posts/abc123.jpg")</returns>
        Task<string> SaveFileAsync(IFormFile file, string folder, string? customFileName = null);

        /// <summary>
        /// Saves an image file with optional resizing
        /// </summary>
        /// <param name="file">The uploaded image file</param>
        /// <param name="folder">Subfolder within uploads directory</param>
        /// <param name="maxWidth">Maximum width in pixels (maintains aspect ratio)</param>
        /// <param name="maxHeight">Maximum height in pixels (maintains aspect ratio)</param>
        /// <returns>Relative URL to the saved image</returns>
        Task<string> SaveImageAsync(IFormFile file, string folder, int? maxWidth = null, int? maxHeight = null);

        /// <summary>
        /// Saves multiple files and returns their URLs
        /// </summary>
        /// <param name="files">Collection of files to upload</param>
        /// <param name="folder">Subfolder within uploads directory</param>
        /// <returns>List of relative URLs to saved files</returns>
        Task<List<string>> SaveFilesAsync(IEnumerable<IFormFile> files, string folder);

        /// <summary>
        /// Deletes a file from storage
        /// </summary>
        /// <param name="relativePath">Relative path to the file (e.g., "/uploads/posts/abc123.jpg")</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        Task<bool> DeleteFileAsync(string relativePath);

        /// <summary>
        /// Checks if a file exists in storage
        /// </summary>
        /// <param name="relativePath">Relative path to the file</param>
        /// <returns>True if file exists, false otherwise</returns>
        bool FileExists(string relativePath);

        /// <summary>
        /// Gets the physical file path from relative URL
        /// </summary>
        /// <param name="relativePath">Relative path (e.g., "/uploads/posts/abc123.jpg")</param>
        /// <returns>Full physical path on disk</returns>
        string GetPhysicalPath(string relativePath);

        /// <summary>
        /// Gets file information without reading the file
        /// </summary>
        /// <param name="relativePath">Relative path to the file</param>
        /// <returns>File info (size, created date, etc.)</returns>
        FileInfo? GetFileInfo(string relativePath);

        /// <summary>
        /// Validates file before upload
        /// </summary>
        /// <param name="file">File to validate</param>
        /// <param name="allowedExtensions">Allowed file extensions</param>
        /// <param name="maxSizeMB">Maximum file size in MB</param>
        /// <returns>Validation result with error message if invalid</returns>
        (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file, string[]? allowedExtensions = null, int maxSizeMB = 10);
    }
}

