using Microsoft.AspNetCore.Mvc;
using Pdfpeaks.Services;
using Pdfpeaks.Models;
using SixLabors.ImageSharp.Processing;

namespace Pdfpeaks.Controllers.Api;

/// <summary>
/// API controller for file processing operations.
/// Works with ImageProcessingService (SkiaSharp edition) and PdfProcessingService.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[IgnoreAntiforgeryToken] // API uses JWT authentication, not CSRF
public class FilesController : ControllerBase
{
    private readonly FileProcessingService _fileProcessingService;
    private readonly PdfProcessingService _pdfProcessingService;
    private readonly ImageProcessingService _imageProcessingService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        FileProcessingService fileProcessingService,
        PdfProcessingService pdfProcessingService,
        ImageProcessingService imageProcessingService,
        ILogger<FilesController> logger)
    {
        _fileProcessingService = fileProcessingService;
        _pdfProcessingService = pdfProcessingService;
        _imageProcessingService = imageProcessingService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    //  UPLOAD
    // ═══════════════════════════════════════════════════════════

    /// <summary>Upload a file for any subsequent processing operation.</summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string? operation = null)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded." });

        try
        {
            var operationType = operation ?? "upload";
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, operationType);
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

    // ═══════════════════════════════════════════════════════════
    //  INFO ENDPOINTS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Return metadata for an uploaded PDF.</summary>
    [HttpPost("pdf/info")]
    public async Task<IActionResult> GetPdfInfo(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded." });

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
                formattedSize = FormatSize(fileSize),
                pageSizes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PDF info");
            return StatusCode(500, new { success = false, message = "Error reading PDF." });
        }
    }

    /// <summary>Return width, height, format and size of an uploaded image.</summary>
    [HttpPost("image/info")]
    public async Task<IActionResult> GetImageInfo(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded." });

        try
        {
            var fileName = await _fileProcessingService.SaveUploadedFileAsync(file, "info");
            var filePath = _fileProcessingService.GetFilePath(fileName);

            // GetInfo() returns a named tuple – no deconstruction ambiguity
            var info = _imageProcessingService.GetInfo(filePath);

            return Ok(new
            {
                success = info.Width > 0,
                width = info.Width,
                height = info.Height,
                format = info.Format,
                fileSize = info.FileSize,
                formattedSize = FormatSize(info.FileSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image info");
            return StatusCode(500, new { success = false, message = "Error reading image." });
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  IMAGE OPERATIONS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Resize an image to the specified dimensions.</summary>
    [HttpPost("image/resize")]
    public async Task<IActionResult> ResizeImage(
        [FromForm] string fileName,
        [FromForm] int width,
        [FromForm] int height,
        [FromForm] string mode = "Max")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var resizeMode = Enum.TryParse<ResizeMode>(mode, true, out var m) ? m : ResizeMode.Max;
        var outName = BuildOutputName(fileName, "resized");
        var result = await _imageProcessingService.ResizeAsync(filePath, outName, width, height, resizeMode);

        return ResultResponse(result);
    }

    /// <summary>Crop an image to the supplied rectangle.</summary>
    [HttpPost("image/crop")]
    public async Task<IActionResult> CropImage(
        [FromForm] string fileName,
        [FromForm] int x,
        [FromForm] int y,
        [FromForm] int width,
        [FromForm] int height)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "cropped");
        var result = await _imageProcessingService.CropAsync(filePath, outName, x, y, width, height);

        return ResultResponse(result);
    }

    /// <summary>Rotate an image by the given degrees.</summary>
    [HttpPost("image/rotate")]
    public async Task<IActionResult> RotateImage(
        [FromForm] string fileName,
        [FromForm] float degrees)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "rotated");
        var result = await _imageProcessingService.RotateAsync(filePath, outName, degrees);

        return ResultResponse(result);
    }

    /// <summary>Flip an image horizontally or vertically.</summary>
    [HttpPost("image/flip")]
    public async Task<IActionResult> FlipImage(
        [FromForm] string fileName,
        [FromForm] bool horizontal = true)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "flipped");
        var result = await _imageProcessingService.FlipAsync(filePath, outName, horizontal);

        return ResultResponse(result);
    }

    /// <summary>Convert an image to a different format (png / jpg / webp / bmp / gif).</summary>
    [HttpPost("image/convert")]
    public async Task<IActionResult> ConvertImageFormat(
        [FromForm] string fileName,
        [FromForm] string targetFormat)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(
            System.IO.Path.GetFileNameWithoutExtension(fileName),
            "converted",
            targetFormat.TrimStart('.'));
        var result = await _imageProcessingService.ConvertFormatAsync(filePath, outName, targetFormat);

        return ResultResponse(result);
    }

    /// <summary>Compress an image at the given JPEG quality (1–100).</summary>
    [HttpPost("image/compress")]
    public async Task<IActionResult> CompressImage(
        [FromForm] string fileName,
        [FromForm] int quality = 75)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "compressed", "jpg");
        var result = await _imageProcessingService.CompressAsync(filePath, outName, quality);

        return Ok(new
        {
            success = result.Success,
            message = result.Message,
            outputPath = result.OutputPath,
            originalSize = result.OriginalBytes,
            compressedSize = result.OutputBytes,
            outputFileName = System.IO.Path.GetFileName(result.OutputPath)
        });
    }

    /// <summary>Adjust brightness, contrast, saturation and hue.</summary>
    [HttpPost("image/adjust")]
    public async Task<IActionResult> AdjustImage(
        [FromForm] string fileName,
        [FromForm] float brightness = 1f,
        [FromForm] float contrast = 1f,
        [FromForm] float saturation = 1f,
        [FromForm] float hue = 0f)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "adjusted");
        var result = await _imageProcessingService.AdjustAsync(
            filePath, outName, brightness, contrast, saturation, hue);

        return ResultResponse(result);
    }

    /// <summary>Apply greyscale filter.</summary>
    [HttpPost("image/grayscale")]
    public async Task<IActionResult> GrayscaleImage([FromForm] string fileName)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "grayscale");
        var result = await _imageProcessingService.GrayscaleAsync(filePath, outName);

        return ResultResponse(result);
    }

    /// <summary>Create a thumbnail (max side ≤ maxSize, aspect preserved).</summary>
    [HttpPost("image/thumbnail")]
    public async Task<IActionResult> CreateThumbnail(
        [FromForm] string fileName,
        [FromForm] int maxSize = 256)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "thumb");
        var result = await _imageProcessingService.ThumbnailAsync(filePath, outName, maxSize);

        return ResultResponse(result);
    }

    /// <summary>Strip EXIF / IPTC / XMP metadata from an image.</summary>
    [HttpPost("image/remove-metadata")]
    public async Task<IActionResult> RemoveMetadata([FromForm] string fileName)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "clean");
        var result = await _imageProcessingService.RemoveMetadataAsync(filePath, outName);
        return ResultResponse(result.success, result.outputPath, result.message);
    }

    // ═══════════════════════════════════════════════════════════
    //  SKIA-SPECIFIC ENHANCEMENTS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Add a repeating text watermark rendered by SkiaSharp.
    /// Replaces the original stub AddWatermarkAsync.
    /// </summary>
    [HttpPost("image/watermark")]
    public async Task<IActionResult> AddWatermark(
        [FromForm] string fileName,
        [FromForm] string text,
        [FromForm] float opacity = 0.35f,
        [FromForm] int fontSize = 48,
        [FromForm] string hexColor = "FF0000",
        [FromForm] float angleDeg = -30f)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "watermarked");
        var result = await _imageProcessingService.AddWatermarkSkiaAsync(
            filePath, outName, text, opacity, fontSize, hexColor, angleDeg);

        return ResultResponse(result);
    }

    /// <summary>
    /// Apply a colour filter (sepia / invert / vintage / cool / warm).
    /// </summary>
    [HttpPost("image/color-filter")]
    public async Task<IActionResult> ApplyColorFilter(
        [FromForm] string fileName,
        [FromForm] string filterName)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, filterName);
        var result = await _imageProcessingService.ApplyColorFilterSkiaAsync(
            filePath, outName, filterName);

        return ResultResponse(result);
    }

    /// <summary>Gaussian blur (sigmaX / sigmaY in pixels).</summary>
    [HttpPost("image/blur")]
    public async Task<IActionResult> BlurImage(
        [FromForm] string fileName,
        [FromForm] float sigmaX = 4f,
        [FromForm] float sigmaY = 4f)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "blurred");
        var result = await _imageProcessingService.BlurSkiaAsync(filePath, outName, sigmaX, sigmaY);

        return ResultResponse(result);
    }

    /// <summary>Unsharp-mask sharpening.</summary>
    [HttpPost("image/sharpen")]
    public async Task<IActionResult> SharpenImage(
        [FromForm] string fileName,
        [FromForm] float sigma = 1.5f,
        [FromForm] float gain = 1.0f)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, "sharpened");
        var result = await _imageProcessingService.SharpenSkiaAsync(filePath, outName, sigma, gain);

        return ResultResponse(result);
    }

    // ═══════════════════════════════════════════════════════════
    //  CAMSCANNER PIPELINE
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Run the full CamScanner-style document scan pipeline:
    /// shadow removal → colour filter → sharpening.
    /// Optionally include perspective correction by supplying four corner points.
    /// </summary>
    [HttpPost("image/scan")]
    public async Task<IActionResult> ScanDocument(
        [FromForm] string fileName,
        [FromForm] string filter = "Enhanced",
        [FromForm] int sharpenLevel = 1,
        [FromForm] bool removeShadows = true,
        [FromForm] bool autoRotate = false,
        [FromForm] bool perspective = false,
        [FromForm] float tlX = 0, [FromForm] float tlY = 0,
        [FromForm] float trX = 0, [FromForm] float trY = 0,
        [FromForm] float brX = 0, [FromForm] float brY = 0,
        [FromForm] float blX = 0, [FromForm] float blY = 0)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var docFilter = Enum.TryParse<DocumentFilter>(filter, true, out var df)
            ? df : DocumentFilter.Enhanced;

        var opts = new ScanOptions
        {
            Filter = docFilter,
            SharpenLevel = sharpenLevel,
            RemoveShadows = removeShadows,
            AutoRotate = autoRotate,
            PerspectiveCorrect = perspective,
            Corners = perspective
                ? new[] { (tlX, tlY), (trX, trY), (brX, brY), (blX, blY) }
                : null
        };

        var outName = BuildOutputName(fileName, "scanned");
        var result = await _imageProcessingService.ScanDocumentAsync(filePath, outName, opts);

        return ResultResponse(result);
    }

    // ═══════════════════════════════════════════════════════════
    //  AI ENHANCEMENT
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// AI-powered upscaling + optional denoise / sharpen.
    /// Scale 2 = double resolution; 4 = quadruple, etc.
    /// </summary>
    [HttpPost("image/ai-enhance")]
    public async Task<IActionResult> AiEnhance(
        [FromForm] string fileName,
        [FromForm] int scale = 2,
        [FromForm] bool denoise = true,
        [FromForm] bool sharpen = true)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var outName = BuildOutputName(fileName, $"ai_x{scale}", "png");
        var result = await _imageProcessingService.AiEnhanceAsync(
            filePath, outName, scale, denoise, sharpen);

        return ResultResponse(result);
    }

    // ═══════════════════════════════════════════════════════════
    //  IMAGE → PDF
    // ═══════════════════════════════════════════════════════════

    /// <summary>Convert a single uploaded image to a PDF.</summary>
    [HttpPost("image/to-pdf")]
    public async Task<IActionResult> ImageToPdf(
        [FromForm] string fileName,
        [FromForm] string pageSize = "MatchImage")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var ps = Enum.TryParse<PdfPageSize>(pageSize, true, out var p) ? p : PdfPageSize.MatchImage;
        var outName = System.IO.Path.GetFileNameWithoutExtension(fileName) + ".pdf";
        var result = await _imageProcessingService.ImageToPdfAsync(filePath, outName, ps);

        return ResultResponse(result);
    }

    /// <summary>
    /// Convert multiple uploaded images to a single multi-page PDF.
    /// Optionally pre-process each image through the scan pipeline.
    /// </summary>
    [HttpPost("image/multi-to-pdf")]
    public async Task<IActionResult> MultipleImagesToPdf(
        [FromForm] List<string> fileNames,
        [FromForm] string outputFileName = "merged.pdf",
        [FromForm] string pageSize = "A4",
        [FromForm] bool preScan = false,
        [FromForm] string filter = "Enhanced")
    {
        if (fileNames is null || fileNames.Count == 0)
            return BadRequest(new { success = false, message = "No file names supplied." });

        var filePaths = fileNames
            .Select(n => _fileProcessingService.GetFilePath(n))
            .Where(System.IO.File.Exists)
            .ToList();

        if (filePaths.Count == 0)
            return NotFound(new { success = false, message = "None of the supplied files were found." });

        var ps = Enum.TryParse<PdfPageSize>(pageSize, true, out var p) ? p : PdfPageSize.A4;

        var result = await _imageProcessingService.ImagesToPdfAsync(
            filePaths.ToArray(), outputFileName, ps);

        return ResultResponse(result);
    }

    // ═══════════════════════════════════════════════════════════
    //  PDF OPERATIONS  (PdfProcessingService)
    // ═══════════════════════════════════════════════════════════

    /// <summary>Merge multiple PDFs (in upload order) into one.</summary>
    [HttpPost("pdf/merge")]
    public async Task<IActionResult> MergePdfs([FromForm] MergePdfRequest request)
    {
        if (request.FileNames is null || request.FileNames.Count < 2)
            return BadRequest(new { success = false, message = "At least 2 files required." });

        var filePaths = request.FileNames
            .Select(n => _fileProcessingService.GetFilePath(n))
            .ToList();

        var (success, outputPath, message) =
            await _pdfProcessingService.MergePdfAsync(filePaths, request.SortOrder ?? new List<int>(), request.OutputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Split a PDF: extract pages startPage–endPage into a new file.</summary>
    [HttpPost("pdf/split")]
    public async Task<IActionResult> SplitPdf(
        [FromForm] string fileName,
        [FromForm] int startPage,
        [FromForm] int endPage,
        [FromForm] string outputPrefix = "split")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPaths, message) = await _pdfProcessingService.SplitPdfAsync(
            filePath, startPage, endPage, outputPrefix);

        return Ok(new
        {
            success,
            message,
            files = outputPaths.Select(System.IO.Path.GetFileName)
        });
    }

    /// <summary>Rotate selected PDF pages.</summary>
    [HttpPost("pdf/rotate")]
    public async Task<IActionResult> RotatePdfPages(
        [FromForm] string fileName,
        [FromForm] List<int> pageIndices,
        [FromForm] int degrees,
        [FromForm] string outputFileName = "rotated.pdf")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.RotatePdfAsync(filePath, pageIndices, degrees, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Compress a PDF (quality 1–100; lower = smaller file).</summary>
    [HttpPost("pdf/compress")]
    public async Task<IActionResult> CompressPdf(
        [FromForm] string fileName,
        [FromForm] int quality = 50,
        [FromForm] string outputFileName = "compressed.pdf")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.CompressPdfAsync(filePath, quality, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Add page numbers to a PDF.</summary>
    [HttpPost("pdf/add-page-numbers")]
    public async Task<IActionResult> AddPageNumbers(
        [FromForm] string fileName,
        [FromForm] int startNumber = 1,
        [FromForm] string position = "bottom-center",
        [FromForm] string outputFileName = "numbered.pdf")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.AddPageNumbersAsync(filePath, outputFileName, startNumber, position);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Reorder / remove pages from a PDF.</summary>
    [HttpPost("pdf/organize")]
    public async Task<IActionResult> OrganizePdf(
        [FromForm] string fileName,
        [FromForm] List<int> pageOrderOneBased,
        [FromForm] string outputFileName = "organized.pdf")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.OrganizePdfAsync(filePath, pageOrderOneBased, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Add a text overlay to a specific page of a PDF.</summary>
    [HttpPost("pdf/add-text")]
    public async Task<IActionResult> AddTextToPdf(
        [FromForm] string fileName,
        [FromForm] string text,
        [FromForm] int pageNumber = 1,
        [FromForm] string outputFileName = "with-text.pdf")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.AddTextToPdfAsync(filePath, text, pageNumber, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Password-protect a PDF.</summary>
    [HttpPost("pdf/protect")]
    public async Task<IActionResult> ProtectPdf(
        [FromForm] string fileName,
        [FromForm] string userPassword,
        [FromForm] string ownerPassword,
        [FromForm] string outputFileName = "protected.pdf")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.ProtectPdfAsync(
                filePath, userPassword, ownerPassword, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Remove password protection from a PDF.</summary>
    [HttpPost("pdf/unprotect")]
    public async Task<IActionResult> RemovePdfProtection(
        [FromForm] string fileName,
        [FromForm] string password,
        [FromForm] string outputFileName = "unlocked.pdf")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.RemoveProtectionAsync(filePath, password, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Extract text content from a PDF.</summary>
    [HttpPost("pdf/extract-text")]
    public async Task<IActionResult> ExtractPdfText([FromForm] string fileName)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, text, message) = await _pdfProcessingService.ExtractTextAsync(filePath);

        return Ok(new { success, message, text });
    }

    /// <summary>Convert a PDF to images (jpg or png), one per page.</summary>
    [HttpPost("pdf/to-images")]
    public async Task<IActionResult> PdfToImages(
        [FromForm] string fileName,
        [FromForm] string format = "jpg",
        [FromForm] List<int>? selectedPages = null)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var prefix = System.IO.Path.GetFileNameWithoutExtension(fileName);
        var (success, outputPaths, message) =
            await _pdfProcessingService.ConvertToImagesAsync(filePath, prefix, format, selectedPages);

        return Ok(new
        {
            success,
            message,
            files = outputPaths.Select(System.IO.Path.GetFileName)
        });
    }

    /// <summary>Convert Word (.doc / .docx) to PDF.</summary>
    [HttpPost("pdf/from-word")]
    public async Task<IActionResult> WordToPdf(
        [FromForm] string fileName,
        [FromForm] string outputFileName = "converted.pdf")
    {
        try
        {
            var filePath = _fileProcessingService.GetFilePath(fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { success = false, message = "File not found." });

            var (success, outputPath, message) =
                await _pdfProcessingService.ConvertWordToPdfAsync(filePath, outputFileName);

            return ResultResponse(success, outputPath, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting Word to PDF: {FileName}", fileName);
            return StatusCode(500, new { success = false, message = $"Error converting Word to PDF: {ex.Message}" });
        }
    }

    /// <summary>Convert a PDF to a Word document.</summary>
    [HttpPost("pdf/to-word")]
    public async Task<IActionResult> PdfToWord(
        [FromForm] string fileName,
        [FromForm] string outputFileName = "converted.docx")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.ConvertToWordAsync(filePath, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Convert a PDF to an Excel workbook.</summary>
    [HttpPost("pdf/to-excel")]
    public async Task<IActionResult> PdfToExcel(
        [FromForm] string fileName,
        [FromForm] string outputFileName = "converted.xlsx",
        [FromForm] string extractionMode = "exact",
        [FromForm] bool aiEnhance = true)
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.ConvertToExcelAsync(filePath, outputFileName, extractionMode, aiEnhance);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Convert an Excel workbook to PDF.</summary>
    [HttpPost("pdf/from-excel")]
    public async Task<IActionResult> ExcelToPdf(
        [FromForm] string fileName,
        [FromForm] string outputFileName = "converted.pdf")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.ConvertExcelToPdfAsync(filePath, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Convert a PowerPoint file to PDF.</summary>
    [HttpPost("pdf/from-powerpoint")]
    public async Task<IActionResult> PowerPointToPdf(
        [FromForm] string fileName,
        [FromForm] string outputFileName = "converted.pdf")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.ConvertPowerPointToPdfAsync(filePath, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    /// <summary>Convert a PDF to a PowerPoint presentation (one slide per page).</summary>
    [HttpPost("pdf/to-powerpoint")]
    public async Task<IActionResult> PdfToPowerPoint(
        [FromForm] string fileName,
        [FromForm] string outputFileName = "converted.pptx")
    {
        var filePath = _fileProcessingService.GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "File not found." });

        var (success, outputPath, message) =
            await _pdfProcessingService.ConvertToPowerPointAsync(filePath, outputFileName);

        return ResultResponse(success, outputPath, message);
    }

    // ═══════════════════════════════════════════════════════════
    //  FILE MANAGEMENT
    // ═══════════════════════════════════════════════════════════

    /// <summary>Delete a temporary file by name.</summary>
    [HttpDelete("delete")]
    public IActionResult DeleteFile([FromQuery] string fileName)
    {
        try
        {
            var filePath = _fileProcessingService.GetFilePath(fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { success = false, message = "File not found." });

            System.IO.File.Delete(filePath);
            return Ok(new { success = true, message = "File deleted." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, new { success = false, message = "Error deleting file." });
        }
    }

    /// <summary>Download a processed file by name.</summary>
    [HttpGet("download/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        try
        {
            var filePath = _fileProcessingService.GetFilePath(fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { success = false, message = "File not found." });

            var mime = MimeType(fileName);
            return PhysicalFile(filePath, mime, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileName}", fileName);
            return StatusCode(500, new { success = false, message = "Error downloading file." });
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════

    /// <summary>Build a standardised output filename.</summary>
    private static string BuildOutputName(string inputName, string suffix, string? ext = null)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(inputName);
        var final = ext ?? System.IO.Path.GetExtension(inputName).TrimStart('.');
        if (string.IsNullOrWhiteSpace(final)) final = "jpg";
        return $"{name}_{suffix}.{final}";
    }

    /// <summary>Turn a ProcessResult into the appropriate IActionResult.</summary>
    private IActionResult ResultResponse(ProcessResult result) =>
        result.Success
            ? Ok(new
            {
                success = true,
                message = result.Message,
                outputPath = result.OutputPath,
                outputFileName = System.IO.Path.GetFileName(result.OutputPath)
            })
            : BadRequest(new { success = false, message = result.Message });

    /// <summary>Overload for services that still return tuple-based results.</summary>
    private IActionResult ResultResponse(bool success, string outputPath, string message) =>
        ResultResponse(new ProcessResult(success, outputPath, message));

    private static string FormatSize(long bytes)
    {
        if (bytes <= 0) return "0 Bytes";
        var sizes = new[] { "Bytes", "KB", "MB", "GB", "TB" };
        var i = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024));
        i = Math.Min(i, sizes.Length - 1);
        return $"{bytes / Math.Pow(1024, i):F2} {sizes[i]}";
    }

    private static string MimeType(string fileName) =>
        System.IO.Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            _ => "application/octet-stream"
        };
}
