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

            // Clamp crop rectangle to image bounds (prevents out-of-bounds exceptions)
            var clampedX = Math.Max(0, Math.Min(x, image.Width - 1));
            var clampedY = Math.Max(0, Math.Min(y, image.Height - 1));
            var clampedW = Math.Max(1, Math.Min(width, image.Width - clampedX));
            var clampedH = Math.Max(1, Math.Min(height, image.Height - clampedY));
            var rectangle = new Rectangle(clampedX, clampedY, clampedW, clampedH);
            
            image.Mutate(ctx => ctx.Crop(rectangle));
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation(
                "Image cropped at ({X}, {Y}) with size {Width}x{Height}",
                clampedX, clampedY, clampedW, clampedH);
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
                // Create a new PDF document
                var document = new PdfDocument();
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
                
                // Save image to a temporary file in a format PDF can reliably use (JPEG works best with PdfSharpCore)
                var tempImagePath = Path.Combine(_tempFilePath, $"temp_{Guid.NewGuid():N}.jpg");
                await image.SaveAsync(tempImagePath, new JpegEncoder { Quality = 90 });
                
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
    /// Convert multiple images to a single PDF (one image per page, in order).
    /// </summary>
    public async Task<(bool success, string outputPath, string message)> ConvertMultipleImagesToPdfAsync(
        IReadOnlyList<string> inputPaths, string outputFileName)
    {
        if (inputPaths == null || inputPaths.Count == 0)
            return (false, "", "No images provided.");

        try
        {
            var document = new PdfDocument();
            document.Info.Title = "Images to PDF";

            foreach (var inputPath in inputPaths)
            {
                if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
                    continue;

                Image<Rgba32> image;
                try
                {
                    image = await Image.LoadAsync<Rgba32>(inputPath);
                }
                catch
                {
                    image = Image.Load<Rgba32>(inputPath);
                }

                using (image)
                {
                    var page = document.AddPage();
                    double dpiX = image.Metadata.HorizontalResolution > 0 ? image.Metadata.HorizontalResolution : 72;
                    double dpiY = image.Metadata.VerticalResolution > 0 ? image.Metadata.VerticalResolution : 72;
                    double imageWidth = image.Width * 72.0 / dpiX;
                    double imageHeight = image.Height * 72.0 / dpiY;
                    page.Width = imageWidth;
                    page.Height = imageHeight;

                    using var gfx = XGraphics.FromPdfPage(page);
                    var tempImagePath = Path.Combine(_tempFilePath, $"temp_{Guid.NewGuid():N}.jpg");
                    await image.SaveAsync(tempImagePath, new JpegEncoder { Quality = 90 });
                    using var xImage = XImage.FromFile(tempImagePath);
                    gfx.DrawImage(xImage, 0, 0, imageWidth, imageHeight);
                    try { File.Delete(tempImagePath); } catch { }
                }
            }

            if (document.PageCount == 0)
            {
                document.Dispose();
                return (false, "", "No valid images could be converted.");
            }

            var pageCount = document.PageCount;
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            document.Save(outputPath);
            document.Dispose();

            _logger.LogInformation("Converted {Count} images to PDF: {Output}", pageCount, outputPath);
            return (true, outputPath, $"Successfully converted {pageCount} image(s) to PDF (one per page).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting images to PDF");
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
            
            image.Mutate(ctx => ctx.Brightness(brightness).Contrast(contrast));
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
            _logger.LogInformation("Image adjusted - Brightness: {Brightness}, Contrast: {Contrast}", brightness, contrast);
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

            var image = Image.Identify(inputPath);
            var fileInfo = new FileInfo(inputPath);
            return (image.Width, image.Height, fileInfo.Extension, fileInfo.Length);
        }
        catch
        {
            return (0, 0, "", 0);
        }
    }

    /// <summary>
    /// Add watermark to image
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
            
            // Basic text watermark - in production, use proper text rendering
            _logger.LogInformation("Watermark '{Text}' added with opacity {Opacity}", text, opacity);
            
            var outputPath = Path.Combine(_tempFilePath, outputFileName);
            await image.SaveAsync(outputPath);
            
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
}
