namespace Pdfpeaks.Services.Infrastructure;

/// <summary>
/// Background service that automatically cleans up temp files older than 10 minutes.
/// Runs every 5 minutes to prevent disk space exhaustion.
/// </summary>
public class TempFileCleanupService : BackgroundService
{
    private readonly ILogger<TempFileCleanupService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _fileMaxAge = TimeSpan.FromMinutes(10);

    public TempFileCleanupService(
        ILogger<TempFileCleanupService> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TempFileCleanupService started. Cleanup interval: {Interval}, Max file age: {MaxAge}",
            _cleanupInterval, _fileMaxAge);

        // Run initial cleanup on startup
        await CleanupOldFilesAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);
                await CleanupOldFilesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TempFileCleanupService loop");
                // Continue running even if one cleanup cycle fails
            }
        }

        _logger.LogInformation("TempFileCleanupService stopped.");
    }

    /// <summary>
    /// Delete all files in temp_files/ that are older than the configured max age.
    /// </summary>
    public async Task CleanupOldFilesAsync(CancellationToken cancellationToken = default)
    {
        var tempFilesPath = Path.Combine(_environment.ContentRootPath, "temp_files");

        if (!Directory.Exists(tempFilesPath))
        {
            _logger.LogDebug("Temp files directory does not exist: {Path}", tempFilesPath);
            return;
        }

        var cutoffTime = DateTime.UtcNow - _fileMaxAge;
        var deletedCount = 0;
        var failedCount = 0;
        long freedBytes = 0;

        try
        {
            var files = Directory.GetFiles(tempFilesPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var filePath in files)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var fileInfo = new FileInfo(filePath);

                    // Check if file is older than max age
                    if (fileInfo.LastWriteTimeUtc < cutoffTime)
                    {
                        var fileSize = fileInfo.Length;
                        fileInfo.Delete();
                        deletedCount++;
                        freedBytes += fileSize;
                        _logger.LogDebug("Deleted temp file: {File} (age: {Age:F1} min)",
                            fileInfo.Name,
                            (DateTime.UtcNow - fileInfo.LastWriteTimeUtc).TotalMinutes);
                    }
                }
                catch (IOException ex)
                {
                    // File may be in use - skip it
                    failedCount++;
                    _logger.LogDebug(ex, "Could not delete temp file (in use): {File}", filePath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    failedCount++;
                    _logger.LogWarning(ex, "Access denied deleting temp file: {File}", filePath);
                }
            }

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Temp file cleanup: deleted {Count} files, freed {Freed:F2} MB. Failed: {Failed}",
                    deletedCount,
                    freedBytes / (1024.0 * 1024.0),
                    failedCount);
            }
            else
            {
                _logger.LogDebug("Temp file cleanup: no files to delete (checked {Total} files)", files.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during temp file cleanup in {Path}", tempFilesPath);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Get statistics about the temp files directory.
    /// </summary>
    public TempFileStats GetStats()
    {
        var tempFilesPath = Path.Combine(_environment.ContentRootPath, "temp_files");

        if (!Directory.Exists(tempFilesPath))
        {
            return new TempFileStats();
        }

        try
        {
            var files = Directory.GetFiles(tempFilesPath, "*", SearchOption.TopDirectoryOnly);
            var cutoffTime = DateTime.UtcNow - _fileMaxAge;

            long totalSize = 0;
            int expiredCount = 0;

            foreach (var filePath in files)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    totalSize += fileInfo.Length;
                    if (fileInfo.LastWriteTimeUtc < cutoffTime)
                        expiredCount++;
                }
                catch { /* ignore */ }
            }

            return new TempFileStats
            {
                TotalFiles = files.Length,
                ExpiredFiles = expiredCount,
                TotalSizeBytes = totalSize,
                TotalSizeMB = totalSize / (1024.0 * 1024.0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting temp file stats");
            return new TempFileStats();
        }
    }
}

/// <summary>
/// Statistics about the temp files directory
/// </summary>
public class TempFileStats
{
    public int TotalFiles { get; set; }
    public int ExpiredFiles { get; set; }
    public long TotalSizeBytes { get; set; }
    public double TotalSizeMB { get; set; }
}
