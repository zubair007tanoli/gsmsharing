using gsmsharing.Interfaces;
using gsmsharing.Models.ImageModels;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;

namespace gsmsharing.Repositories
{
    public class ImageRepository : IFileStorage
    {
        private readonly FileStorageOptions _options;
        private readonly ILogger<ImageRepository> _logger;

        public ImageRepository(
            IOptions<FileStorageOptions> options,
            ILogger<ImageRepository> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<FileUploadResult> SaveImageAsync(IFormFile file, string directory = "uploads")
        {
            try
            {
                _logger.LogInformation($"SaveImageAsync called - FileName: {file?.FileName}, Directory: {directory}");
                
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("SaveImageAsync failed: No file was provided");
                    return FileUploadResult.Failure("No file was provided");
                }

                if (!ValidateImage(file))
                {
                    _logger.LogWarning($"SaveImageAsync failed: Invalid file format or size - FileName: {file.FileName}, Size: {file.Length}, ContentType: {file.ContentType}");
                    return FileUploadResult.Failure("Invalid file format or size");
                }

                string fileName = GetUniqueFileName(file.FileName);
                string relativePath = Path.Combine(directory, fileName);
                string absolutePath = Path.Combine(_options.RootDirectory, relativePath);

                _logger.LogInformation($"Image save paths - Relative: {relativePath}, Absolute: {absolutePath}");
                
                var directoryPath = Path.GetDirectoryName(absolutePath);
                if (!Directory.Exists(directoryPath))
                {
                    _logger.LogInformation($"Creating directory: {directoryPath}");
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(absolutePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"✅ Image saved successfully to: {absolutePath}");
                
                var fileUrl = GetFileUrl(relativePath);
                _logger.LogInformation($"Generated FileUrl: {fileUrl}");

                return FileUploadResult.Success(
                    relativePath,
                    fileUrl,
                    file.Length,
                    file.ContentType
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error saving image file: {FileName}", file?.FileName);
                return FileUploadResult.Failure($"Failed to save the image: {ex.Message}");
            }
        }

        public async Task<FileUploadResult> SaveImageAsync(byte[] fileBytes, string fileName, string directory = "images")
        {
            try
            {
                if (fileBytes == null || fileBytes.Length == 0)
                    return FileUploadResult.Failure("No file data was provided");
                
                // Validate file format using ImageSharp
                using (var image = Image.Load(fileBytes))
                {
                  
                    if (image == null)
                        return FileUploadResult.Failure("Invalid image format");
                }

                string uniqueFileName = GetUniqueFileName(fileName);
                string relativePath = Path.Combine(directory, uniqueFileName);
                string absolutePath = Path.Combine(_options.RootDirectory, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
                await File.WriteAllBytesAsync(absolutePath, fileBytes);

                return FileUploadResult.Success(
                    relativePath,
                    GetFileUrl(relativePath),
                    fileBytes.Length,
                    GetContentType(fileName)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image bytes: {FileName}", fileName);
                return FileUploadResult.Failure("Failed to save the image");
            }
        }

        public async Task<bool> DeleteImageAsync(string filePath)
        {
            try
            {
                string absolutePath = Path.Combine(_options.RootDirectory, filePath);
                if (File.Exists(absolutePath))
                {
                    await Task.Run(() => File.Delete(absolutePath));
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<byte[]> GetImageAsync(string filePath)
        {
            string absolutePath = Path.Combine(_options.RootDirectory, filePath);
            if (!File.Exists(absolutePath))
                return null;

            return await File.ReadAllBytesAsync(absolutePath);
        }

        public bool ValidateImage(IFormFile file)
        {
            if (file.Length > _options.MaxFileSizeInBytes)
                return false;

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_options.AllowedExtensions.Contains(extension))
                return false;

            if (!_options.AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        public string GetFileUrl(string filePath)
        {
            var fileUrl = $"{_options.BaseUrl.TrimEnd('/')}/{filePath.Replace("\\", "/")}";
            _logger.LogInformation($"Generated FileUrl: {fileUrl} from FilePath: {filePath}");
            return fileUrl;
        }

        private string GetUniqueFileName(string fileName)
        {
            if (_options.PreserveFileName)
                return fileName;

            string extension = Path.GetExtension(fileName);
            string uniqueName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{GetRandomString(6)}{extension}";
            return uniqueName;
        }

        private string GetRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
