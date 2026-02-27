using Microsoft.AspNetCore.Identity;
using Pdfpeaks.Models;

namespace Pdfpeaks.Services;

/// <summary>
/// Service for handling file processing operations with user tier support
/// </summary>
public class FileProcessingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FileProcessingService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _tempFilePath;

    // Maximum file sizes (in bytes) per tier
    private const long FREE_TIER_MAX_SIZE = 10 * 1024 * 1024; // 10MB
    private const long PRO_TIER_MAX_SIZE = 100 * 1024 * 1024; // 100MB
    private const long ENTERPRISE_TIER_MAX_SIZE = 500 * 1024 * 1024; // 500MB

    // Download limits per day
    private const int FREE_TIER_DAILY_DOWNLOADS = 1;
    private const int PAID_TIER_DAILY_DOWNLOADS = -1; // Unlimited

    public FileProcessingService(
        IServiceProvider serviceProvider,
        ILogger<FileProcessingService> logger,
        IWebHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _environment = environment;
        _tempFilePath = Path.Combine(environment.ContentRootPath, "temp_files");
        
        // Ensure temp directory exists
        EnsureTempDirectory();
    }

    private void EnsureTempDirectory()
    {
        try
        {
            if (!Directory.Exists(_tempFilePath))
            {
                Directory.CreateDirectory(_tempFilePath);
                _logger.LogInformation("Created temp directory: {Path}", _tempFilePath);
            }
            else
            {
                _logger.LogInformation("Temp directory already exists: {Path}", _tempFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating temp directory: {Path}", _tempFilePath);
        }
    }

    private UserManager<ApplicationUser>? GetUserManager()
    {
        try
        {
            return _serviceProvider.GetService<UserManager<ApplicationUser>>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Check if user can process files based on their tier
    /// </summary>
    public async Task<(bool canProcess, string message)> CanProcessFileAsync(ApplicationUser? user, long fileSize)
    {
        if (user == null)
        {
            // Anonymous users - use free tier limits
            if (fileSize > FREE_TIER_MAX_SIZE)
            {
                return (false, $"File size exceeds the free tier limit of {FormatSize(FREE_TIER_MAX_SIZE)}. Please sign in or upgrade to process larger files.");
            }
            return (true, "Free tier: 10MB limit per file");
        }

        switch (user.SubscriptionTier)
        {
            case SubscriptionTier.Free:
                if (fileSize > FREE_TIER_MAX_SIZE)
                {
                    return (false, $"File size exceeds your limit of {FormatSize(FREE_TIER_MAX_SIZE)}. Upgrade to Pro for 100MB limit.");
                }
                if (user.DownloadsRemaining <= 0)
                {
                    return (false, "You have reached your daily download limit. Upgrade to Pro for unlimited downloads.");
                }
                return (true, $"Free tier: {FormatSize(FREE_TIER_MAX_SIZE)} limit, {user.DownloadsRemaining} download(s) remaining today");

            case SubscriptionTier.Pro:
                if (fileSize > PRO_TIER_MAX_SIZE)
                {
                    return (false, $"File size exceeds Pro tier limit of {FormatSize(PRO_TIER_MAX_SIZE)}. Contact support for Enterprise.");
                }
                return (true, "Pro tier: Unlimited downloads, 100MB limit");

            case SubscriptionTier.Enterprise:
                if (fileSize > ENTERPRISE_TIER_MAX_SIZE)
                {
                    return (false, $"File size exceeds Enterprise limit of {FormatSize(ENTERPRISE_TIER_MAX_SIZE)}.");
                }
                return (true, "Enterprise tier: Unlimited everything");

            default:
                return (false, "Unknown subscription tier");
        }
    }

    /// <summary>
    /// Check if user can download file
    /// </summary>
    public async Task<(bool canDownload, string message)> CanDownloadAsync(ApplicationUser? user)
    {
        if (user == null)
        {
            // Anonymous users - only 1 download
            return (true, "Anonymous download");
        }

        if (user.SubscriptionTier != SubscriptionTier.Free)
        {
            return (true, "Premium user: Unlimited downloads");
        }

        if (user.DownloadsRemaining <= 0)
        {
            return (false, "You have reached your daily download limit. Please upgrade to Pro for unlimited downloads.");
        }

        return (true, $"{user.DownloadsRemaining} download(s) remaining today");
    }

    /// <summary>
    /// Record a download and decrement user's remaining downloads if applicable
    /// </summary>
    public async Task RecordDownloadAsync(ApplicationUser? user, string fileName, long fileSize, string fileType)
    {
        try
        {
            if (user != null && user.SubscriptionTier == SubscriptionTier.Free)
            {
                var userManager = GetUserManager();
                if (userManager != null)
                {
                    user.DownloadsRemaining = Math.Max(0, user.DownloadsRemaining - 1);
                    await userManager.UpdateAsync(user);
                }
            }

            _logger.LogInformation("Download recorded: {FileName} ({Size}) by User: {UserId}", 
                fileName, FormatSize(fileSize), user?.Id ?? "Anonymous");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording download for user {UserId}", user?.Id);
        }
    }

    /// <summary>
    /// Generate a unique filename for processing with sanitized input
    /// </summary>
    public string GenerateFileName(string originalFileName, string operationType)
    {
        // SECURITY: Sanitize the filename to prevent path traversal attacks
        // Get only the filename without any directory path
        var sanitizedName = Path.GetFileName(originalFileName);
        
        // Get the extension and validate it
        var extension = Path.GetExtension(sanitizedName).ToLowerInvariant();
        
        // Validate extension against allowed list
        if (!IsAllowedExtension(extension))
        {
            _logger.LogWarning("Invalid file extension attempted: {Extension}", extension);
            extension = ".bin"; // Default to generic binary if invalid
        }
        
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        return $"{operationType}_{timestamp}_{uniqueId}{extension}";
    }
    
    /// <summary>
    /// SECURITY: Check if file extension is allowed
    /// </summary>
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp",
        ".txt", ".csv", ".json", ".xml", ".html", ".htm"
    };
    
    /// <summary>
    /// SECURITY: Validate file extension against allowlist
    /// </summary>
    public bool IsAllowedExtension(string extension)
    {
        return AllowedExtensions.Contains(extension);
    }
    
    /// <summary>
    /// SECURITY: Validate file by checking both extension AND magic bytes
    /// </summary>
    public async Task<(bool isValid, string message)> ValidateFileAsync(IFormFile file)
    {
        // Check 1: File size
        if (file.Length == 0)
        {
            return (false, "File is empty");
        }
        
        // Max 500MB
        if (file.Length > 500 * 1024 * 1024)
        {
            return (false, "File exceeds maximum size of 500MB");
        }
        
        // Check 2: Validate extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!IsAllowedExtension(extension))
        {
            _logger.LogWarning("Blocked upload with disallowed extension: {Extension}", extension);
            return (false, $"File type {extension} is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
        }
        
        // Check 3: Validate magic bytes (content-type validation)
        var buffer = new byte[8];
        using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 8));
        
        if (bytesRead < 4)
        {
            return (false, "File is too small to validate");
        }
        
        // Check magic bytes for common file types
        if (!IsValidMagicBytes(buffer, extension))
        {
            _logger.LogWarning("Magic bytes mismatch for extension {Extension}. Possible malicious file.", extension);
            return (false, "File content does not match its extension. Upload blocked for security.");
        }
        
        return (true, "File validated successfully");
    }
    
    /// <summary>
    /// SECURITY: Validate magic bytes match expected file type
    /// </summary>
    private bool IsValidMagicBytes(byte[] buffer, string extension)
    {
        // PDF: %PDF-
        if (extension == ".pdf")
        {
            return buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46; // %PDF
        }
        
        // PNG: \x89PNG
        if (extension == ".png")
        {
            return buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
        }
        
        // JPEG: \xFF\xD8\xFF
        if (extension == ".jpg" || extension == ".jpeg")
        {
            return buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
        }
        
        // GIF: GIF87a or GIF89a
        if (extension == ".gif")
        {
            return buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46; // GIF
        }
        
        // BMP: BM
        if (extension == ".bmp")
        {
            return buffer[0] == 0x42 && buffer[1] == 0x4D; // BM
        }
        
        // TIFF: II* or MM*
        if (extension == ".tiff" || extension == ".tif")
        {
            return (buffer[0] == 0x49 && buffer[1] == 0x49 && buffer[2] == 0x2A) ||
                   (buffer[0] == 0x4D && buffer[1] == 0x4D && buffer[2] == 0x2A);
        }
        
        // WebP: RIFF....WEBP
        if (extension == ".webp")
        {
            return buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46; // RIFF
        }
        
        // Office files (ZIP-based): PK (.docx, .xlsx, .pptx)
        if (extension == ".docx" || extension == ".xlsx" || extension == ".pptx" ||
            extension == ".odt" || extension == ".ods" || extension == ".odp")
        {
            return buffer[0] == 0x50 && buffer[1] == 0x4B; // PK (ZIP signature)
        }
        
        // For other types, just check they're not empty
        return buffer[0] != 0;
    }

    /// <summary>
    /// Check if a file is a valid PDF by checking its magic bytes
    /// </summary>
    public bool IsValidPdfFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File does not exist: {Path}", filePath);
                return false;
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                _logger.LogWarning("File is empty: {Path}", filePath);
                return false;
            }

            // Check PDF magic bytes (%PDF-)
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var header = new byte[5];
            stream.ReadExactly(header, 0, 5);
            var headerStr = System.Text.Encoding.ASCII.GetString(header);
            
            if (headerStr.StartsWith("%PDF-"))
            {
                _logger.LogInformation("Valid PDF header detected for: {Path}", filePath);
                return true;
            }
            else
            {
                _logger.LogWarning("Invalid PDF header for: {Path}. Header: {Header}", filePath, headerStr);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating PDF file: {Path}", filePath);
            return false;
        }
    }

    /// <summary>
    /// Save uploaded file to temp directory with validation
    /// </summary>
    public async Task<string> SaveUploadedFileAsync(IFormFile file, string operationType)
    {
        // SECURITY: Validate file before saving
        var (isValid, message) = await ValidateFileAsync(file);
        if (!isValid)
        {
            throw new InvalidOperationException($"File validation failed: {message}");
        }
        
        var fileName = GenerateFileName(file.FileName, operationType);
        var filePath = Path.Combine(_tempFilePath, fileName);

        _logger.LogInformation("Saving file: {Name} to {Path}", file.FileName, filePath);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var fileInfo = new FileInfo(filePath);
        _logger.LogInformation("File saved successfully: {Path} ({Size} bytes)", filePath, fileInfo.Length);

        return fileName;
    }

    /// <summary>
    /// Get file path for processing
    /// </summary>
    public string GetFilePath(string fileName)
    {
        return Path.Combine(_tempFilePath, fileName);
    }

    /// <summary>
    /// Clean up temporary files older than specified hours
    /// </summary>
    public void CleanupTempFiles(int olderThanHours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-olderThanHours);
        
        foreach (var file in Directory.GetFiles(_tempFilePath))
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.CreationTimeUtc < cutoffTime)
            {
                try
                {
                    File.Delete(file);
                    _logger.LogInformation("Cleaned up temp file: {FileName}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file: {FileName}", file);
                }
            }
        }
    }

    /// <summary>
    /// Get processing statistics for user
    /// </summary>
    public async Task<Dictionary<string, object>> GetUserStatsAsync(ApplicationUser user)
    {
        return new Dictionary<string, object>
        {
            ["TotalFilesProcessed"] = user.TotalFilesProcessed,
            ["StorageUsed"] = FormatSize(user.TotalStorageUsed),
            ["StorageLimit"] = FormatSize(user.MaxStorageAllowed),
            ["DownloadsRemaining"] = user.DownloadsRemaining,
            ["SubscriptionTier"] = user.SubscriptionTier.ToString(),
            ["SubscriptionEndDate"] = user.SubscriptionEndDate?.ToString("MMM dd, yyyy") ?? "N/A"
        };
    }

    /// <summary>
    /// Format bytes to human readable size
    /// </summary>
    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
