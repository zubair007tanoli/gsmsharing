using Microsoft.AspNetCore.Mvc;
using Pdfpeaks.Services;

namespace Pdfpeaks.Controllers.Api;

/// <summary>
/// API controller for file processing operations
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly FileProcessingService _fileProcessingService;
    private readonly PdfProcessingService _pdfProcessingService;
    private readonly ImageProcessingService _imageProcessingService;
    private readonly ILogger<FilesController> _logger;
    private readonly IWebHostEnvironment _environment;

    public FilesController(
        FileProcessingService fileProcessingService,
        PdfProcessingService pdfProcessingService,
        ImageProcessingService imageProcessingService,
        ILogger<FilesController> logger,
        IWebHostEnvironment environment)
    {
        _fileProcessingService = fileProcessingService;
        _pdfProcessingService = pdfProcessingService;
        _imageProcessingService = imageProcessingService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Upload file for processing
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, string operation)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, operation);
            var filePath = _fileProcessingService.GetFilePath(fileName);

            return Ok(new
            {
                success = true,
                fileName,
                originalName = file.FileName,
                size = file.Length,
                tempPath = filePath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { success = false, message = "Error uploading file." });
        }
    }

    /// <summary>
    /// Get PDF information
    /// </summary>
    [HttpPost("pdf/info")]
    public async Task<IActionResult> GetPdfInfo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "info");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            var (pageCount, fileSize, pageSizes) = _pdfProcessingService.GetPdfInfo(filePath);

            return Ok(new
            {
                success = true,
                pageCount,
                fileSize,
                formattedSize = FormatSize((long)fileSize),
                pageSizes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PDF info");
            return StatusCode(500, new { success = false, message = "Error reading PDF." });
        }
    }

    /// <summary>
    /// Get image information
    /// </summary>
    [HttpPost("image/info")]
    public async Task<IActionResult> GetImageInfo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "info");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            var (width, height, format, fileSize) = _imageProcessingService.GetImageInfo(filePath);

            return Ok(new
            {
                success = true,
                width,
                height,
                format,
                fileSize,
                formattedSize = FormatSize(fileSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image info");
            return StatusCode(500, new { success = false, message = "Error reading image." });
        }
    }

    /// <summary>
    /// Delete temporary file
    /// </summary>
    [HttpDelete("delete")]
    public IActionResult DeleteFile(string fileName)
    {
        try
        {
            var filePath = _fileProcessingService.GetFilePath(fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return Ok(new { success = true, message = "File deleted." });
            }
            return NotFound(new { success = false, message = "File not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, new { success = false, message = "Error deleting file." });
        }
    }

    private static string FormatSize(long bytes)
    {
        if (bytes == 0) return "0 Bytes";
        var k = 1024;
        var sizes = new[] { "Bytes", "KB", "MB", "GB" };
        var i = (int)Math.Floor(Math.Log(bytes) / Math.Log(k));
        return $"{(long)((double)bytes / Math.Pow(k, i)):F2} {sizes[i]}";
    }
}
