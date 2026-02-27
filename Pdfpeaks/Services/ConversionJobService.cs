using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;

namespace Pdfpeaks.Services;

/// <summary>
/// Background conversion jobs for heavy operations (PDF↔Word, etc.).
/// Uses Hangfire for execution and persistent status via Hangfire storage (SQL).
/// </summary>
public class ConversionJobService
{
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly JobStorage _jobStorage;
    private readonly PdfProcessingService _pdfProcessingService;
    private readonly FileProcessingService _fileProcessingService;
    private readonly ILogger<ConversionJobService> _logger;

    public ConversionJobService(
        IBackgroundJobClient backgroundJobs,
        JobStorage jobStorage,
        PdfProcessingService pdfProcessingService,
        FileProcessingService fileProcessingService,
        ILogger<ConversionJobService> logger)
    {
        _backgroundJobs = backgroundJobs;
        _jobStorage = jobStorage;
        _pdfProcessingService = pdfProcessingService;
        _fileProcessingService = fileProcessingService;
        _logger = logger;
    }

    public string EnqueuePdfToWord(string tempFileName, string outputFileName)
    {
        // Use Hangfire's job id as the public job id so status persists across restarts.
        var hangfireJobId = _backgroundJobs.Enqueue(() => RunPdfToWordJob(null!, tempFileName, outputFileName));
        SetJobParameterSafe(hangfireJobId, "Operation", "pdf-to-word");
        SetJobParameterSafe(hangfireJobId, "InputTempFileName", tempFileName);
        SetJobParameterSafe(hangfireJobId, "OutputFileName", outputFileName);
        SetJobParameterSafe(hangfireJobId, "CreatedAtUtc", DateTime.UtcNow.ToString("O"));
        return hangfireJobId;
    }

    public string EnqueueWordToPdf(string tempFileName, string outputFileName)
    {
        var hangfireJobId = _backgroundJobs.Enqueue(() => RunWordToPdfJob(null!, tempFileName, outputFileName));
        SetJobParameterSafe(hangfireJobId, "Operation", "word-to-pdf");
        SetJobParameterSafe(hangfireJobId, "InputTempFileName", tempFileName);
        SetJobParameterSafe(hangfireJobId, "OutputFileName", outputFileName);
        SetJobParameterSafe(hangfireJobId, "CreatedAtUtc", DateTime.UtcNow.ToString("O"));
        return hangfireJobId;
    }

