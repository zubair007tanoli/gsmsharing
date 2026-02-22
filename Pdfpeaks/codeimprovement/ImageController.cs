using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Pdfpeaks.Models;
using Pdfpeaks.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Pdfpeaks.Controllers;

/// <summary>
/// Controller for Image processing operations.
/// Includes CamScanner-style document scanning with paper size support.
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

    public IActionResult Index() => View();

    #region Resize

    [HttpGet] public IActionResult Resize() => View();

    [HttpPost]
    public async Task<IActionResult> Resize(IFormFile file, int width, int height, string mode = "stretch")
    {
        if (file == null || file.Length == 0)
        { TempData["Error"] = "Please upload an image file."; return View(); }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        if (!canProcess) { TempData["Error"] = message; return View(); }

        try
        {
            var resizeMode = mode switch
            {
                "crop" => ResizeMode.Crop, "pad" => ResizeMode.Pad,
                "max"  => ResizeMode.Max,  "min" => ResizeMode.Min,
                _      => ResizeMode.Stretch
            };

            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "resize");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "resize");

            var (success, _, resultMessage) = await _imageProcessingService.ResizeImageAsync(
                filePath, outputFileName, width, height, resizeMode);

            if (success)
            {
                TempData["Success"] = resultMessage;
                TempData["Width"]   = width;
                TempData["Height"]  = height;
                TempData["DownloadUrl"] = $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}";
                TempData["FileName"]    = outputFileName;
            }
            else TempData["Error"] = resultMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            TempData["Error"] = "An error occurred while resizing the image.";
        }
        return View();
    }

    #endregion

    #region Crop

    [HttpGet] public IActionResult Crop() => View();

    [HttpPost]
    public async Task<IActionResult> Crop(IFormFile file, int x, int y, int width, int height)
    {
        if (file == null || file.Length == 0)
        { TempData["Error"] = "Please upload an image file."; return View(); }

        if (width <= 0 || height <= 0)
        { TempData["Error"] = "Invalid crop dimensions. Width and height must be positive."; return View(); }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        if (!canProcess) { TempData["Error"] = message; return View(); }

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "crop");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "crop");

            var (success, outputPath, resultMessage) = await _imageProcessingService.CropImageAsync(
                filePath, outputFileName, x, y, width, height);

            if (success)
            {
                TempData["Success"]    = resultMessage;
                TempData["CropWidth"]  = width;
                TempData["CropHeight"] = height;
                TempData["DownloadUrl"] = $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}";
                TempData["FileName"]    = outputFileName;

                string? jpgOutputPath = null;
                try
                {
                    var jpgFn = _fileProcessingService.GenerateFileName("cropped.jpg", "crop-jpg");
                    var (jpgOk, jpgPath, _) = await _imageProcessingService.ConvertImageFormatAsync(outputPath, jpgFn, "jpg");
                    if (jpgOk) { jpgOutputPath = jpgPath; TempData["JpgDownloadUrl"] = $"/Image/Download?fileName={Uri.EscapeDataString(jpgFn)}"; }
                }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to generate JPG of cropped image"); }

                try
                {
                    var pdfFn = _fileProcessingService.GenerateFileName("cropped.pdf", "crop-pdf");
                    var (pdfOk, _, _) = await _imageProcessingService.ConvertToPdfAsync(jpgOutputPath ?? outputPath, pdfFn);
                    if (pdfOk) TempData["PdfDownloadUrl"] = $"/Image/Download?fileName={Uri.EscapeDataString(pdfFn)}";
                }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to generate PDF of cropped image"); }
            }
            else TempData["Error"] = resultMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cropping image");
            TempData["Error"] = "An error occurred while cropping the image: " + ex.Message;
        }
        return View();
    }

    #endregion

    #region Convert

    [HttpGet] public IActionResult Convert() => View();

    [HttpPost]
    public async Task<IActionResult> Convert(IFormFile file, string targetFormat, int quality = 85)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        if (!canProcess) return Json(new { success = false, message, requiresUpgrade = true });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, $"convert_{targetFormat}");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = Path.ChangeExtension(fileName, $".{targetFormat}");

            var (success, _, resultMessage) = await _imageProcessingService.ConvertImageFormatAsync(
                filePath, outputFileName, targetFormat);

            return Json(new
            {
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

    [HttpGet] public IActionResult ToJpg()  => View();
    [HttpGet] public IActionResult ToPng()  => View();
    [HttpGet] public IActionResult ToWebp() => View();
    [HttpGet] public IActionResult ToGif()  => View();
    [HttpGet] public IActionResult ToBmp()  => View();
    [HttpGet] public IActionResult ToIco()  => View();

    #endregion

    #region Transform

    [HttpGet] public IActionResult Rotate() => View();

    [HttpPost]
    public async Task<IActionResult> Rotate(IFormFile file, float degrees)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "rotate");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "rotate");

            var (success, _, resultMessage) = await _imageProcessingService.RotateImageAsync(filePath, outputFileName, degrees);
            return Json(new { success, message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating image");
            return Json(new { success = false, message = "An error occurred while rotating the image." });
        }
    }

    [HttpGet] public IActionResult Flip() => View();

    [HttpPost]
    public async Task<IActionResult> Flip(IFormFile file, bool horizontal)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "flip");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "flip");

            var (success, _, resultMessage) = await _imageProcessingService.FlipImageAsync(filePath, outputFileName, horizontal);
            return Json(new { success, message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flipping image");
            return Json(new { success = false, message = "An error occurred while flipping the image." });
        }
    }

    #endregion

    #region Compress

    [HttpGet] public IActionResult Compress() => View();

    [HttpPost]
    public async Task<IActionResult> Compress(IFormFile file, int quality = 75)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        if (!canProcess) return Json(new { success = false, message, requiresUpgrade = true });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "compress");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "compress");

            var (success, _, resultMessage, originalSize, compressedSize) =
                await _imageProcessingService.CompressImageAsync(filePath, outputFileName, quality);

            return Json(new { success, message = resultMessage,
                compressionRatio = originalSize > 0 ? (double)compressedSize / originalSize : 0,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing image");
            return Json(new { success = false, message = "An error occurred while compressing the image." });
        }
    }

    #endregion

    #region PDF Conversion

    [HttpGet] public IActionResult ToPdf() => View();

    /// <summary>
    /// Convert image to PDF with selectable paper size (A4, Letter, Legal, A3, auto).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToPdf(IFormFile file, string paperSize = "auto")
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "img2pdf");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("image.pdf", "img2pdf");

            var (success, _, resultMessage) = await _imageProcessingService.ConvertToPdfAsync(
                filePath, outputFileName, paperSize);

            return Json(new { success, message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image to PDF");
            return Json(new { success = false, message = "An error occurred during PDF conversion." });
        }
    }

    /// <summary>
    /// Convert multiple images to a single PDF (one page per image) with paper size selection.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MultiImagesToPdf(List<IFormFile> files, string paperSize = "a4")
    {
        if (files == null || files.Count == 0)
            return Json(new { success = false, message = "No files uploaded." });

        try
        {
            var paths = new List<string>();
            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                var fn   = await _fileProcessingService.SaveUploadedFileAsync(file, "multi_pdf");
                paths.Add(_fileProcessingService.GetFilePath(fn));
            }

            if (paths.Count == 0)
                return Json(new { success = false, message = "No valid images uploaded." });

            var outputFileName = _fileProcessingService.GenerateFileName("combined.pdf", "multi_pdf");

            var ps = paperSize.ToLower() switch
            {
                "a4"     => PdfPageSize.A4,
                "a3"     => PdfPageSize.A3,
                "letter" => PdfPageSize.Letter,
                "legal"  => PdfPageSize.Legal,
                _        => PdfPageSize.Auto
            };

            var result = await _imageProcessingService.ImagesToPdfAsync(paths.ToArray(), outputFileName, ps);

            return Json(new { success = result.Success, message = result.Message,
                downloadUrl = result.Success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating multi-image PDF");
            return Json(new { success = false, message = "An error occurred." });
        }
    }

    /// <summary>
    /// Legacy endpoint kept for backward compatibility.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ConvertToPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "ctopdf");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("image.pdf", "ctopdf");

            var (success, _, resultMessage) = await _imageProcessingService.ConvertToPdfAsync(filePath, outputFileName);
            return Json(new { success, message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ConvertToPdf");
            return Json(new { success = false, message = "An error occurred." });
        }
    }

    [HttpGet] public IActionResult FromPdf() => View();

    [HttpPost]
    public async Task<IActionResult> FromPdf(IFormFile file, int dpi = 150)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "frompdf");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputPrefix = Path.GetFileNameWithoutExtension(fileName);

            var (success, outputPaths, resultMessage) =
                await _imageProcessingService.ConvertPdfToImagesAsync(filePath, outputPrefix, dpi);

            try { System.IO.File.Delete(filePath); } catch { }

            if (success && outputPaths.Count > 0)
            {
                var files = outputPaths.Select(p =>
                {
                    var fn = Path.GetFileName(p);
                    return new { fileName = fn, url = $"/Image/Download?fileName={Uri.EscapeDataString(fn)}" };
                }).ToList();

                return Json(new { success = true, message = resultMessage, pageCount = outputPaths.Count, files });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to images");
            return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    #endregion

    #region Edit / Filter / Effects

    [HttpGet]
    public IActionResult Edit(string? filePath)
    {
        ViewData["InitialFilePath"] = filePath;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Adjust(IFormFile file,
        float brightness = 1.0f, float contrast = 1.0f, float saturation = 1.0f)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "adjust");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "adjusted");

            var (success, _, resultMessage) = await _imageProcessingService.AdjustImageAsync(
                filePath, outputFileName, brightness, contrast, saturation);

            if (success)
            {
                TempData["Success"] = resultMessage;
                TempData["DownloadUrl"] = $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}";
                TempData["FileName"] = outputFileName;
                return RedirectToAction("Edit", new { filePath = outputFileName });
            }

            return Json(new { success, message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting image");
            TempData["Error"] = "An error occurred while adjusting the image.";
            if (file != null && file.Length > 0)
            {
                var fn = await _fileProcessingService.SaveUploadedFileAsync(file, "adjust_error");
                return RedirectToAction("Edit", new { filePath = fn });
            }
            return RedirectToAction("Edit");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddWatermark(IFormFile file, string text, float opacity, int fontSize)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "watermark");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "watermark");

            var (success, _, resultMessage) = await _imageProcessingService.AddWatermarkAsync(
                filePath, outputFileName, text, opacity, fontSize);

            return Json(new { success, message = resultMessage,
                downloadUrl = success ? $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding watermark");
            return Json(new { success = false, message = "An error occurred while adding watermark." });
        }
    }

    #endregion

    #region AI Enhancement

    /// <summary>
    /// Full CamScanner-style document scan with paper size selection.
    /// Detects document edges, corrects perspective, and fits to paper size.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ScanDocument(IFormFile file,
        string mode = "document", string paper = "a4")
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "scan");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, $"scan_{paper}");

            var (success, outputPath, resultMessage) =
                await _imageProcessingService.ScanDocumentWithAIAsync(filePath, outputFileName, mode, paper);

            if (success)
            {
                var bytes   = await System.IO.File.ReadAllBytesAsync(outputPath);
                var b64     = System.Convert.ToBase64String(bytes);
                var ext     = Path.GetExtension(outputFileName).ToLowerInvariant();
                var ctype   = ext is ".jpg" or ".jpeg" ? "image/jpeg" : "image/png";

                // Also produce a PDF at the same paper size
                string? pdfUrl = null;
                try
                {
                    var pdfFn = _fileProcessingService.GenerateFileName("scanned.pdf", $"scan_{paper}_pdf");
                    var (pdfOk, _, _) = await _imageProcessingService.ConvertToPdfAsync(outputPath, pdfFn, paper);
                    if (pdfOk) pdfUrl = $"/Image/Download?fileName={Uri.EscapeDataString(pdfFn)}";
                }
                catch { /* non-critical */ }

                return Json(new
                {
                    success    = true,
                    message    = resultMessage,
                    imageData  = $"data:{ctype};base64,{b64}",
                    downloadUrl = $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}",
                    pdfUrl,
                    fileName   = outputFileName,
                    paper
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ScanDocument");
            return Json(new { success = false, message = "An error occurred during document scanning." });
        }
    }

    /// <summary>
    /// AI enhancement (legacy, delegates to scan pipeline with paper=auto).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> EnhanceWithAI(IFormFile file, string mode = "document")
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "ai_enhance");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "enhanced");

            var (success, outputPath, resultMessage) =
                await _imageProcessingService.EnhanceImageWithAIAsync(filePath, outputFileName, mode);

            if (success)
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(outputPath);
                var b64   = System.Convert.ToBase64String(bytes);
                var ext   = Path.GetExtension(outputFileName).ToLowerInvariant();
                var ctype = ext is ".jpg" or ".jpeg" ? "image/jpeg" : "image/png";

                return Json(new { success = true, message = resultMessage,
                    imageData  = $"data:{ctype};base64,{b64}",
                    downloadUrl = $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}",
                    fileName   = outputFileName });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EnhanceWithAI");
            return Json(new { success = false, message = "An error occurred during enhancement." });
        }
    }

    /// <summary>
    /// Auto-straighten image endpoint.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> StraightenImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, "straighten");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "straightened");

            var (success, outputPath, resultMessage) =
                await _imageProcessingService.StraightenImageAsync(filePath, outputFileName);

            if (success)
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(outputPath);
                var b64   = System.Convert.ToBase64String(bytes);
                var ext   = Path.GetExtension(outputFileName).ToLowerInvariant();
                var ctype = ext is ".jpg" or ".jpeg" ? "image/jpeg" : "image/png";

                return Json(new { success = true, message = resultMessage,
                    imageData  = $"data:{ctype};base64,{b64}",
                    downloadUrl = $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}",
                    fileName   = outputFileName });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error straightening image");
            return Json(new { success = false, message = "An error occurred while straightening the image." });
        }
    }

    /// <summary>
    /// Document enhancement (white background, border removal, contrast).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> EnhanceDocument(IFormFile file, string operation)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName   = await _fileProcessingService.SaveUploadedFileAsync(file, $"doc_{operation}");
            var filePath   = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, $"doc_{operation}");

            var (success, outputPath, resultMessage) =
                await _imageProcessingService.EnhanceImageWithAIAsync(filePath, outputFileName, "document");

            if (success)
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(outputPath);
                var b64   = System.Convert.ToBase64String(bytes);
                var ext   = Path.GetExtension(outputFileName).ToLowerInvariant();
                var ctype = ext is ".jpg" or ".jpeg" ? "image/jpeg" : "image/png";

                return Json(new { success = true, message = resultMessage,
                    imageData  = $"data:{ctype};base64,{b64}",
                    downloadUrl = $"/Image/Download?fileName={Uri.EscapeDataString(outputFileName)}",
                    fileName   = outputFileName });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing document");
            return Json(new { success = false, message = "An error occurred during document enhancement." });
        }
    }

    #endregion

    #region Download and Info

    [HttpGet]
    public async Task<IActionResult> Download(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return NotFound();

        var decoded = Uri.UnescapeDataString(fileName);

        var user = await GetCurrentUserAsync();
        var (canDownload, message) = await _fileProcessingService.CanDownloadAsync(user);
        if (!canDownload)
            return Json(new { success = false, message, requiresUpgrade = true });

        var filePath = Path.Combine(_environment.ContentRootPath, "temp_files", decoded);
        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found or has expired.");

        var fileBytes   = await System.IO.File.ReadAllBytesAsync(filePath);
        var contentType = GetContentType(decoded);
        var cd = new System.Net.Mime.ContentDisposition { FileName = decoded, Inline = false };
        Response.Headers["Content-Disposition"] = cd.ToString();
        Response.Headers["Content-Length"]      = fileBytes.Length.ToString();
        return File(fileBytes, contentType, decoded);
    }

    [HttpGet]
    public IActionResult GetFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest(new { success = false, message = "No file specified." });

        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        try { return PhysicalFile(filePath, GetContentType(fileName)); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving file");
            return StatusCode(500, new { success = false, message = "Error serving file." });
        }
    }

    [HttpGet]
    public IActionResult ImageInfo(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return Json(new { success = false, message = "No file specified." });

        var filePath = Path.Combine(_environment.ContentRootPath, "temp_files", fileName);
        if (!System.IO.File.Exists(filePath))
            return Json(new { success = false, message = "File not found." });

        try
        {
            var (width, height, format, fileSize) = _imageProcessingService.GetImageInfo(filePath);
            return Json(new { success = true, fileName, width, height, format, fileSize,
                formattedSize = FormatSize(fileSize) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image info");
            return Json(new { success = false, message = "An error occurred while getting image info." });
        }
    }

    #endregion

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        if (User.Identity?.IsAuthenticated == true && _userManager != null)
            return await _userManager.GetUserAsync(User);
        return null;
    }

    private static string GetContentType(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf"              => "application/pdf",
            ".jpg" or ".jpeg"   => "image/jpeg",
            ".png"              => "image/png",
            ".gif"              => "image/gif",
            ".bmp"              => "image/bmp",
            ".webp"             => "image/webp",
            ".ico"              => "image/x-icon",
            _                   => "application/octet-stream"
        };
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes; int order = 0;
        while (len >= 1024 && order < sizes.Length - 1) { order++; len /= 1024; }
        return $"{len:0.##} {sizes[order]}";
    }
}
