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
    private readonly ImageProcessingService _imageProcessingService;
    private readonly IWebHostEnvironment _environment;

    public PdfController(
        ILogger<PdfController> logger,
        UserManager<ApplicationUser>? userManager,
        FileProcessingService fileProcessingService,
        PdfProcessingService pdfProcessingService,
        ImageProcessingService imageProcessingService,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _userManager = userManager;
        _fileProcessingService = fileProcessingService;
        _pdfProcessingService = pdfProcessingService;
        _imageProcessingService = imageProcessingService;
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
        _logger.LogInformation("Merge request received with {Count} files", files?.Count ?? 0);
        
        if (files == null || files.Count < 2)
        {
            var error = "Please upload at least 2 PDF files to merge.";
            _logger.LogWarning("Merge failed: {Error}", error);
            return new JsonResult(new { success = false, message = error });
        }

        var user = await GetCurrentUserAsync();
        var totalSize = files.Sum(f => f.Length);
        _logger.LogInformation("Total file size: {Size}", totalSize);
        
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, totalSize);
        if (!canProcess)
        {
            _logger.LogWarning("Merge failed: {Message}", message);
            return new JsonResult(new { success = false, message });
        }

        try
        {
            // Save uploaded files
            var filePaths = new List<string>();
            var validFiles = new List<string>();
            var invalidFiles = new List<string>();
            
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    _logger.LogInformation("Saving file: {Name} ({Size})", file.FileName, file.Length);
                    var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "merge");
                    var filePath = _fileProcessingService.GetFilePath(fileName);
                    _logger.LogInformation("File saved to: {Path}", filePath);
                    
                    // Validate PDF file
                    if (_fileProcessingService.IsValidPdfFile(filePath))
                    {
                        validFiles.Add(filePath);
                        _logger.LogInformation("File validated as PDF: {Path}", filePath);
                    }
                    else
                    {
                        invalidFiles.Add(filePath);
                        _logger.LogWarning("Invalid PDF file: {Path}", filePath);
                    }
                    
                    filePaths.Add(filePath);
                }
            }

            _logger.LogInformation("Saved {Count} files for merging. Valid: {Valid}, Invalid: {Invalid}", 
                filePaths.Count, validFiles.Count, invalidFiles.Count);
            
            if (invalidFiles.Count > 0)
            {
                _logger.LogWarning("Invalid PDF files detected: {Files}", string.Join(", ", invalidFiles));
            }

            if (validFiles.Count < 2)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = "At least 2 valid PDF files are required for merging. Some files may not be valid PDFs." 
                });
            }

            // Parse sort order (JSON array of indices)
            var sortOrderList = new List<int>();
            if (!string.IsNullOrEmpty(sortOrder))
            {
                try
                {
                    sortOrderList = JsonSerializer.Deserialize<List<int>>(sortOrder) ?? new List<int>();
                    _logger.LogInformation("Sort order: {Order}", string.Join(", ", sortOrderList));
                }
                catch
                {
                    sortOrderList = Enumerable.Range(0, validFiles.Count).ToList();
                }
            }
            else
            {
                sortOrderList = Enumerable.Range(0, validFiles.Count).ToList();
            }

            var outputFileName = _fileProcessingService.GenerateFileName("merged.pdf", "merge");
            _logger.LogInformation("Generated output filename: {FileName}", outputFileName);
            
            var (success, outputPath, resultMessage) = await _pdfProcessingService.MergePdfAsync(
                validFiles, sortOrderList, outputFileName);

            _logger.LogInformation("Merge result: Success={Success}, Message={Message}, OutputPath={Path}", 
                success, resultMessage, outputPath);

            if (success)
            {
                var downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}";
                
                return new JsonResult(new { 
                    success = true, 
                    message = resultMessage,
                    downloadUrl = downloadUrl,
                    fileName = outputFileName
                });
            }
            else
            {
                return new JsonResult(new { success = false, message = resultMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging PDFs");
            return new JsonResult(new { success = false, message = "An error occurred while merging the PDF files." });
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
    public async Task<IActionResult> Split([FromBody] SplitRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.FileData))
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        try
        {
            // Decode base64 file data
            var fileBytes = Convert.FromBase64String(request.FileData);
            
            // Save the file temporarily
            var tempFileName = $"split_{Guid.NewGuid():N}.pdf";
            var tempFilePath = Path.Combine(_environment.ContentRootPath, "temp_files", tempFileName);
            await System.IO.File.WriteAllBytesAsync(tempFilePath, fileBytes);
            
            // Get PDF info
            var info = _pdfProcessingService.GetPdfInfo(tempFilePath);
            var pageCount = info.pageCount;

            if (request.StartPage < 1) request.StartPage = 1;
            if (request.EndPage > pageCount) request.EndPage = pageCount;
            if (request.StartPage > request.EndPage) request.StartPage = request.EndPage;

            var outputPrefix = Path.GetFileNameWithoutExtension(request.FileName);
            var (success, outputPaths, resultMessage) = await _pdfProcessingService.SplitPdfAsync(
                tempFilePath, request.StartPage, request.EndPage, outputPrefix);
            
            // Clean up temp file
            try { System.IO.File.Delete(tempFilePath); } catch { }

            if (success)
            {
                // Return all split files as download URLs
                var downloadUrls = outputPaths.Select(path => 
                {
                    var fileName = Path.GetFileName(path);
                    return new { fileName, url = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(fileName)}" };
                }).ToList();
                
                return Json(new 
                { 
                    success = true, 
                    message = resultMessage,
                    info = $"Pages {request.StartPage} - {request.EndPage} extracted from {pageCount} total pages",
                    files = downloadUrls,
                    fileCount = downloadUrls.Count
                });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error splitting PDF");
            return Json(new { success = false, message = "An error occurred while splitting the PDF file." });
        }
    }

    [HttpPost]
    [Route("Pdf/SplitWithFile")]
    public async Task<IActionResult> SplitWithFile([FromForm] int startPage, [FromForm] int endPage, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        try
        {
            // Ensure temp_files directory exists
            var tempDir = Path.Combine(_environment.ContentRootPath, "temp_files");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            // Generate unique operation ID
            var operationId = Guid.NewGuid().ToString("N");
            
            // Save the file temporarily
            var tempFileName = $"split_{operationId}.pdf";
            var tempFilePath = Path.Combine(tempDir, tempFileName);
            
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            _logger.LogInformation("Temporary file saved: {Path}", tempFilePath);
            
            // Get PDF info
            var info = _pdfProcessingService.GetPdfInfo(tempFilePath);
            var pageCount = info.pageCount;

            _logger.LogInformation("PDF info: {PageCount} pages found", pageCount);

            if (pageCount == 0)
            {
                _logger.LogError("PDF has 0 pages or could not be read");
                return Json(new { success = false, message = "The uploaded file could not be read as a valid PDF with pages." });
            }

            if (startPage < 1) startPage = 1;
            if (endPage > pageCount) endPage = pageCount;
            if (startPage > endPage) startPage = endPage;

            var outputPrefix = Path.GetFileNameWithoutExtension(file.FileName);
            _logger.LogInformation("Starting split: startPage={StartPage}, endPage={EndPage}, outputPrefix={OutputPrefix}, operationId={OperationId}", 
                startPage, endPage, outputPrefix, operationId);

            var (success, outputPaths, resultMessage) = await _pdfProcessingService.SplitPdfAsync(
                tempFilePath, startPage, endPage, outputPrefix, operationId);

            _logger.LogInformation("Split result: success={Success}, message={Message}, outputPaths count={Count}", 
                success, resultMessage, outputPaths.Count);

            if (success)
            {
                _logger.LogInformation("PDF split succeeded. Output paths: {Count}", outputPaths.Count);
                
                // Verify all split files exist before proceeding
                var verifiedPaths = new List<string>();
                foreach (var path in outputPaths)
                {
                    var exists = System.IO.File.Exists(path);
                    var fileInfo = exists ? new FileInfo(path) : null;
                    _logger.LogInformation("  Split file: {Path}, Exists: {Exists}, Size: {Size}", 
                        path, exists, fileInfo?.Length ?? 0);
                    if (exists)
                    {
                        verifiedPaths.Add(path);
                    }
                }
                
                // Clean up temp input file only after verifying split files
                try { System.IO.File.Delete(tempFilePath); } catch { }
                
                if (verifiedPaths.Count == 0)
                {
                    _logger.LogError("No split files were created despite success flag being true");
                    return Json(new { success = false, message = "No split files were created." });
                }

                // Return all split files as download URLs
                var downloadUrls = verifiedPaths.Select(path => 
                {
                    var fileName = Path.GetFileName(path);
                    _logger.LogInformation("Creating download URL for: {FileName}", fileName);
                    return new { fileName, url = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(fileName)}" };
                }).ToList();
                
                _logger.LogInformation("Returning {Count} download URLs", downloadUrls.Count);
                
                return Json(new 
                { 
                    success = true, 
                    message = resultMessage,
                    info = $"Pages {startPage} - {endPage} extracted from {pageCount} total pages",
                    files = downloadUrls,
                    fileCount = downloadUrls.Count
                });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error splitting PDF");
            return Json(new { success = false, message = "An error occurred while splitting the PDF file." });
        }
    }
    
    public class SplitRequest
    {
        public string FileData { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int StartPage { get; set; }
        public int EndPage { get; set; }
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
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message });
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

                var downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}";
                
                return Json(new 
                { 
                    success = true, 
                    message = resultMessage,
                    originalSize = FormatSize(originalSize),
                    compressedSize = FormatSize(compressedSize),
                    reduction = $"{reduction:F1}%",
                    downloadUrl = downloadUrl
                });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing PDF");
            return Json(new { success = false, message = "An error occurred while compressing the PDF file." });
        }
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
        if (file == null || file.Length == 0) return Json(new { success = false, message = "No file." });

        try
        {
            var user = await GetCurrentUserAsync();
            var (canProcess, msg) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
            if (!canProcess) return Json(new { success = false, message = msg });

            var inputFileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2word");
            var inputPath = _fileProcessingService.GetFilePath(inputFileName);
            var outputName = Path.ChangeExtension(inputFileName, ".docx");

            var (success, outputPath, error) = await _pdfProcessingService.ConvertToWordAsync(inputPath, outputName);

            if (success)
            {
                return Json(new
                {
                    success = true,
                    downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(Path.GetFileName(outputPath))}"
                });
            }
            return Json(new { success = false, message = error ?? "Conversion failed." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConvertToWord exception");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult ConvertToExcel()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToExcel(IFormFile file)
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
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2excel");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            
            // Perform actual conversion
            var (success, outputPath, resultMessage) = await _pdfProcessingService.ConvertToExcelAsync(
                filePath, Path.ChangeExtension(fileName, ".xlsx"));
            
            if (success)
            {
                var outputFileName = Path.GetFileName(outputPath);
                var downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}";
                
                return Json(new 
                { 
                    success = true, 
                    message = "PDF successfully converted to Excel spreadsheet.",
                    downloadUrl = downloadUrl,
                    fileName = outputFileName
                });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to Excel");
            return Json(new { success = false, message = "An error occurred during conversion. Please try again." });
        }
    }

    [HttpGet]
    public IActionResult ConvertToPowerPoint()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToPowerPoint(IFormFile file)
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
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2pptx");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            
            // Perform actual conversion
            var (success, outputPath, resultMessage) = await _pdfProcessingService.ConvertToPowerPointAsync(
                filePath, Path.ChangeExtension(fileName, ".pptx"));
            
            if (success)
            {
                var outputFileName = Path.GetFileName(outputPath);
                var downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}";
                
                return Json(new 
                { 
                    success = true, 
                    message = "PDF successfully converted to PowerPoint presentation.",
                    downloadUrl = downloadUrl,
                    fileName = outputFileName
                });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to PowerPoint");
            return Json(new { success = false, message = "An error occurred during conversion. Please try again." });
        }
    }

    [HttpGet]
    public IActionResult ConvertToJpg()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToJpg(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PDF file." });
        }

        // Validate file is PDF
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".pdf")
        {
            return Json(new { success = false, message = "Only PDF files can be converted to JPG." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2jpg");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputPrefix = Path.GetFileNameWithoutExtension(fileName);
            
            // Perform conversion
            var (success, outputPaths, resultMessage) = await _pdfProcessingService.ConvertToImagesAsync(
                filePath, outputPrefix, "jpg");
            
            if (success)
            {
                var downloadUrls = outputPaths.Select(path => 
                {
                    var imgFileName = Path.GetFileName(path);
                    return new { fileName = imgFileName, url = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(imgFileName)}" };
                }).ToList();
                
                return Json(new 
                { 
                    success = true, 
                    message = resultMessage,
                    files = downloadUrls,
                    fileCount = downloadUrls.Count
                });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to JPG");
            return Json(new { success = false, message = "An error occurred during conversion. Please try again." });
        }
    }

    [HttpGet]
    public IActionResult ConvertToPng()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToPng(IFormFile file)
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
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "pdf2png");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputPrefix = Path.GetFileNameWithoutExtension(fileName);
            
            // Perform conversion
            var (success, outputPaths, resultMessage) = await _pdfProcessingService.ConvertToImagesAsync(
                filePath, outputPrefix, "png");
            
            if (success)
            {
                var downloadUrls = outputPaths.Select(path => 
                {
                    var imgFileName = Path.GetFileName(path);
                    return new { fileName = imgFileName, url = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(imgFileName)}" };
                }).ToList();
                
                return Json(new 
                { 
                    success = true, 
                    message = resultMessage,
                    files = downloadUrls,
                    fileCount = downloadUrls.Count
                });
            }
            else
            {
                return Json(new { success = false, message = resultMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to PNG");
            return Json(new { success = false, message = "An error occurred during conversion. Please try again." });
        }
    }

    [HttpGet]
    public IActionResult ConvertFromWord()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertFromWord(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a Word document." });
        }

        // Validate file is Word document
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".doc" && extension != ".docx")
        {
            return Json(new { success = false, message = "Only Word documents (.doc, .docx) can be converted to PDF." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "doc2pdf");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("converted.pdf", "doc2pdf");

            var (success, outputPath, error) = await _pdfProcessingService.ConvertWordToPdfAsync(filePath, outputFileName);
            if (success)
            {
                var downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}";
                return Json(new { success = true, message = "Word document converted to PDF.", downloadUrl = downloadUrl, fileName = outputFileName });
            }
            return Json(new { success = false, message = error ?? "Word to PDF conversion failed. Install LibreOffice (e.g. apt install libreoffice) for server-side conversion." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting Word to PDF");
            return Json(new { success = false, message = ex.Message ?? "An error occurred during conversion. Please try again." });
        }
    }

    [HttpGet]
    public IActionResult ConvertFromExcel()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload an Excel file." });
        }

        // Validate file is Excel
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".xls" && extension != ".xlsx")
        {
            return Json(new { success = false, message = "Only Excel files (.xls, .xlsx) can be converted to PDF." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "xls2pdf");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("converted.pdf", "xls2pdf");
            
            // For Excel to PDF conversion, we copy the file and inform about the limitation
            var outputPath = Path.Combine(_environment.ContentRootPath, "temp_files", outputFileName);
            
            // Copy the file as a placeholder
            System.IO.File.Copy(filePath, outputPath, true);
            
            var downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}";
            
            return Json(new 
            { 
                success = true, 
                message = "Excel file received. Note: Full Excel to PDF conversion requires Microsoft Excel or LibreOffice installed on the server.",
                downloadUrl = downloadUrl,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting Excel to PDF");
            return Json(new { success = false, message = "An error occurred during conversion. Please try again." });
        }
    }

    [HttpGet]
    public IActionResult ConvertFromPowerPoint()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertFromPowerPoint(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a PowerPoint file." });
        }

        // Validate file is PowerPoint
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (extension != ".ppt" && extension != ".pptx")
        {
            return Json(new { success = false, message = "Only PowerPoint files (.ppt, .pptx) can be converted to PDF." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "ppt2pdf");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("converted.pdf", "ppt2pdf");
            
            // For PowerPoint to PDF conversion, we copy the file and inform about the limitation
            var outputPath = Path.Combine(_environment.ContentRootPath, "temp_files", outputFileName);
            
            // Copy the file as a placeholder
            System.IO.File.Copy(filePath, outputPath, true);
            
            var downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}";
            
            return Json(new 
            { 
                success = true, 
                message = "PowerPoint file received. Note: Full PowerPoint to PDF conversion requires Microsoft PowerPoint or LibreOffice installed on the server.",
                downloadUrl = downloadUrl,
                fileName = outputFileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PowerPoint to PDF");
            return Json(new { success = false, message = "An error occurred during conversion. Please try again." });
        }
    }

    [HttpGet]
    public IActionResult ConvertFromJpg()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConvertFromJpg(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Please upload a JPG image." });
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        if (string.IsNullOrEmpty(extension) || !allowed.Contains(extension))
        {
            return Json(new { success = false, message = "Only image files (.jpg, .jpeg, .png, .gif, .bmp) can be converted to PDF." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, message) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        
        if (!canProcess)
        {
            return Json(new { success = false, message, requiresUpgrade = true });
        }

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "jpg2pdf");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("converted.pdf", "jpg2pdf");

            var (success, outputPath, resultMessage) = await _imageProcessingService.ConvertToPdfAsync(filePath, outputFileName);
            if (success)
            {
                var downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}";
                return Json(new { success = true, message = resultMessage, downloadUrl = downloadUrl, fileName = outputFileName });
            }
            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting JPG to PDF");
            return Json(new { success = false, message = ex.Message ?? "An error occurred during conversion. Please try again." });
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
    public async Task<IActionResult> Organize(IFormFile file, string pageOrder)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "Please upload a PDF file." });

        IList<int> order;
        try
        {
            order = string.IsNullOrWhiteSpace(pageOrder)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(pageOrder) ?? new List<int>();
        }
        catch
        {
            return Json(new { success = false, message = "Invalid page order (use JSON array of page numbers, e.g. [3,1,2])." });
        }

        var user = await GetCurrentUserAsync();
        var (canProcess, msg) = await _fileProcessingService.CanProcessFileAsync(user, file.Length);
        if (!canProcess) return Json(new { success = false, message = msg });

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "organize");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("organized.pdf", "organize");
            var (success, outputPath, resultMessage) = await _pdfProcessingService.OrganizePdfAsync(filePath, order, outputFileName);
            if (success)
                return Json(new { success = true, message = resultMessage, downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}", fileName = outputFileName });
            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error organizing PDF");
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Edit Operations

    [HttpGet]
    public IActionResult Edit()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> EditAddText(IFormFile file, string text, int pageNumber = 1)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "Please upload a PDF file." });
        if (string.IsNullOrWhiteSpace(text))
            return Json(new { success = false, message = "Please enter text to add." });
        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "edit");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("edited.pdf", "edit");
            var (success, outputPath, resultMessage) = await _pdfProcessingService.AddTextToPdfAsync(filePath, text.Trim(), pageNumber, outputFileName);
            if (success)
                return Json(new { success = true, message = resultMessage, downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}", fileName = outputFileName });
            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing PDF");
            return Json(new { success = false, message = ex.Message });
        }
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
            // Service expects 0-based page indices; controller receives 1-based page numbers
            var pageIndices = pageNumbers.Select(p => p - 1).ToList();

            var (success, outputPath, resultMessage) = await _pdfProcessingService.RotatePdfAsync(
                filePath, pageIndices, degrees, outputFileName);

            return Json(new { 
                success, 
                message = resultMessage,
                downloadUrl = success ? $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}" : null,
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

    [HttpPost]
    public async Task<IActionResult> Unlock(IFormFile file, string password)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "Please upload a PDF file." });
        if (string.IsNullOrEmpty(password))
            return Json(new { success = false, message = "Please enter the PDF password." });
        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "unlock");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("unlocked.pdf", "unlock");
            var (success, outputPath, resultMessage) = await _pdfProcessingService.RemoveProtectionAsync(filePath, password, outputFileName);
            if (success)
                return Json(new { success = true, message = resultMessage, downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}", fileName = outputFileName });
            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking PDF");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult Protect()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Protect(IFormFile file, string userPassword, string ownerPassword)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "Please upload a PDF file." });
        if (string.IsNullOrEmpty(userPassword) && string.IsNullOrEmpty(ownerPassword))
            return Json(new { success = false, message = "Please enter at least one password (user and/or owner)." });
        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "protect");
            var filePath = _fileProcessingService.GetFilePath(fileName);
            var outputFileName = _fileProcessingService.GenerateFileName("protected.pdf", "protect");
            var (success, outputPath, resultMessage) = await _pdfProcessingService.ProtectPdfAsync(filePath, userPassword ?? "", ownerPassword ?? "", outputFileName);
            if (success)
                return Json(new { success = true, message = resultMessage, downloadUrl = $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}", fileName = outputFileName });
            return Json(new { success = false, message = resultMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error protecting PDF");
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Download and Processing

    [HttpGet]
    [Route("Pdf/DownloadFile")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            _logger.LogWarning("Download request with empty fileName");
            return NotFound("Invalid file name.");
        }

        // Decode the filename if it was URL-encoded
        var decodedFileName = Uri.UnescapeDataString(fileName);
        
        // Use the same temp directory as SplitWithFile
        var tempDir = Path.Combine(_environment.ContentRootPath, "temp_files");
        var filePath = Path.Combine(tempDir, decodedFileName);
        
        _logger.LogInformation("Download request for: {FileName}", fileName);
        _logger.LogInformation("Decoded FileName: {DecodedFileName}", decodedFileName);
        _logger.LogInformation("Looking for file at: {FilePath}", filePath);
        
        // Check if directory exists
        if (!Directory.Exists(tempDir))
        {
            _logger.LogWarning("Temp directory does not exist: {TempDir}", tempDir);
            return NotFound("Storage directory not found.");
        }
        
        // Check if file exists
        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("File not found: {Path}", filePath);
            
            // Log all files in temp directory for debugging
            try
            {
                var files = Directory.GetFiles(tempDir);
                _logger.LogWarning("Files in temp_files ({Count}):", files.Length);
                foreach (var f in files.Take(10))
                {
                    _logger.LogWarning("  - {FileName}", Path.GetFileName(f));
                }
                if (files.Length > 10)
                {
                    _logger.LogWarning("  ... and {More} more files", files.Length - 10);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing temp directory contents");
            }
            
            return NotFound("File not found or has expired.");
        }

        try
        {
            // Read file content into memory
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileInfo = new FileInfo(filePath);
            
            _logger.LogInformation("File found: {Path}, Size: {Size} bytes", filePath, fileInfo.Length);
            
            // Set content type by extension (PDF, DOCX, etc.)
            var contentType = GetContentType(decodedFileName);
            
            // Create proper content-disposition header for download
            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = decodedFileName,
                Inline = false
            };
            
            Response.Headers["Content-Disposition"] = contentDisposition.ToString();
            Response.Headers["Content-Length"] = fileBytes.Length.ToString();
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            
            return File(fileBytes, contentType, decodedFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {Path}", filePath);
            return StatusCode(500, "Error processing file download.");
        }
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
                downloadUrl = success ? $"/Pdf/DownloadFile?fileName={Uri.EscapeDataString(outputFileName)}" : null,
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
            ".html" => "text/html",
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