    public ConversionJobStatusDto? GetStatus(string jobId)
    {
        try
        {
            var api = _jobStorage.GetMonitoringApi();
            var details = api.JobDetails(jobId);
            if (details == null)
                return null;

            var stateName = details.History
                .OrderByDescending(h => h.CreatedAt)
                .FirstOrDefault()
                ?.StateName ?? "Unknown";

            var status = stateName switch
            {
                "Processing" => "running",
                "Succeeded" => "succeeded",
                "Failed" => "failed",
                "Deleted" => "failed",
                _ => "queued"
            };

            var operation = GetProp(details, "Operation") ?? "unknown";
            var inputTemp = GetProp(details, "InputTempFileName") ?? string.Empty;
            var outputFile = GetProp(details, "OutputFileName") ?? string.Empty;
            var message = GetProp(details, "ResultMessage");

            DateTime? createdAtUtc = TryParseDate(GetProp(details, "CreatedAtUtc"));
            DateTime? completedAtUtc = TryParseDate(GetProp(details, "CompletedAtUtc"));

            // If we didn't capture completedAt explicitly, use last state change time for terminal states.
            if (completedAtUtc == null && (status == "succeeded" || status == "failed"))
            {
                completedAtUtc = details.History
                    .OrderByDescending(h => h.CreatedAt)
                    .FirstOrDefault(h => h.StateName is "Succeeded" or "Failed" or "Deleted")
                    ?.CreatedAt;
            }

            string? outputPath = null;
            if (!string.IsNullOrWhiteSpace(outputFile))
            {
                var candidate = _fileProcessingService.GetFilePath(outputFile);
                if (File.Exists(candidate))
                    outputPath = candidate;
            }

            return new ConversionJobStatusDto
            {
                JobId = jobId,
                Operation = operation,
                InputTempFileName = inputTemp,
                OutputFileName = outputFile,
                OutputPath = outputPath,
                Status = status,
                Message = message,
                CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow,
                CompletedAtUtc = completedAtUtc
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read Hangfire job status for {JobId}", jobId);
            return null;
        }
    }

    // Hangfire job methods must be public. PerformContext is injected at runtime.
    public async Task RunPdfToWordJob(PerformContext context, string tempFileName, string outputFileName)
    {
        var jobId = context.BackgroundJob.Id;
        SetJobParameterSafe(jobId, "ResultMessage", "Conversion started.");

        try
        {
            var inputPath = _fileProcessingService.GetFilePath(tempFileName);
            var (success, outputPath, message) = await _pdfProcessingService.ConvertToWordAsync(inputPath, outputFileName);

            if (success && File.Exists(outputPath))
            {
                SetJobParameterSafe(jobId, "ResultMessage", message ?? "Completed.");
                SetJobParameterSafe(jobId, "CompletedAtUtc", DateTime.UtcNow.ToString("O"));
            }
            else
            {
                SetJobParameterSafe(jobId, "ResultMessage", message ?? "Conversion failed.");
                SetJobParameterSafe(jobId, "CompletedAtUtc", DateTime.UtcNow.ToString("O"));
            }

            // Always attempt to remove the uploaded temp input (output is retained for download).
            try { if (File.Exists(inputPath)) File.Delete(inputPath); } catch { }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background PDF→Word job failed: {JobId}", jobId);
            SetJobParameterSafe(jobId, "ResultMessage", ex.Message);
            SetJobParameterSafe(jobId, "CompletedAtUtc", DateTime.UtcNow.ToString("O"));
        }
    }

    public async Task RunWordToPdfJob(PerformContext context, string tempFileName, string outputFileName)
    {
        var jobId = context.BackgroundJob.Id;
        SetJobParameterSafe(jobId, "ResultMessage", "Conversion started.");

        try
        {
            var inputPath = _fileProcessingService.GetFilePath(tempFileName);
            var (success, outputPath, message) = await _pdfProcessingService.ConvertWordToPdfAsync(inputPath, outputFileName);

            if (success && File.Exists(outputPath))
            {
                SetJobParameterSafe(jobId, "ResultMessage", message ?? "Completed.");
                SetJobParameterSafe(jobId, "CompletedAtUtc", DateTime.UtcNow.ToString("O"));
            }
            else
            {
                SetJobParameterSafe(jobId, "ResultMessage", message ?? "Conversion failed.");
                SetJobParameterSafe(jobId, "CompletedAtUtc", DateTime.UtcNow.ToString("O"));
            }

            try { if (File.Exists(inputPath)) File.Delete(inputPath); } catch { }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background Word→PDF job failed: {JobId}", jobId);
            SetJobParameterSafe(jobId, "ResultMessage", ex.Message);
            SetJobParameterSafe(jobId, "CompletedAtUtc", DateTime.UtcNow.ToString("O"));
        }
    }

    private void SetJobParameterSafe(string jobId, string key, string value)
    {
        try
        {
            using var connection = _jobStorage.GetConnection();
            connection.SetJobParameter(jobId, key, value);
        }
        catch
        {
            // ignore parameter write failures
        }
    }

    private static string? GetProp(Hangfire.Storage.Monitoring.JobDetailsDto details, string key)
    {
        if (details.Properties.TryGetValue(key, out var value))
            return value;

        // Case-insensitive fallback
        var kv = details.Properties.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrWhiteSpace(kv.Value) ? null : kv.Value;
    }

    private static DateTime? TryParseDate(string? value)
    {
        return DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
            ? dt
            : null;
    }
}

public sealed record ConversionJobStatusDto
{
    public string JobId { get; init; } = string.Empty;
    public string Operation { get; init; } = string.Empty;
    public string InputTempFileName { get; init; } = string.Empty;
    public string OutputFileName { get; init; } = string.Empty;
    public string? OutputPath { get; init; }
    public string Status { get; init; } = "queued"; // queued, running, succeeded, failed
    public string? Message { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}

