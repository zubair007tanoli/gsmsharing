using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pdfpeaks.Models;
using Pdfpeaks.Services;
using System.Text.Json;

namespace Pdfpeaks.Controllers;

/// <summary>
/// Controller for PDF processing operations
/// </summary>
public class PdfController : Controller
{
    private readonly ILogger<PdfController> _logger;
    private readonly UserManager<ApplicationUser>? _userManager;
    private readonly FileProcessingService _fileProcessingService;
    private readonly PdfProcessingService _pdfProcessingService;
    private readonly IWebHostEnvironment _environment;

    public PdfController(
        ILogger<PdfController> logger,
        UserManager<ApplicationUser>? userManager,
        FileProcessingService fileProcessingService,
        PdfProcessingService pdfProcessingService,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _userManager = userManager;
        _fileProcessingService = fileProcessingService;
        _pdfProcessingService = pdfProcessingService;
        _environment = environment;
    }

    public IActionResult Index()
    {
        return View();
    }

    #region Merge Operations

    [HttpGet]
    public IActionResult Merge()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Merge(List<IFormFile> files, string sortOrder)
    {
        if (files == null || files.Count < 2)
        {
            TempData["Error"] = "Please upload at least 2 PDF files to merge.";
            return View();
        }

        var user = await GetCurrentUserAsync();
        var totalSize = files.Sum(f => f.Length);
        
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, totalSize);
        if (!canProcess)
        {
            TempData["Error"] = message;
            return View();
        }

