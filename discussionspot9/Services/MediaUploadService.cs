using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace discussionspot9.Services
{
    public interface IMediaUploadService
    {
        Task<MediaUploadResult> UploadFileAsync(IFormFile file, string category = "stories");
        Task<bool> DeleteFileAsync(string filePath);
        Task<string> GetFileUrlAsync(string filePath);
        Task<MediaInfo> GetMediaInfoAsync(string filePath);
    }

    public class MediaUploadService : IMediaUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MediaUploadService> _logger;
        private readonly string _uploadPath;
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".webm", ".ogg", ".avi", ".mov", ".wmv" };
        private readonly long _maxFileSize = 50 * 1024 * 1024; // 50MB

        public MediaUploadService(IWebHostEnvironment environment, ILogger<MediaUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
            _uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "media");
            
            // Ensure upload directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<MediaUploadResult> UploadFileAsync(IFormFile file, string category = "stories")
        {
            try
            {
                // Validate file
                var validationResult = ValidateFile(file);
                if (!validationResult.IsValid)
                {
                    return new MediaUploadResult
                    {
                        Success = false,
                        ErrorMessage = validationResult.ErrorMessage
                    };
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = GenerateUniqueFileName(file.FileName);
                var categoryPath = Path.Combine(_uploadPath, category);
                
                // Ensure category directory exists
                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                }

                var filePath = Path.Combine(categoryPath, fileName);
                var relativePath = Path.Combine("uploads", "media", category, fileName).Replace("\\", "/");

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Get media info
                var mediaInfo = await GetMediaInfoAsync(filePath);

                _logger.LogInformation($"File uploaded successfully: {relativePath}");

                return new MediaUploadResult
                {
                    Success = true,
                    FilePath = relativePath,
                    FileName = fileName,
                    FileSize = file.Length,
                    MediaType = GetMediaType(fileExtension),
                    MediaInfo = mediaInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file: {file.FileName}");
                return new MediaUploadResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while uploading the file."
                };
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"File deleted: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string filePath)
        {
            return $"/{filePath.Replace("\\", "/")}";
        }

        public async Task<MediaInfo> GetMediaInfoAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (!File.Exists(fullPath))
                {
                    return null;
                }

                var fileInfo = new FileInfo(fullPath);
                var extension = Path.GetExtension(fullPath).ToLowerInvariant();

                var mediaInfo = new MediaInfo
                {
                    FileName = Path.GetFileName(fullPath),
                    FileSize = fileInfo.Length,
                    MediaType = GetMediaType(extension),
                    Extension = extension,
                    CreatedAt = fileInfo.CreationTime,
                    ModifiedAt = fileInfo.LastWriteTime
                };

                // Get dimensions for images
                if (_allowedImageExtensions.Contains(extension))
                {
                    try
                    {
                        using var image = SixLabors.ImageSharp.Image.Load(fullPath);
                        mediaInfo.Width = image.Width;
                        mediaInfo.Height = image.Height;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Could not get image dimensions for: {fullPath}");
                    }
                }

                return mediaInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting media info for: {filePath}");
                return null;
            }
        }

        private FileValidationResult ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "No file provided."
                };
            }

            if (file.Length > _maxFileSize)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB."
                };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = _allowedImageExtensions.Concat(_allowedVideoExtensions).ToArray();

            if (!allowedExtensions.Contains(extension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}"
                };
            }

            return new FileValidationResult { IsValid = true };
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            
            // Create a hash of the original filename + timestamp
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var hash = ComputeHash($"{nameWithoutExtension}{timestamp}");
            
            return $"{nameWithoutExtension}_{hash}{extension}";
        }

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashBytes)[..8]; // Take first 8 characters
        }

        private string GetMediaType(string extension)
        {
            if (_allowedImageExtensions.Contains(extension))
                return "image";
            if (_allowedVideoExtensions.Contains(extension))
                return "video";
            return "unknown";
        }
    }

    public class MediaUploadResult
    {
        public bool Success { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public MediaInfo MediaInfo { get; set; }
    }

    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class MediaInfo
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
