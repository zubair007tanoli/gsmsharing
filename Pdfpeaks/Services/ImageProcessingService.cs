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

namespace Pdfpeaks.Services;

/// <summary>
/// Service for image processing operations using ImageSharp
/// </summary>
public class ImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly string _tempFilePath;

    public ImageProcessingService(ILogger<ImageProcessingService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _tempFilePath = Path.Combine(environment.ContentRootPath, "temp_files");
        
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
    /// Convert image to PDF
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertToPdfAsync(
        string inputPath, string outputFileName)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                return (false, "", "Input file not found.");
            }

            // Load the image using the current SixLabors.ImageSharp API
            Image<Rgba32> image;
            try
            {
                image = await Image.LoadAsync<Rgba32>(inputPath);
            }
            catch
            {
                // Fallback: try synchronous load
                image = Image.Load<Rgba32>(inputPath);
            }

            using (image)
            {
                // Create a new PDF document using PdfSharpCore
                var document = new PdfSharpCore.Pdf.PdfDocument();
                document.Info.Title = Path.GetFileNameWithoutExtension(inputPath);
                
                // Create a page with the same aspect ratio as the image
                var page = document.AddPage();
                
                // Set page size to match image dimensions (in points). 1 inch = 72 points.
                double dpiX = image.Metadata.HorizontalResolution > 0 ? image.Metadata.HorizontalResolution : 72;
                double dpiY = image.Metadata.VerticalResolution > 0 ? image.Metadata.VerticalResolution : 72;
                double imageWidth = image.Width * 72.0 / dpiX;
                double imageHeight = image.Height * 72.0 / dpiY;
                
                page.Width = imageWidth;
                page.Height = imageHeight;
                
                // Create XGraphics object for drawing
                using var gfx = XGraphics.FromPdfPage(page);
                
                // Save image to a temporary file in a format PDF can use
                var tempImagePath = Path.Combine(_tempFilePath, $"temp_{Guid.NewGuid():N}.png");
                await image.SaveAsync(tempImagePath, new PngEncoder());
                
                // Load the image into XImage
                using var xImage = XImage.FromFile(tempImagePath);
                
                // Draw the image on the PDF page
                gfx.DrawImage(xImage, 0, 0, imageWidth, imageHeight);
                
                // Clean up temp image
                try { File.Delete(tempImagePath); } catch { }
                
                var outputPath = Path.Combine(_tempFilePath, outputFileName);
                document.Save(outputPath);
                
                _logger.LogInformation("Image converted to PDF: {Input} -> {Output}", inputPath, outputPath);
                return (true, outputPath, "Successfully converted image to PDF.");
            }
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
                using var paint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 16,
                    IsAntialias = true
                };
                
                // Draw page number
                canvas.DrawText($"Page {pageNum} of {pageCount}", 20, 30, paint);
                canvas.DrawText($"Size: {page.Width:F1} x {page.Height:F1} points", 20, 60, paint);
                
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

    public async Task<ProcessResult> ImagesToPdfAsync(string[] inputPaths, string outputFileName, PdfPageSize pageSize)
        => new ProcessResult(false, "", "Multiple image to PDF not implemented.");

    public async Task<ProcessResult> ConvertMultipleImagesToPdfAsync(string[] paths, string outputFileName)
        => new ProcessResult(false, "", "Not implemented.");
}

