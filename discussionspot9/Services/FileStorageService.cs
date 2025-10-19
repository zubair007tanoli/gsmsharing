using discussionspot9.Interfaces;
using System.Text.RegularExpressions;

namespace discussionspot9.Services
{
    /// <summary>
    /// Handles all file upload, storage, and management operations
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;
        
        // Configuration constants
        private const long DefaultMaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
        private static readonly string[] DefaultAllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
        private static readonly string[] DefaultAllowedVideoExtensions = { ".mp4", ".webm", ".mov", ".avi" };
        private static readonly string[] DefaultAllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt" };

        public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder, string? customFileName = null)
        {
            // Validate input
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            // Validate file
            var validation = ValidateFile(file);
            if (!validation.IsValid)
            {
                throw new ArgumentException(validation.ErrorMessage ?? "File validation failed");
            }

            try
            {
                // Generate filename
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = string.IsNullOrEmpty(customFileName) 
                    ? $"{Guid.NewGuid()}{extension}" 
                    : $"{SanitizeFileName(customFileName)}{extension}";

                // Build paths
                var relativePath = $"/uploads/{folder.TrimStart('/')}/{fileName}";
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));

                // Ensure directory exists
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("Created directory: {Directory}", directory);
                }

                // Save file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File saved successfully: {Path} ({Size} bytes)", relativePath, file.Length);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FileName}", file.FileName);
                throw new InvalidOperationException($"Failed to save file: {ex.Message}", ex);
            }
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folder, int? maxWidth = null, int? maxHeight = null)
        {
            // Validate it's an image
            if (!file.ContentType.StartsWith("image/"))
            {
                throw new ArgumentException("File must be an image", nameof(file));
            }

            // For now, just save the file without resizing
            // TODO: Add image resizing with SixLabors.ImageSharp if maxWidth/maxHeight specified
            var savedPath = await SaveFileAsync(file, folder);

            // If resizing is needed, implement with ImageSharp:
            // 1. Install SixLabors.ImageSharp NuGet package
            // 2. Load image, resize maintaining aspect ratio
            // 3. Save optimized version
            // 4. Return path

            if (maxWidth.HasValue || maxHeight.HasValue)
            {
                _logger.LogInformation("Image resize requested but not yet implemented. Saved original size.");
                // TODO: Implement resizing
            }

            return savedPath;
        }

        public async Task<List<string>> SaveFilesAsync(IEnumerable<IFormFile> files, string folder)
        {
            var savedPaths = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var path = await SaveFileAsync(file, folder);
                    savedPaths.Add(path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving file in batch: {FileName}", file.FileName);
                    // Continue with other files
                }
            }

            return savedPaths;
        }

        public async Task<bool> DeleteFileAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("File deleted successfully: {Path}", relativePath);
                    return true;
                }
                else
                {
                    _logger.LogWarning("File not found for deletion: {Path}", relativePath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {Path}", relativePath);
                return false;
            }
        }

        public bool FileExists(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
                return File.Exists(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence: {Path}", relativePath);
                return false;
            }
        }

        public string GetPhysicalPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("Relative path cannot be null or empty", nameof(relativePath));
            }

            return Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
        }

        public FileInfo? GetFileInfo(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            try
            {
                var fullPath = GetPhysicalPath(relativePath);
                return File.Exists(fullPath) ? new FileInfo(fullPath) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info: {Path}", relativePath);
                return null;
            }
        }

        public (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file, string[]? allowedExtensions = null, int maxSizeMB = 10)
        {
            // Check if file is null or empty
            if (file == null || file.Length == 0)
            {
                return (false, "File is empty or not provided");
            }

            // Check file size
            var maxSizeBytes = maxSizeMB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
            {
                return (false, $"File size exceeds maximum limit of {maxSizeMB}MB");
            }

            // Get and validate extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                return (false, "File must have a valid extension");
            }

            // Check allowed extensions
            var allowedExts = allowedExtensions ?? GetAllAllowedExtensions();
            if (!allowedExts.Contains(extension))
            {
                return (false, $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", allowedExts)}");
            }

            // Validate content type matches extension
            var expectedContentTypes = GetExpectedContentTypes(extension);
            if (!expectedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("Content type mismatch: File extension is {Extension} but content type is {ContentType}", 
                    extension, file.ContentType);
                // Allow but log warning
            }

            return (true, null);
        }

        #region Private Helper Methods

        /// <summary>
        /// Sanitizes a filename to remove invalid characters
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            // Remove invalid filename characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            
            // Replace spaces and special characters with underscores
            sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9_-]", "_");
            
            // Limit length
            return sanitized.Length > 50 ? sanitized.Substring(0, 50) : sanitized;
        }

        /// <summary>
        /// Gets all allowed file extensions
        /// </summary>
        private string[] GetAllAllowedExtensions()
        {
            return DefaultAllowedImageExtensions
                .Concat(DefaultAllowedVideoExtensions)
                .Concat(DefaultAllowedDocumentExtensions)
                .ToArray();
        }

        /// <summary>
        /// Gets expected content types for a file extension
        /// </summary>
        private string[] GetExpectedContentTypes(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => new[] { "image/jpeg", "image/jpg" },
                ".png" => new[] { "image/png" },
                ".gif" => new[] { "image/gif" },
                ".webp" => new[] { "image/webp" },
                ".svg" => new[] { "image/svg+xml" },
                ".mp4" => new[] { "video/mp4" },
                ".webm" => new[] { "video/webm" },
                ".mov" => new[] { "video/quicktime" },
                ".avi" => new[] { "video/x-msvideo" },
                ".pdf" => new[] { "application/pdf" },
                ".doc" => new[] { "application/msword" },
                ".docx" => new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                _ => Array.Empty<string>()
            };
        }

        #endregion
    }
}

