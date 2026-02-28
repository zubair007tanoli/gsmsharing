using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Pdfpeaks.Models;
using Pdfpeaks.Services;

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
    private readonly ImageProcessingService _imageProcessingService;
    private readonly ImageEnhancementService _imageEnhancementService;
    private readonly IWebHostEnvironment _environment;

    public PdfController(
        ILogger<PdfController> logger,
        UserManager<ApplicationUser>? userManager,
        FileProcessingService fileProcessingService,
        PdfProcessingService pdfProcessingService,
        ImageProcessingService imageProcessingService,
        ImageEnhancementService imageEnhancementService,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _userManager = userManager;
        _fileProcessingService = fileProcessingService;
        _pdfProcessingService = pdfProcessingService;
        _imageProcessingService = imageProcessingService;
        _imageEnhancementService = imageEnhancementService;
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
    public async Task<IActionResult> Merge([FromForm] IFormFile[] files, [FromForm] string sortOrder)
    {
        if (files == null || files.Length < 2)
        {
            return Json(new { success = false, message = "Please select at least 2 PDF files to merge." });
        }

        var user = await GetCurrentUserAsync();
        
        try
        {
            var inputFiles = new List<string>();
            
            // Save all uploaded files
            for (int i = 0; i < files.Length; i++)
            {
                var uploadedFile = files[i];
                if (uploadedFile.Length == 0) continue;
                
                var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, uploadedFile.Length);
                if (!canProcess)
                {
                    return Json(new { success = false, message = authMessage });
                }
                
                var fileName = await _fileProcessingService.SaveUploadedFileAsync(uploadedFile, $"merge_{i}");
                var filePath = _fileProcessingService.GetFilePath(fileName);
                inputFiles.Add(filePath);
            }

            if (inputFiles.Count < 2)
            {
                return Json(new { success = false, message = "At least 2 valid PDF files are required." });
            }

            // Parse sort order
            var order = new List<int>();
            if (!string.IsNullOrEmpty(sortOrder))
            {
                try
                {
                    order = System.Text.Json.JsonSerializer.Deserialize<List<int>>(sortOrder) ?? Enumerable.Range(0, inputFiles.Count).ToList();
                }
                catch
                {
                    order = Enumerable.Range(0, inputFiles.Count).ToList();
                }
            }
            else
            {
                order = Enumerable.Range(0, inputFiles.Count).ToList();
            }

            var outputFileName = _fileProcessingService.GenerateFileName("merged.pdf", "merge");
            var (success, outputPath, mergeMessage) = await _pdfProcessingService.MergePdfAsync(inputFiles, order, outputFileName);

            if (success && System.IO.File.Exists(outputPath))
            {
                // Record download
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, outputFileName, fileInfo.Length, "application/pdf");
                
                var downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(outputFileName)}";
                return Json(new { 
                    success = true, 
                    message = mergeMessage,
                    downloadUrl,
                    fileName = outputFileName
                });
            }

            return Json(new { success = false, message = mergeMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging PDFs");
            return Json(new { success = false, message = $"Error merging PDFs: {ex.Message}" });
        }
    }

    #endregion

    #region Split Operations

    [HttpGet]
    public IActionResult Split()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SplitWithFile(
        [FromForm] IFormFile file,
        [FromForm] int? startPage,
        [FromForm] int? endPage,
        [FromForm] string? selectedPages)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "split");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "split");
            bool success;
            string resultMessage;
            string outputFile;

            // If specific pages are provided, extract exactly those pages (supports non-contiguous selection).
            if (!string.IsNullOrWhiteSpace(selectedPages))
            {
                var pageOrder = selectedPages
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => int.TryParse(s, out var p) ? p : -1)
                    .Where(p => p > 0)
                    .Distinct()
                    .ToList();

                if (pageOrder.Count == 0)
                {
                    return Json(new { success = false, message = "Please select at least one valid page." });
                }

                var organizeResult = await _pdfProcessingService.OrganizePdfAsync(filePath, pageOrder, outputFileName);
                success = organizeResult.success;
                outputFile = organizeResult.outputPath;
                resultMessage = organizeResult.message;
            }
            else
            {
                var start = startPage ?? 1;
                var end = endPage ?? start;
                var splitResult = await _pdfProcessingService.SplitPdfAsync(
                    filePath, start, end, Path.GetFileNameWithoutExtension(outputFileName));

                success = splitResult.success;
                outputFile = splitResult.outputPaths.FirstOrDefault() ?? string.Empty;
                resultMessage = splitResult.message;
            }

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && !string.IsNullOrWhiteSpace(outputFile) && System.IO.File.Exists(outputFile))
            {
                var fileInfo = new FileInfo(outputFile);
                await _fileProcessingService.RecordDownloadAsync(user, Path.GetFileName(outputFile), fileInfo.Length, "application/pdf");
                
                var resultFileName = Path.GetFileName(outputFile);
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    files = new[] { new { url = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}", fileName = resultFileName } }
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error splitting PDF");
            return Json(new { success = false, message = $"Error splitting PDF: {ex.Message}" });
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
    public async Task<IActionResult> Compress([FromForm] IFormFile file, int quality = 50)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "compress");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "compress");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.CompressPdfAsync(
                filePath, quality, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, outputFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(outputFileName)}",
                    fileName = outputFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing PDF");
            return Json(new { success = false, message = $"Error compressing PDF: {ex.Message}" });
        }
    }

    #endregion

    #region Convert to Word

    [HttpGet]
    public IActionResult ConvertToWord()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToWord([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        // Validate file is PDF
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".pdf")
        {
            return Json(new { success = false, message = "Only PDF files can be converted to Word." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2word");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = Path.ChangeExtension(fileName, ".docx");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.ConvertToWordAsync(
                filePath, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to Word");
            return Json(new { success = false, message = $"Error converting PDF to Word: {ex.Message}" });
        }
    }

    #endregion

    #region Convert to Excel

    [HttpGet]
    public IActionResult ConvertToExcel()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToExcel(
        [FromForm] IFormFile file,
        [FromForm] string extractionMode = "exact",
        [FromForm] bool aiEnhance = true)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        // Validate file is PDF
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".pdf")
        {
            return Json(new { success = false, message = "Only PDF files can be converted to Excel." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2excel");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = Path.ChangeExtension(fileName, ".xlsx");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.ConvertToExcelAsync(
                filePath, outputFileName, extractionMode, aiEnhance);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to Excel");
            return Json(new { success = false, message = $"Error converting PDF to Excel: {ex.Message}" });
        }
    }

    #endregion

    #region Convert to PowerPoint

    [HttpGet]
    public IActionResult ConvertToPowerPoint()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToPowerPoint([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        // Validate file is PDF
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".pdf")
        {
            return Json(new { success = false, message = "Only PDF files can be converted to PowerPoint." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2pptx");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = Path.ChangeExtension(fileName, ".pptx");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.ConvertToPowerPointAsync(
                filePath, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/vnd.openxmlformats-officedocument.presentationml.presentation");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to PowerPoint");
            return Json(new { success = false, message = $"Error converting PDF to PowerPoint: {ex.Message}" });
        }
    }

    #endregion

    #region Convert to JPG

    [HttpGet]
    public IActionResult ConvertToJpg()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToJpg([FromForm] IFormFile file, string selectedPages, int dpi = 150)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        // Validate file is PDF
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".pdf")
        {
            return Json(new { success = false, message = "Only PDF files can be converted to images." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2jpg");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputPrefix = Path.GetFileNameWithoutExtension(fileName);

            // Parse selected pages if provided
            List<int>? pages = null;
            if (!string.IsNullOrEmpty(selectedPages))
            {
                try
                {
                    pages = selectedPages.Split(',').Select(s => int.Parse(s.Trim())).ToList();
                }
                catch { }
            }

            var (success, outputPaths, resultMessage) = await _pdfProcessingService.ConvertToImagesAsync(
                filePath, outputPrefix, "jpg", pages);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && outputPaths.Count > 0)
            {
                var downloadUrls = outputPaths.Select(path => new { 
                    url = $"/Pdf/Download?fileName={Uri.EscapeDataString(Path.GetFileName(path))}",
                    fileName = Path.GetFileName(path)
                }).ToList();
                
                return Json(new { 
                    success = true, 
                    message = $"Successfully converted {outputPaths.Count} pages to JPG.",
                    files = downloadUrls
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to JPG");
            return Json(new { success = false, message = $"Error converting PDF to JPG: {ex.Message}" });
        }
    }

    #endregion

    #region Convert to PNG

    [HttpGet]
    public IActionResult ConvertToPng()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToPng([FromForm] IFormFile file, string selectedPages, int dpi = 150)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        // Validate file is PDF
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".pdf")
        {
            return Json(new { success = false, message = "Only PDF files can be converted to PNG." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2png");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputPrefix = Path.GetFileNameWithoutExtension(fileName);

            // Parse selected pages if provided
            List<int>? pages = null;
            if (!string.IsNullOrEmpty(selectedPages))
            {
                try
                {
                    pages = selectedPages.Split(',').Select(s => int.Parse(s.Trim())).ToList();
                }
                catch { }
            }

            var (success, outputPaths, resultMessage) = await _pdfProcessingService.ConvertToImagesAsync(
                filePath, outputPrefix, "png", pages);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && outputPaths.Count > 0)
            {
                var downloadUrls = outputPaths.Select(path => new { 
                    url = $"/Pdf/Download?fileName={Uri.EscapeDataString(Path.GetFileName(path))}",
                    fileName = Path.GetFileName(path)
                }).ToList();
                
                return Json(new { 
                    success = true, 
                    message = $"Successfully converted {outputPaths.Count} pages to PNG.",
                    files = downloadUrls
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to PNG");
            return Json(new { success = false, message = $"Error converting PDF to PNG: {ex.Message}" });
        }
    }

    #endregion

    #region Get PDF Preview

    [HttpPost]
    public async Task<IActionResult> GetPdfPreview([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "preview");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            // Get PDF info
            var (pageCount, fileSize, pageSizes) = _pdfProcessingService.GetPdfInfo(filePath);

            // Cleanup temp file
            try { System.IO.File.Delete(filePath); } catch { }

            return Json(new { 
                success = true, 
                pageCount,
                fileSize,
                pageSizes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PDF preview");
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    #endregion

    #region Get PDF Info

    [HttpPost]
    public async Task<IActionResult> GetPdfInfo([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "info");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            // Get PDF info
            var (pageCount, fileSize, pageSizes) = _pdfProcessingService.GetPdfInfo(filePath);

            return Json(new { 
                success = true, 
                pageCount,
                fileSize,
                pageSizes,
                fileName = file.FileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PDF info");
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    #endregion

    #region Get PDF Thumbnails

    [HttpPost]
    public async Task<IActionResult> GetPdfThumbnails([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "thumb");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputPrefix = Path.GetFileNameWithoutExtension(fileName);

            // Convert to images for thumbnails
            var (success, outputPaths, resultMessage) = await _pdfProcessingService.ConvertToImagesAsync(
                filePath, outputPrefix, "jpg");

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && outputPaths.Count > 0)
            {
                var thumbnails = outputPaths.Select((path, idx) => new { 
                    pageNumber = idx + 1,
                    url = $"/Pdf/Download?fileName={Uri.EscapeDataString(Path.GetFileName(path))}",
                    fileName = Path.GetFileName(path)
                }).ToList();
                
                return Json(new { 
                    success = true, 
                    thumbnails
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PDF thumbnails");
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    #endregion

    #region Convert from Word

    [HttpGet]
    public IActionResult ConvertFromWord()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertFromWord([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a Word document." });
        }

        // Validate file is Word
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".docx" && extension != ".doc")
        {
            return Json(new { success = false, message = "Only Word documents (.doc, .docx) can be converted to PDF." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "word2pdf");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            // Ensure the converted file always has a .pdf extension, even if the source is .doc/.docx
            var outputFileName = Path.ChangeExtension(fileName, ".pdf");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.ConvertWordToPdfAsync(
                filePath, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting Word to PDF");
            return Json(new { success = false, message = $"Error converting Word to PDF: {ex.Message}" });
        }
    }

    #endregion

    #region Convert from Excel

    [HttpGet]
    public IActionResult ConvertFromExcel()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertFromExcel([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload an Excel file." });
        }

        // Validate file is Excel
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return Json(new { success = false, message = "Only Excel files (.xls, .xlsx) can be converted to PDF." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "excel2pdf");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            // Force .pdf extension for the converted file
            var outputFileName = Path.ChangeExtension(fileName, ".pdf");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.ConvertExcelToPdfAsync(
                filePath, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting Excel to PDF");
            return Json(new { success = false, message = $"Error converting Excel to PDF: {ex.Message}" });
        }
    }

    #endregion

    #region Convert from PowerPoint

    [HttpGet]
    public IActionResult ConvertFromPowerPoint()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertFromPowerPoint([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PowerPoint file." });
        }

        // Validate file is PowerPoint
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".pptx" && extension != ".ppt")
        {
            return Json(new { success = false, message = "Only PowerPoint files (.ppt, .pptx) can be converted to PDF." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pptx2pdf");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            // Force .pdf extension for the converted file
            var outputFileName = Path.ChangeExtension(fileName, ".pdf");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.ConvertPowerPointToPdfAsync(
                filePath, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PowerPoint to PDF");
            return Json(new { success = false, message = $"Error converting PowerPoint to PDF: {ex.Message}" });
        }
    }

    #endregion

    #region Convert from JPG

    [HttpGet]
    public IActionResult ConvertFromJpg()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertFromJpg(IFormFile[] files)
    {
        if (files == null || files.Length == 0)
        {
            return Json(new { success = false, message = "Please upload at least one image file." });
        }

        var user = await GetCurrentUserAsync();
        long totalBytes = files.Where(f => f != null).Sum(f => f.Length);
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, totalBytes);
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tif", ".tiff"
            };

            var inputPaths = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                if (file == null || file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !allowed.Contains(ext))
                {
                    continue;
                }

                var savedFile = await _fileProcessingService.SaveUploadedFileAsync(file, $"img2pdf_{i}");
                inputPaths.Add(_fileProcessingService.GetFilePath(savedFile));
            }

            if (inputPaths.Count == 0)
            {
                return Json(new { success = false, message = "No valid image files were uploaded." });
            }

            var outputFileName = _fileProcessingService.GenerateFileName("images.pdf", "img2pdf");
            var result = await _imageProcessingService.ImagesToPdfAsync(
                inputPaths.ToArray(),
                outputFileName,
                PdfPageSize.Auto);

            foreach (var path in inputPaths)
            {
                try { System.IO.File.Delete(path); } catch { }
            }

            if (result.Success && !string.IsNullOrWhiteSpace(result.OutputPath) && System.IO.File.Exists(result.OutputPath))
            {
                var resultFileName = Path.GetFileName(result.OutputPath);
                var fileInfo = new FileInfo(result.OutputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");

                return Json(new
                {
                    success = true,
                    message = result.Message,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = result.Message ?? "Failed to convert images to PDF." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting images to PDF");
            return Json(new { success = false, message = $"Error converting images to PDF: {ex.Message}" });
        }
    }

    #endregion

    #region Rotate Operations

    [HttpGet]
    public IActionResult Rotate()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Rotate([FromForm] IFormFile file, string pages, int degrees)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "rotate");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "rotate");

            // Parse pages - format: "1,2,3" or "all"
            var pageIndices = new List<int>();
            if (pages?.ToLower() == "all")
            {
                var pageCount = _pdfProcessingService.GetPageCount(filePath);
                for (int i = 0; i < pageCount; i++)
                    pageIndices.Add(i);
            }
            else if (!string.IsNullOrEmpty(pages))
            {
                pageIndices = pages.Split(',').Select(s => int.TryParse(s.Trim(), out var n) ? n - 1 : -1)
                    .Where(n => n >= 0).ToList();
            }

            var (success, outputPath, resultMessage) = await _pdfProcessingService.RotatePdfAsync(
                filePath, pageIndices, degrees, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating PDF");
            return Json(new { success = false, message = $"Error rotating PDF: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RotateInteractive([FromForm] IFormFile file, string pages, int degrees)
    {
        // Same as Rotate - just a different endpoint name the view uses
        return await Rotate(file, pages, degrees);
    }

    /// <summary>
    /// Rotate multiple PDF pages with individual rotation angles
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RotateMultiple([FromForm] IFormFile file, [FromForm] string rotations)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        if (string.IsNullOrEmpty(rotations))
        {
            return Json(new { success = false, message = "No rotation data provided." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            // Parse rotations JSON: { "1": 90, "2": -90, "3": 180 }
            var pageRotations = new Dictionary<int, int>();
            try
            {
                var rotationsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(rotations);
                if (rotationsDict != null)
                {
                    foreach (var kvp in rotationsDict)
                    {
                        if (int.TryParse(kvp.Key, out int pageNum))
                        {
                            pageRotations[pageNum - 1] = kvp.Value; // Convert to 0-based index
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse rotations JSON");
                return Json(new { success = false, message = "Invalid rotation data format." });
            }

            if (pageRotations.Count == 0)
            {
                return Json(new { success = false, message = "No pages to rotate." });
            }

            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "rotate");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "rotate");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.RotateMultiplePagesAsync(
                filePath, pageRotations, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating multiple PDF pages");
            return Json(new { success = false, message = $"Error rotating PDF: {ex.Message}" });
        }
    }

    #endregion

    #region Organize Operations

    [HttpGet]
    public IActionResult Organize()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Organize([FromForm] IFormFile file, string pageOrder)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        if (string.IsNullOrEmpty(pageOrder))
        {
            return Json(new { success = false, message = "Please specify the page order." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "organize");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "organize");

            // Parse page order from either JSON array "[3,1,2]" or CSV "3,1,2" (1-based page numbers).
            var order = new List<int>();
            try
            {
                var trimmed = pageOrder.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    order = System.Text.Json.JsonSerializer.Deserialize<List<int>>(trimmed) ?? new List<int>();
                }
                else
                {
                    order = trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(s => int.TryParse(s, out var n) ? n : 0)
                        .Where(n => n > 0)
                        .ToList();
                }
            }
            catch
            {
                order = new List<int>();
            }

            if (order.Count == 0)
            {
                return Json(new { success = false, message = "Invalid page order format." });
            }

            var (success, outputPath, resultMessage) = await _pdfProcessingService.OrganizePdfAsync(
                filePath, order, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error organizing PDF");
            return Json(new { success = false, message = $"Error organizing PDF: {ex.Message}" });
        }
    }

    #endregion

    #region Edit Operations

    [HttpGet]
    public IActionResult Edit()
    {
        return View();
    }

    /// <summary>
    /// Extract OCR text from PDF page (for editable/searchable text)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ExtractOcrText([FromForm] IFormFile file, [FromForm] int page = 1)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "ocr");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            // Get text elements from the page
            var (pdfTextElements, pageWidth, pageHeight) = _pdfProcessingService.GetTextElementsWithDimensions(filePath, page);

            // Convert to frontend format
            var textElements = pdfTextElements?.Select(e => new TextElement 
            { 
                Text = e.Text, 
                X = e.X, 
                Y = e.Y, 
                Width = e.Width, 
                Height = e.Height, 
                FontSize = e.FontSize,
                FontFamily = e.FontFamily,
                Color = "#000000"
            }).ToList() ?? new List<TextElement>();

            // Try to use OCR for image-based PDFs
            if (textElements.Count == 0)
            {
                // Try Python OCR service
                try
                {
                    using var client = new HttpClient();
                    using var content = new MultipartFormDataContent();
                    
                    using var fileStream = System.IO.File.OpenRead(filePath);
                    using var streamContent = new StreamContent(fileStream);
                    content.Add(streamContent, "file", fileName);
                    content.Add(new StringContent(page.ToString()), "page");

                    var ocrResponse = await client.PostAsync("http://localhost:5001/api/v1/ocr", content);
                    if (ocrResponse.IsSuccessStatusCode)
                    {
                        var ocrResult = await ocrResponse.Content.ReadFromJsonAsync<OcrResponse>();
                        if (ocrResult?.Success == true && ocrResult.TextElements != null)
                        {
                            textElements = ocrResult.TextElements;
                        }
                    }
                }
                catch
                {
                    // OCR service not available, continue with basic extraction
                }
            }

            // Cleanup
            try { System.IO.File.Delete(filePath); } catch { }

            return Json(new { 
                success = true, 
                textElements = textElements,
                pageWidth,
                pageHeight
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting OCR text");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Make PDF searchable (convert image to text layer)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MakeSearchable([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "searchable");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "searchable");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.MakeSearchablePdfAsync(
                filePath, outputFileName);

            // Cleanup input
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making PDF searchable");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Crop a specific page of the PDF
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CropPage([FromForm] IFormFile file, [FromForm] int page, 
        [FromForm] int x, [FromForm] int y, [FromForm] int width, [FromForm] int height)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        if (width <= 0 || height <= 0)
        {
            return Json(new { success = false, message = "Invalid crop dimensions." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "crop");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "crop");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.CropPdfPageAsync(
                filePath, page, x, y, width, height, outputFileName);

            // Cleanup input
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cropping PDF page");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Save edited PDF with text boxes and OCR text
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SaveEdited([FromForm] IFormFile file, [FromForm] string textBoxes, [FromForm] string ocrText)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "edited");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "edited");

            List<PageTextBox>? boxes = null;
            if (!string.IsNullOrEmpty(textBoxes))
            {
                try
                {
                    boxes = System.Text.Json.JsonSerializer.Deserialize<List<PageTextBox>>(textBoxes);
                }
                catch { }
            }

            var (success, outputPath, resultMessage) = await _pdfProcessingService.SaveEditedPdfAsync(
                filePath, boxes, outputFileName);

            // Cleanup input
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving edited PDF");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] IFormFile file, string text, int pageNumber)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "edit");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "edit");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.AddTextToPdfAsync(
                filePath, text, pageNumber, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing PDF");
            return Json(new { success = false, message = $"Error editing PDF: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditAddText([FromForm] IFormFile file, string text, int pageNumber)
    {
        // Same as Edit - just a different endpoint name the view uses
        return await Edit(file, text, pageNumber);
    }

    /// <summary>
    /// Get a specific page as image for editing
    /// </summary>
    /// <summary>
    /// Get a specific page as image for editing
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetPageForEdit([FromForm] IFormFile file, int page = 1)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pageedit");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputPrefix = Path.GetFileNameWithoutExtension(fileName);

            // Convert to images
            var (success, outputPaths, resultMessage) = await _pdfProcessingService.ConvertToImagesAsync(
                filePath, outputPrefix, "png", new List<int> { page });

            if (success && outputPaths.Count > 0)
            {
                var imageUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(Path.GetFileName(outputPaths[0]))}";
                
                // Get text elements from PDF (filePath must still exist)
                var (textElements, pageWidth, pageHeight) = _pdfProcessingService.GetTextElementsWithDimensions(filePath, page);

                // Cleanup input file after we're done
                try { System.IO.File.Delete(filePath); } catch { }

                return Json(new { 
                    success = true, 
                    imageUrl,
                    textElements,
                    pageWidth,
                    pageHeight
                });
            }

            try { System.IO.File.Delete(filePath); } catch { }
            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting page for edit");
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Enhance a PDF page image before editing (CamScanner-style)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> EnhancePageForEdit(
        [FromForm] IFormFile file, 
        int page = 1,
        string mode = "document",
        bool enhance = true,
        bool deskew = false,
        bool binarize = false)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        try
        {
            // First convert the page to image
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pageenhance");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputPrefix = Path.GetFileNameWithoutExtension(fileName);

            // Convert to PNG image
            var (success, outputPaths, resultMessage) = await _pdfProcessingService.ConvertToImagesAsync(
                filePath, outputPrefix, "png", new List<int> { page });

            if (!success || outputPaths.Count == 0)
            {
                try { System.IO.File.Delete(filePath); } catch { }
                return Json(new { success = false, message = resultMessage });
            }

            var pageImagePath = outputPaths[0];
            
            // Apply enhancement if requested
            if (enhance)
            {
                var enhancedFileName = $"enhanced_{Path.GetFileName(pageImagePath)}";
                
                var options = new ImageEnhancementOptions
                {
                    Mode = mode,
                    SharpenStrength = 1.0f,
                    AutoDeskew = deskew
                };

                var (enhanceSuccess, enhancedPath, enhanceMessage) = await _imageEnhancementService.EnhanceDocumentAsync(
                    pageImagePath, enhancedFileName, options);

                if (enhanceSuccess)
                {
                    // Clean up original unenhanced image
                    try { System.IO.File.Delete(pageImagePath); } catch { }
                    pageImagePath = enhancedPath;
                }
            }

            // Apply binarization if requested
            if (binarize)
            {
                var bwFileName = $"bw_{Path.GetFileName(pageImagePath)}";
                var (bwSuccess, bwPath, bwMessage) = await _imageEnhancementService.BinarizeAsync(
                    pageImagePath, bwFileName, 128);

                if (bwSuccess)
                {
                    try { System.IO.File.Delete(pageImagePath); } catch { }
                    pageImagePath = bwPath;
                }
            }

            // Apply deskew if requested
            if (deskew && !enhance)
            {
                var deskewFileName = $"deskew_{Path.GetFileName(pageImagePath)}";
                var (deskewSuccess, deskewPath, deskewMessage) = await _imageEnhancementService.DeskewAsync(
                    pageImagePath, deskewFileName);

                if (deskewSuccess)
                {
                    try { System.IO.File.Delete(pageImagePath); } catch { }
                    pageImagePath = deskewPath;
                }
            }

            // Get text elements from original PDF (before enhancement)
            var (textElements, pageWidth, pageHeight) = _pdfProcessingService.GetTextElementsWithDimensions(filePath, page);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            var imageUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(Path.GetFileName(pageImagePath))}";

            return Json(new {
                success = true,
                imageUrl,
                textElements,
                pageWidth,
                pageHeight,
                message = "Page enhanced successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing page for edit");
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Save the edited PDF with text boxes
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SaveEditedPdf([FromForm] IFormFile file, string textBoxes)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "saveedit");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "edited");

            // Parse text boxes
            List<Models.TextBoxPosition>? boxes = null;
            if (!string.IsNullOrEmpty(textBoxes))
            {
                try
                {
                    boxes = System.Text.Json.JsonSerializer.Deserialize<List<Models.TextBoxPosition>>(textBoxes);
                }
                catch
                {
                    boxes = null;
                }
            }

            var (success, outputPath, resultMessage) = await _pdfProcessingService.SaveEditedPdfAsync(
                filePath, boxes, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving edited PDF");
            return Json(new { success = false, message = $"Error saving PDF: {ex.Message}" });
        }
    }

    #region PDF Font Detection

    /// <summary>
    /// Extracts the font names used in a PDF via Python / PyMuPDF.
    /// POST /Pdf/GetPdfFonts
    /// Returns: { success, fonts: string[] }
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetPdfFonts([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file provided." });

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "fonts");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            var (exitCode, stdout, stderr) =
                await _pdfProcessingService.RunPythonScriptAsync("pdf_fonts.py", $"\"{filePath}\"");

            // Cleanup
            try { System.IO.File.Delete(filePath); } catch { }

            if (exitCode != 0)
            {
                _logger.LogWarning("pdf_fonts.py failed: {Stderr}", stderr);
                return Json(new { success = false, fonts = Array.Empty<string>() });
            }

            // stdout should be a JSON array of strings
            var raw = stdout.Trim();
            if (raw.StartsWith("["))
            {
                var fonts = System.Text.Json.JsonSerializer.Deserialize<List<string>>(raw)
                            ?? new List<string>();
                return Json(new { success = true, fonts });
            }

            // stdout might be {"error": "..."} — still return success=false gracefully
            return Json(new { success = false, fonts = Array.Empty<string>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPdfFonts");
            return Json(new { success = false, fonts = Array.Empty<string>() });
        }
    }

    #endregion

    #endregion

    #region Unlock Operations

    [HttpGet]
    public IActionResult Unlock()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Unlock([FromForm] IFormFile file, string password)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        if (string.IsNullOrEmpty(password))
        {
            return Json(new { success = false, message = "Please enter the PDF password." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "unlock");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "unlock");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.RemoveProtectionAsync(
                filePath, password, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking PDF");
            return Json(new { success = false, message = $"Error unlocking PDF: {ex.Message}" });
        }
    }

    #endregion

    #region Protect Operations

    [HttpGet]
    public IActionResult Protect()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Protect([FromForm] IFormFile file, string userPassword, string ownerPassword)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        if (string.IsNullOrEmpty(userPassword))
        {
            return Json(new { success = false, message = "Please enter a user password." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, authMessage) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message = authMessage });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "protect");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName(file.FileName, "protect");

            var (success, outputPath, resultMessage) = await _pdfProcessingService.ProtectPdfAsync(
                filePath, userPassword, string.IsNullOrEmpty(ownerPassword) ? userPassword : ownerPassword, outputFileName);

            // Cleanup input file
            try { System.IO.File.Delete(filePath); } catch { }

            if (success && System.IO.File.Exists(outputPath))
            {
                var resultFileName = Path.GetFileName(outputPath);
                var fileInfo = new FileInfo(outputPath);
                await _fileProcessingService.RecordDownloadAsync(user, resultFileName, fileInfo.Length, "application/pdf");
                
                return Json(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = $"/Pdf/Download?fileName={Uri.EscapeDataString(resultFileName)}",
                    fileName = resultFileName
                });
            }

            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error protecting PDF");
            return Json(new { success = false, message = $"Error protecting PDF: {ex.Message}" });
        }
    }

    #endregion

    #region Download

    [HttpGet]
    public async Task<IActionResult> Download(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name is required.");
        }

        var filePath = _fileProcessingService.GetFilePath(fileName);
        
        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("Download requested but file not found: {FilePath}", filePath);
            return NotFound("File not found.");
        }

        var user = await GetCurrentUserAsync();
        var (canDownload, message) = await _fileProcessingService.CanDownloadAsync(user);
        
        if (!canDownload)
        {
            TempData["Error"] = message;
            return RedirectToAction("Index");
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            var contentType = GetContentType(fileName);

            await _fileProcessingService.RecordDownloadAsync(user, fileName, fileInfo.Length, contentType);

            // Open stream so file is held for the duration of the response (avoids TOCTOU if file is cleaned up)
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, contentType, fileName);
        }
        catch (System.IO.IOException ex)
        {
            _logger.LogWarning(ex, "File could not be opened for download (may have been removed): {FilePath}", filePath);
            return NotFound("File not found or no longer available.");
        }
    }

    #endregion

    #region Helper Methods

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        if (_userManager == null) return null;
        
        try
        {
            return await _userManager.GetUserAsync(User);
        }
        catch
        {
            return null;
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
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

    #endregion
}
