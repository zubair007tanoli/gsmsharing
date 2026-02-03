using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Pdfpeaks.Models;
using Pdfpeaks.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Pdfpeaks.Controllers;

/// <summary>
/// Controller for Image processing operations
/// </summary>
public class ImageController : Controller
{
    private readonly ILogger<ImageController> _logger;
    private readonly UserManager<ApplicationUser>? _userManager;
    private readonly FileProcessingService _fileProcessingService;
    private readonly ImageProcessingService _imageProcessingService;
    private readonly IWebHostEnvironment _environment;

    public ImageController(
        ILogger<ImageController> logger,
        UserManager<ApplicationUser>? userManager,
        FileProcessingService fileProcessingService,
        ImageProcessingService imageProcessingService,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _userManager = userManager;
        _fileProcessingService = fileProcessingService;
        _imageProcessingService = imageProcessingService;
        _environment = environment;
    }

    public IActionResult Index()
    {
        return View();
    }

    #region Resize Operations

    [HttpGet]
    public IActionResult Resize()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Resize(IFormFile file, int width, int height, string mode = "stretch")
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please upload an image file.";
            return View();
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            TempData["Error"] = message;
            return View();
        }

        try
        {
            var resizeMode = mode switch
            {
                "crop" => ResizeMode.Crop,
                "pad" => ResizeMode.Pad,
                "max" => ResizeMode.Max,
                "min" => ResizeMode.Min,
                _ => ResizeMode.Stretch
            };

            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "resize");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "resize");

            var (success, outputPath, resultMessage) = await _imageProcessingService.ResizeImageAsync(
                filePath, outputFileName, width, height, resizeMode);

            if (success)
            {
                TempData["Success"] = resultMessage;
                TempData["Width"] = width;
                TempData["Height"] = height;
                TempData["DownloadUrl"] = $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}";
                TempData["FileName"] = outputFileName;
            }
            else
            {
                TempData["Error"] = resultMessage;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            TempData["Error"] = "An error occurred while resizing the image.";
        }

        return View();
    }

    #endregion

    #region Crop Operations

    [HttpGet]
    public IActionResult Crop()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Crop(IFormFile file, int x, int y, int width, int height)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please upload an image file.";
            return View();
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            TempData["Error"] = message;
            return View();
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "crop");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "crop");

            var (success, outputPath, resultMessage) = await _imageProcessingService.CropImageAsync(
                filePath, outputFileName, x, y, width, height);

            if (success)
            {
                TempData["Success"] = resultMessage;
                TempData["CropWidth"] = width;
                TempData["CropHeight"] = height;
                TempData["DownloadUrl"] = $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}";
                TempData["FileName"] = outputFileName;
            }
            else
            {
                TempData["Error"] = resultMessage;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cropping image");
            TempData["Error"] = "An error occurred while cropping the image.";
        }

        return View();
    }

    #endregion

    #region Convert Operations

    [HttpGet]
    public IActionResult Convert()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Convert(IFormFile file, string targetFormat, int quality = 85)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, $"convert_{targetFormat}");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = Path.ChangeExtension(fileName, $".{targetFormat}");

            var (success, outputPath, resultMessage) = await _imageProcessingService.ConvertImageFormatAsync(
                filePath, outputFileName, targetFormat);

            return Json(new { 
                success, 
                message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image");
            return Json(new { success = false, message = "An error occurred during conversion." });
        }
    }

    [HttpGet]
    public IActionResult ToJpg()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ToPng()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ToWebp()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ToGif()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ToBmp()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ToIco()
    {
        return View();
    }

    #endregion

    #region Transform Operations

    [HttpGet]
    public IActionResult Rotate()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Rotate(IFormFile file, float degrees)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "rotate");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "rotate");

            var (success, outputPath, resultMessage) = await _imageProcessingService.RotateImageAsync(
                filePath, outputFileName, degrees);

            return Json(new { 
                success, 
                message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating image");
            return Json(new { success = false, message = "An error occurred while rotating the image." });
        }
    }

    [HttpGet]
    public IActionResult Flip()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Flip(IFormFile file, bool horizontal)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "flip");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "flip");

            var (success, outputPath, resultMessage) = await _imageProcessingService.FlipImageAsync(
                filePath, outputFileName, horizontal);

            return Json(new { 
                success, 
                message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flipping image");
            return Json(new { success = false, message = "An error occurred while flipping the image." });
        }
    }

    #endregion

    #region Compress Operations

    [HttpGet]
    public IActionResult Compress()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Compress(IFormFile file, int quality = 75)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "compress");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "compress");

            var (success, outputPath, resultMessage, originalSize, compressedSize) = await _imageProcessingService.CompressImageAsync(
                filePath, outputFileName, quality);
            
            var compressionRatio = originalSize > 0 ? (double)compressedSize / originalSize : 0;

            return Json(new { 
                success, 
                message = resultMessage,
                compressionRatio,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing image");
            return Json(new { success = false, message = "An error occurred while compressing the image." });
        }
    }

    #endregion

    #region PDF Conversion

    [HttpGet]
    public IActionResult ToPdf()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ToPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "img2pdf");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("image.pdf", "img2pdf");

            var (success, outputPath, resultMessage) = await _imageProcessingService.ConvertToPdfAsync(
                filePath, outputFileName);

            return Json(new { 
                success, 
                message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image to PDF");
            return Json(new { success = false, message = "An error occurred during conversion." });
        }
    }

    [HttpGet]
    public IActionResult FromPdf()
    {
        return View();
    }

    #endregion

    #region Filter and Effects

    [HttpGet]
    public IActionResult Edit()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> AddWatermark(IFormFile file, string text, float opacity, int fontSize)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "watermark");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "watermark");

            var (success, outputPath, resultMessage) = await _imageProcessingService.AddWatermarkAsync(
                filePath, outputFileName, text, opacity, fontSize);

            return Json(new { 
                success, 
                message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding watermark");
            return Json(new { success = false, message = "An error occurred while adding watermark." });
        }
    }

    #endregion

    #region Download and Info

    [HttpGet]
    public async Task<IActionResult> Download(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return NotFound();
        }

        var user = await GetCurrentUserAsync();
        var (canDownload, message) = await _fileProcessingService.CanDownloadAsync(user);
        
        if (!canDownload)
        {
            TempData["Error"] = message;
            TempData["ShowUpgrade"] = true;
            return RedirectToAction("Index");
        }

        var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", "temp", fileName);
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found or has expired.");
        }

        var fileInfo = new FileInfo(filePath);
        var contentType = GetContentType(fileName);
        
        await _fileProcessingService.RecordDownloadAsync(user, fileName, fileInfo.Length, "Image");

        _logger.LogInformation("Image downloaded: {FileName} by User: {UserId}", fileName, user?.Id ?? "Anonymous");

        return PhysicalFile(filePath, contentType, fileName);
    }

    [HttpPost]
    public async Task<IActionResult> GetImageInfo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "info");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            
            var (width, height, format, fileSize) = _imageProcessingService.GetImageInfo(filePath);
            
            return Json(new {
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
            return Json(new { success = false, message = "An error occurred while reading the image." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateThumbnail(IFormFile file, int maxSize)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "thumb");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "thumb");

            var (success, outputPath, resultMessage) = await _imageProcessingService.CreateThumbnailAsync(
                filePath, outputFileName, maxSize);

            return Json(new { 
                success, 
                message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating thumbnail");
            return Json(new { success = false, message = "An error occurred while creating the thumbnail." });
        }
    }

    #endregion

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        if (User.Identity?.IsAuthenticated == true && _userManager != null)
        {
            return await _userManager.GetUserAsync(User);
        }
        return null;
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".ico" => "image/x-icon",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
