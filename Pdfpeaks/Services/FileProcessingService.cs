using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        _tempFilePath = Path.Combine(environment.ContentRootPath, "wwwroot", "temp");
        
        // Ensure temp directory exists
        if (!Directory.Exists(_tempFilePath))
        {
            Directory.CreateDirectory(_tempFilePath);
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
    /// Generate a unique filename for processing
    /// </summary>
    public string GenerateFileName(string originalFileName, string operationType)
    {
        var extension = Path.GetExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        return $"{operationType}_{timestamp}_{uniqueId}{extension}";
    }

    /// <summary>
    /// Save uploaded file to temp directory
    /// </summary>
    public async Task<string> SaveUploadedFileAsync(IFormFile file, string operationType)
    {
        var fileName = GenerateFileName(file.FileName, operationType);
        var filePath = Path.Combine(_tempFilePath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

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