        try
        {
            // Save uploaded files
            var filePaths = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "merge");
                    filePaths.Add(_fileProcessingService.GetFilePath(fileName));
                }
            }

            // Parse sort order (JSON array of indices)
            var sortOrderList = new List<int>();
            if (!string.IsNullOrEmpty(sortOrder))
            {
                try
                {
                    sortOrderList = JsonSerializer.Deserialize<List<int>>(sortOrder) ?? new List<int>();
                }
                catch
                {
                    // Use default order if parsing fails
                    sortOrderList = Enumerable.Range(0, filePaths.Count).ToList();
                }
            }
            else
            {
                sortOrderList = Enumerable.Range(0, filePaths.Count).ToList();
            }

            var outputFileName = _fileProcessingService.GenerateFileName("merged.pdf", "merge");
            var (success, outputPath, resultMessage) = await _pdfProcessingService.MergePdfAsync(
                filePaths, sortOrderList, outputFileName);

            if (success)
            {
                TempData["Success"] = resultMessage;
                TempData["DownloadUrl"] = $"/Pdf/Download?fileName={outputFileName}";
                TempData["FileName"] = outputFileName;
            }
            else
            {
                TempData["Error"] = resultMessage;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging PDFs");
            TempData["Error"] = "An error occurred while merging the PDF files.";
        }

        return View();
    }

    #endregion

    #region Split Operations

    [HttpGet]
    public IActionResult Split()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Split(IFormFile file, int startPage, int endPage)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please upload a PDF file.";
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
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "split");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            
            // Get PDF info first
            var info = _pdfProcessingService.GetPdfInfo(filePath);
            var pageCount = info.pageCount;
            TempData["PageCount"] = pageCount;
            TempData["PageSizes"] = string.Join(",", info.pageSizes);

            if (startPage < 1) startPage = 1;
            if (endPage > pageCount) endPage = pageCount;
            if (startPage > endPage) startPage = endPage;

            var outputPrefix = Path.GetFileNameWithoutExtension(fileName);
            var (success, outputPaths, resultMessage) = await _pdfProcessingService.SplitPdfAsync(
                filePath, startPage, endPage, outputPrefix);

            if (success)
            {
                TempData["Success"] = resultMessage;
                TempData["OutputFiles"] = string.Join(",", outputPaths.Select(Path.GetFileName));
                TempData["FileName"] = outputPaths.FirstOrDefault() != null ? Path.GetFileName(outputPaths.First()) : "";
                TempData["MultipleFiles"] = outputPaths.Count > 1;
            }
            else
            {
                TempData["Error"] = resultMessage;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error splitting PDF");
            TempData["Error"] = "An error occurred while splitting the PDF file.";
        }

        return View();
    }

    #endregion

    #region Compress Operations

    [HttpGet]
    public IActionResult Compress()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Compress(IFormFile file, string quality = "medium")
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please upload a PDF file.";
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
            var qualityValue = quality switch
            {
                "low" => 50,
                "medium" => 75,
                "high" => 90,
                _ => 75
            };

            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "compress");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("compressed.pdf", "compress");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.CompressPdfAsync(
                filePath, qualityValue, outputFileName);

            if (success)
            {
                var originalSize = new FileInfo(filePath).Length;
                var compressedSize = new FileInfo(outputPath).Length;
                var reduction = (1 - (double)compressedSize / originalSize) * 100;

                TempData["Success"] = resultMessage;
                TempData["OriginalSize"] = FormatSize(originalSize);
                TempData["CompressedSize"] = FormatSize(compressedSize);
                TempData["Reduction"] = $"{reduction:F1}%";
                TempData["DownloadUrl"] = $"/Pdf/Download?fileName={outputFileName}";
                TempData["FileName"] = outputFileName;
            }
            else
            {
                TempData["Error"] = resultMessage;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing PDF");
            TempData["Error"] = "An error occurred while compressing the PDF file.";
        }

        return View();
    }

    #endregion

    #region Convert Operations

    [HttpGet]
    public IActionResult ConvertToWord()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToWord(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2word");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = Path.ChangeExtension(fileName, ".docx");
            
            // Note: Basic text extraction - full formatting requires commercial libraries
            return Json(new { 
                success = true, 
                message = "PDF to Word conversion requires commercial libraries for full formatting.",
                downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(outputFileName)}" ,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to Word");
            return Json(new { success = false, message = "An error occurred during conversion." });
        }
    }

    [HttpGet]
    public IActionResult ConvertToExcel()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ConvertToPowerPoint()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ConvertFromWord()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ConvertFromExcel()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ConvertFromPowerPoint()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ConvertToJpg()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ConvertToPng()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ConvertFromJpg()
    {
        return View();
    }

    #endregion

    #region Organize Operations

    [HttpGet]
    public IActionResult Organize()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Organize(List<IFormFile> files, string pageActions)
    {
        if (files == null || files.Count == 0)
        {
            TempData["Error"] = "Please upload at least one PDF file.";
            return View();
        }

        var user = await GetCurrentUserAsync();
        var totalSize = files.Sum(f => f.Length);
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, totalSize);
        
        if (!canProcess)
        {
            TempData["Error"] = message;
            return View();
        }

        // Parse page actions JSON
        var actions = new List<PageAction>();
        if (!string.IsNullOrEmpty(pageActions))
        {
            try
            {
                actions = JsonSerializer.Deserialize<List<PageAction>>(pageActions) ?? new List<PageAction>();
            }
            catch
            {
                TempData["Error"] = "Invalid page actions.";
                return View();
            }
        }

        // Process organization (move, delete, rotate pages)
        // This is a simplified implementation
        
        return View();
    }

    #endregion

    #region Edit Operations

    [HttpGet]
    public IActionResult Edit()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Rotate()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Rotate(IFormFile file, List<int> pageNumbers, int degrees)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        if (pageNumbers == null || pageNumbers.Count == 0)
        {
            return Json(new { success = false, message = "Please select at least one page." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "rotate");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("rotated.pdf", "rotate");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.RotatePdfAsync(
                filePath, pageNumbers, degrees, outputFileName);

            return Json(new { 
                success, 
                message = resultMessage,
                downloadUrl = success ? $"/Pdf/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating PDF");
            return Json(new { success = false, message = "An error occurred while rotating pages." });
        }
    }

    [HttpGet]
    public IActionResult Unlock()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Protect()
    {
        return View();
    }

    #endregion

    #region Download and Processing

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
        
        await _fileProcessingService.RecordDownloadAsync(user, fileName, fileInfo.Length, "PDF");

        _logger.LogInformation("File downloaded: {FileName} by User: {UserId}", fileName, user?.Id ?? "Anonymous");

        return PhysicalFile(filePath, contentType, fileName);
    }

    [HttpPost]
    public async Task<IActionResult> GetPdfInfo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "info");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            
            var (pageCount, fileSize, pageSizes) = _pdfProcessingService.GetPdfInfo(filePath);
            
            return Json(new {
                success = true,
                pageCount,
                fileSize,
                formattedSize = FormatSize(fileSize),
                pageSizes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PDF info");
            return Json(new { success = false, message = "An error occurred while reading the PDF." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddPageNumbers(IFormFile file, string position, int fontSize)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pagenum");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("with_pagenums.pdf", "pagenum");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.AddPageNumbersAsync(
                filePath, outputFileName, fontSize, position);

            return Json(new { 
                success, 
                message = resultMessage,
                downloadUrl = success ? $"/Pdf/Download?fileName={Uri.EscapeDataString(outputFileName)}" : null,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding page numbers");
            return Json(new { success = false, message = "An error occurred while adding page numbers." });
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
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
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

/// <summary>
/// Represents a page action for PDF organization
/// </summary>
public class PageAction
{
    public int PageNumber { get; set; }
    public string Action { get; set; } = ""; // "move", "delete", "rotate"
    public int NewPosition { get; set; }
    public int RotateDegrees { get; set; }
}
