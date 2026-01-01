using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FluxPDF.Data;
using FluxPDF.Models;
using Microsoft.EntityFrameworkCore;

namespace FluxPDF.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadPath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;
        private readonly ILogger<FileService> _logger;
        private readonly ApplicationDbContext _context;

        public FileService(IConfiguration configuration, ILogger<FileService> logger, ApplicationDbContext context)
        {
            _logger = logger; _context = context;
            _uploadPath = configuration["FileStorage:Path"] ?? "wwwroot/uploads";
            _maxFileSize = long.Parse(configuration["FileStorage:MaxFileSize"] ?? "104857600");
            _allowedExtensions = configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>() ?? new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            if (!Directory.Exists(_uploadPath)) { Directory.CreateDirectory(_uploadPath); _logger.LogInformation("Created upload directory: {Path}", _uploadPath); }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string userId)
        {
            if (!ValidateFile(file)) throw new ArgumentException("Invalid file");
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var userDirectory = Path.Combine(_uploadPath, userId ?? "anonymous");
            if (!Directory.Exists(userDirectory)) Directory.CreateDirectory(userDirectory);
            var filePath = Path.Combine(userDirectory, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
            var fileMetadata = new FileMetadata { FileName = file.FileName, FilePath = filePath, FileSize = file.Length, FileType = GetFileExtension(file.FileName), MimeType = file.ContentType, UserId = userId, CreatedAt = DateTime.UtcNow, IsPermanent = false };
            if (string.IsNullOrEmpty(userId)) fileMetadata.ExpiresAt = DateTime.UtcNow.AddHours(1);
            _context.FileMetadata.Add(fileMetadata); await _context.SaveChangesAsync();
            _logger.LogInformation("File saved: {FilePath}, Metadata ID: {Id}", filePath, fileMetadata.Id);
            return filePath;
        }

        public Task<bool> DeleteFileAsync(string filePath) { try { if (File.Exists(filePath)) { File.Delete(filePath); _logger.LogInformation("File deleted: {FilePath}", filePath); return Task.FromResult(true); } return Task.FromResult(false); } catch (Exception ex) { _logger.LogError(ex, "Error deleting file: {FilePath}", filePath); return Task.FromResult(false); } }
        public Task<Stream> GetFileStreamAsync(string filePath) { if (!File.Exists(filePath)) throw new FileNotFoundException("File not found", filePath); return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read)); }
        public bool ValidateFile(IFormFile file) { if (file == null || file.Length == 0) return false; if (file.Length > _maxFileSize) { _logger.LogWarning("File size exceeds limit: {Size} bytes", file.Length); return false; } var extension = GetFileExtension(file.FileName); if (!_allowedExtensions.Contains(extension.ToLower())) { _logger.LogWarning("File extension not allowed: {Extension}", extension); return false; } return true; }
        public string GetFileExtension(string fileName) { return Path.GetExtension(fileName); }
    }
}
