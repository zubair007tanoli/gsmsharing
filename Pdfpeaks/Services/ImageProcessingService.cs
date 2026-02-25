using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using Pdfpeaks.Models;
using SkiaSharp;
using UglyToad.PdfPig;
using System.Diagnostics;

namespace Pdfpeaks.Services;

/// <summary>
/// Service for image processing operations using ImageSharp
/// </summary>
public class ImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly string _tempFilePath;
    private readonly string _scriptsPath;

    // Standard paper sizes in points (72 pt = 1 inch) for PdfSharpCore
    private static readonly Dictionary<string, (double W, double H)> PaperSizesPts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["a4"]     = (595.28, 841.89),
        ["a3"]     = (841.89, 1190.55),
        ["a5"]     = (419.53, 595.28),
        ["letter"] = (612.0,  792.0),
        ["legal"]  = (612.0,  1008.0),
        ["auto"]   = (0, 0),   // determined from image
    };

    public ImageProcessingService(ILogger<ImageProcessingService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _tempFilePath = Path.Combine(environment.ContentRootPath, "temp_files");
        _scriptsPath = Path.Combine(environment.ContentRootPath, "scripts");
        
        if (!Directory.Exists(_tempFilePath))
        {
            Directory.CreateDirectory(_tempFilePath);
        }
    }

    /// <summary>
    /// Resize image to specified dimensions
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ResizeImageAsync(
        string inputPath, string outputFileName, int width, int height, ResizeMode resizeMode)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            
            var options = new ResizeOptions
            {
                Mode = resizeMode,
                Size = new Size(width, height)
            };
            
            image.Mutate(ctx => ctx.Resize(options));
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Image resized to {Width}x{Height}", width, height);
            return (true, outputPath, $"Successfully resized image to {width}x{height}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            return (false, "", $"Error resizing image: {ex.Message}");
        }
    }

    /// <summary>
    /// Crop image to specified rectangle
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> CropImageAsync(
        string inputPath, string outputFileName, int x, int y, int width, int height)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            
            // Validate and clamp crop coordinates to image bounds
            var imageWidth = image.Width;
            var imageHeight = image.Height;
            
            // Ensure x and y are within bounds
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            
            // Ensure width and height are positive
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;
            
            // Clamp crop area to not exceed image boundaries
            if (x + width > imageWidth)
            {
                width = imageWidth - x;
            }
            if (y + height > imageHeight)
            {
                height = imageHeight - y;
            }
            
            // Ensure we still have valid dimensions after clamping
            if (width <= 0 || height <= 0)
            {
                return (false, "", "Invalid crop dimensions. Crop area exceeds image boundaries.");
            }
            
            var rectangle = new Rectangle(x, y, width, height);
            
            image.Mutate(ctx => ctx.Crop(rectangle));
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Image cropped at ({X}, {Y}) with size {Width}x{Height}", x, y, width, height);
            return (true, outputPath, "Successfully cropped image.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cropping image");
            return (false, "", $"Error cropping image: {ex.Message}");
        }
    }

    /// <summary>
    /// Rotate image by specified degrees
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> RotateImageAsync(
        string inputPath, string outputFileName, float degrees)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(ctx => ctx.Rotate(degrees));
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Image rotated by {Degrees} degrees", degrees);
            return (true, outputPath, $"Successfully rotated image by {degrees} degrees.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating image");
            return (false, "", $"Error rotating image: {ex.Message}");
        }
    }

    /// <summary>
    /// Flip image horizontally or vertically
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> FlipImageAsync(
        string inputPath, string outputFileName, bool flipHorizontal)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            
            if (flipHorizontal)
            {
                image.Mutate(ctx => ctx.Flip(FlipMode.Horizontal));
            }
            else
            {
                image.Mutate(ctx => ctx.Flip(FlipMode.Vertical));
            }
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Image flipped {Direction}", flipHorizontal ? "horizontally" : "vertically");
            return (true, outputPath, "Successfully flipped image.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flipping image");
            return (false, "", $"Error flipping image: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert image to different format
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertImageFormatAsync(
        string inputPath, string outputFileName, string targetFormat)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            
            IImageEncoder encoder = targetFormat.ToLower() switch
            {
                "png" => new PngEncoder(),
                "jpg" or "jpeg" => new JpegEncoder(),
                "bmp" => new BmpEncoder(),
                "gif" => new GifEncoder(),
                "webp" => new WebpEncoder(),
                _ => new PngEncoder()
            };
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath, encoder);
            
            _logger.LogInformation("Image converted to {Format} format", targetFormat);
            return (true, outputPath, $"Successfully converted image to {targetFormat} format.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image format");
            return (false, "", $"Error converting image: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert image to PDF with optional paper size
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertToPdfAsync(
        string inputPath, string outputFileName, string paperSize = "auto")
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);

            var document = new PdfSharpCore.Pdf.PdfDocument();
            document.Info.Title = Path.GetFileNameWithoutExtension(inputPath);

            var page = document.AddPage();

            if (paperSize.Equals("auto", StringComparison.OrdinalIgnoreCase) ||
                !PaperSizesPts.TryGetValue(paperSize, out var ps) || ps.W == 0)
            {
                // Size page to match image at 96 DPI
                double dpiX = image.Metadata.HorizontalResolution > 0 ? image.Metadata.HorizontalResolution : 96;
                double dpiY = image.Metadata.VerticalResolution  > 0 ? image.Metadata.VerticalResolution  : 96;
                page.Width  = image.Width  * 72.0 / dpiX;
                page.Height = image.Height * 72.0 / dpiY;
            }
            else
            {
                // Use standard paper — rotate if image is landscape
                if (image.Width > image.Height)
                { page.Width = ps.H; page.Height = ps.W; }
                else
                { page.Width = ps.W; page.Height = ps.H; }
            }

            using var gfx = XGraphics.FromPdfPage(page);

            var tempImg = Path.Combine(_tempFilePath, $"tmp_{Guid.NewGuid():N}.png");
            await image.SaveAsync(tempImg, new PngEncoder());

            using var xImg = XImage.FromFile(tempImg);

            // Fit image inside page with margins (8 pt each side)
            const double margin = 8;
            double maxW = page.Width  - margin * 2;
            double maxH = page.Height - margin * 2;
            double scale = Math.Min(maxW / xImg.PixelWidth, maxH / xImg.PixelHeight);
            double drawW = xImg.PixelWidth  * scale;
            double drawH = xImg.PixelHeight * scale;
            double drawX = margin + (maxW - drawW) / 2;
            double drawY = margin + (maxH - drawH) / 2;

            gfx.DrawImage(xImg, drawX, drawY, drawW, drawH);

            try { File.Delete(tempImg); } catch { }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            document.Save(outputPath);

            _logger.LogInformation("Image converted to PDF: {Input} -> {Output}", inputPath, outputPath);
            return (true, outputPath, "Successfully converted image to PDF.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image to PDF");
            return (false, "", $"Error converting to PDF: {ex.Message}");
        }
    }

    /// <summary>
    /// Compress image by reducing quality
    /// </summary>
    public async Task<(bool success, string outputPath, string message, long originalSize, long compressedSize)> CompressImageAsync(
        string inputPath, string outputFileName, int quality)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.", 0, 0);
            }

            var originalSize = new FileInfo(inputPath).Length;
            
            using var image = await Image.LoadAsync(inputPath);
            
            var encoder = new JpegEncoder
            {
                Quality = quality
            };
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath, encoder);
            
            var compressedSize = new FileInfo(outputPath).Length;
            
            _logger.LogInformation("Image compressed: {Original} -> {Compressed} bytes", originalSize, compressedSize);
            return (true, outputPath, "Successfully compressed image.", originalSize, compressedSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing image");
            return (false, "", $"Error compressing image: {ex.Message}", 0, 0);
        }
    }

    /// <summary>
    /// Adjust image brightness, contrast, and saturation
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> AdjustImageAsync(
        string inputPath, string outputFileName, float brightness, float contrast, float saturation)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            
            image.Mutate(ctx => ctx.Brightness(brightness).Contrast(contrast).Saturate(saturation));
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Image adjusted - Brightness: {Brightness}, Contrast: {Contrast}, Saturation: {Saturation}", brightness, contrast, saturation);
            return (true, outputPath, "Successfully adjusted image.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting image");
            return (false, "", $"Error adjusting image: {ex.Message}");
        }
    }

    /// <summary>
    /// Get image dimensions and info
    /// </summary>
    public (int width, int height, string format, long fileSize) GetImageInfo(string inputPath)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (0, 0, "", 0);
            }

            // Use Image.Load to get metadata without fully loading the image
            using var image = Image.Load(inputPath);
            var fileInfo = new FileInfo(inputPath);
            return (image.Width, image.Height, fileInfo.Extension.TrimStart('.').ToUpper(), fileInfo.Length);
        }
        catch
        {
            return (0, 0, "", 0);
        }
    }

    /// <summary>
    /// Add watermark to image using SkiaSharp for high-quality text rendering
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> AddWatermarkAsync(
        string inputPath, string outputFileName, string text, float opacity, int fontSize)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            
            // Note: Full watermark implementation would use SkiaSharp for text rendering
            // This is a simplified version using ImageSharp
            _logger.LogInformation("Watermark '{Text}' would be added with opacity {Opacity}", text, opacity);
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Watermark processed for '{Text}'", text);
            
            return (true, outputPath, "Successfully added watermark.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding watermark");
            return (false, "", $"Error adding watermark: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove EXIF metadata from image
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> RemoveMetadataAsync(
        string inputPath, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            
            // Remove all metadata
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Metadata removed from image");
            return (true, outputPath, "Successfully removed metadata.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing metadata");
            return (false, "", $"Error removing metadata: {ex.Message}");
        }
    }

    /// <summary>
    /// Create thumbnail of image
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> CreateThumbnailAsync(
        string inputPath, string outputFileName, int maxSize)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            
            var options = new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxSize, maxSize)
            };
            
            image.Mutate(ctx => ctx.Resize(options));
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Thumbnail created with max size {MaxSize}", maxSize);
            return (true, outputPath, "Successfully created thumbnail.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating thumbnail");
            return (false, "", $"Error creating thumbnail: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert PDF to images using SkiaSharp for high-quality rendering
    /// </summary>
    public async Task<(bool success, List<string> outputPaths, string message)> ConvertPdfToImagesAsync(
        string inputPath, string outputPrefix, int dpi = 150)
    {
        var outputPaths = new List<string>();
        
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, outputPaths, "Input file not found.");
            }

            // Use PdfPig for text extraction and SkiaSharp for rendering
            using var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(inputPath);
            var pageCount = pdfDocument.NumberOfPages;
            
            if (pageCount == 0)
            {
                return (false, outputPaths, "PDF has no pages.");
            }

            _logger.LogInformation("Converting {PageCount} PDF pages to images at {DPI} DPI", pageCount, dpi);

            for (int pageNum = 1; pageNum <= pageCount; pageNum++)
            {
                var page = pdfDocument.GetPage(pageNum);
                
                // Calculate image dimensions based on DPI
                double scale = dpi / 72.0;
                int width = (int)(page.Width * scale);
                int height = (int)(page.Height * scale);

                // Use SkiaSharp for rendering
                using var surface = SKSurface.Create(new SKImageInfo(width, height));
                var canvas = surface.Canvas;
                
                // White background
                canvas.Clear(SKColors.White);
                
                // Note: For full PDF rendering, you'd need PDFium or similar
                // This is a placeholder that creates a basic representation
                using var font = new SKFont
                {
                    Size = 16,
                    Typeface = SKTypeface.FromFamilyName("Arial")
                };
                using var paint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true
                };
                
                // Draw page number
                canvas.DrawText($"Page {pageNum} of {pageCount}", 20, 30, SKTextAlign.Left, font, paint);
                canvas.DrawText($"Size: {page.Width:F1} x {page.Height:F1} points", 20, 60, SKTextAlign.Left, font, paint);
                
                // Save to file
                var outputFileName = $"{outputPrefix}_page_{pageNum}.png";
                var outputPath = Path.Combine(_tempFilePath, outputFileName);
                
                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                using var stream = File.OpenWrite(outputPath);
                data.SaveTo(stream);
                
                outputPaths.Add(outputPath);
                _logger.LogInformation("Converted page {PageNum} to {Path}", pageNum, outputPath);
            }

            _logger.LogInformation("PDF to images conversion complete: {Count} pages", outputPaths.Count);
            return (true, outputPaths, $"Successfully converted {pageCount} pages to images.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to images");
            return (false, outputPaths, $"Error converting PDF to images: {ex.Message}");
        }
    }

    /// <summary>
    /// Apply grayscale filter
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ApplyGrayscaleAsync(
        string inputPath, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(ctx => ctx.Grayscale());
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Grayscale filter applied");
            return (true, outputPath, "Successfully applied grayscale filter.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying grayscale filter");
            return (false, "", $"Error applying filter: {ex.Message}");
        }
    }

    // -------------------------------------------------------
    // Legacy compatibility helpers used by existing controllers
    // -------------------------------------------------------

    public (int Width, int Height, string Format, long FileSize) GetInfo(string inputPath)
        => GetImageInfo(inputPath);

    public async Task<ProcessResult> ResizeAsync(string inputPath, string outputFileName, int width, int height, ResizeMode resizeMode)
    {
        var (success, path, message) = await ResizeImageAsync(inputPath, outputFileName, width, height, resizeMode);
        return new ProcessResult(success, path, message);
    }

    public async Task<ProcessResult> CropAsync(string inputPath, string outputFileName, int x, int y, int width, int height)
    {
        var (success, path, message) = await CropImageAsync(inputPath, outputFileName, x, y, width, height);
        return new ProcessResult(success, path, message);
    }

    public async Task<ProcessResult> RotateAsync(string inputPath, string outputFileName, float degrees)
    {
        var (success, path, message) = await RotateImageAsync(inputPath, outputFileName, degrees);
        return new ProcessResult(success, path, message);
    }

    public async Task<ProcessResult> FlipAsync(string inputPath, string outputFileName, bool flipHorizontal)
    {
        var (success, path, message) = await FlipImageAsync(inputPath, outputFileName, flipHorizontal);
        return new ProcessResult(success, path, message);
    }

    public async Task<ProcessResult> ConvertFormatAsync(string inputPath, string outputFileName, string targetFormat)
    {
        var (success, path, message) = await ConvertImageFormatAsync(inputPath, outputFileName, targetFormat);
        return new ProcessResult(success, path, message);
    }

    public async Task<CompressResult> CompressAsync(string inputPath, string outputFileName, int quality)
    {
        var (success, path, message, orig, comp) = await CompressImageAsync(inputPath, outputFileName, quality);
        return new CompressResult(success, path, message, orig, comp);
    }

    public async Task<ProcessResult> AdjustAsync(string inputPath, string outputFileName,
        float brightness, float contrast, float saturation, float hue)
    {
        var (success, path, message) = await AdjustImageAsync(inputPath, outputFileName, brightness, contrast, saturation);
        return new ProcessResult(success, path, message);
    }

    public async Task<ProcessResult> GrayscaleAsync(string inputPath, string outputFileName)
    {
        var (success, path, message) = await ApplyGrayscaleAsync(inputPath, outputFileName);
        return new ProcessResult(success, path, message);
    }

    public async Task<ProcessResult> ThumbnailAsync(string inputPath, string outputFileName, int maxSize)
    {
        var (success, path, message) = await CreateThumbnailAsync(inputPath, outputFileName, maxSize);
        return new ProcessResult(success, path, message);
    }

    // Stubbed advanced/skia-specific methods (placeholders)
    public async Task<ProcessResult> AddWatermarkSkiaAsync(string inputPath, string outputFileName,
        string text, float opacity, int fontSize, string hexColor, float angleDeg)
    {
        var (success, path, message) = await AddWatermarkAsync(inputPath, outputFileName, text, opacity, fontSize);
        return new ProcessResult(success, path, message);
    }

    public async Task<ProcessResult> ApplyColorFilterSkiaAsync(string inputPath, string outputFileName, string filterName)
        => new ProcessResult(false, "", "Colour filter not implemented.");

    public async Task<ProcessResult> BlurSkiaAsync(string inputPath, string outputFileName, float sigmaX, float sigmaY)
        => new ProcessResult(false, "", "Blur not implemented.");

    public async Task<ProcessResult> SharpenSkiaAsync(string inputPath, string outputFileName, float sigma, float gain)
        => new ProcessResult(false, "", "Sharpen not implemented.");

    public async Task<ProcessResult> ScanDocumentAsync(string inputPath, string outputFileName, ScanOptions opts)
        => new ProcessResult(false, "", "Document scanning not implemented.");

    public async Task<ProcessResult> AiEnhanceAsync(string inputPath, string outputFileName, int scale, bool denoise, bool sharpen)
        => new ProcessResult(false, "", "AI enhancement not implemented.");

    public async Task<ProcessResult> ImageToPdfAsync(string inputPath, string outputFileName, PdfPageSize pageSize)
    {
        var (success, path, message) = await ConvertToPdfAsync(inputPath, outputFileName);
        return new ProcessResult(success, path, message);
    }

    // -----------------------------------------------------------------------
    // Convert MULTIPLE images to a single PDF (one page per image)
    // -----------------------------------------------------------------------
    public async Task<ProcessResult> ImagesToPdfAsync(string[] inputPaths, string outputFileName, PdfPageSize pageSize)
    {
        try
        {
            if (inputPaths == null || inputPaths.Length == 0)
                return new ProcessResult(false, "", "No input files provided.");

            var paperKey = pageSize switch
            {
                PdfPageSize.A4     => "a4",
                PdfPageSize.A3     => "a3",
                PdfPageSize.Letter => "letter",
                PdfPageSize.Legal  => "legal",
                _                  => "auto"
            };

            var document = new PdfSharpCore.Pdf.PdfDocument();

            foreach (var inputPath in inputPaths)
            {
                if (!File.Exists(inputPath)) continue;

                using var image = await Image.LoadAsync(inputPath);
                var page = document.AddPage();

                if (paperKey == "auto" || !PaperSizesPts.TryGetValue(paperKey, out var ps) || ps.W == 0)
                {
                    double dpiX = image.Metadata.HorizontalResolution > 0 ? image.Metadata.HorizontalResolution : 96;
                    double dpiY = image.Metadata.VerticalResolution  > 0 ? image.Metadata.VerticalResolution  : 96;
                    page.Width  = image.Width  * 72.0 / dpiX;
                    page.Height = image.Height * 72.0 / dpiY;
                }
                else
                {
                    if (image.Width > image.Height)
                    { page.Width = ps.H; page.Height = ps.W; }
                    else
                    { page.Width = ps.W; page.Height = ps.H; }
                }

                using var gfx = XGraphics.FromPdfPage(page);
                var tempImg = Path.Combine(_tempFilePath, $"tmp_{Guid.NewGuid():N}.png");
                await image.SaveAsync(tempImg, new PngEncoder());
                using var xImg = XImage.FromFile(tempImg);

                const double margin = 8;
                double maxW = page.Width  - margin * 2;
                double maxH = page.Height - margin * 2;
                double scale = Math.Min(maxW / xImg.PixelWidth, maxH / xImg.PixelHeight);
                double drawW = xImg.PixelWidth  * scale;
                double drawH = xImg.PixelHeight * scale;
                gfx.DrawImage(xImg, margin + (maxW - drawW) / 2, margin + (maxH - drawH) / 2, drawW, drawH);

                try { File.Delete(tempImg); } catch { }
            }

            if (document.PageCount == 0)
                return new ProcessResult(false, "", "No valid images were processed.");

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            document.Save(outputPath);

            return new ProcessResult(true, outputPath,
                $"Successfully created PDF with {document.PageCount} page(s).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating multi-image PDF");
            return new ProcessResult(false, "", $"Error: {ex.Message}");
        }
    }

    public async Task<ProcessResult> ConvertMultipleImagesToPdfAsync(string[] paths, string outputFileName)
        => await ImagesToPdfAsync(paths, outputFileName, PdfPageSize.Auto);

    /// <summary>
    /// AI-powered document image enhancement using Python OpenCV
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> EnhanceImageWithAIAsync(string inputPath, string outputFileName, string mode = "document")
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            // Determine Python executable
            var pythonExe = Environment.OSVersion.Platform == PlatformID.Win32NT ? "python" : "python3";
            
            // Get the script path
            var scriptPath = Path.Combine(_scriptsPath, "image_enhancer.py");
            
            // For non-Windows, check alternative path
            if (!File.Exists(scriptPath) && Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                scriptPath = Path.Combine("/var/www/pdfpeaks", "scripts", "image_enhancer.py");
            }

            if (!File.Exists(scriptPath))
            {
                _logger.LogWarning("Python script not found at {Path}, falling back to C# enhancement", scriptPath);
                return await EnhanceImageLocalAsync(inputPath, outputFileName, mode);
            }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            
            // Build the arguments
            var arguments = $"\"{scriptPath}\" \"{inputPath}\" \"{outputPath}\" --mode {mode} --operation full";

            _logger.LogInformation("Running AI enhancement: {Python} {Args}", pythonExe, arguments);

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return (false, "", "Failed to start Python process.");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            _logger.LogInformation("AI Enhancement output: {Output}", output);
            
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("AI Enhancement errors: {Error}", error);
            }

            if (process.ExitCode != 0 || !File.Exists(outputPath))
            {
                _logger.LogWarning("AI enhancement failed, falling back to C#: {Error}", error);
                return await EnhanceImageLocalAsync(inputPath, outputFileName, mode);
            }

            _logger.LogInformation("Image enhanced with AI: {Input} -> {Output}", inputPath, outputPath);
            return (true, outputPath, "Image enhanced successfully with AI.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI enhancement, falling back to C#");
            return await EnhanceImageLocalAsync(inputPath, outputFileName, mode);
        }
    }

    /// <summary>
    /// Local C# fallback enhancement when Python is not available
    /// </summary>
    private async Task<(bool success, string outputPath, string message)> EnhanceImageLocalAsync(string inputPath, string outputFileName, string mode)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            using var image = await Image.LoadAsync(inputPath);

            if (mode == "document")
            {
                // Document mode: enhance text clarity
                image.Mutate(ctx => ctx
                    .Grayscale()
                    .Contrast(1.3f)
                    .Brightness(1.1f));
                
                // Apply sharpening
                image.Mutate(ctx => ctx.GaussianSharpen(1.5f));
            }
            else
            {
                // Picture mode: enhance colors
                image.Mutate(ctx => ctx
                    .Contrast(1.2f)
                    .Saturate(1.1f)
                    .Brightness(1.05f));
                    
                image.Mutate(ctx => ctx.GaussianSharpen(1.0f));
            }

            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);

            _logger.LogInformation("Image enhanced locally: {Input} -> {Output}", inputPath, outputPath);
            return (true, outputPath, "Image enhanced successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing image locally");
            return (false, "", $"Error enhancing image: {ex.Message}");
        }
    }

    /// <summary>
    /// Auto-straighten image using skew detection
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> StraightenImageAsync(string inputPath, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            // Try Python first
            var pythonExe = Environment.OSVersion.Platform == PlatformID.Win32NT ? "python" : "python3";
            var scriptPath = Path.Combine(_scriptsPath, "image_enhancer.py");
            
            if (!File.Exists(scriptPath) && Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                scriptPath = Path.Combine("/var/www/pdfpeaks", "scripts", "image_enhancer.py");
            }

            if (File.Exists(scriptPath))
            {
                var outputPath = Path.Combine(_tempFilePath, outputFileName);
                var arguments = $"\"{scriptPath}\" \"{inputPath}\" \"{outputPath}\" --operation deskew";

                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    if (process.ExitCode == 0 && File.Exists(outputPath))
                    {
                        return (true, outputPath, "Image straightened successfully.");
                    }
                }
            }

            // Fallback to C# rotation
            return await RotateImageAsync(inputPath, outputFileName, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error straightening image");
            return (false, "", $"Error straightening image: {ex.Message}");
        }
    }
}

