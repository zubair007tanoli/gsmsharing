using Microsoft.AspNetCore.Mvc;
using Pdfpeaks.Services;
using System.Collections.Concurrent;

namespace Pdfpeaks.Controllers.Api;

/// <summary>
/// API endpoints for conversion jobs (PDF↔Word, etc.).
/// Performs conversions directly (no Hangfire/SQL Server required).
/// Returns a jobId and status so the frontend polling contract is preserved.
/// </summary>
[Route("api/jobs")]
[ApiController]
[IgnoreAntiforgeryToken] // API uses JWT authentication, not CSRF
public class ConversionJobsController : ControllerBase
{
    private readonly PdfProcessingService _pdfProcessingService;
    private readonly FileProcessingService _fileProcessingService;
    private readonly ILogger<ConversionJobsController> _logger;

    // In-memory job status store (survives for the lifetime of the app process)
    private static readonly ConcurrentDictionary<string, ConversionJobStatusDto> _jobs = new();

    public ConversionJobsController(
        PdfProcessingService pdfProcessingService,
        FileProcessingService fileProcessingService,
        ILogger<ConversionJobsController> logger)
    {
        _pdfProcessingService = pdfProcessingService;
        _fileProcessingService = fileProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Convert a PDF to Word directly (no background queue required).
    /// Accepts a previously uploaded temp file name (see /api/files/upload).
    /// </summary>
    [HttpPost("pdf/to-word")]
    public async Task<IActionResult> EnqueuePdfToWord(
        [FromForm] string fileName,
        [FromForm] string? outputFileName)
    {
        try
        {
            var inputPath = _fileProcessingService.GetFilePath(fileName);
            if (!System.IO.File.Exists(inputPath))
            {
                return NotFound(new { success = false, message = "Source file not found. Please upload the file first using /api/files/upload" });
            }

            var finalOutputName = string.IsNullOrWhiteSpace(outputFileName)
                ? System.IO.Path.ChangeExtension(fileName, ".docx")
                : outputFileName;

            var jobId = Guid.NewGuid().ToString("N");

            // Record job as running
            var job = new ConversionJobStatusDto
            {
                JobId = jobId,
                Operation = "pdf-to-word",
                InputTempFileName = fileName,
                OutputFileName = finalOutputName,
                Status = "running",
                Message = "Conversion started.",
                CreatedAtUtc = DateTime.UtcNow
            };
            _jobs[jobId] = job;

            // Perform conversion directly
            var (success, outputPath, message) = await _pdfProcessingService.ConvertToWordAsync(inputPath, finalOutputName);

            // Clean up input temp file
            try { if (System.IO.File.Exists(inputPath)) System.IO.File.Delete(inputPath); } catch { }

            // Update job status
            _jobs[jobId] = job with
            {
                Status = success ? "succeeded" : "failed",
                Message = message ?? (success ? "Conversion completed." : "Conversion failed."),
                OutputPath = success && System.IO.File.Exists(outputPath) ? outputPath : null,
                CompletedAtUtc = DateTime.UtcNow
            };

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = System.IO.Path.GetFileName(outputPath);
                return Ok(new
                {
                    success = true,
                    jobId,
                    status = "succeeded",
                    message = message ?? "PDF→Word conversion completed.",
                    outputFileName = resultFileName,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}"
                });
            }

            return StatusCode(500, new { success = false, jobId, message = message ?? "Conversion failed." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF→Word for file {FileName}", fileName);
            return StatusCode(500, new { success = false, message = $"Conversion error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Convert a Word document to PDF directly (no background queue required).
    /// Accepts a previously uploaded temp file name (see /api/files/upload).
    /// </summary>
    [HttpPost("pdf/from-word")]
    public async Task<IActionResult> EnqueueWordToPdf(
        [FromForm] string fileName,
        [FromForm] string? outputFileName)
    {
        try
        {
            var inputPath = _fileProcessingService.GetFilePath(fileName);
            if (!System.IO.File.Exists(inputPath))
            {
                return NotFound(new { success = false, message = "Source file not found. Please upload the file first using /api/files/upload" });
            }

            var finalOutputName = string.IsNullOrWhiteSpace(outputFileName)
                ? System.IO.Path.ChangeExtension(fileName, ".pdf")
                : outputFileName;

            var jobId = Guid.NewGuid().ToString("N");

            // Record job as running
            var job = new ConversionJobStatusDto
            {
                JobId = jobId,
                Operation = "word-to-pdf",
                InputTempFileName = fileName,
                OutputFileName = finalOutputName,
                Status = "running",
                Message = "Conversion started.",
                CreatedAtUtc = DateTime.UtcNow
            };
            _jobs[jobId] = job;

            // Perform conversion directly
            var (success, outputPath, message) = await _pdfProcessingService.ConvertWordToPdfAsync(inputPath, finalOutputName);

            // Clean up input temp file
            try { if (System.IO.File.Exists(inputPath)) System.IO.File.Delete(inputPath); } catch { }

            // Update job status
            _jobs[jobId] = job with
            {
                Status = success ? "succeeded" : "failed",
                Message = message ?? (success ? "Conversion completed." : "Conversion failed."),
                OutputPath = success && System.IO.File.Exists(outputPath) ? outputPath : null,
                CompletedAtUtc = DateTime.UtcNow
            };

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = System.IO.Path.GetFileName(outputPath);
                return Ok(new
                {
                    success = true,
                    jobId,
                    status = "succeeded",
                    message = message ?? "Word→PDF conversion completed.",
                    outputFileName = resultFileName,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}"
                });
            }

            return StatusCode(500, new { success = false, jobId, message = message ?? "Conversion failed." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting Word→PDF for file {FileName}", fileName);
            return StatusCode(500, new { success = false, message = $"Conversion error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get the current status of a conversion job.
    /// </summary>
    [HttpGet("{jobId}")]
    public IActionResult GetStatus(string jobId)
    {
        if (!_jobs.TryGetValue(jobId, out var status))
        {
            return NotFound(new { success = false, message = "Job not found." });
        }

        return Ok(new
        {
            success = true,
            jobId = status.JobId,
            operation = status.Operation,
            status = status.Status,
            message = status.Message,
            outputFileName = status.OutputFileName,
            outputPath = status.OutputPath,
            downloadUrl = status.OutputFileName != null
                ? $"/Pdf/Download?fileName={Uri.EscapeDataString(status.OutputFileName)}"
                : null,
            createdAtUtc = status.CreatedAtUtc,
            completedAtUtc = status.CompletedAtUtc
        });
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
